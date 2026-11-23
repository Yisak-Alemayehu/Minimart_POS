using System.Data;
using System.Data.Common;
using AbayPOS.Data.Repositories;
using AbayPOS.Data.Sqlite.Repositories;

namespace AbayPOS.Data.Sqlite;

/// <summary>SQLite unit of work. <see cref="BeginAsync"/> uses serializable isolation (BEGIN IMMEDIATE).</summary>
public sealed class SqliteUnitOfWork : IUnitOfWork
{
    private readonly IDbConnectionFactory _factory;
    private readonly SqliteSession _session;
    private bool _disposed;

    public SqliteUnitOfWork(IDbConnectionFactory factory)
    {
        _factory = factory ?? throw new ArgumentNullException(nameof(factory));
        _session = new SqliteSession(_factory.Create());
        Users = new SqliteUserRepository(_session);
        Products = new SqliteProductRepository(_session);
        Suppliers = new SqliteSupplierRepository(_session);
        Sales = new SqliteSaleRepository(_session);
        Stock = new SqliteStockRepository(_session);
        Audit = new SqliteAuditRepository(_session);
        Reports = new SqliteReportRepository(_session);
    }

    public IUserRepository Users { get; }
    public IProductRepository Products { get; }
    public ISupplierRepository Suppliers { get; }
    public ISaleRepository Sales { get; }
    public IStockRepository Stock { get; }
    public IAuditRepository Audit { get; }
    public IReportRepository Reports { get; }

    /// <inheritdoc />
    public async Task EnsureOpenAsync(CancellationToken cancellationToken = default)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        if (_session.Connection.State != ConnectionState.Open)
        {
            await _session.Connection.OpenAsync(cancellationToken).ConfigureAwait(false);
            await SqliteSession.ApplyPragmasAsync(_session.Connection, cancellationToken).ConfigureAwait(false);
        }
    }

    /// <inheritdoc />
    public async Task BeginAsync(CancellationToken cancellationToken = default)
    {
        await EnsureOpenAsync(cancellationToken).ConfigureAwait(false);
        if (_session.Transaction is not null)
            return;
        _session.Transaction = await _session.Connection
            .BeginTransactionAsync(IsolationLevel.Serializable, cancellationToken)
            .ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task CommitAsync(CancellationToken cancellationToken = default)
    {
        if (_session.Transaction is null)
            return;
        await _session.Transaction.CommitAsync(cancellationToken).ConfigureAwait(false);
        await _session.Transaction.DisposeAsync().ConfigureAwait(false);
        _session.Transaction = null;
    }

    /// <inheritdoc />
    public async Task RollbackAsync(CancellationToken cancellationToken = default)
    {
        if (_session.Transaction is null)
            return;
        await _session.Transaction.RollbackAsync(cancellationToken).ConfigureAwait(false);
        await _session.Transaction.DisposeAsync().ConfigureAwait(false);
        _session.Transaction = null;
    }

    public async ValueTask DisposeAsync()
    {
        if (_disposed)
            return;
        _disposed = true;
        try
        {
            await RollbackAsync().ConfigureAwait(false);
        }
        finally
        {
            await _session.Connection.DisposeAsync().ConfigureAwait(false);
        }
    }
}

/// <summary>Creates a new SQLite unit of work per use case.</summary>
public sealed class SqliteUnitOfWorkFactory : IUnitOfWorkFactory
{
    private readonly IDbConnectionFactory _factory;

    public SqliteUnitOfWorkFactory(IDbConnectionFactory factory) =>
        _factory = factory ?? throw new ArgumentNullException(nameof(factory));

    public IUnitOfWork Create() => new SqliteUnitOfWork(_factory);
}
