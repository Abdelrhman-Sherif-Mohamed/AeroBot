using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Forms;
using RSBot.Core;
using RSBot.Core.Client.ReferenceObjects;
using RSBot.Core.Components;
using RSBot.Core.Event;
using RSBot.Core.Objects;
using RSBot.Lure.Components;
using SDUI.Controls;

namespace RSBot.Lure.Views;

[ToolboxItem(false)]
public partial class Main : DoubleBufferedControl
{
    private bool _configLocked;

    public Main()
    {
        InitializeComponent();
        SubscribeEvents();
    }

    private void SubscribeEvents()
    {
        EventManager.SubscribeEvent("OnLoadCharacter", OnLoadCharacter);
    }

    private void OnLoadCharacter()
    {
        if (IsDisposed || Disposing || !Game.Ready)
            return;

        checkUseHowlingShout.Enabled = Game.Player != null && Game.Player.Race == ObjectCountry.Europe;
        checkNoHowlingAtCenter.Enabled = Game.Player != null && Game.Player.Race == ObjectCountry.Europe;

        LoadConfig();
    }

    private void LoadConfig()
    {
        if (_configLocked)
            return;

        _configLocked = true;

        bool isAr = Kernel.Language == "ar_AR";
        ApplyLanguage(isAr);

        lblPartyX.Text = LureConfig.Area.Position.X.ToString("0.0");
        lblPartyY.Text = LureConfig.Area.Position.Y.ToString("0.0");
        numPartyRadius.Value = Math.Max(5, LureConfig.Area.Radius > 0 ? LureConfig.Area.Radius : 50);
        numPartyWaitSeconds.Value = Math.Max(1, LureConfig.StayAtCenterForSeconds);
        txtWalkbackScript.Text = LureConfig.WalkscriptPath;

        checkWalkRandomly.Checked = LureConfig.WalkRandomly;

        checkUseHowlingShout.Checked = LureConfig.UseHowlingShout;
        checkNoHowlingAtCenter.Checked = LureConfig.NoHowlingAtCenter;
        checkUseNormalAttack.Checked = LureConfig.UseNormalAttack;
        checkUseAttackingSkills.Checked = LureConfig.UseAttackingSkills;

        checkStopPartyMemberDead.Checked = LureConfig.StopIfNumPartyMemberDead;
        numPartyMemberDead.Value = Math.Max(1, LureConfig.NumPartyMemberDead);

        checkStopPartyMember.Checked = LureConfig.StopIfNumPartyMember;
        numPartyMember.Value = Math.Max(1, LureConfig.NumPartyMember);

        checkStopMonsterType.Checked = LureConfig.StopIfNumMonsterType;
        numMonsterType.Value = Math.Max(0, LureConfig.NumMonsterType);

        comboMonsterType.SelectedIndex = LureConfig.SelectedMonsterType switch
        {
            MonsterRarity.General => 0,
            MonsterRarity.Champion => 1,
            MonsterRarity.Giant => 2,
            MonsterRarity.GeneralParty => 3,
            MonsterRarity.ChampionParty => 4,
            MonsterRarity.GiantParty => 5,
            MonsterRarity.Elite => 6,
            MonsterRarity.EliteStrong => 7,
            MonsterRarity.Unique => 8,
            MonsterRarity.Event => 9,
            _ => 0
        };

        RefreshSpotsList();

        _configLocked = false;
    }

    private void ApplyLanguage(bool isAr)
    {
        groupBoxParty.Text = isAr ? "مكان البارتي (منطقة العودة)" : "Party & Return Area";
        btnSetPartySpot.Text = isAr ? "تحديد مكان البارتي (الموقع الحالي)" : "Set Party Spot (Current)";
        lblPartyWaitTitle.Text = isAr ? "الانتظار عند البارتي (ثواني):" : "Wait at party (seconds):";
        lblWalkscriptTitle.Text = isAr ? "مسار السير للبارتي (Walkscript):" : "Script to location:";

        groupBoxLureSpots.Text = isAr ? "أماكن سحب الوحوش (Lure Spots)" : "Lure Locations";
        btnAddLureSpot.Text = isAr ? "+ إضافة موقع السحب (الموقع الحالي)" : "+ Add Current Location";
        lblSpotRadiusTitle.Text = isAr ? "نصف القطر:" : "Radius:";
        btnDeleteLureSpot.Text = isAr ? "حذف المحدد" : "Delete Selected";
        btnClearLureSpots.Text = isAr ? "مسح الكل" : "Clear All";
        checkWalkRandomly.Text = isAr ? "سحب عشوائي إذا لم يتم تحديد مواقع" : "Walk randomly if no spots added";

        groupBox3.Text = isAr ? "إعدادات السحب والمهارات" : "Settings";
        checkUseHowlingShout.Text = isAr ? "استخدام Howling Shout (محارب أوروبي فقط)" : "Cast howling shout (only EU)";
        checkNoHowlingAtCenter.Text = isAr ? "عدم تفعيل Howling Shout قرب البارتي" : "Don't cast near center/party";
        checkUseNormalAttack.Text = isAr ? "ضرب الوحوش بالضربة العادية لسحبها" : "Use normal attack to pull";
        checkUseAttackingSkills.Text = isAr ? "استخدام مهارة هجومية لسحب الوحوش" : "Use attacking skill to pull";
        checkStopPartyMemberDead.Text = isAr ? "إيقاف السحب إذا مات أعضاء >=" : "Stop if dead party members >=";
        checkStopPartyMember.Text = isAr ? "إيقاف السحب إذا قل أعضاء البارتي <=" : "Stop if num party members <=";
        checkStopMonsterType.Text = isAr ? "إيقاف السحب إذا زاد عدد وحوش >=" : "Stop if num monster of type >=";
    }

    private void RefreshSpotsList()
    {
        lstLureSpots.Items.Clear();
        var spots = LureConfig.Spots;
        for (int i = 0; i < spots.Count; i++)
        {
            var spot = spots[i];
            var item = new ListViewItem((i + 1).ToString());
            item.SubItems.Add(spot.X.ToString("0.0"));
            item.SubItems.Add(spot.Y.ToString("0.0"));
            item.SubItems.Add(spot.Radius.ToString());
            lstLureSpots.Items.Add(item);
        }
    }

    private void btnSetPartySpot_Click(object sender, EventArgs e)
    {
        if (Game.Player == null)
            return;

        var pos = Game.Player.Position;
        LureConfig.Area = new Area
        {
            Name = "Party",
            Position = pos,
            Radius = (int)numPartyRadius.Value
        };

        lblPartyX.Text = pos.X.ToString("0.0");
        lblPartyY.Text = pos.Y.ToString("0.0");
        Log.Notify($"[Lure] Party location set to X: {pos.X:0.0}, Y: {pos.Y:0.0}, Radius: {numPartyRadius.Value}");
    }

    private void btnAddLureSpot_Click(object sender, EventArgs e)
    {
        if (Game.Player == null)
            return;

        var pos = Game.Player.Position;
        var spots = LureConfig.Spots;
        var newSpot = new LureSpot
        {
            Region = pos.Region,
            XOffset = pos.XOffset,
            YOffset = pos.YOffset,
            ZOffset = pos.ZOffset,
            Radius = (int)numSpotRadius.Value
        };

        spots.Add(newSpot);
        LureConfig.Spots = spots;

        RefreshSpotsList();
        Log.Notify($"[Lure] Added lure spot #{spots.Count} at X: {newSpot.X:0.0}, Y: {newSpot.Y:0.0}, Radius: {newSpot.Radius}");
    }

    private void btnDeleteLureSpot_Click(object sender, EventArgs e)
    {
        if (lstLureSpots.SelectedIndices.Count == 0)
            return;

        int index = lstLureSpots.SelectedIndices[0];
        var spots = LureConfig.Spots;
        if (index >= 0 && index < spots.Count)
        {
            spots.RemoveAt(index);
            LureConfig.Spots = spots;
            RefreshSpotsList();
        }
    }

    private void btnClearLureSpots_Click(object sender, EventArgs e)
    {
        bool isAr = Kernel.Language == "ar_AR";
        var msg = isAr ? "هل أنت متأكد من مسح جميع أماكن السحب؟" : "Are you sure you want to clear all lure spots?";
        var title = isAr ? "تأكيد" : "Confirm";

        if (MessageBox.Show(msg, title, MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
            return;

        LureConfig.Spots = new List<LureSpot>();
        RefreshSpotsList();
    }

    private void OnSettingsChanged(object sender, EventArgs e)
    {
        if (_configLocked)
            return;

        _configLocked = true;

        LureConfig.Area = new Area
        {
            Name = "Party",
            Position = LureConfig.Area.Position,
            Radius = (int)numPartyRadius.Value
        };

        LureConfig.StayAtCenterForSeconds = (int)numPartyWaitSeconds.Value;
        LureConfig.WalkRandomly = checkWalkRandomly.Checked;
        LureConfig.UseNormalAttack = checkUseNormalAttack.Checked;
        LureConfig.UseHowlingShout = checkUseHowlingShout.Checked;
        LureConfig.NoHowlingAtCenter = checkNoHowlingAtCenter.Checked;
        LureConfig.UseAttackingSkills = checkUseAttackingSkills.Checked;

        LureConfig.StopIfNumMonsterType = checkStopMonsterType.Checked;
        LureConfig.StopIfNumPartyMemberDead = checkStopPartyMemberDead.Checked;
        LureConfig.StopIfNumPartyMember = checkStopPartyMember.Checked;
        LureConfig.NumMonsterType = (int)numMonsterType.Value;
        LureConfig.NumPartyMember = (int)numPartyMember.Value;
        LureConfig.NumPartyMemberDead = (int)numPartyMemberDead.Value;

        _configLocked = false;
    }

    private void btnBrowseWalkscript_Click(object sender, EventArgs e)
    {
        var fileBrowser = new OpenFileDialog
        {
            Title = "RSBot - Choose walkback script",
            Filter = "RSBot script (*.rbs)|*.rbs",
            Multiselect = false
        };

        if (fileBrowser.ShowDialog() != DialogResult.OK)
            return;

        txtWalkbackScript.Text = fileBrowser.FileName;
        LureConfig.WalkscriptPath = fileBrowser.FileName;
    }

    private void comboMonsterType_SelectedIndexChanged(object sender, EventArgs e)
    {
        LureConfig.SelectedMonsterType = comboMonsterType.SelectedIndex switch
        {
            0 => MonsterRarity.General,
            1 => MonsterRarity.Champion,
            2 => MonsterRarity.Giant,
            3 => MonsterRarity.GeneralParty,
            4 => MonsterRarity.ChampionParty,
            5 => MonsterRarity.GiantParty,
            6 => MonsterRarity.Elite,
            7 => MonsterRarity.EliteStrong,
            8 => MonsterRarity.Unique,
            9 => MonsterRarity.Event,
            _ => LureConfig.SelectedMonsterType
        };
    }

    private void Main_Load(object sender, EventArgs e)
    {
        OnLoadCharacter();
    }
}
