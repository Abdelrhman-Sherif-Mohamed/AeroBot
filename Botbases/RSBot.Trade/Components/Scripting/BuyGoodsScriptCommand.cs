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
            var liveDistance = npc.Position.DistanceToPlayer();
            if (liveDistance > 2.5f)
            {
                Log.Notify($"[Trade] الاقتراب من تاجر البضائع [{npc.Record.GetRealName()}] (المسافة: {Math.Round(liveDistance, 1)}م)...");
                Game.Player?.MoveTo(npc.Position, sleep: false);
                for (var w = 0; w < 40; w++)
                {
                    if (npc.Position.DistanceToPlayer() <= 2.5f)
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

            // In Silkroad Online / vSRO, players MUST REMAIN MOUNTED on their trade transport (Camel)
            // to purchase specialty trade goods. Dismounting separates the player and triggers error 0x1C0D.
            if (!EnsureMountedOnTransport())
                Log.Warn("[Trade] تعذر امتطاء الجمل قبل الشراء! الشراء قد يرفضه السيرفر (0x1C0D).");

            Thread.Sleep(300);

            ShoppingManager.Running = true;
            ShoppingManager.BotOpenedDialog = true;

            // Open shop dialog with verification & retries. ChooseTalkOption always
            // performs the full client sequence (0x7045 select -> 0xB045 -> 0x704B
            // -> 0x7046), so do not pre-set the selected entity here.
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

                var retryDist = npc.Position.DistanceToPlayer();
                if (retryDist > 2.5f)
                {
                    Game.Player?.MoveTo(npc.Position, sleep: false);
                    for (var w = 0; w < 20; w++)
                    {
                        if (npc.Position.DistanceToPlayer() <= 2.5f)
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

            // Settle shop window state before item transactions
            Thread.Sleep(600);

            var activeEntity = (Game.SelectedEntity as SpawnedNpcNpc) ?? (ShoppingManager.SelectedEntity as SpawnedNpcNpc) ?? npc;
            Game.SelectedEntity = activeEntity;
            ShoppingManager.SelectedEntity = activeEntity;

            if (TradeConfig.SellGoods)
            {
                Log.Notify($"[Trade] جاري فحص وبيع بضائع التجارة القديمة لدى [{activeEntity.Record.GetRealName()}]...");
                SellGoods();
                Thread.Sleep(500);
            }

            // Always execute purchase because buy-goods was explicitly invoked in script
            Log.Notify($"[Trade] جاري شراء بضاعة التجارة من [{activeEntity.Record.GetRealName()}] (الوضع: {(isFull ? "Full - حمولة كاملة 5 نجوم" : $"{targetQuantity} وحدة")})...");
            BuyGoods(targetQuantity, isFull);

            // Settle transaction before closing dialog
            Thread.Sleep(600);

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
                TradeStats.OnRunStarted();
            }

            // Remount job transport if MountTransport option is enabled
            if (TradeConfig.MountTransport && Game.Player?.JobTransport != null && !Game.Player.HasActiveVehicle)
            {
                Log.Notify("[Trade] ركوب وسيلة النقل بعد اكتمال الشراء لمتابعة مسار التجارة...");
                Game.Player.JobTransport.Mount();
                Thread.Sleep(600);
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
        { "NPC_WC_SPECIAL2", new SpecialtyGoodsInfo { ItemCode = "ITEM_ETC_TRADE_WC_05", PackageCode = "PACKAGE_ITEM_ETC_TRADE_WC_05", TabIndex = 0, SlotIndex = 0, EnglishName = "Brown Pearl" } },
        { "NPC_KT_SPECIAL", new SpecialtyGoodsInfo { ItemCode = "ITEM_ETC_TRADE_KT_01", PackageCode = "PACKAGE_ITEM_ETC_TRADE_KT_01", TabIndex = 0, SlotIndex = 0, EnglishName = "Nephrite" } },
        { "NPC_CH_SPECIAL", new SpecialtyGoodsInfo { ItemCode = "ITEM_ETC_TRADE_CH_01", PackageCode = "PACKAGE_ITEM_ETC_TRADE_CH_01", TabIndex = 0, SlotIndex = 0, EnglishName = "White Silk" } },
        { "NPC_CH_SPECIAL2", new SpecialtyGoodsInfo { ItemCode = "ITEM_ETC_TRADE_CH_05", PackageCode = "PACKAGE_ITEM_ETC_TRADE_CH_05", TabIndex = 0, SlotIndex = 0, EnglishName = "Tiger Eye Stone" } },
        { "NPC_EU_SPECIAL", new SpecialtyGoodsInfo { ItemCode = "ITEM_ETC_TRADE_EU_01", PackageCode = "PACKAGE_ITEM_ETC_TRADE_EU_01", TabIndex = 0, SlotIndex = 0, EnglishName = "Linen" } },
        { "NPC_CA_SPECIAL", new SpecialtyGoodsInfo { ItemCode = "ITEM_ETC_TRADE_CA_01", PackageCode = "PACKAGE_ITEM_ETC_TRADE_CA_01", TabIndex = 0, SlotIndex = 0, EnglishName = "Wool" } },
        { "NPC_OA_SPECIAL", new SpecialtyGoodsInfo { ItemCode = "ITEM_ETC_TRADE_SD_01", PackageCode = "PACKAGE_ITEM_ETC_TRADE_SD_01", TabIndex = 0, SlotIndex = 0, EnglishName = "Incense" } },
        { "NPC_SD_M_AREA_SPECIAL", new SpecialtyGoodsInfo { ItemCode = "ITEM_ETC_TRADE_SD_01", PackageCode = "PACKAGE_ITEM_ETC_TRADE_SD_01", TabIndex = 0, SlotIndex = 0, EnglishName = "Incense" } },
        { "NPC_SD_M_AREA_SPECIAL2", new SpecialtyGoodsInfo { ItemCode = "ITEM_ETC_TRADE_VE_01", PackageCode = "PACKAGE_ITEM_ETC_TRADE_VE_01", TabIndex = 0, SlotIndex = 0, EnglishName = "Alabastar" } },
        { "NPC_SD_M_AREA_SPECIAL_2", new SpecialtyGoodsInfo { ItemCode = "ITEM_ETC_TRADE_VE_01", PackageCode = "PACKAGE_ITEM_ETC_TRADE_VE_01", TabIndex = 0, SlotIndex = 0, EnglishName = "Alabastar" } },
        { "NPC_SD_M_AREA_SPECIAL3", new SpecialtyGoodsInfo { ItemCode = "ITEM_ETC_TRADE_DE_01", PackageCode = "PACKAGE_ITEM_ETC_TRADE_DE_01", TabIndex = 0, SlotIndex = 0, EnglishName = "Lily" } },
        { "NPC_TK_SPECIAL", new SpecialtyGoodsInfo { ItemCode = "ITEM_ETC_TRADE_TK_01", PackageCode = "PACKAGE_ITEM_ETC_TRADE_TK_01", TabIndex = 0, SlotIndex = 0, EnglishName = "Red Sand" } },
        { "NPC_RM_SPECIAL", new SpecialtyGoodsInfo { ItemCode = "ITEM_ETC_TRADE_RM_01", PackageCode = "PACKAGE_ITEM_ETC_TRADE_RM_01", TabIndex = 0, SlotIndex = 0, EnglishName = "Wind Seed" } },
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

    /// <summary>
    ///     Makes sure the player is actually riding the job transport. The server
    ///     rejects specialty goods purchases (result 0x02 / error 0x1C0D) while the
    ///     player is on foot, so this has to be verified before every purchase.
    /// </summary>
    private static bool EnsureMountedOnTransport(int timeoutMs = 5000)
    {
        var transport = Game.Player?.JobTransport;
        if (transport == null || Game.Player == null)
            return false;

        if (IsMountedOn(transport))
            return true;

        var deadline = Environment.TickCount64 + timeoutMs;

        while (Environment.TickCount64 < deadline)
        {
            if (Game.Player.InAction || Game.Player.State?.BattleState == BattleState.InBattle)
            {
                Thread.Sleep(250);
                continue;
            }

            transport.Mount(1500);

            if (IsMountedOn(transport))
            {
                Log.Debug("[Trade] Player mounted on the job transport.");
                return true;
            }
        }

        return IsMountedOn(transport);
    }

    private static bool IsMountedOn(Cos transport) =>
        Game.Player != null && Game.Player.HasActiveVehicle &&
        Game.Player.Vehicle != null && Game.Player.Vehicle.UniqueId == transport.UniqueId;

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

            // The server only accepts the purchase while the player is riding the
            // transport (otherwise it answers 0x02 / error 0x1C0D and nothing is
            // bought). The player can get dismounted by a thief fight or a stuck
            // mount, so this is verified before every single stack.
            if (!EnsureMountedOnTransport(3000))
                Log.Warn("[Trade] تحذير: الجمل غير مُمتطى الآن! قد يرفض السيرفر عملية الشراء (0x1C0D).");

            var prevCount = CountTradeGoodsInTransport(Game.Player?.JobTransport ?? transport, itemCodeName);
            var confirmed = ShoppingManager.PurchaseItem(transport, (int)tabIndex, (int)slotIndex, buyNextQty);

            // Await inventory count increase from server up to 3 seconds (matching vBot)
            var bought = false;
            for (var w = 0; w < 30; w++)
            {
                Thread.Sleep(100);
                var nowCount = CountTradeGoodsInTransport(Game.Player?.JobTransport ?? transport, itemCodeName);
                if (nowCount > prevCount)
                {
                    bought = true;
                    break;
                }
            }

            var newQuantity = CountTradeGoodsInTransport(Game.Player?.JobTransport ?? transport, itemCodeName);

            if (bought || newQuantity > prevCount || confirmed)
            {
                failedAttempts = 0;
                var boughtUnits = (newQuantity > prevCount) ? (newQuantity - prevCount) : (int)buyNextQty;
                Log.Notify($"[Trade] تم شراء {boughtUnits} وحدة بنجاح (الإجمالي الحالي: {newQuantity}/{targetQuantity})");
                currentQuantity = newQuantity;

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

                Thread.Sleep(300); // polite pause between stacks
            }
            else
            {
                failedAttempts++;
                Log.Warn($"[Trade] لم يتم تأكيد شراء البضاعة من السيرفر (محاولة {failedAttempts}/3، الإجمالي الحالي: {newQuantity}).");

                var camelDistance = transport != null ? transport.Position.DistanceTo(npc.Position) : -1f;
                Log.Debug($"[Trade] تشخيص فشل الشراء: mounted={IsMountedOn(transport)}, " +
                          $"InAction={Game.Player?.InAction}, BattleState={Game.Player?.State?.BattleState}, " +
                          $"camelDistance={camelDistance:F1}, dialogNpc={Game.Player?.State?.DialogState?.Npc?.UniqueId}, " +
                          $"shopNpc={ShoppingManager.SelectedEntity?.UniqueId}");

                if (failedAttempts >= 3)
                {
                    Log.Warn("[Trade] تم إيقاف الشراء بعد 3 محاولات غير مؤكدة لتجنب انقطاع الاتصال بالسيرفر.");
                    break;
                }

                // The server silently closes the shop session when a transaction is
                // rejected or when the player gets separated from the transport.
                // Re-open the dialog exactly like the game client does before retrying,
                // otherwise every new attempt keeps failing with 0x1C0D.
                EnsureMountedOnTransport(2000);

                if (!ShoppingManager.ChooseTalkOption(npc.Record.CodeName, TalkOption.Trade))
                    Log.Debug("[Trade] تعذر إعادة فتح نافذة المتجر قبل إعادة المحاولة.");

                Thread.Sleep(800);
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
