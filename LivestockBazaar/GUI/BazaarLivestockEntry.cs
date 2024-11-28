using LivestockBazaar.Integration;
using LivestockBazaar.Model;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PropertyChanged.SourceGenerator;
using StardewValley;
using StardewValley.GameData.Buildings;
using StardewValley.GameData.FarmAnimals;
using StardewValley.ItemTypeDefinitions;
using StardewValley.Menus;
using StardewValley.TokenizableStrings;

namespace LivestockBazaar.GUI;

public partial class BazaarLivestockEntry(string shopName, FarmAnimalData Data, ShopMenu.ShopCachedTheme Theme)
{
    public readonly SDUISprite? ShopIcon = new(Game1.content.Load<Texture2D>(Data.ShopTexture), Data.ShopSourceRect);

    public ParsedItemData TradeItem = Data.GetTradeItem(shopName);
    public int TradePrice = Data.GetTradePrice(shopName);
    public string TradeDisplayFont => TradePrice > 999999 ? "small" : "dialogue";

    public string? ShopTooltip
    {
        get
        {
            string displayName = TokenParser.ParseText(Data.ShopDisplayName ?? Data.DisplayName) ?? "???";
            if (Data.ShopDescription != null)
            {
                // return new SDUITooltipData(
                //     TokenParser.ParseText(Data.ShopDescription),
                //     Title: displayName,
                //     RequiredItemId: TradeItem.ItemId,
                //     RequiredItemAmount: TradePrice
                // );
                return $"{displayName}\n{TokenParser.ParseText(Data.ShopDescription)}";
            }
            // return new SDUITooltipData(displayName);
            return displayName;
        }
    }

    [Notify]
    private Color backgroundTint = Color.White;

    public void PointerEnter() => BackgroundTint = Theme.ItemRowBackgroundHoverColor;

    public void PointerLeave() => BackgroundTint = Color.White;

    /// <summary>Check that a animal has a place to live in a particular location</summary>
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
