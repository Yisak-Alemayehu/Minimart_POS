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

        // Phase 1 composition root: UI still runs stand-alone.
        // Phase 2+ replaces this with ServiceLocator.Build(...) and LoginForm.
        Application.Run(new CashierForm());
    }
}
