using AbayPOS.Domain.Enums;
using AbayPOS.UI.Controls;
using AbayPOS.UI.Theming;

namespace AbayPOS.UI.Dialogs;

public sealed class UserEditDialog : DialogForm
{
    private readonly GlassTextBox _username = new() { Placeholder = "Username" };
    private readonly GlassTextBox _fullName = new() { Placeholder = "Full name" };
    private readonly GlassTextBox _password = new() { Placeholder = "Password", UseSystemPasswordChar = true };
    private readonly ComboBox _role = new();
    private readonly Label _error = new() { ForeColor = Theme.Danger, AutoSize = true, Font = Theme.FontHint(), MaximumSize = new Size(360, 0) };
    private readonly bool _create;

    public UserEditDialog(string? username, string? fullName, UserRole role, bool create)
    {
        _create = create;
        Text = create ? "AbayPOS – Add user" : "AbayPOS – Edit user";
        AutoSize = true;
        AutoSizeMode = AutoSizeMode.GrowAndShrink;
        MinimumSize = new Size(Theme.Scale(420), Theme.Scale(320));
        _username.Text = username;
        _username.Enabled = create;
        _fullName.Text = fullName;
        _role.DropDownStyle = ComboBoxStyle.DropDownList;
        _role.FlatStyle = FlatStyle.Flat;
        _role.Items.AddRange([UserRole.Cashier, UserRole.Manager]);
        _role.SelectedItem = role;
        _role.BackColor = Theme.InputFill;
        _role.ForeColor = Theme.Text;
        var surface = new GlassPanel { Dock = DockStyle.Top, AutoSize = true, AutoSizeMode = AutoSizeMode.GrowAndShrink, Padding = new Padding(Theme.Scale(24)) };
        var grid = new TableLayoutPanel { Dock = DockStyle.Top, AutoSize = true, ColumnCount = 1, BackColor = Theme.Base };
        var row = 0;
        void Field(string caption, Control input)
        {
            grid.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            grid.Controls.Add(new Label { Text = caption, AutoSize = true, ForeColor = Theme.TextSecondary, Font = Theme.FontHint(), Margin = new Padding(0, 8, 0, 2) }, 0, row++);
            input.Dock = DockStyle.Top;
            input.MinimumSize = new Size(Theme.Scale(280), Theme.Scale(32));
            input.Margin = new Padding(0, 0, 0, 4);
            grid.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            grid.Controls.Add(input, 0, row++);
        }

        Field("Username", _username);
        Field("Full name", _fullName);
        Field("Role", _role);
        if (create)
            Field("Password", _password);
        grid.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        grid.Controls.Add(_error, 0, row++);
        var buttons = new FlowLayoutPanel { AutoSize = true, FlowDirection = FlowDirection.RightToLeft, BackColor = Theme.Base, WrapContents = false };
        var save = new RoundButton { Text = "Save", Width = Theme.Scale(120), Height = Theme.Scale(44), IsPrimary = true };
        save.Click += (_, _) => TrySave();
        var cancel = new RoundButton { Text = "Cancel", Width = Theme.Scale(110), Height = Theme.Scale(44), FillColor = Theme.CancelGlass, BorderColor = Theme.GlassBorder };
        cancel.Click += (_, _) => Close();
        buttons.Controls.Add(save);
        buttons.Controls.Add(cancel);
        grid.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        grid.Controls.Add(buttons, 0, row);
        surface.Controls.Add(grid);
        Controls.Add(surface);
        Surface.Adopt(this, Theme.Base);
        AcceptButton = save;
        CancelButton = cancel;
    }

    public string UsernameValue => _username.Text ?? string.Empty;
    public string FullNameValue => _fullName.Text ?? string.Empty;
    public string PasswordValue => _password.Text ?? string.Empty;
    public UserRole RoleValue => _role.SelectedItem is UserRole role ? role : UserRole.Cashier;

    private void TrySave()
    {
        if (string.IsNullOrWhiteSpace(FullNameValue) || (_create && (string.IsNullOrWhiteSpace(UsernameValue) || PasswordValue.Length < 6)))
        {
            _error.Text = _create ? "Username, full name and a 6+ character password are required." : "Enter a full name.";
            return;
        }

        DialogResult = DialogResult.OK;
        Close();
    }
}
