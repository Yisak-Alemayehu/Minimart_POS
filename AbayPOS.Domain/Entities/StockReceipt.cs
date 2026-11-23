namespace AbayPOS.Domain.Entities;

/// <summary>Goods-received note from a supplier.</summary>
public sealed class StockReceipt
{
    public int Id { get; set; }
    public int SupplierId { get; set; }
    public string? SupplierName { get; set; }
    public int ReceivedBy { get; set; }
    public string? ReceivedByName { get; set; }
    public DateTime ReceivedAt { get; set; }
    public string? Note { get; set; }
    public IList<StockReceiptItem> Items { get; set; } = new List<StockReceiptItem>();
}
