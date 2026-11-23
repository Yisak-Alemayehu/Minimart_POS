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
        AutoScroll = true;
        Icon = AppIcon.Load();
    }

    protected override void OnHandleCreated(EventArgs e)
    {
        base.OnHandleCreated(e);
        Theme.UseDpi(DeviceDpi);
        WindowStyler.Apply(this);
    }

    protected override void OnDpiChanged(DpiChangedEventArgs e)
    {
        base.OnDpiChanged(e);
        Theme.UseDpi(e.DeviceDpiNew);
        PerformLayout();
    }
}
