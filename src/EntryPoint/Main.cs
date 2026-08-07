using ProjectLiberty.Abstractions.Logging;
using ProjectLiberty.Core;

namespace ProjectLiberty.EntryPoint;

/// <summary>
/// Main entry point for Project Liberty - standalone entry point for testing.
/// </summary>
public class Program
{
    private static Application? _application;
    private static ServiceBootstrap? _bootstrap;
    private static ProjectLiberty.Core.DependencyInjection.ServiceContainer? _container;

    public static void Main(string[] args)
    {
        Console.WriteLine("Starting Project Liberty...");
        Console.WriteLine($"Base directory: {AppDomain.CurrentDomain.BaseDirectory}");
        
        try
        {
            var baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
            
            // Initialize bootstrap with mock host for standalone testing
            Console.WriteLine("Initializing bootstrap with MockGameHost...");
            _bootstrap = new ServiceBootstrap(baseDirectory, useMockHost: true);
            
            // Configure services
            Console.WriteLine("Configuring services...");
            _container = _bootstrap.ConfigureServices();
            
            // Get application
            Console.WriteLine("Getting application...");
            _application = _container.GetService<Application>();
            
            if (_application == null)
            {
                throw new Exception("Failed to get application from container");
            }
            
            // Initialize application
            Console.WriteLine("Initializing application...");
            _application.Initialize();
            
            // Run application
            Console.WriteLine("Running application...");
            _application.Run();
            
            Console.WriteLine("Project Liberty initialized successfully");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"ERROR: Failed to initialize Project Liberty");
            Console.WriteLine($"Message: {ex.Message}");
            Console.WriteLine($"Stack Trace: {ex.StackTrace}");
        }
        
        Console.WriteLine("Press any key to exit...");
        Console.ReadLine();
        
        Shutdown();
    }

    private static void Shutdown()
    {
        try
        {
            Console.WriteLine("Shutting down...");
            _application?.Shutdown();
            _application?.Dispose();
            _container?.Dispose();
            _container = null;
            _application = null;
            Console.WriteLine("Shutdown complete");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"ERROR: Failed to shutdown Project Liberty: {ex.Message}");
        }
    }
}
