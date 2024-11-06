using LivestockBazaar.GUI;
using HarmonyLib;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Buildings;
using StardewValley.GameData.Shops;
using StardewValley.Internal;
using StardewValley.Menus;
using StardewValley.TokenizableStrings;

namespace LivestockBazaar;

/// <summary>Add tile action for opening and closing animal shops</summary>
internal static class OpenBazaar
{
    /// <summary>Tile action to open FAB shop</summary>
    internal static readonly string TileAction_Shop = $"{ModEntry.ModId}_Shop";

    internal static void Register(IModHelper helper, Harmony harmony)
    {
        GameLocation.RegisterTileAction(TileAction_Shop, TileAction_ShowLivestockShop);
        harmony.Patch(
            original: AccessTools.DeclaredMethod(typeof(GameLocation), nameof(GameLocation.ShowAnimalShopMenu)),
            prefix: new HarmonyMethod(typeof(OpenBazaar), nameof(GameLocation_ShowAnimalShopMenu_Prefix))
        );
        helper.ConsoleCommands.Add(
            "lb-shop",
            "Triggers sowing (planting of seed and fertilizer from attachment) on all sprinklers with applicable attachment.",
            Console_ShowLivestockShop
        );
    }

    /// <summary>Show specific FAB shop, using vanilla menu</summary>
    /// <param name="command"></param>
    /// <param name="args"></param>
    private static void Console_ShowLivestockShop(string command, string[] args)
    {
        if (!Context.IsWorldReady)
        {
            ModEntry.Log("Must load save first.", LogLevel.Error);
            return;
        }
        if (!ArgUtility.TryGet(args, 0, out var shopName, out string error, allowBlank: true, "string shopId"))
        {
            ModEntry.Log(error, LogLevel.Error);
        }
        ModEntry.Log($"Show animal shop '{shopName}'", LogLevel.Info);
        BazaarMenu.ShowFor(shopName);
    }

    /// <summary>Tile Action show shop, do checks for open/close time and owner present as required</summary>
    /// <param name="location"></param>
    /// <param name="action"></param>
    /// <param name="who"></param>
    /// <param name="tile"></param>
    /// <returns></returns>
    private static bool TileAction_ShowLivestockShop(GameLocation location, string[] action, Farmer who, Point tile)
    {
        if (!ArgUtility.TryGet(action, 1, out var shopName, out string error, allowBlank: true, "string shopId") ||
            !ArgUtility.TryGetOptional(action, 2, out var direction, out error, null, allowBlank: true, "string direction") ||
            !ArgUtility.TryGetOptionalInt(action, 3, out var openTime, out error, -1, "int openTime") ||
            !ArgUtility.TryGetOptionalInt(action, 4, out var closeTime, out error, -1, "int closeTime") ||
            !ArgUtility.TryGetOptionalInt(action, 5, out var shopAreaX, out error, -1, "int shopAreaX") ||
            !ArgUtility.TryGetOptionalInt(action, 6, out var shopAreaY, out error, -1, "int shopAreaY") ||
            !ArgUtility.TryGetOptionalInt(action, 7, out var shopAreaWidth, out error, -1, "int shopAreaWidth") ||
            !ArgUtility.TryGetOptionalInt(action, 8, out var shopAreaHeight, out error, -1, "int shopAreaHeight"))
        {
            ModEntry.Log(error, LogLevel.Error);
            return false;
        }
        // check interact direction
        switch (direction)
        {
            case "down":
                if (who.TilePoint.Y < tile.Y)
                    return false;
                break;
            case "up":
                if (who.TilePoint.Y > tile.Y)
                    return false;
                break;
            case "left":
                if (who.TilePoint.X > tile.X)
                    return false;
                break;
            case "right":
                if (who.TilePoint.X < tile.X)
                    return false;
                break;
        }
        if (AssetManager.BazaarData.TryGetValue(shopName, out BazaarData? shopData) && shopData.ShouldCheckShopOpen(who))
        {
            // check opening and closing times
            if ((openTime >= 0 && Game1.timeOfDay < openTime) || (closeTime >= 0 && Game1.timeOfDay >= closeTime))
            {
                Wheels.DisplayShopTimes(openTime, closeTime);
                return false;
            }
            var shopOwnerDatas = ShopBuilder.GetCurrentOwners(shopData);
            ShopOwnerData? foundOwnerData = null;
            // check owner is within rect
            if (shopAreaX != -1 || shopAreaY != -1 || shopAreaWidth != -1 || shopAreaHeight != -1)
            {
                if (shopAreaX == -1 || shopAreaY == -1 || shopAreaWidth == -1 || shopAreaHeight == -1)
                {
                    ModEntry.Log("when specifying any of the shop area 'x y width height' arguments (indexes 5-8), all four must be specified", LogLevel.Error);
                    return false;
                }
                Rectangle ownerRect = new(shopAreaX, shopAreaY, shopAreaWidth, shopAreaHeight);
                IList<NPC>? locNPCs = location.currentEvent?.actors;
                locNPCs ??= location.characters;

                foreach (ShopOwnerData ownerData in shopOwnerDatas)
                {
                    foreach (NPC npc in locNPCs)
                    {
                        if (ownerRect.Contains(npc.TilePoint) && ownerData.IsValid(npc.Name))
                        {
                            foundOwnerData = ownerData;
                            break;
                        }
                    }
                }
            }
            else
            {
                foundOwnerData = shopOwnerDatas.FirstOrDefault();
            }
            if (foundOwnerData == null)
                return false;
            else if (foundOwnerData.ClosedMessage != null)
            {
                Game1.drawObjectDialogue(TokenParser.ParseText(foundOwnerData.ClosedMessage));
                return false;
            }
        }
        // show shop
        return BazaarMenu.ShowFor(shopName);
    }

    /// <summary>Override marnie shop and menu, if enabled in config</summary>
    /// <param name="__instance"></param>
    /// <param name="onMenuOpened"></param>
    /// <returns></returns>
    public static bool GameLocation_ShowAnimalShopMenu_Prefix(GameLocation __instance, Action<PurchaseAnimalsMenu> onMenuOpened)
    {
        try
        {
            // if ModEntry.Config.VanillaMarnieShop is true or if this menu uses the PurchaseAnimalsMenu delegate, use vanilla
            if (ModEntry.Config.VanillaMarnieStock || onMenuOpened != null)
                return true;
            // use custom menu
            BazaarMenu.ShowFor(AssetManager.MARNIE);
            return false;
        }
        catch (Exception err)
        {
            ModEntry.Log($"Error in GameLocation_ShowAnimalShopMenu_Prefix:\n{err}", LogLevel.Error);
            return false;
        }
    }

    // /// <summary>Show the vanilla animal shop, but with custom stock rules</summary>
    // /// <param name="shopName"></param>
    // /// <returns></returns>
    // public static bool ShowVanillaAnimalShop(string shopName)
    // {
    //     List<KeyValuePair<string, string>> list = [];
    //     foreach (GameLocation location in Game1.locations)
    //     {
    //         if (location.buildings.Any((Building p) => p.GetIndoors() is AnimalHouse) && (!Game1.IsClient || location.CanBeRemotedlyViewed()))
    //         {
    //             list.Add(new KeyValuePair<string, string>(location.NameOrUniqueName, location.DisplayName));
    //         }
    //     }
    //     if (!list.Any())
    //     {
    //         Farm farm = Game1.getFarm();
    //         list.Add(new KeyValuePair<string, string>(farm.NameOrUniqueName, farm.DisplayName));
    //     }
    //     Game1.currentLocation.ShowPagedResponses(Game1.content.LoadString("Strings\\StringsFromCSFiles:PurchaseAnimalsMenu.ChooseLocation"), list, delegate (string value)
    //     {
    //         GameLocation locationFromName = Game1.getLocationFromName(value);
    //         if (locationFromName != null)
    //         {
    //             Game1.activeClickableMenu = new PurchaseAnimalsMenu(
    //                 AssetManager.GetAnimalStockData(shopName, location: locationFromName)
    //                     .Select((entry) => new SObject("100", 1, isRecipe: false, entry.Data.PurchasePrice)
    //                     {
    //                         Name = entry.Key,
    //                         Type = entry.AvailableForLocation ? null : ((entry.Data.ShopMissingBuildingDescription == null) ? "" : TokenParser.ParseText(entry.Data.ShopMissingBuildingDescription)),
    //                         displayNameFormat = entry.Data.ShopDisplayName
    //                     })
    //                     .ToList(),
    //                 locationFromName
    //             );
    //         }
    //     }, auto_select_single_choice: true);
    //     return true;
    // }
}

