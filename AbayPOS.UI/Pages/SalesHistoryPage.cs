using AbayPOS.Domain;
using AbayPOS.Domain.Entities;
using AbayPOS.Domain.Enums;
using AbayPOS.Domain.Models;
using AbayPOS.UI.Controls;
using AbayPOS.UI.Dialogs;
using AbayPOS.UI.Infrastructure;
using AbayPOS.UI.Theming;

namespace AbayPOS.UI.Pages;

public sealed partial class SalesHistoryPage : UserControl
{
    private readonly Session _session;
    private IReadOnlyList<Sale> _sales = [];

    public SalesHistoryPage(Session session)
    {
        _session = session;
        InitializeComponent();
        dtpFrom.Value = DateTime.Now.Date.AddDays(-7);
        dtpTo.Value = DateTime.Now.Date;
        btnFilter.Click += (_, _) => Reload();
        btnReprint.Click += (_, _) => Reprint();
        btnVoid.Click += (_, _) => VoidSelected();
        dgvSales.SelectionChanged += (_, _) => ShowItems();
    }

    protected override void OnLoad(EventArgs e)
    {
        base.OnLoad(e);
        if (DesignTime.IsActive(this))
            return;
        Theme.ApplyRuntime(this);
        ConfigureGrids();
        if (!ServiceLocator.IsConfigured)
            return;
        UiAsync.Run(async () =>
        {
            var users = await ServiceLocator.Users.GetAllAsync();
            cmbCashier.Items.Clear();
            cmbCashier.Items.Add("All cashiers");
            foreach (var user in users)
                cmbCashier.Items.Add(user);
            cmbCashier.DisplayMember = nameof(User.FullName);
            cmbCashier.SelectedIndex = 0;
            Reload();
        });
    }

    private void ConfigureGrids()
    {
        GridTheme.Apply(dgvSales);
        GridTheme.Apply(dgvItems);
        dgvSales.Columns.Clear();
        dgvItems.Columns.Clear();
        dgvSales.Columns.Add("No", "Sale");
        dgvSales.Columns.Add("When", "When");
        dgvSales.Columns.Add("Cashier", "Cashier");
        dgvSales.Columns.Add("Total", "Total");
        dgvSales.Columns.Add("Status", "Status");
        GridTheme.Balance(dgvSales, "Cashier", "Total");
        dgvItems.Columns.Add("Item", "Item");
        dgvItems.Columns.Add("Qty", "Qty");
        dgvItems.Columns.Add("Total", "Total");
        GridTheme.Balance(dgvItems, "Item", "Qty", "Total");
    }

    public void Reload() => UiAsync.Run(async () =>
    {
        int? cashierId = cmbCashier.SelectedItem is User user ? user.Id : null;
        _sales = await ServiceLocator.Sales.GetHistoryAsync(dtpFrom.Value.Date, dtpTo.Value.Date.AddDays(1), cashierId, txtSaleNo.Text);
        dgvSales.Rows.Clear();
        foreach (var sale in _sales)
            dgvSales.Rows.Add(sale.SaleNo, sale.SaleDate.ToString("yyyy-MM-dd HH:mm"), sale.CashierName ?? string.Empty, Money.Format(sale.Total), sale.Status);
    });

    private Sale? Selected() =>
        dgvSales.CurrentRow is null ? null : _sales.ElementAtOrDefault(dgvSales.CurrentRow.Index);

    private void ShowItems() => UiAsync.Run(async () =>
    {
        dgvItems.Rows.Clear();
        if (Selected() is null)
            return;
        var sale = await ServiceLocator.Sales.GetSaleAsync(Selected()!.Id);
        if (sale is null)
            return;
        foreach (var item in sale.Items)
            dgvItems.Rows.Add(item.ProductName, item.Qty, Money.Format(item.LineTotal));
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
