using AbayPOS.UI.Controls;
using AbayPOS.UI.Theming;

namespace AbayPOS.UI.Dialogs;

/// <summary>Frosted confirm used only for destructive actions.</summary>
public sealed partial class ConfirmDialog : DialogForm
{
    public ConfirmDialog()
    {
        InitializeComponent();
        btnConfirm.Click += (_, _) => { DialogResult = DialogResult.Yes; Close(); };
        btnKeep.Click += (_, _) => { DialogResult = DialogResult.No; Close(); };
        AcceptButton = btnConfirm;
        CancelButton = btnKeep;
    }

    public static bool Ask(IWin32Window owner, string title, string message, string confirmText = "Yes", bool danger = true)
    {
        using var overlay = CreateOverlay(owner);
        overlay?.Show();
        using var dialog = new ConfirmDialog();
        dialog.Text = title;
        dialog.lblTitle.Text = title;
        dialog.lblMessage.Text = message;
        dialog.btnConfirm.Text = confirmText;
        dialog.btnConfirm.FillColor = danger ? Theme.Danger : Theme.Accent;
        dialog.btnConfirm.IsPrimary = true;
        var result = dialog.ShowDialog(owner);
        overlay?.Close();
        return result == DialogResult.Yes;
    }

    internal static Form? CreateOverlay(IWin32Window owner)
    {
        if (owner is not Form form)
            return null;
        return new Form
        {
            FormBorderStyle = FormBorderStyle.None,
            ShowInTaskbar = false,
            StartPosition = FormStartPosition.Manual,
            Bounds = form.RectangleToScreen(form.ClientRectangle),
            BackColor = Color.Black,
            Opacity = 0.45,
            Owner = form
        };
    }

    protected override void OnLoad(EventArgs e)
    {
        base.OnLoad(e);
        if (DesignTime.IsActive(this))
            return;
        Theme.ApplyRuntime(this);
    }
}