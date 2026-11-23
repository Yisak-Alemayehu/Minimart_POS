using AbayPOS.Domain;
using AbayPOS.Domain.Entities;
using AbayPOS.Domain.Enums;
using AbayPOS.Domain.Models;
using AbayPOS.UI.Controls;
using AbayPOS.UI.Dialogs;
using AbayPOS.UI.Infrastructure;
using AbayPOS.UI.Theming;

namespace AbayPOS.UI.Pages;

internal sealed class SalesHistoryPage : UserControl
{
    private readonly Session _session;
    private readonly DateTimePicker _from = new() { Value = DateTime.Now.Date.AddDays(-7) };
    private readonly DateTimePicker _to = new() { Value = DateTime.Now.Date };
    private readonly GlassTextBox _saleNo = new() { Placeholder = "Sale no", Width = 140, Height = 36 };
    private readonly ComboBox _cashier = new();
    private readonly DataGridView _grid = new();
    private readonly DataGridView _items = new();
    private IReadOnlyList<Sale> _sales = [];

    public SalesHistoryPage(Session session)
    {
        _session = session;
        Dock = DockStyle.Fill;
        BackColor = Color.Transparent;
        var panel = new GlassPanel { Dock = DockStyle.Fill, Padding = new Padding(16) };
        var layout = new TableLayoutPanel { Dock = DockStyle.Fill, RowCount = 4, BackColor = Color.Transparent };
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 40));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 48));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 55));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 45));
        layout.Controls.Add(new Label { Text = "Sales history", Font = Theme.FontHeading(), ForeColor = Theme.Text, Dock = DockStyle.Fill }, 0, 0);
        var toolbar = new FlowLayoutPanel { Dock = DockStyle.Fill, BackColor = Color.Transparent };
        var run = new RoundButton { Text = "Filter", Width = 90, Height = 36, IsPrimary = true };
        run.Click += (_, _) => Reload();
        var reprint = new RoundButton { Text = "Reprint", Width = 100, Height = 36, FillColor = Theme.NavGlass };
        reprint.Click += (_, _) => Reprint();
        var voidBtn = new RoundButton { Text = "Void", Width = 90, Height = 36, FillColor = Theme.CancelGlass, BorderColor = Theme.GlassBorder };
        voidBtn.Click += (_, _) => VoidSelected();
        _cashier.Width = 160;
        _cashier.DropDownStyle = ComboBoxStyle.DropDownList;
        toolbar.Controls.Add(_from);
        toolbar.Controls.Add(_to);
        toolbar.Controls.Add(_saleNo);
        toolbar.Controls.Add(_cashier);
        toolbar.Controls.Add(run);
        toolbar.Controls.Add(reprint);
        toolbar.Controls.Add(voidBtn);
        layout.Controls.Add(toolbar, 0, 1);
        GridTheme.Apply(_grid);
        GridTheme.Apply(_items);
        _grid.Dock = DockStyle.Fill;
        _items.Dock = DockStyle.Fill;
        _grid.Columns.Add("No", "Sale");
        _grid.Columns.Add("When", "When");
        _grid.Columns.Add("Cashier", "Cashier");
        _grid.Columns.Add("Total", "Total");
        _grid.Columns.Add("Status", "Status");
        _grid.SelectionChanged += (_, _) => ShowItems();
        _items.Columns.Add("Item", "Item");
        _items.Columns.Add("Qty", "Qty");
        _items.Columns.Add("Total", "Total");
        layout.Controls.Add(_grid, 0, 2);
        layout.Controls.Add(_items, 0, 3);
        panel.Controls.Add(layout);
        Controls.Add(panel);
        Load += (_, _) => UiAsync.Run(async () =>
        {
            var users = await ServiceLocator.Users.GetAllAsync();
            _cashier.Items.Clear();
            _cashier.Items.Add("All cashiers");
            foreach (var user in users)
                _cashier.Items.Add(user);
            _cashier.DisplayMember = nameof(User.FullName);
            _cashier.SelectedIndex = 0;
            Reload();
        });
    }

    public void Reload() => UiAsync.Run(async () =>
    {
        int? cashierId = _cashier.SelectedItem is User user ? user.Id : null;
        _sales = await ServiceLocator.Sales.GetHistoryAsync(_from.Value.Date, _to.Value.Date.AddDays(1), cashierId, _saleNo.Text);
        _grid.Rows.Clear();
        foreach (var sale in _sales)
            _grid.Rows.Add(sale.SaleNo, sale.SaleDate.ToString("yyyy-MM-dd HH:mm"), sale.CashierName ?? string.Empty, Money.Format(sale.Total), sale.Status);
    });

    private Sale? Selected() =>
        _grid.CurrentRow is null ? null : _sales.ElementAtOrDefault(_grid.CurrentRow.Index);

    private void ShowItems() => UiAsync.Run(async () =>
    {
        _items.Rows.Clear();
        if (Selected() is null)
            return;
        var sale = await ServiceLocator.Sales.GetSaleAsync(Selected()!.Id);
        if (sale is null)
            return;
        foreach (var item in sale.Items)
            _items.Rows.Add(item.ProductName, item.Qty, Money.Format(item.LineTotal));
    });

    private void Reprint()
    {
        if (Selected() is null)
            return;
        UiAsync.Run(async () =>
        {
            var sale = await ServiceLocator.Sales.GetSaleAsync(Selected()!.Id);
            if (sale is null)
                return;
            using var overlay = ConfirmDialog.CreateOverlay(FindForm()!);
            overlay?.Show();
            using var preview = new ReceiptPreviewDialog(sale);
            preview.ShowDialog(this);
            overlay?.Close();
        });
    }

    private void VoidSelected()
    {
        if (Selected() is not { } sale || sale.Status == SaleStatus.Voided)
            return;
        if (!ConfirmDialog.Ask(FindForm()!, "Void sale", $"Void {sale.SaleNo} and put the stock back?", "Void sale"))
            return;
        UiAsync.Run(async () =>
        {
            await ServiceLocator.Sales.VoidSaleAsync(sale.Id, _session);
            ToastHost.Success("Sale voided.");
            Reload();
        });
    }
}
