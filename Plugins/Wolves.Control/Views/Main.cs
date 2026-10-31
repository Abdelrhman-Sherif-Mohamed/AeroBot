using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using RSBot.Core;
using RSBot.Core.Components;
using RSBot.Core.Objects.Spawn;
using SDUI.Controls;
using SDUI.Helpers;

namespace Wolves.Control.Views;

[ToolboxItem(false)]
public partial class Main : DoubleBufferedControl
{
    private static Main _instance;
    public static Main Instance => _instance ??= new Main();

    private record CommandGuideItem(
        string Command,
        string Channel,
        string Category,
        string DescriptionAr,
        string DescriptionEn,
        string Example
    );

    private static readonly List<CommandGuideItem> AllCommands = new()
    {
        // Target Support
        new("TARGET ON", "البارتي / الهمس", "مساعد الاستهداف (Target Support)", "تفعيل مزامنة ومساعدة هدف القائد تلقائياً", "Enable target sync with leader", "TARGET ON"),
        new("TARGET OFF", "البارتي / الهمس", "مساعد الاستهداف (Target Support)", "إلغاء تفعيل مزامنة هدف القائد", "Disable target sync with leader", "TARGET OFF"),

        // Choreography & Formations (حركات استعراضية وتشكيلات - Wolves & Agnotobot)
        new("!STACK / formasyon,icice", "البارتي / الهمس", "حركات استعراضية (Choreography)", "المشي داخل بعض تماماً وتطابق الموديل للتحرك كشخصية واحدة (İç İçe / Ghost Stack)", "Overlap characters on exact same point into a single model", "!STACK"),
        new("!SCATTER / formasyon,rastgele", "البارتي / الهمس", "حركات استعراضية (Choreography)", "تشتت استعراضي عشوائي للشخصيات حول القائد (Rastgele Scatter)", "Scatter characters randomly in a wide area around leader", "!SCATTER 8"),
        new("!MIMIC / tekrarla", "البارتي / الهمس", "حركات استعراضية (Choreography)", "محاكاة تصرفات القائد فوراً (المشي، الركض، الجلوس، ركوب الحصان، والإيموت)", "Mimic leader's movement, mounts, sit/stand, and emotes", "!MIMIC"),
        new("!GEL / gel", "البارتي / الهمس", "حركات استعراضية (Choreography)", "انتقال جميع الشخصيات لموقع القائد فوراً والاجتماع عنده", "Move all characters immediately to leader position", "!GEL"),
        new("!ORBIT [قطر]", "البارتي / الهمس", "حركات استعراضية (Choreography)", "دوران استعراضي مستمر لجميع الشخصيات حول القائد أو حول بعضهم", "Continuous orbital carousel of characters around leader", "!ORBIT 6"),
        new("!COUNTER [قطر]", "البارتي / الهمس", "حركات استعراضية (Choreography)", "دوران حلزوني متعاكس في حلقتين متداخلتين باتجاهين مختلفين", "Counter-rotating double orbital rings", "!COUNTER 7"),
        new("!LINE [مسافة]", "البارتي / الهمس", "حركات استعراضية (Choreography)", "تشكيل صف أفقي متساوي (كتف بكتف) والمشي المتزامن", "Horizontal line-abreast formation marching forward", "!LINE 4"),
        new("!V / !WEDGE", "البارتي / الهمس", "حركات استعراضية (Choreography)", "تشكيل رأس السهم العسكري V خلف القائد كأجنحة مقاتلة", "Military V-wedge spearhead formation behind leader", "!V"),
        new("!CIRCLE / !RING", "البارتي / الهمس", "حركات استعراضية (Choreography)", "تشكيل حلقة دائرية مغلقة تحيط بالقائد وتتحرك معه", "Equidistant circular ring surrounding the leader", "!CIRCLE 6"),
        new("!SQUARE / !BOX", "البارتي / الهمس", "حركات استعراضية (Choreography)", "تشكيل مربع حماية أمني يحيط بالقائد من كل الجهات", "Square protective box formation surrounding leader", "!BOX 5"),
        new("!TRIANGLE", "البارتي / الهمس", "حركات استعراضية (Choreography)", "تشكيل مثلث استعراضي متساوي الأضلاع حول القائد", "Equilateral triangle formation surrounding leader", "!TRIANGLE 7"),
        new("!CROSS / !PLUS", "البارتي / الهمس", "حركات استعراضية (Choreography)", "تشكيل صليب / تقاطع (+) استعراضي متناظر", "Cross / plus formation around leader", "!CROSS 6"),
        new("!DIAMOND", "البارتي / الهمس", "حركات استعراضية (Choreography)", "تشكيل معيّن ماسي استعراضي هندسي أنيق", "Diamond / rhombus formation around leader", "!DIAMOND 6"),
        new("!COLUMN / !MARCH", "البارتي / الهمس", "حركات استعراضية (Choreography)", "طابور عسكري متتالي خلف القائد بمسافات متساوية", "Single-file military column trailing leader", "!COLUMN"),
        new("!PULSE", "البارتي / الهمس", "حركات استعراضية (Choreography)", "نبض استعراضي (توسع وانكماش دائري متزامن وإيقاعي)", "Rhythmic expanding and contracting circular pulse", "!PULSE"),
        new("!DANCE / dans", "البارتي / الهمس", "حركات استعراضية (Choreography)", "رقص جماعي متزامن لجميع أفراد الفريق في نفس اللحظة", "Simultaneous synchronized dance emote for party", "!DANCE"),
        new("!WAVE", "البارتي / الهمس", "حركات استعراضية (Choreography)", "الموجة الاستعراضية المتتالية (The Stadium Wave) بتأخير متتابع", "Cascading stadium wave emote around formation", "!WAVE"),
        new("!CHEER / selamla", "البارتي / الهمس", "حركات استعراضية (Choreography)", "تحية النصر أو انحناءة ملكية جماعية متزامنة", "Synchronized cheer or bow emote across all bots", "!CHEER"),
        new("!HALT / dur", "البارتي / الهمس", "حركات استعراضية (Choreography)", "إيقاف الحركة الاستعراضية وتثبيت الشخصيات مكانها", "Halt choreography and hold positions immediately", "!HALT"),

        // In-Game Direct Commands
        new("!bot", "العميل (Client)", "أوامر مباشرة (!)", "إظهار أو إخفاء لوحة التحكم الطافية داخل اللعبة", "Show or hide on-demand in-game floating panel", "!bot"),
        new("!start", "العميل (Client)", "أوامر مباشرة (!)", "بدء تشغيل البوت في منطقة التدريب الحالية", "Start bot training in configured area", "!start"),
        new("!starthere", "العميل (Client)", "أوامر مباشرة (!)", "بدء التدريب في موقعك الحالي فوراً وتحديث الإحداثيات", "Start bot training at current position immediately", "!starthere"),
        new("!stop", "العميل (Client)", "أوامر مباشرة (!)", "إيقاف البوت عن التدريب والعمل فوراً", "Stop bot training immediately", "!stop"),
        new("!town", "العميل (Client)", "أوامر مباشرة (!)", "استخدام سكرول العودة للمدينة فوراً", "Use return scroll to town immediately", "!town"),
        new("!area", "العميل (Client)", "أوامر مباشرة (!)", "تعيين منطقة التدريب عند إحداثيات موقع اللاعب الحالي", "Set training area coordinates to current position", "!area"),
        new("!help / !commands", "العميل (Client)", "أوامر مباشرة (!)", "عرض ملخص الأوامر باللعبة وطباعة الدليل الشامل بسجل البوت", "Show command guide in-game and in bot log", "!help"),
        new("!b1 .. !b7", "العميل (Client)", "حرب القلعة (Fortress)", "طلب بفة العضو 1-7 (فحص الكولدون أو إلقاؤها إذا كانت جاهزة)", "Request buff for party slot 1-7 (check CD or cast)", "!b1"),

        // Remote Leader Commands
        new("START / S", "البارتي / الهمس", "أوامر القائد (Leaders)", "بدء تشغيل البوت عن بعد", "Start bot remotely", "START"),
        new("STOP / SS", "البارتي / الهمس", "أوامر القائد (Leaders)", "إيقاف البوت عن بعد", "Stop bot remotely", "STOP"),
        new("TRACE [اسم] / T", "البارتي / الهمس", "أوامر القائد (Leaders)", "تتبع ومشي خلف الليدر أو شخصية محددة في المحيط", "Trace leader or specific player in vicinity", "TRACE LeaderName"),
        new("NOTRACE / N", "البارتي / الهمس", "أوامر القائد (Leaders)", "إيقاف التتبع والوقوف مكان اللاعب", "Stop tracing", "NOTRACE"),
        new("FOLLOW [مسافة] / F", "البارتي / الهمس", "أوامر القائد (Leaders)", "ملاحقة الليدر مع الحفاظ على مسافة أمتار محددة", "Follow leader with specified distance in meters", "FOLLOW 5"),
        new("NOFOLLOW / NF", "البارتي / الهمس", "أوامر القائد (Leaders)", "إيقاف الملاحقة", "Stop following", "NOFOLLOW"),
        new("RETURN / RE", "البارتي / الهمس", "أوامر القائد (Leaders)", "استخدام سكرول العودة للمدينة أو الإنعاش بالمدينة عند الموت", "Use return scroll or resurrect at nearest town", "RETURN"),
        new("ZERK / Z", "البارتي / الهمس", "أوامر القائد (Leaders)", "تفعيل وضع البيرزرك (Berserk)", "Activate Berserk mode", "ZERK"),
        new("GETOUT / G", "البارتي / الهمس", "أوامر القائد (Leaders)", "الخروج من البارتي الحالي فوراً", "Leave current party immediately", "GETOUT"),
        new("MOUNT / M", "البارتي / الهمس", "أوامر القائد (Leaders)", "ركوب الحصان أو وسيلة النقل", "Mount transport horse/vehicle", "MOUNT"),
        new("DISMOUNT / D", "البارتي / الهمس", "أوامر القائد (Leaders)", "النزول من الحصان أو وسيلة النقل", "Dismount horse/vehicle", "DISMOUNT"),
        new("STATUS / HP / MP", "البارتي / الهمس", "أوامر القائد (Leaders)", "الاستعلام عن دم ومانا وجرعات البوت في همس خاص", "Query bot's HP, MP, and potion counts via whisper", "STATUS"),
        new("MOVEON [قطر]", "البارتي / الهمس", "أوامر القائد (Leaders)", "التحرك لموقع عشوائي بنصف قطر محدد (افتراضي 15 متر)", "Move randomly within specified radius (default 15m)", "MOVEON 20"),
        new("SETPOS x y z", "البارتي / الهمس", "أوامر القائد (Leaders)", "تعيين إحداثيات موقع التدريب يدوياً", "Set training area coordinates manually", "SETPOS 150 220 0"),
        new("SETRADIUS [متر]", "البارتي / الهمس", "أوامر القائد (Leaders)", "تحديد نصف قطر منطقة التدريب بالأمتار", "Set training radius in meters", "SETRADIUS 45"),
        new("SETSCRIPT [اسم]", "البارتي / الهمس", "أوامر القائد (Leaders)", "تحميل سكريبت تدريب بالاسم من مجلد السكريبتات", "Load training script by name", "SETSCRIPT Jangan_Cave"),
        new("GETPOS", "البارتي / الهمس", "أوامر القائد (Leaders)", "الاستعلام عن إحداثيات موقع البوت الحالي", "Get bot's current coordinates", "GETPOS"),
        new("SIT", "البارتي / الهمس", "أوامر القائد (Leaders)", "الجلوس أو الوقوف", "Sit or stand up", "SIT"),
        new("CAPE [لون]", "البارتي / الهمس", "أوامر القائد (Leaders)", "ارتداء كيب الـ PVP باللون المفضل (yellow, blue, red, white)", "Equip PVP cape color", "CAPE red"),
        new("USE [اسم الأداة]", "البارتي / الهمس", "أوامر القائد (Leaders)", "استخدام أداة من الحقيبة بالاسم", "Use inventory item by name", "USE Speed Potion"),
        new("DC / DIS", "البارتي / الهمس", "أوامر القائد (Leaders)", "قطع الاتصال والخروج الفوري من اللعبة", "Disconnect client immediately", "DC")
    };

    public Main()
    {
        InitializeComponent();
        ControlSpacer.NormalizePage(this);

        try
        {
            typeof(System.Windows.Forms.Control).GetProperty("DoubleBuffered", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                ?.SetValue(lstCommands, true, null);
            lstCommands.ShowItemToolTips = true;
        }
        catch { }

        ControlManager.Instance.OnStateChanged += () => SafeInvoke(UpdateUI);
        ControlManager.Instance.OnLogAdded += log => SafeInvoke(() => AppendLog(log));
        ChoreographyManager.Instance.OnStateChanged += () => SafeInvoke(UpdateChoreoStatus);
        TargetSupportManager.Instance.OnStateChanged += () => SafeInvoke(UpdateTargetUI);
    }

    private void SafeInvoke(Action action)
    {
        if (IsHandleCreated && InvokeRequired)
            BeginInvoke(action);
        else
            action();
    }

    private void Main_Load(object sender, EventArgs e)
    {
        PopulateCategoryDropdown();
        FilterCommands();

        UpdateUI();
        UpdateChoreoStatus();
        UpdateTargetUI();
        LoadLogs();
    }

    #region Tab 1: Control & Leaders

    private void UpdateUI()
    {
        chkEnabled.Checked = ControlManager.Instance.Enabled;

        lstLeaders.BeginUpdate();
        lstLeaders.Items.Clear();
        foreach (var l in ControlManager.Instance.Leaders)
        {
            lstLeaders.Items.Add(new ListViewItem(l));
        }
        lstLeaders.EndUpdate();
    }

    private void chkEnabled_CheckedChanged(object sender, EventArgs e)
    {
        ControlManager.Instance.Enabled = chkEnabled.Checked;
        ControlManager.Instance.SaveConfig();
    }

    private void btnAddLeader_Click(object sender, EventArgs e)
    {
        var name = cboLeaderName.Text.Trim();
        if (!string.IsNullOrEmpty(name))
        {
            ControlManager.Instance.AddLeader(name);
            cboLeaderName.Text = string.Empty;
        }
    }

    private void btnRemoveLeader_Click(object sender, EventArgs e)
    {
        if (lstLeaders.SelectedItems.Count > 0)
        {
            var name = lstLeaders.SelectedItems[0].Text;
            ControlManager.Instance.RemoveLeader(name);
        }
        else if (!string.IsNullOrWhiteSpace(cboLeaderName.Text))
        {
            ControlManager.Instance.RemoveLeader(cboLeaderName.Text.Trim());
        }
    }

    private void btnGetPartyLeader_Click(object sender, EventArgs e)
    {
        if (Game.Party?.Leader != null && !string.IsNullOrEmpty(Game.Party.Leader.Name))
        {
            ControlManager.Instance.AddLeader(Game.Party.Leader.Name);
            ControlManager.Instance.Log($"تمت إضافة ليدر البارتي [{Game.Party.Leader.Name}] كقائد مصرح له");
        }
        else
        {
            ControlManager.Instance.Log("لا يوجد بارتي نشط أو ليدر بارتي متاح");
        }
    }

    private void cboLeaderName_DropDown(object sender, EventArgs e)
    {
        cboLeaderName.Items.Clear();
        if (Game.Party?.Members != null)
        {
            foreach (var m in Game.Party.Members)
            {
                if (!string.IsNullOrEmpty(m.Name) && !cboLeaderName.Items.Contains(m.Name))
                    cboLeaderName.Items.Add(m.Name);
            }
        }
    }

    private void LoadLogs()
    {
        txtLogs.Clear();
        foreach (var l in ControlManager.Instance.Logs)
            txtLogs.AppendText(l + "\n");
        txtLogs.ScrollToCaret();
    }

    private void AppendLog(string log)
    {
        txtLogs.AppendText(log + "\n");
        txtLogs.ScrollToCaret();
    }

    private void btnClearLogs_Click(object sender, EventArgs e)
    {
        ControlManager.Instance.Logs.Clear();
        txtLogs.Clear();
    }

    #endregion

    #region Tab 2: Choreography

    private void StartFormation(FormationType formation, float? customRadius = null)
    {
        float rad = customRadius ?? ChoreographyManager.Instance.Radius;
        string leader = cboChoreoLeader.Text.Trim();
        ChoreographyManager.Instance.StartFormation(formation, leader, rad);

        if (chkBroadcastParty.Checked)
        {
            string cmdName = formation switch
            {
                FormationType.Orbit => $"!ORBIT {rad:F0}",
                FormationType.CounterOrbit => $"!COUNTER {rad:F0}",
                FormationType.Line => $"!LINE {rad:F0}",
                FormationType.Wedge => $"!V",
                FormationType.Circle => $"!CIRCLE {rad:F0}",
                FormationType.Square => $"!BOX {rad:F0}",
                FormationType.Column => $"!COLUMN",
                FormationType.Pulse => $"!PULSE",
                FormationType.Stack => $"!STACK",
                FormationType.Scatter => $"!SCATTER {rad:F0}",
                _ => string.Empty
            };
            if (!string.IsNullOrEmpty(cmdName))
                ChoreographyManager.Instance.BroadcastCommand(cmdName);
        }
    }

    private void TriggerEmote(EmoteActionType action)
    {
        ChoreographyManager.Instance.PerformSyncEmote(action);
        if (chkBroadcastParty.Checked)
        {
            string cmdName = action switch
            {
                EmoteActionType.Dance => "!DANCE",
                EmoteActionType.Cheer => "!CHEER",
                EmoteActionType.Bow => "!BOW",
                EmoteActionType.Clap => "!CLAP",
                _ => string.Empty
            };
            if (!string.IsNullOrEmpty(cmdName))
                ChoreographyManager.Instance.BroadcastCommand(cmdName);
        }
    }

    private void btnMimic_Click(object sender, EventArgs e)
    {
        ChoreographyManager.Instance.MimicLeader = !ChoreographyManager.Instance.MimicLeader;
        bool ar = RSBot.Core.Kernel.Language == "ar_AR";
        btnMimic.Text = ChoreographyManager.Instance.MimicLeader ? (ar ? "🎭 إلغاء التقليد" : "🎭 Stop Mimic") : (ar ? "🎭 تقليد القائد" : "🎭 Mimic Leader");
        if (chkBroadcastParty.Checked)
        {
            ChoreographyManager.Instance.BroadcastCommand(ChoreographyManager.Instance.MimicLeader ? "!MIMIC" : "!NOMIMIC");
        }
    }

    private void UpdateChoreoStatus()
    {
        txtChoreoStatus.Text = ChoreographyManager.Instance.StatusText;
    }

    private void cboChoreoLeader_DropDown(object sender, EventArgs e)
    {
        cboChoreoLeader.Items.Clear();
        if (Game.Party?.Members != null)
        {
            foreach (var m in Game.Party.Members)
            {
                if (!string.IsNullOrEmpty(m.Name) && !cboChoreoLeader.Items.Contains(m.Name))
                    cboChoreoLeader.Items.Add(m.Name);
            }
        }
        foreach (var l in ControlManager.Instance.Leaders)
        {
            if (!cboChoreoLeader.Items.Contains(l))
                cboChoreoLeader.Items.Add(l);
        }
    }

    private void cboChoreoLeader_TextChanged(object sender, EventArgs e)
    {
        ChoreographyManager.Instance.TargetLeader = cboChoreoLeader.Text.Trim();
    }

    private void cboChoreoRadius_SelectedIndexChanged(object sender, EventArgs e)
    {
        ChoreographyManager.Instance.Radius = cboChoreoRadius.SelectedIndex switch
        {
            0 => 2.0f,
            1 => 4.0f,
            2 => 6.0f,
            3 => 8.0f,
            4 => 10.0f,
            5 => 15.0f,
            _ => 6.0f
        };
    }

    private void cboChoreoSpeed_SelectedIndexChanged(object sender, EventArgs e)
    {
        ChoreographyManager.Instance.IntervalMs = cboChoreoSpeed.SelectedIndex switch
        {
            0 => 400,
            1 => 600,
            2 => 800,
            3 => 1200,
            _ => 600
        };
    }

    private void cboChoreoSlot_SelectedIndexChanged(object sender, EventArgs e)
    {
        ChoreographyManager.Instance.ManualSlotIndex = cboChoreoSlot.SelectedIndex - 1; // 0 = Auto (-1)
    }

    private void chkBroadcastParty_CheckedChanged(object sender, EventArgs e)
    {
        ChoreographyManager.Instance.BroadcastToParty = chkBroadcastParty.Checked;
    }

    #endregion

    #region Tab 3: Target Support

    private void UpdateTargetUI()
    {
        chkTargetEnabled.Checked = TargetSupportManager.Instance.Enabled;
        chkDefensive.Checked = TargetSupportManager.Instance.DefensiveMode;
        chkAutoAttack.Checked = TargetSupportManager.Instance.AutoAttack;

        lstTargetLeaders.BeginUpdate();
        lstTargetLeaders.Items.Clear();
        foreach (var l in TargetSupportManager.Instance.Leaders)
        {
            lstTargetLeaders.Items.Add(new ListViewItem(l));
        }
        lstTargetLeaders.EndUpdate();

        lblTargetStatus.Text = TargetSupportManager.Instance.StatusText;
        bool ar = RSBot.Core.Kernel.Language == "ar_AR";
        if (!string.IsNullOrEmpty(TargetSupportManager.Instance.LastTargetName))
            lblCurrentTarget.Text = $"{TargetSupportManager.Instance.LastTargetName} (UID: {TargetSupportManager.Instance.LastTargetUID})";
        else
            lblCurrentTarget.Text = ar ? "لا يوجد هدف محدد حالياً" : "None";
    }

    private void chkTargetEnabled_CheckedChanged(object sender, EventArgs e)
    {
        TargetSupportManager.Instance.Enabled = chkTargetEnabled.Checked;
        TargetSupportManager.Instance.SaveConfig();
        TargetSupportManager.Instance.NotifyState();
    }

    private void chkDefensive_CheckedChanged(object sender, EventArgs e)
    {
        TargetSupportManager.Instance.DefensiveMode = chkDefensive.Checked;
        TargetSupportManager.Instance.SaveConfig();
    }

    private void chkAutoAttack_CheckedChanged(object sender, EventArgs e)
    {
        TargetSupportManager.Instance.AutoAttack = chkAutoAttack.Checked;
        TargetSupportManager.Instance.SaveConfig();
    }

    private void btnTargetAddLeader_Click(object sender, EventArgs e)
    {
        var name = cboTargetLeaderName.Text.Trim();
        if (!string.IsNullOrEmpty(name))
        {
            TargetSupportManager.Instance.AddLeader(name);
            cboTargetLeaderName.Text = string.Empty;
        }
    }

    private void btnTargetRemoveLeader_Click(object sender, EventArgs e)
    {
        if (lstTargetLeaders.SelectedItems.Count > 0)
        {
            var name = lstTargetLeaders.SelectedItems[0].Text;
            TargetSupportManager.Instance.RemoveLeader(name);
        }
        else if (!string.IsNullOrWhiteSpace(cboTargetLeaderName.Text))
        {
            TargetSupportManager.Instance.RemoveLeader(cboTargetLeaderName.Text.Trim());
        }
    }

    private void btnTargetGetPartyLeader_Click(object sender, EventArgs e)
    {
        if (Game.Party?.Leader != null && !string.IsNullOrEmpty(Game.Party.Leader.Name))
        {
            TargetSupportManager.Instance.AddLeader(Game.Party.Leader.Name);
            ControlManager.Instance.Log($"تمت إضافة ليدر البارتي [{Game.Party.Leader.Name}] كقائد استهداف متزامن");
        }
    }

    private void cboTargetLeaderName_DropDown(object sender, EventArgs e)
    {
        cboTargetLeaderName.Items.Clear();
        if (Game.Party?.Members != null)
        {
            foreach (var m in Game.Party.Members)
            {
                if (!string.IsNullOrEmpty(m.Name) && !cboTargetLeaderName.Items.Contains(m.Name))
                    cboTargetLeaderName.Items.Add(m.Name);
            }
        }
        foreach (var l in ControlManager.Instance.Leaders)
        {
            if (!cboTargetLeaderName.Items.Contains(l))
                cboTargetLeaderName.Items.Add(l);
        }
    }

    #endregion

    #region Tab 4: Commands Guide

    private void PopulateCategoryDropdown()
    {
        cboCategory.Items.Clear();
        cboCategory.Items.Add("جميع التصنيفات (All Categories)");

        var categories = AllCommands.Select(c => c.Category).Distinct().OrderBy(c => c);
        foreach (var cat in categories)
            cboCategory.Items.Add(cat);

        cboCategory.SelectedIndex = 0;
    }

    private void FilterCommands()
    {
        var search = txtSearch.Text.Trim();
        var selectedCategory = cboCategory.SelectedIndex > 0 ? cboCategory.SelectedItem as string : null;

        var filtered = AllCommands.Where(cmd =>
        {
            if (!string.IsNullOrEmpty(selectedCategory) && cmd.Category != selectedCategory)
                return false;

            if (string.IsNullOrEmpty(search))
                return true;

            return cmd.Command.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                   cmd.Category.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                   cmd.DescriptionAr.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                   cmd.DescriptionEn.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                   cmd.Example.Contains(search, StringComparison.OrdinalIgnoreCase);
        }).ToList();

        lstCommands.BeginUpdate();
        lstCommands.Items.Clear();

        foreach (var c in filtered)
        {
            var item = new ListViewItem(c.Command);
            item.SubItems.Add(c.Channel);
            item.SubItems.Add(c.Category);
            item.SubItems.Add(c.DescriptionAr);
            item.SubItems.Add(c.DescriptionEn);
            item.SubItems.Add(c.Example);
            lstCommands.Items.Add(item);
        }

        lstCommands.EndUpdate();
    }

    private void txtSearch_TextChanged(object sender, EventArgs e)
    {
        FilterCommands();
    }

    private void cboCategory_SelectedIndexChanged(object sender, EventArgs e)
    {
        FilterCommands();
    }

    private void btnResetSearch_Click(object sender, EventArgs e)
    {
        txtSearch.Text = string.Empty;
        if (cboCategory.Items.Count > 0)
            cboCategory.SelectedIndex = 0;
    }

    #endregion
    public void ApplyLanguage()
    {
        bool ar = RSBot.Core.Kernel.Language == "ar_AR";

        tabLeaders.Text = ar ? "أوامر التحكم" : "Control Commands";
        groupSettings.Text = ar ? "الإعدادات العامة" : "General Settings";
        chkEnabled.Text = ar ? "تفعيل الاستجابة للأوامر عبر الشات" : "Enable Chat Commands";
        lblCommandsGuide.Text = ar ? "تأكد من تفعيل الأمر عبر كتابة الأوامر في شات الحزب أو الشات العام: START, STOP, TRACE, FOLLOW..." : "Commands accepted in Party or All chat: START, STOP, TRACE, FOLLOW, BUFF...";

        groupLeaders.Text = ar ? "قائمة القادة المصرح لهم" : "Authorized Leaders";
        lblLeader.Text = ar ? "اسم القائد:" : "Leader Name:";
        btnAddLeader.Text = ar ? "إضافة" : "Add";
        btnRemoveLeader.Text = ar ? "حذف" : "Remove";
        btnGetPartyLeader.Text = ar ? "جلب قائد الفريق الحالي تلقائياً" : "Auto-add Party Leader";
        colLeaderName.Text = ar ? "اسم القائد" : "Leader Name";

        groupLogs.Text = ar ? "سجل الأوامر المنفذة" : "Commands Execution Log";
        btnClearLogs.Text = ar ? "مسح السجل" : "Clear Log";

        tabChoreography.Text = ar ? "التشكيلات والاستعراض" : "Choreography";
        groupFormations.Text = ar ? "التشكيلات الحركية" : "Moving Formations";
        btnOrbit.Text = "🌀 Orbit";
        btnCounterOrbit.Text = "🔄 2X Orbit";
        btnLine.Text = "📏 Line";
        btnWedge.Text = "🔺 Wedge (V)";
        btnCircle.Text = "⭕ Circle";
        btnSquare.Text = "🔲 Square";
        btnColumn.Text = "🏛️ Column";
        btnPulse.Text = "💓 Pulse";

        groupEmotes.Text = ar ? "الحركات الاستعراضية والتجمع" : "Ghost Stack & Emotes";
        btnStack.Text = ar ? "🌀 تجسيد شخص واحد (Stack)" : "🌀 Ghost Stack";
        btnScatter.Text = ar ? "💥 تفرق سريع" : "💥 Scatter";
        btnCome.Text = ar ? "🏃 التجمع عند القائد" : "🏃 Come to Leader";
        btnMimic.Text = ChoreographyManager.Instance.MimicLeader ? (ar ? "🎭 إلغاء التقليد" : "🎭 Stop Mimic") : (ar ? "🎭 تقليد القائد" : "🎭 Mimic Leader");
        btnDance.Text = ar ? "💃 رقص" : "💃 Dance";
        btnWave.Text = ar ? "👋 تحية" : "👋 Wave";
        btnCheer.Text = ar ? "🎉 هتاف" : "🎉 Cheer";
        btnBow.Text = ar ? "🙇 انحناء" : "🙇 Bow";
        btnClap.Text = ar ? "👏 تصفيق" : "👏 Clap";
        btnHalt.Text = ar ? "🛑 إيقاف التشكيل" : "🛑 Halt Formation";

        groupChoreoSettings.Text = ar ? "إعدادات مزامنة التشكيل" : "Choreography Settings";
        lblChoreoLeader.Text = ar ? "قائد التشكيل:" : "Formation Leader:";
        lblChoreoRadius.Text = ar ? "نصف القطر (م):" : "Radius (m):";
        lblChoreoSpeed.Text = ar ? "سرعة التحديث:" : "Update Speed:";
        lblChoreoSlot.Text = ar ? "موقع التشكيل:" : "Slot Position:";
        chkBroadcastParty.Text = ar ? "بث أوامر التشكيل لبقية أعضاء الفريق" : "Broadcast commands to party members";
        lblChoreoStatusTitle.Text = ar ? "حالة الكوريوجرافي الحالية:" : "Current Status:";

        tabTargetSupport.Text = ar ? "مساعدة الاستهداف" : "Target Support";
        groupTargetSettings.Text = ar ? "إعدادات مزامنة الأهداف" : "Target Synchronization";
        chkTargetEnabled.Text = ar ? "تفعيل مساندة الضرب مع القائد" : "Enable Target Support";
        chkDefensive.Text = ar ? "الوضع الدفاعي (الهجوم فقط عند استهداف القائد)" : "Defensive Mode (Only if target attacks leader)";
        chkAutoAttack.Text = ar ? "بدء الهجوم تلقائياً فور تحديد القائد للهدف" : "Auto-attack when leader selects target";
        lblTargetDescription.Text = ar 
            ? "أوامر الشات السريعة:\n• TARGET ON - تفعيل المزامنة\n• TARGET OFF - إيقاف المزامنة"
            : "Chat Commands:\n• TARGET ON - Enable support\n• TARGET OFF - Disable support";

        groupTargetLeaders.Text = ar ? "قادة الاستهداف" : "Target Support Leaders";
        lblTargetLeaderPrompt.Text = ar ? "اسم القائد:" : "Leader Name:";
        btnTargetAddLeader.Text = ar ? "إضافة" : "Add";
        btnTargetRemoveLeader.Text = ar ? "حذف" : "Remove";
        btnTargetGetPartyLeader.Text = ar ? "إضافة قائد الفريق الحالي كقائد استهداف" : "Add current party leader";
        colTargetLeaderName.Text = ar ? "اسم القائد المصرح له" : "Target Leader Name";

        groupTargetStatus.Text = ar ? "حالة الاستهداف المباشرة" : "Live Target Status";
        lblTargetStatusTitle.Text = ar ? "حالة المزامنة:" : "Status:";
        lblCurrentTargetTitle.Text = ar ? "الهدف الحالي:" : "Current Target:";

        tabCommandsGuide.Text = ar ? "دليل الأوامر" : "Commands Guide";
        lblSearch.Text = ar ? "بحث:" : "Search:";
        lblCategory.Text = ar ? "التصنيف:" : "Category:";
        btnResetSearch.Text = ar ? "إعادة ضبط" : "Reset Filter";

        colCmdName.Text = ar ? "الأمر" : "Command";
        colCmdChannel.Text = ar ? "القناة" : "Channel";
        colCmdCategory.Text = ar ? "التصنيف" : "Category";
        colCmdDescAr.Text = ar ? "الوصف بالعربي" : "Arabic Description";
        colCmdDescEn.Text = "English Description";
        colCmdExample.Text = ar ? "مثال" : "Example";

        UpdateTargetUI();
    }

}