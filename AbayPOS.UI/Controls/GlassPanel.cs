using System.ComponentModel;
using System.Drawing.Drawing2D;
using AbayPOS.UI.Theming;

namespace AbayPOS.UI.Controls;

/// <summary>Opaque rounded card. Children paint themselves; nothing is drawn over them.</summary>
[ToolboxItem(true)]
[Category("AbayPOS")]
[Description("Rounded surface panel.")]
public class GlassPanel : Panel
{
    private Color _fill = Theme.Card;

    public GlassPanel()
    {
        DoubleBuffered = true;
        BackColor = Theme.Card;
        ForeColor = Theme.Text;
    }

    [Category("AbayPOS")]
    [DefaultValue(12)]
    [Description("Corner radius in pixels (96 DPI).")]
    public int Radius { get; set; } = Theme.PanelRadius;

    [Category("AbayPOS")]
    [Description("Panel fill color.")]
    public Color FillColor
    {
        get => _fill;
        set
        {
            _fill = Theme.Solid(value);
            BackColor = _fill;
            if (!DesignTime.IsActive(this))
                Surface.Adopt(this, _fill);
            Invalidate();
        }
    }

    [Category("AbayPOS")]
    [DefaultValue(false)]
    [Description("Whether to draw a drop shadow (reserved).")]
    public bool DrawShadow { get; set; }

    public bool ShouldSerializeFillColor() => FillColor != Theme.Card;
    public void ResetFillColor() => FillColor = Theme.Card;

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
        RebuildPath();
    }

    protected override void OnPaintBackground(PaintEventArgs e)
    {
        var corner = Parent?.BackColor ?? Theme.Base;
        e.Graphics.Clear(Theme.Solid(corner));
        if (Width <= 2 || Height <= 2)
            return;

        e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
        e.Graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;
        var path = EnsurePath();
        e.Graphics.FillPath(Theme.Brush(BackColor), path);
        e.Graphics.DrawPath(Theme.Stroke(Theme.Line), path);
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
            _path?.Dispose();
        base.Dispose(disposing);
    }

    private GraphicsPath? _path;
    private Size _pathSize;
    private int _pathRadius;

    private GraphicsPath EnsurePath()
    {
        if (_path is not null && _pathSize == Size && _pathRadius == Radius)
            return _path;

        RebuildPath();
        return _path!;
    }

    private void RebuildPath()
    {
        _path?.Dispose();
        _path = Width > 1 && Height > 1
            ? Gfx.Round(new RectangleF(0.5f, 0.5f, Width - 1f, Height - 1f), Radius)
            : new GraphicsPath();
        _pathSize = Size;
        _pathRadius = Radius;
    }
}
