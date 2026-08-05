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
    private readonly Dictionary<Type, Func<IServiceProvider, object>> _scoped = new Dictionary<Type, Func<IServiceProvider, object>>();
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
        _singletons[typeof(TService)] = instance;
        _logger.LogDebug($"Registered singleton: {typeof(TService).Name}");
    }

    /// <summary>
    /// Registers a singleton service with factory.
    /// </summary>
    /// <typeparam name="TService">Service type.</typeparam>
    /// <param name="factory">Service factory.</param>
    public void RegisterSingleton<TService>(Func<IServiceProvider, TService> factory) where TService : class
    {
        _singletons[typeof(TService)] = factory(this);
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
        _transients[typeof(TService)] = sp => Activator.CreateInstance(typeof(TImplementation))!;
        _logger.LogDebug($"Registered transient: {typeof(TService).Name} -> {typeof(TImplementation).Name}");
    }

    /// <summary>
    /// Registers a transient service with factory.
    /// </summary>
    /// <typeparam name="TService">Service type.</typeparam>
    /// <param name="factory">Service factory.</param>
    public void RegisterTransient<TService>(Func<IServiceProvider, TService> factory) where TService : class
    {
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
        if (_singletons.TryGetValue(serviceType, out var singleton))
        {
            return singleton;
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
        return _singletons.ContainsKey(serviceType) || _transients.ContainsKey(serviceType);
    }

    public void Dispose()
    {
        if (_disposed)
            return;

        foreach (var singleton in _singletons.Values)
        {
            if (singleton is IDisposable disposable)
            {
                disposable.Dispose();
            }
        }

        _singletons.Clear();
        _transients.Clear();
        _disposed = true;
    }
}
