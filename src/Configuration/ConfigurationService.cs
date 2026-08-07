using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using ProjectLiberty.Abstractions.Configuration;
using ProjectLiberty.Abstractions.Logging;

namespace ProjectLiberty.Configuration;

/// <summary>Thread-safe JSON configuration with dotted-path access and debounced hot reload.</summary>
public sealed class ConfigurationService : IConfiguration, IDisposable
{
    private readonly Dictionary<string, JToken> _values = new Dictionary<string, JToken>(StringComparer.OrdinalIgnoreCase);
    private readonly object _sync = new object();
    private readonly ILogger _logger;
    private readonly string _configDirectory;
    private FileSystemWatcher? _watcher;
    private Timer? _reloadTimer;
    private string _pendingChangedFile = string.Empty;
    private bool _disposed;

    public event EventHandler<ConfigurationChangedEventArgs>? ConfigurationChanged;

    public ConfigurationService(ILogger logger, string configDirectory)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _configDirectory = configDirectory ?? throw new ArgumentNullException(nameof(configDirectory));
    }

    public T GetValue<T>(string key)
    {
        if (!TryGetToken(key, out var value))
            throw new KeyNotFoundException($"Configuration key not found: {key}");
        return ConvertValue<T>(key, value);
    }

    public T GetValue<T>(string key, T defaultValue)
    {
        return TryGetToken(key, out var value) ? ConvertValue<T>(key, value) : defaultValue;
    }

    public void SetValue<T>(string key, T value)
    {
        ValidateKey(key);
        ThrowIfDisposed();
        lock (_sync) _values[key] = value == null ? JValue.CreateNull() : JToken.FromObject(value);
    }

    public IConfigurationSection GetSection(string section) => new ConfigurationSection(section, this);
    public IConfiguration GetPluginConfiguration(string pluginId) => new PluginConfiguration(pluginId, this);
    public bool HasKey(string key) { lock (_sync) return _values.ContainsKey(key); }

    public void LoadConfiguration()
    {
        ThrowIfDisposed();
        var loaded = ReadConfiguration();
        lock (_sync)
        {
            _values.Clear();
            foreach (var pair in loaded) _values[pair.Key] = pair.Value;
        }
    }

    public void Reload()
    {
        ThrowIfDisposed();
        var loaded = ReadConfiguration();
        lock (_sync)
        {
            _values.Clear();
            foreach (var pair in loaded) _values[pair.Key] = pair.Value;
        }
        _logger.LogInformation("Configuration reloaded");
    }

    public void Save()
    {
        ThrowIfDisposed();
        Directory.CreateDirectory(_configDirectory);
        JObject root;
        lock (_sync) root = BuildObject(_values);

        var path = Path.Combine(_configDirectory, "core.json");
        var temporaryPath = path + ".tmp";
        File.WriteAllText(temporaryPath, root.ToString(Formatting.Indented));
        if (File.Exists(path)) File.Replace(temporaryPath, path, null);
        else File.Move(temporaryPath, path);
        _logger.LogInformation("Configuration saved");
    }

    public void EnableHotReload()
    {
        ThrowIfDisposed();
        if (!Directory.Exists(_configDirectory) || _watcher != null) return;
        _reloadTimer = new Timer(OnReloadTimer, null, Timeout.Infinite, Timeout.Infinite);
        _watcher = new FileSystemWatcher(_configDirectory, "*.json")
        {
            NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.FileName | NotifyFilters.Size
        };
        _watcher.Changed += OnFileChanged;
        _watcher.Created += OnFileChanged;
        _watcher.Deleted += OnFileChanged;
        _watcher.Renamed += OnFileChanged;
        _watcher.EnableRaisingEvents = true;
        _logger.LogInformation("Configuration hot reload enabled");
    }

    private Dictionary<string, JToken> ReadConfiguration()
    {
        var result = new Dictionary<string, JToken>(StringComparer.OrdinalIgnoreCase);
        if (!Directory.Exists(_configDirectory))
        {
            _logger.LogWarning($"Config directory not found: {_configDirectory}");
            return result;
        }

        foreach (var jsonFile in Directory.GetFiles(_configDirectory, "*.json").OrderBy(path => path, StringComparer.OrdinalIgnoreCase))
        {
            try
            {
                var root = JObject.Parse(File.ReadAllText(jsonFile));
                Flatten(root, string.Empty, result);
                _logger.LogDebug($"Loaded configuration: {Path.GetFileName(jsonFile)}");
            }
            catch (Exception ex)
            {
                throw new InvalidDataException($"Failed to load configuration: {jsonFile}", ex);
            }
        }
        return result;
    }

    private static void Flatten(JObject source, string prefix, IDictionary<string, JToken> target)
    {
        foreach (var property in source.Properties())
        {
            var key = string.IsNullOrEmpty(prefix) ? property.Name : prefix + "." + property.Name;
            if (property.Value is JObject child) Flatten(child, key, target);
            else target[key] = property.Value.DeepClone();
        }
    }

    private static JObject BuildObject(IEnumerable<KeyValuePair<string, JToken>> values)
    {
        var root = new JObject();
        foreach (var pair in values.OrderBy(item => item.Key, StringComparer.OrdinalIgnoreCase))
        {
            var parts = pair.Key.Split('.');
            var current = root;
            for (var index = 0; index < parts.Length - 1; index++)
            {
                if (!(current[parts[index]] is JObject child))
                {
                    child = new JObject();
                    current[parts[index]] = child;
                }
                current = child;
            }
            current[parts[parts.Length - 1]] = pair.Value.DeepClone();
        }
        return root;
    }

    private bool TryGetToken(string key, out JToken value)
    {
        ValidateKey(key);
        ThrowIfDisposed();
        lock (_sync)
        {
            if (_values.TryGetValue(key, out var stored))
            {
                value = stored.DeepClone();
                return true;
            }
        }
        value = JValue.CreateNull();
        return false;
    }

    private static T ConvertValue<T>(string key, JToken value)
    {
        try { return value.ToObject<T>()!; }
        catch (Exception ex) { throw new InvalidCastException($"Configuration value '{key}' cannot be converted to {typeof(T).Name}.", ex); }
    }

    private void OnFileChanged(object sender, FileSystemEventArgs e)
    {
        lock (_sync)
        {
            _pendingChangedFile = e.Name ?? string.Empty;
            _reloadTimer?.Change(250, Timeout.Infinite);
        }
    }

    private void OnReloadTimer(object? state)
    {
        string changedFile;
        lock (_sync) changedFile = _pendingChangedFile;
        try
        {
            Reload();
            ConfigurationChanged?.Invoke(this, new ConfigurationChangedEventArgs
            {
                ChangedFile = changedFile,
                ChangedTime = DateTime.UtcNow
            });
        }
        catch (Exception ex) { _logger.LogError(ex, $"Failed to hot reload configuration: {changedFile}"); }
    }

    public void Dispose()
    {
        if (_disposed) return;
        _watcher?.Dispose();
        _reloadTimer?.Dispose();
        lock (_sync) _values.Clear();
        _disposed = true;
    }

    private static void ValidateKey(string key)
    {
        if (string.IsNullOrWhiteSpace(key)) throw new ArgumentException("Configuration key cannot be empty.", nameof(key));
    }

    private void ThrowIfDisposed()
    {
        if (_disposed) throw new ObjectDisposedException(nameof(ConfigurationService));
    }

    private sealed class ConfigurationSection : IConfigurationSection
    {
        private readonly string _section;
        private readonly ConfigurationService _service;
        public string Key => _section;
        public ConfigurationSection(string section, ConfigurationService service) { ValidateKey(section); _section = section; _service = service; }
        public T GetValue<T>(string key) => _service.GetValue<T>($"{_section}.{key}");
        public T GetValue<T>(string key, T defaultValue) => _service.GetValue($"{_section}.{key}", defaultValue);
        public void SetValue<T>(string key, T value) => _service.SetValue($"{_section}.{key}", value);
        public IEnumerable<string> GetKeys()
        {
            var prefix = _section + ".";
            lock (_service._sync)
                return _service._values.Keys.Where(key => key.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                    .Select(key => key.Substring(prefix.Length)).ToArray();
        }
    }

    private sealed class PluginConfiguration : IConfiguration
    {
        private readonly string _prefix;
        private readonly ConfigurationService _service;
        public PluginConfiguration(string pluginId, ConfigurationService service) { ValidateKey(pluginId); _prefix = "plugins." + pluginId; _service = service; }
        public T GetValue<T>(string key) => _service.GetValue<T>($"{_prefix}.{key}");
        public T GetValue<T>(string key, T defaultValue) => _service.GetValue($"{_prefix}.{key}", defaultValue);
        public void SetValue<T>(string key, T value) => _service.SetValue($"{_prefix}.{key}", value);
        public IConfigurationSection GetSection(string section) => new ConfigurationSection($"{_prefix}.{section}", _service);
        public IConfiguration GetPluginConfiguration(string pluginId) => _service.GetPluginConfiguration(pluginId);
        public void Reload() => _service.Reload();
        public void Save() => _service.Save();
        public bool HasKey(string key) => _service.HasKey($"{_prefix}.{key}");
        public event EventHandler<ConfigurationChangedEventArgs>? ConfigurationChanged { add => _service.ConfigurationChanged += value; remove => _service.ConfigurationChanged -= value; }
    }
}
