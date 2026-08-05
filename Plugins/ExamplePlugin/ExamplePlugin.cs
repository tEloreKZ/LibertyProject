using ProjectLiberty.Abstractions.Events;
using ProjectLiberty.PluginSystem;

namespace ExamplePlugin;

/// <summary>
/// Example plugin for testing Project Liberty.
/// </summary>
public class ExamplePlugin : PluginBase
{
    public override IPluginManifest Manifest { get; } = new PluginManifest(
        "example_plugin",
        "Example Plugin",
        "1.0.0",
        "Project Liberty Team",
        "A simple example plugin for testing"
    );

    public override void Initialize()
    {
        base.Initialize();
        
        // Subscribe to events
        EventBus?.Subscribe<TestEvent>(new TestEventHandler(Logger));
        
        Logger?.LogInformation("Example plugin initialized successfully");
    }

    public override void Enable()
    {
        base.Enable();
        Logger?.LogInformation("Example plugin enabled");
    }

    public override void Disable()
    {
        base.Disable();
        Logger?.LogInformation("Example plugin disabled");
    }
}

/// <summary>
/// Test event handler.
/// </summary>
public class TestEventHandler : IEventHandler<TestEvent>
{
    private readonly ILogger? _logger;

    public TestEventHandler(ILogger? logger)
    {
        _logger = logger;
    }

    public Task HandleAsync(TestEvent @event)
    {
        _logger?.LogInformation($"Test event received: {@event.Message}");
        return Task.CompletedTask;
    }
}

/// <summary>
/// Test event.
/// </summary>
public class TestEvent : BaseEvent
{
    public string Message { get; set; } = string.Empty;
}
