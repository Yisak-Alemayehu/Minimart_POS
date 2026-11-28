namespace AbayPOS.UI.Theming;

/// <summary>Fixed native dialog. Enter and Esc are the form AcceptButton and CancelButton.</summary>
public class DialogForm : Form
{
    public DialogForm()
    {
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        ShowInTaskbar = false;
        StartPosition = FormStartPosition.CenterParent;
        AutoScaleMode = AutoScaleMode.Dpi;
        DoubleBuffered = true;
        KeyPreview = true;
        AutoScroll = true;

        if (DesignTime.IsActive())
        {
            BackColor = Color.FromArgb(0x0D, 0x0B, 0x14);
            ForeColor = Color.FromArgb(0xF5, 0xF5, 0xF7);
            Font = new Font("Segoe UI", 13f);
            return;
        }

        BackColor = Theme.Base;
        ForeColor = Theme.Text;
        Font = Theme.FontBody();
        Icon = AppIcon.Load();
    }

    protected override void OnHandleCreated(EventArgs e)
    {
        base.OnHandleCreated(e);
        if (DesignTime.IsActive())
            return;
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