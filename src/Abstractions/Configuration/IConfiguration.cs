namespace ProjectLiberty.Abstractions.Configuration;

/// <summary>
/// Configuration service interface - manages configuration settings.
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
    event EventHandler<ConfigurationChangedEventArgs>? ConfigurationChanged;
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
    /// <returns>All keys.</returns>
    IEnumerable<string> GetKeys();
}

/// <summary>
/// Configuration changed event arguments.
/// </summary>
public class ConfigurationChangedEventArgs : EventArgs
{
    /// <summary>
    /// Gets the changed file.
    /// </summary>
    public string ChangedFile { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets the changed time.
    /// </summary>
    public DateTime ChangedTime { get; set; } = DateTime.UtcNow;
}
