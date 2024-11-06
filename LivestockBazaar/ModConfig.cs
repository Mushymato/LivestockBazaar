using StardewModdingAPI;

namespace LivestockBazaar;

internal sealed class ModConfig
{
    /// <summary>Do not override marnie's stock and shop menu</summary>
    public bool VanillaMarnieShop { get; set; } = false;
    /// <summary>Use vanilla shop menu</summary>
    public bool VanillaAnimalShopMenu { get; set; } = false;
    /// <summary>Restore default config values</summary>
    private void Reset()
    {
        VanillaMarnieShop = false;
        VanillaAnimalShopMenu = false;
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
            reset: () => { Reset(); helper.WriteConfig(this); },
            save: () => { helper.WriteConfig(this); },
            titleScreenOnly: false
        );
    }
}
