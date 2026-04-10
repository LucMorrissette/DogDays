using DogDays.Game.Data;

namespace DogDays.Tests.Unit;

public sealed class MapConfigTests
{
    [Fact]
    public void ForMap__WoodsBehindCabin__DoesNotAutoEnableStarterWeapons()
    {
        var config = MapConfig.ForMap("Maps/WoodsBehindCabin");

        Assert.False(config.AutoEnableStarterWeapons);
    }
}