#nullable enable

using System;
using System.Collections.Generic;
using DogDays.Game.Data;

namespace DogDays.Game.Systems;

/// <summary>
/// Resolves authored NPC dialog overrides keyed by quest id and NPC id.
/// </summary>
internal sealed class QuestNpcDialogRegistry
{
    private readonly Dictionary<string, Dictionary<string, DialogScript>> _dialogsByQuestId = new(StringComparer.Ordinal);

    /// <summary>
    /// Rebuilds the registry from loaded quest definitions.
    /// </summary>
    internal void LoadDefinitions(IEnumerable<QuestDefinition> definitions)
    {
        ArgumentNullException.ThrowIfNull(definitions);

        _dialogsByQuestId.Clear();

        foreach (var definition in definitions)
        {
            if (definition.NpcDialogs.Length == 0)
            {
                continue;
            }

            var dialogByNpcId = new Dictionary<string, DialogScript>(StringComparer.Ordinal);
            for (var i = 0; i < definition.NpcDialogs.Length; i++)
            {
                var npcDialog = definition.NpcDialogs[i];
                dialogByNpcId.Add(npcDialog.NpcId, new DialogScript(npcDialog.Lines));
            }

            _dialogsByQuestId.Add(definition.Id, dialogByNpcId);
        }
    }

    /// <summary>
    /// Returns the authored NPC dialog override for the supplied quest, or null when none exists.
    /// </summary>
    internal DialogScript? Resolve(string? questId, string npcId)
    {
        if (string.IsNullOrWhiteSpace(questId) || string.IsNullOrWhiteSpace(npcId))
        {
            return null;
        }

        return _dialogsByQuestId.TryGetValue(questId, out var dialogByNpcId)
            && dialogByNpcId.TryGetValue(npcId, out var dialog)
            ? dialog
            : null;
    }
}