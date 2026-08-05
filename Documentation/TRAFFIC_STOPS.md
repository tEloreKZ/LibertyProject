# Traffic Stops

**Traffic Stop System Documentation**

---

## Table of Contents

1. [Overview](#overview)
2. [Traffic Stop Architecture](#traffic-stop-architecture)
3. [Traffic Stop Stages](#traffic-stop-stages)
4. [Traffic Stop Service](#traffic-stop-service)
5. [Driver Behavior](#driver-behavior)
6. [Traffic Stop Events](#traffic-stop-events)
7. [Best Practices](#best-practices)

---

## Overview

The Traffic Stop System provides a complete, modular traffic stop implementation with multiple stages, AI-driven driver behavior, and event-based communication. Each traffic stop is an independent, self-contained process.

### Architecture

```
┌─────────────────────────────────────────┐
│         Traffic Stop Service           │
│  (Main traffic stop logic)             │
└─────────────────────────────────────────┘
                  ↓
┌─────────────────────────────────────────┐
│         Traffic Stop Manager            │
│  (Stop lifecycle management)            │
└─────────────────────────────────────────┘
                  ↓
┌─────────────────────────────────────────┐
│         Stage System                    │
│  (Stage execution, transitions)         │
└─────────────────────────────────────────┘
                  ↓
┌─────────────────────────────────────────┐
│         Driver AI                       │
│  (Driver behavior, reactions)           │
└─────────────────────────────────────────┘
```

---

## Traffic Stop Architecture

### Design Principles

1. **Modular Stages**: Each stage is independent and replaceable
2. **Event-Driven**: Communication through events only
3. **AI-Driven**: Driver behavior controlled by AI
4. **Configurable**: All stages configurable
5. **Extensible**: Easy to add new stages

### System Boundaries

```
Traffic Stop System
├── Knows: Stages, Driver AI, Vehicle
├── Doesn't Know: Dispatch, Arrests, Callouts
├── Receives: VehicleStopped, PlayerInteraction
└── Sends: TrafficStopStarted, StageChanged, TrafficStopEnded
```

---

## Traffic Stop Stages

### Stage Overview

Traffic stops progress through multiple stages:

1. **Observe**: Initial observation of vehicle
2. **Pull Over**: Signal and pull over vehicle
3. **Greeting**: Initial contact with driver
4. **Documents**: Request and check documents
5. **Questioning**: Question the driver
6. **Database**: Check driver database
7. **Citation**: Issue citation if needed
8. **Search**: Search vehicle if needed
9. **Arrest**: Arrest driver if needed
10. **Release**: Release driver

### Stage Interface

```csharp
namespace ProjectLiberty.TrafficStops
{
    /// <summary>
    /// Traffic stop stage interface.
    /// </summary>
    public interface ITrafficStopStage
    {
        /// <summary>
        /// Gets the stage type.
        /// </summary>
        TrafficStopStage StageType { get; }
        
        /// <summary>
        /// Gets whether the stage is enabled.
        /// </summary>
        bool IsEnabled { get; }
        
        /// <summary>
        /// Gets the stage duration in seconds.
        /// </summary>
        float Duration { get; }
        
        /// <summary>
        /// Starts the stage.
        /// </summary>
        /// <param name="context">Traffic stop context.</param>
        Task StartAsync(TrafficStopContext context);
        
        /// <summary>
        /// Updates the stage.
        /// </summary>
        /// <param name="context">Traffic stop context.</param>
        /// <param name="deltaTime">Time since last update.</param>
        Task UpdateAsync(TrafficStopContext context, float deltaTime);
        
        /// <summary>
        /// Ends the stage.
        /// </summary>
        /// <param name="context">Traffic stop context.</param>
        Task EndAsync(TrafficStopContext context);
        
        /// <summary>
        /// Checks if the stage can proceed to next.
        /// </summary>
        /// <param name="context">Traffic stop context.</param>
        /// <returns>True if can proceed, false otherwise.</returns>
        bool CanProceed(TrafficStopContext context);
    }
}
```

### Stage Types

```csharp
namespace ProjectLiberty.TrafficStops
{
    /// <summary>
    /// Traffic stop stage type.
    /// </summary>
    public enum TrafficStopStage
    {
        Observe,
        PullOver,
        Greeting,
        Documents,
        Questioning,
        Database,
        Citation,
        Search,
        Arrest,
        Release
    }
}
```

### Stage Implementations

#### Observe Stage

```csharp
namespace ProjectLiberty.TrafficStops.Stages
{
    /// <summary>
    /// Observe stage - initial vehicle observation.
    /// </summary>
    public class ObserveStage : ITrafficStopStage
    {
        public TrafficStopStage StageType => TrafficStopStage.Observe;
        public bool IsEnabled => _configuration.GetValue<bool>("stages.observe.enabled", true);
        public float Duration => _configuration.GetValue<float>("stages.observe.duration", 5f);
        
        private readonly IConfiguration _configuration;
        private readonly ILocalization _localization;
        
        public ObserveStage(IConfiguration configuration, ILocalization localization)
        {
            _configuration = configuration;
            _localization = localization;
        }
        
        public async Task StartAsync(TrafficStopContext context)
        {
            // Start observation
            context.Vehicle.IsSirenOn = false;
            context.Vehicle.IsBlipEnabled = true;
            
            await _eventBus.PublishAsync(new TrafficStopStageChangedEvent
            {
                StopId = context.StopId,
                OldStage = TrafficStopStage.None,
                NewStage = TrafficStopStage.Observe
            });
        }
        
        public async Task UpdateAsync(TrafficStopContext context, float deltaTime)
        {
            // Monitor vehicle behavior
            var speed = context.Vehicle.Speed;
            if (speed > _configuration.GetValue<float>("pullOverSpeed", 5f))
            {
                // Vehicle not slowing down
                context.DriverBehavior = DriverBehavior.Resistant;
            }
        }
        
        public async Task EndAsync(TrafficStopContext context)
        {
            // End observation
        }
        
        public bool CanProceed(TrafficStopContext context)
        {
            // Can proceed if vehicle is stopped or slowing
            return context.Vehicle.Speed < 5f;
        }
    }
}
```

#### Pull Over Stage

```csharp
namespace ProjectLiberty.TrafficStops.Stages
{
    /// <summary>
    /// Pull over stage - signal and pull over vehicle.
    /// </summary>
    public class PullOverStage : ITrafficStopStage
    {
        public TrafficStopStage StageType => TrafficStopStage.PullOver;
        public bool IsEnabled => _configuration.GetValue<bool>("stages.pullOver.enabled", true);
        public float Duration => _configuration.GetValue<float>("stages.pullOver.duration", 10f);
        
        public async Task StartAsync(TrafficStopContext context)
        {
            // Activate siren
            context.Vehicle.IsSirenOn = true;
            
            // Signal driver to pull over
            context.DriverAI.PullOver();
            
            await _eventBus.PublishAsync(new TrafficStopStageChangedEvent
            {
                StopId = context.StopId,
                OldStage = TrafficStopStage.Observe,
                NewStage = TrafficStopStage.PullOver
            });
        }
        
        public async Task UpdateAsync(TrafficStopContext context, float deltaTime)
        {
            // Monitor pull over progress
            var distance = Vector3.Distance(context.Player.Position, context.Vehicle.Position);
            
            if (distance > _configuration.GetValue<float>("stopDistance", 10f))
            {
                // Vehicle not pulling over properly
                context.DriverBehavior = DriverBehavior.Resistant;
            }
        }
        
        public async Task EndAsync(TrafficStopContext context)
        {
            // Vehicle pulled over
            context.Vehicle.IsSirenOn = false;
        }
        
        public bool CanProceed(TrafficStopContext context)
        {
            // Can proceed if vehicle is stopped
            return context.Vehicle.Speed < 1f;
        }
    }
}
```

#### Greeting Stage

```csharp
namespace ProjectLiberty.TrafficStops.Stages
{
    /// <summary>
    /// Greeting stage - initial contact with driver.
    /// </summary>
    public class GreetingStage : ITrafficStopStage
    {
        public TrafficStopStage StageType => TrafficStopStage.Greeting;
        public bool IsEnabled => _configuration.GetValue<bool>("stages.greeting.enabled", true);
        public float Duration => _configuration.GetValue<float>("stages.greeting.duration", 15f);
        
        public async Task StartAsync(TrafficStopContext context)
        {
            // Approach vehicle
            context.Player.Position = GetApproachPosition(context.Vehicle);
            
            // Show greeting dialog
            var greeting = _localization.GetString("TrafficStop.Greeting");
            _uiService.ShowDialog(greeting);
            
            await _eventBus.PublishAsync(new TrafficStopStageChangedEvent
            {
                StopId = context.StopId,
                OldStage = TrafficStopStage.PullOver,
                NewStage = TrafficStopStage.Greeting
            });
        }
        
        public async Task UpdateAsync(TrafficStopContext context, float deltaTime)
        {
            // Wait for player interaction
        }
        
        public async Task EndAsync(TrafficStopContext context)
        {
            // Greeting complete
        }
        
        public bool CanProceed(TrafficStopContext context)
        {
            // Can proceed after player interaction
            return context.PlayerInteracted;
        }
    }
}
```

---

## Traffic Stop Service

### Service Interface

```csharp
namespace ProjectLiberty.TrafficStops
{
    /// <summary>
    /// Traffic stop service interface.
    /// </summary>
    public interface ITrafficStopService
    {
        /// <summary>
        /// Starts a traffic stop.
        /// </summary>
        /// <param name="vehicle">Target vehicle.</param>
        /// <returns>Traffic stop ID.</returns>
        string StartTrafficStop(IVehicle vehicle);
        
        /// <summary>
        /// Ends a traffic stop.
        /// </summary>
        /// <param name="stopId">Traffic stop ID.</param>
        /// <param name="result">Stop result.</param>
        void EndTrafficStop(string stopId, TrafficStopResult result);
        
        /// <summary>
        /// Gets a traffic stop by ID.
        /// </summary>
        /// <param name="stopId">Traffic stop ID.</param>
        /// <returns>Traffic stop, or null if not found.</returns>
        TrafficStop GetTrafficStop(string stopId);
        
        /// <summary>
        /// Gets all active traffic stops.
        /// </summary>
        /// <returns>Active traffic stops.</returns>
        IEnumerable<TrafficStop> GetActiveStops();
        
        /// <summary>
        /// Advances to next stage.
        /// </summary>
        /// <param name="stopId">Traffic stop ID.</param>
        void AdvanceStage(string stopId);
        
        /// <summary>
        /// Skips to specific stage.
        /// </summary>
        /// <param name="stopId">Traffic stop ID.</param>
        /// <param name="stage">Target stage.</param>
        void SkipToStage(string stopId, TrafficStopStage stage);
    }
}
```

### Traffic Stop Context

```csharp
namespace ProjectLiberty.TrafficStops
{
    /// <summary>
    /// Traffic stop context.
    /// </summary>
    public class TrafficStopContext
    {
        public string StopId { get; set; }
        public IVehicle Vehicle { get; set; }
        public IPed Driver { get; set; }
        public IPlayer Player { get; set; }
        public TrafficStopStage CurrentStage { get; set; }
        public DriverBehavior DriverBehavior { get; set; }
        public bool PlayerInteracted { get; set; }
        public Dictionary<string, object> Data { get; set; } = new Dictionary<string, object>();
        public DateTime StartTime { get; set; }
    }
}
```

---

## Driver Behavior

### Behavior Types

```csharp
namespace ProjectLiberty.TrafficStops
{
    /// <summary>
    /// Driver behavior type.
    /// </summary>
    public enum DriverBehavior
    {
        Compliant,
        Resistant,
        Fleeing,
        Aggressive
    }
}
```

### Driver AI

```csharp
namespace ProjectLiberty.TrafficStops.AI
{
    /// <summary>
    /// Driver AI interface.
    /// </summary>
    public interface IDriverAI
    {
        /// <summary>
        /// Gets the driver behavior.
        /// </summary>
        DriverBehavior Behavior { get; }
        
        /// <summary>
        /// Pulls over the vehicle.
        /// </summary>
        void PullOver();
        
        /// <summary>
        /// Flees from the traffic stop.
        /// </summary>
        void Flee();
        
        /// <summary>
        /// Resists arrest.
        /// </summary>
        void Resist();
        
        /// <summary>
        /// Surrenders.
        /// </summary>
        void Surrender();
        
        /// <summary>
        /// Updates the AI.
        /// </summary>
        /// <param name="deltaTime">Time since last update.</param>
        void Update(float deltaTime);
    }
}
```

### Driver AI Implementation

```csharp
namespace ProjectLiberty.TrafficStops.AI
{
    /// <summary>
    /// Driver AI implementation.
    /// </summary>
    public class DriverAI : IDriverAI
    {
        private readonly IPed _driver;
        private readonly IVehicle _vehicle;
        private readonly IEventBus _eventBus;
        private readonly Random _random = new Random();
        
        public DriverBehavior Behavior { get; private set; } = DriverBehavior.Compliant;
        
        public DriverAI(IPed driver, IVehicle vehicle, IEventBus eventBus)
        {
            _driver = driver;
            _vehicle = vehicle;
            _eventBus = eventBus;
            
            // Randomize initial behavior
            var roll = _random.Next(100);
            if (roll < 70)
                Behavior = DriverBehavior.Compliant;
            else if (roll < 85)
                Behavior = DriverBehavior.Resistant;
            else if (roll < 95)
                Behavior = DriverBehavior.Fleeing;
            else
                Behavior = DriverBehavior.Aggressive;
        }
        
        public void PullOver()
        {
            // Slow down and pull over
            _vehicle.MaxSpeed = 5f;
            
            // Find safe pull over location
            var pullOverPosition = FindPullOverLocation();
            _vehicle.DriveTo(pullOverPosition, 5f);
        }
        
        public void Flee()
        {
            Behavior = DriverBehavior.Fleeing;
            _vehicle.MaxSpeed = 100f;
            
            // Drive away from player
            var fleeDirection = (_vehicle.Position - _player.Position).Normalized();
            var fleePosition = _vehicle.Position + fleeDirection * 1000;
            _vehicle.DriveTo(fleePosition, 100f);
            
            _eventBus.Publish(new DriverFleeingEvent
            {
                VehicleHandle = _vehicle.Handle,
                Position = _vehicle.Position
            });
        }
        
        public void Resist()
        {
            Behavior = DriverBehavior.Resistant;
            
            // Refuse to cooperate
            _driver.PlayAnimation("anim@move_m@prisoner_cuffed", "idle");
            
            _eventBus.Publish(new DriverResistingEvent
            {
                PedHandle = _driver.Handle,
                Position = _driver.Position
            });
        }
        
        public void Surrender()
        {
            Behavior = DriverBehavior.Compliant;
            
            // Hands up animation
            _driver.PlayAnimation("missminuteman_1ig_2", "handsup_base");
            
            _eventBus.Publish(new DriverSurrenderedEvent
            {
                PedHandle = _driver.Handle,
                Position = _driver.Position
            });
        }
        
        public void Update(float deltaTime)
        {
            // Update behavior based on situation
            if (Behavior == DriverBehavior.Fleeing)
            {
                // Check if should continue fleeing
                if (_random.Next(100) < 1) // 1% chance per frame to stop fleeing
                {
                    Surrender();
                }
            }
        }
        
        private Vector3 FindPullOverLocation()
        {
            // Find safe location to pull over
            var currentPosition = _vehicle.Position;
            var roadPosition = _worldManager.GetNearestRoad(currentPosition);
            
            return roadPosition;
        }
    }
}
```

---

## Traffic Stop Events

### Event Definitions

```csharp
namespace ProjectLiberty.Events
{
    /// <summary>
    /// Traffic stop started event.
    /// </summary>
    public class TrafficStopStartedEvent : BaseEvent
    {
        public string StopId { get; set; }
        public int VehicleHandle { get; set; }
        public int PlayerHandle { get; set; }
        public Vector3 Position { get; set; }
    }
    
    /// <summary>
    /// Traffic stop ended event.
    /// </summary>
    public class TrafficStopEndedEvent : BaseEvent
    {
        public string StopId { get; set; }
        public TrafficStopResult Result { get; set; }
        public TimeSpan Duration { get; set; }
    }
    
    /// <summary>
    /// Traffic stop stage changed event.
    /// </summary>
    public class TrafficStopStageChangedEvent : BaseEvent
    {
        public string StopId { get; set; }
        public TrafficStopStage OldStage { get; set; }
        public TrafficStopStage NewStage { get; set; }
    }
    
    /// <summary>
    /// Driver fleeing event.
    /// </summary>
    public class DriverFleeingEvent : BaseEvent
    {
        public int VehicleHandle { get; set; }
        public Vector3 Position { get; set; }
    }
    
    /// <summary>
    /// Driver resisting event.
    /// </summary>
    public class DriverResistingEvent : BaseEvent
    {
        public int PedHandle { get; set; }
        public Vector3 Position { get; set; }
    }
    
    /// <summary>
    /// Driver surrendered event.
    /// </summary>
    public class DriverSurrenderedEvent : BaseEvent
    {
        public int PedHandle { get; set; }
        public Vector3 Position { get; set; }
    }
}
```

### Traffic Stop Result

```csharp
namespace ProjectLiberty.TrafficStops
{
    /// <summary>
    /// Traffic stop result.
    /// </summary>
    public enum TrafficStopResult
    {
        Warning,
        Citation,
        Arrest,
        Pursuit,
        Cancelled
    }
}
```

---

## Best Practices

### 1. Use Stages Sequentially

```csharp
// GOOD - Sequential stages
await ExecuteStageAsync(ObserveStage);
await ExecuteStageAsync(PullOverStage);
await ExecuteStageAsync(GreetingStage);

// BAD - Skip stages
await ExecuteStageAsync(ArrestStage); // Skipped all previous stages
```

### 2. Handle Driver Behavior

```csharp
// GOOD - Handle different behaviors
switch (driverAI.Behavior)
{
    case DriverBehavior.Compliant:
        // Normal procedure
        break;
    case DriverBehavior.Resistant:
        // Call for backup
        break;
    case DriverBehavior.Fleeing:
        // Initiate pursuit
        break;
}

// BAD - Assume compliant
// Always assume driver is compliant
```

### 3. Use Events for Communication

```csharp
// GOOD - Use events
_eventBus.Publish(new TrafficStopStageChangedEvent { ... });

// BAD - Direct calls
_arrestSystem.StartArrest(driver);
```

### 4. Configure Stages

```csharp
// GOOD - Configurable stages
var duration = _configuration.GetValue<float>("stages.greeting.duration", 15f);

// BAD - Hardcoded values
const float GreetingDuration = 15f;
```

### 5. Handle Stage Failure

```csharp
// GOOD - Handle failure
try
{
    await stage.StartAsync(context);
}
catch (Exception ex)
{
    _logger.LogError(ex, "Stage failed");
    // Handle failure - skip or retry
}

// BAD - No error handling
await stage.StartAsync(context); // May throw
```

### 6. Clean Up Resources

```csharp
// GOOD - Clean up
public override void Disable()
{
    foreach (var stop in _activeStops.Values)
    {
        EndTrafficStop(stop.StopId, TrafficStopResult.Cancelled);
    }
}

// BAD - Leave active stops
// Stops remain active after disable
```

---

## Summary

The Traffic Stop System provides:

- **Modular stage system** for flexible traffic stops
- **AI-driven driver behavior** for realism
- **Event-based communication** for loose coupling
- **Configurable stages** for customization
- **Multiple behavior types** for variety
- **Complete lifecycle management** for traffic stops

Each traffic stop is independent and communicates only through events.
