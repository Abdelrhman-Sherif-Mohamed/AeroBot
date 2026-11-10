using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using RSBot.Core;
using RSBot.Core.Client.ReferenceObjects;
using RSBot.Core.Components;
using RSBot.Core.Components.Scripting;
using RSBot.Core.Objects;
using RSBot.Core.Objects.Cos;
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
        { "Codename", "The code name of the NPC (e.g. NPC_WC_SPECIAL, NPC_KT_SPECIAL, or 'Full' to auto-detect)" },
        { "QuantityOrMode", "The quantity or 'Full' for maximum 5-star capacity (optional)" }
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
        string codeName = null;
        var isFull = true;
        var targetQuantity = 0;

        if (arguments != null && arguments.Length > 0)
        {
            var first = arguments[0]?.Trim();
            if (!string.IsNullOrEmpty(first))
            {
                if (first.Equals("Full", StringComparison.OrdinalIgnoreCase) ||
                    first.Equals("All", StringComparison.OrdinalIgnoreCase) ||
                    first.Equals("Max", StringComparison.OrdinalIgnoreCase))
                {
                    isFull = true;
                }
                else if (int.TryParse(first, out var q1))
                {
                    targetQuantity = q1;
                    isFull = (targetQuantity <= 0);
                }
                else
                {
                    codeName = first;
                }
            }

            if (arguments.Length > 1)
            {
                var second = arguments[1]?.Trim();
                if (!string.IsNullOrEmpty(second))
                {
                    if (second.Equals("Full", StringComparison.OrdinalIgnoreCase) ||
                        second.Equals("All", StringComparison.OrdinalIgnoreCase) ||
                        second.Equals("Max", StringComparison.OrdinalIgnoreCase))
                    {
                        isFull = true;
                    }
                    else if (int.TryParse(second, out var q2))
                    {
                        targetQuantity = q2;
                        isFull = (targetQuantity <= 0);
                    }
                }
            }
        }

        var transport = Game.Player?.JobTransport;
        if (transport == null)
        {
            Log.Warn("[Trade] تعذر الشراء: لا توجد وسيلة نقل تجارة (جمل/حصان) مستدعاة.");
            return false;
        }

        // Check if transport is ALREADY full
        if (transport.Inventory != null && transport.Inventory.Full && !TradeConfig.SellGoods)
        {
            Log.Notify("[Trade] حقيبة الجمل ممتلئة بالكامل بجميع خانات البضاعة! تخطي الشراء والمتابعة فوراً نحو الوجهة.");
            return true;
        }

        if (IsBusy || ShoppingManager.Running)
            return false;

        try
        {
            IsBusy = true;

            // 1. Locate NPC in range, or wait/retry if nearby
            SpawnedNpcNpc npc = null;
            for (var attempt = 0; attempt < 8; attempt++)
            {
                if (!string.IsNullOrEmpty(codeName))
                {
                    if (SpawnManager.TryGetEntity<SpawnedNpcNpc>(p => p.Record != null && string.Equals(p.Record.CodeName, codeName, StringComparison.OrdinalIgnoreCase), out npc))
                        break;
                }

                if (SpawnManager.TryGetEntity<SpawnedNpcNpc>(p => p.Record != null &&
                    (p.Record.CodeName.Contains("SPECIAL", StringComparison.OrdinalIgnoreCase) ||
                     p.Record.CodeName.Contains("SMUGGLER", StringComparison.OrdinalIgnoreCase) ||
                     (p.Record.GetRealName() != null && (p.Record.GetRealName().Contains("Specialty", StringComparison.OrdinalIgnoreCase) || p.Record.GetRealName().Contains("Trader", StringComparison.OrdinalIgnoreCase)))), out npc))
                {
                    break;
                }

                Thread.Sleep(250);
            }

            if (npc == null)
            {
                Log.Warn($"[Trade] تعذر العثور على تاجر البضائع [{(string.IsNullOrEmpty(codeName) ? "Specialty Trader" : codeName)}] بالقرب من اللاعب.");
                return false;
            }

            // 2. Approach NPC until within interactive talk range (<= 2.5 meters)
            if (npc.DistanceToPlayer > 2.5f)
            {
                Log.Notify($"[Trade] الاقتراب من تاجر البضائع [{npc.Record.GetRealName()}] (المسافة: {Math.Round(npc.DistanceToPlayer, 1)}م)...");
                Game.Player?.MoveTo(npc.Position, sleep: false);
                for (var w = 0; w < 40; w++)
                {
                    if (npc.DistanceToPlayer <= 2.5f)
                        break;
                    Thread.Sleep(200);
                }
            }

            // 3. Ensure player has completely stopped moving before initiating dialog
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

            // 4. Ensure job transport (camel/horse) has arrived near the NPC before trade
            if (Game.Player?.JobTransport != null)
            {
                for (var w = 0; w < 30; w++)
                {
                    var camelDist = Game.Player.JobTransport.Position.DistanceTo(npc.Position);
                    if (camelDist <= 6.0f)
                        break;
                    Thread.Sleep(200);
                }
            }
            Thread.Sleep(400);

            ShoppingManager.Running = true;
            ShoppingManager.BotOpenedDialog = true;
            ShoppingManager.SelectedEntity = npc;
            Game.SelectedEntity = npc;

            // Open shop dialog with verification & retries
            var dialogOpened = false;
            for (var dAttempt = 1; dAttempt <= 3; dAttempt++)
            {
                Log.Notify($"[Trade] فتح نافذة متجر البضائع لدى [{npc.Record.GetRealName()}] (محاولة {dAttempt}/3)...");
                if (ShoppingManager.ChooseTalkOption(npc.Record.CodeName, TalkOption.Trade))
                {
                    dialogOpened = true;
                    Log.Notify($"[Trade] تم فتح نافذة المتجر بنجاح لدى [{npc.Record.GetRealName()}].");
                    break;
                }

                if (npc.DistanceToPlayer > 2.5f)
                {
                    Game.Player?.MoveTo(npc.Position, sleep: false);
                    for (var w = 0; w < 20; w++)
                    {
                        if (npc.DistanceToPlayer <= 2.5f)
                            break;
                        Thread.Sleep(150);
                    }
                    if (Game.Player != null)
                        Game.Player.MoveTo(Game.Player.Position, sleep: false);
                }
                Thread.Sleep(500);
            }

            if (!dialogOpened)
            {
                Log.Warn($"[Trade] تعذر فتح نافذة متجر البضائع لدى [{npc.Record.GetRealName()}]. يتم تخطي الشراء لتجنب الخطأ.");
                return false;
            }

            Thread.Sleep(300);

            var activeEntity = (Game.SelectedEntity as SpawnedNpcNpc) ?? (ShoppingManager.SelectedEntity as SpawnedNpcNpc) ?? npc;
            Game.SelectedEntity = activeEntity;
            ShoppingManager.SelectedEntity = activeEntity;

            if (TradeConfig.SellGoods)
            {
                Log.Notify($"[Trade] جاري فحص وبيع بضائع التجارة القديمة لدى [{activeEntity.Record.GetRealName()}]...");
                SellGoods();
                Thread.Sleep(400);
            }

            // Always execute purchase because buy-goods was explicitly invoked in script
            Log.Notify($"[Trade] جاري شراء بضاعة التجارة من [{activeEntity.Record.GetRealName()}] (الوضع: {(isFull ? "Full - حمولة كاملة 5 نجوم" : $"{targetQuantity} وحدة")})...");
            BuyGoods(targetQuantity, isFull);

            // Settle transaction before closing dialog
            Thread.Sleep(500);

            ShoppingManager.CloseShop();
            Thread.Sleep(400);

            // Verify if transport now has goods
            var nowHasGoods = Game.Player?.JobTransport?.Inventory != null &&
                              Game.Player.JobTransport.Inventory.Any(i => i != null && i.Record != null && i.Amount > 0);

            if (!nowHasGoods)
            {
                Log.Warn("[Trade] تحذير: حقيبة الجمل لا تزال فارغة بعد محاولة الشراء!");
            }
            else
            {
                var totalUnits = Game.Player.JobTransport.Inventory.Sum(i => i?.Amount ?? 0);
                Log.Notify($"[Trade] تم تأكيد شحن البضاعة في الجمل بنجاح ({totalUnits} وحدة)، جاهز للتحرك نحو وجهة البيع!");
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

    #region Specialty Goods Reference Fallback

    private class SpecialtyGoodsInfo
    {
        public string ItemCode { get; set; }
        public string PackageCode { get; set; }
        public byte TabIndex { get; set; }
        public byte SlotIndex { get; set; }
        public string EnglishName { get; set; }
    }

    private static readonly Dictionary<string, SpecialtyGoodsInfo> SpecialtyGoodsMap = new(StringComparer.OrdinalIgnoreCase)
    {
        { "NPC_WC_SPECIAL", new SpecialtyGoodsInfo { ItemCode = "ITEM_ETC_TRADE_WC_01", PackageCode = "PACKAGE_ITEM_ETC_TRADE_WC_01", TabIndex = 0, SlotIndex = 0, EnglishName = "Leather" } },
        { "NPC_KT_SPECIAL", new SpecialtyGoodsInfo { ItemCode = "ITEM_ETC_TRADE_KT_01", PackageCode = "PACKAGE_ITEM_ETC_TRADE_KT_01", TabIndex = 0, SlotIndex = 0, EnglishName = "Nephrite" } },
        { "NPC_CH_SPECIAL", new SpecialtyGoodsInfo { ItemCode = "ITEM_ETC_TRADE_CH_01", PackageCode = "PACKAGE_ITEM_ETC_TRADE_CH_01", TabIndex = 0, SlotIndex = 0, EnglishName = "White Silk" } },
        { "NPC_EU_SPECIAL", new SpecialtyGoodsInfo { ItemCode = "ITEM_ETC_TRADE_EU_01", PackageCode = "PACKAGE_ITEM_ETC_TRADE_EU_01", TabIndex = 0, SlotIndex = 0, EnglishName = "Linen" } },
        { "NPC_CA_SPECIAL", new SpecialtyGoodsInfo { ItemCode = "ITEM_ETC_TRADE_CA_01", PackageCode = "PACKAGE_ITEM_ETC_TRADE_CA_01", TabIndex = 0, SlotIndex = 0, EnglishName = "Carpet" } },
        { "NPC_TK_SPECIAL", new SpecialtyGoodsInfo { ItemCode = "ITEM_ETC_TRADE_TK_01", PackageCode = "PACKAGE_ITEM_ETC_TRADE_TK_01", TabIndex = 0, SlotIndex = 0, EnglishName = "Red Sand" } },
        { "NPC_SD_M_AREA_SPECIAL", new SpecialtyGoodsInfo { ItemCode = "ITEM_ETC_TRADE_SD_01", PackageCode = "PACKAGE_ITEM_ETC_TRADE_SD_01", TabIndex = 0, SlotIndex = 0, EnglishName = "Alexandria Goods 1" } },
        { "NPC_SD_M_AREA_SPECIAL_2", new SpecialtyGoodsInfo { ItemCode = "ITEM_ETC_TRADE_VE_01", PackageCode = "PACKAGE_ITEM_ETC_TRADE_VE_01", TabIndex = 0, SlotIndex = 0, EnglishName = "Alexandria Goods 2" } },
        { "NPC_SD_M_AREA_SPECIAL_3", new SpecialtyGoodsInfo { ItemCode = "ITEM_ETC_TRADE_DE_01", PackageCode = "PACKAGE_ITEM_ETC_TRADE_DE_01", TabIndex = 0, SlotIndex = 0, EnglishName = "Alexandria Goods 3" } },
    };

    private static int CountTradeGoodsInTransport(Cos transport, string specificCode = null)
    {
        if (transport?.Inventory == null) return 0;
        return transport.Inventory
            .Where(i => i != null && i.Record != null &&
                        (i.Record.CodeName.StartsWith("ITEM_ETC_TRADE_", StringComparison.OrdinalIgnoreCase) ||
                         (i.Record.TypeID1 == 3 && i.Record.TypeID2 == 3 && i.Record.TypeID3 == 8) ||
                         (!string.IsNullOrEmpty(specificCode) && string.Equals(i.Record.CodeName, specificCode, StringComparison.OrdinalIgnoreCase))))
            .Sum(i => (int)i.Amount);
    }

    #endregion

    /// <summary>
    ///     Buys specialty goods from the selected merchant.
    /// </summary>
    private void BuyGoods(int targetQuantity, bool isFull)
    {
        var npc = (Game.SelectedEntity as SpawnedNpcNpc)
               ?? (ShoppingManager.SelectedEntity as SpawnedNpcNpc)
               ?? (Game.Player?.State?.DialogState?.Npc as SpawnedNpcNpc);

        if (npc?.Record == null)
        {
            Log.Warn("[Trade] تعذر الشراء: لم يتم العثور على تاجر البضائع المحدد.");
            return;
        }

        var npcCode = npc.Record.CodeName ?? string.Empty;
        SpecialtyGoodsMap.TryGetValue(npcCode, out var fallbackGoods);

        int tabIndex = 0xFF;
        byte slotIndex = 0;
        string itemCodeName = fallbackGoods?.ItemCode ?? "ITEM_ETC_TRADE_WC_01";
        string packageCodeName = fallbackGoods?.PackageCode ?? "PACKAGE_ITEM_ETC_TRADE_WC_01";
        string displayName = fallbackGoods?.EnglishName ?? "Specialty Goods";

        var preferredMap = TradeConfig.GetPreferredGoodsDictionary();
        var shopGroup = Game.ReferenceManager.GetRefShopGroup(npcCode);
        if (shopGroup != null)
        {
            var shopGoods = Game.ReferenceManager.GetRefShopGoods(shopGroup);
            if (shopGoods != null && shopGoods.Count > 0)
            {
                RefShopGood selectedGood = null;
                int preferredQty = 0;

                // 1. Look for preferred good selected by the user
                foreach (var good in shopGoods)
                {
                    if (preferredMap.TryGetValue(good.RefPackageItemCodeName, out var pref) && pref.Buy)
                    {
                        selectedGood = good;
                        preferredQty = pref.Quantity;
                        break;
                    }
                }

                // 2. If none explicitly checked, take the first available specialty good
                if (selectedGood == null)
                    selectedGood = shopGoods.FirstOrDefault();

                if (selectedGood != null)
                {
                    var dynamicTab = Game.ReferenceManager.GetRefShopGoodTabIndex(npcCode, selectedGood);
                    if (dynamicTab != 0xFF)
                    {
                        tabIndex = dynamicTab;
                        slotIndex = selectedGood.SlotIndex;
                        packageCodeName = selectedGood.RefPackageItemCodeName;

                        var pkg = Game.ReferenceManager.GetRefPackageItem(packageCodeName);
                        if (pkg?.RefItem != null)
                        {
                            itemCodeName = pkg.RefItem.CodeName;
                            displayName = pkg.RefItem.GetRealName() ?? pkg.RefItem.CodeName;
                        }

                        if (!isFull && preferredQty > 0 && targetQuantity <= 0)
                        {
                            targetQuantity = preferredQty;
                        }
                    }
                }
            }
        }

        if (tabIndex == 0xFF)
        {
            if (fallbackGoods != null)
            {
                tabIndex = fallbackGoods.TabIndex;
                slotIndex = fallbackGoods.SlotIndex;
                itemCodeName = fallbackGoods.ItemCode;
                packageCodeName = fallbackGoods.PackageCode;
                displayName = fallbackGoods.EnglishName;
                Log.Notify($"[Trade] استخدام الإعداد المرجعي الافتراضي للتاجر [{npcCode}]: سلعة [{displayName}] (تبويب {tabIndex}، خانة {slotIndex}).");
            }
            else
            {
                Log.Warn($"[Trade] تعذر الشراء: لم يتم العثور على تبويب السلعة في متجر [{npcCode}].");
                return;
            }
        }

        var transport = Game.Player.JobTransport;
        if (transport == null)
        {
            Log.Warn("[Trade] تعذر الشراء: وسيلة النقل غير متوفرة.");
            return;
        }

        const ushort MaxStack = 1000;
        const long PricePerUnit = 10000; // 10,000 Gold per unit across all towns

        if (isFull || targetQuantity <= 0)
        {
            targetQuantity = Math.Max(5000, (transport.Inventory?.Capacity ?? 5) * MaxStack);
        }

        var playerGold = Game.Player?.Gold ?? 0;
        var maxAffordable = (int)(playerGold / PricePerUnit);

        Log.Notify($"[Trade] بدء شراء السلعة [{displayName}] - الكمية المستهدفة: {targetQuantity} وحدة (الرصيد: {playerGold:N0} ذهبة، أقصى كمية يمكن شراؤها بالرصيد: {maxAffordable:N0} وحدة)...");
        Log.Debug($"[Trade] بيانات المتجر: NPC={npcCode}, Tab={tabIndex}, Slot={slotIndex}, Item={itemCodeName}, TransportUniqueId={transport.UniqueId}");

        if (maxAffordable < MaxStack)
        {
            Log.Warn($"[Trade] تحذير: رصيد الذهب لدى اللاعب ({playerGold:N0} ذهبة) لا يكفي لشراء رزمة واحدة من بضاعة التجارة (الرزمة 1000 وحدة بسعر 10,000,000 ذهبة)!");
            return;
        }

        var safetyLimit = 35;
        var failedAttempts = 0;

        while (safetyLimit-- > 0)
        {
            transport = Game.Player.JobTransport;
            if (transport?.Inventory == null)
                break;

            if (transport.Inventory.Full)
            {
                Log.Notify("[Trade] حقيبة الجمل ممتلئة بالكامل بجميع خانات البضاعة!");
                break;
            }

            var currentQuantity = CountTradeGoodsInTransport(transport, itemCodeName);
            if (!isFull && currentQuantity >= targetQuantity)
            {
                Log.Notify($"[Trade] تم شراء كامل بضاعة التجارة بنجاح ({currentQuantity} وحدة)! الحمولة 5 نجوم جاهزة.");
                break;
            }

            playerGold = Game.Player?.Gold ?? 0;
            maxAffordable = (int)(playerGold / PricePerUnit);
            if (maxAffordable < MaxStack)
            {
                Log.Warn($"[Trade] نفد الذهب الكافي للاستمرار في الشراء (الرصيد الحالي: {playerGold:N0} ذهبة).");
                break;
            }

            var needed = targetQuantity - currentQuantity;
            if (needed <= 0 && !isFull)
                break;

            // In Silkroad specialty trade, goods MUST be bought in full stack multiples (1000 units per transaction)
            ushort buyNextQty = MaxStack;

            Log.Notify($"[Trade] إرسال طلب شراء {buyNextQty} وحدة (الإجمالي الحالي: {currentQuantity}/{targetQuantity})...");

            var confirmed = ShoppingManager.PurchaseItem(transport, (int)tabIndex, (int)slotIndex, buyNextQty);

            Thread.Sleep(600); // Wait for server confirmation and inventory update

            var newQuantity = CountTradeGoodsInTransport(Game.Player?.JobTransport ?? transport, itemCodeName);

            if (newQuantity > currentQuantity || confirmed)
            {
                failedAttempts = 0;
                var boughtUnits = (newQuantity > currentQuantity) ? (newQuantity - currentQuantity) : (int)buyNextQty;
                Log.Notify($"[Trade] تم شراء {boughtUnits} وحدة بنجاح (الإجمالي الحالي: {newQuantity}/{targetQuantity})");

                if (transport.Inventory != null && transport.Inventory.Full)
                {
                    Log.Notify("[Trade] حقيبة الجمل ممتلئة بالكامل بجميع خانات البضاعة!");
                    break;
                }

                if (!isFull && newQuantity >= targetQuantity)
                {
                    Log.Notify($"[Trade] تم اكتمال شراء بضاعة التجارة بنجاح ({newQuantity}/{targetQuantity} وحدة)! الحمولة 5 نجوم جاهزة.");
                    break;
                }
            }
            else
            {
                failedAttempts++;
                Log.Warn($"[Trade] لم يتم تأكيد شراء البضاعة من السيرفر (محاولة {failedAttempts}/4، الإجمالي الحالي: {newQuantity}).");

                if (failedAttempts >= 4)
                {
                    Log.Warn("[Trade] تم إيقاف الشراء بعد 4 محاولات غير مؤكدة لتجنب انقطاع الاتصال بالسيرفر.");
                    break;
                }

                Thread.Sleep(1000);
            }
        }

        var finalLoaded = CountTradeGoodsInTransport(Game.Player?.JobTransport, itemCodeName);
        Log.Notify($"[Trade] اكتمال عملية الشراء. إجمالي البضاعة المحمّلة بالجمل: {finalLoaded} وحدة.");
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
