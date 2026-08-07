# Architecture

**Project Liberty System Architecture and Design Principles**

---

## Table of Contents

1. [Overview](#overview)
2. [Design Principles](#design-principles)
3. [Architectural Patterns](#architectural-patterns)
4. [Layer Architecture](#layer-architecture)
5. [Core Abstractions](#core-abstractions)
6. [Module System](#module-system)
7. [Event System](#event-system)
8. [Dependency Injection](#dependency-injection)
9. [Plugin Architecture](#plugin-architecture)
10. [Data Flow](#data-flow)

---

## Overview

Project Liberty follows Clean Architecture principles with clear separation between domain logic, application services, and infrastructure. The architecture is designed to be:

- **Testable**: All components can be unit tested
- **Maintainable**: Clear boundaries and responsibilities
- **Extensible**: New features can be added without modifying existing code
- **Performance-Optimized**: Minimal overhead and efficient resource usage
- **ScriptHookVDotNet Integrated**: Built for GTA V modding with .NET Framework 4.8

The presentation runtime is a deliberate exception to the managed application boundary: CEF belongs to the independently loaded native `ProjectLiberty.Core.asi`. SHVDN and other .NET plugins consume only `ProjectLiberty.Managed` and BrowserIds through a C ABI. Managed reload therefore cannot tear down Chromium, DXGI hooks or native input. See [CEF.md](CEF.md) for the subsystem architecture and thread/lifecycle rules.

---

## Design Principles

### SOLID Principles

#### Single Responsibility Principle (SRP)
Each class and module has one reason to change. For example:
- `PlayerManager` only manages player-related operations
- `EventBus` only handles event publishing and subscription
- `LocalizationService` only handles translation

#### Open/Closed Principle (OCP)
Software entities are open for extension but closed for modification:
- New callouts can be added as plugins without modifying the callout system
- New languages can be added by adding translation files
- New event types can be defined without changing the event bus

#### Liskov Substitution Principle (LSP)
Derived classes must be substitutable for their base classes:
- Any `IGameHost` implementation can be used interchangeably
- Plugin implementations can be swapped without breaking the system

#### Interface Segregation Principle (ISP)
Clients should not depend on interfaces they don't use:
- `IPlayerManager` contains only player-related methods
- `IVehicleManager` contains only vehicle-related methods
- Small, focused interfaces instead of large, monolithic ones

#### Dependency Inversion Principle (DIP)
Depend on abstractions, not concretions:
- All systems depend on `IGameHost`, not a specific implementation
- Services depend on interfaces, not concrete classes
- High-level modules don't depend on low-level modules

### DRY (Don't Repeat Yourself)
- Common functionality extracted into shared services
- Utility classes for repeated operations
- Base classes for shared behavior

### KISS (Keep It Simple, Stupid)
- Simple solutions over complex ones
- Clear, readable code
- Avoid over-engineering

### Composition Over Inheritance
- Prefer composition to build complex behaviors
- Use inheritance only for true "is-a" relationships
- Behavior trees for AI instead of deep inheritance hierarchies

---

## Architectural Patterns

### Clean Architecture

```
┌─────────────────────────────────────────┐
│           Presentation Layer            │
│  (UI, Menus, HUD, Commands)             │
└─────────────────────────────────────────┘
                  ↓
┌─────────────────────────────────────────┐
│          Application Layer              │
│  (Use Cases, Plugin System, Scripting)  │
└─────────────────────────────────────────┘
                  ↓
┌─────────────────────────────────────────┐
│            Domain Layer                 │
│  (Entities, Value Objects, Domain Logic)│
└─────────────────────────────────────────┘
                  ↓
┌─────────────────────────────────────────┐
│         Infrastructure Layer            │
│  (Game Host, Native Bridge, File I/O)   │
└─────────────────────────────────────────┘
```

### Event-Driven Architecture

All systems communicate through events, eliminating direct dependencies:

```
┌──────────────┐     Publish     ┌──────────────┐
│   System A   │ ──────────────→ │  Event Bus   │
└──────────────┘                 └──────────────┘
                                         ↓
┌──────────────┐     Subscribe    ┌──────────────┐
│   System B   │ ←────────────── │  Event Bus   │
└──────────────┘                 └──────────────┘
```

### Domain-Driven Design (DDD)

Where appropriate, we use DDD concepts:
- **Bounded Contexts**: Each system has its own domain model
- **Aggregates**: Related entities treated as a unit
- **Value Objects**: Immutable objects defined by their values
- **Domain Events**: Events that represent something that happened in the domain

---

## Layer Architecture

### Presentation Layer

**Responsibility**: User interaction and display

**Components**:
- UI System
- Menus
- HUD
- Commands
- Notifications

**Rules**:
- No business logic
- Only presentation concerns
- Communicates with Application Layer through events

### Application Layer

**Responsibility**: Use cases and orchestration

**Components**:
- Plugin System
- Scripting Engine
- Callout Manager
- Traffic Stop Manager
- Dispatch Manager

**Rules**:
- Orchestrates domain objects
- Implements use cases
- No infrastructure concerns

### Domain Layer

**Responsibility**: Core business logic and entities

**Components**:
- Entities (Player, Vehicle, Ped, Callout, etc.)
- Value Objects (Position, Rotation, Vector3, etc.)
- Domain Services
- Domain Events

**Rules**:
- No infrastructure dependencies
- Pure business logic
- Framework-agnostic

### Infrastructure Layer

**Responsibility**: External system integration

**Components**:
- Game Host Implementation
- Native Bridge
- File System
- Configuration
- Logging
- Save System

**Rules**:
- Implements interfaces defined in Domain/Application layers
- Contains all external dependencies
- No business logic

---

## Core Abstractions

### IGameHost

Main interface for game host interaction:

```csharp
public interface IGameHost
{
    string GameVersion { get; }
    bool IsRunning { get; }
    
    void Initialize();
    void Shutdown();
    void OnTick();
    
    INativeBridge NativeBridge { get; }
    IGameLoop GameLoop { get; }
}
```

### INativeBridge

Bridge to native GTA V functions:

```csharp
public interface INativeBridge
{
    T Invoke<T>(ulong hash, params object[] args);
    void Invoke(ulong hash, params object[] args);
    
    bool IsNativeAvailable(ulong hash);
    ulong GetNativeHash(string name);
}
```

### IGameLoop

Game loop management:

```csharp
public interface IGameLoop
{
    void RegisterTickAction(Action action);
    void UnregisterTickAction(Action action);
    void RegisterKeyDownAction(Keys key, Action action);
    void UnregisterKeyDownAction(Keys key, Action action);
    
    int CurrentFrame { get; }
    float DeltaTime { get; }
    float TotalTime { get; }
}
```

### IEntityManager

Entity management:

```csharp
public interface IEntityManager
{
    IEntity GetEntity(int handle);
    IEnumerable<IEntity> GetAllEntities();
    bool EntityExists(int handle);
    void DestroyEntity(int handle);
}
```

### IPlayerManager

Player management:

```csharp
public interface IPlayerManager
{
    IPlayer GetLocalPlayer();
    IEnumerable<IPlayer> GetAllPlayers();
    void SpawnPlayer(Vector3 position, float heading);
    void TeleportPlayer(IPlayer player, Vector3 position);
}
```

### IVehicleManager

Vehicle management:

```csharp
public interface IVehicleManager
{
    IVehicle CreateVehicle(VehicleModel model, Vector3 position, float heading);
    IVehicle GetVehicle(int handle);
    IEnumerable<IVehicle> GetAllVehicles();
    void DeleteVehicle(IVehicle vehicle);
}
```

### IPedManager

Ped management:

```csharp
public interface IPedManager
{
    IPed CreatePed(PedModel model, Vector3 position, float heading);
    IPed GetPed(int handle);
    IEnumerable<IPed> GetAllPeds();
    void DeletePed(IPed ped);
}
```

### IWorldManager

World management:

```csharp
public interface IWorldManager
{
    Vector3 GetWaypointPosition();
    void SetWaypointPosition(Vector3 position);
    void ClearWaypoint();
    
    Blip CreateBlip(Vector3 position, BlipSprite sprite);
    void RemoveBlip(Blip blip);
    
    void SetWeather(WeatherType weather);
    WeatherType GetWeather();
}
```

### INativeInvoker

Native function invocation:

```csharp
public interface INativeInvoker
{
    T Call<T>(string nativeName, params object[] args);
    void Call(string nativeName, params object[] args);
    
    T CallHash<T>(ulong hash, params object[] args);
    void CallHash(ulong hash, params object[] args);
}
```

---

## Module System

### Module Characteristics

Every module in Project Liberty must be:

1. **Discoverable**: Can be found and loaded at runtime
2. **Loadable**: Can be loaded and unloaded dynamically
3. **Configurable**: Has its own configuration
4. **Localizable**: Supports multiple languages
5. **Replaceable**: Can be replaced with alternative implementations
6. **Extensible**: Can be extended without modification

### Module Lifecycle

```
Discover → Load → Initialize → Enable → [Running] → Disable → Unload
```

### Module Interface

```csharp
public interface IModule
{
    string Name { get; }
    string Version { get; }
    string Description { get; }
    
    void Discover();
    void Load();
    void Initialize();
    void Enable();
    void Disable();
    void Unload();
    
    bool IsLoaded { get; }
    bool IsEnabled { get; }
}
```

---

## Event System

### Event Bus Architecture

The Event Bus is the central communication mechanism:

```csharp
public interface IEventBus
{
    void Publish<TEvent>(TEvent @event) where TEvent : IEvent;
    void Subscribe<TEvent>(IEventHandler<TEvent> handler) where TEvent : IEvent;
    void Unsubscribe<TEvent>(IEventHandler<TEvent> handler) where TEvent : IEvent;
}
```

### Event Types

#### Domain Events
Represent something that happened in the domain:
- `PlayerSpawned`
- `PlayerDied`
- `CalloutStarted`
- `CrimeCommitted`

#### System Events
Represent system state changes:
- `ModuleLoaded`
- `ModuleEnabled`
- `ConfigurationChanged`

#### UI Events
Represent user interactions:
- `MenuOpened`
- `ButtonClicked`
- `CommandExecuted`

### Event Handlers

```csharp
public interface IEventHandler<TEvent> where TEvent : IEvent
{
    void Handle(TEvent @event);
}
```

---

## Dependency Injection

### DI Container

Project Liberty uses a DI container for dependency management:

```csharp
public interface IServiceContainer
{
    void RegisterSingleton<TInterface, TImplementation>()
        where TImplementation : TInterface;
    
    void RegisterTransient<TInterface, TImplementation>()
        where TImplementation : TInterface;
    
    void RegisterInstance<TService>(TService instance);
    
    TService Resolve<TService>();
}
```

### Service Registration

Services are registered in the composition root:

```csharp
public static class ServiceRegistration
{
    public static void RegisterServices(IServiceContainer container)
    {
        // Core services
        container.RegisterSingleton<IGameHost, GameHost>();
        container.RegisterSingleton<IEventBus, EventBus>();
        
        // Manager services
        container.RegisterSingleton<IPlayerManager, PlayerManager>();
        container.RegisterSingleton<IVehicleManager, VehicleManager>();
        
        // Plugin services
        container.RegisterTransient<IPluginLoader, PluginLoader>();
    }
}
```

---

## Plugin Architecture

### Plugin Definition

```csharp
public interface IPlugin : IModule
{
    IPluginManifest Manifest { get; }
    IEnumerable<IDependency> Dependencies { get; }
    
    void OnConfigurationChanged(IConfiguration config);
    void OnLocalizationChanged(ILocalization localization);
}
```

### Plugin Manifest

```csharp
public interface IPluginManifest
{
    string Id { get; }
    string Name { get; }
    string Version { get; }
    string Author { get; }
    string Description { get; }
    
    IEnumerable<string> SupportedLanguages { get; }
    IEnumerable<PluginDependency> Dependencies { get; }
}
```

### Plugin Loader

The Plugin Loader handles:
- Plugin discovery
- Dependency resolution
- Version compatibility
- Lifecycle management

---

## Data Flow

### Typical Request Flow

```
User Action → UI Layer → Application Layer → Domain Layer → Infrastructure Layer
```

### Event Flow

```
Domain Event → Event Bus → Subscribers → Application Logic
```

### Plugin Flow

```
Plugin Load → Dependency Resolution → Initialization → Enable → Event Subscription
```

---

## Performance Considerations

### Minimizing Native Calls

- Cache native function hashes
- Batch native calls where possible
- Use native call pooling

### Minimizing GC Pressure

- Object pooling for frequently created objects
- Structs for small, short-lived objects
- Avoid unnecessary allocations in hot paths

### Minimizing CPU Usage

- Efficient event filtering
- Lazy loading of resources
- Asynchronous operations where appropriate

---

## Testing Strategy

### Unit Tests

- Test domain logic in isolation
- Mock infrastructure dependencies
- Fast execution

### Integration Tests

- Test interaction between components
- Use real implementations where needed
- Slower execution

### System Tests

- Test entire system behavior
- Real game environment
- Slowest execution

---

## Security Considerations

### Plugin Sandboxing

- Current plugins are trusted assemblies loaded into the main application domain.
- `IPluginContext` limits the services exposed by the SDK, but it is not a security boundary.
- True isolation, unloadability, and permissions are future work; do not install untrusted plugins.

### Input Validation

- All user input validated
- Configuration files validated
- Native call parameters validated

---

## Summary

Project Liberty's architecture is designed to be:

- **Clean**: Clear separation of concerns
- **Modular**: Every component is replaceable
- **Extensible**: New features without modification
- **Testable**: All components can be tested
- **Performant**: Optimized for game environment
- **Maintainable**: Easy to understand and modify

This architecture ensures Project Liberty can serve as a stable foundation for police roleplay modifications for years to come.
