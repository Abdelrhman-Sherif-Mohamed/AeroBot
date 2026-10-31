using System;
using System.Collections.Generic;
using System.Linq;
using RSBot.Core;
using RSBot.Core.Client.ReferenceObjects;
using RSBot.Core.Event;
using RSBot.Core.Network;
using RSBot.Core.Objects.Item;

namespace Wolves.AvatarTester;

public enum SwitchCategory
{
    All,
    WeaponGlow,
    ShieldGlow,
    ModelD1D16,
    SpecialSwitcher
}

public class AvatarTesterManager
{
    private static AvatarTesterManager _instance;
    public static AvatarTesterManager Instance => _instance ??= new AvatarTesterManager();

    // Selected items in the UI (Avatars)
    public uint SelectedHatId { get; set; }
    public uint SelectedDressId { get; set; }
    public uint SelectedAccessoryId { get; set; }
    public uint SelectedFlagId { get; set; }

    // Track which avatars are currently applied directly on the player character model
    public uint AppliedHatId { get; private set; }
    public uint AppliedDressId { get; private set; }
    public uint AppliedAccessoryId { get; private set; }
    public uint AppliedFlagId { get; private set; }

    // Track weapon & shield modifications
    public uint AppliedWeaponId { get; private set; }
    public byte AppliedWeaponOpt { get; private set; }
    public uint AppliedShieldId { get; private set; }
    public byte AppliedShieldOpt { get; private set; }

    public const uint PREVIEW_CLONE_UID = 0xFFFFFFFD;
    public bool IsPreviewCloneSpawned { get; private set; }

    public bool HasAnyApplied => AppliedHatId != 0 || AppliedDressId != 0 || AppliedAccessoryId != 0 || AppliedFlagId != 0 || IsPreviewCloneSpawned;

    public event Action OnStateChanged;

    private AvatarTesterManager()
    {
        EventManager.SubscribeEvent("OnLoadCharacter", OnResetState);
        EventManager.SubscribeEvent("OnTeleport", OnResetState);
    }

    private void OnResetState()
    {
        DespawnPreviewClone();

        AppliedHatId = 0;
        AppliedDressId = 0;
        AppliedAccessoryId = 0;
        AppliedFlagId = 0;

        AppliedWeaponId = 0;
        AppliedWeaponOpt = 0;
        AppliedShieldId = 0;
        AppliedShieldOpt = 0;

        OnStateChanged?.Invoke();
    }

    #region Avatar Methods

    public static bool IsAvatarItem(RefObjItem i)
    {
        if (i == null) return false;
        if (i.IsAvatar) return true;
        if (i.TypeID2 == 1 && (i.TypeID3 == 13 || i.TypeID3 == 14)) return true;
        if (i.CodeName != null)
        {
            if (i.CodeName.Contains("AVATAR", StringComparison.OrdinalIgnoreCase) ||
                i.CodeName.StartsWith("ITEM_MALL_AVATAR", StringComparison.OrdinalIgnoreCase) ||
                i.CodeName.StartsWith("ITEM_ETC_AVATAR", StringComparison.OrdinalIgnoreCase) ||
                i.CodeName.Contains("NASRUN", StringComparison.OrdinalIgnoreCase))
                return true;
        }
        return false;
    }

    public List<RefObjItem> SearchAvatarItems(string filter = "")
    {
        if (Game.ReferenceManager?.ItemData == null)
            return new List<RefObjItem>();

        var query = Game.ReferenceManager.ItemData.Values
            .Where(IsAvatarItem);

        if (!string.IsNullOrWhiteSpace(filter))
        {
            query = query.Where(i =>
                (i.CodeName != null && i.CodeName.Contains(filter, StringComparison.OrdinalIgnoreCase)) ||
                (i.GetRealName() != null && i.GetRealName().Contains(filter, StringComparison.OrdinalIgnoreCase)) ||
                GetItemDisplayName(i).Contains(filter, StringComparison.OrdinalIgnoreCase));
        }

        return query.OrderBy(i => GetItemDisplayName(i)).ToList();
    }

    public static byte GetSlotIndex(RefObjItem item)
    {
        if (item == null) return 1;
        var code = item.CodeName ?? "";

        if (item.TypeID4 == 1 || code.Contains("_HAT", StringComparison.OrdinalIgnoreCase) || code.Contains("_HELM", StringComparison.OrdinalIgnoreCase))
            return 0; // Hat
        if (item.TypeID4 == 2 || code.Contains("_DRESS", StringComparison.OrdinalIgnoreCase) || code.Contains("_BODY", StringComparison.OrdinalIgnoreCase) || code.Contains("_SUIT", StringComparison.OrdinalIgnoreCase))
            return 1; // Dress
        if (item.TypeID4 == 3 || code.Contains("_ATTACH", StringComparison.OrdinalIgnoreCase) || code.Contains("_WING", StringComparison.OrdinalIgnoreCase) || code.Contains("_ACC", StringComparison.OrdinalIgnoreCase))
            return 2; // Accessory
        if (item.TypeID4 == 4 || item.TypeID4 == 5 || code.Contains("_FLAG", StringComparison.OrdinalIgnoreCase) || code.Contains("NASRUN", StringComparison.OrdinalIgnoreCase))
            return 3; // Flag

        return 1; // Default to Dress
    }

    public static string GetSlotTypeString(RefObjItem item)
    {
        if (item == null) return "Unknown";
        var code = item.CodeName ?? "";

        string slot;
        if (item.TypeID4 == 1 || code.Contains("_HAT", StringComparison.OrdinalIgnoreCase) || code.Contains("_HELM", StringComparison.OrdinalIgnoreCase))
            slot = "قبعة (Hat)";
        else if (item.TypeID4 == 2 || code.Contains("_DRESS", StringComparison.OrdinalIgnoreCase) || code.Contains("_BODY", StringComparison.OrdinalIgnoreCase) || code.Contains("_SUIT", StringComparison.OrdinalIgnoreCase))
            slot = "فستان (Dress)";
        else if (item.TypeID4 == 3 || code.Contains("_ATTACH", StringComparison.OrdinalIgnoreCase) || code.Contains("_WING", StringComparison.OrdinalIgnoreCase) || code.Contains("_ACC", StringComparison.OrdinalIgnoreCase))
            slot = "ملحق (Accessory)";
        else if (item.TypeID4 == 4 || item.TypeID4 == 5 || code.Contains("_FLAG", StringComparison.OrdinalIgnoreCase) || code.Contains("NASRUN", StringComparison.OrdinalIgnoreCase))
            slot = "علم (Flag)";
        else
            slot = "أخرى";

        string gender = "";
        if (item.ReqGender == 1 || code.Contains("_M_", StringComparison.OrdinalIgnoreCase) || code.EndsWith("_M", StringComparison.OrdinalIgnoreCase))
            gender = " (ذكر)";
        else if (item.ReqGender == 2 || code.Contains("_W_", StringComparison.OrdinalIgnoreCase) || code.EndsWith("_W", StringComparison.OrdinalIgnoreCase))
            gender = " (أنثى)";

        return slot + gender;
    }

    public void EquipAvatarOnPlayer(uint refItemId, byte slot)
    {
        if (Game.Player == null || Game.Player.UniqueId == 0)
        {
            Log.Warn("[AvatarTester] شخصيتك ليست داخل اللعبة حالياً!");
            return;
        }

        if (refItemId == 0)
            return;

        uint previousApplied = GetAppliedItemForSlot(slot);
        if (previousApplied != 0 && previousApplied != refItemId)
        {
            SendUnequipPacket(previousApplied, slot);
        }

        var packet = new Packet(0x3038);
        packet.WriteUInt(Game.Player.UniqueId);
        packet.WriteByte(slot);
        packet.WriteUInt(refItemId);
        packet.WriteByte(0);
        PacketManager.SendPacket(packet, PacketDestination.Client);

        SetAppliedItemForSlot(slot, refItemId);
        OnStateChanged?.Invoke();
    }

    public void UnequipAvatarFromPlayer(byte slot)
    {
        if (Game.Player == null || Game.Player.UniqueId == 0)
            return;

        uint applied = GetAppliedItemForSlot(slot);
        if (applied == 0)
        {
            applied = slot switch
            {
                0 => SelectedHatId,
                1 => SelectedDressId,
                2 => SelectedAccessoryId,
                3 => SelectedFlagId,
                _ => 0
            };
        }

        if (applied != 0)
        {
            SendUnequipPacket(applied, slot);
            SetAppliedItemForSlot(slot, 0);
        }

        var originalAvatar = Game.Player.Avatars?.GetItemAt(slot);
        if (originalAvatar != null && originalAvatar.ItemId != 0)
        {
            var packet = new Packet(0x3038);
            packet.WriteUInt(Game.Player.UniqueId);
            packet.WriteByte(slot);
            packet.WriteUInt(originalAvatar.ItemId);
            packet.WriteByte(originalAvatar.OptLevel);
            PacketManager.SendPacket(packet, PacketDestination.Client);
        }

        OnStateChanged?.Invoke();
    }

    public void ApplyAll()
    {
        if (Game.Player == null || Game.Player.UniqueId == 0)
        {
            Log.Warn("[AvatarTester] شخصيتك ليست داخل اللعبة حالياً!");
            return;
        }

        if (SelectedHatId != 0)
            EquipAvatarOnPlayer(SelectedHatId, 0);
        else if (AppliedHatId != 0)
            UnequipAvatarFromPlayer(0);

        if (SelectedDressId != 0)
            EquipAvatarOnPlayer(SelectedDressId, 1);
        else if (AppliedDressId != 0)
            UnequipAvatarFromPlayer(1);

        if (SelectedAccessoryId != 0)
            EquipAvatarOnPlayer(SelectedAccessoryId, 2);
        else if (AppliedAccessoryId != 0)
            UnequipAvatarFromPlayer(2);

        if (SelectedFlagId != 0)
            EquipAvatarOnPlayer(SelectedFlagId, 3);
        else if (AppliedFlagId != 0)
            UnequipAvatarFromPlayer(3);

        Log.Notify("[AvatarTester] تم تطبيق الافتار بنجاح على شخصيتك!");
        OnStateChanged?.Invoke();
    }

    public void ClearAll()
    {
        UnequipAvatarFromPlayer(0);
        UnequipAvatarFromPlayer(1);
        UnequipAvatarFromPlayer(2);
        UnequipAvatarFromPlayer(3);

        SelectedHatId = 0;
        SelectedDressId = 0;
        SelectedAccessoryId = 0;
        SelectedFlagId = 0;

        Log.Notify("[AvatarTester] تم مسح الافتارات واستعادة مظهر الشخصية الأصلي.");
        OnStateChanged?.Invoke();
    }

    #endregion

    #region Switch Methods

    public static bool IsSwitchItem(RefObjItem i)
    {
        if (i == null || string.IsNullOrEmpty(i.CodeName)) return false;
        var code = i.CodeName;
        if (code.Contains("ESENGAME", StringComparison.OrdinalIgnoreCase)) return true;
        if (code.Contains("LEGACY_MODEL", StringComparison.OrdinalIgnoreCase)) return true;
        if (code.Contains("ARTEMIS_SWITCHER", StringComparison.OrdinalIgnoreCase)) return true;
        if (code.Contains("MODEL_SWITCH", StringComparison.OrdinalIgnoreCase)) return true;
        if (code.Contains("SWITCHER", StringComparison.OrdinalIgnoreCase)) return true;
        return false;
    }

    public static SwitchCategory GetSwitchCategory(RefObjItem item)
    {
        if (item == null || string.IsNullOrEmpty(item.CodeName)) return SwitchCategory.All;
        var code = item.CodeName;
        if (code.Contains("ESENGAME_WEP", StringComparison.OrdinalIgnoreCase))
            return SwitchCategory.WeaponGlow;
        if (code.Contains("ESENGAME_SH", StringComparison.OrdinalIgnoreCase))
            return SwitchCategory.ShieldGlow;
        if (code.Contains("LEGACY_MODEL", StringComparison.OrdinalIgnoreCase))
            return SwitchCategory.ModelD1D16;
        if (code.Contains("ARTEMIS_SWITCHER", StringComparison.OrdinalIgnoreCase))
            return SwitchCategory.SpecialSwitcher;

        if (code.Contains("SHIELD", StringComparison.OrdinalIgnoreCase))
            return SwitchCategory.ShieldGlow;
        if (code.Contains("WEP", StringComparison.OrdinalIgnoreCase))
            return SwitchCategory.WeaponGlow;

        return SwitchCategory.SpecialSwitcher;
    }

    public static string GetSwitchCategoryString(RefObjItem item)
    {
        return GetSwitchCategory(item) switch
        {
            SwitchCategory.WeaponGlow => "توهج سلاح (Weapon)",
            SwitchCategory.ShieldGlow => "توهج درع (Shield)",
            SwitchCategory.ModelD1D16 => "سوتشر موديل D1-D16",
            SwitchCategory.SpecialSwitcher => "سوتشر مميز (Special)",
            _ => "سوتشر (Switcher)"
        };
    }

    public List<RefObjItem> SearchSwitchItems(string filter = "", SwitchCategory category = SwitchCategory.All)
    {
        if (Game.ReferenceManager?.ItemData == null)
            return new List<RefObjItem>();

        var query = Game.ReferenceManager.ItemData.Values
            .Where(IsSwitchItem);

        if (category != SwitchCategory.All)
        {
            query = query.Where(i => GetSwitchCategory(i) == category);
        }

        if (!string.IsNullOrWhiteSpace(filter))
        {
            query = query.Where(i =>
                (i.CodeName != null && i.CodeName.Contains(filter, StringComparison.OrdinalIgnoreCase)) ||
                (i.GetRealName() != null && i.GetRealName().Contains(filter, StringComparison.OrdinalIgnoreCase)) ||
                GetItemDisplayName(i).Contains(filter, StringComparison.OrdinalIgnoreCase));
        }

        return query.OrderBy(i => i.ID).ToList();
    }

    public static RefObjItem FindWeaponForDegree(RefObjItem baseWeapon, int targetDegree)
    {
        if (baseWeapon == null || !baseWeapon.IsWeapon || Game.ReferenceManager?.ItemData == null)
            return null;

        string degStr = targetDegree.ToString("D2"); // "01", "02", ..., "16"
        string altDegStr = "_" + targetDegree + "_";  // "_16_"

        var match = Game.ReferenceManager.ItemData.Values.FirstOrDefault(w =>
            w.IsWeapon &&
            w.Country == baseWeapon.Country &&
            w.TypeID4 == baseWeapon.TypeID4 &&
            (w.CodeName.Contains($"_{degStr}_") || w.CodeName.Contains(altDegStr)) &&
            !w.CodeName.Contains("_RENT_")
        );

        if (match == null)
        {
            match = Game.ReferenceManager.ItemData.Values.FirstOrDefault(w =>
                w.IsWeapon &&
                w.TypeID4 == baseWeapon.TypeID4 &&
                (w.CodeName.Contains($"_{degStr}_") || w.CodeName.Contains(altDegStr))
            );
        }

        return match;
    }

    public static RefObjItem FindMatchingWeapon(RefObjItem baseWeapon, RefObjItem switcherItem)
    {
        if (baseWeapon == null || !baseWeapon.IsWeapon || switcherItem == null || Game.ReferenceManager?.ItemData == null)
            return null;

        var code = switcherItem.CodeName ?? "";

        // 1. Special switcher (e.g. ITEM_ARTEMIS_SWITCHER_D16_BLOOD, ITEM_ARTEMIS_SWITCHER_D17_FLAXEN)
        if (code.Contains("SWITCHER", StringComparison.OrdinalIgnoreCase))
        {
            int targetDeg = 16;
            string theme = null;

            var parts = code.Split('_');
            for (int i = 0; i < parts.Length; i++)
            {
                if (parts[i].StartsWith("D", StringComparison.OrdinalIgnoreCase) &&
                    int.TryParse(parts[i].Substring(1), out var d))
                {
                    targetDeg = d;
                }
                else if (i == parts.Length - 1 && !parts[i].Equals("REMOVER", StringComparison.OrdinalIgnoreCase) && !parts[i].StartsWith("D", StringComparison.OrdinalIgnoreCase))
                {
                    theme = parts[i];
                }
            }

            if (!string.IsNullOrEmpty(theme))
            {
                // First try: same weapon type, country, theme, degree
                var themedMatch = Game.ReferenceManager.ItemData.Values.FirstOrDefault(w =>
                    w.IsWeapon &&
                    w.TypeID4 == baseWeapon.TypeID4 &&
                    w.Country == baseWeapon.Country &&
                    (w.CodeName.Contains($"_{targetDeg}_") || w.CodeName.Contains($"_D{targetDeg}_") || w.CodeName.Contains($"_{targetDeg}")) &&
                    w.CodeName.Contains(theme, StringComparison.OrdinalIgnoreCase));

                if (themedMatch != null)
                    return themedMatch;

                // Second try: same weapon type, theme
                themedMatch = Game.ReferenceManager.ItemData.Values.FirstOrDefault(w =>
                    w.IsWeapon &&
                    w.TypeID4 == baseWeapon.TypeID4 &&
                    w.CodeName.Contains(theme, StringComparison.OrdinalIgnoreCase));

                if (themedMatch != null)
                    return themedMatch;
            }

            return FindWeaponForDegree(baseWeapon, targetDeg) ?? baseWeapon;
        }

        // 2. D1-D16 model switcher (e.g. ITEM_ITEM_LEGACY_MODEL_16)
        if (code.Contains("MODEL", StringComparison.OrdinalIgnoreCase))
        {
            var numStr = code.Substring(code.LastIndexOf('_') + 1);
            if (int.TryParse(numStr, out var deg))
            {
                return FindWeaponForDegree(baseWeapon, deg) ?? baseWeapon;
            }
        }

        // 3. Fallback: if switcherItem is already a weapon
        if (switcherItem.IsWeapon)
            return switcherItem;

        return baseWeapon;
    }

    public static RefObjItem FindMatchingShield(RefObjItem baseShield, RefObjItem switcherItem)
    {
        if (baseShield == null || !baseShield.IsShield || switcherItem == null || Game.ReferenceManager?.ItemData == null)
            return null;

        var code = switcherItem.CodeName ?? "";
        int targetDeg = 16;
        string theme = null;

        var parts = code.Split('_');
        for (int i = 0; i < parts.Length; i++)
        {
            if (parts[i].StartsWith("D", StringComparison.OrdinalIgnoreCase) &&
                int.TryParse(parts[i].Substring(1), out var d))
            {
                targetDeg = d;
            }
            else if (i == parts.Length - 1 && !parts[i].Equals("REMOVER", StringComparison.OrdinalIgnoreCase) && !parts[i].StartsWith("D", StringComparison.OrdinalIgnoreCase))
            {
                theme = parts[i];
            }
        }

        if (!string.IsNullOrEmpty(theme))
        {
            var themedShield = Game.ReferenceManager.ItemData.Values.FirstOrDefault(w =>
                w.IsShield &&
                w.Country == baseShield.Country &&
                w.CodeName.Contains(theme, StringComparison.OrdinalIgnoreCase));

            if (themedShield != null)
                return themedShield;
        }

        string degStr = targetDeg.ToString("D2");
        string altDegStr = "_" + targetDeg + "_";

        var match = Game.ReferenceManager.ItemData.Values.FirstOrDefault(w =>
            w.IsShield &&
            w.Country == baseShield.Country &&
            (w.CodeName.Contains($"_{degStr}_") || w.CodeName.Contains(altDegStr)) &&
            !w.CodeName.Contains("_RENT_"));

        return match ?? Game.ReferenceManager.ItemData.Values.FirstOrDefault(w =>
            w.IsShield &&
            (w.CodeName.Contains($"_{degStr}_") || w.CodeName.Contains(altDegStr))) ?? baseShield;
    }

    public void EquipWeaponOnPlayer(uint refItemId, byte optLevel = 0)
    {
        if (Game.Player == null || Game.Player.UniqueId == 0)
        {
            Log.Warn("[AvatarTester] شخصيتك ليست داخل اللعبة حالياً!");
            return;
        }

        if (refItemId == 0)
            return;

        // Despawn any preview clone if one exists
        DespawnPreviewClone();

        var curWep = Game.Player.Inventory?.GetItemAt(6);
        uint oldWeaponId = AppliedWeaponId != 0 ? AppliedWeaponId : (curWep?.ItemId ?? 0);

        // 1. Unequip existing weapon mesh from hand
        if (oldWeaponId != 0)
        {
            var unequipPacket = new Packet(0x3039);
            unequipPacket.WriteUInt(Game.Player.UniqueId);
            unequipPacket.WriteByte(6); // Slot 6 = Weapon
            unequipPacket.WriteUInt(oldWeaponId);
            PacketManager.SendPacket(unequipPacket, PacketDestination.Client);
        }

        // 2. Synchronize client's internal inventory struct at slot 6
        var invUpdatePacket = new Packet(0x3040);
        invUpdatePacket.WriteByte(6); // Slot 6
        invUpdatePacket.WriteByte((byte)(ItemUpdateFlag.RefObjID | ItemUpdateFlag.OptLevel)); // 3
        invUpdatePacket.WriteUInt(refItemId);
        invUpdatePacket.WriteByte(optLevel);
        PacketManager.SendPacket(invUpdatePacket, PacketDestination.Client);

        // 3. Attach new weapon mesh to hand with glowing plus effect
        var equipPacket = new Packet(0x3038);
        equipPacket.WriteUInt(Game.Player.UniqueId);
        equipPacket.WriteByte(6); // Slot 6
        equipPacket.WriteUInt(refItemId);
        equipPacket.WriteByte(optLevel);
        PacketManager.SendPacket(equipPacket, PacketDestination.Client);

        AppliedWeaponId = refItemId;
        AppliedWeaponOpt = optLevel;

        var name = Game.ReferenceManager?.GetRefItem(refItemId)?.GetRealName();
        Log.Notify($"[AvatarTester] تم تطبيق مظهر السلاح على شخصيتك: {name ?? refItemId.ToString()} (+{optLevel})");
        OnStateChanged?.Invoke();
    }

    public void EquipShieldOnPlayer(uint refItemId, byte optLevel = 0)
    {
        if (Game.Player == null || Game.Player.UniqueId == 0)
        {
            Log.Warn("[AvatarTester] شخصيتك ليست داخل اللعبة حالياً!");
            return;
        }

        if (refItemId == 0)
            return;

        DespawnPreviewClone();

        var curShield = Game.Player.Inventory?.GetItemAt(7);
        uint oldShieldId = AppliedShieldId != 0 ? AppliedShieldId : (curShield?.ItemId ?? 0);

        // 1. Unequip existing shield
        if (oldShieldId != 0)
        {
            var unequipPacket = new Packet(0x3039);
            unequipPacket.WriteUInt(Game.Player.UniqueId);
            unequipPacket.WriteByte(7); // Slot 7 = Shield
            unequipPacket.WriteUInt(oldShieldId);
            PacketManager.SendPacket(unequipPacket, PacketDestination.Client);
        }

        // 2. Synchronize client's internal inventory struct at slot 7
        var invUpdatePacket = new Packet(0x3040);
        invUpdatePacket.WriteByte(7); // Slot 7
        invUpdatePacket.WriteByte((byte)(ItemUpdateFlag.RefObjID | ItemUpdateFlag.OptLevel)); // 3
        invUpdatePacket.WriteUInt(refItemId);
        invUpdatePacket.WriteByte(optLevel);
        PacketManager.SendPacket(invUpdatePacket, PacketDestination.Client);

        // 3. Attach new shield mesh with glow
        var equipPacket = new Packet(0x3038);
        equipPacket.WriteUInt(Game.Player.UniqueId);
        equipPacket.WriteByte(7); // Slot 7
        equipPacket.WriteUInt(refItemId);
        equipPacket.WriteByte(optLevel);
        PacketManager.SendPacket(equipPacket, PacketDestination.Client);

        AppliedShieldId = refItemId;
        AppliedShieldOpt = optLevel;

        var name = Game.ReferenceManager?.GetRefItem(refItemId)?.GetRealName();
        Log.Notify($"[AvatarTester] تم تطبيق مظهر الدرع على شخصيتك: {name ?? refItemId.ToString()} (+{optLevel})");
        OnStateChanged?.Invoke();
    }

    public void DespawnPreviewClone()
    {
        if (IsPreviewCloneSpawned)
        {
            var packet = new Packet(0x3016);
            packet.WriteUInt(PREVIEW_CLONE_UID);
            PacketManager.SendPacket(packet, PacketDestination.Client);
            IsPreviewCloneSpawned = false;
        }
    }

    public void RestoreWeaponAndShield()
    {
        if (Game.Player == null || Game.Player.UniqueId == 0)
            return;

        DespawnPreviewClone();

        // 1. Restore weapon
        if (AppliedWeaponId != 0)
        {
            var unequipPacket = new Packet(0x3039);
            unequipPacket.WriteUInt(Game.Player.UniqueId);
            unequipPacket.WriteByte(6);
            unequipPacket.WriteUInt(AppliedWeaponId);
            PacketManager.SendPacket(unequipPacket, PacketDestination.Client);

            var originalWep = Game.Player.Inventory?.GetItemAt(6);
            if (originalWep != null && originalWep.ItemId != 0)
            {
                var invUpdatePacket = new Packet(0x3040);
                invUpdatePacket.WriteByte(6);
                invUpdatePacket.WriteByte((byte)(ItemUpdateFlag.RefObjID | ItemUpdateFlag.OptLevel));
                invUpdatePacket.WriteUInt(originalWep.ItemId);
                invUpdatePacket.WriteByte(originalWep.OptLevel);
                PacketManager.SendPacket(invUpdatePacket, PacketDestination.Client);

                var equipPacket = new Packet(0x3038);
                equipPacket.WriteUInt(Game.Player.UniqueId);
                equipPacket.WriteByte(6);
                equipPacket.WriteUInt(originalWep.ItemId);
                equipPacket.WriteByte(originalWep.OptLevel);
                PacketManager.SendPacket(equipPacket, PacketDestination.Client);
            }

            AppliedWeaponId = 0;
            AppliedWeaponOpt = 0;
        }

        // 2. Restore shield
        if (AppliedShieldId != 0)
        {
            var unequipPacket = new Packet(0x3039);
            unequipPacket.WriteUInt(Game.Player.UniqueId);
            unequipPacket.WriteByte(7);
            unequipPacket.WriteUInt(AppliedShieldId);
            PacketManager.SendPacket(unequipPacket, PacketDestination.Client);

            var originalShield = Game.Player.Inventory?.GetItemAt(7);
            if (originalShield != null && originalShield.ItemId != 0)
            {
                var invUpdatePacket = new Packet(0x3040);
                invUpdatePacket.WriteByte(7);
                invUpdatePacket.WriteByte((byte)(ItemUpdateFlag.RefObjID | ItemUpdateFlag.OptLevel));
                invUpdatePacket.WriteUInt(originalShield.ItemId);
                invUpdatePacket.WriteByte(originalShield.OptLevel);
                PacketManager.SendPacket(invUpdatePacket, PacketDestination.Client);

                var equipPacket = new Packet(0x3038);
                equipPacket.WriteUInt(Game.Player.UniqueId);
                equipPacket.WriteByte(7);
                equipPacket.WriteUInt(originalShield.ItemId);
                equipPacket.WriteByte(originalShield.OptLevel);
                PacketManager.SendPacket(equipPacket, PacketDestination.Client);
            }

            AppliedShieldId = 0;
            AppliedShieldOpt = 0;
        }

        Log.Notify("[AvatarTester] تم استعادة مظهر السلاح والدرع الأصلي للشخصية.");
        OnStateChanged?.Invoke();
    }

    public bool UseSwitchItemFromInventory(uint refItemId)
    {
        if (Game.Player?.Inventory == null)
        {
            Log.Warn("[AvatarTester] حقيبة الشخصية غير متوفرة!");
            return false;
        }

        var invItem = Game.Player.Inventory.GetNormalPartItems(refItemId).FirstOrDefault()
                   ?? Game.Player.Inventory.GetNormalPartItems().FirstOrDefault(i => i.ItemId == refItemId);

        if (invItem == null)
        {
            Log.Warn("[AvatarTester] هذا السوتش غير متوفر في حقيبتك حالياً!");
            return false;
        }

        bool success = invItem.UseTo(6);
        if (!success)
            success = invItem.Use();

        var name = invItem.Record != null ? GetItemDisplayName(invItem.Record) : invItem.ItemId.ToString();
        Log.Notify($"[AvatarTester] تم إرسال أمر استخدام السوتش ({name}) بنجاح!");
        return true;
    }

    #endregion

    #region Helpers

    public static string GetItemDisplayName(RefObjItem item)
    {
        if (item == null) return "(غير معروف)";
        string name = null;
        try { name = item.GetRealName(); } catch { }
        if (!string.IsNullOrWhiteSpace(name) && name != "0")
            return name;

        if (!string.IsNullOrEmpty(item.CodeName))
            return CleanCodeName(item.CodeName);

        return $"Item {item.ID}";
    }

    public static string CleanCodeName(string codeName)
    {
        if (string.IsNullOrEmpty(codeName)) return "";
        var s = codeName;
        if (s.StartsWith("ITEM_MALL_AVATAR_", StringComparison.OrdinalIgnoreCase))
            s = s.Substring(17);
        else if (s.StartsWith("ITEM_MALL_ESENGAME_", StringComparison.OrdinalIgnoreCase))
            s = s.Substring(19);
        else if (s.StartsWith("ITEM_MALL_", StringComparison.OrdinalIgnoreCase))
            s = s.Substring(10);
        else if (s.StartsWith("ITEM_ETC_AVATAR_", StringComparison.OrdinalIgnoreCase))
            s = s.Substring(16);
        else if (s.StartsWith("ITEM_ETC_", StringComparison.OrdinalIgnoreCase))
            s = s.Substring(9);
        else if (s.StartsWith("ITEM_ITEM_LEGACY_MODEL_", StringComparison.OrdinalIgnoreCase))
            s = "Model " + s.Substring(23) + "Dg";
        else if (s.StartsWith("ITEM_ARTEMIS_SWITCHER_", StringComparison.OrdinalIgnoreCase))
            s = "Switcher " + s.Substring(22);
        else if (s.StartsWith("ITEM_", StringComparison.OrdinalIgnoreCase))
            s = s.Substring(5);

        return s.Replace("_", " ").Trim();
    }

    private void SendUnequipPacket(uint itemId, byte slot)
    {
        var packet = new Packet(0x3039);
        packet.WriteUInt(Game.Player.UniqueId);
        packet.WriteByte(slot);
        packet.WriteUInt(itemId);
        PacketManager.SendPacket(packet, PacketDestination.Client);
    }

    private uint GetAppliedItemForSlot(byte slot) => slot switch
    {
        0 => AppliedHatId,
        1 => AppliedDressId,
        2 => AppliedAccessoryId,
        3 => AppliedFlagId,
        _ => 0
    };

    private void SetAppliedItemForSlot(byte slot, uint itemId)
    {
        switch (slot)
        {
            case 0: AppliedHatId = itemId; break;
            case 1: AppliedDressId = itemId; break;
            case 2: AppliedAccessoryId = itemId; break;
            case 3: AppliedFlagId = itemId; break;
        }
    }

    #endregion
}
