using HarmonyLib;
using LivestockBazaar.GUI;
using LivestockBazaar.Model;
using StardewModdingAPI;
using StardewValley;
using StardewValley.GameData.Shops;
using StardewValley.Menus;
using StardewValley.Triggers;

namespace LivestockBazaar;

internal static class Patches
{
    internal static string PurchasedAnimal_Trigger => $"{ModEntry.ModId}_purchasedAnimal";

    public static void Patch(Harmony patcher)
    {
        try
        {
            patcher.Patch(
                original: AccessTools.DeclaredMethod(typeof(GameLocation), nameof(GameLocation.ShowAnimalShopMenu)),
                prefix: new HarmonyMethod(typeof(Patches), nameof(GameLocation_ShowAnimalShopMenu_Prefix))
            );
            patcher.Patch(
                original: AccessTools.DeclaredMethod(typeof(AnimalHouse), nameof(AnimalHouse.adoptAnimal)),
                postfix: new HarmonyMethod(typeof(Patches), nameof(AnimalHouse_adoptAnimal_Postfix))
            );
            TriggerActionManager.RegisterTrigger(PurchasedAnimal_Trigger);
        }
        catch (Exception err)
        {
            ModEntry.Log($"Failed to patch LivestockBazaar:\n{err}", LogLevel.Error);
        }
    }

    private static bool GameLocation_ShowAnimalShopMenu_Prefix(Action<PurchaseAnimalsMenu> onMenuOpened)
    {
        if (onMenuOpened == null && !ModEntry.Config.VanillaMarnieStock)
        {
            ModEntry.Log("Replace original animal shop menu.");
            ShopOwnerData? marnieOwnerData = null;
            if (AssetManager.BazaarData.TryGetValue(Wheels.MARNIE, out BazaarData? bazaarData))
            {
                marnieOwnerData = bazaarData.GetCurrentOwners().FirstOrDefault();
            }
            BazaarMenu.ShowFor(Wheels.MARNIE, marnieOwnerData);
            return false;
        }
        return true;
    }

    private static void AnimalHouse_adoptAnimal_Postfix(AnimalHouse __instance, FarmAnimal animal)
    {
        string animalType = animal.type.Value;
        foreach (Farmer allFarmer in Game1.getAllFarmers())
        {
            allFarmer.autoGenerateActiveDialogueEvent($"purchasedAnimal_{animalType}");
        }
        string modCustom = $"{ModEntry.ModId}_purchasedAnimal_{animalType}";
        Game1.addMail(modCustom, noLetter: true, sendToEveryone: true);
        TriggerActionManager.Raise(PurchasedAnimal_Trigger, [__instance, animal]);
    }
}
