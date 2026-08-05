using GTA;
using GTA.Native;
using GTA.UI;
using ProjectLiberty.LibertyScript.IPC;
using System.Diagnostics;

namespace ProjectLiberty.LibertyScript;

/// <summary>
/// ScriptHookVDotNet script that loads and manages Project Liberty.
/// </summary>
public class LibertyScript : Script
{
    private static bool _initialized = false;
    private static NamedPipeClient? _ipcClient;
    private static DateTime _lastPositionUpdate = DateTime.MinValue;
    private static readonly TimeSpan _positionUpdateInterval = TimeSpan.FromMilliseconds(500); // Update every 500ms

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
            // Try to connect to .NET 8.0 system via IPC
            _ipcClient = new NamedPipeClient();
            
            if (_ipcClient.Connect())
            {
                Notification.Show("~g~Project Liberty~w~ connected to core system");
                Screen.ShowSubtitle("Project Liberty v1.0 - Connected", 5000);
                
                _ipcClient.MessageReceived += OnIPCMessageReceived;
                _initialized = true;
            }
            else
            {
                Notification.Show("~y~Project Liberty~w~ - Core system not running");
                Screen.ShowSubtitle("Project Liberty v1.0 - Standalone Mode", 5000);
                Screen.ShowSubtitle("Start ProjectLiberty.EntryPoint.exe first", 5000);
            }
        }
        catch (Exception ex)
        {
            Notification.Show($"~r~Error: {ex.Message}");
            Screen.ShowSubtitle($"Error: {ex.Message}", 5000);
            Debug.WriteLine($"Initialize error: {ex}");
        }
    }

    private void OnIPCMessageReceived(string message)
    {
        // Handle messages from .NET 8.0 system
        try
        {
            // Disabled subtitle to avoid conflicts with console
            // Screen.ShowSubtitle($"IPC: {message}", 3000);
            Debug.WriteLine($"IPC Received: {message}");
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"OnIPCMessageReceived error: {ex}");
        }
    }

    private void OnAborted(object sender, EventArgs e)
    {
        try
        {
            if (_initialized)
            {
                _ipcClient?.Disconnect();
                Notification.Show("~y~Project Liberty~w~ disconnected");
                _initialized = false;
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"OnAborted error: {ex}");
        }
    }

    private void OnTick(object sender, EventArgs e)
    {
        try
        {
            // Send game state to .NET 8.0 system if connected
            if (_initialized && _ipcClient != null && _ipcClient.IsConnected)
            {
                var now = DateTime.UtcNow;
                
                // Throttle position updates to every 500ms
                if (now - _lastPositionUpdate >= _positionUpdateInterval)
                {
                    var player = Game.Player.Character;
                    if (player != null && player.Exists())
                    {
                        var pos = player.Position;
                        var message = $"PLAYER_POS:{pos.X:F2},{pos.Y:F2},{pos.Z:F2}";
                        _ipcClient.SendMessage(message);
                        _lastPositionUpdate = now;
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"OnTick error: {ex}");
            // Don't crash on tick errors
        }
    }
}
