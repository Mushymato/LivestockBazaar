using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.GameData.Buildings;
using StardewValley.GameData.FarmAnimals;
using StardewValley.GameData.Shops;

namespace LivestockBazaar;

/// <summary>Hold info about animal that can be bought</summary>
/// <param name="Key">Data/FarmAnimals key</param>
/// <param name="Data"></param>
/// <param name="AvailableForLocation"></param>
public sealed record FarmAnimalBuyEntry(string Key, FarmAnimalData Data, bool AvailableForLocation);

/// <summary>How to check whether the shop should ignore</summary>
public enum OpenFlagType
{
    /// <summary>Shop always follows open/close times + npc nearby</summary>
    None,
    /// <summary>Shop is always open after a stat is set (usually by reading a book)</summary>
    Stat,
    /// <summary>Shop is always open after a mail flag is set</summary>
    Mail
}
/// <summary>Extend vanilla ShopData with some extra fields for use in this mod</summary>
public sealed class BazaarData : ShopData
{
    /// <summary>Which type of shop open check to follow.</summary>
    public OpenFlagType BazaarOpenFlag { get; set; } = OpenFlagType.Stat;
    /// <summary>Which type of shop open check to follow.</summary>
    public string? BazaarOpenKey { get; set; } = "Book_AnimalCatalogue";
    /// <summary>
    /// Check if shop should check the open-close and shop owner in rect conditions.
    /// </summary>
    /// <param name="player"></param>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    public bool ShouldCheckShopOpen(Farmer player)
    {
        return BazaarOpenFlag switch
        {
            OpenFlagType.None => true,
            OpenFlagType.Stat => player.stats.Get(BazaarOpenKey) == 0,
            OpenFlagType.Mail => !player.mailReceived.Contains(BazaarOpenKey),
            _ => throw new NotImplementedException()
        };
    }
}

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

    internal static Dictionary<string, BazaarData> DefaultBazaarData()
    {
        Dictionary<string, BazaarData> bazaarData = [];
        if (DataLoader.Shops(Game1.content).TryGetValue(ANIMAL_SHOP, out ShopData? animalShop))
        {
            bazaarData[MARNIE] = new()
            {
                Currency = animalShop.Currency,
                StackSizeVisibility = animalShop.StackSizeVisibility,
                OpenSound = animalShop.OpenSound,
                PurchaseSound = animalShop.PurchaseSound,
                PurchaseRepeatSound = animalShop.PurchaseRepeatSound,
                ApplyProfitMargins = animalShop.ApplyProfitMargins,
                PriceModifiers = animalShop.PriceModifiers,
                PriceModifierMode = animalShop.PriceModifierMode,
                Owners = animalShop.Owners,
                VisualTheme = animalShop.VisualTheme,
                SalableItemTags = animalShop.SalableItemTags,
                Items = animalShop.Items,
                CustomFields = animalShop.CustomFields,
            };
        }
        return bazaarData;
    }


    /// <summary>Get animal data and whether the animal is fit for a given location</summary>
    /// <param name="shopName"></param>
    /// <param name="location"></param>
    /// <returns></returns>
    public static IEnumerable<FarmAnimalBuyEntry> GetAnimalStockData(string shopName, GameLocation? location = null)
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
            bool available = true;
            if (datum.Value.RequiredBuilding != null && location != null)
            {
                available = HasBuildingOrUpgrade(location, datum.Value.RequiredBuilding);
            }
            yield return new(datum.Key, datum.Value, available);
        }
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
