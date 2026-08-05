using ProjectLiberty.Abstractions.Entities;

namespace ProjectLiberty.Abstractions.GameHost;

/// <summary>
/// World manager interface - manages world state.
/// </summary>
public interface IWorldManager
{
    /// <summary>
    /// Gets the current game time.
    /// </summary>
    /// <returns>Game time.</returns>
    GameTime GetGameTime();
    
    /// <summary>
    /// Sets the game time.
    /// </summary>
    /// <param name="time">Game time to set.</param>
    void SetGameTime(GameTime time);
    
    /// <summary>
    /// Gets the current weather.
    /// </summary>
    /// <returns>Weather type.</returns>
    WeatherType GetWeather();
    
    /// <summary>
    /// Sets the weather.
    /// </summary>
    /// <param name="weather">Weather type to set.</param>
    void SetWeather(WeatherType weather);
    
    /// <summary>
    /// Gets the current waypoint position.
    /// </summary>
    /// <returns>Waypoint position, or null if not set.</returns>
    Vector3? GetWaypoint();
    
    /// <summary>
    /// Sets the waypoint.
    /// </summary>
    /// <param name="position">Waypoint position.</param>
    void SetWaypoint(Vector3 position);
    
    /// <summary>
    /// Clears the waypoint.
    /// </summary>
    void ClearWaypoint();
}

/// <summary>
/// Game time structure.
/// </summary>
public struct GameTime
{
    public int Hour { get; set; }
    public int Minute { get; set; }
    public int Second { get; set; }
}

/// <summary>
/// Weather type enum.
/// </summary>
public enum WeatherType
{
    Clear,
    ExtraSunny,
    Clouds,
    Overcast,
    Rain,
    Clearing,
    Thunder,
    Smog,
    Foggy,
    Xmas,
    Snowlight,
    Blizzards
}
