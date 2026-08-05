# Save System

**Save System Architecture Documentation**

---

## Table of Contents

1. [Overview](#overview)
2. [Save System Interface](#save-system-interface)
3. [Save Data Structure](#save-data-structure)
4. [Save Slots](#save-slots)
5. **Plugin Saves**(#plugin-saves)
6. [Serialization](#serialization)
7. [Validation](#validation)
8. [Auto-Save](#auto-save)
9. [Best Practices](#best-practices)

---

## Overview

Project Liberty provides a comprehensive save system that allows plugins to save their own data independently. The system supports multiple save slots, automatic saving, and plugin-specific saves.

### Architecture

```
┌─────────────────────────────────────────┐
│         Application Layer              │
│  (Core Systems, Plugins)               │
└─────────────────────────────────────────┘
                  ↓
┌─────────────────────────────────────────┐
│         Save Service                   │
│  (ISaveSystem)                          │
└─────────────────────────────────────────┘
                  ↓
┌─────────────────────────────────────────┐
│         Save Manager                   │
│  (Slot management, serialization)      │
└─────────────────────────────────────────┘
                  ↓
┌─────────────────────────────────────────┐
│         Save Files                     │
│  (JSON files in Saves/ directory)      │
└─────────────────────────────────────────┘
```

---

## Save System Interface

```csharp
namespace ProjectLiberty.Abstractions
{
    /// <summary>
    /// Save system interface.
    /// </summary>
    public interface ISaveSystem
    {
        /// <summary>
        /// Gets the number of save slots.
        /// </summary>
        int SlotCount { get; }
        
        /// <summary>
        /// Gets the current save slot.
        /// </summary>
        int CurrentSlot { get; }
        
        /// <summary>
        /// Sets the current save slot.
        /// </summary>
        /// <param name="slot">Slot number.</param>
        void SetCurrentSlot(int slot);
        
        /// <summary>
        /// Saves the current game state.
        /// </summary>
        void Save();
        
        /// <summary>
        /// Saves to a specific slot.
        /// </summary>
        /// <param name="slot">Slot number.</param>
        void Save(int slot);
        
        /// <summary>
        /// Loads the current save slot.
        /// </summary>
        void Load();
        
        /// <summary>
        /// Loads from a specific slot.
        /// </summary>
        /// <param name="slot">Slot number.</param>
        void Load(int slot);
        
        /// <summary>
        /// Gets save slot information.
        /// </summary>
        /// <param name="slot">Slot number.</param>
        /// <returns>Save slot information.</returns>
        SaveSlotInfo GetSlotInfo(int slot);
        
        /// <summary>
        /// Gets all save slot information.
        /// </summary>
        /// <returns>All save slot information.</returns>
        IEnumerable<SaveSlotInfo> GetAllSlotInfo();
        
        /// <summary>
        /// Deletes a save slot.
        /// </summary>
        /// <param name="slot">Slot number.</param>
        void DeleteSlot(int slot);
        
        /// <summary>
        /// Checks if a slot has a save.
        /// </summary>
        /// <param name="slot">Slot number.</param>
        /// <returns>True if has save, false otherwise.</returns>
        bool HasSave(int slot);
        
        /// <summary>
        /// Gets plugin save data.
        /// </summary>
        /// <param name="pluginId">Plugin ID.</param>
        /// <returns>Plugin save data.</returns>
        IPluginSaveData GetPluginSaveData(string pluginId);
        
        /// <summary>
        /// Event raised when save completes.
        /// </summary>
        event EventHandler<SaveCompletedEventArgs> SaveCompleted;
        
        /// <summary>
        /// Event raised when load completes.
        /// </summary>
        event EventHandler<LoadCompletedEventArgs> LoadCompleted;
    }
    
    /// <summary>
    /// Plugin save data interface.
    /// </summary>
    public interface IPluginSaveData
    {
        /// <summary>
        /// Gets the plugin ID.
        /// </summary>
        string PluginId { get; }
        
        /// <summary>
        /// Gets a value from plugin save data.
        /// </summary>
        /// <typeparam name="T">Value type.</typeparam>
        /// <param name="key">Data key.</param>
        /// <returns>Data value.</returns>
        T GetValue<T>(string key);
        
        /// <summary>
        /// Gets a value with default.
        /// </summary>
        /// <typeparam name="T">Value type.</typeparam>
        /// <param name="key">Data key.</param>
        /// <param name="defaultValue">Default value.</param>
        /// <returns>Data value or default.</returns>
        T GetValue<T>(string key, T defaultValue);
        
        /// <summary>
        /// Sets a value in plugin save data.
        /// </summary>
        /// <typeparam name="T">Value type.</typeparam>
        /// <param name="key">Data key.</param>
        /// <param name="value">Value to set.</param>
        void SetValue<T>(string key, T value);
        
        /// <summary>
        /// Checks if a key exists.
        /// </summary>
        /// <param name="key">Data key.</param>
        /// <returns>True if exists, false otherwise.</returns>
        bool HasKey(string key);
        
        /// <summary>
        /// Gets all keys.
        /// </summary>
        /// <returns>All keys.</returns>
        IEnumerable<string> GetKeys();
    }
}
```

---

## Save Data Structure

### Save Data Interface

```csharp
namespace ProjectLiberty.SaveSystem
{
    /// <summary>
    /// Save data structure.
    /// </summary>
    public class SaveData
    {
        /// <summary>
        /// Gets or sets the save version.
        /// </summary>
        public int Version { get; set; }
        
        /// <summary>
        /// Gets or sets the save timestamp.
        /// </summary>
        public DateTime Timestamp { get; set; }
        
        /// <summary>
        /// Gets or sets the game time.
        /// </summary>
        public GameTime GameTime { get; set; }
        
        /// <summary>
        /// Gets or sets the player data.
        /// </summary>
        public PlayerSaveData PlayerData { get; set; }
        
        /// <summary>
        /// Gets or sets the world data.
        /// </summary>
        public WorldSaveData WorldData { get; set; }
        
        /// <summary>
        /// Gets or sets the plugin data.
        /// </summary>
        public Dictionary<string, PluginSaveData> PluginData { get; set; } = new Dictionary<string, PluginSaveData>();
    }
    
    /// <summary>
    /// Game time save data.
    /// </summary>
    public class GameTime
    {
        public int Hour { get; set; }
        public int Minute { get; set; }
        public int Second { get; set; }
        public int Day { get; set; }
        public int Month { get; set; }
        public int Year { get; set; }
    }
    
    /// <summary>
    /// Player save data.
    /// </summary>
    public class PlayerSaveData
    {
        public Vector3 Position { get; set; }
        public float Heading { get; set; }
        public int Health { get; set; }
        public int Armor { get; set; }
        public WeaponHash CurrentWeapon { get; set; }
        public Dictionary<WeaponHash, int> Ammo { get; set; } = new Dictionary<WeaponHash, int>();
    }
    
    /// <summary>
    /// World save data.
    /// </summary>
    public class WorldSaveData
    {
        public WeatherType Weather { get; set; }
        public GameTime Time { get; set; }
        public Vector3? Waypoint { get; set; }
    }
    
    /// <summary>
    /// Plugin save data.
    /// </summary>
    public class PluginSaveData
    {
        public string PluginId { get; set; }
        public string PluginVersion { get; set; }
        public Dictionary<string, object> Data { get; set; } = new Dictionary<string, object>();
    }
}
```

---

## Save Slots

### Save Slot Information

```csharp
namespace ProjectLiberty.SaveSystem
{
    /// <summary>
    /// Save slot information.
    /// </summary>
    public class SaveSlotInfo
    {
        /// <summary>
        /// Gets or sets the slot number.
        /// </summary>
        public int Slot { get; set; }
        
        /// <summary>
        /// Gets or sets whether the slot has a save.
        /// </summary>
        public bool HasSave { get; set; }
        
        /// <summary>
        /// Gets or sets the save timestamp.
        /// </summary>
        public DateTime? Timestamp { get; set; }
        
        /// <summary>
        /// Gets or sets the save version.
        /// </summary>
        public int? Version { get; set; }
        
        /// <summary>
        /// Gets or sets the screenshot file path.
        /// </summary>
        public string ScreenshotPath { get; set; }
        
        /// <summary>
        /// Gets or sets the play time in seconds.
        /// </summary>
        public int PlayTimeSeconds { get; set; }
    }
}
```

### Slot Manager

```csharp
namespace ProjectLiberty.SaveSystem
{
    /// <summary>
    /// Save slot manager.
    /// </summary>
    public class SaveSlotManager
    {
        private const int MaxSlots = 10;
        private readonly SaveSlotInfo[] _slots;
        
        public SaveSlotManager()
        {
            _slots = new SaveSlotInfo[MaxSlots];
            for (int i = 0; i < MaxSlots; i++)
            {
                _slots[i] = new SaveSlotInfo { Slot = i };
            }
        }
        
        public SaveSlotInfo GetSlot(int slot)
        {
            if (slot < 0 || slot >= MaxSlots)
                throw new ArgumentOutOfRangeException(nameof(slot));
            
            return _slots[slot];
        }
        
        public IEnumerable<SaveSlotInfo> GetAllSlots()
        {
            return _slots;
        }
        
        public void UpdateSlot(int slot, SaveSlotInfo info)
        {
            if (slot < 0 || slot >= MaxSlots)
                throw new ArgumentOutOfRangeException(nameof(slot));
            
            _slots[slot] = info;
        }
    }
}
```

---

## Plugin Saves

### Plugin Save Interface

```csharp
namespace ProjectLiberty.SaveSystem
{
    /// <summary>
    /// Plugin save data implementation.
    /// </summary>
    public class PluginSaveDataImpl : IPluginSaveData
    {
        private readonly Dictionary<string, object> _data;
        
        public string PluginId { get; }
        
        public PluginSaveDataImpl(string pluginId, Dictionary<string, object> data)
        {
            PluginId = pluginId;
            _data = data ?? new Dictionary<string, object>();
        }
        
        public T GetValue<T>(string key)
        {
            if (_data.TryGetValue(key, out var value))
                return (T)value;
            
            throw new KeyNotFoundException($"Key '{key}' not found in plugin save data");
        }
        
        public T GetValue<T>(string key, T defaultValue)
        {
            if (_data.TryGetValue(key, out var value))
                return (T)value;
            
            return defaultValue;
        }
        
        public void SetValue<T>(string key, T value)
        {
            _data[key] = value;
        }
        
        public bool HasKey(string key)
        {
            return _data.ContainsKey(key);
        }
        
        public IEnumerable<string> GetKeys()
        {
            return _data.Keys;
        }
    }
}
```

### Plugin Save Usage

```csharp
public class MyPlugin : PluginBase
{
    private IPluginSaveData _saveData;
    
    public override void Initialize()
    {
        _saveData = Context.SaveSystem.GetPluginSaveData(Manifest.Id);
        
        // Load saved data
        var lastPosition = _saveData.GetValue<Vector3>("LastPosition", Vector3.Zero);
        var visitCount = _saveData.GetValue<int>("VisitCount", 0);
        
        Logger.LogInformation($"Loaded save data: Position={lastPosition}, Visits={visitCount}");
    }
    
    public override void Enable()
    {
        // Update save data
        _saveData.SetValue("LastPosition", GetCurrentPosition());
        _saveData.SetValue("VisitCount", _saveData.GetValue<int>("VisitCount", 0) + 1);
    }
}
```

---

## Serialization

### JSON Serializer

```csharp
namespace ProjectLiberty.SaveSystem
{
    /// <summary>
    /// Save serializer.
    /// </summary>
    public class SaveSerializer
    {
        private readonly JsonSerializerOptions _options;
        
        public SaveSerializer()
        {
            _options = new JsonSerializerOptions
            {
                WriteIndented = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };
            
            _options.Converters.Add(new Vector3Converter());
            _options.Converters.Add(new WeaponHashConverter());
        }
        
        public string Serialize(SaveData data)
        {
            return JsonSerializer.Serialize(data, _options);
        }
        
        public SaveData Deserialize(string json)
        {
            return JsonSerializer.Deserialize<SaveData>(json, _options);
        }
    }
    
    /// <summary>
    /// Vector3 JSON converter.
    /// </summary>
    public class Vector3Converter : JsonConverter<Vector3>
    {
        public override Vector3 Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var obj = JsonSerializer.Deserialize<Dictionary<string, float>>(ref reader, options);
            return new Vector3(obj["x"], obj["y"], obj["z"]);
        }
        
        public override void Write(Utf8JsonWriter writer, Vector3 value, JsonSerializerOptions options)
        {
            writer.WriteStartObject();
            writer.WriteNumber("x", value.X);
            writer.WriteNumber("y", value.Y);
            writer.WriteNumber("z", value.Z);
            writer.WriteEndObject();
        }
    }
}
```

---

## Validation

### Save Validator

```csharp
namespace ProjectLiberty.SaveSystem
{
    /// <summary>
    /// Save validator.
    /// </summary>
    public class SaveValidator
    {
        private const int CurrentSaveVersion = 1;
        
        public ValidationResult Validate(SaveData data)
        {
            var result = new ValidationResult();
            
            // Check version
            if (data.Version > CurrentSaveVersion)
            {
                result.AddError($"Save version {data.Version} is newer than current version {CurrentSaveVersion}");
            }
            
            // Check timestamp
            if (data.Timestamp > DateTime.UtcNow.AddMinutes(5))
            {
                result.AddWarning("Save timestamp is in the future");
            }
            
            // Check player data
            if (data.PlayerData == null)
            {
                result.AddError("Player data is missing");
            }
            else
            {
                if (data.PlayerData.Health < 0 || data.PlayerData.Health > 1000)
                {
                    result.AddWarning($"Invalid player health: {data.PlayerData.Health}");
                }
            }
            
            // Check plugin data
            foreach (var pluginData in data.PluginData)
            {
                if (string.IsNullOrEmpty(pluginData.Key))
                {
                    result.AddError("Plugin has empty ID");
                }
            }
            
            return result;
        }
    }
    
    /// <summary>
    /// Validation result.
    /// </summary>
    public class ValidationResult
    {
        public List<string> Errors { get; } = new List<string>();
        public List<string> Warnings { get; } = new List<string>();
        
        public bool IsValid => Errors.Count == 0;
        
        public void AddError(string error)
        {
            Errors.Add(error);
        }
        
        public void AddWarning(string warning)
        {
            Warnings.Add(warning);
        }
    }
}
```

---

## Auto-Save

### Auto-Save Configuration

```json
{
  "autoSave": {
    "enabled": true,
    "interval": 300,
    "onEvent": true,
    "events": [
      "CalloutFinished",
      "TrafficStopEnded",
      "ArrestCompleted"
    ]
  }
}
```

### Auto-Save Implementation

```csharp
namespace ProjectLiberty.SaveSystem
{
    /// <summary>
    /// Auto-save manager.
    /// </summary>
    public class AutoSaveManager
    {
        private readonly ISaveSystem _saveSystem;
        private readonly IConfiguration _configuration;
        private readonly Timer _timer;
        
        public AutoSaveManager(ISaveSystem saveSystem, IConfiguration configuration, IEventBus eventBus)
        {
            _saveSystem = saveSystem;
            _configuration = configuration;
            
            // Timer-based auto-save
            var interval = configuration.GetValue<int>("autoSave.interval", 300);
            _timer = new Timer(OnAutoSave, null, interval * 1000, interval * 1000);
            
            // Event-based auto-save
            if (configuration.GetValue<bool>("autoSave.onEvent", true))
            {
                var events = configuration.GetValue<string[]>("autoSave.events", new string[0]);
                SubscribeToEvents(eventBus, events);
            }
        }
        
        private void OnAutoSave(object state)
        {
            try
            {
                _saveSystem.Save();
                Logger.LogInformation("Auto-save completed");
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Auto-save failed");
            }
        }
        
        private void SubscribeToEvents(IEventBus eventBus, string[] events)
        {
            // Subscribe to specified events
            foreach (var eventName in events)
            {
                // Subscribe and trigger save
            }
        }
        
        public void Dispose()
        {
            _timer.Dispose();
        }
    }
}
```

---

## Best Practices

### 1. Version Your Saves

```csharp
public class SaveData
{
    public int Version { get; set; } = 1; // Always version your saves
}
```

### 2. Validate Before Loading

```csharp
public void Load(int slot)
{
    var data = LoadData(slot);
    var validator = new SaveValidator();
    var result = validator.Validate(data);
    
    if (!result.IsValid)
    {
        Logger.LogError($"Invalid save: {string.Join(", ", result.Errors)}");
        return;
    }
    
    // Load data
}
```

### 3. Handle Migration

```csharp
public SaveData MigrateSave(SaveData oldData)
{
    if (oldData.Version == CurrentVersion)
        return oldData;
    
    // Migrate from old version to new version
    var newData = new SaveData();
    
    // Copy compatible data
    newData.PlayerData = oldData.PlayerData;
    
    // Add new fields with defaults
    newData.NewField = defaultValue;
    
    newData.Version = CurrentVersion;
    return newData;
}
```

### 4. Use Appropriate Data Types

```csharp
// GOOD - Appropriate types
public class PlayerSaveData
{
    public Vector3 Position { get; set; }
    public int Health { get; set; }
    public DateTime LastSave { get; set; }
}

// BAD - Everything as string
public class PlayerSaveData
{
    public string Position { get; set; }
    public string Health { get; set; }
    public string LastSave { get; set; }
}
```

### 5. Clean Up Old Saves

```csharp
public void CleanupOldSaves()
{
    var maxSaves = _configuration.GetValue<int>("maxAutoSaves", 5);
    var saves = GetAllSlotInfo()
        .Where(s => s.HasSave)
        .OrderByDescending(s => s.Timestamp)
        .Skip(maxSaves);
    
    foreach (var save in saves)
    {
        DeleteSlot(save.Slot);
        Logger.LogInformation($"Deleted old save: Slot {save.Slot}");
    }
}
```

### 6. Save Before Critical Operations

```csharp
public void StartCallout()
{
    // Save before starting callout
    _saveSystem.Save();
    
    // Start callout
    _calloutService.StartCallout();
}
```

### 7. Provide User Feedback

```csharp
public void Save()
{
    try
    {
        _saveSystem.Save();
        _uiService.ShowNotification("Game saved");
    }
    catch (Exception ex)
    {
        _uiService.ShowNotification("Save failed: " + ex.Message);
    }
}
```

---

## Summary

The Save System provides:

- **Multiple save slots** for different game states
- **Plugin-specific saves** for third-party plugins
- **JSON serialization** for human-readable saves
- **Validation** to ensure save integrity
- **Auto-save** for automatic game state preservation
- **Version management** for save compatibility
- **Migration support** for save format changes

All plugins must use the save system API. Direct file I/O for save data is FORBIDDEN.
