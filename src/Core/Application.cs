using ProjectLiberty.Abstractions.Configuration;
using ProjectLiberty.Abstractions.Events;
using ProjectLiberty.Abstractions.GameHost;
using ProjectLiberty.Abstractions.Localization;
using ProjectLiberty.Abstractions.Logging;
using ProjectLiberty.Abstractions.Plugins;
using ProjectLiberty.Core.DependencyInjection;
using ProjectLiberty.PluginSystem;

namespace ProjectLiberty.Core;

/// <summary>
/// Main application class - manages the lifecycle of Project Liberty.
/// </summary>
public class Application : IDisposable
{
    private readonly IServiceContainer _container;
    private readonly ILogger _logger;
    private readonly IGameHost _gameHost;
    private readonly PluginManager _pluginManager;
    private bool _disposed;
    private bool _initialized;
    private bool _running;

    public Application(IServiceContainer container)
    {
        _container = container;
        _logger = _container.GetService<ILogger>();
        _gameHost = _container.GetService<IGameHost>();
        _pluginManager = container.GetService<PluginManager>();
    }

    public void Initialize()
    {
        if (_initialized)
            return;

        _logger.LogInformation("Initializing Project Liberty...");

        // Initialize game host
        _gameHost.Initialize();

        // Initialize plugins
        _pluginManager.InitializePlugins();

        _initialized = true;
        _logger.LogInformation("Project Liberty initialized");
    }

    public void Run()
    {
        if (!_initialized)
            throw new InvalidOperationException("Application not initialized");

        if (_running)
            return;

        _logger.LogInformation("Starting Project Liberty...");
        _running = true;
    }

    public void Shutdown()
    {
        if (!_initialized || !_running)
            return;

        _logger.LogInformation("Shutting down Project Liberty...");

        // Shutdown game host
        _gameHost.Shutdown();

        // Dispose plugin manager
        _pluginManager?.Dispose();

        _running = false;
        _logger.LogInformation("Project Liberty shut down");
    }

    public void Dispose()
    {
        if (_disposed)
            return;

        Shutdown();

        _pluginManager?.Dispose();
        _disposed = true;
    }
}
