using AbayPOS.Domain.Enums;

namespace AbayPOS.Domain.Entities;

/// <summary>Completed or voided checkout transaction.</summary>
public sealed class Sale
{
    public int Id { get; set; }
    public string SaleNo { get; set; } = string.Empty;
    public int CashierId { get; set; }
    public string? CashierName { get; set; }
    public DateTime SaleDate { get; set; }
    public decimal Subtotal { get; set; }
    public decimal Discount { get; set; }
    public decimal Total { get; set; }
    public PaymentMethod PaymentMethod { get; set; }
    public decimal AmountPaid { get; set; }
    public decimal Change { get; set; }
    public SaleStatus Status { get; set; } = SaleStatus.Completed;
    public IList<SaleItem> Items { get; set; } = new List<SaleItem>();
}
