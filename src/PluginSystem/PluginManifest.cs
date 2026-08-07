using ProjectLiberty.Abstractions.Plugins;

namespace ProjectLiberty.PluginSystem;

/// <summary>
/// Plugin manifest implementation.
/// </summary>
public class PluginManifest : IPluginManifest, ICoreVersionRequirement
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Version { get; set; } = "1.0.0";
    public string Author { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public IEnumerable<string> Dependencies { get; set; } = Enumerable.Empty<string>();
    public string MinimumCoreVersion { get; set; }

    public PluginManifest(string id, string name, string version, string author, string description, IEnumerable<string>? dependencies = null, string minimumCoreVersion = "1.0.0")
    {
        if (string.IsNullOrWhiteSpace(id)) throw new ArgumentException("Plugin ID cannot be empty.", nameof(id));
        if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Plugin name cannot be empty.", nameof(name));
        if (!System.Version.TryParse(version, out _)) throw new ArgumentException("Plugin version must be a valid version.", nameof(version));
        if (!System.Version.TryParse(minimumCoreVersion, out _)) throw new ArgumentException("Minimum core version must be valid.", nameof(minimumCoreVersion));
        Id = id.Trim();
        Name = name.Trim();
        Version = version;
        Author = author;
        Description = description;
        Dependencies = dependencies ?? Enumerable.Empty<string>();
        MinimumCoreVersion = minimumCoreVersion;
    }
}
