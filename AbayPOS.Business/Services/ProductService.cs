using AbayPOS.Data;
using AbayPOS.Domain.Entities;
using AbayPOS.Domain.Exceptions;
using AbayPOS.Domain.Logging;
using AbayPOS.Domain.Models;

namespace AbayPOS.Business.Services;

/// <summary>Product catalog use cases.</summary>
public interface IProductService
{
    Task<IReadOnlyList<Product>> GetActiveAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Product>> SearchAsync(string? query, string? category, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<string>> GetCategoriesAsync(CancellationToken cancellationToken = default);
    Task<Product?> GetByBarcodeAsync(string barcode, CancellationToken cancellationToken = default);
    Task<Product?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<Product> CreateAsync(Product product, Session session, CancellationToken cancellationToken = default);
    Task UpdateAsync(Product product, Session session, CancellationToken cancellationToken = default);
    Task DeactivateAsync(int id, Session session, CancellationToken cancellationToken = default);
}

/// <inheritdoc />
public sealed class ProductService : IProductService
{
    private readonly IUnitOfWorkFactory _uow;
    private readonly IAppLogger _logger;

    public ProductService(IUnitOfWorkFactory uow, IAppLogger logger)
    {
        _uow = uow;
        _logger = logger;
    }

    public async Task<IReadOnlyList<Product>> GetActiveAsync(CancellationToken cancellationToken = default)
    {
        await using var uow = _uow.Create();
        await uow.EnsureOpenAsync(cancellationToken).ConfigureAwait(false);
        return await uow.Products.GetActiveAsync(cancellationToken).ConfigureAwait(false);
    }

    public async Task<IReadOnlyList<Product>> SearchAsync(string? query, string? category, CancellationToken cancellationToken = default)
    {
        await using var uow = _uow.Create();
        await uow.EnsureOpenAsync(cancellationToken).ConfigureAwait(false);
        return await uow.Products.SearchAsync(query, category, cancellationToken).ConfigureAwait(false);
    }

    public async Task<IReadOnlyList<string>> GetCategoriesAsync(CancellationToken cancellationToken = default)
    {
        await using var uow = _uow.Create();
        await uow.EnsureOpenAsync(cancellationToken).ConfigureAwait(false);
        return await uow.Products.GetCategoriesAsync(cancellationToken).ConfigureAwait(false);
    }

    public async Task<Product?> GetByBarcodeAsync(string barcode, CancellationToken cancellationToken = default)
    {
        await using var uow = _uow.Create();
        await uow.EnsureOpenAsync(cancellationToken).ConfigureAwait(false);
        return await uow.Products.GetByBarcodeAsync(barcode.Trim(), cancellationToken).ConfigureAwait(false);
    }

    public async Task<Product?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        await using var uow = _uow.Create();
        await uow.EnsureOpenAsync(cancellationToken).ConfigureAwait(false);
        return await uow.Products.GetByIdAsync(id, cancellationToken).ConfigureAwait(false);
    }

    public async Task<Product> CreateAsync(Product product, Session session, CancellationToken cancellationToken = default)
    {
        RoleGuard.RequireManager(session);
        Validate(product);
        await using var uow = _uow.Create();
        await uow.BeginAsync(cancellationToken).ConfigureAwait(false);
        if (await uow.Products.BarcodeExistsAsync(product.Barcode, null, cancellationToken).ConfigureAwait(false))
            throw new DomainValidationException("That barcode is already used by another product.", nameof(product.Barcode));
        await uow.Products.InsertAsync(product, cancellationToken).ConfigureAwait(false);
        await uow.Audit.InsertAsync(new AuditLog
        {
            UserId = session.User.Id,
            Action = "ProductCreated",
            Details = $"{product.Name} ({product.Barcode})",
            CreatedAt = DateTime.Now
        }, cancellationToken).ConfigureAwait(false);
        await uow.CommitAsync(cancellationToken).ConfigureAwait(false);
        _logger.Info($"Product '{product.Name}' created.");
        return product;
    }

    public async Task UpdateAsync(Product product, Session session, CancellationToken cancellationToken = default)
    {
        RoleGuard.RequireManager(session);
        Validate(product);
        await using var uow = _uow.Create();
        await uow.BeginAsync(cancellationToken).ConfigureAwait(false);
        if (await uow.Products.BarcodeExistsAsync(product.Barcode, product.Id, cancellationToken).ConfigureAwait(false))
            throw new DomainValidationException("That barcode is already used by another product.", nameof(product.Barcode));
        await uow.Products.UpdateAsync(product, cancellationToken).ConfigureAwait(false);
        await uow.Audit.InsertAsync(new AuditLog
        {
            UserId = session.User.Id,
            Action = "ProductUpdated",
            Details = $"{product.Name} ({product.Barcode})",
            CreatedAt = DateTime.Now
        }, cancellationToken).ConfigureAwait(false);
        await uow.CommitAsync(cancellationToken).ConfigureAwait(false);
    }

    public async Task DeactivateAsync(int id, Session session, CancellationToken cancellationToken = default)
    {
        RoleGuard.RequireManager(session);
        await using var uow = _uow.Create();
        await uow.BeginAsync(cancellationToken).ConfigureAwait(false);
        var existing = await uow.Products.GetByIdAsync(id, cancellationToken).ConfigureAwait(false)
            ?? throw new DomainValidationException("Product was not found.");
        await uow.Products.DeactivateAsync(id, cancellationToken).ConfigureAwait(false);
        await uow.Audit.InsertAsync(new AuditLog
        {
            UserId = session.User.Id,
            Action = "ProductDeactivated",
            Details = existing.Name,
            CreatedAt = DateTime.Now
        }, cancellationToken).ConfigureAwait(false);
        await uow.CommitAsync(cancellationToken).ConfigureAwait(false);
    }

    internal static void Validate(Product product)
    {
        if (string.IsNullOrWhiteSpace(product.Name))
            throw new DomainValidationException("Enter a product name.", nameof(product.Name));
        if (string.IsNullOrWhiteSpace(product.Barcode))
            throw new DomainValidationException("Enter a barcode.", nameof(product.Barcode));
        if (string.IsNullOrWhiteSpace(product.Category))
            throw new DomainValidationException("Choose a category.", nameof(product.Category));
        if (product.Price <= 0)
            throw new DomainValidationException("Price must be greater than zero.", nameof(product.Price));
        if (product.CostPrice < 0)
            throw new DomainValidationException("Cost cannot be negative.", nameof(product.CostPrice));
        if (product.StockQty < 0)
            throw new DomainValidationException("Stock cannot be negative.", nameof(product.StockQty));
        if (product.ReorderLevel < 0)
            throw new DomainValidationException("Reorder level cannot be negative.", nameof(product.ReorderLevel));
        product.Name = product.Name.Trim();
        product.Barcode = product.Barcode.Trim();
        product.Category = product.Category.Trim();
    }
}
