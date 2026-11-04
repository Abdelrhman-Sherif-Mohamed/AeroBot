using System;
using System.Collections.Generic;
using RSBot.Core;
using RSBot.Core.Event;

namespace RSBot.Inventory.Subscriber;

internal class UseItemAtTrainplaceSubscriber
{
    private static readonly Dictionary<string, DateTime> _retryAfter = new();
    private static readonly Dictionary<string, DateTime> _lastAttempt = new();

    public static void SubscribeEvents()
    {
        EventManager.SubscribeEvent("OnTick", OnTick);
    }

    private static void OnTick()
    {
        if (!Kernel.Bot.Running || Kernel.Bot.Botbase == null)
            return;

        var area = Kernel.Bot.Botbase.Area;
        if (area.Position.Region == 0)
            return;

        // Check if player is within/near training area
        var maxDistance = Math.Max(100, area.Radius + 30);
        if (area.Position.DistanceToPlayer() > maxDistance)
            return;

        var itemsToUse = PlayerConfig.GetArray<string>("RSBot.Inventory.ItemsAtTrainplace");
        if (itemsToUse == null || itemsToUse.Length == 0)
            return;

        var now = DateTime.UtcNow;

        foreach (var itemCodeName in itemsToUse)
        {
            if (string.IsNullOrWhiteSpace(itemCodeName))
                continue;

            // Check retry cooldown
            if (_retryAfter.TryGetValue(itemCodeName, out var retryTime) && now < retryTime)
                continue;

            // Enforce at least 2.5 seconds between consecutive attempts for the same item
            if (_lastAttempt.TryGetValue(itemCodeName, out var lastTime) && (now - lastTime).TotalSeconds < 2.5)
                continue;

            var invItem = Game.Player.Inventory.GetItem(itemCodeName);
            if (invItem == null)
                continue;

            // If the item's skill buff or perk is currently active, do not reuse yet
            if (invItem.ItemSkillInUse)
                continue;

            _lastAttempt[itemCodeName] = now;
            Log.Notify($"[AeroBot] Use [{invItem.Record.GetRealName()}] at training place");

            if (!invItem.Use())
            {
                // If usage failed (e.g. cooldown or animation lock), retry in 3 seconds
                _retryAfter[itemCodeName] = now.AddSeconds(3);
                Log.Debug($"[AeroBot] Item [{invItem.Record.GetRealName()}] use failed or timed out, will retry in 3 seconds.");
            }
            else
            {
                // Item used successfully, wait at least 3 seconds before next check
                _retryAfter[itemCodeName] = now.AddSeconds(3);
            }
        }
    }
}
