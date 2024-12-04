using LivestockBazaar.Integration;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewModdingAPI.Utilities;
using StardewValley;
using StardewValley.GameData.Shops;
using StardewValley.Menus;

namespace LivestockBazaar.GUI;

/// <summary>Bazaar menu StardewUI setup.</summary>
internal static class BazaarMenu
{
    private static IViewEngine viewEngine = null!;
    internal static bool StardewUIEnabled => viewEngine is not null;
    internal static string VIEW_ASSET_PREFIX = null!;
    internal static string VIEW_ASSET_MENU = null!;

    private static readonly PerScreen<IMenuController?> menuCtrl = new();
    private static readonly PerScreen<BazaarContextMain?> context = new();

    private static IMenuController? MenuCtrl
    {
        get => menuCtrl.Value;
        set => menuCtrl.Value = value;
    }
    private static BazaarContextMain? Context
    {
        get => context.Value;
        set => context.Value = value;
    }

    internal static void Register(IModHelper helper)
    {
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
        ModEntry.Log($"Show bazaar shop '{shopName}'");
        Context = new(shopLocation, shopName, ownerData);
        MenuCtrl = viewEngine.CreateMenuControllerFromAsset(VIEW_ASSET_MENU, Context);
        MenuCtrl.CloseAction = CloseAction;
        Game1.activeClickableMenu = MenuCtrl.Menu;
        return true;
    }

    public static void CloseAction()
    {
        var justPressed = Context!.justPressed;
        IClickableMenu menu = MenuCtrl!.Menu;
        if (justPressed == null)
            return;
        if (
            Context!.SelectedLivestock != null
            && (
                justPressed == SButton.ControllerB
                || justPressed == SButton.ControllerY
                || justPressed == SButton.Escape
            )
        )
        {
            if (Context!.HoveredBuilding != null)
            {
                Context!.HoveredBuilding.BackgroundTint = Color.White;
                Context!.HoveredBuilding = null;
            }
            Context!.SelectedLivestock = null;
            Context!.justPressed = null;
        }
        else if (menu == Game1.activeClickableMenu)
        {
            Game1.exitActiveMenu();
            Context = null;
            MenuCtrl = null;
        }
        // this menu will never be child menu, no need to handle other cases for now
    }
}
