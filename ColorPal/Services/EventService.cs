using System.Runtime.ExceptionServices;

namespace ColorPal.Services;

public sealed class EventService<T>
{
    public event Func<T, Task>? OnEvent;

    /// <summary>
    /// Publishes the event to every subscriber in subscription order, awaiting each one before the next.
    /// </summary>
    public async Task PublishAsync(T data)
    {
        List<Exception>? exceptions = null;

        foreach (Func<T, Task> callback in Delegate.EnumerateInvocationList(OnEvent))
        {
            try
            {
                await callback(data);
            }
            catch (Exception exception)
            {
                exceptions ??= [];
                exceptions.Add(exception);
            }
        }

        if (exceptions is null)
        {
            return;
        }

        if (exceptions.Count == 1)
        {
            ExceptionDispatchInfo.Throw(exceptions[0]);
        }

        throw new AggregateException(exceptions);
    }

    public void Subscribe(Func<T, Task> callback) =>
        OnEvent += callback;

    public void Unsubscribe(Func<T, Task> callback) =>
        OnEvent -= callback;
}
