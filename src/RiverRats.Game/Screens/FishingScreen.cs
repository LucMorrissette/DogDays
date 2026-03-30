using System;
using System.Collections.Generic;
using System.IO;
using FontStashSharp;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using RiverRats.Game.Components;
using RiverRats.Game.Data;
using RiverRats.Game.Entities;
using RiverRats.Game.Graphics;
using RiverRats.Game.Input;
using RiverRats.Game.Systems;
using RiverRats.Game.Util;
using RiverRats.Game.World;

namespace RiverRats.Game.Screens;

/// <summary>
/// Side-view fishing mini-game screen. Renders the scene from a TMX map
/// designed in Tiled, with animated fish silhouettes swimming in the
/// underwater area. Cancel returns to the overworld.
/// </summary>
public sealed class FishingScreen : IGameScreen
{
    private const float ZoneTransitionFadeDurationSeconds = 0.4f;
    private const float ZoneTransitionBlackHoldSeconds = 0.15f;
    private const float HintFontSize = 10f;
    private const string HintText = "Press Cancel to return";
    private const string DefaultFishingMapAsset = "Maps/FishingSpot";

    /// <summary>Height of the sky cloud region in tile rows.</summary>
    private const int SkyCloudTileRows = 2;

    /// <summary>Tile height in pixels (matches the TMX tileheight).</summary>
    private const int TileHeightPx = 16;

    /// <summary>Top grass shelf row in the fishing map.</summary>
    private const int GrassShelfRow = 6;

    /// <summary>Left margin for the fishing characters on the grass shelf.</summary>
    private const float GrassShelfLeftMarginPx = 8f;

    /// <summary>Horizontal spacing between the follower and player (can be 0 for side-by-side).</summary>
    private const float CharacterSpacingPx = 0.5f;

    /// <summary>Name of the TMX object layer that defines swim bounds for fish.</summary>
    private const string SwimBoundsLayerName = "SwimBounds";

    private static readonly Color HintColor = new(200, 200, 200, 180);

    // Set to true to enable fish attraction / strike / hooking behavior.
    private static readonly bool FishAttractionEnabled = true;

    /// <summary>Screen shake intensity in pixels.</summary>
    private const float ShakeIntensity = 2.5f;

    /// <summary>Duration (seconds) of screen shake.</summary>
    private const float ShakeDuration = 0.25f;

    /// <summary>How fast the fish wiggles while hooked (rad/s).</summary>
    private const float WiggleSpeed = 18f;

    /// <summary>Maximum wiggle rotation in radians.</summary>
    private const float WiggleAmplitude = 0.15f;

    /// <summary>Number of splash particles emitted on breach.</summary>
    private const int SplashParticleCount = 12;

    // --- Water shader tuning ---
    private const float WaterAmplitude = 0.003f;
    private const float WaterFrequency = 20f;
    private const float WaterSpeed = 1.2f;
    private const float WaterRippleAmplitude = 0.012f;
    private const float WaterRippleFrequency = 35f;
    private const float WaterRippleSpeed = 20f;
    private const float WaterSplashBrightness = 0.45f;
    private const float WaterSplashRingSpeed = 25f;
    private const float WaterCausticIntensity = 0.08f;
    private const float WaterCausticScale = 8f;
    private const float WaterSpookBrightness = 0.55f;
    private const float WaterSpookRingSpeed = 12f;

    /// <summary>Duration (seconds) the catch toast is displayed.</summary>
    private const float ToastDurationSeconds = 2.5f;

    /// <summary>Font size for the catch toast text.</summary>
    private const float ToastFontSize = 10f;

    private static readonly Color ToastColor = new(255, 255, 200, 230);
    private static readonly Color ToastShadowColor = new(0, 0, 0, 160);

    // SFX variant counts.
    private const int CastSfxCount = 4;
    private const int PlopSfxCount = 4;
    private const int TwitchSfxCount = 3;
    private const int ReelSfxCount = 3;
    private const int StrikeSfxCount = 4;
    private const int CatchSfxCount = 3;

    // SFX playback volumes.
    private const float CastSfxVolume = 0.7f;
    private const float PlopSfxVolume = 0.65f;
    private const float TwitchSfxVolume = 0.5f;
    private const float ReelSfxVolume = 0.3f;
    private const float StrikeSfxVolume = 0.85f;
    private const float CatchSfxVolume = 0.8f;

    /// <summary>Minimum interval (seconds) between reel tick SFX to avoid overlapping clicks.</summary>
    private const float ReelTickIntervalSeconds = 0.08f;

    // Fish population counts per species.
    private const int MinnowCount = 4;
    private const int BassCount = 3;
    private const int CatfishCount = 2;

    /// <summary>Character sprite frame size in pixels.</summary>
    private const int CharacterFramePixels = 32;

    /// <summary>Walk animation columns per direction row on the sprite sheet.</summary>
    private const int WalkFramesPerDirection = 4;

    /// <summary>Seconds each walk animation frame is displayed.</summary>
    private const float WalkFrameDuration = 0.15f;

    /// <summary>
    /// Offset from the player position to place the idle fishing rod overlay.
    /// Aligns the rod handle with the character's right hand (facing right).
    /// </summary>
    private static readonly Vector2 FishingRodOffset = new(17f, -8f);

    /// <summary>
    /// Offset from the player position to place the wind-up fishing rod overlay.
    /// Rod is swept behind/above the character's head.
    /// </summary>
    private static readonly Vector2 FishingRodWindupOffset = new(-8f, -12f);

    /// <summary>
    /// Offset from the player position to place the cast-complete rod overlay.
    /// Same handle position as idle, rod has a slight droop.
    /// </summary>
    private static readonly Vector2 FishingRodCastOffset = new(17f, -8f);

    /// <summary>Delay before the power gauge appears after holding Confirm.</summary>
    private const float WindupDelaySeconds = 0.5f;

    /// <summary>Speed at which the power gauge needle oscillates (cycles per second).</summary>
    private const float GaugeSpeedCyclesPerSecond = 1.2f;

    /// <summary>Green zone half-width at minimum cast distance (fraction of gauge, 0–1).</summary>
    private const float GaugeGreenHalfClose = 0.20f;

    /// <summary>Green zone half-width at maximum cast distance (fraction of gauge, 0–1).</summary>
    private const float GaugeGreenHalfFar = 0.08f;

    /// <summary>Center of the green zone on the gauge (fraction, 0–1).</summary>
    private const float GaugeGreenCenter = 0.5f;

    // Power gauge visual dimensions (in virtual-resolution pixels).
    private const int GaugeWidth = 4;
    private const int GaugeHeight = 40;
    private const int GaugeMarginRight = 10;

    private static readonly Color GaugeBackColor = new(30, 30, 30, 200);
    private static readonly Color GaugeRedColor = new(200, 50, 40, 220);
    private static readonly Color GaugeGreenColor = new(50, 180, 60, 220);
    private static readonly Color GaugeNeedleColor = new(255, 255, 255, 255);

    /// <summary>Speed of the aim cursor in pixels per second.</summary>
    private const float AimSpeedPxPerSecond = 60f;

    /// <summary>Row where the water surface sits (matches the TMX map).</summary>
    private const int WaterSurfaceRow = 6;

    /// <summary>Left boundary (px) of the aimable water surface. Past the shore slope.</summary>
    private const float AimMinX = 96f;

    /// <summary>Y position of the aim arrow on the water surface.</summary>
    private static readonly float AimY = WaterSurfaceRow * TileHeightPx;

    /// <summary>Size of the aim arrow indicator in pixels.</summary>
    private const int AimArrowSize = 5;

    private static readonly Color AimArrowColor = new(255, 255, 80, 220);

    /// <summary>Duration of the lure flight arc in seconds.</summary>
    private const float LureFlightDurationSeconds = 0.6f;

    /// <summary>Peak height of the lure arc above the straight-line path (pixels).</summary>
    private const float LureArcHeight = 30f;

    /// <summary>How far off-target a bad (red-zone) cast lands (pixels).</summary>
    private const float BadCastMinOffset = 80f;
    private const float BadCastMaxOffset = 150f;

    /// <summary>Color of the fishing line.</summary>
    private static readonly Color LineColor = new(180, 180, 180, 160);

    /// <summary>Number of segments used to approximate the line curve.</summary>
    private const int LineSegments = 24;

    /// <summary>Duration (seconds) for the line to settle from taut to fully slack after landing.</summary>
    private const float LineSettleDurationSeconds = 0.8f;

    /// <summary>Maximum sag (pixels) of the slack catenary at full settle.</summary>
    private const float LineMaxSag = 22f;

    /// <summary>Small sag (pixels) that appears when the player stops reeling, so the line doesn't look perfectly taut.</summary>
    private const float LineRelaxSag = 14f;

    /// <summary>Pixels the fish breaches above the water surface during a strike.</summary>
    private const float StrikeBreachHeight = 10f;

    /// <summary>Total duration of the strike animation (lunge + breach + dive) in seconds.</summary>
    private const float StrikeDuration = 0.8f;

    /// <summary>Time within the strike when the breach reaches its peak.</summary>
    private const float StrikeBreachPeakTime = 0.3f;

    /// <summary>Time within the strike when the dive back down begins.</summary>
    private const float StrikeDiveStartTime = 0.4f;

    /// <summary>Speed (px/sec) at which a hooked fish is reeled toward the rod tip.</summary>
    private const float HookedReelSpeed = 30f;

    /// <summary>Speed (px/sec) at which a hooked fish drifts back when the player is not reeling.</summary>
    private const float HookedDriftBackSpeed = 2f;

    // --- Fish fight & line tension ---

    /// <summary>Minimum seconds between fight bursts.</summary>
    private const float FightCooldownMin = 2.0f;

    /// <summary>Maximum seconds between fight bursts.</summary>
    private const float FightCooldownMax = 5.0f;

    /// <summary>Duration (seconds) of a single fight burst.</summary>
    private const float FightBurstDuration = 1.2f;

    /// <summary>Speed (px/sec) the fish pulls away during a fight burst.</summary>
    private const float FightPullSpeed = 55f;

    /// <summary>Tension gained per second when reeling during a fight burst.</summary>
    private const float TensionReelDuringFight = 0.8f;

    /// <summary>Tension gained per second when reeling normally (no fight).</summary>
    private const float TensionReelNormal = 0.15f;

    /// <summary>Tension lost per second when NOT reeling.</summary>
    private const float TensionDecay = 0.45f;

    /// <summary>Tension value at which the line snaps (0–1 scale).</summary>
    private const float TensionSnapThreshold = 1.0f;

    /// <summary>Line color when tension is at maximum (danger).</summary>
    private static readonly Color LineDangerColor = new(255, 60, 40, 220);

    /// <summary>Stamina drained from the fish per fight burst (0–1 scale).</summary>
    private const float StaminaDrainPerBurst = 0.18f;

    /// <summary>Minimum stamina multiplier — even an exhausted fish puts up a feeble fight.</summary>
    private const float StaminaFloor = 0.15f;

    /// <summary>Rod offset for the hooked-rod sprite (same handle position as the cast rod).</summary>
    private static readonly Vector2 FishingRodHookedOffset = new(17f, -8f);

    /// <summary>Offset from the hooked-rod sprite origin to the bent rod tip.</summary>
    private static readonly Vector2 HookedRodTipLocalOffset = new(37f, 5f);

    /// <summary>Offset from the cast-rod sprite origin to the rod tip.</summary>
    private static readonly Vector2 RodTipLocalOffset = new(42f, 10f);

    /// <summary>Speed at which line slack is reeled in (sag pixels per second).</summary>
    private const float ReelSlackSpeed = 35f;

    /// <summary>Speed at which the lure is retrieved toward the rod tip (pixels per second).</summary>
    private const float ReelLureSpeed = 50f;

    /// <summary>Distance (px) the lure moves toward the player per twitch/pop.</summary>
    private const float TwitchDistancePx = 12f;

    /// <summary>Duration (seconds) of the rod flip-up animation on a twitch.</summary>
    private const float TwitchDurationSeconds = 0.15f;

    /// <summary>Time window (seconds) in which rapid twitches are counted.</summary>
    private const float RapidTwitchWindowSeconds = 0.8f;

    /// <summary>Max twitches within the window before they start spooking fish.</summary>
    private const int RapidTwitchSafeCount = 2;

    /// <summary>Rotation (radians) the rod tilts upward during a twitch. Negative = counter-clockwise.</summary>
    private const float TwitchRotation = -0.25f;

    /// <summary>Offset from the idle-rod sprite origin to the tip (where line attaches).</summary>
    private static readonly Vector2 IdleRodTipLocalOffset = new(40f, 6f);

    /// <summary>Length (pixels) of the hanging line in the idle pose.</summary>
    private const float IdleLineLengthPx = 13f;

    /// <summary>Horizontal amplitude (pixels) of the idle lure sway.</summary>
    private const float IdleSwayAmplitudePx = 1.5f;

    /// <summary>Speed of the idle lure sway (cycles per second).</summary>
    private const float IdleSwayCyclesPerSecond = 0.6f;

    /// <summary>Player top-left position on the shore (world pixels).</summary>
    private Vector2 _playerPosition;

    /// <summary>Follower top-left position on the shore (world pixels).</summary>
    private Vector2 _followerPosition;

    private readonly GraphicsDevice _graphicsDevice;
    private readonly ContentManager _content;
    private readonly int _virtualWidth;
    private readonly int _virtualHeight;
    private readonly ScreenManager _screenManager;
    private readonly Action _requestExit;
    private readonly string _returnMapName;
    private readonly Vector2 _returnPosition;
    private readonly float _dayNightProgress;
    private readonly string _fishingMapAsset;

    private Texture2D _pixelTexture;
    private Texture2D _fishTexture;
    private SimpleTiledRenderer _mapRenderer;
    private SkyCloudRenderer _skyCloudRenderer;
    private SpriteAnimator _playerAnimator;
    private SpriteAnimator _followerAnimator;
    private Texture2D _playerSpriteSheet;
    private Texture2D _followerSpriteSheet;
    private Texture2D _fishingRodTexture;
    private Texture2D _fishingRodWindupTexture;
    private Texture2D _fishingRodCastTexture;
    private Texture2D _fishingRodHookedTexture;
    private Texture2D _frogLureDangle;
    private Texture2D _frogLureRest;
    private Texture2D _frogLureActive;
    private FontSystem _fontSystem;
    private CastState _castState;
    private float _windupTimer;
    private float _gaugePhase;
    private bool _lastCastGood;
    private float _aimX;
    private float _aimMaxX;
    private Vector2 _lureStart;
    private Vector2 _lureEnd;
    private Vector2 _lurePosition;
    private float _lureFlightTime;
    private float _lineSettleTimer;
    private float _currentSag;
    private float _twitchTimer;
    private float _rapidTwitchWindow;
    private int _rapidTwitchCount;
    private float _swayTimer;
    private float _lureSwayOffset;

    /// <summary>The fish that bit the lure (null when no fish is hooked).</summary>
    private FishSilhouette _hookedFish;

    /// <summary>Current line tension (0 = slack, 1 = snap).</summary>
    private float _lineTension;

    /// <summary>Countdown until the next fight burst starts.</summary>
    private float _fightCooldown;

    /// <summary>Remaining duration of the current fight burst (0 = not fighting).</summary>
    private float _fightBurstTimer;

    /// <summary>Fish stamina (1 = fresh, 0 = exhausted). Decreases with each fight burst.</summary>
    private float _fishStamina;

    /// <summary>Whether the fish is currently in a fight burst.</summary>
    private bool IsFighting => _fightBurstTimer > 0f;

    /// <summary>Line color computed each frame from tension.</summary>
    private Color _currentLineColor = LineColor;

    /// <summary>Timer driving the strike animation phases.</summary>
    private float _strikeTimer;

    /// <summary>Position where the fish was when it struck.</summary>
    private Vector2 _strikeStartPos;

    /// <summary>Position the fish dives to after breaching (halfway depth).</summary>
    private Vector2 _hookTarget;

    private readonly List<FishSilhouette> _fish = new();
    private FadeState _fadeState;
    private float _fadeAlpha;
    private float _fadeHoldTimer;
    private string _toastText;
    private float _toastTimer;
    private static readonly Random _catchRng = new();

    // SFX arrays.
    private readonly SoundEffect[] _castSfx = new SoundEffect[CastSfxCount];
    private readonly SoundEffect[] _plopSfx = new SoundEffect[PlopSfxCount];
    private readonly SoundEffect[] _twitchSfx = new SoundEffect[TwitchSfxCount];
    private readonly SoundEffect[] _reelSfx = new SoundEffect[ReelSfxCount];
    private readonly SoundEffect[] _strikeSfx = new SoundEffect[StrikeSfxCount];
    private readonly SoundEffect[] _catchSfx = new SoundEffect[CatchSfxCount];
    private static readonly Random _sfxRng = new();
    private float _reelTickCooldown;

    // Screen shake state.
    private float _shakeTimer;
    private float _wiggleTimer;

    // Splash particles.
    private readonly List<SplashParticle> _splashParticles = new();

    // Water shader state.
    private Effect _fishingWaterEffect;
    private RenderTarget2D _waterRenderTarget;
    private FishingRippleManager _rippleManager;
    private float _waterElapsedSeconds;

    /// <inheritdoc />
    public bool IsTransparent => false;

    /// <summary>
    /// Creates a fishing mini-game screen.
    /// </summary>
    /// <param name="graphicsDevice">Graphics device for rendering.</param>
    /// <param name="content">Content manager for loading assets.</param>
    /// <param name="virtualWidth">Virtual resolution width.</param>
    /// <param name="virtualHeight">Virtual resolution height.</param>
    /// <param name="screenManager">Screen manager for transitions.</param>
    /// <param name="requestExit">Callback to request the game exit.</param>
    /// <param name="returnMapName">Map asset name to return to on exit.</param>
    /// <param name="returnPosition">Exact world-space position to place the player at when returning.</param>
    /// <param name="dayNightProgress">Day/night cycle progress to preserve across transitions.</param>
    /// <param name="fishingMapAsset">TMX map asset name for the fishing scene (defaults to <c>Maps/FishingSpot</c>).</param>
    public FishingScreen(
        GraphicsDevice graphicsDevice,
        ContentManager content,
        int virtualWidth,
        int virtualHeight,
        ScreenManager screenManager,
        Action requestExit,
        string returnMapName,
        Vector2 returnPosition,
        float dayNightProgress,
        string fishingMapAsset = DefaultFishingMapAsset)
    {
        _graphicsDevice = graphicsDevice ?? throw new ArgumentNullException(nameof(graphicsDevice));
        _content = content ?? throw new ArgumentNullException(nameof(content));
        _virtualWidth = virtualWidth;
        _virtualHeight = virtualHeight;
        _screenManager = screenManager ?? throw new ArgumentNullException(nameof(screenManager));
        _requestExit = requestExit ?? throw new ArgumentNullException(nameof(requestExit));
        _returnMapName = returnMapName ?? throw new ArgumentNullException(nameof(returnMapName));
        _returnPosition = returnPosition;
        _dayNightProgress = dayNightProgress;
        _fishingMapAsset = fishingMapAsset ?? DefaultFishingMapAsset;
    }

    /// <inheritdoc />
    public void LoadContent()
    {
        _pixelTexture = new Texture2D(_graphicsDevice, 1, 1);
        _pixelTexture.SetData(new[] { Color.White });

        _fontSystem = new FontSystem(new FontSystemSettings
        {
            FontResolutionFactor = 2f,
            KernelWidth = 0,
            KernelHeight = 0,
        });
        _fontSystem.AddFont(File.ReadAllBytes(
            Path.Combine(global::System.AppContext.BaseDirectory, _content.RootDirectory, "Fonts", "Nunito.ttf")));

        _mapRenderer = new SimpleTiledRenderer(_graphicsDevice, _content, _fishingMapAsset);
        CalculateCharacterPositions();

        // Aim defaults to the midpoint of the water surface.
        _aimMaxX = _mapRenderer.MapPixelWidth - AimArrowSize;
        _aimX = MathHelper.Lerp(AimMinX, _aimMaxX, 0.5f);

        var skyWidth = _mapRenderer.MapPixelWidth;
        var skyHeight = SkyCloudTileRows * TileHeightPx;
        _skyCloudRenderer = new SkyCloudRenderer(_graphicsDevice, skyWidth, skyHeight);
        _skyCloudRenderer.LoadContent(_content);

        _fishTexture = _content.Load<Texture2D>("Sprites/fish-silhouettes");

        _playerSpriteSheet = _content.Load<Texture2D>("Sprites/generic_character_sheet");
        _followerSpriteSheet = _content.Load<Texture2D>("Sprites/companion_character_sheet");
        _fishingRodTexture = _content.Load<Texture2D>("Sprites/fishing_rod");
        _fishingRodWindupTexture = _content.Load<Texture2D>("Sprites/fishing_rod_windup");
        _fishingRodCastTexture = _content.Load<Texture2D>("Sprites/fishing_rod_cast");
        _fishingRodHookedTexture = _content.Load<Texture2D>("Sprites/fishing_rod_hooked");
        _frogLureDangle = _content.Load<Texture2D>("Sprites/frog_lure1");
        _frogLureRest = _content.Load<Texture2D>("Sprites/frog_lure2");
        _frogLureActive = _content.Load<Texture2D>("Sprites/frog_lure3");

        // Load fishing SFX.
        for (var i = 0; i < CastSfxCount; i++)
            _castSfx[i] = _content.Load<SoundEffect>($"Audio/SFX/fishing_cast_{i:D2}");
        for (var i = 0; i < PlopSfxCount; i++)
            _plopSfx[i] = _content.Load<SoundEffect>($"Audio/SFX/fishing_plop_{i:D2}");
        for (var i = 0; i < TwitchSfxCount; i++)
            _twitchSfx[i] = _content.Load<SoundEffect>($"Audio/SFX/fishing_twitch_{i:D2}");
        for (var i = 0; i < ReelSfxCount; i++)
            _reelSfx[i] = _content.Load<SoundEffect>($"Audio/SFX/fishing_reel_{i:D2}");
        for (var i = 0; i < StrikeSfxCount; i++)
            _strikeSfx[i] = _content.Load<SoundEffect>($"Audio/SFX/fishing_strike_{i:D2}");
        for (var i = 0; i < CatchSfxCount; i++)
            _catchSfx[i] = _content.Load<SoundEffect>($"Audio/SFX/fishing_catch_{i:D2}");

        _playerAnimator = new SpriteAnimator(
            CharacterFramePixels, CharacterFramePixels,
            WalkFramesPerDirection, WalkFrameDuration)
        { Direction = FacingDirection.Right };
        _followerAnimator = new SpriteAnimator(
            CharacterFramePixels, CharacterFramePixels,
            WalkFramesPerDirection, WalkFrameDuration)
        { Direction = FacingDirection.Right };

        SpawnFish();

        // Water distortion / ripple / caustic shader.
        _fishingWaterEffect = _content.Load<Effect>("Effects/FishingWater");
        _fishingWaterEffect.Parameters["Amplitude"].SetValue(WaterAmplitude);
        _fishingWaterEffect.Parameters["Frequency"].SetValue(WaterFrequency);
        _fishingWaterEffect.Parameters["Speed"].SetValue(WaterSpeed);
        _fishingWaterEffect.Parameters["RippleAmplitude"].SetValue(WaterRippleAmplitude);
        _fishingWaterEffect.Parameters["RippleFrequency"].SetValue(WaterRippleFrequency);
        _fishingWaterEffect.Parameters["RippleSpeed"].SetValue(WaterRippleSpeed);
        _fishingWaterEffect.Parameters["SplashBrightness"].SetValue(WaterSplashBrightness);
        _fishingWaterEffect.Parameters["SplashRingSpeed"].SetValue(WaterSplashRingSpeed);
        _fishingWaterEffect.Parameters["CausticIntensity"].SetValue(WaterCausticIntensity);
        _fishingWaterEffect.Parameters["CausticScale"].SetValue(WaterCausticScale);
        _fishingWaterEffect.Parameters["SpookBrightness"].SetValue(WaterSpookBrightness);
        _fishingWaterEffect.Parameters["SpookRingSpeed"].SetValue(WaterSpookRingSpeed);
        _fishingWaterEffect.Parameters["AspectRatio"].SetValue((float)_virtualWidth / _virtualHeight);
        _fishingWaterEffect.Parameters["WaterSurfaceV"].SetValue((float)(WaterSurfaceRow * TileHeightPx) / _virtualHeight);
        _waterRenderTarget = new RenderTarget2D(
            _graphicsDevice, _virtualWidth, _virtualHeight,
            false, SurfaceFormat.Color, DepthFormat.None,
            0, RenderTargetUsage.DiscardContents);
        _rippleManager = new FishingRippleManager();

        // Start with a fade-in from black.
        _fadeState = FadeState.FadingIn;
        _fadeAlpha = 1f;
        _fadeHoldTimer = 0f;
    }

    /// <inheritdoc />
    public void Update(GameTime gameTime, IInputManager input)
    {
        if (_fadeState != FadeState.None)
        {
            UpdateFade(gameTime);
            return;
        }

        if (input.IsPressed(InputAction.Exit))
        {
            _requestExit();
            return;
        }

        if (input.IsPressed(InputAction.Cancel))
        {
            // If a fish is hooked or striking, cancel the hook — fish flees.
            if (_castState is CastState.FishStrike or CastState.FishHooked)
            {
                _hookedFish.SetRotation(0f);
                _hookedFish.Flee();
                _hookedFish = null;
                _castState = CastState.Idle;
                _twitchTimer = 0f;
                _currentSag = 0f;
                _lineTension = 0f;
                _currentLineColor = LineColor;
            }
            // If the lure is out, Cancel reels it in instantly.
            else if (_castState is CastState.LureFlying or CastState.CastComplete
                or CastState.ReelingSlack or CastState.ReelingLure)
            {
                _castState = CastState.Idle;
                _twitchTimer = 0f;
                _currentSag = 0f;
            }
            else
            {
                BeginReturnTransition();
            }
            return;
        }

        UpdateCastState(gameTime, input);
        UpdateAim(gameTime, input);

        _skyCloudRenderer.Update(gameTime);
        _rippleManager.Update(gameTime);

        var dt2 = (float)gameTime.ElapsedGameTime.TotalSeconds;
        _waterElapsedSeconds += dt2;

        // Tick catch toast timer.
        if (_toastTimer > 0f)
            _toastTimer -= dt2;

        // Tick screen shake.
        if (_shakeTimer > 0f)
            _shakeTimer -= dt2;

        // Tick reel click cooldown.
        if (_reelTickCooldown > 0f)
            _reelTickCooldown -= dt2;

        // Tick wiggle (hooked fish oscillation).
        if (_castState == CastState.FishHooked)
            _wiggleTimer += dt2;
        else
            _wiggleTimer = 0f;

        // Update splash particles.
        for (var i = _splashParticles.Count - 1; i >= 0; i--)
        {
            _splashParticles[i].Life -= dt2;
            if (_splashParticles[i].Life <= 0f)
            {
                _splashParticles.RemoveAt(i);
                continue;
            }
            _splashParticles[i].Velocity.Y += 120f * dt2; // gravity
            _splashParticles[i].Position += _splashParticles[i].Velocity * dt2;
        }

        for (var i = _fish.Count - 1; i >= 0; i--)
        {
            _fish[i].Update(gameTime);

            // Remove fleeing fish once they leave the screen.
            if (_fish[i].IsFleeing && _fish[i].Center.X > _virtualWidth + 32)
                _fish.RemoveAt(i);
        }
    }

    /// <inheritdoc />
    public void Draw(GameTime gameTime, SpriteBatch spriteBatch)
    {
        // Compute shake offset.
        var shakeOffset = Vector2.Zero;
        if (_shakeTimer > 0f)
        {
            var magnitude = ShakeIntensity * (_shakeTimer / ShakeDuration);
            shakeOffset = new Vector2(
                (float)(_catchRng.NextDouble() * 2 - 1) * magnitude,
                (float)(_catchRng.NextDouble() * 2 - 1) * magnitude);
        }
        var shakeMatrix = Matrix.CreateTranslation(shakeOffset.X, shakeOffset.Y, 0f);

        // --- Pass 1: Render water layer to render target ---
        var previousTarget = _graphicsDevice.GetRenderTargets().Length > 0
            ? _graphicsDevice.GetRenderTargets()[0].RenderTarget as RenderTarget2D
            : null;
        _graphicsDevice.SetRenderTarget(_waterRenderTarget);
        _graphicsDevice.Clear(Color.Transparent);
        _mapRenderer.DrawLayer("water", Matrix.Identity);
        _graphicsDevice.SetRenderTarget(previousTarget);

        // --- Pass 2: Composite water with distortion shader ---
        _fishingWaterEffect.Parameters["Time"].SetValue(_waterElapsedSeconds);
        _rippleManager.SetShaderParameters(_fishingWaterEffect, _virtualWidth, _virtualHeight);
        spriteBatch.Begin(
            sortMode: SpriteSortMode.Deferred,
            blendState: BlendState.AlphaBlend,
            samplerState: SamplerState.LinearClamp,
            effect: _fishingWaterEffect,
            transformMatrix: shakeMatrix);
        spriteBatch.Draw(
            _waterRenderTarget,
            new Rectangle(0, 0, _virtualWidth, _virtualHeight),
            Color.White);
        spriteBatch.End();

        // --- Pass 3: Non-water tile layers (sky, shore, details) ---
        _mapRenderer.DrawLayer("sky", shakeMatrix);
        _mapRenderer.DrawLayer("Shore", shakeMatrix);
        _mapRenderer.DrawLayer("Details", shakeMatrix);

        // Draw procedural clouds over the sky region (top rows).
        _skyCloudRenderer.Draw(spriteBatch);

        // Draw player and follower standing on the shore.
        spriteBatch.Begin(blendState: BlendState.AlphaBlend, samplerState: SamplerState.PointClamp, transformMatrix: shakeMatrix);
        _followerAnimator.Draw(spriteBatch, _followerSpriteSheet, _followerPosition);
        _playerAnimator.Draw(spriteBatch, _playerSpriteSheet, _playerPosition);

        var rodTexture = _castState switch
        {
            CastState.WindingUp => _fishingRodWindupTexture,
            CastState.Charging => _fishingRodWindupTexture,
            CastState.LureFlying => _fishingRodCastTexture,
            CastState.CastComplete => _fishingRodCastTexture,
            CastState.ReelingSlack => _fishingRodCastTexture,
            CastState.ReelingLure => _fishingRodCastTexture,
            CastState.FishStrike => _fishingRodHookedTexture,
            CastState.FishHooked => _fishingRodHookedTexture,
            _ => _fishingRodTexture,
        };
        var rodOffset = _castState switch
        {
            CastState.WindingUp => FishingRodWindupOffset,
            CastState.Charging => FishingRodWindupOffset,
            CastState.LureFlying => FishingRodCastOffset,
            CastState.CastComplete => FishingRodCastOffset,
            CastState.ReelingSlack => FishingRodCastOffset,
            CastState.ReelingLure => FishingRodCastOffset,
            CastState.FishStrike => FishingRodHookedOffset,
            CastState.FishHooked => FishingRodHookedOffset,
            _ => FishingRodOffset,
        };
        if (_twitchTimer <= 0f)
        {
            spriteBatch.Draw(rodTexture, _playerPosition + rodOffset, Color.White);
        }
        else
        {
            // During a twitch, rotate the rod slightly upward around the handle.
            var twitchT = _twitchTimer / TwitchDurationSeconds;
            var rotation = TwitchRotation * twitchT;
            var origin = new Vector2(5f, 30f); // Handle position in the rod sprite.
            spriteBatch.Draw(
                rodTexture,
                _playerPosition + rodOffset + origin,
                null,
                Color.White,
                rotation,
                origin,
                1f,
                SpriteEffects.None,
                0f);
        }

        // Draw the power gauge when charging.
        if (_castState == CastState.Charging)
        {
            DrawPowerGauge(spriteBatch);
        }

        // Draw the fishing line and lure.
        if (_castState == CastState.Idle)
        {
            DrawIdleLineAndLure(spriteBatch);
        }
        else if (_castState is CastState.FishStrike or CastState.FishHooked)
        {
            // Draw taut line from hooked rod tip to hooked fish center, but no lure.
            DrawFishingLine(spriteBatch);
        }
        else if (_castState is CastState.LureFlying or CastState.CastComplete
            or CastState.ReelingSlack or CastState.ReelingLure)
        {
            DrawFishingLine(spriteBatch);
            DrawLure(spriteBatch);
        }

        // Draw the aim arrow when the player can still adjust aim.
        if (_castState is CastState.Idle or CastState.WindingUp)
        {
            DrawAimArrow(spriteBatch);
        }

        spriteBatch.End();

        // Draw fish, splash particles, and fade overlay on top.
        spriteBatch.Begin(blendState: BlendState.AlphaBlend, samplerState: SamplerState.PointClamp, transformMatrix: shakeMatrix);

        for (var i = 0; i < _fish.Count; i++)
        {
            _fish[i].Draw(spriteBatch, _fishTexture, Color.White);
        }

        // Draw splash particles with varied sizes.
        for (var i = 0; i < _splashParticles.Count; i++)
        {
            var p = _splashParticles[i];
            var alpha = MathHelper.Clamp(p.Life / 0.3f, 0f, 1f);
            var size = p.Size;
            var tint = Color.Lerp(p.Tint, Color.Transparent, 1f - alpha);
            spriteBatch.Draw(_pixelTexture,
                new Rectangle((int)p.Position.X, (int)p.Position.Y, size, size),
                tint);
        }

        // Fade overlay.
        if (_fadeAlpha > 0f)
        {
            spriteBatch.Draw(_pixelTexture,
                new Rectangle(0, 0, _virtualWidth, _virtualHeight),
                new Color(0, 0, 0, _fadeAlpha));
        }

        spriteBatch.End();
    }

    /// <inheritdoc />
    public void DrawOverlay(GameTime gameTime, SpriteBatch spriteBatch, int sceneScale)
    {
        if (_fadeState != FadeState.None)
        {
            return;
        }

        var font = _fontSystem.GetFont(HintFontSize * sceneScale);
        var textSize = font.MeasureString(HintText);

        var viewport = _graphicsDevice.Viewport;
        var position = new Vector2(
            (viewport.Width - textSize.X) / 2f,
            viewport.Height - textSize.Y - (8f * sceneScale));

        spriteBatch.Begin(
            sortMode: SpriteSortMode.Deferred,
            blendState: BlendState.AlphaBlend,
            samplerState: SamplerState.LinearClamp);
        spriteBatch.DrawString(font, HintText, position, HintColor);
        spriteBatch.End();

        // Draw catch toast if active.
        if (_toastTimer > 0f && _toastText != null)
        {
            var toastFont = _fontSystem.GetFont(ToastFontSize * sceneScale);
            var toastSize = toastFont.MeasureString(_toastText);
            var toastPos = new Vector2(
                (viewport.Width - toastSize.X) / 2f,
                (_playerPosition.Y - 12f) * sceneScale);

            // Fade out in the last 0.5s.
            float alpha = _toastTimer < 0.5f ? _toastTimer / 0.5f : 1f;
            var color = ToastColor * alpha;
            var shadow = ToastShadowColor * alpha;

            spriteBatch.Begin(
                sortMode: SpriteSortMode.Deferred,
                blendState: BlendState.AlphaBlend,
                samplerState: SamplerState.LinearClamp);
            spriteBatch.DrawString(toastFont, _toastText, toastPos + new Vector2(sceneScale, sceneScale), shadow);
            spriteBatch.DrawString(toastFont, _toastText, toastPos, color);
            spriteBatch.End();
        }
    }

    /// <inheritdoc />
    public void UnloadContent()
    {
        _pixelTexture?.Dispose();
        _fontSystem?.Dispose();
        _skyCloudRenderer?.UnloadContent();
        _waterRenderTarget?.Dispose();
        _mapRenderer?.Dispose();
    }

    private void BeginReturnTransition()
    {
        _fadeState = FadeState.FadingOut;
        _fadeAlpha = 0f;
    }

    private void ShowCatchToast(FishSilhouette fish)
    {
        var name = fish.Species switch
        {
            FishSilhouette.FishType.Minnow => "Minnow",
            FishSilhouette.FishType.Bass => "Bass",
            FishSilhouette.FishType.Catfish => "Catfish",
            _ => "Fish",
        };

        // Weight range by species (in lbs).
        var (minW, maxW) = fish.Species switch
        {
            FishSilhouette.FishType.Minnow => (0.1f, 0.5f),
            FishSilhouette.FishType.Bass => (1.0f, 6.0f),
            FishSilhouette.FishType.Catfish => (3.0f, 15.0f),
            _ => (0.5f, 3.0f),
        };

        var weight = minW + (float)_catchRng.NextDouble() * (maxW - minW);
        _toastText = $"{name} — {weight:F1} lbs";
        _toastTimer = ToastDurationSeconds;
    }

    private static readonly Color[] SplashTints =
    {
        new(200, 230, 255, 220),  // light blue
        new(180, 220, 255, 200),  // pale cyan
        new(255, 255, 255, 200),  // white
        new(160, 210, 240, 180),  // soft blue
    };

    private void SpawnSplash(float x, float y)
    {
        // Spawn CPU particles with varied sizes and tints.
        for (var i = 0; i < SplashParticleCount; i++)
        {
            var angle = -MathHelper.PiOver2 + ((float)_catchRng.NextDouble() - 0.5f) * MathHelper.Pi * 0.8f;
            var speed = 25f + (float)_catchRng.NextDouble() * 60f;
            var size = _catchRng.Next(1, 4); // 1–3 px
            var tint = SplashTints[_catchRng.Next(SplashTints.Length)];
            _splashParticles.Add(new SplashParticle
            {
                Position = new Vector2(x + (float)(_catchRng.NextDouble() * 8 - 4), y),
                Velocity = new Vector2(MathF.Cos(angle) * speed, MathF.Sin(angle) * speed),
                Life = 0.35f + (float)_catchRng.NextDouble() * 0.25f,
                Size = size,
                Tint = tint,
            });
        }

        // Spawn shader ripple distortion and splash highlight ring.
        var splashPos = new Vector2(x, y);
        _rippleManager.SpawnRipple(splashPos);
        _rippleManager.SpawnSplash(splashPos);
    }

    /// <summary>
    /// Spawns a heavier splash for the lure landing — more particles, extra
    /// offset ripples, and multiple highlight rings for stronger visual feedback.
    /// </summary>
    private void SpawnLureLandingSplash(float x, float y)
    {
        // Base splash particles (reuse shared method for the core burst).
        SpawnSplash(x, y);

        // Extra wider particles for a bigger plume.
        for (var i = 0; i < 6; i++)
        {
            var angle = -MathHelper.PiOver2 + ((float)_catchRng.NextDouble() - 0.5f) * MathHelper.Pi * 1.0f;
            var speed = 40f + (float)_catchRng.NextDouble() * 45f;
            var size = _catchRng.Next(2, 5);
            var tint = SplashTints[_catchRng.Next(SplashTints.Length)];
            _splashParticles.Add(new SplashParticle
            {
                Position = new Vector2(x + (float)(_catchRng.NextDouble() * 12 - 6), y),
                Velocity = new Vector2(MathF.Cos(angle) * speed, MathF.Sin(angle) * speed),
                Life = 0.4f + (float)_catchRng.NextDouble() * 0.3f,
                Size = size,
                Tint = tint,
            });
        }

        // Secondary ripples offset slightly left/right for a wider disturbance.
        _rippleManager.SpawnRipple(new Vector2(x - 6f, y));
        _rippleManager.SpawnRipple(new Vector2(x + 6f, y));

        // Second splash highlight ring for a brighter flash.
        _rippleManager.SpawnSplash(new Vector2(x, y));
    }

    private static readonly Color[] BadSplashTints =
    {
        new(255, 80, 60, 220),    // red
        new(255, 120, 80, 200),   // orange-red
        new(255, 60, 40, 200),    // deep red
        new(220, 90, 70, 180),    // muted red
    };

    /// <summary>
    /// Spawns a red-tinted splash for a bad cast — spook rings + red particles.
    /// </summary>
    private void SpawnBadCastSplash(float x, float y)
    {
        // Red CPU particles.
        for (var i = 0; i < SplashParticleCount; i++)
        {
            var angle = -MathHelper.PiOver2 + ((float)_catchRng.NextDouble() - 0.5f) * MathHelper.Pi * 0.9f;
            var speed = 30f + (float)_catchRng.NextDouble() * 55f;
            var size = _catchRng.Next(1, 4);
            var tint = BadSplashTints[_catchRng.Next(BadSplashTints.Length)];
            _splashParticles.Add(new SplashParticle
            {
                Position = new Vector2(x + (float)(_catchRng.NextDouble() * 8 - 4), y),
                Velocity = new Vector2(MathF.Cos(angle) * speed, MathF.Sin(angle) * speed),
                Life = 0.35f + (float)_catchRng.NextDouble() * 0.25f,
                Size = size,
                Tint = tint,
            });
        }

        // Shader ripple distortion at impact.
        var pos = new Vector2(x, y);
        _rippleManager.SpawnRipple(pos);

        // Red spook rings — the key visual cue.
        _rippleManager.SpawnSpookRing(pos);
        _rippleManager.SpawnSpookRing(new Vector2(x - 5f, y));
        _rippleManager.SpawnSpookRing(new Vector2(x + 5f, y));
    }

    /// <summary>Plays a reel tick SFX if the cooldown has elapsed.</summary>
    private void PlayReelTick()
    {
        if (_reelTickCooldown <= 0f)
        {
            _reelSfx[_sfxRng.Next(ReelSfxCount)].Play(ReelSfxVolume, 0f, 0f);
            _reelTickCooldown = ReelTickIntervalSeconds;
        }
    }

    private void SpawnFish()
    {
        var rng = new Random();

        // Prefer polygon swim bounds, fall back to rectangle, then full map.
        PolygonBounds swimBounds;
        var polyBounds = _mapRenderer.GetObjectPolygons(SwimBoundsLayerName);
        if (polyBounds.Count > 0)
        {
            swimBounds = polyBounds[0];
        }
        else
        {
            var rectBounds = _mapRenderer.GetObjectRectangles(SwimBoundsLayerName);
            swimBounds = rectBounds.Count > 0
                ? PolygonBounds.FromRectangle(rectBounds[0])
                : PolygonBounds.FromRectangle(new Rectangle(0, 0, _mapRenderer.MapPixelWidth, _mapRenderer.MapPixelHeight));
        }

        SpawnSpecies(FishSilhouette.FishType.Minnow, MinnowCount, swimBounds, rng);

        // Larger species prefer deeper water — restrict their vertical range.
        var bassBounds = swimBounds.SliceHorizontal(0.35f);
        SpawnSpecies(FishSilhouette.FishType.Bass, BassCount, bassBounds, rng);

        var catfishBounds = swimBounds.SliceHorizontal(0.55f);
        SpawnSpecies(FishSilhouette.FishType.Catfish, CatfishCount, catfishBounds, rng);
    }

    private void SpawnSpecies(FishSilhouette.FishType type, int count, PolygonBounds swimBounds, Random rng)
    {
        for (var i = 0; i < count; i++)
        {
            // RandomPointInside returns a point inside the polygon.
            // Offset by half-sprite so the fish center sits at that point.
            var pos = swimBounds.RandomPointInside(rng) - FishSilhouette.SpriteHalfSize;
            var fish = new FishSilhouette(type, pos, swimBounds, rng);
            _fish.Add(fish);
        }
    }

    private void CalculateCharacterPositions()
    {
        // The standing baseline is 1px above the grass row top so
        //  feet rest on the surface.
        var standingBaselineY = GrassShelfRow * TileHeightPx + 3f;
        var characterTopY = standingBaselineY - CharacterFramePixels;

        _followerPosition = new Vector2(GrassShelfLeftMarginPx, characterTopY);
        _playerPosition = new Vector2(
            _followerPosition.X + CharacterFramePixels + CharacterSpacingPx,
            characterTopY);
    }

    private void UpdateFade(GameTime gameTime)
    {
        var fadeStep = (float)(gameTime.ElapsedGameTime.TotalSeconds / ZoneTransitionFadeDurationSeconds);

        if (_fadeState == FadeState.FadingOut)
        {
            _fadeAlpha = MathHelper.Clamp(_fadeAlpha + fadeStep, 0f, 1f);
            if (_fadeAlpha >= 1f)
            {
                _fadeState = FadeState.HoldingBlack;
                _fadeHoldTimer = ZoneTransitionBlackHoldSeconds;
            }

            return;
        }

        if (_fadeState == FadeState.HoldingBlack)
        {
            _fadeAlpha = 1f;
            _fadeHoldTimer -= (float)gameTime.ElapsedGameTime.TotalSeconds;

            if (_fadeHoldTimer <= 0f)
            {
                _screenManager.Replace(new GameplayScreen(
                    _graphicsDevice,
                    _content,
                    _virtualWidth,
                    _virtualHeight,
                    _screenManager,
                    _requestExit,
                    _returnMapName,
                    fadeInFromBlack: true,
                    dayNightStartProgress: _dayNightProgress,
                    spawnPosition: _returnPosition));
            }

            return;
        }

        if (_fadeState == FadeState.FadingIn)
        {
            _fadeAlpha = MathHelper.Clamp(_fadeAlpha - fadeStep, 0f, 1f);
            if (_fadeAlpha <= 0f)
            {
                _fadeState = FadeState.None;
            }
        }
    }

    private enum FadeState
    {
        None,
        FadingOut,
        HoldingBlack,
        FadingIn,
    }

    /// <summary>Simple particle for water splash effects.</summary>
    private sealed class SplashParticle
    {
        public Vector2 Position;
        public Vector2 Velocity;
        public float Life;
        public int Size;
        public Color Tint;
    }

    private enum CastState
    {
        /// <summary>Rod at rest in front with bobber.</summary>
        Idle,

        /// <summary>Rod behind head, waiting for delay before gauge.</summary>
        WindingUp,

        /// <summary>Power gauge oscillating — release to cast.</summary>
        Charging,

        /// <summary>Lure is flying through the air toward the target.</summary>
        LureFlying,

        /// <summary>Rod in front with slight droop, lure has landed.</summary>
        CastComplete,

        /// <summary>Line slack is being reeled in (sag decreasing, lure stationary).</summary>
        ReelingSlack,

        /// <summary>Line is taut, lure is being retrieved toward the rod tip.</summary>
        ReelingLure,

        /// <summary>Fish lunging + breach animation.</summary>
        FishStrike,

        /// <summary>Fish on the line, player reels in.</summary>
        FishHooked,
    }

    private void UpdateCastState(GameTime gameTime, IInputManager input)
    {
        var dt = (float)gameTime.ElapsedGameTime.TotalSeconds;

        // Track the lure event for this frame (used by fish attraction).
        var currentLureEvent = FishSilhouette.LureEvent.None;

        switch (_castState)
        {
            case CastState.Idle:
                _swayTimer += dt;
                if (input.IsHeld(InputAction.Confirm))
                {
                    _castState = CastState.WindingUp;
                    _windupTimer = 0f;
                    _swayTimer = 0f;
                    _lureSwayOffset = 0f;
                }
                break;

            case CastState.WindingUp:
                if (!input.IsHeld(InputAction.Confirm))
                {
                    // Released too early — return to idle.
                    _castState = CastState.Idle;
                    break;
                }

                _windupTimer += dt;
                if (_windupTimer >= WindupDelaySeconds)
                {
                    _castState = CastState.Charging;
                    _gaugePhase = 0f;
                }
                break;

            case CastState.Charging:
                _gaugePhase += dt * GaugeSpeedCyclesPerSecond;

                if (!input.IsHeld(InputAction.Confirm))
                {
                    // Released — evaluate the gauge position and launch the lure.
                    var gaugeValue = GaugeValue();
                    var (greenStart, greenEnd) = GaugeGreenZone();
                    _lastCastGood = gaugeValue >= greenStart
                                 && gaugeValue <= greenEnd;
                    BeginLureFlight(gaugeValue, greenStart, greenEnd);
                    _castSfx[_sfxRng.Next(CastSfxCount)].Play(CastSfxVolume, 0f, 0f);
                }
                break;

            case CastState.LureFlying:
                _lureFlightTime += dt;
                if (_lureFlightTime >= LureFlightDurationSeconds)
                {
                    _lureFlightTime = LureFlightDurationSeconds;
                    _lurePosition = _lureEnd;
                    _lineSettleTimer = 0f;
                    _castState = CastState.CastComplete;
                    currentLureEvent = _lastCastGood
                        ? FishSilhouette.LureEvent.Splash
                        : FishSilhouette.LureEvent.BadSplash;
                    _plopSfx[_sfxRng.Next(PlopSfxCount)].Play(PlopSfxVolume, 0f, 0f);
                    if (_lastCastGood)
                    {
                        SpawnLureLandingSplash(_lurePosition.X, AimY);
                    }
                    else
                    {
                        SpawnBadCastSplash(_lurePosition.X, AimY);
                    }
                }
                else
                {
                    var t = _lureFlightTime / LureFlightDurationSeconds;
                    _lurePosition = Vector2.Lerp(_lureStart, _lureEnd, t);
                    // Arc: raise the lure above the straight line using a sine curve.
                    _lurePosition.Y -= LureArcHeight * MathF.Sin(MathF.PI * t);
                }
                break;

            case CastState.CastComplete:
                // Advance the line slack settling animation.
                if (_lineSettleTimer < LineSettleDurationSeconds)
                {
                    _lineSettleTimer += dt;
                    // Keep _currentSag in sync while settling.
                    var settleT = MathHelper.Clamp(_lineSettleTimer / LineSettleDurationSeconds, 0f, 1f);
                    _currentSag = LineMaxSag * (1f - (1f - settleT) * (1f - settleT));
                }

                // Tick rapid-twitch window.
                if (_rapidTwitchWindow > 0f)
                    _rapidTwitchWindow -= dt;

                // Tick twitch timer.
                if (_twitchTimer > 0f)
                {
                    _twitchTimer -= dt;
                    if (_twitchTimer <= 0f)
                    {
                        _twitchTimer = 0f;
                        // Line relaxes back to a gentle slack after the twitch.
                        _currentSag = LineRelaxSag;
                    }
                }

                // Tap MoveLeft to pop/twitch the lure toward the player.
                if (input.IsPressed(InputAction.MoveLeft))
                {
                    var rodTip = _playerPosition + FishingRodCastOffset + RodTipLocalOffset;
                    _lurePosition.X = MathHelper.Max(_lurePosition.X - TwitchDistancePx, rodTip.X);
                    _currentSag = MathHelper.Max(_currentSag - 3f, 0f);
                    _twitchTimer = TwitchDurationSeconds;
                    _twitchSfx[_sfxRng.Next(TwitchSfxCount)].Play(TwitchSfxVolume, 0f, 0f);

                    // Track rapid twitches — reset counter if the window expired.
                    if (_rapidTwitchWindow <= 0f)
                        _rapidTwitchCount = 0;
                    _rapidTwitchCount++;
                    _rapidTwitchWindow = RapidTwitchWindowSeconds;

                    var twitchPos = new Vector2(_lurePosition.X, AimY);
                    _rippleManager.SpawnRipple(twitchPos);

                    if (_rapidTwitchCount > RapidTwitchSafeCount)
                    {
                        // Too many twitches in quick succession — spook!
                        currentLureEvent = FishSilhouette.LureEvent.BadSplash;
                        _rippleManager.SpawnSpookRing(twitchPos);
                    }
                    else
                    {
                        // Normal twitch — white attract ring.
                        currentLureEvent = FishSilhouette.LureEvent.Twitch;
                        _rippleManager.SpawnSplash(twitchPos);
                    }

                    // Check for fish strike — a twitch while a fish is strike-ready triggers a bite.
                    if (FishAttractionEnabled)
                    for (int i = 0; i < _fish.Count; i++)
                    {
                        if (_fish[i].Attraction == FishSilhouette.AttractionState.StrikeReady)
                        {
                            _hookedFish = _fish[i];
                            _hookedFish.SetHooked();
                            _strikeStartPos = _hookedFish.Center;
                            _strikeTimer = 0f;
                            _twitchTimer = 0f; // Cancel the twitch rotation for the hooked rod.
                            _hookTarget = new Vector2(_lurePosition.X, AimY + (_virtualHeight - AimY) * 0.5f);
                            _castState = CastState.FishStrike;
                            _shakeTimer = ShakeDuration;
                            SpawnSplash(_lurePosition.X, AimY);
                            // Red spook rings — the strike scatters nearby fish.
                            var strikePos = new Vector2(_lurePosition.X, AimY);
                            _rippleManager.SpawnSpookRing(strikePos);
                            _rippleManager.SpawnSpookRing(new Vector2(_lurePosition.X - 5f, AimY));
                            _strikeSfx[_sfxRng.Next(StrikeSfxCount)].Play(StrikeSfxVolume, 0f, 0f);

                            // Scatter all attracted fish — a strike is violent enough
                            // to reset everything. Unaware fish nearby also scatter.
                            for (int j = 0; j < _fish.Count; j++)
                            {
                                if (_fish[j] != _hookedFish)
                                {
                                    var state = _fish[j].Attraction;
                                    if (state is FishSilhouette.AttractionState.Curious
                                        or FishSilhouette.AttractionState.Approaching
                                        or FishSilhouette.AttractionState.StrikeReady)
                                    {
                                        _fish[j].Spook();
                                    }
                                    else
                                    {
                                        var dist = Vector2.Distance(_fish[j].Center, _hookedFish.Center);
                                        if (dist < 80f)
                                            _fish[j].Spook();
                                    }
                                }
                            }
                            break;
                        }
                    }
                }

                // Sway the lure when it's hanging in the air (past the shoreline).
                if (_lurePosition.Y < AimY)
                {
                    _swayTimer += dt;
                    _lureSwayOffset = (float)Math.Sin(_swayTimer * IdleSwayCyclesPerSecond * MathHelper.TwoPi) * IdleSwayAmplitudePx;
                }
                else
                {
                    _lureSwayOffset = 0f;
                }

                // Hold Confirm to start reeling.
                if (input.IsHeld(InputAction.Confirm))
                {
                    _lureSwayOffset = 0f;
                    _swayTimer = 0f;
                    currentLureEvent = FishSilhouette.LureEvent.ReelTick;
                    _castState = _currentSag > 0f
                        ? CastState.ReelingSlack
                        : CastState.ReelingLure;
                    PlayReelTick();
                }
                break;

            case CastState.ReelingSlack:
                if (!input.IsHeld(InputAction.Confirm))
                {
                    // Released — stop reeling, add slight relaxation.
                    _currentSag = MathHelper.Max(_currentSag, LineRelaxSag);
                    _castState = CastState.CastComplete;
                    break;
                }

                currentLureEvent = FishSilhouette.LureEvent.ReelTick;
                PlayReelTick();

                // Reduce sag toward zero.
                _currentSag -= ReelSlackSpeed * dt;
                if (_currentSag <= 0f)
                {
                    _currentSag = 0f;
                    _castState = CastState.ReelingLure;
                }
                break;

            case CastState.ReelingLure:
            {
                if (!input.IsHeld(InputAction.Confirm))
                {
                    // Released — stop reeling, add slight relaxation.
                    _currentSag = LineRelaxSag;
                    _castState = CastState.CastComplete;
                    break;
                }

                currentLureEvent = FishSilhouette.LureEvent.ReelTick;
                PlayReelTick();

                // Drag the lure along the water surface toward the shore,
                // then up to the rod tip once it reaches the shoreline.
                var rodTip = _playerPosition + FishingRodCastOffset + RodTipLocalOffset;
                var shoreX = rodTip.X;

                if (_lurePosition.X > shoreX)
                {
                    // Still on the water — slide horizontally at the surface.
                    _lurePosition.X -= ReelLureSpeed * dt;
                    _lurePosition.Y = AimY;

                    if (_lurePosition.X <= shoreX)
                    {
                        _lurePosition.X = shoreX;
                    }
                }
                else
                {
                    // Past the shoreline — reel directly up to the rod tip.
                    var toTip = rodTip - _lurePosition;
                    var dist = toTip.Length();
                    if (dist <= ReelLureSpeed * dt)
                    {
                        _lurePosition = rodTip;
                        _castState = CastState.Idle;
                    }
                    else
                    {
                        _lurePosition += Vector2.Normalize(toTip) * ReelLureSpeed * dt;
                    }
                }
                break;
            }

            case CastState.FishStrike:
            {
                _strikeTimer += dt;

                if (_strikeTimer < StrikeBreachPeakTime)
                {
                    // Phase 1: Fish lunges from start position toward lure, then breaches above surface.
                    var t = _strikeTimer / StrikeBreachPeakTime;
                    var breachPos = new Vector2(
                        MathHelper.Lerp(_strikeStartPos.X, _lurePosition.X, t),
                        MathHelper.Lerp(_strikeStartPos.Y, AimY - StrikeBreachHeight, t));
                    _hookedFish.SetPosition(breachPos - FishSilhouette.SpriteHalfSize);
                }
                else if (_strikeTimer < StrikeDiveStartTime)
                {
                    // Phase 2: Brief pause at peak of breach.
                    var breachPos = new Vector2(_lurePosition.X, AimY - StrikeBreachHeight);
                    _hookedFish.SetPosition(breachPos - FishSilhouette.SpriteHalfSize);
                }
                else if (_strikeTimer < StrikeDuration)
                {
                    // Phase 3: Dive down to hook target (halfway depth).
                    var t = (_strikeTimer - StrikeDiveStartTime) / (StrikeDuration - StrikeDiveStartTime);
                    // Ease-out for natural deceleration.
                    t = 1f - (1f - t) * (1f - t);
                    var diveStart = new Vector2(_lurePosition.X, AimY - StrikeBreachHeight);
                    var pos = Vector2.Lerp(diveStart, _hookTarget, t);
                    _hookedFish.SetPosition(pos - FishSilhouette.SpriteHalfSize);
                }
                else
                {
                    // Strike animation complete — transition to hooked.
                    _hookedFish.SetPosition(_hookTarget - FishSilhouette.SpriteHalfSize);
                    _castState = CastState.FishHooked;
                    _wiggleTimer = 0f;
                    _lineTension = 0f;
                    _fightBurstTimer = 0f;
                    _fishStamina = 1f;
                    _fightCooldown = FightCooldownMin
                        + (float)_catchRng.NextDouble() * (FightCooldownMax - FightCooldownMin);
                }
                break;
            }

            case CastState.FishHooked:
            {
                var fishCenter = _hookedFish.Center;
                var fishInWater = fishCenter.X > AimMinX;

                // --- Fight burst management (only while in the water) ---
                if (!fishInWater)
                {
                    _fightBurstTimer = 0f;
                }
                else if (_fightBurstTimer > 0f)
                {
                    _fightBurstTimer -= dt;
                    if (_fightBurstTimer <= 0f)
                    {
                        _fightBurstTimer = 0f;
                        _fightCooldown = FightCooldownMin
                            + (float)_catchRng.NextDouble() * (FightCooldownMax - FightCooldownMin);
                    }
                }
                else
                {
                    _fightCooldown -= dt;
                    if (_fightCooldown <= 0f)
                    {
                        // Scale burst duration by remaining stamina.
                        var staminaFactor = MathHelper.Max(_fishStamina, StaminaFloor);
                        _fightBurstTimer = FightBurstDuration * staminaFactor;
                        _fishStamina = MathHelper.Max(_fishStamina - StaminaDrainPerBurst, 0f);
                        // Spawn a red spook ring at the fish to signal the fight.
                        _rippleManager.SpawnSpookRing(_hookedFish.Center);
                    }
                }

                fishCenter = _hookedFish.Center;
                var isReeling = input.IsHeld(InputAction.Confirm);

                // --- Line tension ---
                if (isReeling)
                {
                    _lineTension += (IsFighting ? TensionReelDuringFight : TensionReelNormal) * dt;
                }
                else
                {
                    _lineTension -= TensionDecay * dt;
                }
                _lineTension = MathHelper.Clamp(_lineTension, 0f, TensionSnapThreshold);

                // Update line color based on tension.
                _currentLineColor = Color.Lerp(LineColor, LineDangerColor, _lineTension);

                // --- Line snap check ---
                if (_lineTension >= TensionSnapThreshold)
                {
                    // Line snapped — fish escapes.
                    _hookedFish.SetRotation(0f);
                    _hookedFish.Flee();
                    _shakeTimer = ShakeDuration;
                    _hookedFish = null;
                    _castState = CastState.Idle;
                    _lineTension = 0f;
                    _currentLineColor = LineColor;
                    break;
                }

                // --- Fight burst: fish pulls hard to the right ---
                if (IsFighting)
                {
                    var staminaFactor = MathHelper.Max(_fishStamina, StaminaFloor);
                    _hookedFish.SetFacingLeft(false);
                    // Frantic wiggle during fight — intensity scales with stamina.
                    var fightWiggle = MathF.Sin(_wiggleTimer * WiggleSpeed * 2.5f) * WiggleAmplitude * 1.8f * staminaFactor;
                    _hookedFish.SetRotation(fightWiggle);

                    var pullDir = new Vector2(1f, MathF.Sin(_wiggleTimer * 4f) * 0.4f);
                    pullDir.Normalize();
                    var newCenter = fishCenter + pullDir * FightPullSpeed * staminaFactor * dt;

                    // Clamp to water bounds.
                    newCenter.Y = MathHelper.Clamp(newCenter.Y, AimY, _hookTarget.Y);
                    newCenter.X = MathHelper.Min(newCenter.X, _virtualWidth - 16f);

                    _hookedFish.SetPosition(newCenter - FishSilhouette.SpriteHalfSize);

                    if (isReeling)
                    {
                        PlayReelTick();
                        // Reeling during a fight still moves the fish slightly toward shore,
                        // but at greatly reduced speed (and at the cost of tension).
                        var reelSpeed = HookedReelSpeed * _hookedFish.ReelSpeedMultiplier * 0.3f;
                        newCenter = _hookedFish.Center;
                        if (newCenter.X > AimMinX)
                        {
                            newCenter.X -= reelSpeed * dt;
                            _hookedFish.SetPosition(newCenter - FishSilhouette.SpriteHalfSize);
                        }
                    }
                }
                else if (isReeling)
                {
                    PlayReelTick();
                    _hookedFish.SetFacingLeft(true);
                    // Reel the fish toward shore along the water, then lift to rod tip.
                    var reelSpeed = HookedReelSpeed * _hookedFish.ReelSpeedMultiplier;
                    var rodTip = _playerPosition + FishingRodHookedOffset + HookedRodTipLocalOffset;
                    var shoreX = AimMinX;

                    if (fishCenter.X > shoreX)
                    {
                        var newX = fishCenter.X - reelSpeed * dt;
                        if (newX <= shoreX)
                            newX = shoreX;

                        var totalDist = _hookTarget.X - shoreX;
                        var progress = totalDist > 1f
                            ? 1f - MathHelper.Clamp((newX - shoreX) / totalDist, 0f, 1f)
                            : 1f;
                        var newY = MathHelper.Lerp(_hookTarget.Y, AimY, progress);
                        newY = MathHelper.Max(newY, AimY);

                        var rotTarget = MathHelper.PiOver2;
                        var wiggle = MathF.Sin(_wiggleTimer * WiggleSpeed) * WiggleAmplitude;
                        _hookedFish.SetRotation(MathHelper.Lerp(0f, rotTarget, progress) + wiggle);

                        _hookedFish.SetPosition(
                            new Vector2(newX, newY) - FishSilhouette.SpriteHalfSize);
                    }
                    else
                    {
                        var airWiggle = MathF.Sin(_wiggleTimer * WiggleSpeed) * WiggleAmplitude;
                        _hookedFish.SetRotation(MathHelper.PiOver2 + airWiggle);
                        var toRod = rodTip - fishCenter;
                        var dist = toRod.Length();
                        if (dist <= reelSpeed * dt)
                        {
                            ShowCatchToast(_hookedFish);
                            _shakeTimer = ShakeDuration;
                            SpawnSplash(rodTip.X, AimY);
                            _catchSfx[_sfxRng.Next(CatchSfxCount)].Play(CatchSfxVolume, 0f, 0f);
                            _fish.Remove(_hookedFish);
                            _hookedFish = null;
                            _castState = CastState.Idle;
                            _lineTension = 0f;
                            _currentLineColor = LineColor;
                        }
                        else
                        {
                            var newCenter = fishCenter + Vector2.Normalize(toRod) * reelSpeed * dt;
                            _hookedFish.SetPosition(newCenter - FishSilhouette.SpriteHalfSize);
                        }
                    }
                }
                else
                {
                    // Not reeling, not fighting — fish drifts back slowly.
                    var rodTipIdle = _playerPosition + FishingRodHookedOffset + HookedRodTipLocalOffset;

                    if (fishCenter.X <= rodTipIdle.X)
                    {
                        // In the air — just hang.
                    }
                    else if (fishCenter.X <= AimMinX)
                    {
                        // Between shoreline and rod — no drift.
                    }
                    else
                    {
                        _hookedFish.SetFacingLeft(false);
                        _hookedFish.SetRotation(0f);
                        var driftDir = new Vector2(1f, 0.3f);
                        driftDir.Normalize();
                        var newCenter = fishCenter + driftDir * HookedDriftBackSpeed * dt;

                        newCenter.Y = MathHelper.Min(newCenter.Y, _hookTarget.Y);
                        newCenter.X = MathHelper.Min(newCenter.X, _virtualWidth - 16f);

                        _hookedFish.SetPosition(newCenter - FishSilhouette.SpriteHalfSize);
                    }
                }
                break;
            }
        }

        // Feed lure events to each fish's attraction state machine while the lure is in the water.
        if (FishAttractionEnabled &&
            _castState is CastState.CastComplete or CastState.ReelingSlack or CastState.ReelingLure
            && _lurePosition.Y >= AimY)
        {
            for (int i = 0; i < _fish.Count; i++)
            {
                _fish[i].UpdateAttraction(_lurePosition, dt, currentLureEvent);
            }
        }
    }

    /// <summary>
    /// Returns the current gauge needle position as a value from 0 to 1.
    /// The needle oscillates back and forth (ping-pong).
    /// </summary>
    private float GaugeValue()
    {
        // Use a triangle wave for smooth ping-pong.
        var t = _gaugePhase % 1f;
        return t <= 0.5f ? t * 2f : 2f - t * 2f;
    }

    /// <summary>
    /// Returns the green zone start/end based on how far the aim cursor is from shore.
    /// Close casts are easy (wide green), far casts are hard (narrow green).
    /// </summary>
    private (float start, float end) GaugeGreenZone()
    {
        var range = _aimMaxX - AimMinX;
        var distanceFraction = range > 1f
            ? MathHelper.Clamp((_aimX - AimMinX) / range, 0f, 1f)
            : 0f;
        var half = MathHelper.Lerp(GaugeGreenHalfClose, GaugeGreenHalfFar, distanceFraction);
        return (GaugeGreenCenter - half, GaugeGreenCenter + half);
    }

    private void DrawPowerGauge(SpriteBatch spriteBatch)
    {
        // Position the gauge to the right of the player character.
        var gaugeX = (int)(_playerPosition.X + CharacterFramePixels + GaugeMarginRight);
        var gaugeY = (int)(_playerPosition.Y + (CharacterFramePixels - GaugeHeight) / 2f);

        // Background bar.
        spriteBatch.Draw(_pixelTexture,
            new Rectangle(gaugeX - 1, gaugeY - 1, GaugeWidth + 2, GaugeHeight + 2),
            GaugeBackColor);

        // Red zone (full bar).
        spriteBatch.Draw(_pixelTexture,
            new Rectangle(gaugeX, gaugeY, GaugeWidth, GaugeHeight),
            GaugeRedColor);

        // Green zone overlay — shrinks with cast distance.
        var (greenStart, greenEnd) = GaugeGreenZone();
        var greenY = gaugeY + (int)(GaugeHeight * (1f - greenEnd));
        var greenH = (int)(GaugeHeight * (greenEnd - greenStart));
        spriteBatch.Draw(_pixelTexture,
            new Rectangle(gaugeX, greenY, GaugeWidth, greenH),
            GaugeGreenColor);

        // Needle (horizontal line at gauge value position).
        var needleValue = GaugeValue();
        var needleY = gaugeY + (int)(GaugeHeight * (1f - needleValue));
        spriteBatch.Draw(_pixelTexture,
            new Rectangle(gaugeX - 1, needleY, GaugeWidth + 2, 2),
            GaugeNeedleColor);
    }

    private void UpdateAim(GameTime gameTime, IInputManager input)
    {
        // Lock the aim once the power gauge is active or the lure is in flight/reeling.
        if (_castState is CastState.Charging or CastState.LureFlying or CastState.CastComplete
            or CastState.ReelingSlack or CastState.ReelingLure
            or CastState.FishStrike or CastState.FishHooked)
        {
            return;
        }

        var dt = (float)gameTime.ElapsedGameTime.TotalSeconds;

        if (input.IsHeld(InputAction.MoveRight))
        {
            _aimX += AimSpeedPxPerSecond * dt;
        }

        if (input.IsHeld(InputAction.MoveLeft))
        {
            _aimX -= AimSpeedPxPerSecond * dt;
        }

        _aimX = MathHelper.Clamp(_aimX, AimMinX, _aimMaxX);
    }

    /// <summary>
    /// Computes the lure start/end positions and transitions to LureFlying.
    /// Low gauge = lands short, green zone = on target, high gauge = overshoots.
    /// </summary>
    private void BeginLureFlight(float gaugeValue, float greenStart, float greenEnd)
    {
        // Launch from the rod tip.
        _lureStart = _playerPosition + FishingRodCastOffset + RodTipLocalOffset;

        var targetX = _aimX + AimArrowSize / 2f;

        if (!_lastCastGood)
        {
            float offset;
            if (gaugeValue < greenStart)
            {
                // Below green — cast lands SHORT. Further from green = bigger miss.
                var miss = 1f - (gaugeValue / greenStart); // 0 at green edge, 1 at bottom
                offset = -MathHelper.Lerp(BadCastMinOffset, BadCastMaxOffset, miss);
            }
            else
            {
                // Above green — cast OVERSHOOTS. Further from green = bigger miss.
                var miss = (gaugeValue - greenEnd) / (1f - greenEnd); // 0 at green edge, 1 at top
                offset = MathHelper.Lerp(BadCastMinOffset, BadCastMaxOffset, miss);
            }
            targetX += offset;
            targetX = MathHelper.Clamp(targetX, 0f, _aimMaxX + AimArrowSize);
        }

        _lureEnd = new Vector2(targetX, AimY);
        _lureFlightTime = 0f;
        _lurePosition = _lureStart;
        _castState = CastState.LureFlying;
    }

    /// <summary>
    /// Draws the fishing line from the rod tip to the lure.
    /// During flight the line is taut (straight). After landing it
    /// transitions to a slack catenary over <see cref="LineSettleDurationSeconds"/>.
    /// </summary>
    private void DrawFishingLine(SpriteBatch spriteBatch)
    {
        Vector2 rodTip;
        Vector2 end;
        float sag;

        if (_castState is CastState.FishStrike or CastState.FishHooked)
        {
            // Taut line from hooked rod tip to fish mouth.
            rodTip = _playerPosition + FishingRodHookedOffset + HookedRodTipLocalOffset;
            end = _hookedFish.MouthPosition;
            sag = 0f;
        }
        else
        {
            rodTip = GetRodTipPosition();
            end = _lurePosition + new Vector2(_lureSwayOffset, 0f);

            // Determine the sag factor. Zero during flight, easing in after landing,
            // then decreasing during reel-in.
            sag = 0f;
            if (_castState == CastState.CastComplete || _castState == CastState.ReelingSlack)
            {
                sag = _currentSag;
            }
        }

        // Draw the line as a series of 1px segments along a quadratic bezier.
        // Control point sits at the midpoint, displaced downward by the sag.
        var mid = (rodTip + end) * 0.5f;
        mid.Y += sag;

        var prev = rodTip;
        for (var i = 1; i <= LineSegments; i++)
        {
            var st = i / (float)LineSegments;
            // Quadratic bezier: B(t) = (1-t)^2 * P0 + 2(1-t)t * P1 + t^2 * P2
            var inv = 1f - st;
            var point = inv * inv * rodTip + 2f * inv * st * mid + st * st * end;

            // Plot 1px line between consecutive curve points (Bresenham).
            DrawLinePixels(spriteBatch, prev, point);

            prev = point;
        }
    }

    /// <summary>
    /// Draws a 1px-thick line between two points using Bresenham's algorithm.
    /// </summary>
    private void DrawLinePixels(SpriteBatch spriteBatch, Vector2 a, Vector2 b)
    {
        var x0 = (int)a.X;
        var y0 = (int)a.Y;
        var x1 = (int)b.X;
        var y1 = (int)b.Y;

        var dx = Math.Abs(x1 - x0);
        var dy = Math.Abs(y1 - y0);
        var sx = x0 < x1 ? 1 : -1;
        var sy = y0 < y1 ? 1 : -1;
        var err = dx - dy;

        while (true)
        {
            spriteBatch.Draw(_pixelTexture, new Rectangle(x0, y0, 1, 1), _currentLineColor);

            if (x0 == x1 && y0 == y1)
            {
                break;
            }

            var e2 = 2 * err;
            if (e2 > -dy)
            {
                err -= dy;
                x0 += sx;
            }

            if (e2 < dx)
            {
                err += dx;
                y0 += sy;
            }
        }
    }

    private void DrawLure(SpriteBatch spriteBatch)
    {
        var pos = new Vector2(
            _lurePosition.X + _lureSwayOffset,
            _lurePosition.Y);

        if (_castState == CastState.LureFlying)
        {
            // In flight: active sprite flipped horizontally (facing left toward water).
            // Eye is at (0,0) which flips to top-right, so origin at (width, 0).
            var origin = new Vector2(_frogLureActive.Width, 0f);
            spriteBatch.Draw(_frogLureActive, pos, null, Color.White,
                0f, origin, 1f, SpriteEffects.FlipHorizontally, 0f);
        }
        else if (_castState is CastState.ReelingSlack or CastState.ReelingLure || _twitchTimer > 0f)
        {
            // Reeling or twitching: eye at line tip, body trails behind.
            spriteBatch.Draw(_frogLureActive, pos, null, Color.White,
                0f, Vector2.Zero, 1f, SpriteEffects.None, 0f);
        }
        else
        {
            // At rest in water: eye at line tip, legs dangle below surface.
            spriteBatch.Draw(_frogLureRest, pos, null, Color.White,
                0f, Vector2.Zero, 1f, SpriteEffects.None, 0f);
        }
    }

    /// <summary>
    /// Draws a short hanging line and lure from the idle rod tip.
    /// </summary>
    private void DrawIdleLineAndLure(SpriteBatch spriteBatch)
    {
        var tip = _playerPosition + FishingRodOffset + IdleRodTipLocalOffset;
        var swayOffset = (float)Math.Sin(_swayTimer * IdleSwayCyclesPerSecond * MathHelper.TwoPi) * IdleSwayAmplitudePx;
        var bottom = tip + new Vector2(swayOffset, IdleLineLengthPx);

        DrawLinePixels(spriteBatch, tip, bottom);

        var origin = new Vector2(_frogLureDangle.Width / 2f, 0f);
        spriteBatch.Draw(_frogLureDangle, bottom, null, Color.White,
            0f, origin, 1f, SpriteEffects.None, 0f);
    }

    /// <summary>
    /// Returns the current rod tip position in world space, accounting for
    /// twitch rotation so the fishing line stays attached to the tip.
    /// </summary>
    private Vector2 GetRodTipPosition()
    {
        var handleWorld = _playerPosition + FishingRodCastOffset;
        var handleOrigin = new Vector2(5f, 30f);
        var tipLocal = RodTipLocalOffset; // (42, 10) relative to sprite origin.

        if (_twitchTimer <= 0f)
        {
            return handleWorld + tipLocal;
        }

        // Rotate the tip around the handle origin by the current twitch angle.
        var twitchT = _twitchTimer / TwitchDurationSeconds;
        var rotation = TwitchRotation * twitchT;
        var offset = tipLocal - handleOrigin;
        var cos = MathF.Cos(rotation);
        var sin = MathF.Sin(rotation);
        var rotated = new Vector2(
            offset.X * cos - offset.Y * sin,
            offset.X * sin + offset.Y * cos);
        return handleWorld + handleOrigin + rotated;
    }

    private void DrawAimArrow(SpriteBatch spriteBatch)
    {
        var arrowX = (int)_aimX;
        var arrowY = (int)AimY;

        // Draw a small downward-pointing triangle on the water surface.
        for (var row = 0; row < AimArrowSize; row++)
        {
            var halfWidth = AimArrowSize - 1 - row;
            var cx = arrowX + AimArrowSize / 2;
            for (var dx = -halfWidth; dx <= halfWidth; dx++)
            {
                spriteBatch.Draw(_pixelTexture,
                    new Rectangle(cx + dx, arrowY + row, 1, 1),
                    AimArrowColor);
            }
        }
    }
}
