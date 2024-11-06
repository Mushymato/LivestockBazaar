
using System.Collections.Immutable;

namespace LivestockBazaar.GUI;

/// <summary>Add tile action for opening and closing animal shops</summary>
public sealed class BazaarContext(string shopName)
{
    public readonly ImmutableList<FarmAnimalBuyEntry> FarmAnimalBuy = AssetManager.GetAnimalStockData(shopName).ToImmutableList();
}

