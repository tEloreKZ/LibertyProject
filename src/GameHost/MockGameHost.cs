using ProjectLiberty.Abstractions.GameHost;
using ProjectLiberty.Abstractions.Logging;

namespace ProjectLiberty.GameHost;

/// <summary>
/// Mock game host for standalone testing - simulates game environment without GTA V.
/// </summary>
public class MockGameHost : IGameHost
{
    private readonly ILogger _logger;
    private readonly INativeBridge _nativeBridge;
    private readonly IGameLoop _gameLoop;
    private readonly IEntityManager _entityManager;
    private readonly IWorldManager _worldManager;
    private readonly INativeInvoker _nativeInvoker;
    private bool _initialized;

    public MockGameHost(
        ILogger logger,
        INativeBridge nativeBridge,
        IGameLoop gameLoop,
        IEntityManager entityManager,
        IWorldManager worldManager,
        INativeInvoker nativeInvoker)
    {
        _logger = logger;
        _nativeBridge = nativeBridge;
        _gameLoop = gameLoop;
        _entityManager = entityManager;
        _worldManager = worldManager;
        _nativeInvoker = nativeInvoker;
    }

    public string GameVersion => "Mock-1.0.0.0";
    public bool IsRunning => _initialized;

    public INativeBridge NativeBridge => _nativeBridge;
    public IGameLoop GameLoop => _gameLoop;
    public IEntityManager EntityManager => _entityManager;
    public IWorldManager WorldManager => _worldManager;
    public INativeInvoker NativeInvoker => _nativeInvoker;

    public void Initialize()
    {
        if (_initialized)
            return;

        _logger.LogInformation("Initializing MockGameHost...");
        _logger.LogInformation("MockGameHost initialized successfully");
        _initialized = true;
    }

    public void Shutdown()
    {
        if (!_initialized)
            return;

        _logger.LogInformation("Shutting down MockGameHost...");
        _initialized = false;
        _logger.LogInformation("MockGameHost shut down");
    }
}
