using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace RiverRats.Game.Entities;

/// <summary>
/// Static world obstacle rendered from a single sprite and treated as solid for movement.
/// </summary>
public sealed class Boulder : IWorldProp
{
    private readonly Texture2D _texture;
    private readonly Vector2 _position;
    private readonly float _rotationRadians;
    private readonly int _collisionHeightPixels;

    /// <summary>
    /// Creates a boulder obstacle at a world position.
    /// </summary>
    /// <param name="position">Top-left world position in pixels.</param>
    /// <param name="texture">Boulder texture used for drawing and bounds.</param>
    /// <param name="suppressOcclusion">When true, the reveal lens will not activate behind this prop.</param>
    /// <param name="rotationRadians">Clockwise rotation in radians.</param>
    /// <param name="collisionHeightPixels">
    /// Height in pixels of the collision box, measured from the bottom of the sprite.
    /// When 0 (default) the full texture height is used.
    /// Use a small value for tall props (e.g. shelves) so only the base blocks movement.
    /// </param>
    public Boulder(Vector2 position, Texture2D texture, bool suppressOcclusion = false, float rotationRadians = 0f, int collisionHeightPixels = 0)
    {
        _position = position;
        _texture = texture;
        _rotationRadians = rotationRadians;
        _collisionHeightPixels = collisionHeightPixels > 0 ? collisionHeightPixels : texture.Height;
        SuppressOcclusion = suppressOcclusion;
    }

    /// <summary>Top-left world position in pixels.</summary>
    public Vector2 Position => _position;

    /// <summary>Texture used for rendering and bounds calculation.</summary>
    public Texture2D Texture => _texture;

    /// <summary>When true, the reveal lens will not activate when a character walks behind this boulder.</summary>
    public bool SuppressOcclusion { get; }

    /// <summary>World-space blocking bounds for this boulder.</summary>
    public Rectangle Bounds => new(
        (int)_position.X,
        (int)_position.Y + _texture.Height - _collisionHeightPixels,
        _texture.Width,
        _collisionHeightPixels);

    /// <summary>
    /// Draws the boulder in world space.
    /// </summary>
    /// <param name="spriteBatch">Sprite batch for the world pass.</param>
    /// <param name="layerDepth">Depth value for Y-sorting (0 = back, 1 = front).</param>
    public void Draw(SpriteBatch spriteBatch, float layerDepth = 0f)
    {
        if (_rotationRadians == 0f)
        {
            spriteBatch.Draw(_texture, _position, null, Color.White, 0f, Vector2.Zero, 1f, SpriteEffects.None, layerDepth);
        }
        else
        {
            // Tiled rotates tile objects around their bottom-left anchor.
            var anchor = new Vector2(_position.X, _position.Y + _texture.Height);
            var origin = new Vector2(0f, _texture.Height);
            spriteBatch.Draw(_texture, anchor, null, Color.White, _rotationRadians, origin, 1f, SpriteEffects.None, layerDepth);
        }
    }
}