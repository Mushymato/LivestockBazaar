using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.ItemTypeDefinitions;

namespace LivestockBazaar;

public abstract record BaseCurrency(ParsedItemData TradeItem)
{
    internal abstract bool HasEnough(int price);

    internal abstract void Deduct(int price);
}

public sealed record MoneyCurrency(ParsedItemData TradeItem) : BaseCurrency(TradeItem)
{
    internal override bool HasEnough(int price) => Game1.player.Money >= price;

    internal override void Deduct(int price) => Game1.player.Money -= price;
}

public sealed record QiGemCurrency(ParsedItemData TradeItem) : BaseCurrency(TradeItem)
{
    internal override bool HasEnough(int price) => Game1.player.QiGems >= price;

    internal override void Deduct(int price) => Game1.player.QiGems -= price;
}

public sealed record GoldenWalnutCurrency(ParsedItemData TradeItem) : BaseCurrency(TradeItem)
{
    internal override bool HasEnough(int price) => Game1.netWorldState.Value.GoldenWalnuts >= price;

    internal override void Deduct(int price) => Game1.netWorldState.Value.GoldenWalnuts -= price;
}

public sealed record ItemCurrency(ParsedItemData TradeItem) : BaseCurrency(TradeItem)
{
    internal override bool HasEnough(int price) => Game1.player.Items.ContainsId(TradeItem.QualifiedItemId, price);

    internal override void Deduct(int price) => Game1.player.Items.ReduceId(TradeItem.QualifiedItemId, price);
}

// public sealed record SpacecoreCurrency(ParsedItemData TradeItem) : BaseCurrency(TradeItem)
// {
//     // internal override bool HasEnough(int price) => Game1.player.Items.ContainsId(TradeItem.QualifiedItemId, price);
//     // internal override void Deduct(int price) => Game1.player.Items.ReduceId(TradeItem.QualifiedItemId, price);
// }

// public sealed record UnlockableBundlesCurrency(ParsedItemData TradeItem) : BaseCurrency(TradeItem)
// {
//     // internal override bool HasEnough(int price) => Game1.player.Items.ContainsId(TradeItem.QualifiedItemId, price);
//     // internal override void Deduct(int price) => Game1.player.Items.ReduceId(TradeItem.QualifiedItemId, price);
// }

/// <summary>Creates and holds different currency classes</summary>
internal static class CurrencyFactory
{
    private static readonly ConditionalWeakTable<string, BaseCurrency?> currencyCache = [];

    internal static Integration.ISpaceCoreApi? scApi = null;
    internal static List<string> scCurrency = null!;

    internal static void RegisterApi(IModRegistry registry)
    {
        scApi = registry.GetApi<Integration.ISpaceCoreApi>("spacechase0.SpaceCore");
    }

    internal static void OnAssetInvalidated(object? sender, AssetsInvalidatedEventArgs e)
    {
        if (
            e.NamesWithoutLocale.Any(an =>
                an.IsEquivalentTo("Data/Objects") || an.IsEquivalentTo("spacechase0.SpaceCore/VirtualCurrencyData")
            )
        )
        {
            scCurrency = null!;
            currencyCache.Clear();
        }
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

        // spacecore
        // if (scApi != null)
        // {
        //     scCurrency ??= scApi.GetVirtualCurrencyList();
        //     if (scCurrency.Contains(itemData.ItemId))
        //     {

        //     }
        // }
        // if (ubApi != null)
        // {

        // }

        return new ItemCurrency(itemData);
    }

    internal static BaseCurrency? Get(string tradeItemId)
    {
        return currencyCache.GetValue(tradeItemId, GetOrCreate);
    }
}
