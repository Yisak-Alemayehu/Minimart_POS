using AbayPOS.Domain.Entities;

namespace AbayPOS.Data.Repositories;

public interface IProductRepository
{
    Task<Product?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<Product?> GetByBarcodeAsync(string barcode, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Product>> GetActiveAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Product>> SearchAsync(string? query, string? category, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<string>> GetCategoriesAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Product>> GetLowStockAsync(CancellationToken cancellationToken = default);
    Task<bool> BarcodeExistsAsync(string barcode, int? exceptId = null, CancellationToken cancellationToken = default);
    Task<int> InsertAsync(Product product, CancellationToken cancellationToken = default);
    Task UpdateAsync(Product product, CancellationToken cancellationToken = default);
    Task DeactivateAsync(int id, CancellationToken cancellationToken = default);
    Task<bool> HasSalesAsync(int productId, CancellationToken cancellationToken = default);

    /// <summary>Decrements stock only when enough units remain. Returns rows affected (0 or 1).</summary>
    Task<int> TryDecrementStockAsync(int productId, int qty, CancellationToken cancellationToken = default);

    Task IncrementStockAsync(int productId, int qty, CancellationToken cancellationToken = default);
}
