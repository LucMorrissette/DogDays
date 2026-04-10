using Microsoft.Xna.Framework;
using DogDays.Game.Data;
using DogDays.Game.Entities;
using DogDays.Game.Systems;
using DogDays.Game.World;
using DogDays.Tests.Helpers;

namespace DogDays.Tests.Unit;

/// <summary>
/// Unit tests for the cottage-exit missing-gnome sequence.
/// </summary>
public sealed class MissingGnomeSequenceTests
{
    [Fact]
    public void Update__AfterMomArrives__QueuesMissingGnomeDialog()
    {
        var sequence = new MissingGnomeSequence();
        var mom = new FakeScriptControllableNpc(new Vector2(220f, 96f));
        var player = new FakeScriptControllableActor(new Vector2(200f, 180f));
        var follower = new FakeScriptControllableActor(new Vector2(232f, 184f));
        var momTarget = new Vector2(160f, 144f);

        sequence.Begin(momTarget, momRoute: null);

        DialogScript? dialog = null;
        for (var i = 0; i < 240 && dialog is null; i++)
        {
            sequence.Update(FakeGameTime.OneFrame(), mom, player, follower, collisionData: null);
            dialog = sequence.ConsumePendingDialog();
        }

        Assert.NotNull(dialog);
        Assert.Equal("Mom", dialog!.Lines[0].SpeakerName);
        Assert.InRange(Vector2.Distance(mom.Position, momTarget), 0f, 1f);
        Assert.Equal(MissingGnomeSequenceState.AwaitingDialog, sequence.State);
        Assert.False(player.IsMoving);
        Assert.False(follower.IsMoving);
    }

    [Fact]
    public void NotifyDialogDismissed__AfterDialog__RequestsCompletion()
    {
        var sequence = new MissingGnomeSequence();
        var mom = new FakeScriptControllableNpc(new Vector2(160f, 144f));
        var player = new FakeScriptControllableActor(new Vector2(200f, 180f));
        var follower = new FakeScriptControllableActor(new Vector2(232f, 184f));

        sequence.Begin(mom.Position, momRoute: null);
        sequence.Update(FakeGameTime.FromSeconds(0f), mom, player, follower, collisionData: null);
        sequence.ConsumePendingDialog();

        sequence.NotifyDialogDismissed();

        Assert.True(sequence.ConsumeCompletionRequested());
        Assert.False(sequence.IsActive);
        Assert.Equal(MissingGnomeSequenceState.Completed, sequence.State);
    }

    private class FakeScriptControllableActor : IScriptControllableActor
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

    private sealed class FakeScriptControllableNpc : FakeScriptControllableActor, IScriptControllableNpc
    {
        public FakeScriptControllableNpc(Vector2 position)
            : base(position)
        {
        }

        public bool AutonomousBehaviorEnabled { get; private set; } = true;

        public Vector2 NavigationPosition => Position + new Vector2(16f, 16f);

        public void SetAutonomousBehaviorEnabled(bool isEnabled)
        {
            AutonomousBehaviorEnabled = isEnabled;
        }

        public bool ApplyScriptedMovement(Vector2 movementDelta, IMapCollisionData? collisionData)
        {
            return ApplyScriptedMovement(movementDelta);
        }
    }
}