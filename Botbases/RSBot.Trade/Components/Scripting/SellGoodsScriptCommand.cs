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

            ShoppingManager.Running = true;
            ShoppingManager.ChooseTalkOption(codeName, TalkOption.Trade);

            if (Game.SelectedEntity == null)
            {
                // Fallback: search for NPC directly and select
                if (SpawnManager.TryGetEntity<SpawnedNpcNpc>(p => p.Record != null && string.Equals(p.Record.CodeName, codeName, StringComparison.OrdinalIgnoreCase), out var npc) ||
                    SpawnManager.TryGetEntity<SpawnedNpcNpc>(p => p.Record != null && p.Record.CodeName.Contains("SPECIAL", StringComparison.OrdinalIgnoreCase), out npc))
                {
                    npc.TrySelect();
                    Thread.Sleep(300);
                    ShoppingManager.ChooseTalkOption(npc.Record.CodeName, TalkOption.Trade);
                }
            }

            if (Game.SelectedEntity == null)
            {
                Log.Warn("[Script] Cannot sell items: Not in dialog with NPC.");
                return false;
            }

            Log.Notify($"[Trade] جاري بيع وتسليم بضاعة التجارة إلى {Game.SelectedEntity.Record.GetRealName()}...");
            SellGoods();

            ShoppingManager.CloseShop();

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
        }
    }

    private void SellGoods()
    {
        if (Game.Player?.JobTransport?.Inventory == null)
            return;

        var items = Game.Player.JobTransport.Inventory.ToArray();
        if (items.Length == 0)
        {
            Log.Notify("[Trade] لا توجد بضائع في حقيبة الجمل لبيعها.");
            return;
        }

        var shopGroup = Game.ReferenceManager.GetRefShopGroup(Game.SelectedEntity?.Record?.CodeName);
        var shopGoods = shopGroup != null ? Game.ReferenceManager.GetRefShopGoods(shopGroup) : null;

        var soldCount = 0;
        foreach (var item in items)
        {
            if (item == null || item.Record == null || item.Amount == 0)
                continue;

            // Check if item was bought from this exact NPC
            if (shopGoods != null)
            {
                var isOriginShopItem = shopGoods.Any(i =>
                {
                    var pkg = Game.ReferenceManager.GetRefPackageItem(i.RefPackageItemCodeName);
                    return pkg?.RefItem != null && pkg.RefItem.ID == item.ItemId;
                });

                if (isOriginShopItem)
                    continue;
            }

            ShoppingManager.SellItem(item, Game.Player.JobTransport.Bionic);
            soldCount++;
            Thread.Sleep(250); // Safe delay for server transaction confirmation
        }

        Log.Notify($"[Trade] تم بيع {soldCount} رزمة بضاعة من الجمل بنجاح.");
    }

    public void Stop()
    {
        IsBusy = false;
    }
}
