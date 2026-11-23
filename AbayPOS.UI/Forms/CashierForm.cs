using AbayPOS.Domain;
using AbayPOS.Domain.Entities;
using AbayPOS.Domain.Models;
using AbayPOS.UI.Controls;
using AbayPOS.UI.Dialogs;
using AbayPOS.UI.Infrastructure;
using AbayPOS.UI.Theming;

namespace AbayPOS.UI.Forms;

/// <summary>UC-03 cashier register bound to ProductService and SalesService.</summary>
public sealed class CashierForm : ThemedForm
{
    private readonly Session _session;
    private readonly Cart _cart;
    private readonly List<Product> _catalog = [];
    private readonly FlowLayoutPanel _productsFlow = new();
    private readonly FlowLayoutPanel _chips = new();
    private readonly CartList _cartList = new();
    private readonly GlassTextBox _barcodeBox = new();
    private readonly Label _lookupMessage = new();
    private readonly Label _subtotalValue = new();
    private readonly Label _discountValue = new();
    private readonly Label _totalValue = new();
    private readonly Label _itemCount = new();
    private readonly Label _clockLabel = new();
    private readonly Label _cashierLabel = new();
    private readonly Label _saleNoLabel = new();
    private readonly RoundButton _payButton = new();
    private readonly RoundButton _salesNav;
    private readonly RoundButton _historyNav;
    private readonly Panel _salesPage = new() { Dock = DockStyle.Fill, BackColor = Color.Transparent };
    private readonly Panel _historyPage = new() { Dock = DockStyle.Fill, BackColor = Color.Transparent, Visible = false };
    private readonly DataGridView _historyGrid = new();
    private readonly System.Windows.Forms.Timer _clock = new();
    private readonly System.Windows.Forms.Timer _debounce = new() { Interval = 200 };
    private string _category = "All";
    private string _salePreview = "S-preview";

    public CashierForm(Session session)
    {
        _session = session;
        _cart = ServiceLocator.Sales.CreateCart();
        Text = "AbayPOS | Cashier Sales";
        Name = nameof(CashierForm);
        ClientSize = new Size(1366, 768);
        MaximizedBounds = Screen.PrimaryScreen?.WorkingArea ?? Rectangle.Empty;
        WindowState = FormWindowState.Maximized;
        _salesNav = CreateNavButton("\uE80F", "Sales", true);
        _historyNav = CreateNavButton("\uE81C", "History", false);
        BuildLayout();
        ConfigureClock();
        _debounce.Tick += (_, _) => { _debounce.Stop(); ApplyFilter(); };
        Shown += (_, _) => UiAsync.Run(LoadCatalogAsync);
        UpdateCart();
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

        if (keyData == Keys.Enter && !_barcodeBox.ContainsFocus)
        {
            CompletePayment();
            return true;
        }

        return base.ProcessCmdKey(ref msg, keyData);
    }

    protected override void OnFormClosed(FormClosedEventArgs e)
    {
        _clock.Stop();
        _clock.Dispose();
        _debounce.Dispose();
        base.OnFormClosed(e);
    }

    private void BuildLayout()
    {
        var root = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, Padding = new Padding(12), BackColor = Color.Transparent };
        root.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, Theme.SidebarWidth));
        root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        Controls.Add(root);
        root.Controls.Add(BuildSidebar(), 0, 0);
        root.Controls.Add(BuildMainArea(), 1, 0);
    }

    private Control BuildSidebar()
    {
        var sidebar = new GlassPanel { Dock = DockStyle.Fill, Margin = new Padding(0, 0, 12, 0), Padding = new Padding(15, 18, 15, 14), FillColor = Theme.SidebarGlass };
        var layout = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 5, BackColor = Color.Transparent };
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 90));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 52));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 52));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 52));
        var logo = new Panel { Dock = DockStyle.Fill, BackColor = Color.Transparent };
        logo.Controls.Add(new Label { Text = "Abay", ForeColor = Theme.Text, Font = Theme.Font(22, FontStyle.Bold), AutoSize = true, Location = new Point(0, 17) });
        logo.Controls.Add(new Label { Text = "POS", ForeColor = Theme.Accent, Font = Theme.Font(12, FontStyle.Bold), AutoSize = true, Location = new Point(2, 52) });
        _salesNav.Click += (_, _) => ShowSales();
        _historyNav.Click += (_, _) => ShowHistory();
        var logout = CreateNavButton("\uE8AC", "Log out  F12", false);
        logout.Click += (_, _) => Logout();
        layout.Controls.Add(logo, 0, 0);
        layout.Controls.Add(_salesNav, 0, 1);
        layout.Controls.Add(_historyNav, 0, 2);
        layout.Controls.Add(logout, 0, 4);
        sidebar.Controls.Add(layout);
        return sidebar;
    }

    private Control BuildMainArea()
    {
        var main = new TableLayoutPanel { Dock = DockStyle.Fill, RowCount = 2, BackColor = Color.Transparent };
        main.RowStyles.Add(new RowStyle(SizeType.Absolute, Theme.TopBarHeight));
        main.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        main.Controls.Add(BuildTopBar(), 0, 0);
        var host = new Panel { Dock = DockStyle.Fill, BackColor = Color.Transparent };
        host.Controls.Add(_historyPage);
        host.Controls.Add(_salesPage);
        BuildSalesPage();
        BuildHistoryPage();
        main.Controls.Add(host, 0, 1);
        return main;
    }

    private Control BuildTopBar()
    {
        var bar = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 3, BackColor = Color.Transparent };
        bar.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 28));
        bar.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 42));
        bar.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 30));
        var title = new Label { Text = "AbayPOS", Dock = DockStyle.Fill, ForeColor = Theme.Text, Font = Theme.Font(15, FontStyle.Bold), TextAlign = ContentAlignment.MiddleLeft };
        title.MouseDown += DragWindow;
        bar.MouseDown += DragWindow;
        bar.Controls.Add(title, 0, 0);
        var search = new GlassPanel { Dock = DockStyle.Fill, Margin = new Padding(8, 7, 8, 7), Radius = 18, FillColor = Theme.SearchGlass };
        var searchText = new Label { Text = "\uE721   Search...  Ctrl+K / F2", Dock = DockStyle.Fill, ForeColor = Theme.TextSecondary, Font = Theme.FontHint(), TextAlign = ContentAlignment.MiddleLeft, Padding = new Padding(12, 0, 0, 0), Cursor = Cursors.Hand };
        searchText.Click += (_, _) => FocusBarcode();
        search.Controls.Add(searchText);
        bar.Controls.Add(search, 1, 0);
        var right = CreateWindowButtons(this);
        _clockLabel.AutoSize = true;
        _clockLabel.ForeColor = Theme.TextSecondary;
        _clockLabel.Font = Theme.FontHint();
        _clockLabel.Margin = new Padding(12, 21, 0, 0);
        _cashierLabel.Text = _session.User.FullName;
        _cashierLabel.AutoSize = true;
        _cashierLabel.ForeColor = Theme.Text;
        _cashierLabel.Font = Theme.Font(9, FontStyle.Bold);
        _cashierLabel.Margin = new Padding(8, 21, 0, 0);
        right.Controls.Add(_clockLabel);
        right.Controls.Add(_cashierLabel);
        bar.Controls.Add(right, 2, 0);
        return bar;
    }

    private void BuildSalesPage()
    {
        var content = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, Padding = new Padding(0, 8, 0, 0), BackColor = Color.Transparent };
        content.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 62));
        content.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 38));
        content.Controls.Add(BuildProductPanel(), 0, 0);
        content.Controls.Add(BuildCartPanel(), 1, 0);
        _salesPage.Controls.Add(content);
    }

    private void BuildHistoryPage()
    {
        var panel = new GlassPanel { Dock = DockStyle.Fill, Margin = new Padding(0, 8, 0, 0), Padding = new Padding(16) };
        var layout = new TableLayoutPanel { Dock = DockStyle.Fill, RowCount = 2, BackColor = Color.Transparent };
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 36));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        layout.Controls.Add(new Label { Text = "Your sales today", Font = Theme.FontHeading(), ForeColor = Theme.Text, Dock = DockStyle.Fill }, 0, 0);
        _historyGrid.Dock = DockStyle.Fill;
        ThemeGrid(_historyGrid);
        _historyGrid.Columns.Add("SaleNo", "Sale");
        _historyGrid.Columns.Add("Time", "Time");
        _historyGrid.Columns.Add("Total", "Total");
        _historyGrid.Columns.Add("Status", "Status");
        layout.Controls.Add(_historyGrid, 0, 1);
        panel.Controls.Add(layout);
        _historyPage.Controls.Add(panel);
    }

    private Control BuildProductPanel()
    {
        var panel = new GlassPanel { Dock = DockStyle.Fill, Margin = new Padding(0, 0, 8, 0), Padding = new Padding(16, 14, 16, 12) };
        var layout = new TableLayoutPanel { Dock = DockStyle.Fill, RowCount = 4, BackColor = Color.Transparent };
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 50));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 40));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 22));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        var scan = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, BackColor = Color.Transparent };
        scan.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        scan.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 72));
        _barcodeBox.Dock = DockStyle.Fill;
        _barcodeBox.Placeholder = "Scan barcode or search product";
        _barcodeBox.KeyDown += BarcodeKeyDown;
        _barcodeBox.TextChanged += (_, _) => { _debounce.Stop(); _debounce.Start(); };
        scan.Controls.Add(_barcodeBox, 0, 0);
        scan.Controls.Add(new Label { Text = "F2", Dock = DockStyle.Fill, ForeColor = Theme.TextSecondary, Font = Theme.FontHint(), TextAlign = ContentAlignment.MiddleCenter }, 1, 0);
        layout.Controls.Add(scan, 0, 0);
        _chips.Dock = DockStyle.Fill;
        _chips.WrapContents = false;
        _chips.AutoScroll = true;
        _chips.BackColor = Color.Transparent;
        layout.Controls.Add(_chips, 0, 1);
        _lookupMessage.Dock = DockStyle.Fill;
        _lookupMessage.ForeColor = Theme.Danger;
        _lookupMessage.Font = Theme.FontHint();
        layout.Controls.Add(_lookupMessage, 0, 2);
        _productsFlow.Dock = DockStyle.Fill;
        _productsFlow.AutoScroll = true;
        _productsFlow.WrapContents = true;
        _productsFlow.BackColor = Color.Transparent;
        _productsFlow.Resize += (_, _) => ResizeTiles();
        layout.Controls.Add(_productsFlow, 0, 3);
        panel.Controls.Add(layout);
        return panel;
    }

    private Control BuildCartPanel()
    {
        var panel = new GlassPanel { Dock = DockStyle.Fill, Margin = new Padding(8, 0, 0, 0), Padding = new Padding(16, 14, 16, 12) };
        var layout = new TableLayoutPanel { Dock = DockStyle.Fill, RowCount = 6, BackColor = Color.Transparent };
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 42));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 120));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, Theme.PayButtonHeight + 8));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 48));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 8));
        _saleNoLabel.Text = "Current sale " + _salePreview;
        _saleNoLabel.Dock = DockStyle.Fill;
        _saleNoLabel.ForeColor = Theme.Text;
        _saleNoLabel.Font = Theme.Font(11, FontStyle.Bold);
        layout.Controls.Add(_saleNoLabel, 0, 0);
        _cartList.Dock = DockStyle.Fill;
        _cartList.QuantityPlus += (_, i) => NudgeAt(i, 1);
        _cartList.QuantityMinus += (_, i) => NudgeAt(i, -1);
        _cartList.LineRemoved += (_, i) => RemoveAt(i);
        layout.Controls.Add(_cartList, 0, 1);
        layout.Controls.Add(BuildSummary(), 0, 2);
        _payButton.Dock = DockStyle.Fill;
        _payButton.Text = "Pay";
        _payButton.Badge = "Enter";
        _payButton.IsPrimary = true;
        _payButton.Click += (_, _) => CompletePayment();
        layout.Controls.Add(_payButton, 0, 3);
        var cancel = new RoundButton { Dock = DockStyle.Fill, Text = "Cancel sale  Esc", FillColor = Theme.CancelGlass, BorderColor = Theme.GlassBorder };
        cancel.Click += (_, _) => CancelSale();
        layout.Controls.Add(cancel, 0, 4);
        panel.Controls.Add(layout);
        return panel;
    }

    private Control BuildSummary()
    {
        var summary = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 4, BackColor = Color.Transparent };
        summary.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 55));
        summary.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 45));
        summary.Controls.Add(Muted("Subtotal"), 0, 0);
        summary.Controls.Add(Bind(_subtotalValue, 12), 1, 0);
        var discountLabel = Muted("Discount  F4");
        discountLabel.Cursor = Cursors.Hand;
        discountLabel.Click += (_, _) => EditDiscount();
        summary.Controls.Add(discountLabel, 0, 1);
        _discountValue.ForeColor = Theme.Accent;
        summary.Controls.Add(Bind(_discountValue, 12), 1, 1);
        summary.Controls.Add(new Label { Text = "Total", Dock = DockStyle.Fill, ForeColor = Theme.Text, Font = Theme.Font(12, FontStyle.Bold), TextAlign = ContentAlignment.MiddleLeft }, 0, 2);
        summary.Controls.Add(Bind(_totalValue, 28, FontStyle.Bold), 1, 2);
        _itemCount.ForeColor = Theme.TextSecondary;
        _itemCount.Font = Theme.FontHint();
        _itemCount.Dock = DockStyle.Fill;
        summary.Controls.Add(_itemCount, 0, 3);
        return summary;
    }

    private async Task LoadCatalogAsync()
    {
        _catalog.Clear();
        _catalog.AddRange(await ServiceLocator.Products.GetActiveAsync());
        var categories = await ServiceLocator.Products.GetCategoriesAsync();
        _salePreview = await ServiceLocator.Sales.PreviewSaleNoAsync();
        _saleNoLabel.Text = "Current sale " + _salePreview;
        RebuildChips(categories);
        RenderTiles(_catalog);
        FocusBarcode();
    }

    private void RebuildChips(IReadOnlyList<string> categories)
    {
        _chips.SuspendLayout();
        _chips.Controls.Clear();
        foreach (var name in new[] { "All" }.Concat(categories))
        {
            var chip = new ChipButton(name) { Selected = name == _category, Margin = new Padding(0, 4, 8, 4) };
            chip.Click += (_, _) =>
            {
                _category = name;
                foreach (Control control in _chips.Controls)
                {
                    if (control is ChipButton other)
                        other.Selected = other.Text == name;
                }

                ApplyFilter();
            };
            _chips.Controls.Add(chip);
        }

        _chips.ResumeLayout();
    }

    private void ApplyFilter()
    {
        var query = (_barcodeBox.Text ?? string.Empty).Trim();
        IEnumerable<Product> filtered = _catalog;
        if (_category != "All")
            filtered = filtered.Where(p => p.Category == _category);
        if (query.Length > 0)
            filtered = filtered.Where(p => p.Name.Contains(query, StringComparison.OrdinalIgnoreCase) || p.Barcode.Contains(query, StringComparison.OrdinalIgnoreCase));
        RenderTiles(filtered.ToList());
    }

    private void RenderTiles(IReadOnlyList<Product> products)
    {
        _productsFlow.SuspendLayout();
        _productsFlow.Controls.Clear();
        if (products.Count == 0)
        {
            _productsFlow.Controls.Add(new Label { Text = "No products match that search.", ForeColor = Theme.TextSecondary, AutoSize = true, Font = Theme.FontBody(), Margin = new Padding(8) });
        }
        else
        {
            foreach (var product in products)
            {
                var tile = new ProductTile(product);
                if (!product.IsOutOfStock)
                    tile.Click += (_, _) => TryAdd(product);
                _productsFlow.Controls.Add(tile);
            }
        }

        _productsFlow.ResumeLayout();
        ResizeTiles();
    }

    private void BarcodeKeyDown(object? sender, KeyEventArgs e)
    {
        if (e.KeyCode != Keys.Enter)
            return;
        e.SuppressKeyPress = true;
        var query = (_barcodeBox.Text ?? string.Empty).Trim();
        if (query.Length == 0)
        {
            CompletePayment();
            return;
        }

        var exact = _catalog.FirstOrDefault(p => p.Barcode.Equals(query, StringComparison.OrdinalIgnoreCase));
        if (exact is null)
        {
            _lookupMessage.Text = "No product found. Check the barcode or search term.";
            _barcodeBox.SelectAll();
            return;
        }

        _barcodeBox.Text = string.Empty;
        TryAdd(exact);
    }

    private void TryAdd(Product product)
    {
        try
        {
            var live = _catalog.FirstOrDefault(p => p.Id == product.Id) ?? product;
            ServiceLocator.Sales.AddProduct(_cart, live);
            _lookupMessage.Text = string.Empty;
            UpdateCart(selectLast: true, flash: true);
            FocusBarcode();
        }
        catch (Exception ex)
        {
            _lookupMessage.Text = UiAsync.Message(ex);
        }
    }

    private void NudgeQty(int delta)
    {
        if (_cartList.SelectedIndex < 0 || _cartList.SelectedIndex >= _cart.Lines.Count)
            return;
        NudgeAt(_cartList.SelectedIndex, delta);
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
        if (_cartList.SelectedIndex >= 0)
            RemoveAt(_cartList.SelectedIndex);
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
        var select = selectLast ? _cart.Lines.Count - 1 : _cartList.SelectedIndex;
        _cartList.SetLines(_cart.Lines, select, flash);
        _subtotalValue.Text = Money.Format(_cart.Subtotal);
        _discountValue.Text = Money.FormatDiscount(_cart.DiscountAmount);
        _totalValue.Text = Money.Format(_cart.Total);
        _itemCount.Text = _cart.ItemCount == 0 ? string.Empty : $"{_cart.ItemCount} items";
        _payButton.Enabled = _cart.Total > 0;
        _payButton.Text = _cart.Total > 0 ? "Pay" : "Pay";
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
        _payButton.Enabled = false;
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
            _saleNoLabel.Text = "Current sale " + _salePreview;
            ToastHost.Success($"Sale {sale.SaleNo} completed.");
            using var overlay2 = ConfirmDialog.CreateOverlay(this);
            overlay2?.Show();
            using var receipt = new ReceiptPreviewDialog(sale);
            receipt.ShowDialog(this);
            overlay2?.Close();
            FocusBarcode();
        }, _payButton);
    }

    private void CancelSale()
    {
        if (_cart.ItemCount == 0)
            return;
        if (!ConfirmDialog.Ask(this, "Cancel sale", "Clear every item on this sale?", "Clear sale"))
            return;
        ServiceLocator.Sales.Clear(_cart);
        _lookupMessage.Text = string.Empty;
        UpdateCart();
        FocusBarcode();
    }

    private void ShowSales()
    {
        _salesPage.Visible = true;
        _historyPage.Visible = false;
        _salesNav.FillColor = Theme.Accent;
        _historyNav.FillColor = Theme.NavGlass;
        FocusBarcode();
    }

    private void ShowHistory()
    {
        _salesPage.Visible = false;
        _historyPage.Visible = true;
        _salesNav.FillColor = Theme.NavGlass;
        _historyNav.FillColor = Theme.Accent;
        UiAsync.Run(async () =>
        {
            var today = DateTime.Now.Date;
            var rows = await ServiceLocator.Sales.GetHistoryAsync(today, today.AddDays(1), _session.User.Id, null);
            _historyGrid.Rows.Clear();
            foreach (var sale in rows)
                _historyGrid.Rows.Add(sale.SaleNo, sale.SaleDate.ToString("HH:mm"), Money.Format(sale.Total), sale.Status);
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
        if (!_barcodeBox.IsDisposed)
            _barcodeBox.Focus();
    }

    private void ResizeTiles()
    {
        if (_productsFlow.ClientSize.Width <= 0)
            return;
        var available = Math.Max(1, _productsFlow.ClientSize.Width - _productsFlow.Padding.Horizontal);
        var columns = Math.Max(2, available / 182);
        var width = Math.Max(145, (available - columns * 14) / columns);
        foreach (Control control in _productsFlow.Controls)
        {
            if (control is ProductTile)
                control.Size = new Size(width, Math.Max(154, (int)(width * 0.96)));
        }
    }

    private void ConfigureClock()
    {
        _clock.Interval = 1000;
        _clock.Tick += (_, _) => _clockLabel.Text = DateTime.Now.ToString("hh:mm tt");
        _clock.Start();
        _clockLabel.Text = DateTime.Now.ToString("hh:mm tt");
    }

    private static RoundButton CreateNavButton(string icon, string text, bool active) => new()
    {
        Text = text,
        Icon = icon,
        Dock = DockStyle.Fill,
        Margin = new Padding(0, 3, 0, 3),
        Font = Theme.Font(10, active ? FontStyle.Bold : FontStyle.Regular),
        FillColor = active ? Theme.Accent : Theme.NavGlass,
        BorderColor = active ? Theme.Accent : Color.Transparent,
        LabelAlign = ContentAlignment.MiddleLeft
    };

    private static Label Muted(string text) => new()
    {
        Text = text,
        Dock = DockStyle.Fill,
        ForeColor = Theme.TextSecondary,
        Font = Theme.FontHint(),
        TextAlign = ContentAlignment.MiddleLeft
    };

    private static Label Bind(Label label, float size, FontStyle style = FontStyle.Regular)
    {
        label.Dock = DockStyle.Fill;
        label.ForeColor = size >= 20 ? Theme.Text : label.ForeColor == Theme.Accent ? Theme.Accent : Theme.Text;
        label.Font = Theme.Font(size, style);
        label.TextAlign = ContentAlignment.MiddleRight;
        return label;
    }

    private static void ThemeGrid(DataGridView grid)
    {
        grid.BackgroundColor = Theme.CartSurface;
        grid.BorderStyle = BorderStyle.None;
        grid.EnableHeadersVisualStyles = false;
        grid.ColumnHeadersDefaultCellStyle.BackColor = Theme.GlassSolid;
        grid.ColumnHeadersDefaultCellStyle.ForeColor = Theme.TextSecondary;
        grid.DefaultCellStyle.BackColor = Theme.CartSurface;
        grid.DefaultCellStyle.ForeColor = Theme.Text;
        grid.DefaultCellStyle.SelectionBackColor = Theme.Selection;
        grid.RowHeadersVisible = false;
        grid.AllowUserToAddRows = false;
        grid.ReadOnly = true;
        grid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        grid.RowTemplate.Height = Theme.TouchTarget;
    }
}
