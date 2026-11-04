using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using RSBot.Core.Network;
using RSBot.Core.Objects;
using RSBot.Core.Objects.Spawn;

namespace RSBot.Core.Components;

public static class HappyManager
{
    private static readonly object _syncLock = new();
    private static volatile bool _cancelRequested;

    /// <summary>
    /// Gets a value indicating whether HappyManager is currently executing.
    /// </summary>
    public static bool Running { get; private set; }

    /// <summary>
    /// Holds counts of event letters (H, A, P, Y).
    /// </summary>
    public struct LetterInfo
    {
        public int H;
        public int A;
        public int P1;
        public int P2;
        public int OtherP;
        public int TotalP => P1 + P2 + OtherP;
        public int Y;
        public int Total => H + A + TotalP + Y;

        /// <summary>
        /// Checks if all letters are available to make an exchange.
        /// </summary>
        public bool CanExchange
        {
            get
            {
                if (H <= 0 || A <= 0 || Y <= 0)
                    return false;

                if (P1 > 0 && P2 > 0)
                    return true;

                return TotalP >= 2;
            }
        }

        /// <summary>
        /// Calculates the maximum number of exchanges possible with current letters.
        /// </summary>
        public int MaxPossibleExchanges
        {
            get
            {
                if (!CanExchange) return 0;
                var pCount = (P1 > 0 && P2 > 0) ? Math.Min(P1, P2) : TotalP / 2;
                return Math.Min(H, Math.Min(A, Math.Min(Y, pCount)));
            }
        }

        public string GetMissingDescription()
        {
            var missing = new List<string>();
            if (H <= 0) missing.Add("H");
            if (A <= 0) missing.Add("A");
            if (TotalP < 2 && !(P1 > 0 && P2 > 0)) missing.Add("P");
            if (Y <= 0) missing.Add("Y");
            return string.Join(", ", missing);
        }
    }

    /// <summary>
    /// Counts event letters in player's inventory safely.
    /// </summary>
    /// <summary>
    /// Gets the count of free slots in NormalPart of player's inventory.
    /// </summary>
    public static int GetNormalInventoryFreeSlots()
    {
        if (Game.Player?.Inventory == null) return 0;
        var inv = Game.Player.Inventory;
        return Math.Max(0, (int)inv.NormalPartSize - inv.GetNormalPartItems().Count);
    }

    public static LetterInfo GetLetterCounts()
    {
        var info = new LetterInfo();
        if (Game.Player?.Inventory == null)
            return info;

        List<InventoryItem> items;
        try
        {
            items = Game.Player.Inventory.ToList();
        }
        catch
        {
            Thread.Sleep(30);
            try { items = Game.Player.Inventory.ToList(); }
            catch { return info; }
        }

        foreach (var item in items)
        {
            if (item?.Record == null || item.Slot < 13)
                continue;

            var code = item.Record.CodeName ?? string.Empty;
            var name = item.Record.GetRealName() ?? string.Empty;

            var isLetter = code.Contains("HAPPY", StringComparison.OrdinalIgnoreCase) ||
                           code.Contains("LETTER", StringComparison.OrdinalIgnoreCase) ||
                           code.Contains("E060529", StringComparison.OrdinalIgnoreCase) ||
                           code.StartsWith("ITEM_ETC_E", StringComparison.OrdinalIgnoreCase) ||
                           name.Contains("Letter", StringComparison.OrdinalIgnoreCase) ||
                           name.Contains("Happy", StringComparison.OrdinalIgnoreCase) ||
                           name.Contains("حرف", StringComparison.OrdinalIgnoreCase);

            if (isLetter)
            {
                if (code.EndsWith("_H", StringComparison.OrdinalIgnoreCase) ||
                    code.Contains("_H_", StringComparison.OrdinalIgnoreCase) ||
                    name.EndsWith("(H)", StringComparison.OrdinalIgnoreCase) ||
                    name.Contains("Letter H", StringComparison.OrdinalIgnoreCase) ||
                    name.Contains("Letter (H)", StringComparison.OrdinalIgnoreCase) ||
                    name.EndsWith(" H", StringComparison.OrdinalIgnoreCase))
                {
                    info.H += item.Amount;
                }
                else if (code.EndsWith("_A", StringComparison.OrdinalIgnoreCase) ||
                         code.Contains("_A_", StringComparison.OrdinalIgnoreCase) ||
                         name.EndsWith("(A)", StringComparison.OrdinalIgnoreCase) ||
                         name.Contains("Letter A", StringComparison.OrdinalIgnoreCase) ||
                         name.Contains("Letter (A)", StringComparison.OrdinalIgnoreCase) ||
                         name.EndsWith(" A", StringComparison.OrdinalIgnoreCase))
                {
                    info.A += item.Amount;
                }
                else if (code.EndsWith("_P_2", StringComparison.OrdinalIgnoreCase) ||
                         code.Contains("_P_2", StringComparison.OrdinalIgnoreCase) ||
                         code.EndsWith("_P2", StringComparison.OrdinalIgnoreCase) ||
                         code.Contains("_P2_", StringComparison.OrdinalIgnoreCase) ||
                         code.EndsWith("_P_02", StringComparison.OrdinalIgnoreCase) ||
                         name.Contains("Letter P(2)", StringComparison.OrdinalIgnoreCase) ||
                         name.Contains("Letter P (2)", StringComparison.OrdinalIgnoreCase) ||
                         name.Contains("Letter [P] 2", StringComparison.OrdinalIgnoreCase) ||
                         name.EndsWith("(P2)", StringComparison.OrdinalIgnoreCase) ||
                         name.EndsWith(" P2", StringComparison.OrdinalIgnoreCase))
                {
                    info.P2 += item.Amount;
                }
                else if (code.EndsWith("_P", StringComparison.OrdinalIgnoreCase) ||
                         code.Contains("_P_", StringComparison.OrdinalIgnoreCase) ||
                         code.EndsWith("_P1", StringComparison.OrdinalIgnoreCase) ||
                         code.Contains("_P1_", StringComparison.OrdinalIgnoreCase) ||
                         code.EndsWith("_P_1", StringComparison.OrdinalIgnoreCase) ||
                         code.Contains("_P_1_", StringComparison.OrdinalIgnoreCase) ||
                         name.EndsWith("(P)", StringComparison.OrdinalIgnoreCase) ||
                         name.Contains("Letter P", StringComparison.OrdinalIgnoreCase) ||
                         name.Contains("Letter (P)", StringComparison.OrdinalIgnoreCase) ||
                         name.EndsWith(" P", StringComparison.OrdinalIgnoreCase) ||
                         name.EndsWith("(P1)", StringComparison.OrdinalIgnoreCase))
                {
                    info.P1 += item.Amount;
                }
                else if (code.EndsWith("_Y", StringComparison.OrdinalIgnoreCase) ||
                         code.Contains("_Y_", StringComparison.OrdinalIgnoreCase) ||
                         name.EndsWith("(Y)", StringComparison.OrdinalIgnoreCase) ||
                         name.Contains("Letter Y", StringComparison.OrdinalIgnoreCase) ||
                         name.Contains("Letter (Y)", StringComparison.OrdinalIgnoreCase) ||
                         name.EndsWith(" Y", StringComparison.OrdinalIgnoreCase))
                {
                    info.Y += item.Amount;
                }
                else if (code.Contains("_P", StringComparison.OrdinalIgnoreCase) || name.Contains(" P", StringComparison.OrdinalIgnoreCase))
                {
                    info.OtherP += item.Amount;
                }
            }
        }

        return info;
    }

    /// <summary>
    /// Counts event Jewel Boxes in player's inventory safely.
    /// </summary>
    public static int GetJewelBoxCount()
    {
        if (Game.Player?.Inventory == null)
            return 0;

        List<InventoryItem> items;
        try
        {
            items = Game.Player.Inventory.ToList();
        }
        catch
        {
            Thread.Sleep(30);
            try { items = Game.Player.Inventory.ToList(); }
            catch { return 0; }
        }

        var count = 0;
        foreach (var item in items)
        {
            if (item?.Record == null || item.Slot < 13)
                continue;

            var code = item.Record.CodeName ?? string.Empty;
            var name = item.Record.GetRealName() ?? string.Empty;

            if (code.Contains("JEWELBOX", StringComparison.OrdinalIgnoreCase) ||
                code.Contains("JEWEL_BOX", StringComparison.OrdinalIgnoreCase) ||
                name.Contains("Jewel Box", StringComparison.OrdinalIgnoreCase) ||
                name.Contains("صندوق الجواهر", StringComparison.OrdinalIgnoreCase))
            {
                count += item.Amount;
            }
        }
        return count;
    }

    /// <summary>
    /// Checks if the given NPC is Event So-Ok.
    /// </summary>
    public static bool IsEventSoOk(SpawnedNpc npc)
    {
        if (npc?.Record == null)
            return false;

        var codeName = npc.Record.CodeName ?? string.Empty;
        var realName = npc.Record.GetRealName() ?? string.Empty;

        if (codeName.IndexOf("SOOK", StringComparison.OrdinalIgnoreCase) >= 0 ||
            codeName.IndexOf("SO_OK", StringComparison.OrdinalIgnoreCase) >= 0 ||
            codeName.IndexOf("SO-OK", StringComparison.OrdinalIgnoreCase) >= 0 ||
            codeName.IndexOf("KISAENG", StringComparison.OrdinalIgnoreCase) >= 0)
            return true;

        if (realName.IndexOf("So-Ok", StringComparison.OrdinalIgnoreCase) >= 0 ||
            realName.IndexOf("So - Ok", StringComparison.OrdinalIgnoreCase) >= 0 ||
            realName.IndexOf("SO OK", StringComparison.OrdinalIgnoreCase) >= 0 ||
            realName.IndexOf("SoOk", StringComparison.OrdinalIgnoreCase) >= 0 ||
            realName.Contains("سو أوك") || realName.Contains("سوك") || realName.Contains("إيفنت"))
            return true;

        if (codeName.EndsWith("_EVENT", StringComparison.OrdinalIgnoreCase) ||
            codeName.Contains("_EVENT_", StringComparison.OrdinalIgnoreCase) ||
            (codeName.StartsWith("NPC_") && codeName.Contains("EVENT", StringComparison.OrdinalIgnoreCase)))
            return true;

        return false;
    }

    /// <summary>
    /// Finds the nearest Event So-Ok NPC.
    /// </summary>
    public static SpawnedNpc GetEventSoOkNpc()
    {
        if (Game.SelectedEntity is SpawnedNpc selected && IsEventSoOk(selected))
            return selected;

        if (!SpawnManager.TryGetEntities<SpawnedNpc>(out var npcs))
            return null;

        return npcs.Where(IsEventSoOk)
                   .OrderBy(n => n.DistanceToPlayer)
                   .FirstOrDefault();
    }

    /// <summary>
    /// Stops HappyManager execution.
    /// </summary>
    public static void Stop()
    {
        if (Running)
        {
            var isAr = RSBot.Core.Kernel.Language == "ar_AR";
            Log.Notify(isAr ? "[Happy] تم طلب إيقاف التبديل. جارٍ الإنهاء..." : "[Happy] Stop requested. Finishing...");
            _cancelRequested = true;
        }
    }

    /// <summary>
    /// Runs the Happy Event interaction.
    /// </summary>
    public static void Run(bool runInBackground = true)
    {
        if (runInBackground)
            Task.Run(() => Execute());
        else
            Execute();
    }

    /// <summary>
    /// Executes fast, continuous, and seamless auto-exchange with Event So-Ok.
    /// Matches the exact official 4-click sequence:
    /// Click 1: 0x7046 (Option 2) -> Open Talk
    /// Click 2: 0x30D4 (Action 0x05) -> Select Happy Event
    /// Click 3: 0x30D4 (Action 0x05) -> Redeem Letters (Reward received here)
    /// Click 4: 0x30D4 (Action 0x05) -> Close Dialog (Server replies with 0xB04B 01)
    /// </summary>
    public static bool Execute()
    {
        lock (_syncLock)
        {
            if (Running)
            {
                Log.Warn("[Happy] Happy manager is already running.");
                return false;
            }

            Running = true;
            _cancelRequested = false;
        }

        var isAr = RSBot.Core.Kernel.Language == "ar_AR";

        try
        {
            Log.Notify(isAr ? "[Happy] جارٍ البحث عن NPC Event So-Ok..." : "[Happy] Searching for Event So-Ok NPC...");

            var npc = GetEventSoOkNpc();
            if (npc == null)
            {
                for (var i = 0; i < 6 && npc == null; i++)
                {
                    Thread.Sleep(300);
                    npc = GetEventSoOkNpc();
                }

                if (npc == null)
                {
                    Log.Warn(isAr ? "[Happy] لم يتم العثور على NPC Event So-Ok في المحيط. تأكد من الوقوف بجانبه." : "[Happy] Event So-Ok NPC not found nearby. Stand close to So-Ok.");
                    return false;
                }
            }

            if (npc.DistanceToPlayer > 15.0f)
            {
                Log.Warn(isAr ? $"[Happy] الشخصية بعيدة عن So-Ok ({Math.Round(npc.DistanceToPlayer, 1)}م). يرجى الاقتراب." : $"[Happy] Too far from So-Ok ({Math.Round(npc.DistanceToPlayer, 1)}m). Move closer.");
                return false;
            }

            var initialLetters = GetLetterCounts();
            var initialJewelBoxes = GetJewelBoxCount();
            var isLetterEvent = initialLetters.Total > 0;
            var isJewelEvent = !isLetterEvent && initialJewelBoxes >= 10;

            if (!isLetterEvent && !isJewelEvent)
            {
                Log.Warn(isAr ? "[Happy] لا توجد حروف H-A-P-P-Y أو صناديق جواهر في الشنطة للتبديل." : "[Happy] No H-A-P-P-Y letters or jewel boxes in inventory.");
                return false;
            }

            if (isLetterEvent && !initialLetters.CanExchange)
            {
                Log.Warn(isAr ? $"[Happy] الحروف غير مكتملة ({initialLetters.GetMissingDescription()}). H:{initialLetters.H}, A:{initialLetters.A}, P:{initialLetters.TotalP}, Y:{initialLetters.Y}." : $"[Happy] Incomplete letter set ({initialLetters.GetMissingDescription()}). H:{initialLetters.H}, A:{initialLetters.A}, P:{initialLetters.TotalP}, Y:{initialLetters.Y}.");
                return false;
            }

            const int maxCycles = 5000;
            var cyclesDone = 0;
            var consecutiveErrors = 0;

            // Clear any lingering callbacks & ensure fresh state
            ClearCallbacks();
            CloseAndDeselect(npc.UniqueId, 250);
            Thread.Sleep(80);

            for (var cycle = 0; cycle < maxCycles; cycle++)
            {
                if (_cancelRequested)
                {
                    Log.Notify(isAr ? "[Happy] تم إيقاف التبديل بناءً على طلبك." : "[Happy] Exchange stopped by user.");
                    break;
                }

                if (Game.Player == null || Game.Player.Inventory == null)
                    break;

                // Check if normal inventory has free slots for rewards
                if (GetNormalInventoryFreeSlots() == 0)
                {
                    Log.Notify(isAr ? "[Happy] الشنطة ممتلئة! جارٍ دمج الأكوام المتشابهة لتوفير مساحة..." : "[Happy] Inventory full! Merging duplicate stacks to free space...");
                    Game.Player.Inventory.Sort();
                    var fullWait = 0;
                    while (Game.Player.Inventory.IsSorting && fullWait++ < 25 && !_cancelRequested)
                        Thread.Sleep(100);

                    if (GetNormalInventoryFreeSlots() == 0)
                    {
                        Log.Warn(isAr ? "[Happy] الشنطة ممتلئة بالكامل (0 خانة فارغة). يجب تفريغ خانة واحدة على الأقل لاستلام الجوائز من So-Ok. تم التوقف." : "[Happy] Inventory is completely full (0 free slots). Please free at least 1 slot to receive rewards. Stopping.");
                        break;
                    }
                }

                // Check remaining letters / jewel boxes before attempting exchange
                if (isLetterEvent)
                {
                    var currentLetters = GetLetterCounts();
                    if (!currentLetters.CanExchange)
                    {
                        var autoLoop = PlayerConfig.Get<bool>("RSBot.Inventory.Happy", false);
                        if (autoLoop && !_cancelRequested)
                        {
                            Thread.Sleep(2000);
                            currentLetters = GetLetterCounts();
                            if (!currentLetters.CanExchange)
                            {
                                Thread.Sleep(2000);
                                continue;
                            }
                        }
                        else
                        {
                            Log.Notify(isAr ? $"[Happy] انتهت الحروف المكتملة ({currentLetters.GetMissingDescription()}). إجمالي التبديلات: {cyclesDone}. (H:{currentLetters.H}, A:{currentLetters.A}, P:{currentLetters.TotalP}, Y:{currentLetters.Y})" : $"[Happy] Complete letter sets finished ({currentLetters.GetMissingDescription()}). Total exchanges: {cyclesDone}. (H:{currentLetters.H}, A:{currentLetters.A}, P:{currentLetters.TotalP}, Y:{currentLetters.Y})");
                            break;
                        }
                    }
                }
                else if (isJewelEvent)
                {
                    var currentJewels = GetJewelBoxCount();
                    if (currentJewels < 10)
                    {
                        var autoLoop = PlayerConfig.Get<bool>("RSBot.Inventory.Happy", false);
                        if (autoLoop && !_cancelRequested)
                        {
                            Thread.Sleep(2000);
                            currentJewels = GetJewelBoxCount();
                            if (currentJewels < 10)
                            {
                                Thread.Sleep(2000);
                                continue;
                            }
                        }
                        else
                        {
                            Log.Notify(isAr ? $"[Happy] انتهت صناديق الجواهر (المتبقي: {currentJewels}). إجمالي التبديلات: {cyclesDone}." : $"[Happy] Jewel boxes finished (Remaining: {currentJewels}). Total exchanges: {cyclesDone}.");
                            break;
                        }
                    }
                }

                npc = GetEventSoOkNpc() ?? npc;

                // Step 0: Ensure So-Ok is selected (0x7045)
                if (Game.SelectedEntity?.UniqueId != npc.UniqueId)
                {
                    SendSelectEntity(npc.UniqueId, 600);
                    Game.SelectedEntity = npc;
                    Thread.Sleep(25);
                }

                if (_cancelRequested) break;

                // Delay before talking to NPC
                var delaySec = PlayerConfig.Get<int>("RSBot.Inventory.HappyDelay", 1);
                if (delaySec > 0)
                {
                    var delayWait = 0;
                    while (delayWait < delaySec * 10 && !_cancelRequested)
                    {
                        Thread.Sleep(100);
                        delayWait++;
                    }
                }

                if (_cancelRequested) break;

                // Step 1 (Click 1): Open NPC talk option (0x7046 Option 2)
                var talkSuccess = SendTalkOption(npc.UniqueId, 2, 800);
                if (!talkSuccess)
                {
                    Thread.Sleep(50);
                    SendSelectEntity(npc.UniqueId, 500);
                    Game.SelectedEntity = npc;
                    Thread.Sleep(25);
                    talkSuccess = SendTalkOption(npc.UniqueId, 2, 800);

                    if (!talkSuccess)
                    {
                        consecutiveErrors++;
                        if (consecutiveErrors >= 3)
                        {
                            Log.Warn(isAr ? "[Happy] فشل فتح محادثة So-Ok ثلاث مرات متتالية. تم التوقف." : "[Happy] Failed to open So-Ok talk 3 times. Stopping.");
                            break;
                        }
                        Thread.Sleep(150);
                        continue;
                    }
                }

                Thread.Sleep(30);
                if (_cancelRequested) { SendCloseDialog(npc.UniqueId); break; }

                // Step 2 (Click 2): Select Happy Event in Dialog (0x30D4 Action 0x05)
                var (step2Success, step2InvFull) = SendDialogAction(0x05, 800);
                if (!step2Success)
                {
                    consecutiveErrors++;
                    SendCloseDialog(npc.UniqueId);
                    if (consecutiveErrors >= 3)
                    {
                        Log.Warn(isAr ? "[Happy] تعذر متابعة حوار So-Ok. تم التوقف." : "[Happy] Could not continue So-Ok dialog. Stopped.");
                        break;
                    }
                    Thread.Sleep(150);
                    continue;
                }
                Thread.Sleep(30);

                if (_cancelRequested) { SendCloseDialog(npc.UniqueId); break; }

                // Step 3 (Click 3): Redeem Letters / Get Reward (0x30D4 Action 0x05)
                // Server deducts letters and gives the reward item right here!
                var (step3Success, step3InvFull) = SendDialogAction(0x05, 800);
                if (step3InvFull)
                {
                    Log.Warn(isAr ? "[Happy] الشنطة ممتلئة ولا تتسع للمزيد من الجوائز! جارٍ دمج الأكوام المتشابهة..." : "[Happy] Inventory full! Merging duplicate stacks...");
                    SendCloseDialog(npc.UniqueId);
                    Game.Player.Inventory.Sort();
                    var fullWait = 0;
                    while (Game.Player.Inventory.IsSorting && fullWait++ < 25 && !_cancelRequested)
                        Thread.Sleep(100);

                    if (GetNormalInventoryFreeSlots() == 0)
                    {
                        Log.Warn(isAr ? "[Happy] الشنطة ممتلئة بالكامل ولا توجد خانة فارغة لاستلام الجوائز. تم التوقف بنجاح." : "[Happy] Inventory is completely full with no free slot for rewards. Stopped.");
                        break;
                    }
                    Thread.Sleep(100);
                    continue;
                }

                if (!step3Success)
                {
                    consecutiveErrors++;
                    SendCloseDialog(npc.UniqueId);
                    if (consecutiveErrors >= 3)
                    {
                        Log.Warn(isAr ? "[Happy] فشل استبدال الهابي من السيرفر. تم التوقف." : "[Happy] Failed to exchange letters from server. Stopped.");
                        break;
                    }
                    Thread.Sleep(150);
                    continue;
                }
                Thread.Sleep(30);

                if (_cancelRequested) { SendCloseDialog(npc.UniqueId); break; }

                // Step 4 (Click 4): Click Exit in Dialog (0x30D4 Action 0x05)
                // Server automatically closes dialog and replies with 0xB04B 01
                SendDialogCloseAction(0x05, 800);

                // Dialog is now closed by server (0xB04B). Reset local dialog state:
                if (Game.Player?.State?.DialogState != null)
                    Game.Player.State.DialogState = null;

                cyclesDone++;
                consecutiveErrors = 0;

                var remLetters = isLetterEvent ? GetLetterCounts() : default;
                if (isLetterEvent)
                {
                    if (cyclesDone % 5 == 0 || !remLetters.CanExchange)
                        Log.Notify(isAr ? $"[Happy] تم الاستبدال (#{cyclesDone}). الحروف المتبقية: H:{remLetters.H}, A:{remLetters.A}, P:{remLetters.TotalP}, Y:{remLetters.Y}" : $"[Happy] Exchanged (#{cyclesDone}). Remaining: H:{remLetters.H}, A:{remLetters.A}, P:{remLetters.TotalP}, Y:{remLetters.Y}");
                }
                else
                {
                    var remJewels = GetJewelBoxCount();
                    if (cyclesDone % 5 == 0 || remJewels < 10)
                        Log.Notify(isAr ? $"[Happy] تم الاستبدال (#{cyclesDone}) بصناديق الجواهر (المتبقي: {remJewels})." : $"[Happy] Exchanged (#{cyclesDone}) with jewel boxes (Remaining: {remJewels}).");
                }

                // Fast and safe delay between consecutive turns
                Thread.Sleep(40);
            }

            // Cleanup when loop finishes
            CloseAndDeselect(npc.UniqueId, 250);
            Thread.Sleep(100);

            Log.Notify(isAr ? $"[Happy] تم الانتهاء بنجاح! تم استبدال {cyclesDone} مجموعة بنجاح." : $"[Happy] Finished successfully! Exchanged {cyclesDone} sets.");

            // Final Step: Execute Auto Sort ONCE at the very end to organize all rewards cleanly
            if (cyclesDone > 0 && Game.Player?.Inventory != null)
            {
                Log.Notify(isAr ? "[Happy] جارٍ تنظيم وترتيب الجوائز في الشنطة..." : "[Happy] Organizing rewards in inventory...");
                try
                {
                    Thread.Sleep(150);
                    Game.Player.Inventory.Sort();
                }
                catch (Exception ex)
                {
                    Log.Debug($"[Happy] Sort error: {ex.Message}");
                }
            }

            return true;
        }
        catch (Exception ex)
        {
            Log.Error($"[Happy] Error: {ex.Message}");
            return false;
        }
        finally
        {
            lock (_syncLock)
            {
                Running = false;
                _cancelRequested = false;
                ClearCallbacks();
                if (Game.Player?.State?.DialogState != null)
                    Game.Player.State.DialogState = null;
                Game.SelectedEntity = null;
            }
        }
    }

    /// <summary>
    /// Clears any lingering callbacks on event opcodes.
    /// </summary>
    private static void ClearCallbacks()
    {
        PacketManager.ClearCallbacks(0xB045);
        PacketManager.ClearCallbacks(0xB046);
        PacketManager.ClearCallbacks(0x30D4);
        PacketManager.ClearCallbacks(0xB04B);
    }

    /// <summary>
    /// Explicitly closes NPC dialog and resets dialog state with 0x704B.
    /// </summary>
    public static bool CloseAndDeselect(uint npcUniqueId, int timeoutMs = 400)
    {
        ClearCallbacks();

        var packet = new Packet(0x704B);
        packet.WriteUInt(npcUniqueId);

        var awaitResult = new AwaitCallback(response =>
        {
            return response.ReadByte() == 1 ? AwaitCallbackResult.Success : AwaitCallbackResult.Fail;
        }, 0xB04B);

        PacketManager.SendPacket(packet, PacketDestination.Server, awaitResult);
        awaitResult.AwaitResponse(timeoutMs);

        if (Game.Player?.State?.DialogState != null)
            Game.Player.State.DialogState = null;

        Game.SelectedEntity = null;

        return awaitResult.IsCompleted;
    }

    /// <summary>
    /// Explicitly closes NPC dialog (0x704B) without hanging.
    /// </summary>
    public static void SendCloseDialog(uint npcUniqueId)
    {
        CloseAndDeselect(npcUniqueId, 250);
    }

    /// <summary>
    /// Selects NPC (0x7045) safely and sets entity selection.
    /// </summary>
    private static bool SendSelectEntity(uint uniqueId, int timeoutMs = 800)
    {
        PacketManager.ClearCallbacks(0xB045);

        var packet = new Packet(0x7045);
        packet.WriteUInt(uniqueId);

        var success = false;
        var awaitResult = new AwaitCallback(response =>
        {
            var res = response.ReadByte();
            if (res == 1)
            {
                success = true;
                return AwaitCallbackResult.Success;
            }
            return AwaitCallbackResult.Fail;
        }, 0xB045);

        PacketManager.SendPacket(packet, PacketDestination.Server, awaitResult);
        awaitResult.AwaitResponse(timeoutMs);

        if (SpawnManager.TryGetEntity<SpawnedNpc>(uniqueId, out var npc))
            Game.SelectedEntity = npc;

        return success || awaitResult.IsCompleted;
    }

    /// <summary>
    /// Sends talk option to NPC (0x7046 Option 2).
    /// </summary>
    private static bool SendTalkOption(uint uniqueId, byte option = 2, int timeoutMs = 1000)
    {
        PacketManager.ClearCallbacks(0xB046);
        PacketManager.ClearCallbacks(0x30D4);

        var packet = new Packet(0x7046);
        packet.WriteUInt(uniqueId);
        packet.WriteByte(option);

        var talkSuccess = false;
        var awaitTalk = new AwaitCallback(response =>
        {
            var res = response.ReadByte();
            if (res == 1)
            {
                talkSuccess = true;
                return AwaitCallbackResult.Success;
            }

            return AwaitCallbackResult.Fail;
        }, 0xB046);

        PacketManager.SendPacket(packet, PacketDestination.Server, awaitTalk);
        awaitTalk.AwaitResponse(timeoutMs);

        return talkSuccess || awaitTalk.IsCompleted;
    }

    /// <summary>
    /// Sends dialog action (0x30D4 Action 0x05) and awaits 0x30D4 dialog response or 0xB04B error/close.
    /// Used for Step 2 (Select Happy Event) and Step 3 (Redeem Letters).
    /// </summary>
    private static (bool success, bool inventoryFull) SendDialogAction(byte action = 0x05, int timeoutMs = 800)
    {
        PacketManager.ClearCallbacks(0x30D4);
        PacketManager.ClearCallbacks(0xB04B);

        var packet = new Packet(0x30D4);
        packet.WriteByte(action);

        var success = false;
        var invFull = false;
        using var doneEvent = new ManualResetEventSlim(false);

        var await30D4 = new AwaitCallback(response =>
        {
            success = true;
            doneEvent.Set();
            return AwaitCallbackResult.Success;
        }, 0x30D4);

        var awaitB04B = new AwaitCallback(response =>
        {
            var res = response.ReadByte();
            if (res == 2)
            {
                invFull = true;
                doneEvent.Set();
                return AwaitCallbackResult.Fail;
            }
            if (res == 1)
            {
                doneEvent.Set();
                return AwaitCallbackResult.Success;
            }
            doneEvent.Set();
            return AwaitCallbackResult.Fail;
        }, 0xB04B);

        PacketManager.SendPacket(packet, PacketDestination.Server, await30D4, awaitB04B);
        doneEvent.Wait(timeoutMs);

        return (success, invFull);
    }

    /// <summary>
    /// Sends the final dialog exit action (0x30D4 Action 0x05) and awaits 0xB04B dialog close response.
    /// Used for Step 4 (Close Dialog).
    /// </summary>
    private static bool SendDialogCloseAction(byte action = 0x05, int timeoutMs = 800)
    {
        PacketManager.ClearCallbacks(0xB04B);

        var packet = new Packet(0x30D4);
        packet.WriteByte(action);

        var success = false;
        using var doneEvent = new ManualResetEventSlim(false);

        var awaitResult = new AwaitCallback(response =>
        {
            var res = response.ReadByte();
            if (res == 1)
            {
                success = true;
                doneEvent.Set();
                return AwaitCallbackResult.Success;
            }
            doneEvent.Set();
            return AwaitCallbackResult.Fail;
        }, 0xB04B);

        PacketManager.SendPacket(packet, PacketDestination.Server, awaitResult);
        doneEvent.Wait(timeoutMs);

        return success;
    }
}
