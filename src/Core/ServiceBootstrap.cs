using ProjectLiberty.Abstractions.Configuration;
using ProjectLiberty.Abstractions.Events;
using ProjectLiberty.Abstractions.GameHost;
using ProjectLiberty.Abstractions.Localization;
using ProjectLiberty.Abstractions.Logging;
using ProjectLiberty.Abstractions.Plugins;
using ProjectLiberty.Configuration;
using ProjectLiberty.Core.DependencyInjection;
using ProjectLiberty.EventBus;
using ProjectLiberty.GameHost;
using ProjectLiberty.Localization;
using ProjectLiberty.Logging;
using ProjectLiberty.PluginSystem;

namespace ProjectLiberty.Core;

/// <summary>
/// Service bootstrap - configures dependency injection.
/// </summary>
public class ServiceBootstrap
{
    private readonly string _baseDirectory;

    public ServiceBootstrap(string baseDirectory)
    {
        _baseDirectory = baseDirectory;
    }

    /// <summary>
    /// Configures all services in the service container.
    /// </summary>
    /// <returns>Configured service container.</returns>
    public ServiceContainer ConfigureServices()
    {
        var container = new ServiceContainer(CreateLogger("ServiceContainer"));

        // Register logging first
        var logger = CreateLogger("ProjectLiberty");
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
        container.RegisterSingleton<ILocalization>(localization);

        // Register event bus
        var eventBus = new ProjectLiberty.EventBus.EventBus(logger);
        container.RegisterSingleton<IEventBus>(eventBus);

        // Register game host components
        var nativeBridge = new NativeBridge(logger);
        container.RegisterSingleton<INativeBridge>(nativeBridge);

        var gameLoop = new GameLoop(logger);
        container.RegisterSingleton<IGameLoop>(gameLoop);

        var entityManager = new EntityManager(logger);
        container.RegisterSingleton<IEntityManager>(entityManager);

        var playerManager = new PlayerManager(logger, entityManager);
        container.RegisterSingleton<IPlayerManager>(playerManager);

        var vehicleManager = new VehicleManager(logger, entityManager);
        container.RegisterSingleton<IVehicleManager>(vehicleManager);

        var pedManager = new PedManager(logger, entityManager);
        container.RegisterSingleton<IPedManager>(pedManager);

        var worldManager = new WorldManager(logger);
        container.RegisterSingleton<IWorldManager>(worldManager);

        var nativeInvoker = new NativeInvoker(logger, nativeBridge);
        container.RegisterSingleton<INativeInvoker>(nativeInvoker);

        // Register game host
        var gameHost = new ScriptHookHost(
            logger,
            nativeBridge,
            gameLoop,
            entityManager,
            playerManager,
            vehicleManager,
            pedManager,
            worldManager,
            nativeInvoker);
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

        // Register application
        var application = new Application(container, logger);
        container.RegisterSingleton<Application>(application);

        return container;
    }

    private ILogger CreateLogger(string categoryName)
    {
        var logDirectory = Path.Combine(_baseDirectory, "Logs");
        return new Logger(categoryName, logDirectory, logToFile: true, logToConsole: true);
    }
}
