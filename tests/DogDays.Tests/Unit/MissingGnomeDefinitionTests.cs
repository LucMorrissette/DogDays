using Microsoft.Xna.Framework;
using DogDays.Game.Core;
using DogDays.Game.Data;
using DogDays.Game.Systems;

namespace DogDays.Tests.Unit;

/// <summary>
/// Unit tests for the missing-gnome exit-trigger rule.
/// </summary>
public sealed class MissingGnomeDefinitionTests
{
    [Fact]
    public void IsOutdoorExitTrigger__WhenCottageDoorLeadsOutside__ReturnsTrue()
    {
        var trigger = new ZoneTriggerData(
            new Rectangle(0, 0, 32, 8),
            SummerIntroDefinition.OutdoorMapAssetName,
            "from-cabin-indoors");

        var result = MissingGnomeDefinition.IsOutdoorExitTrigger(
            SummerIntroDefinition.IndoorMapAssetName,
            trigger);

        Assert.True(result);
    }

    [Fact]
    public void IsOutdoorExitTrigger__WhenTriggerDoesNotLeadOutside__ReturnsFalse()
    {
        var trigger = new ZoneTriggerData(
            new Rectangle(0, 0, 32, 8),
            MissingGnomeDefinition.ForestMapAssetName,
            "forest-start");

        var result = MissingGnomeDefinition.IsOutdoorExitTrigger(
            SummerIntroDefinition.IndoorMapAssetName,
            trigger);

        Assert.False(result);
    }

    [Fact]
    public void ShouldTriggerExitSequence__WhenQuestNotStartedAndTriggerLeadsOutside__ReturnsTrue()
    {
        var trigger = new ZoneTriggerData(
            new Rectangle(0, 0, 32, 8),
            SummerIntroDefinition.OutdoorMapAssetName,
            "from-cabin-indoors");
        var questState = new QuestState(new QuestDefinition
        {
            Id = MissingGnomeDefinition.QuestId,
            Title = "Search for Gnome Chompsky",
            Description = "Find the missing gnome.",
            Objectives =
            [
                new ObjectiveDefinition
                {
                    Id = "search",
                    Description = "Search for the missing gnome",
                    Completion = new QuestEventConditionDefinition { EventType = GameEventType.ZoneEntered, TargetId = MissingGnomeDefinition.ForestMapAssetName },
                },
            ],
        });

        var result = MissingGnomeDefinition.ShouldTriggerExitSequence(
            SummerIntroDefinition.IndoorMapAssetName,
            questState,
            trigger);

        Assert.True(result);
    }

    [Fact]
    public void ShouldTriggerExitSequence__WhenQuestAlreadyStarted__ReturnsFalse()
    {
        var trigger = new ZoneTriggerData(
            new Rectangle(0, 0, 32, 8),
            SummerIntroDefinition.OutdoorMapAssetName,
            "from-cabin-indoors");
        var questState = new QuestState(new QuestDefinition
        {
            Id = MissingGnomeDefinition.QuestId,
            Title = "Search for Gnome Chompsky",
            Description = "Find the missing gnome.",
            Objectives =
            [
                new ObjectiveDefinition
                {
                    Id = "search",
                    Description = "Search for the missing gnome",
                    Completion = new QuestEventConditionDefinition { EventType = GameEventType.ZoneEntered, TargetId = MissingGnomeDefinition.ForestMapAssetName },
                },
            ],
        });
        questState.Start();

        var result = MissingGnomeDefinition.ShouldTriggerExitSequence(
            SummerIntroDefinition.IndoorMapAssetName,
            questState,
            trigger);

        Assert.False(result);
    }
}