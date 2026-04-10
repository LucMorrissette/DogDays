using Microsoft.Xna.Framework;
using DogDays.Game.Data;
using DogDays.Game.Entities;
using DogDays.Game.Systems;

namespace DogDays.Tests.Unit;

/// <summary>
/// Unit tests for scripted actor movement helpers used by authored sequences.
/// </summary>
public sealed class ScriptedActorMotionTests
{
    [Fact]
    public void MoveTowards__DoesNotTeleport_WhenElapsedTimeIsZero()
    {
        var actor = new FakeScriptControllableActor(new Vector2(10f, 10f));

        var arrived = ScriptedActorMotion.MoveTowards(actor, new Vector2(100f, 10f), 0f, 54f);

        Assert.False(arrived);
        Assert.Equal(new Vector2(10f, 10f), actor.Position);
        Assert.False(actor.IsMoving);
    }

    [Fact]
    public void MoveTowards__ClearsMovementState_WhenAlreadyAtTarget()
    {
        var actor = new FakeScriptControllableActor(new Vector2(42f, 18f));
        actor.ApplyScriptedMovement(new Vector2(1f, 0f));

        var arrived = ScriptedActorMotion.MoveTowards(actor, new Vector2(43f, 18f), 1f / 60f, 54f);

        Assert.True(arrived);
        Assert.Equal(new Vector2(43f, 18f), actor.Position);
        Assert.False(actor.IsMoving);
    }

    private sealed class FakeScriptControllableActor : IScriptControllableActor
    {
        public FakeScriptControllableActor(Vector2 position)
        {
            Position = position;
        }

        public Vector2 Position { get; private set; }

        public Vector2 Center => Position + new Vector2(16f, 16f);

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