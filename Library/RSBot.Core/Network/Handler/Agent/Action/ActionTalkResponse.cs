using RSBot.Core.Components;
using RSBot.Core.Event;
using RSBot.Core.Objects;
using RSBot.Core.Objects.Spawn;

namespace RSBot.Core.Network.Handler.Agent.Action;

internal class ActionTalkResponse : IPacketHandler
{
    #region Methods

    /// <summary>
    ///     Handles the packet.
    /// </summary>
    /// <param name="packet">The packet.</param>
    public void Invoke(Packet packet)
    {
        var result = packet.ReadByte();

        if (result != 1)
            return;

        if (Game.Player?.State != null && Game.Player.State.DialogState == null)
            Game.Player.State.DialogState = new DialogState();

        var talkOption = (TalkOption)packet.ReadByte();

        var requestedId = Game.Player?.State?.DialogState?.RequestedNpcId ?? 0;
        var npc = (requestedId > 0 ? SpawnManager.GetEntity<SpawnedNpc>(requestedId) : null)
               ?? (Game.SelectedEntity as SpawnedNpc)
               ?? (ShoppingManager.SelectedEntity as SpawnedNpc);

        if (Game.Player?.State?.DialogState != null)
        {
            if (npc != null)
                Game.Player.State.DialogState.Npc = npc;
            Game.Player.State.DialogState.TalkOption = talkOption;

            if (talkOption == TalkOption.Trade)
                Game.Player.State.DialogState.IsSpecialityTime = packet.ReadBool();
        }

        if (npc != null)
        {
            Game.SelectedEntity = npc;
            ShoppingManager.SelectedEntity = npc;
        }

        EventManager.FireEvent("OnSelectEntity", Game.SelectedEntity);
        EventManager.FireEvent("OnTalkToNpc", requestedId);
    }

    #endregion Methods

    #region Properties

    /// <summary>
    ///     Gets or sets the opcode.
    /// </summary>
    /// <value>
    ///     The opcode.
    /// </value>
    public ushort Opcode => 0xB046;

    /// <summary>
    ///     Gets or sets the destination.
    /// </summary>
    /// <value>
    ///     The destination.
    /// </value>
    public PacketDestination Destination => PacketDestination.Client;

    #endregion Properties
}