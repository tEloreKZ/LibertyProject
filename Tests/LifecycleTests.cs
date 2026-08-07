using ProjectLiberty.Abstractions.GameHost;
using ProjectLiberty.Abstractions.Plugins;
using ProjectLiberty.Core;
using ProjectLiberty.PluginSystem;
using Xunit;

namespace ProjectLiberty.Tests;

public sealed class LifecycleTests : IDisposable
{
    private readonly string _directory = Path.Combine(Path.GetTempPath(), "ProjectLiberty.Tests", Guid.NewGuid().ToString("N"));

    [Fact]
    public void Application_ShutdownBeforeRun_ReleasesInitializedHost()
    {
        Directory.CreateDirectory(Path.Combine(_directory, "Config"));
        Directory.CreateDirectory(Path.Combine(_directory, "Languages"));
        using var container = new ServiceBootstrap(_directory, useMockHost: true).ConfigureServices();
        var application = container.GetService<Application>();
        var host = container.GetService<IGameHost>();

        application.Initialize();
        Assert.True(host.GameLoop.IsRunning);
        application.Shutdown();

        Assert.False(host.GameLoop.IsRunning);
    }

    [Fact]
    public void PluginManager_RunsLifecycleExactlyOnce()
    {
        Directory.CreateDirectory(Path.Combine(_directory, "Config"));
        Directory.CreateDirectory(Path.Combine(_directory, "Languages"));
        using var container = new ServiceBootstrap(_directory, useMockHost: true).ConfigureServices();
        var manager = container.GetService<PluginManager>();
        var plugin = new RecordingPlugin();

        manager.LoadPlugin(plugin);
        manager.InitializePlugins();
        manager.EnablePlugins();
        manager.Dispose();

        Assert.Equal(new[] { "load", "initialize", "enable", "disable", "unload" }, plugin.Calls);
    }

    public void Dispose()
    {
        if (Directory.Exists(_directory)) Directory.Delete(_directory, true);
    }

    private sealed class RecordingPlugin : IPlugin
    {
        public List<string> Calls { get; } = new List<string>();
        public IPluginManifest Manifest { get; } = new Manifest();
        public IPluginContext? Context { get; private set; }
        public bool Discover() => true;
        public void Load() => Calls.Add("load");
        public void Initialize() => Calls.Add("initialize");
        public void Enable() => Calls.Add("enable");
        public void Disable() => Calls.Add("disable");
        public void Unload() => Calls.Add("unload");
        public void SetContext(IPluginContext context) => Context = context;
    }

    private sealed class Manifest : IPluginManifest
    {
        public string Id => "recording";
        public string Name => "Recording";
        public string Version => "1.0.0";
        public string Author => "Tests";
        public string Description => "Lifecycle test plugin";
        public IEnumerable<string> Dependencies => Array.Empty<string>();
    }
}
