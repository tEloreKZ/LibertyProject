namespace ProjectLiberty.Abstractions.GameHost;

/// <summary>
/// Game loop interface - manages game tick and input.
/// </summary>
public interface IGameLoop
{
    /// <summary>
    /// Gets a value indicating whether the game loop is running.
    /// </summary>
    bool IsRunning { get; }
    
    /// <summary>
    /// Gets the current tick rate in ticks per second.
    /// </summary>
    int TickRate { get; }
    
    /// <summary>
    /// Gets the delta time since last tick in seconds.
    /// </summary>
    float DeltaTime { get; }
    
    /// <summary>
    /// Gets the total elapsed time since start in seconds.
    /// </summary>
    float ElapsedTime { get; }
    
    /// <summary>
    /// Starts the game loop.
    /// </summary>
    void Start();
    
    /// <summary>
    /// Stops the game loop.
    /// </summary>
    void Stop();
    
    /// <summary>
    /// Registers a tick handler.
    /// </summary>
    /// <param name="handler">Tick handler to register.</param>
    void RegisterTickHandler(Action<float> handler);
    
    /// <summary>
    /// Unregisters a tick handler.
    /// </summary>
    /// <param name="handler">Tick handler to unregister.</param>
    void UnregisterTickHandler(Action<float> handler);
    
    /// <summary>
    /// Registers an input handler.
    /// </summary>
    /// <param name="handler">Input handler to register.</param>
    void RegisterInputHandler(Action<InputState> handler);
    
    /// <summary>
    /// Unregisters an input handler.
    /// </summary>
    /// <param name="handler">Input handler to unregister.</param>
    void UnregisterInputHandler(Action<InputState> handler);
}

/// <summary>
/// Input state structure.
/// </summary>
public struct InputState
{
    /// <summary>
    /// Gets or sets whether the key is pressed.
    /// </summary>
    public bool IsPressed { get; set; }
    
    /// <summary>
    /// Gets or sets the key code.
    /// </summary>
    public int KeyCode { get; set; }
    
    /// <summary>
    /// Gets or sets the modifier keys.
    /// </summary>
    public ModifierKeys Modifiers { get; set; }
}

/// <summary>
/// Modifier keys flags.
/// </summary>
[Flags]
public enum ModifierKeys
{
    None = 0,
    Shift = 1,
    Control = 2,
    Alt = 4
}
