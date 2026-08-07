using ProjectLiberty.GameHost;
using Xunit;

namespace ProjectLiberty.Tests;

public sealed class GameLoopTests
{
    [Fact]
    public void Tick_ExecutesHandlerOnCallingThread()
    {
        using var loop = new GameLoop(new TestLogger());
        var expectedThread = Environment.CurrentManagedThreadId;
        var actualThread = -1;
        loop.RegisterTickHandler(_ => actualThread = Environment.CurrentManagedThreadId);
        loop.Start();

        loop.Tick();

        Assert.Equal(expectedThread, actualThread);
    }
}
