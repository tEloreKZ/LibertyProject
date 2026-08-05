using Newtonsoft.Json;
using ProjectLiberty.Abstractions.Localization;
using ProjectLiberty.Abstractions.Logging;

namespace ProjectLiberty.Localization;

/// <summary>
/// Localization service implementation.
/// </summary>
public class LocalizationService : ILocalization, IDisposable
{
    private readonly Dictionary<string, Dictionary<string, string>> _translations = new Dictionary<string, Dictionary<string, string>>();
    private readonly ILogger _logger;
    private readonly string _languagesDirectory;
    private string _currentLanguage = "en";
    private bool _disposed;

    public LocalizationService(ILogger logger, string languagesDirectory)
    {
        _logger = logger;
        _languagesDirectory = languagesDirectory;
    }

    public string CurrentLanguage
    {
        get => _currentLanguage;
        set
        {
            if (_currentLanguage != value)
            {
                _currentLanguage = value;
                _logger.LogInformation($"Language changed to: {value}");
            }
        }
    }

    public void SetLanguage(string language)
    {
        CurrentLanguage = language;
    }

    public string GetString(string key)
    {
        if (!_translations.TryGetValue(_currentLanguage, out var languageTranslations))
        {
            _logger.LogWarning($"Language not loaded: {_currentLanguage}");
            return key;
        }

        if (!languageTranslations.TryGetValue(key, out var value))
        {
            _logger.LogDebug($"Translation not found: {key}");
            return key;
        }

        return value;
    }

    public string GetString(string key, params object[] args)
    {
        var format = GetString(key);
        return string.Format(format, args);
    }

    public string GetString(string key, string @default)
    {
        if (!_translations.TryGetValue(_currentLanguage, out var languageTranslations))
        {
            return @default;
        }

        if (!languageTranslations.TryGetValue(key, out var value))
        {
            return @default;
        }

        return value;
    }

    public bool HasKey(string key)
    {
        if (!_translations.TryGetValue(_currentLanguage, out var languageTranslations))
        {
            return false;
        }

        return languageTranslations.ContainsKey(key);
    }

    public void Reload()
    {
        _logger.LogInformation("Reloading translations...");
        _translations.Clear();
        LoadTranslations();
        _logger.LogInformation("Translations reloaded");
    }

    public IEnumerable<string> GetAvailableLanguages()
    {
        return _translations.Keys;
    }

    /// <summary>
    /// Loads all translations from the languages directory.
    /// </summary>
    public void LoadTranslations()
    {
        if (!Directory.Exists(_languagesDirectory))
        {
            _logger.LogWarning($"Languages directory not found: {_languagesDirectory}");
            return;
        }

        var languageDirectories = Directory.GetDirectories(_languagesDirectory);

        foreach (var languageDir in languageDirectories)
        {
            var languageCode = Path.GetFileName(languageDir);
            var jsonFiles = Directory.GetFiles(languageDir, "*.json");

            var languageTranslations = new Dictionary<string, string>();

            foreach (var jsonFile in jsonFiles)
            {
                try
                {
                    var json = File.ReadAllText(jsonFile);
                    var translations = JsonConvert.DeserializeObject<Dictionary<string, string>>(json);

                    if (translations != null)
                    {
                        foreach (var kvp in translations)
                        {
                            languageTranslations[$"{Path.GetFileNameWithoutExtension(jsonFile)}.{kvp.Key}"] = kvp.Value;
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Failed to load translation file: {jsonFile}");
                }
            }

            _translations[languageCode] = languageTranslations;
            _logger.LogInformation($"Loaded {languageTranslations.Count} translations for language: {languageCode}");
        }
    }

    public void Dispose()
    {
        if (_disposed)
            return;

        _translations.Clear();
        _disposed = true;
    }
}
