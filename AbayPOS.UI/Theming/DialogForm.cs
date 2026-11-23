namespace AbayPOS.UI.Theming;

/// <summary>Fixed native dialog. Enter and Esc are the form AcceptButton and CancelButton.</summary>
public class DialogForm : Form
{
    protected DialogForm()
    {
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        ShowInTaskbar = false;
        StartPosition = FormStartPosition.CenterParent;
        AutoScaleMode = AutoScaleMode.Dpi;
        BackColor = Theme.Base;
        ForeColor = Theme.Text;
        Font = Theme.FontBody();
        DoubleBuffered = true;
        KeyPreview = true;
        Icon = AppIcon.Load();
    }

    protected override void OnHandleCreated(EventArgs e)
    {
        base.OnHandleCreated(e);
        WindowStyler.Apply(this);
    }
}
