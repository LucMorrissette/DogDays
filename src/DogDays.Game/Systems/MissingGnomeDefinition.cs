#nullable enable

using System;
using DogDays.Game.Data;
using Microsoft.Xna.Framework;

namespace DogDays.Game.Systems;

/// <summary>
/// Central authored directives for the cottage-exit missing-gnome story beat.
/// </summary>
internal static class MissingGnomeDefinition
{
    internal const string QuestId = "search-for-gnome-chompsky";
    internal const string QuestUnlockedStoryBeatId = "missing_gnome_warning";
    internal const string ForestMapAssetName = "Maps/WoodsBehindCabin";
    internal const string MomApproachDestinationNodeName = "entry";
    internal const float MomApproachSpeedPixelsPerSecond = 56f;

    internal static readonly Vector2 MomDoorFallbackOffsetFromPlayer = new(-52f, -36f);

    internal static readonly ScriptedActorPoseDirective MomDoorApproachPose = new(
        new ScriptedPositionDirective("from-outdoors", new Vector2(-52f, -36f)),
        FacingDirection.Down);

    internal static readonly DialogScript MissingGnomeDialog = new(
        new DialogLine("Mom", "Hold on, boys. Have either of you seen the garden gnome?"),
        new DialogLine("Mom", "Gnome Chompsky has gone missing, and the shed key was hidden inside it."),
        new DialogLine("Mom", "We won't be getting into the shed until we find it. Hopefully it wasn't stolen."));

    internal static bool IsOutdoorExitTrigger(string currentMapAssetName, ZoneTriggerData trigger)
    {
        return string.Equals(currentMapAssetName, SummerIntroDefinition.IndoorMapAssetName, StringComparison.Ordinal)
            && string.Equals(trigger.TargetMap, SummerIntroDefinition.OutdoorMapAssetName, StringComparison.Ordinal);
    }

    internal static bool ShouldTriggerExitSequence(string currentMapAssetName, QuestState? questState, ZoneTriggerData trigger)
    {
        return questState?.Status == QuestStatus.NotStarted
            && IsOutdoorExitTrigger(currentMapAssetName, trigger);
    }
}