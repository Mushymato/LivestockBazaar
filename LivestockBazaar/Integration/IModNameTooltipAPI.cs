using System.Diagnostics.CodeAnalysis;
using Microsoft.Xna.Framework;
using StardewValley;

namespace LivestockBazaar.Integration;

public interface IModNameInfo
{
    /// <summary>The mod's unique id</summary>
    string ModId { get; }

    /// <summary>The mod's name, derived from either the manifest or the special translation asset</summary>
    string ModName { get; }

    /// <summary>Display color for this mod's name</summary>
    Color ModNameColor { get; }
}

public interface IModNameTooltip
{
    /// <summary>
    /// Try and get info about which mod added an item using a real item instance
    /// </summary>
    /// <param name="item">Item to find mod name for</param>
    /// <param name="modName">A <see cref="IModNameInfo"/> record containing info about the mod.</param>
    /// <returns>True if the mod is found</returns>
    bool TryGetModName(Item? item, [NotNullWhen(true)] out IModNameInfo? modName);

    /// <summary>
    /// Try and get info about which mod using a real character instance
    /// </summary>
    /// <param name="character">The character to find mod name for</param>
    /// <param name="modName">A <see cref="IModNameInfo"/> record containing info about the mod.</param>
    /// <returns>True if the mod is found</returns>
    bool TryGetModName(Character? character, [NotNullWhen(true)] out IModNameInfo? modName);

    /// <summary>
    /// Try and get info about which mod added an item using the item id
    /// </summary>
    /// <param name="itemId">The item id, qualified or unqualified</param>
    /// <param name="modName">A <see cref="IModNameInfo"/> record containing info about the mod.</param>
    /// <returns>True if the mod is found</returns>
    bool TryGetModName_FromItemId(string itemId, [NotNullWhen(true)] out IModNameInfo? modName);

    /// <summary>
    /// Try and get info about which mod added a farm animal using the farm animal type
    /// </summary>
    /// <param name="farmAnimalType">The farm animal type</param>
    /// <param name="modName">A <see cref="IModNameInfo"/> record containing info about the mod.</param>
    /// <returns>True if the mod is found</returns>
    bool TryGetModName_FromFarmAnimalType(string farmAnimalType, [NotNullWhen(true)] out IModNameInfo? modName);

    /// <summary>
    /// Try and get info about which mod added a farm animal using the farm animal type
    /// </summary>
    /// <param name="npcName">The item id, qualified or unqualified</param>
    /// <param name="modName">A <see cref="IModNameInfo"/> record containing info about the mod.</param>
    /// <returns>True if the mod is found</returns>
    bool TryGetModName_FromNpcName(string npcName, [NotNullWhen(true)] out IModNameInfo? modName);
}
