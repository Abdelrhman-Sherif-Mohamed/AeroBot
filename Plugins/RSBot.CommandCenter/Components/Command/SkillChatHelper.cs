using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RSBot.Core;
using RSBot.Core.Client.ReferenceObjects;
using RSBot.Core.Components;
using RSBot.Core.Objects;
using RSBot.Core.Objects.Skill;

namespace RSBot.CommandCenter.Components.Command;

/// <summary>
///     Shared logic for the in-game skill chat commands (!addskill, !addbuff,
///     !delskill, !delbuff, !skills). Skills are matched by abbreviation
///     (first letter of each word, e.g. "fb" for "Flame Body") or by any
///     part of the name (e.g. "fire"), so the full name is never required.
/// </summary>
internal static class SkillChatHelper
{
    public static string Normalize(string value)
    {
        if (string.IsNullOrEmpty(value))
            return string.Empty;

        var sb = new StringBuilder(value.Length);
        foreach (var c in value.ToLowerInvariant())
            if (char.IsLetterOrDigit(c))
                sb.Append(c);

        return sb.ToString();
    }

    /// <summary>
    ///     Splits chat args into text filter + numbers, tolerating row/slot
    ///     prefixes ("1", "R1", "r1", "s2", "1,") so "!list fire R1 2",
    ///     "!list fire 1 2" and "!pick r1 s2" all work.
    /// </summary>
    public static (string filter, List<int> numbers) SplitFilterAndNumbers(string[] args)
    {
        var words = new List<string>();
        var numbers = new List<int>();

        foreach (var raw in args)
        {
            var t = raw.Trim().Trim(',', ':', ';', '.');
            if (int.TryParse(t, out var n) && n > 0)
            {
                numbers.Add(n);
                continue;
            }

            // Row/slot style: R1, r1, S2, s2, Row1, Slot2
            var lower = t.ToLowerInvariant();
            string digits = null;
            if ((lower.StartsWith("r") || lower.StartsWith("s")) && lower.Length > 1)
                digits = lower.Substring(1);
            else if (lower.StartsWith("row") && lower.Length > 3)
                digits = lower.Substring(3);
            else if (lower.StartsWith("slot") && lower.Length > 4)
                digits = lower.Substring(4);

            if (digits != null && int.TryParse(digits, out var m) && m > 0)
            {
                numbers.Add(m);
                continue;
            }

            words.Add(raw);
        }

        return (string.Join(" ", words), numbers);
    }

    public static string Initials(string name)
    {
        if (string.IsNullOrEmpty(name))
            return string.Empty;

        var sb = new StringBuilder();
        var takeNext = true;
        foreach (var c in name.ToLowerInvariant())
        {
            if (char.IsLetterOrDigit(c))
            {
                if (takeNext)
                {
                    sb.Append(c);
                    takeNext = false;
                }
            }
            else
            {
                takeNext = true;
            }
        }

        return sb.ToString();
    }

    private static IEnumerable<SkillInfo> KnownAttackSkills()
    {
        if (Game.Player == null)
            return Enumerable.Empty<SkillInfo>();

        // Same filter as the Skills window "add attack" menu
        return Game.Player.Skills.KnownSkills
            .Where(s => s is { Enabled: true, IsPassive: false, IsImbue: false } &&
                        (s.IsAttack || s.Record.TargetGroup_Enemy_M));
    }

    private static IEnumerable<SkillInfo> KnownBuffSkills()
    {
        if (Game.Player == null)
            return Enumerable.Empty<SkillInfo>();

        var result = Game.Player.Skills.KnownSkills
            .Where(s => s is { Enabled: true, IsPassive: false, IsImbue: false } &&
                        !s.IsAttack && !s.Record.TargetGroup_Enemy_M)
            .ToList();

        // Ability skills (e.g. vigor, riding) can be buffs too, like in the Skills window
        if (Game.Player.TryGetAbilitySkills(out var abilitySkills))
            foreach (var ability in abilitySkills)
                if (!ability.IsPassive && result.All(s => s.Id != ability.Id))
                    result.Add(ability);

        return result;
    }

    /// <summary>
    ///     Finds known skills matching the typed text. Deduplicates by name,
    ///     keeping the highest level of each skill.
    /// </summary>
    public static List<SkillInfo> FindMatches(string input, bool wantAttack)
    {
        var result = new List<SkillInfo>();
        if (Game.Player == null || string.IsNullOrWhiteSpace(input))
            return result;

        var source = (wantAttack ? KnownAttackSkills() : KnownBuffSkills())
            .GroupBy(s => Normalize(s.Record.GetRealName()))
            .Select(g => g.OrderByDescending(s => s.Record.Basic_Level).First())
            .ToList();

        var trimmed = input.Trim();

        // Direct skill id (power users)
        if (uint.TryParse(trimmed, out var skillId))
        {
            var byId = Game.Player.Skills.GetSkillInfoById(skillId) ??
                       source.FirstOrDefault(s => s.Id == skillId);
            if (byId != null)
                result.Add(byId);

            return result;
        }

        var needle = Normalize(trimmed);
        if (needle.Length == 0)
            return result;

        var scored = new List<(SkillInfo skill, int score)>();
        foreach (var skill in source)
        {
            var name = Normalize(skill.Record.GetRealName());
            var initials = Initials(skill.Record.GetRealName());

            if (needle == initials || needle == name)
                scored.Add((skill, 0));
            else if (name.StartsWith(needle, StringComparison.Ordinal))
                scored.Add((skill, 1));
            else if (name.Contains(needle, StringComparison.Ordinal))
                scored.Add((skill, 2));
            else if (initials.StartsWith(needle, StringComparison.Ordinal))
                scored.Add((skill, 3));
        }

        return scored
            .OrderBy(s => s.score)
            .ThenByDescending(s => s.skill.Record.Basic_Level)
            .Select(s => s.skill)
            .Take(8)
            .ToList();
    }

    public static string Describe(SkillInfo skill)
    {
        return $"{skill.Record.GetRealName()} (lv.{skill.Record.Basic_Level})";
    }

    /// <summary>Short form for numbered lists: "Bloody Wolf Storm(lv1)".</summary>
    public static string DescribeShort(SkillInfo skill)
    {
        return $"{skill.Record.GetRealName()}(lv{skill.Record.Basic_Level})";
    }

    /// <summary>Full deduplicated attack pool (highest level per name).</summary>
    public static List<SkillInfo> GetAttackPool()
    {
        return KnownAttackSkills()
            .GroupBy(s => Normalize(s.Record.GetRealName()))
            .Select(g => g.OrderByDescending(s => s.Record.Basic_Level).First())
            .OrderBy(s => s.Record.GetRealName())
            .ToList();
    }

    /// <summary>Full deduplicated buff pool (highest level per name).</summary>
    public static List<SkillInfo> GetBuffPool()
    {
        return KnownBuffSkills()
            .GroupBy(s => Normalize(s.Record.GetRealName()))
            .Select(g => g.OrderByDescending(s => s.Record.Basic_Level).First())
            .OrderBy(s => s.Record.GetRealName())
            .ToList();
    }

    /// <summary>Mastery (tab) name of a skill, e.g. "Heuksal".</summary>
    public static string MasteryLabel(SkillInfo skill)
    {
        try
        {
            var mastery = Game.Player?.Skills.Masteries
                .FirstOrDefault(m => m.Id == skill.Record.ReqCommon_Mastery1);
            if (mastery?.Record == null)
                return "?";

            var translated = Game.ReferenceManager.GetTranslation(mastery.Record.NameCode);
            if (!string.IsNullOrWhiteSpace(translated))
                return translated;

            return mastery.Record.NameCode;
        }
        catch
        {
            return "?";
        }
    }

    /// <summary>
    ///     Pool filtered by mastery/tab text (e.g. "heuk" matches Heuksal).
    ///     Empty filter returns the whole pool.
    /// </summary>
    public static (string title, List<SkillInfo> items) ListByMastery(string filter, bool wantAttack)
    {
        var pool = wantAttack ? GetAttackPool() : GetBuffPool();

        if (string.IsNullOrWhiteSpace(filter))
            return (wantAttack ? "Attacks" : "Buffs", pool);

        var needle = Normalize(filter);
        var filtered = pool.Where(s =>
                Normalize(MasteryLabel(s)).Contains(needle, StringComparison.Ordinal))
            .ToList();

        if (filtered.Count == 0)
            return (string.Empty, filtered);

        return (MasteryLabel(filtered[0]), filtered);
    }

    public static bool AddAttack(SkillInfo skill)
    {
        const string key = "RSBot.Skills.Attacks_0"; // General group

        var saved = PlayerConfig.GetArray<uint>(key).ToList();
        if (saved.Contains(skill.Id))
            return false;

        // Same overlap rule as the Skills window
        foreach (var collection in SkillManager.Skills.Values)
            if (collection.Any(p => p.Record.Action_Overlap != 0 &&
                                    p.Record.Action_Overlap == skill.Record.Action_Overlap))
                return false;

        saved.Add(skill.Id);
        PlayerConfig.SetArray(key, saved.ToArray());
        PlayerConfig.Save();

        var info = Game.Player.Skills.GetSkillInfoById(skill.Id) ?? skill;
        SkillManager.Skills[MonsterRarity.General].Add(info);

        return true;
    }

    public static bool AddBuff(SkillInfo skill)
    {
        const string key = "RSBot.Skills.Buffs";

        var saved = PlayerConfig.GetArray<uint>(key).ToList();
        if (saved.Contains(skill.Id))
            return false;

        if (SkillManager.Buffs.Any(p => p.Record.Action_Overlap != 0 &&
                                        p.Record.Action_Overlap == skill.Record.Action_Overlap))
            return false;

        saved.Add(skill.Id);
        PlayerConfig.SetArray(key, saved.ToArray());
        PlayerConfig.Save();

        Game.Player.TryGetAbilitySkills(out var abilitySkills);
        var info = Game.Player.Skills.GetSkillInfoById(skill.Id) ??
                   abilitySkills?.FirstOrDefault(p => p.Id == skill.Id) ?? skill;
        SkillManager.Buffs.Add(info);

        return true;
    }

    public static bool RemoveAttack(SkillInfo skill)
    {
        var removed = false;

        for (var i = 0; i < 9; i++)
        {
            var key = "RSBot.Skills.Attacks_" + i;
            var saved = PlayerConfig.GetArray<uint>(key).ToList();
            if (saved.Remove(skill.Id))
            {
                PlayerConfig.SetArray(key, saved.ToArray());
                removed = true;
            }
        }

        foreach (var collection in SkillManager.Skills.Values)
            collection.RemoveAll(p => p.Id == skill.Id);

        if (removed)
            PlayerConfig.Save();

        return removed;
    }

    public static bool RemoveBuff(SkillInfo skill)
    {
        var saved = PlayerConfig.GetArray<uint>("RSBot.Skills.Buffs").ToList();
        if (!saved.Remove(skill.Id))
            return false;

        PlayerConfig.SetArray("RSBot.Skills.Buffs", saved.ToArray());
        PlayerConfig.Save();

        SkillManager.Buffs.RemoveAll(p => p.Id == skill.Id);

        return true;
    }

    public static List<SkillInfo> CurrentAttacks()
    {
        return SkillManager.Skills.TryGetValue(MonsterRarity.General, out var list)
            ? list.ToList()
            : new List<SkillInfo>();
    }

    public static List<SkillInfo> CurrentBuffs()
    {
        return SkillManager.Buffs.ToList();
    }

    // ---------- Grid (row/slot like the in-game Skill window) ----------

    public sealed record GridCell(string Name, SkillInfo Learned, uint RefId)
    {
        public bool IsLearned => Learned != null;
    }

    public sealed record GridRow(byte ReqMasteryLevel, List<GridCell> Cells);

    /// <summary>Cell text: plain name when learned, "~Name" when not learned yet.</summary>
    public static string CellText(GridCell cell)
    {
        return cell.IsLearned ? cell.Name : "~" + cell.Name;
    }

    /// <summary>One chat row: "Fire R1: 1)Fire Bolt 2)~Fire Wall".</summary>
    public static string GridRowText(string title, int rowOneBased, GridRow row)
    {
        var parts = new List<string>();
        for (var s = 0; s < row.Cells.Count; s++)
            parts.Add($"{s + 1})" + CellText(row.Cells[s]));

        return $"[Sonic] {title} R{rowOneBased}: " + string.Join(" ", parts);
    }

    public static bool RefIsPassive(RefSkill r)
    {
        return r.Basic_Activity == 0;
    }

    public static bool RefIsAttack(RefSkill r)
    {
        return r.Params != null && r.Params.Contains(6386804);
    }

    public static bool RefIsImbue(RefSkill r)
    {
        return r.Basic_Activity == 1 && RefIsAttack(r);
    }

    public static bool RefAttackLike(RefSkill r)
    {
        return !RefIsPassive(r) && !RefIsImbue(r) && (RefIsAttack(r) || r.TargetGroup_Enemy_M);
    }

    public static bool RefBuffLike(RefSkill r)
    {
        return !RefIsPassive(r) && !RefIsImbue(r) && !RefIsAttack(r) && !r.TargetGroup_Enemy_M;
    }

    /// <summary>Auto race detect: Chinese sees SKILL_CH_*, European SKILL_EU_*.</summary>
    public static string RacePrefix()
    {
        try
        {
            return Game.Player.Race switch
            {
                ObjectCountry.Chinese => "SKILL_CH_",
                ObjectCountry.Europe => "SKILL_EU_",
                _ => null
            };
        }
        catch
        {
            return null;
        }
    }

    /// <summary>Race-neutral codes (guild/job/common) always pass.</summary>
    public static bool RaceMatches(RefSkill r, string racePrefix)
    {
        if (racePrefix == null || string.IsNullOrEmpty(r?.Basic_Code))
            return true;

        var code = r.Basic_Code.ToUpperInvariant();
        if ((code.StartsWith("SKILL_CH_") || code.StartsWith("SKILL_EU_")) &&
            !code.StartsWith(racePrefix))
            return false;

        return true;
    }
    public static string MasteryLabelForRef(RefSkill r)
    {
        try
        {
            var mastery = Game.Player?.Skills.Masteries
                .FirstOrDefault(m => m.Id == r.ReqCommon_Mastery1);
            if (mastery?.Record == null)
                return "?";

            var translated = Game.ReferenceManager.GetTranslation(mastery.Record.NameCode);
            return string.IsNullOrWhiteSpace(translated) ? mastery.Record.NameCode : translated;
        }
        catch
        {
            return "?";
        }
    }

    /// <summary>
    ///     Resolves a tab filter ("fire", "heuk", "force"...) to reference skills.
    ///     Matches mastery name, skill code (SKILL_CH_FIRE_...) and group,
    ///     so Chinese sub-tabs like Fire/Cold/Lightning resolve correctly.
    /// </summary>
    public static (string title, List<RefSkill> refs) ResolveTab(
        string filter, bool wantAttack)
    {
        var empty = (string.Empty, new List<RefSkill>());
        if (Game.Player == null || Game.ReferenceManager?.SkillData == null)
            return empty;

        var needle = Normalize(filter);
        if (needle.Length == 0)
            return empty;

        // Only skills of the player's own masteries - kills cross-race
        // garbage (SkillData holds every client/race) and keeps rows
        // aligned with the player's real skill tree.
        var ownMasteryIds = new HashSet<long>();
        try
        {
            foreach (var m in Game.Player.Skills.Masteries)
                ownMasteryIds.Add(m.Id);
        }
        catch
        {
        }

        // Auto race detect: Chinese sees SKILL_CH_*, European SKILL_EU_*.
        // Race-neutral codes (guild/job/common) always pass.
        var racePrefix = RacePrefix();

        var matches = new List<RefSkill>();
        foreach (var r in Game.ReferenceManager.SkillData.Values)
        {
            if (r == null)
                continue;

            if (!ownMasteryIds.Contains(r.ReqCommon_Mastery1))
                continue;

            if (!RaceMatches(r, racePrefix))
                continue;

            var okType = wantAttack ? RefAttackLike(r) : RefBuffLike(r);
            if (!okType)
                continue;

            string name;
            try
            {
                name = r.GetRealName();
            }
            catch
            {
                continue;
            }

            if (Normalize(MasteryLabelForRef(r)).Contains(needle, StringComparison.Ordinal) ||
                Normalize(r.Basic_Code).Contains(needle, StringComparison.Ordinal) ||
                Normalize(r.Basic_Group).Contains(needle, StringComparison.Ordinal) ||
                Normalize(name).Contains(needle, StringComparison.Ordinal))
                matches.Add(r);
        }

        if (matches.Count == 0)
            return empty;

        // Title prefers the mastery label of the first hit, else the typed filter
        var title = MasteryLabelForRef(matches[0]);
        if (string.IsNullOrWhiteSpace(title) || title == "?")
            title = filter.Trim();

        return (title, matches);
    }

    /// <summary>
    ///     Builds game-like rows: one cell per skill LINE (levels collapse),
    ///     rows ordered by unlock tier (required mastery level), slots by skill id.
    ///     Unlearned cells keep their slot so numbering matches the game tree.
    /// </summary>
    public static List<GridRow> BuildGrid(List<RefSkill> refs)
    {
        var rows = new List<GridRow>();
        if (Game.Player == null)
            return rows;

        // Player's learned skills by normalized name (highest level wins)
        var learnedByName = new Dictionary<string, SkillInfo>();
        foreach (var s in Game.Player.Skills.KnownSkills)
        {
            if (s?.Record == null)
                continue;
            var key = Normalize(s.Record.GetRealName());
            if (!learnedByName.TryGetValue(key, out var cur) ||
                s.Record.Basic_Level > cur.Record.Basic_Level)
                learnedByName[key] = s;
        }

        if (Game.Player.TryGetAbilitySkills(out var abilitySkills))
            foreach (var s in abilitySkills)
            {
                if (s?.Record == null || s.IsPassive)
                    continue;
                var key = Normalize(s.Record.GetRealName());
                if (!learnedByName.ContainsKey(key))
                    learnedByName[key] = s;
            }

        // Series first: all levels of one skill line share a single cell,
        // tier = the unlock tier (lowest required mastery level of the line).
        var series = new List<(string key, string name, byte tier, uint minId)>();
        var seenSeries = new HashSet<string>();
        foreach (var g in refs
                     .GroupBy(r =>
                     {
                         try
                         {
                             return Normalize(r.GetRealName());
                         }
                         catch
                         {
                             return string.Empty;
                         }
                     }))
        {
            if (string.IsNullOrEmpty(g.Key) || !seenSeries.Add(g.Key))
                continue;

            string display;
            try
            {
                display = g.OrderBy(r => r.ID).First().GetRealName();
            }
            catch
            {
                continue;
            }

            series.Add((g.Key, display, g.Min(r => r.ReqCommon_MasteryLevel1),
                g.Min(r => r.ID)));
        }

        foreach (var tier in series
                     .GroupBy(s => s.tier)
                     .OrderBy(g => g.Key))
        {
            var cells = new List<GridCell>();
            foreach (var line in tier.OrderBy(s => s.minId))
            {
                learnedByName.TryGetValue(line.key, out var learned);

                uint refId = 0;
                try
                {
                    refId = refs.Where(r =>
                        {
                            try
                            {
                                return Normalize(r.GetRealName()) == line.key;
                            }
                            catch
                            {
                                return false;
                            }
                        })
                        .OrderBy(r => r.ID)
                        .First().ID;
                }
                catch
                {
                }

                cells.Add(new GridCell(line.name, learned, refId));
            }

            if (cells.Count > 0)
                rows.Add(new GridRow(tier.Key, cells));
        }

        return rows;
    }

    /// <summary>Drops unlearned cells (and emptied rows) - "my opened skills only".</summary>
    public static List<GridRow> PruneUnlearned(List<GridRow> rows)
    {
        var pruned = new List<GridRow>();
        foreach (var row in rows)
        {
            var cells = row.Cells.Where(c => c.IsLearned).ToList();
            if (cells.Count > 0)
                pruned.Add(new GridRow(row.ReqMasteryLevel, cells));
        }

        return pruned;
    }

    /// <summary>The player's own masteries for the tab dropdown (id + translated name + level).</summary>
    public static List<(uint Id, string Label, byte Level)> OwnMasteryOptions()
    {
        var options = new List<(uint Id, string Label, byte Level)>();
        try
        {
            foreach (var m in Game.Player.Skills.Masteries)
            {
                string label = null;
                try
                {
                    label = Game.ReferenceManager.GetTranslation(m.Record.NameCode);
                }
                catch
                {
                }

                if (string.IsNullOrWhiteSpace(label))
                    label = m.Record?.NameCode ?? m.Id.ToString();

                options.Add((m.Id, label, m.Level));
            }
        }
        catch
        {
        }

        return options;
    }

    /// <summary>
    ///     The player's OWN learned skills grouped by mastery, like the Skills
    ///     window: header "Heuksal (lv 42)", items icon + name + level.
    /// </summary>
    public static List<(string Header, List<SkillInfo> Skills)> MySkillsGrouped(bool attacks, bool buffs)
    {
        var groups = new List<(string Header, List<SkillInfo> Skills)>();
        if (Game.Player == null)
            return groups;

        var pool = new List<SkillInfo>();
        var seenIds = new HashSet<uint>();
        if (attacks)
            foreach (var s in GetAttackPool())
                if (seenIds.Add(s.Id))
                    pool.Add(s);
        if (buffs)
            foreach (var s in GetBuffPool())
                if (seenIds.Add(s.Id))
                    pool.Add(s);

        var masteries = new List<(uint Id, string Label, byte Level)>();
        try
        {
            masteries = OwnMasteryOptions();
        }
        catch
        {
        }

        var byMastery = new Dictionary<uint, List<SkillInfo>>();
        var other = new List<SkillInfo>();
        foreach (var s in pool.OrderBy(s => s.Id))
        {
            if (masteries.Any(m => (int)m.Id == s.Record.ReqCommon_Mastery1))
            {
                var mid = (uint)s.Record.ReqCommon_Mastery1;
                if (!byMastery.TryGetValue(mid, out var list))
                    byMastery[mid] = list = new List<SkillInfo>();
                list.Add(s);
            }
            else
            {
                other.Add(s);
            }
        }

        foreach (var m in masteries)
        {
            if (byMastery.TryGetValue(m.Id, out var list) && list.Count > 0)
                groups.Add(($"{m.Label} (lv. {m.Level})", list));
        }

        if (other.Count > 0)
            groups.Add(("Other", other));

        return groups;
    }

    /// <summary>Exact tab resolve by mastery id (no text guessing).</summary>
    public static (string title, List<RefSkill> refs) ResolveTabByMastery(uint masteryId, bool wantAttack)
    {
        var empty = (string.Empty, new List<RefSkill>());
        if (Game.Player == null || Game.ReferenceManager?.SkillData == null)
            return empty;

        var racePrefix = RacePrefix();
        var matches = new List<RefSkill>();
        foreach (var r in Game.ReferenceManager.SkillData.Values)
        {
            if (r == null || r.ReqCommon_Mastery1 != (int)masteryId)
                continue;

            if (!RaceMatches(r, racePrefix))
                continue;

            var okType = wantAttack ? RefAttackLike(r) : RefBuffLike(r);
            if (!okType)
                continue;

            matches.Add(r);
        }

        if (matches.Count == 0)
            return empty;

        var title = MasteryLabelForRef(matches[0]);
        if (string.IsNullOrWhiteSpace(title) || title == "?")
            title = "Skills";

        return (title, matches);
    }

    /// <summary>Learned skills of a grid in row-major order (for !pick N).</summary>
    public static List<SkillInfo> GridLearnedFlat(List<GridRow> rows)
    {
        var flat = new List<SkillInfo>();
        foreach (var row in rows)
            foreach (var cell in row.Cells)
                if (cell.IsLearned)
                    flat.Add(cell.Learned);

        return flat;
    }
}
