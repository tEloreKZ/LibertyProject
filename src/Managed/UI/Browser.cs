using Newtonsoft.Json;

namespace ProjectLiberty.UI;

public sealed class Browser : IDisposable
{
    private readonly object _sync = new object();
    private readonly Dictionary<string, List<Action<string>>> _handlers =
        new Dictionary<string, List<Action<string>>>(StringComparer.Ordinal);
    private bool _disposed;

    private Browser(ulong id, string ownerId)
    {
        Id = id;
        OwnerId = ownerId;
        BrowserRuntime.Register(this);
    }

    public ulong Id { get; }
    public string OwnerId { get; }
    public static bool IsNativeCoreReady => BrowserRuntime.IsReady;

    public static Browser Create(string url, BrowserOptions? options = null)
    {
        if (string.IsNullOrWhiteSpace(url)) throw new ArgumentException("URL is required.", nameof(url));
        if (url.Length > 2048) throw new ArgumentException("URL cannot exceed 2048 characters.", nameof(url));
        if (!url.StartsWith("liberty://ui/", StringComparison.Ordinal) &&
            !url.StartsWith("liberty://mods/", StringComparison.Ordinal))
            throw new ArgumentException("Only liberty://ui and liberty://mods URLs are allowed.", nameof(url));
        options ??= new BrowserOptions();
        options.Validate();
        if (!BrowserRuntime.IsReady)
            throw new InvalidOperationException("ProjectLiberty.Core CEF service is not ready.");
        var ownerId = options.OwnerId ?? BrowserRuntime.DefaultOwnerId;
        using var nativeUrl = new Utf8String(url);
        using var nativeOwner = new Utf8String(ownerId);
        var nativeOptions = new NativeBrowserOptions
        {
            StructSize = (uint)System.Runtime.InteropServices.Marshal.SizeOf<NativeBrowserOptions>(),
            Url = nativeUrl.Pointer,
            Owner = nativeOwner.Pointer,
            Width = options.Width,
            Height = options.Height,
            X = options.X,
            Y = options.Y,
            Fps = options.Fps,
            ZIndex = options.ZIndex,
            Transparent = options.Transparent ? (byte)1 : (byte)0,
            Visible = options.Visible ? (byte)1 : (byte)0,
            Focused = options.Focused ? (byte)1 : (byte)0
        };
        BrowserRuntime.ThrowIfFailed("CreateBrowser",
            NativeMethods.Liberty_CreateBrowser(ref nativeOptions, out var id));
        return new Browser(id, ownerId);
    }

    public static int DispatchPendingEvents(int maximumEvents = 256) =>
        BrowserRuntime.DispatchPendingEvents(maximumEvents);

    public static void DestroyOwnedBrowsers(string? ownerId = null) =>
        BrowserRuntime.DestroyOwner(ownerId ?? BrowserRuntime.DefaultOwnerId);

    public void Show() => Invoke("ShowBrowser", () => NativeMethods.Liberty_ShowBrowser(Id));
    public void Hide() => Invoke("HideBrowser", () => NativeMethods.Liberty_HideBrowser(Id));
    public void Focus() => Invoke("FocusBrowser", () => NativeMethods.Liberty_FocusBrowser(Id));
    public void Unfocus() => Invoke("UnfocusBrowser", () => NativeMethods.Liberty_UnfocusBrowser(Id));

    public void LoadUrl(string url)
    {
        ThrowIfDisposed();
        if (string.IsNullOrWhiteSpace(url)) throw new ArgumentException("URL is required.", nameof(url));
        if (url.Length > 2048) throw new ArgumentException("URL cannot exceed 2048 characters.", nameof(url));
        if (!url.StartsWith("liberty://ui/", StringComparison.Ordinal) &&
            !url.StartsWith("liberty://mods/", StringComparison.Ordinal))
            throw new ArgumentException("Only liberty://ui and liberty://mods URLs are allowed.", nameof(url));
        using var value = new Utf8String(url);
        BrowserRuntime.ThrowIfFailed("LoadBrowserUrl", NativeMethods.Liberty_LoadBrowserUrl(Id, value.Pointer));
    }

    public void SetBounds(int x, int y, int width, int height)
    {
        ThrowIfDisposed();
        if (width <= 0 || height <= 0 || width > 8192 || height > 8192 ||
            (long)width * height > 16 * 1024 * 1024) throw new ArgumentOutOfRangeException(nameof(width));
        BrowserRuntime.ThrowIfFailed("SetBrowserBounds",
            NativeMethods.Liberty_SetBrowserBounds(Id, x, y, width, height));
    }

    public void SetZIndex(int zIndex) =>
        Invoke("SetBrowserZIndex", () => NativeMethods.Liberty_SetBrowserZIndex(Id, zIndex));

    public void Emit<T>(string eventName, T data)
    {
        ThrowIfDisposed();
        ValidateEventName(eventName);
        var json = JsonConvert.SerializeObject(data);
        if (System.Text.Encoding.UTF8.GetByteCount(json) > 1024 * 1024)
            throw new ArgumentException("Browser event payload cannot exceed 1 MiB.", nameof(data));
        using var name = new Utf8String(eventName);
        using var payload = new Utf8String(json);
        BrowserRuntime.ThrowIfFailed("BrowserEmit",
            NativeMethods.Liberty_BrowserEmit(Id, name.Pointer, payload.Pointer));
    }

    public void On<T>(string eventName, Action<T> callback)
    {
        ThrowIfDisposed();
        ValidateEventName(eventName);
        if (callback == null) throw new ArgumentNullException(nameof(callback));
        Action<string> adapter = json =>
        {
            var value = JsonConvert.DeserializeObject<T>(json);
            if (value != null) callback(value);
        };
        lock (_sync)
        {
            if (!_handlers.TryGetValue(eventName, out var handlers))
                _handlers[eventName] = handlers = new List<Action<string>>();
            handlers.Add(adapter);
        }
    }

    public void Off(string eventName)
    {
        ValidateEventName(eventName);
        lock (_sync) _handlers.Remove(eventName);
    }

    internal void Dispatch(string eventName, string json)
    {
        Action<string>[] handlers;
        lock (_sync)
            handlers = _handlers.TryGetValue(eventName, out var registered)
                ? registered.ToArray() : Array.Empty<Action<string>>();
        foreach (var handler in handlers) handler(json);
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        BrowserRuntime.Unregister(Id);
        lock (_sync) _handlers.Clear();
        if (BrowserRuntime.IsReady)
        {
            _ = NativeMethods.Liberty_DestroyBrowser(Id);
        }
        GC.SuppressFinalize(this);
    }

    internal void Invalidate()
    {
        _disposed = true;
        lock (_sync) _handlers.Clear();
    }

    private void Invoke(string operation, Func<NativeResult> call)
    {
        ThrowIfDisposed();
        BrowserRuntime.ThrowIfFailed(operation, call());
    }

    private void ThrowIfDisposed()
    {
        if (_disposed) throw new ObjectDisposedException(nameof(Browser));
    }

    private static void ValidateEventName(string eventName)
    {
        if (string.IsNullOrWhiteSpace(eventName) || eventName.Length > 128)
            throw new ArgumentException("Event name must contain 1-128 characters.", nameof(eventName));
    }
}
