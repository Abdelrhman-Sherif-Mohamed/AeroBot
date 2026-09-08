using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using RSBot.Core;
using RSBot.Core.Components;
using RSBot.Core.Components.Scripting;
using RSBot.Core.Event;
using RSBot.Core.Objects;
using RSBot.Core.Objects.Spawn;
using SDUI.Controls;

namespace RSBot.Views;

public partial class ScriptRecorder : UIWindow
{
    private readonly int _ownerId;

    private bool _recording;
    private bool _running;
    private string _currentFile;

    /// <summary>
    ///     Initializes a new instance of the <see cref="ScriptRecorder" /> class.
    /// </summary>
    public ScriptRecorder(int ownerId = 0, bool startRecording = false)
    {
        _ownerId = ownerId;

        InitializeComponent();
        SubscribeEvents();

        if (startRecording)
            StartRecording();

        UpdateLineCount();
    }

    /// <summary>
    ///     Subscribes the events.
    /// </summary>
    private void SubscribeEvents()
    {
        EventManager.SubscribeEvent("OnPlayerMove", OnPlayerMove);
        EventManager.SubscribeEvent("OnVehicleMove", OnPlayerMove);
        EventManager.SubscribeEvent("OnRequestTeleport", new Action<uint, string>(OnRequestTeleport));
        EventManager.SubscribeEvent("OnTerminateVehicle", OnTerminateVehicle);
        EventManager.SubscribeEvent("OnTeleportComplete", OnTeleportComplete);
        EventManager.SubscribeEvent("OnScriptStartExecuteCommand",
            new Action<IScriptCommand, int>(OnScriptStartExecuteCommand));
        EventManager.SubscribeEvent("OnNpcRepairRequest", new Action<uint, byte, byte>(OnNpcRepairRequest));
        EventManager.SubscribeEvent("OnStorageOpenRequest", new Action<uint>(StorageOpenRequest));
        EventManager.SubscribeEvent("OnTalkRequest", new Action<uint, TalkOption>(OnTalkRequest));
        EventManager.SubscribeEvent("OnFinishScript", new Action<bool>(OnFinishScript));
        EventManager.SubscribeEvent("OnCastSkill", new Action<uint>(OnCastSkill));

        // Custom commands from plugins
        EventManager.SubscribeEvent("AppendScriptCommand", new Action<string>(AppendScriptCommand));
    }

    private void OnCastSkill(uint skillId)
    {
        if (!_recording)
            return;

        var refSkill = Game.ReferenceManager.GetRefSkill(skillId);
        if (refSkill != null)
            InsertCommand($"cast {refSkill.Basic_Code}");
    }

    /// <summary>
    ///     Appends the script command fired from a plugin.
    /// </summary>
    private void AppendScriptCommand(string command)
    {
        if (!_recording)
            return;

        InsertCommand(command);
    }

    /// <summary>
    ///     Inserts a command into the script text based on the active mode (Append or Beginning).
    /// </summary>
    private void InsertCommand(string command)
    {
        if (string.IsNullOrWhiteSpace(command))
            return;

        command = command.Trim();

        if (radioInsertBeginning.Checked)
        {
            txtScript.Text = command + Environment.NewLine + txtScript.Text;
        }
        else
        {
            if (txtScript.Text.Length > 0 && !txtScript.Text.EndsWith("\n") && !txtScript.Text.EndsWith("\r\n"))
                txtScript.AppendText(Environment.NewLine);

            txtScript.AppendText(command + Environment.NewLine);
            txtScript.SelectionStart = txtScript.Text.Length;
            txtScript.ScrollToCaret();
        }

        UpdateLineCount();
    }

    private void UpdateLineCount()
    {
        var count = txtScript.Lines.Count(l => !string.IsNullOrWhiteSpace(l));
        lblLineCount.Text = $"Lines: {count}";
    }

    /// <summary>
    ///     Finds a nearby NPC code name matching any of the prefixes, or returns the default code.
    /// </summary>
    private static string GetNearbyNpcCodeName(string[] possiblePrefixes, string defaultCode)
    {
        if (Game.Player == null || !Game.Ready)
            return defaultCode;

        if (SpawnManager.TryGetEntities<SpawnedNpcNpc>(out var npcs))
        {
            foreach (var prefix in possiblePrefixes)
            {
                var found = npcs.FirstOrDefault(n =>
                    n.Record != null && n.Record.CodeName.IndexOf(prefix, StringComparison.OrdinalIgnoreCase) >= 0);

                if (found != null)
                    return found.Record.CodeName;
            }
        }

        return defaultCode;
    }

    #region Recording Events

    private void OnFinishScript(bool error = false)
    {
        _running = false;
        labelStatus.Text = "Status: <Idle>";
        labelStatus.ForeColor = Color.FromArgb(0, 200, 83);
        btnRunNow.Text = "Run Script";
        btnRunNow.Color = Color.FromArgb(0, 150, 136);
        txtScript.ReadOnly = false;
    }

    private void OnTalkRequest(uint entityId, TalkOption option)
    {
        if (!_recording)
            return;

        if (!SpawnManager.TryGetEntity<SpawnedBionic>(entityId, out var entity))
            return;

        switch (option)
        {
            case TalkOption.Store:
                InsertCommand($"buy {entity.Record.CodeName}");
                break;
            case TalkOption.Repair:
                InsertCommand($"repair {entity.Record.CodeName}");
                break;
            case TalkOption.Trade:
                InsertCommand($"buy-goods {entity.Record.CodeName}");
                break;
        }
    }

    private void StorageOpenRequest(uint entityId)
    {
        if (!_recording)
            return;

        if (!SpawnManager.TryGetEntity<SpawnedBionic>(entityId, out var entity))
            return;

        InsertCommand($"store {entity.Record.CodeName}");
    }

    private void OnNpcRepairRequest(uint entityId, byte arg2, byte arg3)
    {
        if (!_recording)
            return;

        if (!SpawnManager.TryGetEntity<SpawnedBionic>(entityId, out var entity))
            return;

        InsertCommand($"repair {entity.Record.CodeName}");
    }

    private void OnScriptStartExecuteCommand(IScriptCommand command, int lineNumber)
    {
        HighlightLine(lineNumber, Color.FromArgb(40, 70, 110));
    }

    private void HighlightLine(int index, Color color)
    {
        txtScript.SelectAll();
        txtScript.SelectionBackColor = txtScript.BackColor;
        var lines = txtScript.Lines;
        if (index < 0 || index >= lines.Length)
            return;

        var start = txtScript.GetFirstCharIndexFromLine(index);
        var length = lines[index].Length;
        txtScript.Select(start, length);
        txtScript.SelectionBackColor = color;
    }

    private void OnPlayerMove()
    {
        if (!_recording)
            return;

        SpawnedEntity entity = Game.Player;
        if (Game.Player.HasActiveVehicle)
            entity = Game.Player.Vehicle.Bionic;

        if (!entity.Movement.HasDestination)
            return;

        var destination = entity.Movement.Destination;
        var stepString = new StringBuilder();
        stepString.Append($"move {destination.XOffset:0}");
        stepString.Append($" {destination.YOffset:0}");
        stepString.Append($" {destination.ZOffset:0}");
        stepString.Append($" {destination.Region.X}");
        stepString.Append($" {destination.Region.Y}");

        InsertCommand(stepString.ToString());
    }

    private void OnRequestTeleport(uint destination, string npcCodeName)
    {
        if (!_recording)
            return;

        InsertCommand($"teleport {npcCodeName} {destination}");
    }

    private void OnTerminateVehicle()
    {
        if (!_recording)
            return;

        InsertCommand("dismount");
    }

    private void OnTeleportComplete()
    {
        if (!_recording)
            return;

        InsertCommand("wait 5000");
    }

    #endregion

    #region Controls GroupBox Handlers

    private void btnRecord_Click(object sender, EventArgs e)
    {
        if (ScriptManager.Running)
            return;

        StartRecording();
    }

    private void btnStop_Click(object sender, EventArgs e)
    {
        StopRecording();
    }

    private void StartRecording()
    {
        _recording = true;
        labelStatus.Text = "Status: <Recording...>";
        labelStatus.ForeColor = Color.FromArgb(255, 82, 82);
        btnRecord.Color = Color.FromArgb(255, 23, 68);
        btnRunNow.Enabled = false;
    }

    private void StopRecording()
    {
        _recording = false;
        labelStatus.Text = "Status: <Idle>";
        labelStatus.ForeColor = Color.FromArgb(0, 200, 83);
        btnRecord.Color = Color.FromArgb(192, 0, 0);
        btnRunNow.Enabled = true;
    }

    private void btnClear_Click(object sender, EventArgs e)
    {
        if (MessageBox.Show("Are you sure you want to clear the script?", "Clear Script",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
        {
            txtScript.Clear();
            UpdateLineCount();
        }
    }

    private void btnSave_Click(object sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(txtScript.Text))
            return;

        if (!string.IsNullOrWhiteSpace(_currentFile) && File.Exists(_currentFile))
        {
            File.WriteAllText(_currentFile, txtScript.Text);
            EventManager.FireEvent("OnSaveScript", _ownerId, _currentFile);
            labelStatus.Text = "Status: Saved!";
            labelStatus.ForeColor = Color.FromArgb(0, 200, 83);
            return;
        }

        btnSaveAs_Click(sender, e);
    }

    private void btnSaveAs_Click(object sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(txtScript.Text))
            return;

        var diag = new SaveFileDialog
        {
            Title = "Save Recorded Script",
            Filter = "AeroBot Script File (*.rbs)|*.rbs|All Files (*.*)|*.*",
            InitialDirectory = ScriptManager.InitialDirectory
        };

        if (diag.ShowDialog() == DialogResult.OK)
        {
            _currentFile = diag.FileName;
            File.WriteAllText(_currentFile, txtScript.Text);
            EventManager.FireEvent("OnSaveScript", _ownerId, _currentFile);
            labelStatus.Text = $"Status: Saved ({Path.GetFileName(_currentFile)})";
            labelStatus.ForeColor = Color.FromArgb(0, 200, 83);
        }
    }

    private void btnOpenScript_Click(object sender, EventArgs e)
    {
        var diag = new OpenFileDialog
        {
            Title = "Open Script File",
            Filter = "AeroBot Script File (*.rbs)|*.rbs|All Files (*.*)|*.*",
            InitialDirectory = ScriptManager.InitialDirectory
        };

        if (diag.ShowDialog() == DialogResult.OK)
        {
            _currentFile = diag.FileName;
            txtScript.Text = File.ReadAllText(_currentFile);
            UpdateLineCount();
            labelStatus.Text = $"Loaded: {Path.GetFileName(_currentFile)}";
            labelStatus.ForeColor = Color.FromArgb(33, 150, 243);
        }
    }

    private void btnRunNow_Click(object sender, EventArgs e)
    {
        if (_recording || string.IsNullOrWhiteSpace(txtScript.Text))
            return;

        if (_running)
        {
            ScriptManager.Stop();
            OnFinishScript();
        }
        else
        {
            if (ScriptManager.Running)
                return;

            ScriptManager.Load(txtScript.Text.Split('\n'));
            Task.Run(() => ScriptManager.RunScript(false, true));

            labelStatus.Text = "Status: <Running...>";
            labelStatus.ForeColor = Color.FromArgb(255, 193, 7);
            btnRunNow.Text = "Stop Script";
            btnRunNow.Color = Color.FromArgb(211, 47, 47);
            txtScript.ReadOnly = true;
            _running = true;
        }
    }

    private void txtScript_TextChanged(object sender, EventArgs e)
    {
        UpdateLineCount();
    }

    private void ScriptRecorder_FormClosed(object sender, FormClosedEventArgs e)
    {
        _recording = false;
        _running = false;
    }

    private void ScriptRecorder_Load(object sender, EventArgs e)
    {
        LanguageManager.Translate(this, Kernel.Language);
    }

    #endregion

    #region Action Buttons - Town & Storage (Column 1)

    private void btnBlacksmith_Click(object sender, EventArgs e)
    {
        var smith = GetNearbyNpcCodeName(new[] { "SMITH" }, "NPC_CH_SMITH");
        InsertCommand($"repair {smith}");
        InsertCommand($"buy {smith}");
    }

    private void btnGrocery_Click(object sender, EventArgs e)
    {
        var grocery = GetNearbyNpcCodeName(new[] { "ACCESSORY", "GROCERY" }, "NPC_CH_ACCESSORY");
        InsertCommand($"buy {grocery}");
    }

    private void btnHerbalist_Click(object sender, EventArgs e)
    {
        var herb = GetNearbyNpcCodeName(new[] { "POTION", "HERBALIST" }, "NPC_CH_POTION");
        InsertCommand($"buy {herb}");
    }

    private void btnStable_Click(object sender, EventArgs e)
    {
        var stable = GetNearbyNpcCodeName(new[] { "HORSE", "STABLE" }, "NPC_CH_HORSE");
        InsertCommand($"buy {stable}");
    }

    private void btnJupiter_Click(object sender, EventArgs e)
    {
        var jupiter = GetNearbyNpcCodeName(new[] { "JUPITER" }, "NPC_JUPITER");
        InsertCommand($"buy {jupiter}");
    }

    private void btnStorage_Click(object sender, EventArgs e)
    {
        var store = GetNearbyNpcCodeName(new[] { "WAREHOUSE" }, "NPC_CH_WAREHOUSE");
        InsertCommand($"store {store}");
    }

    private void btnTakeStorage_Click(object sender, EventArgs e)
    {
        InsertCommand("storage-take");
    }

    private void btnStoreStorage_Click(object sender, EventArgs e)
    {
        var store = GetNearbyNpcCodeName(new[] { "WAREHOUSE" }, "NPC_CH_WAREHOUSE");
        InsertCommand($"store {store}");
    }

    private void btnGuildStorage_Click(object sender, EventArgs e)
    {
        var guildStore = GetNearbyNpcCodeName(new[] { "GUILD" }, "NPC_CH_GUILD_WAREHOUSE");
        InsertCommand($"store {guildStore}");
    }

    private void btnTakeGuildStorage_Click(object sender, EventArgs e)
    {
        InsertCommand("guildstorage-take");
    }

    private void btnStoreGuildStorage_Click(object sender, EventArgs e)
    {
        var guildStore = GetNearbyNpcCodeName(new[] { "GUILD" }, "NPC_CH_GUILD_WAREHOUSE");
        InsertCommand($"store {guildStore}");
    }

    private void btnExecuteScript_Click(object sender, EventArgs e)
    {
        var diag = new OpenFileDialog
        {
            Title = "Choose Script to Execute",
            Filter = "AeroBot Script File (*.rbs)|*.rbs|All Files (*.*)|*.*",
            InitialDirectory = ScriptManager.InitialDirectory
        };

        if (diag.ShowDialog() == DialogResult.OK)
            InsertCommand($"executescript {Path.GetFileName(diag.FileName)}");
    }

    private void btnStyria_Click(object sender, EventArgs e)
    {
        InsertCommand("styria");
    }

    private void btnConsignment_Click(object sender, EventArgs e)
    {
        InsertCommand("consignment");
    }

    private void btnStall_Click(object sender, EventArgs e)
    {
        InsertCommand("stall");
    }

    #endregion

    #region Action Buttons - Movement & Utilities (Column 2)

    private void btnTeleport_Click(object sender, EventArgs e)
    {
        var teleporter = GetNearbyNpcCodeName(new[] { "GATE", "TELEPORT" }, "NPC_CH_GATE");
        var destDiag = new InputDialog("Teleport", "Destination ID", "Enter destination ID (e.g. 1):", InputDialog.InputType.Textbox, "1");
        if (destDiag.ShowDialog() == DialogResult.OK)
        {
            var dest = (string)destDiag.Value;
            if (string.IsNullOrWhiteSpace(dest)) dest = "1";
            InsertCommand($"teleport {teleporter} {dest}");
        }
    }

    private void btnComment_Click(object sender, EventArgs e)
    {
        var commentDiag = new InputDialog("Add Comment", "Comment", "Enter comment text:");
        if (commentDiag.ShowDialog() == DialogResult.OK)
        {
            var text = (string)commentDiag.Value;
            if (!string.IsNullOrWhiteSpace(text))
                InsertCommand($"# {text}");
        }
    }

    private void btnTerminate_Click(object sender, EventArgs e)
    {
        InsertCommand("terminate");
    }

    private void btnQuest_Click(object sender, EventArgs e)
    {
        InsertCommand("quest");
    }

    private void btnMount_Click(object sender, EventArgs e)
    {
        InsertCommand("mount");
    }

    private void btnUseItem_Click(object sender, EventArgs e)
    {
        var itemDiag = new InputDialog("Use Item", "Item Codename", "Enter item code name or keyword:");
        if (itemDiag.ShowDialog() == DialogResult.OK)
        {
            var code = (string)itemDiag.Value;
            if (!string.IsNullOrWhiteSpace(code))
                InsertCommand($"useitem {code}");
        }
    }

    private void btnDismount_Click(object sender, EventArgs e)
    {
        InsertCommand("dismount");
    }

    private void btnWait_Click(object sender, EventArgs e)
    {
        var waitDiag = new InputDialog("Wait Command", "Wait Duration (ms)", "Enter milliseconds to wait (e.g. 5000):", InputDialog.InputType.Textbox, "5000");
        if (waitDiag.ShowDialog() == DialogResult.OK)
        {
            var ms = string.IsNullOrWhiteSpace((string)waitDiag.Value) ? "5000" : (string)waitDiag.Value;
            InsertCommand($"wait {ms}");
        }
    }

    private void btnOldTrade_Click(object sender, EventArgs e)
    {
        var special = GetNearbyNpcCodeName(new[] { "SPECIAL_MERCHANT", "SPECIALTY" }, "NPC_CH_SPECIAL_MERCHANT");
        InsertCommand($"buy-goods {special}");
    }

    private void btnStopBot_Click(object sender, EventArgs e)
    {
        InsertCommand("stop");
    }

    private void btnRecall_Click(object sender, EventArgs e)
    {
        InsertCommand("recall");
    }

    private void btnDisconnect_Click(object sender, EventArgs e)
    {
        InsertCommand("disconnect");
    }

    private void btnProfile_Click(object sender, EventArgs e)
    {
        var profDiag = new InputDialog("Profile", "Profile Name", "Enter profile name:");
        if (profDiag.ShowDialog() == DialogResult.OK)
        {
            var name = (string)profDiag.Value;
            if (!string.IsNullOrWhiteSpace(name))
                InsertCommand($"profile {name}");
        }
    }

    private void btnAutoConfig_Click(object sender, EventArgs e)
    {
        InsertCommand("trainingarea");
    }

    private void btnReverse_Click(object sender, EventArgs e)
    {
        InsertCommand("reverse");
    }

    private void btnPathFind_Click(object sender, EventArgs e)
    {
        var pos = Game.Player?.Position;
        var defaultPos = pos != null ? $"{pos.Value.XOffset:0} {pos.Value.YOffset:0}" : "0 0";
        var pfDiag = new InputDialog("Path Find", "Target Coordinates", "Enter X Y coordinates:", InputDialog.InputType.Textbox, defaultPos);
        if (pfDiag.ShowDialog() == DialogResult.OK)
        {
            var val = (string)pfDiag.Value;
            if (!string.IsNullOrWhiteSpace(val))
                InsertCommand($"pathfind {val}");
        }
    }

    private void btnDismantle_Click(object sender, EventArgs e)
    {
        InsertCommand("dismantle");
    }

    #endregion

    #region Action Buttons - Bottom Trading & Mini Grid

    private void btnBeginTargetTrading_Click(object sender, EventArgs e)
    {
        InsertCommand("begintargettrading");
    }

    private void btnSettleTargetTrading_Click(object sender, EventArgs e)
    {
        InsertCommand("settletargettrading");
    }

    private void btnBeginConsignment_Click(object sender, EventArgs e)
    {
        InsertCommand("beginconsignment");
    }

    private void btnSettleConsignment_Click(object sender, EventArgs e)
    {
        InsertCommand("settleconsignment");
    }

    private void btnSort_Click(object sender, EventArgs e)
    {
        InsertCommand("sort");
    }

    private void btnSplit_Click(object sender, EventArgs e)
    {
        InsertCommand("split");
    }

    private void btnCast_Click(object sender, EventArgs e)
    {
        var castDiag = new InputDialog("Cast Skill", "Skill Code", "Enter skill code name (e.g. SKILL_CH_SWORD...):");
        if (castDiag.ShowDialog() == DialogResult.OK)
        {
            var skill = (string)castDiag.Value;
            if (!string.IsNullOrWhiteSpace(skill))
                InsertCommand($"cast {skill}");
        }
    }

    private void btnRecast_Click(object sender, EventArgs e)
    {
        InsertCommand("recast");
    }

    private void btnEquip_Click(object sender, EventArgs e)
    {
        InsertCommand("equip");
    }

    private void btnUnequip_Click(object sender, EventArgs e)
    {
        InsertCommand("unequip");
    }

    #endregion
}
