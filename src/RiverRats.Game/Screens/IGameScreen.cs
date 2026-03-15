using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RiverRats.Game.Input;

namespace RiverRats.Game.Screens;

/// <summary>
/// Contract for a game screen that participates in the screen manager stack.
/// </summary>
public interface IGameScreen
{
    /// <summary>
    /// Whether screens below this one should still be drawn.
    /// True for overlays (pause), false for opaque screens (gameplay, title).
    /// </summary>
    bool IsTransparent { get; }

    /// <summary>
    /// Called once when the screen is first pushed onto the stack.
    /// Load assets and initialize state here.
    /// </summary>
    void LoadContent();

    /// <summary>
    /// Called every frame for the topmost screen only.
    /// </summary>
    void Update(GameTime gameTime, IInputManager input);

    /// <summary>
    /// Called every frame for all visible screens (from bottom to top).
    /// </summary>
    void Draw(GameTime gameTime, SpriteBatch spriteBatch);

    /// <summary>
    /// Called when the screen is removed from the stack. Dispose resources here.
    /// </summary>
    void UnloadContent();
}
