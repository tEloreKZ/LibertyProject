using ProjectLiberty.Abstractions.GameHost;
using ProjectLiberty.Abstractions.Logging;

namespace ProjectLiberty.GameHost;

/// <summary>
/// Native invoker implementation.
/// </summary>
public class NativeInvoker : INativeInvoker
{
    private readonly ILogger _logger;
    private readonly INativeBridge _nativeBridge;

    public NativeInvoker(ILogger logger, INativeBridge nativeBridge)
    {
        _logger = logger;
        _nativeBridge = nativeBridge;
    }

    public object? Call(string nativeName, params object[] args)
    {
        var hash = _nativeBridge.GetScriptHash(nativeName);
        return CallHash(hash, args);
    }

    public object? CallHash(ulong hash, params object[] args)
    {
        // This would call the actual native function through ScriptHookVDotNet
        // For now, log the call and return null
        _logger.LogDebug($"Native call: hash={hash}, args={args.Length}");
        return null;
    }

    public T Call<T>(string nativeName, params object[] args)
    {
        var result = Call(nativeName, args);
        return result != null ? (T)result : default!;
    }

    public T CallHash<T>(ulong hash, params object[] args)
    {
        var result = CallHash(hash, args);
        return result != null ? (T)result : default!;
    }
}
