using System.ComponentModel;
using System.Drawing.Drawing2D;
using AbayPOS.UI.Theming;

namespace AbayPOS.UI.Controls;

/// <summary>Caption button drawn with Segoe MDL2 glyphs.</summary>
public sealed class WindowButton : Control
{
    private bool _hovered;

    public WindowButton()
    {
        SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.SupportsTransparentBackColor | ControlStyles.ResizeRedraw, true);
        Size = new Size(42, 32);
        Margin = new Padding(0, 14, 0, 0);
        Cursor = Cursors.Hand;
        BackColor = Color.Transparent;
        DoubleBuffered = true;
        TabStop = false;
        AccessibleName = "Window control";
    }

    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public string Glyph { get; set; } = string.Empty;

    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public bool IsClose { get; init; }

    protected override void OnMouseEnter(EventArgs e)
    {
        base.OnMouseEnter(e);
        _hovered = true;
        Invalidate();
    }

    protected override void OnMouseLeave(EventArgs e)
    {
        base.OnMouseLeave(e);
        _hovered = false;
        Invalidate();
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
        if (_hovered)
        {
            using var path = Gfx.Round(new RectangleF(1, 1, Width - 2, Height - 2), Theme.ChipRadius);
            using var fill = new SolidBrush(IsClose ? Theme.Danger : Theme.NavGlass);
            e.Graphics.FillPath(fill, path);
        }

        using var font = Theme.IconFont(9);
        using var brush = new SolidBrush(Theme.Text);
        using var format = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
        e.Graphics.DrawString(Glyph, font, brush, new RectangleF(0, 0, Width, Height), format);
    }
}
