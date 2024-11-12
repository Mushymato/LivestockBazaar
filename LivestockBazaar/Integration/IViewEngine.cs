﻿using System.ComponentModel;
using System.Runtime.CompilerServices;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley.Menus;

namespace LivestockBazaar.Integration;

/// <summary>
/// Public API for StardewUI, abstracting away all implementation details of views and trees.
/// </summary>
public interface IViewEngine
{
    /// <summary>
    /// Creates an <see cref="IViewDrawable"/> from the StarML stored in a game asset, as provided by a mod via SMAPI or
    /// Content Patcher.
    /// </summary>
    /// <remarks>
    /// The <see cref="IViewDrawable.Context"/> and <see cref="IViewDrawable.MaxSize"/> can be provided after creation.
    /// </remarks>
    /// <param name="assetName">The name of the StarML view asset in the content pipeline, e.g.
    /// <c>Mods/MyMod/Views/MyView</c>.</param>
    /// <returns>An <see cref="IViewDrawable"/> for drawing directly to the <see cref="SpriteBatch"/> of a rendering
    /// event or other draw handler.</returns>
    IViewDrawable CreateDrawableFromAsset(string assetName);

    /// <summary>
    /// Creates an <see cref="IViewDrawable"/> from arbitrary markup.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The <see cref="IViewDrawable.Context"/> and <see cref="IViewDrawable.MaxSize"/> can be provided after creation.
    /// </para>
    /// <para>
    /// <b>Warning:</b> Ad-hoc menus created this way cannot be cached, nor patched by other mods. Most mods should not
    /// use this API except for testing/experimentation.
    /// </para>
    /// </remarks>
    /// <param name="markup">The markup in StarML format.</param>
    /// <returns>An <see cref="IViewDrawable"/> for drawing directly to the <see cref="SpriteBatch"/> of a rendering
    /// event or other draw handler.</returns>
    IViewDrawable CreateDrawableFromMarkup(string markup);

    /// <summary>
    /// Creates a menu from the StarML stored in a game asset, as provided by a mod via SMAPI or Content Patcher.
    /// </summary>
    /// <remarks>
    /// Does not make the menu active. To show it, use <see cref="Game1.activeClickableMenu"/> or equivalent.
    /// </remarks>
    /// <param name="assetName">The name of the StarML view asset in the content pipeline, e.g.
    /// <c>Mods/MyMod/Views/MyView</c>.</param>
    /// <param name="context">The context, or "model", for the menu's view, which holds any data-dependent values.
    /// <b>Note:</b> The type must implement <see cref="INotifyPropertyChanged"/> in order for any changes to this data
    /// to be automatically reflected in the UI.</param>
    /// <returns>A menu object which can be shown using the game's standard menu APIs such as
    /// <see cref="Game1.activeClickableMenu"/>.</returns>
    IClickableMenu CreateMenuFromAsset(string assetName, object? context = null);

    /// <summary>
    /// Creates a menu from arbitrary markup.
    /// </summary>
    /// <remarks>
    /// <b>Warning:</b> Ad-hoc menus created this way cannot be cached, nor patched by other mods. Most mods should not
    /// use this API except for testing/experimentation.
    /// </remarks>
    /// <param name="markup">The markup in StarML format.</param>
    /// <param name="context">The context, or "model", for the menu's view, which holds any data-dependent values.
    /// <b>Note:</b> The type must implement <see cref="INotifyPropertyChanged"/> in order for any changes to this data
    /// to be automatically reflected in the UI.</param>
    /// <returns>A menu object which can be shown using the game's standard menu APIs such as
    /// <see cref="Game1.activeClickableMenu"/>.</returns>
    IClickableMenu CreateMenuFromMarkup(string markup, object? context = null);

    /// <summary>
    /// Starts monitoring this mod's directory for changes to assets managed by any of the <c>Register</c> methods, e.g.
    /// views and sprites.
    /// </summary>
    /// <remarks>
    /// <para>
    /// If the <paramref name="sourceDirectory"/> argument is specified, and points to a directory with the same asset
    /// structure as the mod, then an additional sync will be set up such that files modified in the
    /// <c>sourceDirectory</c> while the game is running will be copied to the active mod directory and subsequently
    /// reloaded. In other words, pointing this at the mod's <c>.csproj</c> directory allows hot reloading from the
    /// source files instead of the deployed mod's files.
    /// </para>
    /// <para>
    /// Hot reload may impact game performance and should normally only be used during development and/or in debug mode.
    /// </para>
    /// </remarks>
    /// <param name="sourceDirectory">Optional source directory to watch and sync changes from. If not specified, or not
    /// a valid source directory, then hot reload will only pick up changes from within the live mod directory.</param>
    void EnableHotReloading(string? sourceDirectory = null);

    /// <summary>
    /// Registers a mod directory to be searched for sprite (and corresponding texture/sprite sheet data) assets.
    /// </summary>
    /// <param name="assetPrefix">The prefix for all asset names, e.g. <c>Mods/MyMod/Sprites</c>. This can be any value
    /// but the same prefix must be used in <c>@AssetName</c> view bindings.</param>
    /// <param name="modDirectory">The physical directory where the asset files are located, relative to the mod
    /// directory. Typically a path such as <c>assets/sprites</c>.</param>
    void RegisterSprites(string assetPrefix, string modDirectory);

    /// <summary>
    /// Registers a mod directory to be searched for view (StarML) assets. Uses the <c>.sml</c> extension.
    /// </summary>
    /// <param name="assetPrefix">The prefix for all asset names, e.g. <c>Mods/MyMod/Views</c>. This can be any value
    /// but the same prefix must be used in <c>include</c> elements and in API calls to create views.</param>
    /// <param name="modDirectory">The physical directory where the asset files are located, relative to the mod
    /// directory. Typically a path such as <c>assets/views</c>.</param>
    public void RegisterViews(string assetPrefix, string modDirectory);
}

/// <summary>
/// Provides methods to update and draw a simple, non-interactive UI component, such as a HUD widget.
/// </summary>
public interface IViewDrawable : IDisposable
{
    /// <summary>
    /// The current size required for the content.
    /// </summary>
    /// <remarks>
    /// Use for calculating the correct position for a <see cref="Draw(SpriteBatch, Vector2)"/>, especially for elements
    /// that should be aligned to the center or right edge of the viewport.
    /// </remarks>
    Vector2 ActualSize { get; }

    /// <summary>
    /// The context, or "model", for the menu's view, which holds any data-dependent values.
    /// </summary>
    /// <remarks>
    /// The type must implement <see cref="INotifyPropertyChanged"/> in order for any changes to this data to be
    /// automatically reflected on the next <see cref="Draw(SpriteBatch, Vector2)"/>.
    /// </remarks>
    object? Context { get; set; }

    /// <summary>
    /// The maximum size, in pixels, allowed for this content.
    /// </summary>
    /// <remarks>
    /// If no value is specified, then the content is allowed to use the entire <see cref="Game1.uiViewport"/>.
    /// </remarks>
    Vector2? MaxSize { get; set; }

    /// <summary>
    /// Draws the current contents.
    /// </summary>
    /// <param name="b">Target sprite batch.</param>
    /// <param name="position">Position on the screen or viewport to use as the top-left corner.</param>
    void Draw(SpriteBatch b, Vector2 position);
}

/// <summary>
/// Extensions for the <see cref="IViewEngine"/> interface.
/// </summary>
internal static class ViewEngineExtensions
{
    /// <summary>
    /// Starts monitoring this mod's directory for changes to assets managed by any of the <see cref="IViewEngine"/>'s
    /// <c>Register</c> methods, e.g. views and sprites, and attempts to set up an additional sync from the mod's
    /// project (source) directory to the deployed mod directory so that hot reloads can be initiated from the IDE.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Callers should normally omit the <paramref name="callerFilePath"/> parameter in their call; this will cause it
    /// to be replaced at compile time with the actual file path of the caller, and used to automatically detect the
    /// project path.
    /// </para>
    /// <para>
    /// If detection/sync fails due to an unusual project structure, consider providing an exact path directly to
    /// <see cref="IViewEngine.EnableHotReloading(string)"/> instead of using this extension.
    /// </para>
    /// <para>
    /// Hot reload may impact game performance and should normally only be used during development and/or in debug mode.
    /// </para>
    /// </remarks>
    /// <param name="viewEngine">The view engine API.</param>
    /// <param name="callerFilePath">Do not pass in this argument, so that <see cref="CallerFilePathAttribute"/> can
    /// provide the correct value on build.</param>
    public static void EnableHotReloadingWithSourceSync(
        this IViewEngine viewEngine,
        [CallerFilePath] string? callerFilePath = null
    )
    {
        viewEngine.EnableHotReloading(FindProjectDirectory(callerFilePath));
    }

    // Attempts to determine the project root directory given the path to an arbitrary source file by walking up the
    // directory tree until it finds a directory containing a file with .csproj extension.
    private static string? FindProjectDirectory(string? sourceFilePath)
    {
        if (string.IsNullOrEmpty(sourceFilePath))
        {
            return null;
        }
        for (var dir = Directory.GetParent(sourceFilePath); dir is not null; dir = dir.Parent)
        {
            if (dir.EnumerateFiles("*.csproj").Any())
            {
                return dir.FullName;
            }
        }
        return null;
    }
}
