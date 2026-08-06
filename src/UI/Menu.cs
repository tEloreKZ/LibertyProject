using System;
using GTA;
using ProjectLiberty.Abstractions.UI;
using System.Collections.Generic;
using System.Linq;

namespace ProjectLiberty.UI;

/// <summary>
/// Implementation of a GTA V Native UI menu using Screen subtitles.
/// </summary>
public class Menu : IMenu
{
    private readonly List<IMenuItem> _items = new List<IMenuItem>();
    private int _selectedIndex = 0;
    private bool _isVisible = false;

    public string Id { get; }
    public string Title { get; }
    public IEnumerable<IMenuItem> Items => _items;
    public bool IsVisible => _isVisible;

    public Menu(string id, string title)
    {
        Id = id;
        Title = title;
    }

    public void AddItem(IMenuItem item)
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

        _isVisible = true;
        _selectedIndex = 0;
    }

    public void Hide()
    {
        if (!_isVisible) return;

        _isVisible = false;
    }

    public void Update()
    {
        if (!_isVisible) return;

        try
        {
            // Handle navigation
            if (Game.IsControlJustPressed(GTA.Control.PhoneUp))
            {
                NavigateUp();
            }
            else if (Game.IsControlJustPressed(GTA.Control.PhoneDown))
            {
                NavigateDown();
            }
            else if (Game.IsControlJustPressed(GTA.Control.PhoneSelect))
            {
                SelectItem();
            }
            else if (Game.IsControlJustPressed(GTA.Control.PhoneCancel))
            {
                Hide();
            }

            // Draw menu
            DrawMenu();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Menu update error: {ex.Message}");
            Hide();
        }
    }

    private void NavigateUp()
    {
        var visibleItems = _items.Where(i => i.IsEnabled && i.IsVisible).ToList();
        if (visibleItems.Count == 0) return;

        do
        {
            _selectedIndex--;
            if (_selectedIndex < 0)
            {
                _selectedIndex = visibleItems.Count - 1;
            }
        } while (!visibleItems[_selectedIndex].IsEnabled || !visibleItems[_selectedIndex].IsVisible);
    }

    private void NavigateDown()
    {
        var visibleItems = _items.Where(i => i.IsEnabled && i.IsVisible).ToList();
        if (visibleItems.Count == 0) return;

        do
        {
            _selectedIndex++;
            if (_selectedIndex >= visibleItems.Count)
            {
                _selectedIndex = 0;
            }
        } while (!visibleItems[_selectedIndex].IsEnabled || !visibleItems[_selectedIndex].IsVisible);
    }

    private void SelectItem()
    {
        var visibleItems = _items.Where(i => i.IsEnabled && i.IsVisible).ToList();
        if (_selectedIndex < 0 || _selectedIndex >= visibleItems.Count) return;

        var selectedItem = visibleItems[_selectedIndex];
        selectedItem.OnSelected?.Invoke();
        selectedItem.OnActivated?.Invoke();
    }

    private void DrawMenu()
    {
        var visibleItems = _items.Where(i => i.IsEnabled && i.IsVisible).ToList();
        if (visibleItems.Count == 0) return;

        // Draw title
        GTA.UI.Screen.ShowSubtitle($"~y~{Title}");

        // Draw items
        for (int i = 0; i < visibleItems.Count; i++)
        {
            var item = visibleItems[i];
            var isSelected = i == _selectedIndex;

            var prefix = isSelected ? ">> " : "   ";
            var label = $"{prefix}{item.Label}";

            if (isSelected)
            {
                GTA.UI.Screen.ShowSubtitle($"~w~{label}");
                if (!string.IsNullOrEmpty(item.Description))
                {
                    GTA.UI.Screen.ShowSubtitle($"~b~{item.Description}");
                }
            }
        }
    }
}
