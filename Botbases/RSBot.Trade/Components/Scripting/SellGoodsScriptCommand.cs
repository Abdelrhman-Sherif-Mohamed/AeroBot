using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using RSBot.Core;
using RSBot.Core.Components;
using RSBot.Core.Components.Scripting;
using RSBot.Core.Objects;
using RSBot.Core.Objects.Spawn;

namespace RSBot.Trade.Components.Scripting;

internal class SellGoodsScriptCommand : IScriptCommand
{
    public string Name => "sell-goods";

    public bool IsBusy { get; private set; }

    public Dictionary<string, string> Arguments => new()
    {
        { "Codename", "The code name of the NPC" }
    };

    public bool Execute(string[] arguments = null)
    {
        if (arguments == null || arguments.Length == 0)
        {
            Log.Warn("[Script] Invalid sell-goods command: NPC code name missing.");
            return false;
        }

        if (!TradeConfig.SellGoods)
        {
            Log.Notify("[Trade] SellGoods is disabled in settings. Skipping sell.");
            return true;
        }

        if (Game.Player?.JobTransport == null)
        {
            Log.Warn("[Script] Cannot sell items: No active job transport.");
            return false;
        }

        if (IsBusy || ShoppingManager.Running)
            return false;

        try
        {
            IsBusy = true;
            var codeName = arguments[0];

            SpawnedNpcNpc npc = null;
            for (var attempt = 0; attempt < 8; attempt++)
            {
                if (SpawnManager.TryGetEntity<SpawnedNpcNpc>(p => p.Record != null && string.Equals(p.Record.CodeName, codeName, StringComparison.OrdinalIgnoreCase), out npc) ||
                    SpawnManager.TryGetEntity<SpawnedNpcNpc>(p => p.Record != null &&
                        (p.Record.CodeName.Contains("SPECIAL", StringComparison.OrdinalIgnoreCase) ||
                         p.Record.CodeName.Contains("SMUGGLER", StringComparison.OrdinalIgnoreCase) ||
                         (p.Record.GetRealName() != null && (p.Record.GetRealName().Contains("Specialty", StringComparison.OrdinalIgnoreCase) || p.Record.GetRealName().Contains("Trader", StringComparison.OrdinalIgnoreCase)))), out npc))
                {
                    break;
                }

                Thread.Sleep(250);
            }

            if (npc != null)
            {
                codeName = npc.Record.CodeName;
                if (npc.Position.DistanceToPlayer() > 3.0f)
                {
                    Log.Notify($"[Trade] الاقتراب من تاجر تسليم البضائع [{npc.Record.GetRealName()}] (المسافة: {Math.Round(npc.Position.DistanceToPlayer(), 1)}م)...");
                    Game.Player?.MoveTo(npc.Position, sleep: false);
                    for (var w = 0; w < 40; w++)
                    {
                        if (npc.Position.DistanceToPlayer() <= 3.0f)
                            break;
                        Thread.Sleep(200);
                    }
                }

                if (Game.Player != null && Game.Player.Movement.Moving)
                {
                    Game.Player.MoveTo(Game.Player.Position, sleep: false);
                    for (var s = 0; s < 15; s++)
                    {
                        if (!Game.Player.Movement.Moving)
                            break;
                        Thread.Sleep(100);
                    }
                }

                if (Game.Player?.JobTransport != null)
                {
                    for (var w = 0; w < 30; w++)
                    {
                        if (Game.Player.JobTransport.Position.DistanceTo(npc.Position) <= 6.0f)
                            break;
                        Thread.Sleep(200);
                    }
                }
            }

            // In Silkroad Online / vSRO, players stay mounted on their trade transport
            // when selling specialty trade goods to the merchant.
            Thread.Sleep(300);

            ShoppingManager.Running = true;
            ShoppingManager.BotOpenedDialog = true;

            // Select the NPC, wait for the selection acknowledgement and open
            // the trade dialog before sending any sell operation.
            var talked = ShoppingManager.ChooseTalkOption(codeName, TalkOption.Trade);
            Thread.Sleep(500);

            if (!talked && Game.SelectedEntity == null)
            {
                // Fallback: search for NPC directly and select
                if (SpawnManager.TryGetEntity<SpawnedNpcNpc>(p => p.Record != null && string.Equals(p.Record.CodeName, codeName, StringComparison.OrdinalIgnoreCase), out npc) ||
                    SpawnManager.TryGetEntity<SpawnedNpcNpc>(p => p.Record != null && p.Record.CodeName.Contains("SPECIAL", StringComparison.OrdinalIgnoreCase), out npc))
                {
                    npc.TrySelect();
                    Thread.Sleep(300);
                    ShoppingManager.ChooseTalkOption(npc.Record.CodeName, TalkOption.Trade);
                    Thread.Sleep(500);
                }
            }

            if (Game.SelectedEntity == null)
            {
                Log.Warn("[Script] Cannot sell items: Not in dialog with NPC.");
                return false;
            }

            Log.Notify($"[Trade] جاري بيع وتسليم بضاعة التجارة إلى {Game.SelectedEntity.Record.GetRealName()}...");
            var sold = SellGoods();

            // Let the last transaction settle before closing the dialog,
            // otherwise the server may consider it a protocol violation.
            Thread.Sleep(500);
            ShoppingManager.CloseShop();
            Thread.Sleep(300);

            // Remount if MountTransport is enabled
            if (TradeConfig.MountTransport && Game.Player?.JobTransport != null && !Game.Player.HasActiveVehicle)
            {
                Game.Player.JobTransport.Mount();
                Thread.Sleep(500);
            }

            if (!sold)
            {
                Log.Warn("[Trade] لم يتم تسليم كل البضاعة. سيتم إعادة المحاولة لاحقاً.");
                return false;
            }

            Log.Notify("[Trade] تم تسليم وبيع جميع بضائع التجارة بنجاح!");
            return true;
        }
        catch (Exception ex)
        {
            Log.Error($"[Trade] خطأ أثناء تسليم البضاعة: {ex.Message}");
            return false;
        }
        finally
        {
            IsBusy = false;
            ShoppingManager.Running = false;
            ShoppingManager.BotOpenedDialog = false;
        }
    }

    /// <summary>
    /// Sells every item inside the job transport, one stack at a time,
    /// re-reading the transport inventory after each sale to verify that the
    /// server really removed the item. This avoids sending a burst of stale
    /// sell packets which used to disconnect the game client.
    /// </summary>
    private bool SellGoods()
    {
        var transport = Game.Player?.JobTransport;
        if (transport?.Inventory == null)
            return false;

        if (!transport.Inventory.Any(i => i != null && i.Record != null && i.Amount > 0))
        {
            Log.Notify("[Trade] لا توجد بضائع في حقيبة الجمل لبيعها.");
            return true;
        }

        var soldCount = 0;
        var failures = 0;
        var blacklistedItems = new HashSet<uint>();
        const int maxOperations = 400;

        for (var operation = 0; operation < maxOperations; operation++)
        {
            if (!IsBusy)
            {
                Log.Notify("[Trade] تم إيقاف البيع بناءً على طلب الإيقاف.");
                return false;
            }

            if (Game.SelectedEntity == null)
            {
                Log.Warn("[Trade] تم إغلاق نافذة التاجر قبل إنهاء البيع. إيقاف البيع.");
                return false;
            }

            var item = transport.Inventory
                .GetItems(i => i != null && i.Record != null && i.Amount > 0 &&
                               !blacklistedItems.Contains(i.ItemId))
                .OrderBy(i => i.Slot)
                .FirstOrDefault();

            if (item == null)
                break;

            var slot = item.Slot;
            var itemId = item.ItemId;
            var amountBefore = item.Amount;

            var sold = ShoppingManager.SellItem(item, transport.Bionic, 5000);
            Thread.Sleep(300); // Safe delay for server transaction confirmation

            if (IsSold(itemId, slot, amountBefore))
            {
                soldCount++;
                failures = 0;
                continue;
            }

            // Give the server one more chance, some servers delay the
            // inventory update a little.
            Thread.Sleep(500);

            if (IsSold(itemId, slot, amountBefore))
            {
                soldCount++;
                failures = 0;
                continue;
            }

            failures++;
            blacklistedItems.Add(itemId);
            Log.Warn($"[Trade] تعذر بيع {(item.Record?.GetRealName() ?? itemId.ToString())} (slot {slot}, amount {amountBefore}, sale acknowledged: {sold}).");

            if (failures >= 3)
            {
                Log.Warn("[Trade] فشل بيع 3 أصناف متتالية. إيقاف البيع لتجنب قطع الاتصال.");
                return false;
            }
        }

        Log.Notify($"[Trade] تم بيع {soldCount} رزمة بضاعة من الجمل بنجاح.");

        var leftovers = transport.Inventory.Count(i => i != null && i.Record != null && i.Amount > 0 &&
                                                       !blacklistedItems.Contains(i.ItemId));

        if (leftovers > 0)
        {
            Log.Warn($"[Trade] ما زال هناك {leftovers} رزمة في حقيبة الجمل لم يتم بيعها.");
            return false;
        }

        return true;
    }

    /// <summary>
    /// Checks whether the given stack was removed or reduced in the transport.
    /// </summary>
    private static bool IsSold(uint itemId, byte slot, ushort amountBefore)
    {
        var transport = Game.Player?.JobTransport;
        if (transport?.Inventory == null)
            return true;

        var after = transport.Inventory.GetItemAt(slot);
        if (after == null || after.ItemId != itemId)
            return true;

        return after.Amount < amountBefore;
    }

    public void Stop()
    {
        IsBusy = false;
    }
}
