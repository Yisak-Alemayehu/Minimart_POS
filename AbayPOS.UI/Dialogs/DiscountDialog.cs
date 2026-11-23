using AbayPOS.UI.Controls;
using AbayPOS.UI.Theming;

namespace AbayPOS.UI.Dialogs;

/// <summary>F4 discount: amount in ETB or percent.</summary>
public sealed class DiscountDialog : Form
{
    private readonly NumericUpDown _value;
    private readonly CheckBox _percent;

    public DiscountDialog(decimal current, bool isPercent)
    {
        Text = "Discount";
        FormBorderStyle = FormBorderStyle.None;
        StartPosition = FormStartPosition.CenterParent;
        ShowInTaskbar = false;
        ClientSize = new Size(380, 240);
        BackColor = Theme.Base;
        var surface = new GlassPanel { Dock = DockStyle.Fill, Padding = new Padding(24) };
        var grid = new TableLayoutPanel { Dock = DockStyle.Fill, RowCount = 4, BackColor = Color.Transparent };
        grid.RowStyles.Add(new RowStyle(SizeType.Absolute, 40));
        grid.RowStyles.Add(new RowStyle(SizeType.Absolute, 48));
        grid.RowStyles.Add(new RowStyle(SizeType.Absolute, 36));
        grid.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        grid.Controls.Add(new Label { Text = "Discount", Font = Theme.FontHeading(), ForeColor = Theme.Text, Dock = DockStyle.Fill }, 0, 0);
        _value = new NumericUpDown
        {
            Dock = DockStyle.Fill,
            DecimalPlaces = 2,
            Maximum = 100000,
            Value = Math.Clamp(current, 0, 100000),
            BackColor = Theme.InputFill,
            ForeColor = Theme.Text,
            Font = Theme.Font(16)
        };
        grid.Controls.Add(_value, 0, 1);
        _percent = new CheckBox { Text = "Percent (%)  —  cashiers max 10%", ForeColor = Theme.Text, Dock = DockStyle.Fill, Checked = isPercent };
        grid.Controls.Add(_percent, 0, 2);
        var buttons = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.RightToLeft, BackColor = Color.Transparent };
        var ok = new RoundButton { Text = "Apply  F4", Width = 130, Height = 44, IsPrimary = true };
        ok.Click += (_, _) => { DialogResult = DialogResult.OK; Close(); };
        var cancel = new RoundButton { Text = "Cancel", Width = 110, Height = 44, FillColor = Theme.CancelGlass, BorderColor = Theme.GlassBorder };
        cancel.Click += (_, _) => { DialogResult = DialogResult.Cancel; Close(); };
        buttons.Controls.Add(ok);
        buttons.Controls.Add(cancel);
        grid.Controls.Add(buttons, 0, 3);
        surface.Controls.Add(grid);
        Controls.Add(surface);
        AcceptButton = ok;
        CancelButton = cancel;
        Load += (_, _) => GlassHelper.ApplyToWindow(this);
    }

    public decimal DiscountValue => _value.Value;
    public bool IsPercent => _percent.Checked;

    protected override void OnPaintBackground(PaintEventArgs e) =>
        GlassHelper.PaintFormBackground(this, e.Graphics);
}
