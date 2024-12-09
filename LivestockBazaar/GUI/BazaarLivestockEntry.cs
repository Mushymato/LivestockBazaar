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

    // currency
    private readonly BaseCurrency currency = Ls.GetTradeCurrency(ShopName);
    public ParsedItemData TradeItem => currency.TradeItem;
    public int TradePrice = Ls.GetTradePrice(ShopName);
    public string TradeDisplayFont => TradePrice > 999999 ? "small" : "dialogue";
    public bool HasEnoughTradeItems => currency.HasEnough(TradePrice);
    public float ShopIconOpacity => HasEnoughTradeItems ? 1f : 0.5f;

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

    [Notify]
    private string buyName = Dialogue.randomName();

    public void NextFrame()
    {
        AnimFrame = (AnimFrame + 1) % 16;
    }

    public FarmAnimal BuyNewFarmAnimal()
    {
        currency.Deduct(TradePrice);
        Game1.playSound("sell");
        Game1.playSound("purchase");
        FarmAnimal animal =
            new(Ls.Key, Game1.Multiplayer.getNewID(), Game1.player.UniqueMultiplayerID) { Name = BuyName };
        BuyName = Dialogue.randomName();
        return animal;
    }

    public void RandomizeBuyName()
    {
        BuyName = Dialogue.randomName();
    }
}
