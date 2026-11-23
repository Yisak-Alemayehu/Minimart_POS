namespace AbayPOS.UI.Theming;

/// <summary>Native Windows chrome. Caption, snap, and resize come from the system.</summary>
public class ThemedForm : AppForm
{
    protected ThemedForm(bool sizable = true)
    {
        StartPosition = FormStartPosition.CenterScreen;
        FormBorderStyle = sizable ? FormBorderStyle.Sizable : FormBorderStyle.FixedSingle;
        MaximizeBox = sizable;
        BackColor = Theme.Base;
        Font = Theme.FontBody();
        ForeColor = Theme.Text;
        KeyPreview = true;
        Icon = AppIcon.Load();
    }

    protected override void OnHandleCreated(EventArgs e)
    {
        base.OnHandleCreated(e);
        WindowStyler.Apply(this);
    }

    protected override void OnPaintBackground(PaintEventArgs e) =>
        e.Graphics.Clear(Theme.Base);
}
