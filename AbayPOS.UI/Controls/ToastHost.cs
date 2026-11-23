using System.Drawing.Drawing2D;
using AbayPOS.UI.Theming;

namespace AbayPOS.UI.Controls;

public enum ToastKind
{
    Success,
    Error,
    Warning
}

/// <summary>Bottom-right toasts that auto-hide after 3 seconds.</summary>
public static class ToastHost
{
    public static void Success(string message) => Show(message, ToastKind.Success);
    public static void Error(string message) => Show(message, ToastKind.Error);
    public static void Warn(string message) => Show(message, ToastKind.Warning);

    public static void Show(string message, ToastKind kind)
    {
        var form = Form.ActiveForm;
        if (form is null || form.IsDisposed)
            return;
        var toast = new ToastCard(message, kind);
        form.Controls.Add(toast);
        toast.BringToFront();
        toast.Left = form.ClientSize.Width - toast.Width - 24;
        toast.Top = form.ClientSize.Height - toast.Height - 24;
        toast.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
        toast.Play();
    }
}

internal sealed class ToastCard : Control
{
    private readonly System.Windows.Forms.Timer _life = new() { Interval = 3000 };
    private readonly Color _accent;

    public ToastCard(string message, ToastKind kind)
    {
        Text = message;
        Size = new Size(320, 64);
        _accent = kind switch
        {
            ToastKind.Success => Theme.Success,
            ToastKind.Warning => Theme.Warning,
            _ => Theme.Danger
        };
        SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint | ControlStyles.OptimizedDoubleBuffer, true);
        _life.Tick += (_, _) => BeginInvoke(Dispose);
        AccessibleName = message;
    }

    public void Play() => _life.Start();

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _life.Stop();
            _life.Dispose();
        }

        base.Dispose(disposing);
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
        using var path = Gfx.Round(new RectangleF(1, 1, Width - 2, Height - 2), Theme.ButtonRadius);
        using var fill = new SolidBrush(Theme.GlassSolid);
        using var border = new Pen(_accent, 1.5f);
        e.Graphics.FillPath(fill, path);
        e.Graphics.DrawPath(border, path);
        using var bar = new SolidBrush(_accent);
        e.Graphics.FillRectangle(bar, 10, 14, 4, Height - 28);
        using var font = Theme.FontBody();
        using var brush = new SolidBrush(Theme.Text);
        e.Graphics.DrawString(Text, font, brush, new RectangleF(24, 10, Width - 36, Height - 20));
    }
}
