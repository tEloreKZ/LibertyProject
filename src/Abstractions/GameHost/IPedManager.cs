using ProjectLiberty.Abstractions.Entities;

namespace ProjectLiberty.Abstractions.GameHost;

/// <summary>
/// Ped manager interface - manages ped entities.
/// </summary>
public interface IPedManager
{
    /// <summary>
    /// Gets all peds.
    /// </summary>
    /// <returns>All peds.</returns>
    IEnumerable<IPed> GetAllPeds();
    
    /// <summary>
    /// Gets a ped by handle.
    /// </summary>
    /// <param name="handle">Ped handle.</param>
    /// <returns>Ped, or null if not found.</returns>
    IPed? GetPed(int handle);
    
    /// <summary>
    /// Creates a ped.
    /// </summary>
    /// <param name="model">Ped model.</param>
    /// <param name="position">Spawn position.</param>
    /// <param name="heading">Spawn heading.</param>
    /// <returns>Created ped.</returns>
    IPed CreatePed(string model, Vector3 position, float heading);
    
    /// <summary>
    /// Deletes a ped.
    /// </summary>
    /// <param name="handle">Ped handle.</param>
    void DeletePed(int handle);
}
