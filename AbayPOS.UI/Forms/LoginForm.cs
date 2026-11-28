using AbayPOS.Domain.Exceptions;
using AbayPOS.Domain.Models;
using AbayPOS.UI.Infrastructure;
using AbayPOS.UI.Theming;

namespace AbayPOS.UI.Forms;

public sealed partial class LoginForm : ThemedForm
{
    private static readonly string RememberPath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "AbayPOS", "last-user.txt");

    public LoginForm()
    {
        InitializeComponent();
        AcceptButton = btnSignIn;
        btnSignIn.Click += (_, _) => SignIn();
        lnkForgot.LinkClicked += (_, _) => lblNote.Text = "Ask a manager to reset the password. There is no email reset.";
        lnkContact.LinkClicked += (_, _) => lblNote.Text = "Store manager: admin. A manager unlocks a locked account.";
        KeyUp += (_, _) => { if (Control.IsKeyLocked(Keys.CapsLock)) fldPassword.ErrorText = "Caps Lock is on."; };
        Shown += (_, _) => fldUsername.Focus();
    }

    public event EventHandler<Session>? SessionStarted;

    protected override void OnLoad(EventArgs e)
    {
        base.OnLoad(e);
        if (DesignTime.IsActive(this)) return;
        Theme.ApplyRuntime(this);
        pnlHost.Resize += (_, _) => CenterCard();
        CenterCard();
        LoadRemembered();
    }

    private void CenterCard()
    {
        pnlCard.Left = Math.Max(0, (pnlHost.ClientSize.Width - pnlCard.Width) / 2);
        pnlCard.Top = Math.Max(0, (pnlHost.ClientSize.Height - pnlCard.Height) / 2);
    }

    private void LoadRemembered()
    {
        if (DesignTime.IsActive(this)) return;
        try
        {
            if (!File.Exists(RememberPath)) return;
            var name = File.ReadAllText(RememberPath).Trim();
            if (name.Length == 0) return;
            fldUsername.Text = name;
            chkRemember.Checked = true;
        }
        catch (IOException) { }
    }

    private void SaveRemembered()
    {
        try
        {
            if (!chkRemember.Checked)
            {
                if (File.Exists(RememberPath)) File.Delete(RememberPath);
                return;
            }
            Directory.CreateDirectory(Path.GetDirectoryName(RememberPath)!);
            File.WriteAllText(RememberPath, fldUsername.Text ?? string.Empty);
        }
        catch (IOException) { }
    }

    private void SignIn()
    {
        fldUsername.ErrorText = string.Empty;
        fldPassword.ErrorText = string.Empty;
        lblNote.Text = string.Empty;
        UiAsync.Run(async () =>
        {
            var session = await ServiceLocator.Auth.LoginAsync(fldUsername.Text ?? string.Empty, fldPassword.Text ?? string.Empty);
            SaveRemembered();
            SessionStarted?.Invoke(this, session);
        }, btnSignIn, ex =>
        {
            if (ex is LoginFailedException login)
            {
                if (string.Equals(login.Field, "Username", StringComparison.OrdinalIgnoreCase))
                    fldUsername.ErrorText = login.Message;
                else fldPassword.ErrorText = login.Message;
            }
            else fldPassword.ErrorText = UiAsync.Message(ex);
        });
    }
}
