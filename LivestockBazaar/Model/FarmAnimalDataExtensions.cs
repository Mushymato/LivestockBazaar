using StardewValley;
using StardewValley.GameData.FarmAnimals;
using StardewValley.ItemTypeDefinitions;

namespace LivestockBazaar.Model;

public static class FarmAnimalDataExtensions
{
    static readonly ParsedItemData goldCoin = ItemRegistry.GetData("GoldCoin");

    /// <summary>
    /// Check if the animal can be bought from a particular shop.
    /// Marnie can always sell an animal, unless explictly banned like this:
    /// <code>
    /// "CustomFields": {
    ///     "mushymato.LivestockBazaar/BuyFrom.Marnie": false
    /// }
    /// </code>
    /// Custom shops must be explicitly allowed to sell an animal:
    /// <code>
    /// "CustomFields": {
    ///     "mushymato.LivestockBazaar/BuyFrom.<CUSTOM SHOP NAME>": true
    /// }
    /// </code>
    /// </summary>
    /// <param name="data"></param>
    /// <param name="shopName"></param>
    /// <returns></returns>
    public static bool CanByFrom(this FarmAnimalData data, string shopName)
    {
        if (
            data.CustomFields?.TryGetValue(string.Concat(ModEntry.ModId, "/BuyFrom.", shopName), out string? buyFrom)
            ?? false
        )
            return bool.Parse(buyFrom);
        return shopName == Wheels.MARNIE;
    }

    /// <summary>
    /// Get the trade item
    /// </summary>
    /// <param name="data"></param>
    /// <returns></returns>
    public static ParsedItemData GetTradeItem(this FarmAnimalData data)
    {
        if (
            data.CustomFields?.TryGetValue(string.Concat(ModEntry.ModId, "/TradeItemId"), out string? tradeItem)
            ?? false
        )
            return ItemRegistry.GetData(tradeItem);
        return goldCoin;
    }
}
