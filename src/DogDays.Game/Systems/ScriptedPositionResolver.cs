using System;
using System.Collections.Generic;
using DogDays.Game.Data;
using DogDays.Game.Entities;
using Microsoft.Xna.Framework;

namespace DogDays.Game.Systems;

/// <summary>
/// Resolves scripted actor positions from named map anchors plus authored offsets.
/// </summary>
internal static class ScriptedPositionResolver
{
    /// <summary>
    /// Resolves a top-left world position from a named spawn-point anchor, or falls back to the supplied position.
    /// </summary>
    internal static Vector2 ResolveTopLeftPosition(
        IReadOnlyList<SpawnPointData> spawnPoints,
        ScriptedPositionDirective directive,
        Vector2 fallbackTopLeftPosition,
        Vector2 actorHalfSize)
    {
        ArgumentNullException.ThrowIfNull(spawnPoints);

        var resolvedAnchor = fallbackTopLeftPosition;
        if (directive.SpawnPointId is { Length: > 0 } spawnPointId
            && FindSpawnPoint(spawnPoints, spawnPointId) is { } spawnPoint)
        {
            resolvedAnchor = spawnPoint.Position - actorHalfSize;
        }

        return resolvedAnchor + directive.Offset;
    }

    /// <summary>
    /// Applies a scripted pose by resolving its anchor and setting the actor position + facing.
    /// </summary>
    internal static void ApplyPose(
        IScriptControllableActor actor,
        ScriptedActorPoseDirective directive,
        IReadOnlyList<SpawnPointData> spawnPoints,
        Vector2 fallbackTopLeftPosition,
        Vector2 actorHalfSize)
    {
        ArgumentNullException.ThrowIfNull(actor);

        actor.SetScriptedPose(
            ResolveTopLeftPosition(spawnPoints, directive.Position, fallbackTopLeftPosition, actorHalfSize),
            directive.Facing);
    }

    private static SpawnPointData? FindSpawnPoint(IReadOnlyList<SpawnPointData> spawnPoints, string spawnPointId)
    {
        for (var i = 0; i < spawnPoints.Count; i++)
        {
            if (string.Equals(spawnPoints[i].Name, spawnPointId, StringComparison.Ordinal))
            {
                return spawnPoints[i];
            }
        }

        return null;
    }
}