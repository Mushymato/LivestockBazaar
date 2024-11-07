
using System.Collections.Immutable;

namespace LivestockBazaar.GUI;

/// <summary></summary>
/// <param name="shopName"></param>
public sealed class BazaarContext(string shopName)
{
    public readonly ImmutableList<FarmAnimalBuyEntry> FarmAnimalBuy = AssetManager.GetAnimalStockData(shopName).ToImmutableList();
}

