namespace ProjectLiberty.Abstractions.Events;

/// <summary>
/// Event bus interface - manages event subscription and publishing.
/// </summary>
public interface IEventBus
{
    /// <summary>
    /// Subscribes to an event.
    /// </summary>
    /// <typeparam name="TEvent">Event type.</typeparam>
    /// <param name="handler">Event handler.</param>
    void Subscribe<TEvent>(IEventHandler<TEvent> handler) where TEvent : IEvent;
    
    /// <summary>
    /// Subscribes to an event with a filter.
    /// </summary>
    /// <typeparam name="TEvent">Event type.</typeparam>
    /// <param name="handler">Event handler.</param>
    /// <param name="filter">Event filter.</param>
    void Subscribe<TEvent>(IEventHandler<TEvent> handler, IEventFilter<TEvent> filter) where TEvent : IEvent;
    
    /// <summary>
    /// Unsubscribes from an event.
    /// </summary>
    /// <typeparam name="TEvent">Event type.</typeparam>
    /// <param name="handler">Event handler.</param>
    void Unsubscribe<TEvent>(IEventHandler<TEvent> handler) where TEvent : IEvent;
    
    /// <summary>
    /// Publishes an event.
    /// </summary>
    /// <typeparam name="TEvent">Event type.</typeparam>
    /// <param name="event">Event to publish.</param>
    void Publish<TEvent>(TEvent @event) where TEvent : IEvent;
    
    /// <summary>
    /// Publishes an event asynchronously.
    /// </summary>
    /// <typeparam name="TEvent">Event type.</typeparam>
    /// <param name="event">Event to publish.</param>
    /// <returns>Task representing the async operation.</returns>
    Task PublishAsync<TEvent>(TEvent @event) where TEvent : IEvent;
}

/// <summary>
/// Base event interface.
/// </summary>
public interface IEvent
{
    /// <summary>
    /// Gets the event timestamp.
    /// </summary>
    DateTime Timestamp { get; }
    
    /// <summary>
    /// Gets the event ID.
    /// </summary>
    Guid EventId { get; }
}

/// <summary>
/// Event handler interface.
/// </summary>
/// <typeparam name="TEvent">Event type.</typeparam>
public interface IEventHandler<in TEvent> where TEvent : IEvent
{
    /// <summary>
    /// Handles the event.
    /// </summary>
    /// <param name="event">Event to handle.</param>
    Task HandleAsync(TEvent @event);
}

/// <summary>
/// Event filter interface.
/// </summary>
/// <typeparam name="TEvent">Event type.</typeparam>
public interface IEventFilter<in TEvent> where TEvent : IEvent
{
    /// <summary>
    /// Determines whether the event should be handled.
    /// </summary>
    /// <param name="event">Event to check.</param>
    /// <returns>True if should handle, false otherwise.</returns>
    bool ShouldHandle(TEvent @event);
}

/// <summary>
/// Base event class.
/// </summary>
public abstract class BaseEvent : IEvent
{
    public DateTime Timestamp { get; } = DateTime.UtcNow;
    public Guid EventId { get; } = Guid.NewGuid();
}
