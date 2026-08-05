namespace ProjectLiberty.Abstractions.GameHost;

/// <summary>
/// Vehicle manager interface - manages vehicle entities.
/// </summary>
public interface IVehicleManager
{
    /// <summary>
    /// Gets all vehicles.
    /// </summary>
    /// <returns>All vehicles.</returns>
    IEnumerable<IVehicle> GetAllVehicles();
    
    /// <summary>
    /// Gets a vehicle by handle.
    /// </summary>
    /// <param name="handle">Vehicle handle.</param>
    /// <returns>Vehicle, or null if not found.</returns>
    IVehicle? GetVehicle(int handle);
    
    /// <summary>
    /// Creates a vehicle.
    /// </summary>
    /// <param name="model">Vehicle model.</param>
    /// <param name="position">Spawn position.</param>
    /// <param name="heading">Spawn heading.</param>
    /// <returns>Created vehicle.</returns>
    IVehicle CreateVehicle(string model, Vector3 position, float heading);
    
    /// <summary>
    /// Deletes a vehicle.
    /// </summary>
    /// <param name="handle">Vehicle handle.</param>
    void DeleteVehicle(int handle);
}
