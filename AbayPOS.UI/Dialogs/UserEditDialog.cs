using AbayPOS.Domain.Enums;
using AbayPOS.UI.Controls;
using AbayPOS.UI.Theming;

namespace AbayPOS.UI.Dialogs;

public sealed partial class UserEditDialog : DialogForm
{
    private bool _create;

    public UserEditDialog()
    {
        InitializeComponent();
        cmbRole.Items.AddRange([UserRole.Cashier, UserRole.Manager]);
        cmbRole.SelectedIndex = 0;
        btnSave.Click += (_, _) => TrySave();
        btnCancel.Click += (_, _) => Close();
        AcceptButton = btnSave;
        CancelButton = btnCancel;
    }

    public UserEditDialog(string? username, string? fullName, UserRole role, bool create) : this()
    {
        _create = create;
        Text = create ? "AbayPOS – Add user" : "AbayPOS – Edit user";
        txtUsername.Text = username;
        txtUsername.Enabled = create;
        txtFullName.Text = fullName;
        cmbRole.SelectedItem = role;
        lblPasswordCaption.Visible = create;
        txtPassword.Visible = create;
    }

    public string UsernameValue => txtUsername.Text ?? string.Empty;
    public string FullNameValue => txtFullName.Text ?? string.Empty;
    public string PasswordValue => txtPassword.Text ?? string.Empty;
    public UserRole RoleValue => cmbRole.SelectedItem is UserRole role ? role : UserRole.Cashier;

    private void TrySave()
    {
        if (string.IsNullOrWhiteSpace(FullNameValue) || (_create && (string.IsNullOrWhiteSpace(UsernameValue) || PasswordValue.Length < 6)))
        {
            lblError.Text = _create ? "Username, full name and a 6+ character password are required." : "Enter a full name.";
            return;
        }

        DialogResult = DialogResult.OK;
        Close();
    }

    protected override void OnLoad(EventArgs e)
    {
        base.OnLoad(e);
        if (DesignTime.IsActive(this))
            return;
        Theme.ApplyRuntime(this);
    }
}