#nullable enable

using Microsoft.Xna.Framework;
using DogDays.Game.Data;
using DogDays.Game.World;

namespace DogDays.Game.Entities;

/// <summary>
/// Contract for patrolling NPCs that can be paused and posed by scripted sequences.
/// </summary>
internal interface IScriptControllableNpc : IScriptControllableActor
{
    /// <summary>
    /// Enables or disables the NPC's autonomous behavior.
    /// </summary>
    /// <param name="isEnabled">True to allow autonomous updates; false to hold the NPC in scripted control.</param>
    void SetAutonomousBehaviorEnabled(bool isEnabled);

    /// <summary>
    /// World-space navigation anchor used for pathfinding, typically the NPC's foot center.
    /// </summary>
    Vector2 NavigationPosition { get; }
    /// <summary>
    /// Applies scripted movement while still respecting world collision.
    /// </summary>
    /// <param name="movementDelta">World-space movement delta in pixels for this frame.</param>
    /// <param name="collisionData">Collision source used to prevent moving through obstacles.</param>
    bool ApplyScriptedMovement(Vector2 movementDelta, IMapCollisionData? collisionData);
}