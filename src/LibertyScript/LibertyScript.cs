using GTA;
using GTA.Native;
using GTA.UI;
using ProjectLiberty.Core;
using System.Diagnostics;

namespace ProjectLiberty.LibertyScript;

/// <summary>
/// ScriptHookVDotNet script that loads and manages Project Liberty.
/// </summary>
public class LibertyScript : Script
{
    private static Application? _application;
    private static ServiceBootstrap? _bootstrap;
    private static bool _initialized = false;

    public LibertyScript()
    {
        // ScriptHookVDotNet constructor
        this.Tick += OnTick;
        this.Aborted += OnAborted;
        
        // Initialize on script start
        Initialize();
    }

    private void Initialize()
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
            
            if (_application == null)
            {
                Notification.Show("~r~Failed to get application from container");
                return;
            }
            
            // Initialize application
            _application.Initialize();
            
            // Run application
            _application.Run();
            
            _initialized = true;
            
            Notification.Show("~g~Project Liberty~w~ initialized successfully");
            Debug.WriteLine("Project Liberty initialized successfully");
        }
        catch (Exception ex)
        {
            Notification.Show($"~r~Failed to initialize Project Liberty: {ex.Message}");
            Debug.WriteLine($"Initialize error: {ex}");
        }
    }

    private void OnAborted(object sender, EventArgs e)
    {
        try
        {
            if (_initialized)
            {
                _application?.Shutdown();
                _application?.Dispose();
                _initialized = false;
                Notification.Show("~y~Project Liberty~w~ shut down");
                Debug.WriteLine("Project Liberty shut down");
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"OnAborted error: {ex}");
        }
    }

    private void OnTick(object sender, EventArgs e)
    {
        // Tick logic for ScriptHookVDotNet
        // The game loop is handled by the GameLoop in Project Liberty
    }
}
