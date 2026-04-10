#nullable enable

using System;
using Microsoft.Xna.Framework;
using DogDays.Game.Data;
using DogDays.Game.Entities;
using DogDays.Game.World;

namespace DogDays.Game.Systems;

/// <summary>
/// Drives the short cottage-exit beat where Mom warns the boys about the missing garden gnome.
/// </summary>
internal sealed class MissingGnomeSequence
{
    private MissingGnomeSequenceState _state = MissingGnomeSequenceState.Inactive;
    private DialogScript? _pendingDialog;
    private bool _completionRequested;
    private Vector2 _momTargetPosition;
    private ScriptedNpcRoute? _momRoute;

    /// <summary>Whether the sequence is currently controlling the scene.</summary>
    internal bool IsActive => _state is not MissingGnomeSequenceState.Inactive and not MissingGnomeSequenceState.Completed;

    /// <summary>Current authored sequence state.</summary>
    internal MissingGnomeSequenceState State => _state;

    /// <summary>
    /// Starts the sequence by moving Mom toward the boys near the front door.
    /// </summary>
    internal void Begin(Vector2 momTargetPosition, ScriptedNpcRoute? momRoute)
    {
        _momTargetPosition = momTargetPosition;
        _momRoute = momRoute;
        _state = MissingGnomeSequenceState.MomApproaching;
    }

    /// <summary>
    /// Advances the sequence after its dialog box fully closes.
    /// </summary>
    internal void NotifyDialogDismissed()
    {
        if (_state != MissingGnomeSequenceState.AwaitingDialog)
        {
            return;
        }

        _completionRequested = true;
        _state = MissingGnomeSequenceState.Completed;
    }

    /// <summary>
    /// Updates scripted actor motion for the sequence.
    /// </summary>
    internal void Update(
        GameTime gameTime,
        IScriptControllableNpc mom,
        IScriptControllableActor player,
        IScriptControllableActor follower,
        IMapCollisionData? collisionData)
    {
        ArgumentNullException.ThrowIfNull(mom);
        ArgumentNullException.ThrowIfNull(player);
        ArgumentNullException.ThrowIfNull(follower);

        if (_state != MissingGnomeSequenceState.MomApproaching)
        {
            return;
        }

        player.ClearMovementState();
        follower.ClearMovementState();

        var elapsedSeconds = Math.Max(0f, (float)gameTime.ElapsedGameTime.TotalSeconds);
        var momArrived = _momRoute?.Update(
            gameTime,
            mom,
            collisionData,
            MissingGnomeDefinition.MomApproachSpeedPixelsPerSecond)
            ?? ScriptedActorMotion.MoveTowards(
                mom,
                _momTargetPosition,
                Math.Max(0f, (float)gameTime.ElapsedGameTime.TotalSeconds),
                MissingGnomeDefinition.MomApproachSpeedPixelsPerSecond,
                collisionData);

        if (!momArrived)
        {
            return;
        }

        mom.ClearMovementState();
        FaceActorToward(player, mom.Center);
        FaceActorToward(follower, mom.Center);
        mom.SetFacing(MissingGnomeDefinition.MomDoorApproachPose.Facing);
        _pendingDialog = MissingGnomeDefinition.MissingGnomeDialog;
        _state = MissingGnomeSequenceState.AwaitingDialog;
    }

    /// <summary>
    /// Returns and clears any dialog the owning screen should start.
    /// </summary>
    internal DialogScript? ConsumePendingDialog()
    {
        var dialog = _pendingDialog;
        _pendingDialog = null;
        return dialog;
    }

    /// <summary>
    /// Returns whether the sequence completed this frame and clears the completion flag.
    /// </summary>
    internal bool ConsumeCompletionRequested()
    {
        var completionRequested = _completionRequested;
        _completionRequested = false;
        return completionRequested;
    }

    private static void FaceActorToward(IScriptControllableActor actor, Vector2 targetWorldPosition)
    {
        var direction = targetWorldPosition - actor.Center;
        if (direction.LengthSquared() <= 0f)
        {
            return;
        }

        if (MathF.Abs(direction.X) >= MathF.Abs(direction.Y))
        {
            actor.SetFacing(direction.X >= 0f ? FacingDirection.Right : FacingDirection.Left);
        }
        else
        {
            actor.SetFacing(direction.Y >= 0f ? FacingDirection.Down : FacingDirection.Up);
        }
    }
}

/// <summary>
/// State machine for the missing-gnome exit sequence.
/// </summary>
internal enum MissingGnomeSequenceState
{
    Inactive,
    MomApproaching,
    AwaitingDialog,
    Completed,
}