using DogDays.Game.Core;
using DogDays.Game.Data;
using DogDays.Game.Systems;

namespace DogDays.Tests.Unit;

public class QuestManagerTests
{
    [Fact]
    public void LoadDefinitions__AutoStartsMarkedQuests__AndLeavesOthersNotStarted()
    {
        var (_, manager) = CreateManager(
            CreateQuest(
                "auto-start",
                [CreateObjective("reach-starter-map", GameEventType.ZoneEntered, "Maps/StarterMap")],
                autoStart: true),
            CreateQuest(
                "later",
                [CreateObjective("talk-to-mom", GameEventType.NpcTalkedTo, "mom")],
                startCondition: CreateCondition(GameEventType.NpcTalkedTo, "mom")));

        Assert.Equal(QuestStatus.Active, manager.GetQuest("auto-start")!.Status);
        Assert.Equal(QuestStatus.NotStarted, manager.GetQuest("later")!.Status);
        Assert.Single(manager.ActiveQuests);
    }

    [Fact]
    public void Publish__StartsTriggeredQuest__WhenStartConditionMatches()
    {
        var (bus, manager) = CreateManager(
            CreateQuest(
                "moms-warning",
                [CreateObjective("enter-woods", GameEventType.ZoneEntered, "Maps/WoodsBehindCabin")],
                startCondition: CreateCondition(GameEventType.NpcTalkedTo, "mom")));

        bus.Publish(GameEventType.NpcTalkedTo, "mom", 1);

        Assert.Equal(QuestStatus.Active, manager.GetQuest("moms-warning")!.Status);
        Assert.Single(manager.ActiveQuests);
    }

    [Fact]
    public void Publish__StartsStoryBeatQuestAndCompletesItOnForestZoneEnter()
    {
        var (bus, manager) = CreateManager(
            CreateQuest(
                "search-for-gnome-chompsky",
                [CreateObjective("search-for-the-missing-gnome", GameEventType.ZoneEntered, "Maps/WoodsBehindCabin")],
                startCondition: CreateCondition(GameEventType.StoryBeatReached, "missing_gnome_warning")));

        bus.Publish(GameEventType.StoryBeatReached, "missing_gnome_warning", 1);

        var quest = manager.GetQuest("search-for-gnome-chompsky")!;
        Assert.Equal(QuestStatus.Active, quest.Status);
        Assert.Single(manager.ActiveQuests);

        bus.Publish(GameEventType.ZoneEntered, "Maps/WoodsBehindCabin", 1);

        Assert.Equal(QuestStatus.Completed, quest.Status);
        Assert.Empty(manager.ActiveQuests);
    }

    [Fact]
    public void Publish__SupportsMultipleActiveQuests__AtTheSameTime()
    {
        var (bus, manager) = CreateManager(
            CreateQuest(
                "warning-a",
                [CreateObjective("enter-woods-a", GameEventType.ZoneEntered, "Maps/WoodsBehindCabin")],
                startCondition: CreateCondition(GameEventType.NpcTalkedTo, "mom")),
            CreateQuest(
                "warning-b",
                [CreateObjective("talk-to-grandpa", GameEventType.NpcTalkedTo, "grandpa")],
                startCondition: CreateCondition(GameEventType.NpcTalkedTo, "mom")));

        bus.Publish(GameEventType.NpcTalkedTo, "mom", 1);

        Assert.Equal(2, manager.ActiveQuests.Count);
        Assert.Equal(QuestStatus.Active, manager.GetQuest("warning-a")!.Status);
        Assert.Equal(QuestStatus.Active, manager.GetQuest("warning-b")!.Status);
    }

    [Fact]
    public void Publish__UsesEventAmount__ToAdvanceCounterObjectives()
    {
        var (bus, manager) = CreateManager(
            CreateQuest(
                "clear-the-path",
                [CreateObjective("defeat-three-gnomes", GameEventType.EnemyKilled, requiredCount: 3)],
                autoStart: true));
        var quest = manager.GetQuest("clear-the-path")!;

        bus.Publish(GameEventType.EnemyKilled, amount: 2);

        Assert.Equal(QuestStatus.Active, quest.Status);
        Assert.Equal(2, quest.GetObjectiveProgress(0));
    }

    [Fact]
    public void Publish__CompletesQuest__WhenFinalObjectiveReachesRequiredCount()
    {
        var (bus, manager) = CreateManager(
            CreateQuest(
                "clear-the-path",
                [CreateObjective("defeat-three-gnomes", GameEventType.EnemyKilled, requiredCount: 3)],
                autoStart: true));
        var quest = manager.GetQuest("clear-the-path")!;
        var completedQuestId = string.Empty;

        manager.QuestCompleted += completedQuest => completedQuestId = completedQuest.Definition.Id;

        bus.Publish(GameEventType.EnemyKilled, amount: 3);

        Assert.Equal(QuestStatus.Completed, quest.Status);
        Assert.Equal("clear-the-path", completedQuestId);
        Assert.Empty(manager.ActiveQuests);
        Assert.Null(manager.TrackedQuest);
        Assert.Equal(3, quest.GetObjectiveProgress(0));
    }

    [Fact]
    public void LoadDefinitions__TracksFirstAvailableQuest__WhenAutoStartQuestExists()
    {
        var (_, manager) = CreateManager(
            CreateQuest(
                "meet-grandpa",
                [CreateObjective("talk-to-grandpa", GameEventType.NpcTalkedTo, "grandpa")],
                autoStart: true),
            CreateQuest(
                "later",
                [CreateObjective("enter-woods", GameEventType.ZoneEntered, "Maps/WoodsBehindCabin")],
                startCondition: CreateCondition(GameEventType.NpcTalkedTo, "mom")));

        Assert.Equal("meet-grandpa", manager.TrackedQuest!.Definition.Id);
        Assert.Single(manager.AvailableQuests);
    }

    [Fact]
    public void SetTrackedQuest__SwitchesTracking__ToAnotherAvailableQuest()
    {
        var (_, manager) = CreateManager(
            CreateQuest(
                "quest-a",
                [CreateObjective("a", GameEventType.NpcTalkedTo, "mom")],
                autoStart: true),
            CreateQuest(
                "quest-b",
                [CreateObjective("b", GameEventType.NpcTalkedTo, "grandpa")],
                autoStart: true));

        var changed = manager.SetTrackedQuest("quest-b");

        Assert.True(changed);
        Assert.Equal("quest-b", manager.TrackedQuest!.Definition.Id);
    }

    [Fact]
    public void Publish__RetargetsTrackedQuest__WhenTrackedQuestCompletesAndAnotherQuestIsActive()
    {
        var (bus, manager) = CreateManager(
            CreateQuest(
                "quest-a",
                [CreateObjective("a", GameEventType.EnemyKilled, requiredCount: 1)],
                autoStart: true),
            CreateQuest(
                "quest-b",
                [CreateObjective("b", GameEventType.NpcTalkedTo, "grandpa")],
                autoStart: true));

        Assert.Equal("quest-a", manager.TrackedQuest!.Definition.Id);

        bus.Publish(GameEventType.EnemyKilled, amount: 1);

        Assert.Equal("quest-b", manager.TrackedQuest!.Definition.Id);
        Assert.Equal(QuestStatus.Completed, manager.GetQuest("quest-a")!.Status);
        Assert.Equal(QuestStatus.Active, manager.GetQuest("quest-b")!.Status);
    }

    [Fact]
    public void SetTrackedQuest__RejectsHiddenQuest__WhenQuestHasNotStarted()
    {
        var (_, manager) = CreateManager(
            CreateQuest(
                "quest-a",
                [CreateObjective("a", GameEventType.NpcTalkedTo, "mom")],
                autoStart: true),
            CreateQuest(
                "quest-b",
                [CreateObjective("b", GameEventType.NpcTalkedTo, "grandpa")],
                startCondition: CreateCondition(GameEventType.ZoneEntered, "Maps/CabinIndoors")));

        var changed = manager.SetTrackedQuest("quest-b");

        Assert.False(changed);
        Assert.Equal("quest-a", manager.TrackedQuest!.Definition.Id);
    }

    [Fact]
    public void MainQuest__ReturnsActiveMainQuest__EvenWhenTrackedQuestIsSideQuest()
    {
        var (_, manager) = CreateManager(
            CreateQuest(
                "main-quest",
                [CreateObjective("main", GameEventType.NpcTalkedTo, "mom")],
                autoStart: true,
                isMainQuest: true,
                npcDialogs:
                [
                    CreateNpcDialog(
                        "grandpa",
                        new DialogLine("Grandpa", "Try the woods behind the shed."))
                ]),
            CreateQuest(
                "side-quest",
                [CreateObjective("side", GameEventType.NpcTalkedTo, "grandpa")],
                autoStart: true));

        manager.SetTrackedQuest("side-quest");

        Assert.Equal("main-quest", manager.MainQuest!.Definition.Id);
    }

    [Fact]
    public void ResolveMainQuestNpcDialog__ReturnsAuthoredDialog__ForCurrentMainQuest()
    {
        var (_, manager) = CreateManager(
            CreateQuest(
                "main-quest",
                [CreateObjective("main", GameEventType.NpcTalkedTo, "mom")],
                autoStart: true,
                isMainQuest: true,
                npcDialogs:
                [
                    CreateNpcDialog(
                        "grandpa",
                        new DialogLine("Grandpa", "Try the woods behind the shed."),
                        new DialogLine("Grandpa", "And keep your eyes open."))
                ]));

        var dialog = manager.ResolveMainQuestNpcDialog("grandpa");

        Assert.NotNull(dialog);
        Assert.Equal(2, dialog!.LineCount);
        Assert.Equal("Try the woods behind the shed.", dialog.Lines[0].Text);
        Assert.Equal("And keep your eyes open.", dialog.Lines[1].Text);
    }

    private static (GameEventBus bus, QuestManager manager) CreateManager(params QuestDefinition[] definitions)
    {
        var bus = new GameEventBus();
        var manager = new QuestManager(bus);
        manager.LoadDefinitions(definitions);
        return (bus, manager);
    }

    private static QuestDefinition CreateQuest(
        string id,
        ObjectiveDefinition[] objectives,
        bool autoStart = false,
        QuestEventConditionDefinition? startCondition = null,
        bool isMainQuest = false,
        QuestNpcDialogDefinition[]? npcDialogs = null)
    {
        return new QuestDefinition
        {
            Id = id,
            Title = id,
            Description = $"Description for {id}.",
            AutoStart = autoStart,
            IsMainQuest = isMainQuest,
            StartCondition = startCondition,
            NpcDialogs = npcDialogs ?? [],
            Objectives = objectives,
        };
    }

    private static QuestNpcDialogDefinition CreateNpcDialog(string npcId, params DialogLine[] lines)
    {
        return new QuestNpcDialogDefinition
        {
            NpcId = npcId,
            Lines = lines,
        };
    }

    private static ObjectiveDefinition CreateObjective(
        string id,
        GameEventType eventType,
        string? targetId = null,
        int requiredCount = 1)
    {
        return new ObjectiveDefinition
        {
            Id = id,
            Description = $"Objective {id}.",
            Completion = CreateCondition(eventType, targetId, requiredCount),
        };
    }

    private static QuestEventConditionDefinition CreateCondition(
        GameEventType eventType,
        string? targetId = null,
        int requiredCount = 1)
    {
        return new QuestEventConditionDefinition
        {
            EventType = eventType,
            TargetId = targetId,
            RequiredCount = requiredCount,
        };
    }
}