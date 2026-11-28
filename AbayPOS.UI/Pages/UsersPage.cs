using AbayPOS.Domain.Entities;
using AbayPOS.Domain.Enums;
using AbayPOS.Domain.Models;
using AbayPOS.UI.Controls;
using AbayPOS.UI.Dialogs;
using AbayPOS.UI.Infrastructure;
using AbayPOS.UI.Theming;

namespace AbayPOS.UI.Pages;

public sealed partial class UsersPage : UserControl
{
    private readonly Session _session;
    private IReadOnlyList<User> _users = [];

    public UsersPage(Session session)
    {
        _session = session;
        InitializeComponent();
        btnAdd.Click += (_, _) => Create();
        btnEdit.Click += (_, _) => Edit();
        btnDeactivate.Click += (_, _) => Deactivate();
        btnUnlock.Click += (_, _) => Unlock();
        btnResetPassword.Click += (_, _) => ResetPassword();
    }

    protected override void OnLoad(EventArgs e)
    {
        base.OnLoad(e);
        if (DesignTime.IsActive(this)) return;
        Theme.ApplyRuntime(this);
        ConfigureGrid();
        if (ServiceLocator.IsConfigured) Reload();
    }

    private void ConfigureGrid()
    {
        GridTheme.Apply(dgvUsers);
        dgvUsers.Columns.Clear();
        dgvUsers.Columns.Add("User", "Username");
        dgvUsers.Columns.Add("Name", "Name");
        dgvUsers.Columns.Add("Role", "Role");
        dgvUsers.Columns.Add("Locked", "Locked");
        dgvUsers.Columns.Add("Active", "Active");
        GridTheme.Balance(dgvUsers, "Name");
    }

    public void Reload() => UiAsync.Run(async () =>
    {
        _users = await ServiceLocator.Users.GetAllAsync();
        dgvUsers.Rows.Clear();
        foreach (var user in _users)
            dgvUsers.Rows.Add(user.Username, user.FullName, user.Role, user.IsLocked ? "Yes" : "No", user.IsActive ? "Yes" : "No");
    });

    private User? Selected() => dgvUsers.CurrentRow is null ? null : _users.ElementAtOrDefault(dgvUsers.CurrentRow.Index);

    private void Create()
    {
        using var overlay = ConfirmDialog.CreateOverlay(FindForm()!);
        overlay?.Show();
        using var dialog = new UserEditDialog(null, null, UserRole.Cashier, true);
        var result = dialog.ShowDialog(this);
        overlay?.Close();
        if (result != DialogResult.OK) return;
        UiAsync.Run(async () => { await ServiceLocator.Users.CreateAsync(dialog.UsernameValue, dialog.FullNameValue, dialog.PasswordValue, dialog.RoleValue, _session); Reload(); });
    }

    private void Edit()
    {
        if (Selected() is not { } user) return;
        using var overlay = ConfirmDialog.CreateOverlay(FindForm()!);
        overlay?.Show();
        using var dialog = new UserEditDialog(user.Username, user.FullName, user.Role, false);
        var result = dialog.ShowDialog(this);
        overlay?.Close();
        if (result != DialogResult.OK) return;
        UiAsync.Run(async () => { await ServiceLocator.Users.UpdateAsync(user.Id, dialog.FullNameValue, dialog.RoleValue, user.IsActive, _session); Reload(); });
    }

    private void Deactivate()
    {
        if (Selected() is not { } user) return;
        if (!ConfirmDialog.Ask(FindForm()!, "Deactivate user", $"Deactivate {user.Username}? They will not be able to sign in.", "Deactivate")) return;
        UiAsync.Run(async () => { await ServiceLocator.Users.DeactivateAsync(user.Id, _session); Reload(); });
    }

    private void Unlock()
    {
        if (Selected() is not { } user) return;
        UiAsync.Run(async () => { await ServiceLocator.Users.UnlockAsync(user.Id, _session); ToastHost.Success("Account unlocked."); Reload(); });
    }

    private void ResetPassword()
    {
        if (Selected() is not { } user) return;
        using var overlay = ConfirmDialog.CreateOverlay(FindForm()!);
        overlay?.Show();
        using var dialog = new UserEditDialog(user.Username, user.FullName, user.Role, true);
        var result = dialog.ShowDialog(this);
        overlay?.Close();
        if (result != DialogResult.OK) return;
        UiAsync.Run(async () => { await ServiceLocator.Users.ResetPasswordAsync(user.Id, dialog.PasswordValue, _session); ToastHost.Success("Password reset."); });
    }
}
