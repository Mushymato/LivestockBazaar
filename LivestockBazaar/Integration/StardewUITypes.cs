using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;

namespace LivestockBazaar.Integration;

/// <summary>Duck types for StardewUI</summary>

public record SDUIEdges(int Left, int Top, int Right, int Bottom)
{
    public static readonly SDUIEdges NONE = new(0, 0, 0, 0);

    public SDUIEdges(int all)
        : this(all, all, all, all) { }

    public SDUIEdges(int horizontal, int vertical)
        : this(horizontal, vertical, horizontal, vertical) { }

    public static SDUIEdges operator *(float mult, SDUIEdges edges) =>
        new((int)(edges.Left * mult), (int)(edges.Top * mult), (int)(edges.Right * mult), (int)(edges.Bottom * mult));

    public static SDUIEdges operator *(SDUIEdges edges, float mult) => mult * edges;
}

public enum SDUISliceCenterPosition
{
    Start,
    End,
}

public record SDUISliceSettings(
    int? CenterX = null,
    SDUISliceCenterPosition CenterXPosition = SDUISliceCenterPosition.Start,
    int? CenterY = null,
    SDUISliceCenterPosition CenterYPosition = SDUISliceCenterPosition.Start,
    float Scale = 4,
    bool EdgesOnly = false
);

public record SDUISprite(
    Texture2D Texture,
    Rectangle? SourceRect = null,
    SDUIEdges? FixedEdges = null,
    SDUISliceSettings? SliceSettings = null
)
{
    public SDUISprite(Texture2D Texture)
        : this(Texture, Texture.Bounds, SDUIEdges.NONE, new()) { }

    public SDUISprite(Texture2D Texture, Rectangle SourceRect)
        : this(Texture, SourceRect, SDUIEdges.NONE, new()) { }
};

/// <summary>
/// Cardinal directions used in UI, matching gamepad stick/button directions for navigation.
/// </summary>
public enum SDUIDirection
{
    /// <summary>
    /// "Up" in screen space.
    /// </summary>
    North = 0,

    /// <summary>
    /// "Right" in screen space.
    /// </summary>
    East,

    /// <summary>
    /// "Down" in screen space.
    /// </summary>
    South,

    /// <summary>
    /// "Left" in screen space.
    /// </summary>
    West,
}
