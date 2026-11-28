using System.Text;
using AbayPOS.Domain;
using AbayPOS.Domain.Models;
using AbayPOS.UI.Controls;
using AbayPOS.UI.Infrastructure;
using AbayPOS.UI.Theming;

namespace AbayPOS.UI.Pages;

public sealed partial class ReportsPage : UserControl
{
    private string _csv = string.Empty;

    public ReportsPage(Session session)
    {
        _ = session;
        InitializeComponent();
        dtpFrom.Value = DateTime.Now.Date.AddDays(-7);
        dtpTo.Value = DateTime.Now.Date;
        btnRun.Click += (_, _) => Reload();
        btnExport.Click += (_, _) => Export();
    }

    protected override void OnLoad(EventArgs e)
    {
        base.OnLoad(e);
        if (DesignTime.IsActive(this))
            return;
        Theme.ApplyRuntime(this);
        ConfigureGrids();
        if (ServiceLocator.IsConfigured)
            Reload();
    }

    private void ConfigureGrids()
    {
        GridTheme.Apply(dgvCashiers);
        GridTheme.Apply(dgvTop);
        dgvCashiers.Columns.Clear();
        dgvTop.Columns.Clear();
        dgvCashiers.Columns.Add("Cashier", "Cashier");
        dgvCashiers.Columns.Add("Tx", "Sales");
        dgvCashiers.Columns.Add("Total", "Total");
        GridTheme.Balance(dgvCashiers, "Cashier", "Tx", "Total");
        dgvTop.Columns.Add("Product", "Product");
        dgvTop.Columns.Add("Qty", "Qty");
        dgvTop.Columns.Add("Rev", "Revenue");
        GridTheme.Balance(dgvTop, "Product", "Qty", "Rev");
    }

    public void Reload() => UiAsync.Run(async () =>
    {
        var summary = await ServiceLocator.Reports.GetSalesSummaryAsync(dtpFrom.Value.Date, dtpTo.Value.Date);
        var valuation = await ServiceLocator.Reports.GetStockValuationAsync();
        lblSummary.Text = $"Sales {Money.Format(summary.Total)}  ·  {summary.Transactions} tickets  ·  avg {Money.Format(summary.AverageBasket)}  ·  stock at cost {Money.Format(valuation.CostValue)}";
        var cashiers = await ServiceLocator.Reports.GetSalesByCashierAsync(dtpFrom.Value.Date, dtpTo.Value.Date);
        dgvCashiers.Rows.Clear();
        foreach (var row in cashiers)
            dgvCashiers.Rows.Add(row.CashierName, row.Transactions, Money.Format(row.Total));
        var top = await ServiceLocator.Reports.GetTopProductsAsync(dtpFrom.Value.Date, dtpTo.Value.Date, 10);
        dgvTop.Rows.Clear();
        foreach (var row in top)
            dgvTop.Rows.Add(row.ProductName, row.QtySold, Money.Format(row.Revenue));
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