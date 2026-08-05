# Scripting API

**Scripting API Documentation**

---

## Table of Contents

1. [Overview](#overview)
2. [Scripting Architecture](#scripting-architecture)
3. [Script Engine](#script-engine)
4. [Scripting API Reference](#scripting-api-reference)
5. [Script Examples](#script-examples)
6. [Best Practices](#best-practices)

---

## Overview

The Scripting API allows developers to write custom game scenarios using Lua scripts without modifying the core system. Scripts can interact with game systems through a safe, sandboxed API.

### Architecture

```
┌─────────────────────────────────────────┐
│         Lua Script                      │
│  (Custom scenario logic)                │
└─────────────────────────────────────────┘
                  ↓
┌─────────────────────────────────────────┐
│         Script Engine                   │
│  (Lua runtime, sandbox)                 │
└─────────────────────────────────────────┘
                  ↓
┌─────────────────────────────────────────┐
│         Scripting API                  │
│  (Safe API wrapper)                     │
└─────────────────────────────────────────┘
                  ↓
┌─────────────────────────────────────────┐
│         Core Systems                   │
│  (Player, Vehicle, World, etc.)         │
└─────────────────────────────────────────┘
```

---

## Scripting Architecture

### Design Principles

1. **Sandboxed**: Scripts run in a secure sandbox
2. **Safe API**: Limited access to game systems
3. **Event-Driven**: Scripts respond to events
4. **Hot Reload**: Scripts can be reloaded without restart
5. **Error Handling**: Script errors don't crash the game

### Security

- No direct native calls
- No file system access
- No network access
- Limited memory access
- Time-limited execution

---

## Script Engine

### Engine Interface

```csharp
namespace ProjectLiberty.Scripting
{
    /// <summary>
    /// Script engine interface.
    /// </summary>
    public interface IScriptEngine
    {
        /// <summary>
        /// Loads a script.
        /// </summary>
        /// <param name="scriptPath">Script file path.</param>
        /// <returns>Script ID.</returns>
        string LoadScript(string scriptPath);
        
        /// <summary>
        /// Unloads a script.
        /// </summary>
        /// <param name="scriptId">Script ID.</param>
        void UnloadScript(string scriptId);
        
        /// <summary>
        /// Reloads a script.
        /// </summary>
        /// <param name="scriptId">Script ID.</param>
        void ReloadScript(string scriptId);
        
        /// <summary>
        /// Executes a script function.
        /// </summary>
        /// <param name="scriptId">Script ID.</param>
        /// <param name="functionName">Function name.</param>
        /// <param name="args">Function arguments.</param>
        /// <returns>Function result.</returns>
        object ExecuteFunction(string scriptId, string functionName, params object[] args);
        
        /// <summary>
        /// Gets a script by ID.
        /// </summary>
        /// <param name="scriptId">Script ID.</param>
        /// <returns>Script, or null if not found.</returns>
        IScript GetScript(string scriptId);
        
        /// <summary>
        /// Gets all loaded scripts.
        /// </summary>
        /// <returns>All loaded scripts.</returns>
        IEnumerable<IScript> GetAllScripts();
    }
}
```

### Script Interface

```csharp
namespace ProjectLiberty.Scripting
{
    /// <summary>
    /// Script interface.
    /// </summary>
    public interface IScript
    {
        /// <summary>
        /// Gets the script ID.
        /// </summary>
        string Id { get; }
        
        /// <summary>
        /// Gets the script name.
        /// </summary>
        string Name { get; }
        
        /// <summary>
        /// Gets the script path.
        /// </summary>
        string Path { get; }
        
        /// <summary>
        /// Gets whether the script is loaded.
        /// </summary>
        bool IsLoaded { get; }
        
        /// <summary>
        /// Gets the script context.
        /// </summary>
        IScriptContext Context { get; }
    }
}
```

---

## Scripting API Reference

### Player API

```lua
-- Get local player
local player = API.Player.GetLocal()

-- Get player position
local position = player:GetPosition()

-- Set player position
player:SetPosition(x, y, z)

-- Get player health
local health = player:GetHealth()

-- Set player health
player:SetHealth(health)

-- Get player weapon
local weapon = player:GetCurrentWeapon()

-- Give player weapon
player:GiveWeapon(weaponHash, ammo)

-- Remove player weapon
player:RemoveWeapon(weaponHash)
```

### Vehicle API

```lua
-- Create vehicle
local vehicle = API.Vehicle.Create(modelHash, x, y, z, heading)

-- Get vehicle position
local position = vehicle:GetPosition()

-- Set vehicle position
vehicle:SetPosition(x, y, z)

-- Get vehicle speed
local speed = vehicle:GetSpeed()

-- Set vehicle speed
vehicle:SetSpeed(speed)

-- Delete vehicle
vehicle:Delete()

-- Get all vehicles
local vehicles = API.Vehicle.GetAll()
```

### Ped API

```lua
-- Create ped
local ped = API.Ped.Create(modelHash, x, y, z, heading)

-- Get ped position
local position = ped:GetPosition()

-- Set ped position
ped:SetPosition(x, y, z)

-- Get ped health
local health = ped:GetHealth()

-- Set ped health
ped:SetHealth(health)

-- Make ped follow player
ped:FollowPlayer()

-- Make ped wander
ped:Wander()

-- Delete ped
ped:Delete()
```

### World API

```lua
-- Get game time
local time = API.World.GetTime()

-- Set game time
API.World.SetTime(hour, minute, second)

-- Get weather
local weather = API.World.GetWeather()

-- Set weather
API.World.SetWeather(weatherType)

-- Create waypoint
API.World.SetWaypoint(x, y, z)

-- Clear waypoint
API.World.ClearWaypoint()

-- Spawn vehicle at position
local vehicle = API.World.SpawnVehicle(modelHash, x, y, z)
```

### Dispatch API

```lua
-- Request backup
API.Dispatch.RequestBackup(x, y, z, priority, type)

-- Send dispatch message
API.Dispatch.SendMessage(message, messageType, x, y, z)

-- Get available units
local units = API.Dispatch.GetAvailableUnits()

-- Update unit status
API.Dispatch.UpdateUnitStatus(unitHandle, status)
```

### Event API

```lua
-- Subscribe to event
API.Event.Subscribe("PlayerSpawned", function(data)
    print("Player spawned at", data.x, data.y, data.z)
end)

-- Unsubscribe from event
API.Event.Unsubscribe("PlayerSpawned", handler)

-- Publish event
API.Event.Publish("CustomEvent", { key = value })
```

### Utility API

```lua
-- Get distance between two positions
local distance = API.Utility.GetDistance(x1, y1, z1, x2, y2, z2)

-- Get random number
local random = API.Utility.Random(min, max)

-- Wait for specified time
API.Utility.Wait(seconds)

-- Log message
API.Utility.Log(message, level)

-- Show notification
API.Utility.ShowNotification(message)
```

---

## Script Examples

### Example 1: Simple Teleport Script

```lua
-- teleport.lua

local function TeleportPlayer(x, y, z)
    local player = API.Player.GetLocal()
    if player then
        player:SetPosition(x, y, z)
        API.Utility.ShowNotification("Teleported to " .. x .. ", " .. y .. ", " .. z)
    end
end

-- Register command
API.Command.Register("tp", "Teleport to position", function(args)
    local x = tonumber(args[1])
    local y = tonumber(args[2])
    local z = tonumber(args[3])
    
    if x and y and z then
        TeleportPlayer(x, y, z)
    else
        API.Utility.ShowNotification("Usage: /tp x y z")
    end
end)
```

### Example 2: Vehicle Spawn Script

```lua
-- vehicle_spawn.lua

local function SpawnVehicle(model, x, y, z)
    local vehicle = API.Vehicle.Create(model, x, y, z, 0)
    if vehicle then
        API.Utility.ShowNotification("Vehicle spawned: " .. model)
        return vehicle
    else
        API.Utility.ShowNotification("Failed to spawn vehicle")
        return nil
    end
end

-- Register command
API.Command.Register("spawnveh", "Spawn vehicle", function(args)
    local model = args[1]
    local player = API.Player.GetLocal()
    local pos = player:GetPosition()
    
    if model then
        SpawnVehicle(model, pos.x, pos.y, pos.z)
    else
        API.Utility.ShowNotification("Usage: /spawnveh model")
    end
end)
```

### Example 3: Event Handler Script

```lua
-- event_handler.lua

-- Handle player spawn
API.Event.Subscribe("PlayerSpawned", function(data)
    API.Utility.Log("Player spawned at " .. data.x .. ", " .. data.y .. ", " .. data.z)
    API.Utility.ShowNotification("Welcome to Los Santos!")
end)

-- Handle player death
API.Event.Subscribe("PlayerDied", function(data)
    API.Utility.Log("Player died at " .. data.x .. ", " .. data.y .. ", " .. data.z)
    API.Utility.ShowNotification("Wasted!")
end)

-- Handle vehicle created
API.Event.Subscribe("VehicleCreated", function(data)
    API.Utility.Log("Vehicle created: " .. data.model .. " at " .. data.x .. ", " .. data.y)
end)
```

### Example 4: Custom Scenario Script

```lua
-- custom_scenario.lua

local scenarioActive = false
local suspectVehicle = nil
local suspectPed = nil

local function StartScenario()
    local player = API.Player.GetLocal()
    local pos = player:GetPosition()
    
    -- Spawn suspect vehicle
    suspectVehicle = API.Vehicle.Create("adder", pos.x + 20, pos.y + 20, pos.z, 90)
    
    -- Spawn suspect ped
    suspectPed = API.Ped.Create("s_m_y_cop_01", pos.x + 20, pos.y + 20, pos.z, 90)
    
    -- Make ped flee
    suspectPed:Flee()
    
    scenarioActive = true
    API.Utility.ShowNotification("Scenario started: Pursuit!")
end

local function EndScenario()
    if suspectVehicle then
        suspectVehicle:Delete()
        suspectVehicle = nil
    end
    
    if suspectPed then
        suspectPed:Delete()
        suspectPed = nil
    end
    
    scenarioActive = false
    API.Utility.ShowNotification("Scenario ended")
end

-- Register commands
API.Command.Register("start_scenario", "Start custom scenario", function(args)
    if not scenarioActive then
        StartScenario()
    else
        API.Utility.ShowNotification("Scenario already active")
    end
end)

API.Command.Register("end_scenario", "End custom scenario", function(args)
    if scenarioActive then
        EndScenario()
    else
        API.Utility.ShowNotification("No active scenario")
    end
end)
```

---

## Best Practices

### 1. Error Handling

```lua
-- GOOD - Error handling
local function SafeTeleport(x, y, z)
    local success, err = pcall(function()
        local player = API.Player.GetLocal()
        if player then
            player:SetPosition(x, y, z)
        end
    end)
    
    if not success then
        API.Utility.Log("Teleport failed: " .. err, "Error")
    end
end

-- BAD - No error handling
local function UnsafeTeleport(x, y, z)
    local player = API.Player.GetLocal()
    player:SetPosition(x, y, z) -- May throw
end
```

### 2. Resource Cleanup

```lua
-- GOOD - Cleanup resources
local function Cleanup()
    if vehicle then
        vehicle:Delete()
        vehicle = nil
    end
    
    if ped then
        ped:Delete()
        ped = nil
    end
end

-- BAD - No cleanup
local function NoCleanup()
    local vehicle = API.Vehicle.Create(...)
    -- Vehicle never deleted
end
```

### 3. Use Events

```lua
-- GOOD - Use events
API.Event.Subscribe("PlayerSpawned", function(data)
    -- Handle spawn
end)

-- BAD - Polling
while true do
    local player = API.Player.GetLocal()
    if player then
        -- Check spawn
    end
    API.Utility.Wait(0.1)
end
```

### 4. Validate Inputs

```lua
-- GOOD - Validate inputs
local function Teleport(x, y, z)
    if type(x) ~= "number" or type(y) ~= "number" or type(z) ~= "number" then
        API.Utility.ShowNotification("Invalid coordinates")
        return
    end
    
    local player = API.Player.GetLocal()
    player:SetPosition(x, y, z)
end

-- BAD - No validation
local function Teleport(x, y, z)
    local player = API.Player.GetLocal()
    player:SetPosition(x, y, z) -- May crash with invalid input
end
```

### 5. Use Descriptive Names

```lua
-- GOOD - Descriptive names
local function CreateSuspectVehicle(position)
    local vehicle = API.Vehicle.Create("adder", position.x, position.y, position.z, 90)
    return vehicle
end

-- BAD - Generic names
local function Create(p)
    local v = API.Vehicle.Create("adder", p.x, p.y, p.z, 90)
    return v
end
```

### 6. Limit Execution Time

```lua
-- GOOD - Time-limited
local function ProcessWithTimeout(func, timeout)
    local startTime = os.clock()
    
    func()
    
    if os.clock() - startTime > timeout then
        API.Utility.Log("Script execution timeout", "Warning")
    end
end

-- BAD - No timeout
local function Process(func)
    func() -- May run forever
end
```

### 7. Use Comments

```lua
-- GOOD - Comments
-- Spawn suspect vehicle at offset position
local vehicle = API.Vehicle.Create("adder", pos.x + 20, pos.y + 20, pos.z, 90)

-- BAD - No comments
local vehicle = API.Vehicle.Create("adder", pos.x + 20, pos.y + 20, pos.z, 90)
```

---

## Summary

The Scripting API provides:

- **Lua-based scripting** for custom scenarios
- **Sandboxed execution** for security
- **Safe API wrapper** for game system access
- **Event-driven architecture** for responsiveness
- **Hot reload support** for development
- **Comprehensive API** for all game systems

Scripts allow developers to create custom scenarios without modifying the core system.
