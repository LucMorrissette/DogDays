using Microsoft.Xna.Framework;
using DogDays.Game.Data;
using DogDays.Game.Entities;
using DogDays.Game.Systems;

namespace DogDays.Tests.Unit;

/// <summary>
/// Unit tests for resolving authored scripted positions from map anchors.
/// </summary>
public sealed class ScriptedPositionResolverTests
{
    private static readonly Vector2 HalfFrame = new(16f, 16f);

    [Fact]
    public void ResolveTopLeftPosition__UsesSpawnPointAnchorWhenPresent()
    {
        var spawnPoints = new[]
        {
            new SpawnPointData("entry", new Vector2(80f, 96f)),
        };

        var position = ScriptedPositionResolver.ResolveTopLeftPosition(
            spawnPoints,
            new ScriptedPositionDirective("entry", new Vector2(8f, -4f)),
            new Vector2(10f, 20f),
            HalfFrame);

        Assert.Equal(new Vector2(72f, 76f), position);
    }

    [Fact]
    public void ResolveTopLeftPosition__UsesFallbackWhenSpawnPointMissing()
    {
        var position = ScriptedPositionResolver.ResolveTopLeftPosition(
            Array.Empty<SpawnPointData>(),
            new ScriptedPositionDirective("missing", new Vector2(-10f, 12f)),
            new Vector2(32f, 48f),
            HalfFrame);

        Assert.Equal(new Vector2(22f, 60f), position);
    }

    [Fact]
    public void ApplyPose__SetsResolvedPositionAndFacing()
    {
        var actor = new FakeScriptControllableActor(new Vector2(0f, 0f));
        actor.ApplyScriptedMovement(new Vector2(1f, 0f));

        ScriptedPositionResolver.ApplyPose(
            actor,
            new ScriptedActorPoseDirective(
                new ScriptedPositionDirective("entry", new Vector2(2f, 6f)),
                FacingDirection.Left),
            new[]
            {
                new SpawnPointData("entry", new Vector2(48f, 64f)),
            },
            new Vector2(12f, 16f),
            HalfFrame);

        Assert.Equal(new Vector2(34f, 54f), actor.Position);
        Assert.Equal(FacingDirection.Left, actor.Facing);
        Assert.False(actor.IsMoving);
    }

    private sealed class FakeScriptControllableActor : IScriptControllableActor
    {
        public FakeScriptControllableActor(Vector2 position)
        {
            Position = position;
        }

        public Vector2 Position { get; private set; }

        public Vector2 Center => Position + HalfFrame;

        public FacingDirection Facing { get; private set; } = FacingDirection.Down;

        public bool IsMoving { get; private set; }

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

        public void ClearMovementState()
        {
            IsMoving = false;
        }
    }
}