using ProjectLiberty.Abstractions.Logging;

namespace ProjectLiberty.Core.DependencyInjection;

/// <summary>
/// Service container interface for dependency injection.
/// </summary>
public interface IServiceContainer : IServiceProvider
{
    void RegisterSingleton<TService>(TService instance) where TService : class;
    void RegisterSingleton<TService>(Func<IServiceProvider, TService> factory) where TService : class;
    void RegisterTransient<TService, TImplementation>()
        where TService : class
        where TImplementation : class, TService;
    void RegisterTransient<TService>(Func<IServiceProvider, TService> factory) where TService : class;
    TService GetService<TService>() where TService : class;
    bool IsRegistered(Type serviceType);
}

/// <summary>
/// Service container for dependency injection.
/// </summary>
public class ServiceContainer : IServiceContainer, IDisposable
{
    private readonly Dictionary<Type, object> _singletons = new Dictionary<Type, object>();
    private readonly Dictionary<Type, Func<IServiceProvider, object>> _transients = new Dictionary<Type, Func<IServiceProvider, object>>();
    private readonly Dictionary<Type, Func<IServiceProvider, object>> _singletonFactories = new Dictionary<Type, Func<IServiceProvider, object>>();
    private readonly object _sync = new object();
    private readonly ILogger _logger;
    private bool _disposed;

    public ServiceContainer(ILogger logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Registers a singleton service.
    /// </summary>
    /// <typeparam name="TService">Service type.</typeparam>
    /// <param name="instance">Service instance.</param>
    public void RegisterSingleton<TService>(TService instance) where TService : class
    {
        if (instance == null) throw new ArgumentNullException(nameof(instance));
        ThrowIfDisposed();
        lock (_sync)
        {
            _singletons[typeof(TService)] = instance;
            _singletonFactories.Remove(typeof(TService));
        }
        _logger.LogDebug($"Registered singleton: {typeof(TService).Name}");
    }

    /// <summary>
    /// Registers a singleton service with factory.
    /// </summary>
    /// <typeparam name="TService">Service type.</typeparam>
    /// <param name="factory">Service factory.</param>
    public void RegisterSingleton<TService>(Func<IServiceProvider, TService> factory) where TService : class
    {
        if (factory == null) throw new ArgumentNullException(nameof(factory));
        ThrowIfDisposed();
        lock (_sync)
        {
            _singletonFactories[typeof(TService)] = sp => factory(sp);
            _singletons.Remove(typeof(TService));
        }
        _logger.LogDebug($"Registered singleton with factory: {typeof(TService).Name}");
    }

    /// <summary>
    /// Registers a transient service.
    /// </summary>
    /// <typeparam name="TService">Service type.</typeparam>
    /// <typeparam name="TImplementation">Implementation type.</typeparam>
    public void RegisterTransient<TService, TImplementation>()
        where TService : class
        where TImplementation : class, TService
    {
        ThrowIfDisposed();
        _transients[typeof(TService)] = _ => CreateInstance(typeof(TImplementation));
        _logger.LogDebug($"Registered transient: {typeof(TService).Name} -> {typeof(TImplementation).Name}");
    }

    /// <summary>
    /// Registers a transient service with factory.
    /// </summary>
    /// <typeparam name="TService">Service type.</typeparam>
    /// <param name="factory">Service factory.</param>
    public void RegisterTransient<TService>(Func<IServiceProvider, TService> factory) where TService : class
    {
        if (factory == null) throw new ArgumentNullException(nameof(factory));
        ThrowIfDisposed();
        _transients[typeof(TService)] = factory;
        _logger.LogDebug($"Registered transient with factory: {typeof(TService).Name}");
    }

    /// <summary>
    /// Gets a service.
    /// </summary>
    /// <typeparam name="TService">Service type.</typeparam>
    /// <returns>Service instance.</returns>
    public TService GetService<TService>() where TService : class
    {
        return (TService)GetService(typeof(TService));
    }

    /// <summary>
    /// Gets a service.
    /// </summary>
    /// <param name="serviceType">Service type.</param>
    /// <returns>Service instance.</returns>
    public object GetService(Type serviceType)
    {
        if (serviceType == null) throw new ArgumentNullException(nameof(serviceType));
        ThrowIfDisposed();
        lock (_sync)
        {
            if (_singletons.TryGetValue(serviceType, out var singleton))
                return singleton;

            if (_singletonFactories.TryGetValue(serviceType, out var singletonFactory))
            {
                singleton = singletonFactory(this);
                _singletons[serviceType] = singleton;
                _singletonFactories.Remove(serviceType);
                return singleton;
            }
        }

        if (_transients.TryGetValue(serviceType, out var transientFactory))
        {
            return transientFactory(this);
        }

        throw new InvalidOperationException($"Service not registered: {serviceType.Name}");
    }

    /// <summary>
    /// Checks if a service is registered.
    /// </summary>
    /// <param name="serviceType">Service type.</param>
    /// <returns>True if registered, false otherwise.</returns>
    public bool IsRegistered(Type serviceType)
    {
        lock (_sync)
            return _singletons.ContainsKey(serviceType) || _singletonFactories.ContainsKey(serviceType) || _transients.ContainsKey(serviceType);
    }

    public void Dispose()
    {
        if (_disposed)
            return;

        object[] singletons;
        lock (_sync) singletons = _singletons.Values.Distinct(ReferenceEqualityComparer.Instance).ToArray();
        foreach (var singleton in singletons.Reverse())
        {
            if (singleton is IDisposable disposable)
            {
                disposable.Dispose();
            }
        }

        _singletons.Clear();
        _singletonFactories.Clear();
        _transients.Clear();
        _disposed = true;
    }

    private void ThrowIfDisposed()
    {
        if (_disposed) throw new ObjectDisposedException(nameof(ServiceContainer));
    }

    private object CreateInstance(Type implementationType)
    {
        var constructors = implementationType.GetConstructors()
            .OrderByDescending(constructor => constructor.GetParameters().Length);
        foreach (var constructor in constructors)
        {
            var parameters = constructor.GetParameters();
            if (!parameters.All(parameter => IsRegistered(parameter.ParameterType))) continue;
            var arguments = parameters.Select(parameter => GetService(parameter.ParameterType)).ToArray();
            return constructor.Invoke(arguments);
        }
        throw new InvalidOperationException($"No resolvable public constructor found for {implementationType.Name}");
    }

    private sealed class ReferenceEqualityComparer : IEqualityComparer<object>
    {
        public static readonly ReferenceEqualityComparer Instance = new ReferenceEqualityComparer();
        public new bool Equals(object? x, object? y) => ReferenceEquals(x, y);
        public int GetHashCode(object obj) => System.Runtime.CompilerServices.RuntimeHelpers.GetHashCode(obj);
    }
}
