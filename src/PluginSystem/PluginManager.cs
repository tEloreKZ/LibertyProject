using ProjectLiberty.Abstractions.Logging;
using ProjectLiberty.Abstractions.Plugins;

namespace ProjectLiberty.PluginSystem;

/// <summary>
/// Plugin manager - manages plugin lifecycle.
/// </summary>
public class PluginManager : IDisposable
{
    private readonly Dictionary<string, IPlugin> _plugins = new Dictionary<string, IPlugin>();
    private readonly ILogger _logger;
    private readonly IPluginContext _pluginContext;
    private readonly DependencyResolver _dependencyResolver;
    private readonly VersionChecker _versionChecker;
    private bool _disposed;

    public PluginManager(ILogger logger, IPluginContext pluginContext)
    {
        _logger = logger;
        _pluginContext = pluginContext;
        _dependencyResolver = new DependencyResolver(logger);
        _versionChecker = new VersionChecker(logger, "1.0.0"); // Core version
    }

    /// <summary>
    /// Loads a plugin.
    /// </summary>
    /// <param name="plugin">Plugin to load.</param>
    public void LoadPlugin(IPlugin plugin)
    {
        var pluginId = plugin.Manifest.Id;

        if (_plugins.ContainsKey(pluginId))
        {
            _logger.LogWarning($"Plugin already loaded: {pluginId}");
            return;
        }

        // Check version compatibility
        if (!_versionChecker.IsCompatible(plugin))
        {
            _logger.LogWarning($"Plugin {plugin.Manifest.Name} is not compatible with current core version");
            return;
        }

        try
        {
            plugin.SetContext(_pluginContext);
            plugin.Load();
            _plugins[pluginId] = plugin;
            _logger.LogInformation($"Loaded plugin: {plugin.Manifest.Name} v{plugin.Manifest.Version}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Failed to load plugin: {pluginId}");
        }
    }

    /// <summary>
    /// Loads multiple plugins with dependency resolution.
    /// </summary>
    /// <param name="plugins">Plugins to load.</param>
    public void LoadPlugins(IEnumerable<IPlugin> plugins)
    {
        var pluginList = plugins.ToList();

        // Validate dependencies
        if (!_dependencyResolver.ValidateDependencies(pluginList))
        {
            _logger.LogWarning("Plugin dependency validation failed");
            return;
        }

        // Filter compatible plugins
        var compatiblePlugins = _versionChecker.FilterCompatible(pluginList);

        // Resolve load order
        var loadOrder = _dependencyResolver.ResolveLoadOrder(compatiblePlugins);

        // Load in order
        foreach (var plugin in loadOrder)
        {
            LoadPlugin(plugin);
        }

        _logger.LogInformation($"Loaded {loadOrder.Count} plugins in dependency order");
    }

    /// <summary>
    /// Initializes all loaded plugins.
    /// </summary>
    public void InitializePlugins()
    {
        _logger.LogInformation("Initializing plugins...");

        foreach (var plugin in _plugins.Values)
        {
            try
            {
                plugin.Initialize();
                _logger.LogDebug($"Initialized plugin: {plugin.Manifest.Name}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to initialize plugin: {plugin.Manifest.Id}");
            }
        }

        _logger.LogInformation($"Initialized {_plugins.Count} plugins");
    }

    /// <summary>
    /// Enables a plugin.
    /// </summary>
    /// <param name="pluginId">Plugin ID.</param>
    public void EnablePlugin(string pluginId)
    {
        if (!_plugins.TryGetValue(pluginId, out var plugin))
        {
            _logger.LogWarning($"Plugin not found: {pluginId}");
            return;
        }

        try
        {
            plugin.Enable();
            _logger.LogInformation($"Enabled plugin: {plugin.Manifest.Name}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Failed to enable plugin: {pluginId}");
        }
    }

    /// <summary>
    /// Disables a plugin.
    /// </summary>
    /// <param name="pluginId">Plugin ID.</param>
    public void DisablePlugin(string pluginId)
    {
        if (!_plugins.TryGetValue(pluginId, out var plugin))
        {
            _logger.LogWarning($"Plugin not found: {pluginId}");
            return;
        }

        try
        {
            plugin.Disable();
            _logger.LogInformation($"Disabled plugin: {plugin.Manifest.Name}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Failed to disable plugin: {pluginId}");
        }
    }

    /// <summary>
    /// Unloads a plugin.
    /// </summary>
    /// <param name="pluginId">Plugin ID.</param>
    public void UnloadPlugin(string pluginId)
    {
        if (!_plugins.TryGetValue(pluginId, out var plugin))
        {
            _logger.LogWarning($"Plugin not found: {pluginId}");
            return;
        }

        try
        {
            plugin.Unload();
            _plugins.Remove(pluginId);
            _logger.LogInformation($"Unloaded plugin: {plugin.Manifest.Name}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Failed to unload plugin: {pluginId}");
        }
    }

    /// <summary>
    /// Gets a plugin by ID.
    /// </summary>
    /// <param name="pluginId">Plugin ID.</param>
    /// <returns>Plugin, or null if not found.</returns>
    public IPlugin? GetPlugin(string pluginId)
    {
        return _plugins.TryGetValue(pluginId, out var plugin) ? plugin : null;
    }

    /// <summary>
    /// Gets all loaded plugins.
    /// </summary>
    /// <returns>All loaded plugins.</returns>
    public IEnumerable<IPlugin> GetAllPlugins()
    {
        return _plugins.Values;
    }

    public void Dispose()
    {
        if (_disposed)
            return;

        _logger.LogInformation("Unloading all plugins...");

        foreach (var plugin in _plugins.Values.ToList())
        {
            try
            {
                plugin.Unload();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to unload plugin: {plugin.Manifest.Id}");
            }
        }

        _plugins.Clear();
        _disposed = true;
    }
}
