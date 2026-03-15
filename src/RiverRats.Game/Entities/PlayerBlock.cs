using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RiverRats.Game.Input;
using RiverRats.Game.World;

namespace RiverRats.Game.Entities;

/// <summary>
/// Minimal player entity represented as a solid color block with input-driven movement.
/// </summary>
public sealed class PlayerBlock
{
    private readonly Vector2 _size;
    private readonly float _moveSpeedPixelsPerSecond;
    private readonly Rectangle _worldBounds;
    private readonly Color _color;

    private Vector2 _position;

    /// <summary>
    /// Initializes a player block.
    /// </summary>
    /// <param name="startPosition">Initial top-left world position in pixels.</param>
    /// <param name="size">Block size in pixels.</param>
    /// <param name="moveSpeedPixelsPerSecond">Movement speed in pixels per second.</param>
    /// <param name="worldBounds">World bounds in pixels used for movement clamping.</param>
    /// <param name="color">Block tint color.</param>
    public PlayerBlock(
        Vector2 startPosition,
        Point size,
        float moveSpeedPixelsPerSecond,
        Rectangle worldBounds,
        Color color)
    {
        _position = startPosition;
        _size = new Vector2(size.X, size.Y);
        _moveSpeedPixelsPerSecond = moveSpeedPixelsPerSecond;
        _worldBounds = worldBounds;
        _color = color;

        ClampToBounds();
    }

    /// <summary>Current top-left world position in pixels.</summary>
    public Vector2 Position => _position;

    /// <summary>Current world-space center point used for camera follow.</summary>
    public Vector2 Center => _position + (_size * 0.5f);

    /// <summary>Current AABB in world-space pixels.</summary>
    public Rectangle Bounds => new(
        (int)_position.X,
        (int)_position.Y,
        (int)_size.X,
        (int)_size.Y);

    /// <summary>
    /// Updates player position from movement input.
    /// </summary>
    /// <param name="gameTime">Frame timing values.</param>
    /// <param name="inputManager">Input source abstraction.</param>
    public void Update(GameTime gameTime, IInputManager inputManager, IMapCollisionData mapCollisionData)
    {
        var direction = Vector2.Zero;

        if (inputManager.IsHeld(InputAction.MoveLeft))
        {
            direction.X -= 1f;
        }

        if (inputManager.IsHeld(InputAction.MoveRight))
        {
            direction.X += 1f;
        }

        if (inputManager.IsHeld(InputAction.MoveUp))
        {
            direction.Y -= 1f;
        }

        if (inputManager.IsHeld(InputAction.MoveDown))
        {
            direction.Y += 1f;
        }

        if (direction != Vector2.Zero)
        {
            direction.Normalize();
            var elapsedSeconds = (float)gameTime.ElapsedGameTime.TotalSeconds;
            var movementDelta = direction * _moveSpeedPixelsPerSecond * elapsedSeconds;

            if (movementDelta.X != 0f)
            {
                TryMoveOnAxis(new Vector2(movementDelta.X, 0f), mapCollisionData);
            }

            if (movementDelta.Y != 0f)
            {
                TryMoveOnAxis(new Vector2(0f, movementDelta.Y), mapCollisionData);
            }
        }
    }

    /// <summary>
    /// Draws the player block in world space.
    /// </summary>
    /// <param name="spriteBatch">Sprite batch for world pass.</param>
    /// <param name="solidPixelTexture">1x1 white texture used to render solid color quads.</param>
    public void Draw(SpriteBatch spriteBatch, Texture2D solidPixelTexture)
    {
        spriteBatch.Draw(solidPixelTexture, Bounds, _color);
    }

    private void ClampToBounds()
    {
        _position = ClampPosition(_position);
    }

    private void TryMoveOnAxis(Vector2 axisMovement, IMapCollisionData mapCollisionData)
    {
        var remainingDistance = axisMovement.X != 0f ? axisMovement.X : axisMovement.Y;
        var stepDirection = MathF.Sign(remainingDistance);

        while (remainingDistance != 0f)
        {
            var stepMagnitude = MathF.Abs(remainingDistance) >= 1f ? 1f : MathF.Abs(remainingDistance);
            var step = stepMagnitude * stepDirection;
            var delta = axisMovement.X != 0f
                ? new Vector2(step, 0f)
                : new Vector2(0f, step);

            var candidatePosition = ClampPosition(_position + delta);
            var candidateBounds = new Rectangle(
                (int)candidatePosition.X,
                (int)candidatePosition.Y,
                (int)_size.X,
                (int)_size.Y);

            if (mapCollisionData.IsWorldRectangleBlocked(candidateBounds))
            {
                break;
            }

            _position = candidatePosition;
            remainingDistance -= step;
        }
    }

    private Vector2 ClampPosition(Vector2 position)
    {
        var minX = _worldBounds.Left;
        var minY = _worldBounds.Top;
        var maxX = _worldBounds.Right - _size.X;
        var maxY = _worldBounds.Bottom - _size.Y;

        return new Vector2(
            MathHelper.Clamp(position.X, minX, maxX),
            MathHelper.Clamp(position.Y, minY, maxY));
    }
}
