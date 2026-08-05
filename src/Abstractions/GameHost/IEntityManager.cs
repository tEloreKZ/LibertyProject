using ProjectLiberty.Abstractions.Entities;

namespace ProjectLiberty.Abstractions.GameHost;

/// <summary>
/// Entity manager interface - manages all game entities.
/// </summary>
public interface IEntityManager
{
    /// <summary>
    /// Gets all entities.
    /// </summary>
    /// <returns>All entities.</returns>
    IEnumerable<IEntity> GetAllEntities();
    
    /// <summary>
    /// Gets an entity by handle.
    /// </summary>
    /// <param name="handle">Entity handle.</param>
    /// <returns>Entity, or null if not found.</returns>
    IEntity? GetEntity(int handle);
    
    /// <summary>
    /// Checks if an entity exists.
    /// </summary>
    /// <param name="handle">Entity handle.</param>
    /// <returns>True if exists, false otherwise.</returns>
    bool EntityExists(int handle);
    
    /// <summary>
    /// Creates a new entity.
    /// </summary>
    /// <param name="type">Entity type.</param>
    /// <param name="position">Spawn position.</param>
    /// <returns>Created entity.</returns>
    IEntity CreateEntity(EntityType type, Vector3 position);
    
    /// <summary>
    /// Deletes an entity.
    /// </summary>
    /// <param name="handle">Entity handle.</param>
    void DeleteEntity(int handle);
}
