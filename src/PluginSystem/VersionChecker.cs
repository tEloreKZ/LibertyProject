using ProjectLiberty.Abstractions.Logging;
using ProjectLiberty.Abstractions.Plugins;

namespace ProjectLiberty.PluginSystem;

/// <summary>
/// Checks plugin version compatibility.
/// </summary>
public class VersionChecker
{
    private readonly ILogger _logger;
    private readonly string _coreVersion;

    public VersionChecker(ILogger logger, string coreVersion)
    {
        _logger = logger;
        _coreVersion = coreVersion;
    }

    /// <summary>
    /// Checks if a plugin is compatible with the current core version.
    /// </summary>
    /// <param name="plugin">Plugin to check.</param>
    /// <returns>True if compatible, false otherwise.</returns>
    public bool IsCompatible(IPlugin plugin)
    {
        var pluginVersion = plugin.Manifest.Version;
        var pluginName = plugin.Manifest.Name;

        _logger.LogDebug($"Checking compatibility for {pluginName} v{pluginVersion} with core v{_coreVersion}");

        // Simple version check - plugin must have same major version
        var coreMajor = GetMajorVersion(_coreVersion);
        var pluginMajor = GetMajorVersion(pluginVersion);

        if (coreMajor != pluginMajor)
        {
            _logger.LogWarning($"Plugin {pluginName} v{pluginVersion} is not compatible with core v{_coreVersion} (major version mismatch)");
            return false;
        }

        _logger.LogDebug($"Plugin {pluginName} v{pluginVersion} is compatible with core v{_coreVersion}");
        return true;
    }

    /// <summary>
    /// Checks compatibility for multiple plugins.
    /// </summary>
    /// <param name="plugins">Plugins to check.</param>
    /// <returns>Compatible plugins.</returns>
    public List<IPlugin> FilterCompatible(IEnumerable<IPlugin> plugins)
    {
        var compatible = new List<IPlugin>();

        foreach (var plugin in plugins)
        {
            if (IsCompatible(plugin))
            {
                compatible.Add(plugin);
            }
        }

        _logger.LogInformation($"Filtered {compatible.Count}/{plugins.Count()} compatible plugins");
        return compatible;
    }

    /// <summary>
    /// Extracts major version from version string.
    /// </summary>
    /// <param name="version">Version string (e.g., "1.2.3").</param>
    /// <returns>Major version number.</returns>
    private int GetMajorVersion(string version)
    {
        if (string.IsNullOrEmpty(version))
            return 0;

        var parts = version.Split('.');
        if (parts.Length == 0)
            return 0;

        if (int.TryParse(parts[0], out var major))
            return major;

        return 0;
    }
}
