using StardewModdingAPI;

namespace LivestockBazaar;

internal sealed class ModConfig
{
    /// <summary>Do not override marnie's stock and shop menu</summary>
    public bool VanillaMarnieStock { get; set; } = false;

    /// <summary>Use vanilla livestock shop menu for custom shops</summary>
    public bool VanillaLivestockMenu { get; set; } = false;

    /// <summary>Restore default config values</summary>
    private void Reset()
    {
        VanillaMarnieStock = false;
        VanillaLivestockMenu = false;
    }

    /// <summary>Add mod config to GMCM if available</summary>
    /// <param name="helper"></param>
    /// <param name="mod"></param>
    public void Register(IModHelper helper, IManifest mod)
    {
        Integration.IGenericModConfigMenuApi? GMCM = helper.ModRegistry.GetApi<Integration.IGenericModConfigMenuApi>("spacechase0.GenericModConfigMenu");
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
            setValue: (value) => VanillaMarnieStock = value,
            name: I18n.Config_VanillaMarnieStock_Name,
            tooltip: I18n.Config_VanillaMarnieStock_Tooltip
        );
        GMCM.AddBoolOption(
            mod: mod,
            getValue: () => VanillaLivestockMenu,
            setValue: (value) => VanillaLivestockMenu = value,
            name: I18n.Config_VanillaLivestockMenu_Name,
            tooltip: I18n.Config_VanillaLivestockMenu_Tooltip
        );
    }
}
