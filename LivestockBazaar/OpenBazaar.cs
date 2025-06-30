using LivestockBazaar.GUI;
using LivestockBazaar.Model;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Delegates;
using StardewValley.GameData.Shops;
using StardewValley.TokenizableStrings;
using StardewValley.Triggers;

namespace LivestockBazaar;

/// <summary>Add tile action for opening and closing animal shops</summary>
public static class OpenBazaar
{
    /// <summary>Tile action to open LB shop</summary>
    internal const string LivestockShop = $"{ModEntry.ModId}_Shop";
    internal const string LivestockShopArgs = $"{ModEntry.ModId}_ShopTile";

    internal static void Register(IModHelper helper)
    {
        GameLocation.RegisterTileAction(LivestockShop, TileAction_ShowLivestockShop);
        TriggerActionManager.RegisterAction(LivestockShop, Action_ShowLivestockShop);
        helper.ConsoleCommands.Add("lb-shop", "Open a custom livestock shop by id", Console_ShowLivestockShop);
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
        return BazaarMenu.ShowFor(shopName, null);
    }

    private static void Console_ShowLivestockShop(string command, string[] args)
    {
        if (!Context.IsWorldReady)
        {
            ModEntry.Log("Must load save first.", LogLevel.Error);
            return;
        }
        Args_ShowLivestockShop(args, out string _);
    }

    private static bool Action_ShowLivestockShop(string[] args, TriggerActionContext context, out string error)
    {
        return Args_ShowLivestockShop(args, out error);
    }

    public static bool InteractShowLivestockShop(StardewValley.Object machine, GameLocation location, Farmer player)
    {
        if (machine.GetMachineData()?.CustomFields is Dictionary<string, string> customFields)
        {
            if (customFields.TryGetValue(LivestockShop, out string? shopArgs))
            {
                string[] args = ArgUtility.SplitBySpaceQuoteAware(shopArgs);
                return Args_ShowLivestockShop(args, out _);
            }
            else if (location != null && player != null && customFields.TryGetValue(LivestockShopArgs, out shopArgs))
            {
                string[] args = ArgUtility.SplitBySpaceQuoteAware(shopArgs);
                return TileAction_ShowLivestockShop(location, ["", .. args], player, machine.TileLocation.ToPoint());
            }
        }
        return false;
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
        out ShopOwnerData? foundOwnerData,
        out NPC? foundNPC
    )
    {
        foundOwnerData = null;
        foundNPC = null;
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
                        foundNPC = npc;
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
                    out foundOwnerData,
                    out NPC? foundNPC
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
            // check if we need to show a dialog
            List<Response> responses = [];
            if (AssetManager.HasAnyLivestockDataForShop(shopName))
                responses.Add(
                    new Response("Animals", Wheels.ParseTextOrDefault(bazaarData.ShopDialogAnimals, "Animals"))
                );
            if (bazaarData.ShowShopDialog)
                responses.Insert(
                    0,
                    new Response("Supplies", Wheels.ParseTextOrDefault(bazaarData.ShopDialogSupplies, "Supplies"))
                );
            if (bazaarData.ShowPetShopDialog)
                responses.Add(new Response("Adopt", Wheels.ParseTextOrDefault(bazaarData.ShopDialogAdopt, "Adopt")));

            if (responses.Count <= 0)
                return false;
            void shopHandler(Farmer _, string whichAnswer)
            {
                switch (whichAnswer)
                {
                    case "Supplies":
                        Utility.TryOpenShopMenu(bazaarData.ShopId, foundNPC?.Name ?? "AnyOrNone");
                        break;
                    case "Animals":
                        BazaarMenu.ShowFor(shopName, foundOwnerData);
                        break;
                    case "Adopt":
                        Utility.TryOpenShopMenu(bazaarData.PetShopId, foundNPC?.Name ?? "AnyOrNone");
                        break;
                    case "Leave":
                    default:
                        break;
                }
            }
            if (responses.Count > 1)
            {
                responses.Add(new Response("Leave", Wheels.ParseTextOrDefault(bazaarData.ShopDialogLeave)));
                location.createQuestionDialogue("", responses.ToArray(), shopHandler, speaker: foundNPC);
            }
            else
            {
                shopHandler(who, responses[0].responseKey);
            }
            return true;
        }
        // show shop, no bazaar data
        return BazaarMenu.ShowFor(shopName, foundOwnerData);
    }

    private static bool TileAcction_AnimalShop(GameLocation location, string[] action, Farmer who, Point tile)
    {
        ModEntry.LogOnce("Successfully activated alternate 'AnimalShop' action.");
        return TileAction_ShowLivestockShop(
            location,
            [LivestockShop, Wheels.MARNIE, "down", "-1", "-1", "12", "14", "2", "1"],
            who,
            tile
        );
    }
}
