using AbayPOS.Domain.Entities;
using AbayPOS.Domain.Models;
using AbayPOS.UI.Dialogs;
using AbayPOS.UI.Infrastructure;
using AbayPOS.UI.Theming;

namespace AbayPOS.UI.Pages;

public sealed partial class SuppliersPage : UserControl
{
    private readonly Session _session;
    private IReadOnlyList<Supplier> _items = [];

    public SuppliersPage(Session session)
    {
        _session = session;
        InitializeComponent();
        btnAdd.Click += (_, _) => Edit(new Supplier { IsActive = true });
        btnEdit.Click += (_, _) =>
        {
            if (Selected() is { } s)
                Edit(s);
        };
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
        GridTheme.Apply(dgvSuppliers);
        dgvSuppliers.Columns.Clear();
        dgvSuppliers.Columns.Add("Name", "Name");
        dgvSuppliers.Columns.Add("Phone", "Phone");
        dgvSuppliers.Columns.Add("Email", "Email");
        dgvSuppliers.Columns.Add("Active", "Active");
        GridTheme.Balance(dgvSuppliers, "Name");
    }

    public void Reload() => UiAsync.Run(async () =>
    {
        _items = await ServiceLocator.Suppliers.GetAllAsync(false);
        dgvSuppliers.Rows.Clear();
        foreach (var s in _items)
            dgvSuppliers.Rows.Add(s.Name, s.Phone ?? string.Empty, s.Email ?? string.Empty, s.IsActive ? "Yes" : "No");
    });

    private Supplier? Selected() =>
        dgvSuppliers.CurrentRow is null ? null : _items.ElementAtOrDefault(dgvSuppliers.CurrentRow.Index);

    private void Edit(Supplier supplier)
    {
        using var overlay = ConfirmDialog.CreateOverlay(FindForm()!);
        overlay?.Show();
        using var dialog = new SupplierEditDialog(supplier);
        var result = dialog.ShowDialog(this);
        overlay?.Close();
        if (result != DialogResult.OK)
            return;
        UiAsync.Run(async () =>
        {
            if (supplier.Id == 0)
                await ServiceLocator.Suppliers.CreateAsync(dialog.Supplier, _session);
            else
                await ServiceLocator.Suppliers.UpdateAsync(dialog.Supplier, _session);
            Reload();
        });
    }
}