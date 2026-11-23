using System.Drawing.Text;

namespace AbayPOS.UI.Theming;

/// <summary>Single visual system for every AbayPOS form. No magic numbers in forms.</summary>
public static class Theme
{
    public static readonly Color Base = Color.FromArgb(0x0D, 0x0B, 0x14);
    public static readonly Color GlassFill = Color.FromArgb(175, 0x1C, 0x18, 0x2F);
    public static readonly Color GlassSolid = Color.FromArgb(0x1C, 0x18, 0x2F);
    public static readonly Color GlassBorder = Color.FromArgb(28, 255, 255, 255);
    public static readonly Color Highlight = Color.FromArgb(38, 255, 255, 255);
    public static readonly Color Accent = Color.FromArgb(0xF5, 0x7C, 0x1F);
    public static readonly Color AccentHover = Color.FromArgb(255, 143, 51);
    public static readonly Color AccentPressed = Color.FromArgb(220, 100, 18);
    public static readonly Color Success = Color.FromArgb(0x22, 0xC5, 0x5E);
    public static readonly Color Warning = Color.FromArgb(0xF5, 0x9E, 0x0B);
    public static readonly Color Danger = Color.FromArgb(0xEF, 0x44, 0x44);
    public static readonly Color Text = Color.FromArgb(0xF5, 0xF5, 0xF7);
    public static readonly Color TextSecondary = Color.FromArgb(0xA1, 0xA1, 0xAA);
    public static readonly Color SidebarGlass = Color.FromArgb(190, 0x16, 0x13, 0x24);
    public static readonly Color SearchGlass = Color.FromArgb(150, 0x1C, 0x18, 0x2F);
    public static readonly Color CancelGlass = Color.FromArgb(90, 0x1C, 0x18, 0x2F);
    public static readonly Color NavGlass = Color.FromArgb(40, 255, 255, 255);
    public static readonly Color NavActive = Color.FromArgb(40, 0xF5, 0x7C, 0x1F);
    public static readonly Color InputFill = Color.FromArgb(0x18, 0x15, 0x27);
    public static readonly Color CartSurface = Color.FromArgb(0x16, 0x13, 0x24);
    public static readonly Color Selection = Color.FromArgb(90, 0xF5, 0x7C, 0x1F);
    public static readonly Color Tile = Color.FromArgb(140, 0x23, 0x1E, 0x37);
    public static readonly Color TileHover = Color.FromArgb(175, 0x2A, 0x22, 0x3C);
    public static readonly Color TileMuted = Color.FromArgb(80, 0x23, 0x1E, 0x37);
    public static readonly Color Overlay = Color.FromArgb(160, 0, 0, 0);
    public static readonly Color FocusRing = Accent;
    public static readonly Color Shadow = Color.FromArgb(70, 0, 0, 0);
    public static readonly Color Disabled = Color.FromArgb(102, 0xF5, 0xF5, 0xF7);
    public static readonly Color BadgeFill = Color.FromArgb(45, 255, 255, 255);
    public static readonly Color BadgeBorder = Color.FromArgb(90, 255, 255, 255);

    public const int Space = 8;
    public const int SidebarWidth = 200;
    public const int TopBarHeight = 64;
    public const int MinimumWidth = 1280;
    public const int MinimumHeight = 720;
    public const int PayButtonHeight = 56;
    public const int TouchTarget = 44;
    public const int PanelRadius = 18;
    public const int ButtonRadius = 12;
    public const int ChipRadius = 8;
    public const int InputRadius = 12;
    public const int AnimationMs = 160;

    public static Font FontHeading() => Font(18f, FontStyle.Bold);
    public static Font FontTotal() => Font(28f, FontStyle.Bold);
    public static Font FontBody() => Font(12.5f);
    public static Font FontHint() => Font(10f);
    public static Font Font(float size, FontStyle style = FontStyle.Regular) =>
        new(FontFamilyName(), size, style, GraphicsUnit.Point);

    public static Font IconFont(float size) =>
        new("Segoe MDL2 Assets", size, FontStyle.Regular, GraphicsUnit.Point);

    public static string FontFamilyName()
    {
        using var installed = new InstalledFontCollection();
        foreach (var candidate in new[] { "Segoe UI Variable Display", "Segoe UI Variable", "Segoe UI" })
        {
            if (installed.Families.Any(font => font.Name.Equals(candidate, StringComparison.OrdinalIgnoreCase)))
                return candidate;
        }

        return "Segoe UI";
    }
}
