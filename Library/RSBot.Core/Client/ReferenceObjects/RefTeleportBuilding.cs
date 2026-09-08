using RSBot.Core.Objects;

namespace RSBot.Core.Client.ReferenceObjects;

/// <summary>
///     A teleporter building (dimensional gate / fortress gate house) from
///     TeleportBuilding.txt: the EXACT walkable position of the gate NPC,
///     which usually differs from the teleport destination point.
/// </summary>
public class RefTeleportBuilding : IReference<uint>
{
    public bool Load(ReferenceParser parser)
    {
        //Skip disabled
        if (!parser.TryParse(0, out Service) || Service == 0)
            return false;

        //Skip invalid ID (PK, shared with CharacterData AssocRefObjId)
        if (!parser.TryParse(1, out ID))
            return false;

        //Skip invalid CodeName
        if (!parser.TryParse(2, out CodeName))
            return false;

        parser.TryParse(5, out NpcNameKey);
        parser.TryParse(41, out RegionId);
        parser.TryParse(43, out PosX);
        parser.TryParse(44, out PosZ);
        parser.TryParse(45, out PosY);

        return true;
    }

    /// <summary>
    ///     Gets the walkable position of the gate NPC.
    /// </summary>
    public Position GetPosition()
    {
        return new Position(unchecked((ushort)RegionId), PosX, PosY, PosZ);
    }

    #region Fields

    public byte Service;
    public uint ID;
    public string CodeName;
    public string NpcNameKey;
    public int RegionId;
    public short PosX;
    public short PosZ;
    public short PosY;

    public uint PrimaryKey => ID;

    #endregion Fields
}
