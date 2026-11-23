using System.ComponentModel;
using System.Drawing.Drawing2D;
using AbayPOS.UI.Theming;

namespace AbayPOS.UI.Controls;

/// <summary>Opaque rounded card. Children paint themselves; nothing is drawn over them.</summary>
public class GlassPanel : Panel
{
    private Color _fill = Theme.Card;

    public GlassPanel()
    {
        DoubleBuffered = true;
        BackColor = Theme.Card;
        ForeColor = Theme.Text;
    }

    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public int Radius { get; set; } = Theme.PanelRadius;

    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public Color FillColor
    {
        get => _fill;
        set
        {
            _fill = Theme.Solid(value);
            BackColor = _fill;
            Surface.Adopt(this, _fill);
            ApplyRegion();
            Invalidate();
        }
    }

    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public bool DrawShadow { get; set; }

    protected override CreateParams CreateParams
    {
        get
        {
            var cp = base.CreateParams;
            cp.Style |= 0x02000000; // WS_CLIPCHILDREN
            return cp;
        }
    }

    protected override void OnControlAdded(ControlEventArgs e)
    {
        base.OnControlAdded(e);
        if (e.Control is not null)
            Surface.Adopt(e.Control, BackColor);
    }

    protected override void OnSizeChanged(EventArgs e)
    {
        base.OnSizeChanged(e);
        ApplyRegion();
    }

    protected override void OnPaintBackground(PaintEventArgs e)
    {
        e.Graphics.Clear(BackColor);
        if (Width <= 2 || Height <= 2)
            return;
        e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
        using var path = Gfx.Round(new RectangleF(0.5f, 0.5f, Width - 1f, Height - 1f), Radius);
        using var pen = new Pen(Theme.Line, 1f);
        e.Graphics.DrawPath(pen, path);
    }

    private void ApplyRegion()
    {
        if (Width <= 1 || Height <= 1)
            return;
        using var path = Gfx.Round(new RectangleF(0, 0, Width, Height), Radius);
        var next = new Region(path);
        var previous = Region;
        Region = next;
        previous?.Dispose();
    }
}
