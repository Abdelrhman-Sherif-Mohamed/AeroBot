using System;
using System.Collections.Generic;
using System.Linq;
using System.Timers;
using RSBot.Core.Components;
using RSBot.Core.Network;

namespace RSBot.Core.Objects.Spawn;

public class SpawnedBionic : SpawnedEntity
{
    /// <summary>
    ///     <inheritdoc />
    /// </summary>
    /// <param name="objId">The ref obj id</param>
    public SpawnedBionic(uint objId)
    {
        Id = objId;

        if (Record != null)
            Health = Record.MaxHealth;
    }

    /// <summary>
    ///     Gets the distance to player.
    /// </summary>
    /// <value>
    ///     The distance to player.
    /// </value>
    public double DistanceToPlayer => Game.Player.Movement.Source.DistanceTo(Movement.Source);

    private int _attackingPlayerTick;

    /// <summary>
    ///     Gets a value indicating whether [attacking player].
    /// </summary>
    /// <value>
    ///     <c>true</c> if [attacking player]; otherwise, <c>false</c>.
    /// </value>
    public bool AttackingPlayer => _attackingPlayerTick != 0 && (Kernel.TickCount - _attackingPlayerTick < 10000);

    /// <summary>
    ///     Gets or sets a value indicating whether this instance has health.
    /// </summary>
    /// <value>
    ///     <c>true</c> if this instance has health; otherwise, <c>false</c>.
    /// </value>
    public bool HasHealth => Health > 0;

    /// <summary>
    ///     Gets or sets the health.
    /// </summary>
    /// <value>
    ///     The health.
    /// </value>
    public int Health { get; set; }

    /// <summary>
    ///     Gets or sets the bad effect.
    /// </summary>
    /// <value>
    ///     The bad effect.
    /// </value>
    public BadEffect BadEffect { get; set; }

    /// <summary>
    ///     Gets or sets the target identifier.
    /// </summary>
    /// <value>
    ///     The target identifier.
    /// </value>
    public uint TargetId { get; set; }

    /// <summary>
    ///     Parse the bionic details
    /// </summary>
    /// <param name="packet">The packet</param>
    internal void ParseBionicDetails(Packet packet)
    {
        UniqueId = packet.ReadUInt();

        var movement = Movement.FromPacket(packet);
        State.Deserialize(packet);
        SetMovement(movement);
    }

    /// <summary>
    ///     Starts the attacking timer.
    /// </summary>
    /// <param name="duration">The duration.</param>
    public void StartAttackingTimer(int duration = 10000)
    {
        _attackingPlayerTick = Kernel.TickCount;
    }

    /// <summary>
    ///     Clears the attacking player state.
    /// </summary>
    public void StopAttackingTimer()
    {
        _attackingPlayerTick = 0;
    }

    /// <summary>
    ///     Selects the entity immediately without blocking the bot thread.
    /// </summary>
    /// <returns></returns>
    public bool TrySelect()
    {
        if (Game.SelectedEntity?.UniqueId == UniqueId)
            return true;

        Game.SelectedEntity = this;

        Log.Debug(
            $"Selecting entity: {UniqueId} State: {State?.LifeState} Health: {Health} HasHealth: {HasHealth} Dst: {Math.Round(DistanceToPlayer, 1)}");

        var packet = new Packet(0x7045);
        packet.WriteUInt(UniqueId);

        PacketManager.SendPacket(packet, PacketDestination.Server);

        return true;
    }

    /// <summary>
    ///     Deselects the entity.
    /// </summary>
    /// <returns></returns>
    public bool TryDeselect()
    {
        Log.Debug($"Entity deselected: {UniqueId}");

        // If the server does not acknowledge the close (0xB04B) the NPC dialog stays
        // open server-side and the player becomes unable to move. Retry once.
        for (var attempt = 1; attempt <= 2; attempt++)
        {
            var packet = new Packet(0x704B);
            packet.WriteUInt(UniqueId);

            var awaitResult = new AwaitCallback(response =>
            {
                return response.ReadByte() == 0x01
                    ? AwaitCallbackResult.Success
                    : AwaitCallbackResult.Fail;
            }, 0xB04B);

            PacketManager.SendPacket(packet, PacketDestination.Server, awaitResult);
            awaitResult.AwaitResponse(600);

            if (awaitResult.IsCompleted)
                return true;
        }

        return true;
    }

    /// <summary>
    ///     Gets a list of spawned bionics that are attacking this entity.
    /// </summary>
    /// <returns></returns>
    public List<SpawnedBionic> GetAttackers()
    {
        return !SpawnManager.TryGetEntities<SpawnedBionic>(e => e.TargetId == UniqueId, out var attackers)
            ? new List<SpawnedBionic>()
            : attackers.ToList();
    }
}
