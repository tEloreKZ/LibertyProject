using ProjectLiberty.Abstractions.Entities;
using ProjectLiberty.Abstractions.GameHost;
using ProjectLiberty.Abstractions.Logging;

namespace ProjectLiberty.GameHost;

/// <summary>
/// Entity manager implementation.
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
        IEntity entity;

        // This would create actual game entities through native calls
        // For now, create placeholder entities
        entity = type switch
        {
            EntityType.Ped => new PedEntity(handle, position),
            EntityType.Vehicle => new VehicleEntity(handle, position),
            _ => new BaseEntity(handle, position, type)
        };

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

    private class PedEntity : BaseEntity, IPed
    {
        public string Model { get; set; } = "default";
        public int Health { get; set; } = 100;
        public int Armor { get; set; } = 0;
        public bool CanFight { get; set; } = true;
        public bool CanUseWeapons { get; set; } = true;
        public bool CanRagdoll { get; set; } = true;

        public PedEntity(int handle, Vector3 position) : base(handle, position, EntityType.Ped) { }

        public IVehicle? GetCurrentVehicle() => null;
        public void PlayAnimation(string animDict, string animName, float duration) { }
        public void StopAnimation() { }
        public void FollowPlayer() { }
        public void Wander() { }
        public void Flee() { }
    }

    private class VehicleEntity : BaseEntity, IVehicle
    {
        public string Model { get; set; } = "default";
        public int Health { get; set; } = 1000;
        public float Speed { get; set; }
        public float MaxSpeed { get; set; } = 100f;
        public bool IsSirenOn { get; set; }
        public bool IsEngineOn { get; set; } = true;

        public VehicleEntity(int handle, Vector3 position) : base(handle, position, EntityType.Vehicle) { }

        public IPed? GetDriver() => null;
        public IEnumerable<IPed> GetPassengers() => Enumerable.Empty<IPed>();
        public void DriveTo(Vector3 position, float speed) { }
    }
}
