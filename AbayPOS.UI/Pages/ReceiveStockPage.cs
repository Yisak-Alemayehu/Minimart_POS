using AbayPOS.Business.Services;
using AbayPOS.Domain;
using AbayPOS.Domain.Entities;
using AbayPOS.Domain.Models;
using AbayPOS.UI.Controls;
using AbayPOS.UI.Infrastructure;
using AbayPOS.UI.Theming;

namespace AbayPOS.UI.Pages;

internal sealed class ReceiveStockPage : UserControl
{
    private readonly Session _session;
    private readonly ComboBox _supplier = new();
    private readonly ComboBox _product = new();
    private readonly NumericUpDown _qty = new() { Minimum = 1, Maximum = 100000, Value = 1, Width = 80, BackColor = Theme.InputFill, ForeColor = Theme.Text };
    private readonly NumericUpDown _cost = new() { DecimalPlaces = 2, Maximum = 100000, Width = 100, BackColor = Theme.InputFill, ForeColor = Theme.Text };
    private readonly DataGridView _grid = new();
    private readonly GlassTextBox _note = new() { Placeholder = "Note (optional)" };
    private readonly List<ReceiptLine> _lines = [];
    private IReadOnlyList<Product> _products = [];

    public ReceiveStockPage(Session session)
    {
        _session = session;
        Dock = DockStyle.Fill;
        BackColor = Theme.Base;
        var panel = new GlassPanel { Dock = DockStyle.Fill, Padding = new Padding(16) };
        var layout = new TableLayoutPanel { Dock = DockStyle.Fill, RowCount = 5, BackColor = Theme.Base };
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 40));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 48));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 48));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 52));
        layout.Controls.Add(new Label { Text = "Receive stock", Font = Theme.FontHeading(), ForeColor = Theme.Text, Dock = DockStyle.Fill }, 0, 0);
        _supplier.Dock = DockStyle.Fill;
        _supplier.DropDownStyle = ComboBoxStyle.DropDownList;
        _supplier.FlatStyle = FlatStyle.Flat;
        _supplier.BackColor = Theme.InputFill;
        _supplier.ForeColor = Theme.Text;
        layout.Controls.Add(_supplier, 0, 1);
        var addRow = new FlowLayoutPanel { Dock = DockStyle.Fill, BackColor = Theme.Base };
        _product.Width = 260;
        _product.DropDownStyle = ComboBoxStyle.DropDownList;
        _product.FlatStyle = FlatStyle.Flat;
        _product.BackColor = Theme.InputFill;
        _product.ForeColor = Theme.Text;
        var add = new RoundButton { Text = "Add line", Width = 110, Height = 40, FillColor = Theme.NavGlass };
        add.Click += (_, _) => AddLine();
        addRow.Controls.Add(_product);
        addRow.Controls.Add(_qty);
        addRow.Controls.Add(_cost);
        addRow.Controls.Add(add);
        layout.Controls.Add(addRow, 0, 2);
        GridTheme.Apply(_grid);
        _grid.Dock = DockStyle.Fill;
        _grid.Columns.Add("Product", "Product");
        _grid.Columns.Add("Qty", "Qty");
        _grid.Columns.Add("Cost", "Unit cost");
        GridTheme.Balance(_grid, "Product", "Qty", "Cost");
        layout.Controls.Add(_grid, 0, 3);
        var bottom = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, BackColor = Theme.Base };
        bottom.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 70));
        bottom.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 30));
        _note.Dock = DockStyle.Fill;
        var save = new RoundButton { Text = "Save receipt", Dock = DockStyle.Fill, IsPrimary = true };
        save.Click += (_, _) => Save();
        bottom.Controls.Add(_note, 0, 0);
        bottom.Controls.Add(save, 1, 0);
        layout.Controls.Add(bottom, 0, 4);
        panel.Controls.Add(layout);
        Controls.Add(panel);
        Load += (_, _) => ReloadLookups();
    }

    public void ReloadLookups() => UiAsync.Run(async () =>
    {
        var suppliers = await ServiceLocator.Suppliers.GetAllAsync(true);
        _products = await ServiceLocator.Products.GetActiveAsync();
        _supplier.Items.Clear();
        foreach (var s in suppliers)
            _supplier.Items.Add(s);
        _supplier.DisplayMember = nameof(Supplier.Name);
        if (_supplier.Items.Count > 0)
            _supplier.SelectedIndex = 0;
        _product.Items.Clear();
        foreach (var p in _products)
            _product.Items.Add(p);
        _product.DisplayMember = nameof(Product.Name);
        if (_product.Items.Count > 0)
            _product.SelectedIndex = 0;
    });

    private void AddLine()
    {
        if (_product.SelectedItem is not Product product)
            return;
        _lines.Add(new ReceiptLine { ProductId = product.Id, Qty = (int)_qty.Value, UnitCost = _cost.Value });
        _grid.Rows.Add(product.Name, _qty.Value, Money.Format(_cost.Value));
    }

    private void Save()
    {
        if (_supplier.SelectedItem is not Supplier supplier)
        {
            ToastHost.Warn("Choose a supplier.");
            return;
        }

        UiAsync.Run(async () =>
        {
            await ServiceLocator.Inventory.ReceiveStockAsync(supplier.Id, _lines, _note.Text, _session);
            _lines.Clear();
            _grid.Rows.Clear();
            ToastHost.Success("Stock received.");
        });
    }
}
