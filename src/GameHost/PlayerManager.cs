using ProjectLiberty.Abstractions.Entities;
using ProjectLiberty.Abstractions.GameHost;
using ProjectLiberty.Abstractions.Logging;

namespace ProjectLiberty.GameHost;

/// <summary>
/// Player manager implementation.
/// </summary>
public class PlayerManager : IPlayerManager
{
    private readonly ILogger _logger;
    private readonly EntityManager _entityManager;

    public PlayerManager(ILogger logger, EntityManager entityManager)
    {
        _logger = logger;
        _entityManager = entityManager;
    }

    public IPlayer? GetLocalPlayer()
    {
        // This would get the actual local player from the game
        // For now, return a placeholder
        var player = new PlayerEntity(0, new Vector3(0, 0, 0));
        return player;
    }

    public IEnumerable<IPlayer> GetAllPlayers()
    {
        // This would get all players from the game
        // For now, return local player only
        var localPlayer = GetLocalPlayer();
        if (localPlayer != null)
        {
            yield return localPlayer;
        }
    }

    public IPlayer? GetPlayer(int handle)
    {
        var entity = _entityManager.GetEntity(handle);
        return entity as IPlayer;
    }

    private class PlayerEntity : IPlayer
    {
        public int Handle { get; }
        public EntityType Type => EntityType.Ped;
        public Vector3 Position { get; set; }
        public Vector3 Rotation { get; set; }
        public bool Exists { get; private set; } = true;
        public IPed Character { get; } = new PedEntity(0, new Vector3(0, 0, 0));
        public int Health { get; set; } = 100;
        public int Armor { get; set; } = 0;
        public string CurrentWeapon { get; set; } = "pistol";
        public string Name { get; } = "Player";

        public PlayerEntity(int handle, Vector3 position)
        {
            Handle = handle;
            Position = position;
        }

        public void Delete()
        {
            Exists = false;
        }
    }
}
