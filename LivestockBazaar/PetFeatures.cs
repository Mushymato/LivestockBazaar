using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Runtime.CompilerServices;
using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
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

#region yoinked from TrinketTinker
public sealed class ChatterLinesData
{
    /// <summary>Game state query condition</summary>
    public string? Condition { get; set; } = null;

    /// <summary>Precedence of this chatter line, lower is earlier</summary>
    public int Precedence { get; set; } = 0;

    /// <summary>Setting priority means setting a negative precedence</summary>
    public int Priority
    {
        set => Precedence = -value;
    }

    /// <summary>Ordered dialogue lines, one will be picked at random. Supports translation keys.</summary>
    public List<string>? Lines { get; set; } = null;

    /// <summary>Response dialogue lines, used for $q and other cross dialogue key things. Supports translation keys.</summary>
    public Dictionary<string, string>? Responses { get; set; } = null;

    public static implicit operator ChatterLinesData(string value) => new() { Lines = [value] };
}

public sealed class AnimalTalkCtx(string dialogueAsset, string? portraitAsset)
{
    private const string QQQ = "???";
    private Dialogue? chosenDialogue = null;
    private readonly FieldInfo dialogueField = typeof(NPC).GetField(
        "dialogue",
        BindingFlags.NonPublic | BindingFlags.Instance
    )!;
    private readonly FieldInfo portraitField = typeof(NPC).GetField(
        "portrait",
        BindingFlags.NonPublic | BindingFlags.Instance
    )!;
    private readonly NPC speakerNPC = new(null, Vector2.Zero, "", 0, QQQ, null, eventActor: false) { Name = QQQ };
    private Dictionary<string, ChatterLinesData> Chatter =>
        Game1.content.Load<Dictionary<string, ChatterLinesData>>(dialogueAsset);
    private Texture2D? Portrait =>
        portraitAsset != null && Game1.content.DoesAssetExist<Texture2D>(portraitAsset)
            ? Game1.content.Load<Texture2D>(portraitAsset)
            : null;
    private readonly HashSet<string> seenToday = [];

    public bool TryShowDialogue(string speakerName, Farmer who, GameLocation l)
    {
        if (Game1.activeClickableMenu != null)
        {
            return false;
        }
        if (!chosenDialogue?.isDialogueFinished() ?? false)
        {
            Game1.DrawDialogue(chosenDialogue);
            return true;
        }
        chosenDialogue = null;
        ChatterLinesData? foundLines;
        GameStateQueryContext ctx = new(l, who, null, null, Random.Shared);
        // choose chatter data, either from next chatter key proc'd by ability or by conds
        if (
            Chatter
                .Where(
                    (kv) =>
                        GameStateQuery.CheckConditions(kv.Value.Condition, ctx)
                        && kv.Value.Lines != null
                        && kv.Value.Lines.Except(seenToday).Any()
                )
                .ToList()
                is List<KeyValuePair<string, ChatterLinesData>> foundLinesKV
            && foundLinesKV.Count > 0
        )
        {
            int minPrecedence = foundLinesKV.Min(kv => kv.Value?.Precedence ?? 0);
            foundLines = Random
                .Shared.ChooseFrom(foundLinesKV.Where(kv => (kv.Value?.Precedence ?? 0) == minPrecedence).ToList())
                .Value;
            if (foundLines?.Lines == null)
            {
                return false;
            }
        }
        else
        {
            return false;
        }
        IEnumerable<string> linePool = foundLines.Lines.Except(seenToday);
        if (!linePool.Any())
        {
            // ran out of not yet seen
            return false;
        }

        string? chosen = ctx.Random.ChooseFrom(linePool.ToList());
        if (chosen == null)
        {
            return false;
        }
        seenToday.Add(chosen);

        // draw the dialogue
        portraitField.SetValue(speakerNPC, Portrait);
        speakerNPC.displayName = speakerName;

        if (foundLines.Responses != null)
        {
            Dictionary<string, string> TranslatedResponses = foundLines
                .Responses.Select((kv) => new KeyValuePair<string, string>(kv.Key, ParseText(kv.Value, speakerName)))
                .ToDictionary((kv) => kv.Key, (kv) => kv.Value);
            dialogueField.SetValue(speakerNPC, TranslatedResponses);
        }
        chosenDialogue = new(speakerNPC, chosen, ParseText(chosen, speakerName));
        Game1.DrawDialogue(chosenDialogue);
        return true;
    }

    private static string ParseText(string text, string speakerName)
    {
        return (TokenParser.ParseText(text) ?? text).Replace("%lbspeaker", speakerName);
    }

    internal static AnimalTalkCtx? MakeForPet(Pet pet)
    {
        if (pet.GetPetData() is not PetData petData)
            return null;
        if (!petData.CustomFields.TryGetValue(PetFeatures.TalkingAnimals_CustomField, out string? talks))
            return null;
        if (string.IsNullOrEmpty(talks))
            return null;
        talks = string.Format(talks, pet.petType.Value, pet.whichBreed.Value);
        ModEntry.Log(talks);
        string[] args = ArgUtility.SplitBySpaceQuoteAware(talks);
        if (
            !ArgUtility.TryGet(args, 0, out string? dialogueAsset, out string? error)
            || !ArgUtility.TryGetOptional(
                args,
                1,
                out string? portraitAsset,
                out error,
                defaultValue: pet.Sprite.textureName.Value
            )
        )
        {
            ModEntry.Log(error, LogLevel.Error);
            return null;
        }
        if (!Game1.content.DoesAssetExist<Dictionary<string, ChatterLinesData>>(dialogueAsset))
        {
            ModEntry.Log($"Dialogue asset '{dialogueAsset}' does not exist", LogLevel.Error);
            return null;
        }
        if (!Game1.content.DoesAssetExist<Texture2D>(portraitAsset))
        {
            ModEntry.Log($"Portrait asset '{portraitAsset}' does not exist", LogLevel.Debug);
            portraitAsset = null;
        }
        return new AnimalTalkCtx(dialogueAsset, portraitAsset);
    }
}
#endregion

internal static class PetFeatures
{
    internal const string WildAnimal_ManifestKey = $"{ModEntry.ModId}_WildAnimals";
    internal const string TalkingAnimals_ManifestKey = $"{ModEntry.ModId}_TalkingAnimals";

    internal const string TalkingAnimals_CustomField = $"{ModEntry.ModId}/Talk";

    internal const string ItemQuery_PET_ADOPTION = $"{ModEntry.ModId}_PET_ADOPTION";
    internal const string GSQ_HAVE_PETBOWL = $"{ModEntry.ModId}_HAVE_PETBOWL";
    internal const string GSQ_HAVE_HOUSING = $"{ModEntry.ModId}_HAVE_HOUSING";

    internal const string Action_AdoptPet = $"{ModEntry.ModId}_AdoptPet";
    internal const string Action_AdoptFarmAnimal = $"{ModEntry.ModId}_AdoptFarmAnimal";

    internal const string Action_AddWildPet = $"{ModEntry.ModId}_AddWildPet";
    internal const string Action_RemoveWildPet = $"{ModEntry.ModId}_RemoveWildPet";
    internal const string Action_AddWildFarmAnimal = $"{ModEntry.ModId}_AddWildFarmAnimal";
    internal const string Action_RemoveWildFarmAnimal = $"{ModEntry.ModId}_RemoveWildFarmAnimal";

    internal const string ModData_Wild = $"{ModEntry.ModId}/Wild";
    internal const string ModData_WildInteract = $"{ModEntry.ModId}/WildInteract";
    internal const string WildInteract_Trigger = $"{ModEntry.ModId}_WildInteract";
    internal const string WildEvent_WildPos = $"{ModEntry.ModId}_WildPos";
    internal const string WildEvent_WildName = $"{ModEntry.ModId}_WildName";
    internal const string WildEvent_WildActorName = $"{ModEntry.ModId}_Wild";
    internal const string WildEvent_AddTargetWildActor = $"{ModEntry.ModId}_AddTargetWildActor";
    internal const string WildEvent_AdoptWild = $"{ModEntry.ModId}_AdoptWild";

    internal const long FarmAnimalOwnerId = -29997L;

    internal static PerScreen<Character?> WildEventTarget = new();
    internal static readonly ConditionalWeakTable<Pet, AnimalTalkCtx?> PetChatter = [];

    internal static Action<PetLicense, string>? namePet_Method = AccessTools
        .DeclaredMethod(typeof(PetLicense), "namePet")
        ?.CreateDelegate<Action<PetLicense, string>>();

    internal static void Register(Harmony patcher, IModHelper helper)
    {
        // adoptions
        ItemQueryResolver.Register(ItemQuery_PET_ADOPTION, PET_ADOPTION);
        if (namePet_Method == null)
        {
            ModEntry.Log($"Failed to reflect PetLicense.namePet, pet adoption features unavailable", LogLevel.Error);
            return;
        }

        GameStateQuery.Register(GSQ_HAVE_PETBOWL, HAVE_PETBOWL);
        GameStateQuery.Register(GSQ_HAVE_HOUSING, HAVE_HOUSING);

        TriggerActionManager.RegisterAction(Action_AdoptPet, DoAdoptPet);
        TriggerActionManager.RegisterAction(Action_AdoptFarmAnimal, DoAdoptFarmAnimal);

        bool hasWildAnimal = false;
        bool hasTalkingAnimal = false;
        foreach (IModInfo modInfo in helper.ModRegistry.GetAll())
        {
            hasWildAnimal |= modInfo.Manifest.ExtraFields.ContainsKey(WildAnimal_ManifestKey);
            hasTalkingAnimal |= modInfo.Manifest.ExtraFields.ContainsKey(TalkingAnimals_ManifestKey);
            if (hasWildAnimal && hasTalkingAnimal)
                break;
        }

        if (hasWildAnimal || hasTalkingAnimal)
        {
            try
            {
                patcher.Patch(
                    original: AccessTools.DeclaredMethod(typeof(Pet), nameof(Pet.checkAction)),
                    prefix: new HarmonyMethod(typeof(PetFeatures), nameof(Pet_checkAction_Prefix))
                );
                patcher.Patch(
                    original: AccessTools.DeclaredMethod(typeof(FarmAnimal), nameof(FarmAnimal.pet)),
                    prefix: new HarmonyMethod(typeof(PetFeatures), nameof(FarmAnimal_pet_Prefix))
                );
            }
            catch (Exception err)
            {
                ModEntry.Log($"Failed to patch LivestockBazaar(PetAction):\n{err}", LogLevel.Error);
            }
        }

        // these feature require wild animal to be enabled
        if (hasWildAnimal)
        {
            // wild pet event
            TriggerActionManager.RegisterAction(Action_AddWildPet, DoAddWild);
            TriggerActionManager.RegisterAction(Action_RemoveWildPet, DoRemoveWild);
            TriggerActionManager.RegisterAction(Action_AddWildFarmAnimal, DoAddWild);
            TriggerActionManager.RegisterAction(Action_RemoveWildFarmAnimal, DoRemoveWild);

            TokenParser.RegisterParser(WildEvent_WildPos, TS_WildPos);
            TokenParser.RegisterParser(WildEvent_WildName, TS_WildName);
            Event.RegisterCommand(WildEvent_AddTargetWildActor, Event_AddTargetWildActor);
            Event.RegisterCommand(WildEvent_AdoptWild, Event_AdoptWild);

            TriggerActionManager.RegisterTrigger(WildInteract_Trigger);

            helper.Events.GameLoop.Saving += OnSavingClearWilds;
        }
        else
        {
            ModEntry.Log(
                $"No mod has manifest key '{WildAnimal_ManifestKey}', wild animal features are disabled",
                LogLevel.Debug
            );
        }

        if (hasTalkingAnimal)
        {
            helper.Events.GameLoop.ReturnedToTitle += static (sender, e) => PetChatter.Clear();
            helper.Events.GameLoop.Saving += static (sender, e) => PetChatter.Clear();
        }
        else
        {
            ModEntry.Log(
                $"No mod has manifest key '{TalkingAnimals_ManifestKey}', wild animal features are disabled",
                LogLevel.Debug
            );
        }
    }

    private static void Event_AddTargetWildActor(Event @event, string[] args, EventContext context)
    {
        if (WildEventTarget.Value is not Character wildChara)
        {
            @event.LogCommandErrorAndSkip(
                args,
                "'WildEventTarget' not set, use 'LB_AddWildActor' for generic pet actor'"
            );
            return;
        }
        if (!ArgUtility.TryGetPoint(args, 1, out Point tilePoint, out _, "Point tile"))
        {
            tilePoint = wildChara.TilePoint;
        }
        if (!ArgUtility.TryGetDirection(args, 3, out int direction, out _, "int facingDirection"))
        {
            direction = wildChara.FacingDirection;
        }
        ArgUtility.TryGetOptional(
            args,
            4,
            out string? portraitAsset,
            out _,
            defaultValue: null,
            name: "string portraitAsset"
        );
        MakeWildActor(@event, tilePoint, direction, wildChara, portraitAsset, WildEvent_WildActorName);
        @event.CurrentCommand++;
    }

    private static void MakeWildActor(
        Event @event,
        Point tilePoint,
        int direction,
        Character templateChara,
        string? portraitAsset,
        string name
    )
    {
        AnimatedSprite petSprite = new(
            Game1.temporaryContent,
            templateChara.Sprite.textureName.Value,
            0,
            templateChara.Sprite.SpriteWidth,
            templateChara.Sprite.SpriteHeight
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
        petActor.modData.CopyFrom(templateChara.modData);
        petActor.forceOneTileWide.Value = true;
        @event.actors.Add(petActor);
    }

    private static void Event_AdoptWild(Event @event, string[] args, EventContext context)
    {
        if (Game1.activeClickableMenu is NamingMenu)
        {
            return;
        }
        if (
            !ArgUtility.TryGetOptional(
                args,
                1,
                out string wildName,
                out string? error,
                defaultValue: string.Empty,
                name: "string petName"
            )
        )
        {
            @event.LogCommandErrorAndSkip(args, error);
            return;
        }
        if (WildEventTarget.Value is Pet pet)
        {
            PetLicense license = new() { Name = string.Concat(pet.petType.Value, "|", pet.whichBreed.Value) };
            string title = Game1.content.LoadString("Strings\\StringsFromCSFiles:Event.cs.1236");
            Game1.activeClickableMenu = new NamingMenu(
                (s) =>
                {
                    FinishAdoptPet(s, license);
                    @event.CurrentCommand++;
                },
                title,
                wildName
            );
            WildEventTarget.Value = null;
        }
        else if (WildEventTarget.Value is FarmAnimal animal)
        {
            Utility.ForEachBuilding(building =>
            {
                if (animal.CanLiveIn(building))
                {
                    string title = Game1.content.LoadString("Strings\\StringsFromCSFiles:Event.cs.1236");
                    Game1.activeClickableMenu = new NamingMenu(
                        (s) =>
                        {
                            FarmAnimal adoptAnimal = new(
                                animal.type.Value,
                                Game1.Multiplayer.getNewID(),
                                Game1.player.UniqueMultiplayerID
                            );
                            adoptAnimal.skinID.Value = animal.skinID.Value;
                            adoptAnimal.age.Value = animal.age.Value;
                            adoptAnimal.ReloadTextureIfNeeded(true);
                            FinishAdoptFarmAnimal(building, adoptAnimal, s);
                            @event.CurrentCommand++;
                        },
                        title,
                        wildName
                    );
                    return false;
                }
                return true;
            });
            if (Game1.activeClickableMenu == null)
                @event.LogCommandErrorAndSkip(args, $"No available building for {animal.type.Value}");
            WildEventTarget.Value = null;
        }
        else
        {
            @event.LogCommandErrorAndSkip(args, $"Incorrect wild event target type");
        }
    }

    private static bool TS_WildName(string[] query, out string replacement, Random random, Farmer player)
    {
        replacement = "WILD";
        if (WildEventTarget.Value is not Character wildChara)
        {
            ModEntry.Log("'WildEventTarget' not set", LogLevel.Error);
            return false;
        }
        if (wildChara is Pet pet)
        {
            replacement = pet.displayName ?? pet.petType.Value;
            return true;
        }
        else if (wildChara is FarmAnimal animal)
        {
            replacement = animal.displayType ?? animal.type.Value;
            return true;
        }
        return false;
    }

    private static bool TS_WildPos(string[] query, out string replacement, Random random, Farmer player)
    {
        if (WildEventTarget.Value is not Character wildChara)
        {
            ModEntry.Log("'WildEventTarget' not set", LogLevel.Error);
            replacement = "0 0";
            return false;
        }
        Point WildEventPos = wildChara.TilePoint;
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
        WildEventTarget.Value = null;
        Utility.ForEachLocation(
            (location) =>
            {
                location.characters.RemoveWhere(chara =>
                    chara is Pet petChara && petChara.modData.ContainsKey(ModData_Wild)
                );
                location.animals.RemoveWhere(pair => pair.Value.modData.ContainsKey(ModData_Wild));
                return true;
            }
        );
    }

    private static bool FarmAnimal_pet_Prefix(FarmAnimal __instance, Farmer who)
    {
        if (__instance.modData.ContainsKey(ModData_Wild))
        {
            if (!TryInteractTriggerOrEvent(__instance, who, who.currentLocation, out string? error))
            {
                ModEntry.Log(error, LogLevel.Error);
                who.currentLocation.animals.Remove(__instance.myID.Value);
            }
            return false;
        }

        return true;
    }

    internal static bool Pet_checkAction_Prefix(Pet __instance, Farmer who, GameLocation l)
    {
        // wild pet
        if (__instance.modData.ContainsKey(ModData_Wild))
        {
            if (!TryInteractTriggerOrEvent(__instance, who, l, out string? error))
            {
                ModEntry.Log(error, LogLevel.Error);
                DelayedAction.functionAfterDelay(() => l.characters.Remove(__instance), 0);
            }
            return false;
        }

        // talking pet
        if (
            __instance.grantedFriendshipForPet.Value
            && PetChatter?.GetValue(__instance, AnimalTalkCtx.MakeForPet) is AnimalTalkCtx talkCtx
            && talkCtx.TryShowDialogue(__instance.displayName, who, l)
        )
        {
            return false;
        }

        return true;
    }

    private static bool TryInteractTriggerOrEvent(
        Character chara,
        Farmer who,
        GameLocation l,
        [NotNullWhen(false)] out string? error
    )
    {
        error = null;
        if (!chara.modData.TryGetValue(ModData_WildInteract, out string triggerOrEvent))
        {
            triggerOrEvent = "NONE";
        }

        if (triggerOrEvent.EqualsIgnoreCase("NONE"))
        {
            return true;
        }

        if (triggerOrEvent.EqualsIgnoreCase("TRIGGER"))
        {
            TriggerActionManager.Raise(WildInteract_Trigger);
            return true;
        }

        string[] parts = triggerOrEvent.Split(':');
        if (parts.Length < 2)
        {
            error = $"Event script key '{triggerOrEvent}' not in 'asset:key' form";
            return false;
        }

        if (Game1.content.LoadStringReturnNullIfNotFound(triggerOrEvent) is not string eventScript)
        {
            error = $"Failed to load wild interact event script from '{triggerOrEvent}'";
            return false;
        }

        chara.Halt();
        WildEventTarget.Value = chara;
        Event wildEvent = new(eventScript, parts[0], parts[1], who)
        {
            eventPositionTileOffset = chara.TilePoint.ToVector2(),
        };

        if (chara is FarmAnimal animal)
        {
            l.animals.Remove(animal.myID.Value);
        }
        else if (chara is Pet pet)
        {
            DelayedAction.functionAfterDelay(() => l.characters.Remove(pet), 0);
        }

        l.startEvent(wildEvent);
        return true;
    }

    private static bool TryAddWildPet(
        GameLocation location,
        Point pnt,
        string petId,
        string breedId,
        string triggerOrEvent,
        [NotNullWhen(false)] out string? error
    )
    {
        if (!ValidatePetIds(ref petId, ref breedId, out error))
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
        ModEntry.Log($"Add wild pet '{WildKey}' to {location.NameOrUniqueName}{pnt}");
        return true;
    }

    private static bool TryAddWildFarmAnimal(
        GameLocation location,
        Point pnt,
        string farmAnimalId,
        string? skinId,
        bool isAdult,
        string triggerOrEvent,
        [NotNullWhen(false)] out string? error
    )
    {
        if (!ValidateFarmAnimalIds(ref farmAnimalId, ref skinId, out error))
        {
            return false;
        }
        string WildKey = FormWildKey(pnt, farmAnimalId, skinId ?? "BASE");
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

        DelayedAction.functionAfterDelay(
            () =>
            {
                FarmAnimal animal = new(farmAnimalId, Game1.Multiplayer.getNewID(), FarmAnimalOwnerId)
                {
                    currentLocation = location,
                };
                animal.hideFromAnimalSocialMenu.Value = true;
                animal.modData[ModData_Wild] = WildKey;
                animal.modData[ModData_WildInteract] = triggerOrEvent;
                location.Animals.Add(animal.myID.Value, animal);
                animal.StopAllActions();
                animal.Position = new(pnt.X * Game1.tileSize, pnt.Y * Game1.tileSize);
                animal.allowReproduction.Value = false;
                animal.wasPet.Value = true;
                if (isAdult)
                {
                    animal.age.Value = animal.GetAnimalData()?.DaysToMature + 1 ?? 0;
                }
                animal.ReloadTextureIfNeeded(true);
                ModEntry.Log($"Add wild farm animal '{WildKey}' to {location.NameOrUniqueName}{pnt}");
            },
            0
        );

        return true;
    }

    private static bool DoAddWild(string[] args, TriggerActionContext context, [NotNullWhen(false)] out string? error)
    {
        if (!Context.IsMainPlayer)
        {
            error = $"Only the main player can use '{Action_AddWildPet}'";
            return false;
        }
        if (
            !ArgUtility.TryGet(args, 1, out string? locationName, out error, allowBlank: false, name: "string location")
            || !ArgUtility.TryGetPoint(args, 2, out Point pnt, out error, name: "Point pnt")
            || !ArgUtility.TryGet(args, 4, out string? mainId, out error, allowBlank: false, name: "string mainId")
            || !ArgUtility.TryGet(args, 5, out string? subId, out error, allowBlank: false, name: "string subId")
            || !ArgUtility.TryGetOptional(
                args,
                6,
                out string? extraArgs,
                out error,
                allowBlank: false,
                name: "string extraArgs"
            )
            || !ArgUtility.TryGetOptional(
                args,
                7,
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
        if (!TryGetLocationFromName(locationName, ref error, out GameLocation? location))
        {
            return false;
        }

        return args[0] switch
        {
            Action_AddWildPet => TryAddWildPet(location, pnt, mainId, subId, triggerOrEvent, out error),
            Action_AddWildFarmAnimal => TryAddWildFarmAnimal(
                location,
                pnt,
                mainId,
                subId,
                extraArgs.EqualsIgnoreCase("ADULT"),
                triggerOrEvent,
                out error
            ),
            _ => false,
        };
    }

    private static bool DoRemoveWild(
        string[] args,
        TriggerActionContext context,
        [NotNullWhen(false)] out string? error
    )
    {
        if (!Context.IsMainPlayer)
        {
            error = $"Only the main player can use '{Action_RemoveWildPet}'";
            return false;
        }
        if (
            !ArgUtility.TryGet(args, 1, out string? locationName, out error, allowBlank: false, name: "string location")
        )
        {
            return false;
        }

        if (!TryGetLocationFromName(locationName, ref error, out GameLocation? location))
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
            !ArgUtility.TryGet(args, 4, out string? petId, out error, allowBlank: false, name: "string petId")
            || !ArgUtility.TryGet(args, 5, out string? breedId, out error, allowBlank: false, name: "string breedId")
        )
        {
            return false;
        }

        string wildKey = FormWildKey(pnt, petId, breedId);
        switch (args[0])
        {
            case Action_RemoveWildPet:
                RemoveWildPetByKey(location, wildKey);
                break;
            case Action_RemoveWildFarmAnimal:
                RemoveWildFarmAnimalByKey(location, wildKey);
                break;
        }
        return true;
    }

    private static void RemoveWildPetByKey(GameLocation location, string wildKey)
    {
        location.characters.RemoveWhere(chara =>
            chara is Pet petChara
            && petChara.modData.TryGetValue(ModData_Wild, out string prevWildKey)
            && prevWildKey == wildKey
        );
    }

    private static void RemoveWildFarmAnimalByKey(GameLocation location, string wildKey)
    {
        location.animals.RemoveWhere(pair =>
            pair.Value.modData.TryGetValue(ModData_Wild, out string prevWildKey) && prevWildKey == wildKey
        );
    }

    private static bool TryGetLocationFromName(
        string locationName,
        [NotNullWhen(false)] ref string? error,
        [NotNullWhen(true)] out GameLocation? location
    )
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

    private static bool HAVE_HOUSING(string[] query, GameStateQueryContext context)
    {
        if (
            !ArgUtility.TryGet(
                query,
                1,
                out string? farmAnimalId,
                out string? error,
                allowBlank: false,
                name: "string farmAnimalId"
            )
        )
        {
            ModEntry.Log(error, LogLevel.Error);
            return false;
        }

        FarmAnimal animal = new(farmAnimalId, Game1.Multiplayer.getNewID(), FarmAnimalOwnerId);
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

    private static bool HAVE_PETBOWL(string[] query, GameStateQueryContext context)
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

    private static void FinishAdoptFarmAnimal(Building building, FarmAnimal animal, string farmAnimalName)
    {
        ModEntry.Log(
            $"Adopted {animal.type.Value}({animal.skinID.Value}) with name '{farmAnimalName}' to '{building.GetIndoorsName()}'"
        );
        (building.GetIndoors() as AnimalHouse)?.adoptAnimal(animal);
        animal.Name = farmAnimalName;
        animal.displayName = farmAnimalName;
        Game1.exitActiveMenu();
        Game1.dialogueUp = false;
        Game1.player.CanMove = true;
    }

    private static bool ValidateFarmAnimalIds(
        ref string farmAnimalId,
        ref string? skinId,
        [NotNullWhen(false)] out string? error
    )
    {
        if (!Game1.farmAnimalData.TryGetValue(farmAnimalId, out FarmAnimalData? farmAnimalDataSpecific))
        {
            error = $"No farm animal with id '{farmAnimalId}'";
            return false;
        }
        string? localSkinId = skinId;
        if (localSkinId.EqualsIgnoreCase("RANDOM"))
        {
            skinId = null;
        }
        else if (!farmAnimalDataSpecific.Skins.Any(skin => skin.Id == localSkinId))
        {
            error = $"No skin id '{localSkinId}' for '{farmAnimalId}'";
            return false;
        }
        error = null;
        return true;
    }

    private static bool DoAdoptFarmAnimal(
        string[] args,
        TriggerActionContext context,
        [NotNullWhen(false)] out string? error
    )
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

        if (!ValidateFarmAnimalIds(ref farmAnimalId, ref skinId, out error))
        {
            error = $"No farm animal with id '{farmAnimalId}'";
            return false;
        }

        FarmAnimal animal = new(farmAnimalId, Game1.Multiplayer.getNewID(), Game1.player.UniqueMultiplayerID);
        if (skinId != null)
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
                        (s) => FinishAdoptFarmAnimal(building, animal, s),
                        title,
                        farmAnimalName
                    );
                }
                else
                {
                    FinishAdoptFarmAnimal(building, animal, farmAnimalName);
                }
                return false;
            }
            return true;
        });
        ModEntry.Log($"Did not find building for {animal.type.Value}({animal.skinID.Value}) to live in", LogLevel.Info);
        return true;
    }

    private static bool ValidatePetIds(ref string petId, ref string breedId, [NotNullWhen(false)] out string? error)
    {
        if (!Game1.petData.TryGetValue(petId, out PetData? petDataSpecific))
        {
            error = $"No pet with id '{petId}'";
            return false;
        }

        if (petDataSpecific.Breeds.Count == 0)
        {
            error = $"No '{petId}' pet with breed id '{breedId}'";
            return false;
        }

        if (breedId.EqualsIgnoreCase("RANDOM"))
        {
            breedId = Random.Shared.ChooseFrom(petDataSpecific.Breeds.Select(breed => breed.Id).ToList())!;
        }

#if SDV17
        if (petDataSpecific.GetPreferredBreed(breedId) is null)
#else
        if (petDataSpecific.GetBreedById(breedId, allowNull: true) is null)
#endif
        {
            error = $"No pet with id '{breedId}'";
            return false;
        }

        error = null;
        return true;
    }

    private static bool DoAdoptPet(string[] args, TriggerActionContext context, [NotNullWhen(false)] out string? error)
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

        if (ValidatePetIds(ref petId, ref breedId, out error))
        {
            PetLicense license = new() { Name = string.Concat(petId, "|", breedId) };

            if (showNamingMenu)
            {
                string title = Game1.content.LoadString("Strings\\StringsFromCSFiles:Event.cs.1236");
                Game1.activeClickableMenu = new NamingMenu((s) => FinishAdoptPet(s, license), title, petName);
            }
            else
            {
                FinishAdoptPet(petName, license);
            }
            return true;
        }

        ModEntry.Log($"Invalid pet '{petId}|{breedId}'");
        return false;
    }

    private static void FinishAdoptPet(string petName, PetLicense license)
    {
        namePet_Method?.Invoke(license, petName);
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
