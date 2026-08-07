using System.Diagnostics;
using GTA;
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
    private ServiceContainer? _container;
    private bool _initialized = false;

    public LibertyScript()
    {
        Tick += OnTick;
        KeyDown += OnKeyDown;
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
            _container = _bootstrap.ConfigureServices();

            // Get application
            _application = _container.GetService<Application>();

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
            CreateTestMenu(_container);

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
            _application.GetContainer().GetService<ProjectLiberty.Abstractions.GameHost.IGameLoop>().Tick();
            var menuManager = _application.GetContainer().GetService<IMenuManager>();
            menuManager?.Update();
        }
        catch (Exception ex)
        {
            Notification.Show($"~r~Menu error: {ex.Message}");
            Debug.WriteLine($"Menu error: {ex}");
        }
    }

    private void OnKeyDown(object sender, System.Windows.Forms.KeyEventArgs e)
    {
        if (!_initialized || _application == null) return;

        var modifiers = ProjectLiberty.Abstractions.GameHost.ModifierKeys.None;
        if (e.Shift) modifiers |= ProjectLiberty.Abstractions.GameHost.ModifierKeys.Shift;
        if (e.Control) modifiers |= ProjectLiberty.Abstractions.GameHost.ModifierKeys.Control;
        if (e.Alt) modifiers |= ProjectLiberty.Abstractions.GameHost.ModifierKeys.Alt;
        _application.GetContainer().GetService<ProjectLiberty.Abstractions.GameHost.IGameLoop>().ProcessInput(
            new ProjectLiberty.Abstractions.GameHost.InputState { IsPressed = true, KeyCode = (int)e.KeyCode, Modifiers = modifiers });

        if (e.KeyCode != System.Windows.Forms.Keys.F2) return;
        var menuManager = _application.GetContainer().GetService<IMenuManager>();
        var mainMenu = menuManager.GetMenu("liberty_main");
        if (mainMenu == null) return;
        if (mainMenu.IsVisible) menuManager.HideMenu(mainMenu.Id);
        else menuManager.ShowMenu(mainMenu.Id);
    }

    private void OnAborted(object sender, EventArgs e)
    {
        try { _application?.Shutdown(); }
        finally
        {
            _container?.Dispose();
            _container = null;
            _application = null;
            _initialized = false;
        }
    }
}
