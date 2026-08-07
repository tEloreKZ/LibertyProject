namespace ProjectLiberty.UI;

public sealed class BrowserOptions
{
    public int Width { get; set; } = 1920;
    public int Height { get; set; } = 1080;
    public int X { get; set; }
    public int Y { get; set; }
    public int Fps { get; set; } = 60;
    public bool Transparent { get; set; } = true;
    public bool Visible { get; set; } = true;
    public bool Focused { get; set; }
    public int ZIndex { get; set; }

    /// <summary>
    /// Stable owner used for bulk cleanup when a plugin or script unloads.
    /// Defaults to the current managed client instance.
    /// </summary>
    public string? OwnerId { get; set; }

    internal void Validate()
    {
        if (Width <= 0 || Width > 8192) throw new ArgumentOutOfRangeException(nameof(Width));
        if (Height <= 0 || Height > 8192) throw new ArgumentOutOfRangeException(nameof(Height));
        if ((long)Width * Height > 16 * 1024 * 1024)
            throw new ArgumentException("Browser surface cannot exceed 16 megapixels.");
        if (Fps < 1 || Fps > 120) throw new ArgumentOutOfRangeException(nameof(Fps));
        if (OwnerId != null && string.IsNullOrWhiteSpace(OwnerId))
            throw new ArgumentException("OwnerId cannot be empty.", nameof(OwnerId));
        if (OwnerId?.Length > 128)
            throw new ArgumentException("OwnerId cannot exceed 128 characters.", nameof(OwnerId));
    }
}
