using System;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using RSBot.Core;
using RSBot.Core.Components;
using RSBot.Core.Objects;
using RSBot.Core.Objects.Skill;
using SDUI;
using SDUI.Controls;
using SDUI.Helpers;

namespace Wolves.Fortress.Views;

[ToolboxItem(false)]
public partial class Main : DoubleBufferedControl
{
    private static Main _instance;
    public static Main Instance => _instance ??= new Main();

    private bool _updatingUI;
    private bool _applyingFortressLayout;

    private class SkillDropdownItem
    {
        public uint Id { get; set; }
        public string Name { get; set; }

        public override string ToString() => Name;
    }

    public Main()
    {
        InitializeComponent();
        ControlSpacer.NormalizePage(this);

        ApplyFortressLocalization();
        ApplyFortressLayout();

        FortressManager.Instance.OnStateChanged += () => SafeInvoke(UpdateUI);
        FortressManager.Instance.OnLogAdded += log => SafeInvoke(() => AppendLog(log));
    }

    protected override void OnResize(EventArgs e)
    {
        base.OnResize(e);
        ApplyFortressLayout();
    }

    protected override void OnVisibleChanged(EventArgs e)
    {
        base.OnVisibleChanged(e);

        if (!Visible)
            return;

        ApplyFortressLocalization();
        ApplyFortressLayout();
    }

    private int S(float value)
    {
        return (int)Math.Round(value * DeviceDpi / 96f);
    }

    private void Place(Control control, int x, int y, int width, int height)
    {
        if (control == null)
            return;

        control.Anchor = AnchorStyles.None;
        control.Dock = DockStyle.None;
        control.AutoSize = false;
        control.SetBounds(S(x), S(y), Math.Max(1, S(width)), Math.Max(1, S(height)));
    }

    private void PlaceLabel(SDUI.Controls.Label label, int x, int y, int width, int height,
        ContentAlignment alignment)
    {
        if (label == null)
            return;

        Place(label, x, y, width, height);
        label.TextAlign = alignment;
    }

    private static string T(string key, string fallback)
    {
        return LanguageManager.GetLangBySpecificKey("Wolves.Fortress", key, fallback);
    }

    private static Color MutedColor => ColorScheme.BackColor.IsDark()
        ? Color.FromArgb(160, 175, 195)
        : Color.FromArgb(100, 116, 139);

    private static Color ErrorColor => ColorScheme.BackColor.IsDark()
        ? Color.FromArgb(248, 113, 113)
        : Color.FromArgb(220, 38, 38);

    /// <summary>
    ///     Applies the current language to the page captions, dropdown items and
    ///     status strings. Called from Bootstrap.Translate and whenever the page
    ///     becomes visible so a language switch is reflected immediately.
    /// </summary>
    public void ApplyFortressLocalization()
    {
        if (IsDisposed || groupSettings == null)
            return;

        var wasUpdating = _updatingUI;
        _updatingUI = true;

        try
        {
            groupSettings.Text = T("Wolves.Fortress.Settings.Title", "إعدادات حرب القلعة");
            chkEnabled.Text = T("Wolves.Fortress.Settings.Enable", "تفعيل نظام حرب القلعة");
            chkAutoCast.Text = T("Wolves.Fortress.Settings.AutoCast", "إلقاء تلقائي عند الجاهزية");
            lblChannel.Text = T("Wolves.Fortress.Settings.Channel", "قناة الرد:");
            btnAutoFill.Text = T("Wolves.Fortress.Settings.AutoFill", "توزيع أعضاء المجموعة تلقائيًا");

            var channelIndex = cboChannel.SelectedIndex;
            cboChannel.BeginUpdate();
            cboChannel.Items.Clear();
            cboChannel.Items.AddRange(new object[]
            {
                T("Wolves.Fortress.Settings.ChannelParty", "شات المجموعة"),
                T("Wolves.Fortress.Settings.ChannelWhisper", "الهمس الخاص")
            });
            cboChannel.EndUpdate();
            cboChannel.SelectedIndex = channelIndex >= 0 && channelIndex < cboChannel.Items.Count
                ? channelIndex
                : (FortressManager.Instance.ResponseChannel == ChatType.Private ? 1 : 0);

            for (var i = 0; i < 7; i++)
            {
                groupSlots[i].Text = string.Format(
                    T("Wolves.Fortress.Slot.Title", "الخانة {0}"), i + 1);
                chkSlotEnabled[i].Text = T("Wolves.Fortress.Slot.Enable", "تفعيل");
                lblSlotBadge[i].Text = $"[b{i + 1}]";
                lblPlayerCaption[i].Text = T("Wolves.Fortress.Slot.Player", "اللاعب");
                lblSkillCaption[i].Text = T("Wolves.Fortress.Slot.Skill", "البفة");
                btnCast[i].Text = T("Wolves.Fortress.Slot.Cast", "إلقاء");
                lblStatus[i].Text = T("Wolves.Fortress.Slot.StatusUnset", "غير محدد");
            }

            groupLogs.Text = T("Wolves.Fortress.Log.Title", "سجل عمليات القلعة");
            btnClearLogs.Text = T("Wolves.Fortress.Log.Clear", "مسح السجل");

            UpdateMasterInfo();
            ApplyFortressTheme();
        }
        finally
        {
            _updatingUI = wasUpdating;
        }
    }

    /// <summary>
    ///     Reflows the designer's fixed 930x458 surface into the actual tab area.
    ///     Seven slot cards form a 3x3 grid; the last row's free two columns are
    ///     used by the log card, so no dead space is left at the bottom.
    /// </summary>
    private void ApplyFortressLayout()
    {
        if (_applyingFortressLayout || IsDisposed || groupSettings == null ||
            groupSlots == null || groupLogs == null)
            return;

        var scale = DeviceDpi / 96f;
        var pageW = (int)(ClientSize.Width / scale);
        var pageH = (int)(ClientSize.Height / scale);
        if (pageW < 640 || pageH < 420)
            return;

        _applyingFortressLayout = true;
        try
        {
            const int margin = 14;
            const int gap = 12;
            const int settingsH = 98;

            var contentW = pageW - margin * 2;

            Place(groupSettings, margin, margin, contentW, settingsH);

            Place(chkEnabled, 20, 30, 230, 24);
            Place(chkAutoCast, 262, 30, 250, 24);
            Place(btnAutoFill, contentW - 248, 29, 228, 26);
            PlaceLabel(lblChannel, 20, 62, 116, 22, ContentAlignment.MiddleRight);
            Place(cboChannel, 142, 60, 190, 26);
            PlaceLabel(lblMasterInfo, 350, 62, contentW - 370, 22, ContentAlignment.MiddleLeft);

            var gridTop = margin + settingsH + gap;
            var gridH = pageH - margin - gridTop;
            var cardW = (contentW - gap * 2) / 3;
            var rowH = (gridH - gap * 2) / 3;
            if (cardW < 250 || rowH < 120)
                return;

            for (var i = 0; i < 7; i++)
            {
                var row = i / 3;
                var column = i % 3;
                var x = margin + column * (cardW + gap);
                var width = column == 2 ? contentW - (cardW + gap) * 2 : cardW;
                var y = gridTop + row * (rowH + gap);
                var height = row == 2 ? gridH - (rowH + gap) * 2 : rowH;

                Place(groupSlots[i], x, y, width, height);
                ApplySlotLayout(i, width);
            }

            var logX = margin + cardW + gap;
            var logW = contentW - cardW - gap;
            var logY = gridTop + (rowH + gap) * 2;
            var logH = gridH - (rowH + gap) * 2;

            Place(groupLogs, logX, logY, logW, logH);
            ApplyLogLayout(logW, logH);

            ApplyFortressTheme();
        }
        finally
        {
            _applyingFortressLayout = false;
        }
    }

    private void ApplySlotLayout(int index, int width)
    {
        const int pad = 12;
        const int row1 = 30;
        const int row2 = 62;
        const int row3 = 94;

        Place(chkSlotEnabled[index], pad, row1 + 1, 60, 24);
        Place(lblSlotBadge[index], pad + 62, row1, 40, 24);
        Place(btnCast[index], width - 120, row1, 108, 26);
        PlaceLabel(lblStatus[index], 118, 4, width - 130, 20, ContentAlignment.MiddleRight);

        PlaceLabel(lblPlayerCaption[index], pad, row2, 52, 26, ContentAlignment.MiddleRight);
        Place(cboPlayer[index], 68, row2, width - 80, 26);
        PlaceLabel(lblSkillCaption[index], pad, row3, 52, 26, ContentAlignment.MiddleRight);
        Place(cboSkill[index], 68, row3, width - 80, 26);
    }

    private void ApplyLogLayout(int width, int height)
    {
        Place(txtLogs, 12, 30, width - 24, Math.Max(40, height - 44));
        Place(btnClearLogs, width - 108, 3, 96, 23);
    }

    private void ApplyFortressTheme()
    {
        groupSettings.ForeColor = ColorScheme.ForeColor;
        groupLogs.ForeColor = ColorScheme.ForeColor;

        chkEnabled.ForeColor = ColorScheme.ForeColor;
        chkAutoCast.ForeColor = ColorScheme.ForeColor;
        lblChannel.ForeColor = ColorScheme.ForeColor;
        lblMasterInfo.ForeColor = ColorScheme.AccentColor;

        cboChannel.BackColor = ColorScheme.InputBackColor;
        cboChannel.ForeColor = ColorScheme.ForeColor;

        for (var i = 0; i < 7; i++)
        {
            groupSlots[i].ForeColor = ColorScheme.ForeColor;
            chkSlotEnabled[i].ForeColor = ColorScheme.ForeColor;
            lblSlotBadge[i].ForeColor = ColorScheme.AccentColor;
            lblPlayerCaption[i].ForeColor = MutedColor;
            lblSkillCaption[i].ForeColor = MutedColor;
            cboPlayer[i].BackColor = ColorScheme.InputBackColor;
            cboPlayer[i].ForeColor = ColorScheme.ForeColor;
            cboSkill[i].BackColor = ColorScheme.InputBackColor;
            cboSkill[i].ForeColor = ColorScheme.ForeColor;
        }

        btnClearLogs.ForeColor = ColorScheme.ForeColor;
        txtLogs.ApplyTheme();
    }

    private void SafeInvoke(System.Action action)
    {
        if (IsHandleCreated && InvokeRequired)
            BeginInvoke(action);
        else
            action();
    }

    private void Main_Load(object sender, EventArgs e)
    {
        UpdateUI();
        LoadLogs();
        timerCooldown.Start();
    }

    private void UpdateUI()
    {
        if (_updatingUI) return;
        _updatingUI = true;

        try
        {
            chkEnabled.Checked = FortressManager.Instance.Enabled;
            chkAutoCast.Checked = FortressManager.Instance.AutoCastWhenReady;
            cboChannel.SelectedIndex = FortressManager.Instance.ResponseChannel == ChatType.Private ? 1 : 0;

            UpdateMasterInfo();

            for (int i = 0; i < 7; i++)
            {
                var slot = FortressManager.Instance.Slots[i];
                chkSlotEnabled[i].Checked = slot.Enabled;
                cboPlayer[i].Text = slot.PlayerName ?? string.Empty;

                RefreshPlayerSkillsDropDown(cboSkill[i]);

                // Select current skill in dropdown if present
                for (int s = 0; s < cboSkill[i].Items.Count; s++)
                {
                    if (cboSkill[i].Items[s] is SkillDropdownItem item && item.Id == slot.SkillId)
                    {
                        cboSkill[i].SelectedIndex = s;
                        break;
                    }
                }

                if (slot.SkillId != 0 && cboSkill[i].SelectedIndex == -1 && !string.IsNullOrEmpty(slot.SkillName))
                {
                    cboSkill[i].Text = slot.SkillName;
                }
            }
        }
        finally
        {
            _updatingUI = false;
        }
    }

    private void UpdateMasterInfo()
    {
        if (Game.Player != null && Game.Ready)
        {
            lblMasterInfo.Text = string.Format(
                T("Wolves.Fortress.Settings.MasterOnline",
                    "صاحب البوت: {0} — المستوى {1} — أعضاء المجموعة: {2}/8"),
                Game.Player.Name, Game.Player.Level, Game.Party?.Members?.Count ?? 0);
        }
        else
        {
            lblMasterInfo.Text = T("Wolves.Fortress.Settings.MasterOffline",
                "صاحب البوت: غير متصل باللعبة");
        }
    }

    private void RefreshPartyMembersDropDown(SDUI.Controls.ComboBox cbo)
    {
        var currentText = cbo.Text;
        cbo.Items.Clear();

        if (Game.Party?.Members != null)
        {
            var myName = Game.Player?.Name ?? string.Empty;
            foreach (var m in Game.Party.Members)
            {
                if (!string.IsNullOrEmpty(m.Name) && !string.Equals(m.Name, myName, StringComparison.OrdinalIgnoreCase))
                {
                    if (!cbo.Items.Contains(m.Name))
                        cbo.Items.Add(m.Name);
                }
            }
        }

        cbo.Text = currentText;
    }

    private void RefreshPlayerSkillsDropDown(SDUI.Controls.ComboBox cbo)
    {
        var selectedId = (cbo.SelectedItem as SkillDropdownItem)?.Id ?? 0;
        cbo.Items.Clear();

        cbo.Items.Add(new SkillDropdownItem
        {
            Id = 0,
            Name = T("Wolves.Fortress.Slot.None", "-- بدون بفة --")
        });

        if (Game.Player?.Skills?.KnownSkills != null)
        {
            foreach (var skill in Game.Player.Skills.KnownSkills)
            {
                if (skill.Record == null || skill.IsPassive || skill.IsAttack)
                    continue;

                // Candidate buff skill
                var skillName = skill.Record.GetRealName();
                cbo.Items.Add(new SkillDropdownItem
                {
                    Id = skill.Id,
                    Name = $"{skillName} (Lv.{skill.Record.Basic_Level})"
                });
            }
        }

        if (selectedId != 0)
        {
            for (int i = 0; i < cbo.Items.Count; i++)
            {
                if (cbo.Items[i] is SkillDropdownItem item && item.Id == selectedId)
                {
                    cbo.SelectedIndex = i;
                    break;
                }
            }
        }
    }

    private void timerCooldown_Tick(object sender, EventArgs e)
    {
        UpdateMasterInfo();

        for (int i = 0; i < 7; i++)
        {
            var slot = FortressManager.Instance.Slots[i];
            if (!slot.Enabled)
            {
                lblStatus[i].Text = T("Wolves.Fortress.Slot.Disabled", "معطّل");
                lblStatus[i].ForeColor = MutedColor;
                continue;
            }

            if (slot.SkillId == 0)
            {
                lblStatus[i].Text = T("Wolves.Fortress.Slot.NoSkill", "بدون بفة");
                lblStatus[i].ForeColor = MutedColor;
                continue;
            }

            var skill = Game.Player?.Skills?.GetSkillInfoById(slot.SkillId);
            if (skill == null)
            {
                lblStatus[i].Text = T("Wolves.Fortress.Slot.Unavailable", "غير متاحة");
                lblStatus[i].ForeColor = ErrorColor;
                continue;
            }

            if (skill.HasCooldown)
            {
                var rem = skill.RemainingCooldownSeconds;
                lblStatus[i].Text = string.Format(
                    T("Wolves.Fortress.Slot.Cooldown", "متاح بعد: {0} ث"), rem);
                lblStatus[i].ForeColor = ErrorColor;
            }
            else
            {
                lblStatus[i].Text = T("Wolves.Fortress.Slot.Ready", "جاهزة");
                lblStatus[i].ForeColor = ColorScheme.AccentColor;
            }
        }
    }

    private void chkEnabled_CheckedChanged(object sender, EventArgs e)
    {
        if (_updatingUI) return;
        FortressManager.Instance.Enabled = chkEnabled.Checked;
        FortressManager.Instance.SaveConfig();
    }

    private void chkAutoCast_CheckedChanged(object sender, EventArgs e)
    {
        if (_updatingUI) return;
        FortressManager.Instance.AutoCastWhenReady = chkAutoCast.Checked;
        FortressManager.Instance.SaveConfig();
    }

    private void cboChannel_SelectedIndexChanged(object sender, EventArgs e)
    {
        if (_updatingUI) return;
        FortressManager.Instance.ResponseChannel = cboChannel.SelectedIndex == 1 ? ChatType.Private : ChatType.Party;
        FortressManager.Instance.SaveConfig();
    }

    private void btnAutoFill_Click(object sender, EventArgs e)
    {
        FortressManager.Instance.AutoFillFromParty();
        UpdateUI();
    }

    private void SlotEnabled_Changed(int index)
    {
        if (_updatingUI || index < 0 || index >= 7) return;
        FortressManager.Instance.Slots[index].Enabled = chkSlotEnabled[index].Checked;
        FortressManager.Instance.SaveConfig();
    }

    private void SlotPlayer_Changed(int index)
    {
        if (_updatingUI || index < 0 || index >= 7) return;
        FortressManager.Instance.Slots[index].PlayerName = cboPlayer[index].Text.Trim();
        FortressManager.Instance.SaveConfig();
    }

    private void SlotSkill_Changed(int index)
    {
        if (_updatingUI || index < 0 || index >= 7) return;
        if (cboSkill[index].SelectedItem is SkillDropdownItem item)
        {
            FortressManager.Instance.Slots[index].SkillId = item.Id;
            FortressManager.Instance.Slots[index].SkillName = item.Id != 0 ? item.Name : string.Empty;
            FortressManager.Instance.SaveConfig();
        }
    }

    private void CastSlotBuff(int index)
    {
        if (index < 0 || index >= 7) return;
        var slotIndex = index + 1;
        FortressManager.Instance.HandleSlotTrigger(slotIndex, Game.Player?.Name ?? "Master", ChatType.Party);
    }

    private void LoadLogs()
    {
        txtLogs.Clear();
        foreach (var l in FortressManager.Instance.Logs)
        {
            txtLogs.AppendText(l + "\n");
        }
        txtLogs.ScrollToCaret();
    }

    private void AppendLog(string log)
    {
        txtLogs.AppendText(log + "\n");
        txtLogs.ScrollToCaret();
    }

    private void btnClearLogs_Click(object sender, EventArgs e)
    {
        FortressManager.Instance.Logs.Clear();
        txtLogs.Clear();
    }
}
