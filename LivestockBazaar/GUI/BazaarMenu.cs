using LivestockBazaar.Integration;
using StardewModdingAPI;
using StardewValley;
using StardewValley.GameData.Shops;

namespace LivestockBazaar.GUI;

/// <summary>Bazaar menu StardewUI setup.</summary>
internal static class BazaarMenu
{
    private static IViewEngine viewEngine = null!;
    internal static bool StardewUIEnabled => viewEngine is not null;
    internal static string VIEW_ASSET_PREFIX = null!;
    internal static string VIEW_ASSET_MENU = null!;

    internal static void Register(IModHelper helper)
    {
        if (!helper.ModRegistry.IsLoaded("focustense.StardewUI"))
        {
            ModEntry.Log("focustense.StardewUI not available, will use vanilla menu.", LogLevel.Info);
            return;
        }
        viewEngine = helper.ModRegistry.GetApi<IViewEngine>("focustense.StardewUI")!;
        // viewEngine.RegisterSprites($"{ModEntry.ModId}/sprites", "assets/sprites");
        VIEW_ASSET_PREFIX = $"{ModEntry.ModId}/views";
        VIEW_ASSET_MENU = $"{VIEW_ASSET_PREFIX}/bazaar-menu";
        viewEngine.RegisterViews(VIEW_ASSET_PREFIX, "assets/views");
#if DEBUG
        viewEngine.EnableHotReloadingWithSourceSync();
#endif
    }

    internal static bool ShowFor(GameLocation shopLocation, string shopName, ShopOwnerData? ownerData = null)
    {
#if DEBUG
        ModEntry.Log($"Show Bazaar: {shopName}");
#endif
        BazaarContextMain context = new(shopLocation, shopName, ownerData);
        Game1.activeClickableMenu = viewEngine.CreateMenuFromAsset(VIEW_ASSET_MENU, context);
        return true;
    }
}
