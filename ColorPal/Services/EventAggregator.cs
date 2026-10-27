using ColorPal.Common;

namespace ColorPal.Services;

public sealed class EventAggregator<T>
{
    private readonly Dictionary<string, EventService<T>> _eventServiceMap = [];

    /// <summary>
    /// Gets event service for given event type.
    /// </summary>
    public EventService<T> GetService(Event eventType)
    {
        string key = eventType.ToString();
        return _eventServiceMap.TryGetValue(key, out EventService<T>? service)
            ? service
            : _eventServiceMap[key] = new();
    }
}
