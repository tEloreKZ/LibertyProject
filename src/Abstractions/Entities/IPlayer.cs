namespace ProjectLiberty.Abstractions.Entities;

/// <summary>
/// Player interface.
/// </summary>
public interface IPlayer : IEntity
{
    /// <summary>
    /// Gets the player's character ped.
    /// </summary>
    IPed Character { get; }
    
    /// <summary>
    /// Gets or sets the player's health.
    /// </summary>
    int Health { get; set; }
    
    /// <summary>
    /// Gets or sets the player's armor.
    /// </summary>
    int Armor { get; set; }
    
    /// <summary>
    /// Gets or sets the player's current weapon.
    /// </summary>
    string CurrentWeapon { get; set; }
    
    /// <summary>
    /// Gets the player's name.
    /// </summary>
    string Name { get; }
}
