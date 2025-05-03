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

/// <summary>A model that holds reference to shop data, and some other bazaar settings</summary>
public sealed record BazaarData
{
    /// <summary>
    /// Special owner name, for any case where the NPC is not in the shop but it is open anyways
    /// </summary>
    internal static readonly string Owner_AwayButOpen = $"{ModEntry.ModId}/AwayButOpen";

    /// <summary>
    /// List of shop owners, similar to field of same name in Data/Shops.
    /// If not given then the owner list from shop data will be used instead.
    /// </summary>
    public List<ShopOwnerData>? Owners { get; set; } = null;

    /// <summary>
    /// String id to an entry in Data/Shops.
    /// The shop owner data will be used if <see cref="Owners"/> is not set.
    /// OpenSound and VisualTheme will also be used.
    /// </summary>
    public string? ShopId { get; set; } = null;
    private ShopData? _shopData;
    internal ShopData? ShopData
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

    /// <summary>
    /// Pet adoption shop id, only used as alt shop option.
    /// The shop owner data will be used if <see cref="Owners"/> and <see cref="ShopId"/> is not set.
    /// </summary>
    public string? PetShopId { get; set; } = null;
    private ShopData? _petShopData;
    internal ShopData? PetShopData
    {
        get
        {
            if (_petShopData != null)
                return _petShopData;
            if (PetShopId == null)
                return null;
            if (DataLoader.Shops(Game1.content).TryGetValue(PetShopId, out _petShopData))
                return _petShopData;
            ModEntry.LogOnce($"No shop data found for '{PetShopId}'", LogLevel.Warn);
            PetShopId = null;
            return null;
        }
    }

    /// <summary>Which type of shop open check to follow.</summary>
    public OpenFlagType OpenFlag { get; set; } = OpenFlagType.Stat;

    /// <summary>Which type of shop open check to follow.</summary>
    public string? OpenKey { get; set; } = "Book_AnimalCatalogue";

    /// <summary>If true and there is item shop data, show a dialog to pick either the item shop or the animal shop.</summary>
    private bool showShopDialog = true;
    public bool ShowShopDialog
    {
        get => showShopDialog && (ShopData?.Items.Any() ?? false);
        set => showShopDialog = value;
    }

    /// <summary>If true and there is item shop data, show a dialog to pick either the item shop or the animal shop.</summary>
    private bool showPetShopDialog = true;
    public bool ShowPetShopDialog
    {
        get => showPetShopDialog && Wheels.CanAdoptPets && (PetShopData?.Items.Any() ?? false);
        set => showPetShopDialog = value;
    }
    public string ShopDialogSupplies { get; set; } = "[LocalizedText Strings\\Locations:AnimalShop_Marnie_Supplies]";
    public string ShopDialogAnimals { get; set; } = "[LocalizedText Strings\\Locations:AnimalShop_Marnie_Animals]";
    public string ShopDialogAdopt { get; set; } = "[LocalizedText Strings\\1_6_Strings:AdoptPets]";
    public string ShopDialogLeave { get; set; } = "[LocalizedText Strings\\Locations:AnimalShop_Marnie_Leave]";

    public void InvalidateShopData()
    {
        _shopData = null;
        _petShopData = null;
    }

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

    /// <summary>Get owner data, using ShopData</summary>
    /// <returns></returns>
    public IEnumerable<ShopOwnerData> GetCurrentOwners() =>
        Owners ?? ShopBuilder.GetCurrentOwners(ShopData) ?? ShopBuilder.GetCurrentOwners(PetShopData);

    public static ShopOwnerData? GetAwayOwner(IEnumerable<ShopOwnerData> shopOwnerDatas)
    {
        foreach (ShopOwnerData ownerData in shopOwnerDatas)
            if (ownerData.Name == Owner_AwayButOpen)
                return ownerData;
        return null;
    }
}
