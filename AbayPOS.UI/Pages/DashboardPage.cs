using AbayPOS.Domain;
using AbayPOS.Domain.Models;
using AbayPOS.Domain.Reporting;
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
    private readonly Label _periodTotal = new();
    private readonly Label _periodCaption = new();
    private readonly ChipButton _days7 = new("7 Days") { Selected = true };
    private readonly ChipButton _days30 = new("30 Days");
    private readonly ChipButton _month = new("This Month");
    private string _range = "7";

    public event EventHandler? ViewProducts;

    public DashboardPage(Session session)
    {
        Dock = DockStyle.Fill;
        BackColor = Theme.Base;
        var layout = new TableLayoutPanel { Dock = DockStyle.Fill, RowCount = 3, BackColor = Theme.Base };
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 128));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 52));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 48));

        var kpis = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 4, BackColor = Theme.Base };
        foreach (var card in new[] { _sales, _tx, _avg, _low })
        {
            card.Dock = DockStyle.Fill;
            card.Margin = new Padding(0, 0, 12, 12);
            kpis.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25));
            kpis.Controls.Add(card);
        }

        layout.Controls.Add(kpis, 0, 0);

        var middle = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, BackColor = Theme.Base, Margin = new Padding(0, 0, 0, 8) };
        middle.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 72));
        middle.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 28));
        var chartPanel = new GlassPanel { Dock = DockStyle.Fill, Padding = new Padding(16), Margin = new Padding(0, 0, 12, 0) };
        var chartHead = new TableLayoutPanel { Dock = DockStyle.Top, Height = 40, ColumnCount = 2, BackColor = Theme.Card };
        chartHead.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
        chartHead.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
        chartHead.Controls.Add(new Label { Text = "Last period", Font = Theme.Font(13, FontStyle.Bold), ForeColor = Theme.Text, Dock = DockStyle.Fill, BackColor = Theme.Card }, 0, 0);
        var chips = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.RightToLeft, WrapContents = false, BackColor = Theme.Card };
        Wire(_days7, "7");
        Wire(_days30, "30");
        Wire(_month, "month");
        chips.Controls.Add(_month);
        chips.Controls.Add(_days30);
        chips.Controls.Add(_days7);
        chartHead.Controls.Add(chips, 1, 0);
        _chart.Dock = DockStyle.Fill;
        chartPanel.Controls.Add(_chart);
        chartPanel.Controls.Add(chartHead);
        middle.Controls.Add(chartPanel, 0, 0);

        var summary = new GlassPanel { Dock = DockStyle.Fill, Padding = new Padding(20), FillColor = Theme.Accent };
        var summaryLayout = new TableLayoutPanel { Dock = DockStyle.Fill, RowCount = 3, BackColor = Theme.Accent };
        summaryLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 28));
        summaryLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        summaryLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 36));
        _periodCaption.Text = "Total sales";
        _periodCaption.Dock = DockStyle.Fill;
        _periodCaption.ForeColor = Theme.OnAccent;
        _periodCaption.BackColor = Theme.Accent;
        _periodCaption.Font = Theme.Font(11, FontStyle.Bold);
        _periodTotal.Dock = DockStyle.Fill;
        _periodTotal.ForeColor = Theme.OnAccent;
        _periodTotal.BackColor = Theme.Accent;
        _periodTotal.Font = Theme.Font(26, FontStyle.Bold);
        _periodTotal.TextAlign = ContentAlignment.MiddleLeft;
        summaryLayout.Controls.Add(_periodCaption, 0, 0);
        summaryLayout.Controls.Add(_periodTotal, 0, 1);
        summary.Controls.Add(summaryLayout);
        middle.Controls.Add(summary, 1, 0);
        layout.Controls.Add(middle, 0, 1);

        var topPanel = new GlassPanel { Dock = DockStyle.Fill, Padding = new Padding(16) };
        var head = new TableLayoutPanel { Dock = DockStyle.Top, Height = 36, ColumnCount = 2, BackColor = Theme.Card };
        head.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 70));
        head.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 30));
        head.Controls.Add(new Label { Text = "Top products", Font = Theme.Font(13, FontStyle.Bold), ForeColor = Theme.Text, Dock = DockStyle.Fill, BackColor = Theme.Card }, 0, 0);
        var viewAll = new LinkLabel { Text = "View all", Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleRight, LinkColor = Theme.Accent, BackColor = Theme.Card };
        viewAll.LinkClicked += (_, _) => ViewProducts?.Invoke(this, EventArgs.Empty);
        head.Controls.Add(viewAll, 1, 0);
        GridTheme.Apply(_top);
        _top.Dock = DockStyle.Fill;
        _top.Columns.Add("Name", "Product");
        _top.Columns.Add("Qty", "Qty");
        _top.Columns.Add("Rev", "Revenue");
        topPanel.Controls.Add(_top);
        topPanel.Controls.Add(head);
        layout.Controls.Add(topPanel, 0, 2);
        Controls.Add(layout);
        Load += (_, _) => Reload();
        _ = session;
    }

    public void Reload() => UiAsync.Run(LoadAsync);

    private void Wire(ChipButton chip, string key)
    {
        chip.Margin = new Padding(6, 4, 0, 0);
        chip.Click += (_, _) =>
        {
            _range = key;
            _days7.Selected = key == "7";
            _days30.Selected = key == "30";
            _month.Selected = key == "month";
            Reload();
        };
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
        _sales.SetValue(Money.Format(kpi.TodaySales));
        _tx.SetValue(kpi.TodayTransactions.ToString());
        _avg.SetValue(Money.Format(kpi.AverageBasket));
        _low.SetValue(kpi.LowStockCount.ToString());
        var series = await ServiceLocator.Reports.GetDailySalesAsync(from, today);
        _chart.SetPoints(series);
        _periodCaption.Text = caption;
        _periodTotal.Text = Money.Format(series.Sum(p => p.Total));
        var top = await ServiceLocator.Reports.GetTopProductsAsync(from, today, 5);
        _top.Rows.Clear();
        foreach (var row in top)
            _top.Rows.Add(row.ProductName, row.QtySold, Money.Format(row.Revenue));
    }
}
