namespace ProjectLiberty.Abstractions.GameHost;

/// <summary>
/// Native invoker interface - invokes native game functions.
/// </summary>
public interface INativeInvoker
{
    /// <summary>
    /// Invokes a native function by name.
    /// </summary>
    /// <param name="nativeName">Native function name.</param>
    /// <param name="args">Function arguments.</param>
    /// <returns>Function result.</returns>
    object? Call(string nativeName, params object[] args);
    
    /// <summary>
    /// Invokes a native function by hash.
    /// </summary>
    /// <param name="hash">Native function hash.</param>
    /// <param name="args">Function arguments.</param>
    /// <returns>Function result.</returns>
    object? CallHash(ulong hash, params object[] args);
    
    /// <summary>
    /// Invokes a native function with typed result.
    /// </summary>
    /// <typeparam name="T">Result type.</typeparam>
    /// <param name="nativeName">Native function name.</param>
    /// <param name="args">Function arguments.</param>
    /// <returns>Function result.</returns>
    T Call<T>(string nativeName, params object[] args);
    
    /// <summary>
    /// Invokes a native function by hash with typed result.
    /// </summary>
    /// <typeparam name="T">Result type.</typeparam>
    /// <param name="hash">Native function hash.</param>
    /// <param name="args">Function arguments.</param>
    /// <returns>Function result.</returns>
    T CallHash<T>(ulong hash, params object[] args);
}
