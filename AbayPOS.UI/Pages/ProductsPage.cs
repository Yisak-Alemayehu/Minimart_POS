using AbayPOS.Domain;
using AbayPOS.Domain.Entities;
using AbayPOS.Domain.Enums;
using AbayPOS.Domain.Models;
using AbayPOS.UI.Controls;
using AbayPOS.UI.Dialogs;
using AbayPOS.UI.Imaging;
using AbayPOS.UI.Infrastructure;
using AbayPOS.UI.Theming;

namespace AbayPOS.UI.Pages;

public sealed partial class ProductsPage : UserControl
{
    private Session _session = null!;
    private IReadOnlyList<Product> _items = [];

    public ProductsPage()
    {
        InitializeComponent();
        _session = new Session(new User { Username = "design", FullName = "Designer", Role = UserRole.Manager });
        btnAdd.Click += (_, _) => Edit(new Product { IsActive = true, Price = 1, Category = "Staples" });
        btnEdit.Click += (_, _) =>
        {
            if (Selected() is { } product)
                Edit(product);
        };
        btnDeactivate.Click += (_, _) =>
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
        txtSearch.TextChanged += (_, _) => Filter();
    }

    public ProductsPage(Session session) : this() => _session = session;

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

    public void Reload() => UiAsync.Run(async () =>
    {
        _items = await ServiceLocator.Products.SearchAsync(null, null);
        Filter();
    });

    private void ConfigureGrid()
    {
        GridTheme.Apply(dgvProducts);
        dgvProducts.Columns.Clear();
        var photo = new DataGridViewImageColumn { Name = "Photo", HeaderText = string.Empty, ImageLayout = DataGridViewImageCellLayout.Zoom, AutoSizeMode = DataGridViewAutoSizeColumnMode.None, Width = Theme.Scale(44) };
        dgvProducts.Columns.Add(photo);
        dgvProducts.Columns.Add("Barcode", "Barcode");
        dgvProducts.Columns.Add("Name", "Name");
        dgvProducts.Columns.Add("Cat", "Category");
        dgvProducts.Columns.Add("Price", "Price");
        dgvProducts.Columns.Add("Stock", "Stock");
        dgvProducts.Columns.Add("Reorder", "Reorder");
        GridTheme.Balance(dgvProducts, "Name", "Price", "Stock", "Reorder");
    }

    private void Filter()
    {
        var q = (txtSearch.Text ?? string.Empty).Trim();
        foreach (DataGridViewRow existing in dgvProducts.Rows)
        {
            if (existing.Cells["Photo"].Value is Bitmap old)
                old.Dispose();
        }

        dgvProducts.Rows.Clear();
        foreach (var p in _items.Where(p => q.Length == 0 || p.Name.Contains(q, StringComparison.OrdinalIgnoreCase) || p.Barcode.Contains(q, StringComparison.OrdinalIgnoreCase)))
        {
            dgvProducts.Rows.Add(DBNull.Value, p.Barcode, p.Name, p.Category, Money.Format(p.Price), p.StockQty, p.ReorderLevel);
            if (string.IsNullOrWhiteSpace(p.ImagePath))
                continue;
            var path = p.ImagePath;
            var barcode = p.Barcode;
            ThumbnailCache.Shared.Request(dgvProducts, path, Theme.Scale(32), () =>
            {
                var thumb = ThumbnailCache.Shared.Get(path, Theme.Scale(32));
                if (thumb is null || dgvProducts.IsDisposed)
                    return;
                foreach (DataGridViewRow gridRow in dgvProducts.Rows)
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
        if (dgvProducts.CurrentRow is null)
            return null;
        var barcode = dgvProducts.CurrentRow.Cells["Barcode"].Value?.ToString();
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