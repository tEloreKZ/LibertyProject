using ProjectLiberty.Abstractions.Configuration;
using ProjectLiberty.Abstractions.GameHost;
using ProjectLiberty.Abstractions.Events;
using ProjectLiberty.Abstractions.Localization;
using ProjectLiberty.Abstractions.Logging;

namespace ProjectLiberty.Abstractions.Plugins;

/// <summary>
/// Plugin interface - base interface for all plugins.
/// </summary>
public interface IPlugin
{
    /// <summary>
    /// Gets the plugin manifest.
    /// </summary>
    IPluginManifest Manifest { get; }
    
    /// <summary>
    /// Gets the plugin context.
    /// </summary>
    IPluginContext? Context { get; }
    
    /// <summary>
    /// Discovers the plugin - called during plugin discovery.
    /// </summary>
    /// <returns>True if discovery successful, false otherwise.</returns>
    bool Discover();
    
    /// <summary>
    /// Loads the plugin - called during plugin loading.
    /// </summary>
    void Load();
    
    /// <summary>
    /// Initializes the plugin - called after all plugins are loaded.
    /// </summary>
    void Initialize();
    
    /// <summary>
    /// Enables the plugin - called when plugin is enabled.
    /// </summary>
    void Enable();
    
    /// <summary>
    /// Disables the plugin - called when plugin is disabled.
    /// </summary>
    void Disable();
    
    /// <summary>
    /// Unloads the plugin - called during plugin unloading.
    /// </summary>
    void Unload();
    
    /// <summary>
    /// Sets the plugin context.
    /// </summary>
    /// <param name="context">Plugin context.</param>
    void SetContext(IPluginContext context);
}

/// <summary>
/// Plugin manifest interface.
/// </summary>
public interface IPluginManifest
{
    /// <summary>
    /// Gets the plugin ID.
    /// </summary>
    string Id { get; }
    
    /// <summary>
    /// Gets the plugin name.
    /// </summary>
    string Name { get; }
    
    /// <summary>
    /// Gets the plugin version.
    /// </summary>
    string Version { get; }
    
    /// <summary>
    /// Gets the plugin author.
    /// </summary>
    string Author { get; }
    
    /// <summary>
    /// Gets the plugin description.
    /// </summary>
    string Description { get; }
    
    /// <summary>
    /// Gets the plugin dependencies.
    /// </summary>
    IEnumerable<string> Dependencies { get; }
}

/// <summary>Optional core-version compatibility requirement for a plugin manifest.</summary>
public interface ICoreVersionRequirement
{
    string MinimumCoreVersion { get; }
}

/// <summary>
/// Plugin context interface - provides access to core services.
/// </summary>
public interface IPluginContext
{
    /// <summary>
    /// Gets the logger.
    /// </summary>
    ILogger Logger { get; }
    
    /// <summary>
    /// Gets the event bus.
    /// </summary>
    IEventBus EventBus { get; }
    
    /// <summary>
    /// Gets the localization service.
    /// </summary>
    ILocalization Localization { get; }
    
    /// <summary>
    /// Gets the configuration service.
    /// </summary>
    IConfiguration Configuration { get; }
    
    /// <summary>
    /// Gets the game host.
    /// </summary>
    IGameHost GameHost { get; }
}
