using System.Runtime.InteropServices;

namespace AbayPOS.UI.Theming;

/// <summary>Paints the native caption to match the current palette. No blur, acrylic, or frame extension.</summary>
public static class WindowStyler
{
    private const int UseImmersiveDarkMode = 20;
    private const int CaptionColor = 35;
    private const int TextColor = 36;

    /// <summary>Applies immersive caption colors. Older Windows ignore the attributes that they do not support.</summary>
    public static void Apply(Form form)
    {
        ArgumentNullException.ThrowIfNull(form);
        if (!form.IsHandleCreated)
            return;

        var hwnd = form.Handle;
        var dark = Theme.IsLight(Theme.Base) ? 0 : 1;
        Try(hwnd, UseImmersiveDarkMode, dark);
        Try(hwnd, CaptionColor, ColorRef(Theme.Base));
        Try(hwnd, TextColor, ColorRef(Theme.Text));
    }

    private static int ColorRef(Color color) => color.R | (color.G << 8) | (color.B << 16);

    private static void Try(IntPtr hwnd, int attribute, int value)
    {
        try
        {
            _ = DwmSetWindowAttribute(hwnd, attribute, ref value, sizeof(int));
        }
        catch (DllNotFoundException)
        {
            // dwmapi is absent on some server SKUs.
        }
        catch (EntryPointNotFoundException)
        {
            // The attribute is newer than this Windows build.
        }
    }

    [DllImport("dwmapi.dll")]
    private static extern int DwmSetWindowAttribute(IntPtr hwnd, int attribute, ref int value, int size);
}
