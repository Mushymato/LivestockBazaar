using System.Diagnostics.CodeAnalysis;
using Microsoft.Xna.Framework;
using StardewModdingAPI;

namespace LivestockBazaar.Integration;

public interface IModNameInfo
{
    /// <summary>The mod's unique id</summary>
    string ModId { get; }

    /// <summary>The mod info, if this entry matches a real mod</summary>
    IModInfo? ModInfo { get; }

    /// <summary>The mod's name, derived from either the manifest or the special translation asset</summary>
    string ModName { get; }

    /// <summary>Display color for this mod's name</summary>
    Color ModNameColor { get; }
}

public interface IModNameAPI
{
    /// <summary>
    /// Try and get info about which mod added a farm animal using the farm animal type
    /// </summary>
    /// <param name="farmAnimalType">The farm animal type</param>
    /// <param name="modName">A <see cref="IModNameInfo"/> record containing info about the mod.</param>
    /// <returns>True if the mod is found</returns>
    bool TryGetModName_FromFarmAnimalType(string farmAnimalType, [NotNullWhen(true)] out IModNameInfo? modName);
}

public static class IModNameAPIExtension
{
    internal static IModNameInfo? GetModName_FromFarmAnimalType(this IModNameAPI api, string key)
    {
        if (api.TryGetModName_FromFarmAnimalType(key, out IModNameInfo? modName))
            return modName;
        return null;
    }
}
