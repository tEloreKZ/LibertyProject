using ProjectLiberty.Abstractions.Configuration;
using ProjectLiberty.Abstractions.Events;
using ProjectLiberty.Abstractions.GameHost;
using ProjectLiberty.Abstractions.Localization;
using ProjectLiberty.Abstractions.Logging;
using ProjectLiberty.Abstractions.Plugins;

namespace ProjectLiberty.PluginSystem;

/// <summary>
/// Plugin context implementation - provides access to core services for plugins.
/// </summary>
public class PluginContext : IPluginContext
{
    public ILogger Logger { get; }
    public IEventBus EventBus { get; }
    public ILocalization Localization { get; }
    public IConfiguration Configuration { get; }
    public IGameHost GameHost { get; }

    public PluginContext(
        ILogger logger,
        IEventBus eventBus,
        ILocalization localization,
        IConfiguration configuration,
        IGameHost gameHost)
    {
        Logger = logger;
        EventBus = eventBus;
        Localization = localization;
        Configuration = configuration;
        GameHost = gameHost;
    }
}
