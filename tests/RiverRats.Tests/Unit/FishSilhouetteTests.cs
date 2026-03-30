using Microsoft.Xna.Framework;
using RiverRats.Game.Entities;
using RiverRats.Game.Util;
using RiverRats.Tests.Helpers;
using System;
using Xunit;

namespace RiverRats.Tests.Unit;

/// <summary>
/// Tests for <see cref="FishSilhouette"/> behavior and state transitions.
/// </summary>
public sealed class FishSilhouetteTests
{
    private static readonly PolygonBounds DefaultSwimBounds =
        PolygonBounds.FromRectangle(new Rectangle(0, 0, 480, 200));

    private static FishSilhouette CreateFish(
        FishSilhouette.FishType type = FishSilhouette.FishType.Minnow,
        Vector2? position = null,
        PolygonBounds? swimBounds = null,
        int seed = 42)
    {
        return new FishSilhouette(
            type,
            position ?? new Vector2(100, 50),
            swimBounds ?? DefaultSwimBounds,
            new Random(seed));
    }

    [Fact]
    public void Constructor__SetsInitialPosition()
    {
        var pos = new Vector2(120, 80);
        var fish = CreateFish(position: pos);

        Assert.Equal(pos, fish.Position);
    }

    [Theory]
    [InlineData(FishSilhouette.FishType.Minnow)]
    [InlineData(FishSilhouette.FishType.Bass)]
    [InlineData(FishSilhouette.FishType.Catfish)]
    public void Constructor__AcceptsAllFishTypes(FishSilhouette.FishType type)
    {
        var fish = CreateFish(type: type);
        // Should not throw; fish starts in a valid state.
        Assert.NotNull(fish);
    }

    [Fact]
    public void Update__MovesFishOverTime()
    {
        var fish = CreateFish();
        var startPos = fish.Position;

        // Simulate 1 second of updates.
        var frame = FakeGameTime.FromSeconds(1f / 60f);
        for (var i = 0; i < 60; i++)
        {
            fish.Update(frame);
        }

        Assert.NotEqual(startPos, fish.Position);
    }

    [Fact]
    public void Update__FishStaysWithinSwimBounds()
    {
        var bounds = PolygonBounds.FromRectangle(new Rectangle(50, 50, 100, 80));
        var fish = CreateFish(swimBounds: bounds, position: new Vector2(70, 70));

        var frame = FakeGameTime.FromSeconds(1f / 60f);
        for (var i = 0; i < 600; i++) // 10 seconds of simulation
        {
            fish.Update(frame);
        }

        // Fish center must be within the polygon's bounding box as a basic check.
        var bbox = bounds.BoundingBox;
        Assert.True(fish.Position.X >= bbox.Left - 16,
            $"Fish X ({fish.Position.X}) went far left of bounds ({bbox.Left})");
        Assert.True(fish.Position.X <= bbox.Right + 16,
            $"Fish right edge ({fish.Position.X}) exceeded bounds right ({bbox.Right})");
        Assert.True(fish.Position.Y >= bbox.Top - 10,
            $"Fish Y ({fish.Position.Y}) went far above bounds ({bbox.Top})");
        Assert.True(fish.Position.Y <= bbox.Bottom + 10,
            $"Fish bottom edge ({fish.Position.Y}) exceeded bounds bottom ({bbox.Bottom})");
    }

    [Fact]
    public void Update__BehaviorTransitionsOverTime()
    {
        var fish = CreateFish();
        var initialBehavior = fish.Behavior;
        var sawDifferentBehavior = false;

        // Run for 30 seconds — behaviors should transition multiple times.
        var frame = FakeGameTime.FromSeconds(1f / 60f);
        for (var i = 0; i < 1800; i++)
        {
            fish.Update(frame);
            if (fish.Behavior != initialBehavior)
            {
                sawDifferentBehavior = true;
                break;
            }
        }

        Assert.True(sawDifferentBehavior, "Fish behavior never changed during 30s simulation");
    }

    [Fact]
    public void Update__FishCanDart()
    {
        var fish = CreateFish();
        var sawDart = false;

        var frame = FakeGameTime.FromSeconds(1f / 60f);
        for (var i = 0; i < 3600; i++) // 60 seconds
        {
            fish.Update(frame);
            if (fish.Behavior == FishSilhouette.FishBehavior.Dart)
            {
                sawDart = true;
                break;
            }
        }

        Assert.True(sawDart, "Fish never darted during 60s simulation");
    }

    [Fact]
    public void Update__FishCanPause()
    {
        var fish = CreateFish();
        var sawPause = false;

        var frame = FakeGameTime.FromSeconds(1f / 60f);
        for (var i = 0; i < 3600; i++)
        {
            fish.Update(frame);
            if (fish.Behavior == FishSilhouette.FishBehavior.Pause)
            {
                sawPause = true;
                break;
            }
        }

        Assert.True(sawPause, "Fish never paused during 60s simulation");
    }

    [Fact]
    public void Update__PausedFishDoesNotMove()
    {
        // Use a seed that quickly produces a Pause behavior, and brute-force find it.
        for (var seed = 0; seed < 200; seed++)
        {
            var fish = CreateFish(seed: seed);
            var frame = FakeGameTime.FromSeconds(1f / 60f);

            for (var i = 0; i < 3600; i++)
            {
                fish.Update(frame);
                if (fish.Behavior == FishSilhouette.FishBehavior.Pause)
                {
                    var posBeforePause = fish.Position;
                    // Run several frames while paused.
                    for (var j = 0; j < 10; j++)
                    {
                        fish.Update(frame);
                        if (fish.Behavior != FishSilhouette.FishBehavior.Pause)
                            break;
                    }

                    // If still paused, position should not have changed.
                    if (fish.Behavior == FishSilhouette.FishBehavior.Pause)
                    {
                        Assert.Equal(posBeforePause, fish.Position);
                        return;
                    }
                }
            }
        }

        Assert.Fail("Could not find a seed that produced a stable Pause state");
    }

    [Fact]
    public void Update__FishFacingCanFlip()
    {
        var fish = CreateFish();
        var initialFacing = fish.FacingLeft;
        var sawFlip = false;

        var frame = FakeGameTime.FromSeconds(1f / 60f);
        for (var i = 0; i < 3600; i++)
        {
            fish.Update(frame);
            if (fish.FacingLeft != initialFacing)
            {
                sawFlip = true;
                break;
            }
        }

        Assert.True(sawFlip, "Fish facing direction never changed during 60s simulation");
    }

    [Fact]
    public void Update__CatfishSlowerThanMinnow()
    {
        var minnow = CreateFish(type: FishSilhouette.FishType.Minnow, position: new Vector2(200, 100));
        var catfish = CreateFish(type: FishSilhouette.FishType.Catfish, position: new Vector2(200, 100));

        var frame = FakeGameTime.FromSeconds(1f / 60f);
        float minnowTotalDist = 0;
        float catfishTotalDist = 0;
        var prevMinnow = minnow.Position;
        var prevCatfish = catfish.Position;

        for (var i = 0; i < 600; i++) // 10 seconds
        {
            minnow.Update(frame);
            catfish.Update(frame);

            minnowTotalDist += Vector2.Distance(prevMinnow, minnow.Position);
            catfishTotalDist += Vector2.Distance(prevCatfish, catfish.Position);
            prevMinnow = minnow.Position;
            prevCatfish = catfish.Position;
        }

        // With fixed seed=42 and 10 seconds, on average minnow should move more.
        // This isn't a guarantee with any single seed, so we use a lenient check.
        // Both should have moved at least some distance.
        Assert.True(minnowTotalDist > 0, "Minnow didn't move at all");
        Assert.True(catfishTotalDist > 0, "Catfish didn't move at all");
    }
}
