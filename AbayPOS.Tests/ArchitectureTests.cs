using AbayPOS.Domain;
using AbayPOS.Domain.Models;

namespace AbayPOS.Tests;

public sealed class ArchitectureTests
{
    [Fact]
    public void Money_round_trips_cents()
    {
        Assert.Equal(4500, Money.ToCents(45m));
        Assert.Equal(45m, Money.FromCents(4500));
        Assert.Equal("ETB 1,000.00", Money.Format(1000m));
    }

    [Fact]
    public void Cart_computes_percent_discount()
    {
        var cart = new Cart();
        cart.MutableLines.Add(new CartLine { ProductName = "Milk 1L", UnitPrice = 85m, Qty = 2 });
        cart.DiscountIsPercent = true;
        cart.DiscountValue = 10m;
        Assert.Equal(170m, cart.Subtotal);
        Assert.Equal(17m, cart.DiscountAmount);
        Assert.Equal(153m, cart.Total);
    }
}
