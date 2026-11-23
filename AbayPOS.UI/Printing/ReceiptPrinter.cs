using System.Drawing.Printing;
using AbayPOS.Domain;
using AbayPOS.Domain.Entities;

namespace AbayPOS.UI.Printing;

/// <summary>80mm thermal layout drawn with PrintDocument.</summary>
public sealed class ReceiptPrinter
{
    public const string StoreName = "Abay Mini Mart";
    public const string StoreAddress = "Unity University · Addis Ababa";

    public void Print(Sale sale)
    {
        using var document = CreateDocument(sale);
        document.Print();
    }

    public PrintDocument CreateDocument(Sale sale)
    {
        var document = new PrintDocument { DocumentName = sale.SaleNo };
        document.DefaultPageSettings.PaperSize = new PaperSize("80mm", 315, 2000);
        document.DefaultPageSettings.Margins = new Margins(10, 10, 10, 10);
        document.PrintPage += (_, e) => Draw(e, sale);
        return document;
    }

    private static void Draw(PrintPageEventArgs e, Sale sale)
    {
        var g = e.Graphics;
        if (g is null)
            return;
        var y = 8f;
        var width = e.MarginBounds.Width;
        using var title = new Font("Consolas", 12, FontStyle.Bold);
        using var body = new Font("Consolas", 8);
        using var bold = new Font("Consolas", 8, FontStyle.Bold);
        using var black = Brushes.Black;
        void Line(string text, Font font)
        {
            g.DrawString(text, font, black, 0, y);
            y += font.GetHeight(g) + 2;
        }

        Line(StoreName, title);
        Line(StoreAddress, body);
        Line($"Sale {sale.SaleNo}", bold);
        Line(sale.SaleDate.ToString("yyyy-MM-dd HH:mm"), body);
        Line($"Cashier: {sale.CashierName}", body);
        Line(new string('-', 32), body);
        foreach (var item in sale.Items)
        {
            Line($"{item.Qty} x {item.ProductName}", body);
            Line($"    {Money.Format(item.UnitPrice)}  {Money.Format(item.LineTotal),10}", body);
        }

        Line(new string('-', 32), body);
        Line($"Subtotal  {Money.Format(sale.Subtotal)}", body);
        Line($"Discount  {Money.Format(sale.Discount)}", body);
        Line($"TOTAL     {Money.Format(sale.Total)}", bold);
        Line($"{sale.PaymentMethod}  {Money.Format(sale.AmountPaid)}", body);
        Line($"Change    {Money.Format(sale.Change)}", body);
        Line(string.Empty, body);
        Line("Thank you", title);
        e.HasMorePages = false;
    }
}
