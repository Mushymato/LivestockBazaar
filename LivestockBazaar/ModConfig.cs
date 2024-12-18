using StardewModdingAPI;

namespace LivestockBazaar;

public enum LivestockSortMode
{
    Name,
    Price,
    House,
}

public static class LivestockSortModeExtension
{
    static readonly LivestockSortMode minSortMode = (LivestockSortMode)
        Enum.GetValues(typeof(LivestockSortMode)).Cast<int>().Min();
    static readonly LivestockSortMode maxSortMode = (LivestockSortMode)
        Enum.GetValues(typeof(LivestockSortMode)).Cast<int>().Max();

    public static LivestockSortMode Next(this LivestockSortMode mode)
    {
        if (mode == maxSortMode)
            return minSortMode;
        return mode + 1;
    }
}

internal sealed class ModConfig
{
    /// <summary>Do not override marnie's stock and shop menu</summary>
    public bool VanillaMarnieStock { get; set; } = false;

    /// <summary>Sort mode for livestock, normally changed in the shop UI</summary>
    public LivestockSortMode SortMode { get; set; } = LivestockSortMode.Name;

    /// <summary>Sort mode asc/desc, normally changed in the shop UI</summary>
    public bool SortIsAsc { get; set; } = true;

    /// <summary>Restore default config values</summary>
    private void Reset()
    {
        VanillaMarnieStock = false;
    }

    /// <summary>Add mod config to GMCM if available</summary>
    /// <param name="helper"></param>
    /// <param name="mod"></param>
    public void Register(IModHelper helper, IManifest mod)
    {
        Integration.IGenericModConfigMenuApi? GMCM = helper.ModRegistry.GetApi<Integration.IGenericModConfigMenuApi>(
            "spacechase0.GenericModConfigMenu"
        );
        if (GMCM == null)
        {
            helper.WriteConfig(this);
            return;
        }
        GMCM.Register(
            mod: mod,
            reset: () =>
            {
                Reset();
                helper.WriteConfig(this);
            },
            save: () =>
            {
                helper.WriteConfig(this);
            },
            titleScreenOnly: false
        );
        GMCM.AddBoolOption(
            mod: mod,
            getValue: () => VanillaMarnieStock,
            setValue: (value) =>
            {
                VanillaMarnieStock = value;
                OpenBazaar.UpdateAnimalShop();
            },
            name: I18n.Config_VanillaMarnieStock_Name,
            tooltip: I18n.Config_VanillaMarnieStock_Tooltip
        );
    }
}
