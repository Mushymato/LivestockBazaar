using System.Collections.Immutable;
using System.Runtime.CompilerServices;
using LivestockBazaar.Model;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.GameData.FarmAnimals;

namespace LivestockBazaar;

/// <summary>Handles caching of custom target.</summary>
internal static class AssetManager
{
    /// <summary>Vanilla AnimalShop in Data/Shops, for copying into Bazaar data.</summary>
    internal const string ANIMAL_SHOP = "AnimalShop";

    /// <summary>Shop asset target</summary>
    private static string BazaarAsset => $"{ModEntry.ModId}/Shops";

    /// <summary>Backing field for bazaar data</summary>
    private static Dictionary<string, BazaarData>? _bazaarData = null;

    /// <summary>Bazaar data lazy loader</summary>
    internal static Dictionary<string, BazaarData> BazaarData
    {
        get
        {
            _bazaarData ??= Game1.content.Load<Dictionary<string, BazaarData>>(BazaarAsset);
            return _bazaarData;
        }
    }

    /// <summary>Backing field for livestock data</summary>
    private static Dictionary<string, LivestockData>? _lsData = null;

    /// <summary>Livestock data lazy loader</summary>
    internal static Dictionary<string, LivestockData> LsData
    {
        get
        {
            if (_lsData == null)
            {
                _lsData = [];
                foreach ((string key, FarmAnimalData data) in Game1.farmAnimalData)
                {
                    if (LivestockData.IsValid(data))
                        _lsData[key] = new(key, data);
                }
                foreach (LivestockData data in _lsData.Values)
                {
                    data.PopulateAltPurchase(_lsData);
                }
            }
            return _lsData;
        }
    }

    internal static void OnAssetRequested(object? sender, AssetRequestedEventArgs e)
    {
        if (e.Name.IsEquivalentTo(BazaarAsset))
            e.LoadFrom(DefaultBazaarData, AssetLoadPriority.Exclusive);
    }

    internal static void OnAssetInvalidated(object? sender, AssetsInvalidatedEventArgs e)
    {
        if (e.NamesWithoutLocale.Any(an => an.IsEquivalentTo(BazaarAsset)))
            _bazaarData = null;
        if (e.NamesWithoutLocale.Any(an => an.IsEquivalentTo("Data/FarmAnimals")))
            _lsData = null;
        CurrencyFactory.OnAssetInvalidated(sender, e);
    }

    /// <summary>Make a copy of AnimalShop to use as <see cref="MARNIE"/>'s bazaar data.</summary>
    /// <returns></returns>
    internal static Dictionary<string, BazaarData> DefaultBazaarData()
    {
        Dictionary<string, BazaarData> bazaarData = [];
        bazaarData[Wheels.MARNIE] = new() { ShopId = ANIMAL_SHOP };
        return bazaarData;
    }

    public static BazaarData? GetBazaarData(string shopName)
    {
        BazaarData.TryGetValue(shopName, out BazaarData? data);
        return data;
    }

    /// <summary>Get available livestock for shop</summary>
    /// <param name="shopName"></param>
    /// <returns></returns>
    public static IEnumerable<LivestockData> GetLivestockDataForShop(string shopName) =>
        LsData.Values.Where((ls) => ls.CanBuyFrom(shopName));
}
