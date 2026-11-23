using AbayPOS.Domain;
using AbayPOS.UI.Imaging;
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
        var photo = new DataGridViewImageColumn { Name = "Photo", HeaderText = string.Empty, ImageLayout = DataGridViewImageCellLayout.Zoom, AutoSizeMode = DataGridViewAutoSizeColumnMode.None, Width = Theme.Scale(44) };
        _grid.Columns.Add(photo);
        _grid.Columns.Add("Barcode", "Barcode");
        _grid.Columns.Add("Name", "Name");
        _grid.Columns.Add("Cat", "Category");
        _grid.Columns.Add("Price", "Price");
        _grid.Columns.Add("Stock", "Stock");
        _grid.Columns.Add("Reorder", "Reorder");
        GridTheme.Balance(_grid, "Name", "Price", "Stock", "Reorder");
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
        foreach (DataGridViewRow existing in _grid.Rows)
        {
            if (existing.Cells["Photo"].Value is Bitmap old)
                old.Dispose();
        }

        _grid.Rows.Clear();
        foreach (var p in _items.Where(p => q.Length == 0 || p.Name.Contains(q, StringComparison.OrdinalIgnoreCase) || p.Barcode.Contains(q, StringComparison.OrdinalIgnoreCase)))
        {
            _grid.Rows.Add(DBNull.Value, p.Barcode, p.Name, p.Category, Money.Format(p.Price), p.StockQty, p.ReorderLevel);
            if (string.IsNullOrWhiteSpace(p.ImagePath))
                continue;
            var path = p.ImagePath;
            var barcode = p.Barcode;
            ThumbnailCache.Shared.Request(_grid, path, Theme.Scale(32), () =>
            {
                var thumb = ThumbnailCache.Shared.Get(path, Theme.Scale(32));
                if (thumb is null || _grid.IsDisposed)
                    return;
                foreach (DataGridViewRow gridRow in _grid.Rows)
                {
                    if (gridRow.Cells["Barcode"].Value?.ToString() != barcode)
                        continue;
                    if (gridRow.Cells["Photo"].Value is Bitmap previous)
                        previous.Dispose();
                    gridRow.Cells["Photo"].Value = (Bitmap)thumb.Clone();
                }
            });
        }
    }

    private Product? Selected()
    {
        if (_grid.CurrentRow is null)
            return null;
        var barcode = _grid.CurrentRow.Cells["Barcode"].Value?.ToString();
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
            var saved = dialog.Product;
            if (product.Id == 0)
                saved = await ServiceLocator.Products.CreateAsync(saved, _session);
            else
            {
                saved.Id = product.Id;
                await ServiceLocator.Products.UpdateAsync(saved, _session);
            }

            if (dialog.ImageRemoved)
            {
                ServiceLocator.Images.Delete(saved.ImagePath);
                saved.ImagePath = null;
                await ServiceLocator.Products.UpdateAsync(saved, _session);
            }
            else if (dialog.ReplacementJpeg is not null)
            {
                saved.ImagePath = ServiceLocator.Images.Save(saved.Id, dialog.ReplacementJpeg, saved.ImagePath);
                await ServiceLocator.Products.UpdateAsync(saved, _session);
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
        Emoji = p.Emoji,
        ImagePath = p.ImagePath
    };
}
