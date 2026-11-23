using AbayPOS.Domain.Entities;
using AbayPOS.UI.Controls;
using AbayPOS.UI.Theming;

namespace AbayPOS.UI.Dialogs;

public sealed class SupplierEditDialog : Form
{
    private readonly GlassTextBox _name = new() { Placeholder = "Name" };
    private readonly GlassTextBox _phone = new() { Placeholder = "Phone" };
    private readonly GlassTextBox _email = new() { Placeholder = "Email" };
    private readonly GlassTextBox _address = new() { Placeholder = "Address" };
    private readonly CheckBox _active = new() { Text = "Active", ForeColor = Theme.Text, Checked = true, Dock = DockStyle.Fill };
    private readonly Label _error = new() { ForeColor = Theme.Danger, Dock = DockStyle.Fill, Font = Theme.FontHint() };

    public SupplierEditDialog(Supplier supplier)
    {
        Supplier = supplier;
        Text = supplier.Id == 0 ? "Add supplier" : "Edit supplier";
        FormBorderStyle = FormBorderStyle.None;
        StartPosition = FormStartPosition.CenterParent;
        ClientSize = new Size(420, 380);
        BackColor = Theme.Base;
        ShowInTaskbar = false;
        _name.Text = supplier.Name;
        _phone.Text = supplier.Phone;
        _email.Text = supplier.Email;
        _address.Text = supplier.Address;
        _active.Checked = supplier.IsActive;
        var surface = new GlassPanel { Dock = DockStyle.Fill, Padding = new Padding(24) };
        var grid = new TableLayoutPanel { Dock = DockStyle.Fill, RowCount = 8, BackColor = Theme.Base };
        grid.Controls.Add(new Label { Text = Text, Font = Theme.FontHeading(), ForeColor = Theme.Text, Dock = DockStyle.Fill }, 0, 0);
        foreach (var box in new Control[] { _name, _phone, _email, _address })
            box.Dock = DockStyle.Fill;
        grid.Controls.Add(_name, 0, 1);
        grid.Controls.Add(_phone, 0, 2);
        grid.Controls.Add(_email, 0, 3);
        grid.Controls.Add(_address, 0, 4);
        grid.Controls.Add(_active, 0, 5);
        grid.Controls.Add(_error, 0, 6);
        var buttons = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.RightToLeft, BackColor = Theme.Base };
        var save = new RoundButton { Text = "Save", Width = 120, Height = 44, IsPrimary = true };
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
        var cancel = new RoundButton { Text = "Cancel", Width = 110, Height = 44, FillColor = Theme.CancelGlass, BorderColor = Theme.GlassBorder };
        cancel.Click += (_, _) => Close();
        buttons.Controls.Add(save);
        buttons.Controls.Add(cancel);
        grid.Controls.Add(buttons, 0, 7);
        surface.Controls.Add(grid);
        Controls.Add(surface);
        Surface.Adopt(this, Theme.Base);
        Load += (_, _) => GlassHelper.ApplyToWindow(this);
    }

    public Supplier Supplier { get; }

    protected override void OnPaintBackground(PaintEventArgs e) => GlassHelper.PaintFormBackground(this, e.Graphics);
}
