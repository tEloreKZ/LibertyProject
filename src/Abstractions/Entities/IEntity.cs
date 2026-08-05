namespace ProjectLiberty.Abstractions.Entities;

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

/// <summary>
/// Vector3 structure.
/// </summary>
public struct Vector3
{
    public float X { get; set; }
    public float Y { get; set; }
    public float Z { get; set; }
    
    public Vector3(float x, float y, float z)
    {
        X = x;
        Y = y;
        Z = z;
    }
    
    public static Vector3 Zero => new Vector3(0, 0, 0);
}
