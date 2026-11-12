using System;
using System.Diagnostics;
using RSBot.Core.Components;
using RSBot.Core.Event;
using RSBot.Core.Network;
using RSBot.Core.Objects.Spawn;

namespace RSBot.Core.Objects;

public class Action
{
    /// <summary>
    ///     Gets or sets the action identifier.
    /// </summary>
    /// <value>
    ///     The action identifier.
    /// </value>
    public byte Code { get; set; }

    /// <summary>
    ///     Gets or sets the skill identifier.
    /// </summary>
    /// w
    /// <value>
    ///     The skill identifier.
    /// </value>
    public uint SkillId { get; set; }

    /// <summary>
    ///     Gets or sets the executor identifier.
    /// </summary>
    /// <value>
    ///     The executor identifier.
    /// </value>
    public uint ExecutorId { get; set; }

    /// <summary>
    ///     Gets or sets the identifier.
    /// </summary>
    /// <value>
    ///     The identifier.
    /// </value>
    public uint Id { get; set; }

    /// <summary>
    ///     Gets or sets the target identifier.
    /// </summary>
    /// <value>
    ///     The target identifier.
    /// </value>
    public uint TargetId { get; set; }

    /// <summary>
    ///     Gets or sets the target identifier.
    /// </summary>
    /// <value>
    ///     The target identifier.
    /// </value>
    public uint UnknownId { get; set; }

    /// <summary>
    ///     Gets or sets the flag.
    /// </summary>
    /// <value>
    ///     The flag.
    /// </value>
    public ActionStateFlag Flag { get; set; }

    /// <summary>
    ///     Gets a value indicating whether [player is executor].
    /// </summary>
    /// <value>
    ///     <c>true</c> if [player is executor]; otherwise, <c>false</c>.
    /// </value>
    public bool PlayerIsExecutor => Game.Player != null && Game.Player.UniqueId == ExecutorId;

    /// <summary>
    ///     Gets a value indicating whether [player is target].
    /// </summary>
    /// <value>
    ///     <c>true</c> if [player is target]; otherwise, <c>false</c>.
    /// </value>
    public bool PlayerIsTarget => Game.Player != null && Game.Player.UniqueId == TargetId;

    /// <summary>
    ///     Deserialize the packet. 0xB070
    /// </summary>
    /// <param name="packet">The packet.</param>
    /// <returns>Deserialized <see cref="Action" /></returns>
    public static Action DeserializeBegin(Packet packet)
    {
        var actionCode = packet.ReadByte();

        if (Game.ClientType > GameClientType.Thailand)
            packet.ReadByte(); // always 0x30

        var action = new Action
        {
            Code = actionCode,
            SkillId = packet.ReadUInt(),
            ExecutorId = packet.ReadUInt(),
            Id = packet.ReadUInt()
        };

        if (Game.ClientType >= GameClientType.Global)
            action.UnknownId = packet.ReadUInt();

        action.TargetId = packet.ReadUInt();
        if (Game.ClientType == GameClientType.Turkey ||
            Game.ClientType == GameClientType.Global ||
            Game.ClientType == GameClientType.VTC_Game)
        {
            packet.ReadByte();
            action.Flag = (ActionStateFlag)packet.ReadByte();
        }
        else if (Game.ClientType == GameClientType.Rigid)
        {
            action.Flag = (ActionStateFlag)packet.ReadByte();
            var flag = packet.ReadByte();
            Debug.WriteLine("Flag:" + flag);
        }
        else
        {
            action.Flag = (ActionStateFlag)packet.ReadByte();
        }

        /*if (Game.ClientType >= GameClientType.Global)
            packet.ReadByte();

        action.Flag = (ActionStateFlag)packet.ReadByte();*/
        KillDeathTracker.OnSkillCast(action);

        if (packet.Remaining > 0)
            action.SerializeDetail(packet);

        if (action.TargetId != 0)
        {
            if (SpawnManager.TryGetEntity<SpawnedBionic>(action.ExecutorId, out var executor))
            {
                executor.TargetId = action.TargetId;
                if (action.TargetId == Game.Player?.UniqueId)
                    executor.StartAttackingTimer();
            }

            EventManager.FireEvent("OnEntityHit", action.Id, action.ExecutorId, action.TargetId, 0, false);
        }

        return action;
    }

    /// <summary>
    ///     Deserialize the packet. 0xB071
    /// </summary>
    /// <param name="packet">The packet.</param>
    /// <returns>Deserialized <see cref="Action" /></returns>
    public static Action DeserializeEnd(Packet packet)
    {
        var action = new Action();
        action.Id = packet.ReadUInt(); //ActionId
        action.TargetId = packet.ReadUInt(); //originalTargetId

        action.Flag = (ActionStateFlag)packet.ReadByte();
        action.SerializeDetail(packet);

        return action;
    }

    public void SerializeDetail(Packet packet)
    {
        try
        {
            if (packet == null || packet.Remaining == 0)
                return;

            if (Flag.HasFlag(ActionStateFlag.Attack))
            {
                if (packet.Remaining < 2)
                    return;

                var hitCount = packet.ReadByte();
                var affectedObjectCount = packet.ReadByte();

                for (var i = 0; i < affectedObjectCount; i++)
                {
                    if (packet.Remaining < 4)
                        break;

                    var uniqueId = packet.ReadUInt();
                    SpawnManager.TryGetEntity<SpawnedBionic>(uniqueId, out var entity);

                    for (var j = 0; j < hitCount; j++)
                    {
                        if (packet.Remaining < 1)
                            break;

                        var state = (ActionHitStateFlag)packet.ReadByte();
                        if (state == ActionHitStateFlag.Abort)
                            break;

                        if (entity != null)
                        {
                            entity.State.HitState = state;
                            if (state.HasFlag(ActionHitStateFlag.Dead))
                            {
                                entity.State.LifeState = LifeState.Dead;
                                entity.Health = 0;
                                if (Game.SelectedEntity?.UniqueId == uniqueId)
                                    Game.SelectedEntity = null;
                            }
                        }

                        if (state != ActionHitStateFlag.Block)
                        {
                            if (packet.Remaining < 8)
                            {
                                KillDeathTracker.OnHitDetail(Id, ExecutorId, uniqueId, state, 0);
                                break;
                            }

                            var critStatus = packet.ReadByte(); // 0x01: normal 0x02 critical

                            var damage = BitConverter.ToInt32(new byte[]
                            {
                                packet.ReadByte(),
                                packet.ReadByte(),
                                packet.ReadByte(),
                                0
                            }, 0);

                            var unknownState = packet.ReadInt();
                            Debug.WriteLine("UnknownState:" + unknownState);

                            KillDeathTracker.OnHitDetail(Id, ExecutorId, uniqueId, state, damage);
                            EventManager.FireEvent("OnEntityHit", Id, ExecutorId, uniqueId, damage, critStatus == 0x02);
                        }
                        else
                        {
                            KillDeathTracker.OnHitDetail(Id, ExecutorId, uniqueId, state, 0);
                        }

                        // dont worry it will return true for knockdown states
                        if (state.HasFlag(ActionHitStateFlag.KnockBack))
                        {
                            if (packet.Remaining < 14)
                                break;

                            var position = Position.FromPacketInt(packet);
                            if (entity != null)
                                entity.SetSource(position);
                        }
                    }
                }
            }

            if (Flag.HasFlag(ActionStateFlag.Teleport))
            {
                if (packet.Remaining < 14)
                    return;

                var position = Position.FromPacketInt(packet);
                if (PlayerIsExecutor)
                {
                    Game.Player?.SetSource(position);
                }
                else
                {
                    if (!TryGetExecutor<SpawnedBionic>(out var executor))
                        return;

                    executor?.SetSource(position);
                }
            }
        }
        catch (System.IO.EndOfStreamException)
        {
            // Packet detail ended early, safe to ignore
        }
        catch (Exception ex)
        {
            Log.Debug($"Action.SerializeDetail: {ex.Message}");
        }
    }

    /// <summary>
    ///     Gets the executor.
    /// </summary>
    public bool TryGetExecutor<T>(out T entity) where T : SpawnedBionic
    {
        return SpawnManager.TryGetEntity(ExecutorId, out entity);
    }

    /// <summary>
    ///     Gets the target.
    /// </summary>
    /// <returns></returns>
    public bool TryGetTarget<T>(out T entity) where T : SpawnedBionic
    {
        return SpawnManager.TryGetEntity(TargetId, out entity);
    }
}