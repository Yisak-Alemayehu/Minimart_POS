namespace AbayPOS.Domain.Entities;

/// <summary>Sellable minimart item. Money is decimal ETB in the domain.</summary>
public sealed class Product
{
    public int Id { get; set; }
    public string Barcode { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public decimal CostPrice { get; set; }
    public int StockQty { get; set; }
    public int ReorderLevel { get; set; }
    public int? SupplierId { get; set; }
    public bool IsActive { get; set; } = true;
    public string? Emoji { get; set; }
    public string? ImagePath { get; set; }

    /// <summary>True when quantity is at or below the reorder level.</summary>
    public bool IsLowStock => StockQty <= ReorderLevel;

    /// <summary>True when the shelf is empty.</summary>
    public bool IsOutOfStock => StockQty <= 0;
}
