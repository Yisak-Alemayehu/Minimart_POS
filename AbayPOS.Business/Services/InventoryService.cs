using AbayPOS.Data;
using AbayPOS.Domain.Entities;
using AbayPOS.Domain.Exceptions;
using AbayPOS.Domain.Logging;
using AbayPOS.Domain.Models;

namespace AbayPOS.Business.Services;

/// <summary>One line on a goods-received note.</summary>
public sealed class ReceiptLine
{
    public int ProductId { get; set; }
    public int Qty { get; set; }
    public decimal UnitCost { get; set; }
}

/// <summary>Receive stock and list items at or below reorder level.</summary>
public interface IInventoryService
{
    Task ReceiveStockAsync(int supplierId, IReadOnlyList<ReceiptLine> lines, string? note, Session session, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Product>> GetLowStockAsync(CancellationToken cancellationToken = default);
}

/// <inheritdoc />
public sealed class InventoryService : IInventoryService
{
    private readonly IUnitOfWorkFactory _uow;
    private readonly IAppLogger _logger;

    public InventoryService(IUnitOfWorkFactory uow, IAppLogger logger)
    {
        _uow = uow;
        _logger = logger;
    }

    public async Task ReceiveStockAsync(int supplierId, IReadOnlyList<ReceiptLine> lines, string? note, Session session, CancellationToken cancellationToken = default)
    {
        RoleGuard.RequireManager(session);
        if (lines is null || lines.Count == 0)
            throw new DomainValidationException("Add at least one product to the receipt.");
        if (lines.Any(line => line.Qty <= 0))
            throw new DomainValidationException("Quantity must be at least 1.");
        if (lines.Any(line => line.UnitCost < 0))
            throw new DomainValidationException("Unit cost cannot be negative.");

        await using var uow = _uow.Create();
        await uow.BeginAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            var supplier = await uow.Suppliers.GetByIdAsync(supplierId, cancellationToken).ConfigureAwait(false)
                ?? throw new DomainValidationException("Choose a supplier.");
            var receipt = new StockReceipt
            {
                SupplierId = supplier.Id,
                ReceivedBy = session.User.Id,
                ReceivedAt = DateTime.Now,
                Note = note
            };
            await uow.Stock.InsertReceiptAsync(receipt, cancellationToken).ConfigureAwait(false);
            foreach (var line in lines)
            {
                await uow.Stock.InsertReceiptItemAsync(new StockReceiptItem
                {
                    ReceiptId = receipt.Id,
                    ProductId = line.ProductId,
                    Qty = line.Qty,
                    UnitCost = line.UnitCost
                }, cancellationToken).ConfigureAwait(false);
                await uow.Products.IncrementStockAsync(line.ProductId, line.Qty, cancellationToken).ConfigureAwait(false);
            }

            await uow.Audit.InsertAsync(new AuditLog
            {
                UserId = session.User.Id,
                Action = "StockReceived",
                Details = $"{lines.Count} lines from {supplier.Name}",
                CreatedAt = DateTime.Now
            }, cancellationToken).ConfigureAwait(false);
            await uow.CommitAsync(cancellationToken).ConfigureAwait(false);
            _logger.Info($"Stock received from {supplier.Name}.");
        }
        catch (Exception ex) when (ex is not DomainValidationException and not UnauthorizedAccessException)
        {
            await uow.RollbackAsync(cancellationToken).ConfigureAwait(false);
            throw;
        }
    }

    public async Task<IReadOnlyList<Product>> GetLowStockAsync(CancellationToken cancellationToken = default)
    {
        await using var uow = _uow.Create();
        await uow.EnsureOpenAsync(cancellationToken).ConfigureAwait(false);
        return await uow.Products.GetLowStockAsync(cancellationToken).ConfigureAwait(false);
    }
}
