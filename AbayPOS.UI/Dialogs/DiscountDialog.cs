using AbayPOS.UI.Controls;
using AbayPOS.UI.Theming;

namespace AbayPOS.UI.Dialogs;

/// <summary>F4 discount: amount in ETB or percent.</summary>
public sealed partial class DiscountDialog : DialogForm
{
    public DiscountDialog()
    {
        InitializeComponent();
        btnApply.Click += (_, _) => { DialogResult = DialogResult.OK; Close(); };
        btnCancel.Click += (_, _) => { DialogResult = DialogResult.Cancel; Close(); };
        AcceptButton = btnApply;
        CancelButton = btnCancel;
    }

    public DiscountDialog(decimal current, bool isPercent) : this()
    {
        numValue.Value = Math.Clamp(current, 0, 100000);
        chkPercent.Checked = isPercent;
    }

    public decimal DiscountValue => numValue.Value;
    public bool IsPercent => chkPercent.Checked;

    protected override void OnLoad(EventArgs e)
    {
        base.OnLoad(e);
        if (DesignTime.IsActive(this))
            return;
        Theme.ApplyRuntime(this);
    }
}