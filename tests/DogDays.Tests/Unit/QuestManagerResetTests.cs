using DogDays.Game.Core;
using DogDays.Game.Data;
using DogDays.Game.Systems;

namespace DogDays.Tests.Unit;

/// <summary>
/// Unit tests for quest-manager session reset behavior.
/// </summary>
public sealed class QuestManagerResetTests
{
    private static (GameEventBus EventBus, QuestManager Manager) CreateQuestManager()
    {
        var eventBus = new GameEventBus();
        var manager = new QuestManager(eventBus);
        manager.LoadDefinitions(
        [
            new QuestDefinition
            {
                Id = "quest_intro",
                Title = "Intro Quest",
                AutoStart = true,
                Objectives =
                [
                    new ObjectiveDefinition
                    {
                        Id = "beat-intro",
                        Description = "Beat the intro.",
                        Completion = new QuestEventConditionDefinition
                        {
                            EventType = GameEventType.EnemyKilled,
                            RequiredCount = 1,
                        },
                    },
                ],
            },
            new QuestDefinition
            {
                Id = "quest_side",
                Title = "Side Quest",
                StartCondition = new QuestEventConditionDefinition
                {
                    EventType = GameEventType.NpcTalkedTo,
                    TargetId = "mom",
                    RequiredCount = 1,
                },
                Objectives =
                [
                    new ObjectiveDefinition
                    {
                        Id = "catch-fish",
                        Description = "Catch a fish.",
                        Completion = new QuestEventConditionDefinition
                        {
                            EventType = GameEventType.FishCaught,
                            RequiredCount = 1,
                        },
                    },
                ],
            },
        ]);

        return (eventBus, manager);
    }

    [Fact]
    public void ResetProgress__AfterProgressExists__RestartsAutoStartQuestAndClearsTriggeredQuest()
    {
        var (eventBus, manager) = CreateQuestManager();

        eventBus.Publish(GameEventType.EnemyKilled, amount: 1);
        eventBus.Publish(GameEventType.NpcTalkedTo, "mom", 1);

        Assert.Equal(QuestStatus.Completed, manager.GetQuest("quest_intro")!.Status);
        Assert.Equal(QuestStatus.Active, manager.GetQuest("quest_side")!.Status);
        Assert.Equal(2, manager.AvailableQuests.Count);

        manager.ResetProgress();

        var introQuest = manager.GetQuest("quest_intro")!;
        var sideQuest = manager.GetQuest("quest_side")!;

        Assert.Equal(QuestStatus.Active, introQuest.Status);
        Assert.Equal(0, introQuest.CurrentObjectiveIndex);
        Assert.Equal(0, introQuest.GetObjectiveProgress(0));
        Assert.Equal(QuestStatus.NotStarted, sideQuest.Status);
        Assert.Single(manager.ActiveQuests);
        Assert.Single(manager.AvailableQuests);
        Assert.Same(introQuest, manager.TrackedQuest);
    }
}