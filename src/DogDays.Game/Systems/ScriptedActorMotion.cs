#nullable enable

using System;
using Microsoft.Xna.Framework;
using DogDays.Game.Entities;
using DogDays.Game.World;

namespace DogDays.Game.Systems;

/// <summary>
/// Shared movement helpers for scripted sequences that steer actors toward authored targets.
/// </summary>
internal static class ScriptedActorMotion
{
    /// <summary>
    /// Moves an actor toward a target position using a fixed speed.
    /// </summary>
    internal static bool MoveTowards(
        IScriptControllableActor actor,
        Vector2 targetPosition,
        float elapsedSeconds,
        float speedPixelsPerSecond)
    {
        ArgumentNullException.ThrowIfNull(actor);

        var movementDelta = ComputeMovementDelta(actor.Position, targetPosition, elapsedSeconds, speedPixelsPerSecond);
        if (movementDelta == Vector2.Zero)
        {
            actor.ClearMovementState();
            return Vector2.DistanceSquared(actor.Position, targetPosition) <= 0.01f;
        }

        actor.ApplyScriptedMovement(movementDelta);
        if (Vector2.DistanceSquared(actor.Position, targetPosition) <= 0.01f)
        {
            actor.ClearMovementState();
            return true;
        }

        return false;
    }

    /// <summary>
    /// Moves a patrolling NPC toward a target position while respecting collision.
    /// </summary>
    internal static bool MoveTowards(
        IScriptControllableNpc npc,
        Vector2 targetPosition,
        float elapsedSeconds,
        float speedPixelsPerSecond,
        IMapCollisionData? collisionData)
    {
        ArgumentNullException.ThrowIfNull(npc);

        var movementDelta = ComputeMovementDelta(npc.Position, targetPosition, elapsedSeconds, speedPixelsPerSecond);
        if (movementDelta == Vector2.Zero)
        {
            npc.ClearMovementState();
            return Vector2.DistanceSquared(npc.Position, targetPosition) <= 0.01f;
        }

        npc.ApplyScriptedMovement(movementDelta, collisionData);
        if (Vector2.DistanceSquared(npc.Position, targetPosition) <= 0.01f)
        {
            npc.ClearMovementState();
            return true;
        }

        return false;
    }

    private static Vector2 ComputeMovementDelta(
        Vector2 currentPosition,
        Vector2 targetPosition,
        float elapsedSeconds,
        float speedPixelsPerSecond)
    {
        var toTarget = targetPosition - currentPosition;
        var distanceSquared = toTarget.LengthSquared();
        if (distanceSquared <= 0.01f)
        {
            return Vector2.Zero;
        }

        if (elapsedSeconds <= 0f || speedPixelsPerSecond <= 0f)
        {
            return Vector2.Zero;
        }

        var distance = MathF.Sqrt(distanceSquared);
        var maxStep = speedPixelsPerSecond * elapsedSeconds;
        if (maxStep >= distance)
        {
            return toTarget;
        }

        return toTarget / distance * maxStep;
    }
}