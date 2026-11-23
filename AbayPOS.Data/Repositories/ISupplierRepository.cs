using AbayPOS.Domain.Entities;

namespace AbayPOS.Data.Repositories;

public interface ISupplierRepository
{
    Task<Supplier?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Supplier>> GetAllAsync(bool activeOnly, CancellationToken cancellationToken = default);
    Task<int> InsertAsync(Supplier supplier, CancellationToken cancellationToken = default);
    Task UpdateAsync(Supplier supplier, CancellationToken cancellationToken = default);
}
