using System;
using Microsoft.Xna.Framework;
using DogDays.Game.Core;
using DogDays.Game.Data.Save;
using DogDays.Game.Input;
using DogDays.Game.Screens;
using DogDays.Game.Systems;
using DogDays.Tests.Helpers;
using Xunit;

namespace DogDays.Tests.Unit;

public sealed class TitleScreenTests
{
    private static TitleScreen CreateTitleScreen(
        ScreenManager? manager = null,
        bool withSaveData = false)
    {
        var eventBus = new GameEventBus();
        var quests = new QuestManager(eventBus);
        var save = new FakeSaveGameService();

        if (withSaveData)
        {
            save.Save(0, new SaveGameData
            {
                Player = new SavePlayerData
                {
                    X = 100f,
                    Y = 200f,
                    ZoneMapAssetName = "Maps/StarterMap",
                },
                SavedAtUtc = DateTime.UtcNow,
            });
        }

        var services = new GameSessionServices(eventBus, quests, save);
        return new TitleScreen(
            null!,
            null!,
            480,
            270,
            manager ?? new ScreenManager(),
            services,
            () => { });
    }

    [Fact]
    public void IsTransparent__ReturnsFalse()
    {
        var screen = CreateTitleScreen();
        Assert.False(screen.IsTransparent);
    }

    [Fact]
    public void Constructor__NoSaveData__SelectedIndexIsZero()
    {
        var screen = CreateTitleScreen(withSaveData: false);
        Assert.Equal(0, screen.SelectedIndex);
        Assert.False(screen.HasSaveData);
    }

    [Fact]
    public void Constructor__WithSaveData__SelectedIndexIsOne()
    {
        var screen = CreateTitleScreen(withSaveData: true);
        Assert.Equal(1, screen.SelectedIndex);
        Assert.True(screen.HasSaveData);
    }

    [Fact]
    public void Update__MoveDownWithSave__WrapsAroundToZero()
    {
        var screen = CreateTitleScreen(withSaveData: true);
        // Start at 1 (Continue), move down wraps to 0.
        var input = new FakeInputManager();
        input.Press(InputAction.MoveDown);

        screen.Update(FakeGameTime.OneFrame(), input);

        Assert.Equal(0, screen.SelectedIndex);
    }

    [Fact]
    public void Update__MoveUpFromNewGame__WrapsToLastItem()
    {
        var screen = CreateTitleScreen(withSaveData: true);
        // Move to "New Game" first.
        var input = new FakeInputManager();
        input.Press(InputAction.MoveDown);
        screen.Update(FakeGameTime.OneFrame(), input);
        Assert.Equal(0, screen.SelectedIndex);

        // Move up wraps to Continue (index 1).
        input.Update();
        input.Press(InputAction.MoveUp);
        screen.Update(FakeGameTime.OneFrame(), input);

        Assert.Equal(1, screen.SelectedIndex);
    }

    [Fact]
    public void Update__MoveDownNoSave__StaysAtZero()
    {
        var screen = CreateTitleScreen(withSaveData: false);
        var input = new FakeInputManager();
        input.Press(InputAction.MoveDown);

        screen.Update(FakeGameTime.OneFrame(), input);

        // Continue is disabled, so selection cannot move there.
        Assert.Equal(0, screen.SelectedIndex);
    }

    [Fact]
    public void Update__MoveUpNoSave__StaysAtZero()
    {
        var screen = CreateTitleScreen(withSaveData: false);
        var input = new FakeInputManager();
        input.Press(InputAction.MoveUp);

        screen.Update(FakeGameTime.OneFrame(), input);

        Assert.Equal(0, screen.SelectedIndex);
    }

    [Fact]
    public void Update__NoInputPressed__SelectionUnchanged()
    {
        var screen = CreateTitleScreen(withSaveData: true);
        Assert.Equal(1, screen.SelectedIndex);

        var input = new FakeInputManager();
        screen.Update(FakeGameTime.OneFrame(), input);

        Assert.Equal(1, screen.SelectedIndex);
    }
}
