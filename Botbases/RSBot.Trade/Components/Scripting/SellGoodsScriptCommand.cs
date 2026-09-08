using System.Collections.Generic;
using System.Linq;
using System.Threading;
using RSBot.Core;
using RSBot.Core.Components;
using RSBot.Core.Components.Scripting;
using RSBot.Core.Objects;

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

        if (Game.Player.JobTransport == null)
        {
            Log.Warn("[Script] Cannot sell items: No active job transport.");
            return true;
        }

        var items = Game.Player.JobTransport.Inventory.ToArray();
        if (items.Length == 0)
        {
            Log.Notify("[Trade] Transport inventory has no goods to sell.");
            return true;
        }

        if (IsBusy || ShoppingManager.Running)
            return false;

        try
        {
            IsBusy = true;
            var codeName = arguments[0].Trim();

            ShoppingManager.Running = true;
            ShoppingManager.ChooseTalkOption(codeName, TalkOption.Trade);

            if (Game.SelectedEntity == null)
            {
                Log.Warn($"[Script] Could not open trade dialog with NPC [{codeName}].");
                return false;
            }

            Log.Notify($"[Script] Selling specialty goods to {Game.SelectedEntity.Record.GetRealName()}...");

            var shopGroup = Game.ReferenceManager.GetRefShopGroup(Game.SelectedEntity?.Record.CodeName);
            var shopGoods = shopGroup != null ? Game.ReferenceManager.GetRefShopGoods(shopGroup) : null;

            foreach (var item in items)
            {
                if (shopGoods != null)
                {
                    var canSellToNpc = shopGoods.FirstOrDefault(i =>
                        Game.ReferenceManager.GetRefPackageItem(i.RefPackageItemCodeName)?.RefItem?.ID == item.ItemId) == null;

                    if (!canSellToNpc)
                        continue;
                }

                ShoppingManager.SellItem(item, Game.Player.JobTransport.Bionic);
                Thread.Sleep(80);
            }

            ShoppingManager.CloseShop();
            Log.Notify("[Trade] Specialty goods sold successfully!");

            return true;
        }
        finally
        {
            IsBusy = false;
            ShoppingManager.Running = false;
        }
    }

    public void Stop()
    {
        IsBusy = false;
    }
}
