#nullable enable

using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using DogDays.Game.Entities;
using DogDays.Game.World;

namespace DogDays.Game.Systems;

/// <summary>
/// Follows an authored A* route for a scripted patrol NPC using the map's nav graph.
/// </summary>
internal sealed class ScriptedNpcRoute
{
    private readonly Vector2[] _topLeftTargets;
    private int _targetIndex;

    private ScriptedNpcRoute(Vector2[] topLeftTargets)
    {
        _topLeftTargets = topLeftTargets;
    }

    /// <summary>
    /// Builds a scripted NPC route from the NPC's nearest nav node to a named destination node.
    /// </summary>
    internal static ScriptedNpcRoute? Create(
        IndoorNavGraph? navGraph,
        IScriptControllableNpc npc,
        string destinationNodeName)
    {
        ArgumentNullException.ThrowIfNull(npc);

        if (navGraph is null)
        {
            return null;
        }

        var destinationNode = navGraph.FindNodeByName(destinationNodeName);
        var startNode = navGraph.FindNearestNode(npc.NavigationPosition);
        if (destinationNode is null || startNode is null)
        {
            return null;
        }

        var routeNodes = navGraph.FindRoute(startNode.Id, destinationNode.Id);
        if (routeNodes.Count == 0)
        {
            return null;
        }

        var navigationOffset = npc.NavigationPosition - npc.Position;
        var topLeftTargets = new List<Vector2>(routeNodes.Count);
        for (var i = 0; i < routeNodes.Count; i++)
        {
            topLeftTargets.Add(routeNodes[i].Position - navigationOffset);
        }

        return new ScriptedNpcRoute(topLeftTargets.ToArray());
    }

    /// <summary>
    /// Advances the NPC along the authored route.
    /// </summary>
    internal bool Update(
        GameTime gameTime,
        IScriptControllableNpc npc,
        IMapCollisionData? collisionData,
        float speedPixelsPerSecond)
    {
        ArgumentNullException.ThrowIfNull(npc);

        var elapsedSeconds = Math.Max(0f, (float)gameTime.ElapsedGameTime.TotalSeconds);
        while (_targetIndex < _topLeftTargets.Length)
        {
            var arrived = ScriptedActorMotion.MoveTowards(
                npc,
                _topLeftTargets[_targetIndex],
                elapsedSeconds,
                speedPixelsPerSecond,
                collisionData);

            if (!arrived)
            {
                return false;
            }

            _targetIndex++;
        }

        npc.ClearMovementState();
        return true;
    }
}