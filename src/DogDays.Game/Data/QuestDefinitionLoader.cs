using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace DogDays.Game.Data;

/// <summary>
/// Loads and validates quest definition JSON files.
/// </summary>
internal static class QuestDefinitionLoader
{
    private const string GrandpaNpcId = "grandpa";

    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        AllowTrailingCommas = true,
        PropertyNameCaseInsensitive = true,
        ReadCommentHandling = JsonCommentHandling.Skip,
        Converters =
        {
            new JsonStringEnumConverter(),
        },
    };

    /// <summary>
    /// Loads quest definitions from a JSON file on disk.
    /// </summary>
    /// <param name="filePath">Absolute path to the quest definition JSON file.</param>
    /// <returns>Validated quest definitions in file order.</returns>
    internal static QuestDefinition[] LoadFromFile(string filePath)
    {
        if (string.IsNullOrWhiteSpace(filePath))
        {
            throw new ArgumentException("Quest definition path is required.", nameof(filePath));
        }

        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException("Quest definition file was not found.", filePath);
        }

        var json = File.ReadAllText(filePath);
        var definitions = JsonSerializer.Deserialize<QuestDefinition[]>(json, SerializerOptions) ?? [];

        ValidateDefinitions(definitions, filePath);
        return definitions;
    }

    private static void ValidateDefinitions(QuestDefinition[] definitions, string filePath)
    {
        var questIds = new HashSet<string>(StringComparer.Ordinal);

        for (var questIndex = 0; questIndex < definitions.Length; questIndex++)
        {
            var quest = definitions[questIndex];
            if (quest is null)
            {
                throw new InvalidDataException($"Quest entry {questIndex} in '{filePath}' is null.");
            }

            if (string.IsNullOrWhiteSpace(quest.Id))
            {
                throw new InvalidDataException($"Quest entry {questIndex} in '{filePath}' is missing an id.");
            }

            if (!questIds.Add(quest.Id))
            {
                throw new InvalidDataException($"Quest id '{quest.Id}' is duplicated in '{filePath}'.");
            }

            if (string.IsNullOrWhiteSpace(quest.Title))
            {
                throw new InvalidDataException($"Quest '{quest.Id}' is missing a title.");
            }

            if (quest.Objectives is null || quest.Objectives.Length == 0)
            {
                throw new InvalidDataException($"Quest '{quest.Id}' must declare at least one objective.");
            }

            if (quest.StartCondition is not null)
            {
                ValidateCondition(quest.StartCondition, $"Quest '{quest.Id}' start condition");
            }

            ValidateNpcDialogs(quest, filePath);

            var objectiveIds = new HashSet<string>(StringComparer.Ordinal);
            for (var objectiveIndex = 0; objectiveIndex < quest.Objectives.Length; objectiveIndex++)
            {
                var objective = quest.Objectives[objectiveIndex];
                if (objective is null)
                {
                    throw new InvalidDataException($"Quest '{quest.Id}' has a null objective at index {objectiveIndex}.");
                }

                if (string.IsNullOrWhiteSpace(objective.Id))
                {
                    throw new InvalidDataException($"Quest '{quest.Id}' objective {objectiveIndex} is missing an id.");
                }

                if (!objectiveIds.Add(objective.Id))
                {
                    throw new InvalidDataException($"Quest '{quest.Id}' has duplicate objective id '{objective.Id}'.");
                }

                if (objective.Completion is null)
                {
                    throw new InvalidDataException($"Quest '{quest.Id}' objective '{objective.Id}' is missing a completion rule.");
                }

                ValidateCondition(objective.Completion, $"Quest '{quest.Id}' objective '{objective.Id}'");
            }
        }
    }

    private static void ValidateNpcDialogs(QuestDefinition quest, string filePath)
    {
        if (quest.NpcDialogs is null)
        {
            throw new InvalidDataException($"Quest '{quest.Id}' in '{filePath}' cannot use a null npcDialogs array.");
        }

        var npcIds = new HashSet<string>(StringComparer.Ordinal);
        for (var dialogIndex = 0; dialogIndex < quest.NpcDialogs.Length; dialogIndex++)
        {
            var npcDialog = quest.NpcDialogs[dialogIndex];
            if (npcDialog is null)
            {
                throw new InvalidDataException($"Quest '{quest.Id}' has a null npc dialog at index {dialogIndex}.");
            }

            if (string.IsNullOrWhiteSpace(npcDialog.NpcId))
            {
                throw new InvalidDataException($"Quest '{quest.Id}' npc dialog {dialogIndex} is missing an npcId.");
            }

            if (!npcIds.Add(npcDialog.NpcId))
            {
                throw new InvalidDataException($"Quest '{quest.Id}' has duplicate npc dialog id '{npcDialog.NpcId}'.");
            }

            if (npcDialog.Lines is null || npcDialog.Lines.Length == 0)
            {
                throw new InvalidDataException($"Quest '{quest.Id}' npc dialog '{npcDialog.NpcId}' must declare at least one line.");
            }

            for (var lineIndex = 0; lineIndex < npcDialog.Lines.Length; lineIndex++)
            {
                if (string.IsNullOrWhiteSpace(npcDialog.Lines[lineIndex].Text))
                {
                    throw new InvalidDataException($"Quest '{quest.Id}' npc dialog '{npcDialog.NpcId}' line {lineIndex} is missing text.");
                }
            }
        }

        if (quest.IsMainQuest && !npcIds.Contains(GrandpaNpcId))
        {
            throw new InvalidDataException($"Main quest '{quest.Id}' in '{filePath}' must include an npc dialog for '{GrandpaNpcId}'.");
        }
    }

    private static void ValidateCondition(QuestEventConditionDefinition condition, string label)
    {
        if (condition.RequiredCount <= 0)
        {
            throw new InvalidDataException($"{label} must use a requiredCount greater than zero.");
        }
    }
}