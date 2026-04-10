using Microsoft.Xna.Framework;
using DogDays.Game.Entities;

namespace DogDays.Tests.Unit;

public sealed class GardenShedTests
{
    [Fact]
    public void Constructor__LockedShed__StartsClosedAndLocked()
    {
        var shed = new GardenShed(new Vector2(100f, 200f), new Point(64, 64), isLocked: true);

        Assert.True(shed.IsLocked);
        Assert.False(shed.IsDoorOpen);
    }

    [Fact]
    public void Constructor__StartOpenFalse__StartsClosed()
    {
        var shed = new GardenShed(new Vector2(100f, 200f), new Point(64, 64));

        Assert.False(shed.IsDoorOpen);
    }

    [Fact]
    public void Constructor__StartOpenTrue__StartsOpen()
    {
        var shed = new GardenShed(new Vector2(100f, 200f), new Point(64, 64), startOpen: true);

        Assert.True(shed.IsDoorOpen);
    }

    [Fact]
    public void UpdateDoorState__PlayerFootBoundsOnRamp__OpensDoor()
    {
        var shed = new GardenShed(new Vector2(100f, 200f), new Point(64, 64));

        shed.UpdateDoorState(new Rectangle(126, 252, 8, 8));

        Assert.True(shed.IsDoorOpen);
    }

    [Fact]
    public void UpdateDoorState__PlayerFootBoundsOffRamp__ClosesDoor()
    {
        var shed = new GardenShed(new Vector2(100f, 200f), new Point(64, 64), startOpen: true);

        shed.UpdateDoorState(new Rectangle(104, 206, 8, 8));

        Assert.False(shed.IsDoorOpen);
    }

    [Fact]
    public void UpdateDoorState__LockedShedOnRamp__StaysClosedAndQueuesFeedbackOnce()
    {
        var shed = new GardenShed(new Vector2(100f, 200f), new Point(64, 64), isLocked: true);

        shed.UpdateDoorState(new Rectangle(126, 252, 8, 8));

        Assert.False(shed.IsDoorOpen);
        Assert.True(shed.ConsumeLockedFeedbackRequest());
        Assert.False(shed.ConsumeLockedFeedbackRequest());

        shed.UpdateDoorState(new Rectangle(126, 252, 8, 8));

        Assert.False(shed.ConsumeLockedFeedbackRequest());
    }

    [Fact]
    public void SetLocked__AfterUnlocking__AllowsRampToOpenDoor()
    {
        var shed = new GardenShed(new Vector2(100f, 200f), new Point(64, 64), isLocked: true);

        shed.SetLocked(false);
        shed.UpdateDoorState(new Rectangle(126, 252, 8, 8));

        Assert.False(shed.IsLocked);
        Assert.True(shed.IsDoorOpen);
    }
}
