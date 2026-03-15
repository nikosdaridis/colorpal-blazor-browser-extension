using ColorPal.Common;

namespace ColorPal.Services;

public sealed class EventAggregator<T>
{
    private readonly Dictionary<string, EventService<T>> _eventServices = [];

    /// <summary>
    /// Gets event service for given event type
    /// </summary>
    public EventService<T> GetService(Event eventType) =>
        _eventServices.TryGetValue(eventType.ToString(), out EventService<T>? service)
        ? service
        : _eventServices[eventType.ToString()] = new();
}
