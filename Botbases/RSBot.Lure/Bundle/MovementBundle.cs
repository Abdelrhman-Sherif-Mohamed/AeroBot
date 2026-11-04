using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using RSBot.Core;
using RSBot.Core.Components;
using RSBot.Core.Event;
using RSBot.Core.Objects;
using RSBot.Lure.Components;

namespace RSBot.Lure.Bundle;

internal static class MovementBundle
{
    private enum LureState
    {
        GoingToLureSpot,
        ReturningToParty,
        WaitingAtParty
    }

    private static LureState _state = LureState.GoingToLureSpot;
    private static int _currentSpotIndex;
    private static DateTime _waitingStartTime = DateTime.MinValue;
    private static Position _currentTargetPosition;
    private static DateTime _lastMoveCommandTime = DateTime.MinValue;

    public static void Reset()
    {
        _state = LureState.GoingToLureSpot;
        _currentSpotIndex = 0;
        _waitingStartTime = DateTime.MinValue;
        _currentTargetPosition = default;
        _lastMoveCommandTime = DateTime.MinValue;
    }

    public static void Tick()
    {
        if (Game.Player == null)
            return;

        var partyArea = LureConfig.Area;
        var spots = LureConfig.Spots;

        // If no spots configured, fallback to random roam or stay at party
        if (spots == null || spots.Count == 0)
        {
            if (LureConfig.WalkRandomly)
                TickRandomRoam(partyArea);
            return;
        }

        if (_currentSpotIndex >= spots.Count)
            _currentSpotIndex = 0;

        var currentSpot = spots[_currentSpotIndex];

        switch (_state)
        {
            case LureState.GoingToLureSpot:
            {
                var spotPos = currentSpot.Position;
                var distToSpot = spotPos.DistanceToPlayer();

                // Reached the lure spot!
                if (distToSpot <= 4f)
                {
                    Log.Debug($"[Lure] Reached lure spot #{_currentSpotIndex + 1} ({spotPos.X:0.0}, {spotPos.Y:0.0})");
                    _state = LureState.ReturningToParty;
                    _lastMoveCommandTime = DateTime.MinValue;
                    return;
                }

                if (!Game.Player.Movement.Moving || (DateTime.Now - _lastMoveCommandTime).TotalSeconds > 8)
                {
                    Log.Status($"Walking to lure spot #{_currentSpotIndex + 1}...");
                    _currentTargetPosition = spotPos;
                    _lastMoveCommandTime = DateTime.Now;
                    Game.Player.MoveTo(spotPos, false);
                }
                break;
            }

            case LureState.ReturningToParty:
            {
                var partyPos = partyArea.Position;
                var distToParty = partyPos.DistanceToPlayer();

                // Reached the party!
                if (distToParty <= 4f)
                {
                    Log.Debug("[Lure] Returned to party center");
                    _state = LureState.WaitingAtParty;
                    _waitingStartTime = DateTime.Now;
                    _lastMoveCommandTime = DateTime.MinValue;
                    return;
                }

                if (!Game.Player.Movement.Moving || (DateTime.Now - _lastMoveCommandTime).TotalSeconds > 8)
                {
                    Log.Status("Returning to party...");
                    _currentTargetPosition = partyPos;
                    _lastMoveCommandTime = DateTime.Now;
                    Game.Player.MoveTo(partyPos, false);
                }
                break;
            }

            case LureState.WaitingAtParty:
            {
                int waitSeconds = Math.Max(1, LureConfig.StayAtCenterForSeconds);
                var elapsed = (DateTime.Now - _waitingStartTime).TotalSeconds;

                if (elapsed >= waitSeconds)
                {
                    _currentSpotIndex = (_currentSpotIndex + 1) % spots.Count;
                    Log.Debug($"[Lure] Finished waiting at party, heading to spot #{_currentSpotIndex + 1}");
                    _state = LureState.GoingToLureSpot;
                    _lastMoveCommandTime = DateTime.MinValue;
                    return;
                }

                int remaining = Math.Max(0, waitSeconds - (int)elapsed);
                EventManager.FireEvent("OnChangeStatusText", $"Waiting at party ({remaining}s)...");
                break;
            }
        }
    }

    private static void TickRandomRoam(Area partyArea)
    {
        var distToParty = partyArea.Position.DistanceToPlayer();

        if (_state == LureState.WaitingAtParty)
        {
            int waitSeconds = Math.Max(1, LureConfig.StayAtCenterForSeconds);
            var elapsed = (DateTime.Now - _waitingStartTime).TotalSeconds;
            if (elapsed >= waitSeconds)
            {
                _state = LureState.GoingToLureSpot;
                _lastMoveCommandTime = DateTime.MinValue;
                return;
            }
            int remaining = Math.Max(0, waitSeconds - (int)elapsed);
            EventManager.FireEvent("OnChangeStatusText", $"Waiting at party ({remaining}s)...");
            return;
        }

        if (_state == LureState.ReturningToParty)
        {
            if (distToParty <= 4f)
            {
                _state = LureState.WaitingAtParty;
                _waitingStartTime = DateTime.Now;
                return;
            }

            if (!Game.Player.Movement.Moving || (DateTime.Now - _lastMoveCommandTime).TotalSeconds > 8)
            {
                Log.Status("Returning to party...");
                _lastMoveCommandTime = DateTime.Now;
                Game.Player.MoveTo(partyArea.Position, false);
            }
            return;
        }

        // Going out:
        if (distToParty >= (partyArea.Radius * 0.7f))
        {
            _state = LureState.ReturningToParty;
            _lastMoveCommandTime = DateTime.MinValue;
            return;
        }

        if (!Game.Player.Movement.Moving || (DateTime.Now - _lastMoveCommandTime).TotalSeconds > 8)
        {
            var destination = partyArea.GetRandomPosition();
            Log.Status("Walking to pull position...");
            _lastMoveCommandTime = DateTime.Now;
            Game.Player.MoveTo(destination, false);
        }
    }
}
