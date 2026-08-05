# Call Outs

**Callout System Documentation**

---

## Table of Contents

1. [Overview](#overview)
2. [Callout Architecture](#callout-architecture)
3. [Callout Base Class](#callout-base-class)
4. [Callout Service](#callout-service)
5. [Callout Scanner](#callout-scanner)
6. [Callout Areas](#callout-areas)
7. [Callout Events](#callout-events)
8. [Best Practices](#best-practices)

---

## Overview

The Callout System provides a modular framework for creating police callouts. Each callout is an independent plugin that can be loaded, enabled, and managed separately. The system handles callout spawning, management, and lifecycle.

### Architecture

```
┌─────────────────────────────────────────┐
│         Callout Service                 │
│  (Main callout logic)                  │
└─────────────────────────────────────────┘
                  ↓
┌─────────────────────────────────────────┐
│         Callout Manager                │
│  (Callout lifecycle management)         │
└─────────────────────────────────────────┘
                  ↓
┌─────────────────────────────────────────┐
│         Callout Scanner                 │
│  (Callout spawning logic)              │
└─────────────────────────────────────────┘
                  ↓
┌─────────────────────────────────────────┐
│         Callout Plugins                 │
│  (Individual callout implementations)    │
└─────────────────────────────────────────┘
```

---

## Callout Architecture

### Design Principles

1. **Plugin-Based**: Each callout is a separate plugin
2. **Independent**: Callouts don't depend on each other
3. **Event-Driven**: Communication through events only
4. **Configurable**: All callouts have their own configuration
5. **Extensible**: Easy to add new callout types

### System Boundaries

```
Callout System
├── Knows: Callout plugins, spawning logic
├── Doesn't Know: Traffic stops, arrests, dispatch details
├── Receives: CalloutAccepted, CalloutDeclined
└── Sends: CalloutStarted, CalloutFinished, CalloutFailed
```

---

## Callout Base Class

### Callout Interface

```csharp
namespace ProjectLiberty.Callouts
{
    /// <summary>
    /// Callout interface.
    /// </summary>
    public interface ICallout : IPlugin
    {
        /// <summary>
        /// Gets the callout name.
        /// </summary>
        string Name { get; }
        
        /// <summary>
        /// Gets the callout description.
        /// </summary>
        string Description { get; }
        
        /// <summary>
        /// Gets the callout priority.
        /// </summary>
        CalloutPriority Priority { get; }
        
        /// <summary>
        /// Gets the callout spawn area.
        /// </summary>
        CalloutArea SpawnArea { get; }
        
        /// <summary>
        /// Gets the minimum distance from player.
        /// </summary>
        float MinDistance { get; }
        
        /// <summary>
        /// Gets the maximum distance from player.
        /// </summary>
        float MaxDistance { get; }
        
        /// <summary>
        /// Gets the callout weight for random selection.
        /// </summary>
        int Weight { get; }
        
        /// <summary>
        /// Checks if the callout can spawn.
        /// </summary>
        /// <param name="playerPosition">Player position.</param>
        /// <returns>True if can spawn, false otherwise.</returns>
        bool CanSpawn(Vector3 playerPosition);
        
        /// <summary>
        /// Spawns the callout.
        /// </summary>
        /// <param name="position">Spawn position.</param>
        void Spawn(Vector3 position);
        
        /// <summary>
        /// Accepts the callout.
        /// </summary>
        void Accept();
        
        /// <summary>
        /// Declines the callout.
        /// </summary>
        void Decline();
        
        /// <summary>
        /// Ends the callout.
        /// </summary>
        /// <param name="result">Callout result.</param>
        void End(CalloutResult result);
    }
}
```

### Callout Base Class

```csharp
namespace ProjectLiberty.Callouts
{
    /// <summary>
    /// Base class for callouts.
    /// </summary>
    public abstract class CalloutBase : PluginBase, ICallout
    {
        protected ICalloutContext Context { get; private set; }
        protected Vector3 SpawnPosition { get; private set; }
        protected bool IsActive { get; private set; }
        protected DateTime StartTime { get; private set; }
        
        public abstract string Name { get; }
        public abstract string Description { get; }
        public abstract CalloutPriority Priority { get; }
        public abstract CalloutArea SpawnArea { get; }
        public abstract float MinDistance { get; }
        public abstract float MaxDistance { get; }
        public abstract int Weight { get; }
        
        public virtual bool CanSpawn(Vector3 playerPosition)
        {
            // Check distance
            var distance = Vector3.Distance(playerPosition, GetPotentialSpawnPosition());
            if (distance < MinDistance || distance > MaxDistance)
                return false;
            
            // Check if already active
            if (IsActive)
                return false;
            
            // Check configuration
            if (!_configuration.GetValue<bool>($"callouts.{Name.ToLower()}.enabled", true))
                return false;
            
            return true;
        }
        
        public virtual void Spawn(Vector3 position)
        {
            SpawnPosition = position;
            IsActive = true;
            StartTime = DateTime.UtcNow;
            
            OnSpawn();
            
            _eventBus.Publish(new CalloutStartedEvent
            {
                CalloutId = Manifest.Id,
                CalloutName = Name,
                Position = position,
                Priority = Priority
            });
        }
        
        public virtual void Accept()
        {
            Context.IsAccepted = true;
            
            OnAccept();
            
            _eventBus.Publish(new CalloutAcceptedEvent
            {
                CalloutId = Manifest.Id,
                PlayerHandle = _playerManager.GetLocalPlayer().Handle
            });
        }
        
        public virtual void Decline()
        {
            Context.IsAccepted = false;
            
            OnDecline();
            
            _eventBus.Publish(new CalloutDeclinedEvent
            {
                CalloutId = Manifest.Id
            });
            
            End(CalloutResult.Declined);
        }
        
        public virtual void End(CalloutResult result)
        {
            IsActive = false;
            var duration = DateTime.UtcNow - StartTime;
            
            OnEnd(result);
            
            Cleanup();
            
            _eventBus.Publish(new CalloutFinishedEvent
            {
                CalloutId = Manifest.Id,
                Result = result,
                Duration = duration
            });
        }
        
        protected virtual void OnSpawn() { }
        protected virtual void OnAccept() { }
        protected virtual void OnDecline() { }
        protected virtual void OnEnd(CalloutResult result) { }
        protected virtual void Cleanup() { }
        
        protected virtual Vector3 GetPotentialSpawnPosition()
        {
            // Default implementation - override in derived classes
            return Vector3.Zero;
        }
    }
}
```

### Callout Priority

```csharp
namespace ProjectLiberty.Callouts
{
    /// <summary>
    /// Callout priority.
    /// </summary>
    public enum CalloutPriority
    {
        Low,
        Medium,
        High,
        Emergency
    }
}
```

### Callout Result

```csharp
namespace ProjectLiberty.Callouts
{
    /// <summary>
    /// Callout result.
    /// </summary>
    public enum CalloutResult
    {
        Success,
        Failed,
        Declined,
        Cancelled,
        PlayerDeath,
        Timeout
    }
}
```

---

## Callout Service

### Service Interface

```csharp
namespace ProjectLiberty.Callouts
{
    /// <summary>
    /// Callout service interface.
    /// </summary>
    public interface ICalloutService
    {
        /// <summary>
        /// Registers a callout.
        /// </summary>
        /// <param name="callout">Callout to register.</param>
        void RegisterCallout(ICallout callout);
        
        /// <summary>
        /// Unregisters a callout.
        /// </summary>
        /// <param name="calloutId">Callout ID.</param>
        void UnregisterCallout(string calloutId);
        
        /// <summary>
        /// Gets a callout by ID.
        /// </summary>
        /// <param name="calloutId">Callout ID.</param>
        /// <returns>Callout, or null if not found.</returns>
        ICallout GetCallout(string calloutId);
        
        /// <summary>
        /// Gets all registered callouts.
        /// </summary>
        /// <returns>All registered callouts.</returns>
        IEnumerable<ICallout> GetAllCallouts();
        
        /// <summary>
        /// Gets available callouts for spawning.
        /// </summary>
        /// <param name="playerPosition">Player position.</param>
        /// <returns>Available callouts.</returns>
        IEnumerable<ICallout> GetAvailableCallouts(Vector3 playerPosition);
        
        /// <summary>
        /// Spawns a random callout.
        /// </summary>
        /// <returns>Spawned callout, or null if none available.</returns>
        ICallout SpawnRandomCallout();
        
        /// <summary>
        /// Spawns a specific callout.
        /// </summary>
        /// <param name="calloutId">Callout ID.</param>
        /// <param name="position">Spawn position.</param>
        /// <returns>Spawned callout, or null if failed.</returns>
        ICallout SpawnCallout(string calloutId, Vector3 position);
        
        /// <summary>
        /// Gets the active callout.
        /// </summary>
        /// <returns>Active callout, or null if none active.</returns>
        ICallout GetActiveCallout();
        
        /// <summary>
        /// Gets the maximum number of active callouts.
        /// </summary>
        int MaxActiveCallouts { get; }
        
        /// <summary>
        /// Gets the callout spawn interval in seconds.
        /// </summary>
        float SpawnInterval { get; }
    }
}
```

### Callout Service Implementation

```csharp
namespace ProjectLiberty.Callouts
{
    /// <summary>
    /// Callout service implementation.
    /// </summary>
    public class CalloutService : ICalloutService
    {
        private readonly Dictionary<string, ICallout> _callouts = new Dictionary<string, ICallout>();
        private readonly IEventBus _eventBus;
        private readonly IPlayerManager _playerManager;
        private readonly ILocalization _localization;
        private readonly IConfiguration _configuration;
        private readonly ILogger _logger;
        private readonly Random _random = new Random();
        
        private ICallout _activeCallout;
        private float _spawnTimer;
        
        public int MaxActiveCallouts => _configuration.GetValue<int>("callouts.maxActiveCallouts", 3);
        public float SpawnInterval => _configuration.GetValue<float>("callouts.calloutInterval", 60f);
        
        public CalloutService(
            IEventBus eventBus,
            IPlayerManager playerManager,
            ILocalization localization,
            IConfiguration configuration,
            ILogger logger)
        {
            _eventBus = eventBus;
            _playerManager = playerManager;
            _localization = localization;
            _configuration = configuration;
            _logger = logger;
            
            // Subscribe to events
            _eventBus.Subscribe<CalloutAcceptedEvent>(OnCalloutAccepted);
            _eventBus.Subscribe<CalloutDeclinedEvent>(OnCalloutDeclined);
        }
        
        public void RegisterCallout(ICallout callout)
        {
            _callouts[callout.Manifest.Id] = callout;
            _logger.LogInformation($"Registered callout: {callout.Name}");
        }
        
        public void UnregisterCallout(string calloutId)
        {
            if (_callouts.Remove(calloutId))
            {
                _logger.LogInformation($"Unregistered callout: {calloutId}");
            }
        }
        
        public ICallout GetCallout(string calloutId)
        {
            return _callouts.TryGetValue(calloutId, out var callout) ? callout : null;
        }
        
        public IEnumerable<ICallout> GetAllCallouts()
        {
            return _callouts.Values;
        }
        
        public IEnumerable<ICallout> GetAvailableCallouts(Vector3 playerPosition)
        {
            return _callouts.Values
                .Where(c => c.CanSpawn(playerPosition))
                .OrderByDescending(c => c.Priority);
        }
        
        public ICallout SpawnRandomCallout()
        {
            var player = _playerManager.GetLocalPlayer();
            if (player == null)
                return null;
            
            var availableCallouts = GetAvailableCallouts(player.Position).ToList();
            if (!availableCallouts.Any())
                return null;
            
            // Weighted random selection
            var totalWeight = availableCallouts.Sum(c => c.Weight);
            var randomWeight = _random.Next(totalWeight);
            var cumulativeWeight = 0;
            
            ICallout selectedCallout = null;
            foreach (var callout in availableCallouts)
            {
                cumulativeWeight += callout.Weight;
                if (randomWeight < cumulativeWeight)
                {
                    selectedCallout = callout;
                    break;
                }
            }
            
            if (selectedCallout == null)
                selectedCallout = availableCallouts.First();
            
            var spawnPosition = selectedCallout.SpawnArea.GetRandomPosition(player.Position);
            return SpawnCallout(selectedCallout.Manifest.Id, spawnPosition);
        }
        
        public ICallout SpawnCallout(string calloutId, Vector3 position)
        {
            if (_activeCallout != null)
            {
                _logger.LogWarning("Cannot spawn callout: another callout is already active");
                return null;
            }
            
            var callout = GetCallout(calloutId);
            if (callout == null)
            {
                _logger.LogWarning($"Callout not found: {calloutId}");
                return null;
            }
            
            callout.Spawn(position);
            _activeCallout = callout;
            
            _logger.LogInformation($"Spawned callout: {callout.Name} at {position}");
            return callout;
        }
        
        public ICallout GetActiveCallout()
        {
            return _activeCallout;
        }
        
        public void OnTick(float deltaTime)
        {
            // Spawn timer
            if (_activeCallout == null)
            {
                _spawnTimer += deltaTime;
                if (_spawnTimer >= SpawnInterval)
                {
                    _spawnTimer = 0;
                    SpawnRandomCallout();
                }
            }
            
            // Update active callout
            _activeCallout?.OnTick(deltaTime);
        }
        
        private void OnCalloutAccepted(CalloutAcceptedEvent @event)
        {
            _logger.LogInformation($"Callout accepted: {@event.CalloutId}");
        }
        
        private void OnCalloutDeclined(CalloutDeclinedEvent @event)
        {
            _logger.LogInformation($"Callout declined: {@event.CalloutId}");
            _activeCallout = null;
        }
    }
}
```

---

## Callout Scanner

### Scanner Interface

```csharp
namespace ProjectLiberty.Callouts
{
    /// <summary>
    /// Callout scanner interface.
    /// </summary>
    public interface ICalloutScanner
    {
        /// <summary>
        /// Scans for potential callout spawns.
        /// </summary>
        /// <param name="playerPosition">Player position.</param>
        /// <returns>Potential spawn positions.</returns>
        IEnumerable<CalloutSpawnPoint> Scan(Vector3 playerPosition);
        
        /// <summary>
        /// Gets the scan radius.
        /// </summary>
        float ScanRadius { get; }
        
        /// <summary>
        /// Gets the scan interval in seconds.
        /// </summary>
        float ScanInterval { get; }
    }
}
```

### Callout Spawn Point

```csharp
namespace ProjectLiberty.Callouts
{
    /// <summary>
    /// Callout spawn point.
    /// </summary>
    public class CalloutSpawnPoint
    {
        public Vector3 Position { get; set; }
        public CalloutArea Area { get; set; }
        public float Weight { get; set; }
        public Dictionary<string, object> Data { get; set; } = new Dictionary<string, object>();
    }
}
```

---

## Callout Areas

### Area Interface

```csharp
namespace ProjectLiberty.Callouts
{
    /// <summary>
    /// Callout area interface.
    /// </summary>
    public interface ICalloutArea
    {
        /// <summary>
        /// Gets the area name.
        /// </summary>
        string Name { get; }
        
        /// <summary>
        /// Gets the area center.
        /// </summary>
        Vector3 Center { get; }
        
        /// <summary>
        /// Gets the area radius.
        /// </summary>
        float Radius { get; }
        
        /// <summary>
        /// Gets a random position within the area.
        /// </summary>
        /// <param name="nearPosition">Position to be near.</param>
        /// <returns>Random position.</returns>
        Vector3 GetRandomPosition(Vector3 nearPosition);
        
        /// <summary>
        /// Checks if a position is within the area.
        /// </summary>
        /// <param name="position">Position to check.</param>
        /// <returns>True if within area, false otherwise.</returns>
        bool Contains(Vector3 position);
    }
}
```

### Area Implementations

```csharp
namespace ProjectLiberty.Callouts
{
    /// <summary>
    /// Circular callout area.
    /// </summary>
    public class CircularCalloutArea : ICalloutArea
    {
        public string Name { get; set; }
        public Vector3 Center { get; set; }
        public float Radius { get; set; }
        
        public CircularCalloutArea(string name, Vector3 center, float radius)
        {
            Name = name;
            Center = center;
            Radius = radius;
        }
        
        public Vector3 GetRandomPosition(Vector3 nearPosition)
        {
            var angle = (float)(Random.Shared.NextDouble() * Math.PI * 2);
            var distance = (float)(Random.Shared.NextDouble() * Radius);
            
            return new Vector3(
                Center.X + MathF.Cos(angle) * distance,
                Center.Y + MathF.Sin(angle) * distance,
                Center.Z
            );
        }
        
        public bool Contains(Vector3 position)
        {
            return Vector3.Distance(position, Center) <= Radius;
        }
    }
    
    /// <summary>
    /// City-wide callout area.
    /// </summary>
    public class CityCalloutArea : ICalloutArea
    {
        public string Name => "Los Santos";
        public Vector3 Center { get; } = new Vector3(0, 0, 0);
        public float Radius { get; } = 5000f;
        
        public Vector3 GetRandomPosition(Vector3 nearPosition)
        {
            var angle = (float)(Random.Shared.NextDouble() * Math.PI * 2);
            var minDistance = 200f;
            var maxDistance = 1000f;
            var distance = minDistance + (float)(Random.Shared.NextDouble() * (maxDistance - minDistance));
            
            return new Vector3(
                nearPosition.X + MathF.Cos(angle) * distance,
                nearPosition.Y + MathF.Sin(angle) * distance,
                nearPosition.Z
            );
        }
        
        public bool Contains(Vector3 position)
        {
            return Vector3.Distance(position, Center) <= Radius;
        }
    }
}
```

---

## Callout Events

### Event Definitions

```csharp
namespace ProjectLiberty.Events
{
    /// <summary>
    /// Callout started event.
    /// </summary>
    public class CalloutStartedEvent : BaseEvent
    {
        public string CalloutId { get; set; }
        public string CalloutName { get; set; }
        public Vector3 Position { get; set; }
        public CalloutPriority Priority { get; set; }
    }
    
    /// <summary>
    /// Callout accepted event.
    /// </summary>
    public class CalloutAcceptedEvent : BaseEvent
    {
        public string CalloutId { get; set; }
        public int PlayerHandle { get; set; }
    }
    
    /// <summary>
    /// Callout declined event.
    /// </summary>
    public class CalloutDeclinedEvent : BaseEvent
    {
        public string CalloutId { get; set; }
    }
    
    /// <summary>
    /// Callout finished event.
    /// </summary>
    public class CalloutFinishedEvent : BaseEvent
    {
        public string CalloutId { get; set; }
        public CalloutResult Result { get; set; }
        public TimeSpan Duration { get; set; }
    }
}
```

---

## Best Practices

### 1. Use Callout Base Class

```csharp
// GOOD - Use base class
public class RobberyCallout : CalloutBase
{
    public override string Name => "Store Robbery";
    // ...
}

// BAD - Implement interface directly
public class RobberyCallout : ICallout
{
    // Must implement all methods manually
}
```

### 2. Override Lifecycle Methods

```csharp
// GOOD - Override lifecycle methods
protected override void OnSpawn()
{
    // Create robbery scenario
    _suspect = CreateSuspect();
    _victim = CreateVictim();
}

protected override void OnEnd(CalloutResult result)
{
    // Cleanup
    DeleteSuspect();
    DeleteVictim();
}

// BAD - Don't override
// No cleanup, resources leak
```

### 3. Use Appropriate Priority

```csharp
// GOOD - Appropriate priority
public override CalloutPriority Priority => CalloutPriority.Emergency; // For violent crimes

// BAD - Always high priority
public override CalloutPriority Priority => CalloutPriority.High; // Even for minor incidents
```

### 4. Configure Spawn Area

```csharp
// GOOD - Configurable area
public override CalloutArea SpawnArea => new CityCalloutArea();

// BAD - Hardcoded position
public Vector3 SpawnPosition => new Vector3(100, 100, 100);
```

### 5. Handle Player Death

```csharp
// GOOD - Handle player death
public override void OnTick(float deltaTime)
{
    if (_player.Health <= 0)
    {
        End(CalloutResult.PlayerDeath);
    }
}

// BAD - Don't handle
// Callout continues after player death
```

### 6. Use Events for Communication

```csharp
// GOOD - Use events
_eventBus.Publish(new CalloutStartedEvent { ... });

// BAD - Direct calls
_dispatchService.SendBackup(position);
```

### 7. Clean Up Resources

```csharp
// GOOD - Clean up
protected virtual void Cleanup()
{
    _suspect?.Delete();
    _victim?.Delete();
    _blip?.Remove();
}

// BAD - No cleanup
// Entities remain after callout ends
```

---

## Summary

The Callout System provides:

- **Plugin-based architecture** for modular callouts
- **Independent callouts** that don't depend on each other
- **Event-driven communication** for loose coupling
- **Configurable spawning** with areas and weights
- **Lifecycle management** for callout states
- **Random selection** with weighted probabilities

Each callout is an independent plugin and communicates only through events.
