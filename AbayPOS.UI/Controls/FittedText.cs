using System.ComponentModel;
using AbayPOS.UI.Theming;

namespace AbayPOS.UI.Controls;

/// <summary>Draws one string at the largest font that fits. Money is never ellipsized.</summary>
[ToolboxItem(true)]
[Category("AbayPOS")]
[Description("Auto-sized text display.")]
public sealed class FittedText : Control
{
    private readonly ToolTip _tip = new();

    public FittedText()
    {
        SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint | ControlStyles.OptimizedDoubleBuffer, true);
        Dock = DockStyle.Fill;
        AccessibleRole = AccessibleRole.StaticText;
    }

    [Category("AbayPOS")]
    [DefaultValue(24f)]
    [Description("Maximum font size in points.")]
    public float StartSize { get; set; } = 24f;

    [Category("AbayPOS")]
    [DefaultValue(14f)]
    [Description("Minimum font size in points.")]
    public float MinSize { get; set; } = 14f;

    [Category("AbayPOS")]
    [DefaultValue(false)]
    [Description("Use money formatting rules.")]
    public bool Money { get; set; }

    [Category("AbayPOS")]
    [DefaultValue(ContentAlignment.MiddleLeft)]
    [Description("Text alignment.")]
    public ContentAlignment Align { get; set; } = ContentAlignment.MiddleLeft;

    public void SetValue(string? value)
    {
        Text = value ?? string.Empty;
        _tip.SetToolTip(this, Text);
        AccessibleName = Text;
        Invalidate();
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        e.Graphics.Clear(Parent?.BackColor ?? Theme.Card);
        var bounds = new Rectangle(0, 0, Math.Max(1, Width), Math.Max(1, Height));
        var font = TextKit.FitFont(e.Graphics, Text, Theme.Font(StartSize, FontStyle.Bold), Math.Max(1, Width - 4), MinSize);
        if (Money || Align == ContentAlignment.MiddleRight)
            TextKit.DrawMoney(e.Graphics, Text, font, bounds, ForeColor);
        else
            TextKit.DrawFit(e.Graphics, Text, font, bounds, ForeColor);
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
            _tip.Dispose();
        base.Dispose(disposing);
    }
}
