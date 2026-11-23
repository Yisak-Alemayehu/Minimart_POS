using AbayPOS.Domain.Entities;
using AbayPOS.UI.Controls;
using AbayPOS.UI.Theming;

namespace AbayPOS.UI.Dialogs;

public sealed class SupplierEditDialog : DialogForm
{
    private readonly GlassTextBox _name = new() { Placeholder = "Name" };
    private readonly GlassTextBox _phone = new() { Placeholder = "Phone" };
    private readonly GlassTextBox _email = new() { Placeholder = "Email" };
    private readonly GlassTextBox _address = new() { Placeholder = "Address" };
    private readonly CheckBox _active = new() { Text = "Active", ForeColor = Theme.Text, Checked = true, AutoSize = true };
    private readonly Label _error = new() { ForeColor = Theme.Danger, AutoSize = true, Font = Theme.FontHint(), MaximumSize = new Size(360, 0) };

    public SupplierEditDialog(Supplier supplier)
    {
        Supplier = supplier;
        Text = supplier.Id == 0 ? "AbayPOS – Add supplier" : "AbayPOS – Edit supplier";
        AutoSize = true;
        AutoSizeMode = AutoSizeMode.GrowAndShrink;
        MinimumSize = new Size(Theme.Scale(420), Theme.Scale(360));
        _name.Text = supplier.Name;
        _phone.Text = supplier.Phone;
        _email.Text = supplier.Email;
        _address.Text = supplier.Address;
        _active.Checked = supplier.IsActive;
        var surface = new GlassPanel { Dock = DockStyle.Top, AutoSize = true, AutoSizeMode = AutoSizeMode.GrowAndShrink, Padding = new Padding(Theme.Scale(24)) };
        var grid = new TableLayoutPanel { Dock = DockStyle.Top, AutoSize = true, ColumnCount = 1, BackColor = Theme.Base };
        var row = 0;
        void Field(string caption, Control input)
        {
            grid.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            grid.Controls.Add(new Label { Text = caption, AutoSize = true, ForeColor = Theme.TextSecondary, Font = Theme.FontHint(), Margin = new Padding(0, 8, 0, 2) }, 0, row++);
            input.Dock = DockStyle.Top;
            input.MinimumSize = new Size(Theme.Scale(280), Theme.Scale(32));
            input.Margin = new Padding(0, 0, 0, 4);
            grid.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            grid.Controls.Add(input, 0, row++);
        }

        Field("Name", _name);
        Field("Phone", _phone);
        Field("Email", _email);
        Field("Address", _address);
        grid.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        grid.Controls.Add(_active, 0, row++);
        grid.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        grid.Controls.Add(_error, 0, row++);
        var buttons = new FlowLayoutPanel { AutoSize = true, FlowDirection = FlowDirection.RightToLeft, BackColor = Theme.Base, WrapContents = false };
        var save = new RoundButton { Text = "Save", Width = Theme.Scale(120), Height = Theme.Scale(44), IsPrimary = true };
        save.Click += (_, _) =>
        {
            if (string.IsNullOrWhiteSpace(_name.Text))
            {
                _error.Text = "Enter a supplier name.";
                return;
            }

            Supplier.Name = _name.Text!.Trim();
            Supplier.Phone = _phone.Text;
            Supplier.Email = _email.Text;
            Supplier.Address = _address.Text;
            Supplier.IsActive = _active.Checked;
            DialogResult = DialogResult.OK;
            Close();
        };
        var cancel = new RoundButton { Text = "Cancel", Width = Theme.Scale(110), Height = Theme.Scale(44), FillColor = Theme.CancelGlass, BorderColor = Theme.GlassBorder };
        cancel.Click += (_, _) => Close();
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

    public Supplier Supplier { get; }
}
