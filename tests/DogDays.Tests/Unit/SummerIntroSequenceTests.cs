using Microsoft.Xna.Framework;
using DogDays.Game.Data;
using DogDays.Game.Entities;
using DogDays.Game.Systems;
using DogDays.Tests.Helpers;

namespace DogDays.Tests.Unit;

/// <summary>
/// Unit tests for the authored opening summer intro sequence.
/// </summary>
public sealed class SummerIntroSequenceTests
{
    private const int FrameSize = 32;
    private static readonly Rectangle WorldBounds = new(0, 0, 512, 512);

    private static PlayerBlock CreatePlayer(Vector2 position)
    {
        return new PlayerBlock(position, new Point(FrameSize, FrameSize), 96f, WorldBounds);
    }

    private static FollowerBlock CreateFollower(Vector2 position)
    {
        return new FollowerBlock(position, new Point(FrameSize, FrameSize), WorldBounds);
    }

    [Fact]
    public void BeginMomConversation__AfterWalkIn__QueuesCompanionArrivalDialog()
    {
        var sequence = new SummerIntroSequence();
        var player = CreatePlayer(new Vector2(96f, 96f));
        var follower = CreateFollower(new Vector2(180f, 180f));
        var companionTarget = new Vector2(120f, 120f);
        var playerDoorTarget = new Vector2(198f, 53f);
        var companionDoorTarget = playerDoorTarget + new Vector2(-22f, 10f);

        sequence.BeginMomConversation(companionTarget, playerDoorTarget, companionDoorTarget);

        var momDialog = sequence.ConsumePendingDialog();
        Assert.NotNull(momDialog);
        Assert.Equal("Mom", momDialog!.Lines[0].SpeakerName);

        sequence.NotifyDialogDismissed();

        DialogScript? companionDialog = null;
        for (var i = 0; i < 240 && companionDialog is null; i++)
        {
            sequence.Update(FakeGameTime.OneFrame(), player, follower);
            companionDialog = sequence.ConsumePendingDialog();
        }

        Assert.NotNull(companionDialog);
        Assert.Equal("Companion", companionDialog!.Lines[0].SpeakerName);
        Assert.InRange(Vector2.Distance(follower.Position, companionTarget), 0f, 1f);
        Assert.Equal(SummerIntroSequenceState.AwaitingCompanionDialog, sequence.State);
    }

    [Fact]
    public void NotifyDialogDismissed__AfterCompanionDialog__WalksToBedroomDoorBeforeTransition()
    {
        var sequence = new SummerIntroSequence();
        var player = CreatePlayer(new Vector2(96f, 96f));
        var follower = CreateFollower(new Vector2(120f, 120f));
        var playerDoorTarget = new Vector2(198f, 53f);
        var companionDoorTarget = playerDoorTarget + new Vector2(-22f, 10f);

        sequence.BeginMomConversation(follower.Position, playerDoorTarget, companionDoorTarget);
        sequence.ConsumePendingDialog();
        sequence.NotifyDialogDismissed();
        sequence.Update(FakeGameTime.FromSeconds(0f), player, follower);
        sequence.ConsumePendingDialog();

        sequence.NotifyDialogDismissed();

        Assert.Equal(SummerIntroSequenceState.WalkingToBedroomDoor, sequence.State);
        Assert.False(sequence.ConsumePendingTransition().HasValue);

        SummerIntroTransition? transition = null;
        for (var i = 0; i < 240 && !transition.HasValue; i++)
        {
            sequence.Update(FakeGameTime.OneFrame(), player, follower);
            transition = sequence.ConsumePendingTransition();
        }

        Assert.True(transition.HasValue);
        Assert.InRange(Vector2.Distance(player.Position, playerDoorTarget), 0f, 1f);
        Assert.InRange(Vector2.Distance(follower.Position, companionDoorTarget), 0f, 1f);
        Assert.Equal("Maps/CabinBedroom", transition.Value.MapAssetName);
        Assert.Equal("from-cabin-indoors", transition.Value.SpawnPointId);
        Assert.Equal(SummerIntroStartState.BedroomReturn, transition.Value.StartState);
        Assert.Equal(SummerIntroSequenceState.TransitionToBedroom, sequence.State);
    }

    [Fact]
    public void Update__WhenPlayerReachesBedroomDoorFirst__StopsPlayerWalkingAnimation()
    {
        var sequence = new SummerIntroSequence();
        var playerDoorTarget = new Vector2(198f, 53f);
        var companionDoorTarget = playerDoorTarget + new Vector2(-22f, 10f);
        var player = CreatePlayer(playerDoorTarget);
        var follower = CreateFollower(new Vector2(120f, 120f));

        player.ApplyScriptedMovement(new Vector2(0.1f, 0f));
        sequence.BeginMomConversation(follower.Position, playerDoorTarget, companionDoorTarget);
        sequence.ConsumePendingDialog();
        sequence.NotifyDialogDismissed();
        sequence.Update(FakeGameTime.FromSeconds(0f), player, follower);
        sequence.ConsumePendingDialog();
        sequence.NotifyDialogDismissed();

        sequence.Update(FakeGameTime.OneFrame(), player, follower);

        Assert.Equal(SummerIntroSequenceState.WalkingToBedroomDoor, sequence.State);
        Assert.False(player.IsMoving);
        Assert.True(follower.IsMoving);
    }

    [Fact]
    public void BeginBedroomReturn__AfterBedroomChat__RequestsCompletion()
    {
        var sequence = new SummerIntroSequence();
        var player = CreatePlayer(new Vector2(12f, 140f));
        var follower = CreateFollower(new Vector2(-10f, 150f));

        sequence.BeginBedroomReturn();

        DialogScript? bedroomDialog = null;
        for (var i = 0; i < 240 && bedroomDialog is null; i++)
        {
            sequence.Update(FakeGameTime.OneFrame(), player, follower);
            bedroomDialog = sequence.ConsumePendingDialog();
        }

        Assert.NotNull(bedroomDialog);
        Assert.Equal("Player", bedroomDialog!.Lines[0].SpeakerName);

        sequence.NotifyDialogDismissed();

        Assert.True(sequence.ConsumeCompletionRequested());
        Assert.False(sequence.IsActive);
        Assert.Equal(SummerIntroSequenceState.Completed, sequence.State);
    }
}