using System;
using System.IO;
using FontStashSharp;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using DogDays.Game.Core;
using DogDays.Game.Data.Save;
using DogDays.Game.Input;

#nullable enable

namespace DogDays.Game.Screens;

/// <summary>
/// Title/start screen shown on boot. Displays the splash image with
/// "New Game" and "Continue" menu options.
/// </summary>
internal sealed class TitleScreen : IGameScreen
{
    private const float MenuFontSize = 14f;
    private const float MenuItemSpacing = 6f;
    private const float MenuBottomMargin = 40f;
    private const float CursorBlinkRate = 3f;

    private static readonly Color SelectedColor = Color.White;
    private static readonly Color UnselectedColor = new(180, 180, 180, 200);
    private static readonly Color DisabledColor = new(100, 100, 100, 120);
    private static readonly Color ShadowColor = new(0, 0, 0, 180);

    private readonly GraphicsDevice _graphicsDevice;
    private readonly ContentManager _content;
    private readonly int _virtualWidth;
    private readonly int _virtualHeight;
    private readonly ScreenManager _screenManager;
    private readonly GameSessionServices _gameSessionServices;
    private readonly Action _requestExit;

    private Texture2D _splashTexture = null!;
    private FontSystem _fontSystem = null!;
    private int _selectedIndex;
    private bool _hasSaveData;
    private float _elapsed;

    /// <inheritdoc />
    public bool IsTransparent => false;

    /// <summary>Currently selected menu index (0 = New Game, 1 = Continue).</summary>
    internal int SelectedIndex => _selectedIndex;

    /// <summary>Whether the Continue option is available.</summary>
    internal bool HasSaveData => _hasSaveData;

    /// <summary>
    /// Creates a new title screen.
    /// </summary>
    /// <param name="graphicsDevice">The graphics device.</param>
    /// <param name="content">Content manager for loading assets.</param>
    /// <param name="virtualWidth">Virtual resolution width.</param>
    /// <param name="virtualHeight">Virtual resolution height.</param>
    /// <param name="screenManager">Screen manager for screen transitions.</param>
    /// <param name="gameSessionServices">Shared session services (save, quests, events).</param>
    /// <param name="requestExit">Callback to request game exit.</param>
    internal TitleScreen(
        GraphicsDevice graphicsDevice,
        ContentManager content,
        int virtualWidth,
        int virtualHeight,
        ScreenManager screenManager,
        GameSessionServices gameSessionServices,
        Action requestExit)
    {
        _graphicsDevice = graphicsDevice;
        _content = content;
        _virtualWidth = virtualWidth;
        _virtualHeight = virtualHeight;
        _screenManager = screenManager;
        _gameSessionServices = gameSessionServices;
        _requestExit = requestExit;

        // Check all save slots for existing data (no GPU needed).
        _hasSaveData = false;
        for (var i = 0; i < _gameSessionServices.SaveGame.SlotCount; i++)
        {
            if (_gameSessionServices.SaveGame.HasSave(i))
            {
                _hasSaveData = true;
                break;
            }
        }

        // Default selection to Continue when a save exists.
        _selectedIndex = _hasSaveData ? 1 : 0;
    }

    /// <inheritdoc />
    public void LoadContent()
    {
        _splashTexture = _content.Load<Texture2D>("Sprites/splash_screen");

        _fontSystem = new FontSystem(new FontSystemSettings
        {
            FontResolutionFactor = 2f,
            KernelWidth = 0,
            KernelHeight = 0,
        });
        _fontSystem.AddFont(File.ReadAllBytes(
            Path.Combine(AppContext.BaseDirectory, _content.RootDirectory, "Fonts", "Nunito.ttf")));
    }

    /// <inheritdoc />
    public void Update(GameTime gameTime, IInputManager input)
    {
        _elapsed += (float)gameTime.ElapsedGameTime.TotalSeconds;

        if (input.IsPressed(InputAction.MoveUp))
        {
            _selectedIndex--;
            if (_selectedIndex < 0)
            {
                _selectedIndex = 1;
            }

            // Skip Continue if no save data.
            if (_selectedIndex == 1 && !_hasSaveData)
            {
                _selectedIndex = 0;
            }
        }

        if (input.IsPressed(InputAction.MoveDown))
        {
            _selectedIndex++;
            if (_selectedIndex > 1)
            {
                _selectedIndex = 0;
            }

            // Skip Continue if no save data.
            if (_selectedIndex == 1 && !_hasSaveData)
            {
                _selectedIndex = 0;
            }
        }

        if (input.IsPressed(InputAction.Confirm))
        {
            if (_selectedIndex == 0)
            {
                StartNewGame();
            }
            else if (_selectedIndex == 1 && _hasSaveData)
            {
                ContinueSavedGame();
            }
        }
    }

    /// <inheritdoc />
    public void Draw(GameTime gameTime, SpriteBatch spriteBatch)
    {
        // Draw splash image scaled to fill the virtual resolution.
        spriteBatch.Begin(samplerState: SamplerState.LinearClamp);
        spriteBatch.Draw(
            _splashTexture,
            new Rectangle(0, 0, _virtualWidth, _virtualHeight),
            Color.White);
        spriteBatch.End();
    }

    /// <inheritdoc />
    public void DrawOverlay(GameTime gameTime, SpriteBatch spriteBatch, int sceneScale)
    {
        var font = _fontSystem.GetFont(MenuFontSize * sceneScale);
        var screenWidth = _graphicsDevice.Viewport.Width;
        var screenHeight = _graphicsDevice.Viewport.Height;
        var spacing = (MenuItemSpacing + MenuFontSize) * sceneScale;
        var shadowOffset = MathF.Max(1f, sceneScale);

        // Position menu near bottom center.
        var menuY = screenHeight - MenuBottomMargin * sceneScale;

        string[] labels = { "New Game", "Continue" };

        spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp);

        for (var i = 0; i < labels.Length; i++)
        {
            var label = labels[i];
            var size = font.MeasureString(label);
            var x = (screenWidth - size.X) / 2f;
            var y = menuY + i * spacing;

            Color color;
            if (i == 1 && !_hasSaveData)
            {
                color = DisabledColor;
            }
            else if (i == _selectedIndex)
            {
                // Gentle pulse on the selected item.
                var pulse = 0.85f + 0.15f * MathF.Sin(_elapsed * CursorBlinkRate);
                color = SelectedColor * pulse;
            }
            else
            {
                color = UnselectedColor;
            }

            // Draw cursor indicator for the selected item.
            if (i == _selectedIndex && !(i == 1 && !_hasSaveData))
            {
                var cursorText = ">";
                var cursorSize = font.MeasureString(cursorText);
                var cursorX = x - cursorSize.X - 4f * sceneScale;
                spriteBatch.DrawString(font, cursorText,
                    new Vector2(cursorX + shadowOffset, y + shadowOffset), ShadowColor);
                spriteBatch.DrawString(font, cursorText,
                    new Vector2(cursorX, y), color);
            }

            // Shadow pass.
            spriteBatch.DrawString(font, label,
                new Vector2(x + shadowOffset, y + shadowOffset), ShadowColor);
            // Main text.
            spriteBatch.DrawString(font, label,
                new Vector2(x, y), color);
        }

        spriteBatch.End();
    }

    /// <inheritdoc />
    public void UnloadContent()
    {
        _fontSystem?.Dispose();
    }

    private void StartNewGame()
    {
        _gameSessionServices.ResetSessionState();

        _screenManager.Replace(new GameplayScreen(
            _graphicsDevice,
            _content,
            _virtualWidth,
            _virtualHeight,
            _screenManager,
            _gameSessionServices,
            _requestExit,
            "Maps/CabinBedroom",
            "default"));
    }

    private void ContinueSavedGame()
    {
        // Try auto-save slot first, then manual slots.
        SaveGameData? data = null;
        int loadedSlot = -1;

        for (var i = 0; i < _gameSessionServices.SaveGame.SlotCount; i++)
        {
            var candidate = _gameSessionServices.SaveGame.Load(i);
            if (candidate is not null && (data is null || candidate.SavedAtUtc > data.SavedAtUtc))
            {
                data = candidate;
                loadedSlot = i;
            }
        }

        if (data is null)
        {
            return;
        }

        _gameSessionServices.LastUsedSaveSlot = loadedSlot;
        _gameSessionServices.ResetSessionState();
        _gameSessionServices.LastUsedSaveSlot = loadedSlot;

        // Restore quest state before rebuilding the screen.
        SaveGameMapper.RestoreQuests(data, _gameSessionServices.Quests);
        _gameSessionServices.Quests.RebuildListsFromRestoredState();
        SaveGameMapper.RestoreProgression(data, _gameSessionServices);

        var savedPlayer = data.Player;
        var savedPosition = new Vector2(savedPlayer.X, savedPlayer.Y);

        _screenManager.Replace(new GameplayScreen(
            _graphicsDevice,
            _content,
            _virtualWidth,
            _virtualHeight,
            _screenManager,
            _gameSessionServices,
            _requestExit,
            savedPlayer.ZoneMapAssetName,
            spawnPointId: null,
            fadeInFromBlack: true,
            spawnPosition: savedPosition,
            saveDataToRestore: data));
    }
}
