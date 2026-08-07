using ProjectLiberty.Abstractions.Entities;
using ProjectLiberty.GameHost;
using Xunit;

namespace ProjectLiberty.Tests;

public sealed class GameHostTests
{
    [Fact]
    public void DeleteEntity_UpdatesExistingReference()
    {
        var manager = new EntityManager(new TestLogger());
        var entity = manager.CreateEntity(EntityType.Object, new Vector3());

        manager.DeleteEntity(entity.Handle);

        Assert.False(entity.Exists);
        Assert.False(manager.EntityExists(entity.Handle));
    }

    [Fact]
    public void NativeInvoker_RejectsCallsInMockMode()
    {
        var bridge = new NativeBridge(new TestLogger(), useScriptHook: false);
        bridge.Initialize();
        var invoker = new NativeInvoker(new TestLogger(), bridge, useScriptHook: false);

        Assert.Throws<InvalidOperationException>(() => invoker.CallHash(1UL));
    }
}
