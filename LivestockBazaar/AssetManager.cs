using LivestockBazaar.Model;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.GameData.FarmAnimals;

namespace LivestockBazaar;

/// <summary>Handles caching of custom target.</summary>
internal static class AssetManager
{
    /// <summary>The animal tycoon herself</summary>
    internal const string MARNIE = "Marnie";

    /// <summary>Vanilla AnimalShop in Data/Shops, for copying into Bazaar data.</summary>
    internal const string ANIMAL_SHOP = "AnimalShop";

    /// <summary>Shop asset target</summary>
    private static string BazaarAsset => $"{ModEntry.ModId}/Shops";

    /// <summary>Backing field</summary>
    private static Dictionary<string, BazaarData>? _bazaarData = null;

    /// <summary>Shop data lazy loader</summary>
    internal static Dictionary<string, BazaarData> BazaarData
    {
        get
        {
            _bazaarData ??= Game1.content.Load<Dictionary<string, BazaarData>>(BazaarAsset);
            return _bazaarData;
        }
    }

    /// <summary>Buy from animal data CustomField</summary>
    internal static readonly string Field_BuyFrom = $"{ModEntry.ModId}/BuyFrom.";

    internal static void OnAssetRequested(object? sender, AssetRequestedEventArgs e)
    {
        if (e.Name.IsEquivalentTo(BazaarAsset))
            e.LoadFrom(DefaultBazaarData, AssetLoadPriority.Exclusive);
    }

    internal static void OnAssetInvalidated(object? sender, AssetsInvalidatedEventArgs e)
    {
        if (e.NamesWithoutLocale.Any(an => an.IsEquivalentTo(BazaarAsset)))
            _bazaarData = null;
    }

    /// <summary>Make a copy of AnimalShop to use as <see cref="MARNIE"/>'s bazaar data.</summary>
    /// <returns></returns>
    internal static Dictionary<string, BazaarData> DefaultBazaarData()
    {
        Dictionary<string, BazaarData> bazaarData = [];
        bazaarData[MARNIE] = new() { ShopId = ANIMAL_SHOP };
        return bazaarData;
    }

    public static BazaarData? GetBazaarData(string shopName)
    {
        BazaarData.TryGetValue(shopName, out BazaarData? data);
        return data;
    }

    /// <summary>Get animal data and whether the animal is fit for a given location</summary>
    /// <param name="shopName"></param>
    /// <param name="location"></param>
    /// <returns></returns>
    public static IEnumerable<LivestockBuyEntry> GetAnimalStockData(string shopName)
    {
        string buyFromKey = Field_BuyFrom + shopName;
        foreach (KeyValuePair<string, FarmAnimalData> datum in Game1.farmAnimalData)
        {
            if (datum.Value.PurchasePrice <= 0 || !GameStateQuery.CheckConditions(datum.Value.UnlockCondition))
                continue;
            if (datum.Value.CustomFields?.TryGetValue(buyFromKey, out string? buyFrom) ?? false)
            {
                if (bool.Parse(buyFrom) == false)
                    continue;
            }
            else if (shopName != MARNIE)
                continue;
            yield return new(datum.Key, datum.Value);
        }
    }
}
