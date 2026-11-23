using System.Drawing.Drawing2D;
using AbayPOS.Domain.Exceptions;
using AbayPOS.Domain.Models;
using AbayPOS.UI.Controls;
using AbayPOS.UI.Infrastructure;
using AbayPOS.UI.Theming;

namespace AbayPOS.UI.Forms;

/// <summary>UC-01 sign-in. Blue brand pane beside a white card.</summary>
public sealed class LoginForm : ThemedForm
{
    private readonly LoginField _username = new("Username", "Enter your username", false)
    {
        Dock = DockStyle.Fill,
        AccessibleName = "Username"
    };
    private readonly LoginField _password = new("Password", "Enter your password", true)
    {
        Dock = DockStyle.Fill,
        AccessibleName = "Password"
    };
    private readonly CheckBox _remember = new()
    {
        Text = "Remember me",
        Dock = DockStyle.Left,
        AutoSize = true,
        ForeColor = Theme.Text,
        BackColor = Theme.Card,
        Font = Theme.FontHint()
    };
    private readonly Label _note = new()
    {
        Dock = DockStyle.Fill,
        ForeColor = Theme.TextSecondary,
        BackColor = Theme.Card,
        Font = Theme.FontHint(),
        TextAlign = ContentAlignment.MiddleLeft
    };
    private readonly RoundButton _submit = new();
    private static readonly string RememberPath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "AbayPOS",
        "last-user.txt");

    public LoginForm()
        : base(sizable: false)
    {
        Text = "AbayPOS – Sign in";
        MaximizeBox = false;
        ClientSize = new Size(1100, 720);
        MinimumSize = new Size(960, 640);
        AcceptButton = _submit;
        Build();
        LoadRemembered();
    }

    public event EventHandler<Session>? SessionStarted;

    private void Build()
    {
        var root = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            BackColor = Theme.Base,
            Padding = new Padding(28)
        };
        root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 46));
        root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 54));
        root.Controls.Add(new BrandPane { Dock = DockStyle.Fill, Margin = new Padding(0, 0, 16, 0) }, 0, 0);

        var host = new Panel { Dock = DockStyle.Fill, BackColor = Theme.Base };
        var card = new GlassPanel
        {
            Size = new Size(460, 620),
            FillColor = Theme.Card,
            Padding = new Padding(36, 28, 36, 24),
            Anchor = AnchorStyles.None
        };
        host.Resize += (_, _) =>
        {
            card.Left = Math.Max(0, (host.ClientSize.Width - card.Width) / 2);
            card.Top = Math.Max(0, (host.ClientSize.Height - card.Height) / 2);
        };

        var body = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 8,
            BackColor = Theme.Card
        };
        body.RowStyles.Add(new RowStyle(SizeType.Absolute, 72));
        body.RowStyles.Add(new RowStyle(SizeType.Absolute, 28));
        body.RowStyles.Add(new RowStyle(SizeType.Absolute, 108));
        body.RowStyles.Add(new RowStyle(SizeType.Absolute, 108));
        body.RowStyles.Add(new RowStyle(SizeType.Absolute, 36));
        body.RowStyles.Add(new RowStyle(SizeType.Absolute, 52));
        body.RowStyles.Add(new RowStyle(SizeType.Absolute, 56));
        body.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

        body.Controls.Add(new MarkLabel(), 0, 0);
        body.Controls.Add(new Label
        {
            Text = "Sign in to the register",
            Dock = DockStyle.Fill,
            TextAlign = ContentAlignment.TopCenter,
            ForeColor = Theme.TextSecondary,
            BackColor = Theme.Card,
            Font = Theme.Font(12f)
        }, 0, 1);
        body.Controls.Add(_username, 0, 2);
        body.Controls.Add(_password, 0, 3);

        var rememberRow = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, BackColor = Theme.Card };
        rememberRow.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
        rememberRow.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
        rememberRow.Controls.Add(_remember, 0, 0);
        var forgot = new LinkLabel
        {
            Text = "Forgot password?",
            Dock = DockStyle.Fill,
            TextAlign = ContentAlignment.MiddleRight,
            LinkColor = Theme.Accent,
            ActiveLinkColor = Theme.AccentPressed,
            BackColor = Theme.Card,
            Font = Theme.FontHint()
        };
        forgot.LinkClicked += (_, _) => _note.Text = "Ask a manager to reset the password. There is no email reset.";
        rememberRow.Controls.Add(forgot, 1, 0);
        body.Controls.Add(rememberRow, 0, 4);

        _submit.Dock = DockStyle.Fill;
        _submit.Text = "Sign in";
        _submit.IsPrimary = true;
        _submit.FillColor = Theme.Accent;
        _submit.Click += (_, _) => SignIn();
        body.Controls.Add(_submit, 0, 5);

        var contact = new LinkLabel
        {
            Text = "Contact Administrator",
            Dock = DockStyle.Fill,
            TextAlign = ContentAlignment.MiddleCenter,
            LinkColor = Theme.Text,
            ActiveLinkColor = Theme.Accent,
            BackColor = Theme.Card,
            Font = Theme.Font(11, FontStyle.Bold)
        };
        contact.LinkClicked += (_, _) => _note.Text = "Store manager: admin. A manager unlocks a locked account.";
        body.Controls.Add(contact, 0, 6);
        body.Controls.Add(_note, 0, 7);

        card.Controls.Add(body);
        host.Controls.Add(card);
        root.Controls.Add(host, 1, 0);
        Controls.Add(root);
        Shown += (_, _) => _username.Focus();
    }

    private void LoadRemembered()
    {
        try
        {
            if (!File.Exists(RememberPath))
                return;
            var name = File.ReadAllText(RememberPath).Trim();
            if (name.Length == 0)
                return;
            _username.Text = name;
            _remember.Checked = true;
        }
        catch (IOException)
        {
            // A missing preference must not block sign-in.
        }
    }

    private void SaveRemembered()
    {
        try
        {
            if (!_remember.Checked)
            {
                if (File.Exists(RememberPath))
                    File.Delete(RememberPath);
                return;
            }

            Directory.CreateDirectory(Path.GetDirectoryName(RememberPath)!);
            File.WriteAllText(RememberPath, _username.Text ?? string.Empty);
        }
        catch (IOException)
        {
            // Preference write is optional.
        }
    }

    private void SignIn()
    {
        _username.ErrorText = string.Empty;
        _password.ErrorText = string.Empty;
        _note.Text = string.Empty;
        UiAsync.Run(async () =>
        {
            var session = await ServiceLocator.Auth.LoginAsync(_username.Text ?? string.Empty, _password.Text ?? string.Empty);
            SaveRemembered();
            SessionStarted?.Invoke(this, session);
        }, _submit, ex =>
        {
            if (ex is LoginFailedException login)
            {
                if (string.Equals(login.Field, "Username", StringComparison.OrdinalIgnoreCase))
                    _username.ErrorText = login.Message;
                else
                    _password.ErrorText = login.Message;
            }
            else
            {
                _password.ErrorText = UiAsync.Message(ex);
            }
        });
    }

    private sealed class MarkLabel : Control
    {
        public MarkLabel()
        {
            Dock = DockStyle.Fill;
            BackColor = Theme.Card;
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint | ControlStyles.OptimizedDoubleBuffer, true);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            e.Graphics.Clear(Theme.Card);
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            using var font = Theme.Font(28, FontStyle.Bold);
            using var brush = new SolidBrush(Theme.Accent);
            using var format = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
            e.Graphics.DrawString("AbayPOS", font, brush, ClientRectangle, format);
        }
    }

    private sealed class BrandPane : Control
    {
        public BrandPane()
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
            BackColor = Theme.Brand;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            if (Width <= 1 || Height <= 1)
                return;
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            using var brush = new LinearGradientBrush(ClientRectangle, Theme.Brand, Theme.Accent, 135f);
            e.Graphics.FillRectangle(brush, ClientRectangle);
            using var white = new SolidBrush(Color.White);
            using var soft = new SolidBrush(Color.FromArgb(230, 236, 255));
            using var mark = Theme.IconFont(42);
            using var title = Theme.Font(36, FontStyle.Bold);
            using var sub = Theme.Font(16);
            using var tag = Theme.Font(13, FontStyle.Bold);
            using var center = new StringFormat { Alignment = StringAlignment.Center };
            var y = Height * 0.28f;
            e.Graphics.DrawString("\uE7BF", mark, white, new RectangleF(0, y, Width, 70), center);
            e.Graphics.DrawString("AbayPOS", title, white, new RectangleF(0, y + 78, Width, 52), center);
            e.Graphics.DrawString("Smart Retail Solution", sub, soft, new RectangleF(40, y + 136, Width - 80, 32), center);
            e.Graphics.DrawString("Fast   ·   Simple   ·   Reliable", tag, white, new RectangleF(40, y + 188, Width - 80, 28), center);
        }
    }
}
