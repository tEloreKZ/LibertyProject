using ProjectLiberty.Abstractions.GameHost;
using ProjectLiberty.Abstractions.Logging;

namespace ProjectLiberty.GameHost;

/// <summary>
/// Game loop implementation.
/// </summary>
public class GameLoop : IGameLoop, IDisposable
{
    private readonly ILogger _logger;
    private readonly List<Action<float>> _tickHandlers = new List<Action<float>>();
    private readonly List<Action<InputState>> _inputHandlers = new List<Action<InputState>>();
    private readonly Timer _tickTimer;
    private int _tickRate = 60;
    private float _deltaTime;
    private float _elapsedTime;
    private bool _running;
    private bool _disposed;

    public bool IsRunning => _running;
    public int TickRate
    {
        get => _tickRate;
        set
        {
            _tickRate = value;
            UpdateTimerInterval();
        }
    }
    public float DeltaTime => _deltaTime;
    public float ElapsedTime => _elapsedTime;

    public GameLoop(ILogger logger)
    {
        _logger = logger;
        var interval = 1000 / _tickRate;
        _tickTimer = new Timer(OnTick, null, interval, interval);
    }

    public void Start()
    {
        if (_running)
        {
            return;
        }

        _logger.LogInformation("Starting game loop...");
        _running = true;
        _elapsedTime = 0;
        UpdateTimerInterval();
        _logger.LogInformation($"Game loop started at {_tickRate} ticks per second");
    }

    public void Stop()
    {
        if (!_running)
        {
            return;
        }

        _logger.LogInformation("Stopping game loop...");
        _running = false;
        _tickTimer.Change(Timeout.Infinite, Timeout.Infinite);
        _logger.LogInformation("Game loop stopped");
    }

    public void RegisterTickHandler(Action<float> handler)
    {
        _tickHandlers.Add(handler);
        _logger.LogDebug($"Registered tick handler. Total handlers: {_tickHandlers.Count}");
    }

    public void UnregisterTickHandler(Action<float> handler)
    {
        _tickHandlers.Remove(handler);
        _logger.LogDebug($"Unregistered tick handler. Total handlers: {_tickHandlers.Count}");
    }

    public void RegisterInputHandler(Action<InputState> handler)
    {
        _inputHandlers.Add(handler);
        _logger.LogDebug($"Registered input handler. Total handlers: {_inputHandlers.Count}");
    }

    public void UnregisterInputHandler(Action<InputState> handler)
    {
        _inputHandlers.Remove(handler);
        _logger.LogDebug($"Unregistered input handler. Total handlers: {_inputHandlers.Count}");
    }

    private void OnTick(object? state)
    {
        if (!_running)
        {
            return;
        }

        _deltaTime = 1f / _tickRate;
        _elapsedTime += _deltaTime;

        // Execute tick handlers
        foreach (var handler in _tickHandlers)
        {
            try
            {
                handler(_deltaTime);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in tick handler");
            }
        }
    }

    private void UpdateTimerInterval()
    {
        var interval = 1000 / _tickRate;
        _tickTimer.Change(interval, interval);
    }

    public void Dispose()
    {
        if (_disposed)
            return;

        Stop();
        _tickTimer.Dispose();
        _tickHandlers.Clear();
        _inputHandlers.Clear();
        _disposed = true;
    }
}
