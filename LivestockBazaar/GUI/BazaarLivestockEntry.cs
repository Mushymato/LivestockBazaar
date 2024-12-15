using LivestockBazaar.Integration;
using LivestockBazaar.Model;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PropertyChanged.SourceGenerator;
using StardewValley;
using StardewValley.ItemTypeDefinitions;
using StardewValley.TokenizableStrings;

namespace LivestockBazaar.GUI;

public sealed partial record BazaarLivestockPurchaseEntry(LivestockData Ls)
{
    public readonly SDUISprite SpriteIcon = Ls.SpriteIcon;

    [Notify]
    private float iconOpacity = 0.4f;

    // skin
    [Notify]
    private int skinId = Ls.SkinData.Any() ? 0 : -2;
    public LivestockSkinData? Skin => skinId < 0 ? null : Ls.SkinData[skinId];
    public Texture2D SpriteSheet => Skin?.SpriteSheet ?? Ls.SpriteSheet;

    public void PrevSkin()
    {
        if (skinId != -2)
        {
            SkinId -= 1;
            if (skinId == -2)
                SkinId = Ls.SkinData.Count - 1;
        }
    }

    public void NextSkin()
    {
        if (skinId != -2)
        {
            SkinId += 1;
            if (skinId == Ls.SkinData.Count)
                SkinId = -1;
        }
    }
}

public sealed partial record BazaarLivestockEntry(BazaarContextMain Main, string ShopName, LivestockData Ls)
{
    // icon
    public readonly SDUISprite ShopIcon = Ls.ShopIcon;
    public Color ShopIconTint => HasRequiredBuilding ? Color.White : Color.Black * 0.4f;

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

    [Notify]
    private Texture2D animSpriteSheet = Ls.SpriteSheet;
    public SDUISprite AnimSprite
    {
        get
        {
            int realFrame = AnimRow * FRAME_PER_ROW + AnimFrame;
            if (Ls.Data.UseFlippedRightForLeft && AnimRow == 3)
                realFrame -= 8;
            return new(
                AnimSpriteSheet,
                new(
                    realFrame * Ls.Data.SpriteWidth % AnimSpriteSheet.Width,
                    realFrame * Ls.Data.SpriteWidth / AnimSpriteSheet.Width * Ls.Data.SpriteHeight,
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
                    AnimRow = 0;
            }
        }
    }

    // alt purchase
    public bool HasAltPurchase => AltPurchase.Any();

    [Notify]
    private int skinId = -2;
    public bool HasSkin => SkinId != -2;
    public float RandSkinOpacity => SkinId == -1 ? 1f : 0f;
    public Color AnimTint => SkinId == -1 ? Color.Black * 0.4f : Color.White;
    private IReadOnlyList<BazaarLivestockPurchaseEntry>? altPurchase = null;
    public IReadOnlyList<BazaarLivestockPurchaseEntry> AltPurchase
    {
        get
        {
            if (altPurchase == null)
            {
                altPurchase = Ls.AltPurchase.Select((ls) => new BazaarLivestockPurchaseEntry(ls)).ToList();
                if (altPurchase.Any())
                    HandleSelectedPurchase(altPurchase[0]);
                else
                    HandleSelectedPurchase(new BazaarLivestockPurchaseEntry(Ls));
            }
            return altPurchase;
        }
    }

    [Notify]
    public float purchaseOpacity = 1f;
    private BazaarLivestockPurchaseEntry? selectedPurchase;

    public void HandleSelectedPurchase(BazaarLivestockPurchaseEntry purchase)
    {
        if (selectedPurchase != null)
            selectedPurchase.IconOpacity = 0.4f;
        selectedPurchase = purchase;
        selectedPurchase.IconOpacity = 1f;
        SkinId = selectedPurchase.SkinId;
        AnimSpriteSheet = selectedPurchase.SpriteSheet;
    }

    public void PrevSkin()
    {
        if (selectedPurchase != null)
        {
            selectedPurchase.PrevSkin();
            SkinId = selectedPurchase.SkinId;
            AnimSpriteSheet = selectedPurchase.SpriteSheet;
        }
    }

    public void NextSkin()
    {
        if (selectedPurchase != null)
        {
            selectedPurchase.NextSkin();
            SkinId = selectedPurchase.SkinId;
            AnimSpriteSheet = selectedPurchase.SpriteSheet;
        }
    }

    // buy animal
    [Notify]
    private string buyName = Dialogue.randomName();

    public FarmAnimal? BuyNewFarmAnimal()
    {
        if (selectedPurchase == null)
        {
            return null;
        }
        currency.Deduct(TradePrice);
        LivestockData ls = selectedPurchase.Ls;
        FarmAnimal animal =
            new(ls.Key, Game1.Multiplayer.getNewID(), Game1.player.UniqueMultiplayerID) { Name = BuyName };
        if (selectedPurchase.SkinId > -1)
        {
            animal.skinID.Value = selectedPurchase.Skin!.Skin.Id;
        }
        Game1.playSound(animal.GetSoundId() ?? "purchase", 1200 + Game1.random.Next(-200, 201));
        RandomizeBuyName();
        return animal;
    }

    public void RandomizeBuyName() => BuyName = Dialogue.randomName();
}
