using ProjectLiberty.Abstractions.GameHost;
using ProjectLiberty.Abstractions.Logging;

namespace ProjectLiberty.GameHost;

/// <summary>
/// Game loop advanced explicitly by the host on the game main thread.
/// </summary>
public sealed class GameLoop : IGameLoop, IDisposable
{
    private readonly ILogger _logger;
    private readonly object _sync = new object();
    private readonly List<Action<float>> _tickHandlers = new List<Action<float>>();
    private readonly List<Action<InputState>> _inputHandlers = new List<Action<InputState>>();
    private DateTime _lastTickUtc;
    private int _tickRate = 60;
    private bool _disposed;

    public bool IsRunning { get; private set; }
    public int TickRate
    {
        get => _tickRate;
        set
        {
            if (value <= 0) throw new ArgumentOutOfRangeException(nameof(value));
            _tickRate = value;
        }
    }
    public float DeltaTime { get; private set; }
    public float ElapsedTime { get; private set; }

    public GameLoop(ILogger logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public void Start()
    {
        ThrowIfDisposed();
        if (IsRunning) return;
        IsRunning = true;
        DeltaTime = 0;
        ElapsedTime = 0;
        _lastTickUtc = DateTime.UtcNow;
        _logger.LogInformation($"Game loop started at {_tickRate} ticks per second");
    }

    public void Stop()
    {
        if (!IsRunning) return;
        IsRunning = false;
        _logger.LogInformation("Game loop stopped");
    }

    public void Tick()
    {
        ThrowIfDisposed();
        if (!IsRunning) return;

        var now = DateTime.UtcNow;
        DeltaTime = Math.Max(0f, (float)(now - _lastTickUtc).TotalSeconds);
        _lastTickUtc = now;
        ElapsedTime += DeltaTime;

        Action<float>[] handlers;
        lock (_sync) handlers = _tickHandlers.ToArray();
        foreach (var handler in handlers)
        {
            try { handler(DeltaTime); }
            catch (Exception ex) { _logger.LogError(ex, "Error in tick handler"); }
        }
    }

    public void ProcessInput(InputState inputState)
    {
        ThrowIfDisposed();
        if (!IsRunning) return;

        Action<InputState>[] handlers;
        lock (_sync) handlers = _inputHandlers.ToArray();
        foreach (var handler in handlers)
        {
            try { handler(inputState); }
            catch (Exception ex) { _logger.LogError(ex, "Error in input handler"); }
        }
    }

    public void RegisterTickHandler(Action<float> handler)
    {
        if (handler == null) throw new ArgumentNullException(nameof(handler));
        ThrowIfDisposed();
        lock (_sync) _tickHandlers.Add(handler);
    }

    public void UnregisterTickHandler(Action<float> handler)
    {
        if (handler == null) throw new ArgumentNullException(nameof(handler));
        lock (_sync) _tickHandlers.Remove(handler);
    }

    public void RegisterInputHandler(Action<InputState> handler)
    {
        if (handler == null) throw new ArgumentNullException(nameof(handler));
        ThrowIfDisposed();
        lock (_sync) _inputHandlers.Add(handler);
    }

    public void UnregisterInputHandler(Action<InputState> handler)
    {
        if (handler == null) throw new ArgumentNullException(nameof(handler));
        lock (_sync) _inputHandlers.Remove(handler);
    }

    public void Dispose()
    {
        if (_disposed) return;
        Stop();
        lock (_sync)
        {
            _tickHandlers.Clear();
            _inputHandlers.Clear();
        }
        _disposed = true;
    }

    private void ThrowIfDisposed()
    {
        if (_disposed) throw new ObjectDisposedException(nameof(GameLoop));
    }
}
