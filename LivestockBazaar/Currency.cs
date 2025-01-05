using System.Runtime.CompilerServices;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.ItemTypeDefinitions;

namespace LivestockBazaar;

public abstract record BaseCurrency(ParsedItemData TradeItem)
{
    internal abstract bool HasEnough(int price);

    internal abstract void Deduct(int price);

    internal abstract int GetTotal();
}

public sealed record MoneyCurrency(ParsedItemData TradeItem) : BaseCurrency(TradeItem)
{
    internal override bool HasEnough(int price) => Game1.player.Money >= price;

    internal override void Deduct(int price) => Game1.player.Money -= price;

    internal override int GetTotal() => Game1.player.Money;
}

public sealed record QiGemCurrency(ParsedItemData TradeItem) : BaseCurrency(TradeItem)
{
    internal override bool HasEnough(int price) => Game1.player.QiGems >= price;

    internal override void Deduct(int price) => Game1.player.QiGems -= price;

    internal override int GetTotal() => Game1.player.QiGems;
}

public sealed record GoldenWalnutCurrency(ParsedItemData TradeItem) : BaseCurrency(TradeItem)
{
    internal override bool HasEnough(int price) => Game1.netWorldState.Value.GoldenWalnuts >= price;

    internal override void Deduct(int price) => Game1.netWorldState.Value.GoldenWalnuts -= price;

    internal override int GetTotal() => Game1.netWorldState.Value.GoldenWalnuts;
}

public sealed record ItemCurrency(ParsedItemData TradeItem) : BaseCurrency(TradeItem)
{
    internal override bool HasEnough(int price) => Game1.player.Items.ContainsId(TradeItem.QualifiedItemId, price);

    internal override void Deduct(int price) => Game1.player.Items.ReduceId(TradeItem.QualifiedItemId, price);

    internal override int GetTotal() => Game1.player.Items.CountId(TradeItem.QualifiedItemId);
}

/// <summary>Creates and holds different currency classes</summary>
internal static class CurrencyFactory
{
    private static readonly ConditionalWeakTable<string, BaseCurrency?> currencyCache = [];

    internal static void OnAssetInvalidated(object? sender, AssetsInvalidatedEventArgs e)
    {
        if (e.NamesWithoutLocale.Any(an => an.IsEquivalentTo("Data/Objects")))
            currencyCache.Clear();
    }

    private static BaseCurrency? GetOrCreate(string tradeItemId)
    {
        ParsedItemData itemData = ItemRegistry.GetData(tradeItemId);
        if (itemData == null)
            return null;
        if (itemData.QualifiedItemId == "(O)GoldCoin")
            return new MoneyCurrency(itemData);
        if (itemData.QualifiedItemId == "(O)858")
            return new QiGemCurrency(itemData);
        if (itemData.QualifiedItemId == "(O)73")
            return new GoldenWalnutCurrency(itemData);

        return new ItemCurrency(itemData);
    }

    internal static BaseCurrency? Get(string tradeItemId)
    {
        return currencyCache.GetValue(tradeItemId, GetOrCreate);
    }
}
