namespace ProjectLiberty.Abstractions.Localization;

/// <summary>
/// Localization service interface - manages translations.
/// </summary>
public interface ILocalization
{
    /// <summary>
    /// Gets the current language.
    /// </summary>
    string CurrentLanguage { get; }
    
    /// <summary>
    /// Sets the current language.
    /// </summary>
    /// <param name="language">Language code (e.g., "en", "ru").</param>
    void SetLanguage(string language);
    
    /// <summary>
    /// Gets a localized string.
    /// </summary>
    /// <param name="key">Localization key.</param>
    /// <returns>Localized string.</returns>
    string GetString(string key);
    
    /// <summary>
    /// Gets a localized string with formatting.
    /// </summary>
    /// <param name="key">Localization key.</param>
    /// <param name="args">Format arguments.</param>
    /// <returns>Formatted localized string.</returns>
    string GetString(string key, params object[] args);
    
    /// <summary>
    /// Gets a localized string with default fallback.
    /// </summary>
    /// <param name="key">Localization key.</param>
    /// <param name="default">Default value if key not found.</param>
    /// <returns>Localized string or default.</returns>
    string GetString(string key, string @default);
    
    /// <summary>
    /// Checks if a localization key exists.
    /// </summary>
    /// <param name="key">Localization key.</param>
    /// <returns>True if exists, false otherwise.</returns>
    bool HasKey(string key);
    
    /// <summary>
    /// Reloads all translations.
    /// </summary>
    void Reload();
    
    /// <summary>
    /// Gets all available languages.
    /// </summary>
    /// <returns>Available language codes.</returns>
    IEnumerable<string> GetAvailableLanguages();
}
