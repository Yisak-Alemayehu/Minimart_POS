using AbayPOS.Domain.Entities;
using AbayPOS.UI.Controls;
using AbayPOS.UI.Theming;

namespace AbayPOS.UI.Dialogs;

/// <summary>Add/edit product with inline validation.</summary>
public sealed class ProductEditDialog : Form
{
    private readonly GlassTextBox _name = new() { Placeholder = "Name" };
    private readonly GlassTextBox _barcode = new() { Placeholder = "Barcode" };
    private readonly GlassTextBox _category = new() { Placeholder = "Category" };
    private readonly GlassTextBox _emoji = new() { Placeholder = "Emoji" };
    private readonly NumericUpDown _price = MoneyBox();
    private readonly NumericUpDown _cost = MoneyBox();
    private readonly NumericUpDown _stock = IntBox();
    private readonly NumericUpDown _reorder = IntBox();
    private readonly ComboBox _supplier = new();
    private readonly Label _error = new() { ForeColor = Theme.Danger, Dock = DockStyle.Fill, Font = Theme.FontHint() };

    public ProductEditDialog(Product product, IReadOnlyList<Supplier> suppliers)
    {
        Product = product;
        Text = product.Id == 0 ? "Add product" : "Edit product";
        FormBorderStyle = FormBorderStyle.None;
        StartPosition = FormStartPosition.CenterParent;
        ClientSize = new Size(460, 560);
        BackColor = Theme.Base;
        ShowInTaskbar = false;
        var surface = new GlassPanel { Dock = DockStyle.Fill, Padding = new Padding(24) };
        var grid = new TableLayoutPanel { Dock = DockStyle.Fill, RowCount = 12, BackColor = Theme.Base };
        for (var i = 0; i < 11; i++)
            grid.RowStyles.Add(new RowStyle(SizeType.Absolute, i is 0 or 10 ? 40 : 44));
        grid.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        grid.Controls.Add(new Label { Text = Text, Font = Theme.FontHeading(), ForeColor = Theme.Text, Dock = DockStyle.Fill }, 0, 0);
        _name.Text = product.Name;
        _barcode.Text = product.Barcode;
        _category.Text = product.Category;
        _emoji.Text = product.Emoji;
        _price.Value = Math.Max(0.01m, product.Price == 0 ? 1 : product.Price);
        _cost.Value = Math.Max(0, product.CostPrice);
        _stock.Value = product.StockQty;
        _reorder.Value = product.ReorderLevel;
        _supplier.Dock = DockStyle.Fill;
        _supplier.DropDownStyle = ComboBoxStyle.DropDownList;
        _supplier.FlatStyle = FlatStyle.Flat;
        _supplier.BackColor = Theme.InputFill;
        _supplier.ForeColor = Theme.Text;
        _supplier.Items.Add(new Supplier { Id = 0, Name = "(none)" });
        foreach (var supplier in suppliers)
            _supplier.Items.Add(supplier);
        _supplier.DisplayMember = nameof(Supplier.Name);
        _supplier.SelectedIndex = 0;
        if (product.SupplierId is int sid)
        {
            for (var i = 1; i < _supplier.Items.Count; i++)
            {
                if (_supplier.Items[i] is Supplier s && s.Id == sid)
                    _supplier.SelectedIndex = i;
            }
        }

        Add(grid, 1, _name);
        Add(grid, 2, _barcode);
        Add(grid, 3, _category);
        Add(grid, 4, _emoji);
        Add(grid, 5, _price);
        Add(grid, 6, _cost);
        Add(grid, 7, _stock);
        Add(grid, 8, _reorder);
        Add(grid, 9, _supplier);
        grid.Controls.Add(_error, 0, 10);
        var buttons = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.RightToLeft, BackColor = Theme.Base };
        var save = new RoundButton { Text = "Save", Width = 120, Height = 44, IsPrimary = true };
        save.Click += (_, _) => TrySave();
        var cancel = new RoundButton { Text = "Cancel", Width = 110, Height = 44, FillColor = Theme.CancelGlass, BorderColor = Theme.GlassBorder };
        cancel.Click += (_, _) => { DialogResult = DialogResult.Cancel; Close(); };
        buttons.Controls.Add(save);
        buttons.Controls.Add(cancel);
        grid.Controls.Add(buttons, 0, 11);
        surface.Controls.Add(grid);
        Controls.Add(surface);
        Surface.Adopt(this, Theme.Base);
        AcceptButton = save;
        Load += (_, _) => GlassHelper.ApplyToWindow(this);
    }

    public Product Product { get; }

    protected override void OnPaintBackground(PaintEventArgs e) => GlassHelper.PaintFormBackground(this, e.Graphics);

    private void TrySave()
    {
        Product.Name = _name.Text ?? string.Empty;
        Product.Barcode = _barcode.Text ?? string.Empty;
        Product.Category = _category.Text ?? string.Empty;
        Product.Emoji = string.IsNullOrWhiteSpace(_emoji.Text) ? null : _emoji.Text;
        Product.Price = _price.Value;
        Product.CostPrice = _cost.Value;
        Product.StockQty = (int)_stock.Value;
        Product.ReorderLevel = (int)_reorder.Value;
        Product.SupplierId = _supplier.SelectedItem is Supplier s && s.Id != 0 ? s.Id : null;
        if (string.IsNullOrWhiteSpace(Product.Name) || string.IsNullOrWhiteSpace(Product.Barcode) || Product.Price <= 0)
        {
            _error.Text = "Name, barcode and a price above zero are required.";
            return;
        }

        DialogResult = DialogResult.OK;
        Close();
    }

    private static void Add(TableLayoutPanel grid, int row, Control control)
    {
        control.Dock = DockStyle.Fill;
        grid.Controls.Add(control, 0, row);
    }

    private static NumericUpDown MoneyBox() => new()
    {
        DecimalPlaces = 2,
        Maximum = 1_000_000,
        Minimum = 0,
        Dock = DockStyle.Fill,
        BackColor = Theme.InputFill,
        ForeColor = Theme.Text
    };

    private static NumericUpDown IntBox() => new()
    {
        DecimalPlaces = 0,
        Maximum = 1_000_000,
        Dock = DockStyle.Fill,
        BackColor = Theme.InputFill,
        ForeColor = Theme.Text
    };
}
