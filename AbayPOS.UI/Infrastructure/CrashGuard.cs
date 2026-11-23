using AbayPOS.Domain.Logging;

namespace AbayPOS.UI.Infrastructure;

/// <summary>Last-resort handlers so the POS never dies on an uncaught exception.</summary>
internal static class CrashGuard
{
    public static IAppLogger? Logger { get; set; }

    public static void Handle(Exception? exception, string source)
    {
        var message = exception?.Message ?? "An unexpected error occurred.";
        Logger?.Error($"Unhandled ({source}): {message}", exception);
        try
        {
            MessageBox.Show(
                "Something went wrong. The sale was not changed.\n\n" + message,
                "AbayPOS",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
        }
        catch (Exception)
        {
            // If even the message box fails, swallowing is better than a process crash.
        }
    }
}
