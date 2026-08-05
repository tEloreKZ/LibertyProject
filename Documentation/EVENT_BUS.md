# Event Bus

**Event System Architecture and Documentation**

---

## Table of Contents

1. [Overview](#overview)
2. [Design Principles](#design-principles)
3. [Event Definitions](#event-definitions)
4. [Event Bus Interface](#event-bus-interface)
5. [Event Handlers](#event-handlers)
6. [Event Filtering](#event-filtering)
7. [Usage Examples](#usage-examples)
8. [Best Practices](#best-practices)
9. [Performance Considerations](#performance-considerations)

---

## Overview

The Event Bus is the central communication mechanism in Project Liberty. All systems communicate through events, eliminating direct dependencies between modules. This architecture ensures:

- **Loose coupling**: Systems don't know about each other
- **Modularity**: Systems can be added/removed without affecting others
- **Testability**: Events can be easily tested and mocked
- **Extensibility**: New events can be added without modifying existing code

### Architecture

```
┌──────────────┐     Publish     ┌──────────────┐
│   System A   │ ──────────────→ │  Event Bus   │
└──────────────┘                 └──────────────┘
                                         ↓
┌──────────────┐     Subscribe    ┌──────────────┐
│   System B   │ ←────────────── │  Event Bus   │
└──────────────┘                 └──────────────┘
                                         ↓
┌──────────────┐     Subscribe    ┌──────────────┐
│   System C   │ ←────────────── │  Event Bus   │
└──────────────┘                 └──────────────┘
```

---

## Design Principles

### 1. No Direct Dependencies

**FORBIDDEN**: Direct method calls between systems

```csharp
// FORBIDDEN - Direct dependency
public class TrafficStopService
{
    private readonly DispatchService _dispatchService;
    
    public void StartTrafficStop()
    {
        // BAD - Direct call
        _dispatchService.SendBackupRequest();
    }
}
```

**REQUIRED**: Communication through events

```csharp
// REQUIRED - Event-based communication
public class TrafficStopService
{
    private readonly IEventBus _eventBus;
    
    public void StartTrafficStop()
    {
        // GOOD - Event-based
        _eventBus.Publish(new BackupRequestedEvent
        {
            Location = _currentPosition,
            Priority = BackupPriority.High
        });
    }
}
```

### 2. Event Immutability

All events must be immutable:

```csharp
// GOOD - Immutable event
public class PlayerSpawnedEvent : IEvent
{
    public int PlayerHandle { get; }
    public Vector3 Position { get; }
    public DateTime SpawnTime { get; }
    
    public PlayerSpawnedEvent(int playerHandle, Vector3 position)
    {
        PlayerHandle = playerHandle;
        Position = position;
        SpawnTime = DateTime.UtcNow;
    }
}

// BAD - Mutable event
public class PlayerSpawnedEvent : IEvent
{
    public int PlayerHandle { get; set; }  // BAD
    public Vector3 Position { get; set; }  // BAD
}
```

### 3. Event Naming

Events use past tense for things that happened:

```csharp
// GOOD - Past tense for completed actions
public class PlayerSpawnedEvent : IEvent { }
public class PlayerDiedEvent : IEvent { }
public class CalloutStartedEvent : IEvent { }
public class CalloutFinishedEvent : IEvent { }

// GOOD - Present tense for ongoing states
public class PlayerHealthChangedEvent : IEvent { }
public class VehicleSpeedChangedEvent : IEvent { }
```

---

## Event Definitions

### Core Events

```csharp
namespace ProjectLiberty.Events
{
    /// <summary>
    /// Base interface for all events.
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
    /// Base class for all events.
    /// </summary>
    public abstract class BaseEvent : IEvent
    {
        public DateTime Timestamp { get; }
        public Guid EventId { get; }
        
        protected BaseEvent()
        {
            Timestamp = DateTime.UtcNow;
            EventId = Guid.NewGuid();
        }
    }
}
```

### Player Events

```csharp
namespace ProjectLiberty.Events
{
    /// <summary>
    /// Event raised when a player spawns.
    /// </summary>
    public class PlayerSpawnedEvent : BaseEvent
    {
        public int PlayerHandle { get; }
        public Vector3 Position { get; }
        
        public PlayerSpawnedEvent(int playerHandle, Vector3 position)
        {
            PlayerHandle = playerHandle;
            Position = position;
        }
    }
    
    /// <summary>
    /// Event raised when a player dies.
    /// </summary>
    public class PlayerDiedEvent : BaseEvent
    {
        public int PlayerHandle { get; }
        public Vector3 Position { get; }
        public int KillerHandle { get; }
        public DeathReason Reason { get; }
        
        public PlayerDiedEvent(int playerHandle, Vector3 position, int killerHandle, DeathReason reason)
        {
            PlayerHandle = playerHandle;
            Position = position;
            KillerHandle = killerHandle;
            Reason = reason;
        }
    }
    
    /// <summary>
    /// Event raised when player health changes.
    /// </summary>
    public class PlayerHealthChangedEvent : BaseEvent
    {
        public int PlayerHandle { get; }
        public int OldHealth { get; }
        public int NewHealth { get; }
        
        public PlayerHealthChangedEvent(int playerHandle, int oldHealth, int newHealth)
        {
            PlayerHandle = playerHandle;
            OldHealth = oldHealth;
            NewHealth = newHealth;
        }
    }
    
    /// <summary>
    /// Event raised when player armor changes.
    /// </summary>
    public class PlayerArmorChangedEvent : BaseEvent
    {
        public int PlayerHandle { get; }
        public int OldArmor { get; }
        public int NewArmor { get; }
        
        public PlayerArmorChangedEvent(int playerHandle, int oldArmor, int newArmor)
        {
            PlayerHandle = playerHandle;
            OldArmor = oldArmor;
            NewArmor = newArmor;
        }
    }
    
    /// <summary>
    /// Event raised when a weapon is drawn.
    /// </summary>
    public class WeaponDrawnEvent : BaseEvent
    {
        public int PlayerHandle { get; }
        public WeaponHash Weapon { get; }
        
        public WeaponDrawnEvent(int playerHandle, WeaponHash weapon)
        {
            PlayerHandle = playerHandle;
            Weapon = weapon;
        }
    }
}
```

### Vehicle Events

```csharp
namespace ProjectLiberty.Events
{
    /// <summary>
    /// Event raised when a vehicle is created.
    /// </summary>
    public class VehicleCreatedEvent : BaseEvent
    {
        public int VehicleHandle { get; }
        public VehicleModel Model { get; }
        public Vector3 Position { get; }
        
        public VehicleCreatedEvent(int vehicleHandle, VehicleModel model, Vector3 position)
        {
            VehicleHandle = vehicleHandle;
            Model = model;
            Position = position;
        }
    }
    
    /// <summary>
    /// Event raised when a vehicle is destroyed.
    /// </summary>
    public class VehicleDestroyedEvent : BaseEvent
    {
        public int VehicleHandle { get; }
        public Vector3 Position { get; }
        public DestructionReason Reason { get; }
        
        public VehicleDestroyedEvent(int vehicleHandle, Vector3 position, DestructionReason reason)
        {
            VehicleHandle = vehicleHandle;
            Position = position;
            Reason = reason;
        }
    }
    
    /// <summary>
    /// Event raised when a vehicle is stolen.
    /// </summary>
    public class VehicleStolenEvent : BaseEvent
    {
        public int VehicleHandle { get; }
        public int ThiefHandle { get; }
        public Vector3 Position { get; }
        
        public VehicleStolenEvent(int vehicleHandle, int thiefHandle, Vector3 position)
        {
            VehicleHandle = vehicleHandle;
            ThiefHandle = thiefHandle;
            Position = position;
        }
    }
    
    /// <summary>
    /// Event raised when a vehicle is stopped.
    /// </summary>
    public class VehicleStoppedEvent : BaseEvent
    {
        public int VehicleHandle { get; }
        public Vector3 Position { get; }
        public float Speed { get; }
        
        public VehicleStoppedEvent(int vehicleHandle, Vector3 position, float speed)
        {
            VehicleHandle = vehicleHandle;
            Position = position;
            Speed = speed;
        }
    }
}
```

### Callout Events

```csharp
namespace ProjectLiberty.Events
{
    /// <summary>
    /// Event raised when a callout starts.
    /// </summary>
    public class CalloutStartedEvent : BaseEvent
    {
        public string CalloutId { get; }
        public string CalloutName { get; }
        public Vector3 Position { get; }
        public CalloutPriority Priority { get; }
        
        public CalloutStartedEvent(string calloutId, string calloutName, Vector3 position, CalloutPriority priority)
        {
            CalloutId = calloutId;
            CalloutName = calloutName;
            Position = position;
            Priority = priority;
        }
    }
    
    /// <summary>
    /// Event raised when a callout finishes.
    /// </summary>
    public class CalloutFinishedEvent : BaseEvent
    {
        public string CalloutId { get; }
        public CalloutResult Result { get; }
        public TimeSpan Duration { get; }
        
        public CalloutFinishedEvent(string calloutId, CalloutResult result, TimeSpan duration)
        {
            CalloutId = calloutId;
            Result = result;
            Duration = duration;
        }
    }
    
    /// <summary>
    /// Event raised when a callout is accepted.
    /// </summary>
    public class CalloutAcceptedEvent : BaseEvent
    {
        public string CalloutId { get; }
        public int PlayerHandle { get; }
        
        public CalloutAcceptedEvent(string calloutId, int playerHandle)
        {
            CalloutId = calloutId;
            PlayerHandle = playerHandle;
        }
    }
}
```

### Dispatch Events

```csharp
namespace ProjectLiberty.Events
{
    /// <summary>
    /// Event raised when backup is requested.
    /// </summary>
    public class BackupRequestedEvent : BaseEvent
    {
        public Vector3 Location { get; }
        public BackupPriority Priority { get; }
        public BackupType Type { get; }
        public int RequestingUnitHandle { get; }
        
        public BackupRequestedEvent(Vector3 location, BackupPriority priority, BackupType type, int requestingUnitHandle)
        {
            Location = location;
            Priority = priority;
            Type = type;
            RequestingUnitHandle = requestingUnitHandle;
        }
    }
    
    /// <summary>
    /// Event raised when backup arrives.
    /// </summary>
    public class BackupArrivedEvent : BaseEvent
    {
        public int UnitHandle { get; }
        public Vector3 Location { get; }
        
        public BackupArrivedEvent(int unitHandle, Vector3 location)
        {
            UnitHandle = unitHandle;
            Location = location;
        }
    }
    
    /// <summary>
    /// Event raised when a dispatch message is sent.
    /// </summary>
    public class DispatchMessageEvent : BaseEvent
    {
        public string Message { get; }
        public DispatchMessageType Type { get; }
        public Vector3? Location { get; }
        
        public DispatchMessageEvent(string message, DispatchMessageType type, Vector3? location)
        {
            Message = message;
            Type = type;
            Location = location;
        }
    }
}
```

### Traffic Stop Events

```csharp
namespace ProjectLiberty.Events
{
    /// <summary>
    /// Event raised when a traffic stop starts.
    /// </summary>
    public class TrafficStopStartedEvent : BaseEvent
    {
        public string StopId { get; }
        public int VehicleHandle { get; }
        public int PlayerHandle { get; }
        public Vector3 Position { get; }
        
        public TrafficStopStartedEvent(string stopId, int vehicleHandle, int playerHandle, Vector3 position)
        {
            StopId = stopId;
            VehicleHandle = vehicleHandle;
            PlayerHandle = playerHandle;
            Position = position;
        }
    }
    
    /// <summary>
    /// Event raised when a traffic stop ends.
    /// </summary>
    public class TrafficStopEndedEvent : BaseEvent
    {
        public string StopId { get; }
        public TrafficStopResult Result { get; }
        public TimeSpan Duration { get; }
        
        public TrafficStopEndedEvent(string stopId, TrafficStopResult result, TimeSpan duration)
        {
            StopId = stopId;
            Result = result;
            Duration = duration;
        }
    }
    
    /// <summary>
    /// Event raised when a traffic stop stage changes.
    /// </summary>
    public class TrafficStopStageChangedEvent : BaseEvent
    {
        public string StopId { get; }
        public TrafficStopStage OldStage { get; }
        public TrafficStopStage NewStage { get; }
        
        public TrafficStopStageChangedEvent(string stopId, TrafficStopStage oldStage, TrafficStopStage newStage)
        {
            StopId = stopId;
            OldStage = oldStage;
            NewStage = newStage;
        }
    }
}
```

### Arrest Events

```csharp
namespace ProjectLiberty.Events
{
    /// <summary>
    /// Event raised when a ped is arrested.
    /// </summary>
    public class PedArrestedEvent : BaseEvent
    {
        public int PedHandle { get; }
        public int OfficerHandle { get; }
        public Vector3 Position { get; }
        public ArrestReason Reason { get; }
        
        public PedArrestedEvent(int pedHandle, int officerHandle, Vector3 position, ArrestReason reason)
        {
            PedHandle = pedHandle;
            OfficerHandle = officerHandle;
            Position = position;
            Reason = reason;
        }
    }
    
    /// <summary>
    /// Event raised when a ped escapes.
    /// </summary>
    public class PedEscapedEvent : BaseEvent
    {
        public int PedHandle { get; }
        public Vector3 Position { get; }
        public EscapeMethod Method { get; }
        
        public PedEscapedEvent(int pedHandle, Vector3 position, EscapeMethod method)
        {
            PedHandle = pedHandle;
            Position = position;
            Method = method;
        }
    }
    
    /// <summary>
    /// Event raised when a ped surrenders.
    /// </summary>
    public class PedSurrenderedEvent : BaseEvent
    {
        public int PedHandle { get; }
        public Vector3 Position { get; }
        
        public PedSurrenderedEvent(int pedHandle, Vector3 position)
        {
            PedHandle = pedHandle;
            Position = position;
        }
    }
}
```

### Crime Events

```csharp
namespace ProjectLiberty.Events
{
    /// <summary>
    /// Event raised when a crime is committed.
    /// </summary>
    public class CrimeCommittedEvent : BaseEvent
    {
        public int SuspectHandle { get; }
        public CrimeType CrimeType { get; }
        public Vector3 Position { get; }
        public int? VictimHandle { get; }
        
        public CrimeCommittedEvent(int suspectHandle, CrimeType crimeType, Vector3 position, int? victimHandle)
        {
            SuspectHandle = suspectHandle;
            CrimeType = crimeType;
            Position = position;
            VictimHandle = victimHandle;
        }
    }
    
    /// <summary>
    /// Event raised when a shot is fired.
    /// </summary>
    public class ShotFiredEvent : BaseEvent
    {
        public int ShooterHandle { get; }
        public Vector3 Position { get; }
        public WeaponHash Weapon { get; }
        
        public ShotFiredEvent(int shooterHandle, Vector3 position, WeaponHash weapon)
        {
            ShooterHandle = shooterHandle;
            Position = position;
            Weapon = weapon;
        }
    }
}
```

---

## Event Bus Interface

```csharp
namespace ProjectLiberty.Abstractions
{
    /// <summary>
    /// Event bus interface for publishing and subscribing to events.
    /// </summary>
    public interface IEventBus
    {
        /// <summary>
        /// Publishes an event to all subscribers.
        /// </summary>
        /// <typeparam name="TEvent">Event type.</typeparam>
        /// <param name="event">Event to publish.</param>
        void Publish<TEvent>(TEvent @event) where TEvent : IEvent;
        
        /// <summary>
        /// Publishes an event asynchronously to all subscribers.
        /// </summary>
        /// <typeparam name="TEvent">Event type.</typeparam>
        /// <param name="event">Event to publish.</param>
        Task PublishAsync<TEvent>(TEvent @event) where TEvent : IEvent;
        
        /// <summary>
        /// Subscribes to events of the specified type.
        /// </summary>
        /// <typeparam name="TEvent">Event type.</typeparam>
        /// <param name="handler">Event handler.</param>
        void Subscribe<TEvent>(IEventHandler<TEvent> handler) where TEvent : IEvent;
        
        /// <summary>
        /// Subscribes to events of the specified type with a filter.
        /// </summary>
        /// <typeparam name="TEvent">Event type.</typeparam>
        /// <param name="handler">Event handler.</param>
        /// <param name="filter">Event filter.</param>
        void Subscribe<TEvent>(IEventHandler<TEvent> handler, IEventFilter<TEvent> filter) where TEvent : IEvent;
        
        /// <summary>
        /// Unsubscribes from events of the specified type.
        /// </summary>
        /// <typeparam name="TEvent">Event type.</typeparam>
        /// <param name="handler">Event handler.</param>
        void Unsubscribe<TEvent>(IEventHandler<TEvent> handler) where TEvent : IEvent;
        
        /// <summary>
        /// Gets the number of subscribers for an event type.
        /// </summary>
        /// <typeparam name="TEvent">Event type.</typeparam>
        int GetSubscriberCount<TEvent>() where TEvent : IEvent;
        
        /// <summary>
        /// Clears all subscribers.
        /// </summary>
        void Clear();
    }
}
```

---

## Event Handlers

### Handler Interface

```csharp
namespace ProjectLiberty.Abstractions
{
    /// <summary>
    /// Event handler interface.
    /// </summary>
    /// <typeparam name="TEvent">Event type.</typeparam>
    public interface IEventHandler<TEvent> where TEvent : IEvent
    {
        /// <summary>
        /// Handles the event.
        /// </summary>
        /// <param name="event">Event to handle.</param>
        void Handle(TEvent @event);
        
        /// <summary>
        /// Gets the handler priority (higher = earlier).
        /// </summary>
        int Priority { get; }
    }
}
```

### Handler Base Class

```csharp
namespace ProjectLiberty.EventBus
{
    /// <summary>
    /// Base class for event handlers.
    /// </summary>
    /// <typeparam name="TEvent">Event type.</typeparam>
    public abstract class EventHandlerBase<TEvent> : IEventHandler<TEvent> where TEvent : IEvent
    {
        public virtual int Priority => 0;
        
        public abstract void Handle(TEvent @event);
    }
}
```

### Example Handler

```csharp
namespace ProjectLiberty.Dispatch
{
    public class BackupRequestedHandler : EventHandlerBase<BackupRequestedEvent>
    {
        private readonly DispatchService _dispatchService;
        
        public BackupRequestedHandler(DispatchService dispatchService)
        {
            _dispatchService = dispatchService;
        }
        
        public override int Priority => 100; // High priority
        
        public override void Handle(BackupRequestedEvent @event)
        {
            _dispatchService.ProcessBackupRequest(@event);
        }
    }
}
```

---

## Event Filtering

### Filter Interface

```csharp
namespace ProjectLiberty.Abstractions
{
    /// <summary>
    /// Event filter interface.
    /// </summary>
    /// <typeparam name="TEvent">Event type.</typeparam>
    public interface IEventFilter<TEvent> where TEvent : IEvent
    {
        /// <summary>
        /// Determines if the event should be handled.
        /// </summary>
        /// <param name="event">Event to filter.</param>
        /// <returns>True if should handle, false otherwise.</returns>
        bool ShouldHandle(TEvent @event);
    }
}
```

### Example Filter

```csharp
namespace ProjectLiberty.Dispatch
{
    public class HighPriorityBackupFilter : IEventFilter<BackupRequestedEvent>
    {
        public bool ShouldHandle(BackupRequestedEvent @event)
        {
            return @event.Priority == BackupPriority.High;
        }
    }
}
```

---

## Usage Examples

### Publishing Events

```csharp
namespace ProjectLiberty.TrafficStops
{
    public class TrafficStopService
    {
        private readonly IEventBus _eventBus;
        
        public TrafficStopService(IEventBus eventBus)
        {
            _eventBus = eventBus;
        }
        
        public void StartTrafficStop(IVehicle vehicle)
        {
            var stopId = Guid.NewGuid().ToString();
            
            _eventBus.Publish(new TrafficStopStartedEvent
            {
                StopId = stopId,
                VehicleHandle = vehicle.Handle,
                PlayerHandle = _playerManager.GetLocalPlayer().Handle,
                Position = vehicle.Position
            });
        }
    }
}
```

### Subscribing to Events

```csharp
namespace ProjectLiberty.Dispatch
{
    public class DispatchService
    {
        private readonly IEventBus _eventBus;
        
        public DispatchService(IEventBus eventBus)
        {
            _eventBus = eventBus;
            
            // Subscribe to events
            _eventBus.Subscribe(new BackupRequestedHandler(this));
            _eventBus.Subscribe(new CrimeCommittedHandler(this));
        }
    }
}
```

### Unsubscribing from Events

```csharp
namespace ProjectLiberty.SomeSystem
{
    public class SomeService : IEventHandler<PlayerDiedEvent>
    {
        private readonly IEventBus _eventBus;
        
        public SomeService(IEventBus eventBus)
        {
            _eventBus = eventBus;
            _eventBus.Subscribe(this);
        }
        
        public void Handle(PlayerDiedEvent @event)
        {
            // Handle event
        }
        
        public void Shutdown()
        {
            _eventBus.Unsubscribe(this);
        }
    }
}
```

---

## Best Practices

### 1. Keep Events Simple

Events should be simple data containers:

```csharp
// GOOD - Simple event
public class PlayerDiedEvent : BaseEvent
{
    public int PlayerHandle { get; }
    public Vector3 Position { get; }
}

// BAD - Complex event with logic
public class PlayerDiedEvent : BaseEvent
{
    public int PlayerHandle { get; }
    public Vector3 Position { get; }
    
    public bool WasMurder() { /* Logic */ }  // BAD
    public void Respawn() { /* Logic */ }    // BAD
}
```

### 2. Use Specific Event Types

Create specific events rather than generic ones:

```csharp
// GOOD - Specific events
public class PlayerDiedEvent : BaseEvent { }
public class VehicleDestroyedEvent : BaseEvent { }

// BAD - Generic event
public class EntityDestroyedEvent : BaseEvent 
{
    public EntityType Type { get; }
}
```

### 3. Handle Errors Gracefully

Event handlers should not throw exceptions:

```csharp
// GOOD - Error handling
public override void Handle(PlayerDiedEvent @event)
{
    try
    {
        ProcessDeath(@event);
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Error handling PlayerDiedEvent");
    }
}

// BAD - No error handling
public override void Handle(PlayerDiedEvent @event)
{
    ProcessDeath(@event); // May throw
}
```

### 4. Unsubscribe When Done

Always unsubscribe when shutting down:

```csharp
public class Service : IDisposable
{
    private readonly IEventBus _eventBus;
    private readonly IEventHandler<PlayerDiedEvent> _handler;
    
    public Service(IEventBus eventBus)
    {
        _eventBus = eventBus;
        _handler = new PlayerDiedHandler(this);
        _eventBus.Subscribe(_handler);
    }
    
    public void Dispose()
    {
        _eventBus.Unsubscribe(_handler);
    }
}
```

---

## Performance Considerations

### 1. Event Publishing Overhead

Event publishing should be fast:

- Use struct events where possible
- Avoid complex serialization
- Use object pooling for frequent events

### 2. Handler Execution

Handlers should execute quickly:

- Avoid blocking operations
- Use async for long operations
- Consider priority ordering

### 3. Memory Management

- Unsubscribe handlers when done
- Use weak references where appropriate
- Avoid memory leaks from event subscriptions

---

## Summary

The Event Bus provides:

- **Loose coupling** between systems
- **Event-driven architecture** for modularity
- **Immutable events** for thread safety
- **Filtering** for selective handling
- **Priority ordering** for handler execution

All system communication MUST go through the Event Bus. Direct system-to-system calls are FORBIDDEN.
