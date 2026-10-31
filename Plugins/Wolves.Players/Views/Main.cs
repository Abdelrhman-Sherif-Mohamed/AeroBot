using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using RSBot.Core;
using RSBot.Core.Client.ReferenceObjects;
using RSBot.Core.Components;
using RSBot.Core.Extensions;
using RSBot.Core.Objects;
using RSBot.Core.Objects.Party;
using RSBot.Core.Objects.Spawn;
using SDUI;
using SDUI.Controls;
using ListView = System.Windows.Forms.ListView;
using ListViewExtensions = RSBot.Core.Extensions.ListViewExtensions;

namespace Wolves.Players.Views;

[ToolboxItem(false)]
public partial class Main : DoubleBufferedControl
{
    private static Main _instance;

    public static Main Instance => _instance ??= new Main();

    private bool _syncingControls;
    private bool _updatingLists;
    private uint _inspectedPlayerId;

    public Main()
    {
        CheckForIllegalCrossThreadCalls = false;
        InitializeComponent();

        listViewPlayers.SmallImageList = ListViewExtensions.StaticItemsImageList;
        listViewInspect.SmallImageList = ListViewExtensions.StaticItemsImageList;
        listViewEquipment.SmallImageList = ListViewExtensions.StaticItemsImageList;
        listViewAvatars.SmallImageList = ListViewExtensions.StaticItemsImageList;

        listViewPlayers.BackColor = ColorScheme.BackColor;
        listViewPlayers.ForeColor = ColorScheme.ForeColor;
        listViewInspect.BackColor = ColorScheme.BackColor;
        listViewInspect.ForeColor = ColorScheme.ForeColor;
        listViewEquipment.BackColor = ColorScheme.BackColor;
        listViewEquipment.ForeColor = ColorScheme.ForeColor;
        listViewAvatars.BackColor = ColorScheme.BackColor;
        listViewAvatars.ForeColor = ColorScheme.ForeColor;

        listViewPlayers.ContextMenuStrip = contextMenuPlayers;
        listViewInspect.ContextMenuStrip = contextMenuPlayers;

        PlayerTraceManager.Instance.LoadSettings();
        SyncControlsFromManager();
        SubscribeEvents();

        RefreshPlayers();
        UpdateTraceControls();
        Translate();
    }

    public void Translate()
    {
        LanguageManager.Translate(this, Kernel.Language);
    }

    private void SubscribeEvents()
    {
        PlayerTraceManager.Instance.OnStateChanged += OnManagerStateChanged;

        contextMenuPlayers.Opening += contextMenuPlayers_Opening;
        contextMenuPlayers.Closed += contextMenuPlayers_Closed;
    }

    private void SafeInvoke(System.Action action)
    {
        if (IsDisposed || !IsHandleCreated)
            return;

        if (InvokeRequired)
            BeginInvoke(action);
        else
            action();
    }

    private void OnManagerStateChanged()
    {
        SafeInvoke(() =>
        {
            SyncControlsFromManager();
            UpdateTraceControls();
            RefreshPlayers();
        });
    }

    private void SyncControlsFromManager()
    {
        _syncingControls = true;

        var manager = PlayerTraceManager.Instance;

        chkInGameTrace.Checked = manager.InGameTrace;
        chkBuff.Checked = manager.EnableBuffs;
        chkPartyBuff.Checked = manager.EnablePartyBuffs;
        chkAttackMonsters.Checked = manager.AttackMonsters;

        numTraceDistance.Value = Clamp(manager.TraceDistance, numTraceDistance.Minimum, numTraceDistance.Maximum);
        numAttackRange.Value = Clamp(manager.AttackRange, numAttackRange.Minimum, numAttackRange.Maximum);

        _syncingControls = false;
    }

    private static decimal Clamp(int value, decimal minimum, decimal maximum)
    {
        if (value < minimum)
            return minimum;

        if (value > maximum)
            return maximum;

        return value;
    }

    private void TraceSettingChanged(object sender, EventArgs e)
    {
        if (_syncingControls)
            return;

        var manager = PlayerTraceManager.Instance;
        var wasInGameTrace = manager.InGameTrace;

        manager.InGameTrace = chkInGameTrace.Checked;
        manager.EnableBuffs = chkBuff.Checked;
        manager.EnablePartyBuffs = chkPartyBuff.Checked;
        manager.AttackMonsters = chkAttackMonsters.Checked;
        manager.TraceDistance = (int)numTraceDistance.Value;
        manager.AttackRange = (int)numAttackRange.Value;
        manager.SaveSettings();

        if (wasInGameTrace != manager.InGameTrace)
            manager.ApplyTraceMode();
    }

    private void btnTraceStart_Click(object sender, EventArgs e)
    {
        var targetName = cboTraceTarget.SelectedItem as string;

        if (string.IsNullOrWhiteSpace(targetName))
        {
            MessageBox.Show(this, "يرجى اختيار لاعب من القائمة أولاً.", "اللاعبين",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        if (!PlayerTraceManager.Instance.StartTrace(targetName))
        {
            MessageBox.Show(this, PlayerTraceManager.Instance.StatusText, "اللاعبين",
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }

        RefreshPlayers();
        UpdateTraceControls();
    }

    private void btnTraceStop_Click(object sender, EventArgs e)
    {
        PlayerTraceManager.Instance.StopTrace();
        RefreshPlayers();
        UpdateTraceControls();
    }

    private void btnTraceReset_Click(object sender, EventArgs e)
    {
        PlayerTraceManager.Instance.Reset();

        _inspectedPlayerId = 0;
        ClearInspection();

        if (cboTraceTarget.Items.Count > 0)
            cboTraceTarget.SelectedIndex = 0;

        RefreshPlayers();
        UpdateTraceControls();
    }

    private void btnRefreshPlayers_Click(object sender, EventArgs e)
    {
        RefreshPlayers();
    }

    private void timerRefresh_Tick(object sender, EventArgs e)
    {
        RefreshPlayers();
        UpdateTraceControls();
    }

    private void RefreshPlayers()
    {
        if (IsDisposed)
            return;

        var players = GetNearbyPlayers();

        UpdatePlayerList(players);
        UpdateInspectList(players);
        UpdateStats(players);
        UpdateTargetCombo(players);
    }

    private static List<SpawnedPlayer> GetNearbyPlayers()
    {
        var result = new List<SpawnedPlayer>();

        if (Game.Player == null || !Game.Ready)
            return result;

        if (!SpawnManager.TryGetEntities<SpawnedPlayer>(out var entities) || entities == null)
            return result;

        foreach (var player in entities)
        {
            if (player == null || string.IsNullOrWhiteSpace(player.Name))
                continue;

            result.Add(player);
        }

        return result
            .OrderBy(GetDistanceSafe)
            .ThenBy(p => p.Name, StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    private static double GetDistanceSafe(SpawnedPlayer player)
    {
        try
        {
            return player.DistanceToPlayer;
        }
        catch
        {
            return double.MaxValue;
        }
    }

    private void UpdatePlayerList(List<SpawnedPlayer> players)
    {
        var selectedId = GetSelectedUid(listViewPlayers);

        _updatingLists = true;
        listViewPlayers.BeginUpdate();
        listViewPlayers.Items.Clear();

        foreach (var player in players)
        {
            var item = new ListViewItem(player.Name)
            {
                Tag = player.UniqueId,
                ForeColor = GetPlayerColor(player)
            };

            item.SubItems.Add(GetPartyLevel(player.Name)?.ToString() ?? "-");
            item.SubItems.Add(FormatDistance(player));
            item.SubItems.Add(GetJobName(player.Job));
            item.SubItems.Add(player.WearsJobSuite ? "نعم" : "لا");
            item.SubItems.Add(GetPlayerStatus(player));

            listViewPlayers.Items.Add(item);
        }

        listViewPlayers.EndUpdate();
        _updatingLists = false;

        if (selectedId.HasValue)
            SelectItemByUid(listViewPlayers, selectedId.Value, false);
    }

    private void UpdateInspectList(List<SpawnedPlayer> players)
    {
        var selectedId = _inspectedPlayerId != 0 ? _inspectedPlayerId : GetSelectedUid(listViewInspect);

        _updatingLists = true;
        listViewInspect.BeginUpdate();
        listViewInspect.Items.Clear();

        foreach (var player in players)
        {
            var item = new ListViewItem(player.Name)
            {
                Tag = player.UniqueId,
                ForeColor = GetPlayerColor(player)
            };

            var guildName = player.Guild?.Name;
            item.SubItems.Add(string.IsNullOrWhiteSpace(guildName) ? "-" : guildName);

            listViewInspect.Items.Add(item);
        }

        listViewInspect.EndUpdate();

        if (selectedId.HasValue)
            SelectItemByUid(listViewInspect, selectedId.Value, true);
        else if (listViewInspect.Items.Count > 0)
        {
            var first = listViewInspect.Items[0];
            first.Selected = true;
            first.Focused = true;
            _inspectedPlayerId = (uint)first.Tag;
        }

        _updatingLists = false;

        var inspected = players.FirstOrDefault(p => p.UniqueId == _inspectedPlayerId);

        if (inspected != null)
            ShowInspection(inspected);
        else
        {
            _inspectedPlayerId = 0;
            ClearInspection();
        }
    }

    private void UpdateStats(List<SpawnedPlayer> players)
    {
        var manager = PlayerTraceManager.Instance;

        lblCurrentTraceValue.Text = manager.IsTracing && !string.IsNullOrWhiteSpace(manager.TraceTargetName)
            ? manager.TraceTargetName
            : "-";
        lblCurrentTraceValue.ForeColor = manager.IsTracing ? Color.LimeGreen : ColorScheme.ForeColor;

        var thieves = players.Count(p => p.Job == JobType.Thief);
        var hunters = players.Count(p => p.Job == JobType.Hunter);

        lblPlayersValue.Text = players.Count.ToString();
        lblThievesValue.Text = thieves.ToString();
        lblHtValue.Text = $"{hunters} / {thieves}";
        lblRefreshTime.Text = $"آخر تحديث: {DateTime.Now:HH:mm:ss}";
    }

    private void UpdateTargetCombo(List<SpawnedPlayer> players)
    {
        var names = players
            .Select(p => p.Name)
            .Where(n => !string.IsNullOrWhiteSpace(n))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(n => n, StringComparer.OrdinalIgnoreCase)
            .ToList();

        var current = cboTraceTarget.SelectedItem as string;
        var manager = PlayerTraceManager.Instance;

        var desired = manager.IsTracing && !string.IsNullOrWhiteSpace(manager.TraceTargetName)
            ? manager.TraceTargetName
            : current;

        var changed = cboTraceTarget.Items.Count != names.Count;
        if (!changed)
        {
            for (var i = 0; i < names.Count; i++)
            {
                if (string.Equals(cboTraceTarget.Items[i] as string, names[i], StringComparison.OrdinalIgnoreCase))
                    continue;

                changed = true;
                break;
            }
        }

        if (changed)
        {
            _syncingControls = true;
            cboTraceTarget.BeginUpdate();
            cboTraceTarget.Items.Clear();
            cboTraceTarget.Items.AddRange(names.Cast<object>().ToArray());
            cboTraceTarget.EndUpdate();
            _syncingControls = false;
        }

        if (!string.IsNullOrWhiteSpace(desired))
        {
            var match = names.FirstOrDefault(n =>
                string.Equals(n, desired, StringComparison.OrdinalIgnoreCase));

            if (match != null)
                cboTraceTarget.SelectedItem = match;
            else if (cboTraceTarget.SelectedIndex < 0 && names.Count > 0)
                cboTraceTarget.SelectedIndex = 0;
        }
        else if (cboTraceTarget.SelectedIndex < 0 && cboTraceTarget.Items.Count > 0)
        {
            cboTraceTarget.SelectedIndex = 0;
        }
    }

    private void UpdateTraceControls()
    {
        var manager = PlayerTraceManager.Instance;

        lblTraceStatus.Text = $"الحالة: {manager.StatusText}";

        if (manager.IsTracing)
            lblTraceStatus.ForeColor = Color.LimeGreen;
        else if (manager.StatusText.StartsWith("توقف:", StringComparison.Ordinal))
            lblTraceStatus.ForeColor = Color.OrangeRed;
        else
            lblTraceStatus.ForeColor = Color.Gray;

        btnTraceStart.Enabled = !manager.IsTracing;
        btnTraceStop.Enabled = manager.IsTracing;
    }

    private void listViewInspect_SelectedIndexChanged(object sender, EventArgs e)
    {
        if (_updatingLists)
            return;

        var player = GetSelectedPlayer(listViewInspect);
        if (player == null)
        {
            if (listViewInspect.Items.Count == 0)
            {
                _inspectedPlayerId = 0;
                ClearInspection();
            }

            return;
        }

        _inspectedPlayerId = player.UniqueId;
        ShowInspection(player);
    }

    private void listViewPlayers_MouseDown(object sender, MouseEventArgs e)
    {
        if (e.Button != MouseButtons.Right)
            return;

        if (sender is not ListView listView)
            return;

        var item = listView.GetItemAt(e.X, e.Y);
        if (item == null)
            return;

        item.Selected = true;
        item.Focused = true;
        listView.FocusedItem = item;
    }

    private void contextMenuPlayers_Opening(object sender, CancelEventArgs e)
    {
        if (GetSelectedPlayer() == null)
        {
            e.Cancel = true;
            return;
        }

        timerRefresh.Stop();
    }

    private void contextMenuPlayers_Closed(object sender, ToolStripDropDownClosedEventArgs e)
    {
        timerRefresh.Start();
    }

    private void menuTracePlayer_Click(object sender, EventArgs e)
    {
        var player = GetSelectedPlayer();
        if (player == null)
            return;

        if (!PlayerTraceManager.Instance.StartTrace(player))
            return;

        tabControl.SelectedTab = tabPlayers;
        RefreshPlayers();
        UpdateTraceControls();
    }

    private void menuInspectPlayer_Click(object sender, EventArgs e)
    {
        var player = GetSelectedPlayer();
        if (player == null)
            return;

        _inspectedPlayerId = player.UniqueId;
        tabControl.SelectedTab = tabInspect;

        RefreshPlayers();
        ShowInspection(player);
        SelectItemByUid(listViewInspect, player.UniqueId, false);
    }

    private SpawnedPlayer GetSelectedPlayer()
    {
        if (contextMenuPlayers.SourceControl is ListView source)
            return GetSelectedPlayer(source);

        return GetSelectedPlayer(listViewPlayers) ?? GetSelectedPlayer(listViewInspect);
    }

    private static SpawnedPlayer GetSelectedPlayer(ListView listView)
    {
        if (listView == null || listView.SelectedItems.Count == 0)
            return null;

        if (listView.SelectedItems[0].Tag is not uint uniqueId)
            return null;

        return SpawnManager.TryGetEntity<SpawnedPlayer>(uniqueId, out var player) ? player : null;
    }

    private static uint? GetSelectedUid(ListView listView)
    {
        if (listView == null || listView.SelectedItems.Count == 0)
            return null;

        return listView.SelectedItems[0].Tag is uint uniqueId ? uniqueId : null;
    }

    private static void SelectItemByUid(ListView listView, uint uniqueId, bool clearOtherSelections)
    {
        if (listView == null)
            return;

        foreach (ListViewItem item in listView.Items)
        {
            if (item.Tag is not uint itemUid || itemUid != uniqueId)
                continue;

            if (clearOtherSelections)
                listView.SelectedItems.Clear();

            item.Selected = true;
            item.Focused = true;
            listView.FocusedItem = item;
            listView.EnsureVisible(item.Index);
            return;
        }
    }

    private void ShowInspection(SpawnedPlayer player)
    {
        if (player == null)
        {
            ClearInspection();
            return;
        }

        lblInspectNameValue.Text = player.Name;

        var guild = player.Guild;

        lblInspectGuildValue.Text = string.IsNullOrWhiteSpace(guild?.Name) ? "-" : guild.Name;
        lblInspectMemberValue.Text = string.IsNullOrWhiteSpace(guild?.Member?.Nickname)
            ? "-"
            : guild.Member.Nickname;
        lblInspectGuildIdValue.Text = guild != null && guild.Id != 0 ? guild.Id.ToString() : "-";
        lblInspectUnionIdValue.Text = guild?.Union != null && guild.Union.Id != 0
            ? guild.Union.Id.ToString()
            : "-";
        lblInspectStatusValue.Text = GetPlayerStatus(player);

        FillItemList(listViewEquipment, player.Inventory);
        FillItemList(listViewAvatars, player.Avatars);
    }

    private void ClearInspection()
    {
        lblInspectNameValue.Text = "-";
        lblInspectGuildValue.Text = "-";
        lblInspectMemberValue.Text = "-";
        lblInspectGuildIdValue.Text = "-";
        lblInspectUnionIdValue.Text = "-";
        lblInspectStatusValue.Text = "-";

        listViewEquipment.Items.Clear();
        listViewAvatars.Items.Clear();
    }

    private static void FillItemList(ListView listView, Dictionary<RefObjItem, byte> items)
    {
        listView.BeginUpdate();
        listView.Items.Clear();

        if (items != null)
        {
            foreach (var pair in items
                         .Where(p => p.Key != null)
                         .OrderBy(p => p.Key.TypeID3)
                         .ThenBy(p => p.Key.GetRealName(), StringComparer.OrdinalIgnoreCase))
            {
                var itemRecord = pair.Key;
                var name = itemRecord.GetRealName();

                if (string.IsNullOrWhiteSpace(name))
                    name = itemRecord.CodeName;

                var item = new ListViewItem(name)
                {
                    Tag = itemRecord,
                    ForeColor = ColorScheme.ForeColor
                };

                item.SubItems.Add($"+{pair.Value}");
                item.SubItems.Add(GetItemType(itemRecord));
                item.SubItems.Add(GetItemRarity(itemRecord));

                item.LoadItemImageAsync(itemRecord);
                listView.Items.Add(item);
            }
        }

        listView.EndUpdate();
    }

    private static string GetItemType(RefObjItem item)
    {
        if (item == null)
            return "-";

        if (item.TypeID2 == 1)
        {
            switch (item.TypeID3)
            {
                case 1:
                    return "ملابس";
                case 2:
                    return "درع خفيف";
                case 3:
                    return "درع ثقيل";
                case 4:
                    return "ترس";
                case 5:
                    return "إكسسوار";
                case 6:
                    return "سلاح";
                case 7:
                    return "زي مهني";
                case 9:
                case 10:
                case 11:
                    return "درع";
                case 12:
                    return "إكسسوار";
                case 13:
                case 14:
                    return "أفاتار";
                default:
                    return "معدات";
            }
        }

        return "أخرى";
    }

    private static string GetItemRarity(RefObjItem item)
    {
        if (item == null)
            return "-";

        return item.Rarity switch
        {
            ObjectRarity.ClassA => "عادي",
            ObjectRarity.ClassB => "أزرق",
            ObjectRarity.ClassC => "SoX",
            ObjectRarity.ClassD => "طقم",
            ObjectRarity.ClassE => "نادر",
            ObjectRarity.ClassF => "نادر+",
            ObjectRarity.ClassG => "ROC",
            ObjectRarity.ClassH => "أسطوري",
            ObjectRarity.ClassI => "فريد",
            _ => item.Rarity.ToString()
        };
    }

    private static string GetJobName(JobType job)
    {
        return job switch
        {
            JobType.Trade => "تاجر",
            JobType.Thief => "لص",
            JobType.Hunter => "صياد",
            _ => "-"
        };
    }

    private static string GetPlayerStatus(SpawnedPlayer player)
    {
        if (player == null)
            return "-";

        if (player.State.LifeState == LifeState.Dead)
            return "ميت";

        var states = new List<string>();

        if (player.InCombat)
            states.Add("قتال");

        if (player.PvpState == PvpState.Purple)
            states.Add("بنفسجي");
        else if (player.PvpState == PvpState.Red)
            states.Add("أحمر");

        if (player.OnTransport)
            states.Add("مركبة");

        if (player.ScrollMode != ScrollMode.None)
            states.Add("سكرول");

        if (player.Stall != null || player.InteractMode == InteractMode.OPNMKT_DEAL)
            states.Add("متجر");

        return states.Count == 0 ? "طبيعي" : string.Join("، ", states);
    }

    private static Color GetPlayerColor(SpawnedPlayer player)
    {
        if (player == null)
            return ColorScheme.ForeColor;

        if (player.State.LifeState == LifeState.Dead)
            return Color.Gray;

        if (player.PvpState == PvpState.Red)
            return Color.IndianRed;

        if (player.PvpState == PvpState.Purple)
            return Color.MediumPurple;

        return ColorScheme.ForeColor;
    }

    private static byte? GetPartyLevel(string playerName)
    {
        var party = Game.Party;
        if (party == null || string.IsNullOrWhiteSpace(playerName))
            return null;

        if (party.Leader != null &&
            string.Equals(party.Leader.Name, playerName, StringComparison.OrdinalIgnoreCase))
            return party.Leader.Level;

        var member = party.Members?.FirstOrDefault(m =>
            m != null && string.Equals(m.Name, playerName, StringComparison.OrdinalIgnoreCase));

        return member?.Level;
    }

    private static string FormatDistance(SpawnedPlayer player)
    {
        var distance = GetDistanceSafe(player);

        return distance == double.MaxValue ? "-" : distance.ToString("0.0");
    }
}
