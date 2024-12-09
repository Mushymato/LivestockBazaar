using LivestockBazaar.Integration;
using LivestockBazaar.Model;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PropertyChanged.SourceGenerator;
using StardewValley;
using StardewValley.ItemTypeDefinitions;
using StardewValley.TokenizableStrings;

namespace LivestockBazaar.GUI;

public sealed partial record BazaarLivestockEntry(BazaarContextMain Main, string ShopName, LivestockEntry Ls)
{
    // icon
    public readonly SDUISprite? ShopIcon =
        new(Game1.content.Load<Texture2D>(Ls.Data.ShopTexture), Ls.Data.ShopSourceRect);
    public bool HasValidHouse => ValidAnimalHouseLocations?.Any() ?? false;
    public Color ShopIconTint => HasValidHouse ? Color.White : (Color.Black * 0.4f);

    // trade cost
    public ParsedItemData TradeItem = Ls.GetTradeItem(ShopName);
    public int TradePrice = Ls.GetTradePrice(ShopName);
    public string TradeDisplayFont => TradePrice > 999999 ? "small" : "dialogue";
    public bool HasEnoughCurrency
    {
        get
        {
            if (TradeItem == LivestockEntry.goldCoin)
                return Game1.player.Money >= TradePrice;
            else if (TradeItem.QualifiedItemId == "(O)858") // Qi Gem
                return Game1.player.QiGems >= TradePrice;
            // else if (TradeItem.QualifiedItemId == "(O)73") // Golden Walnut
            //     return Game1.netWorldState.Value.GoldenWalnuts >= TradePrice;
            else
                return Game1.player.Items.ContainsId(TradeItem.QualifiedItemId, TradePrice);
        }
    }
    public float ShopIconOpacity => HasEnoughCurrency ? 1f : 0.5f;

    // valid animal locations
    private IReadOnlyList<BazaarLocationEntry>? validAnimalHouseLocations = null;
    public IReadOnlyList<BazaarLocationEntry>? ValidAnimalHouseLocations
    {
        get
        {
            validAnimalHouseLocations ??= Main
                .AllAnimalHouseLocations.Values.Where((v) => v.CheckCanAcceptLivestock(this))
                .ToList();
            return validAnimalHouseLocations;
        }
    }

    // hover color, controlled by main context
    [Notify]
    private Color backgroundTint = Color.White;

    // infobox anim
    public readonly string LivestockName = TokenParser.ParseText(
        Ls.Data.ShopDisplayName ?? Ls.Data.DisplayName ?? "???"
    );
    public readonly string Description = TokenParser.ParseText(Ls.Data.ShopDescription ?? "");
    private Texture2D SpriteSheet => Game1.content.Load<Texture2D>(Ls.Data.Texture);
    public readonly string AnimLayout = $"content[{Ls.Data.SpriteWidth * 4}..] content[{Ls.Data.SpriteHeight * 4}..]";

    [Notify]
    private int animFrame = 0;
    public SpriteEffects AnimFlip =>
        Ls.Data.UseFlippedRightForLeft && AnimFrame >= 12 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;

    public void ResetAnim()
    {
        AnimFrame = 0;
    }

    public SDUISprite AnimSprite
    {
        get
        {
            int adjAnimFrame = AnimFrame;
            if (Ls.Data.UseFlippedRightForLeft && adjAnimFrame >= 12)
                adjAnimFrame -= 8;
            return new(
                SpriteSheet,
                new(
                    adjAnimFrame * Ls.Data.SpriteWidth % SpriteSheet.Width,
                    adjAnimFrame * Ls.Data.SpriteWidth / SpriteSheet.Width * Ls.Data.SpriteHeight,
                    Ls.Data.SpriteWidth,
                    Ls.Data.SpriteHeight
                ),
                SDUIEdges.NONE,
                new(Scale: 4)
            );
        }
    }

    public void NextFrame()
    {
        AnimFrame = (AnimFrame + 1) % 16;
    }

    public void DeductTradeItem()
    {
        if (TradeItem == LivestockEntry.goldCoin)
        {
            Game1.player.Money -= TradePrice;
        }
        else if (TradeItem.QualifiedItemId == "(O)858")
        {
            Game1.player.QiGems -= TradePrice;
        }
        else
        {
            Game1.player.Items.ReduceId(TradeItem.QualifiedItemId, TradePrice);
        }
    }

    public FarmAnimal GetNewFarmAnimal()
    {
        DeductTradeItem();
        FarmAnimal animal =
            new(Ls.Key, Game1.Multiplayer.getNewID(), Game1.player.UniqueMultiplayerID)
            {
                Name = Dialogue.randomName(),
            };
        return animal;
    }
}
