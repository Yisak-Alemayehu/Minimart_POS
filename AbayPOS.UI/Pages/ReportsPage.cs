using System.Text;
using AbayPOS.Domain;
using AbayPOS.Domain.Models;
using AbayPOS.UI.Controls;
using AbayPOS.UI.Infrastructure;
using AbayPOS.UI.Theming;

namespace AbayPOS.UI.Pages;

internal sealed class ReportsPage : UserControl
{
    private readonly DateTimePicker _from = new() { Value = DateTime.Now.Date.AddDays(-7), Format = DateTimePickerFormat.Short };
    private readonly DateTimePicker _to = new() { Value = DateTime.Now.Date, Format = DateTimePickerFormat.Short };
    private readonly Label _summary = new() { Dock = DockStyle.Fill, ForeColor = Theme.Text, Font = Theme.FontBody() };
    private readonly DataGridView _cashiers = new();
    private readonly DataGridView _top = new();
    private string _csv = string.Empty;

    public ReportsPage(Session session)
    {
        Dock = DockStyle.Fill;
        BackColor = Theme.Base;
        var panel = new GlassPanel { Dock = DockStyle.Fill, Padding = new Padding(16) };
        var layout = new TableLayoutPanel { Dock = DockStyle.Fill, RowCount = 4, BackColor = Theme.Base };
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 40));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 48));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 48));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        layout.Controls.Add(new Label { Text = "Reports", Font = Theme.FontHeading(), ForeColor = Theme.Text, Dock = DockStyle.Fill }, 0, 0);
        var toolbar = new FlowLayoutPanel { Dock = DockStyle.Fill, BackColor = Theme.Base };
        var run = new RoundButton { Text = "Run", Width = 90, Height = 36, IsPrimary = true };
        run.Click += (_, _) => Reload();
        var export = new RoundButton { Text = "Export CSV", Width = 120, Height = 36, FillColor = Theme.NavGlass };
        export.Click += (_, _) => Export();
        toolbar.Controls.Add(new Label { Text = "From", ForeColor = Theme.TextSecondary, AutoSize = true, Margin = new Padding(0, 10, 8, 0) });
        toolbar.Controls.Add(_from);
        toolbar.Controls.Add(new Label { Text = "To", ForeColor = Theme.TextSecondary, AutoSize = true, Margin = new Padding(8, 10, 8, 0) });
        toolbar.Controls.Add(_to);
        toolbar.Controls.Add(run);
        toolbar.Controls.Add(export);
        layout.Controls.Add(toolbar, 0, 1);
        layout.Controls.Add(_summary, 0, 2);
        var split = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, BackColor = Theme.Base };
        GridTheme.Apply(_cashiers);
        GridTheme.Apply(_top);
        _cashiers.Dock = DockStyle.Fill;
        _top.Dock = DockStyle.Fill;
        _cashiers.Columns.Add("Cashier", "Cashier");
        _cashiers.Columns.Add("Tx", "Sales");
        _cashiers.Columns.Add("Total", "Total");
        GridTheme.Balance(_cashiers, "Cashier", "Tx", "Total");
        _top.Columns.Add("Product", "Product");
        _top.Columns.Add("Qty", "Qty");
        _top.Columns.Add("Rev", "Revenue");
        GridTheme.Balance(_top, "Product", "Qty", "Rev");
        split.Controls.Add(_cashiers, 0, 0);
        split.Controls.Add(_top, 1, 0);
        layout.Controls.Add(split, 0, 3);
        panel.Controls.Add(layout);
        Controls.Add(panel);
        Load += (_, _) => Reload();
        _ = session;
    }

    public void Reload() => UiAsync.Run(async () =>
    {
        var summary = await ServiceLocator.Reports.GetSalesSummaryAsync(_from.Value.Date, _to.Value.Date);
        var valuation = await ServiceLocator.Reports.GetStockValuationAsync();
        _summary.Text = $"Sales {Money.Format(summary.Total)}  ·  {summary.Transactions} tickets  ·  avg {Money.Format(summary.AverageBasket)}  ·  stock at cost {Money.Format(valuation.CostValue)}";
        var cashiers = await ServiceLocator.Reports.GetSalesByCashierAsync(_from.Value.Date, _to.Value.Date);
        _cashiers.Rows.Clear();
        foreach (var row in cashiers)
            _cashiers.Rows.Add(row.CashierName, row.Transactions, Money.Format(row.Total));
        var top = await ServiceLocator.Reports.GetTopProductsAsync(_from.Value.Date, _to.Value.Date, 10);
        _top.Rows.Clear();
        foreach (var row in top)
            _top.Rows.Add(row.ProductName, row.QtySold, Money.Format(row.Revenue));
        var csv = new StringBuilder();
        csv.AppendLine("Type,Name,Transactions,Amount");
        csv.AppendLine($"Summary,Range,{summary.Transactions},{summary.Total}");
        foreach (var row in cashiers)
            csv.AppendLine($"Cashier,{row.CashierName},{row.Transactions},{row.Total}");
        foreach (var row in top)
            csv.AppendLine($"Product,{row.ProductName},{row.QtySold},{row.Revenue}");
        _csv = csv.ToString();
    });

    private void Export()
    {
        using var dialog = new SaveFileDialog { Filter = "CSV (*.csv)|*.csv", FileName = "abaypos-report.csv" };
        if (dialog.ShowDialog(this) != DialogResult.OK)
            return;
        File.WriteAllText(dialog.FileName, _csv, Encoding.UTF8);
        ToastHost.Success("Report exported.");
    }
}
