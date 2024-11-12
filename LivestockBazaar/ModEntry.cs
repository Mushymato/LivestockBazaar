global using SObject = StardewValley.Object;
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
    internal static string ModId = null!;

    public override void Entry(IModHelper helper)
    {
        I18n.Init(helper.Translation);
        mon = Monitor;
        ModId = ModManifest.UniqueID;
        Config = Helper.ReadConfig<ModConfig>();
        // events
        helper.Events.GameLoop.GameLaunched += OnGameLaunched;
        helper.Events.Content.AssetRequested += AssetManager.OnAssetRequested;
        helper.Events.Content.AssetsInvalidated += AssetManager.OnAssetInvalidated;
        // setup bazaar actions
        Harmony harmony = new(ModId);
        OpenBazaar.Register(Helper, harmony);
    }

    /// <summary>Setup config menu</summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void OnGameLaunched(object? sender, GameLaunchedEventArgs e)
    {
        Config.Register(Helper, ModManifest);
        BazaarMenu.Register(Helper);
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
