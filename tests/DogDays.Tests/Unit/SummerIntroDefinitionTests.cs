using Microsoft.Xna.Framework;
using DogDays.Game.Data;
using DogDays.Game.Systems;

namespace DogDays.Tests.Unit;

/// <summary>
/// Unit tests for authored summer-intro rule helpers.
/// </summary>
public sealed class SummerIntroDefinitionTests
{
    [Fact]
    public void ShouldBlockOutdoorExit__WhenIntroIncompleteAndTriggerLeadsOutside__ReturnsTrue()
    {
        var trigger = new ZoneTriggerData(
            new Rectangle(0, 0, 32, 8),
            SummerIntroDefinition.OutdoorMapAssetName,
            "from-cabin-indoors");

        var result = SummerIntroDefinition.ShouldBlockOutdoorExit(
            SummerIntroDefinition.IndoorMapAssetName,
            isIntroComplete: false,
            trigger);

        Assert.True(result);
    }

    [Fact]
    public void ShouldBlockOutdoorExit__WhenIntroComplete__ReturnsFalse()
    {
        var trigger = new ZoneTriggerData(
            new Rectangle(0, 0, 32, 8),
            SummerIntroDefinition.OutdoorMapAssetName,
            "from-cabin-indoors");

        var result = SummerIntroDefinition.ShouldBlockOutdoorExit(
            SummerIntroDefinition.IndoorMapAssetName,
            isIntroComplete: true,
            trigger);

        Assert.False(result);
    }

    [Fact]
    public void ShouldBlockOutdoorExit__WhenTriggerStaysInsideHouse__ReturnsFalse()
    {
        var trigger = new ZoneTriggerData(
            new Rectangle(0, 0, 32, 8),
            "Maps/CabinBedroom",
            "from-cabin-indoors");

        var result = SummerIntroDefinition.ShouldBlockOutdoorExit(
            SummerIntroDefinition.IndoorMapAssetName,
            isIntroComplete: false,
            trigger);

        Assert.False(result);
    }
}