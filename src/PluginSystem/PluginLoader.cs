using System.Reflection;
using ProjectLiberty.Abstractions.Logging;
using ProjectLiberty.Abstractions.Plugins;

namespace ProjectLiberty.PluginSystem;

/// <summary>
/// Plugin loader - loads plugin assemblies.
/// </summary>
public class PluginLoader
{
    private readonly ILogger _logger;
    private readonly string _pluginsDirectory;

    public PluginLoader(ILogger logger, string pluginsDirectory)
    {
        _logger = logger;
        _pluginsDirectory = pluginsDirectory;
    }

    /// <summary>
    /// Discovers all plugins in the plugins directory.
    /// </summary>
    /// <returns>Discovered plugins.</returns>
    public IEnumerable<IPlugin> DiscoverPlugins()
    {
        var plugins = new List<IPlugin>();

        if (!Directory.Exists(_pluginsDirectory))
        {
            _logger.LogWarning($"Plugins directory not found: {_pluginsDirectory}");
            return plugins;
        }

        var dllFiles = Directory.GetFiles(_pluginsDirectory, "*.dll", SearchOption.AllDirectories);

        foreach (var dllFile in dllFiles)
        {
            try
            {
                var assembly = Assembly.LoadFrom(dllFile);
                var pluginTypes = assembly.GetTypes()
                    .Where(t => typeof(IPlugin).IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract);

                foreach (var pluginType in pluginTypes)
                {
                    try
                    {
                        var plugin = (IPlugin)Activator.CreateInstance(pluginType)!;
                        
                        if (plugin.Discover())
                        {
                            plugins.Add(plugin);
                            _logger.LogInformation($"Discovered plugin: {plugin.Manifest.Name} v{plugin.Manifest.Version}");
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, $"Failed to instantiate plugin: {pluginType.Name}");
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to load assembly: {dllFile}");
            }
        }

        return plugins;
    }
}
