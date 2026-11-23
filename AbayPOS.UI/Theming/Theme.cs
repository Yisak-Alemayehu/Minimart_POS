using System.Drawing.Text;

namespace AbayPOS.UI.Theming;

/// <summary>Light retail visual system. Every control color is opaque.</summary>
public static class Theme
{
    public static readonly Color Base = Color.FromArgb(0xF3, 0xF6, 0xFB);
    public static readonly Color Card = Color.White;
    public static readonly Color GlassSolid = Card;
    public static readonly Color GlassFill = Card;
    public static readonly Color Sidebar = Color.White;
    public static readonly Color SidebarGlass = Sidebar;
    public static readonly Color CartSurface = Color.White;
    public static readonly Color NavRaised = Color.FromArgb(0xE8, 0xF0, 0xFE);
    public static readonly Color NavGlass = NavRaised;
    public static readonly Color CancelGlass = Color.FromArgb(0xF1, 0xF5, 0xF9);
    public static readonly Color Search = Color.White;
    public static readonly Color SearchGlass = Search;
    public static readonly Color Tile = Color.White;
    public static readonly Color TileHover = Color.FromArgb(0xF8, 0xFA, 0xFC);
    public static readonly Color TileMuted = Color.FromArgb(0xF1, 0xF5, 0xF9);
    public static readonly Color Line = Color.FromArgb(0xE2, 0xE8, 0xF0);
    public static readonly Color GlassBorder = Line;
    public static readonly Color Highlight = NavRaised;
    public static readonly Color Accent = Color.FromArgb(0x2F, 0x6F, 0xED);
    public static readonly Color AccentHover = Color.FromArgb(0x4C, 0x84, 0xF5);
    public static readonly Color AccentPressed = Color.FromArgb(0x1D, 0x4E, 0xD8);
    public static readonly Color NavActive = Accent;
    public static readonly Color OnAccent = Color.White;
    public static readonly Color Brand = Color.FromArgb(0x1D, 0x4E, 0xD8);
    public static readonly Color Success = Color.FromArgb(0x16, 0xA3, 0x4A);
    public static readonly Color Warning = Color.FromArgb(0xD9, 0x77, 0x06);
    public static readonly Color Danger = Color.FromArgb(0xDC, 0x26, 0x26);
    public static readonly Color Text = Color.FromArgb(0x0F, 0x17, 0x2A);
    public static readonly Color TextSecondary = Color.FromArgb(0x64, 0x74, 0x8B);
    public static readonly Color InputFill = Color.FromArgb(0xF8, 0xFA, 0xFC);
    public static readonly Color Selection = Color.FromArgb(0xDB, 0xEA, 0xFE);
    public static readonly Color GridLine = Color.FromArgb(0xE2, 0xE8, 0xF0);
    public static readonly Color Overlay = Color.FromArgb(0xE2, 0xE8, 0xF0);
    public static readonly Color FocusRing = Accent;
    public static readonly Color Shadow = Color.FromArgb(0xE2, 0xE8, 0xF0);
    public static readonly Color Disabled = Color.FromArgb(0x94, 0xA3, 0xB8);
    public static readonly Color BadgeFill = Color.FromArgb(0xE8, 0xF0, 0xFE);
    public static readonly Color BadgeBorder = Color.FromArgb(0xCB, 0xD5, 0xE1);

    public const int Space = 8;
    public const int SidebarWidth = 228;
    public const int TopBarHeight = 72;
    public const int MinimumWidth = 1280;
    public const int MinimumHeight = 720;
    public const int PayButtonHeight = 52;
    public const int TouchTarget = 44;
    public const int PanelRadius = 16;
    public const int ButtonRadius = 12;
    public const int ChipRadius = 10;
    public const int InputRadius = 12;
    public const int AnimationMs = 160;

    public static readonly StringFormat CenterFormat = new()
    {
        Alignment = StringAlignment.Center,
        LineAlignment = StringAlignment.Center,
        Trimming = StringTrimming.EllipsisCharacter,
        FormatFlags = StringFormatFlags.NoWrap
    };

    public static readonly StringFormat NearFormat = new()
    {
        Alignment = StringAlignment.Near,
        LineAlignment = StringAlignment.Center,
        Trimming = StringTrimming.EllipsisCharacter,
        FormatFlags = StringFormatFlags.NoWrap
    };

    public static readonly StringFormat FarFormat = new()
    {
        Alignment = StringAlignment.Far,
        LineAlignment = StringAlignment.Center,
        Trimming = StringTrimming.EllipsisCharacter,
        FormatFlags = StringFormatFlags.NoWrap
    };

    private static readonly Dictionary<(string Family, float Size, FontStyle Style), Font> FontCache = new();
    private static readonly Dictionary<int, SolidBrush> BrushCache = new();
    private static readonly Dictionary<(int Argb, int Width), Pen> PenCache = new();
    private static string? _family;
    private static string? _iconFamily;

    public static Font FontHeading() => Font(18f, FontStyle.Bold);
    public static Font FontTotal() => Font(28f, FontStyle.Bold);
    public static Font FontBody() => Font(12.5f);
    public static Font FontHint() => Font(10f);

    public static Font Font(float size, FontStyle style = FontStyle.Regular) =>
        CachedFont(FontFamilyName(), size, style);

    public static Font IconFont(float size) =>
        CachedFont(IconFamilyName(), size, FontStyle.Regular);

    public static SolidBrush Brush(Color color)
    {
        var key = color.ToArgb();
        if (BrushCache.TryGetValue(key, out var brush))
            return brush;

        brush = new SolidBrush(color);
        BrushCache[key] = brush;
        return brush;
    }

    public static Pen Stroke(Color color, float width = 1f)
    {
        var key = (color.ToArgb(), (int)Math.Round(width * 100));
        if (PenCache.TryGetValue(key, out var pen))
            return pen;

        pen = new Pen(color, width);
        PenCache[key] = pen;
        return pen;
    }

    private static Font CachedFont(string family, float size, FontStyle style)
    {
        var key = (family, size, style);
        if (FontCache.TryGetValue(key, out var font))
            return font;

        font = new Font(family, size, style, GraphicsUnit.Point);
        FontCache[key] = font;
        return font;
    }

    public static string FontFamilyName()
    {
        if (_family is not null)
            return _family;

        using var installed = new InstalledFontCollection();
        foreach (var candidate in new[] { "Segoe UI Variable Display", "Segoe UI Variable", "Segoe UI" })
        {
            if (installed.Families.Any(font => font.Name.Equals(candidate, StringComparison.OrdinalIgnoreCase)))
            {
                _family = candidate;
                return _family;
            }
        }

        _family = "Segoe UI";
        return _family;
    }

    private static string IconFamilyName()
    {
        if (_iconFamily is not null)
            return _iconFamily;

        using var installed = new InstalledFontCollection();
        foreach (var candidate in new[] { "Segoe Fluent Icons", "Segoe MDL2 Assets" })
        {
            if (installed.Families.Any(font => font.Name.Equals(candidate, StringComparison.OrdinalIgnoreCase)))
            {
                _iconFamily = candidate;
                return _iconFamily;
            }
        }

        _iconFamily = "Segoe MDL2 Assets";
        return _iconFamily;
    }

    public static Color Solid(Color color) => color.A == 255 ? color : Base;

    public static bool IsLight(Color color) =>
        (color.R * 0.299 + color.G * 0.587 + color.B * 0.114) > 170;

    public static Color On(Color fill) => IsLight(fill) ? Text : OnAccent;
}
