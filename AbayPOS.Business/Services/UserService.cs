using AbayPOS.Business.Security;
using AbayPOS.Data;
using AbayPOS.Domain.Entities;
using AbayPOS.Domain.Enums;
using AbayPOS.Domain.Exceptions;
using AbayPOS.Domain.Models;

namespace AbayPOS.Business.Services;

/// <summary>Manager user administration.</summary>
public interface IUserService
{
    Task<IReadOnlyList<User>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<User> CreateAsync(string username, string fullName, string password, UserRole role, Session session, CancellationToken cancellationToken = default);
    Task UpdateAsync(int id, string fullName, UserRole role, bool isActive, Session session, CancellationToken cancellationToken = default);
    Task DeactivateAsync(int id, Session session, CancellationToken cancellationToken = default);
    Task UnlockAsync(int id, Session session, CancellationToken cancellationToken = default);
    Task ResetPasswordAsync(int id, string newPassword, Session session, CancellationToken cancellationToken = default);
}

/// <inheritdoc />
public sealed class UserService : IUserService
{
    private readonly IUnitOfWorkFactory _uow;
    private readonly IPasswordHasher _hasher;

    public UserService(IUnitOfWorkFactory uow, IPasswordHasher hasher)
    {
        _uow = uow;
        _hasher = hasher;
    }

    public async Task<IReadOnlyList<User>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        await using var uow = _uow.Create();
        await uow.EnsureOpenAsync(cancellationToken).ConfigureAwait(false);
        var users = await uow.Users.GetAllAsync(cancellationToken).ConfigureAwait(false);
        foreach (var user in users)
        {
            user.PasswordHash = string.Empty;
            user.Salt = string.Empty;
        }

        return users;
    }

    public async Task<User> CreateAsync(string username, string fullName, string password, UserRole role, Session session, CancellationToken cancellationToken = default)
    {
        RoleGuard.RequireManager(session);
        ValidateCredentials(username, fullName, password);
        await using var uow = _uow.Create();
        await uow.BeginAsync(cancellationToken).ConfigureAwait(false);
        if (await uow.Users.GetByUsernameAsync(username.Trim(), cancellationToken).ConfigureAwait(false) is not null)
            throw new DomainValidationException("That username is already taken.", "Username");
        var (hash, salt) = _hasher.Hash(password);
        var user = new User
        {
            Username = username.Trim(),
            FullName = fullName.Trim(),
            PasswordHash = hash,
            Salt = salt,
            Role = role,
            IsActive = true,
            CreatedAt = DateTime.Now
        };
        await uow.Users.InsertAsync(user, cancellationToken).ConfigureAwait(false);
        await uow.Audit.InsertAsync(new AuditLog
        {
            UserId = session.User.Id,
            Action = "UserCreated",
            Details = user.Username,
            CreatedAt = DateTime.Now
        }, cancellationToken).ConfigureAwait(false);
        await uow.CommitAsync(cancellationToken).ConfigureAwait(false);
        user.PasswordHash = string.Empty;
        user.Salt = string.Empty;
        return user;
    }

    public async Task UpdateAsync(int id, string fullName, UserRole role, bool isActive, Session session, CancellationToken cancellationToken = default)
    {
        RoleGuard.RequireManager(session);
        if (string.IsNullOrWhiteSpace(fullName))
            throw new DomainValidationException("Enter a full name.", "FullName");
        await using var uow = _uow.Create();
        await uow.BeginAsync(cancellationToken).ConfigureAwait(false);
        var user = await uow.Users.GetByIdAsync(id, cancellationToken).ConfigureAwait(false)
            ?? throw new DomainValidationException("User was not found.");
        user.FullName = fullName.Trim();
        user.Role = role;
        user.IsActive = isActive;
        await uow.Users.UpdateAsync(user, cancellationToken).ConfigureAwait(false);
        await uow.CommitAsync(cancellationToken).ConfigureAwait(false);
    }

    public async Task DeactivateAsync(int id, Session session, CancellationToken cancellationToken = default)
    {
        RoleGuard.RequireManager(session);
        if (id == session.User.Id)
            throw new DomainValidationException("You cannot deactivate your own account.");
        await using var uow = _uow.Create();
        await uow.BeginAsync(cancellationToken).ConfigureAwait(false);
        var user = await uow.Users.GetByIdAsync(id, cancellationToken).ConfigureAwait(false)
            ?? throw new DomainValidationException("User was not found.");
        user.IsActive = false;
        await uow.Users.UpdateAsync(user, cancellationToken).ConfigureAwait(false);
        await uow.Audit.InsertAsync(new AuditLog
        {
            UserId = session.User.Id,
            Action = "UserDeactivated",
            Details = user.Username,
            CreatedAt = DateTime.Now
        }, cancellationToken).ConfigureAwait(false);
        await uow.CommitAsync(cancellationToken).ConfigureAwait(false);
    }

    public async Task UnlockAsync(int id, Session session, CancellationToken cancellationToken = default)
    {
        RoleGuard.RequireManager(session);
        await using var uow = _uow.Create();
        await uow.BeginAsync(cancellationToken).ConfigureAwait(false);
        await uow.Users.UpdateLockoutAsync(id, 0, false, cancellationToken).ConfigureAwait(false);
        await uow.Audit.InsertAsync(new AuditLog
        {
            UserId = session.User.Id,
            Action = "UserUnlocked",
            Details = $"UserId {id}",
            CreatedAt = DateTime.Now
        }, cancellationToken).ConfigureAwait(false);
        await uow.CommitAsync(cancellationToken).ConfigureAwait(false);
    }

    public async Task ResetPasswordAsync(int id, string newPassword, Session session, CancellationToken cancellationToken = default)
    {
        RoleGuard.RequireManager(session);
        if (string.IsNullOrWhiteSpace(newPassword) || newPassword.Length < 6)
            throw new DomainValidationException("Use a password of at least 6 characters.", "Password");
        await using var uow = _uow.Create();
        await uow.BeginAsync(cancellationToken).ConfigureAwait(false);
        var user = await uow.Users.GetByIdAsync(id, cancellationToken).ConfigureAwait(false)
            ?? throw new DomainValidationException("User was not found.");
        var (hash, salt) = _hasher.Hash(newPassword);
        user.PasswordHash = hash;
        user.Salt = salt;
        user.FailedAttempts = 0;
        user.IsLocked = false;
        await uow.Users.UpdateAsync(user, cancellationToken).ConfigureAwait(false);
        await uow.Audit.InsertAsync(new AuditLog
        {
            UserId = session.User.Id,
            Action = "PasswordReset",
            Details = user.Username,
            CreatedAt = DateTime.Now
        }, cancellationToken).ConfigureAwait(false);
        await uow.CommitAsync(cancellationToken).ConfigureAwait(false);
    }

    private static void ValidateCredentials(string username, string fullName, string password)
    {
        if (string.IsNullOrWhiteSpace(username))
            throw new DomainValidationException("Enter a username.", "Username");
        if (string.IsNullOrWhiteSpace(fullName))
            throw new DomainValidationException("Enter a full name.", "FullName");
        if (string.IsNullOrWhiteSpace(password) || password.Length < 6)
            throw new DomainValidationException("Use a password of at least 6 characters.", "Password");
    }
}
