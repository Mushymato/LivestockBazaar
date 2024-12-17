using LivestockBazaar.Integration;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.GameData.FarmAnimals;

namespace LivestockBazaar.Model;

public sealed record LivestockSkinData(FarmAnimalSkin Skin)
{
    public readonly Texture2D SpriteSheet = Game1.content.Load<Texture2D>(Skin.Texture);
}

public sealed record LivestockData
{
    public const string BUY_FROM = "BuyFrom";
    public const string TRADE_ITEM_ID = "TradeItemId";
    public const string TRADE_ITEM_AMOUNT = "TradeItemAmount";
    public const string TRADE_ITEM_MULT = "TradeItemMult";

    public readonly string Key;
    public readonly FarmAnimalData Data;

    public readonly Texture2D SpriteSheet;
    public readonly SDUISprite SpriteIcon;
    public readonly SDUISprite ShopIcon;

    public readonly IList<LivestockData> AltPurchase = [];
    public readonly IList<LivestockSkinData> SkinData = [];

    public LivestockData(string key, FarmAnimalData data)
    {
        Key = key;
        Data = data;

        SpriteSheet = Game1.content.Load<Texture2D>(Data.Texture);
        SpriteIcon = new(SpriteSheet, new(0, 0, Data.SpriteWidth, Data.SpriteHeight));
        ShopIcon = Game1.content.DoesAssetExist<Texture2D>(Data.ShopTexture)
            ? new(Game1.content.Load<Texture2D>(Data.ShopTexture), Data.ShopSourceRect)
            : SpriteIcon;

        if (data.Skins != null)
            foreach (FarmAnimalSkin skin in data.Skins)
                if (Game1.content.DoesAssetExist<Texture2D>(skin.Texture))
                    SkinData.Add(new(skin));
    }

    /// <summary>
    /// Check if the animal can be bought from a particular shop.
    /// Marnie can always sell an animal, unless explictly banned.
    /// </summary>
    /// <param name="data"></param>
    /// <param name="shopName"></param>
    /// <returns></returns>
    public bool CanBuyFrom(string shopName)
    {
        if (Data.PurchasePrice < 0 || !GameStateQuery.CheckConditions(Data.UnlockCondition))
            return false;
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
    public BaseCurrency GetTradeCurrency(string shopName = Wheels.MARNIE)
    {
        if (Data.CustomFields is Dictionary<string, string> customFields)
        {
            if (
                (
                    customFields.TryGetValue(
                        string.Concat(ModEntry.ModId, "/", TRADE_ITEM_ID, ".", shopName),
                        out string? tradeItemId
                    ) || customFields.TryGetValue(string.Concat(ModEntry.ModId, TRADE_ITEM_ID), out tradeItemId)
                ) && CurrencyFactory.Get(tradeItemId) is BaseCurrency currency
            )
                return currency;
        }
        return CurrencyFactory.Get("(O)GoldCoin")!;
    }

    /// <summary>
    /// Get the trade item amount, and apply a multiplier.
    /// If TradeItemAmount is not specified, use default multiplier of 2x.
    /// </summary>
    /// <param name="data"></param>
    /// <returns></returns>
    public int GetTradePrice(string shopName = Wheels.MARNIE)
    {
        int price = Math.Max(Data.PurchasePrice, 1);
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

    public static bool IsValid(FarmAnimalData data)
    {
        bool valid = !string.IsNullOrEmpty(data.Texture) && Game1.content.DoesAssetExist<Texture2D>(data.Texture);
        if (!valid)
            ModEntry.LogOnce(
                $"Got invalid Texture on farm animal: {data.DisplayName}",
                StardewModdingAPI.LogLevel.Warn
            );
        return valid;
    }

    public void PopulateAltPurchase(Dictionary<string, LivestockData> LsData)
    {
        if (Data.AlternatePurchaseTypes == null)
        {
            AltPurchase.Clear();
            return;
        }
        foreach (AlternatePurchaseAnimals altPurchase in Data.AlternatePurchaseTypes)
            if (Wheels.GSQCheckNoRandom(altPurchase.Condition))
                foreach (string animalId in altPurchase.AnimalIds)
                    if (LsData.TryGetValue(animalId, out LivestockData? altPurchaseData))
                        AltPurchase.Add(altPurchaseData);
    }
}
