using AbayPOS.Domain.Enums;
using AbayPOS.UI.Controls;
using AbayPOS.UI.Theming;

namespace AbayPOS.UI.Dialogs;

public sealed class UserEditDialog : Form
{
    private readonly GlassTextBox _username = new() { Placeholder = "Username" };
    private readonly GlassTextBox _fullName = new() { Placeholder = "Full name" };
    private readonly GlassTextBox _password = new() { Placeholder = "Password", UseSystemPasswordChar = true };
    private readonly ComboBox _role = new();
    private readonly Label _error = new() { ForeColor = Theme.Danger, Dock = DockStyle.Fill, Font = Theme.FontHint() };
    private readonly bool _create;

    public UserEditDialog(string? username, string? fullName, UserRole role, bool create)
    {
        _create = create;
        Text = create ? "Add user" : "Edit user";
        FormBorderStyle = FormBorderStyle.None;
        StartPosition = FormStartPosition.CenterParent;
        ClientSize = new Size(400, 360);
        BackColor = Theme.Base;
        ShowInTaskbar = false;
        _username.Text = username;
        _username.Enabled = create;
        _fullName.Text = fullName;
        _role.Dock = DockStyle.Fill;
        _role.DropDownStyle = ComboBoxStyle.DropDownList;
        _role.Items.AddRange([UserRole.Cashier, UserRole.Manager]);
        _role.SelectedItem = role;
        _role.BackColor = Theme.InputFill;
        _role.ForeColor = Theme.Text;
        var surface = new GlassPanel { Dock = DockStyle.Fill, Padding = new Padding(24) };
        var grid = new TableLayoutPanel { Dock = DockStyle.Fill, RowCount = 7, BackColor = Color.Transparent };
        grid.Controls.Add(new Label { Text = Text, Font = Theme.FontHeading(), ForeColor = Theme.Text, Dock = DockStyle.Fill }, 0, 0);
        _username.Dock = DockStyle.Fill;
        _fullName.Dock = DockStyle.Fill;
        _password.Dock = DockStyle.Fill;
        grid.Controls.Add(_username, 0, 1);
        grid.Controls.Add(_fullName, 0, 2);
        grid.Controls.Add(_role, 0, 3);
        if (create)
            grid.Controls.Add(_password, 0, 4);
        grid.Controls.Add(_error, 0, 5);
        var buttons = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.RightToLeft, BackColor = Color.Transparent };
        var save = new RoundButton { Text = "Save", Width = 120, Height = 44, IsPrimary = true };
        save.Click += (_, _) => TrySave();
        var cancel = new RoundButton { Text = "Cancel", Width = 110, Height = 44, FillColor = Theme.CancelGlass, BorderColor = Theme.GlassBorder };
        cancel.Click += (_, _) => Close();
        buttons.Controls.Add(save);
        buttons.Controls.Add(cancel);
        grid.Controls.Add(buttons, 0, 6);
        surface.Controls.Add(grid);
        Controls.Add(surface);
        Load += (_, _) => GlassHelper.ApplyToWindow(this);
    }

    public string UsernameValue => _username.Text ?? string.Empty;
    public string FullNameValue => _fullName.Text ?? string.Empty;
    public string PasswordValue => _password.Text ?? string.Empty;
    public UserRole RoleValue => _role.SelectedItem is UserRole role ? role : UserRole.Cashier;

    protected override void OnPaintBackground(PaintEventArgs e) => GlassHelper.PaintFormBackground(this, e.Graphics);

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
