using AbayPOS.Domain.Exceptions;
using AbayPOS.UI.Controls;

namespace AbayPOS.UI.Infrastructure;

/// <summary>Runs service calls from WinForms without blocking or using <c>.Result</c>.</summary>
internal static class UiAsync
{
    public static async void Run(Func<Task> work, Control? busy = null, Action<Exception>? onError = null)
    {
        var previous = busy is { Enabled: true };
        try
        {
            if (busy is not null)
                busy.Enabled = false;
            await work().ConfigureAwait(true);
        }
        catch (Exception ex)
        {
            ServiceLocator.Logger.Error(ex.Message, ex);
            if (onError is not null)
                onError(ex);
            else
                ToastHost.Show(Message(ex), ToastKind.Error);
        }
        finally
        {
            if (busy is not null && !busy.IsDisposed && previous)
                busy.Enabled = true;
        }
    }

    public static string Message(Exception ex) => ex switch
    {
        InsufficientStockException or LoginFailedException or DomainValidationException or UnauthorizedAccessException
            => ex.Message,
        _ => "Something went wrong. Nothing was saved. If it keeps happening, check logs/app.log."
    };
}
