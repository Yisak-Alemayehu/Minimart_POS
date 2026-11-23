using AbayPOS.Domain;
using AbayPOS.Domain.Models;
using AbayPOS.UI.Controls;
using AbayPOS.UI.Infrastructure;
using AbayPOS.UI.Theming;

namespace AbayPOS.UI.Pages;

internal sealed class DashboardPage : UserControl
{
    private readonly KpiCard _sales = new("Today's sales");
    private readonly KpiCard _tx = new("Transactions");
    private readonly KpiCard _avg = new("Avg basket");
    private readonly KpiCard _low = new("Low stock");
    private readonly BarChart _chart = new();
    private readonly DataGridView _top = new();

    public DashboardPage(Session session)
    {
        Dock = DockStyle.Fill;
        BackColor = Color.Transparent;
        var layout = new TableLayoutPanel { Dock = DockStyle.Fill, RowCount = 3, BackColor = Color.Transparent };
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 110));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 55));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 45));
        var kpis = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 4, BackColor = Color.Transparent };
        foreach (var card in new[] { _sales, _tx, _avg, _low })
        {
            card.Dock = DockStyle.Fill;
            card.Margin = new Padding(0, 0, 8, 8);
            kpis.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25));
            kpis.Controls.Add(card);
        }

        layout.Controls.Add(kpis, 0, 0);
        var chartPanel = new GlassPanel { Dock = DockStyle.Fill, Padding = new Padding(16), Margin = new Padding(0, 0, 0, 8) };
        chartPanel.Controls.Add(new Label { Text = "Last 7 days", Font = Theme.Font(12, FontStyle.Bold), ForeColor = Theme.Text, Dock = DockStyle.Top, Height = 28 });
        _chart.Dock = DockStyle.Fill;
        chartPanel.Controls.Add(_chart);
        layout.Controls.Add(chartPanel, 0, 1);
        var topPanel = new GlassPanel { Dock = DockStyle.Fill, Padding = new Padding(16) };
        topPanel.Controls.Add(new Label { Text = "Top products", Font = Theme.Font(12, FontStyle.Bold), ForeColor = Theme.Text, Dock = DockStyle.Top, Height = 28 });
        GridTheme.Apply(_top);
        _top.Dock = DockStyle.Fill;
        _top.Columns.Add("Name", "Product");
        _top.Columns.Add("Qty", "Qty");
        _top.Columns.Add("Rev", "Revenue");
        topPanel.Controls.Add(_top);
        layout.Controls.Add(topPanel, 0, 2);
        Controls.Add(layout);
        Load += (_, _) => Reload();
        _ = session;
    }

    public void Reload() => UiAsync.Run(async () =>
    {
        var kpi = await ServiceLocator.Reports.GetDashboardAsync();
        _sales.SetValue(Money.Format(kpi.TodaySales));
        _tx.SetValue(kpi.TodayTransactions.ToString());
        _avg.SetValue(Money.Format(kpi.AverageBasket));
        _low.SetValue(kpi.LowStockCount.ToString());
        _chart.SetPoints(await ServiceLocator.Reports.GetLast7DaysAsync());
        var top = await ServiceLocator.Reports.GetTopProductsAsync(DateTime.Now.Date.AddDays(-30), DateTime.Now.Date, 5);
        _top.Rows.Clear();
        foreach (var row in top)
            _top.Rows.Add(row.ProductName, row.QtySold, Money.Format(row.Revenue));
    });
}
