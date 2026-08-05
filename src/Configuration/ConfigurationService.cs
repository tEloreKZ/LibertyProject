using Newtonsoft.Json;
using ProjectLiberty.Abstractions.Configuration;
using ProjectLiberty.Abstractions.Logging;

namespace ProjectLiberty.Configuration;

/// <summary>
/// Configuration service implementation.
/// </summary>
public class ConfigurationService : IConfiguration, IDisposable
{
    private readonly Dictionary<string, object> _values = new Dictionary<string, object>();
    private readonly ILogger _logger;
    private readonly string _configDirectory;
    private FileSystemWatcher? _watcher;
    private bool _disposed;

    public event EventHandler<ConfigurationChangedEventArgs>? ConfigurationChanged;

    public ConfigurationService(ILogger logger, string configDirectory)
    {
        _logger = logger;
        _configDirectory = configDirectory;
    }

    public T GetValue<T>(string key)
    {
        if (!_values.TryGetValue(key, out var value))
        {
            throw new KeyNotFoundException($"Configuration key not found: {key}");
        }

        return (T)value;
    }

    public T GetValue<T>(string key, T defaultValue)
    {
        if (!_values.TryGetValue(key, out var value))
        {
            return defaultValue;
        }

        return (T)value;
    }

    public void SetValue<T>(string key, T value)
    {
        _values[key] = value;
    }

    public IConfigurationSection GetSection(string section)
    {
        return new ConfigurationSection(section, this);
    }

    public IConfiguration GetPluginConfiguration(string pluginId)
    {
        return new PluginConfiguration(pluginId, this);
    }

    public void Reload()
    {
        _logger.LogInformation("Reloading configuration...");
        _values.Clear();
        LoadConfiguration();
        _logger.LogInformation("Configuration reloaded");
    }

    public void Save()
    {
        _logger.LogInformation("Saving configuration...");
        SaveConfiguration();
        _logger.LogInformation("Configuration saved");
    }

    public bool HasKey(string key)
    {
        return _values.ContainsKey(key);
    }

    /// <summary>
    /// Loads configuration from files.
    /// </summary>
    public void LoadConfiguration()
    {
        if (!Directory.Exists(_configDirectory))
        {
            _logger.LogWarning($"Config directory not found: {_configDirectory}");
            return;
        }

        var jsonFiles = Directory.GetFiles(_configDirectory, "*.json");

        foreach (var jsonFile in jsonFiles)
        {
            try
            {
                var json = File.ReadAllText(jsonFile);
                var config = JsonConvert.DeserializeObject<Dictionary<string, object>>(json);

                if (config != null)
                {
                    foreach (var kvp in config)
                    {
                        _values[kvp.Key] = kvp.Value;
                    }
                }

                _logger.LogDebug($"Loaded configuration: {Path.GetFileName(jsonFile)}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to load configuration: {jsonFile}");
            }
        }
    }

    /// <summary>
    /// Saves configuration to files.
    /// </summary>
    private void SaveConfiguration()
    {
        if (!Directory.Exists(_configDirectory))
        {
            Directory.CreateDirectory(_configDirectory);
        }

        // Save to core.json
        var coreConfig = new Dictionary<string, object>();
        foreach (var kvp in _values.Where(kvp => !kvp.Key.Contains('.')))
        {
            coreConfig[kvp.Key] = kvp.Value;
        }

        var json = JsonConvert.SerializeObject(coreConfig, Formatting.Indented);
        File.WriteAllText(Path.Combine(_configDirectory, "core.json"), json);
    }

    /// <summary>
    /// Enables hot reload for configuration files.
    /// </summary>
    public void EnableHotReload()
    {
        if (!Directory.Exists(_configDirectory))
        {
            return;
        }

        _watcher = new FileSystemWatcher(_configDirectory, "*.json");
        _watcher.Changed += OnFileChanged;
        _watcher.EnableRaisingEvents = true;
        _logger.LogInformation("Configuration hot reload enabled");
    }

    private void OnFileChanged(object sender, FileSystemEventArgs e)
    {
        try
        {
            Thread.Sleep(100); // Debounce
            Reload();
            ConfigurationChanged?.Invoke(this, new ConfigurationChangedEventArgs
            {
                ChangedFile = e.Name,
                ChangedTime = DateTime.UtcNow
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Failed to reload configuration: {e.Name}");
        }
    }

    public void Dispose()
    {
        if (_disposed)
            return;

        _watcher?.Dispose();
        _values.Clear();
        _disposed = true;
    }

    private class ConfigurationSection : IConfigurationSection
    {
        private readonly string _section;
        private readonly ConfigurationService _service;

        public string Key => _section;

        public ConfigurationSection(string section, ConfigurationService service)
        {
            _section = section;
            _service = service;
        }

        public T GetValue<T>(string key)
        {
            return _service.GetValue<T>($"{_section}.{key}");
        }

        public T GetValue<T>(string key, T defaultValue)
        {
            return _service.GetValue($"{_section}.{key}", defaultValue);
        }

        public void SetValue<T>(string key, T value)
        {
            _service.SetValue($"{_section}.{key}", value);
        }

        public IEnumerable<string> GetKeys()
        {
            return _service._values.Keys
                .Where(k => k.StartsWith($"{_section}."))
                .Select(k => k.Substring(_section.Length + 1));
        }
    }

    private class PluginConfiguration : IConfiguration
    {
        private readonly string _pluginId;
        private readonly ConfigurationService _service;

        public PluginConfiguration(string pluginId, ConfigurationService service)
        {
            _pluginId = pluginId;
            _service = service;
        }

        public T GetValue<T>(string key)
        {
            return _service.GetValue<T>($"plugins.{_pluginId}.{key}");
        }

        public T GetValue<T>(string key, T defaultValue)
        {
            return _service.GetValue($"plugins.{_pluginId}.{key}", defaultValue);
        }

        public void SetValue<T>(string key, T value)
        {
            _service.SetValue($"plugins.{_pluginId}.{key}", value);
        }

        public IConfigurationSection GetSection(string section)
        {
            return new ConfigurationSection($"plugins.{_pluginId}.{section}", _service);
        }

        public IConfiguration GetPluginConfiguration(string pluginId)
        {
            return new PluginConfiguration(pluginId, _service);
        }

        public void Reload()
        {
            _service.Reload();
        }

        public void Save()
        {
            _service.Save();
        }

        public bool HasKey(string key)
        {
            return _service.HasKey($"plugins.{_pluginId}.{key}");
        }

        public event EventHandler<ConfigurationChangedEventArgs>? ConfigurationChanged
        {
            add => _service.ConfigurationChanged += value;
            remove => _service.ConfigurationChanged -= value;
        }
    }
}
