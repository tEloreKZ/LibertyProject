using ProjectLiberty.Abstractions.Events;
using ProjectLiberty.Abstractions.Logging;

namespace ProjectLiberty.EventBus;

/// <summary>
/// Event bus implementation with priority support.
/// </summary>
public class EventBus : IEventBus, IDisposable
{
    private readonly Dictionary<Type, List<EventHandlerRegistration>> _handlers = new Dictionary<Type, List<EventHandlerRegistration>>();
    private readonly ILogger _logger;
    private bool _disposed;

    public EventBus(ILogger logger)
    {
        _logger = logger;
    }

    public void Subscribe<TEvent>(IEventHandler<TEvent> handler) where TEvent : IEvent
    {
        Subscribe(handler, null, EventPriority.Normal);
    }

    public void Subscribe<TEvent>(IEventHandler<TEvent> handler, IEventFilter<TEvent>? filter) where TEvent : IEvent
    {
        Subscribe(handler, filter, EventPriority.Normal);
    }

    public void Subscribe<TEvent>(IEventHandler<TEvent> handler, EventPriority priority) where TEvent : IEvent
    {
        Subscribe(handler, null, priority);
    }

    public void Subscribe<TEvent>(IEventHandler<TEvent> handler, IEventFilter<TEvent>? filter, EventPriority priority) where TEvent : IEvent
    {
        var eventType = typeof(TEvent);

        if (!_handlers.ContainsKey(eventType))
        {
            _handlers[eventType] = new List<EventHandlerRegistration>();
        }

        _handlers[eventType].Add(new EventHandlerRegistration(handler, filter, priority));
        _handlers[eventType].Sort((a, b) => b.Priority.CompareTo(a.Priority)); // Sort by priority (highest first)
        _logger.LogDebug($"Subscribed to event: {eventType.Name} with priority: {priority}");
    }

    public void Unsubscribe<TEvent>(IEventHandler<TEvent> handler) where TEvent : IEvent
    {
        var eventType = typeof(TEvent);

        if (_handlers.TryGetValue(eventType, out var handlers))
        {
            handlers.RemoveAll(h => ReferenceEquals(h.Handler, handler));
            _logger.LogDebug($"Unsubscribed from event: {eventType.Name}");
        }
    }

    public void Publish<TEvent>(TEvent @event) where TEvent : IEvent
    {
        var eventType = typeof(TEvent);

        if (!_handlers.TryGetValue(eventType, out var handlers) || handlers.Count == 0)
        {
            _logger.LogDebug($"No handlers for event: {eventType.Name}");
            return;
        }

        foreach (var registration in handlers)
        {
            if (registration.Filter != null)
            {
                var filter = (IEventFilter<TEvent>)registration.Filter;
                if (!filter.ShouldHandle(@event))
                {
                    continue;
                }
            }

            try
            {
                var handler = (IEventHandler<TEvent>)registration.Handler;
                handler.HandleAsync(@event).GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error handling event: {eventType.Name}");
            }
        }
    }

    public async Task PublishAsync<TEvent>(TEvent @event) where TEvent : IEvent
    {
        var eventType = typeof(TEvent);

        if (!_handlers.TryGetValue(eventType, out var handlers) || handlers.Count == 0)
        {
            _logger.LogDebug($"No handlers for event: {eventType.Name}");
            return;
        }

        var tasks = new List<Task>();

        foreach (var registration in handlers)
        {
            if (registration.Filter != null)
            {
                var filter = (IEventFilter<TEvent>)registration.Filter;
                if (!filter.ShouldHandle(@event))
                {
                    continue;
                }
            }

            tasks.Add(Task.Run(async () =>
            {
                try
                {
                    var handler = (IEventHandler<TEvent>)registration.Handler;
                    await handler.HandleAsync(@event);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Error handling event: {eventType.Name}");
                }
            }));
        }

        await Task.WhenAll(tasks);
    }

    public void Dispose()
    {
        if (_disposed)
            return;

        _handlers.Clear();
        _disposed = true;
    }

    private class EventHandlerRegistration
    {
        public object Handler { get; }
        public object? Filter { get; }
        public EventPriority Priority { get; }

        public EventHandlerRegistration(object handler, object? filter, EventPriority priority)
        {
            Handler = handler;
            Filter = filter;
            Priority = priority;
        }
    }
}
