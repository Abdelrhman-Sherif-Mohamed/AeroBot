using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using RSBot.Core;
using RSBot.Core.Client.ReferenceObjects;
using RSBot.Core.Objects;

namespace RSBot.Lure.Components;

public class LureSpot
{
    public ushort Region { get; set; }
    public float XOffset { get; set; }
    public float YOffset { get; set; }
    public float ZOffset { get; set; }
    public int Radius { get; set; } = 30;

    public Position Position => new Position(Region, XOffset, YOffset, ZOffset);
    public float X => Position.X;
    public float Y => Position.Y;

    public string Serialize()
    {
        return $"{Region},{XOffset.ToString(CultureInfo.InvariantCulture)},{YOffset.ToString(CultureInfo.InvariantCulture)},{ZOffset.ToString(CultureInfo.InvariantCulture)},{Radius}";
    }

    public static LureSpot Deserialize(string s)
    {
        if (string.IsNullOrWhiteSpace(s)) return null;
        var parts = s.Split(',');
        if (parts.Length < 4) return null;
        if (!ushort.TryParse(parts[0], out var reg)) return null;
        if (!float.TryParse(parts[1], NumberStyles.Float, CultureInfo.InvariantCulture, out var x)) return null;
        if (!float.TryParse(parts[2], NumberStyles.Float, CultureInfo.InvariantCulture, out var y)) return null;
        if (!float.TryParse(parts[3], NumberStyles.Float, CultureInfo.InvariantCulture, out var z)) return null;
        int rad = 30;
        if (parts.Length >= 5) int.TryParse(parts[4], out rad);
        return new LureSpot { Region = reg, XOffset = x, YOffset = y, ZOffset = z, Radius = rad };
    }
}

internal static class LureConfig
{
    public static List<LureSpot> Spots
    {
        get
        {
            var raw = PlayerConfig.Get("RSBot.Lure.SpotsList", "");
            if (string.IsNullOrWhiteSpace(raw))
                return new List<LureSpot>();

            var items = new List<LureSpot>();
            foreach (var part in raw.Split(new[] { '|' }, StringSplitOptions.RemoveEmptyEntries))
            {
                var spot = LureSpot.Deserialize(part);
                if (spot != null) items.Add(spot);
            }
            return items;
        }
        set
        {
            if (value == null || value.Count == 0)
            {
                PlayerConfig.Set("RSBot.Lure.SpotsList", "");
                return;
            }

            var raw = string.Join("|", value.Select(s => s.Serialize()));
            PlayerConfig.Set("RSBot.Lure.SpotsList", raw);
        }
    }

    public static bool UseHowlingShout
    {
        get => PlayerConfig.Get("RSBot.Lure.UseHowlingShout", true) && Game.Player != null && Game.Player.Race == ObjectCountry.Europe;
        set => PlayerConfig.Set("RSBot.Lure.UseHowlingShout", value);
    }

    public static bool UseNormalAttack
    {
        get => PlayerConfig.Get("RSBot.Lure.UseNormalAttack", false);
        set => PlayerConfig.Set("RSBot.Lure.UseNormalAttack", value);
    }

    public static bool StopIfNumPartyMemberDead
    {
        get => PlayerConfig.Get("RSBot.Lure.StopIfNumPartyMemberDead", false);
        set => PlayerConfig.Set("RSBot.Lure.StopIfNumPartyMemberDead", value);
    }

    public static int NumPartyMemberDead
    {
        get => PlayerConfig.Get("RSBot.Lure.NumPartyMemberDead", 3);
        set => PlayerConfig.Set("RSBot.Lure.NumPartyMemberDead", value);
    }

    public static bool StopIfNumPartyMember
    {
        get => PlayerConfig.Get("RSBot.Lure.StopIfNumPartyMember", false);
        set => PlayerConfig.Set("RSBot.Lure.StopIfNumPartyMember", value);
    }

    public static int NumPartyMember
    {
        get => PlayerConfig.Get("RSBot.Lure.NumPartyMember", 3);
        set => PlayerConfig.Set("RSBot.Lure.NumPartyMember", value);
    }

    public static bool StopIfNumMonsterType
    {
        get => PlayerConfig.Get("RSBot.Lure.StopIfNumMonsterType", false);
        set => PlayerConfig.Set("RSBot.Lure.StopIfNumMonsterType", value);
    }

    public static MonsterRarity SelectedMonsterType
    {
        get => PlayerConfig.GetEnum("RSBot.Lure.SelectedMonsterType", MonsterRarity.General);
        set => PlayerConfig.Set("RSBot.Lure.SelectedMonsterType", (byte)value);
    }

    public static int NumMonsterType
    {
        get => PlayerConfig.Get("RSBot.Lure.NumMonsterType", 3);
        set => PlayerConfig.Set("RSBot.Lure.NumMonsterType", value);
    }

    public static bool UseScript
    {
        get => PlayerConfig.Get("RSBot.Lure.UseScript", false);
        set => PlayerConfig.Set("RSBot.Lure.UseScript", value);
    }

    public static string SelectedScriptPath
    {
        get => PlayerConfig.Get("RSBot.Lure.SelectedScriptPath", "");
        set => PlayerConfig.Set("RSBot.Lure.SelectedScriptPath", value);
    }

    public static bool WalkRandomly
    {
        get => PlayerConfig.Get("RSBot.Lure.WalkRandomly", true);
        set => PlayerConfig.Set("RSBot.Lure.WalkRandomly", value);
    }

    public static bool StayAtCenter
    {
        get => PlayerConfig.Get("RSBot.Lure.StayAtCenter", false);
        set => PlayerConfig.Set("RSBot.Lure.StayAtCenter", value);
    }

    public static bool StayAtCenterFor
    {
        get => true; // Always return to party in lure bot
        set => PlayerConfig.Set("RSBot.Lure.StayAtCenterFor", value);
    }

    public static int StayAtCenterForSeconds
    {
        get => PlayerConfig.Get("RSBot.Lure.StayAtCenterForSeconds", 3);
        set => PlayerConfig.Set("RSBot.Lure.StayAtCenterForSeconds", value);
    }

    public static bool NoHowlingAtCenter
    {
        get => PlayerConfig.Get("RSBot.Lure.NoHowlingAtCenter", true);
        set => PlayerConfig.Set("RSBot.Lure.NoHowlingAtCenter", value);
    }

    public static bool UseAttackingSkills
    {
        get => PlayerConfig.Get("RSBot.Lure.UseAttackingSkills", false);
        set => PlayerConfig.Set("RSBot.Lure.UseAttackingSkills", value);
    }

    public static string WalkscriptPath
    {
        get => PlayerConfig.Get("RSBot.Walkback.File", "");
        set => PlayerConfig.Set("RSBot.Walkback.File", value);
    }

    public static Area Area
    {
        get
        {
            var region = PlayerConfig.Get<ushort>("RSBot.Lure.Area.Region");
            var x = PlayerConfig.Get("RSBot.Lure.Area.X", 0f);
            var y = PlayerConfig.Get("RSBot.Lure.Area.Y", 0f);
            var z = PlayerConfig.Get("RSBot.Lure.Area.Z", 0f);
            var r = PlayerConfig.Get("RSBot.Lure.Area.Radius", 50);

            return new Area
            {
                Name = "Party",
                Position = new Position(region, x, y, z),
                Radius = r
            };
        }
        set
        {
            PlayerConfig.Set("RSBot.Lure.Area.Region", value.Position.Region);
            PlayerConfig.Set("RSBot.Lure.Area.X", value.Position.XOffset);
            PlayerConfig.Set("RSBot.Lure.Area.Y", value.Position.YOffset);
            PlayerConfig.Set("RSBot.Lure.Area.Z", value.Position.ZOffset);
            PlayerConfig.Set("RSBot.Lure.Area.Radius", value.Radius);
        }
    }
}
