using System;
using System.Collections.Generic;
using System.Linq;
using RSBot.Core;
using RSBot.Core.Client.ReferenceObjects;
using RSBot.Core.Network;

namespace RSBot.AvatarTester;

public class AvatarTesterManager
{
    private static AvatarTesterManager _instance;
    public static AvatarTesterManager Instance => _instance ??= new AvatarTesterManager();

    public const uint AVATAR_TEST_UID = 0xFFFFFFFE;
    public bool IsPreviewSpawned { get; private set; }

    public uint SelectedHatId { get; set; }
    public uint SelectedDressId { get; set; }
    public uint SelectedAccessoryId { get; set; }
    public uint SelectedFlagId { get; set; }

    public event System.Action OnStateChanged;

    private AvatarTesterManager()
    {
    }

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
                (i.GetRealName() != null && i.GetRealName().Contains(filter, StringComparison.OrdinalIgnoreCase)));
        }

        return query.OrderBy(i => i.GetRealName()).ToList();
    }

    public List<RefObjItem> GetAvatarItemsByType(byte typeId4)
    {
        if (Game.ReferenceManager?.ItemData == null)
            return new List<RefObjItem>();

        return Game.ReferenceManager.ItemData.Values
            .Where(i => IsAvatarItem(i) && (i.TypeID4 == typeId4 || (typeId4 == 0)))
            .OrderBy(i => i.GetRealName())
            .ToList();
    }

    public void SpawnAvatarPreview()
    {
        if (Game.Player == null)
            return;

        // Despawn previous if already spawned
        if (IsPreviewSpawned)
            DespawnAvatarPreview();

        var player = Game.Player;
        var model = player.Id; // character model ID

        var isMale = player.Gender != ObjectGender.Female;
        var isChinese = player.Race == ObjectCountry.Chinese;

        var prefix = isChinese ? "CH" : "EU";
        var genderStr = isMale ? "M" : "W";

        var armorCode = $"ITEM_{prefix}_{genderStr}_LIGHT_01_BA_A_DEF";
        var legsCode = $"ITEM_{prefix}_{genderStr}_LIGHT_01_LA_A_DEF";
        var footsCode = $"ITEM_{prefix}_{genderStr}_LIGHT_01_FA_A_DEF";

        uint armorId = Game.ReferenceManager.ItemData.Values.FirstOrDefault(i => i.CodeName == armorCode)?.ID ?? 0;
        uint legsId = Game.ReferenceManager.ItemData.Values.FirstOrDefault(i => i.CodeName == legsCode)?.ID ?? 0;
        uint footsId = Game.ReferenceManager.ItemData.Values.FirstOrDefault(i => i.CodeName == footsCode)?.ID ?? 0;

        var avatars = new List<uint>();
        if (SelectedDressId != 0) avatars.Add(SelectedDressId);
        if (SelectedHatId != 0) avatars.Add(SelectedHatId);
        if (SelectedAccessoryId != 0) avatars.Add(SelectedAccessoryId);
        if (SelectedFlagId != 0) avatars.Add(SelectedFlagId);

        var packet = new Packet(0x3015);
        packet.WriteUInt(model);
        packet.WriteByte(34); // regular scale
        packet.WriteByte(0);  // zerk lv
        packet.WriteByte(0);  // pvp cape
        packet.WriteByte(0);  // exp type

        // Normal equipment inventory
        packet.WriteByte(45); // inv size
        packet.WriteByte(3);  // inv count
        packet.WriteUInt(armorId);
        packet.WriteByte(0);
        packet.WriteUInt(legsId);
        packet.WriteByte(0);
        packet.WriteUInt(footsId);
        packet.WriteByte(0);

        // Avatar inventory
        packet.WriteByte(5); // avatar inv size
        packet.WriteByte((byte)avatars.Count);
        foreach (var avId in avatars)
        {
            packet.WriteUInt(avId);
            packet.WriteByte(0);
        }

        packet.WriteByte(0); // has mask

        var pos = player.Movement.Source;
        packet.WriteUInt(AVATAR_TEST_UID);
        packet.WriteUShort(pos.Region.Id);
        packet.WriteFloat(pos.XOffset);
        packet.WriteFloat(pos.ZOffset + 50f);
        packet.WriteFloat(pos.YOffset);
        packet.WriteUShort(0); // angle

        packet.WriteByte(0); // has movement
        packet.WriteByte(1); // running mode
        packet.WriteByte(0);
        packet.WriteUShort(0); // angle

        packet.WriteByte(1); // life state
        packet.WriteByte(0);
        packet.WriteByte(0); // stand up
        packet.WriteByte(0); // bad status flags

        // Generic Speed (12 bytes)
        packet.WriteByteArray(new byte[] { 0xCD, 0xCC, 0x0C, 0x42, 0x00, 0x00, 0xDC, 0x42, 0x00, 0x00, 0xC8, 0x42 });

        packet.WriteByte(0); // buff count

        var name = "[Avatar Preview]";
        packet.WriteUShort((ushort)name.Length);
        packet.WriteByteArray(System.Text.Encoding.ASCII.GetBytes(name));

        // Generic job and other stuffs (33 bytes)
        packet.WriteByteArray(new byte[]
        {
            0x00, 0x01, 0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0xFF, 0x04
        });

        PacketManager.SendPacket(packet, PacketDestination.Client);
        IsPreviewSpawned = true;
        OnStateChanged?.Invoke();
    }

    public void DespawnAvatarPreview()
    {
        var packet = new Packet(0x3016);
        packet.WriteUInt(AVATAR_TEST_UID);
        PacketManager.SendPacket(packet, PacketDestination.Client);
        IsPreviewSpawned = false;
        OnStateChanged?.Invoke();
    }
}
