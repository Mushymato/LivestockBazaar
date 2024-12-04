using StardewValley;
using StardewValley.GameData.FarmAnimals;
using StardewValley.ItemTypeDefinitions;

namespace LivestockBazaar.Model;

public static class FarmAnimalDataExtensions
{
    static readonly ParsedItemData goldCoin = ItemRegistry.GetData("GoldCoin");

    /// <summary>
    /// Check if the animal can be bought from a particular shop.
    /// Marnie can always sell an animal, unless explictly banned.
    /// </summary>
    /// <param name="data"></param>
    /// <param name="shopName"></param>
    /// <returns></returns>
    public static bool CanByFrom(this FarmAnimalData data, string shopName)
    {
        if (
            data.CustomFields is not Dictionary<string, string> customFields
            || !customFields.TryGetValue(string.Concat(ModEntry.ModId, "/BuyFrom.", shopName), out string? buyFrom)
        )
            return shopName == Wheels.MARNIE;
        if (!bool.Parse(buyFrom))
            return false;
        return (
            !customFields.TryGetValue(
                string.Concat(ModEntry.ModId, "/BuyFrom.", shopName, ".Condition"),
                out string? buyFromCond
            ) || GameStateQuery.CheckConditions(buyFromCond)
        );
    }

    /// <summary>
    /// Get the trade item, if it's not gold coin
    /// </summary>
    /// <param name="data"></param>
    /// <returns></returns>
    public static ParsedItemData GetTradeItem(this FarmAnimalData data, string shopName = Wheels.MARNIE)
    {
        if (
            (
                (
                    data.CustomFields?.TryGetValue(
                        string.Concat(ModEntry.ModId, "/TradeItemId.", shopName),
                        out string? tradeItemId
                    ) ?? false
                )
                || (
                    data.CustomFields?.TryGetValue(string.Concat(ModEntry.ModId, "/TradeItemId"), out tradeItemId)
                    ?? false
                )
            ) && ItemRegistry.GetData(tradeItemId) is ParsedItemData itemData
        )
        {
            return itemData;
        }
        return goldCoin;
    }

    /// <summary>
    /// Get the trade item, if it's not gold coin
    /// </summary>
    /// <param name="data"></param>
    /// <returns></returns>
    public static int GetTradePrice(this FarmAnimalData data, string shopName = Wheels.MARNIE)
    {
        if (
            data.CustomFields?.TryGetValue(
                string.Concat(ModEntry.ModId, "/TradeItemAmount.", shopName),
                out string? tradeItemPrice
            ) ?? false
        )
            return int.Parse(tradeItemPrice);
        if (
            data.CustomFields?.TryGetValue(string.Concat(ModEntry.ModId, "/TradeItemAmount"), out tradeItemPrice)
            ?? false
        )
            return int.Parse(tradeItemPrice);
        return data.PurchasePrice;
    }
}
