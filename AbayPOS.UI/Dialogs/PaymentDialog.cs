using AbayPOS.Business.Services;
using AbayPOS.Domain;
using AbayPOS.Domain.Enums;
using AbayPOS.UI.Controls;
using AbayPOS.UI.Theming;

namespace AbayPOS.UI.Dialogs;

/// <summary>UC-04 frosted payment: Cash / Mobile Money, quick amounts, live change.</summary>
public sealed class PaymentDialog : DialogForm
{
    private readonly decimal _total;
    private readonly NumericUpDown _amount;
    private readonly Label _change;
    private readonly RoundButton _confirm;
    private readonly RoundButton _cashTab;
    private readonly RoundButton _mobileTab;
    private PaymentMethod _method = PaymentMethod.Cash;

    public PaymentDialog(decimal total)
    {
        _total = total;
        AmountPaid = total;
        Text = "AbayPOS – Payment";
        ClientSize = new Size(480, 460);
        BackColor = Theme.Base;
        KeyPreview = true;
        DoubleBuffered = true;

        var surface = new GlassPanel { Dock = DockStyle.Fill, Padding = new Padding(26), FillColor = Theme.GlassFill };
        var grid = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 8, BackColor = Theme.Base };
        grid.RowStyles.Add(new RowStyle(SizeType.Absolute, 36));
        grid.RowStyles.Add(new RowStyle(SizeType.Absolute, 52));
        grid.RowStyles.Add(new RowStyle(SizeType.Absolute, 48));
        grid.RowStyles.Add(new RowStyle(SizeType.Absolute, 52));
        grid.RowStyles.Add(new RowStyle(SizeType.Absolute, 44));
        grid.RowStyles.Add(new RowStyle(SizeType.Absolute, 24));
        grid.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        grid.RowStyles.Add(new RowStyle(SizeType.Absolute, 54));
        grid.Controls.Add(new Label { Text = "Take payment", Dock = DockStyle.Fill, ForeColor = Theme.Text, Font = Theme.FontHeading(), TextAlign = ContentAlignment.MiddleLeft }, 0, 0);
        grid.Controls.Add(new Label { Text = Money.Format(_total), Dock = DockStyle.Fill, ForeColor = Theme.Accent, Font = Theme.FontTotal(), TextAlign = ContentAlignment.MiddleLeft }, 0, 1);

        var tabs = new FlowLayoutPanel { Dock = DockStyle.Fill, BackColor = Theme.Base };
        _cashTab = Tab("Cash", true);
        _mobileTab = Tab("Mobile Money", false);
        _cashTab.Click += (_, _) => SetMethod(PaymentMethod.Cash);
        _mobileTab.Click += (_, _) => SetMethod(PaymentMethod.MobileMoney);
        tabs.Controls.Add(_cashTab);
        tabs.Controls.Add(_mobileTab);
        grid.Controls.Add(tabs, 0, 2);

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
            Font = Theme.Font(18, FontStyle.Bold)
        };
        _amount.ValueChanged += (_, _) => UpdateChange();
        grid.Controls.Add(_amount, 0, 3);

        var quick = new FlowLayoutPanel { Dock = DockStyle.Fill, BackColor = Theme.Base };
        foreach (var value in new decimal[] { total, 100, 200, 500, 1000 })
        {
            var captured = decimal.Round(value, 2, MidpointRounding.AwayFromZero);
            var label = captured == total ? "Exact" : captured.ToString("0");
            var button = new RoundButton { Text = label, Width = 72, Height = 40, FillColor = Theme.NavGlass, Font = Theme.FontHint() };
            button.Click += (_, _) => _amount.Value = Math.Min(captured, _amount.Maximum);
            quick.Controls.Add(button);
        }

        grid.Controls.Add(quick, 0, 4);
        grid.Controls.Add(new Label { Text = "Change", Dock = DockStyle.Fill, ForeColor = Theme.TextSecondary, TextAlign = ContentAlignment.BottomLeft }, 0, 5);
        _change = new Label { Dock = DockStyle.Fill, ForeColor = Theme.Success, Font = Theme.Font(18, FontStyle.Bold), TextAlign = ContentAlignment.MiddleLeft };
        grid.Controls.Add(_change, 0, 6);

        var buttons = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.RightToLeft, WrapContents = false, BackColor = Theme.Base };
        _confirm = new RoundButton { Text = "Pay", Badge = "Enter", Width = 150, Height = 44, IsPrimary = true, FillColor = Theme.Accent };
        _confirm.Click += (_, _) => Confirm();
        var cancel = new RoundButton { Text = "Back", Width = 110, Height = 44, FillColor = Theme.CancelGlass, BorderColor = Theme.GlassBorder };
        cancel.Click += (_, _) => { DialogResult = DialogResult.Cancel; Close(); };
        buttons.Controls.Add(_confirm);
        buttons.Controls.Add(cancel);
        grid.Controls.Add(buttons, 0, 7);
        surface.Controls.Add(grid);
        Controls.Add(surface);
        Surface.Adopt(this, Theme.Base);
        AcceptButton = _confirm;
        CancelButton = cancel;
        UpdateChange();
    }

    public PaymentRequest Request => new() { Method = _method, AmountPaid = AmountPaid };
    public decimal AmountPaid { get; private set; }

    private void SetMethod(PaymentMethod method)
    {
        _method = method;
        _cashTab.FillColor = method == PaymentMethod.Cash ? Theme.Accent : Theme.NavGlass;
        _mobileTab.FillColor = method == PaymentMethod.MobileMoney ? Theme.Accent : Theme.NavGlass;
        _cashTab.Invalidate();
        _mobileTab.Invalidate();
    }

    private void UpdateChange()
    {
        AmountPaid = _amount.Value;
        var change = Math.Max(0, _amount.Value - _total);
        _change.Text = Money.Format(change);
        _confirm.Enabled = _amount.Value >= _total;
    }

    private void Confirm()
    {
        if (_amount.Value < _total)
            return;
        AmountPaid = _amount.Value;
        DialogResult = DialogResult.OK;
        Close();
    }

    private static RoundButton Tab(string text, bool active) => new()
    {
        Text = text,
        Width = 140,
        Height = 40,
        FillColor = active ? Theme.Accent : Theme.NavGlass,
        Margin = new Padding(0, 4, 8, 4)
    };
}
