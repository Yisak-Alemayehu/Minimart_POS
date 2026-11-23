using System.ComponentModel;
using AbayPOS.UI.Theming;

namespace AbayPOS.UI.Controls;

/// <summary>Inner frosted panel. Child transparency is simulated by painting the parent blur cache.</summary>
public class GlassPanel : Panel
{
    public GlassPanel()
    {
        DoubleBuffered = true;
        BackColor = Color.Transparent;
        SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw | ControlStyles.SupportsTransparentBackColor, true);
    }

    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public int Radius { get; set; } = Theme.PanelRadius;

    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public Color FillColor { get; set; } = Theme.GlassFill;

    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public bool DrawShadow { get; set; } = true;

    protected override void OnPaintBackground(PaintEventArgs e)
    {
        // Background is painted in OnPaint so the blur slice and tint stay in one pass.
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        GlassHelper.PaintGlassPanel(e.Graphics, this, FillColor, Radius, DrawShadow);
        base.OnPaint(e);
    }
}
