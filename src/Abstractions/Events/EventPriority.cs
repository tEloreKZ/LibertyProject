namespace ProjectLiberty.Abstractions.Events;

/// <summary>
/// Event priority levels for event handlers.
/// </summary>
public enum EventPriority
{
    /// <summary>
    /// Lowest priority - handled last.
    /// </summary>
    Low = 0,

    /// <summary>
    /// Normal priority - default.
    /// </summary>
    Normal = 1,

    /// <summary>
    /// High priority - handled before normal.
    /// </summary>
    High = 2,

    /// <summary>
    /// Critical priority - handled first.
    /// </summary>
    Critical = 3
}
