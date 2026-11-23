using AbayPOS.Domain.Enums;

namespace AbayPOS.Domain.Entities;

/// <summary>Store operator who can sign in to AbayPOS.</summary>
public sealed class User
{
    public int Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string Salt { get; set; } = string.Empty;
    public UserRole Role { get; set; }
    public int FailedAttempts { get; set; }
    public bool IsLocked { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.Now;
}
