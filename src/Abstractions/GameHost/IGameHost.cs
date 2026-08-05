namespace ProjectLiberty.Abstractions.GameHost;

/// <summary>
/// Main game host interface - provides access to game systems.
/// </summary>
public interface IGameHost
{
    /// <summary>
    /// Gets the native bridge for native game calls.
    /// </summary>
    INativeBridge NativeBridge { get; }
    
    /// <summary>
    /// Gets the game loop for tick management.
    /// </summary>
    IGameLoop GameLoop { get; }
    
    /// <summary>
    /// Gets the entity manager.
    /// </summary>
    IEntityManager EntityManager { get; }
    
    /// <summary>
    /// Gets the player manager.
    /// </summary>
    IPlayerManager PlayerManager { get; }
    
    /// <summary>
    /// Gets the vehicle manager.
    /// </summary>
    IVehicleManager VehicleManager { get; }
    
    /// <summary>
    /// Gets the ped manager.
    /// </summary>
    IPedManager PedManager { get; }
    
    /// <summary>
    /// Gets the world manager.
    /// </summary>
    IWorldManager WorldManager { get; }
    
    /// <summary>
    /// Gets the native invoker for native function calls.
    /// </summary>
    INativeInvoker NativeInvoker { get; }
    
    /// <summary>
    /// Initializes the game host.
    /// </summary>
    void Initialize();
    
    /// <summary>
    /// Shuts down the game host.
    /// </summary>
    void Shutdown();
}
