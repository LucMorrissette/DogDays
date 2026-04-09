using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

#nullable enable

namespace DogDays.Game.Entities;

/// <summary>
/// Static front-door prop that renders one of two authored visual states.
/// </summary>
public sealed class FrontDoor : IWorldProp
{
    private const float LockedShakeDurationSeconds = 0.16f;
    private const float LockedShakeFrequencyHz = 26f;
    private const float LockedShakeAmplitudePixels = 1.5f;

    private readonly Texture2D? _closedTexture;
    private readonly Texture2D? _openTexture;
    private readonly Vector2 _position;
    private readonly Point _size;
    private float _lockedShakeTimeRemaining;
    private bool _wasActorWithinInvitationRange;
    private bool _lockedFeedbackPending;

    /// <summary>
    /// Creates a logic-only front-door prop at a world position.
    /// </summary>
    /// <param name="position">Top-left world position in pixels.</param>
    /// <param name="size">Door size in pixels.</param>
    /// <param name="startOpen">Whether the door starts in the open state.</param>
    /// <param name="isLocked">When true the door stays shut and plays locked feedback instead of opening.</param>
    /// <param name="suppressOcclusion">When true, the reveal lens will not activate behind this prop.</param>
    public FrontDoor(Vector2 position, Point size, bool startOpen = false, bool isLocked = false, bool suppressOcclusion = false)
    {
        _position = position;
        _size = size;
        IsLocked = isLocked;
        IsOpen = startOpen && !isLocked;
        SuppressOcclusion = suppressOcclusion;
    }

    /// <summary>
    /// Creates a front-door prop at a world position.
    /// </summary>
    /// <param name="position">Top-left world position in pixels.</param>
    /// <param name="closedTexture">Closed-door texture used for drawing.</param>
    /// <param name="openTexture">Open-door texture used for drawing.</param>
    /// <param name="startOpen">Whether the door starts in the open state.</param>
    /// <param name="isLocked">When true the door stays shut and plays locked feedback instead of opening.</param>
    /// <param name="suppressOcclusion">When true, the reveal lens will not activate behind this prop.</param>
    public FrontDoor(Vector2 position, Texture2D closedTexture, Texture2D openTexture, bool startOpen = false, bool isLocked = false, bool suppressOcclusion = false)
        : this(
            position,
            closedTexture,
            openTexture,
            new Point(Math.Max(closedTexture.Width, openTexture.Width), Math.Max(closedTexture.Height, openTexture.Height)),
            startOpen,
            isLocked,
            suppressOcclusion)
    {
    }

    /// <summary>
    /// Creates a front-door prop at a world position with an explicit draw size.
    /// </summary>
    /// <param name="position">Top-left world position in pixels.</param>
    /// <param name="closedTexture">Closed-door texture used for drawing.</param>
    /// <param name="openTexture">Open-door texture used for drawing.</param>
    /// <param name="size">Door size in pixels.</param>
    /// <param name="startOpen">Whether the door starts in the open state.</param>
    /// <param name="isLocked">When true the door stays shut and plays locked feedback instead of opening.</param>
    /// <param name="suppressOcclusion">When true, the reveal lens will not activate behind this prop.</param>
    public FrontDoor(Vector2 position, Texture2D closedTexture, Texture2D openTexture, Point size, bool startOpen = false, bool isLocked = false, bool suppressOcclusion = false)
        : this(
            position,
            size,
            startOpen,
            isLocked,
            suppressOcclusion)
    {
        _closedTexture = closedTexture;
        _openTexture = openTexture;
    }

    /// <summary>Top-left world position in pixels.</summary>
    public Vector2 Position => _position;

    /// <summary>When true this instance uses the open-door sprite.</summary>
    public bool IsOpen { get; private set; }

    /// <summary>When true this door never auto-opens and instead emits locked feedback.</summary>
    public bool IsLocked { get; }

    /// <summary>When true the locked-door shake feedback is currently active.</summary>
    public bool IsLockedFeedbackActive => _lockedShakeTimeRemaining > 0f;

    /// <summary>When true, the reveal lens will not activate when a character walks behind this door.</summary>
    public bool SuppressOcclusion { get; }

    /// <summary>World-space bounds covered by this sprite.</summary>
    public Rectangle Bounds => new(
        (int)_position.X,
        (int)_position.Y,
        _size.X,
        _size.Y);

    /// <summary>
    /// Updates the visible door state based on whether the player is close enough to be invited in.
    /// </summary>
    /// <param name="actorBounds">Actor bounds to test, usually the player.</param>
    /// <param name="invitationDistancePixels">Maximum edge-to-edge distance that opens the door.</param>
    /// <param name="elapsedSeconds">Frame delta in seconds.</param>
    public void UpdateInvitationState(Rectangle actorBounds, int invitationDistancePixels)
    {
        UpdateInvitationState(actorBounds, invitationDistancePixels, 0f);
    }

    /// <summary>
    /// Updates the visible door state and any locked-door feedback based on player proximity.
    /// </summary>
    /// <param name="actorBounds">Actor bounds to test, usually the player.</param>
    /// <param name="invitationDistancePixels">Maximum edge-to-edge distance that opens the door.</param>
    /// <param name="elapsedSeconds">Frame delta in seconds.</param>
    public void UpdateInvitationState(Rectangle actorBounds, int invitationDistancePixels, float elapsedSeconds)
    {
        if (elapsedSeconds > 0f)
        {
            _lockedShakeTimeRemaining = Math.Max(0f, _lockedShakeTimeRemaining - elapsedSeconds);
        }

        var isWithinRange = IsActorWithinInvitationRange(actorBounds, invitationDistancePixels);
        if (IsLocked)
        {
            if (isWithinRange && !_wasActorWithinInvitationRange)
            {
                _lockedShakeTimeRemaining = LockedShakeDurationSeconds;
                _lockedFeedbackPending = true;
            }

            _wasActorWithinInvitationRange = isWithinRange;
            IsOpen = false;
            return;
        }

        _wasActorWithinInvitationRange = isWithinRange;
        IsOpen = isWithinRange;
    }

    /// <summary>
    /// Returns whether a locked-door sound cue should be played, consuming the pending request.
    /// </summary>
    public bool ConsumeLockedFeedbackRequest()
    {
        if (!_lockedFeedbackPending)
        {
            return false;
        }

        _lockedFeedbackPending = false;
        return true;
    }

    /// <summary>
    /// Returns true when the actor is within the configured invitation range of the door.
    /// </summary>
    public bool IsActorWithinInvitationRange(Rectangle actorBounds, int invitationDistancePixels)
    {
        if (actorBounds.Width <= 0 || actorBounds.Height <= 0)
        {
            return false;
        }

        var allowedDistance = Math.Max(0, invitationDistancePixels);
        var horizontalGap = GetAxisGap(Bounds.Left, Bounds.Right, actorBounds.Left, actorBounds.Right);
        var verticalGap = GetAxisGap(Bounds.Top, Bounds.Bottom, actorBounds.Top, actorBounds.Bottom);
        var distanceSquared = (horizontalGap * horizontalGap) + (verticalGap * verticalGap);
        return distanceSquared <= (allowedDistance * allowedDistance);
    }

    /// <summary>
    /// Draws the front door in world space.
    /// </summary>
    /// <param name="spriteBatch">Sprite batch for the world pass.</param>
    /// <param name="layerDepth">Depth value for Y-sorting (0 = back, 1 = front).</param>
    public void Draw(SpriteBatch spriteBatch, float layerDepth = 0f)
    {
        var texture = IsOpen ? _openTexture : _closedTexture;
        if (texture is null)
        {
            throw new InvalidOperationException("Door textures are required to draw FrontDoor.");
        }

        var drawPosition = _position + GetLockedShakeOffset();
        var destination = new Rectangle((int)MathF.Round(drawPosition.X), (int)MathF.Round(drawPosition.Y), _size.X, _size.Y);
        spriteBatch.Draw(texture, destination, null, Color.White, 0f, Vector2.Zero, SpriteEffects.None, layerDepth);
    }

    private Vector2 GetLockedShakeOffset()
    {
        if (_lockedShakeTimeRemaining <= 0f)
        {
            return Vector2.Zero;
        }

        var normalizedTime = _lockedShakeTimeRemaining / LockedShakeDurationSeconds;
        var elapsed = LockedShakeDurationSeconds - _lockedShakeTimeRemaining;
        var oscillation = MathF.Sin(elapsed * LockedShakeFrequencyHz * MathHelper.TwoPi);
        return new Vector2(oscillation * LockedShakeAmplitudePixels * normalizedTime, 0f);
    }

    private static int GetAxisGap(int firstMin, int firstMax, int secondMin, int secondMax)
    {
        if (secondMax <= firstMin)
        {
            return firstMin - secondMax;
        }

        if (secondMin >= firstMax)
        {
            return secondMin - firstMax;
        }

        return 0;
    }
}