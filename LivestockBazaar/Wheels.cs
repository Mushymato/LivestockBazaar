using StardewValley;
using StardewValley.TokenizableStrings;

namespace LivestockBazaar;

internal static class Wheels
{
    /// <summary>The animal tycoon herself</summary>
    internal const string MARNIE = "Marnie";
    internal static readonly HashSet<string> GSQRandomKeys =
    [
        "RANDOM", /*"SYNCED_RANDOM"*/
    ];

    /// <summary>24hr time format, make it better later</summary>
    /// <param name="timeCode"></param>
    /// <returns></returns>
    internal static string FormatTime(int timeCode)
    {
        int hour = timeCode / 100;
        if (hour > 24)
            hour -= 24;
        return $"{hour:D2}:{timeCode % 100:D2}";
    }

    /// <summary>Show a small pop up for shop open/close times.</summary>
    /// <param name="openTime"></param>
    /// <param name="closeTime"></param>
    internal static void DisplayShopTimes(int openTime, int closeTime)
    {
        string shopClosed;
        if (openTime > -1 && closeTime > 1)
            shopClosed = I18n.Shop_TimeRange(openTime: FormatTime(openTime), closeTime: FormatTime(closeTime));
        else if (openTime > -1)
            shopClosed = I18n.Shop_TimeStart(openTime: FormatTime(openTime));
        else if (closeTime > 1)
            shopClosed = I18n.Shop_TimeEnd(closeTime: FormatTime(closeTime));
        else
            return;
        Game1.drawObjectDialogue(shopClosed);
    }

    /// <summary>
    /// Check the condition but ignore random (<see cref="GSQRandomKeys"/>).
    /// </summary>
    /// <param name="condition"></param>
    /// <returns></returns>
    internal static bool GSQCheckNoRandom(string condition, GameLocation? location = null)
    {
        return GameStateQuery.CheckConditions(condition, location: location, ignoreQueryKeys: GSQRandomKeys);
    }

    internal static bool CanAdoptPets =>
        (Utility.getAllPets().Count == 0 && Game1.year >= 2)
        || Game1.player.mailReceived.Contains("MarniePetAdoption")
        || Game1.player.mailReceived.Contains("MarniePetRejectedAdoption");

    internal static string ParseTextOrDefault(string? tokenText, string? defaultText = "")
    {
        return tokenText == null ? (defaultText ?? "") : TokenParser.ParseText(tokenText);
    }
}
