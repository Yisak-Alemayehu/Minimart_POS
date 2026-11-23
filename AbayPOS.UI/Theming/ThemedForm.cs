using System.ComponentModel;
using System.Runtime.InteropServices;
using AbayPOS.UI.Controls;

namespace AbayPOS.UI.Theming;

/// <summary>Borderless dark form with frosted glass, zero-flicker painting, and drag-to-move.</summary>
public class ThemedForm : Form
{
    protected ThemedForm()
    {
        AutoScaleMode = AutoScaleMode.Dpi;
        StartPosition = FormStartPosition.CenterScreen;
        FormBorderStyle = FormBorderStyle.None;
        BackColor = Theme.Base;
        Font = Theme.FontBody();
        ForeColor = Theme.Text;
        MinimumSize = new Size(Theme.MinimumWidth, Theme.MinimumHeight);
        DoubleBuffered = true;
        KeyPreview = true;
        SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
        Load += (_, _) => GlassHelper.ApplyToWindow(this, PreferMica);
        Resize += (_, _) => GlassHelper.EnsureFallbackCache(ClientSize);
    }

    /// <summary>Manager windows may prefer Mica; cashier/login use Acrylic.</summary>
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    protected bool PreferMica { get; set; }

    protected override CreateParams CreateParams
    {
        get
        {
            var cp = base.CreateParams;
            cp.Style |= 0x02000000; // WS_CLIPCHILDREN — keep child controls from being painted over
            return cp;
        }
    }

    protected override void OnPaintBackground(PaintEventArgs e) =>
        e.Graphics.Clear(Theme.Base);

    /// <summary>Lets the user drag a borderless window from chrome that calls this handler.</summary>
    protected void DragWindow(object? sender, MouseEventArgs e)
    {
        if (e.Button != MouseButtons.Left || WindowState == FormWindowState.Maximized)
            return;
        ReleaseCapture();
        _ = SendMessage(Handle, 0xA1, (IntPtr)2, IntPtr.Zero);
    }

    /// <summary>Caption buttons used by shell forms.</summary>
    protected static FlowLayoutPanel CreateWindowButtons(Form owner)
    {
        var right = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.RightToLeft,
            WrapContents = false,
            BackColor = Theme.Base,
            Padding = new Padding(0, 0, 2, 0)
        };
        var close = new WindowButton { Glyph = "\uE8BB", IsClose = true };
        close.Click += (_, _) => owner.Close();
        var maximize = new WindowButton { Glyph = owner.WindowState == FormWindowState.Maximized ? "\uE923" : "\uE922" };
        maximize.Click += (_, _) =>
        {
            owner.WindowState = owner.WindowState == FormWindowState.Maximized
                ? FormWindowState.Normal
                : FormWindowState.Maximized;
            maximize.Glyph = owner.WindowState == FormWindowState.Maximized ? "\uE923" : "\uE922";
            maximize.Invalidate();
        };
        var minimize = new WindowButton { Glyph = "\uE921" };
        minimize.Click += (_, _) => owner.WindowState = FormWindowState.Minimized;
        right.Controls.Add(close);
        right.Controls.Add(maximize);
        right.Controls.Add(minimize);
        return right;
    }

    [DllImport("user32.dll")]
    private static extern bool ReleaseCapture();

    [DllImport("user32.dll", EntryPoint = "SendMessageW")]
    private static extern IntPtr SendMessage(IntPtr handle, int message, IntPtr wParam, IntPtr lParam);
}
