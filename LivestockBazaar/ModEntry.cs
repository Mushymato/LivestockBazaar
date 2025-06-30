using HarmonyLib;
using LivestockBazaar.GUI;
using StardewModdingAPI;
using StardewModdingAPI.Events;

namespace LivestockBazaar;

public sealed class ModEntry : Mod
{
#if DEBUG
    private const LogLevel DEFAULT_LOG_LEVEL = LogLevel.Debug;
#else
    private const LogLevel DEFAULT_LOG_LEVEL = LogLevel.Trace;
#endif
    private static IMonitor? mon;
    internal static ModConfig Config = null!;
    internal const string ModId = "mushymato.LivestockBazaar";
    internal static Integration.IExtraAnimalConfigApi? EAC = null;

    public override void Entry(IModHelper helper)
    {
        I18n.Init(helper.Translation);
        mon = Monitor;
        Config = Helper.ReadConfig<ModConfig>();
        // events
        helper.Events.GameLoop.GameLaunched += OnGameLaunched;
        helper.Events.GameLoop.SaveLoaded += OnSaveLoaded;
        helper.Events.Content.AssetRequested += AssetManager.OnAssetRequested;
        helper.Events.Content.AssetsInvalidated += AssetManager.OnAssetInvalidated;
        // setup bazaar actions
        OpenBazaar.Register(helper);
        // harmony
        Patches.Patch(new Harmony(ModId));
    }

    /// <summary>Setup config menu</summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void OnGameLaunched(object? sender, GameLaunchedEventArgs e)
    {
        BazaarMenu.Register(Helper);
        Config.Register(Helper, ModManifest);
        EAC = Helper.ModRegistry.GetApi<Integration.IExtraAnimalConfigApi>("selph.ExtraAnimalConfig");
        if (
            Helper.ModRegistry.GetApi<Integration.IBetterGameMenuApi>("leclair.bettergamemenu")
            is Integration.IBetterGameMenuApi BGM
        )
        {
            BazaarMenu.RegisterBGMContextMenu(BGM);
        }
    }

    /// <summary>Warm the cache</summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    /// <exception cref="NotImplementedException"></exception>
    private void OnSaveLoaded(object? sender, SaveLoadedEventArgs e)
    {
        // preload this dict
        var _ = AssetManager.LsData;
    }

    /// <summary>SMAPI static monitor Log wrapper</summary>
    /// <param name="msg"></param>
    /// <param name="level"></param>
    internal static void Log(string msg, LogLevel level = DEFAULT_LOG_LEVEL)
    {
        mon!.Log(msg, level);
    }

    /// <summary>SMAPI static monitor LogOnce wrapper</summary>
    /// <param name="msg"></param>
    /// <param name="level"></param>
    internal static void LogOnce(string msg, LogLevel level = DEFAULT_LOG_LEVEL)
    {
        mon!.LogOnce(msg, level);
    }
}
