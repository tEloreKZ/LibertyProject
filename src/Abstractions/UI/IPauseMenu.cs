namespace ProjectLiberty.Abstractions.UI;

/// <summary>
/// Represents a pause menu tab in GTA V.
/// </summary>
public interface IPauseMenu
{
    /// <summary>
    /// Gets the menu ID.
    /// </summary>
    string Id { get; }

    /// <summary>
    /// Gets the menu title.
    /// </summary>
    string Title { get; }

    /// <summary>
    /// Gets whether the menu is currently visible.
    /// </summary>
    bool IsVisible { get; }

    /// <summary>
    /// Adds an item to the menu.
    /// </summary>
    /// <param name="item">Item to add.</param>
    void AddItem(IPauseMenuItem item);

    /// <summary>
    /// Removes an item from the menu.
    /// </summary>
    /// <param name="itemId">Item ID to remove.</param>
    void RemoveItem(string itemId);

    /// <summary>
    /// Shows the menu.
    /// </summary>
    void Show();

    /// <summary>
    /// Hides the menu.
    /// </summary>
    void Hide();

    /// <summary>
    /// Updates the menu state (called every frame).
    /// </summary>
    void Update();
}
