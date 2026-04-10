#nullable enable

namespace DogDays.Game.Data;

/// <summary>
/// Immutable quest definition loaded from JSON.
/// </summary>
internal sealed class QuestDefinition
{
    /// <summary>Stable quest identifier.</summary>
    public string Id { get; init; } = string.Empty;

    /// <summary>Player-facing quest title.</summary>
    public string Title { get; init; } = string.Empty;

    /// <summary>Player-facing quest description.</summary>
    public string Description { get; init; } = string.Empty;

    /// <summary>When true, the quest starts as soon as definitions are loaded.</summary>
    public bool AutoStart { get; init; }

    /// <summary>When true, the quest participates in main-story dialog and progression surfaces.</summary>
    public bool IsMainQuest { get; init; }

    /// <summary>
    /// Optional event condition that starts the quest when it is still in the not-started state.
    /// </summary>
    public QuestEventConditionDefinition? StartCondition { get; init; }

    /// <summary>Optional authored NPC dialog overrides keyed by NPC id while this quest is active.</summary>
    public QuestNpcDialogDefinition[] NpcDialogs { get; init; } = [];

    /// <summary>Ordered linear objectives for the quest.</summary>
    public ObjectiveDefinition[] Objectives { get; init; } = [];
}