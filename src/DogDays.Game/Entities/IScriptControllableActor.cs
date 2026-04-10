using Microsoft.Xna.Framework;
using DogDays.Game.Data;

namespace DogDays.Game.Entities;

/// <summary>
/// Contract for actors that scripted sequences can position, face, and move.
/// </summary>
internal interface IScriptControllableActor
{
    /// <summary>Current top-left world position in pixels.</summary>
    Vector2 Position { get; }

    /// <summary>Current world-space center point.</summary>
    Vector2 Center { get; }

    /// <summary>Current facing direction.</summary>
    FacingDirection Facing { get; }

    /// <summary>Whether the actor moved this frame.</summary>
    bool IsMoving { get; }

    /// <summary>
    /// Directly sets the actor's world position for a scripted sequence.
    /// </summary>
    void SetPosition(Vector2 position);

    /// <summary>
    /// Directly sets the actor's facing direction for a scripted sequence.
    /// </summary>
    void SetFacing(FacingDirection facing);

    /// <summary>
    /// Applies scripted movement for the current frame.
    /// </summary>
    bool ApplyScriptedMovement(Vector2 movementDelta);

    /// <summary>
    /// Clears transient movement state when a sequence wants the actor idle.
    /// </summary>
    void ClearMovementState();
}