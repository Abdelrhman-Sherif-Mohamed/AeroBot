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

internal class BuyGoodsScriptCommand : IScriptCommand
{
    #region Properties

    /// <summary>
    ///     The name of the command.
    /// </summary>
    public string Name => "buy-goods";

    /// <summary>
    ///     A value indicating if the command is busy.
    /// </summary>
    public bool IsBusy { get; private set; }

    /// <summary>
    ///     A dictionary of available arguments for this command.
    /// </summary>
    public Dictionary<string, string> Arguments => new()
    {
        { "Codename", "The code name of the NPC" }
    };

    #endregion Properties

    #region Methods

    /// <summary>
    ///     Executes the command.
    /// </summary>
    /// <param name="arguments"></param>
    /// <returns></returns>
    public bool Execute(string[] arguments = null)
    {
        if (arguments == null || arguments.Length < Arguments.Count)
        {
            Log.Warn("[Script] Invalid buy command: NPC code name information missing.");

            return false;
        }

        if (Game.Player.JobTransport == null)
        {
            Log.Warn("[Script] Can not buy items: No active job transport.");

            return false;
        }

        // Check if transport already has specialty goods loaded
        var transportHasGoods = Game.Player.JobTransport.Inventory != null &&
                                Game.Player.JobTransport.Inventory.Any(i => i != null && i.Record != null && i.Amount > 0);

        if (transportHasGoods && !TradeConfig.SellGoods)
        {
            Log.Notify("[Trade] الجمل يحتوي بالفعل على بضاعة تجارة! تم تخطي الشراء والمتابعة فوراً نحو الوجهة.");
            return true;
        }

        if (!TradeConfig.SellGoods && !TradeConfig.BuyGoods)
            return true;

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
                    SpawnManager.TryGetEntity<SpawnedNpcNpc>(p => p.Record != null && (p.Record.CodeName.Contains("SPECIAL", StringComparison.OrdinalIgnoreCase) || p.Record.CodeName.Contains("SMUGGLER", StringComparison.OrdinalIgnoreCase)), out npc))
                {
                    npc.TrySelect();
                    Thread.Sleep(300);
                    ShoppingManager.ChooseTalkOption(npc.Record.CodeName, TalkOption.Trade);
                }
            }

            if (Game.SelectedEntity == null)
            {
                Log.Warn("[Script] Can not buy items: Not in dialog with an NPC.");

                return false;
            }

            Log.Notify($"[Script] Selling goods to {Game.SelectedEntity.Record.GetRealName()}...");

            SellGoods();

            Log.Notify($"[Script] Purchasing goods from {Game.SelectedEntity.Record.GetRealName()}...");

            BuyGoods();

            ShoppingManager.CloseShop();

            return true;
        }
        finally
        {
            IsBusy = false;
            ShoppingManager.Running = false;
        }
    }

    /// <summary>
    ///     Sells all specialty goods to the selected trader which are
    ///     not in the merchant's inventory.
    /// </summary>
    private void SellGoods()
    {
        if (!TradeConfig.SellGoods || Game.Player.JobTransport == null)
            return;

        var items = Game.Player.JobTransport.Inventory.ToArray();
        if (items.Length == 0)
            return;

        var shopGroup = Game.ReferenceManager.GetRefShopGroup(Game.SelectedEntity?.Record.CodeName);
        if (shopGroup == null)
        {
            Log.Warn("[Script] Can not buy items: Can not find the shop info.");

            return;
        }

        var shopGoods = Game.ReferenceManager.GetRefShopGoods(shopGroup);

        foreach (var item in items)
        {
            var canSellToNpc = shopGoods.FirstOrDefault(i =>
                Game.ReferenceManager.GetRefPackageItem(i.RefPackageItemCodeName).RefItem.ID == item.ItemId) == null;

            if (!canSellToNpc)
                continue;

            ShoppingManager.SellItem(item, Game.Player.JobTransport.Bionic);
        }
    }

    /// <summary>
    ///     Buys specialty goods from the selected merchant.
    /// </summary>
    private void BuyGoods()
    {
        if (!TradeConfig.BuyGoods)
            return;

        var shopGroup = Game.ReferenceManager.GetRefShopGroup(Game.SelectedEntity?.Record.CodeName);
        if (shopGroup == null)
        {
            Log.Warn("[Script] Can not buy items: Can not find the shop info.");

            return;
        }

        var shopGoods = Game.ReferenceManager.GetRefShopGoods(shopGroup);
        var item = shopGoods?.FirstOrDefault();

        if (item == null)
        {
            Log.Warn("[Script] Can not buy items: Can not find the shop info.");

            return;
        }

        var tabIndex = Game.ReferenceManager.GetRefShopGoodTabIndex(Game.SelectedEntity?.Record.CodeName, item);
        if (tabIndex == 0xFF) //Specified item not available in this shop!
        {
            Log.Warn("[Script] Can not buy items: Can not find the item in the shop.");

            return;
        }

        var packageItem = Game.ReferenceManager.GetRefPackageItem(item.RefPackageItemCodeName);
        if (packageItem?.RefItem == null)
        {
            Log.Warn("[Script] Can not buy items: Can not find the referenced item.");

            return;
        }

        var maxStack = packageItem.RefItem.MaxStack > 0 ? packageItem.RefItem.MaxStack : 1000;
        // If BuyGoodsQuantity is 0, target is at least 5 slots (5-star trade cargo: 5000 units) or full inventory capacity
        var targetQuantity = TradeConfig.BuyGoodsQuantity > 0
            ? TradeConfig.BuyGoodsQuantity
            : Math.Max(5000, (Game.Player.JobTransport?.Inventory?.Capacity ?? 5) * maxStack);

        var safetyLimit = 25;
        while (safetyLimit-- > 0)
        {
            var transport = Game.Player.JobTransport;
            if (transport?.Inventory == null)
                break;

            if (transport.Inventory.Full)
            {
                Log.Notify("[Trade] حقيبة الجمل ممتلئة بالكامل بجميع خانات البضاعة!");
                break;
            }

            var currentQuantity = transport.Inventory.GetSumAmount(packageItem.RefItemCodeName);
            if (currentQuantity >= targetQuantity)
            {
                Log.Notify($"[Trade] تم شراء كامل بضاعة التجارة بنجاح ({currentQuantity} وحدة - حمولة 5 نجوم مكتملة)!");
                break;
            }

            var buyNextQty = maxStack;
            if (currentQuantity + buyNextQty > targetQuantity)
                buyNextQty = targetQuantity - currentQuantity;

            Log.Notify($"[Trade] شراء بضاعة خاصة: حزمة من {buyNextQty} (الإجمالي الحالي: {currentQuantity}/{targetQuantity})...");

            ShoppingManager.PurchaseItem(transport, tabIndex, item.SlotIndex, (ushort)buyNextQty);

            System.Threading.Thread.Sleep(350); // Intentionally wait so server responds and inventory count updates
        }

        Log.Notify($"[Trade] اكتمال عملية الشراء. إجمالي البضاعة المحمّلة بالجمل: {Game.Player.JobTransport?.Inventory?.GetSumAmount(packageItem.RefItemCodeName)} وحدة (5 نجوم كاملة).");
    }

    /// <summary>
    ///     Stops the command.
    /// </summary>
    public void Stop()
    {
        IsBusy = false;
    }

    #endregion Methods
}