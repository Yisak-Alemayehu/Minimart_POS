namespace AbayPOS.UI.Theming;

/// <summary>GDI text that is measured before it is drawn. Money is never ellipsized.</summary>
public static class TextKit
{
    private static readonly Dictionary<(string Text, int Width, int BaseTenths, int MinTenths, FontStyle Style), float> Fitted = new();

    private const TextFormatFlags Single =
        TextFormatFlags.NoPadding | TextFormatFlags.NoPrefix | TextFormatFlags.SingleLine |
        TextFormatFlags.EndEllipsis | TextFormatFlags.VerticalCenter | TextFormatFlags.Left;

    private const TextFormatFlags Wrapped =
        TextFormatFlags.NoPadding | TextFormatFlags.NoPrefix | TextFormatFlags.WordBreak |
        TextFormatFlags.EndEllipsis | TextFormatFlags.Top | TextFormatFlags.Left;

    private const TextFormatFlags MoneyFlags =
        TextFormatFlags.NoPadding | TextFormatFlags.NoPrefix | TextFormatFlags.SingleLine |
        TextFormatFlags.VerticalCenter | TextFormatFlags.Right;

    private const TextFormatFlags CenterFlags =
        TextFormatFlags.NoPadding | TextFormatFlags.NoPrefix | TextFormatFlags.SingleLine |
        TextFormatFlags.EndEllipsis | TextFormatFlags.VerticalCenter | TextFormatFlags.HorizontalCenter;

    public static Size Measure(Graphics graphics, string? text, Font font)
    {
        ArgumentNullException.ThrowIfNull(graphics);
        ArgumentNullException.ThrowIfNull(font);
        return TextRenderer.MeasureText(graphics, text ?? string.Empty, font, new Size(int.MaxValue, int.MaxValue),
            TextFormatFlags.NoPadding | TextFormatFlags.NoPrefix | TextFormatFlags.SingleLine);
    }

    public static void DrawFit(Graphics graphics, string? text, Font font, Rectangle bounds, Color color, bool wrap = false)
    {
        TextRenderer.DrawText(graphics, text ?? string.Empty, font, bounds, color, wrap ? Wrapped : Single);
    }

    public static void DrawCenter(Graphics graphics, string? text, Font font, Rectangle bounds, Color color)
    {
        TextRenderer.DrawText(graphics, text ?? string.Empty, font, bounds, color, CenterFlags);
    }

    public static void DrawExact(Graphics graphics, string? text, Font font, Rectangle bounds, Color color)
    {
        TextRenderer.DrawText(graphics, text ?? string.Empty, font, bounds, color,
            TextFormatFlags.NoPadding | TextFormatFlags.NoPrefix | TextFormatFlags.SingleLine |
            TextFormatFlags.VerticalCenter | TextFormatFlags.Left);
    }

    /// <summary>Draws a money string in full. Call <see cref="FitFont"/> first when the slot is narrow.</summary>
    public static void DrawMoney(Graphics graphics, string text, Font font, Rectangle bounds, Color color)
    {
        TextRenderer.DrawText(graphics, text, font, bounds, color, MoneyFlags);
    }

    public static Font FitFont(Graphics graphics, string text, Font baseFont, int maxWidth, float minSize)
    {
        ArgumentNullException.ThrowIfNull(graphics);
        ArgumentNullException.ThrowIfNull(baseFont);
        var key = (text, Math.Max(1, maxWidth), (int)(baseFont.Size * 10), (int)(minSize * 10), baseFont.Style);
        if (!Fitted.TryGetValue(key, out var chosen))
        {
            chosen = baseFont.Size;
            while (chosen > minSize && Measure(graphics, text, Theme.Font(chosen, baseFont.Style)).Width > maxWidth)
                chosen -= 1f;
            Fitted[key] = chosen;
        }

        return Theme.Font(chosen, baseFont.Style);
    }
}
