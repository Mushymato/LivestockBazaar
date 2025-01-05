using LivestockBazaar.Integration;
using LivestockBazaar.Model;
using StardewModdingAPI;
using StardewModdingAPI.Utilities;
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

    private static readonly PerScreen<BazaarContextMain?> context = new();

    private static BazaarContextMain? Context
    {
        get => context.Value;
        set => context.Value = value;
    }

    internal static void Register(IModHelper helper)
    {
        viewEngine = helper.ModRegistry.GetApi<IViewEngine>("focustense.StardewUI")!;
        viewEngine.RegisterSprites($"{ModEntry.ModId}/sprites", "assets/sprites");
        VIEW_ASSET_PREFIX = $"{ModEntry.ModId}/views";
        VIEW_ASSET_MENU = $"{VIEW_ASSET_PREFIX}/bazaar-menu";
        viewEngine.RegisterViews(VIEW_ASSET_PREFIX, "assets/views");
#if DEBUG
        viewEngine.EnableHotReloadingWithSourceSync();
#endif
    }

    internal static bool ShowFor(string shopName, ShopOwnerData? ownerData = null, BazaarData? bazaarData = null)
    {
        ModEntry.Log($"Show bazaar shop '{shopName}'");
        Context = new(shopName, ownerData, bazaarData);
        var menuCtrl = viewEngine.CreateMenuControllerFromAsset(VIEW_ASSET_MENU, Context);
        menuCtrl.CloseAction = CloseAction;
        menuCtrl.EnableCloseButton();
        Game1.activeClickableMenu = menuCtrl.Menu;
        return true;
    }

    public static void CloseAction()
    {
        if (Context!.SelectedLivestock != null)
        {
            Context!.ClearSelectedLivestock();
        }
        else
        {
            Game1.exitActiveMenu();
            Context = null;
            Game1.player.forceCanMove();
            ModEntry.Config.SaveConfig();
        }
    }
}
