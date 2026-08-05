# Game Host Abstraction

**Complete Game Host Abstraction Layer Documentation**

---

## Table of Contents

1. [Overview](#overview)
2. [Design Principles](#design-principles)
3. [Interface Definitions](#interface-definitions)
4. [Implementation Guide](#implementation-guide)
5. [Usage Examples](#usage-examples)
6. [Testing](#testing)
7. [Performance Considerations](#performance-considerations)

---

## Overview

The Game Host Abstraction Layer is the critical boundary between Project Liberty's core systems and the GTA V game engine. This layer ensures that:

- **Core systems are loader-agnostic**: Work with ScriptHookVDotNet, SHVDN3, or future loaders
- **No direct native calls**: All game interaction goes through abstractions
- **Testable**: Core systems can be unit tested without the game
- **Portable**: Easy to port to other games or platforms

### Architecture

```
┌─────────────────────────────────────────┐
│         Core Game Systems                │
│  (PlayerManager, VehicleManager, etc.)   │
└─────────────────────────────────────────┘
                  ↓
┌─────────────────────────────────────────┐
│      Abstraction Interfaces              │
│  (IPlayerManager, IVehicleManager)      │
└─────────────────────────────────────────┘
                  ↓
┌─────────────────────────────────────────┐
│         Game Host Abstraction           │
│  (IGameHost, INativeBridge, etc.)       │
└─────────────────────────────────────────┘
                  ↓
┌─────────────────────────────────────────┐
│      Game Host Implementation            │
│  (ScriptHookHost, CustomHost, etc.)     │
└─────────────────────────────────────────┘
                  ↓
┌─────────────────────────────────────────┐
│           GTA V Game Engine             │
└─────────────────────────────────────────┘
```

---

## Design Principles

### 1. Complete Abstraction

**FORBIDDEN**: Direct calls to ScriptHookVDotNet API in core systems

```csharp
// FORBIDDEN - This is in core system
using GTA;

public class PlayerManager
{
    public void TeleportPlayer(Vector3 position)
    {
        // BAD - Direct SHVDN call
        Game.Player.Character.Position = position;
    }
}
```

**REQUIRED**: All calls through abstraction

```csharp
// REQUIRED - Through abstraction
public class PlayerManager
{
    private readonly INativeInvoker _nativeInvoker;
    
    public void TeleportPlayer(Vector3 position)
    {
        // GOOD - Through abstraction
        _nativeInvoker.CallHash(NativeHash.SET_ENTITY_COORDS, 
            player.Handle, 
            position.X, 
            position.Y, 
            position.Z, 
            false, 
            false, 
            false, 
            false);
    }
}
```

### 2. Interface Segregation

Each interface is focused and minimal:

```csharp
// Focused interface for player operations
public interface IPlayerManager
{
    IPlayer GetLocalPlayer();
    void TeleportPlayer(IPlayer player, Vector3 position);
}

// Separate interface for vehicle operations
public interface IVehicleManager
{
    IVehicle CreateVehicle(VehicleModel model, Vector3 position, float heading);
    void DeleteVehicle(IVehicle vehicle);
}
```

### 3. Dependency Inversion

Core systems depend on abstractions, not implementations:

```csharp
// Core system depends on interface
public class TrafficStopService
{
    private readonly IPlayerManager _playerManager;
    private readonly IVehicleManager _vehicleManager;
    
    public TrafficStopService(
        IPlayerManager playerManager,
        IVehicleManager vehicleManager)
    {
        _playerManager = playerManager;
        _vehicleManager = vehicleManager;
    }
}
```

---

## Interface Definitions

### IGameHost

Main game host interface - the entry point for all game interaction.

```csharp
namespace ProjectLiberty.Abstractions
{
    /// <summary>
    /// Main game host interface providing access to game systems.
    /// </summary>
    public interface IGameHost
    {
        /// <summary>
        /// Gets the game version string.
        /// </summary>
        string GameVersion { get; }
        
        /// <summary>
        /// Gets whether the game is currently running.
        /// </summary>
        bool IsRunning { get; }
        
        /// <summary>
        /// Gets whether the game has finished loading.
        /// </summary>
        bool IsLoaded { get; }
        
        /// <summary>
        /// Initializes the game host.
        /// </summary>
        void Initialize();
        
        /// <summary>
        /// Shuts down the game host.
        /// </summary>
        void Shutdown();
        
        /// <summary>
        /// Called each game tick.
        /// </summary>
        void OnTick();
        
        /// <summary>
        /// Gets the native bridge for native function calls.
        /// </summary>
        INativeBridge NativeBridge { get; }
        
        /// <summary>
        /// Gets the game loop for tick and input management.
        /// </summary>
        IGameLoop GameLoop { get; }
        
        /// <summary>
        /// Gets the entity manager for entity operations.
        /// </summary>
        IEntityManager EntityManager { get; }
        
        /// <summary>
        /// Gets the player manager for player operations.
        /// </summary>
        IPlayerManager PlayerManager { get; }
        
        /// <summary>
        /// Gets the vehicle manager for vehicle operations.
        /// </summary>
        IVehicleManager VehicleManager { get; }
        
        /// <summary>
        /// Gets the ped manager for ped operations.
        /// </summary>
        IPedManager PedManager { get; }
        
        /// <summary>
        /// Gets the world manager for world operations.
        /// </summary>
        IWorldManager WorldManager { get; }
        
        /// <summary>
        /// Gets the native invoker for native function calls.
        /// </summary>
        INativeInvoker NativeInvoker { get; }
    }
}
```

### INativeBridge

Bridge to native GTA V functions - abstracts the native call mechanism.

```csharp
namespace ProjectLiberty.Abstractions
{
    /// <summary>
    /// Bridge to native GTA V functions.
    /// </summary>
    public interface INativeBridge
    {
        /// <summary>
        /// Invokes a native function with a return value.
        /// </summary>
        /// <typeparam name="T">Return type.</typeparam>
        /// <param name="hash">Native function hash.</param>
        /// <param name="args">Function arguments.</param>
        /// <returns>Native function result.</returns>
        T Invoke<T>(ulong hash, params object[] args);
        
        /// <summary>
        /// Invokes a native function without a return value.
        /// </summary>
        /// <param name="hash">Native function hash.</param>
        /// <param name="args">Function arguments.</param>
        void Invoke(ulong hash, params object[] args);
        
        /// <summary>
        /// Checks if a native function is available.
        /// </summary>
        /// <param name="hash">Native function hash.</param>
        /// <returns>True if available, false otherwise.</returns>
        bool IsNativeAvailable(ulong hash);
        
        /// <summary>
        /// Gets the hash for a native function name.
        /// </summary>
        /// <param name="name">Native function name.</param>
        /// <returns>Native function hash.</returns>
        ulong GetNativeHash(string name);
        
        /// <summary>
        /// Gets the name for a native function hash.
        /// </summary>
        /// <param name="hash">Native function hash.</param>
        /// <returns>Native function name, or null if not found.</returns>
        string GetNativeName(ulong hash);
    }
}
```

### IGameLoop

Game loop management for tick and input handling.

```csharp
namespace ProjectLiberty.Abstractions
{
    /// <summary>
    /// Game loop management interface.
    /// </summary>
    public interface IGameLoop
    {
        /// <summary>
        /// Registers an action to be called each tick.
        /// </summary>
        /// <param name="action">Action to register.</param>
        void RegisterTickAction(Action action);
        
        /// <summary>
        /// Unregisters a tick action.
        /// </summary>
        /// <param name="action">Action to unregister.</param>
        void UnregisterTickAction(Action action);
        
        /// <summary>
        /// Registers an action to be called when a key is pressed.
        /// </summary>
        /// <param name="key">Key to listen for.</param>
        /// <param name="action">Action to call.</param>
        void RegisterKeyDownAction(Keys key, Action action);
        
        /// <summary>
        /// Unregisters a key down action.
        /// </summary>
        /// <param name="key">Key to unregister.</param>
        /// <param name="action">Action to unregister.</param>
        void UnregisterKeyDownAction(Keys key, Action action);
        
        /// <summary>
        /// Registers an action to be called when a key is released.
        /// </summary>
        /// <param name="key">Key to listen for.</param>
        /// <param name="action">Action to call.</param>
        void RegisterKeyUpAction(Keys key, Action action);
        
        /// <summary>
        /// Unregisters a key up action.
        /// </summary>
        /// <param name="key">Key to unregister.</param>
        /// <param name="action">Action to unregister.</param>
        void UnregisterKeyUpAction(Keys key, Action action);
        
        /// <summary>
        /// Gets the current frame number.
        /// </summary>
        int CurrentFrame { get; }
        
        /// <summary>
        /// Gets the delta time since last frame in seconds.
        /// </summary>
        float DeltaTime { get; }
        
        /// <summary>
        /// Gets the total time since game start in seconds.
        /// </summary>
        float TotalTime { get; }
        
        /// <summary>
        /// Gets the current game time.
        /// </summary>
        DateTime GameTime { get; }
    }
}
```

### IEntityManager

Entity management interface.

```csharp
namespace ProjectLiberty.Abstractions
{
    /// <summary>
    /// Entity manager interface.
    /// </summary>
    public interface IEntityManager
    {
        /// <summary>
        /// Gets an entity by handle.
        /// </summary>
        /// <param name="handle">Entity handle.</param>
        /// <returns>Entity, or null if not found.</returns>
        IEntity GetEntity(int handle);
        
        /// <summary>
        /// Gets all entities.
        /// </summary>
        /// <returns>All entities.</returns>
        IEnumerable<IEntity> GetAllEntities();
        
        /// <summary>
        /// Checks if an entity exists.
        /// </summary>
        /// <param name="handle">Entity handle.</param>
        /// <returns>True if exists, false otherwise.</returns>
        bool EntityExists(int handle);
        
        /// <summary>
        /// Destroys an entity.
        /// </summary>
        /// <param name="handle">Entity handle.</param>
        void DestroyEntity(int handle);
        
        /// <summary>
        /// Gets entities within a radius.
        /// </summary>
        /// <param name="position">Center position.</param>
        /// <param name="radius">Search radius.</param>
        /// <returns>Entities within radius.</returns>
        IEnumerable<IEntity> GetEntitiesInRadius(Vector3 position, float radius);
    }
}
```

### IPlayerManager

Player management interface.

```csharp
namespace ProjectLiberty.Abstractions
{
    /// <summary>
    /// Player manager interface.
    /// </summary>
    public interface IPlayerManager
    {
        /// <summary>
        /// Gets the local player.
        /// </summary>
        /// <returns>Local player, or null if not available.</returns>
        IPlayer GetLocalPlayer();
        
        /// <summary>
        /// Gets all players.
        /// </summary>
        /// <returns>All players.</returns>
        IEnumerable<IPlayer> GetAllPlayers();
        
        /// <summary>
        /// Spawns a player at the specified position.
        /// </summary>
        /// <param name="position">Spawn position.</param>
        /// <param name="heading">Spawn heading in degrees.</param>
        /// <returns>Spawned player.</returns>
        IPlayer SpawnPlayer(Vector3 position, float heading);
        
        /// <summary>
        /// Teleports a player to the specified position.
        /// </summary>
        /// <param name="player">Player to teleport.</param>
        /// <param name="position">Target position.</param>
        void TeleportPlayer(IPlayer player, Vector3 position);
        
        /// <summary>
        /// Gets a player by handle.
        /// </summary>
        /// <param name="handle">Player handle.</param>
        /// <returns>Player, or null if not found.</returns>
        IPlayer GetPlayer(int handle);
        
        /// <summary>
        /// Checks if a player exists.
        /// </summary>
        /// <param name="handle">Player handle.</param>
        /// <returns>True if exists, false otherwise.</returns>
        bool PlayerExists(int handle);
    }
}
```

### IVehicleManager

Vehicle management interface.

```csharp
namespace ProjectLiberty.Abstractions
{
    /// <summary>
    /// Vehicle manager interface.
    /// </summary>
    public interface IVehicleManager
    {
        /// <summary>
        /// Creates a vehicle at the specified position.
        /// </summary>
        /// <param name="model">Vehicle model.</param>
        /// <param name="position">Spawn position.</param>
        /// <param name="heading">Spawn heading in degrees.</param>
        /// <returns>Created vehicle.</returns>
        IVehicle CreateVehicle(VehicleModel model, Vector3 position, float heading);
        
        /// <summary>
        /// Gets a vehicle by handle.
        /// </summary>
        /// <param name="handle">Vehicle handle.</param>
        /// <returns>Vehicle, or null if not found.</returns>
        IVehicle GetVehicle(int handle);
        
        /// <summary>
        /// Gets all vehicles.
        /// </summary>
        /// <returns>All vehicles.</returns>
        IEnumerable<IVehicle> GetAllVehicles();
        
        /// <summary>
        /// Deletes a vehicle.
        /// </summary>
        /// <param name="vehicle">Vehicle to delete.</param>
        void DeleteVehicle(IVehicle vehicle);
        
        /// <summary>
        /// Checks if a vehicle exists.
        /// </summary>
        /// <param name="handle">Vehicle handle.</param>
        /// <returns>True if exists, false otherwise.</returns>
        bool VehicleExists(int handle);
        
        /// <summary>
        /// Gets vehicles within a radius.
        /// </summary>
        /// <param name="position">Center position.</param>
        /// <param name="radius">Search radius.</param>
        /// <returns>Vehicles within radius.</returns>
        IEnumerable<IVehicle> GetVehiclesInRadius(Vector3 position, float radius);
    }
}
```

### IPedManager

Ped management interface.

```csharp
namespace ProjectLiberty.Abstractions
{
    /// <summary>
    /// Ped manager interface.
    /// </summary>
    public interface IPedManager
    {
        /// <summary>
        /// Creates a ped at the specified position.
        /// </summary>
        /// <param name="model">Ped model.</param>
        /// <param name="position">Spawn position.</param>
        /// <param name="heading">Spawn heading in degrees.</param>
        /// <returns>Created ped.</returns>
        IPed CreatePed(PedModel model, Vector3 position, float heading);
        
        /// <summary>
        /// Gets a ped by handle.
        /// </summary>
        /// <param name="handle">Ped handle.</param>
        /// <returns>Ped, or null if not found.</returns>
        IPed GetPed(int handle);
        
        /// <summary>
        /// Gets all peds.
        /// </summary>
        /// <returns>All peds.</returns>
        IEnumerable<IPed> GetAllPeds();
        
        /// <summary>
        /// Deletes a ped.
        /// </summary>
        /// <param name="ped">Ped to delete.</param>
        void DeletePed(IPed ped);
        
        /// <summary>
        /// Checks if a ped exists.
        /// </summary>
        /// <param name="handle">Ped handle.</param>
        /// <returns>True if exists, false otherwise.</returns>
        bool PedExists(int handle);
        
        /// <summary>
        /// Gets peds within a radius.
        /// </summary>
        /// <param name="position">Center position.</param>
        /// <param name="radius">Search radius.</param>
        /// <returns>Peds within radius.</returns>
        IEnumerable<IPed> GetPedsInRadius(Vector3 position, float radius);
    }
}
```

### IWorldManager

World management interface.

```csharp
namespace ProjectLiberty.Abstractions
{
    /// <summary>
    /// World manager interface.
    /// </summary>
    public interface IWorldManager
    {
        /// <summary>
        /// Gets the current waypoint position.
        /// </summary>
        /// <returns>Waypoint position, or null if not set.</returns>
        Vector3? GetWaypointPosition();
        
        /// <summary>
        /// Sets the waypoint position.
        /// </summary>
        /// <param name="position">Waypoint position.</param>
        void SetWaypointPosition(Vector3 position);
        
        /// <summary>
        /// Clears the waypoint.
        /// </summary>
        void ClearWaypoint();
        
        /// <summary>
        /// Creates a blip at the specified position.
        /// </summary>
        /// <param name="position">Blip position.</param>
        /// <param name="sprite">Blip sprite.</param>
        /// <returns>Created blip.</returns>
        IBlip CreateBlip(Vector3 position, BlipSprite sprite);
        
        /// <summary>
        /// Removes a blip.
        /// </summary>
        /// <param name="blip">Blip to remove.</param>
        void RemoveBlip(IBlip blip);
        
        /// <summary>
        /// Sets the weather.
        /// </summary>
        /// <param name="weather">Weather type.</param>
        void SetWeather(WeatherType weather);
        
        /// <summary>
        /// Gets the current weather.
        /// </summary>
        /// <returns>Current weather type.</returns>
        WeatherType GetWeather();
        
        /// <summary>
        /// Sets the game time.
        /// </summary>
        /// <param name="hour">Hour (0-23).</param>
        /// <param name="minute">Minute (0-59).</param>
        /// <param name="second">Second (0-59).</param>
        void SetTime(int hour, int minute, int second);
        
        /// <summary>
        /// Gets the current game time.
        /// </summary>
        /// <returns>Current game time.</returns>
        TimeSpan GetTime();
    }
}
```

### INativeInvoker

Native function invocation interface - simplified native calling.

```csharp
namespace ProjectLiberty.Abstractions
{
    /// <summary>
    /// Native function invoker interface.
    /// </summary>
    public interface INativeInvoker
    {
        /// <summary>
        /// Calls a native function by name with a return value.
        /// </summary>
        /// <typeparam name="T">Return type.</typeparam>
        /// <param name="nativeName">Native function name.</param>
        /// <param name="args">Function arguments.</param>
        /// <returns>Native function result.</returns>
        T Call<T>(string nativeName, params object[] args);
        
        /// <summary>
        /// Calls a native function by name without a return value.
        /// </summary>
        /// <param name="nativeName">Native function name.</param>
        /// <param name="args">Function arguments.</param>
        void Call(string nativeName, params object[] args);
        
        /// <summary>
        /// Calls a native function by hash with a return value.
        /// </summary>
        /// <typeparam name="T">Return type.</typeparam>
        /// <param name="hash">Native function hash.</param>
        /// <param name="args">Function arguments.</param>
        /// <returns>Native function result.</returns>
        T CallHash<T>(ulong hash, params object[] args);
        
        /// <summary>
        /// Calls a native function by hash without a return value.
        /// </summary>
        /// <param name="hash">Native function hash.</param>
        /// <param name="args">Function arguments.</param>
        void CallHash(ulong hash, params object[] args);
        
        /// <summary>
        /// Checks if a native function is available.
        /// </summary>
        /// <param name="nativeName">Native function name.</param>
        /// <returns>True if available, false otherwise.</returns>
        bool IsAvailable(string nativeName);
    }
}
```

---

## Implementation Guide

### ScriptHookVDotNet Implementation

Example implementation for ScriptHookVDotNet:

```csharp
namespace ProjectLiberty.GameHost
{
    public class ScriptHookHost : IGameHost
    {
        private readonly INativeBridge _nativeBridge;
        private readonly IGameLoop _gameLoop;
        private readonly IEntityManager _entityManager;
        private readonly IPlayerManager _playerManager;
        private readonly IVehicleManager _vehicleManager;
        private readonly IPedManager _pedManager;
        private readonly IWorldManager _worldManager;
        private readonly INativeInvoker _nativeInvoker;
        
        public ScriptHookHost()
        {
            _nativeBridge = new ScriptHookNativeBridge();
            _gameLoop = new ScriptHookGameLoop();
            _entityManager = new EntityManager(_nativeBridge);
            _playerManager = new PlayerManager(_nativeBridge, _entityManager);
            _vehicleManager = new VehicleManager(_nativeBridge, _entityManager);
            _pedManager = new PedManager(_nativeBridge, _entityManager);
            _worldManager = new WorldManager(_nativeBridge);
            _nativeInvoker = new NativeInvoker(_nativeBridge);
        }
        
        public string GameVersion => Game.Version;
        public bool IsRunning => true;
        public bool IsLoaded => !Game.IsLoading;
        
        public INativeBridge NativeBridge => _nativeBridge;
        public IGameLoop GameLoop => _gameLoop;
        public IEntityManager EntityManager => _entityManager;
        public IPlayerManager PlayerManager => _playerManager;
        public IVehicleManager VehicleManager => _vehicleManager;
        public IPedManager PedManager => _pedManager;
        public IWorldManager WorldManager => _worldManager;
        public INativeInvoker NativeInvoker => _nativeInvoker;
        
        public void Initialize()
        {
            // Initialize all systems
        }
        
        public void Shutdown()
        {
            // Shutdown all systems
        }
        
        public void OnTick()
        {
            _gameLoop.ProcessTick();
        }
    }
}
```

### Native Bridge Implementation

```csharp
namespace ProjectLiberty.GameHost
{
    public class ScriptHookNativeBridge : INativeBridge
    {
        public T Invoke<T>(ulong hash, params object[] args)
        {
            return Function.Call<T>(hash, args);
        }
        
        public void Invoke(ulong hash, params object[] args)
        {
            Function.Call(hash, args);
        }
        
        public bool IsNativeAvailable(ulong hash)
        {
            return Function.NativeExists(hash);
        }
        
        public ulong GetNativeHash(string name)
        {
            return Game.GenerateHash(name);
        }
        
        public string GetNativeName(ulong hash)
        {
            return Function.GetNativeName(hash);
        }
    }
}
```

---

## Usage Examples

### Core System Using Abstractions

```csharp
namespace ProjectLiberty.TrafficStops
{
    public class TrafficStopService
    {
        private readonly IPlayerManager _playerManager;
        private readonly IVehicleManager _vehicleManager;
        private readonly INativeInvoker _nativeInvoker;
        
        public TrafficStopService(
            IPlayerManager playerManager,
            IVehicleManager vehicleManager,
            INativeInvoker nativeInvoker)
        {
            _playerManager = playerManager;
            _vehicleManager = vehicleManager;
            _nativeInvoker = nativeInvoker;
        }
        
        public void StartTrafficStop(IVehicle targetVehicle)
        {
            var player = _playerManager.GetLocalPlayer();
            if (player == null)
                return;
            
            // Teleport player behind vehicle
            var position = targetVehicle.Position;
            var behindPosition = position - targetVehicle.ForwardVector * 10;
            _playerManager.TeleportPlayer(player, behindPosition);
            
            // Make player face vehicle
            _nativeInvoker.CallHash(NativeHash.SET_ENTITY_HEADING, 
                player.Handle, 
                targetVehicle.Heading + 180);
        }
    }
}
```

### Unit Testing with Mocks

```csharp
namespace ProjectLiberty.Tests.TrafficStops
{
    [TestFixture]
    public class TrafficStopServiceTests
    {
        private Mock<IPlayerManager> _mockPlayerManager;
        private Mock<IVehicleManager> _mockVehicleManager;
        private Mock<INativeInvoker> _mockNativeInvoker;
        private TrafficStopService _service;
        
        [SetUp]
        public void Setup()
        {
            _mockPlayerManager = new Mock<IPlayerManager>();
            _mockVehicleManager = new Mock<IVehicleManager>();
            _mockNativeInvoker = new Mock<INativeInvoker>();
            
            _service = new TrafficStopService(
                _mockPlayerManager.Object,
                _mockVehicleManager.Object,
                _mockNativeInvoker.Object);
        }
        
        [Test]
        public void StartTrafficStop_TeleportsPlayerBehindVehicle()
        {
            // Arrange
            var mockVehicle = new Mock<IVehicle>();
            var mockPlayer = new Mock<IPlayer>();
            
            mockVehicle.Setup(v => v.Position).Returns(new Vector3(100, 100, 100));
            mockVehicle.Setup(v => v.ForwardVector).Returns(new Vector3(0, 1, 0));
            mockVehicle.Setup(v => v.Heading).Returns(90f);
            
            _mockPlayerManager.Setup(p => p.GetLocalPlayer()).Returns(mockPlayer.Object);
            
            // Act
            _service.StartTrafficStop(mockVehicle.Object);
            
            // Assert
            _mockPlayerManager.Verify(p => p.TeleportPlayer(
                mockPlayer.Object, 
                It.Is<Vector3>(v => v.X == 100 && v.Y == 90 && v.Z == 100)), 
                Times.Once);
        }
    }
}
```

---

## Testing

### Unit Testing Strategy

1. **Mock all abstractions**: Use mocking frameworks to mock interfaces
2. **Test business logic**: Test core logic without game dependencies
3. **Integration testing**: Test with real game host in game environment
4. **Performance testing**: Test native call overhead

### Test Coverage Requirements

- All interface implementations: 100% coverage
- Core systems using abstractions: 90%+ coverage
- Native bridge implementations: 80%+ coverage

---

## Performance Considerations

### Native Call Optimization

1. **Cache native hashes**: Store hashes to avoid string-to-hash conversion
2. **Batch native calls**: Group related native calls
3. **Avoid redundant calls**: Cache results where possible
4. **Use native call pooling**: Reuse native call objects

### Example: Native Hash Cache

```csharp
public static class NativeHash
{
    public static readonly ulong SET_ENTITY_COORDS = Game.GenerateHash("SET_ENTITY_COORDS");
    public static readonly ulong SET_ENTITY_HEADING = Game.GenerateHash("SET_ENTITY_HEADING");
    public static readonly ulong GET_ENTITY_POSITION = Game.GenerateHash("GET_ENTITY_POSITION");
}
```

### Example: Native Call Pooling

```csharp
public class NativeInvoker : INativeInvoker
{
    private readonly ObjectPool<NativeCall> _callPool;
    
    public T CallHash<T>(ulong hash, params object[] args)
    {
        var call = _callPool.Get();
        call.Setup(hash, args);
        var result = call.Invoke<T>();
        _callPool.Return(call);
        return result;
    }
}
```

---

## Summary

The Game Host Abstraction Layer provides:

- **Complete separation** between core systems and game engine
- **Loader independence** for easy porting
- **Testability** without game dependencies
- **Performance optimization** through caching and pooling
- **Clean architecture** following SOLID principles

All core systems MUST use these abstractions. Direct native calls are FORBIDDEN in core systems.
