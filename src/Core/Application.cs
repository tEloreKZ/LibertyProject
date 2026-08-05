using ProjectLiberty.Abstractions.GameHost;
using ProjectLiberty.Abstractions.IPC;
using ProjectLiberty.Abstractions.Logging;
using ProjectLiberty.Core.DependencyInjection;

namespace ProjectLiberty.Core;

/// <summary>
/// Main application class - coordinates all systems.
/// </summary>
public class Application : IDisposable
{
    private readonly ServiceContainer _serviceContainer;
    private readonly ILogger _logger;
    private IGameHost? _gameHost;
    private IIPCService? _ipcService;
    private bool _disposed;

    public Application(ServiceContainer serviceContainer, ILogger logger)
    {
        _serviceContainer = serviceContainer;
        _logger = logger;
    }

    /// <summary>
    /// Initializes the application.
    /// </summary>
    public void Initialize()
    {
        _logger.LogInformation("Initializing Project Liberty...");

        // Get game host from container
        _gameHost = _serviceContainer.GetService<IGameHost>();
        
        // Initialize game host
        _gameHost.Initialize();

        // Get IPC service
        _ipcService = _serviceContainer.GetService<IIPCService>();
        
        // Start IPC server
        _ipcService?.Start();

        _logger.LogInformation("Project Liberty initialized successfully");
    }

    /// <summary>
    /// Runs the application.
    /// </summary>
    public void Run()
    {
        _logger.LogInformation("Starting Project Liberty...");

        if (_gameHost == null)
        {
            _logger.LogError("Game host not initialized");
            return;
        }

        // Start game loop
        _gameHost.GameLoop.Start();

        _logger.LogInformation("Project Liberty started");
    }

    /// <summary>
    /// Shuts down the application.
    /// </summary>
    public void Shutdown()
    {
        _logger.LogInformation("Shutting down Project Liberty...");

        // Stop IPC server
        _ipcService?.Stop();

        if (_gameHost != null)
        {
            // Stop game loop
            _gameHost.GameLoop.Stop();
            
            // Shutdown game host
            _gameHost.Shutdown();
        }

        _logger.LogInformation("Project Liberty shut down successfully");
    }

    public void Dispose()
    {
        if (_disposed)
            return;

        Shutdown();
        _serviceContainer.Dispose();
        _disposed = true;
    }
}
