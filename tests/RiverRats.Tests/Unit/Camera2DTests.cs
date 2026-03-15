using Microsoft.Xna.Framework;
using RiverRats.Game.Graphics;
using Xunit;

namespace RiverRats.Tests.Unit;

/// <summary>
/// Unit tests for <see cref="Camera2D"/> clamping behaviour and view matrix output.
/// No GraphicsDevice is required — Camera2D performs pure math on XNA value types.
/// </summary>
public class Camera2DTests
{
    // Baseline dimensions: 32×20 tile map at 32px tiles → 1024×640 pixels.
    // Virtual viewport: 960×540.
    // Expected clamp ranges:
    //   X: [halfViewW=480, mapW-halfViewW=544]
    //   Y: [halfViewH=270, mapH-halfViewH=370]
    private const int ViewW = 960;
    private const int ViewH = 540;
    private const int MapW = 1024;
    private const int MapH = 640;

    [Fact]
    public void Position__WhenCreated__StartsAtMinClampedPosition()
    {
        var camera = new Camera2D(ViewW, ViewH, MapW, MapH);
        Assert.Equal(new Vector2(480f, 270f), camera.Position);
    }

    [Fact]
    public void LookAt__PositionWithinBounds__SetsPositionExactly()
    {
        var camera = new Camera2D(ViewW, ViewH, MapW, MapH);
        camera.LookAt(new Vector2(512f, 320f));
        Assert.Equal(new Vector2(512f, 320f), camera.Position);
    }

    [Fact]
    public void LookAt__TooFarLeft__ClampsToMinX()
    {
        var camera = new Camera2D(ViewW, ViewH, MapW, MapH);
        camera.LookAt(new Vector2(0f, 320f));
        Assert.Equal(480f, camera.Position.X);
    }

    [Fact]
    public void LookAt__TooFarRight__ClampsToMaxX()
    {
        var camera = new Camera2D(ViewW, ViewH, MapW, MapH);
        camera.LookAt(new Vector2(9999f, 320f));
        Assert.Equal(544f, camera.Position.X); // 1024 - 480
    }

    [Fact]
    public void LookAt__TooFarUp__ClampsToMinY()
    {
        var camera = new Camera2D(ViewW, ViewH, MapW, MapH);
        camera.LookAt(new Vector2(512f, 0f));
        Assert.Equal(270f, camera.Position.Y);
    }

    [Fact]
    public void LookAt__TooFarDown__ClampsToMaxY()
    {
        var camera = new Camera2D(ViewW, ViewH, MapW, MapH);
        camera.LookAt(new Vector2(512f, 9999f));
        Assert.Equal(370f, camera.Position.Y); // 640 - 270
    }

    [Fact]
    public void LookAt__MapNarrowerThanViewport__LocksXToMapCentre()
    {
        // Map 500 px wide < viewport 960 px: X should be locked to 250 regardless of input.
        var camera = new Camera2D(ViewW, ViewH, 500, MapH);
        camera.LookAt(new Vector2(0f, 320f));
        Assert.Equal(250f, camera.Position.X); // 500 / 2
    }

    [Fact]
    public void LookAt__MapShorterThanViewport__LocksYToMapCentre()
    {
        // Map 400 px tall < viewport 540 px: Y should be locked to 200 regardless of input.
        var camera = new Camera2D(ViewW, ViewH, MapW, 400);
        camera.LookAt(new Vector2(512f, 9999f));
        Assert.Equal(200f, camera.Position.Y); // 400 / 2
    }

    [Fact]
    public void GetViewMatrix__TranslatesCameraPositionToViewportCentre()
    {
        var camera = new Camera2D(ViewW, ViewH, MapW, MapH);
        camera.LookAt(new Vector2(512f, 320f));
        var matrix = camera.GetViewMatrix();

        // Expected: viewport_centre − camera_position
        // X: 960/2 − 512 = 480 − 512 = −32
        // Y: 540/2 − 320 = 270 − 320 = −50
        Assert.Equal(-32f, matrix.M41);
        Assert.Equal(-50f, matrix.M42);
    }

    [Fact]
    public void GetViewMatrix__CameraAtMapCentre__TranslationCentresMap()
    {
        var camera = new Camera2D(ViewW, ViewH, MapW, MapH);
        camera.LookAt(new Vector2(MapW / 2f, MapH / 2f));
        var matrix = camera.GetViewMatrix();

        // Camera centred at (512, 320):
        // X: 480 − 512 = −32, Y: 270 − 320 = −50
        Assert.Equal(-32f, matrix.M41);
        Assert.Equal(-50f, matrix.M42);
    }
}
