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
                name.Contains("صندوق المجوهرات", StringComparison.OrdinalIgnoreCase))
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
            codeName.IndexOf("SO-OK", StringComparison.OrdinalIgnoreCase) >= 0)
            return true;

        if (realName.IndexOf("So-Ok", StringComparison.OrdinalIgnoreCase) >= 0 ||
            realName.IndexOf("So - Ok", StringComparison.OrdinalIgnoreCase) >= 0 ||
            realName.IndexOf("SO OK", StringComparison.OrdinalIgnoreCase) >= 0 ||
            realName.IndexOf("SoOk", StringComparison.OrdinalIgnoreCase) >= 0 ||
            realName.Contains("سو-أوك") || realName.Contains("سو أوك") || realName.Contains("سوك"))
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
            Log.Notify("[Happy] تم طلب إيقاف التبديل. جاري الإنهاء بشكل سليم...");
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
    /// Handles both servers where dialog stays open and servers where dialog auto-closes per exchange.
    /// Monitors inventory in real-time after each reward, waits for Auto Sort to finish, and immediately follows up.
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

        try
        {
            Log.Notify("[Happy] بدء التبديل السريع والمستمر مع Event So-Ok...");

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
                    Log.Warn("[Happy] لم يتم العثور على NPC Event So-Ok في المحيط القريب. تأكد من وقوف الشخصية بجانبها.");
                    return false;
                }
            }

            if (npc.DistanceToPlayer > 15.0f)
            {
                Log.Warn($"[Happy] المسافة إلى So-Ok بعيدة ({Math.Round(npc.DistanceToPlayer, 1)}م). يرجى الاقتراب منها.");
                return false;
            }

            var initialLetters = GetLetterCounts();
            var initialJewelBoxes = GetJewelBoxCount();
            var isLetterEvent = initialLetters.Total > 0;
            var isJewelEvent = !isLetterEvent && initialJewelBoxes >= 10;

            if (!isLetterEvent && !isJewelEvent)
            {
                Log.Warn("[Happy] لا توجد حروف H-A-P-P-Y أو صناديق مجوهرات في الحقيبة للتبديل.");
                return false;
            }

            if (isLetterEvent && !initialLetters.CanExchange)
            {
                Log.Warn($"[Happy] الحروف غير مكتملة للتبديل ({initialLetters.GetMissingDescription()}). H:{initialLetters.H}, A:{initialLetters.A}, P:{initialLetters.TotalP}, Y:{initialLetters.Y}.");
                return false;
            }

            const int maxCycles = 5000;
            var cyclesDone = 0;
            var consecutiveErrors = 0;

            // Clear any lingering callbacks & close any stale dialog
            ClearCallbacks();
            CloseAndDeselect(npc.UniqueId, 300);
            Thread.Sleep(120);

            for (var cycle = 0; cycle < maxCycles; cycle++)
            {
                if (_cancelRequested)
                {
                    Log.Notify("[Happy] تم الإيقاف يدوياً من المستخدم.");
                    break;
                }

                if (Game.Player == null || Game.Player.Inventory == null)
                    break;

                // If inventory is completely full, try stacking once to see if slots free up
                if (Game.Player.Inventory.Full)
                {
                    Log.Notify("[Happy] الحقيبة ممتلئة، جاري محاولة دمج العناصر لتوفير مساحة...");
                    Game.Player.Inventory.Sort();
                    var fullWait = 0;
                    while (Game.Player.Inventory.IsSorting && fullWait++ < 25)
                        Thread.Sleep(100);

                    if (Game.Player.Inventory.Full)
                    {
                        Log.Warn("[Happy] الحقيبة ممتلئة تماماً ولا يوجد مساحات كافية للجوائز. تم التوقف.");
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
                            Log.Notify($"[Happy] انتهت الحروف المتاحة للتبديل ({currentLetters.GetMissingDescription()}). إجمالي التبديلات: {cyclesDone}. (H:{currentLetters.H}, A:{currentLetters.A}, P:{currentLetters.TotalP}, Y:{currentLetters.Y})");
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
                            Log.Notify($"[Happy] انتهت صناديق المجوهرات (المتبقي: {currentJewels}). إجمالي التبديلات: {cyclesDone}.");
                            break;
                        }
                    }
                }

                npc = GetEventSoOkNpc() ?? npc;

                // Step 0: Ensure So-Ok is selected (only send if not already selected)
                if (Game.SelectedEntity?.UniqueId != npc.UniqueId)
                {
                    SendSelectEntity(npc.UniqueId, 1000);
                    Game.SelectedEntity = npc;
                    Thread.Sleep(50);
                }

                if (_cancelRequested) break;

                // Step 1: Open NPC talk option (0x7046 Option 2)
                var talkSuccess = SendTalkOption(npc.UniqueId, 2, 1200);
                if (!talkSuccess)
                {
                    // Fast retry once
                    Thread.Sleep(100);
                    CloseAndDeselect(npc.UniqueId, 300);
                    Thread.Sleep(80);
                    SendSelectEntity(npc.UniqueId, 800);
                    Game.SelectedEntity = npc;
                    Thread.Sleep(50);
                    talkSuccess = SendTalkOption(npc.UniqueId, 2, 1200);

                    if (!talkSuccess)
                    {
                        consecutiveErrors++;
                        if (consecutiveErrors >= 3)
                        {
                            Log.Warn("[Happy] تعذر فتح محادثة So-Ok بعد 3 محاولات. جاري الإنهاء.");
                            break;
                        }
                        Thread.Sleep(250);
                        continue;
                    }
                }

                Thread.Sleep(50);
                if (_cancelRequested) { CloseAndDeselect(npc.UniqueId, 300); break; }

                // Step 2: Select Event in Dialog (0x30D4 Action 0x05)
                SendDialogAction(0x05, 1000);
                Thread.Sleep(50);

                if (_cancelRequested) { CloseAndDeselect(npc.UniqueId, 300); break; }

                // Step 3: Receive Gift (0x30D4 Action 0x05)
                SendDialogAction(0x05, 1000);
                Thread.Sleep(60);

                // Step 4: Cleanly close dialog
                CloseAndDeselect(npc.UniqueId, 400);

                cyclesDone++;
                consecutiveErrors = 0;

                var remLetters = isLetterEvent ? GetLetterCounts() : default;
                if (isLetterEvent)
                    Log.Notify($"[Happy] تبديل ناجح (#{cyclesDone}). الحروف المتبقية: H:{remLetters.H}, A:{remLetters.A}, P:{remLetters.TotalP}, Y:{remLetters.Y}");
                else
                    Log.Notify($"[Happy] تبديل ناجح (#{cyclesDone}) لصناديق المجوهرات (المتبقي: {GetJewelBoxCount()}).");

                // Step 5: Real-time inventory tracking & Auto Sort synchronization
                // Wait briefly for the server's reward item packet to update the inventory collection
                Thread.Sleep(80);

                if (Game.Player?.Inventory != null && !_cancelRequested)
                {
                    // 1. Wait if an external sort is already in progress
                    var sortWait = 0;
                    while (Game.Player.Inventory.IsSorting && sortWait++ < 40 && !_cancelRequested)
                    {
                        Thread.Sleep(50);
                    }

                    // 2. Check if there are duplicate stackable items to merge (Elixirs, Potions, Stones, Coins, etc.)
                    var hasMergeableItems = Game.Player.Inventory
                        .Where(i => i.Slot > 12 && i.Record != null && i.Record.IsStackable && i.Record.MaxStack > i.Amount)
                        .GroupBy(i => i.ItemId)
                        .Any(g => g.Count() >= 2);

                    if (hasMergeableItems || Game.Player.Inventory.Full)
                    {
                        Log.Notify("[Happy] تم استلام الجائزة، جاري فحص الحقيبة ودمج العناصر تلقائياً...");
                        Game.Player.Inventory.Sort();

                        // Wait for Auto Sort to finish completely before proceeding to the next exchange
                        sortWait = 0;
                        while (Game.Player.Inventory.IsSorting && sortWait++ < 40 && !_cancelRequested)
                        {
                            Thread.Sleep(50);
                        }
                    }

                    // 3. If bag is full and cannot free any more slots, terminate gracefully
                    if (Game.Player.Inventory.Full)
                    {
                        Log.Warn("[Happy] الحقيبة ممتلئة تماماً ولا يوجد مساحات كافية للجوائز. تم التوقف.");
                        break;
                    }
                }

                // Immediately follow up with the next exchange without unnecessary delay ("يلحم وراه علي طول")
                Thread.Sleep(40);
            }

            // Cleanup: cleanly close dialog
            CloseAndDeselect(npc.UniqueId, 300);
            Thread.Sleep(150);

            Log.Notify($"[Happy] اكتملت عملية التبديل! تم إجراء {cyclesDone} تبديل بنجاح.");

            // Final Step: Execute Auto Sort ONCE at the very end to stack and organize all rewards cleanly
            if (cyclesDone > 0 && Game.Player?.Inventory != null)
            {
                Log.Notify("[Happy] جاري ترتيب ودمج الجوائز في الحقيبة (Auto Sort)...");
                try
                {
                    Thread.Sleep(250);
                    Game.Player.Inventory.Sort();
                    var sortWait = 0;
                    while (Game.Player.Inventory.IsSorting && sortWait++ < 40)
                    {
                        Thread.Sleep(100);
                    }
                    Log.Notify("[Happy] تم ترتيب ودمج العناصر بنجاح! جاهز تماماً.");
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
            Log.Error($"[Happy] خطأ أثناء التبديل: {ex.Message}");
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
    public static bool CloseAndDeselect(uint npcUniqueId, int timeoutMs = 600)
    {
        PacketManager.ClearCallbacks(0xB04B);
        PacketManager.ClearCallbacks(0x30D4);

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
        CloseAndDeselect(npcUniqueId, 300);
    }

    /// <summary>
    /// Selects NPC (0x7045) safely and sets entity selection.
    /// </summary>
    private static bool SendSelectEntity(uint uniqueId, int timeoutMs = 1200)
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
    /// Sends talk option to NPC (0x7046 Option).
    /// </summary>
    private static bool SendTalkOption(uint uniqueId, byte option = 2, int timeoutMs = 1200)
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
    /// Sends dialog action for So-Ok (0x30D4 Action).
    /// </summary>
    private static bool SendDialogAction(byte action = 0x05, int timeoutMs = 1000)
    {
        PacketManager.ClearCallbacks(0x30D4);

        var packet = new Packet(0x30D4);
        packet.WriteByte(action);

        var success = false;
        var awaitResult = new AwaitCallback(response =>
        {
            success = true;
            return AwaitCallbackResult.Success;
        }, 0x30D4);

        PacketManager.SendPacket(packet, PacketDestination.Server, awaitResult);
        awaitResult.AwaitResponse(timeoutMs);

        return success || awaitResult.IsCompleted;
    }
}
