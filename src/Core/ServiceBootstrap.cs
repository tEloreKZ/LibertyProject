using ProjectLiberty.Abstractions.Configuration;
using ProjectLiberty.Abstractions.Events;
using ProjectLiberty.Abstractions.GameHost;
using ProjectLiberty.Abstractions.Localization;
using ProjectLiberty.Abstractions.Logging;
using ProjectLiberty.Abstractions.Plugins;
using ProjectLiberty.Abstractions.UI;
using ProjectLiberty.Configuration;
using ProjectLiberty.Core.DependencyInjection;
using ProjectLiberty.EventBus;
using ProjectLiberty.GameHost;
using ProjectLiberty.Localization;
using ProjectLiberty.Logging;
using ProjectLiberty.PluginSystem;
using ProjectLiberty.UI;

namespace ProjectLiberty.Core;

/// <summary>
/// Service bootstrap - configures dependency injection.
/// </summary>
public class ServiceBootstrap
{
    private readonly string _baseDirectory;
    private readonly bool _useMockHost;

    public ServiceBootstrap(string baseDirectory, bool useMockHost = false)
    {
        _baseDirectory = baseDirectory;
        _useMockHost = useMockHost;
    }

    /// <summary>
    /// Configures all services in the service container.
    /// </summary>
    /// <returns>Configured service container.</returns>
    public ServiceContainer ConfigureServices()
    {
        var logger = CreateLogger("ProjectLiberty");
        var container = new ServiceContainer(logger);
        container.RegisterSingleton<ILogger>(logger);

        // Register configuration
        var configDirectory = Path.Combine(_baseDirectory, "Config");
        var configuration = new ConfigurationService(logger, configDirectory);
        configuration.LoadConfiguration();
        configuration.EnableHotReload();
        container.RegisterSingleton<IConfiguration>(configuration);

        // Register localization
        var languagesDirectory = Path.Combine(_baseDirectory, "Languages");
        var localization = new LocalizationService(logger, languagesDirectory);
        localization.LoadTranslations();
        localization.SetLanguage(configuration.GetValue("localization.defaultLanguage", "en"));
        container.RegisterSingleton<ILocalization>(localization);
        container.RegisterSingleton<ILocalizationCatalog>(localization);

        // Register event bus
        var eventBus = new ProjectLiberty.EventBus.EventBus(logger);
        container.RegisterSingleton<IEventBus>(eventBus);

        // Register game host components
        var nativeBridge = new NativeBridge(logger, useScriptHook: !_useMockHost);
        container.RegisterSingleton<INativeBridge>(nativeBridge);

        var gameLoop = new GameLoop(logger);
        gameLoop.TickRate = configuration.GetValue("game.tickRate", 60);
        container.RegisterSingleton<IGameLoop>(gameLoop);

        var entityManager = new EntityManager(logger);
        container.RegisterSingleton<IEntityManager>(entityManager);

        var worldManager = new WorldManager(logger);
        container.RegisterSingleton<IWorldManager>(worldManager);

        var nativeInvoker = new NativeInvoker(logger, nativeBridge, useScriptHook: !_useMockHost);
        container.RegisterSingleton<INativeInvoker>(nativeInvoker);

        // Register game host
        IGameHost gameHost;
        if (_useMockHost)
        {
            gameHost = new MockGameHost(
                logger,
                nativeBridge,
                gameLoop,
                entityManager,
                worldManager,
                nativeInvoker);
        }
        else
        {
            gameHost = new ScriptHookHost(
                logger,
                nativeBridge,
                gameLoop,
                entityManager,
                worldManager,
                nativeInvoker);
        }
        container.RegisterSingleton<IGameHost>(gameHost);

        // Register plugin context
        var pluginContext = new PluginContext(
            logger,
            eventBus,
            localization,
            configuration,
            gameHost);
        container.RegisterSingleton<IPluginContext>(pluginContext);

        // Register plugin system
        var pluginsDirectory = Path.Combine(_baseDirectory, "Plugins");
        var pluginLoader = new PluginLoader(logger, pluginsDirectory);
        container.RegisterSingleton<PluginLoader>(pluginLoader);

        var pluginManager = new PluginManager(logger, pluginContext);
        container.RegisterSingleton<PluginManager>(pluginManager);

        // Register UI system
        var menuManager = new MenuManager();
        container.RegisterSingleton<IMenuManager>(menuManager);

        // Register application
        var application = new Application(container);
        container.RegisterSingleton<Application>(application);

        return container;
    }

    private ILogger CreateLogger(string categoryName)
    {
        var logDirectory = Path.Combine(_baseDirectory, "Logs");
        return new Logger(categoryName, logDirectory, logToFile: true, logToConsole: true);
    }
}
