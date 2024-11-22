using StardewModdingAPI;
using StardewValley;
using StardewValley.GameData.Shops;
using StardewValley.Internal;

namespace LivestockBazaar.Model;

/// <summary>How to check whether the shop should ignore</summary>
public enum OpenFlagType
{
    /// <summary>Shop always follows open/close times + npc nearby</summary>
    None,

    /// <summary>Shop is always open after a stat is set (usually by reading a book)</summary>
    Stat,

    /// <summary>Shop is always open after a mail flag is set</summary>
    Mail,
}

/// <summary>Extend vanilla ShopData with some extra fields for use in this mod</summary>
public class BazaarData
{
    public static readonly string Owner_AwayButOpen = $"{ModEntry.ModId}/AwayButOpen";
    public string? ShopId { get; set; } = null;
    private ShopData? _shopData;
    public ShopData? ShopData
    {
        get
        {
            if (_shopData != null)
                return _shopData;
            if (ShopId == null)
                return null;
            if (DataLoader.Shops(Game1.content).TryGetValue(ShopId, out _shopData))
                return _shopData;
            ModEntry.LogOnce($"No shop data found for '{ShopId}'", LogLevel.Warn);
            ShopId = null;
            return null;
        }
    }

    /// <summary>Which type of shop open check to follow.</summary>
    public OpenFlagType OpenFlag { get; set; } = OpenFlagType.Stat;

    /// <summary>Which type of shop open check to follow.</summary>
    public string? OpenKey { get; set; } = "Book_AnimalCatalogue";

    /// <summary>
    /// Check if shop should check the open-close and shop owner in rect conditions.
    /// </summary>
    /// <param name="player"></param>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    public bool ShouldCheckShopOpen(Farmer player)
    {
        return OpenFlag switch
        {
            OpenFlagType.None => true,
            OpenFlagType.Stat => player.stats.Get(OpenKey) == 0,
            OpenFlagType.Mail => !player.mailReceived.Contains(OpenKey),
            _ => throw new NotImplementedException(),
        };
    }

    public IEnumerable<ShopOwnerData> GetCurrentOwners() => ShopBuilder.GetCurrentOwners(ShopData);

    public static ShopOwnerData? GetAwayOwner(IEnumerable<ShopOwnerData> shopOwnerDatas)
    {
        foreach (ShopOwnerData ownerData in shopOwnerDatas)
        {
            if (ownerData.Name == Owner_AwayButOpen)
            {
                return ownerData;
            }
        }
        return null;
    }
}
