using LivestockBazaar.Integration;
using LivestockBazaar.Model;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewModdingAPI.Events;
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
    private static string viewAnimalManageTooltip = null!;

    private static readonly PerScreen<BazaarContextMain?> shopContext = new();

    private static BazaarContextMain? ShopContext
    {
        get => shopContext.Value;
        set => shopContext.Value = value;
    }

    private static readonly PerScreen<AnimalManageContext?> amContext = new();

    private static AnimalManageContext? AMContext
    {
        get => amContext.Value;
        set => amContext.Value = value;
    }

    private static readonly PerScreen<AnimalManageEntry?> amfaeEntry = new();
    private static readonly PerScreen<IViewDrawable?> amfaeTooltip = new();

    internal static AnimalManageEntry? AMFAEEntry
    {
        get => amfaeEntry.Value;
        set
        {
            amfaeEntry.Value = value;
            amfaeTooltip.Value ??= viewEngine.CreateDrawableFromAsset(viewAnimalManageTooltip);
            if (value is AnimalManageFarmAnimalEntry amfaee)
            {
                amfaeTooltip.Value.Context = amfaee;
            }
            else
            {
                amfaeTooltip.Value.Context = null;
            }
        }
    }

    internal static void Register(IModHelper helper)
    {
        viewEngine = helper.ModRegistry.GetApi<IViewEngine>("focustense.StardewUI")!;
        viewEngine.RegisterSprites($"{ModEntry.ModId}/sprites", "assets/sprites");
        viewAssetPrefix = $"{ModEntry.ModId}/views";
        viewBazaarMenu = $"{viewAssetPrefix}/bazaar-menu";
        viewAnimalManage = $"{viewAssetPrefix}/animal-manage";
        viewAnimalManageTooltip = $"{viewAssetPrefix}/includes/animal-manage-tooltip";
        viewEngine.RegisterViews(viewAssetPrefix, "assets/views");
#if DEBUG
        viewEngine.EnableHotReloadingWithSourceSync();
#endif
        helper.Events.Display.RenderedActiveMenu += OnRenderedActiveMenu;
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
        ShopContext = new(shopName, ownerData, bazaarData);
        var menuCtrl = viewEngine.CreateMenuControllerFromAsset(viewBazaarMenu, ShopContext);
        menuCtrl.CloseAction = ShopCloseAction;
        menuCtrl.EnableCloseButton();
        Game1.activeClickableMenu = menuCtrl.Menu;
        return true;
    }

    /// <summary>
    /// Special close action, return to page 1 instead of exiting completely
    /// </summary>
    public static void ShopCloseAction()
    {
        if (ShopContext!.SelectedLivestock != null)
        {
            ShopContext!.ClearSelectedLivestock();
        }
        else
        {
            Game1.exitActiveMenu();
            ShopContext = null;
            Game1.player.forceCanMove();
            ModEntry.Config.SaveConfig();
        }
    }

    /// <summary>
    /// Show the animal manager menu
    /// </summary>
    internal static void ShowAnimalManage()
    {
        AMContext = new();
        var menuCtrl = viewEngine.CreateMenuControllerFromAsset(viewAnimalManage, AMContext);
        menuCtrl.CloseAction = AMCloseAction;
        menuCtrl.EnableCloseButton();
        Game1.activeClickableMenu = menuCtrl.Menu;
        // if (Game1.activeClickableMenu == null)
        // {
        //     Game1.activeClickableMenu = menuCtrl.Menu;
        // }
        // else
        // {
        //     Game1.activeClickableMenu.SetChildMenu(menuCtrl.Menu);
        // }
    }

    private static void AMCloseAction()
    {
        amfaeTooltip.Value?.Dispose();
        amfaeTooltip.Value = null;
        Game1.exitActiveMenu();
        AMContext = null;
        Game1.player.forceCanMove();
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

    private static void OnRenderedActiveMenu(object? sender, RenderedActiveMenuEventArgs e)
    {
        if (amfaeTooltip.Value?.Context is AnimalManageFarmAnimalEntry)
        {
            float offset = 32 * Game1.options.uiScale;
            amfaeTooltip.Value.Draw(e.SpriteBatch, new Vector2(Game1.getMouseX() + offset, Game1.getMouseY() + offset));
        }
    }
}
