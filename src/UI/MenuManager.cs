using ProjectLiberty.Abstractions.UI;
using System.Collections.Generic;

namespace ProjectLiberty.UI;

/// <summary>
/// Implementation of the menu manager.
/// </summary>
public class MenuManager : IMenuManager
{
    private readonly Dictionary<string, IMenu> _menus = new Dictionary<string, IMenu>();

    public void RegisterMenu(IMenu menu)
    {
        if (!_menus.ContainsKey(menu.Id))
        {
            _menus[menu.Id] = menu;
        }
    }

    public void UnregisterMenu(string menuId)
    {
        _menus.Remove(menuId);
    }

    public IMenu? GetMenu(string menuId)
    {
        return _menus.TryGetValue(menuId, out var menu) ? menu : null;
    }

    public IEnumerable<IMenu> GetAllMenus()
    {
        return _menus.Values;
    }

    public void ShowMenu(string menuId)
    {
        var menu = GetMenu(menuId);
        menu?.Show();
    }

    public void HideMenu(string menuId)
    {
        var menu = GetMenu(menuId);
        menu?.Hide();
    }

    public void Update()
    {
        foreach (var menu in _menus.Values)
        {
            menu.Update();
        }
    }
}
