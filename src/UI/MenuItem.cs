using System;
using ProjectLiberty.Abstractions.UI;

namespace ProjectLiberty.UI;

/// <summary>
/// Implementation of a menu item.
/// </summary>
public class MenuItem : IMenuItem
{
    public string Id { get; }
    public string Label { get; }
    public string? Description { get; set; }
    public bool IsEnabled { get; set; } = true;
    public bool IsVisible { get; set; } = true;
    public Action? OnSelected { get; set; }
    public Action? OnActivated { get; set; }

    public MenuItem(string id, string label, string? description = null)
    {
        Id = id;
        Label = label;
        Description = description;
    }
}
