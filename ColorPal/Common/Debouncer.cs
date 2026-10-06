namespace ColorPal.Common;

public sealed class Debouncer(TimeSpan delay, Func<Exception, Task> handleExceptionAsync) : IDisposable
{
    private CancellationTokenSource? _pendingActionCancellation;

    public void Debounce(Func<Task> action)
    {
        CancelPendingAction();
        _pendingActionCancellation = new CancellationTokenSource();
        _ = RunAfterDelayAsync(action, _pendingActionCancellation.Token);
    }

    public void Dispose() =>
        CancelPendingAction();

    private void CancelPendingAction()
    {
        _pendingActionCancellation?.Cancel();
        _pendingActionCancellation?.Dispose();
        _pendingActionCancellation = null;
    }

    private async Task RunAfterDelayAsync(Func<Task> action, CancellationToken cancellationToken)
    {
        await Task.Delay(delay, cancellationToken)
            .ConfigureAwait(ConfigureAwaitOptions.ContinueOnCapturedContext | ConfigureAwaitOptions.SuppressThrowing);

        if (cancellationToken.IsCancellationRequested)
        {
            return;
        }

        try
        {
            await action();
        }
        catch (Exception exception)
        {
            await handleExceptionAsync(exception);
        }
    }
}
