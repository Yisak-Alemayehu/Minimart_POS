using AbayPOS.Domain.Exceptions;
using AbayPOS.Domain.Models;
using AbayPOS.UI.Controls;
using AbayPOS.UI.Infrastructure;
using AbayPOS.UI.Theming;

namespace AbayPOS.UI.Forms;

/// <summary>UC-01 sign-in. Frosted card centered on a blurred backdrop.</summary>
public sealed class LoginForm : ThemedForm
{
    private readonly GlassTextBox _username = new();
    private readonly GlassTextBox _password = new();
    private readonly Label _userError = new();
    private readonly Label _passError = new();
    private readonly RoundButton _submit = new();
    private readonly RoundButton _toggle = new();
    private bool _showPassword;

    public LoginForm()
    {
        Text = "AbayPOS";
        MinimumSize = new Size(900, 600);
        ClientSize = new Size(1100, 720);
        WindowState = FormWindowState.Maximized;
        MaximizedBounds = Screen.PrimaryScreen?.WorkingArea ?? Rectangle.Empty;
        AcceptButton = _submit;
        Build();
    }

    public event EventHandler<Session>? SessionStarted;

    private void Build()
    {
        var card = new GlassPanel
        {
            Size = new Size(400, 480),
            Padding = new Padding(32, 28, 32, 24),
            Anchor = AnchorStyles.None
        };
        Layout += (_, _) =>
        {
            card.Left = Math.Max(0, (ClientSize.Width - card.Width) / 2);
            card.Top = Math.Max(0, (ClientSize.Height - card.Height) / 2);
        };
        Controls.Add(card);

        var y = 8;
        card.Controls.Add(Caption("AbayPOS", Theme.Font(28, FontStyle.Bold), Theme.Text, ref y, 40));
        card.Controls.Add(Caption("Sign in to the register", Theme.FontHint(), Theme.TextSecondary, ref y, 28));
        card.Controls.Add(Caption("Username", Theme.FontHint(), Theme.TextSecondary, ref y, 22));
        Place(_username, card, ref y, 44);
        _username.Placeholder = "admin or cashier";
        _username.AccessibleName = "Username";
        StyleError(_userError, card, ref y);

        card.Controls.Add(Caption("Password", Theme.FontHint(), Theme.TextSecondary, ref y, 22));
        _password.UseSystemPasswordChar = true;
        _password.Placeholder = "Your password";
        _password.AccessibleName = "Password";
        _password.SetBounds(0, y, card.ClientSize.Width - card.Padding.Horizontal - 88, 44);
        _toggle.Text = "Show";
        _toggle.SetBounds(_password.Right + 8, y, 80, 44);
        _toggle.FillColor = Theme.CancelGlass;
        _toggle.BorderColor = Theme.GlassBorder;
        _toggle.Font = Theme.FontHint();
        _toggle.Click += (_, _) =>
        {
            _showPassword = !_showPassword;
            _password.UseSystemPasswordChar = !_showPassword;
            _toggle.Text = _showPassword ? "Hide" : "Show";
        };
        card.Controls.Add(_password);
        card.Controls.Add(_toggle);
        y += 52;
        StyleError(_passError, card, ref y);

        _submit.Text = "Sign in";
        _submit.Badge = "Enter";
        _submit.IsPrimary = true;
        _submit.SetBounds(0, card.ClientSize.Height - card.Padding.Bottom - 52, card.ClientSize.Width - card.Padding.Horizontal, 48);
        _submit.Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom;
        _submit.Click += (_, _) => SignIn();
        card.Controls.Add(_submit);
        Shown += (_, _) => _username.Focus();
    }

    private static Label Caption(string text, Font font, Color color, ref int y, int height)
    {
        var label = new Label
        {
            Text = text,
            Font = font,
            ForeColor = color,
            AutoSize = false,
            Location = new Point(0, y),
            Size = new Size(330, height),
            TextAlign = ContentAlignment.MiddleLeft,
            BackColor = Color.Transparent
        };
        y += height;
        return label;
    }

    private static void Place(Control control, Control parent, ref int y, int height)
    {
        control.SetBounds(0, y, parent.ClientSize.Width - parent.Padding.Horizontal, height);
        parent.Controls.Add(control);
        y += height + 4;
    }

    private void SignIn()
    {
        _userError.Text = string.Empty;
        _passError.Text = string.Empty;
        UiAsync.Run(async () =>
        {
            var session = await ServiceLocator.Auth.LoginAsync(_username.Text ?? string.Empty, _password.Text ?? string.Empty);
            SessionStarted?.Invoke(this, session);
        }, _submit, ex =>
        {
            if (ex is LoginFailedException login)
            {
                if (string.Equals(login.Field, "Username", StringComparison.OrdinalIgnoreCase))
                    _userError.Text = login.Message;
                else
                    _passError.Text = login.Message;
            }
            else
            {
                _passError.Text = UiAsync.Message(ex);
            }
        });
    }

    private static void StyleError(Label label, Control parent, ref int y)
    {
        label.SetBounds(0, y, 330, 20);
        label.ForeColor = Theme.Danger;
        label.Font = Theme.FontHint();
        parent.Controls.Add(label);
        y += 22;
    }
}
