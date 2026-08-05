using ProjectLiberty.Abstractions.Entities;
using ProjectLiberty.Abstractions.GameHost;
using ProjectLiberty.Abstractions.Logging;

namespace ProjectLiberty.GameHost;

/// <summary>
/// Ped manager implementation.
/// </summary>
public class PedManager : IPedManager
{
    private readonly ILogger _logger;
    private readonly EntityManager _entityManager;

    public PedManager(ILogger logger, EntityManager entityManager)
    {
        _logger = logger;
        _entityManager = entityManager;
    }

    public IEnumerable<IPed> GetAllPeds()
    {
        return _entityManager.GetAllEntities().OfType<IPed>();
    }

    public IPed? GetPed(int handle)
    {
        var entity = _entityManager.GetEntity(handle);
        return entity as IPed;
    }

    public IPed CreatePed(string model, Vector3 position, float heading)
    {
        var ped = (IPed)_entityManager.CreateEntity(EntityType.Ped, position);
        _logger.LogInformation($"Created ped: {model} at {position}");
        return ped;
    }

    public void DeletePed(int handle)
    {
        _entityManager.DeleteEntity(handle);
        _logger.LogDebug($"Deleted ped with handle {handle}");
    }
}
