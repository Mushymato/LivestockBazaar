using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PropertyChanged.SourceGenerator;
using StardewValley;
using StardewValley.GameData.Buildings;
using StardewValley.GameData.FarmAnimals;
using StardewValley.TokenizableStrings;

namespace LivestockBazaar.GUI;

public partial class BazaarLivestockEntry(FarmAnimalData Data)
{
    // layout="70%[1204..] 80%[648..]"
    public readonly Tuple<Texture2D, Rectangle>? ShopIcon = new(Game1.content.Load<Texture2D>(Data.ShopTexture), Data.ShopSourceRect);
    public string ShopDisplayName => TokenParser.ParseText(Data.ShopDisplayName ?? Data.DisplayName) ?? "???";

    [Notify]
    private int buyCount = 0;

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
    /// <returns></returns>
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
