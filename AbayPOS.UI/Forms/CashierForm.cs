using AbayPOS.Domain;
using AbayPOS.Domain.Entities;
using AbayPOS.Domain.Models;
using AbayPOS.UI.Controls;
using AbayPOS.UI.Dialogs;
using AbayPOS.UI.Infrastructure;
using AbayPOS.UI.Theming;

namespace AbayPOS.UI.Forms;

/// <summary>UC-03 cashier register bound to ProductService and SalesService.</summary>
public sealed partial class CashierForm : ThemedForm
{
    private readonly Session _session;
    private Cart _cart = new();
    private readonly List<Product> _catalog = [];
    private readonly List<ProductTile> _tiles = [];
    private readonly System.Windows.Forms.Timer tmrClock = new();
    private readonly System.Windows.Forms.Timer tmrDebounce = new() { Interval = 200 };
    private string _category = "All";
    private string _salePreview = "S-preview";

    public CashierForm(Session session)
    {
        _session = session;
        if (ServiceLocator.IsConfigured)
            _cart = ServiceLocator.Sales.CreateCart();
        InitializeComponent();
        btnNavSales.Click += (_, _) => ShowSales();
        btnNavHistory.Click += (_, _) => ShowHistory();
        btnNavLogout.Click += (_, _) => Logout();
        txtBarcode.KeyDown += BarcodeKeyDown;
        txtBarcode.TextChanged += (_, _) => { tmrDebounce.Stop(); tmrDebounce.Start(); };
        btnPay.Click += (_, _) => CompletePayment();
        btnCancelSale.Click += (_, _) => CancelSale();
        lblDiscountCaption.Click += (_, _) => EditDiscount();
        cartList.QuantityPlus += (_, i) => NudgeAt(i, 1);
        cartList.QuantityMinus += (_, i) => NudgeAt(i, -1);
        cartList.LineRemoved += (_, i) => RemoveAt(i);
        flpProducts.Resize += (_, _) => ResizeTiles();
        Text = $"AbayPOS – Register ({session.User.FullName})";
        Name = nameof(CashierForm);
        MinimumSize = new Size(Theme.MinimumWidth, Theme.MinimumHeight);
        ClientSize = new Size(1366, 768);
        Shown += (_, _) => UiAsync.Run(LoadCatalogAsync);
        UpdateCart();
    }


    protected override void OnLoad(EventArgs e)
    {
        base.OnLoad(e);
        if (DesignTime.IsActive(this)) return;
        Theme.ApplyRuntime(this);
        GridTheme.Apply(dgvHistory);
        dgvHistory.Columns.Clear();
        dgvHistory.Columns.Add("SaleNo", "Sale");
        dgvHistory.Columns.Add("Time", "Time");
        dgvHistory.Columns.Add("Total", "Total");
        dgvHistory.Columns.Add("Status", "Status");
        GridTheme.Balance(dgvHistory, null, "Total");
        lblCashier.Text = _session.User.FullName;
        cartList.ImageName = id => _catalog.FirstOrDefault(p => p.Id == id)?.ImagePath;
        tmrDebounce.Tick += (_, _) => { tmrDebounce.Stop(); ApplyFilter(); };
        tmrClock.Interval = 1000;
        tmrClock.Tick += (_, _) => lblClock.Text = DateTime.Now.ToString("MMM d, yyyy   hh:mm tt");
        tmrClock.Start();
        lblClock.Text = DateTime.Now.ToString("MMM d, yyyy   hh:mm tt");
        WindowState = FormWindowState.Maximized;
    }

    protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
    {
        if (keyData is Keys.F2 or (Keys.Control | Keys.K))
        {
            FocusBarcode();
            return true;
        }

        if (keyData == Keys.F4)
        {
            EditDiscount();
            return true;
        }

        if (keyData == Keys.F12)
        {
            Logout();
            return true;
        }

        if (keyData == Keys.Escape)
        {
            CancelSale();
            return true;
        }

        if (keyData == Keys.Delete)
        {
            RemoveSelected();
            return true;
        }

        if (keyData is Keys.Add or Keys.Oemplus)
        {
            NudgeQty(1);
            return true;
        }

        if (keyData is Keys.Subtract or Keys.OemMinus)
        {
            NudgeQty(-1);
            return true;
        }

        if (keyData == Keys.Enter && !txtBarcode.ContainsFocus)
        {
            CompletePayment();
            return true;
        }

        return base.ProcessCmdKey(ref msg, keyData);
    }

    protected override void OnFormClosed(FormClosedEventArgs e)
    {
        tmrClock.Stop();
        tmrClock.Dispose();
        tmrDebounce.Dispose();
        base.OnFormClosed(e);
    }

    private async Task LoadCatalogAsync()
    {
        _catalog.Clear();
        _catalog.AddRange(await ServiceLocator.Products.GetActiveAsync());
        var categories = await ServiceLocator.Products.GetCategoriesAsync();
        _salePreview = await ServiceLocator.Sales.PreviewSaleNoAsync();
        lblSaleNo.Text = "Current sale " + _salePreview;
        RebuildChips(categories);
        BindTiles();
        FocusBarcode();
    }

    private void RebuildChips(IReadOnlyList<string> categories)
    {
        flpChips.SuspendLayout();
        flpChips.Controls.Clear();
        foreach (var name in new[] { "All" }.Concat(categories))
        {
            var chip = new ChipButton(name) { Selected = name == _category, Margin = new Padding(0, 4, 8, 4) };
            chip.Click += (_, _) =>
            {
                _category = name;
                foreach (Control control in flpChips.Controls)
                {
                    if (control is ChipButton other)
                        other.Selected = other.Text == name;
                }

                ApplyFilter();
            };
            flpChips.Controls.Add(chip);
        }

        flpChips.ResumeLayout();
    }

    private void ApplyFilter()
    {
        var query = (txtBarcode.Text ?? string.Empty).Trim();
        flpProducts.SuspendLayout();
        var any = false;
        foreach (var tile in _tiles)
        {
            tile.SyncStock();
            var product = tile.Product;
            var categoryMatches = _category == "All" || product.Category == _category;
            var textMatches = query.Length == 0
                || product.Name.Contains(query, StringComparison.OrdinalIgnoreCase)
                || product.Barcode.Contains(query, StringComparison.OrdinalIgnoreCase);
            tile.Visible = categoryMatches && textMatches;
            any |= tile.Visible;
        }

        lblEmptyProducts.Visible = !any;
        flpProducts.ResumeLayout(true);
    }

    private void BindTiles()
    {
        flpProducts.SuspendLayout();
        foreach (var tile in _tiles)
            tile.Dispose();
        _tiles.Clear();
        flpProducts.Controls.Clear();
        flpProducts.Controls.Add(lblEmptyProducts);
        foreach (var product in _catalog)
        {
            var tile = new ProductTile(product);
            tile.Click += (_, _) =>
            {
                if (!tile.Product.IsOutOfStock)
                    TryAdd(tile.Product);
            };
            _tiles.Add(tile);
            flpProducts.Controls.Add(tile);
        }

        flpProducts.ResumeLayout(true);
        ResizeTiles();
        ApplyFilter();
    }

    private void BarcodeKeyDown(object? sender, KeyEventArgs e)
    {
        if (e.KeyCode != Keys.Enter)
            return;
        e.SuppressKeyPress = true;
        var query = (txtBarcode.Text ?? string.Empty).Trim();
        if (query.Length == 0)
        {
            CompletePayment();
            return;
        }

        var exact = _catalog.FirstOrDefault(p => p.Barcode.Equals(query, StringComparison.OrdinalIgnoreCase));
        if (exact is null)
        {
            lblLookup.Text = $"Barcode {query} not found.";
            System.Media.SystemSounds.Beep.Play();
            ToastHost.Warn($"Barcode {query} not found.");
            txtBarcode.SelectAll();
            return;
        }

        txtBarcode.Text = string.Empty;
        TryAdd(exact);
    }

    private void TryAdd(Product product)
    {
        try
        {
            var live = _catalog.FirstOrDefault(p => p.Id == product.Id) ?? product;
            ServiceLocator.Sales.AddProduct(_cart, live);
            lblLookup.Text = string.Empty;
            UpdateCart(selectLast: true, flash: true);
            FocusBarcode();
        }
        catch (Exception ex)
        {
            lblLookup.Text = UiAsync.Message(ex);
        }
    }

    private void NudgeQty(int delta)
    {
        if (cartList.SelectedIndex < 0 || cartList.SelectedIndex >= _cart.Lines.Count)
            return;
        NudgeAt(cartList.SelectedIndex, delta);
    }

    private void NudgeAt(int index, int delta)
    {
        if (index < 0 || index >= _cart.MutableLines.Count)
            return;
        var line = _cart.MutableLines[index];
        try
        {
            ServiceLocator.Sales.ChangeQty(_cart, line.ProductId, line.Qty + delta);
            UpdateCart();
        }
        catch (Exception ex)
        {
            ToastHost.Warn(UiAsync.Message(ex));
        }
    }

    private void RemoveSelected()
    {
        if (cartList.SelectedIndex >= 0)
            RemoveAt(cartList.SelectedIndex);
    }

    private void RemoveAt(int index)
    {
        if (index < 0 || index >= _cart.MutableLines.Count)
            return;
        ServiceLocator.Sales.Remove(_cart, _cart.MutableLines[index].ProductId);
        UpdateCart();
    }

    private void UpdateCart(bool selectLast = false, bool flash = false)
    {
        var select = selectLast ? _cart.Lines.Count - 1 : cartList.SelectedIndex;
        cartList.SetLines(_cart.Lines, select, flash);
        lblSubtotal.Text = Money.Format(_cart.Subtotal);
        lblDiscount.Text = Money.FormatDiscount(_cart.DiscountAmount);
        fitTotal.SetValue(Money.Format(_cart.Total));
        lblItemCount.Text = _cart.ItemCount == 0 ? string.Empty : $"{_cart.ItemCount} items";
        btnPay.Enabled = _cart.Total > 0;
        btnPay.Text = _cart.Total > 0 ? "Pay" : "Pay";
    }

    private void EditDiscount()
    {
        using var overlay = ConfirmDialog.CreateOverlay(this);
        overlay?.Show();
        using var dialog = new DiscountDialog(_cart.DiscountValue, _cart.DiscountIsPercent);
        if (dialog.ShowDialog(this) == DialogResult.OK)
        {
            try
            {
                ServiceLocator.Sales.ApplyDiscount(_cart, dialog.DiscountValue, dialog.IsPercent, _session);
                UpdateCart();
            }
            catch (Exception ex)
            {
                ToastHost.Warn(UiAsync.Message(ex));
            }
        }

        overlay?.Close();
    }

    private void CompletePayment()
    {
        if (_cart.Total <= 0)
            return;
        using var overlay = ConfirmDialog.CreateOverlay(this);
        overlay?.Show();
        using var dialog = new PaymentDialog(_cart.Total);
        var result = dialog.ShowDialog(this);
        overlay?.Close();
        if (result != DialogResult.OK)
            return;
        btnPay.Enabled = false;
        UiAsync.Run(async () =>
        {
            var sale = await ServiceLocator.Sales.CompleteSaleAsync(_cart, dialog.Request, _session);
            foreach (var line in sale.Items)
            {
                var product = _catalog.FirstOrDefault(p => p.Id == line.ProductId);
                if (product is not null)
                    product.StockQty = Math.Max(0, product.StockQty - line.Qty);
            }

            UpdateCart();
            ApplyFilter();
            _salePreview = await ServiceLocator.Sales.PreviewSaleNoAsync();
            lblSaleNo.Text = "Current sale " + _salePreview;
            ToastHost.Success($"Sale {sale.SaleNo} completed.");
            using var overlay2 = ConfirmDialog.CreateOverlay(this);
            overlay2?.Show();
            using var receipt = new ReceiptPreviewDialog(sale);
            receipt.ShowDialog(this);
            overlay2?.Close();
            FocusBarcode();
        }, btnPay);
    }

    private void CancelSale()
    {
        if (_cart.ItemCount == 0)
            return;
        if (!ConfirmDialog.Ask(this, "Cancel sale", "Clear every item on this sale?", "Clear sale"))
            return;
        ServiceLocator.Sales.Clear(_cart);
        lblLookup.Text = string.Empty;
        UpdateCart();
        FocusBarcode();
    }

    private void ShowSales()
    {
        pnlSales.Visible = true;
        pnlHistory.Visible = false;
        btnNavSales.FillColor = Theme.Accent;
        btnNavHistory.FillColor = Theme.NavRaised;
        FocusBarcode();
    }

    private void ShowHistory()
    {
        pnlSales.Visible = false;
        pnlHistory.Visible = true;
        btnNavSales.FillColor = Theme.NavRaised;
        btnNavHistory.FillColor = Theme.Accent;
        UiAsync.Run(async () =>
        {
            var today = DateTime.Now.Date;
            var rows = await ServiceLocator.Sales.GetHistoryAsync(today, today.AddDays(1), _session.User.Id, null);
            dgvHistory.Rows.Clear();
            foreach (var sale in rows)
                dgvHistory.Rows.Add(sale.SaleNo, sale.SaleDate.ToString("HH:mm"), Money.Format(sale.Total), sale.Status);
            if (rows.Count == 0)
                ToastHost.Warn("No sales yet today.");
        });
    }

    private void Logout()
    {
        if (_cart.ItemCount > 0 && !ConfirmDialog.Ask(this, "Log out", "The current sale will be cleared. Log out anyway?", "Log out"))
            return;
        DialogResult = DialogResult.Retry;
        Close();
    }

    private void FocusBarcode()
    {
        if (!txtBarcode.IsDisposed)
            txtBarcode.Focus();
    }

    private void ResizeTiles()
    {
        if (flpProducts.ClientSize.Width <= 0)
            return;
        var available = Math.Max(1, flpProducts.ClientSize.Width - flpProducts.Padding.Horizontal);
        var gap = Theme.Scale(12);
        var preferred = Theme.Scale(180);
        var columns = Math.Max(2, available / preferred);
        var width = Math.Clamp((available - columns * gap) / columns, Theme.Scale(150), Theme.Scale(200));
        foreach (Control control in flpProducts.Controls)
        {
            if (control is ProductTile)
                control.Size = new Size(width, Theme.Scale(140));
        }
    }
}

