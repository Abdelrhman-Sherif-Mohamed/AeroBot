using System;
using System.Diagnostics;
using System.Linq;
using RSBot.Core.Client.ReferenceObjects;
using RSBot.Core.Components;
using RSBot.Core.Event;
using RSBot.Core.Objects;

namespace RSBot.Alchemy.Stats;

internal readonly struct AlchemyStatsSnapshot
{
    public bool HasData { get; init; }
    public TimeSpan Elapsed { get; init; }
    public int? HighestPlus { get; init; }
    public int FailCount { get; init; }
    public int ElixirCount { get; init; }
    public int LuckyStoneCount { get; init; }
    public int SteadyStoneCount { get; init; }
    public int ImmortalStoneCount { get; init; }
    public int AstralStoneCount { get; init; }
}

internal sealed class AlchemySessionStats
{
    private readonly object _sync = new();
    private readonly Stopwatch _sessionTimer = new();

    private bool _hasData;
    private TimeSpan _accumulatedElapsed = TimeSpan.Zero;
    private int? _highestPlus;
    private int _failCount;
    private int _elixirCount;
    private int _luckyStoneCount;
    private int _steadyStoneCount;
    private int _immortalStoneCount;
    private int _astralStoneCount;

    public event System.Action StatsUpdated;

    public void SubscribeEvents()
    {
        EventManager.SubscribeEvent("OnFuseRequest", new Action<AlchemyAction, AlchemyType>(OnFuseRequest));
        EventManager.SubscribeEvent("OnAlchemySuccess", new Action<InventoryItem, InventoryItem, AlchemyType>(OnAlchemySuccess));
        EventManager.SubscribeEvent("OnAlchemyFailed", new Action<InventoryItem, InventoryItem, AlchemyType>(OnAlchemyFailed));
        EventManager.SubscribeEvent("OnAlchemyDestroyed", new Action<InventoryItem, AlchemyType>(OnAlchemyDestroyed));
    }

    public void NotifyBotStarted()
    {
        lock (_sync)
        {
            _hasData = true;
            _sessionTimer.Restart();
        }

        StatsUpdated?.Invoke();
    }

    public void NotifyBotStopped()
    {
        lock (_sync)
        {
            if (_sessionTimer.IsRunning)
            {
                _accumulatedElapsed += _sessionTimer.Elapsed;
                _sessionTimer.Reset();
            }
        }

        StatsUpdated?.Invoke();
    }

    public void Reset()
    {
        lock (_sync)
        {
            _sessionTimer.Reset();
            _accumulatedElapsed = TimeSpan.Zero;
            _hasData = false;
            _highestPlus = null;
            _failCount = 0;
            _elixirCount = 0;
            _luckyStoneCount = 0;
            _steadyStoneCount = 0;
            _immortalStoneCount = 0;
            _astralStoneCount = 0;
        }

        StatsUpdated?.Invoke();
    }

    public AlchemyStatsSnapshot GetSnapshot()
    {
        lock (_sync)
        {
            var totalElapsed = _accumulatedElapsed + (_sessionTimer.IsRunning ? _sessionTimer.Elapsed : TimeSpan.Zero);

            return new AlchemyStatsSnapshot
            {
                HasData = _hasData,
                Elapsed = totalElapsed,
                HighestPlus = _highestPlus,
                FailCount = _failCount,
                ElixirCount = _elixirCount,
                LuckyStoneCount = _luckyStoneCount,
                SteadyStoneCount = _steadyStoneCount,
                ImmortalStoneCount = _immortalStoneCount,
                AstralStoneCount = _astralStoneCount
            };
        }
    }

    private void OnFuseRequest(AlchemyAction action, AlchemyType type)
    {
        if (AlchemyManager.ActiveAlchemyItems == null || AlchemyManager.ActiveAlchemyItems.Count < 2)
            return;

        var ingredient = AlchemyManager.ActiveAlchemyItems.ElementAtOrDefault(1);
        var desc1 = ingredient?.Record?.Desc1;

        lock (_sync)
        {
            _hasData = true;

            if (type == AlchemyType.Elixir)
            {
                _elixirCount++;
            }
            else if (type == AlchemyType.MagicStone && !string.IsNullOrEmpty(desc1))
            {
                if (desc1 == RefMagicOpt.MaterialLuck)
                    _luckyStoneCount++;
                else if (desc1 == RefMagicOpt.MaterialSteady)
                    _steadyStoneCount++;
                else if (desc1 == RefMagicOpt.MaterialImmortal)
                    _immortalStoneCount++;
                else if (desc1 == RefMagicOpt.MaterialAstral)
                    _astralStoneCount++;
            }
        }

        StatsUpdated?.Invoke();
    }

    private void OnAlchemySuccess(InventoryItem oldItem, InventoryItem newItem, AlchemyType type)
    {
        if (type != AlchemyType.Elixir || newItem == null)
            return;

        lock (_sync)
        {
            _hasData = true;
            if (!_highestPlus.HasValue || newItem.OptLevel > _highestPlus.Value)
                _highestPlus = newItem.OptLevel;
        }

        StatsUpdated?.Invoke();
    }

    private void OnAlchemyFailed(InventoryItem oldItem, InventoryItem newItem, AlchemyType type)
    {
        lock (_sync)
        {
            _hasData = true;
            _failCount++;
        }

        StatsUpdated?.Invoke();
    }

    private void OnAlchemyDestroyed(InventoryItem oldItem, AlchemyType type)
    {
        lock (_sync)
        {
            _hasData = true;
            _failCount++;
        }

        StatsUpdated?.Invoke();
    }
}
