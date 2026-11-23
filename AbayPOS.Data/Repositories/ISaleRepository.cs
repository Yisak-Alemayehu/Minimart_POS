using AbayPOS.Domain.Entities;
using AbayPOS.Domain.Enums;

namespace AbayPOS.Data.Repositories;

public interface ISaleRepository
{
    Task<int> NextSequenceAsync(CancellationToken cancellationToken = default);
    Task<int> InsertAsync(Sale sale, CancellationToken cancellationToken = default);
    Task InsertItemAsync(SaleItem item, CancellationToken cancellationToken = default);
    Task<Sale?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<Sale?> GetBySaleNoAsync(string saleNo, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Sale>> QueryAsync(DateTime? from, DateTime? to, int? cashierId, string? saleNo, CancellationToken cancellationToken = default);
    Task UpdateStatusAsync(int saleId, SaleStatus status, CancellationToken cancellationToken = default);
}
