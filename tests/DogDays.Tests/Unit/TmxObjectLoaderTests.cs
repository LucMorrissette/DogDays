using System.Collections.Generic;
using System.Globalization;
using System.Xml.Linq;
using DogDays.Game.World;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace DogDays.Tests.Unit;

/// <summary>
/// Unit tests for decorative TMX tile-object parsing.
/// </summary>
public class TmxObjectLoaderTests
{
    [Fact]
    public void LoadPropPlacements__ResizedPropTileObject__CapturesPlacementSize()
    {
        var mapElement = XElement.Parse(
            """
            <map>
              <objectgroup name="Props">
                <object id="1" gid="39" x="224" y="64" width="27" height="43.2" />
              </objectgroup>
            </map>
            """);
        var propMetadata = new Dictionary<int, PropTileMetadata>
        {
            [39] = new("front-door-closed", false, false, true),
        };

        var placements = TmxObjectLoader.LoadPropPlacements(mapElement, propMetadata);

        var placement = Assert.Single(placements);
        Assert.Equal("front-door-closed", placement.PropType);
        Assert.Equal(new Vector2(224f, 21f), placement.Position);
        Assert.Equal(new Point(27, 43), placement.SizePixels);
        Assert.True(placement.SuppressOcclusion);
    }

    [Fact]
    public void LoadDecorativeTileObjects__NonPropTileObject__ReturnsPlacement()
    {
        var mapElement = XElement.Parse(
            """
            <map>
              <objectgroup name="WallBorders">
                <object id="1" gid="53" x="64" y="96" width="32" height="12" />
              </objectgroup>
            </map>
            """);

        var placements = TmxObjectLoader.LoadDecorativeTileObjects(
            mapElement,
            new Dictionary<int, PropTileMetadata>());

        var placement = Assert.Single(placements);
        Assert.Equal(53, placement.GlobalIdentifier);
        Assert.Equal(new Vector2(64f, 84f), placement.Position);
        Assert.Equal(new Point(32, 12), placement.SizePixels);
        Assert.Equal(SpriteEffects.None, placement.SpriteEffects);
    }

    [Fact]
    public void LoadDecorativeTileObjects__PropTileObject__SkipsPlacement()
    {
        var mapElement = XElement.Parse(
            """
            <map>
              <objectgroup name="stuff">
                <object id="1" gid="53" x="64" y="96" width="32" height="12" />
              </objectgroup>
            </map>
            """);

        var propMetadata = new Dictionary<int, PropTileMetadata>
        {
            [53] = new("boulder", false, false, false),
        };

        var placements = TmxObjectLoader.LoadDecorativeTileObjects(mapElement, propMetadata);

        Assert.Empty(placements);
    }

    [Fact]
    public void LoadDecorativeTileObjects__FlippedTileObject__CapturesSpriteEffects()
    {
        var rawGlobalIdentifier = 53u | 0x80000000u | 0x40000000u;
        var mapElement = XElement.Parse(
            $"""
            <map>
              <objectgroup name="WallBorders">
                <object id="1" gid="{rawGlobalIdentifier.ToString(CultureInfo.InvariantCulture)}" x="10" y="20" width="12" height="12" />
              </objectgroup>
            </map>
            """);

        var placements = TmxObjectLoader.LoadDecorativeTileObjects(
            mapElement,
            new Dictionary<int, PropTileMetadata>());

        var placement = Assert.Single(placements);
        Assert.Equal(SpriteEffects.FlipHorizontally | SpriteEffects.FlipVertically, placement.SpriteEffects);
        Assert.Equal(53, placement.GlobalIdentifier);
    }
}