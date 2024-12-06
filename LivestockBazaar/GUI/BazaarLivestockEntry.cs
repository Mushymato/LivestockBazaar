using LivestockBazaar.Integration;
using LivestockBazaar.Model;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PropertyChanged.SourceGenerator;
using StardewValley;
using StardewValley.GameData.FarmAnimals;
using StardewValley.ItemTypeDefinitions;
using StardewValley.TokenizableStrings;

namespace LivestockBazaar.GUI;

public sealed partial record BazaarLivestockEntry(BazaarContextMain Main, string ShopName, FarmAnimalData Data)
{
    // BazaarContextMain.AllAnimalHouseLocations.Select((kv) => kv.Value.CanAcceptLivestock(this))
    // icon
    public readonly SDUISprite? ShopIcon = new(Game1.content.Load<Texture2D>(Data.ShopTexture), Data.ShopSourceRect);
    public bool CanPurchase => ValidAnimalHouseLocations?.Any() ?? false;
    public Color ShopIconTint => CanPurchase ? Color.White : (Color.Black * 0.4f);

    // trade cost
    public ParsedItemData TradeItem = Data.GetTradeItem(ShopName);
    public int TradePrice = Data.GetTradePrice(ShopName);
    public string TradeDisplayFont => TradePrice > 999999 ? "small" : "dialogue";

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
    public readonly string LivestockName = TokenParser.ParseText(Data.ShopDisplayName ?? Data.DisplayName ?? "???");
    public readonly string Description = TokenParser.ParseText(Data.ShopDescription ?? "");
    private Texture2D SpriteSheet => Game1.content.Load<Texture2D>(Data.Texture);
    public readonly string AnimLayout = $"content[{Data.SpriteWidth * 4}..] content[{Data.SpriteHeight * 4}..]";

    [Notify]
    private int animFrame = 0;

    public void ResetAnim()
    {
        AnimFrame = 0;
    }

    public SDUISprite AnimSprite
    {
        get
        {
            // TODO: flip the sprite too
            int adjAnimFrame = AnimFrame;
            if (Data.UseFlippedRightForLeft && adjAnimFrame >= 12)
            {
                adjAnimFrame -= 8;
            }
            return new(
                SpriteSheet,
                new(
                    adjAnimFrame * Data.SpriteWidth % SpriteSheet.Width,
                    adjAnimFrame * Data.SpriteWidth / SpriteSheet.Width * Data.SpriteHeight,
                    Data.SpriteWidth,
                    Data.SpriteHeight
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
}
