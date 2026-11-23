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
        BackColor = Theme.NavRaised;
        SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
        Padding = new Padding(12, 0, 12, 0);
    }

    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public bool Selected
    {
        get => _selected;
        set
        {
            if (_selected == value)
                return;
            _selected = value;
            Invalidate();
        }
    }

    protected override void OnMouseEnter(EventArgs e)
    {
        base.OnMouseEnter(e);
        if (_hovered)
            return;
        _hovered = true;
        Invalidate();
    }

    protected override void OnMouseLeave(EventArgs e)
    {
        base.OnMouseLeave(e);
        if (!_hovered)
            return;
        _hovered = false;
        Invalidate();
    }
    protected override void OnClick(EventArgs e) { base.OnClick(e); }

    protected override void OnPaint(PaintEventArgs e)
    {
        var backdrop = Parent?.BackColor ?? Theme.Card;
        e.Graphics.Clear(Theme.Solid(backdrop));
        e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
        using var path = Gfx.Round(new RectangleF(0.5f, 0.5f, Width - 1, Height - 1), Theme.ChipRadius);
        var fill = Theme.Brush(Selected ? Theme.Accent : _hovered ? Theme.TileHover : Theme.NavRaised);
        var border = Theme.Stroke(Selected ? Theme.Accent : Theme.Line);
        e.Graphics.FillPath(fill, path);
        e.Graphics.DrawPath(border, path);
        TextKit.DrawCenter(e.Graphics, Text, Theme.FontHint(), ClientRectangle, Selected ? Theme.OnAccent : Theme.Text);
    }

    protected override void OnPaddingChanged(EventArgs e)
    {
        base.OnPaddingChanged(e);
        using var g = CreateGraphics();
        var font = Theme.FontHint();
        Width = Math.Max(64, (int)g.MeasureString(Text, font).Width + 28);
    }
}
