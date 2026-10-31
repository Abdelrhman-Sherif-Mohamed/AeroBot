using System.Collections.Generic;
using System.IO;
using System.Linq;
using RSBot.Core;

namespace RSBot.Trade.Components;

internal static class TradeConfig
{
    public static string TracePlayerName
    {
        get => PlayerConfig.Get("RSBot.Trade.TracePlayerName", "");
        set => PlayerConfig.Set("RSBot.Trade.TracePlayerName", value);
    }

    public static bool TracePlayer
    {
        get => PlayerConfig.Get("RSBot.Trade.TracePlayer", false);
        set => PlayerConfig.Set("RSBot.Trade.TracePlayer", value);
    }

    public static bool UseCityToCity
    {
        get => PlayerConfig.Get("RSBot.Trade.UseCityToCity", true);
        set => PlayerConfig.Set("RSBot.Trade.UseCityToCity", value);
    }

    public static string StartCity
    {
        get => PlayerConfig.Get("RSBot.Trade.StartCity", "Auto-Detect");
        set => PlayerConfig.Set("RSBot.Trade.StartCity", value);
    }

    public static string DestinationCity
    {
        get => PlayerConfig.Get("RSBot.Trade.DestinationCity", "Donwhang");
        set => PlayerConfig.Set("RSBot.Trade.DestinationCity", value);
    }

    public static int TradeLoops
    {
        get => PlayerConfig.Get("RSBot.Trade.TradeLoops", 0);
        // 0 means infinite loops, just like vBot.
        set => PlayerConfig.Set("RSBot.Trade.TradeLoops", System.Math.Clamp(value, 0, 999));
    }

    /// <summary>
    ///     Whether repeat trade loop is enabled.
    /// </summary>
    public static bool RepeatTradeLoop
    {
        get => PlayerConfig.Get("RSBot.Trade.RepeatTradeLoop", true);
        set => PlayerConfig.Set("RSBot.Trade.RepeatTradeLoop", value);
    }

    /// <summary>
    ///     Dismount transport immediately when attacking or engaged with a thief.
    /// </summary>
    public static bool DismountToAttackThief
    {
        get => PlayerConfig.Get("RSBot.Trade.DismountToAttackThief", false);
        set => PlayerConfig.Set("RSBot.Trade.DismountToAttackThief", value);
    }

    /// <summary>
    ///     Dismount, take off Job outfit, and switch to default bot when loops complete.
    /// </summary>
    public static bool DismountTakeOffJobAndStartDefault
    {
        get => PlayerConfig.Get("RSBot.Trade.DismountTakeOffJobAndStartDefault", false);
        set => PlayerConfig.Set("RSBot.Trade.DismountTakeOffJobAndStartDefault", value);
    }

    public static int CurrentTradeLoop
    {
        get => PlayerConfig.Get("RSBot.Trade.CurrentTradeLoop", 0);
        set => PlayerConfig.Set("RSBot.Trade.CurrentTradeLoop", value);
    }

    public static bool SmartObstacleEvasion
    {
        get => PlayerConfig.Get("RSBot.Trade.SmartObstacleEvasion", true);
        set => PlayerConfig.Set("RSBot.Trade.SmartObstacleEvasion", value);
    }

    public static bool SmartThiefCombat
    {
        get => PlayerConfig.Get("RSBot.Trade.SmartThiefCombat", true);
        set => PlayerConfig.Set("RSBot.Trade.SmartThiefCombat", value);
    }

    public static bool SkipNpcIfHasGoods
    {
        get => PlayerConfig.Get("RSBot.Trade.SkipNpcIfHasGoods", true);
        set => PlayerConfig.Set("RSBot.Trade.SkipNpcIfHasGoods", value);
    }

    public static bool MountTransport
    {
        get => PlayerConfig.Get("RSBot.Trade.MountTransport", false);
        set => PlayerConfig.Set("RSBot.Trade.MountTransport", value);
    }

    public static bool UseRouteScripts
    {
        get => PlayerConfig.Get("RSBot.Trade.UseRouteScripts", true);
        set => PlayerConfig.Set("RSBot.Trade.UseRouteScripts", value);
    }

    public static int SelectedRouteListIndex
    {
        get => PlayerConfig.Get("RSBot.Trade.SelectedRouteListIndex", 0);
        set => PlayerConfig.Set("RSBot.Trade.SelectedRouteListIndex", value);
    }

    public static bool RunTownScript
    {
        get => PlayerConfig.Get("RSBot.Trade.RunTownScript", false);
        set => PlayerConfig.Set("RSBot.Trade.RunTownScript", value);
    }

    /// <summary>
    ///     Skip the town script entirely. This is the inverse of the old
    ///     RunTownScript option and is the value used by the new UI.
    /// </summary>
    public static bool SkipTownScript
    {
        get => PlayerConfig.Get("RSBot.Trade.SkipTownScript",
            !PlayerConfig.Get("RSBot.Trade.RunTownScript", false));
        set => PlayerConfig.Set("RSBot.Trade.SkipTownScript", value);
    }

    /// <summary>
    ///     Automatically start the trade bot after the character finished
    ///     loading (login / return to character selection).
    /// </summary>
    public static bool AutoStartAfterLogin
    {
        get => PlayerConfig.Get("RSBot.Trade.AutoStartAfterLogin", false);
        set => PlayerConfig.Set("RSBot.Trade.AutoStartAfterLogin", value);
    }

    /// <summary>
    ///     Use a return scroll after a trade loop has been completed.
    /// </summary>
    public static bool UseReturnScrollAfterLoop
    {
        get => PlayerConfig.Get("RSBot.Trade.UseReturnScrollAfterLoop", false);
        set => PlayerConfig.Set("RSBot.Trade.UseReturnScrollAfterLoop", value);
    }

    /// <summary>
    ///     The maximum distance (in meters) used when searching for thieves.
    /// </summary>
    public static int MaxAttackDistance
    {
        get => PlayerConfig.Get("RSBot.Trade.MaxAttackDistance", 50);
        set => PlayerConfig.Set("RSBot.Trade.MaxAttackDistance", System.Math.Clamp(value, 5, 200));
    }

    /// <summary>
    ///     Transport handling mode: "Auto" summons the transport, "Manual"
    ///     waits for the player to summon it.
    /// </summary>
    public static string TransportMode
    {
        get => PlayerConfig.Get("RSBot.Trade.TransportMode", "Manual");
        set => PlayerConfig.Set("RSBot.Trade.TransportMode", value);
    }

    /// <summary>
    ///     Returns whether the given route script is enabled in the route table.
    /// </summary>
    public static bool IsRouteActive(string fileName)
    {
        return PlayerConfig.Get($"RSBot.Trade.RouteActive.{fileName}", true);
    }

    /// <summary>
    ///     Enables or disables the given route script.
    /// </summary>
    public static void SetRouteActive(string fileName, bool active)
    {
        PlayerConfig.Set($"RSBot.Trade.RouteActive.{fileName}", active);
    }

    /// <summary>
    ///     Gets the planned loop count for a route script.
    /// </summary>
    public static int GetRouteLoop(string fileName)
    {
        return PlayerConfig.Get($"RSBot.Trade.RouteLoop.{fileName}", 1);
    }

    /// <summary>
    ///     Sets the planned loop count for a route script.
    /// </summary>
    public static void SetRouteLoop(string fileName, int loops)
    {
        PlayerConfig.Set($"RSBot.Trade.RouteLoop.{fileName}", System.Math.Clamp(loops, 1, 999));
    }

    /// <summary>
    ///     Removes a route list from the configuration.
    /// </summary>
    public static void RemoveRouteList(string name)
    {
        var lists = RouteScriptList;
        if (!lists.Remove(name))
            return;

        RouteScriptList = lists;
    }

    public static bool WaitForHunter
    {
        get => PlayerConfig.Get("RSBot.Trade.WaitForHunter", false);
        set => PlayerConfig.Set("RSBot.Trade.WaitForHunter", value);
    }

    public static bool AttackThiefPlayers
    {
        get => PlayerConfig.Get("RSBot.Trade.AttackThiefPlayers", false);
        set => PlayerConfig.Set("RSBot.Trade.AttackThiefPlayers", value);
    }

    public static bool AttackThiefNpcs
    {
        get => PlayerConfig.Get("RSBot.Trade.AttackThiefNpcs", false);
        set => PlayerConfig.Set("RSBot.Trade.AttackThiefNpcs", value);
    }

    public static bool CastBuffs
    {
        get => PlayerConfig.Get("RSBot.Trade.CastBuffs", false);
        set => PlayerConfig.Set("RSBot.Trade.CastBuffs", value);
    }

    public static bool CounterAttack
    {
        get => PlayerConfig.Get("RSBot.Trade.CounterAttack", false);
        set => PlayerConfig.Set("RSBot.Trade.CounterAttack", value);
    }

    public static bool ProtectTransport
    {
        get => PlayerConfig.Get("RSBot.Trade.ProtectTransport", false);
        set => PlayerConfig.Set("RSBot.Trade.ProtectTransport", value);
    }

    public static bool BuyGoods
    {
        get => PlayerConfig.Get("RSBot.Trade.BuyGoods", true);
        set => PlayerConfig.Set("RSBot.Trade.BuyGoods", value);
    }

    public static bool SellGoods
    {
        get => PlayerConfig.Get("RSBot.Trade.SellGoods", true);
        set => PlayerConfig.Set("RSBot.Trade.SellGoods", value);
    }

    public static int BuyGoodsQuantity
    {
        get => PlayerConfig.Get("RSBot.Trade.BuyGoodsQuantity", 0);
        set => PlayerConfig.Set("RSBot.Trade.BuyGoodsQuantity", value);
    }

    public static int MaxTransportDistance
    {
        get => PlayerConfig.Get("RSBot.Trade.MaxTransportDistance", 15);
        set => PlayerConfig.Set("RSBot.Trade.MaxTransportDistance", value == 0 ? 1 : value);
    }

    public static List<string> RouteScriptList
    {
        get
        {
            var result = PlayerConfig.GetArray<string>("RSBot.Trade.RouteScriptList", ';').ToList();

            if (!result.Contains("Default"))
                result.Add("Default");

            return result;
        }
        set => PlayerConfig.SetArray("RSBot.Trade.RouteScriptList", value, ";");
    }


    public static Dictionary<string, List<string>> RouteScripts
    {
        get
        {
            var result = new Dictionary<string, List<string>>(16);

            foreach (var scriptList in RouteScriptList)
            {
                var scripts = PlayerConfig.GetArray<string>($"RSBot.Trade.RouteScriptList.{scriptList}")
                                  .Where(File.Exists).ToList() ??
                              new List<string>();

                result.Add(scriptList, scripts);
            }

            return result;
        }
        set
        {
            foreach (var scriptList in value)
                PlayerConfig.SetArray($"RSBot.Trade.RouteScriptList.{scriptList.Key}", scriptList.Value);

            RouteScriptList = value.Keys.ToList();
        }
    }
}