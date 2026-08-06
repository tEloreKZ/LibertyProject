using System.Diagnostics;
using GTA;
using GTA.Native;
using GTA.UI;
using ProjectLiberty.Abstractions.UI;
using ProjectLiberty.Core;
using ProjectLiberty.Core.DependencyInjection;
using ProjectLiberty.UI;

namespace ProjectLiberty.LibertyScript;

/// <summary>
/// Main script for Project Liberty.
/// </summary>
public class LibertyScript : Script
{
    private ServiceBootstrap? _bootstrap;
    private Application? _application;
    private bool _initialized = false;

    public LibertyScript()
    {
        Tick += OnTick;
        Aborted += OnAborted;

        // Initialize on script start
        Initialize();
    }

    private void Initialize()
    {
        try
        {
            var baseDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ProjectLiberty");

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

            // Create test menu
            CreateTestMenu(container);

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

    private void CreateTestMenu(ServiceContainer container)
    {
        var menuManager = container.GetService<IMenuManager>();
        if (menuManager == null) return;

        var mainMenu = new Menu("liberty_main", "Project Liberty");

        mainMenu.AddItem(new MenuItem("go_on_duty", "Go On Duty", "Start your shift as an officer")
        {
            OnActivated = () =>
            {
                Notification.Show("~g~Going on duty...");
            }
        });

        mainMenu.AddItem(new MenuItem("go_off_duty", "Go Off Duty", "End your shift")
        {
            OnActivated = () =>
            {
                Notification.Show("~y~Going off duty...");
            }
        });

        mainMenu.AddItem(new MenuItem("call_backup", "Call Backup", "Request backup units")
        {
            OnActivated = () =>
            {
                Notification.Show("~b~Backup requested!");
            }
        });

        menuManager.RegisterMenu(mainMenu);
    }

    private void OnTick(object sender, EventArgs e)
    {
        if (!_initialized || _application == null)
            return;

        try
        {
            // Update menu manager
            var menuManager = _application.GetContainer().GetService<IMenuManager>();
            menuManager?.Update();

            // Toggle menu with F2 key
            if (Game.IsKeyDown(System.Windows.Forms.Keys.F2))
            {
                Debug.WriteLine("F2 pressed");

                var menuManager2 = _application.GetContainer().GetService<IMenuManager>();
                if (menuManager2 == null)
                {
                    Debug.WriteLine("MenuManager is null");
                    return;
                }

                var mainMenu = menuManager2.GetMenu("liberty_main");
                if (mainMenu == null)
                {
                    Debug.WriteLine("Main menu is null");
                    return;
                }

                Debug.WriteLine($"Menu visibility before: {mainMenu.IsVisible}");

                if (mainMenu.IsVisible)
                {
                    menuManager2.HideMenu("liberty_main");
                    Debug.WriteLine("Menu hidden");
                }
                else
                {
                    menuManager2.ShowMenu("liberty_main");
                    Debug.WriteLine("Menu shown");
                }

                Debug.WriteLine($"Menu visibility after: {mainMenu.IsVisible}");
            }
        }
        catch (Exception ex)
        {
            Notification.Show($"~r~Menu error: {ex.Message}");
            Debug.WriteLine($"Menu error: {ex}");
        }
    }

    private void OnAborted(object sender, EventArgs e)
    {
        _application?.Shutdown();
    }
}
