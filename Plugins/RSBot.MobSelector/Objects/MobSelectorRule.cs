using RSBot.Core.Objects;
using RSBot.Core.Objects.Spawn;

namespace RSBot.MobSelector.Objects;

public class MobSelectorRule
{
    public string Name { get; set; } = string.Empty;
    public byte Rarity { get; set; } = 0xFF; // 0xFF = Any rarity

    public MobSelectorRule()
    {
    }

    public MobSelectorRule(string name, byte rarity = 0xFF)
    {
        Name = name;
        Rarity = rarity;
    }

    public bool Matches(SpawnedMonster monster)
    {
        if (monster == null) return false;

        // If specific rarity required, check it
        if (Rarity != 0xFF && (byte)monster.Rarity != Rarity)
            return false;

        // If name specified, check match
        if (!string.IsNullOrEmpty(Name))
        {
            var mobName = monster.Record?.GetRealName() ?? "";
            var mobCode = monster.Record?.CodeName ?? "";

            if (!mobName.Contains(Name, System.StringComparison.OrdinalIgnoreCase) &&
                !mobCode.Contains(Name, System.StringComparison.OrdinalIgnoreCase))
                return false;
        }

        return true;
    }

    public string DisplayText
    {
        get
        {
            var rarityText = Rarity == 0xFF ? "Any Rarity" : ((MonsterRarity)Rarity).ToString();
            return string.IsNullOrEmpty(Name) ? $"[{rarityText}]" : $"{Name} [{rarityText}]";
        }
    }
}
