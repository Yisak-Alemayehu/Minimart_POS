using AbayPOS.Domain;
using AbayPOS.UI.Infrastructure;
using AbayPOS.UI.Theming;

namespace AbayPOS.UI.Pages;

public sealed partial class LowStockPage : UserControl
{
    public event EventHandler<int>? CountChanged;

    public LowStockPage()
    {
        InitializeComponent();
    }

    protected override void OnLoad(EventArgs e)
    {
        base.OnLoad(e);
        if (DesignTime.IsActive(this))
            return;
        Theme.ApplyRuntime(this);
        ConfigureGrid();
        if (ServiceLocator.IsConfigured)
            Reload();
    }

    private void ConfigureGrid()
    {
        GridTheme.Apply(dgvLowStock);
        dgvLowStock.Columns.Clear();
        dgvLowStock.Columns.Add("Name", "Product");
        dgvLowStock.Columns.Add("Stock", "On hand");
        dgvLowStock.Columns.Add("Reorder", "Reorder at");
        dgvLowStock.Columns.Add("Suggest", "Suggested order");
        GridTheme.Balance(dgvLowStock, "Name", "Stock", "Reorder", "Suggest");
    }

    public void Reload() => UiAsync.Run(async () =>
    {
        var items = await ServiceLocator.Inventory.GetLowStockAsync();
        dgvLowStock.Rows.Clear();
        foreach (var p in items)
        {
            var suggest = Math.Max(p.ReorderLevel * 2 - p.StockQty, p.ReorderLevel);
            dgvLowStock.Rows.Add(p.Name, p.StockQty, p.ReorderLevel, suggest);
        }

        if (items.Count == 0)
            dgvLowStock.Rows.Add("All products are above reorder level.", "", "", "");
        CountChanged?.Invoke(this, items.Count);
    });
}