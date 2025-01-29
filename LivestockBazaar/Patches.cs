using System.Reflection.Emit;
using HarmonyLib;
using Netcode;
using StardewModdingAPI;
using StardewValley;

namespace LivestockBazaar;

internal static class Patches
{
    public static void Patch(Harmony patcher)
    {
        try
        {
            patcher.Patch(
                original: AccessTools.DeclaredMethod(typeof(AnimalHouse), nameof(AnimalHouse.adoptAnimal)),
                transpiler: new HarmonyMethod(typeof(Patches), nameof(AnimalHouse_adoptAnimal_Transpiler))
            );
        }
        catch (Exception err)
        {
            ModEntry.Log($"Failed to patch LivestockBazaar:\n{err}", LogLevel.Error);
        }
    }

    /// <summary>
    /// Fixes the adoptAnimal conversation topic
    /// </summary>
    /// <param name="instructions"></param>
    /// <param name="generator"></param>
    /// <returns></returns>
    private static IEnumerable<CodeInstruction> AnimalHouse_adoptAnimal_Transpiler(
        IEnumerable<CodeInstruction> instructions,
        ILGenerator generator
    )
    {
        try
        {
            CodeMatcher matcher = new(instructions, generator);

            // IL_0055: ldarg.1
            // IL_0056: callvirt instance string StardewValley.FarmAnimal::get_displayType()

            // IL_0006: ldfld class Netcode.NetString StardewValley.FarmAnimal::'type'
            // IL_000b: callvirt instance !0 class Netcode.NetFieldBase`2<string, class Netcode.NetString>::get_Value()
            matcher
                .MatchEndForward(
                    [
                        new(OpCodes.Ldarg_1),
                        new(
                            OpCodes.Callvirt,
                            AccessTools.PropertyGetter(typeof(FarmAnimal), nameof(FarmAnimal.displayType))
                        ),
                    ]
                )
                .InsertAndAdvance([new(OpCodes.Ldfld, AccessTools.Field(typeof(FarmAnimal), nameof(FarmAnimal.type)))])
                .SetOperandAndAdvance(AccessTools.PropertyGetter(typeof(NetString), nameof(NetString.Value)));

            return matcher.Instructions();
        }
        catch (Exception err)
        {
            ModEntry.Log($"Error in AnimalHouse_adoptAnimal_Transpiler:\n{err}", LogLevel.Error);
            return instructions;
        }
    }
}
