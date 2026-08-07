using ProjectLiberty.Abstractions.Logging;
using ProjectLiberty.Abstractions.Plugins;

namespace ProjectLiberty.PluginSystem;

/// <summary>
/// Resolves plugin dependencies and determines load order.
/// </summary>
public class DependencyResolver
{
    private readonly ILogger _logger;

    public DependencyResolver(ILogger logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Resolves plugin dependencies and returns load order.
    /// </summary>
    /// <param name="plugins">Plugins to resolve.</param>
    /// <returns>Load order (plugins without dependencies first).</returns>
    public List<IPlugin> ResolveLoadOrder(IEnumerable<IPlugin> plugins)
    {
        var pluginList = plugins.ToList();
        var loadOrder = new List<IPlugin>();
        var loaded = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        _logger.LogInformation($"Resolving dependencies for {pluginList.Count} plugins...");

        // Load plugins with no dependencies first
        var noDeps = pluginList.Where(p => !p.Manifest.Dependencies.Any()).ToList();
        foreach (var plugin in noDeps)
        {
            loadOrder.Add(plugin);
            loaded.Add(plugin.Manifest.Id);
            _logger.LogDebug($"Added to load order: {plugin.Manifest.Name} (no dependencies)");
        }

        // Resolve remaining plugins
        var remaining = pluginList.Where(p => !loaded.Contains(p.Manifest.Id)).ToList();
        var attempts = 0;
        var maxAttempts = remaining.Count * 2;

        while (remaining.Count > 0 && attempts < maxAttempts)
        {
            var canLoad = new List<IPlugin>();

            foreach (var plugin in remaining)
            {
                var deps = plugin.Manifest.Dependencies.ToList();
                var allDepsLoaded = deps.All(depId => loaded.Contains(depId));

                if (allDepsLoaded)
                {
                    canLoad.Add(plugin);
                    loaded.Add(plugin.Manifest.Id);
                    _logger.LogDebug($"Added to load order: {plugin.Manifest.Name} (dependencies satisfied)");
                }
            }

            if (canLoad.Count == 0)
            {
                attempts++;
                continue;
            }

            loadOrder.AddRange(canLoad);
            remaining.RemoveAll(p => canLoad.Contains(p));
            attempts = 0;
        }

        // Check for circular dependencies
        if (remaining.Count > 0)
        {
            _logger.LogWarning($"Could not resolve dependencies for {remaining.Count} plugins:");
            foreach (var plugin in remaining)
            {
                _logger.LogWarning($"  - {plugin.Manifest.Name} (missing: {string.Join(", ", plugin.Manifest.Dependencies)})");
            }
        }

        _logger.LogInformation($"Resolved load order: {loadOrder.Count} plugins");
        return loadOrder;
    }

    /// <summary>
    /// Validates that all plugin dependencies can be satisfied.
    /// </summary>
    /// <param name="plugins">Plugins to validate.</param>
    /// <returns>True if all dependencies can be satisfied, false otherwise.</returns>
    public bool ValidateDependencies(IEnumerable<IPlugin> plugins)
    {
        var pluginList = plugins.ToList();
        var pluginIds = new HashSet<string>(pluginList.Select(p => p.Manifest.Id), StringComparer.OrdinalIgnoreCase);
        var valid = true;

        foreach (var duplicate in pluginList.GroupBy(plugin => plugin.Manifest.Id, StringComparer.OrdinalIgnoreCase).Where(group => group.Count() > 1))
        {
            _logger.LogError($"Duplicate plugin ID: {duplicate.Key}");
            valid = false;
        }

        foreach (var plugin in pluginList)
        {
            foreach (var depId in plugin.Manifest.Dependencies)
            {
                if (!pluginIds.Contains(depId))
                {
                    _logger.LogError($"Plugin {plugin.Manifest.Name} depends on missing plugin: {depId}");
                    valid = false;
                }
            }
        }

        return valid;
    }
}
