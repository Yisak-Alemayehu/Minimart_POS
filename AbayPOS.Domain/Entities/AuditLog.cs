namespace AbayPOS.Domain.Entities;

/// <summary>Security and operations trail written by services.</summary>
public sealed class AuditLog
{
    public int Id { get; set; }
    public int? UserId { get; set; }
    public string? Username { get; set; }
    public string Action { get; set; } = string.Empty;
    public string? Details { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.Now;
}
