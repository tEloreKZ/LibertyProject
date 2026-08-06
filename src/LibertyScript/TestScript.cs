using GTA;
using GTA.Native;
using GTA.UI;
using System;

namespace ProjectLiberty.LibertyScript;

/// <summary>
/// Test script to verify ScriptHookVDotNet integration.
/// </summary>
public class TestScript : Script
{
    public TestScript()
    {
        Tick += OnTick;
        Interval = 100;
    }

    private void OnTick(object sender, EventArgs e)
    {
        // Display test info every 60 ticks (approx 6 seconds)
        if (Game.GameTime % 600 == 0)
        {
            var player = Game.Player.Character;
            if (player.Exists())
            {
                var position = player.Position;
                var health = player.Health;
                var armor = player.Armor;

                Notification.Show($"LibertyScript Test - Pos: {position.X:F1}, {position.Y:F1}, {position.Z:F1}");
                Notification.Show($"Health: {health} | Armor: {armor}");
            }
        }
    }
}
