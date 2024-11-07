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
using System.Reflection.Emit;

namespace LivestockBazaar;

/// <summary>Add tile action for opening and closing animal shops</summary>
internal static class OpenBazaar
{
    /// <summary>Tile action to open FAB shop</summary>
    internal static readonly string TileAction_Shop = $"{ModEntry.ModId}_Shop";

    /// <summary>Delegate for opening shop</summary>
    internal static Func<GameLocation, string, bool> ShowShopDelegate =>
        (!BazaarMenu.StardewUIEnabled || ModEntry.Config.VanillaLivestockMenu) ?
        ShowVanillaAnimalShop : BazaarMenu.ShowFor;

    /// <summary>Location that opened the shop, used to warp back after buying animal.</summary>
    internal static string ShopLocationName { get; set; } = "AnimalShop";

    /// <summary>NPC that owns the shop, if applicable, used to show message.</summary>
    internal static string? ShopOwnerNPCName { get; set; } = null;

    internal static void Register(IModHelper helper, Harmony harmony)
    {
        GameLocation.RegisterTileAction(TileAction_Shop, TileAction_ShowLivestockShop);
        helper.ConsoleCommands.Add(
            "lb-shop",
            "Triggers sowing (planting of seed and fertilizer from attachment) on all sprinklers with applicable attachment.",
            Console_ShowLivestockShop
        );

        try
        {
            // change Marnie's shop
            harmony.Patch(
                original: AccessTools.DeclaredMethod(typeof(GameLocation), nameof(GameLocation.ShowAnimalShopMenu)),
                prefix: new HarmonyMethod(typeof(OpenBazaar), nameof(GameLocation_ShowAnimalShopMenu_Prefix))
            );
            // these 3 patches are needed if using vanilla menu
            harmony.Patch(
                original: AccessTools.DeclaredMethod(typeof(PurchaseAnimalsMenu), nameof(PurchaseAnimalsMenu.setUpForReturnAfterPurchasingAnimal)),
                transpiler: new HarmonyMethod(typeof(OpenBazaar), nameof(PurchaseAnimalsMenu_ReturnToPreviousLocation_Transpiler))
            );
            harmony.Patch(
                original: AccessTools.DeclaredMethod(typeof(PurchaseAnimalsMenu), nameof(PurchaseAnimalsMenu.setUpForReturnToShopMenu)),
                transpiler: new HarmonyMethod(typeof(OpenBazaar), nameof(PurchaseAnimalsMenu_ReturnToPreviousLocation_Transpiler))
            );
            harmony.Patch(
                original: AccessTools.DeclaredMethod(typeof(PurchaseAnimalsMenu), nameof(PurchaseAnimalsMenu.marnieAnimalPurchaseMessage)),
                transpiler: new HarmonyMethod(typeof(OpenBazaar), nameof(PurchaseAnimalsMenu_marnieAnimalPurchaseMessage_Transpiler))
            );
        }
        catch (Exception err)
        {
            ModEntry.Log($"Failed to patch LivestockBazaar:\n{err}", LogLevel.Error);
        }
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
        ShopOwnerNPCName = null;
        ShowShopDelegate(Game1.currentLocation, shopName);
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
            ShopOwnerNPCName = foundOwnerData.Name;
        }
        // show shop
        return ShowShopDelegate(location, shopName);
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
            // use custom stock and maybe menu
            ShopOwnerNPCName = AssetManager.MARNIE;
            ShowShopDelegate(__instance, AssetManager.MARNIE);
            return false;
        }
        catch (Exception err)
        {
            ModEntry.Log($"Error in GameLocation_ShowAnimalShopMenu_Prefix:\n{err}", LogLevel.Error);
            return false;
        }
    }

    /// <summary>Swap</summary>
    /// <param name="instructions"></param>
    /// <param name="generator"></param>
    /// <returns></returns>
    private static IEnumerable<CodeInstruction> PurchaseAnimalsMenu_ReturnToPreviousLocation_Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
    {
        try
        {
            CodeMatcher matcher = new(instructions, generator);
            // ldstr "AnimalShop"
            // ldc.i4.0
            // call class StardewValley.LocationRequest StardewValley.Game1::getLocationRequest(string, bool)

            matcher.Start()
            .MatchStartForward([
                new(OpCodes.Ldstr, "AnimalShop"),
                new(OpCodes.Ldc_I4_0),
                new(OpCodes.Call, AccessTools.Method(typeof(Game1), nameof(Game1.getLocationRequest)))
            ]);
            matcher.Opcode = OpCodes.Call;
            matcher.Operand = AccessTools.PropertyGetter(typeof(OpenBazaar), nameof(ShopLocationName));

            return matcher.Instructions();
        }
        catch (Exception err)
        {
            ModEntry.Log($"Error in PurchaseAnimalsMenu_ReturnToPreviousLocation_Transpiler:\n{err}", LogLevel.Error);
            return instructions;
        }
    }

    private static IEnumerable<CodeInstruction> PurchaseAnimalsMenu_marnieAnimalPurchaseMessage_Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
    {
        try
        {
            CodeMatcher matcher = new(instructions, generator);
            // IL_0018: ldstr "Marnie"
            // IL_001d: ldc.i4.1
            // IL_001e: ldc.i4.0
            // IL_001f: call class StardewValley.NPC StardewValley.Game1::getCharacterFromName(string, bool, bool)

            matcher.Start()
            .MatchEndForward([
                new(OpCodes.Ldstr, "Marnie"),
                new(OpCodes.Ldc_I4_1),
                new(OpCodes.Ldc_I4_0),
                // magic knowledge about this method, strangely hard to match for
                new(OpCodes.Call)
            ]);
            matcher.Operand = AccessTools.DeclaredMethod(typeof(OpenBazaar), nameof(GetShopOwnerNPC));

            return matcher.Instructions();
        }
        catch (Exception err)
        {
            ModEntry.Log($"Error in PurchaseAnimalsMenu_marnieAnimalPurchaseMessage_Transpiler:\n{err}", LogLevel.Error);
            return instructions;
        }
    }

    private static NPC GetShopOwnerNPC(string _name, bool mustBeVillager = true, bool includeEventActors = false)
    {
        if (ShopOwnerNPCName != null)
        {
            return Game1.getCharacterFromName(ShopOwnerNPCName, mustBeVillager, includeEventActors);
        }
        return null!;
    }

    /// <summary>Show the vanilla animal shop, but with custom stock rules</summary>
    /// <param name="shopName"></param>
    /// <returns></returns>
    public static bool ShowVanillaAnimalShop(GameLocation shopLocation, string shopName)
    {
        ShopLocationName = shopLocation.Name;
        List<KeyValuePair<string, string>> list = [];
        foreach (GameLocation location in Game1.locations)
        {
            if (location.buildings.Any((Building p) => p.GetIndoors() is AnimalHouse) && (!Game1.IsClient || location.CanBeRemotedlyViewed()))
            {
                list.Add(new KeyValuePair<string, string>(location.NameOrUniqueName, location.DisplayName));
            }
        }
        if (!list.Any())
        {
            Farm farm = Game1.getFarm();
            list.Add(new KeyValuePair<string, string>(farm.NameOrUniqueName, farm.DisplayName));
        }
        Game1.currentLocation.ShowPagedResponses(Game1.content.LoadString("Strings\\StringsFromCSFiles:PurchaseAnimalsMenu.ChooseLocation"), list, delegate (string value)
        {
            GameLocation locationFromName = Game1.getLocationFromName(value);
            if (locationFromName != null)
            {
                Game1.activeClickableMenu = new PurchaseAnimalsMenu(
                    AssetManager.GetAnimalStockData(shopName)
                        .Select((entry) => new SObject("100", 1, isRecipe: false, entry.Data.PurchasePrice)
                        {
                            Name = entry.Key,
                            Type = entry.AvailableForLocation(locationFromName) ?
                                null : ((entry.Data.ShopMissingBuildingDescription == null) ?
                                    "" : TokenParser.ParseText(entry.Data.ShopMissingBuildingDescription)),
                            displayNameFormat = entry.Data.ShopDisplayName
                        })
                        .ToList(),
                    locationFromName
                );
            }
        }, auto_select_single_choice: true);
        return true;
    }
}

