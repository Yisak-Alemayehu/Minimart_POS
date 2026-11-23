using AbayPOS.Business.Composition;
using AbayPOS.Business.Logging;
using AbayPOS.Business.Security;
using AbayPOS.Business.Services;
using AbayPOS.Domain.Exceptions;

namespace AbayPOS.Tests;

public sealed class PasswordHasherTests
{
    [Fact]
    public void Hash_then_verify_succeeds()
    {
        var hasher = new PasswordHasher();
        var (hash, salt) = hasher.Hash("admin123");
        Assert.True(hasher.Verify("admin123", hash, salt));
        Assert.False(hasher.Verify("wrong", hash, salt));
        Assert.NotEqual(hash, salt);
    }

    [Fact]
    public async Task Login_locks_after_five_failures()
    {
        var host = await TestHost.CreateAsync();
        for (var i = 0; i < AuthService.MaxFailedAttempts - 1; i++)
        {
            var ex = await Assert.ThrowsAsync<LoginFailedException>(() => host.Auth.LoginAsync("cashier", "nope"));
            Assert.False(ex.IsLocked);
        }

        var locked = await Assert.ThrowsAsync<LoginFailedException>(() => host.Auth.LoginAsync("cashier", "nope"));
        Assert.True(locked.IsLocked);

        var stillLocked = await Assert.ThrowsAsync<LoginFailedException>(() => host.Auth.LoginAsync("cashier", "cashier123"));
        Assert.True(stillLocked.IsLocked);
    }

    [Fact]
    public async Task Successful_login_resets_failures()
    {
        var host = await TestHost.CreateAsync();
        await Assert.ThrowsAsync<LoginFailedException>(() => host.Auth.LoginAsync("admin", "bad"));
        var session = await host.Auth.LoginAsync("admin", "admin123");
        Assert.Equal("Store Manager", session.User.FullName);
        Assert.True(session.IsManager);
    }
}

internal static class TestHost
{
    public static Task<AppHost> CreateAsync()
    {
        var folder = Path.Combine(Path.GetTempPath(), "abaypos-tests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(folder);
        var db = Path.Combine(folder, "abaypos.db");
        var log = Path.Combine(folder, "app.log");
        return AppHost.StartAsync(db, new FileLogger(log));
    }
}
