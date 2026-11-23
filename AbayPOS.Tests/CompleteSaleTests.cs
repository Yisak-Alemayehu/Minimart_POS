using AbayPOS.Business.Services;
using AbayPOS.Domain.Entities;
using AbayPOS.Domain.Enums;
using AbayPOS.Domain.Exceptions;
using AbayPOS.Domain.Models;

namespace AbayPOS.Tests;

public sealed class CompleteSaleTests
{
    [Fact]
    public async Task CompleteSale_rolls_back_when_stock_is_insufficient()
    {
        var host = await TestHost.CreateAsync();
        var session = await host.Auth.LoginAsync("cashier", "cashier123");
        var product = await host.Products.GetByBarcodeAsync("1003");
        Assert.NotNull(product);
        var originalStock = product!.StockQty;
        Assert.True(originalStock < 50, "seed milk should be a small quantity");

        var cart = host.Sales.CreateCart();
        cart.MutableLines.Add(new CartLine
        {
            ProductId = product.Id,
            ProductName = product.Name,
            Barcode = product.Barcode,
            UnitPrice = product.Price,
            Qty = originalStock + 5,
            AvailableStock = originalStock
        });

        await Assert.ThrowsAsync<InsufficientStockException>(() =>
            host.Sales.CompleteSaleAsync(cart, new PaymentRequest
            {
                Method = PaymentMethod.Cash,
                AmountPaid = 10_000m
            }, session));

        var after = await host.Products.GetByIdAsync(product.Id);
        Assert.Equal(originalStock, after!.StockQty);
        var history = await host.Sales.GetHistoryAsync(DateTime.Now.Date, DateTime.Now.Date, session.User.Id, null);
        Assert.Empty(history);
        Assert.NotEmpty(cart.Lines);
    }

    [Fact]
    public async Task CompleteSale_decrements_stock_and_writes_sale()
    {
        var host = await TestHost.CreateAsync();
        var session = await host.Auth.LoginAsync("cashier", "cashier123");
        var product = await host.Products.GetByBarcodeAsync("1001");
        Assert.NotNull(product);
        var original = product!.StockQty;

        var cart = host.Sales.CreateCart();
        host.Sales.AddProduct(cart, product, 2);
        var sale = await host.Sales.CompleteSaleAsync(cart, new PaymentRequest
        {
            Method = PaymentMethod.Cash,
            AmountPaid = product.Price * 2
        }, session);

        Assert.StartsWith("S-", sale.SaleNo);
        Assert.Equal(SaleStatus.Completed, sale.Status);
        var after = await host.Products.GetByIdAsync(product.Id);
        Assert.Equal(original - 2, after!.StockQty);
        Assert.Empty(cart.Lines);
    }

    [Fact]
    public async Task Cashier_cannot_void_a_sale()
    {
        var host = await TestHost.CreateAsync();
        var session = await host.Auth.LoginAsync("cashier", "cashier123");
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => host.Sales.VoidSaleAsync(1, session));
    }
}
