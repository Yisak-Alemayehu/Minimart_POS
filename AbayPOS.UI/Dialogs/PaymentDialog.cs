using AbayPOS.Business.Services;
using AbayPOS.Domain;
using AbayPOS.Domain.Enums;
using AbayPOS.UI.Controls;
using AbayPOS.UI.Theming;

namespace AbayPOS.UI.Dialogs;

/// <summary>UC-04 frosted payment: Cash / Mobile Money, quick amounts, live change.</summary>
public sealed partial class PaymentDialog : DialogForm
{
    private decimal _total;
    private PaymentMethod _method = PaymentMethod.Cash;

    public PaymentDialog()
    {
        InitializeComponent();
        btnPay.Click += (_, _) => Confirm();
        btnBack.Click += (_, _) => { DialogResult = DialogResult.Cancel; Close(); };
        btnCashTab.Click += (_, _) => SetMethod(PaymentMethod.Cash);
        btnMobileTab.Click += (_, _) => SetMethod(PaymentMethod.MobileMoney);
        numAmount.ValueChanged += (_, _) => UpdateChange();
        AcceptButton = btnPay;
        CancelButton = btnBack;
    }

    public PaymentDialog(decimal total) : this()
    {
        _total = total;
        AmountPaid = total;
        txtDue.SetValue(Money.Format(_total));
        numAmount.Value = Math.Min(total, numAmount.Maximum);
        foreach (var value in new decimal[] { total, 100, 200, 500, 1000 })
        {
            var captured = decimal.Round(value, 2, MidpointRounding.AwayFromZero);
            var label = captured == total ? "Exact" : captured.ToString("0");
            var button = new RoundButton
            {
                Text = label,
                Width = 72,
                Height = 40,
                FillColor = Theme.NavGlass,
                Font = Theme.FontHint()
            };
            button.Click += (_, _) => numAmount.Value = Math.Min(captured, numAmount.Maximum);
            flpQuick.Controls.Add(button);
        }

        Shown += (_, _) =>
        {
            numAmount.Focus();
            numAmount.Select(0, numAmount.Text.Length);
        };
        UpdateChange();
    }

    public PaymentRequest Request => new() { Method = _method, AmountPaid = AmountPaid };
    public decimal AmountPaid { get; private set; }

    private void SetMethod(PaymentMethod method)
    {
        _method = method;
        btnCashTab.FillColor = method == PaymentMethod.Cash ? Theme.Accent : Theme.NavGlass;
        btnMobileTab.FillColor = method == PaymentMethod.MobileMoney ? Theme.Accent : Theme.NavGlass;
        btnCashTab.Invalidate();
        btnMobileTab.Invalidate();
    }

    private void UpdateChange()
    {
        AmountPaid = numAmount.Value;
        var shortfall = numAmount.Value < _total;
        lblChange.ForeColor = shortfall ? Theme.Danger : Theme.Success;
        lblChange.Text = shortfall ? "Insufficient" : Money.Format(numAmount.Value - _total);
        btnPay.Enabled = !shortfall;
    }

    private void Confirm()
    {
        if (numAmount.Value < _total)
            return;
        AmountPaid = numAmount.Value;
        DialogResult = DialogResult.OK;
        Close();
    }

    protected override void OnLoad(EventArgs e)
    {
        base.OnLoad(e);
        if (DesignTime.IsActive(this))
            return;
        Theme.ApplyRuntime(this);
    }
}