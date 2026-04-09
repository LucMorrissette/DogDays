using Microsoft.Xna.Framework;
using DogDays.Game.Entities;

namespace DogDays.Tests.Unit;

public sealed class FrontDoorTests
{
    [Fact]
    public void Constructor__StartOpenFalse__StartsClosed()
    {
        var door = new FrontDoor(new Vector2(100f, 200f), new Point(20, 32));

        Assert.False(door.IsOpen);
    }

    [Fact]
    public void Bounds__ExplicitSizeProvided__UsesConfiguredDimensions()
    {
        var door = new FrontDoor(new Vector2(100f, 200f), new Point(27, 43));

        Assert.Equal(new Rectangle(100, 200, 27, 43), door.Bounds);
    }

    [Fact]
    public void UpdateInvitationState__PlayerWithinSixteenthTile__OpensDoor()
    {
        var door = new FrontDoor(new Vector2(100f, 200f), new Point(20, 32));
        var playerBounds = new Rectangle(122, 202, 32, 32);

        door.UpdateInvitationState(playerBounds, invitationDistancePixels: 2);

        Assert.True(door.IsOpen);
    }

    [Fact]
    public void UpdateInvitationState__FollowerWithinRange__DoesNotOpenDoor()
    {
        var door = new FrontDoor(new Vector2(100f, 200f), new Point(20, 32));
        var followerBounds = new Rectangle(124, 202, 32, 32);

        door.UpdateInvitationState(new Rectangle(400, 400, 32, 32), invitationDistancePixels: 2);

        Assert.False(door.IsOpen);
    }

    [Fact]
    public void UpdateInvitationState__PlayerFarAway__ClosesDoor()
    {
        var door = new FrontDoor(new Vector2(100f, 200f), new Point(20, 32), startOpen: true);

        door.UpdateInvitationState(new Rectangle(300, 300, 32, 32), invitationDistancePixels: 2);

        Assert.False(door.IsOpen);
    }

    [Fact]
    public void UpdateInvitationState__LockedDoorWithinRange__StaysClosedAndQueuesFeedback()
    {
        var door = new FrontDoor(new Vector2(100f, 200f), new Point(20, 32), isLocked: true);
        var playerBounds = new Rectangle(122, 202, 32, 32);

        door.UpdateInvitationState(playerBounds, invitationDistancePixels: 2, elapsedSeconds: 0.016f);

        Assert.True(door.IsLocked);
        Assert.False(door.IsOpen);
        Assert.True(door.IsLockedFeedbackActive);
        Assert.True(door.ConsumeLockedFeedbackRequest());
        Assert.False(door.ConsumeLockedFeedbackRequest());
    }

    [Fact]
    public void UpdateInvitationState__LockedDoorLeavesAndReEntersRange__QueuesFeedbackAgain()
    {
        var door = new FrontDoor(new Vector2(100f, 200f), new Point(20, 32), isLocked: true);
        var nearBounds = new Rectangle(122, 202, 32, 32);
        var farBounds = new Rectangle(300, 300, 32, 32);

        door.UpdateInvitationState(nearBounds, invitationDistancePixels: 2, elapsedSeconds: 0.016f);
        Assert.True(door.ConsumeLockedFeedbackRequest());

        door.UpdateInvitationState(nearBounds, invitationDistancePixels: 2, elapsedSeconds: 0.016f);
        Assert.False(door.ConsumeLockedFeedbackRequest());

        door.UpdateInvitationState(farBounds, invitationDistancePixels: 2, elapsedSeconds: 0.016f);
        door.UpdateInvitationState(nearBounds, invitationDistancePixels: 2, elapsedSeconds: 0.016f);

        Assert.True(door.ConsumeLockedFeedbackRequest());
    }
}