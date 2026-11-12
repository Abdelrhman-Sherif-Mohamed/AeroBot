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
using Action = System.Action;

namespace RSBot.Skills.Components;

/// <summary>
///     Represents a single deduplicated skill progression line (level 1..max) belonging to a mastery.
/// </summary>
internal sealed class SkillLineInfo
{
    public SkillLineInfo(string codePrefix, uint masteryId, IReadOnlyList<RefSkill> levels)
    {
        CodePrefix = codePrefix;
        MasteryId = masteryId;
        Levels = levels;
        FirstSkill = levels[0];
        MaxSkill = levels[levels.Count - 1];
    }

    public string CodePrefix { get; }
    public uint MasteryId { get; }
    public IReadOnlyList<RefSkill> Levels { get; }
    public RefSkill FirstSkill { get; }
    public RefSkill MaxSkill { get; }

    public byte UIPage => FirstSkill.UI_SkillPage;
    public byte UIColumn => FirstSkill.UI_SkillColumn;
    public byte UIRow => FirstSkill.UI_SkillRow;
    public string BasicGroup => FirstSkill.Basic_Group;
    public string DisplayName => FirstSkill.GetRealName();
}

/// <summary>
///     Handles manual and automatic upgrades of masteries and skills.
/// </summary>
internal static class SkillUpgradeManager
{
    private const string AutoMasteryConfigKey = "RSBot.Skills.AutoMasteryEnabled";
    private const string AutoSkillsConfigKey = "RSBot.Skills.AutoSkillsEnabled";
    private const string AutoWhenBotStoppedConfigKey = "RSBot.Skills.AutoWhenBotStopped";
    private const string AutoMasteryGapConfigKey = "RSBot.Skills.AutoMasteryGap";

    private const int LearnDelay = 250;
    private const int MaxActionsPerRun = 250;

    private static readonly ConcurrentDictionary<uint, byte> _autoMasteries = new();
    private static readonly ConcurrentDictionary<string, byte> _autoSkills = new(StringComparer.OrdinalIgnoreCase);
    private static readonly ConcurrentDictionary<uint, byte> _blockedSkills = new();
    private static readonly ConcurrentQueue<UpgradeRequest> _manualRequests = new();
    private static readonly object _stateLock = new();
    private static readonly object _indexLock = new();

    private static Dictionary<(string Prefix, byte Level), RefSkill> _skillIndex;
    private static Dictionary<string, List<RefSkill>> _skillLinesByPrefix;

    private static int _actionActive;
    private static int _masteryCursor;
    private static volatile bool _pendingRun;
    private static bool _settingsLoaded;
    private static bool _autoWhenBotStopped = true;
    private static byte _autoMasteryGap;

    public static event Action BatchCompleted;

    public static bool AutoWhenBotStopped
    {
        get
        {
            EnsureSettingsLoaded();
            return _autoWhenBotStopped;
        }
        set
        {
            EnsureSettingsLoaded();
            _autoWhenBotStopped = value;
            SaveSettings();
            if (value)
                RunAutoUpgrade();
        }
    }

    public static byte AutoMasteryGap
    {
        get
        {
            EnsureSettingsLoaded();
            return _autoMasteryGap;
        }
        set
        {
            EnsureSettingsLoaded();
            _autoMasteryGap = value;
            SaveSettings();
            RunAutoUpgrade();
        }
    }

    public static bool IsAutoEnabled(uint masteryId)
    {
        EnsureSettingsLoaded();
        return _autoMasteries.ContainsKey(masteryId);
    }

    public static bool IsSkillAutoEnabled(string skillPrefix)
    {
        EnsureSettingsLoaded();
        return !string.IsNullOrEmpty(skillPrefix) && _autoSkills.ContainsKey(skillPrefix);
    }

    public static bool IsEffectiveSkillAutoEnabled(string skillPrefix, uint masteryId)
    {
        return IsAutoEnabled(masteryId) || IsSkillAutoEnabled(skillPrefix);
    }

    public static void LoadSettings()
    {
        _autoMasteries.Clear();
        _autoSkills.Clear();
        _blockedSkills.Clear();

        lock (_indexLock)
        {
            _skillIndex = null;
            _skillLinesByPrefix = null;
        }

        foreach (var masteryId in PlayerConfig.GetArray<uint>(AutoMasteryConfigKey))
        {
            if (masteryId != 0)
                _autoMasteries[masteryId] = 0;
        }

        foreach (var prefix in PlayerConfig.GetArray<string>(AutoSkillsConfigKey))
        {
            if (!string.IsNullOrWhiteSpace(prefix))
                _autoSkills[prefix.Trim()] = 0;
        }

        _autoWhenBotStopped = PlayerConfig.Get(AutoWhenBotStoppedConfigKey, true);
        _autoMasteryGap = PlayerConfig.Get<byte>(AutoMasteryGapConfigKey, 0);

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

    public static void SetSkillAutoEnabled(string skillPrefix, bool enabled)
    {
        EnsureSettingsLoaded();
        if (string.IsNullOrWhiteSpace(skillPrefix))
            return;

        if (enabled)
            _autoSkills[skillPrefix] = 0;
        else
            _autoSkills.TryRemove(skillPrefix, out _);

        SaveSettings();

        if (enabled)
            RunAutoUpgrade();
    }

    public static void ToggleSkillAuto(string skillPrefix)
    {
        SetSkillAutoEnabled(skillPrefix, !IsSkillAutoEnabled(skillPrefix));
    }

    public static void SetSkillsAutoEnabled(IEnumerable<string> skillPrefixes, bool enabled)
    {
        EnsureSettingsLoaded();
        if (skillPrefixes == null)
            return;

        foreach (var prefix in skillPrefixes)
        {
            if (string.IsNullOrWhiteSpace(prefix))
                continue;

            if (enabled)
                _autoSkills[prefix] = 0;
            else
                _autoSkills.TryRemove(prefix, out _);
        }

        SaveSettings();

        if (enabled)
            RunAutoUpgrade();
    }

    public static void SaveSettings()
    {
        PlayerConfig.SetArray(AutoMasteryConfigKey, _autoMasteries.Keys.OrderBy(p => p).ToArray());
        PlayerConfig.SetArray(AutoSkillsConfigKey, _autoSkills.Keys.OrderBy(p => p, StringComparer.OrdinalIgnoreCase).ToArray());
        PlayerConfig.Set(AutoWhenBotStoppedConfigKey, _autoWhenBotStopped);
        PlayerConfig.Set(AutoMasteryGapConfigKey, _autoMasteryGap);
        PlayerConfig.Save();
    }

    public static void ResetBlockedSkills()
    {
        _blockedSkills.Clear();
    }

    /// <summary>
    ///     Returns deduplicated skill progression lines for the specified mastery.
    /// </summary>
    public static IReadOnlyList<SkillLineInfo> GetSkillLinesForMastery(uint masteryId, Player player = null)
    {
        EnsureSkillIndex();

        var race = player?.Race ?? Game.Player?.Race ?? ObjectCountry.Unassigned;
        var result = new List<SkillLineInfo>();

        if (_skillLinesByPrefix == null)
            return result;

        foreach (var kvp in _skillLinesByPrefix)
        {
            var levels = kvp.Value;
            if (levels == null || levels.Count == 0)
                continue;

            var first = levels[0];
            if (first.ReqCommon_Mastery1 != (int)masteryId &&
                first.ReqCommon_Mastery2 != (int)masteryId)
                continue;

            if (race != ObjectCountry.Unassigned &&
                first.ReqLearn_Race != 0 &&
                first.ReqLearn_Race != (byte)race)
                continue;

            result.Add(new SkillLineInfo(kvp.Key, masteryId, levels));
        }

        return result
            .OrderBy(s => s.UIPage == 0 ? byte.MaxValue : s.UIPage)
            .ThenBy(s => s.UIRow)
            .ThenBy(s => s.UIColumn)
            .ThenBy(s => s.FirstSkill.ReqCommon_MasteryLevel1)
            .ThenBy(s => s.FirstSkill.ID)
            .ToList();
    }

    /// <summary>
    ///     Gets the known skill level (and RefSkill) for a given code prefix, or (0, null) if not learned.
    /// </summary>
    public static (byte Level, RefSkill KnownRecord) GetKnownSkillState(string codePrefix, Player player)
    {
        if (player?.Skills?.KnownSkills == null || string.IsNullOrEmpty(codePrefix))
            return (0, null);

        RefSkill bestRecord = null;
        byte bestLevel = 0;

        foreach (var known in player.Skills.KnownSkills)
        {
            var record = known?.Record;
            if (record == null || string.IsNullOrEmpty(record.Basic_Code))
                continue;

            if (!string.Equals(GetCodePrefix(record.Basic_Code), codePrefix, StringComparison.OrdinalIgnoreCase))
                continue;

            if (record.Basic_Level > bestLevel)
            {
                bestLevel = record.Basic_Level;
                bestRecord = record;
            }
        }

        return (bestLevel, bestRecord);
    }

    /// <summary>
    ///     Gets the next RefSkill level in a skill line, or null if already at max level.
    /// </summary>
    public static RefSkill GetNextLevelForLine(SkillLineInfo line, Player player)
    {
        if (line == null || line.Levels.Count == 0)
            return null;

        var (knownLevel, knownRecord) = GetKnownSkillState(line.CodePrefix, player);
        if (knownRecord == null)
            return line.Levels[0];

        return FindNextSkill(knownRecord)
               ?? line.Levels.FirstOrDefault(l => l.Basic_Level > knownLevel);
    }

    public static bool CanLearnNextLevel(RefSkill skill, Player player)
    {
        return CanLearnSkill(skill, player);
    }

    /// <summary>
    ///     Levels the given mastery up once, or until it reaches the player level.
    /// </summary>
    public static void LearnMastery(uint masteryId, bool maxLevel)
    {
        EnsureSettingsLoaded();

        _manualRequests.Enqueue(UpgradeRequest.ForMastery(masteryId, maxLevel));
        TryProcessManualRequests();
    }

    /// <summary>
    ///     Opens or upgrades a single skill line once or up to the highest currently learnable level.
    /// </summary>
    public static void LearnSkillLine(uint masteryId, string skillPrefix, bool maxLevel)
    {
        EnsureSettingsLoaded();
        if (string.IsNullOrWhiteSpace(skillPrefix))
            return;

        _manualRequests.Enqueue(UpgradeRequest.ForSkill(masteryId, skillPrefix, maxLevel));
        TryProcessManualRequests();
    }

    /// <summary>
    ///     Opens or upgrades all eligible skills in a subset (group) or the whole mastery.
    /// </summary>
    public static void LearnSkillsBatch(uint masteryId, IEnumerable<string> skillPrefixesOrNull, bool maxLevel)
    {
        EnsureSettingsLoaded();

        var prefixes = skillPrefixesOrNull?
            .Where(p => !string.IsNullOrWhiteSpace(p))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();

        _manualRequests.Enqueue(UpgradeRequest.ForBatch(masteryId, prefixes, maxLevel));
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
                ResetBlockedSkills();

                while (_manualRequests.TryDequeue(out var request))
                {
                    switch (request.Kind)
                    {
                        case RequestKind.Mastery:
                            ExecuteManualMastery(request.MasteryId, request.MaxLevel);
                            break;
                        case RequestKind.SingleSkill:
                            ExecuteManualSingleSkill(request.MasteryId, request.SkillPrefix, request.MaxLevel);
                            break;
                        case RequestKind.BatchSkills:
                            ExecuteManualBatchSkills(request.MasteryId, request.SkillPrefixes, request.MaxLevel);
                            break;
                    }
                }
            }
            finally
            {
                try
                {
                    BatchCompleted?.Invoke();
                }
                catch
                {
                }

                ExitAction();
            }
        });
    }

    private static void ExecuteManualMastery(uint masteryId, bool maxLevel)
    {
        var player = Game.Player;
        if (player?.Skills == null)
            return;

        var mastery = player.Skills.GetMasteryInfoById(masteryId);
        if (mastery == null)
            return;

        var actions = 0;
        do
        {
            if (!TryLearnMastery(mastery, true, 0))
                break;

            actions++;
            if (!maxLevel || actions >= MaxActionsPerRun)
                break;

            Thread.Sleep(LearnDelay);
            player = Game.Player;
            mastery = player?.Skills?.GetMasteryInfoById(masteryId);
        } while (mastery != null && player != null && mastery.Level < player.Level);
    }

    private static void ExecuteManualSingleSkill(uint masteryId, string skillPrefix, bool maxLevel)
    {
        var actions = 0;
        do
        {
            var player = Game.Player;
            if (player?.Skills == null)
                break;

            if (!TryLearnNextSkillForPrefix(skillPrefix, masteryId, player, true))
                break;

            actions++;
            if (!maxLevel || actions >= MaxActionsPerRun)
                break;

            Thread.Sleep(LearnDelay);
        } while (true);
    }

    private static void ExecuteManualBatchSkills(uint masteryId, string[] skillPrefixesOrNull, bool maxLevel)
    {
        var player = Game.Player;
        if (player?.Skills == null)
            return;

        HashSet<string> allowedPrefixes = null;
        if (skillPrefixesOrNull != null && skillPrefixesOrNull.Length > 0)
            allowedPrefixes = new HashSet<string>(skillPrefixesOrNull, StringComparer.OrdinalIgnoreCase);

        if (!maxLevel)
        {
            // One pass: open or add +1 level to every eligible skill in the selection.
            var lines = GetSkillLinesForMastery(masteryId, player);
            foreach (var line in lines)
            {
                if (allowedPrefixes != null && !allowedPrefixes.Contains(line.CodePrefix))
                    continue;

                var currentPlayer = Game.Player;
                if (currentPlayer?.Skills == null)
                    break;

                if (TryLearnNextSkillForPrefix(line.CodePrefix, masteryId, currentPlayer, true))
                    Thread.Sleep(LearnDelay);
            }

            return;
        }

        // Max mode: keep upgrading eligible skills until nothing more can be learned.
        for (var action = 0; action < MaxActionsPerRun; action++)
        {
            var currentPlayer = Game.Player;
            if (currentPlayer?.Skills == null)
                break;

            if (!TryLearnNextSkill(masteryId, allowedPrefixes))
                break;

            Thread.Sleep(LearnDelay);
        }
    }

    /// <summary>
    ///     Runs the auto upgrade loop for every enabled mastery and individually enabled auto-skill.
    /// </summary>
    public static void RunAutoUpgrade()
    {
        EnsureSettingsLoaded();

        if (_autoMasteries.IsEmpty && _autoSkills.IsEmpty)
            return;

        if (!_autoWhenBotStopped && !Kernel.Bot.Running)
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

            if (!_autoWhenBotStopped && !Kernel.Bot.Running)
                return;

            var enabledMasteries = _autoMasteries.Keys.ToArray();
            var enabledSkills = _autoSkills.Keys.ToArray();
            if (enabledMasteries.Length == 0 && enabledSkills.Length == 0)
                return;

            var didSomething = false;

            if (enabledMasteries.Length > 0)
            {
                for (var index = 0; index < enabledMasteries.Length; index++)
                {
                    var masteryId = enabledMasteries[(_masteryCursor + index) % enabledMasteries.Length];
                    if (!IsAutoEnabled(masteryId))
                        continue;

                    var mastery = player.Skills.GetMasteryInfoById(masteryId);
                    if (mastery == null)
                        continue;

                    if (mastery.Level + _autoMasteryGap < player.Level &&
                        TryLearnMastery(mastery, true, _autoMasteryGap))
                    {
                        _masteryCursor = (_masteryCursor + index + 1) % enabledMasteries.Length;
                        didSomething = true;
                        break;
                    }

                    if (TryLearnNextSkill(masteryId, null))
                    {
                        _masteryCursor = (_masteryCursor + index + 1) % enabledMasteries.Length;
                        didSomething = true;
                        break;
                    }
                }
            }

            if (!didSomething && enabledSkills.Length > 0)
            {
                foreach (var prefix in enabledSkills)
                {
                    if (!IsSkillAutoEnabled(prefix))
                        continue;

                    if (TryLearnNextSkillForPrefix(prefix, 0, player, true))
                    {
                        didSomething = true;
                        break;
                    }
                }
            }

            if (!didSomething)
                return;

            Thread.Sleep(LearnDelay);
        }
    }

    private static bool TryLearnMastery(MasteryInfo mastery, bool log, byte gap)
    {
        var player = Game.Player;
        if (player == null || mastery == null || mastery.Level + gap >= player.Level)
            return false;

        var nextLevel = Game.ReferenceManager.GetRefLevel((byte)(mastery.Level + 1));
        if (nextLevel == null)
            return false;

        if (nextLevel.Exp_M > 0 && (uint)nextLevel.Exp_M > player.SkillPoints)
        {
            if (log)
                Log.Debug(
                    $"Mastery upgrade cancelled due to insufficient skill points. Required: {nextLevel.Exp_M}");

            return false;
        }

        if (log)
            Log.Notify($"Train mastery [{mastery.Record?.Name}] to lv. {mastery.Level + 1}");

        var previousLevel = mastery.Level;

        if (!LearnMasteryHandler.LearnMastery(mastery.Id))
            return false;

        var updated = Game.Player?.Skills?.GetMasteryInfoById(mastery.Id);
        return updated != null && updated.Level > previousLevel;
    }

    private static bool TryLearnNextSkillForPrefix(string skillPrefix, uint masteryIdFilter, Player player, bool manualLog)
    {
        if (player?.Skills == null || string.IsNullOrEmpty(skillPrefix))
            return false;

        EnsureSkillIndex();

        var (_, knownRecord) = GetKnownSkillState(skillPrefix, player);
        RefSkill nextSkill = null;

        if (knownRecord != null)
        {
            nextSkill = FindNextSkill(knownRecord);
        }
        else if (_skillLinesByPrefix != null && _skillLinesByPrefix.TryGetValue(skillPrefix, out var levels) && levels.Count > 0)
        {
            nextSkill = levels[0];
        }

        if (nextSkill == null)
            return false;

        if (masteryIdFilter != 0 &&
            nextSkill.ReqCommon_Mastery1 != (int)masteryIdFilter &&
            nextSkill.ReqCommon_Mastery2 != (int)masteryIdFilter)
            return false;

        if (_blockedSkills.ContainsKey(nextSkill.ID))
            return false;

        if (player.Skills.HasSkill(nextSkill.ID))
            return false;

        if (!CanLearnSkill(nextSkill, player))
            return false;

        var success = LearnSkillHandler.LearnSkill(nextSkill.ID);
        var learned = Game.Player?.Skills?.HasSkill(nextSkill.ID) == true;

        if (success && learned)
        {
            if (manualLog)
                Log.Notify($"Learned skill [{nextSkill.GetRealName()}] lv. {nextSkill.Basic_Level}");
            return true;
        }

        _blockedSkills[nextSkill.ID] = 0;
        if (manualLog)
            Log.Debug($"Skill upgrade failed for [{nextSkill.GetRealName()}] lv. {nextSkill.Basic_Level}");

        return false;
    }

    private static bool TryLearnNextSkill(uint masteryId, HashSet<string> allowedPrefixesOrNull)
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

            if (allowedPrefixesOrNull != null &&
                !allowedPrefixesOrNull.Contains(GetCodePrefix(record.Basic_Code)))
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

            if (allowedPrefixesOrNull != null &&
                !allowedPrefixesOrNull.Contains(GetCodePrefix(skill.Basic_Code)))
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
                Log.Notify($"Learned skill [{nextSkill.GetRealName()}] lv. {nextSkill.Basic_Level}");
                return true;
            }

            _blockedSkills[nextSkill.ID] = 0;
            Log.Debug($"Skill upgrade failed for [{nextSkill.GetRealName()}] lv. {nextSkill.Basic_Level}");
        }

        return false;
    }

    /// <summary>
    ///     Finds the skill of the next level in the same skill line.
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
        if (_skillIndex != null && _skillLinesByPrefix != null)
            return;

        lock (_indexLock)
        {
            if (_skillIndex != null && _skillLinesByPrefix != null)
                return;

            var index = new Dictionary<(string Prefix, byte Level), RefSkill>();
            var lines = new Dictionary<string, List<RefSkill>>(StringComparer.OrdinalIgnoreCase);

            foreach (var skill in Game.ReferenceManager.SkillData.Values)
            {
                if (skill == null ||
                    skill.ReqLearn_SP <= 0 ||
                    string.IsNullOrEmpty(skill.Basic_Code))
                    continue;

                var prefix = GetCodePrefix(skill.Basic_Code);
                var key = (prefix, skill.Basic_Level);
                if (index.ContainsKey(key))
                    continue;

                index[key] = skill;

                if (!lines.TryGetValue(prefix, out var list))
                {
                    list = new List<RefSkill>();
                    lines[prefix] = list;
                }

                list.Add(skill);
            }

            foreach (var list in lines.Values)
                list.Sort((a, b) =>
                {
                    var cmp = a.Basic_Level.CompareTo(b.Basic_Level);
                    return cmp != 0 ? cmp : a.ID.CompareTo(b.ID);
                });

            _skillIndex = index;
            _skillLinesByPrefix = lines;
        }
    }

    public static string GetCodePrefix(string code)
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

    private enum RequestKind
    {
        Mastery,
        SingleSkill,
        BatchSkills
    }

    private readonly struct UpgradeRequest
    {
        private UpgradeRequest(RequestKind kind, uint masteryId, string skillPrefix, string[] skillPrefixes, bool maxLevel)
        {
            Kind = kind;
            MasteryId = masteryId;
            SkillPrefix = skillPrefix;
            SkillPrefixes = skillPrefixes;
            MaxLevel = maxLevel;
        }

        public RequestKind Kind { get; }
        public uint MasteryId { get; }
        public string SkillPrefix { get; }
        public string[] SkillPrefixes { get; }
        public bool MaxLevel { get; }

        public static UpgradeRequest ForMastery(uint masteryId, bool maxLevel)
        {
            return new UpgradeRequest(RequestKind.Mastery, masteryId, null, null, maxLevel);
        }

        public static UpgradeRequest ForSkill(uint masteryId, string skillPrefix, bool maxLevel)
        {
            return new UpgradeRequest(RequestKind.SingleSkill, masteryId, skillPrefix, null, maxLevel);
        }

        public static UpgradeRequest ForBatch(uint masteryId, string[] skillPrefixes, bool maxLevel)
        {
            return new UpgradeRequest(RequestKind.BatchSkills, masteryId, null, skillPrefixes, maxLevel);
        }
    }
}
