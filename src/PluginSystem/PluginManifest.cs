using ProjectLiberty.Abstractions.Plugins;

namespace ProjectLiberty.PluginSystem;

/// <summary>
/// Plugin manifest implementation.
/// </summary>
public class PluginManifest : IPluginManifest
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Version { get; set; } = "1.0.0";
    public string Author { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public IEnumerable<string> Dependencies { get; set; } = Enumerable.Empty<string>();

    public PluginManifest(string id, string name, string version, string author, string description, IEnumerable<string>? dependencies = null)
    {
        Id = id;
        Name = name;
        Version = version;
        Author = author;
        Description = description;
        Dependencies = dependencies ?? Enumerable.Empty<string>();
    }
}
