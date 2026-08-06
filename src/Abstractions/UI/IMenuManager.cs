namespace ProjectLiberty.Abstractions.UI;

/// <summary>
/// Manages UI menus.
/// </summary>
public interface IMenuManager
{
    /// <summary>
    /// Registers a menu with the manager.
    /// </summary>
    /// <param name="menu">Menu to register.</param>
    void RegisterMenu(IMenu menu);

    /// <summary>
    /// Unregisters a menu.
    /// </summary>
    /// <param name="menuId">Menu ID to unregister.</param>
    void UnregisterMenu(string menuId);

    /// <summary>
    /// Gets a menu by ID.
    /// </summary>
    /// <param name="menuId">Menu ID.</param>
    /// <returns>Menu, or null if not found.</returns>
    IMenu? GetMenu(string menuId);

    /// <summary>
    /// Gets all registered menus.
    /// </summary>
    /// <returns>All registered menus.</returns>
    IEnumerable<IMenu> GetAllMenus();

    /// <summary>
    /// Shows a menu.
    /// </summary>
    /// <param name="menuId">Menu ID to show.</param>
    void ShowMenu(string menuId);

    /// <summary>
    /// Hides a menu.
    /// </summary>
    /// <param name="menuId">Menu ID to hide.</param>
    void HideMenu(string menuId);

    /// <summary>
    /// Updates all menus (called every frame).
    /// </summary>
    void Update();
}
