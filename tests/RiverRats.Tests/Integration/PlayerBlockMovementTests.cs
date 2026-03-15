using Microsoft.Xna.Framework;
using RiverRats.Game.Entities;
using RiverRats.Game.Input;
using RiverRats.Game.World;
using RiverRats.Tests.Helpers;

namespace RiverRats.Tests.Integration;

/// <summary>
/// Integration-style movement tests that simulate frame updates with scripted input.
/// </summary>
public class PlayerBlockMovementTests
{
    private static readonly IMapCollisionData NoBlockedTiles = new NoCollisionData();

    [Fact]
    public void Update__HeldMoveRightAcrossThreeFrames__AccumulatesPosition()
    {
        var input = new FakeInputManager();
        var player = new PlayerBlock(
            startPosition: new Vector2(0f, 0f),
            size: new Point(32, 32),
            moveSpeedPixelsPerSecond: 120f,
            worldBounds: new Rectangle(0, 0, 1024, 640),
            color: Color.CornflowerBlue);

        input.Press(InputAction.MoveRight);

        player.Update(FakeGameTime.OneFrame(), input, NoBlockedTiles);
        input.Update();
        player.Update(FakeGameTime.OneFrame(), input, NoBlockedTiles);
        input.Update();
        player.Update(FakeGameTime.OneFrame(), input, NoBlockedTiles);

        Assert.Equal(6f, player.Position.X, 4);
        Assert.Equal(0f, player.Position.Y);
    }

    [Fact]
    public void Update__ReleaseMovementAfterOneFrame__StopsFurtherMotion()
    {
        var input = new FakeInputManager();
        var player = new PlayerBlock(
            startPosition: new Vector2(0f, 0f),
            size: new Point(32, 32),
            moveSpeedPixelsPerSecond: 120f,
            worldBounds: new Rectangle(0, 0, 1024, 640),
            color: Color.CornflowerBlue);

        input.Press(InputAction.MoveRight);
        player.Update(FakeGameTime.OneFrame(), input, NoBlockedTiles);

        input.Release(InputAction.MoveRight);
        input.Update();
        player.Update(FakeGameTime.OneFrame(), input, NoBlockedTiles);

        Assert.Equal(2f, player.Position.X, 4);
        Assert.Equal(0f, player.Position.Y);
    }

    private sealed class NoCollisionData : IMapCollisionData
    {
        public bool IsWorldRectangleBlocked(Rectangle worldBounds)
        {
            _ = worldBounds;
            return false;
        }
    }
}
