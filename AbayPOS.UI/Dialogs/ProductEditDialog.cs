using AbayPOS.Domain.Entities;
using AbayPOS.UI.Controls;
using AbayPOS.UI.Theming;

namespace AbayPOS.UI.Dialogs;

/// <summary>Add/edit product with inline validation.</summary>
public sealed class ProductEditDialog : DialogForm
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
        Text = product.Id == 0 ? "AbayPOS – Add product" : "AbayPOS – Edit product";
        AutoScroll = true;
        MinimumSize = new Size(Theme.Scale(420), Theme.Scale(480));
        ClientSize = new Size(Theme.Scale(460), Theme.Scale(640));
        var surface = new GlassPanel { Dock = DockStyle.Fill, Padding = new Padding(Theme.Scale(24)) };
        var grid = new TableLayoutPanel { Dock = DockStyle.Top, AutoSize = true, ColumnCount = 1, BackColor = Theme.Base };
        var row = 0;
        void Field(string caption, Control input)
        {
            grid.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            grid.Controls.Add(new Label { Text = caption, AutoSize = true, ForeColor = Theme.TextSecondary, Font = Theme.FontHint(), Margin = new Padding(0, 8, 0, 2) }, 0, row++);
            input.Dock = DockStyle.Fill;
            input.Margin = new Padding(0, 0, 0, 4);
            input.MinimumSize = new Size(Theme.Scale(180), Theme.Scale(32));
            grid.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            grid.Controls.Add(input, 0, row++);
        }
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

        Field("Name", _name);
        Field("Barcode", _barcode);
        Field("Category", _category);
        Field("Emoji", _emoji);
        Field("Price (ETB)", _price);
        Field("Cost (ETB)", _cost);
        Field("Stock", _stock);
        Field("Reorder level", _reorder);
        Field("Supplier", _supplier);
        grid.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        grid.Controls.Add(_error, 0, row++);
        var buttons = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.RightToLeft, BackColor = Theme.Base };
        var save = new RoundButton { Text = "Save", Width = 120, Height = 44, IsPrimary = true };
        save.Click += (_, _) => TrySave();
        var cancel = new RoundButton { Text = "Cancel", Width = 110, Height = 44, FillColor = Theme.CancelGlass, BorderColor = Theme.GlassBorder };
        cancel.Click += (_, _) => { DialogResult = DialogResult.Cancel; Close(); };
        buttons.Controls.Add(save);
        buttons.Controls.Add(cancel);
        grid.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        grid.Controls.Add(buttons, 0, row);
        surface.Controls.Add(grid);
        Controls.Add(surface);
        Surface.Adopt(this, Theme.Base);
        AcceptButton = save;
        CancelButton = cancel;
    }

    public Product Product { get; }

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

    private static NumericUpDown MoneyBox() => new()
    {
        DecimalPlaces = 2,
        ThousandsSeparator = true,
        Maximum = 10_000_000,
        Width = Theme.Scale(220),
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
