using ColorPal.Common;

namespace ColorPal.Services;

public sealed class EventAggregator<T>
{
    private readonly Dictionary<Event, EventService<T>> _eventServiceMap = [];

    /// <summary>
    /// Gets event service for given event type.
    /// </summary>
    public EventService<T> GetService(Event eventType)
    {
        return _eventServiceMap.TryGetValue(eventType, out EventService<T>? service)
            ? service
            : _eventServiceMap[eventType] = new();
    }
}
