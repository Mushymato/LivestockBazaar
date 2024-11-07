using System.Runtime.CompilerServices;
using LivestockBazaar.Integration;
using StardewModdingAPI;
using StardewValley;

namespace LivestockBazaar.GUI;

/// <summary>Bazaar menu StardewUI setup.</summary>
internal static class BazaarMenu
{
    private static IViewEngine viewEngine = null!;
    internal static bool StardewUIEnabled => viewEngine is not null;
    private static string VIEW_ASSET_PREFIX = null!;
    private static string VIEW_ASSET_MENU = null!;

    /// <summary>Reload with project sync</summary>
    /// <param name="callerFilePath"></param>
    public static void EnableHotReloadingWithProjectSync([CallerFilePath] string? callerFilePath = null)
    {
        if (callerFilePath != null &&
            Directory.GetParent(callerFilePath)?.Parent?.FullName is string projectRoot)
        {
            viewEngine.EnableHotReloading(projectRoot);
        }
    }

    internal static void Register(IModHelper helper)
    {
        if (!helper.ModRegistry.IsLoaded("focustense.StardewUI"))
        {
            ModEntry.Log("focustense.StardewUI not available, will use vanilla menu.", LogLevel.Info);
            return;
        }
        viewEngine = helper.ModRegistry.GetApi<IViewEngine>("focustense.StardewUI")!;
        // viewEngine.RegisterSprites($"Mods/{ModEntry.ModId}/Sprites", "assets/sprites");
        VIEW_ASSET_PREFIX = $"Mods/{ModEntry.ModId}/Views";
        VIEW_ASSET_MENU = $"{VIEW_ASSET_PREFIX}/bazaar-menu";
        viewEngine.RegisterViews(VIEW_ASSET_PREFIX, "assets/views");
#if DEBUG
        EnableHotReloadingWithProjectSync();
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
