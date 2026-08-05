namespace ProjectLiberty.Abstractions.Entities;

/// <summary>
/// Entity type enum.
/// </summary>
public enum EntityType
{
    Ped,
    Vehicle,
    Object,
    Prop
}

/// <summary>
/// Base entity interface.
/// </summary>
public interface IEntity
{
    /// <summary>
    /// Gets the entity handle.
    /// </summary>
    int Handle { get; }
    
    /// <summary>
    /// Gets the entity type.
    /// </summary>
    EntityType Type { get; }
    
    /// <summary>
    /// Gets or sets the entity position.
    /// </summary>
    Vector3 Position { get; set; }
    
    /// <summary>
    /// Gets or sets the entity rotation.
    /// </summary>
    Vector3 Rotation { get; set; }
    
    /// <summary>
    /// Gets a value indicating whether the entity exists.
    /// </summary>
    bool Exists { get; }
    
    /// <summary>
    /// Deletes the entity.
    /// </summary>
    void Delete();
}
