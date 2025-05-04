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
    private static string viewAssetPrefix = null!;
    private static string viewBazaarMenu = null!;
    private static string viewAnimalManage = null!;

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
        viewAssetPrefix = $"{ModEntry.ModId}/views";
        viewBazaarMenu = $"{viewAssetPrefix}/bazaar-menu";
        viewAnimalManage = $"{viewAssetPrefix}/animal-manage";
        viewEngine.RegisterViews(viewAssetPrefix, "assets/views");
#if DEBUG
        viewEngine.EnableHotReloadingWithSourceSync();
#endif
    }

    /// <summary>
    /// Display a bazaar shop menu
    /// </summary>
    /// <param name="shopName"></param>
    /// <param name="ownerData"></param>
    /// <param name="bazaarData"></param>
    /// <returns></returns>
    internal static bool ShowFor(string shopName, ShopOwnerData? ownerData = null, BazaarData? bazaarData = null)
    {
        ModEntry.Log($"Show bazaar shop '{shopName}'");
        Context = new(shopName, ownerData, bazaarData);
        var menuCtrl = viewEngine.CreateMenuControllerFromAsset(viewBazaarMenu, Context);
        menuCtrl.CloseAction = CloseAction;
        menuCtrl.EnableCloseButton();
        Game1.activeClickableMenu = menuCtrl.Menu;
        return true;
    }

    /// <summary>
    /// Special close action, return to page 1 instead of exiting completely
    /// </summary>
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

    internal static void ShowAnimalManage()
    {
        AnimalManageContext context = new();
        var menuCtrl = viewEngine.CreateMenuControllerFromAsset(viewAnimalManage, context);
        menuCtrl.EnableCloseButton();
        if (Game1.activeClickableMenu == null)
        {
            Game1.activeClickableMenu = menuCtrl.Menu;
        }
        else
        {
            Game1.activeClickableMenu.SetChildMenu(menuCtrl.Menu);
        }
    }

    /// <summary>
    /// Better game menu integration, add animal manage to right click context
    /// </summary>
    /// <param name="BGM"></param>
    public static void RegisterBGMContextMenu(IBetterGameMenuApi BGM)
    {
        BGM.OnTabContextMenu(ShowAnimalManageFromBGM);
    }

    private static void ShowAnimalManageFromBGM(ITabContextMenuEvent evt)
    {
        if (evt.Tab == nameof(VanillaTabOrders.Animals))
            evt.Entries.Add(evt.CreateEntry(I18n.CMCT_LivestockBazaar_AnimalManage(), ShowAnimalManage));
    }
}
