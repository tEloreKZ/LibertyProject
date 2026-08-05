# Configuration

**Configuration System Documentation**

---

## Table of Contents

1. [Overview](#overview)
2. [Configuration Interface](#configuration-interface)
3. [Configuration Files](#configuration-files)
4. [Configuration Validation](#configuration-validation)
5. [Hot Reload](#hot-reload)
6. [Plugin Configuration](#plugin-configuration)
7. [Best Practices](#best-practices)

---

## Overview

Project Liberty uses JSON-based configuration files for all settings. The configuration system supports hot reload, validation, and plugin-specific configurations.

### Architecture

```
┌─────────────────────────────────────────┐
│         Application Layer              │
│  (Core Systems, Plugins)               │
└─────────────────────────────────────────┘
                  ↓
┌─────────────────────────────────────────┐
│      Configuration Service             │
│  (IConfiguration)                       │
└─────────────────────────────────────────┘
                  ↓
┌─────────────────────────────────────────┐
│      Configuration Cache               │
│  (In-memory cache)                      │
└─────────────────────────────────────────┘
                  ↓
┌─────────────────────────────────────────┐
│      Configuration Files              │
│  (JSON files)                           │
└─────────────────────────────────────────┘
```

---

## Configuration Interface

```csharp
namespace ProjectLiberty.Abstractions
{
    /// <summary>
    /// Configuration service interface.
    /// </summary>
    public interface IConfiguration
    {
        /// <summary>
        /// Gets a configuration value.
        /// </summary>
        /// <typeparam name="T">Value type.</typeparam>
        /// <param name="key">Configuration key.</param>
        /// <returns>Configuration value.</returns>
        T GetValue<T>(string key);
        
        /// <summary>
        /// Gets a configuration value with default.
        /// </summary>
        /// <typeparam name="T">Value type.</typeparam>
        /// <param name="key">Configuration key.</param>
        /// <param name="defaultValue">Default value if key not found.</param>
        /// <returns>Configuration value or default.</returns>
        T GetValue<T>(string key, T defaultValue);
        
        /// <summary>
        /// Sets a configuration value.
        /// </summary>
        /// <typeparam name="T">Value type.</typeparam>
        /// <param name="key">Configuration key.</param>
        /// <param name="value">Value to set.</param>
        void SetValue<T>(string key, T value);
        
        /// <summary>
        /// Gets a configuration section.
        /// </summary>
        /// <param name="section">Section name.</param>
        /// <returns>Configuration section.</returns>
        IConfigurationSection GetSection(string section);
        
        /// <summary>
        /// Gets plugin configuration.
        /// </summary>
        /// <param name="pluginId">Plugin ID.</param>
        /// <returns>Plugin configuration.</returns>
        IConfiguration GetPluginConfiguration(string pluginId);
        
        /// <summary>
        /// Reloads configuration from files.
        /// </summary>
        void Reload();
        
        /// <summary>
        /// Saves configuration to files.
        /// </summary>
        void Save();
        
        /// <summary>
        /// Checks if a configuration key exists.
        /// </summary>
        /// <param name="key">Configuration key.</param>
        /// <returns>True if exists, false otherwise.</returns>
        bool HasKey(string key);
        
        /// <summary>
        /// Event raised when configuration changes.
        /// </summary>
        event EventHandler<ConfigurationChangedEventArgs> ConfigurationChanged;
    }
    
    /// <summary>
    /// Configuration section interface.
    /// </summary>
    public interface IConfigurationSection
    {
        /// <summary>
        /// Gets the section key.
        /// </summary>
        string Key { get; }
        
        /// <summary>
        /// Gets a value from the section.
        /// </summary>
        /// <typeparam name="T">Value type.</typeparam>
        /// <param name="key">Value key.</param>
        /// <returns>Configuration value.</returns>
        T GetValue<T>(string key);
        
        /// <summary>
        /// Gets a value from the section with default.
        /// </summary>
        /// <typeparam name="T">Value type.</typeparam>
        /// <param name="key">Value key.</param>
        /// <param name="defaultValue">Default value.</param>
        /// <returns>Configuration value or default.</returns>
        T GetValue<T>(string key, T defaultValue);
        
        /// <summary>
        /// Sets a value in the section.
        /// </summary>
        /// <typeparam name="T">Value type.</typeparam>
        /// <param name="key">Value key.</param>
        /// <param name="value">Value to set.</param>
        void SetValue<T>(string key, T value);
        
        /// <summary>
        /// Gets all keys in the section.
        /// </summary>
        IEnumerable<string> GetKeys();
    }
}
```

---

## Configuration Files

### Core Configuration (core.json)

```json
{
  "enabled": true,
  "debugMode": false,
  "language": "en",
  "logLevel": "Information",
  "tickRate": 60,
  "maxMemoryMB": 500,
  "hotReloadEnabled": true,
  "performance": {
    "enableProfiling": false,
    "logNativeCalls": false,
    "logGC": false
  }
}
```

### Plugin Configuration (plugins.json)

```json
{
  "enabledPlugins": [
    "core",
    "dispatch",
    "trafficstops",
    "callouts",
    "arrests"
  ],
  "disabledPlugins": [],
  "autoLoad": true,
  "loadOrder": [
    "core",
    "dispatch",
    "trafficstops",
    "callouts",
    "arrests"
  ]
}
```

### Dispatch Configuration (dispatch.json)

```json
{
  "enabled": true,
  "radioChannels": 3,
  "defaultChannel": 1,
  "backupResponseTime": 30,
  "unitResponseTime": 15,
  "callCodes": {
    "pursuit": "10-10",
    "robbery": "10-11",
    "assault": "10-12",
    "trafficStop": "10-50"
  }
}
```

### Traffic Stops Configuration (trafficstops.json)

```json
{
  "enabled": true,
  "maxConcurrentStops": 5,
  "stopDistance": 10.0,
  "pullOverSpeed": 5.0,
  "stages": {
    "observe": {
      "duration": 5,
      "enabled": true
    },
    "pullOver": {
      "duration": 10,
      "enabled": true
    },
    "greeting": {
      "duration": 15,
      "enabled": true
    },
    "documents": {
      "duration": 20,
      "enabled": true
    },
    "questioning": {
      "duration": 30,
      "enabled": true
    },
    "database": {
      "duration": 10,
      "enabled": true
    },
    "citation": {
      "duration": 15,
      "enabled": true
    },
    "search": {
      "duration": 30,
      "enabled": true
    },
    "arrest": {
      "duration": 20,
      "enabled": true
    },
    "release": {
      "duration": 10,
      "enabled": true
    }
  }
}
```

### Callouts Configuration (callouts.json)

```json
{
  "enabled": true,
  "maxActiveCallouts": 3,
  "calloutInterval": 60,
  "calloutRadius": 500.0,
  "callouts": {
    "pursuit": {
      "enabled": true,
      "weight": 10,
      "minDistance": 200,
      "maxDistance": 1000
    },
    "robbery": {
      "enabled": true,
      "weight": 8,
      "minDistance": 100,
      "maxDistance": 800
    },
    "assault": {
      "enabled": true,
      "weight": 7,
      "minDistance": 50,
      "maxDistance": 600
    }
  }
}
```

### Arrest System Configuration (arrests.json)

```json
{
  "enabled": true,
  "handcuffTime": 5,
  "transportTime": 30,
  "bookingTime": 60,
  "maxPrisoners": 10,
  "arrestReasons": {
    "speeding": {
      "enabled": true,
      "fine": 500
    },
    "recklessDriving": {
      "enabled": true,
      "fine": 1000
    },
    "dui": {
      "enabled": true,
      "fine": 2000
    },
    "warrant": {
      "enabled": true,
      "fine": 0
    },
    "violence": {
      "enabled": true,
      "fine": 5000
    }
  }
}
```

### AI Configuration (ai.json)

```json
{
  "enabled": true,
  "updateFrequency": 0.1,
  "perceptionRange": 100.0,
  "memoryDuration": 300,
  "behaviorTree": {
    "maxDepth": 10,
    "timeout": 5.0
  },
  "blackboard": {
    "maxEntries": 1000,
    "entryTTL": 600
  }
}
```

### Performance Configuration (performance.json)

```json
{
  "enabled": true,
  "targetFPS": 60,
  "maxTickTime": 1.0,
  "maxNativeCalls": 1000,
  "objectPooling": {
    "enabled": true,
    "poolSize": 100
  },
  "spatialPartitioning": {
    "enabled": true,
    "cellSize": 50.0
  }
}
```

### Logging Configuration (logging.json)

```json
{
  "enabled": true,
  "logLevel": "Information",
  "logToFile": true,
  "logToConsole": true,
  "logDirectory": "Logs",
  "maxLogFiles": 10,
  "maxLogFileSizeMB": 10,
  "categories": {
    "Core": "Information",
    "Dispatch": "Information",
    "TrafficStops": "Information",
    "Callouts": "Information",
    "Arrests": "Information",
    "AI": "Debug"
  }
}
```

---

## Configuration Validation

### JSON Schema

Create JSON schemas for validation:

```json
{
  "$schema": "http://json-schema.org/draft-07/schema#",
  "type": "object",
  "properties": {
    "enabled": {
      "type": "boolean"
    },
    "debugMode": {
      "type": "boolean"
    },
    "language": {
      "type": "string",
      "enum": ["en", "ru", "fr", "de", "es", "pt", "it", "pl", "uk", "zh", "ja", "ko"]
    },
    "logLevel": {
      "type": "string",
      "enum": ["Debug", "Information", "Warning", "Error", "Critical"]
    },
    "tickRate": {
      "type": "integer",
      "minimum": 30,
      "maximum": 120
    }
  },
  "required": ["enabled", "language", "logLevel"]
}
```

### Validation Interface

```csharp
namespace ProjectLiberty.Configuration
{
    /// <summary>
    /// Configuration validator interface.
    /// </summary>
    public interface IConfigurationValidator
    {
        /// <summary>
        /// Validates configuration.
        /// </summary>
        /// <param name="config">Configuration to validate.</param>
        /// <returns>Validation result.</returns>
        ValidationResult Validate(IConfiguration config);
    }
    
    /// <summary>
    /// Validation result.
    /// </summary>
    public class ValidationResult
    {
        public bool IsValid { get; set; }
        public List<string> Errors { get; set; } = new List<string>();
        public List<string> Warnings { get; set; } = new List<string>();
    }
}
```

---

## Hot Reload

### File Watcher

```csharp
namespace ProjectLiberty.Configuration
{
    /// <summary>
    /// Configuration watcher for hot reload.
    /// </summary>
    public class ConfigurationWatcher
    {
        private readonly IConfiguration _configuration;
        private readonly FileSystemWatcher _watcher;
        
        public ConfigurationWatcher(IConfiguration configuration, string configDirectory)
        {
            _configuration = configuration;
            _watcher = new FileSystemWatcher(configDirectory, "*.json");
            _watcher.Changed += OnFileChanged;
            _watcher.EnableRaisingEvents = true;
        }
        
        private void OnFileChanged(object sender, FileSystemEventArgs e)
        {
            // Debounce rapid changes
            Thread.Sleep(100);
            
            try
            {
                _configuration.Reload();
                Logger.LogInformation($"Configuration reloaded: {e.Name}");
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, $"Failed to reload configuration: {e.Name}");
            }
        }
        
        public void Dispose()
        {
            _watcher.Dispose();
        }
    }
}
```

### Configuration Changed Event

```csharp
public class ConfigurationChangedEventArgs : EventArgs
{
    public string ChangedFile { get; }
    public DateTime ChangedTime { get; }
    
    public ConfigurationChangedEventArgs(string changedFile)
    {
        ChangedFile = changedFile;
        ChangedTime = DateTime.UtcNow;
    }
}
```

---

## Plugin Configuration

### Plugin Configuration File

Plugins can have their own configuration files:

```
Config/
├── core.json
├── plugins/
│   ├── myplugin.json
│   └── anotherplugin.json
```

### Plugin Configuration (myplugin.json)

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

### Accessing Plugin Configuration

```csharp
public class MyPlugin : PluginBase
{
    public override void Initialize()
    {
        var config = Configuration.GetPluginConfiguration(Manifest.Id);
        
        var maxInstances = config.GetValue<int>("settings.maxInstances", 5);
        var timeout = config.GetValue<int>("settings.timeout", 30);
        var debugMode = config.GetValue<bool>("settings.debugMode", false);
        
        Logger.LogInformation($"Configuration: MaxInstances={maxInstances}, Timeout={timeout}, DebugMode={debugMode}");
    }
    
    public override void OnConfigurationChanged(IConfiguration config)
    {
        Logger.LogInformation("Configuration changed");
        // Reload configuration
        // Apply new settings
    }
}
```

---

## Best Practices

### 1. Use Hierarchical Keys

```json
// GOOD - Hierarchical
{
  "TrafficStop.Stage.Greeting.Duration": 15
}

// BAD - Flat
{
  "GreetingDuration": 15
}
```

### 2. Provide Defaults

```csharp
// GOOD - With default
var timeout = config.GetValue<int>("timeout", 30);

// BAD - Without default (may throw)
var timeout = config.GetValue<int>("timeout");
```

### 3. Validate Configuration

```csharp
public override void Initialize()
{
    var config = Configuration.GetPluginConfiguration(Manifest.Id);
    var validator = new MyPluginConfigValidator();
    var result = validator.Validate(config);
    
    if (!result.IsValid)
    {
        Logger.LogError($"Invalid configuration: {string.Join(", ", result.Errors)}");
        return;
    }
}
```

### 4. Use Configuration Sections

```csharp
// GOOD - Use sections
var trafficStopConfig = config.GetSection("TrafficStop");
var duration = trafficStopConfig.GetValue<int>("Stage.Greeting.Duration");

// BAD - Long keys
var duration = config.GetValue<int>("TrafficStop.Stage.Greeting.Duration");
```

### 5. Handle Configuration Changes

```csharp
public class Service
{
    public Service(IConfiguration configuration)
    {
        configuration.ConfigurationChanged += OnConfigurationChanged;
    }
    
    private void OnConfigurationChanged(object sender, ConfigurationChangedEventArgs e)
    {
        Logger.LogInformation($"Configuration changed: {e.ChangedFile}");
        ReloadSettings();
    }
}
```

### 6. Use Appropriate Types

```json
// GOOD - Appropriate types
{
  "enabled": true,
  "count": 5,
  "timeout": 30.5,
  "name": "MyPlugin"
}

// BAD - String for everything
{
  "enabled": "true",
  "count": "5",
  "timeout": "30.5",
  "name": "MyPlugin"
}
```

### 7. Document Configuration

```json
{
  // Maximum number of concurrent instances
  "maxInstances": 5,
  
  // Timeout in seconds
  "timeout": 30,
  
  // Enable debug logging
  "debugMode": false
}
```

---

## Summary

The Configuration System provides:

- **JSON-based configuration** for all settings
- **Hot reload** for runtime configuration changes
- **Validation** using JSON schemas
- **Plugin-specific configuration** for third-party plugins
- **Hierarchical structure** for organization
- **Type-safe access** through generic methods
- **Default values** for missing keys

All configuration must be stored in JSON files. Hardcoded configuration values in code are FORBIDDEN.
