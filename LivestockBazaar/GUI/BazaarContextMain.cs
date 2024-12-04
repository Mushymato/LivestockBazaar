using System.Collections.Immutable;
using LivestockBazaar.Integration;
using LivestockBazaar.Model;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PropertyChanged.SourceGenerator;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Extensions;
using StardewValley.GameData.Shops;
using StardewValley.Menus;
using StardewValley.TokenizableStrings;

namespace LivestockBazaar.GUI;

/// <summary>Context for bazaar menu</summary>
/// <param name="shopName"></param>
public sealed partial class BazaarContextMain
{
    private const int CELL_W = 192;

    // viewport size, could change but ppl should just reopen menu
    // public bool IsWidescreen => Game1.viewport.Width >= 1920 * Game1.options.uiScale;
    public bool IsWidescreen = Game1.viewport.Width >= 1920 * Game1.options.uiScale;

    // fields
    private readonly GameLocation shopLocation;
    private readonly string shopName;
    private readonly ShopOwnerData? ownerData;

    // data
    public readonly BazaarData? Data;
    public readonly ImmutableList<BazaarLivestockEntry> LivestockEntries;
    public readonly ImmutableList<BazaarLocationEntry> TargetLocations;

    // theme
    public readonly ShopMenu.ShopCachedTheme Theme;
    public SDUISprite Theme_WindowBorder =>
        new(
            Theme.WindowBorderTexture,
            Theme.WindowBorderSourceRect,
            new(Theme.WindowBorderSourceRect.Width / 3),
            new(Scale: 4)
        );
    public SDUIEdges Theme_WindowBorderThickness => 4 * (Theme_WindowBorder.FixedEdges ?? SDUIEdges.NONE);
    public SDUISprite Theme_PortraitBackground =>
        new(Theme.PortraitBackgroundTexture, Theme.PortraitBackgroundSourceRect);
    public SDUISprite Theme_DialogueBackground =>
        new(
            Theme.DialogueBackgroundTexture,
            Theme.DialogueBackgroundSourceRect,
            new(Theme.DialogueBackgroundSourceRect.Width / 3),
            new(Scale: 1)
        );
    public SDUISprite Theme_ItemRowBackground =>
        new(
            Theme.ItemRowBackgroundTexture,
            Theme.ItemRowBackgroundSourceRect,
            new(Theme.ItemRowBackgroundSourceRect.Width / 3),
            new(Scale: 4)
        );
    public SDUISprite Theme_ItemIconBackground =>
        new(Theme.ItemIconBackgroundTexture, Theme.ItemIconBackgroundSourceRect);
    public SDUISprite Theme_ScrollUp => new(Theme.ScrollUpTexture, Theme.ScrollUpSourceRect);
    public SDUISprite Theme_ScrollDown => new(Theme.ScrollDownTexture, Theme.ScrollDownSourceRect);
    public SDUISprite Theme_ScrollBarFront => new(Theme.ScrollBarFrontTexture, Theme.ScrollBarFrontSourceRect);
    public SDUISprite Theme_ScrollBarBack =>
        new(Theme.ScrollBarBackTexture, Theme.ScrollBarBackSourceRect, new(2), new(Scale: 4));

    public Color? Theme_DialogueColor => Theme.DialogueColor ?? Game1.textColor;
    public Color? Theme_DialogueShadowColor => Theme.DialogueShadowColor ?? Game1.textShadowColor;

    // button text
    public readonly string BtnCancel = TokenParser.ParseText("[LocalizedText Strings\\UI:Cancel]");

    // layouts
    public readonly string MainBodyLayout;
    public readonly string ForSaleLayout;

    // shop owner portrait
    public bool ShowPortraitBox => IsWidescreen && OwnerPortrait != null;
    public readonly SDUISprite? OwnerPortrait = null;
    public string OwnerDialog = I18n.Shop_DefaultDialog();

    // hovered livestock entry
    [Notify]
    private BazaarLivestockEntry? hoveredLivestock = null;
    public bool HasLivestock => HoveredLivestock != null;

    // selected livestock entry
    [Notify]
    private BazaarLivestockEntry? selectedLivestock = null;
    public int CurrentPage => SelectedLivestock == null ? 1 : 2;

    // hovered building entry
    [Notify]
    private BazaarBuildingEntry? hoveredBuilding = null;

    public BazaarContextMain(GameLocation shopLocation, string shopName, ShopOwnerData? ownerData = null)
    {
        this.shopLocation = shopLocation;
        this.shopName = shopName;
        this.ownerData = ownerData;

        // bazaar data
        Data = AssetManager.GetBazaarData(shopName);
        Theme = new ShopMenu.ShopCachedTheme(
            Data?.ShopData?.VisualTheme?.FirstOrDefault(
                (ShopThemeData theme) => GameStateQuery.CheckConditions(theme.Condition)
            )
        );
        // livestock data
        LivestockEntries = AssetManager
            .GetAnimalStockData(shopName)
            .Select((data) => new BazaarLivestockEntry(this, shopName, data))
            .ToImmutableList();

        Dictionary<GameLocation, List<BazaarBuildingEntry>> locationToBuildings = [];
        Utility.ForEachBuilding(
            (building) =>
            {
                if (building.GetIndoors() is not AnimalHouse)
                    return true;
                GameLocation parentLocation = building.GetParentLocation();
                if (locationToBuildings.ContainsKey(building.GetParentLocation()))
                    locationToBuildings[parentLocation].Add(new(this, building));
                else
                    locationToBuildings[parentLocation] = [new(this, building)];
                return true;
            }
        );
        TargetLocations = locationToBuildings
            .Select((kv) => new BazaarLocationEntry(this, kv.Key, kv.Value))
            .ToImmutableList();

        // layout shenanigans
        var viewport = Game1.viewport;
        int desiredWidth = (int)((MathF.Max(viewport.Width * 0.6f, 1280) - 256) / CELL_W) * CELL_W;
        ForSaleLayout = $"{desiredWidth}px 70%[676..]";
        MainBodyLayout = $"{desiredWidth + 256}px content";

        // shop owner setup
        if (ownerData != null && ownerData.Type != ShopOwnerType.None)
        {
            Texture2D? portraitTexture = null;
            if (ownerData.Portrait != null && !string.IsNullOrWhiteSpace(ownerData.Portrait))
            {
                if (Game1.content.DoesAssetExist<Texture2D>(ownerData.Portrait))
                {
                    portraitTexture = Game1.content.Load<Texture2D>(ownerData.Portrait);
                }
                else if (Game1.getCharacterFromName(ownerData.Portrait) is NPC ownerNPC && ownerNPC.Portrait != null)
                {
                    portraitTexture = ownerNPC.Portrait;
                }
            }
            else if (
                ownerData.Type == ShopOwnerType.NamedNpc
                && !string.IsNullOrWhiteSpace(ownerData.Name)
                && Game1.getCharacterFromName(ownerData.Name) is NPC ownerNPC
                && ownerNPC.Portrait != null
            )
            {
                portraitTexture = ownerNPC.Portrait;
            }
            OwnerPortrait = portraitTexture != null ? new(portraitTexture, new(0, 0, 64, 64)) : null;

            if (ownerData.Dialogues != null)
            {
                Random random = ownerData.RandomizeDialogueOnOpen
                    ? Game1.random
                    : Utility.CreateRandom(Game1.uniqueIDForThisGame, Game1.stats.DaysPlayed);
                foreach (ShopDialogueData sdd in ownerData.Dialogues)
                {
                    if (
                        GameStateQuery.CheckConditions(sdd.Condition)
                        && (
                            (sdd.RandomDialogue != null && sdd.RandomDialogue.Any())
                                ? random.ChooseFrom(sdd.RandomDialogue)
                                : sdd.Dialogue
                        )
                            is string rawDialog
                    )
                    {
                        OwnerDialog = TokenParser.ParseText(rawDialog);
                        return;
                    }
                }
            }
        }
    }

    // events
    private TimeSpan animTimer = TimeSpan.Zero;
    private readonly TimeSpan animInterval = TimeSpan.FromMilliseconds(175);
    public SButton? justPressed = null;

    public void Update(TimeSpan elapsed)
    {
        animTimer += elapsed;
        if (animTimer >= animInterval)
        {
            HoveredLivestock?.NextFrame();
            animTimer = TimeSpan.Zero;
        }
    }

    public bool HandleButtonPress(SButton button)
    {
        justPressed = button;
        return false;
    }

    // page 1 (shop grid) hover and select
    public void HandleHoverLivestock(BazaarLivestockEntry livestock)
    {
        if (HoveredLivestock != null)
            HoveredLivestock.BackgroundTint = Color.White;
        livestock.BackgroundTint = Theme.ItemRowBackgroundHoverColor;
        if (HoveredLivestock != livestock)
        {
            livestock.ResetAnim();
            HoveredLivestock = livestock;
        }
    }

    public void HandleSelectLivestock(BazaarLivestockEntry livestock)
    {
        livestock.BackgroundTint = Color.White;
        if (SelectedLivestock != livestock)
        {
            SelectedLivestock = livestock;
        }
    }

    // page 1 (shop grid) hover and select
    public void HandleHoverBuilding(BazaarBuildingEntry house)
    {
        if (HoveredBuilding != null)
            HoveredBuilding.BackgroundTint = Color.White;
        house.BackgroundTint = Theme.ItemRowBackgroundHoverColor;
        if (HoveredBuilding != house)
        {
            HoveredBuilding = house;
        }
    }

    // public void HandleSelectLivestock(BazaarLivestockEntry livestock)
    // {
    //     livestock.BackgroundTint = Color.White;
    //     if (SelectedLivestock != livestock)
    //     {
    //         SelectedLivestock = livestock;
    //     }
    // }
}
