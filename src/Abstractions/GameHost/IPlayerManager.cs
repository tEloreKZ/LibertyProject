using ProjectLiberty.Abstractions.Entities;

namespace ProjectLiberty.Abstractions.GameHost;

/// <summary>
/// Player manager interface - manages player entities.
/// </summary>
public interface IPlayerManager
{
    /// <summary>
    /// Gets the local player.
    /// </summary>
    /// <returns>Local player, or null if not available.</returns>
    IPlayer? GetLocalPlayer();
    
    /// <summary>
    /// Gets all players.
    /// </summary>
    /// <returns>All players.</returns>
    IEnumerable<IPlayer> GetAllPlayers();
    
    /// <summary>
    /// Gets a player by handle.
    /// </summary>
    /// <param name="handle">Player handle.</param>
    /// <returns>Player, or null if not found.</returns>
    IPlayer? GetPlayer(int handle);
}
