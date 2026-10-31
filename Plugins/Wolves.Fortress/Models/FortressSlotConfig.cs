namespace Wolves.Fortress.Models;

public class FortressSlotConfig
{
    public int SlotIndex { get; set; }
    public string PlayerName { get; set; } = string.Empty;
    public uint SkillId { get; set; }
    public string SkillName { get; set; } = string.Empty;
    public bool Enabled { get; set; } = true;

    public FortressSlotConfig() { }

    public FortressSlotConfig(int index)
    {
        SlotIndex = index;
    }
}
