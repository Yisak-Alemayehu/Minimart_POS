using AbayPOS.Domain;
using AbayPOS.Domain.Entities;
using AbayPOS.Domain.Models;
using AbayPOS.UI.Controls;
using AbayPOS.UI.Dialogs;
using AbayPOS.UI.Theming;

namespace AbayPOS.UI.Forms;

/// <summary>Cashier sales screen. Phase 1 keeps the existing in-memory catalog; Phase 3 binds services.</summary>
public sealed class CashierForm : ThemedForm
{
    private readonly List<Product> _products =
    [
        new() { Name = "Coca-Cola 500ml", Barcode = "1001", Price = 45m, Emoji = "🥤", StockQty = 80, ReorderLevel = 12, Category = "Beverages" },
        new() { Name = "Bread", Barcode = "1002", Price = 20m, Emoji = "🍞", StockQty = 8, ReorderLevel = 15, Category = "Bakery" },
        new() { Name = "Milk 1L", Barcode = "1003", Price = 85m, Emoji = "🥛", StockQty = 6, ReorderLevel = 10, Category = "Dairy" },
        new() { Name = "Rice 5kg", Barcode = "1004", Price = 650m, Emoji = "🍚", StockQty = 24, ReorderLevel = 8, Category = "Staples" },
        new() { Name = "Sugar 1kg", Barcode = "1005", Price = 120m, Emoji = "🍬", StockQty = 40, ReorderLevel = 10, Category = "Staples" },
        new() { Name = "Soap", Barcode = "1006", Price = 40m, Emoji = "🧼", StockQty = 55, ReorderLevel = 12, Category = "Personal Care" }
    ];

    private readonly Cart _cart = new();
    private readonly FlowLayoutPanel _productsFlow = new();
    private readonly DataGridView _cartGrid = new();
    private readonly GlassTextBox _barcodeBox = new();
    private readonly Label _lookupMessage = new();
    private readonly Label _subtotalValue = new();
    private readonly Label _discountValue = new();
    private readonly Label _totalValue = new();
    private readonly Label _clockLabel = new();
    private readonly Label _cashierLabel = new();
    private readonly RoundButton _payButton = new();
    private readonly System.Windows.Forms.Timer _clock = new();

    public CashierForm()
    {
        Text = "AbayPOS | Cashier Sales";
        Name = nameof(CashierForm);
        ClientSize = new Size(1366, 768);
        MaximizedBounds = Screen.PrimaryScreen?.WorkingArea ?? Rectangle.Empty;
        WindowState = FormWindowState.Maximized;
        BuildLayout();
        ConfigureClock();
        UpdateCart();
    }

    protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
    {
        if (keyData is Keys.F2 or (Keys.Control | Keys.K))
        {
            FocusBarcode();
            return true;
        }

        if (keyData == Keys.Escape)
        {
            CancelSale();
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
        base.OnFormClosed(e);
    }

    private void BuildLayout()
    {
        var root = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 1,
            Padding = new Padding(12),
            BackColor = Color.Transparent
        };
        root.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, Theme.SidebarWidth));
        root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        Controls.Add(root);
        root.Controls.Add(BuildSidebar(), 0, 0);
        root.Controls.Add(BuildMainArea(), 1, 0);
    }

    private Control BuildSidebar()
    {
        var sidebar = new GlassPanel
        {
            Dock = DockStyle.Fill,
            Margin = new Padding(0, 0, 12, 0),
            Padding = new Padding(15, 18, 15, 14),
            FillColor = Theme.SidebarGlass
        };
        var layout = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 5, BackColor = Color.Transparent };
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 90));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 52));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 52));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 52));
        sidebar.Controls.Add(layout);

        var logo = new Panel { Dock = DockStyle.Fill, BackColor = Color.Transparent };
        logo.Controls.Add(new Label { Text = "Abay", ForeColor = Theme.Text, Font = Theme.Font(22, FontStyle.Bold), AutoSize = true, Location = new Point(0, 17) });
        logo.Controls.Add(new Label { Text = "POS", ForeColor = Theme.Accent, Font = Theme.Font(12, FontStyle.Bold), AutoSize = true, Location = new Point(2, 52) });
        layout.Controls.Add(logo, 0, 0);
        layout.Controls.Add(CreateNavButton("\uE80F", "Sales", true), 0, 1);
        layout.Controls.Add(CreateNavButton("\uE81C", "History", false), 0, 2);
        layout.Controls.Add(CreateNavButton("\uE8AC", "Log out", false), 0, 4);
        return sidebar;
    }

    private Control BuildMainArea()
    {
        var main = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 2, BackColor = Color.Transparent };
        main.RowStyles.Add(new RowStyle(SizeType.Absolute, Theme.TopBarHeight));
        main.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        main.Controls.Add(BuildTopBar(), 0, 0);

        var content = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 1, Padding = new Padding(0, 8, 0, 0), BackColor = Color.Transparent };
        content.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 62));
        content.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 38));
        content.Controls.Add(BuildProductPanel(), 0, 0);
        content.Controls.Add(BuildCartPanel(), 1, 0);
        main.Controls.Add(content, 0, 1);
        return main;
    }

    private Control BuildTopBar()
    {
        var bar = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 3, RowCount = 1, BackColor = Color.Transparent };
        bar.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 28));
        bar.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 42));
        bar.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 30));
        var title = new Label { Text = "AbayPOS", Dock = DockStyle.Fill, ForeColor = Theme.Text, Font = Theme.Font(15, FontStyle.Bold), TextAlign = ContentAlignment.MiddleLeft };
        title.MouseDown += DragWindow;
        bar.MouseDown += DragWindow;
        bar.Controls.Add(title, 0, 0);

        var search = new GlassPanel { Dock = DockStyle.Fill, Margin = new Padding(8, 7, 8, 7), Radius = 18, FillColor = Theme.SearchGlass };
        var searchText = new Label { Text = "\uE721   Search...  Ctrl+K", Dock = DockStyle.Fill, ForeColor = Theme.TextSecondary, Font = Theme.FontHint(), TextAlign = ContentAlignment.MiddleLeft, Padding = new Padding(12, 0, 0, 0), Cursor = Cursors.Hand };
        searchText.Click += (_, _) => FocusBarcode();
        search.Controls.Add(searchText);
        bar.Controls.Add(search, 1, 0);

        var right = CreateWindowButtons(this);
        _clockLabel.Text = DateTime.Now.ToString("hh:mm tt");
        _clockLabel.AutoSize = true;
        _clockLabel.ForeColor = Theme.TextSecondary;
        _clockLabel.Font = Theme.FontHint();
        _clockLabel.Margin = new Padding(12, 21, 0, 0);
        _cashierLabel.Text = "Cashier: Abebe Kebede";
        _cashierLabel.AutoSize = true;
        _cashierLabel.ForeColor = Theme.Text;
        _cashierLabel.Font = Theme.Font(9, FontStyle.Bold);
        _cashierLabel.Margin = new Padding(8, 21, 0, 0);
        right.Controls.Add(_clockLabel);
        right.Controls.Add(_cashierLabel);
        bar.Controls.Add(right, 2, 0);
        return bar;
    }

    private Control BuildProductPanel()
    {
        var panel = new GlassPanel { Dock = DockStyle.Fill, Margin = new Padding(0, 0, 8, 0), Padding = new Padding(16, 14, 16, 12) };
        var layout = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 3, BackColor = Color.Transparent };
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 50));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 24));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        panel.Controls.Add(layout);

        var scan = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, BackColor = Color.Transparent };
        scan.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        scan.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 72));
        _barcodeBox.Dock = DockStyle.Fill;
        _barcodeBox.Margin = new Padding(0, 2, 8, 4);
        _barcodeBox.Placeholder = "Scan barcode or search product";
        _barcodeBox.KeyDown += BarcodeBox_KeyDown;
        scan.Controls.Add(_barcodeBox, 0, 0);
        scan.Controls.Add(new Label { Text = "Ctrl+K", Dock = DockStyle.Fill, ForeColor = Theme.TextSecondary, Font = Theme.FontHint(), TextAlign = ContentAlignment.MiddleCenter }, 1, 0);
        layout.Controls.Add(scan, 0, 0);

        _lookupMessage.Dock = DockStyle.Fill;
        _lookupMessage.ForeColor = Theme.Danger;
        _lookupMessage.Font = Theme.FontHint();
        layout.Controls.Add(_lookupMessage, 0, 1);

        _productsFlow.Dock = DockStyle.Fill;
        _productsFlow.AutoScroll = true;
        _productsFlow.WrapContents = true;
        _productsFlow.FlowDirection = FlowDirection.LeftToRight;
        _productsFlow.BackColor = Color.Transparent;
        _productsFlow.Padding = new Padding(2, 8, 2, 0);
        _productsFlow.Resize += (_, _) => ResizeProductTiles();
        foreach (var product in _products)
        {
            var tile = new ProductTile(product);
            tile.Click += (_, _) => AddProduct(product);
            _productsFlow.Controls.Add(tile);
        }

        layout.Controls.Add(_productsFlow, 0, 2);
        return panel;
    }

    private Control BuildCartPanel()
    {
        var panel = new GlassPanel { Dock = DockStyle.Fill, Margin = new Padding(8, 0, 0, 0), Padding = new Padding(16, 14, 16, 12) };
        var layout = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 6, BackColor = Color.Transparent };
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 42));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 32));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 108));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, Theme.PayButtonHeight + 8));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 48));
        panel.Controls.Add(layout);

        var saleHeader = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.LeftToRight, WrapContents = false, BackColor = Color.Transparent, Padding = new Padding(0, 3, 0, 0) };
        saleHeader.Controls.Add(new Label { Text = "●", AutoSize = true, ForeColor = Theme.Accent, Font = Theme.Font(11), Margin = new Padding(0, 6, 7, 0) });
        saleHeader.Controls.Add(new Label { Text = "Current Sale #S-preview", AutoSize = true, ForeColor = Theme.Text, Font = Theme.Font(11, FontStyle.Bold), Margin = new Padding(0, 3, 0, 0) });
        layout.Controls.Add(saleHeader, 0, 0);

        var headings = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 3, BackColor = Color.Transparent };
        headings.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 52));
        headings.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 18));
        headings.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 30));
        headings.Controls.Add(Header("Item", ContentAlignment.MiddleLeft), 0, 0);
        headings.Controls.Add(Header("Qty", ContentAlignment.MiddleCenter), 1, 0);
        headings.Controls.Add(Header("Total", ContentAlignment.MiddleRight), 2, 0);
        layout.Controls.Add(headings, 0, 1);

        ConfigureCartGrid();
        layout.Controls.Add(_cartGrid, 0, 2);
        layout.Controls.Add(BuildSummary(), 0, 3);

        _payButton.Dock = DockStyle.Fill;
        _payButton.Margin = new Padding(0, 4, 0, 3);
        _payButton.Text = "Pay";
        _payButton.Icon = "\uE8C7";
        _payButton.Badge = "Enter";
        _payButton.IsPrimary = true;
        _payButton.Click += (_, _) => CompletePayment();
        _payButton.MouseEnter += (_, _) => UpdatePayText(true);
        _payButton.MouseLeave += (_, _) => UpdatePayText(false);
        layout.Controls.Add(_payButton, 0, 4);

        var cancel = new RoundButton { Dock = DockStyle.Fill, Margin = new Padding(0, 4, 0, 0), Text = "Cancel Sale", FillColor = Theme.CancelGlass, BorderColor = Theme.GlassBorder, Icon = "\uE711" };
        cancel.Click += (_, _) => CancelSale();
        layout.Controls.Add(cancel, 0, 5);
        return panel;
    }

    private Control BuildSummary()
    {
        var summary = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 4, Padding = new Padding(0, 6, 0, 0), BackColor = Color.Transparent };
        summary.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 58));
        summary.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 42));
        summary.RowStyles.Add(new RowStyle(SizeType.Percent, 27));
        summary.RowStyles.Add(new RowStyle(SizeType.Percent, 27));
        summary.RowStyles.Add(new RowStyle(SizeType.Absolute, 1));
        summary.RowStyles.Add(new RowStyle(SizeType.Percent, 46));
        summary.Controls.Add(SummaryLabel("Subtotal"), 0, 0);
        summary.Controls.Add(ValueLabel(_subtotalValue, Theme.Text, 9), 1, 0);
        summary.Controls.Add(SummaryLabel("Discount"), 0, 1);
        summary.Controls.Add(ValueLabel(_discountValue, Theme.Accent, 9), 1, 1);
        var rule = new Panel { Dock = DockStyle.Fill, BackColor = Theme.GlassBorder, Margin = new Padding(0, 3, 0, 3) };
        summary.Controls.Add(rule, 0, 2);
        summary.SetColumnSpan(rule, 2);
        summary.Controls.Add(new Label { Text = "Total", Dock = DockStyle.Fill, ForeColor = Theme.Text, Font = Theme.Font(11, FontStyle.Bold), TextAlign = ContentAlignment.MiddleLeft }, 0, 3);
        summary.Controls.Add(ValueLabel(_totalValue, Theme.Text, 28, FontStyle.Bold), 1, 3);
        return summary;
    }

    private void ConfigureCartGrid()
    {
        _cartGrid.Dock = DockStyle.Fill;
        _cartGrid.BackgroundColor = Theme.CartSurface;
        _cartGrid.BorderStyle = BorderStyle.None;
        _cartGrid.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
        _cartGrid.GridColor = Color.FromArgb(52, 48, 64);
        _cartGrid.ColumnHeadersVisible = false;
        _cartGrid.RowHeadersVisible = false;
        _cartGrid.AllowUserToAddRows = false;
        _cartGrid.AllowUserToDeleteRows = false;
        _cartGrid.AllowUserToResizeRows = false;
        _cartGrid.MultiSelect = false;
        _cartGrid.ReadOnly = true;
        _cartGrid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        _cartGrid.RowTemplate.Height = Theme.TouchTarget;
        _cartGrid.DefaultCellStyle.BackColor = Theme.CartSurface;
        _cartGrid.DefaultCellStyle.ForeColor = Theme.Text;
        _cartGrid.DefaultCellStyle.SelectionBackColor = Theme.Selection;
        _cartGrid.DefaultCellStyle.SelectionForeColor = Theme.Text;
        _cartGrid.DefaultCellStyle.Font = Theme.Font(12);
        _cartGrid.DefaultCellStyle.Padding = new Padding(4, 0, 4, 0);
        _cartGrid.EnableHeadersVisualStyles = false;
        _cartGrid.Columns.Add(new DataGridViewTextBoxColumn { Name = "Item", AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill, FillWeight = 52 });
        _cartGrid.Columns.Add(new DataGridViewTextBoxColumn { Name = "Qty", AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill, FillWeight = 18, DefaultCellStyle = new DataGridViewCellStyle { Alignment = DataGridViewContentAlignment.MiddleCenter } });
        _cartGrid.Columns.Add(new DataGridViewTextBoxColumn { Name = "Total", AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill, FillWeight = 30, DefaultCellStyle = new DataGridViewCellStyle { Alignment = DataGridViewContentAlignment.MiddleRight } });
        _cartGrid.KeyDown += CartGrid_KeyDown;
    }

    private void ConfigureClock()
    {
        _clock.Interval = 1000;
        _clock.Tick += (_, _) => _clockLabel.Text = DateTime.Now.ToString("hh:mm tt");
        _clock.Start();
    }

    private void ResizeProductTiles()
    {
        if (_productsFlow.ClientSize.Width <= 0)
            return;
        var available = Math.Max(1, _productsFlow.ClientSize.Width - _productsFlow.Padding.Horizontal);
        var columns = Math.Max(2, available / 182);
        var width = Math.Max(145, (available - columns * 14) / columns);
        foreach (Control control in _productsFlow.Controls)
            control.Size = new Size(width, Math.Max(154, (int)(width * 0.96)));
    }

    private void AddProduct(Product product)
    {
        if (product.IsOutOfStock)
            return;

        var line = _cart.MutableLines.FirstOrDefault(item => item.Barcode == product.Barcode);
        if (line is null)
        {
            _cart.MutableLines.Add(new CartLine
            {
                ProductId = product.Id,
                ProductName = product.Name,
                Barcode = product.Barcode,
                Category = product.Category,
                Emoji = product.Emoji,
                UnitPrice = product.Price,
                Qty = 1,
                AvailableStock = product.StockQty
            });
        }
        else
        {
            line.Qty++;
        }

        _lookupMessage.Text = string.Empty;
        UpdateCart(product.Barcode);
        FocusBarcode();
    }

    private void BarcodeBox_KeyDown(object? sender, KeyEventArgs e)
    {
        if (e.KeyCode != Keys.Enter)
            return;
        e.SuppressKeyPress = true;
        e.Handled = true;
        var query = (_barcodeBox.Text ?? string.Empty).Trim();
        if (query.Length == 0)
            return;
        var product = _products.FirstOrDefault(item =>
            item.Barcode.Equals(query, StringComparison.OrdinalIgnoreCase) ||
            item.Name.Contains(query, StringComparison.OrdinalIgnoreCase));
        if (product is null)
        {
            _lookupMessage.Text = "No product found. Check the barcode or search term.";
            _barcodeBox.SelectAll();
            return;
        }

        _barcodeBox.Text = string.Empty;
        AddProduct(product);
    }

    private void CartGrid_KeyDown(object? sender, KeyEventArgs e)
    {
        if (e.KeyCode == Keys.Delete)
        {
            RemoveSelectedLine();
            e.Handled = true;
        }
        else if (e.KeyCode is Keys.Add or Keys.Oemplus)
        {
            ChangeSelectedQuantity(1);
            e.Handled = true;
        }
        else if (e.KeyCode is Keys.Subtract or Keys.OemMinus)
        {
            ChangeSelectedQuantity(-1);
            e.Handled = true;
        }
    }

    private void UpdateCart(string? selectBarcode = null)
    {
        var selectedIndex = selectBarcode is null
            ? _cartGrid.CurrentRow?.Index ?? -1
            : _cart.MutableLines.FindIndex(line => line.Barcode == selectBarcode);
        _cartGrid.Rows.Clear();
        foreach (var line in _cart.Lines)
            _cartGrid.Rows.Add(line.ProductName, line.Qty, Money.Format(line.LineTotal));
        if (_cartGrid.Rows.Count > 0)
        {
            var row = Math.Clamp(selectedIndex < 0 ? _cartGrid.Rows.Count - 1 : selectedIndex, 0, _cartGrid.Rows.Count - 1);
            _cartGrid.Rows[row].Selected = true;
            _cartGrid.CurrentCell = _cartGrid.Rows[row].Cells[0];
        }

        _subtotalValue.Text = Money.Format(_cart.Subtotal);
        _discountValue.Text = Money.FormatDiscount(_cart.DiscountAmount);
        _totalValue.Text = Money.Format(_cart.Total);
        _payButton.Enabled = _cart.Total > 0;
        UpdatePayText(false);
    }

    private void ChangeSelectedQuantity(int delta)
    {
        if (_cartGrid.CurrentRow is null)
            return;
        var index = _cartGrid.CurrentRow.Index;
        if (index < 0 || index >= _cart.MutableLines.Count)
            return;
        _cart.MutableLines[index].Qty += delta;
        if (_cart.MutableLines[index].Qty <= 0)
            _cart.MutableLines.RemoveAt(index);
        UpdateCart();
    }

    private void RemoveSelectedLine()
    {
        if (_cartGrid.CurrentRow is null)
            return;
        var index = _cartGrid.CurrentRow.Index;
        if (index >= 0 && index < _cart.MutableLines.Count)
        {
            _cart.MutableLines.RemoveAt(index);
            UpdateCart();
        }
    }

    private void CompletePayment()
    {
        if (_cart.Total <= 0)
            return;
        using var dialog = new PaymentDialog(_cart.Total);
        if (dialog.ShowDialog(this) != DialogResult.OK)
            return;
        _cart.Clear();
        UpdateCart();
        FocusBarcode();
    }

    private void CancelSale()
    {
        if (_cart.ItemCount == 0)
            return;
        if (MessageBox.Show(this, "Clear the current sale?", "Cancel sale", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
            return;
        _cart.Clear();
        _lookupMessage.Text = string.Empty;
        UpdateCart();
        FocusBarcode();
    }

    private void FocusBarcode()
    {
        if (_barcodeBox.IsDisposed)
            return;
        _barcodeBox.Focus();
        _barcodeBox.SelectAll();
    }

    private void UpdatePayText(bool hovered)
    {
        _payButton.Text = hovered && _payButton.Enabled ? $"Collect {_totalValue.Text}" : "Pay";
        _payButton.Invalidate();
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

    private static Label Header(string text, ContentAlignment alignment) => new()
    {
        Text = text,
        Dock = DockStyle.Fill,
        ForeColor = Theme.TextSecondary,
        Font = Theme.Font(9, FontStyle.Bold),
        TextAlign = alignment
    };

    private static Label SummaryLabel(string text) => new()
    {
        Text = text,
        Dock = DockStyle.Fill,
        ForeColor = Theme.TextSecondary,
        Font = Theme.FontHint(),
        TextAlign = ContentAlignment.MiddleLeft
    };

    private static Label ValueLabel(Label label, Color color, float size, FontStyle style = FontStyle.Regular)
    {
        label.Dock = DockStyle.Fill;
        label.ForeColor = color;
        label.Font = Theme.Font(size, style);
        label.TextAlign = ContentAlignment.MiddleRight;
        return label;
    }
}
