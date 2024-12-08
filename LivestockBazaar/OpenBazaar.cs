using System.Reflection.Emit;
using HarmonyLib;
using LivestockBazaar.GUI;
using LivestockBazaar.Model;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Delegates;
using StardewValley.GameData.Shops;
using StardewValley.Menus;
using StardewValley.TokenizableStrings;
using StardewValley.Triggers;

namespace LivestockBazaar;

/// <summary>Add tile action for opening and closing animal shops</summary>
internal static class OpenBazaar
{
    /// <summary>Tile action to open FAB shop</summary>
    internal static string LivestockShop => $"{ModEntry.ModId}_Shop";

    internal static void Register(IModHelper helper)
    {
        GameLocation.RegisterTileAction(LivestockShop, TileAction_ShowLivestockShop);
        TriggerActionManager.RegisterAction(LivestockShop, Action_ShowLivestockShop);
        helper.ConsoleCommands.Add("lb-shop", "Open a custom livestock shop by id", Console_ShowLivestockShop);

        // try
        // {
        //     // change Marnie's shop
        //     harmony.Patch(
        //         original: AccessTools.DeclaredMethod(typeof(GameLocation), nameof(GameLocation.ShowAnimalShopMenu)),
        //         prefix: new HarmonyMethod(typeof(OpenBazaar), nameof(GameLocation_ShowAnimalShopMenu_Prefix))
        //     );
        // }
        // catch (Exception err)
        // {
        //     ModEntry.Log($"Failed to patch LivestockBazaar:\n{err}", LogLevel.Error);
        // }
    }

    /// <summary>Show livestock bazaar menu</summary>
    /// <param name="command"></param>
    /// <param name="args"></param>
    private static bool Args_ShowLivestockShop(string[] args, out string error)
    {
        if (!ArgUtility.TryGet(args, 0, out var shopName, out error, allowBlank: true, "string shopId"))
        {
            ModEntry.Log(error, LogLevel.Error);
            return false;
        }
        ModEntry.Log($"Show animal shop '{shopName}'");
        return BazaarMenu.ShowFor(shopName, null);
    }

    private static void Console_ShowLivestockShop(string command, string[] args)
    {
        if (!Context.IsWorldReady)
        {
            ModEntry.Log("Must load save first.", LogLevel.Error);
            return;
        }
        if (!Args_ShowLivestockShop(args, out string error))
            ModEntry.Log(error, LogLevel.Error);
    }

    private static bool Action_ShowLivestockShop(string[] args, TriggerActionContext context, out string error)
    {
        return Args_ShowLivestockShop(args, out error);
    }

    private static bool CheckShopOpen(
        GameLocation location,
        IEnumerable<ShopOwnerData> shopOwnerDatas,
        int openTime,
        int closeTime,
        int shopAreaX,
        int shopAreaY,
        int shopAreaWidth,
        int shopAreaHeight,
        out ShopOwnerData? foundOwnerData
    )
    {
        foundOwnerData = null;
        // check opening and closing times
        if ((openTime >= 0 && Game1.timeOfDay < openTime) || (closeTime >= 0 && Game1.timeOfDay >= closeTime))
        {
            // shop closed
            Wheels.DisplayShopTimes(openTime, closeTime);
            return false;
        }

        // check owner is within rect
        if (shopAreaX != -1 || shopAreaY != -1 || shopAreaWidth != -1 || shopAreaHeight != -1)
        {
            if (shopAreaX == -1 || shopAreaY == -1 || shopAreaWidth == -1 || shopAreaHeight == -1)
            {
                // invalid rect
                ModEntry.Log(
                    "when specifying any of the shop area 'x y width height' arguments (indexes 5-8), all four must be specified",
                    LogLevel.Error
                );
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
                        // found npc
                        foundOwnerData = ownerData;
                        return true;
                    }
                }
            }
            return false;
        }

        // either didnt need to check check, or passed both
        return true;
    }

    /// <summary>Tile Action show shop, do checks for open/close time and owner present as required</summary>
    /// <param name="location"></param>
    /// <param name="action"></param>
    /// <param name="who"></param>
    /// <param name="tile"></param>
    /// <returns></returns>
    private static bool TileAction_ShowLivestockShop(GameLocation location, string[] action, Farmer who, Point tile)
    {
        if (
            !ArgUtility.TryGet(action, 1, out var shopName, out string error, allowBlank: true, "string shopId")
            || !ArgUtility.TryGetOptional(
                action,
                2,
                out var direction,
                out error,
                null,
                allowBlank: true,
                "string direction"
            )
            || !ArgUtility.TryGetOptionalInt(action, 3, out int openTime, out error, -1, "int openTime")
            || !ArgUtility.TryGetOptionalInt(action, 4, out int closeTime, out error, -1, "int closeTime")
            || !ArgUtility.TryGetOptionalInt(action, 5, out int shopAreaX, out error, -1, "int shopAreaX")
            || !ArgUtility.TryGetOptionalInt(action, 6, out int shopAreaY, out error, -1, "int shopAreaY")
            || !ArgUtility.TryGetOptionalInt(action, 7, out int shopAreaWidth, out error, -1, "int shopAreaWidth")
            || !ArgUtility.TryGetOptionalInt(action, 8, out int shopAreaHeight, out error, -1, "int shopAreaHeight")
        )
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
        ShopOwnerData? foundOwnerData = null;
        if (AssetManager.BazaarData.TryGetValue(shopName, out BazaarData? bazaarData))
        {
            var shopOwnerDatas = bazaarData.GetCurrentOwners();
            bool shouldCheck = bazaarData.ShouldCheckShopOpen(who);
            if (
                CheckShopOpen(
                    location,
                    shopOwnerDatas,
                    openTime,
                    closeTime,
                    shopAreaX,
                    shopAreaY,
                    shopAreaWidth,
                    shopAreaHeight,
                    out foundOwnerData
                ) || !shouldCheck
            )
            {
                foundOwnerData ??= BazaarData.GetAwayOwner(shopOwnerDatas) ?? shopOwnerDatas.FirstOrDefault();
            }
            else
            {
                return false;
            }
            if (foundOwnerData?.ClosedMessage != null)
            {
                Game1.drawObjectDialogue(TokenParser.ParseText(foundOwnerData.ClosedMessage));
                return false;
            }
        }
        // show shop
        return BazaarMenu.ShowFor(shopName, foundOwnerData);
    }

    /// <summary>Override marnie shop and menu, if enabled in config</summary>
    /// <param name="__instance"></param>
    /// <param name="onMenuOpened"></param>
    /// <returns></returns>
    public static bool GameLocation_ShowAnimalShopMenu_Prefix(
        GameLocation __instance,
        Action<PurchaseAnimalsMenu> onMenuOpened
    )
    {
        try
        {
            // if ModEntry.Config.VanillaMarnieShop is true or if this menu uses the PurchaseAnimalsMenu delegate, use vanilla
            if (ModEntry.Config.VanillaMarnieStock || onMenuOpened != null)
                return true;
            // use custom stock and menu
            BazaarMenu.ShowFor(Wheels.MARNIE, null);
            return false;
        }
        catch (Exception err)
        {
            ModEntry.Log($"Error in GameLocation_ShowAnimalShopMenu_Prefix:\n{err}", LogLevel.Error);
            return false;
        }
    }
}
