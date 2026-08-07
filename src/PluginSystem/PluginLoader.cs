using System.Reflection;
using ProjectLiberty.Abstractions.Logging;
using ProjectLiberty.Abstractions.Plugins;

namespace ProjectLiberty.PluginSystem;

/// <summary>
/// Plugin loader - loads plugin assemblies.
/// </summary>
public class PluginLoader : IDisposable
{
    private readonly ILogger _logger;
    private readonly string _pluginsDirectory;
    private readonly string _coreDirectory;
    private bool _disposed;

    public PluginLoader(ILogger logger, string pluginsDirectory)
    {
        _logger = logger;
        _pluginsDirectory = pluginsDirectory;
        _coreDirectory = Path.Combine(Path.GetDirectoryName(pluginsDirectory) ?? "", "Core");

        // Setup assembly resolve to load dependencies from Core directory
        AppDomain.CurrentDomain.AssemblyResolve += OnAssemblyResolve;
    }

    private Assembly? OnAssemblyResolve(object? sender, ResolveEventArgs args)
    {
        try
        {
            var assemblyName = new AssemblyName(args.Name).Name;
            var coreDllPath = Path.Combine(_coreDirectory, $"{assemblyName}.dll");

            if (File.Exists(coreDllPath))
            {
                _logger.LogDebug($"Loading assembly from Core: {assemblyName}");
                return Assembly.LoadFrom(coreDllPath);
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning($"Failed to resolve assembly {args.Name}: {ex.Message}");
        }

        return null;
    }

    /// <summary>
    /// Discovers all plugins in the plugins directory.
    /// </summary>
    /// <returns>Discovered plugins.</returns>
    public IEnumerable<IPlugin> DiscoverPlugins()
    {
        if (_disposed) throw new ObjectDisposedException(nameof(PluginLoader));
        var plugins = new List<IPlugin>();

        if (!Directory.Exists(_pluginsDirectory))
        {
            _logger.LogWarning($"Plugins directory not found: {_pluginsDirectory}");
            return plugins;
        }

        // Look for plugin subdirectories
        var pluginDirectories = Directory.GetDirectories(_pluginsDirectory).OrderBy(path => path, StringComparer.OrdinalIgnoreCase);

        foreach (var pluginDir in pluginDirectories)
        {
            var pluginName = Path.GetFileName(pluginDir);
            var dllFile = Path.Combine(pluginDir, $"{pluginName}.dll");

            if (!File.Exists(dllFile))
            {
                _logger.LogWarning($"Plugin DLL not found: {dllFile}");
                continue;
            }

            try
            {
                var assembly = Assembly.LoadFrom(dllFile);
                var pluginTypes = GetLoadableTypes(assembly)
                    .Where(t => typeof(IPlugin).IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract);

                foreach (var pluginType in pluginTypes)
                {
                    try
                    {
                        if (pluginType.GetConstructor(Type.EmptyTypes) == null)
                            throw new InvalidOperationException($"Plugin type {pluginType.FullName} requires a public parameterless constructor.");
                        var plugin = (IPlugin)Activator.CreateInstance(pluginType)!;

                        if (plugin.Discover())
                        {
                            plugins.Add(plugin);
                            _logger.LogInformation($"Discovered plugin: {plugin.Manifest.Name} v{plugin.Manifest.Version}");
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, $"Failed to instantiate plugin: {pluginType.Name}");
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to load assembly: {dllFile}");
            }
        }

        return plugins;
    }

    private IEnumerable<Type> GetLoadableTypes(Assembly assembly)
    {
        try { return assembly.GetTypes(); }
        catch (ReflectionTypeLoadException ex)
        {
            foreach (var loaderException in ex.LoaderExceptions.Where(item => item != null))
                _logger.LogWarning($"Plugin type load warning: {loaderException!.Message}");
            return ex.Types.Where(type => type != null).Cast<Type>();
        }
    }

    public void Dispose()
    {
        if (_disposed) return;
        AppDomain.CurrentDomain.AssemblyResolve -= OnAssemblyResolve;
        _disposed = true;
    }
}
