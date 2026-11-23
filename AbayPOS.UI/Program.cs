using AbayPOS.Business.Composition;
using AbayPOS.Domain.Models;
using AbayPOS.UI.Forms;
using AbayPOS.UI.Infrastructure;

namespace AbayPOS.UI;

internal static class Program
{
    [STAThread]
    private static void Main()
    {
        ApplicationConfiguration.Initialize();
        Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);
        Application.ThreadException += (_, e) => CrashGuard.Handle(e.Exception, "UI thread");
        AppDomain.CurrentDomain.UnhandledException += (_, e) =>
            CrashGuard.Handle(e.ExceptionObject as Exception ?? new Exception(e.ExceptionObject?.ToString()), "AppDomain");
        TaskScheduler.UnobservedTaskException += (_, e) =>
        {
            CrashGuard.Handle(e.Exception, "Task");
            e.SetObserved();
        };

        Application.Run(new PosApplicationContext());
    }
}

internal sealed class PosApplicationContext : ApplicationContext
{
    public PosApplicationContext() => _ = BootAsync();

    private async Task BootAsync()
    {
        try
        {
            var host = await AppHost.StartAsync().ConfigureAwait(true);
            ServiceLocator.Configure(host);
            ShowLogin();
        }
        catch (Exception ex)
        {
            CrashGuard.Handle(ex, "startup");
            ExitThread();
        }
    }

    private void ShowLogin()
    {
        var login = new LoginForm();
        login.SessionStarted += (_, session) => OpenShell(login, session);
        MainForm = login;
        login.Show();
    }

    private void OpenShell(LoginForm login, Session session)
    {
        Form shell = session.IsManager ? new ManagerForm(session) : new CashierForm(session);
        shell.FormClosed += (_, _) =>
        {
            if (shell.DialogResult == DialogResult.Retry)
                ShowLogin();
            else
                ExitThread();
        };
        MainForm = shell;
        login.Hide();
        shell.Show();
        login.Close();
    }
}
