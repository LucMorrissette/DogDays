using Microsoft.Xna.Framework;
using DogDays.Game.Data;
using DogDays.Game.Entities;
using DogDays.Game.Systems;
using DogDays.Game.World;
using DogDays.Tests.Helpers;

namespace DogDays.Tests.Unit;

/// <summary>
/// Unit tests for scripted NPC routes that follow authored nav-graph A* paths.
/// </summary>
public sealed class ScriptedNpcRouteTests
{
    [Fact]
    public void Update__FollowsIntermediateNavNodesInsteadOfCuttingDirectlyAcrossRoom()
    {
        var graph = new IndoorNavGraph(
            new IndoorNavNode[]
            {
                new(1, new Vector2(100f, 100f), "start", null),
                new(2, new Vector2(100f, 200f), "middle", null),
                new(3, new Vector2(200f, 200f), "entry", null),
            },
            new IndoorNavLink[]
            {
                new(1, 2),
                new(2, 3),
            });

        var npc = new FakeScriptControllableNpc(new Vector2(84f, 84f));
        var route = ScriptedNpcRoute.Create(graph, npc, "entry");

        Assert.NotNull(route);

        for (var i = 0; i < 20; i++)
        {
            route!.Update(FakeGameTime.OneFrame(), npc, collisionData: null, speedPixelsPerSecond: 60f);
        }

        Assert.Equal(84f, npc.Position.X);
        Assert.True(npc.Position.Y > 84f,
            $"NPC should have started by moving down toward the intermediate node. Position={npc.Position}");
    }

    private sealed class FakeScriptControllableNpc : IScriptControllableNpc
    {
        public FakeScriptControllableNpc(Vector2 position)
        {
            Position = position;
        }

        public Vector2 Position { get; private set; }

        public Vector2 Center => Position + new Vector2(16f, 16f);

        public FacingDirection Facing { get; private set; } = FacingDirection.Down;

        public bool IsMoving { get; private set; }

        public Vector2 NavigationPosition => Position + new Vector2(16f, 16f);

        public void SetPosition(Vector2 position)
        {
            Position = position;
        }

        public void SetFacing(FacingDirection facing)
        {
            Facing = facing;
        }

        public bool ApplyScriptedMovement(Vector2 movementDelta)
        {
            if (movementDelta == Vector2.Zero)
            {
                IsMoving = false;
                return false;
            }

            Position += movementDelta;
            IsMoving = true;
            return true;
        }

        public bool ApplyScriptedMovement(Vector2 movementDelta, IMapCollisionData? collisionData)
        {
            return ApplyScriptedMovement(movementDelta);
        }

        public void ClearMovementState()
        {
            IsMoving = false;
        }

        public void SetAutonomousBehaviorEnabled(bool isEnabled)
        {
        }
    }
}