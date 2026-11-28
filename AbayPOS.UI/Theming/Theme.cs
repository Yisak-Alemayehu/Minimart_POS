using System.Drawing.Text;

namespace AbayPOS.UI.Theming;

/// <summary>Retail visual system. Colors come from <see cref="Colors"/>; dark is the default.</summary>
public static class Theme
{
    /// <summary>Change this one assignment to <see cref="Palette.Light"/> for the light set.</summary>
    public static Palette Colors { get; set; } = Palette.Dark;

    public static Color Base => Colors.Background;
    public static Color Card => Colors.Surface;
    public static Color GlassSolid => Colors.Surface;
    public static Color GlassFill => Colors.Surface;
    public static Color Sidebar => Colors.Surface;
    public static Color SidebarGlass => Colors.Surface;
    public static Color CartSurface => Colors.Surface;
    public static Color NavRaised => Colors.Raised;
    public static Color NavGlass => Colors.Raised;
    public static Color CancelGlass => Colors.Raised;
    public static Color Search => Colors.Raised;
    public static Color SearchGlass => Colors.Raised;
    public static Color Tile => Colors.Surface;
    public static Color TileHover => Colors.Raised;
    public static Color TileMuted => Colors.Background;
    public static Color Line => Colors.Border;
    public static Color GlassBorder => Colors.Border;
    public static Color Highlight => Colors.Raised;
    public static Color Accent => Colors.Accent;
    public static Color AccentHover => Colors.AccentHover;
    public static Color AccentPressed => Colors.AccentPressed;
    public static Color NavActive => Colors.Accent;
    public static Color OnAccent => Colors.OnAccent;
    public static Color Brand => Colors.AccentPressed;
    public static Color Success => Colors.Success;
    public static Color Warning => Colors.Warning;
    public static Color Danger => Colors.Danger;
    public static Color Info => Colors.Info;
    public static Color Text => Colors.Text;
    public static Color TextSecondary => Colors.TextSecondary;
    public static Color InputFill => Colors.Raised;
    public static Color Selection => Colors.Raised;
    public static Color GridLine => Colors.Border;
    public static Color Overlay => Colors.Border;
    public static Color FocusRing => Colors.Accent;
    public static Color Shadow => Colors.Border;
    public static Color Disabled => Colors.Disabled;
    public static Color BadgeFill => Colors.Raised;
    public static Color BadgeBorder => Colors.Border;

    public const int Space = 8;
    public const int SidebarWidth = 240;
    public const int CashierRail = 72;
    public const int CartWidth = 420;
    public const int TopBarHeight = 64;
    public const int MinimumWidth = 1280;
    public const int MinimumHeight = 720;
    public const int PayButtonHeight = 56;
    public const int TouchTarget = 44;
    public const int PanelRadius = 12;
    public const int ButtonRadius = 8;
    public const int ChipRadius = 999;
    public const int InputRadius = 8;
    public const int AnimationMs = 150;

    /// <summary>Current monitor DPI. Updated from the active form.</summary>
    public static int DeviceDpi { get; private set; } = 96;

    public static void UseDpi(int dpi) => DeviceDpi = Math.Max(96, dpi);

    /// <summary>Converts a 96-DPI pixel size to the current monitor.</summary>
    public static int Scale(int px) => Math.Max(1, (int)Math.Round(px * DeviceDpi / 96f));

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

    public static Font FontHeading() => Font(20f, FontStyle.Bold);
    public static Font FontTotal() => Font(32f, FontStyle.Bold);
    public static Font FontSection() => Font(15f, FontStyle.Bold);
    public static Font FontBody() => Font(13f);
    public static Font FontHint() => Font(11f);

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

    /// <summary>Re-applies theme colors and fonts after Designer defaults at runtime load.</summary>
    public static void ApplyRuntime(Control root)
    {
        if (DesignTime.IsActive(root))
            return;
        Surface.Adopt(root, Base);
    }
}
