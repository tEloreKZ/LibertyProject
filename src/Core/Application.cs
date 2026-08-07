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
    private readonly PluginLoader _pluginLoader;
    private bool _disposed;
    private bool _initialized;
    private bool _running;

    public Application(IServiceContainer container)
    {
        _container = container;
        _logger = _container.GetService<ILogger>();
        _gameHost = _container.GetService<IGameHost>();
        _pluginManager = container.GetService<PluginManager>();
        _pluginLoader = container.GetService<PluginLoader>();
    }

    public void Initialize()
    {
        ThrowIfDisposed();
        if (_initialized)
            return;

        _logger.LogInformation("Initializing Project Liberty...");

        try
        {
            _gameHost.Initialize();
            var discoveredPlugins = _pluginLoader.DiscoverPlugins();
            _pluginManager.LoadPlugins(discoveredPlugins);
            _pluginManager.InitializePlugins();
            _initialized = true;
            _logger.LogInformation("Project Liberty initialized");
        }
        catch
        {
            _pluginManager.Dispose();
            _gameHost.Shutdown();
            throw;
        }
    }

    public void Run()
    {
        ThrowIfDisposed();
        if (!_initialized)
            throw new InvalidOperationException("Application not initialized");

        if (_running)
            return;

        _logger.LogInformation("Starting Project Liberty...");
        _pluginManager.EnablePlugins();
        _running = true;
    }

    public void Shutdown()
    {
        if (!_initialized)
            return;

        _logger.LogInformation("Shutting down Project Liberty...");

        _pluginManager.Dispose();
        _gameHost.Shutdown();

        _running = false;
        _initialized = false;
        _logger.LogInformation("Project Liberty shut down");
    }

    public void Dispose()
    {
        if (_disposed)
            return;

        Shutdown();

        _disposed = true;
    }

    public IServiceContainer GetContainer()
    {
        return _container;
    }

    private void ThrowIfDisposed()
    {
        if (_disposed) throw new ObjectDisposedException(nameof(Application));
    }
}
