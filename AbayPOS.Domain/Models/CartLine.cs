namespace AbayPOS.Domain.Models;

/// <summary>In-memory sale line. Not persisted until checkout completes.</summary>
public sealed class CartLine
{
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string Barcode { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string? Emoji { get; set; }
    public decimal UnitPrice { get; set; }
    public int Qty { get; set; }
    public int AvailableStock { get; set; }

    /// <summary>Unit price times quantity.</summary>
    public decimal LineTotal => decimal.Round(UnitPrice * Qty, 2, MidpointRounding.AwayFromZero);
}
