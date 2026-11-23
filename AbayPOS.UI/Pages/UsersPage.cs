using AbayPOS.Domain.Entities;
using AbayPOS.Domain.Enums;
using AbayPOS.Domain.Models;
using AbayPOS.UI.Controls;
using AbayPOS.UI.Dialogs;
using AbayPOS.UI.Infrastructure;
using AbayPOS.UI.Theming;

namespace AbayPOS.UI.Pages;

internal sealed class UsersPage : UserControl
{
    private readonly Session _session;
    private readonly DataGridView _grid = new();
    private IReadOnlyList<User> _users = [];

    public UsersPage(Session session)
    {
        _session = session;
        Dock = DockStyle.Fill;
        BackColor = Theme.Base;
        var panel = new GlassPanel { Dock = DockStyle.Fill, Padding = new Padding(16) };
        var layout = new TableLayoutPanel { Dock = DockStyle.Fill, RowCount = 3, BackColor = Theme.Base };
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 40));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 48));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        layout.Controls.Add(new Label { Text = "Users", Font = Theme.FontHeading(), ForeColor = Theme.Text, Dock = DockStyle.Fill }, 0, 0);
        var toolbar = new FlowLayoutPanel { Dock = DockStyle.Fill, BackColor = Theme.Base };
        toolbar.Controls.Add(Btn("Add", true, () => Create()));
        toolbar.Controls.Add(Btn("Edit", false, Edit));
        toolbar.Controls.Add(Btn("Deactivate", false, Deactivate));
        toolbar.Controls.Add(Btn("Unlock", false, Unlock));
        toolbar.Controls.Add(Btn("Reset password", false, ResetPassword));
        layout.Controls.Add(toolbar, 0, 1);
        GridTheme.Apply(_grid);
        _grid.Dock = DockStyle.Fill;
        _grid.Columns.Add("User", "Username");
        _grid.Columns.Add("Name", "Name");
        _grid.Columns.Add("Role", "Role");
        _grid.Columns.Add("Locked", "Locked");
        _grid.Columns.Add("Active", "Active");
        GridTheme.Balance(_grid, "Name");
        layout.Controls.Add(_grid, 0, 2);
        panel.Controls.Add(layout);
        Controls.Add(panel);
        Load += (_, _) => Reload();
    }

    public void Reload() => UiAsync.Run(async () =>
    {
        _users = await ServiceLocator.Users.GetAllAsync();
        _grid.Rows.Clear();
        foreach (var user in _users)
            _grid.Rows.Add(user.Username, user.FullName, user.Role, user.IsLocked ? "Yes" : "No", user.IsActive ? "Yes" : "No");
    });

    private User? Selected() =>
        _grid.CurrentRow is null ? null : _users.ElementAtOrDefault(_grid.CurrentRow.Index);

    private void Create()
    {
        using var overlay = ConfirmDialog.CreateOverlay(FindForm()!);
        overlay?.Show();
        using var dialog = new UserEditDialog(null, null, UserRole.Cashier, true);
        var result = dialog.ShowDialog(this);
        overlay?.Close();
        if (result != DialogResult.OK)
            return;
        UiAsync.Run(async () =>
        {
            await ServiceLocator.Users.CreateAsync(dialog.UsernameValue, dialog.FullNameValue, dialog.PasswordValue, dialog.RoleValue, _session);
            Reload();
        });
    }

    private void Edit()
    {
        if (Selected() is not { } user)
            return;
        using var overlay = ConfirmDialog.CreateOverlay(FindForm()!);
        overlay?.Show();
        using var dialog = new UserEditDialog(user.Username, user.FullName, user.Role, false);
        var result = dialog.ShowDialog(this);
        overlay?.Close();
        if (result != DialogResult.OK)
            return;
        UiAsync.Run(async () =>
        {
            await ServiceLocator.Users.UpdateAsync(user.Id, dialog.FullNameValue, dialog.RoleValue, user.IsActive, _session);
            Reload();
        });
    }

    private void Deactivate()
    {
        if (Selected() is not { } user)
            return;
        if (!ConfirmDialog.Ask(FindForm()!, "Deactivate user", $"Deactivate {user.Username}? They will not be able to sign in.", "Deactivate"))
            return;
        UiAsync.Run(async () =>
        {
            await ServiceLocator.Users.DeactivateAsync(user.Id, _session);
            Reload();
        });
    }

    private void Unlock()
    {
        if (Selected() is not { } user)
            return;
        UiAsync.Run(async () =>
        {
            await ServiceLocator.Users.UnlockAsync(user.Id, _session);
            ToastHost.Success("Account unlocked.");
            Reload();
        });
    }

    private void ResetPassword()
    {
        if (Selected() is not { } user)
            return;
        using var overlay = ConfirmDialog.CreateOverlay(FindForm()!);
        overlay?.Show();
        using var dialog = new UserEditDialog(user.Username, user.FullName, user.Role, true);
        var result = dialog.ShowDialog(this);
        overlay?.Close();
        if (result != DialogResult.OK)
            return;
        UiAsync.Run(async () =>
        {
            await ServiceLocator.Users.ResetPasswordAsync(user.Id, dialog.PasswordValue, _session);
            ToastHost.Success("Password reset.");
        });
    }

    private RoundButton Btn(string text, bool primary, Action click)
    {
        var button = new RoundButton
        {
            Text = text,
            Width = Math.Max(100, text.Length * 10),
            Height = 40,
            IsPrimary = primary,
            FillColor = primary ? Theme.Accent : Theme.NavGlass
        };
        button.Click += (_, _) => click();
        return button;
    }
}
