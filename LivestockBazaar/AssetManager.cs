using LivestockBazaar.Model;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.GameData.FarmAnimals;
using StardewValley.GameData.Shops;

namespace LivestockBazaar;

/// <summary>Handles caching of custom target.</summary>
internal static class AssetManager
{
    /// <summary>Vanilla AnimalShop in Data/Shops, for copying into Bazaar data.</summary>
    internal const string ANIMAL_SHOP = "AnimalShop";
    internal const string PET_ADOPTION = "PetAdoption";

    /// <summary>Shop asset target</summary>
    private const string BazaarAsset = $"{ModEntry.ModId}/Shops";

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
                Dictionary<string, FarmAnimalData> altPurchaseValidate = [];
                foreach ((string key, FarmAnimalData data) in Game1.farmAnimalData)
                {
                    if (LivestockData.IsValid(key, data, out bool needValidAltPurchase))
                        _lsData[key] = new(key, data);
                    else if (needValidAltPurchase)
                        altPurchaseValidate[key] = data;
                }
                foreach ((string key, FarmAnimalData data) in altPurchaseValidate)
                {
                    ValidateAtLeastOneAltPurchase(_lsData, key, data);
                }
            }
            return _lsData;
        }
    }

    /// <summary>Handle an edge case where animal is not valid, but has valid alt purchases</summary>
    private static void ValidateAtLeastOneAltPurchase(
        Dictionary<string, LivestockData> _lsData,
        string key,
        FarmAnimalData data
    )
    {
        foreach (AlternatePurchaseAnimals altPurchase in data.AlternatePurchaseTypes)
        {
            if (GameStateQuery.IsImmutablyFalse(altPurchase.Condition))
                continue;
            foreach (string animalId in altPurchase.AnimalIds)
            {
                if (_lsData.TryGetValue(animalId, out LivestockData? altPurchaseData))
                {
                    ModEntry.Log(
                        $"Set animal '{key}' texture to {altPurchaseData.Data.Texture} (from '{animalId}')",
                        LogLevel.Debug
                    );
                    data.Texture = altPurchaseData.Data.Texture;
                    _lsData[key] = new(key, data);
                    return;
                }
            }
        }
    }

    internal static void PopulateAltPurchase()
    {
        foreach (LivestockData data in LsData.Values)
            data.PopulateAltPurchase(LsData);
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
        if (_bazaarData != null && e.NamesWithoutLocale.Any(an => an.IsEquivalentTo("Data/Shops")))
        {
            foreach (BazaarData data in _bazaarData.Values)
            {
                data.InvalidateShopData();
            }
        }
        if (e.NamesWithoutLocale.Any(an => an.IsEquivalentTo("Data/FarmAnimals")))
            _lsData = null;
        CurrencyFactory.OnAssetInvalidated(sender, e);
    }

    /// <summary>By default add a entry for Marnie's shop.</summary>
    /// <returns></returns>
    internal static Dictionary<string, BazaarData> DefaultBazaarData()
    {
        Dictionary<string, BazaarData> bazaarData = [];
        bazaarData[Wheels.MARNIE] = new()
        {
            Owners =
            [
                new ShopOwnerData()
                {
                    Id = Wheels.MARNIE,
                    Name = Wheels.MARNIE,
                    Dialogues =
                    [
                        new ShopDialogueData()
                        {
                            Id = "Rare",
                            Condition = "RANDOM 0.0001",
                            Dialogue = "[LocalizedText Strings\\StringsFromCSFiles:ShopMenu.cs.11508]",
                        },
                        new ShopDialogueData()
                        {
                            Id = "Default",
                            Dialogue = "[LocalizedText Strings\\1_6_Strings:Marnie_Pet_Adoption]",
                        },
                    ],
                },
            ],
            ShopId = ANIMAL_SHOP,
            PetShopId = PET_ADOPTION,
        };
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

    /// <summary>Check if there's any livestock shop</summary>
    /// <param name="shopName"></param>
    /// <returns></returns>
    public static bool HasAnyLivestockDataForShop(string shopName) =>
        LsData.Values.Any((ls) => ls.CanBuyFrom(shopName));
}
