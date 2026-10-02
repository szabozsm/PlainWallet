using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls;

namespace PlainWallet.Services;

public static class ExceptionReporter
{
    private static readonly object PendingLock = new();
    private static readonly SemaphoreSlim AlertLock = new(1, 1);
    private static (string Title, Exception Exception)? _pendingException;

    public static void Report(Exception exception, string title)
    {
        var page = Application.Current?.Windows.FirstOrDefault(window => window.Page is not null)?.Page;
        if (page is null)
        {
            lock (PendingLock)
                _pendingException ??= (title, exception);
            return;
        }

        _ = ShowAlertAsync(page, title, exception);
    }

    public static (string Title, Exception Exception)? TakePending()
    {
        lock (PendingLock)
        {
            var exception = _pendingException;
            _pendingException = null;
            return exception;
        }
    }

    private static async Task ShowAlertAsync(Page page, string title, Exception exception)
    {
        try
        {
            await AlertLock.WaitAsync();
            await MainThread.InvokeOnMainThreadAsync(
                () => page.DisplayAlertAsync(title, exception.ToString(), "OK"));
        }
        catch
        {
            lock (PendingLock)
                _pendingException ??= (title, exception);
        }
        finally
        {
            if (AlertLock.CurrentCount == 0)
                AlertLock.Release();
        }
    }
}