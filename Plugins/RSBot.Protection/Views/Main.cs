using System;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using RSBot.Core;
using RSBot.Core.Components;
using RSBot.Core.Event;
using RSBot.Core.Objects.Skill;
using RSBot.Protection.Components.Player;
using SDUI.Controls;
using CheckBox = SDUI.Controls.CheckBox;

namespace RSBot.Protection.Views;

[ToolboxItem(false)]
public partial class Main : DoubleBufferedControl
{
    public Main()
    {
        InitializeComponent();
        SubscribeEvents();
        ApplyProtectionLayout();
    }

    private bool _applyingProtectionLayout;

    protected override void OnResize(EventArgs e)
    {
        base.OnResize(e);
        ApplyProtectionLayout();
    }

    private int S(float value)
    {
        return (int)Math.Round(value * DeviceDpi / 96f);
    }

    private void Place(Control control, int x, int y, int width, int height)
    {
        if (control == null)
            return;

        control.AutoSize = false;
        control.SetBounds(S(x), S(y), Math.Max(1, S(width)), Math.Max(1, S(height)));
    }

    private void PlaceLabel(SDUI.Controls.Label label, int x, int y, int width, int height,
        ContentAlignment alignment)
    {
        if (label == null)
            return;

        label.AutoSize = false;
        label.TextAlign = alignment;
        label.SetBounds(S(x), S(y), Math.Max(1, S(width)), Math.Max(1, S(height)));
    }

    /// <summary>
    ///     Lays the Protection page out as two responsive columns. The designer
    ///     stored fixed coordinates for the English labels; the Arabic captions
    ///     are wider and the page is shown in a much larger surface, so the cards
    ///     are re-positioned to use the available width and keep every label
    ///     inside its card.
    /// </summary>
    private void ApplyProtectionLayout()
    {
        if (_applyingProtectionLayout || IsDisposed || groupHPMP == null)
            return;

        _applyingProtectionLayout = true;
        SuspendLayout();

        try
        {
            var scale = DeviceDpi / 96f;
            var width = Math.Max((int)(ClientSize.Width / scale), 1000);
            var height = Math.Max((int)(ClientSize.Height / scale), 600);

            AutoScroll = true;
            AutoScrollMinSize = new Size(S(width), S(height));

            // Child coordinates below are relative to the card client area; the
            // native group box title is painted separately by SDUI.GroupBox.
            groupHPMP.Padding = Padding.Empty;
            groupBadStatus.Padding = Padding.Empty;
            groupPet.Padding = Padding.Empty;
            groupBackTown.Padding = Padding.Empty;
            groupStatPoints.Padding = Padding.Empty;

            const int margin = 14;
            const int gap = 14;
            const int top = margin;
            var contentW = width - margin * 2;
            var rightW = Math.Min(460, Math.Max(340, (int)(contentW * 0.40f)));
            var leftW = contentW - rightW - gap;
            var leftX = margin;
            var rightX = leftX + leftW + gap;

            // ---- Left column: recovery, bad status and pet --------------------
            const int hpmpH = 232;
            const int badH = 100;
            const int petH = 196;
            Place(groupHPMP, leftX, top, leftW, hpmpH);
            Place(groupBadStatus, leftX, top + hpmpH + gap, leftW, badH);
            var petY = top + hpmpH + gap + badH + gap;
            Place(groupPet, leftX, petY, leftW, petH);

            // Shared grid: checkbox | < | value | % | skill combo.
            const int rowH = 26;
            const int rowGap = 32;
            const int numW = 90;
            const int pctW = 24;
            const int ltW = 16;
            var comboW = Math.Min(160, Math.Max(120, (int)(leftW * 0.235f)));
            var comboX = leftW - 16 - comboW;
            var pctX = comboX - 12 - pctW;
            var numX = pctX - 6 - numW;
            var ltX = numX - 4 - ltW;
            var checkW = ltX - 8 - 16;

            var rowY = 32;
            PlaceRow(checkUseHPPotionsPlayer, label1, numPlayerHPPotionMin, label2, rowY,
                checkW, ltX, numX, pctX, numW, pctW);
            rowY += rowGap;
            PlaceRow(checkUseMPPotionsPlayer, label4, numPlayerMPPotionMin, label3, rowY,
                checkW, ltX, numX, pctX, numW, pctW);
            rowY += rowGap;
            PlaceRow(checkUseVigorHP, label6, numPlayerHPVigorPotionMin, label5, rowY,
                checkW, ltX, numX, pctX, numW, pctW);
            rowY += rowGap;
            PlaceRow(checkUseVigorMP, label8, numPlayerMPVigorPotionMin, label7, rowY,
                checkW, ltX, numX, pctX, numW, pctW);
            rowY += rowGap;
            PlaceRow(checkUseSkillHP, label10, numPlayerSkillHPMin, label9, rowY,
                checkW, ltX, numX, pctX, numW, pctW);
            Place(comboSkillPlayerHP, comboX, rowY - 1, comboW, 28);
            rowY += rowGap;
            PlaceRow(checkUseSkillMP, label12, numPlayerSkillMPMin, label11, rowY,
                checkW, ltX, numX, pctX, numW, pctW);
            Place(comboSkillPlayerMP, comboX, rowY - 1, comboW, 28);

            PlaceLabel(label17, comboX, 132, comboW, 20, ContentAlignment.MiddleLeft);

            // Bad status: the skill picker occupies the same column as the
            // recovery pickers, so the two cards line up visually.
            Place(checkUseUniversalPills, 16, 26, checkW, rowH);
            Place(checkUseBadStatusSkill, 16, 54, checkW, rowH);
            PlaceLabel(label18, comboX, 29, comboW, 20, ContentAlignment.MiddleLeft);
            Place(comboSkillBadStatus, comboX, 47, comboW, 28);

            // Pet recovery follows the same value grid as the player rows.
            rowY = 32;
            PlaceRow(checkUsePetHP, label16, numPetMinHP, label15, rowY,
                checkW, ltX, numX, pctX, numW, pctW);
            rowY += rowGap;
            PlaceRow(checkUseHGP, label14, numPetMinHGP, label13, rowY,
                checkW, ltX, numX, pctX, numW, pctW);
            Place(checkUseAbnormalStatePotion, 16, 96, leftW - 32, rowH);
            Place(checkReviveAttackPet, 16, 128, leftW - 32, rowH);
            Place(checkAutoSummonAttackPet, 16, 160, leftW - 32, rowH);

            // ---- Right column: return to town and stat points -----------------
            const int backH = 292;
            const int statH = 244;
            Place(groupBackTown, rightX, top, rightW, backH);
            Place(groupStatPoints, rightX, top + backH + gap, rightW, statH);

            const int deadNumW = 90;
            var deadNumX = rightW - 16 - 55 - 6 - deadNumW;
            Place(checkDead, 16, 32, deadNumX - 26, 28);
            Place(numDeadTimeout, deadNumX, 34, deadNumW, 25);
            PlaceLabel(label21, deadNumX + deadNumW + 6, 36, 55, 22, ContentAlignment.MiddleLeft);

            var townRowY = 64;
            Place(checkStopBotOnReturnToTown, 16, townRowY, rightW - 32, 28);
            townRowY += 27;
            Place(checkNoArrows, 16, townRowY, rightW - 32, 28);
            townRowY += 27;
            Place(checkInventory, 16, townRowY, rightW - 32, 28);
            townRowY += 27;
            Place(checkFullPetInventory, 16, townRowY, rightW - 32, 28);
            townRowY += 27;
            Place(checkNoHPPotions, 16, townRowY, rightW - 32, 28);
            townRowY += 27;
            Place(checkNoMPPotions, 16, townRowY, rightW - 32, 28);
            townRowY += 27;
            Place(checkDurability, 16, townRowY, rightW - 32, 28);
            townRowY += 27;
            Place(checkLevelUp, 16, townRowY, rightW - 32, 28);

            const int statNumW = 90;
            var statNumX = rightW - 16 - statNumW;
            var statCheckW = statNumX - 16 - 12;
            Place(checkIncInt, 16, 34, statCheckW, 30);
            Place(numIncInt, statNumX, 36, statNumW, 25);
            Place(checkIncStr, 16, 78, statCheckW, 30);
            Place(numIncStr, statNumX, 80, statNumW, 25);
            Place(checkIncBotStopped, 16, 122, rightW - 32, 28);
            Place(buttonRun, 16, 168, rightW - 32, 34);

            var noteY = Math.Max(petY + petH, top + backH + gap + statH) + 6;
            PlaceLabel(label22, leftX, noteY, contentW, 22, ContentAlignment.MiddleLeft);
        }
        finally
        {
            ResumeLayout(true);
            _applyingProtectionLayout = false;
        }
    }

    private void PlaceRow(CheckBox check, SDUI.Controls.Label lessThan, NumUpDown value,
        SDUI.Controls.Label percent, int y, int checkW, int ltX, int numX, int pctX,
        int numW, int pctW)
    {
        Place(check, 16, y, checkW, 26);
        PlaceLabel(lessThan, ltX, y + 1, 16, 24, ContentAlignment.MiddleLeft);
        Place(value, numX, y + 1, numW, 25);
        PlaceLabel(percent, pctX, y + 1, pctW, 24, ContentAlignment.MiddleLeft);
    }

    /// <summary>
    ///     Localizes the Run/Cancel caption instead of falling back to English
    ///     when the state changes.
    /// </summary>
    private void UpdateRunButtonCaption()
    {
        var key = _statIncreaseRunning ? "buttonRunCancel" : "buttonRun";
        buttonRun.Text = LanguageManager.GetLangBySpecificKey(
            "RSBot.Protection",
            $"RSBot.Protection.Main.GroupBox.{key}",
            _statIncreaseRunning ? "Cancel" : "Run");
    }

    /// <summary>
    ///     Subscribes the events.
    /// </summary>
    private void SubscribeEvents()
    {
        EventManager.SubscribeEvent("OnLoadCharacter", OnLoadCharacter);

        EventManager.SubscribeEvent("OnSkillLearned", new Action<SkillInfo>(OnSkillLearned));
        EventManager.SubscribeEvent("OnSkillUpgraded", new Action<SkillInfo, SkillInfo>(OnSkillUpgraded));

        EventManager.SubscribeEvent("OnIncreaseStrength", OnIncreaseStat);
        EventManager.SubscribeEvent("OnIncreaseIntelligence", OnIncreaseStat);
    }

    /// <summary>
    ///     Loads the settings.
    /// </summary>
    private void LoadSettings()
    {
        const string key = "RSBot.Protection.";

        foreach (var checkbox in groupHPMP.Controls.OfType<CheckBox>())
            checkbox.Checked = PlayerConfig.Get(key + checkbox.Name, checkbox.Checked);

        foreach (var num in groupHPMP.Controls.OfType<NumUpDown>())
            num.Value = PlayerConfig.Get(key + num.Name, num.Value);

        foreach (var checkbox in groupBadStatus.Controls.OfType<CheckBox>())
            checkbox.Checked = PlayerConfig.Get(key + checkbox.Name, checkbox.Checked);

        foreach (var checkbox in groupPet.Controls.OfType<CheckBox>())
            checkbox.Checked = PlayerConfig.Get(key + checkbox.Name, checkbox.Checked);

        foreach (var num in groupPet.Controls.OfType<NumUpDown>())
            num.Value = PlayerConfig.Get(key + num.Name, num.Value);

        foreach (var checkbox in groupBackTown.Controls.OfType<CheckBox>())
            checkbox.Checked = PlayerConfig.Get(key + checkbox.Name, checkbox.Checked);

        foreach (var num in groupBackTown.Controls.OfType<NumUpDown>())
            num.Value = PlayerConfig.Get(key + num.Name, num.Value);

        foreach (var checkbox in groupStatPoints.Controls.OfType<CheckBox>())
            checkbox.Checked = PlayerConfig.Get(key + checkbox.Name, checkbox.Checked);

        foreach (var num in groupStatPoints.Controls.OfType<NumUpDown>())
            num.Value = PlayerConfig.Get(key + num.Name, num.Value);
    }

    /// <summary>
    ///     Saves the settings.
    /// </summary>
    private void ApplySettings()
    {
        const string key = "RSBot.Protection.";
        foreach (var checkbox in groupHPMP.Controls.OfType<CheckBox>())
            PlayerConfig.Set(key + checkbox.Name, checkbox.Checked);

        foreach (var num in groupHPMP.Controls.OfType<NumUpDown>())
            PlayerConfig.Set(key + num.Name, num.Value);

        foreach (var checkbox in groupBadStatus.Controls.OfType<CheckBox>())
            PlayerConfig.Set(key + checkbox.Name, checkbox.Checked);

        foreach (var checkbox in groupPet.Controls.OfType<CheckBox>())
            PlayerConfig.Set(key + checkbox.Name, checkbox.Checked);

        foreach (var num in groupPet.Controls.OfType<NumUpDown>())
            PlayerConfig.Set(key + num.Name, num.Value);

        foreach (var checkbox in groupBackTown.Controls.OfType<CheckBox>())
            PlayerConfig.Set(key + checkbox.Name, checkbox.Checked);

        foreach (var num in groupBackTown.Controls.OfType<NumUpDown>())
            PlayerConfig.Set(key + num.Name, num.Value);

        foreach (var checkbox in groupStatPoints.Controls.OfType<CheckBox>())
            PlayerConfig.Set(key + checkbox.Name, checkbox.Checked);

        foreach (var num in groupStatPoints.Controls.OfType<NumUpDown>())
            PlayerConfig.Set(key + num.Name, num.Value);

        SkillInfo skill = null;

        skill = comboSkillPlayerHP.SelectedItem as SkillInfo;
        PlayerConfig.Set(key + "HpSkill", skill?.Id);

        skill = comboSkillPlayerMP.SelectedItem as SkillInfo;
        PlayerConfig.Set(key + "MpSkill", skill?.Id);

        skill = comboSkillBadStatus.SelectedItem as SkillInfo;
        PlayerConfig.Set(key + "BadStatusSkill", skill?.Id);

        skill = null;
    }

    /// <summary>
    ///     Refreshes the skills.
    /// </summary>
    private void RefreshSkills()
    {
        _skillSettingsLoaded = false;

        comboSkillBadStatus.Items.Clear();
        comboSkillPlayerHP.Items.Clear();
        comboSkillPlayerMP.Items.Clear();

        foreach (var skill in Game.Player.Skills.KnownSkills)
        {
            if (!skill.Enabled)
                continue;

            if (skill.IsPassive)
                continue;

            if (skill.Record.Target_Required && !skill.Record.TargetGroup_Self)
                continue;

            // TODO: Check is the cure skill?
            var index = comboSkillBadStatus.Items.Add(skill);
            var skillId = PlayerConfig.Get<uint>("RSBot.Protection.BadStatusSkill");
            if (skillId == skill.Id)
                comboSkillBadStatus.SelectedIndex = index;

            // TODO: Check is the hp skill?
            index = comboSkillPlayerHP.Items.Add(skill);
            skillId = PlayerConfig.Get<uint>("RSBot.Protection.HpSkill");
            if (skillId == skill.Id)
                comboSkillPlayerHP.SelectedIndex = index;

            // TODO: Check is the mp skill?
            index = comboSkillPlayerMP.Items.Add(skill);
            skillId = PlayerConfig.Get<uint>("RSBot.Protection.MpSkill");
            if (skillId == skill.Id)
                comboSkillPlayerMP.SelectedIndex = index;
        }

        _skillSettingsLoaded = true;
    }

    /// <summary>
    ///     Handles the CheckedChanged event of the settings control.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
    private void settings_CheckedChanged(object sender, EventArgs e)
    {
        if (_settingsLoaded)
            ApplySettings();
    }

    /// <summary>
    ///     Handles the ValueChanged event of the numSettings control.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
    private void numSettings_ValueChanged(object sender, EventArgs e)
    {
        if (_settingsLoaded)
            ApplySettings();
    }

    /// <summary>
    ///     Handles the SelectedIndexChanged event of the comboSkill control.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
    private void comboSkill_SelectedIndexChanged(object sender, EventArgs e)
    {
        if (_skillSettingsLoaded)
            ApplySettings();
    }

    /// <summary>
    ///     Call after skill learned
    /// </summary>
    /// <param name="skill">The learned skill.</param>
    private void OnSkillLearned(SkillInfo skill)
    {
        RefreshSkills();
    }

    /// <summary>
    ///     Call after skill learned
    /// </summary>
    /// <param name="skill">The learned skill.</param>
    private void OnSkillUpgraded(SkillInfo oldSkill, SkillInfo newSkill)
    {
        // TODO: Update old skill ids in config
    }

    /// <summary>
    /// </summary>
    private void OnLoadCharacter()
    {
        RefreshSkills();
    }

    /// <summary>
    ///     Re-calculates the max points of the Str numeric
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void numIncInt_ValueChanged(object sender, EventArgs e)
    {
        numIncStr.Maximum = 3 - numIncInt.Value;

        PlayerConfig.Set("RSBot.Protection.numIncInt", numIncInt.Value);
    }

    /// <summary>
    ///     Re-calculates the max points of the Int numeric
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void numIncStr_ValueChanged(object sender, EventArgs e)
    {
        numIncInt.Maximum = 3 - numIncStr.Value;

        PlayerConfig.Set("RSBot.Protection.numIncStr", numIncStr.Value);
    }

    private void OnIncreaseStat()
    {
        if (Game.Player.StatPoints < numIncInt.Value + numIncStr.Value)
        {
            _statIncreaseRunning = false;
            UpdateRunButtonCaption();
        }
    }

    private void buttonRun_Click(object sender, EventArgs e)
    {
        if (_statIncreaseRunning)
        {
            _statIncreaseRunning = false;
            UpdateRunButtonCaption();

            StatPointsHandler.CancellationRequested = true;

            return;
        }

        StatPointsHandler.CancellationRequested = false;
        var stepSize = numIncInt.Value + numIncStr.Value;

        if (stepSize == 0) return;
        //Only run if at least 3 stat points can be increased
        if (Game.Player.StatPoints < stepSize)
            return;

        var availableSteps = Math.Floor(Game.Player.StatPoints / stepSize);

        if (Game.Player.StatPoints == stepSize)
            availableSteps = 1;

        if (availableSteps == 0) return;

        Task.Run(() => StatPointsHandler.IncreaseStatPoints((int)availableSteps));

        _statIncreaseRunning = true;

        UpdateRunButtonCaption();
    }

    /// <summary>
    ///     Occurs before Main form is displayed.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void Main_Load(object sender, EventArgs e)
    {
        _settingsLoaded = false;
        LoadSettings();
        _settingsLoaded = true;
        UpdateRunButtonCaption();
    }

    #region Fields

    private bool _settingsLoaded;
    private bool _skillSettingsLoaded;
    private bool _statIncreaseRunning;

    #endregion Fields
}