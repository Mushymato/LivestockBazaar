using System.Collections.Immutable;
using LivestockBazaar.Model;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.Extensions;
using StardewValley.GameData.Shops;
using StardewValley.TokenizableStrings;

namespace LivestockBazaar.GUI;

/// <summary>Context for bazaar menu</summary>
/// <param name="shopName"></param>
public sealed class BazaarContextMain
{
    // fields
    private readonly GameLocation shopLocation;
    private readonly string shopName;
    private readonly ShopOwnerData? ownerData;

    // derived
    public readonly ImmutableList<BazaarLivestockEntry> LivestockData;
    public readonly BazaarData? Data;

    // layout based on "70%[1204..] 80%[648..]"
    public readonly string ForSaleLayout;

    // Shop owner portrait
    public bool DisplayOwner => OwnerPortrait != null;
    public readonly Tuple<Texture2D, Rectangle>? OwnerPortrait = null;
    public string OwnerDialog = I18n.Shop_DefaultDialog();

    public BazaarContextMain(GameLocation shopLocation, string shopName, ShopOwnerData? ownerData = null)
    {
        this.shopLocation = shopLocation;
        this.shopName = shopName;
        this.ownerData = ownerData;

        LivestockData = AssetManager.GetAnimalStockData(shopName).Select((data) => new BazaarLivestockEntry(data)).ToImmutableList();
        Data = AssetManager.GetBazaarData(shopName);

        var viewport = Game1.viewport;
        ForSaleLayout = $"{(int)(MathF.Max(viewport.Width * 0.7f, 1204) / 160) * 160}px {(int)(MathF.Max(viewport.Height * 0.8f, 648) / 160) * 160}px";

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
            Random random = ownerData.RandomizeDialogueOnOpen ? Game1.random : Utility.CreateRandom(Game1.uniqueIDForThisGame, Game1.stats.DaysPlayed);
            foreach (ShopDialogueData sdd in ownerData.Dialogues)
            {
                if (
                    GameStateQuery.CheckConditions(sdd.Condition)
                    && ((sdd.RandomDialogue != null && sdd.RandomDialogue.Any()) ? random.ChooseFrom(sdd.RandomDialogue) : sdd.Dialogue) is string rawDialog
                )
                {
                    OwnerDialog = TokenParser.ParseText(rawDialog);
                    return;
                }
            }
        }
    }
}
