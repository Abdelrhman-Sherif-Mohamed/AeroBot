using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using RSBot.Core;
using RSBot.Core.Components;
using RSBot.Core.Event;
using RSBot.Core.Objects;
using RSBot.Default.Views.Dialogs;
using SDUI;
using SDUI.Controls;
using CheckBox = SDUI.Controls.CheckBox;

namespace RSBot.Default.Views;

[ToolboxItem(false)]
public partial class Main : DoubleBufferedControl
{
    private const int ScriptRecorderOwnerId = 2000;

    #region Fields

    private bool _settingsLoaded;

    #endregion Fields

    /// <summary>
    ///     Initializes a new instance of the <see cref="Main" /> class.
    /// </summary>
    public Main()
    {
        InitializeComponent();
        SubscribeEvents();

        // Runtime checkbox: skip the town script (visiting all NPCs) so the bot goes
        // straight to the training area. Persisted via PlayerConfig (RSBot.Training.checkSkipTownScript).
        var skipTownCheckBox = new CheckBox
        {
            Name = "checkSkipTownScript",
            Text = "Skip town script (don't visit NPCs, go to combat)",
            AutoSize = false,
            Size = new System.Drawing.Size(360, 30),
            Location = new System.Drawing.Point(16, 170),
            Checked = false
        };
        groupBoxWalkback.Controls.Add(skipTownCheckBox);

        // Ensure Avoidance / Preference ListView matches the active theme palette
        lvAvoidance.BackColor = SDUI.ColorScheme.TableBackColor;
        lvAvoidance.ForeColor = SDUI.ColorScheme.ForeColor;

        lvAvoidance.Items[0].Tag = MonsterRarity.General;
        lvAvoidance.Items[1].Tag = MonsterRarity.Champion;
        lvAvoidance.Items[2].Tag = MonsterRarity.Giant;
        lvAvoidance.Items[3].Tag = MonsterRarity.GeneralParty;
        lvAvoidance.Items[4].Tag = MonsterRarity.ChampionParty;
        lvAvoidance.Items[5].Tag = MonsterRarity.GiantParty;
        lvAvoidance.Items[6].Tag = MonsterRarity.Unique | MonsterRarity.Unique2;
        lvAvoidance.Items[7].Tag = MonsterRarity.EliteStrong;
        lvAvoidance.Items[8].Tag = MonsterRarity.Elite;
        lvAvoidance.Items[9].Tag = MonsterRarity.Event;

        ApplyTrainingLayout();
    }

    private bool _applyingTrainingLayout;

    protected override void OnResize(EventArgs e)
    {
        base.OnResize(e);
        ApplyTrainingLayout();
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

    /// <summary>
    ///     Lays the Training page out as two columns. The card sizes are driven
    ///     by the available surface instead of the fixed WinForms designer
    ///     coordinates, so the translated Arabic labels always keep their room
    ///     and the Avoidance list fills the free height below the area card.
    /// </summary>
    private void ApplyTrainingLayout()
    {
        if (_applyingTrainingLayout || IsDisposed || groupBoxArea == null)
            return;

        _applyingTrainingLayout = true;
        SuspendLayout();

        try
        {
            var scale = DeviceDpi / 96f;
            var width = Math.Max((int)(ClientSize.Width / scale), 800);
            var height = Math.Max((int)(ClientSize.Height / scale), 570);

            AutoScroll = true;
            AutoScrollMinSize = new Size(S(width), S(height));

            // Child coordinates below are relative to the card client area; the
            // native group box title is painted separately by SDUI.GroupBox.
            groupBoxArea.Padding = Padding.Empty;
            groupBox2.Padding = Padding.Empty;
            groupBoxWalkback.Padding = Padding.Empty;
            groupBoxBerserk.Padding = Padding.Empty;
            groupBoxAdvanced.Padding = Padding.Empty;

            const int margin = 14;
            const int gap = 14;
            var contentW = width - margin * 2;
            var leftW = Math.Min(400, Math.Max(320, (int)(contentW * 0.35f)));
            var rightW = contentW - leftW - gap;
            var leftX = margin;
            var rightX = leftX + leftW + gap;
            var top = margin;
            var bottom = height - margin;

            // ---- Left column: training area and avoidance list ---------------
            const int areaH = 238;
            Place(groupBoxArea, leftX, top, leftW, areaH);

            const int fieldLabelW = 84;
            var fieldLabelX = 16;
            var fieldX = fieldLabelX + fieldLabelW + 8;
            var selectW = 34;
            var selectX = leftW - 16 - selectW;
            var fieldW = selectX - 8 - fieldX;

            Place(label1, fieldLabelX, 32, fieldLabelW, 28);
            label1.TextAlign = ContentAlignment.MiddleRight;
            Place(txtXCoord, fieldX, 32, fieldW, 28);

            Place(label2, fieldLabelX, 62, fieldLabelW, 28);
            label2.TextAlign = ContentAlignment.MiddleRight;
            Place(txtYCoord, fieldX, 62, fieldW, 28);

            Place(label3, fieldLabelX, 92, fieldLabelW, 28);
            label3.TextAlign = ContentAlignment.MiddleRight;
            Place(txtRadius, fieldX, 92, fieldW, 28);

            Place(buttonSelectTrainingArea, selectX, 91, selectW, 30);
            Place(btnGetCurrent, fieldX, 126, Math.Min(150, leftW - fieldX - 16), 30);

            label5.BackColor = Color.FromArgb(70, ColorScheme.BorderColor);
            Place(label5, 16, 160, leftW - 32, 1);

            Place(label6, 16, 164, leftW - 32, 30);
            label6.TextAlign = ContentAlignment.MiddleLeft;

            var radioW = (leftW - 32 - 10) / 2;
            Place(radioCenter, 16, 198, radioW, 30);
            Place(radioWalkAround, 16 + radioW + 10, 198, radioW, 30);

            var avoidY = top + areaH + gap;
            Place(groupBox2, leftX, avoidY, leftW, Math.Max(180, bottom - avoidY));

            lvAvoidance.Dock = DockStyle.None;
            const int listInset = 14;
            const int listTop = 34;
            var listW = leftW - listInset * 2;
            Place(lvAvoidance, listInset, listTop, listW,
                Math.Max(100, groupBox2.Height - listTop - 14));

            if (lvAvoidance.Columns.Count > 0)
                lvAvoidance.Columns[0].Width = Math.Max(80, listW - 8);

            // ---- Right column: walkback, berserk and advanced settings --------
            const int backH = 190;
            Place(groupBoxWalkback, rightX, top, rightW, backH);

            Place(label4, 16, 32, 112, 28);
            label4.TextAlign = ContentAlignment.MiddleLeft;
            Place(linkRecord, 134, 32, 64, 28);
            linkRecord.TextAlign = ContentAlignment.MiddleLeft;
            linkRecord.LinkColor = ColorScheme.AccentColor;

            const int browseW = 80;
            var browseX = rightW - 16 - browseW;
            Place(txtWalkscript, 16, 64, browseX - 8 - 16, 28);
            Place(btnBrowse, browseX, 63, browseW, 30);

            var optionW = (rightW - 32 - 12) / 2;
            Place(checkUseMount, 16, 104, optionW, 30);
            Place(checkCastBuffs, 16 + optionW + 12, 104, optionW, 30);
            Place(checkUseSpeedDrug, 16, 138, optionW, 30);
            Place(checkBoxUseReverse, 16 + optionW + 12, 138, optionW, 30);

            var berserkY = top + backH + gap;
            const int berserkH = 200;
            Place(groupBoxBerserk, rightX, berserkY, rightW, berserkH);

            Place(checkBerzerkWhenFull, 16, 32, rightW - 32, 30);

            const int numW = 74;
            const int monstersW = 72;
            var amountW = Math.Min(360, rightW - 32 - numW - monstersW - 16);
            Place(checkBerzerkMonsterAmount, 16, 68, amountW, 30);
            var numX = 16 + amountW + 8;
            Place(numBerzerkMonsterAmount, numX, 71, numW, 26);
            Place(label7, numX + numW + 8, 68, monstersW, 30);
            label7.TextAlign = ContentAlignment.MiddleLeft;

            Place(checkBerzerkAvoidance, 16, 104, rightW - 32, 30);
            Place(checkBerserkOnMonsterRarity, 16, 140, rightW - 32, 30);

            var advancedY = berserkY + berserkH + gap;
            var advancedH = Math.Min(240, Math.Max(130, bottom - advancedY));
            Place(groupBoxAdvanced, rightX, advancedY, rightW, advancedH);

            Place(checkBoxDimensionPillar, 16, 32, rightW - 32, 30);
            Place(checkAttackWeakerFirst, 16, 68, rightW - 64, 30);
            Place(linkAttackWeakerMobsHelp, rightW - 16 - 24, 68, 24, 30);
            linkAttackWeakerMobsHelp.TextAlign = ContentAlignment.MiddleCenter;
            linkAttackWeakerMobsHelp.LinkColor = ColorScheme.AccentColor;

            TranslateAvoidanceGroups();
        }
        finally
        {
            ResumeLayout(true);
            _applyingTrainingLayout = false;
        }
    }

    /// <summary>
    ///     The rarity group headers and the list context menu are not regular
    ///     controls, so LanguageManager cannot translate them automatically.
    /// </summary>
    private void TranslateAvoidanceGroups()
    {
        var avoid = LanguageManager.GetLangBySpecificKey("RSBot.Default", "AvoidanceGroupAvoid", "Avoid");
        var prefer = LanguageManager.GetLangBySpecificKey("RSBot.Default", "AvoidanceGroupPrefer", "Prefer");
        var berserk = LanguageManager.GetLangBySpecificKey("RSBot.Default", "AvoidanceGroupBerserk", "Berserk");
        var none = LanguageManager.GetLangBySpecificKey("RSBot.Default", "AvoidanceGroupNone", "No custom behavior");

        lvAvoidance.Groups["grpAvoid"].Header = avoid;
        lvAvoidance.Groups["grpPrefer"].Header = prefer;
        lvAvoidance.Groups["grpBerserk"].Header = berserk;
        lvAvoidance.Groups["grpNone"].Header = none;

        btnAvoid.Text = avoid;
        btnPrefer.Text = prefer;
        btnBerserk.Text = berserk;
        btnNoCustomBehavior.Text = none;
    }

    /// <summary>
    ///     Subscribes the events.
    /// </summary>
    private void SubscribeEvents()
    {
        EventManager.SubscribeEvent("OnLoadCharacter", OnLoadCharacter);
        EventManager.SubscribeEvent("OnSetTrainingArea", OnSetTrainingArea);
        EventManager.SubscribeEvent("OnSaveScript", new Action<int, string>(OnSaveScript));
    }

    /// <summary>
    ///     Loads the settings.
    /// </summary>
    private void LoadSettings()
    {
        const string key = "RSBot.Training.";

        foreach (var checkbox in groupBoxAdvanced.Controls.OfType<CheckBox>())
            checkbox.Checked = PlayerConfig.Get(key + checkbox.Name, checkbox.Checked);

        foreach (var checkbox in groupBoxBerserk.Controls.OfType<CheckBox>())
            checkbox.Checked = PlayerConfig.Get(key + checkbox.Name, checkbox.Checked);

        foreach (var num in groupBoxBerserk.Controls.OfType<NumUpDown>())
            num.Value = PlayerConfig.Get(key + num.Name, num.Value);

        foreach (var checkbox in groupBoxWalkback.Controls.OfType<CheckBox>())
            checkbox.Checked = PlayerConfig.Get(key + checkbox.Name, checkbox.Checked);

        radioCenter.Checked = PlayerConfig.Get(key + radioCenter.Name, false);
        radioWalkAround.Checked = PlayerConfig.Get(key + radioWalkAround.Name, true);

        LoadAvoidance();
    }

    /// <summary>
    ///     Saves the settings.
    /// </summary>
    private void ApplySettings()
    {
        const string key = "RSBot.Training.";

        foreach (var checkbox in groupBoxAdvanced.Controls.OfType<CheckBox>())
            PlayerConfig.Set(key + checkbox.Name, checkbox.Checked);

        foreach (var checkbox in groupBoxBerserk.Controls.OfType<CheckBox>())
            PlayerConfig.Set(key + checkbox.Name, checkbox.Checked);

        foreach (var num in groupBoxBerserk.Controls.OfType<NumUpDown>())
            PlayerConfig.Set(key + num.Name, num.Value);

        foreach (var checkbox in groupBoxWalkback.Controls.OfType<CheckBox>())
            PlayerConfig.Set(key + checkbox.Name, checkbox.Checked);

        PlayerConfig.Set(key + radioCenter.Name, radioCenter.Checked);
        PlayerConfig.Set(key + radioWalkAround.Name, radioWalkAround.Checked);
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
    ///     Called when the script recorder saves a script.
    /// </summary>
    /// <param name="ownerId"></param>
    /// <param name="path"></param>
    private void OnSaveScript(int ownerId, string path)
    {
        if (IsDisposed || Disposing)
            return;

        if (ownerId != ScriptRecorderOwnerId)
            return;

        txtWalkscript.Text = path;
        PlayerConfig.Set("RSBot.Walkback.File", txtWalkscript.Text);
    }

    /// <summary>
    ///     Called when the training area has been set to a certain location. Will append a new script command "area" and
    ///     update the UI.
    /// </summary>
    private void OnSetTrainingArea()
    {
        if (IsDisposed || Disposing)
            return;

        var area = Kernel.Bot.Botbase.Area;

        txtXCoord.Text = area.Position.X.ToString("0.0");
        txtYCoord.Text = area.Position.Y.ToString("0.0");
        txtRadius.Text = area.Radius.ToString();

        EventManager.FireEvent("AppendScriptCommand", area.GetScriptLine());
    }

    /// <summary>
    ///     Saves the avoidance.
    /// </summary>
    private void SaveAvoidance()
    {
        var avoid = new List<MonsterRarity>();
        var prefer = new List<MonsterRarity>();
        var berserk = new List<MonsterRarity>(); 
        foreach (ListViewItem item in lvAvoidance.Items)
            if (item.Group == lvAvoidance.Groups["grpAvoid"])
                avoid.Add((MonsterRarity)item.Tag);
            else if (item.Group == lvAvoidance.Groups["grpPrefer"])
                prefer.Add((MonsterRarity)item.Tag);
            else if (item.Group == lvAvoidance.Groups["grpBerserk"])
                berserk.Add((MonsterRarity)item.Tag);

        PlayerConfig.SetArray("RSBot.Avoidance.Avoid", avoid);
        PlayerConfig.SetArray("RSBot.Avoidance.Prefer", prefer);
        PlayerConfig.SetArray("RSBot.Avoidance.Berserk", berserk);
    }

    /// <summary>
    ///     Loads the avoidance.
    /// </summary>
    private void LoadAvoidance()
    {
        var prefer = PlayerConfig.GetEnums<MonsterRarity>("RSBot.Avoidance.Prefer").ToLookup(p => "Prefer", p => p);
        var avoid = PlayerConfig.GetEnums<MonsterRarity>("RSBot.Avoidance.Avoid").ToLookup(p => "Avoid", p => p);
        var berserk = PlayerConfig.GetEnums<MonsterRarity>("RSBot.Avoidance.Berserk").ToLookup(p => "Berserk", p => p);

        foreach (var group in avoid.Union(prefer).Union(berserk))
        foreach (var item in group)
        {
            var listViewItem = lvAvoidance.Items.Cast<ListViewItem>()
                .FirstOrDefault(p => ((MonsterRarity)p.Tag & item) == item);
            if (listViewItem == null)
                continue;

            listViewItem.Group = lvAvoidance.Groups[$"grp{group.Key}"];
        }
    }

    /// <summary>
    ///     Handles the Click event of the btnGetCurrent control.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
    private void btnGetCurrent_Click(object sender, EventArgs e)
    {
        var pos = Game.Player.Position;

        PlayerConfig.Set("RSBot.Area.Region", pos.Region);
        PlayerConfig.Set("RSBot.Area.X", pos.XOffset);
        PlayerConfig.Set("RSBot.Area.Y", pos.YOffset);
        PlayerConfig.Set("RSBot.Area.Z", pos.ZOffset);

        EventManager.FireEvent("OnSetTrainingArea");
    }

    /// <summary>
    ///     Handles the TextChanged event of the txtXCoord control.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
    private void txtXCoord_TextChanged(object sender, EventArgs e)
    {
        if (!float.TryParse(txtXCoord.Text, out var result))
            return;
    }

    /// <summary>
    ///     Handles the TextChanged event of the txtYCoord control.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
    private void txtYCoord_TextChanged(object sender, EventArgs e)
    {
        if (!float.TryParse(txtYCoord.Text, out var result))
            return;
    }

    /// <summary>
    ///     Handles the TextChanged event of the txtRadius control.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
    private void txtRadius_TextChanged(object sender, EventArgs e)
    {
        if (!int.TryParse(txtRadius.Text, out var result))
            return;

        PlayerConfig.Set("RSBot.Area.Radius", result);
    }

    /// <summary>
    ///     Handles the Click event of the btnBrowse control.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
    private void btnBrowse_Click(object sender, EventArgs e)
    {
        var diag = new OpenFileDialog
        {
            Filter = @"RSBot Bot script (*.rbs)|*.rbs",
            Title = @"Browse for a walkback script"
        };

        if (diag.ShowDialog() != DialogResult.OK) return;

        txtWalkscript.Text = diag.FileName;
        PlayerConfig.Set("RSBot.Walkback.File", txtWalkscript.Text);
    }

    /// <summary>
    ///     Handles the Click event of the btnAvoid control.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="System.EventArgs" /> instance containing the event data.</param>
    private void btnAvoid_Click(object sender, EventArgs e)
    {
        if (lvAvoidance.SelectedItems.Count <= 0)
            return;

        foreach (ListViewItem item in lvAvoidance.SelectedItems)
            item.Group = lvAvoidance.Groups["grpAvoid"];

        SaveAvoidance();
    }

    /// <summary>
    ///     Handles the Click event of the btnPrefer control.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
    private void btnPrefer_Click(object sender, EventArgs e)
    {
        if (lvAvoidance.SelectedItems.Count <= 0) return;
        foreach (ListViewItem item in lvAvoidance.SelectedItems)
            item.Group = lvAvoidance.Groups["grpPrefer"];

        SaveAvoidance();
    }

    /// <summary>
    ///     Handles the Click event of the btnBerserk control.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
    private void btnBerserk_Click(object sender, EventArgs e)
    {
        if (lvAvoidance.SelectedItems.Count <= 0) return;
        foreach (ListViewItem item in lvAvoidance.SelectedItems)
            item.Group = lvAvoidance.Groups["grpBerserk"];

        SaveAvoidance();
    }

    /// <summary>
    ///     Handles the Click event of the btnNoCustomBehavior control.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
    private void btnNoCustomBehavior_Click(object sender, EventArgs e)
    {
        if (lvAvoidance.SelectedItems.Count <= 0) return;
        foreach (ListViewItem item in lvAvoidance.SelectedItems)
            item.Group = lvAvoidance.Groups["grpNone"];

        SaveAvoidance();
    }

    /// <summary>
    /// </summary>
    private void OnLoadCharacter()
    {
        if (IsDisposed || Disposing)
            return;

        var area = Kernel.Bot.Botbase.Area;
        //Training Area
        txtXCoord.Text = area.Position.X.ToString("0.0");
        txtYCoord.Text = area.Position.Y.ToString("0.0");
        txtRadius.Text = area.Radius.ToString();
        //Walkback
        txtWalkscript.Text = PlayerConfig.Get<string>("RSBot.Walkback.File");
    }

    private void buttonSelectTrainingArea_Click(object sender, EventArgs e)
    {
        var trainingArea = new TrainingAreasDialog();
        if (trainingArea.ShowDialog(this) == DialogResult.OK)
            EventManager.FireEvent("OnSetTrainingArea");
    }

    private void linkAttackWeakerMobsHelp_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
    {
        MessageBox.Show(
            "If the player is under attack by a monster that is set to be avoided the bot will counter attack weaker mobs that are currently attacking the player first before targeting the avoided monster again. The bot will only kill weaker monsters that are attacking the player and won't start to pull new mobs to the battle.",
            "Attack weaker mobs first", MessageBoxButtons.OK, MessageBoxIcon.Question);
    }

    private void linkRecord_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
    {
        EventManager.FireEvent("OnShowScriptRecorder", ScriptRecorderOwnerId, true);
    }

    private void Main_Load(object sender, EventArgs e)
    {
        _settingsLoaded = false;
        LoadSettings();
        _settingsLoaded = true;
        ApplyTrainingLayout();
    }
}