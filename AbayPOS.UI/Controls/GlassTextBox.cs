using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Drawing.Drawing2D;
using AbayPOS.UI.Theming;

namespace AbayPOS.UI.Controls;

/// <summary>Rounded input that hosts a real TextBox so IME, passwords, and Enter still work.</summary>
public sealed class GlassTextBox : Panel
{
    private readonly TextBox _inner = new();
    private bool _focused;

    public GlassTextBox()
    {
        Height = Theme.TouchTarget;
        BackColor = Theme.InputFill;
        Padding = new Padding(12, 8, 12, 8);
        DoubleBuffered = true;
        SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
        _inner.BorderStyle = BorderStyle.None;
        _inner.Dock = DockStyle.Fill;
        _inner.BackColor = Theme.InputFill;
        _inner.ForeColor = Theme.Text;
        _inner.Font = Theme.FontBody();
        _inner.GotFocus += (_, _) => { _focused = true; Invalidate(); };
        _inner.LostFocus += (_, _) => { _focused = false; Invalidate(); };
        _inner.TextChanged += (_, e) => TextChanged?.Invoke(this, e);
        _inner.KeyDown += (_, e) => KeyDown?.Invoke(this, e);
        Controls.Add(_inner);
    }

    [AllowNull]
    public override string Text
    {
        get => _inner.Text;
        set => _inner.Text = value ?? string.Empty;
    }

    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public char PasswordChar
    {
        get => _inner.PasswordChar;
        set => _inner.PasswordChar = value;
    }

    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public bool UseSystemPasswordChar
    {
        get => _inner.UseSystemPasswordChar;
        set => _inner.UseSystemPasswordChar = value;
    }

    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public string Placeholder { get; set; } = string.Empty;

    public new event EventHandler? TextChanged;
    public new event KeyEventHandler? KeyDown;

    public void SelectAll() => _inner.SelectAll();

    public new void Focus() => _inner.Focus();

    protected override void OnPaintBackground(PaintEventArgs e)
    {
        e.Graphics.Clear(Theme.InputFill);
        e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
        using var path = Gfx.Round(new RectangleF(0.5f, 0.5f, Width - 1f, Height - 1f), Theme.InputRadius);
        using var fill = new SolidBrush(Theme.InputFill);
        e.Graphics.FillPath(fill, path);
        using var border = new Pen(_focused ? Theme.Accent : Theme.GlassBorder, _focused ? 2f : 1f);
        e.Graphics.DrawPath(border, path);
        if (!_focused)
            return;
        using var ring = new Pen(Color.FromArgb(80, Theme.Accent), 2f);
        using var ringPath = Gfx.Round(new RectangleF(2, 2, Width - 4, Height - 4), Theme.InputRadius);
        e.Graphics.DrawPath(ring, ringPath);
    }

    protected override void OnPaint(PaintEventArgs e)
    {
    }

    protected override void OnHandleCreated(EventArgs e)
    {
        base.OnHandleCreated(e);
        if (Placeholder.Length > 0)
            SendMessage(_inner.Handle, 0x1501, IntPtr.Zero, Placeholder);
    }

    [System.Runtime.InteropServices.DllImport("user32.dll", CharSet = System.Runtime.InteropServices.CharSet.Unicode)]
    private static extern IntPtr SendMessage(IntPtr handle, int message, IntPtr wParam, string text);
}
