#nullable enable

using System;
using Microsoft.Xna.Framework;
using DogDays.Game.Data;
using DogDays.Game.Entities;

namespace DogDays.Game.Systems;

/// <summary>
/// Drives the authored opening story beat where Mom introduces the summer,
/// the companion arrives, and the boys walk back into the bedroom.
/// </summary>
internal sealed class SummerIntroSequence
{
    private SummerIntroSequenceState _state = SummerIntroSequenceState.Inactive;
    private DialogScript? _pendingDialog;
    private SummerIntroTransition? _pendingTransition;
    private bool _completionRequested;
    private Vector2 _companionTargetPosition;
    private Vector2 _playerTransitionTargetPosition;
    private Vector2 _companionTransitionTargetPosition;

    /// <summary>Whether the opener is actively controlling the scene.</summary>
    internal bool IsActive => _state is not SummerIntroSequenceState.Inactive and not SummerIntroSequenceState.Completed;

    /// <summary>Whether the companion should be rendered even before they permanently join the party.</summary>
    internal bool CompanionShouldBeVisible => _state is SummerIntroSequenceState.CompanionWalkingIn
        or SummerIntroSequenceState.AwaitingCompanionDialog
        or SummerIntroSequenceState.WalkingToBedroomDoor
        or SummerIntroSequenceState.TransitionToBedroom
        or SummerIntroSequenceState.AwaitingBedroomDialog;

    /// <summary>Current authored intro state.</summary>
    internal SummerIntroSequenceState State => _state;

    /// <summary>
    /// Starts the Mom conversation portion of the opener.
    /// </summary>
    /// <param name="companionTargetPosition">Where the companion should stop after walking into the cabin.</param>
    /// <param name="playerTransitionTargetPosition">Where the player should stop before transitioning into the bedroom.</param>
    /// <param name="companionTransitionTargetPosition">Where the companion should stop before transitioning into the bedroom.</param>
    internal void BeginMomConversation(
        Vector2 companionTargetPosition,
        Vector2 playerTransitionTargetPosition,
        Vector2 companionTransitionTargetPosition)
    {
        _companionTargetPosition = companionTargetPosition;
        _playerTransitionTargetPosition = playerTransitionTargetPosition;
        _companionTransitionTargetPosition = companionTransitionTargetPosition;
        _pendingDialog = SummerIntroDefinition.MomIntroDialog;
        _state = SummerIntroSequenceState.AwaitingMomDialog;
    }

    /// <summary>
    /// Starts the bedroom conversation after the screen transition into the bedroom.
    /// </summary>
    internal void BeginBedroomReturn()
    {
        _pendingDialog = SummerIntroDefinition.BedroomChatDialog;
        _state = SummerIntroSequenceState.AwaitingBedroomDialog;
    }

    /// <summary>
    /// Advances the opener after a dialog box fully closes.
    /// </summary>
    internal void NotifyDialogDismissed()
    {
        switch (_state)
        {
            case SummerIntroSequenceState.AwaitingMomDialog:
                _state = SummerIntroSequenceState.CompanionWalkingIn;
                break;

            case SummerIntroSequenceState.AwaitingCompanionDialog:
                _state = SummerIntroSequenceState.WalkingToBedroomDoor;
                break;

            case SummerIntroSequenceState.AwaitingBedroomDialog:
                _completionRequested = true;
                _state = SummerIntroSequenceState.Completed;
                break;
        }
    }

    /// <summary>
    /// Updates the walk-in segments of the opener.
    /// </summary>
    internal void Update(GameTime gameTime, IScriptControllableActor player, IScriptControllableActor follower)
    {
        ArgumentNullException.ThrowIfNull(player);
        ArgumentNullException.ThrowIfNull(follower);

        var elapsedSeconds = Math.Max(0f, (float)gameTime.ElapsedGameTime.TotalSeconds);

        switch (_state)
        {
            case SummerIntroSequenceState.CompanionWalkingIn:
            {
                player.ClearMovementState();
                var arrived = ScriptedActorMotion.MoveTowards(
                    follower,
                    _companionTargetPosition,
                    elapsedSeconds,
                    SummerIntroDefinition.WalkSpeedPixelsPerSecond);
                if (!arrived)
                {
                    break;
                }

                follower.ClearMovementState();
                FaceCharactersTowardEachOther(player, follower);
                _pendingDialog = SummerIntroDefinition.CompanionArrivalDialog;
                _state = SummerIntroSequenceState.AwaitingCompanionDialog;
                break;
            }

            case SummerIntroSequenceState.WalkingToBedroomDoor:
            {
                var playerArrived = ScriptedActorMotion.MoveTowards(
                    player,
                    _playerTransitionTargetPosition,
                    elapsedSeconds,
                    SummerIntroDefinition.WalkSpeedPixelsPerSecond);
                var companionArrived = ScriptedActorMotion.MoveTowards(
                    follower,
                    _companionTransitionTargetPosition,
                    elapsedSeconds,
                    SummerIntroDefinition.WalkSpeedPixelsPerSecond);

                if (!playerArrived || !companionArrived)
                {
                    break;
                }

                player.ClearMovementState();
                follower.ClearMovementState();
                _pendingTransition = SummerIntroDefinition.BedroomTransition;
                _state = SummerIntroSequenceState.TransitionToBedroom;
                break;
            }
        }
    }

    /// <summary>
    /// Returns and clears any dialog that the screen should start this frame.
    /// </summary>
    internal DialogScript? ConsumePendingDialog()
    {
        var dialog = _pendingDialog;
        _pendingDialog = null;
        return dialog;
    }

    /// <summary>
    /// Returns and clears any screen transition requested by the opener.
    /// </summary>
    internal SummerIntroTransition? ConsumePendingTransition()
    {
        var transition = _pendingTransition;
        _pendingTransition = null;
        return transition;
    }

    /// <summary>
    /// Returns whether the sequence finished this frame and clears the completion flag.
    /// </summary>
    internal bool ConsumeCompletionRequested()
    {
        var completionRequested = _completionRequested;
        _completionRequested = false;
        return completionRequested;
    }

    private static void FaceCharactersTowardEachOther(IScriptControllableActor player, IScriptControllableActor follower)
    {
        if (player.Center.X <= follower.Center.X)
        {
            player.SetFacing(FacingDirection.Right);
            follower.SetFacing(FacingDirection.Left);
            return;
        }

        player.SetFacing(FacingDirection.Left);
        follower.SetFacing(FacingDirection.Right);
    }
}

/// <summary>
/// Transient opener state that a new gameplay screen should begin with.
/// </summary>
internal enum SummerIntroStartState
{
    None,
    BedroomReturn,
}

/// <summary>
/// State machine for the authored summer opener.
/// </summary>
internal enum SummerIntroSequenceState
{
    Inactive,
    AwaitingMomDialog,
    CompanionWalkingIn,
    AwaitingCompanionDialog,
    WalkingToBedroomDoor,
    TransitionToBedroom,
    AwaitingBedroomDialog,
    Completed,
}

/// <summary>
/// Request for the owning gameplay screen to replace itself with another intro phase.
/// </summary>
internal readonly record struct SummerIntroTransition(string MapAssetName, string? SpawnPointId, SummerIntroStartState StartState);