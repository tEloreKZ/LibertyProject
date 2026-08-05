namespace ProjectLiberty.Abstractions.GameHost;

/// <summary>
/// Native bridge interface - provides abstraction for native game calls.
/// </summary>
public interface INativeBridge
{
    /// <summary>
    /// Gets a value indicating whether the native bridge is initialized.
    /// </summary>
    bool IsInitialized { get; }
    
    /// <summary>
    /// Initializes the native bridge.
    /// </summary>
    void Initialize();
    
    /// <summary>
    /// Shuts down the native bridge.
    /// </summary>
    void Shutdown();
    
    /// <summary>
    /// Gets the game version.
    /// </summary>
    /// <returns>Game version string.</returns>
    string GetGameVersion();
    
    /// <summary>
    /// Gets the script hash for a native function name.
    /// </summary>
    /// <param name="nativeName">Native function name.</param>
    /// <returns>Script hash.</returns>
    ulong GetScriptHash(string nativeName);
}
