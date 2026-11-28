using AbayPOS.Business.Services;
using AbayPOS.Domain;
using AbayPOS.Domain.Entities;
using AbayPOS.Domain.Enums;
using AbayPOS.Domain.Models;
using AbayPOS.UI.Controls;
using AbayPOS.UI.Infrastructure;
using AbayPOS.UI.Theming;

namespace AbayPOS.UI.Pages;

public sealed partial class ReceiveStockPage : UserControl
{
    private Session _session = null!;
    private readonly List<ReceiptLine> _lines = [];
    private IReadOnlyList<Product> _products = [];

    public ReceiveStockPage()
    {
        InitializeComponent();
        _session = new Session(new User { Username = "design", FullName = "Designer", Role = UserRole.Manager });
        btnAddLine.Click += (_, _) => AddLine();
        btnSave.Click += (_, _) => Save();
    }

    public ReceiveStockPage(Session session) : this() => _session = session;

    protected override void OnLoad(EventArgs e)
    {
        base.OnLoad(e);
        if (DesignTime.IsActive(this))
            return;
        Theme.ApplyRuntime(this);
        ConfigureGrid();
        if (ServiceLocator.IsConfigured)
            ReloadLookups();
    }

    public void ReloadLookups() => UiAsync.Run(async () =>
    {
        var suppliers = await ServiceLocator.Suppliers.GetAllAsync(true);
        _products = await ServiceLocator.Products.GetActiveAsync();
        cboSupplier.Items.Clear();
        foreach (var s in suppliers)
            cboSupplier.Items.Add(s);
        cboSupplier.DisplayMember = nameof(Supplier.Name);
        if (cboSupplier.Items.Count > 0)
            cboSupplier.SelectedIndex = 0;
        cboProduct.Items.Clear();
        foreach (var p in _products)
            cboProduct.Items.Add(p);
        cboProduct.DisplayMember = nameof(Product.Name);
        if (cboProduct.Items.Count > 0)
            cboProduct.SelectedIndex = 0;
    });

    private void ConfigureGrid()
    {
        GridTheme.Apply(dgvLines);
        dgvLines.Columns.Clear();
        dgvLines.Columns.Add("Product", "Product");
        dgvLines.Columns.Add("Qty", "Qty");
        dgvLines.Columns.Add("Cost", "Unit cost");
        GridTheme.Balance(dgvLines, "Product", "Qty", "Cost");
    }

    private void AddLine()
    {
        if (cboProduct.SelectedItem is not Product product)
            return;
        _lines.Add(new ReceiptLine { ProductId = product.Id, Qty = (int)numQty.Value, UnitCost = numCost.Value });
        dgvLines.Rows.Add(product.Name, numQty.Value, Money.Format(numCost.Value));
    }

    private void Save()
    {
        if (cboSupplier.SelectedItem is not Supplier supplier)
        {
            ToastHost.Warn("Choose a supplier.");
            return;
        }

        UiAsync.Run(async () =>
        {
            await ServiceLocator.Inventory.ReceiveStockAsync(supplier.Id, _lines, txtNote.Text, _session);
            _lines.Clear();
            dgvLines.Rows.Clear();
            ToastHost.Success("Stock received.");
        });
    }
}