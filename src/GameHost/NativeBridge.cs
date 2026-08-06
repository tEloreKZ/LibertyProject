using ProjectLiberty.Abstractions.GameHost;
using ProjectLiberty.Abstractions.Logging;

namespace ProjectLiberty.GameHost;

/// <summary>
/// Native bridge implementation for ScriptHookVDotNet.
/// </summary>
public class NativeBridge : INativeBridge
{
    private readonly ILogger _logger;
    private readonly NativeHashCache _hashCache;
    private readonly bool _useScriptHook;
    private bool _initialized;

    public NativeBridge(ILogger logger, bool useScriptHook = true)
    {
        _logger = logger;
        _useScriptHook = useScriptHook;
        _hashCache = new NativeHashCache(logger, useScriptHook);
    }

    public bool IsInitialized => _initialized;

    public void Initialize()
    {
        if (_initialized)
            return;

        _logger.LogInformation("Initializing NativeBridge...");
        _initialized = true;
        _logger.LogInformation("NativeBridge initialized");
    }

    public void Shutdown()
    {
        if (!_initialized)
            return;

        _logger.LogInformation("Shutting down NativeBridge...");
        _hashCache.Clear();
        _initialized = false;
        _logger.LogInformation("NativeBridge shut down");
    }

    public string GetGameVersion()
    {
        return "1.0.0.0";
    }

    public ulong GetScriptHash(string nativeName)
    {
        return _hashCache.GetHash(nativeName);
    }
}
