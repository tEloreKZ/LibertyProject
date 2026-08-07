using ProjectLiberty.Abstractions.GameHost;
using ProjectLiberty.Abstractions.Logging;
using GTA.Native;

namespace ProjectLiberty.GameHost;

/// <summary>
/// Native invoker implementation.
/// </summary>
public class NativeInvoker : INativeInvoker
{
    private readonly ILogger _logger;
    private readonly INativeBridge _nativeBridge;
    private readonly bool _useScriptHook;

    public NativeInvoker(ILogger logger, INativeBridge nativeBridge, bool useScriptHook = true)
    {
        _logger = logger;
        _nativeBridge = nativeBridge;
        _useScriptHook = useScriptHook;
    }

    public object? Call(string nativeName, params object[] args)
    {
        var hash = _nativeBridge.GetScriptHash(nativeName);
        return CallHash(hash, args);
    }

    public object? CallHash(ulong hash, params object[] args)
    {
        EnsureAvailable();
        Function.Call((Hash)hash, ConvertArguments(args));
        return null;
    }

    public T Call<T>(string nativeName, params object[] args)
    {
        return CallHash<T>(_nativeBridge.GetScriptHash(nativeName), args);
    }

    public T CallHash<T>(ulong hash, params object[] args)
    {
        EnsureAvailable();
        return Function.Call<T>((Hash)hash, ConvertArguments(args));
    }

    private void EnsureAvailable()
    {
        if (!_useScriptHook)
            throw new InvalidOperationException("Native calls are unavailable when using the mock game host.");
        if (!_nativeBridge.IsInitialized)
            throw new InvalidOperationException("Native bridge is not initialized.");
    }

    private static InputArgument[] ConvertArguments(object[] args)
    {
        if (args == null || args.Length == 0) return Array.Empty<InputArgument>();
        return args.Select(argument => new InputArgument(argument)).ToArray();
    }
}
