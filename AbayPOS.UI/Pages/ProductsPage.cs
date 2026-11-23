using AbayPOS.Domain;
using AbayPOS.Domain.Entities;
using AbayPOS.Domain.Models;
using AbayPOS.UI.Controls;
using AbayPOS.UI.Dialogs;
using AbayPOS.UI.Infrastructure;
using AbayPOS.UI.Theming;

namespace AbayPOS.UI.Pages;

internal sealed class ProductsPage : UserControl
{
    private readonly Session _session;
    private readonly DataGridView _grid = new();
    private readonly GlassTextBox _search = new() { Placeholder = "Search name or barcode" };
    private IReadOnlyList<Product> _items = [];

    public ProductsPage(Session session)
    {
        _session = session;
        Dock = DockStyle.Fill;
        BackColor = Theme.Base;
        var panel = new GlassPanel { Dock = DockStyle.Fill, Padding = new Padding(16) };
        var layout = new TableLayoutPanel { Dock = DockStyle.Fill, RowCount = 3, BackColor = Theme.Base };
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 40));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 48));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        layout.Controls.Add(new Label { Text = "Products", Font = Theme.FontHeading(), ForeColor = Theme.Text, Dock = DockStyle.Fill }, 0, 0);
        var toolbar = new FlowLayoutPanel { Dock = DockStyle.Fill, BackColor = Theme.Base };
        _search.Width = 280;
        _search.Height = 40;
        _search.TextChanged += (_, _) => Filter();
        var add = new RoundButton { Text = "Add", Width = 100, Height = 40, IsPrimary = true };
        add.Click += (_, _) => Edit(new Product { IsActive = true, Price = 1, Category = "Staples" });
        var edit = new RoundButton { Text = "Edit", Width = 100, Height = 40, FillColor = Theme.NavGlass };
        edit.Click += (_, _) =>
        {
            if (Selected() is { } product)
                Edit(product);
        };
        var deactivate = new RoundButton { Text = "Deactivate", Width = 130, Height = 40, FillColor = Theme.CancelGlass, BorderColor = Theme.GlassBorder };
        deactivate.Click += (_, _) =>
        {
            if (Selected() is not { } product)
                return;
            if (!ConfirmDialog.Ask(FindForm()!, "Deactivate product", $"Hide {product.Name} from the register? Past sales stay on file.", "Deactivate"))
                return;
            UiAsync.Run(async () =>
            {
                await ServiceLocator.Products.DeactivateAsync(product.Id, _session);
                Reload();
            });
        };
        toolbar.Controls.Add(_search);
        toolbar.Controls.Add(add);
        toolbar.Controls.Add(edit);
        toolbar.Controls.Add(deactivate);
        layout.Controls.Add(toolbar, 0, 1);
        GridTheme.Apply(_grid);
        _grid.Dock = DockStyle.Fill;
        _grid.Columns.Add("Barcode", "Barcode");
        _grid.Columns.Add("Name", "Name");
        _grid.Columns.Add("Cat", "Category");
        _grid.Columns.Add("Price", "Price");
        _grid.Columns.Add("Stock", "Stock");
        _grid.Columns.Add("Reorder", "Reorder");
        layout.Controls.Add(_grid, 0, 2);
        panel.Controls.Add(layout);
        Controls.Add(panel);
        Load += (_, _) => Reload();
    }

    public void Reload() => UiAsync.Run(async () =>
    {
        _items = await ServiceLocator.Products.SearchAsync(null, null);
        Filter();
    });

    private void Filter()
    {
        var q = (_search.Text ?? string.Empty).Trim();
        _grid.Rows.Clear();
        foreach (var p in _items.Where(p => q.Length == 0 || p.Name.Contains(q, StringComparison.OrdinalIgnoreCase) || p.Barcode.Contains(q, StringComparison.OrdinalIgnoreCase)))
            _grid.Rows.Add(p.Barcode, p.Name, p.Category, Money.Format(p.Price), p.StockQty, p.ReorderLevel);
    }

    private Product? Selected()
    {
        if (_grid.CurrentRow is null)
            return null;
        var barcode = _grid.CurrentRow.Cells[0].Value?.ToString();
        return _items.FirstOrDefault(p => p.Barcode == barcode);
    }

    private void Edit(Product product)
    {
        UiAsync.Run(async () =>
        {
            var suppliers = await ServiceLocator.Suppliers.GetAllAsync(true);
            using var overlay = ConfirmDialog.CreateOverlay(FindForm()!);
            overlay?.Show();
            using var dialog = new ProductEditDialog(Clone(product), suppliers);
            var result = dialog.ShowDialog(this);
            overlay?.Close();
            if (result != DialogResult.OK)
                return;
            if (product.Id == 0)
                await ServiceLocator.Products.CreateAsync(dialog.Product, _session);
            else
            {
                dialog.Product.Id = product.Id;
                await ServiceLocator.Products.UpdateAsync(dialog.Product, _session);
            }

            Reload();
            ToastHost.Success("Product saved.");
        });
    }

    private static Product Clone(Product p) => new()
    {
        Id = p.Id,
        Name = p.Name,
        Barcode = p.Barcode,
        Category = p.Category,
        Price = p.Price,
        CostPrice = p.CostPrice,
        StockQty = p.StockQty,
        ReorderLevel = p.ReorderLevel,
        SupplierId = p.SupplierId,
        IsActive = p.IsActive,
        Emoji = p.Emoji
    };
}
