using System.ComponentModel;
using System.Drawing.Drawing2D;
using AbayPOS.UI.Theming;

namespace AbayPOS.UI.Controls;

/// <summary>Category filter chip.</summary>
public sealed class ChipButton : Control
{
    private bool _hovered;
    private bool _selected;

    public ChipButton(string text)
    {
        Text = text;
        Height = 32;
        MinimumSize = new Size(64, 32);
        Cursor = Cursors.Hand;
        TabStop = true;
        AccessibleName = text;
        DoubleBuffered = true;
        SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
        Padding = new Padding(12, 0, 12, 0);
    }

    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public bool Selected
    {
        get => _selected;
        set
        {
            _selected = value;
            Invalidate();
        }
    }

    protected override void OnMouseEnter(EventArgs e) { base.OnMouseEnter(e); _hovered = true; Invalidate(); }
    protected override void OnMouseLeave(EventArgs e) { base.OnMouseLeave(e); _hovered = false; Invalidate(); }
    protected override void OnClick(EventArgs e) { base.OnClick(e); }

    protected override void OnPaint(PaintEventArgs e)
    {
        e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
        using var path = Gfx.Round(new RectangleF(0.5f, 0.5f, Width - 1, Height - 1), Theme.ChipRadius);
        using var fill = new SolidBrush(Selected ? Color.FromArgb(60, Theme.Accent) : _hovered ? Theme.NavGlass : Theme.CancelGlass);
        using var border = new Pen(Selected ? Theme.Accent : Theme.GlassBorder);
        e.Graphics.FillPath(fill, path);
        e.Graphics.DrawPath(border, path);
        using var font = Theme.FontHint();
        using var brush = new SolidBrush(Theme.Text);
        using var format = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
        e.Graphics.DrawString(Text, font, brush, ClientRectangle, format);
    }

    protected override void OnPaddingChanged(EventArgs e)
    {
        base.OnPaddingChanged(e);
        using var g = CreateGraphics();
        using var font = Theme.FontHint();
        Width = Math.Max(64, (int)g.MeasureString(Text, font).Width + 28);
    }
}
