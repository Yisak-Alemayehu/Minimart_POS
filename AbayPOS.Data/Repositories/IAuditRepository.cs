using AbayPOS.Domain.Entities;

namespace AbayPOS.Data.Repositories;

public interface IAuditRepository
{
    Task InsertAsync(AuditLog log, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<AuditLog>> GetRecentAsync(int take, CancellationToken cancellationToken = default);
}
