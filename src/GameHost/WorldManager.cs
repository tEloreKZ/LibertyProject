using ProjectLiberty.Abstractions.GameHost;
using ProjectLiberty.Abstractions.Logging;

namespace ProjectLiberty.GameHost;

/// <summary>
/// World manager implementation.
/// </summary>
public class WorldManager : IWorldManager
{
    private readonly ILogger _logger;
    private GameTime _gameTime = new GameTime { Hour = 12, Minute = 0, Second = 0 };
    private WeatherType _weather = WeatherType.Clear;
    private Vector3? _waypoint;

    public WorldManager(ILogger logger)
    {
        _logger = logger;
    }

    public GameTime GetGameTime()
    {
        return _gameTime;
    }

    public void SetGameTime(GameTime time)
    {
        _gameTime = time;
        _logger.LogDebug($"Game time set to {time.Hour}:{time.Minute}:{time.Second}");
    }

    public WeatherType GetWeather()
    {
        return _weather;
    }

    public void SetWeather(WeatherType weather)
    {
        _weather = weather;
        _logger.LogDebug($"Weather set to {weather}");
    }

    public Vector3? GetWaypoint()
    {
        return _waypoint;
    }

    public void SetWaypoint(Vector3 position)
    {
        _waypoint = position;
        _logger.LogDebug($"Waypoint set to {position}");
    }

    public void ClearWaypoint()
    {
        _waypoint = null;
        _logger.LogDebug("Waypoint cleared");
    }
}
