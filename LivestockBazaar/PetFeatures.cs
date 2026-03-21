using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Buildings;
using StardewValley.Characters;
using StardewValley.Delegates;
using StardewValley.Extensions;
using StardewValley.GameData.FarmAnimals;
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
    internal const string GSQ_HAS_PETBOWL = $"{ModEntry.ModId}_HAVE_PETBOWL";
    internal const string GSQ_HAS_HOUSING = $"{ModEntry.ModId}_HAVE_HOUSING";

    internal const string Action_AdoptPet = $"{ModEntry.ModId}_AdoptPet";
    internal const string Action_AdoptFarmAnimal = $"{ModEntry.ModId}_AdoptFarmAnimal";

    internal const string Action_AddWild = $"{ModEntry.ModId}_AddWild";
    internal const string Action_RemoveWild = $"{ModEntry.ModId}_RemoveWild";
    internal const string ModData_Wild = $"{ModEntry.ModId}/Wild";
    internal const string ModData_WildInteract = $"{ModEntry.ModId}/WildInteract";
    internal const string WildInteract_Trigger = $"{ModEntry.ModId}_WildInteract";
    internal const string WildEvent_WildPos = "LB_WildPos";
    internal const string WildEvent_WildActorName = "LB_Wild";
    internal const string WildEvent_AddTargetWildActor = "LB_AddTargetWildActor";
    internal const string WildEvent_AddWildActor = "LB_AddWildActor";
    internal const string WildEvent_AdoptWild = "LB_AdoptWild";

    internal static Pet? WildEventTarget = null;

    internal static MethodInfo? namePet_Method = AccessTools.DeclaredMethod(typeof(PetLicense), "namePet");

    internal static void Register(Harmony patcher, IModHelper helper)
    {
        // adoptions
        ItemQueryResolver.Register(ItemQuery_PET_ADOPTION, PET_ADOPTION);
        if (namePet_Method == null)
        {
            ModEntry.Log($"Failed to reflect PetLicense.namePet, pet adoption features unavailable", LogLevel.Error);
            return;
        }

        GameStateQuery.Register(GSQ_HAS_PETBOWL, HAS_PETBOWL);
        GameStateQuery.Register(GSQ_HAS_HOUSING, HAS_HOUSING);

        TriggerActionManager.RegisterAction(Action_AdoptPet, DoAdoptPet);
        TriggerActionManager.RegisterAction(Action_AdoptFarmAnimal, DoAdoptFarmAnimal);

        // wild pet event
        TriggerActionManager.RegisterAction(Action_AddWild, DoAddWild);
        TriggerActionManager.RegisterAction(Action_RemoveWild, DoRemoveWild);

        TokenParser.RegisterParser(WildEvent_WildPos, TS_WildPos);
        Event.RegisterCommand(WildEvent_AddTargetWildActor, Event_AddTargetWildActor);
        Event.RegisterCommand(WildEvent_AddWildActor, Event_AddWildActor);
        Event.RegisterCommand(WildEvent_AdoptWild, Event_AdoptWild);

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
        TriggerActionManager.RegisterTrigger(WildInteract_Trigger);

        helper.Events.GameLoop.Saving += OnSavingClearWilds;
    }

    private static void Event_AddWildActor(Event @event, string[] args, EventContext context)
    {
        if (
            !ArgUtility.TryGet(args, 1, out string petId, out string error, allowBlank: false, name: "string petId")
            || !ArgUtility.TryGet(args, 2, out string breedId, out error, allowBlank: false, name: "string breedId")
            || !ArgUtility.TryGetPoint(args, 3, out Point tilePoint, out _, "Point tile")
            || !ArgUtility.TryGetDirection(args, 5, out int direction, out _, "int facingDirection")
            || !ArgUtility.TryGetOptional(
                args,
                6,
                out string? portraitAsset,
                out _,
                defaultValue: null,
                name: "string portraitAsset"
            )
        )
        {
            @event.LogCommandErrorAndSkip(args, error);
            return;
        }
        Pet templatePet = new(tilePoint.X, tilePoint.Y, breedId, petId);
        templatePet.reloadSprite(true);
        MakePetActor(
            @event,
            tilePoint,
            direction,
            templatePet,
            portraitAsset,
            string.Concat(WildEvent_WildActorName, '_', petId, '_', breedId)
        );
    }

    private static void Event_AddTargetWildActor(Event @event, string[] args, EventContext context)
    {
        if (WildEventTarget == null)
        {
            @event.LogCommandErrorAndSkip(
                args,
                "'WildEventTarget' not set, use 'LB_AddWildActor' for generic pet actor'"
            );
            return;
        }
        if (!ArgUtility.TryGetPoint(args, 1, out Point tilePoint, out _, "Point tile"))
        {
            tilePoint = WildEventTarget.TilePoint;
        }
        if (!ArgUtility.TryGetDirection(args, 3, out int direction, out _, "int facingDirection"))
        {
            direction = WildEventTarget.FacingDirection;
        }
        ArgUtility.TryGetOptional(
            args,
            4,
            out string? portraitAsset,
            out _,
            defaultValue: null,
            name: "string portraitAsset"
        );
        MakePetActor(@event, tilePoint, direction, WildEventTarget, portraitAsset, WildEvent_WildActorName);
    }

    private static void MakePetActor(
        Event @event,
        Point tilePoint,
        int direction,
        Pet templatePet,
        string portraitAsset,
        string name
    )
    {
        AnimatedSprite petSprite = new(
            Game1.temporaryContent,
            templatePet.Sprite.textureName.Value,
            0,
            templatePet.Sprite.SpriteWidth,
            templatePet.Sprite.SpriteHeight
        );
        NPC petActor = new(petSprite, @event.OffsetPosition(new(tilePoint.X * 64f, tilePoint.Y * 64f)), direction, name)
        {
            portraitOverridden = true,
            spriteOverridden = true,
            Breather = false,
            HideShadow = true,
        };
        if (!string.IsNullOrEmpty(portraitAsset) && Game1.temporaryContent.DoesAssetExist<Texture2D>(portraitAsset))
        {
            petActor.Portrait = Game1.temporaryContent.Load<Texture2D>(portraitAsset);
        }
        else
        {
            petActor.Portrait = petSprite.Texture;
        }
        petActor.modData.CopyFrom(templatePet.modData);
        petActor.forceOneTileWide.Value = true;
        @event.actors.Add(petActor);
        @event.CurrentCommand++;
    }

    private static void Event_AdoptWild(Event @event, string[] args, EventContext context)
    {
        if (Game1.activeClickableMenu is NamingMenu)
        {
            return;
        }
        if (!ArgUtility.TryGetOptional(args, 1, out string petName, out string error, name: "string petName"))
        {
            @event.LogCommandErrorAndSkip(args, error);
            return;
        }
        if (WildEventTarget != null)
        {
            context.Location.characters.Remove(WildEventTarget);
            PetLicense license = new()
            {
                Name = string.Concat(WildEventTarget.petType.Value, "|", WildEventTarget.whichBreed.Value),
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
            WildEventTarget = null;
        }
        else
        {
            @event.LogCommandErrorAndSkip(args, "Failed to find wild pet actor");
        }
    }

    private static bool TS_WildPos(string[] query, out string replacement, Random random, Farmer player)
    {
        if (WildEventTarget == null)
        {
            ModEntry.Log("'WildEventTarget' not set", LogLevel.Error);
            replacement = "0 0";
            return false;
        }
        Point WildEventPos = WildEventTarget.TilePoint;
        if (ArgUtility.TryGetPoint(query, 1, out Point offset, out _, name: "Point offset"))
        {
            replacement = $"{WildEventPos.X + offset.X} {WildEventPos.Y + offset.Y}";
            return true;
        }
        replacement = $"{WildEventPos.X} {WildEventPos.Y}";
        return true;
    }

    private static void OnSavingClearWilds(object? sender, SavingEventArgs e)
    {
        WildEventTarget = null;
        Utility.ForEachLocation(
            (location) =>
            {
                location.characters.RemoveWhere(chara =>
                    chara is Pet petChara && petChara.modData.ContainsKey(ModData_Wild)
                );
                return true;
            }
        );
    }

    internal static bool Pet_checkAction_Prefix(Pet __instance, Farmer who, GameLocation l)
    {
        if (!__instance.modData.ContainsKey(ModData_Wild))
        {
            return true;
        }

        if (!__instance.modData.TryGetValue(ModData_WildInteract, out string triggerOrEvent))
        {
            triggerOrEvent = "TRIGGER";
        }

        if (triggerOrEvent.EqualsIgnoreCase("TRIGGER"))
        {
            TriggerActionManager.Raise(WildInteract_Trigger);
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
        WildEventTarget = __instance;
        Event WildEvent = new(eventScript, parts[0], parts[1], who)
        {
            eventPositionTileOffset = WildEventTarget.TilePoint.ToVector2(),
        };
        l.startEvent(WildEvent);

        return false;
    }

    private static bool DoAddWild(string[] args, TriggerActionContext context, out string? error)
    {
        if (!Context.IsMainPlayer)
        {
            error = $"Only the main player can use '{Action_AddWild}'";
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

        if (!ValidatePetIds(ref petId, ref breedId, out _, out error))
        {
            return false;
        }

        if (!TryGetLocationFromName(locationName, ref error, out GameLocation location))
        {
            return false;
        }

        string WildKey = FormWildKey(pnt, petId, breedId);
        foreach (Character chara in location.characters)
        {
            if (
                chara is Pet prevPet
                && prevPet.modData.TryGetValue(ModData_Wild, out string prevWildKey)
                && prevWildKey == WildKey
            )
            {
                // already spawned, simply update ModData_WildInteract
                prevPet.modData[ModData_WildInteract] = triggerOrEvent;
                return true;
            }
        }

        Pet pet = new(pnt.X, pnt.Y, breedId, petId);
        pet.hideFromAnimalSocialMenu.Value = true;
        pet.modData[ModData_Wild] = WildKey;
        pet.modData[ModData_WildInteract] = triggerOrEvent;
        location.characters.Add(pet);
        pet.update(Game1.currentGameTime, location);
        pet.CurrentBehavior = "SitDown";
        pet.OnNewBehavior();
        ModEntry.Log($"Add wild pet '{WildKey}' to {locationName}{pnt}");

        return true;
    }

    private static bool DoRemoveWild(string[] args, TriggerActionContext context, out string? error)
    {
        if (!Context.IsMainPlayer)
        {
            error = $"Only the main player can use '{Action_RemoveWild}'";
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
                chara is Pet petChara && petChara.modData.ContainsKey(ModData_Wild)
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

        string WildKey = FormWildKey(pnt, petId, breedId);
        RemoveWildByKey(location, WildKey);
        return true;
    }

    private static void RemoveWildByKey(GameLocation location, string WildKey)
    {
        location.characters.RemoveWhere(chara =>
            chara is Pet petChara
            && petChara.modData.TryGetValue(ModData_Wild, out string prevWildKey)
            && prevWildKey == WildKey
        );
    }

    private static bool TryGetLocationFromName(string locationName, ref string? error, out GameLocation location)
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

    private static string FormWildKey(Point pnt, string petId, string breedId)
    {
        return $"{pnt.X},{pnt.Y}:{petId}_{breedId}";
    }

    private static bool HAS_HOUSING(string[] query, GameStateQueryContext context)
    {
        if (
            !ArgUtility.TryGet(
                query,
                1,
                out string? farmAnimalId,
                out string error,
                allowBlank: false,
                name: "string farmAnimalId"
            )
        )
        {
            ModEntry.Log(error, LogLevel.Error);
            return false;
        }

        FarmAnimal animal = new(farmAnimalId, Game1.Multiplayer.getNewID(), -1L);
        bool result = false;
        Utility.ForEachBuilding(building =>
        {
            if (animal.CanLiveIn(building))
            {
                result = true;
                return false;
            }
            return true;
        });
        return result;
    }

    private static bool HAS_PETBOWL(string[] query, GameStateQueryContext context)
    {
        foreach (Building building in Game1.getFarm().buildings)
        {
            if (building is PetBowl petBowl && !petBowl.HasPet())
            {
                return true;
            }
        }
        return false;
    }

    private static void AdoptFarmAnimalToBuilding(Building building, FarmAnimal animal, string farmAnimalName)
    {
        ModEntry.Log($"Adopted {animal.type.Value}({animal.skinID.Value}) with name '{farmAnimalName}'");
        animal.displayName = farmAnimalName;
        (building.GetIndoors() as AnimalHouse)?.adoptAnimal(animal);
    }

    private static bool DoAdoptFarmAnimal(string[] args, TriggerActionContext context, out string error)
    {
        if (
            !ArgUtility.TryGet(
                args,
                1,
                out string? farmAnimalId,
                out error,
                allowBlank: false,
                name: "string farmAnimalId"
            )
            || !ArgUtility.TryGet(args, 2, out string? skinId, out error, allowBlank: false, name: "string skinId")
            || !ArgUtility.TryGet(
                args,
                3,
                out string? farmAnimalName,
                out error,
                allowBlank: false,
                name: "string petName"
            )
            || !ArgUtility.TryGetOptionalBool(args, 4, out bool showNamingMenu, out error, name: "bool showNamingMenu")
        )
        {
            return false;
        }

        if (!Game1.farmAnimalData.TryGetValue(farmAnimalId, out FarmAnimalData? farmAnimalDataSpecific))
        {
            error = $"No farm animal with id '{farmAnimalId}'";
            return false;
        }

        FarmAnimal animal = new(farmAnimalId, Game1.Multiplayer.getNewID(), Game1.player.UniqueMultiplayerID);
        if (!skinId.EqualsIgnoreCase("RANDOM"))
        {
            animal.skinID.Value = skinId;
        }

        Utility.ForEachBuilding(building =>
        {
            if (animal.CanLiveIn(building))
            {
                if (showNamingMenu)
                {
                    string title = Game1.content.LoadString("Strings\\StringsFromCSFiles:Event.cs.1236");
                    Game1.activeClickableMenu = new NamingMenu(
                        (s) => AdoptFarmAnimalToBuilding(building, animal, s),
                        title,
                        farmAnimalName
                    );
                }
                else
                {
                    AdoptFarmAnimalToBuilding(building, animal, farmAnimalName);
                }
                return false;
            }
            return true;
        });
        ModEntry.Log($"Did not find building for {animal.type.Value}({animal.skinID.Value}) to live in", LogLevel.Info);
        return true;
    }

    private static bool ValidatePetIds(
        ref string petId,
        ref string breedId,
        out PetData? petDataSpecific,
        [NotNullWhen(false)] out string? error
    )
    {
        if (!Game1.petData.TryGetValue(petId, out petDataSpecific))
        {
            error = $"No pet with id '{petId}'";
            return false;
        }

        if (breedId.EqualsIgnoreCase("RANDOM"))
        {
            breedId = Random.Shared.ChooseFrom(petDataSpecific.Breeds.Select(breed => breed.Id).ToList());
        }

        if (petDataSpecific.GetBreedById(breedId, allowNull: true) is null)
        {
            error = $"No pet with id '{breedId}'";
            return false;
        }

        error = null;
        return true;
    }

    private static bool DoAdoptPet(string[] args, TriggerActionContext context, out string? error)
    {
        if (
            !ArgUtility.TryGet(args, 1, out string? petId, out error, allowBlank: false, name: "string petId")
            || !ArgUtility.TryGet(args, 2, out string? breedId, out error, allowBlank: false, name: "string breedId")
            || !ArgUtility.TryGet(args, 3, out string? petName, out error, allowBlank: false, name: "string petName")
            || !ArgUtility.TryGetOptionalBool(args, 4, out bool showNamingMenu, out error, name: "bool showNamingMenu")
        )
        {
            return false;
        }

        if (ValidatePetIds(ref petId, ref breedId, out PetData? petDataSpecific, out error))
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
        Game1.dialogueUp = false;
        Game1.player.CanMove = true;
    }

    internal static IEnumerable<ItemQueryResult> PET_ADOPTION(
        string key,
        string arguments,
        ItemQueryContext context,
        bool avoidRepeat,
        HashSet<string>? avoidItemIds,
        Action<string, string> logError
    )
    {
        string[] args = ArgUtility.SplitBySpaceQuoteAware(arguments);
        if (
            !ArgUtility.TryGetOptional(
                args,
                0,
                out string petId,
                out string? error,
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
