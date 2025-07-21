using System.Reflection;
using HarmonyLib;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Delegates;
using StardewValley.GameData.Pets;
using StardewValley.Internal;
using StardewValley.Objects;
using StardewValley.Triggers;

namespace LivestockBazaar;

internal static class PetFeatures
{
    internal const string ItemQuery_PET_ADOPTION = $"{ModEntry.ModId}_PET_ADOPTION";
    internal const string Action_AdoptPet = $"{ModEntry.ModId}_AdoptPet";
    internal static MethodInfo? namePet_Method = AccessTools.DeclaredMethod(typeof(PetLicense), "namePet");

    internal static void Register()
    {
        ItemQueryResolver.Register(ItemQuery_PET_ADOPTION, PET_ADOPTION);
        if (namePet_Method != null)
            TriggerActionManager.RegisterAction(Action_AdoptPet, DoAdoptPet);
    }

    private static bool DoAdoptPet(string[] args, TriggerActionContext context, out string error)
    {
        if (
            !ArgUtility.TryGet(args, 1, out string petId, out error, allowBlank: false, name: "string petId")
            || !ArgUtility.TryGet(args, 2, out string breedId, out error, allowBlank: false, name: "string breedId")
            || !ArgUtility.TryGet(args, 3, out string petName, out error, allowBlank: false, name: "string petName")
        )
        {
            return false;
        }

        if (
            Game1.petData.TryGetValue(petId, out PetData? petDataSpecific)
            && petDataSpecific.GetBreedById(breedId, allowNull: true) != null
        )
        {
            PetLicense license = new() { Name = string.Concat(petId, "|", breedId) };
            namePet_Method?.Invoke(license, [petName]);
            return true;
        }

        ModEntry.Log($"Invalid pet '{petId}|{breedId}'");
        return false;
    }

    internal static IEnumerable<ItemQueryResult> PET_ADOPTION(
        string key,
        string arguments,
        ItemQueryContext context,
        bool avoidRepeat,
        HashSet<string> avoidItemIds,
        Action<string, string> logError
    )
    {
        string[] args = ArgUtility.SplitBySpaceQuoteAware(arguments);
        if (
            !ArgUtility.TryGetOptional(
                args,
                0,
                out string petId,
                out string error,
                defaultValue: "T",
                allowBlank: false,
                name: "string petId"
            )
            || !ArgUtility.TryGetOptional(
                args,
                1,
                out string breedId,
                out error,
                defaultValue: "T",
                allowBlank: false,
                name: "string breedId"
            )
            || !ArgUtility.TryGetOptionalBool(
                args,
                2,
                out bool ignoreBasePrice,
                out error,
                defaultValue: false,
                name: "bool ignoreBasePrice"
            )
            || !ArgUtility.TryGetOptionalBool(
                args,
                3,
                out bool ignoreCanBeAdoptedFromMarnie,
                out error,
                defaultValue: false,
                name: "bool ignoreCanBeAdoptedFromMarnie"
            )
        )
        {
            ModEntry.Log(error, LogLevel.Error);
            return [];
        }
        IEnumerable<KeyValuePair<string, PetData>> searchPets;
        if (petId == "T")
        {
            searchPets = Game1.petData;
        }
        else
        {
            if (Game1.petData.TryGetValue(petId, out PetData? petDataSpecific))
            {
                searchPets = [new(petId, petDataSpecific)];
            }
            else
            {
                ModEntry.Log($"Invalid pet type '{petId}'", LogLevel.Error);
                return [];
            }
        }

        List<ItemQueryResult> list = [];
        foreach (KeyValuePair<string, PetData> petDatum in searchPets)
        {
            foreach (PetBreed breed in petDatum.Value.Breeds)
            {
                if (!breed.CanBeAdoptedFromMarnie && !ignoreCanBeAdoptedFromMarnie)
                {
                    continue;
                }
                if (breedId != "T" && breedId != breed.Id)
                {
                    continue;
                }

                ItemQueryResult result = new(new PetLicense() { Name = string.Concat(petDatum.Key, "|", breed.Id) });
                if (!ignoreBasePrice)
                {
                    result.OverrideBasePrice = breed.AdoptionPrice;
                }
                list.Add(result);
            }
        }
        return list;
    }
}
