using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Drawing.Drawing2D;
using System.Runtime.InteropServices;
using AbayPOS.UI.Theming;

namespace AbayPOS.UI.Controls;

/// <summary>Opaque labeled login input. Avoids WinForms fake-transparency so labels never paint inside the field.</summary>
public sealed class LoginField : UserControl
{
    private readonly Label _caption = new();
    private readonly InputShell _shell = new();
    private readonly TextBox _input = new();
    private readonly Label _error = new();
    private Button? _reveal;
    private bool _secret;
    private bool _revealed;

    public LoginField()
    {
        AutoScaleMode = AutoScaleMode.Dpi;
        BackColor = Theme.GlassSolid;
        ForeColor = Theme.Text;
        Margin = Padding.Empty;
        Padding = Padding.Empty;
        TabStop = false;
        Height = 102;

        _caption.Dock = DockStyle.Top;
        _caption.AutoSize = false;
        _caption.Height = 22;
        _caption.Font = Theme.FontHint();
        _caption.ForeColor = Theme.TextSecondary;
        _caption.BackColor = Theme.GlassSolid;
        _caption.TextAlign = ContentAlignment.BottomLeft;
        _caption.Padding = new Padding(2, 0, 0, 4);

        _error.Dock = DockStyle.Bottom;
        _error.AutoSize = false;
        _error.Height = 22;
        _error.Font = Theme.FontHint();
        _error.ForeColor = Theme.Danger;
        _error.BackColor = Theme.GlassSolid;
        _error.TextAlign = ContentAlignment.TopLeft;
        _error.Padding = new Padding(2, 4, 0, 0);

        _shell.Dock = DockStyle.Fill;
        _shell.Padding = new Padding(14, 10, 6, 10);

        _input.BorderStyle = BorderStyle.None;
        _input.Dock = DockStyle.Fill;
        _input.BackColor = Theme.InputFill;
        _input.ForeColor = Theme.Text;
        _input.Font = Theme.Font(13f);
        _input.Margin = Padding.Empty;
        _input.GotFocus += (_, _) => { _shell.Active = true; _shell.Invalidate(); };
        _input.LostFocus += (_, _) => { _shell.Active = false; _shell.Invalidate(); };
        _input.TextChanged += (_, e) => TextChanged?.Invoke(this, e);

        _shell.Controls.Add(_input);
        Controls.Add(_shell);
        Controls.Add(_caption);
        Controls.Add(_error);
    }

    public LoginField(string caption, string placeholder, bool password) : this()
    {
        Caption = caption;
        Placeholder = placeholder;
        IsPassword = password;
    }

    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public string Caption
    {
        get => _caption.Text;
        set => _caption.Text = value;
    }

    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public string Placeholder { get; set; } = string.Empty;

    [AllowNull]
    public override string Text
    {
        get => _input.Text;
        set => _input.Text = value ?? string.Empty;
    }

    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public string ErrorText
    {
        get => _error.Text;
        set => _error.Text = value;
    }

    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public bool IsPassword
    {
        get => _secret;
        set
        {
            _secret = value;
            ApplySecret();
        }
    }

    public new event EventHandler? TextChanged;

    public new void Focus() => _input.Focus();

    protected override void OnHandleCreated(EventArgs e)
    {
        base.OnHandleCreated(e);
        ApplySecret();
        if (Placeholder.Length > 0 && _input.IsHandleCreated)
            SendMessage(_input.Handle, 0x1501, IntPtr.Zero, Placeholder);
    }

    private void ApplySecret()
    {
        _input.UseSystemPasswordChar = _secret && !_revealed;
        if (!_secret || _reveal is not null)
            return;

        _reveal = new Button
        {
            Text = "Show",
            Dock = DockStyle.Right,
            Width = 72,
            FlatStyle = FlatStyle.Flat,
            BackColor = Theme.InputFill,
            ForeColor = Theme.TextSecondary,
            Font = Theme.FontHint(),
            Cursor = Cursors.Hand,
            TabStop = true,
            AccessibleName = "Show password"
        };
        _reveal.FlatAppearance.BorderSize = 0;
        _reveal.FlatAppearance.MouseOverBackColor = Theme.InputFill;
        _reveal.FlatAppearance.MouseDownBackColor = Theme.InputFill;
        _reveal.Click += (_, _) =>
        {
            _revealed = !_revealed;
            _input.UseSystemPasswordChar = !_revealed;
            if (_reveal is null)
                return;
            _reveal.Text = _revealed ? "Hide" : "Show";
            _reveal.AccessibleName = _revealed ? "Hide password" : "Show password";
        };
        _shell.Controls.Add(_reveal);
        _reveal.BringToFront();
    }

    private sealed class InputShell : Panel
    {
        public InputShell()
        {
            BackColor = Theme.InputFill;
            DoubleBuffered = true;
            SetStyle(ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw | ControlStyles.AllPaintingInWmPaint, true);
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool Active { get; set; }

        protected override void OnPaintBackground(PaintEventArgs e)
        {
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            e.Graphics.Clear(Theme.GlassSolid);
            using var path = Gfx.Round(new RectangleF(1f, 1f, Width - 2f, Height - 2f), Theme.InputRadius);
            using var fill = new SolidBrush(Theme.InputFill);
            e.Graphics.FillPath(fill, path);
            using var border = new Pen(Active ? Theme.Accent : Theme.GlassBorder, Active ? 2f : 1f);
            e.Graphics.DrawPath(border, path);
        }
    }

    [DllImport("user32.dll", CharSet = CharSet.Unicode)]
    private static extern IntPtr SendMessage(IntPtr handle, int message, IntPtr wParam, string text);
}
