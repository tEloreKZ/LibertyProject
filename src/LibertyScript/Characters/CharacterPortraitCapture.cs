using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace ProjectLiberty.LibertyScript.Characters;

internal static class CharacterPortraitCapture
{
    [DllImport("user32.dll")]
    private static extern bool GetClientRect(IntPtr window, out NativeRect rectangle);

    [DllImport("user32.dll")]
    private static extern bool ClientToScreen(IntPtr window, ref NativePoint point);

    public static bool Capture(string outputPath)
    {
        var window = System.Diagnostics.Process.GetCurrentProcess().MainWindowHandle;
        if (window == IntPtr.Zero || !GetClientRect(window, out var client)) return false;
        var origin = new NativePoint();
        if (!ClientToScreen(window, ref origin)) return false;

        var width = client.Right - client.Left;
        var height = client.Bottom - client.Top;
        if (width < 640 || height < 480) return false;

        using var frame = new Bitmap(width, height, PixelFormat.Format24bppRgb);
        using (var graphics = Graphics.FromImage(frame))
            graphics.CopyFromScreen(origin.X, origin.Y, 0, 0, frame.Size, CopyPixelOperation.SourceCopy);

        var cropWidth = Math.Min(width, (int)(height * .63f));
        var cropHeight = Math.Min(height, (int)(height * .82f));
        var crop = new Rectangle((width - cropWidth) / 2, Math.Max(0, (int)(height * .08f)), cropWidth, cropHeight);
        using var portrait = new Bitmap(420, 560, PixelFormat.Format24bppRgb);
        using (var graphics = Graphics.FromImage(portrait))
        {
            graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
            graphics.SmoothingMode = SmoothingMode.HighQuality;
            graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;
            graphics.DrawImage(frame, new Rectangle(0, 0, portrait.Width, portrait.Height), crop, GraphicsUnit.Pixel);
        }

        var directory = Path.GetDirectoryName(outputPath);
        if (!string.IsNullOrEmpty(directory)) Directory.CreateDirectory(directory);
        portrait.Save(outputPath, ImageFormat.Jpeg);
        return true;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct NativePoint { public int X; public int Y; }

    [StructLayout(LayoutKind.Sequential)]
    private struct NativeRect { public int Left; public int Top; public int Right; public int Bottom; }
}
