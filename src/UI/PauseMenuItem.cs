using System;
using ProjectLiberty.Abstractions.UI;

namespace ProjectLiberty.UI;

/// <summary>
/// Implementation of a pause menu item.
/// </summary>
public class PauseMenuItem : IPauseMenuItem
{
    public string Id { get; }
    public string Label { get; }
    public string? Description { get; set; }
    public bool IsEnabled { get; set; } = true;
    public Action? OnSelected { get; set; }

    public PauseMenuItem(string id, string label, string? description = null)
    {
        Id = id;
        Label = label;
        Description = description;
    }
}
