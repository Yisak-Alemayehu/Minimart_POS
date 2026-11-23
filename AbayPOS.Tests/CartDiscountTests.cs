using AbayPOS.Business.Services;
using AbayPOS.Domain.Entities;
using AbayPOS.Domain.Enums;
using AbayPOS.Domain.Exceptions;
using AbayPOS.Domain.Models;

namespace AbayPOS.Tests;

public sealed class CartDiscountTests
{
    private static Session Cashier() => new(new User { Id = 2, Username = "cashier", FullName = "Abebe Kebede", Role = UserRole.Cashier });
    private static Session Manager() => new(new User { Id = 1, Username = "admin", FullName = "Store Manager", Role = UserRole.Manager });

    [Fact]
    public void Totals_include_line_qty()
    {
        var sales = new SalesService(null!, null!);
        var cart = sales.CreateCart();
        sales.AddProduct(cart, new Product { Id = 1, Name = "Milk 1L", Barcode = "1003", Price = 85m, StockQty = 10 }, 2);
        Assert.Equal(170m, cart.Subtotal);
        Assert.Equal(2, cart.ItemCount);
        Assert.Equal(170m, cart.Total);
    }

    [Fact]
    public void Cashier_cannot_exceed_ten_percent()
    {
        var sales = new SalesService(null!, null!);
        var cart = sales.CreateCart();
        sales.AddProduct(cart, new Product { Id = 1, Name = "Rice 5kg", Barcode = "1004", Price = 650m, StockQty = 10 });
        Assert.Throws<DomainValidationException>(() => sales.ApplyDiscount(cart, 15m, true, Cashier()));
        sales.ApplyDiscount(cart, 10m, true, Cashier());
        Assert.Equal(65m, cart.DiscountAmount);
        Assert.Equal(585m, cart.Total);
    }

    [Fact]
    public void Manager_can_apply_any_percent()
    {
        var sales = new SalesService(null!, null!);
        var cart = sales.CreateCart();
        sales.AddProduct(cart, new Product { Id = 1, Name = "Rice 5kg", Barcode = "1004", Price = 650m, StockQty = 10 });
        sales.ApplyDiscount(cart, 50m, true, Manager());
        Assert.Equal(325m, cart.DiscountAmount);
    }

    [Fact]
    public void Amount_discount_is_capped_at_subtotal()
    {
        var cart = new Cart();
        cart.MutableLines.Add(new CartLine { ProductName = "Soap", UnitPrice = 40m, Qty = 1 });
        cart.DiscountIsPercent = false;
        cart.DiscountValue = 100m;
        Assert.Equal(40m, cart.DiscountAmount);
        Assert.Equal(0m, cart.Total);
    }
}
