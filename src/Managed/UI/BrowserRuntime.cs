using System.Collections.Concurrent;
using System.Text;

namespace ProjectLiberty.UI;

internal static class BrowserRuntime
{
    private static readonly ConcurrentDictionary<ulong, WeakReference<Browser>> Browsers =
        new ConcurrentDictionary<ulong, WeakReference<Browser>>();

    internal static readonly string DefaultOwnerId =
        $"managed:{AppDomain.CurrentDomain.FriendlyName}:{Guid.NewGuid():N}";

    internal static bool IsReady
    {
        get
        {
            try { return NativeMethods.Liberty_GetApiVersion() == 1 && NativeMethods.Liberty_IsReady() != 0; }
            catch (DllNotFoundException) { return false; }
            catch (EntryPointNotFoundException) { return false; }
            catch (BadImageFormatException) { return false; }
        }
    }

    internal static void Register(Browser browser) =>
        Browsers[browser.Id] = new WeakReference<Browser>(browser);

    internal static void Unregister(ulong id) => Browsers.TryRemove(id, out _);

    internal static int DispatchPendingEvents(int maximumEvents)
    {
        if (maximumEvents <= 0) throw new ArgumentOutOfRangeException(nameof(maximumEvents));
        if (!IsReady) return 0;
        var dispatched = 0;
        while (dispatched < maximumEvents)
        {
            var info = new NativeBrowserEventInfo { StructSize = (uint)System.Runtime.InteropServices.Marshal.SizeOf<NativeBrowserEventInfo>() };
            var result = NativeMethods.Liberty_PeekBrowserEvent(ref info);
            if (result == NativeResult.NotFound) break;
            ThrowIfFailed("PeekBrowserEvent", result);
            if (info.EventNameBytes == 0 || info.JsonBytes == 0 ||
                info.EventNameBytes > 1024 || info.JsonBytes > 16 * 1024 * 1024)
                throw new InvalidOperationException("Native browser event has invalid buffer sizes.");
            var name = new byte[info.EventNameBytes];
            var json = new byte[info.JsonBytes];
            result = NativeMethods.Liberty_PollBrowserEvent(name, (uint)name.Length, json, (uint)json.Length, ref info);
            if (result == NativeResult.NotFound) continue;
            ThrowIfFailed("PollBrowserEvent", result);
            if (Browsers.TryGetValue(info.BrowserId, out var weak) && weak.TryGetTarget(out var browser))
                browser.Dispatch(Decode(name), Decode(json));
            else
                Browsers.TryRemove(info.BrowserId, out _);
            dispatched++;
        }
        return dispatched;
    }

    internal static void DestroyOwner(string ownerId)
    {
        if (!IsReady) return;
        using var owner = new Utf8String(ownerId);
        ThrowIfFailed("DestroyBrowsersByOwner", NativeMethods.Liberty_DestroyBrowsersByOwner(owner.Pointer));
        foreach (var pair in Browsers.ToArray())
        {
            if (pair.Value.TryGetTarget(out var browser) && browser.OwnerId == ownerId)
            {
                browser.Invalidate();
                Browsers.TryRemove(pair.Key, out _);
            }
        }
    }

    internal static void ThrowIfFailed(string operation, NativeResult result)
    {
        if (result != NativeResult.Ok) throw new BrowserNativeException(operation, result);
    }

    private static string Decode(byte[] bytes)
    {
        var length = Array.IndexOf(bytes, (byte)0);
        if (length < 0) length = bytes.Length;
        return Encoding.UTF8.GetString(bytes, 0, length);
    }
}
