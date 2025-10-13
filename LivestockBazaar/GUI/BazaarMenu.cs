using LivestockBazaar.Integration;
using LivestockBazaar.Model;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using StardewValley;
using StardewValley.GameData.Shops;
using StardewValley.Menus;
using StardewValley.Network;

namespace LivestockBazaar.GUI;

/// <summary>Bazaar menu StardewUI setup.</summary>
internal static class BazaarMenu
{
    private const string MutexId = $"{ModEntry.ModId}/AnimalManageLock";
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
            if (AMContext == null)
                return;
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

    private static NetMutex AMMutex => Game1.player.team.GetOrCreateGlobalInventoryMutex(MutexId);

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
        helper.Events.Display.MenuChanged += OnMenuChanged;

        if (helper.ModRegistry.GetApi<IIconicFrameworkApi>("furyx639.ToolbarIcons") is IIconicFrameworkApi iconic)
        {
            iconic.AddToolbarIcon(
                $"{ModEntry.ModId}/AnimalManage",
                "LooseSprites/emojis",
                new(36, 45, 9, 9),
                I18n.GUI_LivestockBazaar_Title,
                I18n.GUI_AnimalManage_Title,
                ShowAnimalManage
            );
        }
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
            ShopContext = null;
            Game1.exitActiveMenu();
            Game1.player.forceCanMove();
            ModEntry.Config.SaveConfig();
        }
    }

    private static void ShowAnimalManageInner()
    {
        try
        {
            AMContext = new();
        }
        catch (ArgumentException)
        {
            ModEntry.Log("No animal buildings to manage.", LogLevel.Info);
            return;
        }
        var menuCtrl = viewEngine.CreateMenuControllerFromAsset(viewAnimalManage, AMContext);
        menuCtrl.Closing += AMClosing;
        menuCtrl.EnableCloseButton();
        if (Game1.activeClickableMenu != null)
        {
            Game1.activeClickableMenu.SetChildMenu(menuCtrl.Menu);
        }
        else
        {
            menuCtrl.EnableCloseButton();
            Game1.activeClickableMenu = menuCtrl.Menu;
        }
    }

    private static void ShowAnimalManageCannot()
    {
        Game1.addHUDMessage(new HUDMessage(I18n.GUI_AnimalManage_Cannot()) { noIcon = true });
    }

    /// <summary>
    /// Show the animal manager menu
    /// </summary>
    internal static void ShowAnimalManage()
    {
        AMMutex.RequestLock(ShowAnimalManageInner, ShowAnimalManageCannot);
    }

    private static void AMClosing()
    {
        amfaeEntry.Value = null;
        amfaeTooltip.Value?.Dispose();
        amfaeTooltip.Value = null;
        AMContext = null;
        AMMutex.ReleaseLock();
    }

    private static void OnRenderedActiveMenu(object? sender, RenderedActiveMenuEventArgs e)
    {
        if (amfaeTooltip.Value?.Context is AnimalManageFarmAnimalEntry)
        {
            float offset = 32 * Game1.options.uiScale;
            amfaeTooltip.Value.Draw(e.SpriteBatch, new Vector2(Game1.getMouseX() + offset, Game1.getMouseY() + offset));
        }
    }

    private static void OnMenuChanged(object? sender, MenuChangedEventArgs e)
    {
        if (AMContext != null && (e.OldMenu is AnimalQueryMenu aqm))
        {
            aqm.exitFunction();
        }
    }
}
