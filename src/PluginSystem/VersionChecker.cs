using ProjectLiberty.Abstractions.Logging;
using ProjectLiberty.Abstractions.Plugins;

namespace ProjectLiberty.PluginSystem;

/// <summary>Validates plugin versions and optional minimum-core requirements.</summary>
public sealed class VersionChecker
{
    private readonly ILogger _logger;
    private readonly Version _coreVersion;

    public VersionChecker(ILogger logger, string coreVersion)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        if (!Version.TryParse(coreVersion, out _coreVersion!))
            throw new ArgumentException("Core version must be valid.", nameof(coreVersion));
    }

    public bool IsCompatible(IPlugin plugin)
    {
        if (plugin == null) throw new ArgumentNullException(nameof(plugin));
        if (!Version.TryParse(plugin.Manifest.Version, out _))
        {
            _logger.LogWarning($"Plugin {plugin.Manifest.Name} has invalid version '{plugin.Manifest.Version}'");
            return false;
        }

        if (plugin.Manifest is ICoreVersionRequirement requirement)
        {
            if (!Version.TryParse(requirement.MinimumCoreVersion, out var minimum))
            {
                _logger.LogWarning($"Plugin {plugin.Manifest.Name} has invalid minimum core version");
                return false;
            }
            if (_coreVersion < minimum)
            {
                _logger.LogWarning($"Plugin {plugin.Manifest.Name} requires core {minimum} or newer");
                return false;
            }
        }
        return true;
    }

    public List<IPlugin> FilterCompatible(IEnumerable<IPlugin> plugins)
    {
        var pluginList = plugins.ToList();
        var compatible = pluginList.Where(IsCompatible).ToList();
        _logger.LogInformation($"Filtered {compatible.Count}/{pluginList.Count} compatible plugins");
        return compatible;
    }
}
