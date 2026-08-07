using ProjectLiberty.Abstractions.Events;
using ProjectLiberty.EventBus;
using Xunit;

namespace ProjectLiberty.Tests;

public sealed class EventBusTests
{
    [Fact]
    public async Task PublishAsync_PreservesPriorityAndSequence()
    {
        var calls = new List<string>();
        using var bus = new ProjectLiberty.EventBus.EventBus(new TestLogger());
        bus.Subscribe(new Handler("normal", calls), EventPriority.Normal);
        bus.Subscribe(new Handler("high", calls), EventPriority.High);

        await bus.PublishAsync(new TestEvent());

        Assert.Equal(new[] { "high", "normal" }, calls);
    }

    [Fact]
    public void Publish_UsesSnapshotWhenHandlerUnsubscribes()
    {
        using var bus = new ProjectLiberty.EventBus.EventBus(new TestLogger());
        var calls = new List<string>();
        var second = new Handler("second", calls);
        bus.Subscribe(new Handler("first", calls, () => bus.Unsubscribe(second)), EventPriority.High);
        bus.Subscribe(second);

        bus.Publish(new TestEvent());

        Assert.Equal(new[] { "first", "second" }, calls);
    }

    private sealed class TestEvent : BaseEvent { }
    private sealed class Handler : IEventHandler<TestEvent>
    {
        private readonly string _name;
        private readonly IList<string> _calls;
        private readonly Action? _after;
        public Handler(string name, IList<string> calls, Action? after = null) { _name = name; _calls = calls; _after = after; }
        public Task HandleAsync(TestEvent @event) { _calls.Add(_name); _after?.Invoke(); return Task.CompletedTask; }
    }
}
