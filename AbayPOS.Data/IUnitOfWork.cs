using AbayPOS.Data.Repositories;

namespace AbayPOS.Data;

/// <summary>One connection and optional transaction shared by every repository in a use case.</summary>
public interface IUnitOfWork : IAsyncDisposable
{
    IUserRepository Users { get; }
    IProductRepository Products { get; }
    ISupplierRepository Suppliers { get; }
    ISaleRepository Sales { get; }
    IStockRepository Stock { get; }
    IAuditRepository Audit { get; }
    IReportRepository Reports { get; }

    /// <summary>Opens the connection and applies SQLite PRAGMAs without starting a transaction.</summary>
    Task EnsureOpenAsync(CancellationToken cancellationToken = default);

    /// <summary>Opens the connection and starts <c>BEGIN IMMEDIATE</c> (SQLite serializable).</summary>
    Task BeginAsync(CancellationToken cancellationToken = default);

    Task CommitAsync(CancellationToken cancellationToken = default);

    Task RollbackAsync(CancellationToken cancellationToken = default);
}

/// <summary>Creates a short-lived unit of work. Dispose after each use case.</summary>
public interface IUnitOfWorkFactory
{
    IUnitOfWork Create();
}
