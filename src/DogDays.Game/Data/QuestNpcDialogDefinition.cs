#nullable enable

namespace DogDays.Game.Data;

/// <summary>
/// Authored dialog override for a specific NPC while a quest is active.
/// </summary>
internal sealed class QuestNpcDialogDefinition
{
    /// <summary>Stable NPC identifier, matching gameplay interaction ids such as <c>grandpa</c>.</summary>
    public string NpcId { get; init; } = string.Empty;

    /// <summary>Ordered dialog lines the NPC should speak for this quest.</summary>
    public DialogLine[] Lines { get; init; } = [];
}