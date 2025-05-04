using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI.Events;

namespace LivestockBazaar.Integration;

/// <summary>
/// A tab context menu event is emitted whenever the user right-clicks
/// on a tab in the game menu.
/// </summary>
public interface ITabContextMenuEvent
{
    /// <summary>
    /// The id of the tab the context menu is opening for.
    /// </summary>
    string Tab { get; }

    /// <summary>
    /// A list of context menu entries to display. You can freely add
    /// or remove entries. An entry with a null <c>OnSelect</c> will
    /// appear disabled.
    /// </summary>
    IList<ITabContextMenuEntry> Entries { get; }

    /// <summary>
    /// Create a new context menu entry.
    /// </summary>
    /// <param name="label">The string to display.</param>
    /// <param name="onSelect">The action to perform when this entry is selected.</param>
    /// <param name="icon">An icon to display alongside this entry.</param>
    public ITabContextMenuEntry CreateEntry(
        string label,
        Action? onSelect,
        IBetterGameMenuApi.DrawDelegate? icon = null
    );
}

/// <summary>
/// An entry in a tab context menu.
/// </summary>
public interface ITabContextMenuEntry
{
    /// <summary>The string to display for this entry.</summary>
    string Label { get; }

    /// <summary>
    /// An action to perform when this entry is selected. If this is
    /// <c>null</c>, the entry will be disabled.
    /// </summary>
    Action? OnSelect { get; }

    /// <summary>An optional icon to display to the left of this entry.</summary>
    IBetterGameMenuApi.DrawDelegate? Icon { get; }
}

/// <summary>
/// This enum is included for reference and has the order value for
/// all the default tabs from the base game. These values are intentionally
/// spaced out to allow for modded tabs to be inserted at specific points.
/// </summary>
public enum VanillaTabOrders
{
    Inventory = 0,
    Skills = 20,
    Social = 40,
    Map = 60,
    Crafting = 80,
    Animals = 100,
    Powers = 120,
    Collections = 140,
    Options = 160,
    Exit = 200,
}

public interface IBetterGameMenuApi
{
    /// <summary>
    /// A delegate for drawing something onto the screen.
    /// </summary>
    /// <param name="batch">The <see cref="SpriteBatch"/> to draw with.</param>
    /// <param name="bounds">The region where the thing should be drawn.</param>
    public delegate void DrawDelegate(SpriteBatch batch, Rectangle bounds);

    public delegate void TabContextMenuDelegate(ITabContextMenuEvent evt);

    /// <summary>
    /// This event fires whenever the user opens a context menu for a menu tab
    /// by right-clicking on it.
    /// </summary>
    void OnTabContextMenu(TabContextMenuDelegate handler, EventPriority priority = EventPriority.Normal);

    /// <summary>
    /// Unregister a handler for the TabContextMenu event.
    /// </summary>
    void OffTabContextMenu(TabContextMenuDelegate handler);
}
