using AbayPOS.Domain.Entities;
using AbayPOS.Domain.Enums;

namespace AbayPOS.Domain.Models;

/// <summary>Signed-in operator for the current UI session.</summary>
public sealed class Session
{
    public Session(User user) => User = user ?? throw new ArgumentNullException(nameof(user));

    /// <summary>The authenticated user snapshot.</summary>
    public User User { get; }

    /// <summary>Role copied from the user for convenient checks.</summary>
    public UserRole Role => User.Role;

    /// <summary>True when the operator may run manager-only use cases.</summary>
    public bool IsManager => Role == UserRole.Manager;
}
