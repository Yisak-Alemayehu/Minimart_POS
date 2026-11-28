using AbayPOS.Domain;
using AbayPOS.Domain.Entities;
using AbayPOS.Domain.Enums;
using AbayPOS.Domain.Models;
using AbayPOS.Domain.Reporting;
using AbayPOS.UI.Controls;
using AbayPOS.UI.Infrastructure;
using AbayPOS.UI.Theming;

namespace AbayPOS.UI.Pages;

public sealed partial class DashboardPage : UserControl
{
    private Session _session = null!;
    private string _range = "7";

    public DashboardPage()
    {
        InitializeComponent();
        _session = new Session(new User { Username = "design", FullName = "Designer", Role = UserRole.Manager });
        WireUi();
    }

    public DashboardPage(Session session) : this()
    {
        _session = session;
    }

    public event EventHandler? ViewProducts;

    protected override void OnLoad(EventArgs e)
    {
        base.OnLoad(e);
        if (DesignTime.IsActive(this))
            return;
        Theme.ApplyRuntime(this);
        ConfigureGrid();
        Resize += (_, _) => ArrangeKpis();
        ArrangeKpis();
        if (ServiceLocator.IsConfigured)
            Reload();
    }

    public void Reload() => UiAsync.Run(LoadAsync);

    private void WireUi()
    {
        chip7Days.Click += (_, _) => SelectRange("7");
        chip30Days.Click += (_, _) => SelectRange("30");
        chipMonth.Click += (_, _) => SelectRange("month");
        lnkViewAll.LinkClicked += (_, _) => ViewProducts?.Invoke(this, EventArgs.Empty);
    }

    private void SelectRange(string key)
    {
        _range = key;
        chip7Days.Selected = key == "7";
        chip30Days.Selected = key == "30";
        chipMonth.Selected = key == "month";
        if (ServiceLocator.IsConfigured)
            Reload();
    }

    private void ArrangeKpis()
    {
        var wide = Width >= Theme.Scale(1100);
        tlpKpis.SuspendLayout();
        tlpKpis.ColumnStyles.Clear();
        tlpKpis.RowStyles.Clear();
        tlpKpis.ColumnCount = wide ? 4 : 2;
        tlpKpis.RowCount = wide ? 1 : 2;
        for (var i = 0; i < tlpKpis.ColumnCount; i++)
            tlpKpis.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f / tlpKpis.ColumnCount));
        for (var i = 0; i < tlpKpis.RowCount; i++)
            tlpKpis.RowStyles.Add(new RowStyle(SizeType.Percent, 100f / tlpKpis.RowCount));
        var cards = new[] { kpiSales, kpiTx, kpiAvg, kpiLow };
        for (var i = 0; i < cards.Length; i++)
        {
            var col = wide ? i : i % 2;
            var row = wide ? 0 : i / 2;
            cards[i].Dock = DockStyle.Fill;
            cards[i].Margin = new Padding(0, 0, Theme.Scale(12), Theme.Scale(12));
            tlpKpis.SetCellPosition(cards[i], new TableLayoutPanelCellPosition(col, row));
        }

        tlpKpis.ResumeLayout();
        tlpRoot.RowStyles[0] = new RowStyle(SizeType.Absolute, Theme.Scale(wide ? 150 : 300));
    }

    private void ConfigureGrid()
    {
        GridTheme.Apply(dgvTopProducts);
        dgvTopProducts.Columns.Clear();
        dgvTopProducts.Columns.Add("Name", "Product");
        dgvTopProducts.Columns.Add("Qty", "Qty");
        dgvTopProducts.Columns.Add("Rev", "Revenue");
        GridTheme.Balance(dgvTopProducts, "Name", "Qty", "Rev");
    }

    private async Task LoadAsync()
    {
        var today = DateTime.Now.Date;
        var (from, caption) = _range switch
        {
            "30" => (today.AddDays(-29), "Total sales (30 days)"),
            "month" => (new DateTime(today.Year, today.Month, 1), "Total sales (this month)"),
            _ => (today.AddDays(-6), "Total sales (7 days)")
        };
        var kpi = await ServiceLocator.Reports.GetDashboardAsync();
        kpiSales.SetValue(Money.Format(kpi.TodaySales));
        kpiTx.SetValue(kpi.TodayTransactions.ToString());
        kpiAvg.SetValue(Money.Format(kpi.AverageBasket));
        kpiLow.SetValue(kpi.LowStockCount.ToString());
        var series = await ServiceLocator.Reports.GetDailySalesAsync(from, today);
        chartSales.SetPoints(series);
        lblPeriodCaption.Text = caption;
        txtPeriodTotal.SetValue(Money.Format(series.Sum(p => p.Total)));
        var top = await ServiceLocator.Reports.GetTopProductsAsync(from, today, 5);
        dgvTopProducts.Rows.Clear();
        foreach (var row in top)
            dgvTopProducts.Rows.Add(row.ProductName, row.QtySold, Money.Format(row.Revenue));
        _ = _session;
    }
}