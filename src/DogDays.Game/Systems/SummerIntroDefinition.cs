using System;
using Microsoft.Xna.Framework;
using DogDays.Game.Data;

namespace DogDays.Game.Systems;

/// <summary>
/// Central authored directives for the opening summer intro beat.
/// </summary>
internal static class SummerIntroDefinition
{
    internal const string QuestId = "first-day-of-summer";
    internal const string CompletionStoryBeatId = "summer_intro_complete";
    internal const string MomHoldNavNodeName = "center";
    internal const string IndoorMapAssetName = "Maps/CabinIndoors";
    internal const string OutdoorMapAssetName = "Maps/StarterMap";
    internal const float WalkSpeedPixelsPerSecond = 54f;

    internal static readonly Vector2 CompanionDoorFallbackOffsetFromPlayer = new(44f, 0f);
    internal static readonly Vector2 BedroomDoorFallbackOffsetFromPlayer = new(128f, -64f);

    internal static readonly ScriptedActorPoseDirective CompanionEntranceStartPose = new(
        new ScriptedPositionDirective("from-outdoors", new Vector2(0f, 18f)),
        FacingDirection.Up);

    internal static readonly ScriptedPositionDirective CompanionEntranceTarget = new(
        "from-outdoors",
        new Vector2(0f, -44f));

    internal static readonly ScriptedPositionDirective PlayerBedroomDoorTarget = new(
        "from-bedroom",
        Vector2.Zero);

    internal static readonly ScriptedPositionDirective CompanionBedroomDoorTarget = new(
        "from-bedroom",
        new Vector2(-22f, 10f));

    internal static readonly ScriptedActorPoseDirective BedroomPlayerPose = new(
        new ScriptedPositionDirective("intro-player", Vector2.Zero),
        FacingDirection.Right);

    internal static readonly ScriptedActorPoseDirective BedroomCompanionPose = new(
        new ScriptedPositionDirective("intro-companion", Vector2.Zero),
        FacingDirection.Left);

    internal static readonly DialogScript MomIntroDialog = new(
        new DialogLine("Mom", "There you are. It's the first day of summer vacation."),
        new DialogLine("Mom", "Your friend is spending the summer here at the cottage with us."),
        new DialogLine("Mom", "They should be here any minute now, so try to look awake when they walk in."));

    internal static readonly DialogScript CompanionArrivalDialog = new(
        new DialogLine("Companion", "Hey! I made it!"),
        new DialogLine("Companion", "I can't wait to spend the summer by the river with you."),
        new DialogLine("Companion", "We're going to get into all sorts of adventures."));

    internal static readonly DialogScript BedroomChatDialog = new(
        new DialogLine("Player", "This is going to be the best summer we've had yet."),
        new DialogLine("Companion", "River first. Woods second. Everything else after that."),
        new DialogLine("Player", "Then let's not waste a single day of it."));

    internal static readonly DialogScript ExitBlockedDialog = new(
        new DialogLine("Player", "I shouldn't leave without talking to Mom first."));

    internal static readonly SummerIntroTransition BedroomTransition = new(
        "Maps/CabinBedroom",
        "from-cabin-indoors",
        SummerIntroStartState.BedroomReturn);

    internal static bool ShouldBlockOutdoorExit(string currentMapAssetName, bool isIntroComplete, ZoneTriggerData trigger)
    {
        return !isIntroComplete
            && string.Equals(currentMapAssetName, IndoorMapAssetName, StringComparison.Ordinal)
            && string.Equals(trigger.TargetMap, OutdoorMapAssetName, StringComparison.Ordinal);
    }
}