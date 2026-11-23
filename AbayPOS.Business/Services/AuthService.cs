using AbayPOS.Data;
using AbayPOS.Domain.Entities;
using AbayPOS.Domain.Exceptions;
using AbayPOS.Domain.Logging;
using AbayPOS.Domain.Models;
using AbayPOS.Business.Security;

namespace AbayPOS.Business.Services;

/// <summary>Signs operators in and out, locking accounts after five bad passwords.</summary>
public interface IAuthService
{
    Task<Session> LoginAsync(string username, string password, CancellationToken cancellationToken = default);
}

/// <inheritdoc />
public sealed class AuthService : IAuthService
{
    public const int MaxFailedAttempts = 5;
    private readonly IUnitOfWorkFactory _uow;
    private readonly IPasswordHasher _hasher;
    private readonly IAppLogger _logger;

    public AuthService(IUnitOfWorkFactory uow, IPasswordHasher hasher, IAppLogger logger)
    {
        _uow = uow;
        _hasher = hasher;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<Session> LoginAsync(string username, string password, CancellationToken cancellationToken = default)
    {
        username = username.Trim();
        if (username.Length == 0)
            throw new LoginFailedException("Enter your username.", field: "Username");
        if (string.IsNullOrEmpty(password))
            throw new LoginFailedException("Enter your password.");

        await using var uow = _uow.Create();
        await uow.BeginAsync(cancellationToken).ConfigureAwait(false);
        var user = await uow.Users.GetByUsernameAsync(username, cancellationToken).ConfigureAwait(false);
        if (user is null || !user.IsActive)
        {
            await uow.RollbackAsync(cancellationToken).ConfigureAwait(false);
            throw new LoginFailedException("Username or password is incorrect.");
        }

        if (user.IsLocked)
        {
            await uow.RollbackAsync(cancellationToken).ConfigureAwait(false);
            throw new LoginFailedException(
                "This account is locked after too many failed sign-ins. Ask a manager to unlock it.",
                isLocked: true,
                field: "Username");
        }

        if (!_hasher.Verify(password, user.PasswordHash, user.Salt))
        {
            user.FailedAttempts++;
            var locked = user.FailedAttempts >= MaxFailedAttempts;
            user.IsLocked = locked;
            await uow.Users.UpdateLockoutAsync(user.Id, user.FailedAttempts, locked, cancellationToken).ConfigureAwait(false);
            await uow.Audit.InsertAsync(new AuditLog
            {
                UserId = user.Id,
                Action = locked ? "LoginLocked" : "LoginFailed",
                Details = locked
                    ? $"Account locked after {MaxFailedAttempts} failed attempts."
                    : $"Failed sign-in ({user.FailedAttempts}/{MaxFailedAttempts}).",
                CreatedAt = DateTime.Now
            }, cancellationToken).ConfigureAwait(false);
            await uow.CommitAsync(cancellationToken).ConfigureAwait(false);
            if (locked)
                throw new LoginFailedException(
                    "This account is locked after too many failed sign-ins. Ask a manager to unlock it.",
                    isLocked: true);
            throw new LoginFailedException("Username or password is incorrect.");
        }

        user.FailedAttempts = 0;
        user.IsLocked = false;
        await uow.Users.UpdateLockoutAsync(user.Id, 0, false, cancellationToken).ConfigureAwait(false);
        await uow.Audit.InsertAsync(new AuditLog
        {
            UserId = user.Id,
            Action = "Login",
            Details = $"Signed in as {user.Role}.",
            CreatedAt = DateTime.Now
        }, cancellationToken).ConfigureAwait(false);
        await uow.CommitAsync(cancellationToken).ConfigureAwait(false);
        _logger.Info($"User '{user.Username}' signed in.");
        user.PasswordHash = string.Empty;
        user.Salt = string.Empty;
        return new Session(user);
    }
}
