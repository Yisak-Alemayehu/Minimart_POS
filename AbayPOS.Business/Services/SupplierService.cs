using AbayPOS.Data;
using AbayPOS.Domain.Entities;
using AbayPOS.Domain.Exceptions;
using AbayPOS.Domain.Models;

namespace AbayPOS.Business.Services;

/// <summary>Supplier CRUD for managers.</summary>
public interface ISupplierService
{
    Task<IReadOnlyList<Supplier>> GetAllAsync(bool activeOnly, CancellationToken cancellationToken = default);
    Task<Supplier?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<Supplier> CreateAsync(Supplier supplier, Session session, CancellationToken cancellationToken = default);
    Task UpdateAsync(Supplier supplier, Session session, CancellationToken cancellationToken = default);
}

/// <inheritdoc />
public sealed class SupplierService : ISupplierService
{
    private readonly IUnitOfWorkFactory _uow;

    public SupplierService(IUnitOfWorkFactory uow) => _uow = uow;

    public async Task<IReadOnlyList<Supplier>> GetAllAsync(bool activeOnly, CancellationToken cancellationToken = default)
    {
        await using var uow = _uow.Create();
        await uow.EnsureOpenAsync(cancellationToken).ConfigureAwait(false);
        return await uow.Suppliers.GetAllAsync(activeOnly, cancellationToken).ConfigureAwait(false);
    }

    public async Task<Supplier?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        await using var uow = _uow.Create();
        await uow.EnsureOpenAsync(cancellationToken).ConfigureAwait(false);
        return await uow.Suppliers.GetByIdAsync(id, cancellationToken).ConfigureAwait(false);
    }

    public async Task<Supplier> CreateAsync(Supplier supplier, Session session, CancellationToken cancellationToken = default)
    {
        RoleGuard.RequireManager(session);
        Validate(supplier);
        await using var uow = _uow.Create();
        await uow.BeginAsync(cancellationToken).ConfigureAwait(false);
        await uow.Suppliers.InsertAsync(supplier, cancellationToken).ConfigureAwait(false);
        await uow.Audit.InsertAsync(new AuditLog
        {
            UserId = session.User.Id,
            Action = "SupplierCreated",
            Details = supplier.Name,
            CreatedAt = DateTime.Now
        }, cancellationToken).ConfigureAwait(false);
        await uow.CommitAsync(cancellationToken).ConfigureAwait(false);
        return supplier;
    }

    public async Task UpdateAsync(Supplier supplier, Session session, CancellationToken cancellationToken = default)
    {
        RoleGuard.RequireManager(session);
        Validate(supplier);
        await using var uow = _uow.Create();
        await uow.BeginAsync(cancellationToken).ConfigureAwait(false);
        await uow.Suppliers.UpdateAsync(supplier, cancellationToken).ConfigureAwait(false);
        await uow.Audit.InsertAsync(new AuditLog
        {
            UserId = session.User.Id,
            Action = "SupplierUpdated",
            Details = supplier.Name,
            CreatedAt = DateTime.Now
        }, cancellationToken).ConfigureAwait(false);
        await uow.CommitAsync(cancellationToken).ConfigureAwait(false);
    }

    private static void Validate(Supplier supplier)
    {
        if (string.IsNullOrWhiteSpace(supplier.Name))
            throw new DomainValidationException("Enter a supplier name.", nameof(supplier.Name));
        supplier.Name = supplier.Name.Trim();
    }
}
