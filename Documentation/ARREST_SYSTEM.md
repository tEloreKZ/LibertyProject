# Arrest System

**Arrest System Documentation**

---

## Table of Contents

1. [Overview](#overview)
2. [Arrest Architecture](#arrest-architecture)
3. [Arrest Service](#arrest-service)
4. [Handcuff System](#handcuff-system)
5. [Transport System](#transport-system)
6. [Booking System](#booking-system)
7. [Arrest Events](#arrest-events)
8. [Best Practices](#best-practices)

---

## Overview

The Arrest System provides complete arrest functionality including handcuffing, transport, and booking. It operates independently from other systems, communicating only through events.

### Architecture

```
┌─────────────────────────────────────────┐
│         Arrest Service                 │
│  (Main arrest logic)                   │
└─────────────────────────────────────────┘
                  ↓
┌─────────────────────────────────────────┐
│         Handcuff System                │
│  (Handcuff mechanics)                  │
└─────────────────────────────────────────┘
                  ↓
┌─────────────────────────────────────────┐
│         Transport System               │
│  (Prisoner transport)                  │
└─────────────────────────────────────────┘
                  ↓
┌─────────────────────────────────────────┐
│         Booking System                 │
│  (Booking process)                     │
└─────────────────────────────────────────┘
```

---

## Arrest Architecture

### Design Principles

1. **Independence**: Arrest system doesn't know about traffic stops or callouts
2. **Event-Driven**: All communication through events
3. **Modular**: Each component (handcuff, transport, booking) is independent
4. **Configurable**: All arrest procedures are configurable
5. **Extensible**: Easy to add new arrest types

### System Boundaries

```
Arrest System
├── Knows: Peds, handcuffs, transport, booking
├── Doesn't Know: Traffic stops, callouts, dispatch details
├── Receives: PedSurrendered, PedResisting
└── Sends: PedArrested, TransportStarted, BookingCompleted
```

---

## Arrest Service

### Service Interface

```csharp
namespace ProjectLiberty.ArrestSystem
{
    /// <summary>
    /// Arrest service interface.
    /// </summary>
    public interface IArrestService
    {
        /// <summary>
        /// Starts an arrest.
        /// </summary>
        /// <param name="suspect">Suspect ped.</param>
        /// <param name="officer">Officer ped.</param>
        /// <param name="reason">Arrest reason.</param>
        /// <param name="position">Arrest position.</param>
        /// <returns>Arrest ID.</returns>
        string StartArrest(IPed suspect, IPed officer, ArrestReason reason, Vector3 position);
        
        /// <summary>
        /// Completes an arrest.
        /// </summary>
        /// <param name="arrestId">Arrest ID.</param>
        /// <param name="result">Arrest result.</param>
        void CompleteArrest(string arrestId, ArrestResult result);
        
        /// <summary>
        /// Cancels an arrest.
        /// </summary>
        /// <param name="arrestId">Arrest ID.</param>
        void CancelArrest(string arrestId);
        
        /// <summary>
        /// Gets an arrest by ID.
        /// </summary>
        /// <param name="arrestId">Arrest ID.</param>
        /// <returns>Arrest, or null if not found.</returns>
        Arrest GetArrest(string arrestId);
        
        /// <summary>
        /// Gets all active arrests.
        /// </summary>
        /// <returns>Active arrests.</returns>
        IEnumerable<Arrest> GetActiveArrests();
        
        /// <summary>
        /// Gets the maximum number of prisoners.
        /// </summary>
        int MaxPrisoners { get; }
    }
}
```

### Arrest Service Implementation

```csharp
namespace ProjectLiberty.ArrestSystem
{
    /// <summary>
    /// Arrest service implementation.
    /// </summary>
    public class ArrestService : IArrestService
    {
        private readonly Dictionary<string, Arrest> _arrests = new Dictionary<string, Arrest>();
        private readonly IEventBus _eventBus;
        private readonly IHandcuffSystem _handcuffSystem;
        private readonly ITransportSystem _transportSystem;
        private readonly IBookingSystem _bookingSystem;
        private readonly ILocalization _localization;
        private readonly IConfiguration _configuration;
        private readonly ILogger _logger;
        
        public int MaxPrisoners => _configuration.GetValue<int>("arrests.maxPrisoners", 10);
        
        public ArrestService(
            IEventBus eventBus,
            IHandcuffSystem handcuffSystem,
            ITransportSystem transportSystem,
            IBookingSystem bookingSystem,
            ILocalization localization,
            IConfiguration configuration,
            ILogger logger)
        {
            _eventBus = eventBus;
            _handcuffSystem = handcuffSystem;
            _transportSystem = transportSystem;
            _bookingSystem = bookingSystem;
            _localization = localization;
            _configuration = configuration;
            _logger = logger;
            
            // Subscribe to events
            _eventBus.Subscribe<PedSurrenderedEvent>(OnPedSurrendered);
            _eventBus.Subscribe<PedResistingEvent>(OnPedResisting);
        }
        
        public string StartArrest(IPed suspect, IPed officer, ArrestReason reason, Vector3 position)
        {
            // Check max prisoners
            if (_arrests.Count >= MaxPrisoners)
            {
                _logger.LogWarning("Cannot start arrest: maximum prisoners reached");
                return null;
            }
            
            var arrestId = Guid.NewGuid().ToString();
            var arrest = new Arrest
            {
                Id = arrestId,
                Suspect = suspect,
                Officer = officer,
                Reason = reason,
                Position = position,
                StartTime = DateTime.UtcNow,
                Status = ArrestStatus.InProgress
            };
            
            _arrests[arrestId] = arrest;
            
            // Handcuff suspect
            _handcuffSystem.Handcuff(suspect);
            
            _eventBus.Publish(new PedArrestedEvent
            {
                PedHandle = suspect.Handle,
                OfficerHandle = officer.Handle,
                Position = position,
                Reason = reason
            });
            
            _logger.LogInformation($"Arrest started: {arrestId} for reason {reason}");
            return arrestId;
        }
        
        public void CompleteArrest(string arrestId, ArrestResult result)
        {
            if (!_arrests.TryGetValue(arrestId, out var arrest))
            {
                _logger.LogWarning($"Arrest not found: {arrestId}");
                return;
            }
            
            arrest.Status = ArrestStatus.Completed;
            arrest.EndTime = DateTime.UtcNow;
            arrest.Result = result;
            
            // Remove handcuffs if not transporting
            if (result != ArrestResult.Transported)
            {
                _handcuffSystem.Unhandcuff(arrest.Suspect);
            }
            
            _eventBus.Publish(new ArrestCompletedEvent
            {
                ArrestId = arrestId,
                Result = result,
                Duration = arrest.EndTime - arrest.StartTime
            });
            
            _arrests.Remove(arrestId);
            _logger.LogInformation($"Arrest completed: {arrestId} with result {result}");
        }
        
        public void CancelArrest(string arrestId)
        {
            if (!_arrests.TryGetValue(arrestId, out var arrest))
            {
                _logger.LogWarning($"Arrest not found: {arrestId}");
                return;
            }
            
            // Remove handcuffs
            _handcuffSystem.Unhandcuff(arrest.Suspect);
            
            _arrests.Remove(arrestId);
            _logger.LogInformation($"Arrest cancelled: {arrestId}");
        }
        
        public Arrest GetArrest(string arrestId)
        {
            return _arrests.TryGetValue(arrestId, out var arrest) ? arrest : null;
        }
        
        public IEnumerable<Arrest> GetActiveArrests()
        {
            return _arrests.Values;
        }
        
        private void OnPedSurrendered(PedSurrenderedEvent @event)
        {
            // Auto-arrest surrendered peds if configured
            if (_configuration.GetValue<bool>("arrests.autoArrestSurrendered", false))
            {
                var player = _playerManager.GetLocalPlayer();
                if (player != null)
                {
                    var suspect = _pedManager.GetPed(@event.PedHandle);
                    if (suspect != null)
                    {
                        StartArrest(suspect, player.Character, ArrestReason.Surrender, @event.Position);
                    }
                }
            }
        }
        
        private void OnPedResisting(PedResistingEvent @event)
        {
            // Handle resisting peds
            _logger.LogInformation($"Ped resisting: {@event.PedHandle}");
        }
    }
}
```

### Arrest Entity

```csharp
namespace ProjectLiberty.ArrestSystem
{
    /// <summary>
    /// Arrest entity.
    /// </summary>
    public class Arrest
    {
        public string Id { get; set; }
        public IPed Suspect { get; set; }
        public IPed Officer { get; set; }
        public ArrestReason Reason { get; set; }
        public Vector3 Position { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public ArrestStatus Status { get; set; }
        public ArrestResult Result { get; set; }
        public Dictionary<string, object> Data { get; set; } = new Dictionary<string, object>();
    }
    
    /// <summary>
    /// Arrest status.
    /// </summary>
    public enum ArrestStatus
    {
        InProgress,
        Transporting,
        Booking,
        Completed,
        Cancelled
    }
    
    /// <summary>
    /// Arrest result.
    /// </summary>
    public enum ArrestResult
    {
        Booked,
        Transported,
        Released,
        Escaped,
        Cancelled
    }
    
    /// <summary>
    /// Arrest reason.
    /// </summary>
    public enum ArrestReason
    {
        Speeding,
        RecklessDriving,
        DUI,
        Warrant,
        Violence,
        Surrender,
        ResistingArrest,
        Other
    }
}
```

---

## Handcuff System

### System Interface

```csharp
namespace ProjectLiberty.ArrestSystem
{
    /// <summary>
    /// Handcuff system interface.
    /// </summary>
    public interface IHandcuffSystem
    {
        /// <summary>
        /// Handcuffs a ped.
        /// </summary>
        /// <param name="ped">Ped to handcuff.</param>
        void Handcuff(IPed ped);
        
        /// <summary>
        /// Unhandcuffs a ped.
        /// </summary>
        /// <param name="ped">Ped to unhandcuff.</param>
        void Unhandcuff(IPed ped);
        
        /// <summary>
        /// Checks if a ped is handcuffed.
        /// </summary>
        /// <param name="ped">Ped to check.</param>
        /// <returns>True if handcuffed, false otherwise.</returns>
        bool IsHandcuffed(IPed ped);
        
        /// <summary>
        /// Gets the handcuff time in seconds.
        /// </summary>
        float HandcuffTime { get; }
    }
}
```

### Handcuff System Implementation

```csharp
namespace ProjectLiberty.ArrestSystem
{
    /// <summary>
    /// Handcuff system implementation.
    /// </summary>
    public class HandcuffSystem : IHandcuffSystem
    {
        private readonly HashSet<int> _handcuffedPeds = new HashSet<int>();
        private readonly IEventBus _eventBus;
        private readonly IConfiguration _configuration;
        private readonly ILogger _logger;
        
        public float HandcuffTime => _configuration.GetValue<float>("arrests.handcuffTime", 5f);
        
        public HandcuffSystem(
            IEventBus eventBus,
            IConfiguration configuration,
            ILogger logger)
        {
            _eventBus = eventBus;
            _configuration = configuration;
            _logger = logger;
        }
        
        public void Handcuff(IPed ped)
        {
            if (ped == null)
                return;
            
            if (_handcuffedPeds.Contains(ped.Handle))
                return;
            
            // Apply handcuff animation
            ped.PlayAnimation("missminuteman_1ig_2", "handsup_base", HandcuffTime);
            
            // Disable ped abilities
            ped.CanFight = false;
            ped.CanUseWeapons = false;
            ped.CanRagdoll = false;
            
            _handcuffedPeds.Add(ped.Handle);
            
            _eventBus.Publish(new PedHandcuffedEvent
            {
                PedHandle = ped.Handle,
                Position = ped.Position
            });
            
            _logger.LogInformation($"Handcuffed ped: {ped.Handle}");
        }
        
        public void Unhandcuff(IPed ped)
        {
            if (ped == null)
                return;
            
            if (!_handcuffedPeds.Contains(ped.Handle))
                return;
            
            // Stop animation
            ped.StopAnimation();
            
            // Re-enable ped abilities
            ped.CanFight = true;
            ped.CanUseWeapons = true;
            ped.CanRagdoll = true;
            
            _handcuffedPeds.Remove(ped.Handle);
            
            _eventBus.Publish(new PedUnhandcuffedEvent
            {
                PedHandle = ped.Handle,
                Position = ped.Position
            });
            
            _logger.LogInformation($"Unhandcuffed ped: {ped.Handle}");
        }
        
        public bool IsHandcuffed(IPed ped)
        {
            return ped != null && _handcuffedPeds.Contains(ped.Handle);
        }
    }
}
```

---

## Transport System

### System Interface

```csharp
namespace ProjectLiberty.ArrestSystem
{
    /// <summary>
    /// Transport system interface.
    /// </summary>
    public interface ITransportSystem
    {
        /// <summary>
        /// Starts transport.
        /// </summary>
        /// <param name="arrestId">Arrest ID.</param>
        /// <param name="destination">Destination position.</param>
        void StartTransport(string arrestId, Vector3 destination);
        
        /// <summary>
        /// Cancels transport.
        /// </summary>
        /// <param name="arrestId">Arrest ID.</param>
        void CancelTransport(string arrestId);
        
        /// <summary>
        /// Gets the transport time in seconds.
        /// </summary>
        float TransportTime { get; }
        
        /// <summary>
        /// Gets the transport vehicle.
        /// </summary>
        IVehicle TransportVehicle { get; }
    }
}
```

### Transport System Implementation

```csharp
namespace ProjectLiberty.ArrestSystem
{
    /// <summary>
    /// Transport system implementation.
    /// </summary>
    public class TransportSystem : ITransportSystem
    {
        private readonly Dictionary<string, Transport> _transports = new Dictionary<string, Transport>();
        private readonly IEventBus _eventBus;
        private readonly IVehicleManager _vehicleManager;
        private readonly ILocalization _localization;
        private readonly IConfiguration _configuration;
        private readonly ILogger _logger;
        
        public float TransportTime => _configuration.GetValue<float>("arrests.transportTime", 30f);
        public IVehicle TransportVehicle { get; private set; }
        
        public TransportSystem(
            IEventBus eventBus,
            IVehicleManager vehicleManager,
            ILocalization localization,
            IConfiguration configuration,
            ILogger logger)
        {
            _eventBus = eventBus;
            _vehicleManager = vehicleManager;
            _localization = localization;
            _configuration = configuration;
            _logger = logger;
        }
        
        public void StartTransport(string arrestId, Vector3 destination)
        {
            // Create transport vehicle
            TransportVehicle = _vehicleManager.CreateVehicle(
                VehicleModel.PoliceTransporter,
                _playerManager.GetLocalPlayer().Position,
                0f);
            
            var transport = new Transport
            {
                ArrestId = arrestId,
                Destination = destination,
                StartTime = DateTime.UtcNow,
                Status = TransportStatus.EnRoute
            };
            
            _transports[arrestId] = transport;
            
            _eventBus.Publish(new TransportStartedEvent
            {
                ArrestId = arrestId,
                Destination = destination
            });
            
            _logger.LogInformation($"Transport started: {arrestId} to {destination}");
        }
        
        public void CancelTransport(string arrestId)
        {
            if (_transports.Remove(arrestId))
            {
                // Delete transport vehicle
                TransportVehicle?.Delete();
                TransportVehicle = null;
                
                _logger.LogInformation($"Transport cancelled: {arrestId}");
            }
        }
    }
    
    /// <summary>
    /// Transport entity.
    /// </summary>
    public class Transport
    {
        public string ArrestId { get; set; }
        public Vector3 Destination { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public TransportStatus Status { get; set; }
    }
    
    /// <summary>
    /// Transport status.
    /// </summary>
    public enum TransportStatus
    {
        EnRoute,
        Arrived,
        Completed,
        Cancelled
    }
}
```

---

## Booking System

### System Interface

```csharp
namespace ProjectLiberty.ArrestSystem
{
    /// <summary>
    /// Booking system interface.
    /// </summary>
    public interface IBookingSystem
    {
        /// <summary>
        /// Starts booking.
        /// </summary>
        /// <param name="arrestId">Arrest ID.</param>
        void StartBooking(string arrestId);
        
        /// <summary>
        /// Completes booking.
        /// </summary>
        /// <param name="arrestId">Arrest ID.</param>
        void CompleteBooking(string arrestId);
        
        /// <summary>
        /// Gets the booking time in seconds.
        /// </summary>
        float BookingTime { get; }
        
        /// <summary>
        /// Gets the booking station.
        /// </summary>
        BookingStation BookingStation { get; }
    }
}
```

### Booking System Implementation

```csharp
namespace ProjectLiberty.ArrestSystem
{
    /// <summary>
    /// Booking system implementation.
    /// </summary>
    public class BookingSystem : IBookingSystem
    {
        private readonly Dictionary<string, Booking> _bookings = new Dictionary<string, Booking>();
        private readonly IEventBus _eventBus;
        private readonly ILocalization _localization;
        private readonly IConfiguration _configuration;
        private readonly ILogger _logger;
        
        public float BookingTime => _configuration.GetValue<float>("arrests.bookingTime", 60f);
        public BookingStation BookingStation { get; private set; }
        
        public BookingSystem(
            IEventBus eventBus,
            ILocalization localization,
            IConfiguration configuration,
            ILogger logger)
        {
            _eventBus = eventBus;
            _localization = localization;
            _configuration = configuration;
            _logger = logger;
            
            BookingStation = new BookingStation
            {
                Position = new Vector3(450, -1000, 30),
                Name = "Mission Row Police Station"
            };
        }
        
        public void StartBooking(string arrestId)
        {
            var booking = new Booking
            {
                ArrestId = arrestId,
                Station = BookingStation,
                StartTime = DateTime.UtcNow,
                Status = BookingStatus.InProgress
            };
            
            _bookings[arrestId] = booking;
            
            _eventBus.Publish(new BookingStartedEvent
            {
                ArrestId = arrestId,
                StationName = BookingStation.Name
            });
            
            _logger.LogInformation($"Booking started: {arrestId} at {BookingStation.Name}");
        }
        
        public void CompleteBooking(string arrestId)
        {
            if (!_bookings.TryGetValue(arrestId, out var booking))
                return;
            
            booking.Status = BookingStatus.Completed;
            booking.EndTime = DateTime.UtcNow;
            
            _eventBus.Publish(new BookingCompletedEvent
            {
                ArrestId = arrestId,
                Duration = booking.EndTime - booking.StartTime
            });
            
            _bookings.Remove(arrestId);
            _logger.LogInformation($"Booking completed: {arrestId}");
        }
    }
    
    /// <summary>
    /// Booking entity.
    /// </summary>
    public class Booking
    {
        public string ArrestId { get; set; }
        public BookingStation Station { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public BookingStatus Status { get; set; }
    }
    
    /// <summary>
    /// Booking station.
    /// </summary>
    public class BookingStation
    {
        public string Name { get; set; }
        public Vector3 Position { get; set; }
    }
    
    /// <summary>
    /// Booking status.
    /// </summary>
    public enum BookingStatus
    {
        InProgress,
        Completed,
        Cancelled
    }
}
```

---

## Arrest Events

### Event Definitions

```csharp
namespace ProjectLiberty.Events
{
    /// <summary>
    /// Ped arrested event.
    /// </summary>
    public class PedArrestedEvent : BaseEvent
    {
        public int PedHandle { get; set; }
        public int OfficerHandle { get; set; }
        public Vector3 Position { get; set; }
        public ArrestReason Reason { get; set; }
    }
    
    /// <summary>
    /// Ped handcuffed event.
    /// </summary>
    public class PedHandcuffedEvent : BaseEvent
    {
        public int PedHandle { get; set; }
        public Vector3 Position { get; set; }
    }
    
    /// <summary>
    /// Ped unhandcuffed event.
    /// </summary>
    public class PedUnhandcuffedEvent : BaseEvent
    {
        public int PedHandle { get; set; }
        public Vector3 Position { get; set; }
    }
    
    /// <summary>
    /// Transport started event.
    /// </summary>
    public class TransportStartedEvent : BaseEvent
    {
        public string ArrestId { get; set; }
        public Vector3 Destination { get; set; }
    }
    
    /// <summary>
    /// Transport completed event.
    /// </summary>
    public class TransportCompletedEvent : BaseEvent
    {
        public string ArrestId { get; set; }
    }
    
    /// <summary>
    /// Booking started event.
    /// </summary>
    public class BookingStartedEvent : BaseEvent
    {
        public string ArrestId { get; set; }
        public string StationName { get; set; }
    }
    
    /// <summary>
    /// Booking completed event.
    /// </summary>
    public class BookingCompletedEvent : BaseEvent
    {
        public string ArrestId { get; set; }
        public TimeSpan Duration { get; set; }
    }
    
    /// <summary>
    /// Arrest completed event.
    /// </summary>
    public class ArrestCompletedEvent : BaseEvent
    {
        public string ArrestId { get; set; }
        public ArrestResult Result { get; set; }
        public TimeSpan Duration { get; set; }
    }
}
```

---

## Best Practices

### 1. Use Events for Communication

```csharp
// GOOD - Use events
_eventBus.Publish(new PedArrestedEvent { ... });

// BAD - Direct calls
_transportSystem.StartTransport(arrestId, destination);
```

### 2. Handcuff Before Transport

```csharp
// GOOD - Handcuff first
_handcuffSystem.Handcuff(suspect);
_transportSystem.StartTransport(arrestId, destination);

// BAD - Transport without handcuffs
_transportSystem.StartTransport(arrestId, destination);
```

### 3. Clean Up Resources

```csharp
// GOOD - Clean up
public void CompleteArrest(string arrestId, ArrestResult result)
{
    _handcuffSystem.Unhandcuff(arrest.Suspect);
    _transportVehicle?.Delete();
    _arrests.Remove(arrestId);
}

// BAD - No cleanup
// Resources leak
```

### 4. Handle Escape Attempts

```csharp
// GOOD - Handle escape
if (suspect.IsRunning)
{
    _eventBus.Publish(new PedEscapedEvent { ... });
    CompleteArrest(arrestId, ArrestResult.Escaped);
}

// BAD - Don't handle
// Suspect escapes without notification
```

### 5. Configure Times

```csharp
// GOOD - Configurable times
var handcuffTime = _configuration.GetValue<float>("arrests.handcuffTime", 5f);

// BAD - Hardcoded times
const float HandcuffTime = 5f;
```

### 6. Check Max Prisoners

```csharp
// GOOD - Check limit
if (_arrests.Count >= MaxPrisoners)
{
    _logger.LogWarning("Maximum prisoners reached");
    return;
}

// BAD - No check
// Exceeds limit
```

### 7. Use Appropriate Arrest Reasons

```csharp
// GOOD - Appropriate reason
StartArrest(suspect, officer, ArrestReason.DUI, position);

// BAD - Always use Other
StartArrest(suspect, officer, ArrestReason.Other, position);
```

---

## Summary

The Arrest System provides:

- **Independent operation** from other game systems
- **Modular components** (handcuff, transport, booking)
- **Event-driven communication** for loose coupling
- **Configurable procedures** for customization
- **Complete lifecycle management** for arrests
- **Transport and booking** integration

The arrest system operates independently and communicates only through events.
