using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace RiverRats.Game;

public class Game1 : Microsoft.Xna.Framework.Game
{
    private GraphicsDeviceManager _graphics;
    private SpriteBatch _spriteBatch;
    private Texture2D _pixel;
    private Vector2 _markerPosition;
    private Vector2 _markerVelocity = new(180f, 120f);
    private readonly Color _backgroundColor = new(18, 24, 38);

    public Game1()
    {
        _graphics = new GraphicsDeviceManager(this);
        Content.RootDirectory = "Content";
        IsMouseVisible = true;
        Window.Title = "River Rats";

        _graphics.PreferredBackBufferWidth = 1280;
        _graphics.PreferredBackBufferHeight = 720;
    }

    protected override void Initialize()
    {
        _markerPosition = new Vector2(
            _graphics.PreferredBackBufferWidth * 0.5f,
            _graphics.PreferredBackBufferHeight * 0.5f);

        base.Initialize();
    }

    protected override void LoadContent()
    {
        _spriteBatch = new SpriteBatch(GraphicsDevice);
        _pixel = new Texture2D(GraphicsDevice, 1, 1);
        _pixel.SetData(new[] { Color.White });
    }

    protected override void Update(GameTime gameTime)
    {
        if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
            Exit();

        var elapsedSeconds = (float)gameTime.ElapsedGameTime.TotalSeconds;
        _markerPosition += _markerVelocity * elapsedSeconds;

        var viewport = GraphicsDevice.Viewport;
        const float markerSize = 64f;

        if (_markerPosition.X <= 0 || _markerPosition.X >= viewport.Width - markerSize)
        {
            _markerVelocity.X *= -1;
            _markerPosition.X = MathHelper.Clamp(_markerPosition.X, 0, viewport.Width - markerSize);
        }

        if (_markerPosition.Y <= 0 || _markerPosition.Y >= viewport.Height - markerSize)
        {
            _markerVelocity.Y *= -1;
            _markerPosition.Y = MathHelper.Clamp(_markerPosition.Y, 0, viewport.Height - markerSize);
        }

        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(_backgroundColor);

        _spriteBatch.Begin();
        _spriteBatch.Draw(_pixel, new Rectangle((int)_markerPosition.X, (int)_markerPosition.Y, 64, 64), Color.CadetBlue);
        _spriteBatch.Draw(_pixel, new Rectangle(0, GraphicsDevice.Viewport.Height - 96, GraphicsDevice.Viewport.Width, 96), new Color(29, 78, 137));
        _spriteBatch.End();

        base.Draw(gameTime);
    }
}
