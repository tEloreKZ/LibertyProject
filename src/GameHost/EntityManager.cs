using ProjectLiberty.Abstractions.Entities;
using ProjectLiberty.Abstractions.GameHost;
using ProjectLiberty.Abstractions.Logging;

namespace ProjectLiberty.GameHost;

/// <summary>
/// Entity manager implementation - basic pooling for plugin tracking.
/// Use ScriptHookVDotNet directly for game entity operations.
/// </summary>
public class EntityManager : IEntityManager
{
    private readonly ILogger _logger;
    private readonly Dictionary<int, IEntity> _entities = new Dictionary<int, IEntity>();
    private int _nextHandle = 1;

    public EntityManager(ILogger logger)
    {
        _logger = logger;
    }

    public IEnumerable<IEntity> GetAllEntities()
    {
        return _entities.Values;
    }

    public IEntity? GetEntity(int handle)
    {
        return _entities.TryGetValue(handle, out var entity) ? entity : null;
    }

    public bool EntityExists(int handle)
    {
        return _entities.ContainsKey(handle);
    }

    public IEntity CreateEntity(EntityType type, Vector3 position)
    {
        var handle = _nextHandle++;
        var entity = new BaseEntity(handle, position, type);
        _entities[handle] = entity;
        _logger.LogDebug($"Created entity: {type} with handle {handle}");
        return entity;
    }

    public void DeleteEntity(int handle)
    {
        if (_entities.Remove(handle))
        {
            _logger.LogDebug($"Deleted entity with handle {handle}");
        }
    }

    private class BaseEntity : IEntity
    {
        public int Handle { get; }
        public EntityType Type { get; }
        public Vector3 Position { get; set; }
        public Vector3 Rotation { get; set; }
        public bool Exists { get; private set; } = true;

        public BaseEntity(int handle, Vector3 position, EntityType type)
        {
            Handle = handle;
            Position = position;
            Type = type;
        }

        public void Delete()
        {
            Exists = false;
        }
    }
}
