using ProjectLiberty.Abstractions.Events;
using ProjectLiberty.Abstractions.Logging;

namespace ProjectLiberty.EventBus;

/// <summary>
/// Event bus implementation with priority support.
/// </summary>
public class EventBus : IEventBus, IDisposable
{
    private readonly Dictionary<Type, List<EventHandlerRegistration>> _handlers = new Dictionary<Type, List<EventHandlerRegistration>>();
    private readonly object _sync = new object();
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
        if (handler == null) throw new ArgumentNullException(nameof(handler));
        ThrowIfDisposed();
        var eventType = typeof(TEvent);
        lock (_sync)
        {
            if (!_handlers.ContainsKey(eventType))
                _handlers[eventType] = new List<EventHandlerRegistration>();
            _handlers[eventType].Add(new EventHandlerRegistration(handler, filter, priority));
            _handlers[eventType].Sort((a, b) => b.Priority.CompareTo(a.Priority));
        }
        _logger.LogDebug($"Subscribed to event: {eventType.Name} with priority: {priority}");
    }

    public void Unsubscribe<TEvent>(IEventHandler<TEvent> handler) where TEvent : IEvent
    {
        var eventType = typeof(TEvent);

        lock (_sync)
        {
            if (_handlers.TryGetValue(eventType, out var handlers))
            {
                handlers.RemoveAll(h => ReferenceEquals(h.Handler, handler));
                if (handlers.Count == 0) _handlers.Remove(eventType);
            }
        }
        _logger.LogDebug($"Unsubscribed from event: {eventType.Name}");
    }

    public void Publish<TEvent>(TEvent @event) where TEvent : IEvent
    {
        if (@event == null) throw new ArgumentNullException(nameof(@event));
        ThrowIfDisposed();
        var eventType = typeof(TEvent);

        EventHandlerRegistration[] handlers;
        lock (_sync)
            handlers = _handlers.TryGetValue(eventType, out var registered)
                ? registered.ToArray()
                : Array.Empty<EventHandlerRegistration>();
        if (handlers.Length == 0)
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
        if (@event == null) throw new ArgumentNullException(nameof(@event));
        ThrowIfDisposed();
        var eventType = typeof(TEvent);

        EventHandlerRegistration[] handlers;
        lock (_sync)
            handlers = _handlers.TryGetValue(eventType, out var registered)
                ? registered.ToArray()
                : Array.Empty<EventHandlerRegistration>();
        if (handlers.Length == 0)
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
                await handler.HandleAsync(@event).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error handling event: {eventType.Name}");
            }
        }
    }

    public void Dispose()
    {
        if (_disposed)
            return;

        lock (_sync) _handlers.Clear();
        _disposed = true;
    }

    private void ThrowIfDisposed()
    {
        if (_disposed) throw new ObjectDisposedException(nameof(EventBus));
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
