using System;
using RSBot.Core.Objects.Spawn;

namespace RSBot.Core.Components;

/// <summary>
///     Allows other components (e.g. the Control plugin's Target Support) to
///     force a target that the active bot base must keep selected and attack.
///     The default bot base normally only targets monsters, so this is the
///     bridge that makes "follow and attack the leader's target" work on
///     players as well.
/// </summary>
public static class TargetOverride
{
    private static uint _uniqueId;
    private static int _expiresAt;

    /// <summary>
    ///     The forced target unique id, or zero when no override is active.
    /// </summary>
    public static uint UniqueId => IsActive ? _uniqueId : 0;

    /// <summary>
    ///     Whether a forced target is currently active and not expired.
    /// </summary>
    public static bool IsActive => _uniqueId != 0 && Kernel.TickCount < _expiresAt;

    /// <summary>
    ///     Forces the given entity as the current target.
    /// </summary>
    /// <param name="uniqueId">The entity unique id.</param>
    /// <param name="durationMs">How long the override stays valid.</param>
    public static void Set(uint uniqueId, int durationMs = 1500)
    {
        if (uniqueId == 0)
        {
            Clear();
            return;
        }

        _uniqueId = uniqueId;
        _expiresAt = Kernel.TickCount + Math.Max(100, durationMs);
    }

    /// <summary>
    ///     Extends the lifetime of the current override without changing it.
    /// </summary>
    public static void Refresh(int durationMs = 1500)
    {
        if (_uniqueId == 0)
            return;

        _expiresAt = Kernel.TickCount + Math.Max(100, durationMs);
    }

    /// <summary>
    ///     Clears the forced target.
    /// </summary>
    public static void Clear()
    {
        _uniqueId = 0;
        _expiresAt = 0;
    }

    /// <summary>
    ///     Checks whether the given entity is the active forced target.
    /// </summary>
    public static bool IsForced(uint uniqueId)
    {
        return IsActive && _uniqueId == uniqueId;
    }

    /// <summary>
    ///     Resolves the forced target to a spawned entity.
    /// </summary>
    public static bool TryGet(out SpawnedBionic bionic)
    {
        bionic = null;

        if (!IsActive)
            return false;

        return SpawnManager.TryGetEntity(_uniqueId, out bionic);
    }
}
