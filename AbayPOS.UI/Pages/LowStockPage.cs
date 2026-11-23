using AbayPOS.Domain;
using AbayPOS.UI.Controls;
using AbayPOS.UI.Infrastructure;
using AbayPOS.UI.Theming;

namespace AbayPOS.UI.Pages;

internal sealed class LowStockPage : UserControl
{
    private readonly DataGridView _grid = new();
    public event EventHandler<int>? CountChanged;

    public LowStockPage()
    {
        Dock = DockStyle.Fill;
        BackColor = Theme.Base;
        var panel = new GlassPanel { Dock = DockStyle.Fill, Padding = new Padding(16) };
        var layout = new TableLayoutPanel { Dock = DockStyle.Fill, RowCount = 2, BackColor = Theme.Base };
        layout.Controls.Add(new Label { Text = "Low stock", Font = Theme.FontHeading(), ForeColor = Theme.Text, Dock = DockStyle.Fill }, 0, 0);
        GridTheme.Apply(_grid);
        _grid.Dock = DockStyle.Fill;
        _grid.Columns.Add("Name", "Product");
        _grid.Columns.Add("Stock", "On hand");
        _grid.Columns.Add("Reorder", "Reorder at");
        _grid.Columns.Add("Suggest", "Suggested order");
        GridTheme.Balance(_grid, "Name", "Stock", "Reorder", "Suggest");
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 40));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        layout.Controls.Add(_grid, 0, 1);
        panel.Controls.Add(layout);
        Controls.Add(panel);
        Load += (_, _) => Reload();
    }

    public void Reload() => UiAsync.Run(async () =>
    {
        var items = await ServiceLocator.Inventory.GetLowStockAsync();
        _grid.Rows.Clear();
        foreach (var p in items)
        {
            var suggest = Math.Max(p.ReorderLevel * 2 - p.StockQty, p.ReorderLevel);
            _grid.Rows.Add(p.Name, p.StockQty, p.ReorderLevel, suggest);
        }

        if (items.Count == 0)
            _grid.Rows.Add("All products are above reorder level.", "", "", "");
        CountChanged?.Invoke(this, items.Count);
    });
}
