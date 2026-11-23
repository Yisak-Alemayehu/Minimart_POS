using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using Microsoft.Win32;

namespace AbayPOS.UI.Theming;

/// <summary>OS-aware frosted glass: Win11 Acrylic/Mica, Win10 accent acrylic, or a cached blur fallback.</summary>
public static class GlassHelper
{
    public enum BackdropKind
    {
        Acrylic,
        Mica,
        Fallback
    }

    private const int DwmwaUseImmersiveDarkMode = 20;
    private const int DwmwaWindowCornerPreference = 33;
    private const int DwmwaSystemBackdropType = 38;
    private const int DwmWcpRound = 2;
    private const int DwmsbtMainWindow = 2;
    private const int DwmsbtTransientWindow = 3;
    private const int AccentEnableAcrylicBlurBehind = 4;
    private const int WindowCompositionAccentPolicy = 19;

    private static readonly object CacheGate = new();
    private static Bitmap? _blurred;
    private static Bitmap? _noise;
    private static Size _cachedSize;

    /// <summary>True when Windows transparency effects are enabled.</summary>
    public static bool TransparencyEnabled { get; } = ReadTransparencyEnabled();

    /// <summary>Best backdrop the current session can actually show.</summary>
    public static BackdropKind Detect(bool preferMica = false)
    {
        if (!TransparencyEnabled || SystemInformation.TerminalServerSession)
            return BackdropKind.Fallback;

        if (OperatingSystem.IsWindowsVersionAtLeast(10, 0, 22621))
            return preferMica ? BackdropKind.Mica : BackdropKind.Acrylic;

        if (OperatingSystem.IsWindowsVersionAtLeast(10, 0, 17763))
            return BackdropKind.Acrylic;

        return BackdropKind.Fallback;
    }

    /// <summary>Applies DWM chrome and dark mode to a top-level window.</summary>
    public static void ApplyToWindow(Form form, bool mica = false)
    {
        ArgumentNullException.ThrowIfNull(form);
        form.BackColor = Theme.Base;
        if (!form.IsHandleCreated)
        {
            form.HandleCreated += (_, _) => ApplyToWindow(form, mica);
            return;
        }

        var hwnd = form.Handle;
        var dark = 1;
        _ = DwmSetWindowAttribute(hwnd, DwmwaUseImmersiveDarkMode, ref dark, sizeof(int));
        var corners = DwmWcpRound;
        _ = DwmSetWindowAttribute(hwnd, DwmwaWindowCornerPreference, ref corners, sizeof(int));

        var kind = Detect(mica);
        if (kind is BackdropKind.Acrylic or BackdropKind.Mica)
        {
            var backdrop = kind == BackdropKind.Mica ? DwmsbtMainWindow : DwmsbtTransientWindow;
            if (DwmSetWindowAttribute(hwnd, DwmwaSystemBackdropType, ref backdrop, sizeof(int)) == 0)
            {
                var margins = new Margins { Left = -1, Right = -1, Top = -1, Bottom = -1 };
                _ = DwmExtendFrameIntoClientArea(hwnd, ref margins);
                return;
            }
        }

        if (kind == BackdropKind.Acrylic && TryApplyWin10Acrylic(hwnd))
            return;

        EnsureFallbackCache(form.ClientSize);
    }

    /// <summary>Paints the form background: DWM shows through black, otherwise the cached blur.</summary>
    public static void PaintFormBackground(Form form, Graphics graphics)
    {
        ArgumentNullException.ThrowIfNull(form);
        ArgumentNullException.ThrowIfNull(graphics);
        graphics.Clear(Theme.Base);
    }

    /// <summary>Blurred bitmap used by inner glass panels, or null when DWM supplies the blur.</summary>
    public static Bitmap? BlurredBackground
    {
        get
        {
            lock (CacheGate)
                return _blurred;
        }
    }

    /// <summary>Rebuilds the fallback blur only when the window size changes.</summary>
    public static void EnsureFallbackCache(Size size)
    {
        if (size.Width <= 0 || size.Height <= 0)
            return;

        lock (CacheGate)
        {
            if (_blurred is not null && _cachedSize == size)
                return;

            _blurred?.Dispose();
            _blurred = BuildFallback(size);
            _cachedSize = size;
        }
    }

    /// <summary>Draws a glass panel: parent blur slice, tint, highlight, border, and a soft shadow.</summary>
    public static void PaintGlassPanel(Graphics g, Control panel, Color fill, int radius, bool shadow = true)
    {
        ArgumentNullException.ThrowIfNull(g);
        ArgumentNullException.ThrowIfNull(panel);
        g.SmoothingMode = SmoothingMode.AntiAlias;
        var bounds = new RectangleF(1.5f, 1.5f, panel.Width - 3f, panel.Height - 3f);
        if (bounds.Width <= 2 || bounds.Height <= 2)
            return;

        if (shadow)
        {
            using var shadowPath = Controls.Gfx.Round(new RectangleF(3, 4, panel.Width - 4, panel.Height - 4), radius);
            using var shadowBrush = new SolidBrush(Theme.Shadow);
            g.FillPath(shadowBrush, shadowPath);
        }

        using var path = Controls.Gfx.Round(bounds, radius);
        var blur = BlurredBackground;
        var form = panel.FindForm();
        if (blur is not null && form is not null && Detect() == BackdropKind.Fallback)
        {
            var panelScreen = panel.PointToScreen(Point.Empty);
            var formScreen = form.PointToScreen(Point.Empty);
            var ox = panelScreen.X - formScreen.X;
            var oy = panelScreen.Y - formScreen.Y;
            g.SetClip(path);
            g.DrawImage(blur, new Rectangle(0, 0, panel.Width, panel.Height),
                new Rectangle(ox, oy, panel.Width, panel.Height), GraphicsUnit.Pixel);
            g.ResetClip();
        }

        using var fillBrush = new SolidBrush(TransparencyEnabled ? fill : Theme.GlassSolid);
        g.FillPath(fillBrush, path);
        DrawNoise(g, path);

        using var highlight = new Pen(Theme.Highlight, 1f);
        g.DrawLine(highlight, bounds.X + radius, bounds.Y, bounds.Right - radius, bounds.Y);
        using var border = new Pen(Theme.GlassBorder, 1f);
        g.DrawPath(border, path);
    }

    private static void DrawNoise(Graphics g, GraphicsPath path)
    {
        _noise ??= CreateNoise();
        var state = g.Save();
        g.SetClip(path);
        using var attribs = new ImageAttributes();
        attribs.SetWrapMode(WrapMode.Tile);
        g.DrawImage(_noise, new Rectangle(0, 0, 4096, 4096), 0, 0, _noise.Width, _noise.Height, GraphicsUnit.Pixel, attribs);
        g.Restore(state);
    }

    private static Bitmap CreateNoise()
    {
        var bmp = new Bitmap(64, 64, PixelFormat.Format32bppArgb);
        var rng = new Random(4021);
        for (var y = 0; y < bmp.Height; y++)
        {
            for (var x = 0; x < bmp.Width; x++)
            {
                var a = rng.Next(4, 9);
                bmp.SetPixel(x, y, Color.FromArgb(a, 255, 255, 255));
            }
        }

        return bmp;
    }

    private static Bitmap BuildFallback(Size size)
    {
        var bmp = new Bitmap(size.Width, size.Height, PixelFormat.Format32bppArgb);
        using (var g = Graphics.FromImage(bmp))
        {
            using var brush = new LinearGradientBrush(
                new Rectangle(Point.Empty, size),
                Color.FromArgb(0x12, 0x10, 0x1C),
                Color.FromArgb(0x3A, 0x22, 0x1A),
                32f);
            g.FillRectangle(brush, 0, 0, size.Width, size.Height);
            using var orb = new GraphicsPath();
            orb.AddEllipse(size.Width * 0.55f, -size.Height * 0.2f, size.Width * 0.7f, size.Height * 0.7f);
            using var glow = new PathGradientBrush(orb)
            {
                CenterColor = Color.FromArgb(90, 245, 124, 31),
                SurroundColors = [Color.FromArgb(0, 245, 124, 31)]
            };
            g.FillPath(glow, orb);
        }

        BoxBlur(bmp, 8);
        BoxBlur(bmp, 8);
        BoxBlur(bmp, 8);
        using (var g = Graphics.FromImage(bmp))
        using (var tint = new SolidBrush(Color.FromArgb(120, 13, 11, 20)))
            g.FillRectangle(tint, 0, 0, bmp.Width, bmp.Height);

        return bmp;
    }

    private static void BoxBlur(Bitmap bmp, int radius)
    {
        var rect = new Rectangle(0, 0, bmp.Width, bmp.Height);
        var data = bmp.LockBits(rect, ImageLockMode.ReadWrite, PixelFormat.Format32bppArgb);
        try
        {
            var stride = Math.Abs(data.Stride);
            var length = stride * bmp.Height;
            var src = new byte[length];
            var dst = new byte[length];
            Marshal.Copy(data.Scan0, src, 0, length);
            BlurPass(src, dst, bmp.Width, bmp.Height, stride, radius, horizontal: true);
            BlurPass(dst, src, bmp.Width, bmp.Height, stride, radius, horizontal: false);
            Marshal.Copy(src, 0, data.Scan0, length);
        }
        finally
        {
            bmp.UnlockBits(data);
        }
    }

    private static void BlurPass(byte[] src, byte[] dst, int width, int height, int stride, int radius, bool horizontal)
    {
        var diameter = radius * 2 + 1;
        if (horizontal)
        {
            for (var y = 0; y < height; y++)
            {
                var r = 0;
                var g = 0;
                var b = 0;
                var a = 0;
                for (var i = -radius; i <= radius; i++)
                    Accumulate(src, stride, y, Clamp(i, 0, width - 1), ref r, ref g, ref b, ref a);

                for (var x = 0; x < width; x++)
                {
                    Write(dst, stride, x, y, r / diameter, g / diameter, b / diameter, a / diameter);
                    var outX = Clamp(x - radius, 0, width - 1);
                    var inX = Clamp(x + radius + 1, 0, width - 1);
                    Subtract(src, stride, y, outX, ref r, ref g, ref b, ref a);
                    Accumulate(src, stride, y, inX, ref r, ref g, ref b, ref a);
                }
            }
        }
        else
        {
            for (var x = 0; x < width; x++)
            {
                var r = 0;
                var g = 0;
                var b = 0;
                var a = 0;
                for (var i = -radius; i <= radius; i++)
                    Accumulate(src, stride, Clamp(i, 0, height - 1), x, ref r, ref g, ref b, ref a);

                for (var y = 0; y < height; y++)
                {
                    Write(dst, stride, x, y, r / diameter, g / diameter, b / diameter, a / diameter);
                    var outY = Clamp(y - radius, 0, height - 1);
                    var inY = Clamp(y + radius + 1, 0, height - 1);
                    Subtract(src, stride, outY, x, ref r, ref g, ref b, ref a);
                    Accumulate(src, stride, inY, x, ref r, ref g, ref b, ref a);
                }
            }
        }
    }

    private static int Clamp(int value, int min, int max) => value < min ? min : value > max ? max : value;

    private static void Accumulate(byte[] src, int stride, int y, int x, ref int r, ref int g, ref int b, ref int a)
    {
        var i = y * stride + x * 4;
        b += src[i];
        g += src[i + 1];
        r += src[i + 2];
        a += src[i + 3];
    }

    private static void Subtract(byte[] src, int stride, int y, int x, ref int r, ref int g, ref int b, ref int a)
    {
        var i = y * stride + x * 4;
        b -= src[i];
        g -= src[i + 1];
        r -= src[i + 2];
        a -= src[i + 3];
    }

    private static void Write(byte[] dst, int stride, int x, int y, int r, int g, int b, int a)
    {
        var i = y * stride + x * 4;
        dst[i] = (byte)Math.Clamp(b, 0, 255);
        dst[i + 1] = (byte)Math.Clamp(g, 0, 255);
        dst[i + 2] = (byte)Math.Clamp(r, 0, 255);
        dst[i + 3] = (byte)Math.Clamp(a, 0, 255);
    }

    private static bool TryApplyWin10Acrylic(IntPtr hwnd)
    {
        var policy = new AccentPolicy
        {
            State = AccentEnableAcrylicBlurBehind,
            Flags = 2,
            GradientColor = unchecked((int)0xB01C182F)
        };
        var size = Marshal.SizeOf<AccentPolicy>();
        var memory = Marshal.AllocHGlobal(size);
        try
        {
            Marshal.StructureToPtr(policy, memory, false);
            var data = new WindowCompositionAttributeData
            {
                Attribute = WindowCompositionAccentPolicy,
                Data = memory,
                SizeOfData = size
            };
            if (!SetWindowCompositionAttribute(hwnd, ref data))
                return false;
            var margins = new Margins { Left = -1, Right = -1, Top = -1, Bottom = -1 };
            return DwmExtendFrameIntoClientArea(hwnd, ref margins) == 0;
        }
        catch (DllNotFoundException)
        {
            return false;
        }
        catch (EntryPointNotFoundException)
        {
            return false;
        }
        finally
        {
            Marshal.FreeHGlobal(memory);
        }
    }

    private static bool ReadTransparencyEnabled()
    {
        try
        {
            using var key = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Themes\Personalize");
            var value = key?.GetValue("EnableTransparency");
            if (value is int enabled)
                return enabled != 0;
        }
        catch (Exception)
        {
            // Registry access can fail under restricted accounts; assume effects are on.
        }

        return true;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct Margins
    {
        internal int Left;
        internal int Right;
        internal int Top;
        internal int Bottom;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct AccentPolicy
    {
        internal int State;
        internal int Flags;
        internal int GradientColor;
        internal int AnimationId;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct WindowCompositionAttributeData
    {
        internal int Attribute;
        internal IntPtr Data;
        internal int SizeOfData;
    }

    [DllImport("dwmapi.dll")]
    private static extern int DwmExtendFrameIntoClientArea(IntPtr hwnd, ref Margins margins);

    [DllImport("dwmapi.dll")]
    private static extern int DwmSetWindowAttribute(IntPtr hwnd, int attribute, ref int value, int size);

    [DllImport("user32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool SetWindowCompositionAttribute(IntPtr hwnd, ref WindowCompositionAttributeData data);
}
