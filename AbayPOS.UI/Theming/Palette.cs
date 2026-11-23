namespace AbayPOS.UI.Theming;

/// <summary>Opaque color set. Switch <see cref="Theme.Colors"/> to change the whole app.</summary>
public sealed record Palette(
    Color Background,
    Color Surface,
    Color Raised,
    Color Border,
    Color Accent,
    Color AccentHover,
    Color AccentPressed,
    Color Success,
    Color Warning,
    Color Danger,
    Color Info,
    Color Text,
    Color TextSecondary,
    Color Disabled,
    Color OnAccent)
{
    public static Palette Dark { get; } = new(
        Background: Color.FromArgb(0x0D, 0x0B, 0x14),
        Surface: Color.FromArgb(0x1C, 0x18, 0x2F),
        Raised: Color.FromArgb(0x25, 0x20, 0x38),
        Border: Color.FromArgb(0x2E, 0x29, 0x45),
        Accent: Color.FromArgb(0xF5, 0x7C, 0x1F),
        AccentHover: Color.FromArgb(0xFF, 0x8F, 0x3A),
        AccentPressed: Color.FromArgb(0xD9, 0x69, 0x0F),
        Success: Color.FromArgb(0x22, 0xC5, 0x5E),
        Warning: Color.FromArgb(0xF5, 0x9E, 0x0B),
        Danger: Color.FromArgb(0xEF, 0x44, 0x44),
        Info: Color.FromArgb(0x3B, 0x82, 0xF6),
        Text: Color.FromArgb(0xF5, 0xF5, 0xF7),
        TextSecondary: Color.FromArgb(0xA1, 0xA1, 0xAA),
        Disabled: Color.FromArgb(0x6B, 0x68, 0x80),
        OnAccent: Color.White);

    public static Palette Light { get; } = new(
        Background: Color.FromArgb(0xF3, 0xF6, 0xFB),
        Surface: Color.White,
        Raised: Color.FromArgb(0xE8, 0xF0, 0xFE),
        Border: Color.FromArgb(0xE2, 0xE8, 0xF0),
        Accent: Color.FromArgb(0x2F, 0x6F, 0xED),
        AccentHover: Color.FromArgb(0x4C, 0x84, 0xF5),
        AccentPressed: Color.FromArgb(0x1D, 0x4E, 0xD8),
        Success: Color.FromArgb(0x16, 0xA3, 0x4A),
        Warning: Color.FromArgb(0xD9, 0x77, 0x06),
        Danger: Color.FromArgb(0xDC, 0x26, 0x26),
        Info: Color.FromArgb(0x3B, 0x82, 0xF6),
        Text: Color.FromArgb(0x0F, 0x17, 0x2A),
        TextSecondary: Color.FromArgb(0x64, 0x74, 0x8B),
        Disabled: Color.FromArgb(0x94, 0xA3, 0xB8),
        OnAccent: Color.White);
}
