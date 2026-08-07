using System;
using System.Collections.Generic;
using System.Linq;
using GTA;
using ProjectLiberty.Abstractions.UI;

namespace ProjectLiberty.UI;

/// <summary>
/// Implementation of a GTA V pause menu tab.
/// </summary>
public class PauseMenu : IPauseMenu
{
    private readonly List<IPauseMenuItem> _items = new List<IPauseMenuItem>();
    private bool _isVisible = false;

    public string Id { get; }
    public string Title { get; }
    public bool IsVisible => _isVisible;

    public PauseMenu(string id, string title)
    {
        Id = id;
        Title = title;
    }

    public void AddItem(IPauseMenuItem item)
    {
        _items.Add(item);
    }

    public void RemoveItem(string itemId)
    {
        var item = _items.FirstOrDefault(i => i.Id == itemId);
        if (item != null)
        {
            _items.Remove(item);
        }
    }

    public void Show()
    {
        if (_isVisible) return;

        try
        {
            // Placeholder for pause menu integration
            // This requires native function calls that may not be available in SHVDN
            // For now, we'll use a simple notification to indicate the menu is "shown"
            GTA.UI.Notification.Show($"~y~{Title} - Pause Menu (SCALEFORM integration pending)");
            _isVisible = true;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error showing pause menu: {ex.Message}");
        }
    }

    public void Hide()
    {
        if (!_isVisible) return;

        try
        {
            GTA.UI.Notification.Show("Pause Menu Closed");
            _isVisible = false;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error hiding pause menu: {ex.Message}");
        }
    }

    public void Update()
    {
        if (!_isVisible) return;

        try
        {
            // Placeholder for pause menu update logic
            // This will handle item selection when SCALEFORM integration is complete
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error updating pause menu: {ex.Message}");
        }
    }
}
