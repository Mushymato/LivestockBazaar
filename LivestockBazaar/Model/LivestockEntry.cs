using System.Diagnostics.CodeAnalysis;
using LivestockBazaar.Integration;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.GameData.FarmAnimals;
using StardewValley.ItemTypeDefinitions;

namespace LivestockBazaar.Model;

public record LivestockEntry(string Key, FarmAnimalData Data)
{
    public static readonly ParsedItemData goldCoin = ItemRegistry.GetData("GoldCoin");
    public readonly SDUISprite? ShopIcon = new(Game1.content.Load<Texture2D>(Data.ShopTexture), Data.ShopSourceRect);

    public const string BUY_FROM = "BuyFrom";
    public const string TRADE_ITEM_ID = "TradeItemId";
    public const string TRADE_ITEM_AMOUNT = "TradeItemAmount";
    public const string TRADE_ITEM_MULT = "TradeItemMult";

    /// <summary>
    /// Check if the animal can be bought from a particular shop.
    /// Marnie can always sell an animal, unless explictly banned.
    /// </summary>
    /// <param name="data"></param>
    /// <param name="shopName"></param>
    /// <returns></returns>
    public bool CanByFrom(string shopName)
    {
        if (
            Data.CustomFields is not Dictionary<string, string> customFields
            || !customFields.TryGetValue(
                string.Concat(ModEntry.ModId, "/", BUY_FROM, ".", shopName),
                out string? buyFrom
            )
        )
            return shopName == Wheels.MARNIE;
        if (!bool.Parse(buyFrom))
            return false;
        return (
            !customFields.TryGetValue(
                string.Concat(ModEntry.ModId, "/", BUY_FROM, ".", shopName, ".Condition"),
                out string? buyFromCond
            ) || GameStateQuery.CheckConditions(buyFromCond)
        );
    }

    /// <summary>
    /// Get the trade item, if it's not gold coin
    /// </summary>
    /// <param name="data"></param>
    /// <returns></returns>
    public ParsedItemData GetTradeItem(string shopName = Wheels.MARNIE)
    {
        if (Data.CustomFields is Dictionary<string, string> customFields)
        {
            if (
                (
                    customFields.TryGetValue(
                        string.Concat(ModEntry.ModId, "/", TRADE_ITEM_ID, ".", shopName),
                        out string? tradeItemId
                    ) || customFields.TryGetValue(string.Concat(ModEntry.ModId, "/TradeItemId"), out tradeItemId)
                ) && ItemRegistry.GetData(tradeItemId) is ParsedItemData itemData
            )
                return itemData;
        }
        return goldCoin;
    }

    /// <summary>
    /// Get the trade item amount, and apply a multiplier.
    /// If TradeItemAmount is not specified, use default multiplier of 2x.
    /// </summary>
    /// <param name="data"></param>
    /// <returns></returns>
    public int GetTradePrice(string shopName = Wheels.MARNIE)
    {
        int price = Data.PurchasePrice;
        float mult = 2f;
        if (Data.CustomFields is Dictionary<string, string> customFields)
        {
            if (
                customFields.TryGetValue(
                    string.Concat(ModEntry.ModId, "/", TRADE_ITEM_AMOUNT, ".", shopName),
                    out string? tradeItemPrice
                ) || customFields.TryGetValue(string.Concat(ModEntry.ModId, "/", TRADE_ITEM_AMOUNT), out tradeItemPrice)
            )
            {
                price = int.Parse(tradeItemPrice);
                mult = 1f;
            }
            if (
                customFields.TryGetValue(
                    string.Concat(ModEntry.ModId, "/", TRADE_ITEM_MULT, ".", shopName),
                    out string? tradeItemMultiplier
                )
                || customFields.TryGetValue(
                    string.Concat(ModEntry.ModId, "/", TRADE_ITEM_MULT),
                    out tradeItemMultiplier
                )
            )
            {
                mult = float.Parse(tradeItemMultiplier);
            }
        }
        return (int)(price * mult);
    }
}
