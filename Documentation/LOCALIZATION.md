# Localization

**Localization System Documentation**

---

## Table of Contents

1. [Overview](#overview)
2. [Design Principles](#design-principles)
3. [Localization Interface](#localization-interface)
4. [Translation Files](#translation-files)
5. [String Formatting](#string-formatting)
6. [Plugin Localization](#plugin-localization)
7. [Adding New Languages](#adding-new-languages)
8. [Best Practices](#best-practices)

---

## Overview

Project Liberty requires full multi-language support. All text must be stored outside code, allowing for easy translation and language switching without code modifications.

### Current CEF integration

The production React UI consumes the existing localization service; it does not maintain a separate JavaScript translation database.

```text
Languages/<code>/ui.json
    -> LocalizationService (English fallback)
    -> ILocalizationCatalog.GetStrings("ui.")
    -> managed ui:state
    -> React translator
```

`ILocalizationCatalog` is intentionally separate from `ILocalization`, preserving source compatibility for third-party implementations of the original plugin API. The shipped and fully populated UI catalogs are English and Russian. Select the active language through `localization.defaultLanguage` in `Config/core.json`.

### Planned Language Set

The repository currently ships English (`en`) and Russian (`ru`) resources. The remaining entries are planned and require translation files before they are supported:

- English (en)
- Russian (ru)
- French (fr)
- German (de)
- Spanish (es)
- Portuguese (pt)
- Italian (it)
- Polish (pl)
- Ukrainian (uk)
- Chinese (zh)
- Japanese (ja)
- Korean (ko)

### Architecture

```
┌─────────────────────────────────────────┐
│         Application Layer              │
│  (Plugins, Systems, UI)                │
└─────────────────────────────────────────┘
                  ↓
┌─────────────────────────────────────────┐
│      Localization Service               │
│  (ILocalization)                        │
└─────────────────────────────────────────┘
                  ↓
┌─────────────────────────────────────────┐
│      Translation Cache                  │
│  (In-memory cache)                      │
└─────────────────────────────────────────┘
                  ↓
┌─────────────────────────────────────────┐
│      Translation Files                  │
│  (JSON files per language)              │
└─────────────────────────────────────────┘
```

---

## Design Principles

### 1. No Hardcoded Strings

**FORBIDDEN**: Hardcoded strings in code

```csharp
// FORBIDDEN - Hardcoded string
public class PlayerManager
{
    public void SpawnPlayer()
    {
        Console.WriteLine("Player spawned");  // BAD
    }
}
```

**REQUIRED**: All strings through localization

```csharp
// REQUIRED - Through localization
public class PlayerManager
{
    private readonly ILocalization _localization;
    
    public PlayerManager(ILocalization localization)
    {
        _localization = localization;
    }
    
    public void SpawnPlayer()
    {
        var message = _localization.GetString("PlayerSpawned");
        Console.WriteLine(message);  // GOOD
    }
}
```

### 2. Key Naming Convention

Use hierarchical keys with dot notation:

```json
{
  "PlayerSpawned": "Player spawned",
  "TrafficStop.Started": "Traffic stop started",
  "Callout.Accepted": "Callout accepted",
  "Dispatch.BackupRequested": "Backup requested"
}
```

### 3. Fallback Mechanism

Provide fallback to English if translation missing:

```csharp
public string GetString(string key)
{
    if (_translations.ContainsKey(key))
        return _translations[key];
    
    // Fallback to English
    if (_englishTranslations.ContainsKey(key))
        return _englishTranslations[key];
    
    // Fallback to key itself
    return key;
}
```

---

## Localization Interface

```csharp
namespace ProjectLiberty.Abstractions
{
    /// <summary>
    /// Localization service interface.
    /// </summary>
    public interface ILocalization
    {
        /// <summary>
        /// Gets the current language code.
        /// </summary>
        string CurrentLanguage { get; }
        
        /// <summary>
        /// Gets the available languages.
        /// </summary>
        IEnumerable<string> AvailableLanguages { get; }
        
        /// <summary>
        /// Gets a localized string.
        /// </summary>
        /// <param name="key">Translation key.</param>
        /// <returns>Localized string.</returns>
        string GetString(string key);
        
        /// <summary>
        /// Gets a localized string with formatting.
        /// </summary>
        /// <param name="key">Translation key.</param>
        /// <param name="args">Format arguments.</param>
        /// <returns>Formatted localized string.</returns>
        string GetString(string key, params object[] args);
        
        /// <summary>
        /// Gets a localized string for a specific plugin.
        /// </summary>
        /// <param name="pluginId">Plugin ID.</param>
        /// <param name="key">Translation key.</param>
        /// <returns>Localized string.</returns>
        string GetPluginString(string pluginId, string key);
        
        /// <summary>
        /// Gets a localized string for a specific plugin with formatting.
        /// </summary>
        /// <param name="pluginId">Plugin ID.</param>
        /// <param name="key">Translation key.</param>
        /// <param name="args">Format arguments.</param>
        /// <returns>Formatted localized string.</returns>
        string GetPluginString(string pluginId, string key, params object[] args);
        
        /// <summary>
        /// Sets the current language.
        /// </summary>
        /// <param name="languageCode">Language code.</param>
        void SetLanguage(string languageCode);
        
        /// <summary>
        /// Reloads translations for the current language.
        /// </summary>
        void Reload();
        
        /// <summary>
        /// Checks if a translation key exists.
        /// </summary>
        /// <param name="key">Translation key.</param>
        /// <returns>True if exists, false otherwise.</returns>
        bool HasKey(string key);
        
        /// <summary>
        /// Checks if a plugin translation key exists.
        /// </summary>
        /// <param name="pluginId">Plugin ID.</param>
        /// <param name="key">Translation key.</param>
        /// <returns>True if exists, false otherwise.</returns>
        bool HasPluginKey(string pluginId, string key);
    }
}
```

---

## Translation Files

### File Structure

```
Languages/
├── en/
│   ├── core.json
│   ├── dispatch.json
│   ├── trafficstops.json
│   ├── callouts.json
│   ├── arrests.json
│   └── ui.json
├── ru/
│   ├── core.json
│   ├── dispatch.json
│   ├── trafficstops.json
│   ├── callouts.json
│   ├── arrests.json
│   └── ui.json
└── ...
```

### Core Translations (core.json)

```json
{
  "General.Loading": "Loading...",
  "General.Error": "Error",
  "General.Warning": "Warning",
  "General.Information": "Information",
  "General.Success": "Success",
  "General.Cancel": "Cancel",
  "General.Confirm": "Confirm",
  "General.Yes": "Yes",
  "General.No": "No",
  "General.Ok": "OK",
  
  "Player.Spawned": "Player spawned",
  "Player.Died": "Player died",
  "Player.Teleported": "Player teleported",
  
  "Vehicle.Created": "Vehicle created",
  "Vehicle.Destroyed": "Vehicle destroyed",
  "Vehicle.Stolen": "Vehicle stolen",
  
  "Game.Loading": "Game loading",
  "Game.Ready": "Game ready",
  "Game.Paused": "Game paused"
}
```

### Dispatch Translations (dispatch.json)

```json
{
  "Dispatch.BackupRequested": "Backup requested",
  "Dispatch.BackupArrived": "Backup arrived",
  "Dispatch.UnitAvailable": "Unit available",
  "Dispatch.UnitBusy": "Unit busy",
  "Dispatch.UnitOffline": "Unit offline",
  
  "CallCode.Pursuit": "Pursuit in progress",
  "CallCode.Robbery": "Robbery in progress",
  "CallCode.Assault": "Assault in progress",
  "CallCode.TrafficStop": "Traffic stop",
  
  "Radio.Channel1": "Channel 1",
  "Radio.Channel2": "Channel 2",
  "Radio.Channel3": "Channel 3"
}
```

### Traffic Stop Translations (trafficstops.json)

```json
{
  "TrafficStop.Started": "Traffic stop started",
  "TrafficStop.Ended": "Traffic stop ended",
  "TrafficStop.Stage.Observe": "Observing vehicle",
  "TrafficStop.Stage.PullOver": "Pulling over vehicle",
  "TrafficStop.Stage.Greeting": "Greeting driver",
  "TrafficStop.Stage.Documents": "Requesting documents",
  "TrafficStop.Stage.Questioning": "Questioning driver",
  "TrafficStop.Stage.Database": "Checking database",
  "TrafficStop.Stage.Citation": "Writing citation",
  "TrafficStop.Stage.Search": "Searching vehicle",
  "TrafficStop.Stage.Arrest": "Arresting suspect",
  "TrafficStop.Stage.Release": "Releasing driver",
  
  "TrafficStop.Driver.Compliant": "Driver is compliant",
  "TrafficStop.Driver.Resistant": "Driver is resistant",
  "TrafficStop.Driver.Fleeing": "Driver is fleeing"
}
```

### Callout Translations (callouts.json)

```json
{
  "Callout.Started": "Callout started: {0}",
  "Callout.Accepted": "Callout accepted",
  "Callout.Declined": "Callout declined",
  "Callout.Finished": "Callout finished",
  "Callout.Failed": "Callout failed",
  
  "Callout.Pursuit": "Vehicle pursuit",
  "Callout.Robbery": "Store robbery",
  "Callout.Assault": "Assault in progress",
  "Callout.Shooting": "Shooting reported",
  "Callout.StolenVehicle": "Stolen vehicle"
}
```

### Arrest Translations (arrests.json)

```json
{
  "Arrest.Started": "Arrest started",
  "Arrest.Completed": "Arrest completed",
  "Arrest.Failed": "Arrest failed",
  
  "Arrest.Reason.Speeding": "Speeding",
  "Arrest.Reason.RecklessDriving": "Reckless driving",
  "Arrest.Reason.DUI": "Driving under influence",
  "Arrest.Reason.Warrant": "Outstanding warrant",
  "Arrest.Reason.Violence": "Violence",
  
  "Arrest.Handcuffed": "Suspect handcuffed",
  "Arrest.Transporting": "Transporting to station",
  "Arrest.Booked": "Suspect booked"
}
```

### UI Translations (ui.json)

```json
{
  "UI.MainMenu": "Main Menu",
  "UI.Settings": "Settings",
  "UI.Plugins": "Plugins",
  "UI.Exit": "Exit",
  
  "UI.Settings.Language": "Language",
  "UI.Settings.Graphics": "Graphics",
  "UI.Settings.Audio": "Audio",
  "UI.Settings.Controls": "Controls",
  
  "UI.Button.Click": "Click",
  "UI.Button.Press": "Press",
  "UI.Button.Hold": "Hold"
}
```

---

## String Formatting

### Basic Formatting

```csharp
// Translation file
{
  "PlayerSpawnedAt": "Player spawned at {0}"
}

// Code
var message = _localization.GetString("PlayerSpawnedAt", position);
// Result: "Player spawned at (100, 100, 100)"
```

### Multiple Parameters

```csharp
// Translation file
{
  "CalloutInfo": "Callout: {0} at {1}, Priority: {2}"
}

// Code
var message = _localization.GetString("CalloutInfo", 
    calloutName, 
    position, 
    priority);
// Result: "Callout: Robbery at (100, 100, 100), Priority: High"
```

### Named Parameters

```csharp
// Translation file
{
  "ArrestInfo": "Arrest: {suspect} for {reason} at {location}"
}

// Code
var message = _localization.GetString("ArrestInfo", 
    suspectName, 
    arrestReason, 
    location);
// Result: "Arrest: John Doe for Speeding at (100, 100, 100)"
```

---

## Plugin Localization

### Plugin Translation Files

Plugins can have their own translation files:

```
Languages/
├── en/
│   └── plugins/
│       ├── myplugin.json
│       └── anotherplugin.json
├── ru/
│   └── plugins/
│       ├── myplugin.json
│       └── anotherplugin.json
```

### Plugin Translation File (myplugin.json)

```json
{
  "MyPlugin.Name": "My Plugin",
  "MyPlugin.Description": "An example plugin",
  "MyPlugin.Command.Help": "Shows plugin information",
  "MyPlugin.Event.PlayerSpawned": "Player spawned in my plugin"
}
```

### Accessing Plugin Translations

```csharp
public class MyPlugin : PluginBase
{
    public override void Initialize()
    {
        var pluginName = Localization.GetPluginString("myplugin", "MyPlugin.Name");
        var description = Localization.GetPluginString("myplugin", "MyPlugin.Description");
        
        Logger.LogInformation($"{pluginName}: {description}");
    }
    
    private void OnPlayerSpawned(PlayerSpawnedEvent @event)
    {
        var message = Localization.GetPluginString("myplugin", "MyPlugin.Event.PlayerSpawned");
        Logger.LogInformation(message);
    }
}
```

---

## Adding New Languages

### Step 1: Create Language Directory

```bash
mkdir Languages/xx
```

### Step 2: Create Translation Files

Create all required JSON files:

```bash
touch Languages/xx/core.json
touch Languages/xx/dispatch.json
touch Languages/xx/trafficstops.json
touch Languages/xx/callouts.json
touch Languages/xx/arrests.json
touch Languages/xx/ui.json
```

### Step 3: Translate Content

Translate all strings from English to the new language:

```json
{
  "General.Loading": "Загрузка...",
  "General.Error": "Ошибка",
  "General.Warning": "Предупреждение",
  "General.Information": "Информация",
  "General.Success": "Успех",
  "General.Cancel": "Отмена",
  "General.Confirm": "Подтвердить",
  "General.Yes": "Да",
  "General.No": "Нет",
  "General.Ok": "ОК"
}
```

### Step 4: Register Language

Add the language code to the localization service:

```csharp
public class LocalizationService : ILocalization
{
    private readonly HashSet<string> _availableLanguages = new HashSet<string>
    {
        "en", "ru", "fr", "de", "es", "pt", "it", "pl", "uk", "zh", "ja", "ko", "xx"
    };
    
    public IEnumerable<string> AvailableLanguages => _availableLanguages;
}
```

### Step 5: Test

Test the new language:

```csharp
localization.SetLanguage("xx");
var message = localization.GetString("General.Loading");
Console.WriteLine(message); // Should show translated text
```

---

## Best Practices

### 1. Use Descriptive Keys

```json
// GOOD - Descriptive key
{
  "TrafficStop.Stage.Greeting": "Greeting driver"
}

// BAD - Generic key
{
  "Stage1": "Greeting driver"
}
```

### 2. Keep Keys Hierarchical

```json
// GOOD - Hierarchical
{
  "TrafficStop.Stage.Greeting": "Greeting driver",
  "TrafficStop.Stage.Documents": "Requesting documents"
}

// BAD - Flat
{
  "Greeting": "Greeting driver",
  "Documents": "Requesting documents"
}
```

### 3. Provide Context in Comments

```json
{
  // Shown when player spawns
  "Player.Spawned": "Player spawned",
  
  // Shown when player dies
  "Player.Died": "Player died"
}
```

### 4. Handle Missing Translations

```csharp
public string GetString(string key)
{
    if (_translations.TryGetValue(key, out var value))
        return value;
    
    // Log missing translation
    _logger.LogWarning($"Missing translation for key: {key}");
    
    // Fallback to English
    if (_englishTranslations.TryGetValue(key, out var englishValue))
        return englishValue;
    
    // Fallback to key
    return key;
}
```

### 5. Use Pluralization

```json
{
  "Vehicle.Count.One": "1 vehicle",
  "Vehicle.Count.Many": "{0} vehicles"
}
```

```csharp
public string GetVehicleCount(int count)
{
    var key = count == 1 ? "Vehicle.Count.One" : "Vehicle.Count.Many";
    return _localization.GetString(key, count);
}
```

### 6. Avoid Concatenation

```json
// GOOD - Single string
{
  "PlayerInfo": "Player {0} has {1} health"
}

// BAD - Concatenation
{
  "PlayerName": "Player {0}",
  "PlayerHealth": "has {0} health"
}
```

### 7. Test All Languages

Test all supported languages before release:

```csharp
[Test]
public void TestAllLanguages()
{
    foreach (var language in _localization.AvailableLanguages)
    {
        _localization.SetLanguage(language);
        
        var message = _localization.GetString("General.Loading");
        Assert.IsNotNull(message);
        Assert.AreNotEqual("General.Loading", message);
    }
}
```

---

## Summary

The Localization System provides:

- **Extensible multi-language support** (English and Russian currently shipped; additional languages planned)
- **No hardcoded strings** in code
- **Hierarchical key structure** for organization
- **Plugin localization** for third-party plugins
- **Fallback mechanism** for missing translations
- **String formatting** for dynamic content
- **Easy language addition** without code changes

All text MUST be stored in translation files. Hardcoded strings in code are FORBIDDEN.
