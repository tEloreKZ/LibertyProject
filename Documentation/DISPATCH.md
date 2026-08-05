# Dispatch

**Dispatch System Documentation**

---

## Table of Contents

1. [Overview](#overview)
2. [Dispatch Architecture](#dispatch-architecture)
3. [Dispatch Service](#dispatch-service)
4. [Radio System](#radio-system)
5. [Unit Management](#unit-management)
6. [Call Codes](#call-codes)
7. [Dispatch Events](#dispatch-events)
8. [Best Practices](#best-practices)

---

## Overview

The Dispatch System manages all police dispatch operations including radio communication, unit management, and call code handling. It operates independently from other systems, communicating only through events.

### Architecture

```
┌─────────────────────────────────────────┐
│         Dispatch Service                │
│  (Main dispatch logic)                  │
└─────────────────────────────────────────┘
                  ↓
┌─────────────────────────────────────────┐
│         Radio System                    │
│  (Radio channels, audio)                │
└─────────────────────────────────────────┘
                  ↓
┌─────────────────────────────────────────┐
│         Unit Manager                    │
│  (Unit tracking, status)               │
└─────────────────────────────────────────┘
                  ↓
┌─────────────────────────────────────────┐
│         Call Code System                │
│  (Code registry, mapping)              │
└─────────────────────────────────────────┘
```

---

## Dispatch Architecture

### Design Principles

1. **Independence**: Dispatch system doesn't know about callouts or traffic stops
2. **Event-Driven**: All communication through events
3. **Radio-Centric**: Primary communication through radio
4. **Unit-Based**: Everything revolves around units
5. **Code-Standardized**: Uses standard call codes

### System Boundaries

```
Dispatch System
├── Knows: Units, Radio, Call Codes
├── Doesn't Know: Callouts, Traffic Stops, Arrests
├── Receives: BackupRequested, UnitStatusChanged
└── Sends: BackupArrived, DispatchMessage, UnitStatusChanged
```

---

## Dispatch Service

### Dispatch Service Interface

```csharp
namespace ProjectLiberty.Dispatch
{
    /// <summary>
    /// Dispatch service interface.
    /// </summary>
    public interface IDispatchService
    {
        /// <summary>
        /// Requests backup.
        /// </summary>
        /// <param name="location">Location.</param>
        /// <param name="priority">Priority.</param>
        /// <param name="type">Backup type.</param>
        /// <param name="requestingUnit">Requesting unit.</param>
        void RequestBackup(Vector3 location, BackupPriority priority, BackupType type, int requestingUnit);
        
        /// <summary>
        /// Cancels backup request.
        /// </summary>
        /// <param name="requestId">Request ID.</param>
        void CancelBackupRequest(Guid requestId);
        
        /// <summary>
        /// Sends dispatch message.
        /// </summary>
        /// <param name="message">Message.</param>
        /// <param name="type">Message type.</param>
        /// <param name="location">Location.</param>
        void SendMessage(string message, DispatchMessageType type, Vector3? location);
        
        /// <summary>
        /// Updates unit status.
        /// </summary>
        /// <param name="unitHandle">Unit handle.</param>
        /// <param name="status">New status.</param>
        void UpdateUnitStatus(int unitHandle, UnitStatus status);
        
        /// <summary>
        /// Gets unit by handle.
        /// </summary>
        /// <param name="unitHandle">Unit handle.</param>
        /// <returns>Unit, or null if not found.</returns>
        DispatchUnit GetUnit(int unitHandle);
        
        /// <summary>
        /// Gets all available units.
        /// </summary>
        /// <returns>Available units.</returns>
        IEnumerable<DispatchUnit> GetAvailableUnits();
        
        /// <summary>
        /// Gets units in radius.
        /// </summary>
        /// <param name="position">Center position.</param>
        /// <param name="radius">Search radius.</param>
        /// <returns>Units in radius.</returns>
        IEnumerable<DispatchUnit> GetUnitsInRadius(Vector3 position, float radius);
    }
}
```

### Dispatch Service Implementation

```csharp
namespace ProjectLiberty.Dispatch
{
    /// <summary>
    /// Dispatch service implementation.
    /// </summary>
    public class DispatchService : IDispatchService
    {
        private readonly IEventBus _eventBus;
        private readonly IUnitManager _unitManager;
        private readonly IRadioSystem _radioSystem;
        private readonly ICallCodeRegistry _callCodeRegistry;
        private readonly ILogger _logger;
        
        public DispatchService(
            IEventBus eventBus,
            IUnitManager unitManager,
            IRadioSystem radioSystem,
            ICallCodeRegistry callCodeRegistry,
            ILogger logger)
        {
            _eventBus = eventBus;
            _unitManager = unitManager;
            _radioSystem = radioSystem;
            _callCodeRegistry = callCodeRegistry;
            _logger = logger;
            
            // Subscribe to events
            _eventBus.Subscribe<BackupRequestedEvent>(OnBackupRequested);
            _eventBus.Subscribe<UnitStatusChangedEvent>(OnUnitStatusChanged);
        }
        
        public void RequestBackup(Vector3 location, BackupPriority priority, BackupType type, int requestingUnit)
        {
            var requestId = Guid.NewGuid();
            
            // Find available units
            var availableUnits = _unitManager.GetAvailableUnits()
                .Where(u => Vector3.Distance(u.Position, location) < 1000)
                .OrderBy(u => Vector3.Distance(u.Position, location))
                .Take(3);
            
            if (availableUnits.Any())
            {
                // Dispatch units
                foreach (var unit in availableUnits)
                {
                    _unitManager.AssignUnit(unit.Handle, requestId);
                    _eventBus.Publish(new BackupDispatchedEvent
                    {
                        RequestId = requestId,
                        UnitHandle = unit.Handle,
                        Location = location
                    });
                }
                
                // Announce on radio
                var message = _localization.GetString("Dispatch.BackupDispatched", priority, location);
                _radioSystem.Announce(message);
            }
            else
            {
                // No units available
                _logger.LogWarning("No available units for backup request");
                _radioSystem.Announce(_localization.GetString("Dispatch.NoUnitsAvailable"));
            }
        }
        
        public void CancelBackupRequest(Guid requestId)
        {
            var units = _unitManager.GetUnitsByRequest(requestId);
            
            foreach (var unit in units)
            {
                _unitManager.ReleaseUnit(unit.Handle);
            }
            
            _radioSystem.Announce(_localization.GetString("Dispatch.BackupCancelled"));
        }
        
        public void SendMessage(string message, DispatchMessageType type, Vector3? location)
        {
            _radioSystem.Announce(message);
            
            _eventBus.Publish(new DispatchMessageEvent
            {
                Message = message,
                Type = type,
                Location = location
            });
        }
        
        public void UpdateUnitStatus(int unitHandle, UnitStatus status)
        {
            _unitManager.UpdateStatus(unitHandle, status);
            
            _eventBus.Publish(new UnitStatusChangedEvent
            {
                UnitHandle = unitHandle,
                NewStatus = status
            });
        }
        
        public DispatchUnit GetUnit(int unitHandle)
        {
            return _unitManager.GetUnit(unitHandle);
        }
        
        public IEnumerable<DispatchUnit> GetAvailableUnits()
        {
            return _unitManager.GetAvailableUnits();
        }
        
        public IEnumerable<DispatchUnit> GetUnitsInRadius(Vector3 position, float radius)
        {
            return _unitManager.GetUnitsInRadius(position, radius);
        }
        
        private void OnBackupRequested(BackupRequestedEvent @event)
        {
            RequestBackup(@event.Location, @event.Priority, @event.Type, @event.RequestingUnitHandle);
        }
        
        private void OnUnitStatusChanged(UnitStatusChangedEvent @event)
        {
            _logger.LogInformation($"Unit {@event.UnitHandle} status changed to {@event.NewStatus}");
        }
    }
}
```

---

## Radio System

### Radio System Interface

```csharp
namespace ProjectLiberty.Dispatch
{
    /// <summary>
    /// Radio system interface.
    /// </summary>
    public interface IRadioSystem
    {
        /// <summary>
        /// Gets the current radio channel.
        /// </summary>
        int CurrentChannel { get; }
        
        /// <summary>
        /// Sets the radio channel.
        /// </summary>
        /// <param name="channel">Channel number.</param>
        void SetChannel(int channel);
        
        /// <summary>
        /// Announces a message on the radio.
        /// </summary>
        /// <param name="message">Message to announce.</param>
        void Announce(string message);
        
        /// <summary>
        /// Announces a message on a specific channel.
        /// </summary>
        /// <param name="message">Message to announce.</param>
        /// <param name="channel">Channel number.</param>
        void Announce(string message, int channel);
        
        /// <summary>
        /// Plays radio audio.
        /// </summary>
        /// <param name="audio">Audio to play.</param>
        void PlayAudio(RadioAudio audio);
        
        /// <summary>
        /// Mutes the radio.
        /// </summary>
        void Mute();
        
        /// <summary>
        /// Unmutes the radio.
        /// </summary>
        void Unmute();
    }
}
```

### Radio Channels

```csharp
namespace ProjectLiberty.Dispatch
{
    /// <summary>
    /// Radio channel configuration.
    /// </summary>
    public class RadioChannel
    {
        public int Number { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public bool IsEncrypted { get; set; }
    }
    
    public static class RadioChannels
    {
        public static readonly RadioChannel Channel1 = new RadioChannel
        {
            Number = 1,
            Name = "Main",
            Description = "Primary dispatch channel",
            IsEncrypted = false
        };
        
        public static readonly RadioChannel Channel2 = new RadioChannel
        {
            Number = 2,
            Name = "Tactical",
            Description = "Tactical operations",
            IsEncrypted = true
        };
        
        public static readonly RadioChannel Channel3 = new RadioChannel
        {
            Number = 3,
            Name = "Local",
            Description = "Local area coordination",
            IsEncrypted = false
        };
    }
}
```

---

## Unit Management

### Unit Types

```csharp
namespace ProjectLiberty.Dispatch
{
    /// <summary>
    /// Unit type.
    /// </summary>
    public enum UnitType
    {
        Patrol,
        SWAT,
        Traffic,
        K9,
        Air,
        Marine
    }
    
    /// <summary>
    /// Unit status.
    /// </summary>
    public enum UnitStatus
    {
        Available,
        Busy,
        Offline,
        EnRoute,
        OnScene,
        InPursuit
    }
    
    /// <summary>
    /// Dispatch unit.
    /// </summary>
    public class DispatchUnit
    {
        public int Handle { get; set; }
        public string CallSign { get; set; }
        public UnitType Type { get; set; }
        public UnitStatus Status { get; set; }
        public Vector3 Position { get; set; }
        public int CurrentChannel { get; set; }
        public Guid? CurrentRequest { get; set; }
        public DateTime LastUpdate { get; set; }
    }
}
```

### Unit Manager

```csharp
namespace ProjectLiberty.Dispatch
{
    /// <summary>
    /// Unit manager interface.
    /// </summary>
    public interface IUnitManager
    {
        /// <summary>
        /// Registers a unit.
        /// </summary>
        /// <param name="unit">Unit to register.</param>
        void RegisterUnit(DispatchUnit unit);
        
        /// <summary>
        /// Unregisters a unit.
        /// </summary>
        /// <param name="unitHandle">Unit handle.</param>
        void UnregisterUnit(int unitHandle);
        
        /// <summary>
        /// Gets a unit by handle.
        /// </summary>
        /// <param name="unitHandle">Unit handle.</param>
        /// <returns>Unit, or null if not found.</returns>
        DispatchUnit GetUnit(int unitHandle);
        
        /// <summary>
        /// Gets all units.
        /// </summary>
        /// <returns>All units.</returns>
        IEnumerable<DispatchUnit> GetAllUnits();
        
        /// <summary>
        /// Gets available units.
        /// </summary>
        /// <returns>Available units.</returns>
        IEnumerable<DispatchUnit> GetAvailableUnits();
        
        /// <summary>
        /// Gets units in radius.
        /// </summary>
        /// <param name="position">Center position.</param>
        /// <param name="radius">Search radius.</param>
        /// <returns>Units in radius.</returns>
        IEnumerable<DispatchUnit> GetUnitsInRadius(Vector3 position, float radius);
        
        /// <summary>
        /// Updates unit status.
        /// </summary>
        /// <param name="unitHandle">Unit handle.</param>
        /// <param name="status">New status.</param>
        void UpdateStatus(int unitHandle, UnitStatus status);
        
        /// <summary>
        /// Assigns unit to request.
        /// </summary>
        /// <param name="unitHandle">Unit handle.</param>
        /// <param name="requestId">Request ID.</param>
        void AssignUnit(int unitHandle, Guid requestId);
        
        /// <summary>
        /// Releases unit from request.
        /// </summary>
        /// <param name="unitHandle">Unit handle.</param>
        void ReleaseUnit(int unitHandle);
        
        /// <summary>
        /// Gets units by request.
        /// </summary>
        /// <param name="requestId">Request ID.</param>
        /// <returns>Units assigned to request.</returns>
        IEnumerable<DispatchUnit> GetUnitsByRequest(Guid requestId);
    }
}
```

---

## Call Codes

### Call Code Interface

```csharp
namespace ProjectLiberty.Dispatch
{
    /// <summary>
    /// Call code interface.
    /// </summary>
    public interface ICallCode
    {
        /// <summary>
        /// Gets the code.
        /// </summary>
        string Code { get; }
        
        /// <summary>
        /// Gets the description.
        /// </summary>
        string Description { get; }
        
        /// <summary>
        /// Gets the priority.
        /// </summary>
        CallCodePriority Priority { get; }
        
        /// <summary>
        /// Gets the category.
        /// </summary>
        CallCodeCategory Category { get; }
    }
    
    /// <summary>
    /// Call code priority.
    /// </summary>
    public enum CallCodePriority
    {
        Low,
        Medium,
        High,
        Emergency
    }
    
    /// <summary>
    /// Call code category.
    /// </summary>
    public enum CallCodeCategory
    {
        General,
        Pursuit,
        Crime,
        Traffic,
        Emergency
    }
}
```

### Default Call Codes

```csharp
namespace ProjectLiberty.Dispatch
{
    /// <summary>
    /// Default call codes.
    /// </summary>
    public static class DefaultCallCodes
    {
        public static readonly ICallCode TenFour = new CallCode("10-4", "Acknowledged", CallCodePriority.Low, CallCodeCategory.General);
        public static readonly ICallCode TenTen = new CallCode("10-10", "Pursuit in progress", CallCodePriority.High, CallCodeCategory.Pursuit);
        public static readonly ICallCode TenEleven = new CallCode("10-11", "Robbery in progress", CallCodePriority.Emergency, CallCodeCategory.Crime);
        public static readonly ICallCode TenTwelve = new CallCode("10-12", "Assault in progress", CallCodePriority.Emergency, CallCodeCategory.Crime);
        public static readonly ICallCode TenFifty = new CallCode("10-50", "Traffic stop", CallCodePriority.Medium, CallCodeCategory.Traffic);
        public static readonly ICallCode TenFiftyFour = new CallCode("10-54", "Suspicious vehicle", CallCodePriority.Medium, CallCodeCategory.General);
        public static readonly ICallCode TenThirtyThree = new CallCode("10-33", "Emergency traffic only", CallCodePriority.Emergency, CallCodeCategory.Emergency);
        
        private class CallCode : ICallCode
        {
            public string Code { get; }
            public string Description { get; }
            public CallCodePriority Priority { get; }
            public CallCodeCategory Category { get; }
            
            public CallCode(string code, string description, CallCodePriority priority, CallCodeCategory category)
            {
                Code = code;
                Description = description;
                Priority = priority;
                Category = category;
            }
        }
    }
}
```

### Call Code Registry

```csharp
namespace ProjectLiberty.Dispatch
{
    /// <summary>
    /// Call code registry interface.
    /// </summary>
    public interface ICallCodeRegistry
    {
        /// <summary>
        /// Registers a call code.
        /// </summary>
        /// <param name="callCode">Call code to register.</param>
        void RegisterCallCode(ICallCode callCode);
        
        /// <summary>
        /// Gets a call code by code string.
        /// </summary>
        /// <param name="code">Code string.</param>
        /// <returns>Call code, or null if not found.</returns>
        ICallCode GetCallCode(string code);
        
        /// <summary>
        /// Gets all call codes.
        /// </summary>
        /// <returns>All call codes.</returns>
        IEnumerable<ICallCode> GetAllCallCodes();
        
        /// <summary>
        /// Gets call codes by category.
        /// </summary>
        /// <param name="category">Category.</param>
        /// <returns>Call codes in category.</returns>
        IEnumerable<ICallCode> GetCallCodesByCategory(CallCodeCategory category);
    }
}
```

---

## Dispatch Events

### Event Definitions

```csharp
namespace ProjectLiberty.Events
{
    /// <summary>
    /// Backup requested event.
    /// </summary>
    public class BackupRequestedEvent : BaseEvent
    {
        public Vector3 Location { get; set; }
        public BackupPriority Priority { get; set; }
        public BackupType Type { get; set; }
        public int RequestingUnitHandle { get; set; }
    }
    
    /// <summary>
    /// Backup dispatched event.
    /// </summary>
    public class BackupDispatchedEvent : BaseEvent
    {
        public Guid RequestId { get; set; }
        public int UnitHandle { get; set; }
        public Vector3 Location { get; set; }
    }
    
    /// <summary>
    /// Backup arrived event.
    /// </summary>
    public class BackupArrivedEvent : BaseEvent
    {
        public int UnitHandle { get; set; }
        public Vector3 Location { get; set; }
    }
    
    /// <summary>
    /// Unit status changed event.
    /// </summary>
    public class UnitStatusChangedEvent : BaseEvent
    {
        public int UnitHandle { get; set; }
        public UnitStatus NewStatus { get; set; }
    }
    
    /// <summary>
    /// Dispatch message event.
    /// </summary>
    public class DispatchMessageEvent : BaseEvent
    {
        public string Message { get; set; }
        public DispatchMessageType Type { get; set; }
        public Vector3? Location { get; set; }
    }
    
    /// <summary>
    /// Dispatch message type.
    /// </summary>
    public enum DispatchMessageType
    {
        General,
        Emergency,
        Traffic,
        Weather,
        Administrative
    }
}
```

---

## Best Practices

### 1. Use Radio for Communication

```csharp
// GOOD - Use radio
_radioSystem.Announce("Backup requested at location");

// BAD - Direct messaging
_unit.SendMessage("Backup requested");
```

### 2. Track Unit Status

```csharp
// GOOD - Update status
_unitManager.UpdateStatus(unitHandle, UnitStatus.EnRoute);

// BAD - Don't track status
// Unit status unknown
```

### 3. Use Standard Call Codes

```csharp
// GOOD - Use standard codes
var code = _callCodeRegistry.GetCallCode("10-10");
_radioSystem.Announce($"{code.Code}: {code.Description}");

// BAD - Custom codes
_radioSystem.Announce("Pursuit happening");
```

### 4. Handle No Units Available

```csharp
// GOOD - Handle no units
var units = _unitManager.GetAvailableUnits();
if (!units.Any())
{
    _radioSystem.Announce(_localization.GetString("Dispatch.NoUnitsAvailable"));
    return;
}

// BAD - Assume units available
var unit = _unitManager.GetAvailableUnits().First();
```

### 5. Release Units When Done

```csharp
// GOOD - Release unit
_unitManager.ReleaseUnit(unitHandle);

// BAD - Leave unit assigned
// Unit remains assigned indefinitely
```

### 6. Use Appropriate Priority

```csharp
// GOOD - Appropriate priority
var priority = isEmergency ? BackupPriority.Emergency : BackupPriority.Medium;
RequestBackup(location, priority, type, unitHandle);

// BAD - Always high priority
RequestBackup(location, BackupPriority.High, type, unitHandle);
```

---

## Summary

The Dispatch System provides:

- **Independent operation** from other game systems
- **Radio-based communication** for realism
- **Unit management** for tracking officers
- **Standard call codes** for consistency
- **Event-driven architecture** for loose coupling
- **Backup request system** for officer assistance

The dispatch system operates independently and communicates only through events.
