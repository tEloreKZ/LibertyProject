using ProjectLiberty.Core;

namespace ProjectLiberty.EntryPoint;

/// <summary>
/// Main entry point for Project Liberty - loaded by ScriptHookVDotNet.
/// </summary>
public class Main
{
    private static Application? _application;
    private static ServiceBootstrap? _bootstrap;

    /// <summary>
    /// Called when the script is loaded by ScriptHookVDotNet.
    /// </summary>
    public static void OnScriptLoad()
    {
        try
        {
            var baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
            
            // Initialize bootstrap
            _bootstrap = new ServiceBootstrap(baseDirectory);
            
            // Configure services
            var container = _bootstrap.ConfigureServices();
            
            // Get application
            _application = container.GetService<Application>();
            
            // Initialize application
            _application.Initialize();
            
            // Run application
            _application.Run();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to initialize Project Liberty: {ex.Message}");
            Console.WriteLine(ex.StackTrace);
        }
    }

    /// <summary>
    /// Called when the script is unloaded by ScriptHookVDotNet.
    /// </summary>
    public static void OnScriptUnload()
    {
        try
        {
            _application?.Shutdown();
            _application?.Dispose();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to shutdown Project Liberty: {ex.Message}");
        }
    }

    /// <summary>
    /// Called every tick by ScriptHookVDotNet.
    /// </summary>
    public static void OnScriptTick()
    {
        // Tick is handled by the game loop
    }
}
