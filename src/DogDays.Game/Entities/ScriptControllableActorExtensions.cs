using Microsoft.Xna.Framework;
using DogDays.Game.Data;

namespace DogDays.Game.Entities;

/// <summary>
/// Helper methods for applying scripted control to actors and patrol NPCs.
/// </summary>
internal static class ScriptControllableActorExtensions
{
    /// <summary>
    /// Applies an immediate scripted pose, clearing movement state.
    /// </summary>
    internal static void SetScriptedPose(this IScriptControllableActor actor, Vector2 position, FacingDirection facing)
    {
        actor.SetPosition(position);
        actor.SetFacing(facing);
        actor.ClearMovementState();
    }

    /// <summary>
    /// Freezes a patrol NPC and applies an immediate scripted pose.
    /// </summary>
    internal static void HoldForScriptedSequence(this IScriptControllableNpc npc, Vector2 position, FacingDirection facing)
    {
        npc.SetAutonomousBehaviorEnabled(false);
        npc.SetScriptedPose(position, facing);
    }
}