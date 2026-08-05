using ProjectLiberty.Abstractions.Logging;
using ProjectLiberty.Abstractions.Plugins;

namespace ProjectLiberty.PluginSystem;

/// <summary>
/// Base class for plugins - provides common plugin functionality.
/// </summary>
public abstract class PluginBase : IPlugin
{
    protected IPluginContext? Context { get; private set; }
    protected ILogger? Logger => Context?.Logger;
    protected IEventBus? EventBus => Context?.EventBus;
    protected ILocalization? Localization => Context?.Localization;
    protected IConfiguration? Configuration => Context?.Configuration;
    protected IGameHost? GameHost => Context?.GameHost;

    public abstract IPluginManifest Manifest { get; }

    IPluginContext? IPlugin.Context => Context;

    public virtual bool Discover()
    {
        return true;
    }

    public virtual void Load()
    {
        Logger?.LogInformation($"Loading plugin: {Manifest.Name}");
    }

    public virtual void Initialize()
    {
        Logger?.LogInformation($"Initializing plugin: {Manifest.Name}");
    }

    public virtual void Enable()
    {
        Logger?.LogInformation($"Enabling plugin: {Manifest.Name}");
    }

    public virtual void Disable()
    {
        Logger?.LogInformation($"Disabling plugin: {Manifest.Name}");
    }

    public virtual void Unload()
    {
        Logger?.LogInformation($"Unloading plugin: {Manifest.Name}");
    }

    public void SetContext(IPluginContext context)
    {
        Context = context;
    }
}
