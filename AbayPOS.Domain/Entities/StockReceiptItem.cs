namespace AbayPOS.Domain.Entities;

/// <summary>One product line on a stock receipt.</summary>
public sealed class StockReceiptItem
{
    public int Id { get; set; }
    public int ReceiptId { get; set; }
    public int ProductId { get; set; }
    public string? ProductName { get; set; }
    public int Qty { get; set; }
    public decimal UnitCost { get; set; }
}
