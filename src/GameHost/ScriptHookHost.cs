using ProjectLiberty.Abstractions.GameHost;
using ProjectLiberty.Abstractions.Logging;

namespace ProjectLiberty.GameHost;

/// <summary>
/// ScriptHookVDotNet host implementation.
/// </summary>
public class ScriptHookHost : IGameHost, IDisposable
{
    private readonly ILogger _logger;
    private readonly INativeBridge _nativeBridge;
    private readonly IGameLoop _gameLoop;
    private readonly IEntityManager _entityManager;
    private readonly IPlayerManager _playerManager;
    private readonly IVehicleManager _vehicleManager;
    private readonly IPedManager _pedManager;
    private readonly IWorldManager _worldManager;
    private readonly INativeInvoker _nativeInvoker;
    private bool _disposed;

    public INativeBridge NativeBridge => _nativeBridge;
    public IGameLoop GameLoop => _gameLoop;
    public IEntityManager EntityManager => _entityManager;
    public IPlayerManager PlayerManager => _playerManager;
    public IVehicleManager VehicleManager => _vehicleManager;
    public IPedManager PedManager => _pedManager;
    public IWorldManager WorldManager => _worldManager;
    public INativeInvoker NativeInvoker => _nativeInvoker;

    public ScriptHookHost(
        ILogger logger,
        INativeBridge nativeBridge,
        IGameLoop gameLoop,
        IEntityManager entityManager,
        IPlayerManager playerManager,
        IVehicleManager vehicleManager,
        IPedManager pedManager,
        IWorldManager worldManager,
        INativeInvoker nativeInvoker)
    {
        _logger = logger;
        _nativeBridge = nativeBridge;
        _gameLoop = gameLoop;
        _entityManager = entityManager;
        _playerManager = playerManager;
        _vehicleManager = vehicleManager;
        _pedManager = pedManager;
        _worldManager = worldManager;
        _nativeInvoker = nativeInvoker;
    }

    public void Initialize()
    {
        _logger.LogInformation("Initializing ScriptHookHost...");

        // Initialize native bridge
        _nativeBridge.Initialize();

        // Initialize game loop
        _gameLoop.Start();

        _logger.LogInformation($"ScriptHookHost initialized. Game version: {_nativeBridge.GetGameVersion()}");
    }

    public void Shutdown()
    {
        _logger.LogInformation("Shutting down ScriptHookHost...");

        // Stop game loop
        _gameLoop.Stop();

        // Shutdown native bridge
        _nativeBridge.Shutdown();

        _logger.LogInformation("ScriptHookHost shut down");
    }

    public void Dispose()
    {
        if (_disposed)
            return;

        Shutdown();
        _disposed = true;
    }
}
