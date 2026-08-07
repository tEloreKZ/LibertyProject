using Newtonsoft.Json;
using ProjectLiberty.Abstractions.Localization;
using ProjectLiberty.Abstractions.Logging;

namespace ProjectLiberty.Localization;

/// <summary>Thread-safe JSON localization service with English fallback.</summary>
public sealed class LocalizationService : ILocalization, IDisposable
{
    private readonly Dictionary<string, Dictionary<string, string>> _translations = new Dictionary<string, Dictionary<string, string>>(StringComparer.OrdinalIgnoreCase);
    private readonly object _sync = new object();
    private readonly ILogger _logger;
    private readonly string _languagesDirectory;
    private string _currentLanguage = "en";
    private bool _disposed;

    public LocalizationService(ILogger logger, string languagesDirectory)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _languagesDirectory = languagesDirectory ?? throw new ArgumentNullException(nameof(languagesDirectory));
    }

    public string CurrentLanguage
    {
        get { lock (_sync) return _currentLanguage; }
        set
        {
            if (string.IsNullOrWhiteSpace(value)) throw new ArgumentException("Language cannot be empty.", nameof(value));
            ThrowIfDisposed();
            lock (_sync)
            {
                if (string.Equals(_currentLanguage, value, StringComparison.OrdinalIgnoreCase)) return;
                _currentLanguage = value;
            }
            _logger.LogInformation($"Language changed to: {value}");
        }
    }

    public void SetLanguage(string language) => CurrentLanguage = language;

    public string GetString(string key)
    {
        if (string.IsNullOrWhiteSpace(key)) throw new ArgumentException("Translation key cannot be empty.", nameof(key));
        ThrowIfDisposed();
        lock (_sync)
        {
            if (TryGetString(_currentLanguage, key, out var value) ||
                (!string.Equals(_currentLanguage, "en", StringComparison.OrdinalIgnoreCase) && TryGetString("en", key, out value)))
                return value;
        }
        _logger.LogDebug($"Translation not found: {key}");
        return key;
    }

    public string GetString(string key, params object[] args)
    {
        return string.Format(System.Globalization.CultureInfo.CurrentCulture, GetString(key), args);
    }

    public string GetString(string key, string @default)
    {
        var value = GetString(key);
        return value == key ? @default : value;
    }

    public bool HasKey(string key)
    {
        ThrowIfDisposed();
        lock (_sync) return TryGetString(_currentLanguage, key, out _) || TryGetString("en", key, out _);
    }

    public IEnumerable<string> GetAvailableLanguages()
    {
        ThrowIfDisposed();
        lock (_sync) return _translations.Keys.OrderBy(key => key, StringComparer.OrdinalIgnoreCase).ToArray();
    }

    public void LoadTranslations()
    {
        ThrowIfDisposed();
        var loaded = ReadTranslations();
        lock (_sync)
        {
            _translations.Clear();
            foreach (var pair in loaded) _translations[pair.Key] = pair.Value;
        }
    }

    public void Reload()
    {
        LoadTranslations();
        _logger.LogInformation("Translations reloaded");
    }

    private Dictionary<string, Dictionary<string, string>> ReadTranslations()
    {
        var loaded = new Dictionary<string, Dictionary<string, string>>(StringComparer.OrdinalIgnoreCase);
        if (!Directory.Exists(_languagesDirectory))
        {
            _logger.LogWarning($"Languages directory not found: {_languagesDirectory}");
            return loaded;
        }

        foreach (var languageDir in Directory.GetDirectories(_languagesDirectory))
        {
            var languageCode = Path.GetFileName(languageDir);
            var languageTranslations = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            foreach (var jsonFile in Directory.GetFiles(languageDir, "*.json").OrderBy(path => path, StringComparer.OrdinalIgnoreCase))
            {
                try
                {
                    var values = JsonConvert.DeserializeObject<Dictionary<string, string>>(File.ReadAllText(jsonFile));
                    if (values == null) continue;
                    var prefix = Path.GetFileNameWithoutExtension(jsonFile) + ".";
                    foreach (var pair in values) languageTranslations[prefix + pair.Key] = pair.Value;
                }
                catch (Exception ex) { throw new InvalidDataException($"Failed to load translation file: {jsonFile}", ex); }
            }
            loaded[languageCode] = languageTranslations;
            _logger.LogInformation($"Loaded {languageTranslations.Count} translations for language: {languageCode}");
        }
        return loaded;
    }

    private bool TryGetString(string language, string key, out string value)
    {
        value = string.Empty;
        return _translations.TryGetValue(language, out var values) && values.TryGetValue(key, out value!);
    }

    public void Dispose()
    {
        if (_disposed) return;
        lock (_sync) _translations.Clear();
        _disposed = true;
    }

    private void ThrowIfDisposed()
    {
        if (_disposed) throw new ObjectDisposedException(nameof(LocalizationService));
    }
}
