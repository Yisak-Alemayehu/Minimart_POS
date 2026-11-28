namespace AbayPOS.UI.Theming;

/// <summary>Native Windows chrome. Caption, snap, and resize come from the system.</summary>
public class ThemedForm : AppForm
{
    public ThemedForm()
    {
        StartPosition = FormStartPosition.CenterScreen;
        KeyPreview = true;
        if (DesignTime.IsActive)
        {
            BackColor = Color.FromArgb(0x0D, 0x0B, 0x14);
            ForeColor = Color.FromArgb(0xF5, 0xF5, 0xF7);
            Font = new Font("Segoe UI", 13f);
            return;
        }

        BackColor = Theme.Base;
        Font = Theme.FontBody();
        ForeColor = Theme.Text;
        Icon = AppIcon.Load();
    }

    protected override void OnHandleCreated(EventArgs e)
    {
        base.OnHandleCreated(e);
        if (!DesignTime.IsActive)
            WindowStyler.Apply(this);
    }

    protected override void OnPaintBackground(PaintEventArgs e) =>
        e.Graphics.Clear(DesignTime.IsActive(this) ? BackColor : Theme.Base);
}