using System.Runtime.InteropServices;
using System.Text;

namespace ProjectLiberty.UI;

internal enum NativeResult
{
    Ok = 0,
    NotReady = 1,
    InvalidArgument = 2,
    NotFound = 3,
    BufferTooSmall = 4,
    InternalError = 5
}

[StructLayout(LayoutKind.Sequential)]
internal struct NativeBrowserOptions
{
    public uint StructSize;
    public IntPtr Url;
    public IntPtr Owner;
    public int Width;
    public int Height;
    public int X;
    public int Y;
    public int Fps;
    public int ZIndex;
    public byte Transparent;
    public byte Visible;
    public byte Focused;
    public byte Reserved;
}

[StructLayout(LayoutKind.Sequential)]
internal struct NativeBrowserEventInfo
{
    public uint StructSize;
    public ulong BrowserId;
    public uint EventNameBytes;
    public uint JsonBytes;
}

internal static class NativeMethods
{
    private const string Library = "ProjectLiberty.Core.asi";

    [DllImport(Library, CallingConvention = CallingConvention.Cdecl)]
    internal static extern uint Liberty_GetApiVersion();
    [DllImport(Library, CallingConvention = CallingConvention.Cdecl)]
    internal static extern int Liberty_IsReady();
    [DllImport(Library, CallingConvention = CallingConvention.Cdecl)]
    internal static extern NativeResult Liberty_CreateBrowser(ref NativeBrowserOptions options, out ulong browserId);
    [DllImport(Library, CallingConvention = CallingConvention.Cdecl)]
    internal static extern NativeResult Liberty_DestroyBrowser(ulong browserId);
    [DllImport(Library, CallingConvention = CallingConvention.Cdecl)]
    internal static extern NativeResult Liberty_DestroyBrowsersByOwner(IntPtr owner);
    [DllImport(Library, CallingConvention = CallingConvention.Cdecl)]
    internal static extern NativeResult Liberty_ShowBrowser(ulong browserId);
    [DllImport(Library, CallingConvention = CallingConvention.Cdecl)]
    internal static extern NativeResult Liberty_HideBrowser(ulong browserId);
    [DllImport(Library, CallingConvention = CallingConvention.Cdecl)]
    internal static extern NativeResult Liberty_FocusBrowser(ulong browserId);
    [DllImport(Library, CallingConvention = CallingConvention.Cdecl)]
    internal static extern NativeResult Liberty_UnfocusBrowser(ulong browserId);
    [DllImport(Library, CallingConvention = CallingConvention.Cdecl)]
    internal static extern NativeResult Liberty_LoadBrowserUrl(ulong browserId, IntPtr url);
    [DllImport(Library, CallingConvention = CallingConvention.Cdecl)]
    internal static extern NativeResult Liberty_SetBrowserBounds(ulong browserId, int x, int y, int width, int height);
    [DllImport(Library, CallingConvention = CallingConvention.Cdecl)]
    internal static extern NativeResult Liberty_SetBrowserZIndex(ulong browserId, int zIndex);
    [DllImport(Library, CallingConvention = CallingConvention.Cdecl)]
    internal static extern NativeResult Liberty_BrowserEmit(ulong browserId, IntPtr eventName, IntPtr json);
    [DllImport(Library, CallingConvention = CallingConvention.Cdecl)]
    internal static extern NativeResult Liberty_PeekBrowserEvent(ref NativeBrowserEventInfo info);
    [DllImport(Library, CallingConvention = CallingConvention.Cdecl)]
    internal static extern NativeResult Liberty_PollBrowserEvent(
        [Out] byte[] eventName, uint eventNameCapacity,
        [Out] byte[] json, uint jsonCapacity,
        ref NativeBrowserEventInfo info);
}

internal sealed class Utf8String : IDisposable
{
    public Utf8String(string value)
    {
        if (value == null) throw new ArgumentNullException(nameof(value));
        var bytes = Encoding.UTF8.GetBytes(value + "\0");
        Pointer = Marshal.AllocHGlobal(bytes.Length);
        Marshal.Copy(bytes, 0, Pointer, bytes.Length);
    }

    public IntPtr Pointer { get; private set; }

    public void Dispose()
    {
        if (Pointer == IntPtr.Zero) return;
        Marshal.FreeHGlobal(Pointer);
        Pointer = IntPtr.Zero;
    }
}

