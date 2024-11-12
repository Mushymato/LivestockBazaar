using System.Collections.Immutable;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.Extensions;
using StardewValley.GameData.Shops;
using StardewValley.TokenizableStrings;

namespace LivestockBazaar.GUI;

/// <summary>Context for bazaar menu</summary>
/// <param name="shopName"></param>
public sealed class ContextMain
{
    // fields
    private readonly GameLocation shopLocation;
    private readonly string shopName;
    private readonly ShopOwnerData? ownerData;

    // derived
    public readonly ImmutableList<FarmAnimalBuyEntry> FarmAnimalBuy;
    public readonly BazaarData? Data;

    // Shop owner portrait
    public bool DisplayOwner => OwnerPortrait != null;
    public readonly Tuple<Texture2D, Rectangle>? OwnerPortrait = null;
    public string OwnerDialog = I18n.Shop_DefaultDialog();

    // Viewport dependent layout, just for testing
    public readonly string MenuSize = "";

    public ContextMain(GameLocation shopLocation, string shopName, ShopOwnerData? ownerData = null)
    {
        this.shopLocation = shopLocation;
        this.shopName = shopName;
        this.ownerData = ownerData;

        FarmAnimalBuy = AssetManager.GetAnimalStockData(shopName).ToImmutableList();
        Data = AssetManager.GetBazaarData(shopName);

        var viewportSize = Game1.viewport.Size;
        MenuSize = $"{viewportSize.Width - 400}px {viewportSize.Height - 300}px";

        // Shop owner setup
        if (ownerData == null || ownerData.Type == ShopOwnerType.None)
            return;

        Texture2D? portraitTexture = null;
        if (ownerData.Portrait != null && string.IsNullOrWhiteSpace(ownerData.Portrait))
        {
            if (Game1.content.DoesAssetExist<Texture2D>(ownerData.Portrait))
            {
                portraitTexture = Game1.content.Load<Texture2D>(ownerData.Portrait);
            }
            else if (
                Game1.getCharacterFromName(ownerData.Portrait) is NPC ownerNPC
                && ownerNPC.Portrait != null
            )
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
}
