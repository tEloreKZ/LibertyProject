using System;
using System.Collections.Generic;
using System.Linq;
using ProjectLiberty.Abstractions.Plugins;
using ProjectLiberty.Abstractions.Logging;

namespace TestPlugin;

/// <summary>
/// Test plugin to verify Plugin System integration.
/// </summary>
public class TestPlugin : IPlugin
{
    private IPluginContext? _context;
    private ILogger? _logger;

    public IPluginManifest Manifest { get; } = new TestPluginManifest();
    public IPluginContext? Context => _context;

    public bool Discover()
    {
        return true;
    }

    public void Load()
    {
        _logger?.LogInformation("TestPlugin loaded successfully!");
    }

    public void Initialize()
    {
        if (_context == null || _logger == null) return;

        _logger.LogInformation($"GameHost type: {_context.GameHost.GetType().Name}");
        _logger.LogInformation($"NativeBridge type: {_context.GameHost.NativeBridge.GetType().Name}");
        _logger.LogInformation($"GameLoop type: {_context.GameHost.GameLoop.GetType().Name}");

        _logger.LogInformation("TestPlugin initialized successfully!");
    }

    public void Enable()
    {
        _logger?.LogInformation("TestPlugin enabled!");
    }

    public void Disable()
    {
        _logger?.LogInformation("TestPlugin disabled!");
    }

    public void Unload()
    {
        _logger?.LogInformation("TestPlugin unloaded successfully!");
    }

    public void SetContext(IPluginContext context)
    {
        _context = context;
        _logger = context.Logger;
    }

    private class TestPluginManifest : IPluginManifest
    {
        public string Id => "test-plugin";
        public string Name => "TestPlugin";
        public string Version => "1.0.0";
        public string Author => "ProjectLiberty";
        public string Description => "Test plugin to verify Plugin System integration";
        public IEnumerable<string> Dependencies => Enumerable.Empty<string>();
    }
}
