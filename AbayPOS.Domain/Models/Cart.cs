namespace AbayPOS.Domain.Models;

/// <summary>Working sale being built at the register.</summary>
public sealed class Cart
{
    private readonly List<CartLine> _lines = [];

    /// <summary>Lines currently in the cart.</summary>
    public IReadOnlyList<CartLine> Lines => _lines;

    /// <summary>Discount entered by the cashier or manager. Amount in ETB, or a percent when <see cref="DiscountIsPercent"/> is true.</summary>
    public decimal DiscountValue { get; set; }

    /// <summary>True when <see cref="DiscountValue"/> is a percentage (0–100).</summary>
    public bool DiscountIsPercent { get; set; }

    /// <summary>Sum of line totals before discount.</summary>
    public decimal Subtotal => decimal.Round(_lines.Sum(line => line.LineTotal), 2, MidpointRounding.AwayFromZero);

    /// <summary>Resolved discount in ETB, never exceeding the subtotal.</summary>
    public decimal DiscountAmount
    {
        get
        {
            var raw = DiscountIsPercent
                ? Subtotal * DiscountValue / 100m
                : DiscountValue;
            raw = decimal.Round(raw, 2, MidpointRounding.AwayFromZero);
            return Math.Clamp(raw, 0m, Subtotal);
        }
    }

    /// <summary>Amount due after discount.</summary>
    public decimal Total => Math.Max(0m, Subtotal - DiscountAmount);

    /// <summary>Total units in the cart.</summary>
    public int ItemCount => _lines.Sum(line => line.Qty);

    /// <summary>Adds or returns the mutable list used by the sales service.</summary>
    public List<CartLine> MutableLines => _lines;

    /// <summary>Removes every line and any discount.</summary>
    public void Clear()
    {
        _lines.Clear();
        DiscountValue = 0m;
        DiscountIsPercent = false;
    }
}
