using System.Reflection;
using HarmonyLib;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Characters;
using StardewValley.Delegates;
using StardewValley.Extensions;
using StardewValley.GameData.Pets;
using StardewValley.Internal;
using StardewValley.Menus;
using StardewValley.Objects;
using StardewValley.TokenizableStrings;
using StardewValley.Triggers;

namespace LivestockBazaar;

internal static class PetFeatures
{
    internal const string ItemQuery_PET_ADOPTION = $"{ModEntry.ModId}_PET_ADOPTION";
    internal const string Action_AdoptPet = $"{ModEntry.ModId}_AdoptPet";
    internal const string Action_AddWildPet = $"{ModEntry.ModId}_AddWildPet";
    internal const string Action_RemoveWildPet = $"{ModEntry.ModId}_RemoveWildPet";
    internal const string ModData_WildPet = $"{ModEntry.ModId}/WildPet";
    internal const string ModData_WildPetInteract = $"{ModEntry.ModId}/WildPetInteract";
    internal const string WildPetInteract_Trigger = $"{ModEntry.ModId}_WildPetInteract";
    internal const string WildPetEvent_WildPetPos = "LB_WildPetPos";
    internal const string WildPetEvent_WildPetActorName = "LB_WildPet";
    internal const string WildPetEvent_WildPetActor = "LB_WildPetActor";
    internal const string WildPetEvent_AdoptWildPet = "LB_AdoptWildPet";

    internal static MethodInfo? namePet_Method = AccessTools.DeclaredMethod(typeof(PetLicense), "namePet");
    internal static Pet? wildPetEventTarget = null;

    internal static void Register(Harmony patcher, IModHelper helper)
    {
        ItemQueryResolver.Register(ItemQuery_PET_ADOPTION, PET_ADOPTION);
        if (namePet_Method == null)
        {
            ModEntry.Log($"Failed to reflect PetLicense.namePet, '{Action_AdoptPet}' unavailable", LogLevel.Error);
        }
        else
        {
            TriggerActionManager.RegisterAction(Action_AdoptPet, DoAdoptPet);
        }
        TriggerActionManager.RegisterAction(Action_AddWildPet, DoAddWildPet);
        TriggerActionManager.RegisterAction(Action_RemoveWildPet, DoRemoveWildPet);

        TokenParser.RegisterParser(WildPetEvent_WildPetPos, TS_WildPetPos);
        Event.RegisterCommand(WildPetEvent_WildPetActor, Event_WildPetActor);
        Event.RegisterCommand(WildPetEvent_AdoptWildPet, Event_AdoptWildPet);

        try
        {
            patcher.Patch(
                original: AccessTools.DeclaredMethod(typeof(Pet), nameof(Pet.checkAction)),
                prefix: new HarmonyMethod(typeof(PetFeatures), nameof(Pet_checkAction_Prefix))
            );
        }
        catch (Exception err)
        {
            ModEntry.Log($"Failed to patch LivestockBazaar(PetAction):\n{err}", LogLevel.Error);
        }
        TriggerActionManager.RegisterTrigger(WildPetInteract_Trigger);

        helper.Events.GameLoop.Saving += OnSavingClearWildPets;
    }

    private static void Event_WildPetActor(Event @event, string[] args, EventContext context)
    {
        if (wildPetEventTarget == null)
        {
            @event.LogCommandError(args, "'wildPetEventTarget' not set");
            return;
        }
        if (!ArgUtility.TryGetPoint(args, 4, out Point tilePoint, out _, "Point tile"))
        {
            tilePoint = wildPetEventTarget.TilePoint;
        }
        if (!ArgUtility.TryGetDirection(args, 6, out int direction, out _, "int facingDirection"))
        {
            direction = wildPetEventTarget.FacingDirection;
        }
        Pet petActor = new(
            tilePoint.X,
            tilePoint.Y,
            wildPetEventTarget.whichBreed.Value,
            wildPetEventTarget.petType.Value
        )
        {
            Name = WildPetEvent_WildPetActorName,
            EventActor = true,
            FacingDirection = direction,
        };
        petActor.modData.CopyFrom(wildPetEventTarget.modData);
        petActor.forceOneTileWide.Value = true;
        @event.actors.Add(petActor);
        @event.CurrentCommand++;
    }

    private static void Event_AdoptWildPet(Event @event, string[] args, EventContext context)
    {
        if (Game1.activeClickableMenu is NamingMenu)
        {
            return;
        }
        if (!ArgUtility.TryGetOptional(args, 1, out string petName, out string error, name: "string petName"))
        {
            @event.LogCommandError(args, error);
            return;
        }
        if (
            @event.actors.Find(chara => chara is Pet && chara.Name == WildPetEvent_WildPetActorName) is Pet wildPetActor
        )
        {
            context.Location.characters.Remove(wildPetEventTarget);
            PetLicense license = new()
            {
                Name = string.Concat(wildPetActor.petType.Value, "|", wildPetActor.whichBreed.Value),
            };
            string title = Game1.content.LoadString("Strings\\StringsFromCSFiles:Event.cs.1236");
            Game1.activeClickableMenu = new NamingMenu(
                (s) =>
                {
                    DoNamePet(s, license);
                    @event.CurrentCommand++;
                },
                title,
                petName
            );
        }
        else
        {
            @event.LogCommandErrorAndSkip(args, "Failed to find wild pet actor");
        }
    }

    private static bool TS_WildPetPos(string[] query, out string replacement, Random random, Farmer player)
    {
        if (wildPetEventTarget == null)
        {
            ModEntry.Log("'wildPetEventTarget' not set", LogLevel.Error);
            replacement = "0 0";
            return false;
        }
        Point wildPetEventPos = wildPetEventTarget.TilePoint;
        if (ArgUtility.TryGetPoint(query, 1, out Point offset, out _, name: "Point offset"))
        {
            replacement = $"{wildPetEventPos.X + offset.X} {wildPetEventPos.Y + offset.Y}";
            return true;
        }
        replacement = $"{wildPetEventPos.X} {wildPetEventPos.Y}";
        return true;
    }

    private static void OnSavingClearWildPets(object? sender, SavingEventArgs e)
    {
        wildPetEventTarget = null;
        Utility.ForEachLocation(
            (location) =>
            {
                location.characters.RemoveWhere(chara =>
                    chara is Pet petChara && petChara.modData.ContainsKey(ModData_WildPet)
                );
                return true;
            }
        );
    }

    internal static bool Pet_checkAction_Prefix(Pet __instance, Farmer who, GameLocation l)
    {
        if (!__instance.modData.ContainsKey(ModData_WildPet))
        {
            return true;
        }
        if (!__instance.modData.TryGetValue(ModData_WildPetInteract, out string triggerOrEvent))
        {
            triggerOrEvent = "TRIGGER";
        }

        if (triggerOrEvent.EqualsIgnoreCase("TRIGGER"))
        {
            TriggerActionManager.Raise(WildPetInteract_Trigger);
            return false;
        }

        string[] parts = triggerOrEvent.Split(':');
        if (parts.Length < 2)
        {
            ModEntry.Log($"Event script key '{triggerOrEvent}' not in 'asset:key' form");
            return false;
        }

        if (Game1.content.LoadStringReturnNullIfNotFound(triggerOrEvent) is not string eventScript)
        {
            ModEntry.Log($"Failed to load wild pet event script from '{triggerOrEvent}'", LogLevel.Error);
            return false;
        }

        __instance.Halt();
        wildPetEventTarget = __instance;
        Event wildPetEvent = new(eventScript, parts[0], parts[1], who);
        l.startEvent(wildPetEvent);

        return false;
    }

    private static bool DoAddWildPet(string[] args, TriggerActionContext context, out string error)
    {
        if (!Context.IsMainPlayer)
        {
            error = $"Only the main player can use '{Action_AddWildPet}'";
            return false;
        }
        if (
            !ArgUtility.TryGet(args, 1, out string locationName, out error, allowBlank: false, name: "string location")
            || !ArgUtility.TryGetPoint(args, 2, out Point pnt, out error, name: "Point pnt")
            || !ArgUtility.TryGet(args, 4, out string petId, out error, allowBlank: false, name: "string petId")
            || !ArgUtility.TryGet(args, 5, out string breedId, out error, allowBlank: false, name: "string breedId")
            || !ArgUtility.TryGetOptional(
                args,
                6,
                out string triggerOrEvent,
                out error,
                defaultValue: "TRIGGER",
                allowBlank: false,
                name: "string triggerOrEvent"
            )
        )
        {
            return false;
        }

        if (!TryGetLocationFromName(locationName, ref error, out GameLocation location))
        {
            return false;
        }

        string wildPetKey = FormWildPetKey(pnt, petId, breedId);
        foreach (Character chara in location.characters)
        {
            if (
                chara is Pet prevPet
                && prevPet.modData.TryGetValue(ModData_WildPet, out string prevWildPetKey)
                && prevWildPetKey == wildPetKey
            )
            {
                // already spawned, simply update ModData_WildPetInteract
                prevPet.modData[ModData_WildPetInteract] = triggerOrEvent;
                return true;
            }
        }

        Pet pet = new(pnt.X, pnt.Y, breedId, petId);
        pet.hideFromAnimalSocialMenu.Value = true;
        pet.modData[ModData_WildPet] = wildPetKey;
        pet.modData[ModData_WildPetInteract] = triggerOrEvent;
        location.characters.Add(pet);
        pet.update(Game1.currentGameTime, location);
        pet.CurrentBehavior = "SitDown";
        pet.OnNewBehavior();
        ModEntry.Log($"Add wild pet {petId}/{breedId} to {locationName}{pnt}");

        return true;
    }

    private static bool DoRemoveWildPet(string[] args, TriggerActionContext context, out string error)
    {
        if (!Context.IsMainPlayer)
        {
            error = $"Only the main player can use '{Action_RemoveWildPet}'";
            return false;
        }
        if (!ArgUtility.TryGet(args, 1, out string locationName, out error, allowBlank: false, name: "string location"))
        {
            return false;
        }

        if (!TryGetLocationFromName(locationName, ref error, out GameLocation location))
        {
            return false;
        }

        if (!ArgUtility.TryGetPoint(args, 2, out Point pnt, out error, name: "Point pnt"))
        {
            location.characters.RemoveWhere(chara =>
                chara is Pet petChara && petChara.modData.ContainsKey(ModData_WildPet)
            );
            return true;
        }

        if (
            !ArgUtility.TryGetOptional(args, 4, out string petId, out error, allowBlank: false, name: "string petId")
            || !ArgUtility.TryGetOptional(
                args,
                5,
                out string breedId,
                out error,
                allowBlank: false,
                name: "string breedId"
            )
        )
        {
            return false;
        }

        string wildPetKey = FormWildPetKey(pnt, petId, breedId);
        RemoveWildPetByKey(location, wildPetKey);
        return true;
    }

    private static void RemoveWildPetByKey(GameLocation location, string wildPetKey)
    {
        location.characters.RemoveWhere(chara =>
            chara is Pet petChara
            && petChara.modData.TryGetValue(ModData_WildPet, out string prevWildPetKey)
            && prevWildPetKey == wildPetKey
        );
    }

    private static bool TryGetLocationFromName(string locationName, ref string error, out GameLocation location)
    {
        if (locationName.EqualsIgnoreCase("Here"))
            location = Game1.currentLocation;
        else
            location = Game1.getLocationFromName(locationName);
        if (location == null)
        {
            error = $"Location '{locationName}' is null";
            return false;
        }
        return true;
    }

    private static string FormWildPetKey(Point pnt, string petId, string breedId)
    {
        return $"{pnt.X},{pnt.Y}:{petId}/{breedId}";
    }

    private static bool DoAdoptPet(string[] args, TriggerActionContext context, out string error)
    {
        if (
            !ArgUtility.TryGet(args, 1, out string petId, out error, allowBlank: false, name: "string petId")
            || !ArgUtility.TryGet(args, 2, out string breedId, out error, allowBlank: false, name: "string breedId")
            || !ArgUtility.TryGet(args, 3, out string petName, out error, allowBlank: false, name: "string petName")
            || !ArgUtility.TryGetOptionalBool(args, 4, out bool showNamingMenu, out error, name: "bool showNamingMenu")
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

            if (showNamingMenu)
            {
                string title = Game1.content.LoadString("Strings\\StringsFromCSFiles:Event.cs.1236");
                Game1.activeClickableMenu = new NamingMenu((s) => DoNamePet(s, license), title, petName);
            }
            else
            {
                DoNamePet(petName, license);
            }
            return true;
        }

        ModEntry.Log($"Invalid pet '{petId}|{breedId}'");
        return false;
    }

    private static void DoNamePet(string petName, PetLicense license)
    {
        namePet_Method?.Invoke(license, [petName]);
        Game1.exitActiveMenu();
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
