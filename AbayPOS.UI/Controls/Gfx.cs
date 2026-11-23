using System.Drawing.Drawing2D;

namespace AbayPOS.UI.Controls;

/// <summary>Shared GDI+ geometry used by themed controls.</summary>
public static class Gfx
{
    /// <summary>Rounded rectangle path. Caller must dispose the result.</summary>
    public static GraphicsPath Round(RectangleF rect, float radius)
    {
        var path = new GraphicsPath();
        if (rect.Width <= 0 || rect.Height <= 0)
        {
            path.AddRectangle(new RectangleF(rect.X, rect.Y, Math.Max(0.1f, rect.Width), Math.Max(0.1f, rect.Height)));
            return path;
        }

        var d = Math.Min(radius * 2f, Math.Min(rect.Width, rect.Height));
        if (d <= 0.5f)
        {
            path.AddRectangle(rect);
            return path;
        }

        path.AddArc(rect.X, rect.Y, d, d, 180, 90);
        path.AddArc(rect.Right - d, rect.Y, d, d, 270, 90);
        path.AddArc(rect.Right - d, rect.Bottom - d, d, d, 0, 90);
        path.AddArc(rect.X, rect.Bottom - d, d, d, 90, 90);
        path.CloseFigure();
        return path;
    }

    /// <summary>Ease-out cubic used by short UI animations.</summary>
    public static float EaseOut(float t)
    {
        var p = Math.Clamp(t, 0f, 1f);
        return 1f - MathF.Pow(1f - p, 3f);
    }
}
