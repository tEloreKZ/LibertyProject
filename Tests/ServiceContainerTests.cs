using ProjectLiberty.Core.DependencyInjection;
using Xunit;

namespace ProjectLiberty.Tests;

public sealed class ServiceContainerTests
{
    [Fact]
    public void SingletonFactory_IsLazyAndDisposedOnce()
    {
        var created = 0;
        var container = new ServiceContainer(new TestLogger());
        container.RegisterSingleton<DisposableService>(_ => { created++; return new DisposableService(); });
        Assert.Equal(0, created);

        var first = container.GetService<DisposableService>();
        var second = container.GetService<DisposableService>();
        container.Dispose();

        Assert.Same(first, second);
        Assert.Equal(1, created);
        Assert.Equal(1, first.DisposeCount);
    }

    private sealed class DisposableService : IDisposable
    {
        public int DisposeCount { get; private set; }
        public void Dispose() => DisposeCount++;
    }

    [Fact]
    public void Transient_ReceivesRegisteredConstructorDependencies()
    {
        using var container = new ServiceContainer(new TestLogger());
        var dependency = new Dependency();
        container.RegisterSingleton(dependency);
        container.RegisterTransient<ServiceWithDependency, ServiceWithDependency>();

        var service = container.GetService<ServiceWithDependency>();

        Assert.Same(dependency, service.Dependency);
    }

    private sealed class Dependency { }
    private sealed class ServiceWithDependency
    {
        public Dependency Dependency { get; }
        public ServiceWithDependency(Dependency dependency) => Dependency = dependency;
    }
}
