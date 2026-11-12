using System.Runtime.InteropServices;

namespace AeroBot.Data.Models;

/// <summary>
/// Binary snapshot header placed at byte offset 0 of <c>GameData.bin</c>.
/// </summary>
[StructLayout(LayoutKind.Sequential, Pack = 4)]
public readonly record struct GameDataHeader
{
    /// <summary>
    /// Little-endian ASCII signature <c>"SOT0"</c> (<c>0x30544F53</c>).
    /// </summary>
    public const uint ExpectedMagic = 0x30544F53;

    /// <summary>
    /// Current binary snapshot layout version.
    /// </summary>
    public const uint CurrentVersion = 1;

    public readonly uint Magic;
    public readonly uint Version;
    public readonly int ItemCount;
    public readonly int SkillCount;
    public readonly int CharacterCount;
    public readonly int StringArenaSizeBytes;

    public GameDataHeader(
        uint magic,
        uint version,
        int itemCount,
        int skillCount,
        int characterCount,
        int stringArenaSizeBytes)
    {
        Magic = magic;
        Version = version;
        ItemCount = itemCount;
        SkillCount = skillCount;
        CharacterCount = characterCount;
        StringArenaSizeBytes = stringArenaSizeBytes;
    }

    public bool IsValid =>
        Magic == ExpectedMagic &&
        Version == CurrentVersion &&
        ItemCount >= 0 &&
        SkillCount >= 0 &&
        CharacterCount >= 0 &&
        StringArenaSizeBytes >= 0;
}
