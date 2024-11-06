
using LivestockBazaar.Integration;
using StardewModdingAPI;
using StardewValley;

namespace LivestockBazaar.GUI;

/// <summary>Add tile action for opening and closing animal shops</summary>
internal static class BazaarMenu
{
    private static IViewEngine viewEngine = null!;
    private static string VIEW_ASSET_PREFIX = null!;
    private static string VIEW_ASSET_MENU = null!;
    internal static void Register(IModHelper helper)
    {
        viewEngine = helper.ModRegistry.GetApi<IViewEngine>("focustense.StardewUI")!;
        // viewEngine.RegisterSprites($"Mods/{ModEntry.ModId}/Sprites", "assets/sprites");
        VIEW_ASSET_PREFIX = $"Mods/{ModEntry.ModId}/Views";
        VIEW_ASSET_MENU = $"{VIEW_ASSET_PREFIX}/bazaar-menu";
        viewEngine.RegisterViews(VIEW_ASSET_PREFIX, "assets/views");
#if DEBUG
        viewEngine.EnableHotReloading();
#endif
    }

    internal static bool ShowFor(GameLocation shopLocation, string shopName)
    {
#if DEBUG
        ModEntry.Log($"Show Bazaar: {shopName}");
#endif
        object context = new BazaarContext(shopName);
        Game1.activeClickableMenu = viewEngine.CreateMenuFromAsset(VIEW_ASSET_MENU, context);
        return true;
    }
}
