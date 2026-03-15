namespace ColorPal.Services;

public sealed class EventService<T>
{
    public event Func<T, Task>? OnEvent;

    /// <summary>
    /// Publishes the event
    /// </summary>
    public async Task PublishAsync(T data) =>
        await (OnEvent?.Invoke(data) ?? Task.CompletedTask);

    /// <summary>
    /// Subscribes to the event
    /// </summary>
    public void Subscribe(Func<T, Task> callback) =>
        OnEvent += callback;

    /// <summary>
    /// Unsubscribes from the event
    /// </summary>
    public void Unsubscribe(Func<T, Task> callback) =>
        OnEvent -= callback;
}
