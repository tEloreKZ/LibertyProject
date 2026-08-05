namespace ProjectLiberty.Abstractions.Entities;

/// <summary>
/// Ped interface.
/// </summary>
public interface IPed : IEntity
{
    /// <summary>
    /// Gets the ped model.
    /// </summary>
    string Model { get; }
    
    /// <summary>
    /// Gets or sets the ped's health.
    /// </summary>
    int Health { get; set; }
    
    /// <summary>
    /// Gets or sets the ped's armor.
    /// </summary>
    int Armor { get; set; }
    
    /// <summary>
    /// Gets or sets whether the ped can fight.
    /// </summary>
    bool CanFight { get; set; }
    
    /// <summary>
    /// Gets or sets whether the ped can use weapons.
    /// </summary>
    bool CanUseWeapons { get; set; }
    
    /// <summary>
    /// Gets or sets whether the ped can ragdoll.
    /// </summary>
    bool CanRagdoll { get; set; }
    
    /// <summary>
    /// Gets the ped's current vehicle.
    /// </summary>
    /// <returns>Current vehicle, or null if not in vehicle.</returns>
    IVehicle? GetCurrentVehicle();
    
    /// <summary>
    /// Plays an animation on the ped.
    /// </summary>
    /// <param name="animDict">Animation dictionary.</param>
    /// <param name="animName">Animation name.</param>
    /// <param name="duration">Animation duration in seconds.</param>
    void PlayAnimation(string animDict, string animName, float duration);
    
    /// <summary>
    /// Stops the current animation.
    /// </summary>
    void StopAnimation();
    
    /// <summary>
    /// Makes the ped follow the player.
    /// </summary>
    void FollowPlayer();
    
    /// <summary>
    /// Makes the ped wander.
    /// </summary>
    void Wander();
    
    /// <summary>
    /// Makes the ped flee.
    /// </summary>
    void Flee();
}
