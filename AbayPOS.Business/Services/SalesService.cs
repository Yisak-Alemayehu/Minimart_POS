using AbayPOS.Data;
using AbayPOS.Domain.Entities;
using AbayPOS.Domain.Enums;
using AbayPOS.Domain.Exceptions;
using AbayPOS.Domain.Logging;
using AbayPOS.Domain.Models;

namespace AbayPOS.Business.Services;

/// <summary>Payment details collected by the cashier dialog.</summary>
public sealed class PaymentRequest
{
    public PaymentMethod Method { get; set; } = PaymentMethod.Cash;
    public decimal AmountPaid { get; set; }
}

/// <summary>Cart and checkout use cases.</summary>
public interface ISalesService
{
    Cart CreateCart();
    void AddProduct(Cart cart, Product product, int qty = 1);
    void ChangeQty(Cart cart, int productId, int qty);
    void Remove(Cart cart, int productId);
    void Clear(Cart cart);
    void ApplyDiscount(Cart cart, decimal value, bool isPercent, Session session);
    Task<string> PreviewSaleNoAsync(CancellationToken cancellationToken = default);
    Task<Sale> CompleteSaleAsync(Cart cart, PaymentRequest payment, Session session, CancellationToken cancellationToken = default);
    Task VoidSaleAsync(int saleId, Session session, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Sale>> GetHistoryAsync(DateTime? from, DateTime? to, int? cashierId, string? saleNo, CancellationToken cancellationToken = default);
    Task<Sale?> GetSaleAsync(int id, CancellationToken cancellationToken = default);
}

/// <inheritdoc />
public sealed class SalesService : ISalesService
{
    public const decimal CashierMaxDiscountPercent = 10m;
    private readonly IUnitOfWorkFactory _uow;
    private readonly IAppLogger _logger;

    public SalesService(IUnitOfWorkFactory uow, IAppLogger logger)
    {
        _uow = uow;
        _logger = logger;
    }

    public Cart CreateCart() => new();

    public void AddProduct(Cart cart, Product product, int qty = 1)
    {
        ArgumentNullException.ThrowIfNull(cart);
        ArgumentNullException.ThrowIfNull(product);
        if (qty <= 0)
            throw new DomainValidationException("Quantity must be at least 1.");
        if (product.IsOutOfStock)
            throw new InsufficientStockException(product.Name, 0);

        var line = cart.MutableLines.FirstOrDefault(item => item.ProductId == product.Id || item.Barcode == product.Barcode);
        var nextQty = (line?.Qty ?? 0) + qty;
        if (nextQty > product.StockQty)
            throw new InsufficientStockException(product.Name, product.StockQty);

        if (line is null)
        {
            cart.MutableLines.Add(new CartLine
            {
                ProductId = product.Id,
                ProductName = product.Name,
                Barcode = product.Barcode,
                Category = product.Category,
                Emoji = product.Emoji,
                UnitPrice = product.Price,
                Qty = qty,
                AvailableStock = product.StockQty
            });
        }
        else
        {
            line.Qty = nextQty;
            line.AvailableStock = product.StockQty;
        }
    }

    public void ChangeQty(Cart cart, int productId, int qty)
    {
        ArgumentNullException.ThrowIfNull(cart);
        var line = cart.MutableLines.FirstOrDefault(item => item.ProductId == productId)
            ?? throw new DomainValidationException("That item is not in the cart.");
        if (qty <= 0)
        {
            cart.MutableLines.Remove(line);
            return;
        }

        if (qty > line.AvailableStock)
            throw new InsufficientStockException(line.ProductName, line.AvailableStock);
        line.Qty = qty;
    }

    public void Remove(Cart cart, int productId)
    {
        ArgumentNullException.ThrowIfNull(cart);
        cart.MutableLines.RemoveAll(line => line.ProductId == productId);
    }

    public void Clear(Cart cart)
    {
        ArgumentNullException.ThrowIfNull(cart);
        cart.Clear();
    }

    public void ApplyDiscount(Cart cart, decimal value, bool isPercent, Session session)
    {
        ArgumentNullException.ThrowIfNull(cart);
        ArgumentNullException.ThrowIfNull(session);
        if (value < 0)
            throw new DomainValidationException("Discount cannot be negative.");

        if (isPercent && value > 100m)
            throw new DomainValidationException("Percent discount cannot exceed 100%.");

        var percent = isPercent ? value : cart.Subtotal == 0 ? 0 : value / cart.Subtotal * 100m;
        if (!session.IsManager && percent > CashierMaxDiscountPercent)
            throw new DomainValidationException($"Cashiers can apply at most {CashierMaxDiscountPercent:0}% off. Ask a manager for a larger discount.");

        cart.DiscountValue = value;
        cart.DiscountIsPercent = isPercent;
    }

    public async Task<string> PreviewSaleNoAsync(CancellationToken cancellationToken = default)
    {
        await using var uow = _uow.Create();
        await uow.EnsureOpenAsync(cancellationToken).ConfigureAwait(false);
        var next = await uow.Sales.NextSequenceAsync(cancellationToken).ConfigureAwait(false);
        return FormatSaleNo(next);
    }

    public async Task<Sale> CompleteSaleAsync(Cart cart, PaymentRequest payment, Session session, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(cart);
        ArgumentNullException.ThrowIfNull(payment);
        ArgumentNullException.ThrowIfNull(session);
        if (cart.Lines.Count == 0)
            throw new DomainValidationException("Add at least one item before taking payment.");
        if (payment.AmountPaid < cart.Total)
            throw new DomainValidationException("Amount paid is less than the total due.");

        await using var uow = _uow.Create();
        await uow.BeginAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            foreach (var line in cart.Lines)
            {
                var affected = await uow.Products.TryDecrementStockAsync(line.ProductId, line.Qty, cancellationToken).ConfigureAwait(false);
                if (affected == 0)
                {
                    var product = await uow.Products.GetByIdAsync(line.ProductId, cancellationToken).ConfigureAwait(false);
                    var available = product?.StockQty ?? 0;
                    throw new InsufficientStockException(line.ProductName, available);
                }
            }

            var sequence = await uow.Sales.NextSequenceAsync(cancellationToken).ConfigureAwait(false);
            var sale = new Sale
            {
                SaleNo = FormatSaleNo(sequence),
                CashierId = session.User.Id,
                CashierName = session.User.FullName,
                SaleDate = DateTime.Now,
                Subtotal = cart.Subtotal,
                Discount = cart.DiscountAmount,
                Total = cart.Total,
                PaymentMethod = payment.Method,
                AmountPaid = payment.AmountPaid,
                Change = decimal.Round(payment.AmountPaid - cart.Total, 2, MidpointRounding.AwayFromZero),
                Status = SaleStatus.Completed
            };
            await uow.Sales.InsertAsync(sale, cancellationToken).ConfigureAwait(false);
            foreach (var line in cart.Lines)
            {
                var item = new SaleItem
                {
                    SaleId = sale.Id,
                    ProductId = line.ProductId,
                    ProductName = line.ProductName,
                    Qty = line.Qty,
                    UnitPrice = line.UnitPrice,
                    LineTotal = line.LineTotal
                };
                await uow.Sales.InsertItemAsync(item, cancellationToken).ConfigureAwait(false);
                sale.Items.Add(item);
            }

            await uow.Audit.InsertAsync(new AuditLog
            {
                UserId = session.User.Id,
                Action = "SaleCompleted",
                Details = $"{sale.SaleNo} total {sale.Total:0.00} via {sale.PaymentMethod}",
                CreatedAt = DateTime.Now
            }, cancellationToken).ConfigureAwait(false);
            await uow.CommitAsync(cancellationToken).ConfigureAwait(false);
            cart.Clear();
            _logger.Info($"Sale {sale.SaleNo} completed.");
            return sale;
        }
        catch (Exception ex)
        {
            await uow.RollbackAsync(cancellationToken).ConfigureAwait(false);
            _logger.Error("CompleteSale rolled back.", ex);
            throw;
        }
    }

    public async Task VoidSaleAsync(int saleId, Session session, CancellationToken cancellationToken = default)
    {
        RoleGuard.RequireManager(session);
        await using var uow = _uow.Create();
        await uow.BeginAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            var sale = await uow.Sales.GetByIdAsync(saleId, cancellationToken).ConfigureAwait(false)
                ?? throw new DomainValidationException("Sale was not found.");
            if (sale.Status == SaleStatus.Voided)
                throw new DomainValidationException("This sale is already voided.");

            foreach (var item in sale.Items)
                await uow.Products.IncrementStockAsync(item.ProductId, item.Qty, cancellationToken).ConfigureAwait(false);

            await uow.Sales.UpdateStatusAsync(saleId, SaleStatus.Voided, cancellationToken).ConfigureAwait(false);
            await uow.Audit.InsertAsync(new AuditLog
            {
                UserId = session.User.Id,
                Action = "SaleVoided",
                Details = sale.SaleNo,
                CreatedAt = DateTime.Now
            }, cancellationToken).ConfigureAwait(false);
            await uow.CommitAsync(cancellationToken).ConfigureAwait(false);
            _logger.Warn($"Sale {sale.SaleNo} voided.");
        }
        catch (Exception ex) when (ex is not DomainValidationException and not UnauthorizedAccessException)
        {
            await uow.RollbackAsync(cancellationToken).ConfigureAwait(false);
            _logger.Error("VoidSale rolled back.", ex);
            throw;
        }
    }

    public async Task<IReadOnlyList<Sale>> GetHistoryAsync(DateTime? from, DateTime? to, int? cashierId, string? saleNo, CancellationToken cancellationToken = default)
    {
        await using var uow = _uow.Create();
        await uow.EnsureOpenAsync(cancellationToken).ConfigureAwait(false);
        return await uow.Sales.QueryAsync(from, to, cashierId, saleNo, cancellationToken).ConfigureAwait(false);
    }

    public async Task<Sale?> GetSaleAsync(int id, CancellationToken cancellationToken = default)
    {
        await using var uow = _uow.Create();
        await uow.EnsureOpenAsync(cancellationToken).ConfigureAwait(false);
        return await uow.Sales.GetByIdAsync(id, cancellationToken).ConfigureAwait(false);
    }

    internal static string FormatSaleNo(int sequence) => $"S-{sequence:000000}";
}
