using System;
using System.Collections.Generic;
using System.Linq;
using RSBot.Core;
using RSBot.Core.Components;
using RSBot.Core.Extensions;
using RSBot.Core.Network;
using RSBot.Core.Objects;
using RSBot.Core.Objects.Party;
using RSBot.Core.Objects.Skill;
using RSBot.Core.Objects.Spawn;
using Wolves.Fortress.Models;

namespace Wolves.Fortress;

public class FortressManager
{
    private static FortressManager _instance;
    public static FortressManager Instance => _instance ??= new FortressManager();

    public bool Enabled { get; set; } = true;
    public bool AutoCastWhenReady { get; set; } = true;
    public ChatType ResponseChannel { get; set; } = ChatType.Party;

    public FortressSlotConfig[] Slots { get; } = new FortressSlotConfig[7];
    public List<string> Logs { get; } = new();

    public event System.Action OnStateChanged;
    public event System.Action<string> OnLogAdded;

    private FortressManager()
    {
        for (int i = 0; i < 7; i++)
        {
            Slots[i] = new FortressSlotConfig(i + 1);
        }

        LoadConfig();
    }

    public void LoadConfig()
    {
        Enabled = PlayerConfig.Get("Wolves.Fortress.Enabled", PlayerConfig.Get("SonicBot.Fortress.Enabled", true));
        AutoCastWhenReady = PlayerConfig.Get("Wolves.Fortress.AutoCastWhenReady", PlayerConfig.Get("SonicBot.Fortress.AutoCastWhenReady", true));
        ResponseChannel = (ChatType)PlayerConfig.Get("Wolves.Fortress.ResponseChannel", PlayerConfig.Get("SonicBot.Fortress.ResponseChannel", (byte)ChatType.Party));

        for (int i = 0; i < 7; i++)
        {
            var prefix = $"Wolves.Fortress.Slot{i + 1}.";
            var oldPrefix = $"SonicBot.Fortress.Slot{i + 1}.";
            Slots[i].PlayerName = PlayerConfig.Get(prefix + "PlayerName", PlayerConfig.Get(oldPrefix + "PlayerName", string.Empty));
            Slots[i].SkillId = PlayerConfig.Get<uint>(prefix + "SkillId", PlayerConfig.Get<uint>(oldPrefix + "SkillId", 0));
            Slots[i].SkillName = PlayerConfig.Get(prefix + "SkillName", PlayerConfig.Get(oldPrefix + "SkillName", string.Empty));
            Slots[i].Enabled = PlayerConfig.Get(prefix + "Enabled", PlayerConfig.Get(oldPrefix + "Enabled", true));
        }
    }

    public void SaveConfig()
    {
        PlayerConfig.Set("Wolves.Fortress.Enabled", Enabled);
        PlayerConfig.Set("Wolves.Fortress.AutoCastWhenReady", AutoCastWhenReady);
        PlayerConfig.Set("Wolves.Fortress.ResponseChannel", (byte)ResponseChannel);

        for (int i = 0; i < 7; i++)
        {
            var prefix = $"Wolves.Fortress.Slot{i + 1}.";
            PlayerConfig.Set(prefix + "PlayerName", Slots[i].PlayerName ?? string.Empty);
            PlayerConfig.Set(prefix + "SkillId", Slots[i].SkillId);
            PlayerConfig.Set(prefix + "SkillName", Slots[i].SkillName ?? string.Empty);
            PlayerConfig.Set(prefix + "Enabled", Slots[i].Enabled);
        }
    }

    public void Log(string msg)
    {
        var entry = $"[{DateTime.Now:HH:mm:ss}] {msg}";
        Logs.Add(entry);
        if (Logs.Count > 100)
            Logs.RemoveAt(0);

        OnLogAdded?.Invoke(entry);
    }

    /// <summary>
    ///     Automatically populates the 7 slots with current party members (excluding the current player).
    /// </summary>
    public void AutoFillFromParty()
    {
        if (Game.Party?.Members == null || Game.Party.Members.Count == 0)
        {
            Log("لا يوجد أعضاء في البارتي للتوزيع التلقائي");
            return;
        }

        var myName = Game.Player?.Name ?? string.Empty;
        var eligibleMembers = Game.Party.Members
            .Where(m => !string.IsNullOrEmpty(m.Name) && !string.Equals(m.Name, myName, StringComparison.OrdinalIgnoreCase))
            .ToList();

        for (int i = 0; i < 7; i++)
        {
            if (i < eligibleMembers.Count)
            {
                Slots[i].PlayerName = eligibleMembers[i].Name;
                Slots[i].Enabled = true;
            }
            else
            {
                Slots[i].PlayerName = string.Empty;
            }
        }

        SaveConfig();
        OnStateChanged?.Invoke();
        Log($"تم توزيع {eligibleMembers.Count} من أعضاء البارتي على السلوتات b1 - b7 بنجاح");
    }

    /// <summary>
    ///     Handles a b1-b7 trigger from party chat, whisper, or client command.
    /// </summary>
    public void HandleSlotTrigger(int slotIndex, string sender, ChatType incomingType)
    {
        if (!Enabled)
            return;

        if (slotIndex < 1 || slotIndex > 7)
            return;

        var slot = Slots[slotIndex - 1];
        if (!slot.Enabled)
        {
            SendChatResponse($"[Fortress] b{slotIndex}: Slot is currently disabled!", sender, incomingType);
            Log($"[Fortress] تم تجاهل طلب b{slotIndex} لأن السلوت معطل");
            return;
        }

        if (string.IsNullOrWhiteSpace(slot.PlayerName))
        {
            SendChatResponse($"[Fortress] b{slotIndex}: No player assigned to this slot!", sender, incomingType);
            Log($"[Fortress] طلب b{slotIndex} لا يوجد لاعب مخصص له");
            return;
        }

        if (slot.SkillId == 0 && string.IsNullOrWhiteSpace(slot.SkillName))
        {
            SendChatResponse($"[Fortress] b{slotIndex} ({slot.PlayerName}): No buff skill assigned!", sender, incomingType);
            Log($"[Fortress] طلب b{slotIndex} ({slot.PlayerName}) لم يتم تحديد مهارة بفة له");
            return;
        }

        SkillInfo skill = null;
        if (slot.SkillId != 0)
            skill = Game.Player?.Skills?.GetSkillInfoById(slot.SkillId);

        if (skill == null && !string.IsNullOrWhiteSpace(slot.SkillName))
            skill = Game.Player?.Skills?.GetSkillByName(slot.SkillName);

        if (skill == null)
        {
            SendChatResponse($"[Fortress] b{slotIndex} ({slot.PlayerName}): Skill not learned or not available!", sender, incomingType);
            Log($"[Fortress] طلب b{slotIndex} المهارة غير موجودة في مهارات اللاعب الحالية");
            return;
        }

        var skillName = skill.Record?.GetRealName() ?? slot.SkillName;

        if (skill.HasCooldown)
        {
            var remainingSec = skill.RemainingCooldownSeconds;
            var response = $"[Fortress] b{slotIndex} ({slot.PlayerName}) [{skillName}]: Cooldown! {remainingSec}s remaining.";
            SendChatResponse(response, sender, incomingType);
            Log($"[Fortress] b{slotIndex} ({slot.PlayerName}) البفة [{skillName}] في الكولدون (متبقي {remainingSec} ثانية)");
            return;
        }

        // Skill is ready!
        if (AutoCastWhenReady)
        {
            var castSuccess = CastBuffOnMember(slot, skill);
            if (castSuccess)
            {
                var response = $"[Fortress] b{slotIndex} ({slot.PlayerName}) [{skillName}]: Ready & Casted!";
                SendChatResponse(response, sender, incomingType);
                Log($"[Fortress] تم إلقاء البفة [{skillName}] بنجاح على اللاعب ({slot.PlayerName}) [b{slotIndex}]");
            }
            else
            {
                var response = $"[Fortress] b{slotIndex} ({slot.PlayerName}) [{skillName}]: Ready, but target not in range!";
                SendChatResponse(response, sender, incomingType);
                Log($"[Fortress] البفة [{skillName}] جاهزة لكن اللاعب ({slot.PlayerName}) غير موجود في المحيط للإلقاء عليه");
            }
        }
        else
        {
            var response = $"[Fortress] b{slotIndex} ({slot.PlayerName}) [{skillName}]: Buff is READY!";
            SendChatResponse(response, sender, incomingType);
            Log($"[Fortress] b{slotIndex} ({slot.PlayerName}) البفة [{skillName}] جاهزة للإلقاء");
        }
    }

    /// <summary>
    ///     Attempts to cast the configured buff on the target member.
    /// </summary>
    public bool CastBuffOnMember(FortressSlotConfig slot, SkillInfo skill)
    {
        if (Game.Player == null || skill == null)
            return false;

        // Try getting target player UID from vicinity spawn or party
        uint targetUid = 0;
        if (SpawnManager.TryGetEntity<SpawnedPlayer>(p => string.Equals(p.Name, slot.PlayerName, StringComparison.OrdinalIgnoreCase), out var target))
        {
            targetUid = target.UniqueId;
        }
        else if (Game.Party?.Members != null)
        {
            var member = Game.Party.Members.FirstOrDefault(m => string.Equals(m.Name, slot.PlayerName, StringComparison.OrdinalIgnoreCase));
            if (member?.Player != null)
                targetUid = member.Player.UniqueId;
        }

        if (targetUid == 0)
            return false;

        SkillManager.CastBuff(skill, targetUid, awaitBuffResponse: false);
        return true;
    }

    /// <summary>
    ///     Sends in-game response message to Party or Whisper in pure ASCII.
    /// </summary>
    public void SendChatResponse(string message, string sender, ChatType incomingType)
    {
        if (string.IsNullOrWhiteSpace(message))
            return;

        try
        {
            var targetChannel = ResponseChannel;
            if (incomingType == ChatType.Private && ResponseChannel == ChatType.Private)
                targetChannel = ChatType.Private;

            var packet = new Packet(0x7025);
            if (targetChannel == ChatType.Private && !string.IsNullOrEmpty(sender))
            {
                packet.WriteByte(ChatType.Private);
                packet.WriteByte(1);
                if (Game.ClientType > GameClientType.Vietnam) packet.WriteByte(0);
                if (Game.ClientType >= GameClientType.Chinese) packet.WriteByte(0);
                packet.WriteString(sender);
                packet.WriteConditonalString(message);
            }
            else
            {
                packet.WriteByte(ChatType.Party);
                packet.WriteByte(1);
                if (Game.ClientType > GameClientType.Vietnam) packet.WriteByte(0);
                if (Game.ClientType >= GameClientType.Chinese) packet.WriteByte(0);
                packet.WriteConditonalString(message);
            }

            PacketManager.SendPacket(packet, PacketDestination.Server);
        }
        catch (Exception ex)
        {
            Log($"خطأ في إرسال رسالة الشات: {ex.Message}");
        }
    }
}
