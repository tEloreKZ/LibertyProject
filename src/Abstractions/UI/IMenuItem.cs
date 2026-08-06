namespace ProjectLiberty.Abstractions.UI;

/// <summary>
/// Represents a menu item.
/// </summary>
public interface IMenuItem
{
    /// <summary>
    /// Gets the item ID.
    /// </summary>
    string Id { get; }

    /// <summary>
    /// Gets the item label.
    /// </summary>
    string Label { get; }

    /// <summary>
    /// Gets the item description.
    /// </summary>
    string? Description { get; }

    /// <summary>
    /// Gets whether the item is enabled.
    /// </summary>
    bool IsEnabled { get; set; }

    /// <summary>
    /// Gets whether the item is visible.
    /// </summary>
    bool IsVisible { get; set; }

    /// <summary>
    /// Gets the action to execute when the item is selected.
    /// </summary>
    Action? OnSelected { get; set; }

    /// <summary>
    /// Gets the action to execute when the item is activated.
    /// </summary>
    Action? OnActivated { get; set; }
}
