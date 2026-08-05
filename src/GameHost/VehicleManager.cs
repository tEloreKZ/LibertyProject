using ProjectLiberty.Abstractions.Entities;
using ProjectLiberty.Abstractions.GameHost;
using ProjectLiberty.Abstractions.Logging;

namespace ProjectLiberty.GameHost;

/// <summary>
/// Vehicle manager implementation.
/// </summary>
public class VehicleManager : IVehicleManager
{
    private readonly ILogger _logger;
    private readonly EntityManager _entityManager;

    public VehicleManager(ILogger logger, EntityManager entityManager)
    {
        _logger = logger;
        _entityManager = entityManager;
    }

    public IEnumerable<IVehicle> GetAllVehicles()
    {
        return _entityManager.GetAllEntities().OfType<IVehicle>();
    }

    public IVehicle? GetVehicle(int handle)
    {
        var entity = _entityManager.GetEntity(handle);
        return entity as IVehicle;
    }

    public IVehicle CreateVehicle(string model, Vector3 position, float heading)
    {
        var vehicle = (IVehicle)_entityManager.CreateEntity(EntityType.Vehicle, position);
        _logger.LogInformation($"Created vehicle: {model} at {position}");
        return vehicle;
    }

    public void DeleteVehicle(int handle)
    {
        _entityManager.DeleteEntity(handle);
        _logger.LogDebug($"Deleted vehicle with handle {handle}");
    }
}
