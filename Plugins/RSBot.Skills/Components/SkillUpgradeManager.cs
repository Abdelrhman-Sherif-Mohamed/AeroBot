using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using RSBot.Core;
using RSBot.Core.Client.ReferenceObjects;
using RSBot.Core.Objects;
using RSBot.Core.Objects.Skill;

namespace RSBot.Skills.Components;

/// <summary>
///     Handles the automatic upgrade of masteries and the skills that belong to them.
/// </summary>
internal static class SkillUpgradeManager
{
    private const string AutoMasteryConfigKey = "RSBot.Skills.AutoMasteryEnabled";
    private const int LearnDelay = 250;
    private const int MaxActionsPerRun = 250;

    private static readonly ConcurrentDictionary<uint, byte> _autoMasteries = new();
    private static readonly ConcurrentDictionary<uint, byte> _blockedSkills = new();
    private static readonly ConcurrentQueue<MasteryRequest> _manualRequests = new();
    private static readonly object _stateLock = new();
    private static readonly object _indexLock = new();

    private static Dictionary<(string Prefix, byte Level), RefSkill> _skillIndex;

    private static int _actionActive;
    private static int _masteryCursor;
    private static volatile bool _pendingRun;
    private static bool _settingsLoaded;

    public static bool IsAutoEnabled(uint masteryId)
    {
        return _autoMasteries.ContainsKey(masteryId);
    }

    public static void LoadSettings()
    {
        _autoMasteries.Clear();
        _blockedSkills.Clear();

        lock (_indexLock)
            _skillIndex = null;

        foreach (var masteryId in PlayerConfig.GetArray<uint>(AutoMasteryConfigKey))
            if (masteryId != 0)
                _autoMasteries[masteryId] = 0;

        _settingsLoaded = true;
    }

    public static void EnsureSettingsLoaded()
    {
        if (_settingsLoaded)
            return;

        lock (_stateLock)
        {
            if (!_settingsLoaded)
                LoadSettings();
        }
    }

    public static void SetAutoEnabled(uint masteryId, bool enabled)
    {
        EnsureSettingsLoaded();

        if (enabled)
            _autoMasteries[masteryId] = 0;
        else
            _autoMasteries.TryRemove(masteryId, out _);

        SaveSettings();

        if (enabled)
            RunAutoUpgrade();
    }

    public static void ToggleAuto(uint masteryId)
    {
        SetAutoEnabled(masteryId, !IsAutoEnabled(masteryId));
    }

    public static void SaveSettings()
    {
        PlayerConfig.SetArray(AutoMasteryConfigKey, _autoMasteries.Keys.OrderBy(p => p).ToArray());
        PlayerConfig.Save();
    }

    public static void ResetBlockedSkills()
    {
        _blockedSkills.Clear();
    }

    /// <summary>
    ///     Levels the given mastery up once, or until it reaches the player level.
    /// </summary>
    public static void LearnMastery(uint masteryId, bool maxLevel)
    {
        EnsureSettingsLoaded();

        _manualRequests.Enqueue(new MasteryRequest(masteryId, maxLevel));
        TryProcessManualRequests();
    }

    private static void TryProcessManualRequests()
    {
        if (_manualRequests.IsEmpty)
            return;

        if (!TryEnterAction())
            return;

        Task.Run(() =>
        {
            try
            {
                while (_manualRequests.TryDequeue(out var request))
                {
                    var player = Game.Player;
                    if (player?.Skills == null)
                        continue;

                    var mastery = player.Skills.GetMasteryInfoById(request.MasteryId);
                    if (mastery == null)
                        continue;

                    var actions = 0;
                    do
                    {
                        if (!TryLearnMastery(mastery, true))
                            break;

                        actions++;
                        if (!request.MaxLevel || actions >= MaxActionsPerRun)
                            break;

                        mastery = Game.Player?.Skills?.GetMasteryInfoById(request.MasteryId);
                    } while (mastery != null && mastery.Level < player.Level);
                }
            }
            finally
            {
                ExitAction();
            }
        });
    }

    /// <summary>
    ///     Runs the auto upgrade loop for every mastery that has auto upgrade enabled.
    /// </summary>
    public static void RunAutoUpgrade()
    {
        EnsureSettingsLoaded();

        if (_autoMasteries.IsEmpty)
            return;

        if (!TryEnterAction())
        {
            _pendingRun = true;
            return;
        }

        Task.Run(() =>
        {
            try
            {
                do
                {
                    _pendingRun = false;
                    AutoUpgradeLoop();
                } while (_pendingRun);
            }
            finally
            {
                ExitAction();
            }
        });
    }

    private static void AutoUpgradeLoop()
    {
        for (var action = 0; action < MaxActionsPerRun; action++)
        {
            var player = Game.Player;
            if (player?.Skills == null)
                return;

            var enabledMasteries = _autoMasteries.Keys.ToArray();
            if (enabledMasteries.Length == 0)
                return;

            var didSomething = false;

            for (var index = 0; index < enabledMasteries.Length; index++)
            {
                var masteryId = enabledMasteries[(_masteryCursor + index) % enabledMasteries.Length];
                if (!IsAutoEnabled(masteryId))
                    continue;

                var mastery = player.Skills.GetMasteryInfoById(masteryId);
                if (mastery == null)
                    continue;

                if (mastery.Level < player.Level && TryLearnMastery(mastery, true))
                {
                    _masteryCursor = (_masteryCursor + index + 1) % enabledMasteries.Length;
                    didSomething = true;
                    break;
                }

                if (TryLearnNextSkill(masteryId))
                {
                    _masteryCursor = (_masteryCursor + index + 1) % enabledMasteries.Length;
                    didSomething = true;
                    break;
                }
            }

            if (!didSomething)
                return;

            Thread.Sleep(LearnDelay);
        }
    }

    private static bool TryLearnMastery(MasteryInfo mastery, bool log)
    {
        var player = Game.Player;
        if (player == null || mastery == null || mastery.Level >= player.Level)
            return false;

        var nextLevel = Game.ReferenceManager.GetRefLevel((byte)(mastery.Level + 1));
        if (nextLevel == null)
            return false;

        if (nextLevel.Exp_M > 0 && (uint)nextLevel.Exp_M > player.SkillPoints)
        {
            if (log)
                Log.Debug(
                    $"Auto. mastery upgrade cancelled due to insufficient skill points. Required: {nextLevel.Exp_M}");

            return false;
        }

        if (log)
            Log.Notify($"Auto. train mastery [{mastery.Record?.Name}] to lv. {mastery.Level + 1}");

        var previousLevel = mastery.Level;

        if (!LearnMasteryHandler.LearnMastery(mastery.Id))
            return false;

        var updated = Game.Player?.Skills?.GetMasteryInfoById(mastery.Id);
        return updated != null && updated.Level > previousLevel;
    }

    private static bool TryLearnNextSkill(uint masteryId)
    {
        var player = Game.Player;
        if (player?.Skills == null)
            return false;

        var candidates = new List<RefSkill>();
        var knownSkillIds = new HashSet<uint>(player.Skills.KnownSkills.Select(p => p.Id));

        // The next level of every skill line the player already knows.
        foreach (var knownSkill in player.Skills.KnownSkills)
        {
            var record = knownSkill.Record;
            if (record == null ||
                (record.ReqCommon_Mastery1 != (int)masteryId &&
                 record.ReqCommon_Mastery2 != (int)masteryId))
                continue;

            var next = FindNextSkill(record);
            if (next != null && !knownSkillIds.Contains(next.ID))
                candidates.Add(next);
        }

        // New skill lines that become available with the current mastery level.
        foreach (var skill in Game.ReferenceManager.SkillData.Values)
        {
            if (skill == null || skill.Basic_Level != 1 || skill.ReqLearn_SP <= 0)
                continue;

            if (skill.ReqCommon_Mastery1 != (int)masteryId &&
                skill.ReqCommon_Mastery2 != (int)masteryId)
                continue;

            if (knownSkillIds.Contains(skill.ID))
                continue;

            candidates.Add(skill);
        }

        foreach (var nextSkill in candidates
                     .Where(p => p != null && !_blockedSkills.ContainsKey(p.ID))
                     .GroupBy(p => p.ID)
                     .Select(g => g.First())
                     .OrderBy(p => p.Basic_Level)
                     .ThenBy(p => p.ID))
        {
            if (knownSkillIds.Contains(nextSkill.ID))
                continue;

            if (!CanLearnSkill(nextSkill, player))
                continue;

            var success = LearnSkillHandler.LearnSkill(nextSkill.ID);
            var learned = Game.Player?.Skills?.HasSkill(nextSkill.ID) == true;

            if (success && learned)
            {
                Log.Notify($"Auto. learned skill [{nextSkill.GetRealName()}] lv. {nextSkill.Basic_Level}");
                return true;
            }

            _blockedSkills[nextSkill.ID] = 0;
            Log.Debug($"Auto. skill upgrade failed for [{nextSkill.GetRealName()}] lv. {nextSkill.Basic_Level}");
        }

        return false;
    }

    /// <summary>
    ///     Finds the skill of the next level in the same skill line.
    ///     <para>
    ///         Note: <see cref="RefSkill.Basic_ChainCode" /> does not point to the previous level, it links
    ///         the skill to its hidden companion skill (e.g. the "A2" variants). Therefore the next level is
    ///         resolved by the code prefix (the code without its level suffix) and the level.
    ///     </para>
    /// </summary>
    private static RefSkill FindNextSkill(RefSkill currentSkill)
    {
        if (currentSkill == null ||
            currentSkill.Basic_Level == byte.MaxValue ||
            string.IsNullOrEmpty(currentSkill.Basic_Code))
            return null;

        EnsureSkillIndex();

        var key = (GetCodePrefix(currentSkill.Basic_Code), (byte)(currentSkill.Basic_Level + 1));

        return _skillIndex != null && _skillIndex.TryGetValue(key, out var nextSkill)
            ? nextSkill
            : null;
    }

    private static void EnsureSkillIndex()
    {
        if (_skillIndex != null)
            return;

        lock (_indexLock)
        {
            if (_skillIndex != null)
                return;

            var index = new Dictionary<(string Prefix, byte Level), RefSkill>();

            foreach (var skill in Game.ReferenceManager.SkillData.Values)
            {
                if (skill == null ||
                    skill.ReqLearn_SP <= 0 ||
                    string.IsNullOrEmpty(skill.Basic_Code))
                    continue;

                // Hidden companion skills have no learn cost and are learned with their base skill.
                var key = (GetCodePrefix(skill.Basic_Code), skill.Basic_Level);
                if (!index.ContainsKey(key))
                    index[key] = skill;
            }

            _skillIndex = index;
        }
    }

    private static string GetCodePrefix(string code)
    {
        if (string.IsNullOrEmpty(code))
            return string.Empty;

        var separatorIndex = code.LastIndexOf('_');
        return separatorIndex > 0 ? code.Substring(0, separatorIndex) : code;
    }

    private static bool CanLearnSkill(RefSkill skill, Player player)
    {
        if (skill == null || player?.Skills == null)
            return false;

        // Hidden companion skills (chain code targets) are learned together with their base skill.
        if (skill.ReqLearn_SP <= 0)
            return false;

        if (skill.Basic_Level > player.Level)
            return false;

        if (skill.ReqLearn_Race != 0 &&
            player.Race != ObjectCountry.Unassigned &&
            (byte)player.Race != skill.ReqLearn_Race)
            return false;

        if (!CheckMasteryRequirement(skill.ReqCommon_Mastery1, skill.ReqCommon_MasteryLevel1, player))
            return false;

        if (!CheckMasteryRequirement(skill.ReqCommon_Mastery2, skill.ReqCommon_MasteryLevel2, player))
            return false;

        if (skill.ReqLearn_SP > 0 && (uint)skill.ReqLearn_SP > player.SkillPoints)
            return false;

        if (!CheckRequiredSkill(skill, skill.ReqLearn_Skill1, skill.ReqLearn_SkillLevel1, player))
            return false;

        if (!CheckRequiredSkill(skill, skill.ReqLearn_Skill2, skill.ReqLearn_SkillLevel2, player))
            return false;

        if (!CheckRequiredSkill(skill, skill.ReqLearn_Skill3, skill.ReqLearn_SkillLevel3, player))
            return false;

        return true;
    }

    private static bool CheckMasteryRequirement(int masteryId, byte requiredLevel, Player player)
    {
        // 1000 is used by the client for "no mastery requirement".
        if (masteryId == 0 || masteryId == 1000)
            return true;

        var mastery = player.Skills.GetMasteryInfoById((uint)masteryId);
        return mastery != null && mastery.Level >= requiredLevel;
    }

    private static bool CheckRequiredSkill(RefSkill skill, int requiredSkillId, byte requiredLevel, Player player)
    {
        if (requiredSkillId <= 0 || requiredLevel == 0)
            return true;

        var requiredSkill = Game.ReferenceManager.GetRefSkill((uint)requiredSkillId);
        if (requiredSkill == null || string.IsNullOrEmpty(requiredSkill.Basic_Group))
            return true;

        // Only enforce the prerequisite when it belongs to the same skill line. Some
        // custom server files contain unrelated entries in these columns.
        if (!string.Equals(requiredSkill.Basic_Group, skill.Basic_Group, StringComparison.OrdinalIgnoreCase))
            return true;

        var knownLevel = player.Skills.KnownSkills
            .Where(p => p.Record != null &&
                        string.Equals(p.Record.Basic_Group, requiredSkill.Basic_Group,
                            StringComparison.OrdinalIgnoreCase))
            .Select(p => (int)p.Record.Basic_Level)
            .DefaultIfEmpty(0)
            .Max();

        return knownLevel == 0 || knownLevel >= requiredLevel;
    }

    private static bool TryEnterAction()
    {
        return Interlocked.CompareExchange(ref _actionActive, 1, 0) == 0;
    }

    private static void ExitAction()
    {
        Interlocked.Exchange(ref _actionActive, 0);

        // A manual action was requested while another action was running.
        if (!_manualRequests.IsEmpty)
        {
            TryProcessManualRequests();
            return;
        }

        if (!_pendingRun)
            return;

        _pendingRun = false;
        RunAutoUpgrade();
    }

    private readonly struct MasteryRequest
    {
        public MasteryRequest(uint masteryId, bool maxLevel)
        {
            MasteryId = masteryId;
            MaxLevel = maxLevel;
        }

        public uint MasteryId { get; }

        public bool MaxLevel { get; }
    }
}
