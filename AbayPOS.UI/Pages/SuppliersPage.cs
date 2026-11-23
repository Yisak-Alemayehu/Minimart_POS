using AbayPOS.Domain.Entities;
using AbayPOS.Domain.Models;
using AbayPOS.UI.Controls;
using AbayPOS.UI.Dialogs;
using AbayPOS.UI.Infrastructure;
using AbayPOS.UI.Theming;

namespace AbayPOS.UI.Pages;

internal sealed class SuppliersPage : UserControl
{
    private readonly Session _session;
    private readonly DataGridView _grid = new();
    private IReadOnlyList<Supplier> _items = [];

    public SuppliersPage(Session session)
    {
        _session = session;
        Dock = DockStyle.Fill;
        BackColor = Theme.Base;
        var panel = new GlassPanel { Dock = DockStyle.Fill, Padding = new Padding(16) };
        var layout = new TableLayoutPanel { Dock = DockStyle.Fill, RowCount = 3, BackColor = Theme.Base };
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 40));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 48));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        layout.Controls.Add(new Label { Text = "Suppliers", Font = Theme.FontHeading(), ForeColor = Theme.Text, Dock = DockStyle.Fill }, 0, 0);
        var toolbar = new FlowLayoutPanel { Dock = DockStyle.Fill, BackColor = Theme.Base };
        var add = new RoundButton { Text = "Add", Width = 100, Height = 40, IsPrimary = true };
        add.Click += (_, _) => Edit(new Supplier { IsActive = true });
        var edit = new RoundButton { Text = "Edit", Width = 100, Height = 40, FillColor = Theme.NavGlass };
        edit.Click += (_, _) =>
        {
            if (Selected() is { } s)
                Edit(s);
        };
        toolbar.Controls.Add(add);
        toolbar.Controls.Add(edit);
        layout.Controls.Add(toolbar, 0, 1);
        GridTheme.Apply(_grid);
        _grid.Dock = DockStyle.Fill;
        _grid.Columns.Add("Name", "Name");
        _grid.Columns.Add("Phone", "Phone");
        _grid.Columns.Add("Email", "Email");
        _grid.Columns.Add("Active", "Active");
        GridTheme.Balance(_grid, "Name");
        layout.Controls.Add(_grid, 0, 2);
        panel.Controls.Add(layout);
        Controls.Add(panel);
        Load += (_, _) => Reload();
    }

    public void Reload() => UiAsync.Run(async () =>
    {
        _items = await ServiceLocator.Suppliers.GetAllAsync(false);
        _grid.Rows.Clear();
        foreach (var s in _items)
            _grid.Rows.Add(s.Name, s.Phone ?? string.Empty, s.Email ?? string.Empty, s.IsActive ? "Yes" : "No");
    });

    private Supplier? Selected() =>
        _grid.CurrentRow is null ? null : _items.ElementAtOrDefault(_grid.CurrentRow.Index);

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
