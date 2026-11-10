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

            // 1. Locate NPC in range, or wait/retry if nearby
            SpawnedNpcNpc npc = null;
            for (var attempt = 0; attempt < 6; attempt++)
            {
                if (SpawnManager.TryGetEntity<SpawnedNpcNpc>(p => p.Record != null && string.Equals(p.Record.CodeName, codeName, StringComparison.OrdinalIgnoreCase), out npc) ||
                    SpawnManager.TryGetEntity<SpawnedNpcNpc>(p => p.Record != null && (p.Record.CodeName.Contains("SPECIAL", StringComparison.OrdinalIgnoreCase) || p.Record.CodeName.Contains("SMUGGLER", StringComparison.OrdinalIgnoreCase)), out npc))
                {
                    break;
                }
                Thread.Sleep(300);
            }

            if (npc == null)
            {
                Log.Warn($"[Trade] تعذر العثور على تاجر البضائع [{codeName}] بالقرب من اللاعب.");
                return false;
            }

            // 2. Approach NPC if farther than 5 meters
            if (npc.DistanceToPlayer > 5.0f)
            {
                Log.Notify($"[Trade] الاقتراب من تاجر البضائع [{npc.Record.GetRealName()}] (المسافة: {Math.Round(npc.DistanceToPlayer, 1)}م)...");
                Game.Player?.MoveTo(npc.Position, sleep: false);
                for (var w = 0; w < 30; w++)
                {
                    if (npc.DistanceToPlayer <= 5.0f)
                        break;
                    Thread.Sleep(200);
                }
            }

            ShoppingManager.Running = true;
            ShoppingManager.BotOpenedDialog = true;
            ShoppingManager.SelectedEntity = npc;
            Game.SelectedEntity = npc;

            ShoppingManager.ChooseTalkOption(npc.Record.CodeName, TalkOption.Trade);
            Thread.Sleep(400);

            var activeEntity = (Game.SelectedEntity as SpawnedNpcNpc) ?? (ShoppingManager.SelectedEntity as SpawnedNpcNpc) ?? npc;
            Game.SelectedEntity = activeEntity;
            ShoppingManager.SelectedEntity = activeEntity;

            if (TradeConfig.SellGoods)
            {
                Log.Notify($"[Trade] جاري فحص وبيع بضائع التجارة القديمة لدى [{activeEntity.Record.GetRealName()}]...");
                SellGoods();
                Thread.Sleep(400);
            }

            if (TradeConfig.BuyGoods)
            {
                Log.Notify($"[Trade] جاري شراء بضاعة التجارة من [{activeEntity.Record.GetRealName()}]...");
                BuyGoods();
            }

            // Settle transaction before closing dialog
            Thread.Sleep(500);

            ShoppingManager.CloseShop();
            Thread.Sleep(400);

            // Verify if transport now has goods
            var nowHasGoods = Game.Player?.JobTransport?.Inventory != null &&
                              Game.Player.JobTransport.Inventory.Any(i => i != null && i.Record != null && i.Amount > 0);

            if (!nowHasGoods && TradeConfig.BuyGoods)
            {
                Log.Warn("[Trade] تحذير: حقيبة الجمل لا تزال فارغة بعد محاولة الشراء!");
            }
            else if (nowHasGoods)
            {
                Log.Notify("[Trade] تم تأكيد شحن البضاعة في الجمل بنجاح، جاهز للتحرك نحو وجهة البيع!");
            }

            return true;
        }
        finally
        {
            IsBusy = false;
            ShoppingManager.Running = false;
            ShoppingManager.BotOpenedDialog = false;
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
            if (item?.Record == null || item.Amount == 0)
                continue;

            var canSellToNpc = shopGoods == null || shopGoods.All(i =>
            {
                var package = Game.ReferenceManager.GetRefPackageItem(i.RefPackageItemCodeName);
                return package?.RefItem == null || package.RefItem.ID != item.ItemId;
            });

            if (!canSellToNpc)
                continue;

            ShoppingManager.SellItem(item, Game.Player.JobTransport.Bionic);
            Thread.Sleep(300); // Safe delay for server transaction confirmation
        }
    }

    /// <summary>
    ///     Buys specialty goods from the selected merchant.
    /// </summary>
    private void BuyGoods()
    {
        if (!TradeConfig.BuyGoods)
            return;

        var npc = (Game.SelectedEntity as SpawnedNpcNpc)
               ?? (ShoppingManager.SelectedEntity as SpawnedNpcNpc)
               ?? (Game.Player?.State?.DialogState?.Npc as SpawnedNpcNpc);

        if (npc?.Record == null)
        {
            Log.Warn("[Trade] تعذر الشراء: لم يتم العثور على تاجر البضائع المحدد.");
            return;
        }

        var shopGroup = Game.ReferenceManager.GetRefShopGroup(npc.Record.CodeName);
        if (shopGroup == null)
        {
            Log.Warn($"[Trade] تعذر الشراء: لم يتم العثور على معلومات المتجر للتاجر [{npc.Record.CodeName}].");
            return;
        }

        var shopGoods = Game.ReferenceManager.GetRefShopGoods(shopGroup);
        var item = shopGoods?.FirstOrDefault();

        if (item == null)
        {
            Log.Warn($"[Trade] تعذر الشراء: لا توجد بضائع متوفرة في متجر [{npc.Record.CodeName}].");
            return;
        }

        var tabIndex = Game.ReferenceManager.GetRefShopGoodTabIndex(npc.Record.CodeName, item);
        if (tabIndex == 0xFF) //Specified item not available in this shop!
        {
            Log.Warn($"[Trade] تعذر الشراء: لم يتم العثور على تبويب السلعة في متجر [{npc.Record.CodeName}].");
            return;
        }

        var packageItem = Game.ReferenceManager.GetRefPackageItem(item.RefPackageItemCodeName);
        if (packageItem?.RefItem == null)
        {
            Log.Warn($"[Trade] تعذر الشراء: تعذر قراءة بيانات السلعة [{item.RefPackageItemCodeName}].");
            return;
        }

        var maxStack = packageItem.RefItem.MaxStack > 0 ? packageItem.RefItem.MaxStack : 1000;
        // If BuyGoodsQuantity is 0, target is at least 5 slots (5-star trade cargo: 5000 units) or full inventory capacity
        var targetQuantity = TradeConfig.BuyGoodsQuantity > 0
            ? TradeConfig.BuyGoodsQuantity
            : Math.Max(5000, (Game.Player.JobTransport?.Inventory?.Capacity ?? 5) * maxStack);

        Log.Notify($"[Trade] بدء شراء السلعة [{packageItem.RefItem.GetRealName()}] - الكمية المستهدفة: {targetQuantity} وحدة...");

        var safetyLimit = 30;
        var failedAttempts = 0;
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
                Log.Notify($"[Trade] تم شراء كامل بضاعة التجارة بنجاح ({currentQuantity} وحدة - حمولة كاملة)! الحمولة 5 نجوم جاهزة.");
                break;
            }

            var buyNextQty = maxStack;
            if (currentQuantity + buyNextQty > targetQuantity)
                buyNextQty = targetQuantity - currentQuantity;

            Log.Notify($"[Trade] إرسال طلب شراء {buyNextQty} وحدة (الإجمالي الحالي: {currentQuantity}/{targetQuantity})...");

            var confirmed = ShoppingManager.PurchaseItem(transport, tabIndex, item.SlotIndex, (ushort)buyNextQty);

            System.Threading.Thread.Sleep(350); // Intentionally wait so server responds and inventory count updates

            var newQuantity = transport.Inventory?.GetSumAmount(packageItem.RefItemCodeName) ?? currentQuantity;

            if (newQuantity >= targetQuantity)
            {
                Log.Notify($"[Trade] تم اكتمال شراء بضاعة التجارة بنجاح ({newQuantity}/{targetQuantity} وحدة)! الحمولة 5 نجوم جاهزة.");
                break;
            }

            // If the server did not confirm the purchase or the transport
            // inventory did not change, never keep spamming buy requests: that
            // is what makes the game client disconnect right after buying.
            if (!confirmed || newQuantity <= currentQuantity)
            {
                failedAttempts++;

                Log.Warn($"[Trade] لم يتم تأكيد شراء البضاعة من السيرفر (محاولة {failedAttempts}/3، الإجمالي الحالي: {newQuantity}).");

                if (failedAttempts >= 3)
                {
                    Log.Warn("[Trade] تم إيقاف الشراء بعد 3 محاولات غير مؤكدة لتجنب قطع الاتصال.");
                    break;
                }
            }
            else
            {
                failedAttempts = 0;
            }
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