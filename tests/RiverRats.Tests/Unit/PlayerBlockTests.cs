using Microsoft.Xna.Framework;
using RiverRats.Game.Entities;
using RiverRats.Game.Input;
using RiverRats.Game.World;
using RiverRats.Tests.Helpers;

namespace RiverRats.Tests.Unit;

/// <summary>
/// Unit tests for PlayerBlock movement math and world-bound clamping.
/// </summary>
public class PlayerBlockTests
{
    private static readonly IMapCollisionData NoBlockedTiles = new DelegateCollisionData(_ => false);

    [Fact]
    public void Update__MoveRightForOneSecond__AdvancesByConfiguredSpeed()
    {
        var input = new FakeInputManager();
        var player = new PlayerBlock(
            startPosition: new Vector2(100f, 100f),
            size: new Point(32, 32),
            moveSpeedPixelsPerSecond: 120f,
            worldBounds: new Rectangle(0, 0, 1024, 640),
            color: Color.CornflowerBlue);

        input.Press(InputAction.MoveRight);

        player.Update(FakeGameTime.FromSeconds(1f), input, NoBlockedTiles);

        Assert.Equal(220f, player.Position.X);
        Assert.Equal(100f, player.Position.Y);
    }

    [Fact]
    public void Update__MoveDiagonallyForOneSecond__NormalizesSpeed()
    {
        var input = new FakeInputManager();
        var player = new PlayerBlock(
            startPosition: new Vector2(100f, 100f),
            size: new Point(32, 32),
            moveSpeedPixelsPerSecond: 120f,
            worldBounds: new Rectangle(0, 0, 1024, 640),
            color: Color.CornflowerBlue);

        input.Press(InputAction.MoveRight);
        input.Press(InputAction.MoveDown);

        player.Update(FakeGameTime.FromSeconds(1f), input, NoBlockedTiles);

        Assert.Equal(184.85281f, player.Position.X, 4);
        Assert.Equal(184.85281f, player.Position.Y, 4);
    }

    [Fact]
    public void Update__MovePastRightEdge__ClampsToWorldBounds()
    {
        var input = new FakeInputManager();
        var player = new PlayerBlock(
            startPosition: new Vector2(980f, 100f),
            size: new Point(32, 32),
            moveSpeedPixelsPerSecond: 120f,
            worldBounds: new Rectangle(0, 0, 1024, 640),
            color: Color.CornflowerBlue);

        input.Press(InputAction.MoveRight);

        player.Update(FakeGameTime.FromSeconds(1f), input, NoBlockedTiles);

        Assert.Equal(992f, player.Position.X);
        Assert.Equal(100f, player.Position.Y);
    }

    [Fact]
    public void Center__FromPositionAndSize__ReturnsMidpoint()
    {
        var player = new PlayerBlock(
            startPosition: new Vector2(100f, 120f),
            size: new Point(32, 32),
            moveSpeedPixelsPerSecond: 120f,
            worldBounds: new Rectangle(0, 0, 1024, 640),
            color: Color.CornflowerBlue);

        Assert.Equal(new Vector2(116f, 136f), player.Center);
    }

    [Fact]
    public void Update__MoveIntoBlockedTile__DoesNotEnterBlockedArea()
    {
        var input = new FakeInputManager();
        var blockedRegion = new Rectangle(64, 0, 32, 32);
        var collisionData = new DelegateCollisionData(bounds => bounds.Intersects(blockedRegion));
        var player = new PlayerBlock(
            startPosition: new Vector2(32f, 0f),
            size: new Point(32, 32),
            moveSpeedPixelsPerSecond: 64f,
            worldBounds: new Rectangle(0, 0, 256, 256),
            color: Color.CornflowerBlue);

        input.Press(InputAction.MoveRight);
        player.Update(FakeGameTime.FromSeconds(1f), input, collisionData);

        Assert.Equal(32f, player.Position.X);
        Assert.Equal(0f, player.Position.Y);
    }

    private sealed class DelegateCollisionData : IMapCollisionData
    {
        private readonly Func<Rectangle, bool> _isBlocked;

        public DelegateCollisionData(Func<Rectangle, bool> isBlocked)
        {
            _isBlocked = isBlocked;
        }

        public bool IsWorldRectangleBlocked(Rectangle worldBounds)
        {
            return _isBlocked(worldBounds);
        }
    }
}
