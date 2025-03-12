using LivestockBazaar.Integration;
using LivestockBazaar.Model;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PropertyChanged.SourceGenerator;
using StardewValley;
using StardewValley.GameData.Buildings;
using StardewValley.GameData.FarmAnimals;
using StardewValley.ItemTypeDefinitions;

namespace LivestockBazaar.GUI;

public sealed partial record BazaarLivestockPurchaseEntry(LivestockData Ls)
{
    public readonly string LivestockName = Wheels.ParseTextOrDefault(
        Ls.Data.ShopDisplayName ?? Ls.Data.DisplayName,
        "???"
    );
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
    public bool CurrencyIsMoney => currency is MoneyCurrency;
    public ParsedItemData TradeItem => currency.TradeItem;
    public int TradePrice = Ls.GetTradePrice(ShopName);
    public string TradePriceFmt => TradePrice > 99999 ? $"{TradePrice / 1000f}k" : TradePrice.ToString();
    public bool HasEnoughTradeItems => currency.HasEnough(TradePrice);
    public int TotalCurrency => currency.GetTotal();
    public float ShopIconOpacity => HasEnoughTradeItems && Main.HasSpaceForLivestock(this) ? 1f : 0.5f;

    // has required animal building
    public string House => Ls.Data.House;
    private BuildingData? requiredBuildingData = null;
    public BuildingData? RequiredBuildingData
    {
        get
        {
            if (Ls.Data.RequiredBuilding == null)
                return null;
            if (requiredBuildingData != null)
                return requiredBuildingData;
            if (Game1.buildingData.TryGetValue(Ls.Data.RequiredBuilding, out requiredBuildingData))
                return requiredBuildingData;
            return null;
        }
    }
    public string RequiredBuilding => Ls.Data.RequiredBuilding;
    private bool? hasRequiredBuilding = null;
    public bool HasRequiredBuilding
    {
        get
        {
            if (Ls.Data.RequiredBuilding == null)
                return true;
            hasRequiredBuilding ??= Main.AnimalHouseByLocation.Any(bld => bld.Value.CheckHasRequiredBuilding(this));
            return hasRequiredBuilding ?? false;
        }
    }
    public string? RequiredBuildingText =>
        Wheels.ParseTextOrDefault(
            Ls.Data.ShopMissingBuildingDescription,
            RequiredBuildingData?.Name ?? Ls.Data.RequiredBuilding ?? "???"
        );
    public SDUISprite? RequiredBuildingSprite =>
        RequiredBuildingData != null
            ? new SDUISprite(
                Game1.content.Load<Texture2D>(RequiredBuildingData.Texture),
                RequiredBuildingData.SourceRect
            )
            : null;
    public bool CanBuy => HasEnoughTradeItems && HasRequiredBuilding;

    // hover color, controlled by main context
    [Notify]
    private Color backgroundTint = Color.White;

    // infobox anim
    public const int FRAME_PER_ROW = 4;
    public const int ROW_MAX = 4;
    public const int ROW_REPEAT_MAX = 2;

    public readonly string LivestockName = Wheels.ParseTextOrDefault(
        Ls.Data.ShopDisplayName ?? Ls.Data.DisplayName,
        "???"
    );
    public readonly string Description = Wheels.ParseTextOrDefault(Ls.Data.ShopDescription, "??? ???? ?? ????? ?");

    public IEnumerable<ParsedItemData> LivestockProduce
    {
        get
        {
            FarmAnimalData data = selectedPurchase == null ? Ls.Data : selectedPurchase.Ls.Data;
            HashSet<string> seenProduce = [];
            foreach (FarmAnimalProduce prod in data.ProduceItemIds.Concat(data.DeluxeProduceItemIds))
                if (
                    !seenProduce.Contains(prod.ItemId)
                    && ItemRegistry.GetData("(O)" + prod.ItemId) is ParsedItemData itemData
                )
                {
                    yield return itemData;
                    seenProduce.Add(prod.ItemId);
                }
        }
    }

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
        OnPropertyChanged(new(nameof(LivestockProduce)));
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
        OnPropertyChanged(new(nameof(TotalCurrency)));
        LivestockData ls = selectedPurchase.Ls;
        FarmAnimal animal =
            new(ls.Key, Game1.Multiplayer.getNewID(), Game1.player.UniqueMultiplayerID) { Name = BuyName };
        if (selectedPurchase.SkinId > -1)
        {
            if (selectedPurchase.Skin == null)
                animal.skinID.Value = null;
            else
                animal.skinID.Value = selectedPurchase.Skin.Skin.Id;
        }
        Game1.playSound(animal.GetSoundId() ?? "purchase", 1200 + Game1.random.Next(-200, 201));
        RandomizeBuyName();
        return animal;
    }

    public void RandomizeBuyName() => BuyName = Dialogue.randomName();
}
