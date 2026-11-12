using System;
using System.Collections.Generic;
using RSBot.Core;
using RSBot.Core.Event;

namespace RSBot.Trade.Components;

/// <summary>
///     Tracks live caravan statistics (runs, run durations, gold profit, and gold/hour)
///     and emits trade lifecycle events for notification plugins.
/// </summary>
internal static class TradeStats
{
    private static readonly object _sync = new();
    private static readonly List<long> _runDurationsMs = new();
    private static DateTime? _sessionStartedAtUtc;
    private static DateTime? _currentRunStartedAtUtc;
    private static ulong? _baselineGold;

    public static int Runs { get; private set; }
    public static long BestRunMs { get; private set; }
    public static long AvgRunMs { get; private set; }

    public static void OnSessionStarted()
    {
        lock (_sync)
        {
            _sessionStartedAtUtc ??= DateTime.UtcNow;
            if (!_baselineGold.HasValue && Game.Player != null)
                _baselineGold = Game.Player.Gold;
        }
    }

    public static void OnRunStarted()
    {
        OnSessionStarted();
        lock (_sync)
        {
            _currentRunStartedAtUtc = DateTime.UtcNow;
        }

        EventManager.FireEvent("OnTradeRunStarted");
    }

    public static void OnRunSettled()
    {
        long durationMs = 0;
        long profitGold;
        int runs;

        lock (_sync)
        {
            _sessionStartedAtUtc ??= DateTime.UtcNow;
            if (!_baselineGold.HasValue && Game.Player != null)
                _baselineGold = Game.Player.Gold;

            if (_currentRunStartedAtUtc.HasValue)
            {
                durationMs = Math.Max(0, (long)(DateTime.UtcNow - _currentRunStartedAtUtc.Value).TotalMilliseconds);
                _currentRunStartedAtUtc = null;
                _runDurationsMs.Add(durationMs);
                BestRunMs = BestRunMs == 0 ? durationMs : Math.Min(BestRunMs, durationMs);
                AvgRunMs = Sum(_runDurationsMs) / _runDurationsMs.Count;
            }

            Runs++;
            runs = Runs;
            profitGold = ComputeProfitGold();
        }

        EventManager.FireEvent("OnTradeRunSettled", runs, durationMs, profitGold);
    }

    public static void Reset()
    {
        lock (_sync)
        {
            Runs = 0;
            BestRunMs = 0;
            AvgRunMs = 0;
            _runDurationsMs.Clear();
            _sessionStartedAtUtc = TradeBotbase.IsActive ? DateTime.UtcNow : null;
            _currentRunStartedAtUtc = null;
            _baselineGold = Game.Player?.Gold;
        }
    }

    public static string FormatSummary(bool isArabic)
    {
        lock (_sync)
        {
            var avg = FormatDuration(AvgRunMs);
            var best = BestRunMs > 0 ? FormatDuration(BestRunMs) : "--";
            var profit = ComputeProfitGold();
            var goldPerHour = ComputeGoldPerHour(profit);

            return isArabic
                ? $"الجولات: {Runs}  |  المتوسط: {avg}  |  الأفضل: {best}  |  الربح: {FormatCompact(profit)} ({FormatCompact(goldPerHour)}/س)"
                : $"Runs: {Runs}  |  Avg: {avg}  |  Best: {best}  |  Profit: {FormatCompact(profit)} ({FormatCompact(goldPerHour)}/h)";
        }
    }

    private static long ComputeProfitGold()
    {
        if (Game.Player == null || !_baselineGold.HasValue)
            return 0;

        return Game.Player.Gold >= _baselineGold.Value
            ? (long)(Game.Player.Gold - _baselineGold.Value)
            : 0;
    }

    private static long ComputeGoldPerHour(long profitGold)
    {
        if (!_sessionStartedAtUtc.HasValue || profitGold <= 0)
            return 0;

        var hours = (DateTime.UtcNow - _sessionStartedAtUtc.Value).TotalHours;
        if (hours < 0.01)
            return 0;

        return (long)(profitGold / hours);
    }

    private static long Sum(List<long> values)
    {
        long total = 0;
        for (var i = 0; i < values.Count; i++)
            total += values[i];
        return total;
    }

    private static string FormatDuration(long ms)
    {
        if (ms <= 0)
            return "0m";

        var ts = TimeSpan.FromMilliseconds(ms);
        return ts.TotalHours >= 1
            ? $"{(int)ts.TotalHours}h {ts.Minutes:D2}m"
            : $"{ts.Minutes}m {ts.Seconds:D2}s";
    }

    private static string FormatCompact(long value)
    {
        if (value >= 1_000_000_000) return $"{value / 1_000_000_000.0:0.#}B";
        if (value >= 1_000_000) return $"{value / 1_000_000.0:0.#}M";
        if (value >= 1_000) return $"{value / 1_000.0:0.#}K";
        return value.ToString("N0");
    }
}
