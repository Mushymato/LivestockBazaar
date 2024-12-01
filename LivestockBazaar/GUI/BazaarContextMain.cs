using System.Collections.Immutable;
using LivestockBazaar.Integration;
using LivestockBazaar.Model;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PropertyChanged.SourceGenerator;
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
    private const int CELL_WIDTH = 192;

    // viewport size, could change but ppl should just reopen menu
    // public bool IsWidescreen => Game1.viewport.Width >= 1920 * Game1.options.uiScale;
    public bool IsWidescreen => Game1.viewport.Width >= 1920 * Game1.options.uiScale;

    // fields
    private readonly GameLocation shopLocation;
    private readonly string shopName;
    private readonly ShopOwnerData? ownerData;

    // derived
    public readonly BazaarData? Data;
    public readonly ImmutableList<BazaarLivestockEntry> LivestockData;

    // theme
    public readonly ShopMenu.ShopCachedTheme Theme;

    // SDUI style sprites
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

    // Forwarded props
    public Color? Theme_DialogueColor => Theme.DialogueColor ?? Game1.textColor;
    public Color? Theme_DialogueShadowColor => Theme.DialogueShadowColor ?? Game1.textShadowColor;

    public readonly string ForSaleLayout;

    // Shop owner portrait
    public bool ShowPortraitBox => IsWidescreen && OwnerPortrait != null;
    public readonly SDUISprite? OwnerPortrait = null;
    public string OwnerDialog = I18n.Shop_DefaultDialog();

    // Hovered livestock entry
    [Notify]
    private BazaarLivestockEntry? hoveredLivestock = null;
    public bool HasLivestock => HoveredLivestock != null;

    public BazaarContextMain(GameLocation shopLocation, string shopName, ShopOwnerData? ownerData = null)
    {
        this.shopLocation = shopLocation;
        this.shopName = shopName;
        this.ownerData = ownerData;

        Data = AssetManager.GetBazaarData(shopName);
        Theme = new ShopMenu.ShopCachedTheme(
            Data?.ShopData?.VisualTheme?.FirstOrDefault(
                (ShopThemeData theme) => GameStateQuery.CheckConditions(theme.Condition)
            )
        );
        LivestockData = AssetManager
            .GetAnimalStockData(shopName)
            .Select((data) => new BazaarLivestockEntry(this, shopName, data))
            .ToImmutableList();

        var viewport = Game1.viewport;
        ForSaleLayout =
            $"{(int)(MathF.Max(viewport.Width * 0.6f, 1280) / CELL_WIDTH) * CELL_WIDTH}px {(int)(MathF.Max(viewport.Height * 0.7f, 720) / CELL_WIDTH) * CELL_WIDTH}px";

        // Shop owner setup
        if (ownerData == null || ownerData.Type == ShopOwnerType.None)
            return;

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

        if (portraitTexture != null)
        {
            OwnerPortrait = new(portraitTexture, new(0, 0, 64, 64));
        }
        else
        {
            OwnerPortrait = null;
            return;
        }

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

    public void Update(TimeSpan elapsed)
    {
        if (elapsed.TotalMilliseconds > 100)
            HoveredLivestock?.NextFrame();
    }
}
