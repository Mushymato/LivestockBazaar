using LivestockBazaar.Integration;
using LivestockBazaar.Model;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PropertyChanged.SourceGenerator;
using StardewValley;
using StardewValley.ItemTypeDefinitions;
using StardewValley.TokenizableStrings;

namespace LivestockBazaar.GUI;

public sealed partial record BazaarLivestockEntry(BazaarContextMain Main, string ShopName, LivestockData Ls)
{
    // icon
    public readonly SDUISprite ShopIcon = Ls.ShopIcon;
    public Color ShopIconTint => HasRequiredBuilding ? Color.White : (Color.Black * 0.4f);

    // currency
    private readonly BaseCurrency currency = Ls.GetTradeCurrency(ShopName);
    public ParsedItemData TradeItem => currency.TradeItem;
    public int TradePrice = Ls.GetTradePrice(ShopName);
    public string TradeDisplayFont => TradePrice > 999999 ? "small" : "dialogue";
    public bool HasEnoughTradeItems => currency.HasEnough(TradePrice);
    public float ShopIconOpacity => HasEnoughTradeItems ? 1f : 0.5f;

    // has required animal building
    private bool? hasRequiredBuilding = null;
    public bool HasRequiredBuilding
    {
        get
        {
            hasRequiredBuilding ??= Main.AnimalHouseByLocation.Any(bld => bld.Value.CheckHasRequiredBuilding(this));
            return hasRequiredBuilding ?? false;
        }
    }

    // alternate purchase
    public bool HasAltPurchase => AltPurchase.Any();
    public IList<LivestockData> AltPurchase => Ls.AltPurchase;

    // hover color, controlled by main context
    [Notify]
    private Color backgroundTint = Color.White;

    // infobox anim
    public const int FRAME_PER_ROW = 4;
    public const int ROW_MAX = 4;
    public const int ROW_REPEAT_MAX = 2;

    public readonly string LivestockName = TokenParser.ParseText(
        Ls.Data.ShopDisplayName ?? Ls.Data.DisplayName ?? "???"
    );
    public readonly string Description = TokenParser.ParseText(Ls.Data.ShopDescription ?? "");

    [Notify]
    private int animRow = 0;

    [Notify]
    private int animFrame = 0;
    private int rowRepeat = 0;
    public SpriteEffects AnimFlip =>
        Ls.Data.UseFlippedRightForLeft && AnimRow == 3 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;

    public void ResetAnim()
    {
        AnimRow = 0;
        AnimFrame = 0;
        rowRepeat = 0;
    }

    public SDUISprite AnimSprite
    {
        get
        {
            int realFrame = AnimRow * FRAME_PER_ROW + AnimFrame;
            if (Ls.Data.UseFlippedRightForLeft && AnimRow == 3)
                realFrame -= 8;
            return new(
                Ls.SpriteSheet,
                new(
                    realFrame * Ls.Data.SpriteWidth % Ls.SpriteSheet.Width,
                    realFrame * Ls.Data.SpriteWidth / Ls.SpriteSheet.Width * Ls.Data.SpriteHeight,
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
        AnimFrame++;
        if (AnimFrame == 4)
        {
            AnimFrame = 0;
            rowRepeat++;
            if (rowRepeat == ROW_REPEAT_MAX)
            {
                rowRepeat = 0;
                AnimRow++;
                if (AnimRow == ROW_MAX)
                {
                    AnimRow = 0;
                }
            }
        }
    }

    // alt purchase

    // buy animal
    [Notify]
    private string buyName = Dialogue.randomName();

    public FarmAnimal BuyNewFarmAnimal()
    {
        currency.Deduct(TradePrice);
        // Game1.playSound("sell");
        // Game1.playSound("purchase");
        FarmAnimal animal =
            new(Ls.Key, Game1.Multiplayer.getNewID(), Game1.player.UniqueMultiplayerID) { Name = BuyName };
        Game1.playSound(animal.GetSoundId() ?? "purchase", 1200 + Game1.random.Next(-200, 201));
        BuyName = Dialogue.randomName();
        return animal;
    }

    public void RandomizeBuyName()
    {
        BuyName = Dialogue.randomName();
    }
}
