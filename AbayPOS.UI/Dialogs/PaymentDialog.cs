using AbayPOS.Domain;
using AbayPOS.UI.Controls;
using AbayPOS.UI.Theming;

namespace AbayPOS.UI.Dialogs;

/// <summary>Frosted payment modal used by the cashier until Phase 3 wires SalesService.</summary>
public sealed class PaymentDialog : Form
{
    private readonly decimal _total;
    private readonly NumericUpDown _amount;
    private readonly Label _change;
    private readonly RoundButton _confirm;

    public PaymentDialog(decimal total)
    {
        _total = total;
        AmountPaid = total;
        Text = "Payment";
        FormBorderStyle = FormBorderStyle.None;
        StartPosition = FormStartPosition.CenterParent;
        ShowInTaskbar = false;
        AutoScaleMode = AutoScaleMode.Dpi;
        ClientSize = new Size(440, 370);
        MinimumSize = Size;
        BackColor = Theme.Base;
        DoubleBuffered = true;
        SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint | ControlStyles.OptimizedDoubleBuffer, true);

        var surface = new GlassPanel { Dock = DockStyle.Fill, Padding = new Padding(26), FillColor = Theme.GlassFill };
        var grid = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 7, BackColor = Color.Transparent };
        grid.RowStyles.Add(new RowStyle(SizeType.Absolute, 38));
        grid.RowStyles.Add(new RowStyle(SizeType.Absolute, 62));
        grid.RowStyles.Add(new RowStyle(SizeType.Absolute, 26));
        grid.RowStyles.Add(new RowStyle(SizeType.Absolute, 44));
        grid.RowStyles.Add(new RowStyle(SizeType.Absolute, 28));
        grid.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        grid.RowStyles.Add(new RowStyle(SizeType.Absolute, 54));
        grid.Controls.Add(new Label { Text = "Complete payment", Dock = DockStyle.Fill, ForeColor = Theme.Text, Font = Theme.FontHeading(), TextAlign = ContentAlignment.MiddleLeft }, 0, 0);
        grid.Controls.Add(new Label { Text = Money.Format(_total), Dock = DockStyle.Fill, ForeColor = Theme.Accent, Font = Theme.FontTotal(), TextAlign = ContentAlignment.MiddleLeft }, 0, 1);
        grid.Controls.Add(new Label { Text = "Amount received", Dock = DockStyle.Fill, ForeColor = Theme.TextSecondary, TextAlign = ContentAlignment.BottomLeft }, 0, 2);
        _amount = new NumericUpDown
        {
            Dock = DockStyle.Fill,
            DecimalPlaces = 2,
            Minimum = 0,
            Maximum = 100000000,
            Value = Math.Min(total, 100000000),
            BackColor = Theme.InputFill,
            ForeColor = Theme.Text,
            BorderStyle = BorderStyle.None,
            Font = Theme.Font(14)
        };
        _amount.ValueChanged += (_, _) => UpdateChange();
        grid.Controls.Add(_amount, 0, 3);
        grid.Controls.Add(new Label { Text = "Change", Dock = DockStyle.Fill, ForeColor = Theme.TextSecondary, TextAlign = ContentAlignment.BottomLeft }, 0, 4);
        _change = new Label { Dock = DockStyle.Fill, ForeColor = Theme.Text, Font = Theme.Font(17, FontStyle.Bold), TextAlign = ContentAlignment.MiddleLeft };
        grid.Controls.Add(_change, 0, 5);
        var buttons = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.RightToLeft, WrapContents = false, BackColor = Color.Transparent };
        _confirm = new RoundButton { Text = "Confirm", Width = 145, Height = 44, IsPrimary = true, FillColor = Theme.Accent };
        _confirm.Click += (_, _) =>
        {
            AmountPaid = _amount.Value;
            DialogResult = DialogResult.OK;
            Close();
        };
        var cancel = new RoundButton { Text = "Cancel", Width = 110, Height = 44, FillColor = Theme.CancelGlass, BorderColor = Theme.GlassBorder };
        cancel.Click += (_, _) =>
        {
            DialogResult = DialogResult.Cancel;
            Close();
        };
        buttons.Controls.Add(_confirm);
        buttons.Controls.Add(cancel);
        grid.Controls.Add(buttons, 0, 6);
        surface.Controls.Add(grid);
        Controls.Add(surface);
        AcceptButton = _confirm;
        CancelButton = cancel;
        Load += (_, _) => GlassHelper.ApplyToWindow(this);
        UpdateChange();
    }

    public decimal AmountPaid { get; private set; }

    protected override void OnPaintBackground(PaintEventArgs e) =>
        GlassHelper.PaintFormBackground(this, e.Graphics);

    private void UpdateChange()
    {
        AmountPaid = _amount.Value;
        _change.Text = Money.Format(Math.Max(0, _amount.Value - _total));
        _confirm.Enabled = _amount.Value >= _total;
    }
}
