namespace ProjectLiberty.Abstractions.Entities;

/// <summary>
/// Vehicle interface.
/// </summary>
public interface IVehicle : IEntity
{
    /// <summary>
    /// Gets the vehicle model.
    /// </summary>
    string Model { get; }
    
    /// <summary>
    /// Gets or sets the vehicle health.
    /// </summary>
    int Health { get; set; }
    
    /// <summary>
    /// Gets or sets the vehicle speed.
    /// </summary>
    float Speed { get; set; }
    
    /// <summary>
    /// Gets or sets the maximum speed.
    /// </summary>
    float MaxSpeed { get; set; }
    
    /// <summary>
    /// Gets or sets whether the siren is on.
    /// </summary>
    bool IsSirenOn { get; set; }
    
    /// <summary>
    /// Gets or sets whether the engine is on.
    /// </summary>
    bool IsEngineOn { get; set; }
    
    /// <summary>
    /// Gets the driver of the vehicle.
    /// </summary>
    /// <returns>Driver ped, or null if no driver.</returns>
    IPed? GetDriver();
    
    /// <summary>
    /// Gets all passengers in the vehicle.
    /// </summary>
    /// <returns>All passengers.</returns>
    IEnumerable<IPed> GetPassengers();
    
    /// <summary>
    /// Makes the vehicle drive to a position.
    /// </summary>
    /// <param name="position">Target position.</param>
    /// <param name="speed">Driving speed.</param>
    void DriveTo(Vector3 position, float speed);
}
