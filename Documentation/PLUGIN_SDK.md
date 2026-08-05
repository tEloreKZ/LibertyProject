# Plugin SDK

**Plugin Development Guide for Project Liberty**

---

## Table of Contents

1. [Overview](#overview)
2. [Plugin Architecture](#plugin-architecture)
3. [Plugin Lifecycle](#plugin-lifecycle)
4. [Plugin Manifest](#plugin-manifest)
5. [Creating a Plugin](#creating-a-plugin)
6. [Plugin Dependencies](#plugin-dependencies)
7. [Plugin Configuration](#plugin-configuration)
8. [Plugin Localization](#plugin-localization)
9. [Plugin Events](#plugin-events)
10. [Best Practices](#best-practices)
11. [Examples](#examples)

---

## Overview

The Project Liberty Plugin SDK allows developers to create extensions that integrate seamlessly with the platform. Plugins can:

- Add new callouts
- Create custom systems
- Extend existing functionality
- Provide new UI elements
- Implement custom AI behaviors
- Add new commands

### Plugin Types

1. **System Plugins**: Core system extensions
2. **Callout Plugins**: Add new callouts
3. **Script Plugins**: Custom scripts
4. **UI Plugins**: Custom UI elements
5. **AI Plugins**: Custom AI behaviors

---

## Plugin Architecture

### Plugin Interface

```csharp
namespace ProjectLiberty.PluginSystem
{
    /// <summary>
    /// Base interface for all plugins.
    /// </summary>
    public interface IPlugin : IModule
    {
        /// <summary>
        /// Gets the plugin manifest.
        /// </summary>
        IPluginManifest Manifest { get; }
        
        /// <summary>
        /// Gets the plugin dependencies.
        /// </summary>
        IEnumerable<IDependency> Dependencies { get; }
        
        /// <summary>
        /// Called when configuration changes.
        /// </summary>
        /// <param name="config">New configuration.</param>
        void OnConfigurationChanged(IConfiguration config);
        
        /// <summary>
        /// Called when localization changes.
        /// </summary>
        /// <param name="localization">New localization.</param>
        void OnLocalizationChanged(ILocalization localization);
    }
}
```

### Plugin Base Class

```csharp
namespace ProjectLiberty.PluginSystem
{
    /// <summary>
    /// Base class for plugins.
    /// </summary>
    public abstract class PluginBase : IPlugin
    {
        protected IPluginContext Context { get; private set; }
        protected ILogger Logger { get; private set; }
        protected IEventBus EventBus { get; private set; }
        protected ILocalization Localization { get; private set; }
        protected IConfiguration Configuration { get; private set; }
        
        public abstract IPluginManifest Manifest { get; }
        public abstract IEnumerable<IDependency> Dependencies { get; }
        
        public virtual void SetContext(IPluginContext context)
        {
            Context = context;
            Logger = context.Logger;
            EventBus = context.EventBus;
            Localization = context.Localization;
            Configuration = context.Configuration;
        }
        
        public virtual void Discover() { }
        public virtual void Load() { }
        public virtual void Initialize() { }
        public virtual void Enable() { }
        public virtual void Disable() { }
        public virtual void Unload() { }
        
        public virtual void OnConfigurationChanged(IConfiguration config) { }
        public virtual void OnLocalizationChanged(ILocalization localization) { }
        
        public bool IsLoaded { get; private set; }
        public bool IsEnabled { get; private set; }
    }
}
```

---

## Plugin Lifecycle

### Lifecycle States

```
Discover → Load → Initialize → Enable → [Running] → Disable → Unload
```

### Lifecycle Methods

#### Discover

Called when the plugin is discovered by the loader:

```csharp
public override void Discover()
{
    Logger.LogInformation($"Discovering plugin: {Manifest.Name}");
    // Validate plugin requirements
    // Check dependencies
}
```

#### Load

Called when the plugin is loaded into memory:

```csharp
public override void Load()
{
    Logger.LogInformation($"Loading plugin: {Manifest.Name}");
    // Load resources
    // Initialize internal state
    IsLoaded = true;
}
```

#### Initialize

Called when the plugin is initialized:

```csharp
public override void Initialize()
{
    Logger.LogInformation($"Initializing plugin: {Manifest.Name}");
    // Register event handlers
    // Register commands
    // Initialize services
}
```

#### Enable

Called when the plugin is enabled and active:

```csharp
public override void Enable()
{
    Logger.LogInformation($"Enabling plugin: {Manifest.Name}");
    // Subscribe to events
    // Start timers
    // Activate features
    IsEnabled = true;
}
```

#### Disable

Called when the plugin is disabled:

```csharp
public override void Disable()
{
    Logger.LogInformation($"Disabling plugin: {Manifest.Name}");
    // Unsubscribe from events
    // Stop timers
    // Deactivate features
    IsEnabled = false;
}
```

#### Unload

Called when the plugin is unloaded from memory:

```csharp
public override void Unload()
{
    Logger.LogInformation($"Unloading plugin: {Manifest.Name}");
    // Release resources
    // Clean up
    IsLoaded = false;
}
```

---

## Plugin Manifest

### Manifest Interface

```csharp
namespace ProjectLiberty.PluginSystem
{
    /// <summary>
    /// Plugin manifest interface.
    /// </summary>
    public interface IPluginManifest
    {
        /// <summary>
        /// Gets the unique plugin ID.
        /// </summary>
        string Id { get; }
        
        /// <summary>
        /// Gets the plugin name.
        /// </summary>
        string Name { get; }
        
        /// <summary>
        /// Gets the plugin version.
        /// </summary>
        Version Version { get; }
        
        /// <summary>
        /// Gets the plugin author.
        /// </summary>
        string Author { get; }
        
        /// <summary>
        /// Gets the plugin description.
        /// </summary>
        string Description { get; }
        
        /// <summary>
        /// Gets the minimum Project Liberty version required.
        /// </summary>
        Version MinimumLibertyVersion { get; }
        
        /// <summary>
        /// Gets the supported languages.
        /// </summary>
        IEnumerable<string> SupportedLanguages { get; }
        
        /// <summary>
        /// Gets the plugin dependencies.
        /// </summary>
        IEnumerable<PluginDependency> Dependencies { get; }
        
        /// <summary>
        /// Gets the plugin type.
        /// </summary>
        PluginType Type { get; }
    }
}
```

### Manifest Example

```csharp
namespace MyPlugin
{
    public class MyPluginManifest : IPluginManifest
    {
        public string Id => "myplugin.example";
        public string Name => "My Example Plugin";
        public Version Version => new Version(1, 0, 0);
        public string Author => "Your Name";
        public string Description => "An example plugin for Project Liberty";
        public Version MinimumLibertyVersion => new Version(1, 0, 0);
        
        public IEnumerable<string> SupportedLanguages => new[]
        {
            "en", "ru", "de", "fr"
        };
        
        public IEnumerable<PluginDependency> Dependencies => new[]
        {
            new PluginDependency("core", new Version(1, 0, 0))
        };
        
        public PluginType Type => PluginType.System;
    }
}
```

### Manifest File (plugin.json)

```json
{
  "id": "myplugin.example",
  "name": "My Example Plugin",
  "version": "1.0.0",
  "author": "Your Name",
  "description": "An example plugin for Project Liberty",
  "minimumLibertyVersion": "1.0.0",
  "supportedLanguages": ["en", "ru", "de", "fr"],
  "dependencies": [
    {
      "id": "core",
      "minimumVersion": "1.0.0"
    }
  ],
  "type": "System"
}
```

---

## Creating a Plugin

### Step 1: Create Project

```bash
dotnet new classlib -n MyPlugin
cd MyPlugin
```

### Step 2: Add Reference

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net8.0</TargetFramework>
  </PropertyGroup>
  <ItemGroup>
    <PackageReference Include="ProjectLiberty.SDK" Version="1.0.0" />
  </ItemGroup>
</Project>
```

### Step 3: Implement Plugin

```csharp
using ProjectLiberty.PluginSystem;
using ProjectLiberty.Abstractions;

namespace MyPlugin
{
    public class MyPlugin : PluginBase
    {
        public override IPluginManifest Manifest => new MyPluginManifest();
        
        public override IEnumerable<IDependency> Dependencies => Array.Empty<IDependency>();
        
        public override void Initialize()
        {
            Logger.LogInformation("Initializing MyPlugin");
            
            // Subscribe to events
            EventBus.Subscribe<PlayerSpawnedEvent>(OnPlayerSpawned);
        }
        
        public override void Enable()
        {
            Logger.LogInformation("Enabling MyPlugin");
            
            // Register command
            Context.CommandService.RegisterCommand(new MyCommand());
        }
        
        public override void Disable()
        {
            Logger.LogInformation("Disabling MyPlugin");
            
            // Unsubscribe from events
            EventBus.Unsubscribe<PlayerSpawnedEvent>(OnPlayerSpawned);
        }
        
        private void OnPlayerSpawned(PlayerSpawnedEvent @event)
        {
            Logger.LogInformation($"Player spawned at {@event.Position}");
        }
    }
    
    public class MyPluginManifest : IPluginManifest
    {
        public string Id => "myplugin.example";
        public string Name => "My Example Plugin";
        public Version Version => new Version(1, 0, 0);
        public string Author => "Your Name";
        public string Description => "An example plugin for Project Liberty";
        public Version MinimumLibertyVersion => new Version(1, 0, 0);
        public IEnumerable<string> SupportedLanguages => new[] { "en" };
        public IEnumerable<PluginDependency> Dependencies => Array.Empty<PluginDependency>();
        public PluginType Type => PluginType.System;
    }
}
```

### Step 4: Create Manifest File

Create `plugin.json` in the project root:

```json
{
  "id": "myplugin.example",
  "name": "My Example Plugin",
  "version": "1.0.0",
  "author": "Your Name",
  "description": "An example plugin for Project Liberty",
  "minimumLibertyVersion": "1.0.0",
  "supportedLanguages": ["en"],
  "dependencies": [],
  "type": "System"
}
```

### Step 5: Build

```bash
dotnet build -c Release
```

### Step 6: Install

Copy the built DLL and `plugin.json` to the `Plugins/` directory.

---

## Plugin Dependencies

### Dependency Interface

```csharp
namespace ProjectLiberty.PluginSystem
{
    /// <summary>
    /// Plugin dependency interface.
    /// </summary>
    public interface IDependency
    {
        /// <summary>
        /// Gets the dependency ID.
        /// </summary>
        string Id { get; }
        
        /// <summary>
        /// Gets the minimum version required.
        /// </summary>
        Version MinimumVersion { get; }
        
        /// <summary>
        /// Gets whether the dependency is required.
        /// </summary>
        bool IsRequired { get; }
    }
}
```

### Dependency Example

```csharp
public class PluginDependency : IDependency
{
    public string Id { get; }
    public Version MinimumVersion { get; }
    public bool IsRequired { get; }
    
    public PluginDependency(string id, Version minimumVersion, bool isRequired = true)
    {
        Id = id;
        MinimumVersion = minimumVersion;
        IsRequired = isRequired;
    }
}
```

### Declaring Dependencies

```csharp
public override IEnumerable<IDependency> Dependencies => new[]
{
    new PluginDependency("core", new Version(1, 0, 0)),
    new PluginDependency("dispatch", new Version(1, 0, 0)),
    new PluginDependency("trafficstops", new Version(1, 0, 0), isRequired: false)
};
```

---

## Plugin Configuration

### Configuration File

Create `config.json` in your plugin directory:

```json
{
  "enabled": true,
  "settings": {
    "maxInstances": 5,
    "timeout": 30,
    "debugMode": false
  }
}
```

### Accessing Configuration

```csharp
public override void Initialize()
{
    var config = Configuration.GetPluginConfiguration(Manifest.Id);
    
    var maxInstances = config.GetValue<int>("settings.maxInstances", 5);
    var timeout = config.GetValue<int>("settings.timeout", 30);
    var debugMode = config.GetValue<bool>("settings.debugMode", false);
    
    Logger.LogInformation($"Configuration: MaxInstances={maxInstances}, Timeout={timeout}, DebugMode={debugMode}");
}
```

### Configuration Changes

```csharp
public override void OnConfigurationChanged(IConfiguration config)
{
    Logger.LogInformation("Configuration changed");
    // Reload configuration
    // Apply new settings
}
```

---

## Plugin Localization

### Localization File

Create `Languages/en/myplugin.json`:

```json
{
  "PluginName": "My Example Plugin",
  "PluginDescription": "An example plugin for Project Liberty",
  "CommandHelp": "Shows plugin information",
  "PlayerSpawned": "Player spawned at {0}",
  "Error": "An error occurred: {0}"
}
```

### Accessing Localization

```csharp
public override void Initialize()
{
    var pluginName = Localization.GetString("myplugin.PluginName");
    var description = Localization.GetString("myplugin.PluginDescription");
    
    Logger.LogInformation($"{pluginName}: {description}");
}
```

### Localization Changes

```csharp
public override void OnLocalizationChanged(ILocalization localization)
{
    Logger.LogInformation("Localization changed");
    // Reload localized strings
}
```

---

## Plugin Events

### Publishing Events

```csharp
public void DoSomething()
{
    EventBus.Publish(new MyCustomEvent
    {
        Data = "Some data",
        Timestamp = DateTime.UtcNow
    });
}
```

### Subscribing to Events

```csharp
public override void Initialize()
{
    EventBus.Subscribe<PlayerSpawnedEvent>(OnPlayerSpawned);
    EventBus.Subscribe<VehicleCreatedEvent>(OnVehicleCreated);
}

private void OnPlayerSpawned(PlayerSpawnedEvent @event)
{
    Logger.LogInformation($"Player spawned at {@event.Position}");
}

private void OnVehicleCreated(VehicleCreatedEvent @event)
{
    Logger.LogInformation($"Vehicle {@event.Model} created at {@event.Position}");
}
```

### Unsubscribing from Events

```csharp
public override void Disable()
{
    EventBus.Unsubscribe<PlayerSpawnedEvent>(OnPlayerSpawned);
    EventBus.Unsubscribe<VehicleCreatedEvent>(OnVehicleCreated);
}
```

---

## Best Practices

### 1. Resource Management

Always clean up resources:

```csharp
public override void Disable()
{
    // Unsubscribe from events
    EventBus.Unsubscribe<PlayerSpawnedEvent>(OnPlayerSpawned);
    
    // Stop timers
    _timer?.Stop();
    _timer?.Dispose();
    
    // Release resources
    _resources?.Dispose();
}
```

### 2. Error Handling

Handle errors gracefully:

```csharp
public override void Enable()
{
    try
    {
        // Enable logic
    }
    catch (Exception ex)
    {
        Logger.LogError(ex, "Error enabling plugin");
        // Disable plugin on error
    }
}
```

### 3. Logging

Use appropriate log levels:

```csharp
Logger.LogDebug("Debug information");
Logger.LogInformation("Information message");
Logger.LogWarning("Warning message");
Logger.LogError(ex, "Error message");
Logger.LogCritical("Critical message");
```

### 4. Thread Safety

Ensure thread safety for shared state:

```csharp
private readonly object _lock = new object();
private List<int> _items = new List<int>();

public void AddItem(int item)
{
    lock (_lock)
    {
        _items.Add(item);
    }
}
```

### 5. Performance

Avoid expensive operations in hot paths:

```csharp
// GOOD - Cache expensive operations
private string _cachedValue;

public override void Initialize()
{
    _cachedValue = ExpensiveOperation();
}

// BAD - Expensive operation in hot path
public void OnTick()
{
    var value = ExpensiveOperation(); // BAD
}
```

---

## Examples

### Example 1: Simple Command Plugin

```csharp
using ProjectLiberty.PluginSystem;
using ProjectLiberty.Commands;

namespace SimpleCommandPlugin
{
    public class SimpleCommandPlugin : PluginBase
    {
        public override IPluginManifest Manifest => new SimpleCommandManifest();
        public override IEnumerable<IDependency> Dependencies => Array.Empty<IDependency>();
        
        public override void Enable()
        {
            Context.CommandService.RegisterCommand(new HelloCommand());
        }
        
        public override void Disable()
        {
            Context.CommandService.UnregisterCommand("hello");
        }
    }
    
    public class HelloCommand : Command
    {
        public override string Name => "hello";
        public override string Description => "Says hello";
        
        public override void Execute(string[] args)
        {
            Logger.LogInformation("Hello, World!");
        }
    }
}
```

### Example 2: Callout Plugin

```csharp
using ProjectLiberty.PluginSystem;
using ProjectLiberty.Callouts;

namespace RobberyCalloutPlugin
{
    public class RobberyCalloutPlugin : PluginBase
    {
        public override IPluginManifest Manifest => new RobberyCalloutManifest();
        public override IEnumerable<IDependency> Dependencies => new[]
        {
            new PluginDependency("callouts", new Version(1, 0, 0))
        };
        
        public override void Initialize()
        {
            Context.CalloutService.RegisterCallout<RobberyCallout>();
        }
        
        public override void Disable()
        {
            Context.CalloutService.UnregisterCallout<RobberyCallout>();
        }
    }
    
    public class RobberyCallout : CalloutBase
    {
        public override string Name => "Store Robbery";
        public override string Description => "Respond to a store robbery in progress";
        
        protected override void OnStart()
        {
            // Create robbery scenario
        }
        
        protected override void OnEnd(CalloutResult result)
        {
            // Cleanup
        }
    }
}
```

### Example 3: Event Handler Plugin

```csharp
using ProjectLiberty.PluginSystem;
using ProjectLiberty.Events;

namespace EventHandlerPlugin
{
    public class EventHandlerPlugin : PluginBase
    {
        public override IPluginManifest Manifest => new EventHandlerManifest();
        public override IEnumerable<IDependency> Dependencies => Array.Empty<IDependency>();
        
        public override void Initialize()
        {
            EventBus.Subscribe<PlayerDiedEvent>(OnPlayerDied);
            EventBus.Subscribe<VehicleDestroyedEvent>(OnVehicleDestroyed);
        }
        
        public override void Disable()
        {
            EventBus.Unsubscribe<PlayerDiedEvent>(OnPlayerDied);
            EventBus.Unsubscribe<VehicleDestroyedEvent>(OnVehicleDestroyed);
        }
        
        private void OnPlayerDied(PlayerDiedEvent @event)
        {
            Logger.LogInformation($"Player died at {@event.Position}");
            // Handle player death
        }
        
        private void OnVehicleDestroyed(VehicleDestroyedEvent @event)
        {
            Logger.LogInformation($"Vehicle destroyed at {@event.Position}");
            // Handle vehicle destruction
        }
    }
}
```

---

## Summary

The Plugin SDK provides:

- **Clean plugin architecture** with lifecycle management
- **Manifest system** for plugin metadata
- **Dependency management** for plugin interoperability
- **Configuration system** for plugin settings
- **Localization support** for multi-language plugins
- **Event system** for plugin communication
- **Command system** for plugin commands

All plugins must follow the plugin lifecycle and properly manage resources.
