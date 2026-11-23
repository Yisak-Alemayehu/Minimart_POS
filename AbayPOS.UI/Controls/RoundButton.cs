using System.ComponentModel;
using System.Drawing.Drawing2D;
using AbayPOS.UI.Theming;

namespace AbayPOS.UI.Controls;

/// <summary>Primary/secondary rounded button with hover, pressed, disabled, and focus ring.</summary>
public class RoundButton : Button
{
    private bool _hovered;
    private bool _pressed;

    public RoundButton()
    {
        FlatStyle = FlatStyle.Flat;
        FlatAppearance.BorderSize = 0;
        FlatAppearance.MouseOverBackColor = Theme.Accent;
        FlatAppearance.MouseDownBackColor = Theme.AccentPressed;
        BackColor = Theme.Accent;
        ForeColor = Theme.OnAccent;
        Font = Theme.Font(12f, FontStyle.Bold);
        Cursor = Cursors.Hand;
        MinimumSize = new Size(Theme.TouchTarget, Theme.TouchTarget);
        DoubleBuffered = true;
        SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
        TabStop = true;
    }

    private Color _fill = Theme.Accent;
    private string _badge = string.Empty;

    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public Color FillColor
    {
        get => _fill;
        set
        {
            _fill = Theme.Solid(value);
            BackColor = _fill;
            ForeColor = Theme.On(_fill);
            FlatAppearance.MouseOverBackColor = _fill;
            FlatAppearance.MouseDownBackColor = _fill;
            Invalidate();
        }
    }

    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public Color BorderColor { get; set; } = Color.Empty;

    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public string Icon { get; set; } = string.Empty;

    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public string Badge
    {
        get => _badge;
        set
        {
            _badge = value ?? string.Empty;
            Invalidate();
        }
    }

    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public bool IsPrimary { get; set; }

    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public ContentAlignment LabelAlign { get; set; } = ContentAlignment.MiddleCenter;

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
        if (!_hovered && !_pressed)
            return;
        _hovered = false;
        _pressed = false;
        Invalidate();
    }

    protected override void OnMouseDown(MouseEventArgs e)
    {
        base.OnMouseDown(e);
        if (_pressed)
            return;
        _pressed = true;
        Invalidate();
    }

    protected override void OnMouseUp(MouseEventArgs mevent)
    {
        base.OnMouseUp(mevent);
        if (!_pressed)
            return;
        _pressed = false;
        Invalidate();
    }

    protected override void OnGotFocus(EventArgs e)
    {
        base.OnGotFocus(e);
        Invalidate();
    }

    protected override void OnLostFocus(EventArgs e)
    {
        base.OnLostFocus(e);
        Invalidate();
    }

    protected override void OnPaintBackground(PaintEventArgs e)
    {
        var backdrop = Parent?.BackColor ?? Theme.Base;
        e.Graphics.Clear(Theme.Solid(backdrop));
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
        var scale = _pressed && Enabled ? 0.98f : 1f;
        var w = Width * scale;
        var h = Height * scale;
        var x = (Width - w) / 2f;
        var y = (Height - h) / 2f;
        using var path = Gfx.Round(new RectangleF(x + 0.5f, y + 0.5f, w - 1f, h - 1f), Theme.ButtonRadius);
        var fill = FillColor;
        if (!Enabled)
            fill = Theme.NavRaised;
        else if (_pressed)
            fill = Darken(fill, 0.12f);
        else if (_hovered)
            fill = Lighten(fill, 0.10f);

        e.Graphics.FillPath(Theme.Brush(fill), path);
        if (BorderColor.A == 255)
            e.Graphics.DrawPath(Theme.Stroke(Enabled ? BorderColor : Color.FromArgb(255, 40, 40, 40)), path);

        if (Focused && Enabled)
        {
            using var ringPath = Gfx.Round(new RectangleF(2, 2, Width - 4, Height - 4), Theme.ButtonRadius);
            e.Graphics.DrawPath(Theme.Stroke(Theme.FocusRing, 2f), ringPath);
        }

        var textColor = Enabled ? ForeColor : Theme.Disabled;
        var textBrush = Theme.Brush(textColor);
        if (Icon.Length > 0)
        {
            var iconFont = Theme.IconFont(12);
            e.Graphics.DrawString(Icon, iconFont, textBrush, 16, (Height - iconFont.Height) / 2f - 1);
        }
        using var format = new StringFormat
        {
            Alignment = LabelAlign is ContentAlignment.MiddleLeft or ContentAlignment.TopLeft or ContentAlignment.BottomLeft
                ? StringAlignment.Near
                : StringAlignment.Center,
            LineAlignment = StringAlignment.Center,
            Trimming = StringTrimming.EllipsisCharacter
        };
        var left = Icon.Length > 0 ? 38 : 8;
        var right = Badge.Length > 0 ? 86 : 16;
        e.Graphics.DrawString(Text, Font, textBrush, new RectangleF(left, 0, Width - left - right, Height), format);

        if (Badge.Length == 0)
            return;

        var rect = new RectangleF(Width - 70, (Height - 24) / 2f, 56, 24);
        using var badgePath = Gfx.Round(rect, Theme.ChipRadius);
        var badgeFont = Theme.Font(8, FontStyle.Bold);
        e.Graphics.FillPath(Theme.Brush(Theme.BadgeFill), badgePath);
        e.Graphics.DrawPath(Theme.Stroke(Theme.BadgeBorder), badgePath);
        e.Graphics.DrawString(Badge, badgeFont, Theme.Brush(Theme.Text), rect, Theme.CenterFormat);
    }

    private static Color Lighten(Color color, float amount) =>
        Color.FromArgb(color.A,
            Math.Min(255, color.R + (int)((255 - color.R) * amount)),
            Math.Min(255, color.G + (int)((255 - color.G) * amount)),
            Math.Min(255, color.B + (int)((255 - color.B) * amount)));

    private static Color Darken(Color color, float amount) =>
        Color.FromArgb(color.A,
            Math.Max(0, (int)(color.R * (1 - amount))),
            Math.Max(0, (int)(color.G * (1 - amount))),
            Math.Max(0, (int)(color.B * (1 - amount))));
}
