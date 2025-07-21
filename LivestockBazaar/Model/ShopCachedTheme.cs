// This class is directly lifted from game for purpose of phone smapi compat.
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.GameData.Shops;

namespace LivestockBazaar.Model;

/// <summary>A cached visual theme for the <see cref="T:StardewValley.Menus.ShopMenu" />.</summary>
public class ShopCachedTheme
{
    /// <summary>The visual theme data from <c>Data/Shops</c>, if applicable.</summary>
    public ShopThemeData? ThemeData { get; }

    /// <summary>The texture for the shop window border.</summary>
    public Texture2D WindowBorderTexture { get; }

    /// <summary>The pixel area within the <see cref="P:StardewValley.Menus.ShopMenu.ShopCachedTheme.WindowBorderSourceRect" /> for the shop window border. This should be an 18x18 pixel area.</summary>
    public Rectangle WindowBorderSourceRect { get; }

    /// <summary>The texture for the NPC portrait background.</summary>
    public Texture2D PortraitBackgroundTexture { get; }

    /// <summary>The pixel area within the <see cref="P:StardewValley.Menus.ShopMenu.ShopCachedTheme.PortraitBackgroundTexture" /> for the NPC portrait background. This should be a 74x47 pixel area.</summary>
    public Rectangle PortraitBackgroundSourceRect { get; }

    /// <summary>The texture for the NPC dialogue background.</summary>
    public Texture2D DialogueBackgroundTexture { get; }

    /// <summary>The pixel area within the <see cref="P:StardewValley.Menus.ShopMenu.ShopCachedTheme.DialogueBackgroundTexture" /> for the NPC dialogue background. This should be a 60x60 pixel area.</summary>
    public Rectangle DialogueBackgroundSourceRect { get; }

    /// <summary>The sprite text color for the dialogue text, or <c>null</c> for the default color.</summary>
    public Color? DialogueColor { get; }

    /// <summary>The sprite text shadow color for the dialogue text, or <c>null</c> for the default color.</summary>
    public Color? DialogueShadowColor { get; }

    /// <summary>The texture for the item row background.</summary>
    public Texture2D ItemRowBackgroundTexture { get; }

    /// <summary>The pixel area within the <see cref="P:StardewValley.Menus.ShopMenu.ShopCachedTheme.ItemRowBackgroundTexture" /> for the item row background. This should be a 15x15 pixel area.</summary>
    public Rectangle ItemRowBackgroundSourceRect { get; }

    /// <summary>The color tint to apply to the item row background when the cursor is hovering over it</summary>
    public Color ItemRowBackgroundHoverColor { get; }

    /// <summary>The sprite text color for the item text, or <c>null</c> for the default color.</summary>
    public Color? ItemRowTextColor { get; }

    /// <summary>The texture for the box behind the item icons.</summary>
    public Texture2D ItemIconBackgroundTexture { get; }

    /// <summary>The pixel area within the <see cref="P:StardewValley.Menus.ShopMenu.ShopCachedTheme.ItemIconBackgroundTexture" /> for the item icon background. This should be an 18x18 pixel area.</summary>
    public Rectangle ItemIconBackgroundSourceRect { get; }

    /// <summary>The texture for the scroll up icon.</summary>
    public Texture2D ScrollUpTexture { get; }

    /// <summary>The pixel area within the <see cref="P:StardewValley.Menus.ShopMenu.ShopCachedTheme.ScrollUpTexture" /> for the scroll up icon. This should be an 11x12 pixel area.</summary>
    public Rectangle ScrollUpSourceRect { get; }

    /// <summary>The texture for the scroll down icon.</summary>
    public Texture2D ScrollDownTexture { get; }

    /// <summary>The pixel area within the <see cref="P:StardewValley.Menus.ShopMenu.ShopCachedTheme.ScrollDownTexture" /> for the scroll down icon. This should be an 11x12 pixel area.</summary>
    public Rectangle ScrollDownSourceRect { get; }

    /// <summary>The texture for the scrollbar foreground texture.</summary>
    public Texture2D ScrollBarFrontTexture { get; }

    /// <summary>The pixel area within the <see cref="P:StardewValley.Menus.ShopMenu.ShopCachedTheme.ScrollBarFrontTexture" /> for the scroll foreground. This should be a 6x10 pixel area.</summary>
    public Rectangle ScrollBarFrontSourceRect { get; }

    /// <summary>The texture for the scrollbar background texture.</summary>
    public Texture2D ScrollBarBackTexture { get; }

    /// <summary>The pixel area within the <see cref="P:StardewValley.Menus.ShopMenu.ShopCachedTheme.ScrollBarBackTexture" /> for the scroll background. This should be a 6x6 pixel area.</summary>
    public Rectangle ScrollBarBackSourceRect { get; }

    /// <summary>Construct an instance.</summary>
    /// <param name="theme">The visual theme data, or <c>null</c> for the default shop theme.</param>
    public ShopCachedTheme(ShopThemeData? theme)
    {
        ThemeData = theme;
        WindowBorderTexture = LoadThemeTexture(theme?.WindowBorderTexture, Game1.mouseCursors);
        WindowBorderSourceRect = theme?.WindowBorderSourceRect ?? new Rectangle(384, 373, 18, 18);
        PortraitBackgroundTexture = LoadThemeTexture(theme?.PortraitBackgroundTexture, Game1.mouseCursors);
        PortraitBackgroundSourceRect = theme?.PortraitBackgroundSourceRect ?? new Rectangle(603, 414, 74, 74);
        DialogueBackgroundTexture = LoadThemeTexture(theme?.DialogueBackgroundTexture, Game1.menuTexture);
        DialogueBackgroundSourceRect = theme?.DialogueBackgroundSourceRect ?? new Rectangle(0, 256, 60, 60);
        DialogueColor = Utility.StringToColor(theme?.DialogueColor);
        DialogueShadowColor = Utility.StringToColor(theme?.DialogueShadowColor);
        ItemRowBackgroundTexture = LoadThemeTexture(theme?.ItemRowBackgroundTexture, Game1.mouseCursors);
        ItemRowBackgroundSourceRect = theme?.ItemRowBackgroundSourceRect ?? new Rectangle(384, 396, 15, 15);
        ItemRowBackgroundHoverColor = Utility.StringToColor(theme?.ItemRowBackgroundHoverColor) ?? Color.Wheat;
        ItemRowTextColor = Utility.StringToColor(theme?.ItemRowTextColor);
        ItemIconBackgroundTexture = LoadThemeTexture(theme?.ItemIconBackgroundTexture, Game1.mouseCursors);
        ItemIconBackgroundSourceRect = theme?.ItemIconBackgroundSourceRect ?? new Rectangle(296, 363, 18, 18);
        ScrollUpTexture = LoadThemeTexture(theme?.ScrollUpTexture, Game1.mouseCursors);
        ScrollUpSourceRect = theme?.ScrollUpSourceRect ?? new Rectangle(421, 459, 11, 12);
        ScrollDownTexture = LoadThemeTexture(theme?.ScrollDownTexture, Game1.mouseCursors);
        ScrollDownSourceRect = theme?.ScrollDownSourceRect ?? new Rectangle(421, 472, 11, 12);
        ScrollBarFrontTexture = LoadThemeTexture(theme?.ScrollBarFrontTexture, Game1.mouseCursors);
        ScrollBarFrontSourceRect = theme?.ScrollBarFrontSourceRect ?? new Rectangle(435, 463, 6, 10);
        ScrollBarBackTexture = LoadThemeTexture(theme?.ScrollBarBackTexture, Game1.mouseCursors);
        ScrollBarBackSourceRect = theme?.ScrollBarBackSourceRect ?? new Rectangle(403, 383, 6, 6);
    }

    /// <summary>Load a theme texture if it's non-null and exists, else get the default texture.</summary>
    /// <param name="customTextureName">The custom texture asset name to load.</param>
    /// <param name="defaultTexture">The default texture.</param>
    private Texture2D LoadThemeTexture(string? customTextureName, Texture2D defaultTexture)
    {
        if (customTextureName == null || !Game1.content.DoesAssetExist<Texture2D>(customTextureName))
        {
            return defaultTexture;
        }
        return Game1.content.Load<Texture2D>(customTextureName);
    }
}
