using LivestockBazaar.Integration;
using LivestockBazaar.Model;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PropertyChanged.SourceGenerator;
using StardewModdingAPI;
using StardewValley;
using StardewValley.GameData.Buildings;
using StardewValley.GameData.FarmAnimals;
using StardewValley.ItemTypeDefinitions;
using StardewValley.TokenizableStrings;

namespace LivestockBazaar.GUI;

public sealed partial class BazaarLivestockEntry(BazaarContextMain Main, string shopName, FarmAnimalData Data)
{
    // events
    public void GridCell_PointerEnter()
    {
        BackgroundTint = Main.Theme.ItemRowBackgroundHoverColor;
        if (Main.HoveredLivestock != this)
        {
            animFrame = 0;
            Main.HoveredLivestock = this;
        }
    }

    public void GridCell_PointerLeave()
    {
        BackgroundTint = Color.White;
        if (Main.HoveredLivestock == this)
            Main.HoveredLivestock = null;
    }

    public void GridCell_LeftClick()
    {
        BackgroundTint = Color.White;
        if (Main.SelectedLivestock != this)
            Main.SelectedLivestock = this;
    }

    // icon
    public readonly SDUISprite? ShopIcon = new(Game1.content.Load<Texture2D>(Data.ShopTexture), Data.ShopSourceRect);

    // trade cost
    public ParsedItemData TradeItem = Data.GetTradeItem(shopName);
    public int TradePrice = Data.GetTradePrice(shopName);
    public string TradeDisplayFont => TradePrice > 999999 ? "small" : "dialogue";

    // hover color
    [Notify]
    private Color backgroundTint = Color.White;

    // infobox
    public readonly string DisplayName = TokenParser.ParseText(Data.ShopDisplayName ?? Data.DisplayName ?? "???");
    public readonly string Description = TokenParser.ParseText(Data.ShopDescription ?? "");
    private Texture2D SpriteSheet => Game1.content.Load<Texture2D>(Data.Texture);
    public readonly string AnimLayout = $"content[{Data.SpriteWidth * 4}..] content[{Data.SpriteHeight * 4}..]";

    [Notify]
    private int animFrame = 0;
    public SDUISprite AnimSprite
    {
        get
        {
            // TODO: flip the sprite too
            int adjAnimFrame = AnimFrame;
            if (Data.UseFlippedRightForLeft && adjAnimFrame >= 12)
            {
                adjAnimFrame -= 8;
            }
            return new(
                SpriteSheet,
                new(
                    adjAnimFrame * Data.SpriteWidth % SpriteSheet.Width,
                    adjAnimFrame * Data.SpriteWidth / SpriteSheet.Width * Data.SpriteHeight,
                    Data.SpriteWidth,
                    Data.SpriteHeight
                ),
                SDUIEdges.NONE,
                new(Scale: 4)
            );
        }
    }

    public void NextFrame()
    {
        AnimFrame = (AnimFrame + 1) % 16;
    }

    /// <summary>Check that
    /// a animal has a place to live in a particular location</summary>
    /// <param name="location">game location or null</param>
    /// <returns>True if animal has place to live</returns>
    public bool AvailableForLocation(GameLocation? location)
    {
        if (location != null && Data.RequiredBuilding != null)
            return HasBuildingOrUpgrade(location, Data.RequiredBuilding);
        return true;
    }

    /// <summary>Why is Utility._HasBuildingOrUpgrade protected...</summary>
    /// <param name="location"></param>
    /// <param name="buildingId"></param>
    /// <returns>True if the location has a building or its upgrade</returns>
    public static bool HasBuildingOrUpgrade(GameLocation location, string buildingId)
    {
        if (location.getNumberBuildingsConstructed(buildingId) > 0)
        {
            return true;
        }
        foreach (KeyValuePair<string, BuildingData> buildingDatum in Game1.buildingData)
        {
            string key = buildingDatum.Key;
            BuildingData value = buildingDatum.Value;
            if (!(key == buildingId) && value.BuildingToUpgrade == buildingId && HasBuildingOrUpgrade(location, key))
            {
                return true;
            }
        }
        return false;
    }
}
