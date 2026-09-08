using System;
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

    public static List<TradeRouteItem> Routes
    {
        get
        {
            try
            {
                var json = PlayerConfig.Get("RSBot.Trade.RoutesJson", "");
                if (string.IsNullOrWhiteSpace(json))
                    return new List<TradeRouteItem>();

                return System.Text.Json.JsonSerializer.Deserialize<List<TradeRouteItem>>(json) ?? new List<TradeRouteItem>();
            }
            catch
            {
                return new List<TradeRouteItem>();
            }
        }
        set
        {
            try
            {
                var json = System.Text.Json.JsonSerializer.Serialize(value);
                PlayerConfig.Set("RSBot.Trade.RoutesJson", json);
            }
            catch { }
        }
    }

    public static bool RepeatLoop
    {
        get => PlayerConfig.Get("RSBot.Trade.RepeatLoop", true);
        set => PlayerConfig.Set("RSBot.Trade.RepeatLoop", value);
    }

    public static int RepeatTimes
    {
        get => PlayerConfig.Get("RSBot.Trade.RepeatTimes", 0);
        set => PlayerConfig.Set("RSBot.Trade.RepeatTimes", value);
    }

    public static bool DisableAutoActivation
    {
        get => PlayerConfig.Get("RSBot.Trade.DisableAutoActivation", false);
        set => PlayerConfig.Set("RSBot.Trade.DisableAutoActivation", value);
    }

    public static bool ReturnScrollAfterLoop
    {
        get => PlayerConfig.Get("RSBot.Trade.ReturnScrollAfterLoop", false);
        set => PlayerConfig.Set("RSBot.Trade.ReturnScrollAfterLoop", value);
    }

    public static bool UnequipJobSuitAfterLoop
    {
        get => PlayerConfig.Get("RSBot.Trade.UnequipJobSuitAfterLoop", false);
        set => PlayerConfig.Set("RSBot.Trade.UnequipJobSuitAfterLoop", value);
    }

    public static bool SkipTownScripts
    {
        get => PlayerConfig.Get("RSBot.Trade.SkipTownScripts", false);
        set => PlayerConfig.Set("RSBot.Trade.SkipTownScripts", value);
    }

    public static string SelectedTransport
    {
        get => PlayerConfig.Get("RSBot.Trade.SelectedTransport", "Auto");
        set => PlayerConfig.Set("RSBot.Trade.SelectedTransport", value);
    }

    public static string ResolveTradeScript(string startCity, string endCity)
    {
        if (string.IsNullOrWhiteSpace(startCity) || string.IsNullOrWhiteSpace(endCity))
            return string.Empty;

        var s = startCity.Trim().ToLowerInvariant();
        var e = endCity.Trim().ToLowerInvariant();

        if (s == "donwhang" || s == "downhang") s = "downhang";
        if (e == "donwhang" || e == "downhang") e = "downhang";
        if (s == "constantinople" || s == "costantinople") s = "costantinople";
        if (e == "constantinople" || e == "costantinople") e = "costantinople";

        var searchDirs = new[]
        {
            Path.Combine(Kernel.BasePath, "Data", "Scripts", "Trade"),
            Path.Combine(Kernel.BasePath, "Dependencies", "Scripts", "Trade"),
            @"E:\RSBot-2.9.5\Scripts\Scripts\Trade"
        };

        foreach (var dir in searchDirs)
        {
            if (!Directory.Exists(dir)) continue;

            var files = Directory.GetFiles(dir, "*.vb");
            foreach (var file in files)
            {
                var fname = Path.GetFileNameWithoutExtension(file).ToLowerInvariant();
                if (fname == $"{s} to {e}" || fname == $"{s}to{e}")
                    return file;
            }

            foreach (var file in files)
            {
                var fname = Path.GetFileNameWithoutExtension(file).ToLowerInvariant();
                var idxS = fname.IndexOf(s, System.StringComparison.Ordinal);
                var idxE = fname.IndexOf(e, System.StringComparison.Ordinal);
                if (idxS >= 0 && idxE >= 0 && idxS < idxE)
                    return file;
            }
        }

        return string.Empty;
    }

    /// <summary>
    ///     Generates a reverse trade script by reversing walk waypoints and swapping buy/sell NPCs.
    /// </summary>
    public static string GenerateReverseScript(string scriptPath, string startCity = "", string endCity = "")
    {
        if (string.IsNullOrEmpty(scriptPath) || !File.Exists(scriptPath))
            return string.Empty;

        try
        {
            var lines = File.ReadAllLines(scriptPath);
            if (lines.Length == 0) return string.Empty;

            var moveCommands = new List<string>();
            string buyCommand = null;
            string sellCommand = null;

            foreach (var line in lines)
            {
                var trimmed = line.Trim();
                if (string.IsNullOrWhiteSpace(trimmed) || trimmed.StartsWith("//") || trimmed.StartsWith("#"))
                    continue;

                if (trimmed.StartsWith("move", StringComparison.OrdinalIgnoreCase))
                    moveCommands.Add(trimmed);
                else if (trimmed.StartsWith("buy-goods", StringComparison.OrdinalIgnoreCase))
                    buyCommand = trimmed;
                else if (trimmed.StartsWith("sell-goods", StringComparison.OrdinalIgnoreCase))
                    sellCommand = trimmed;
            }

            if (moveCommands.Count == 0)
                return string.Empty;

            // Reverse waypoints
            moveCommands.Reverse();

            var reversedLines = new List<string>();
            string newBuyCommand = null;
            string newSellCommand = null;

            if (!string.IsNullOrEmpty(sellCommand))
            {
                var parts = sellCommand.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length >= 2)
                    newBuyCommand = $"buy-goods {parts[1]} Full";
            }

            if (!string.IsNullOrEmpty(buyCommand))
            {
                var parts = buyCommand.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length >= 2)
                    newSellCommand = $"sell-goods {parts[1]}";
            }

            var insertBuyIndex = Math.Min(10, moveCommands.Count / 10);
            for (int i = 0; i < moveCommands.Count; i++)
            {
                reversedLines.Add(moveCommands[i]);
                if (i == insertBuyIndex && !string.IsNullOrEmpty(newBuyCommand))
                {
                    reversedLines.Add("summon-transport");
                    reversedLines.Add(newBuyCommand);
                }
            }

            if (!string.IsNullOrEmpty(newSellCommand))
                reversedLines.Add(newSellCommand);

            var dir = Path.GetDirectoryName(scriptPath);
            var baseName = Path.GetFileNameWithoutExtension(scriptPath);
            string reverseFileName;

            if (!string.IsNullOrEmpty(startCity) && !string.IsNullOrEmpty(endCity))
                reverseFileName = $"{endCity} to {startCity}.vb";
            else
                reverseFileName = $"{baseName}_reversed.vb";

            var reversePath = Path.Combine(dir, reverseFileName);
            File.WriteAllLines(reversePath, reversedLines);
            Log.Notify($"[Trade] Auto-generated reverse trade script: {reverseFileName}");

            return reversePath;
        }
        catch (Exception ex)
        {
            Log.Warn($"[Trade] Could not generate reverse script: {ex.Message}");
            return string.Empty;
        }
    }
}

public class TradeRouteItem
{
    public int Index { get; set; }
    public string StartCity { get; set; }
    public string EndCity { get; set; }
    public string ScriptFile { get; set; }
    public bool Active { get; set; }
    public int LoopCount { get; set; } = 1;
}