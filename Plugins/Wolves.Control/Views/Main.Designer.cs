namespace Wolves.Control.Views
{
    partial class Main
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Component Designer generated code

        private void InitializeComponent()
        {
            tabControl = new SDUI.Controls.TabControl();
            tabLeaders = new System.Windows.Forms.TabPage();
            tabChoreography = new System.Windows.Forms.TabPage();
            tabTargetSupport = new System.Windows.Forms.TabPage();
            tabCommandsGuide = new System.Windows.Forms.TabPage();

            // Tab 1: Leaders & General Control
            groupSettings = new SDUI.Controls.GroupBox();
            chkEnabled = new SDUI.Controls.CheckBox();
            lblCommandsGuide = new SDUI.Controls.Label();

            groupLeaders = new SDUI.Controls.GroupBox();
            lblLeader = new SDUI.Controls.Label();
            cboLeaderName = new SDUI.Controls.ComboBox();
            btnAddLeader = new SDUI.Controls.Button();
            btnRemoveLeader = new SDUI.Controls.Button();
            btnGetPartyLeader = new SDUI.Controls.Button();
            lstLeaders = new SDUI.Controls.ListView();
            colLeaderName = new System.Windows.Forms.ColumnHeader();

            groupLogs = new SDUI.Controls.GroupBox();
            txtLogs = new System.Windows.Forms.RichTextBox();
            btnClearLogs = new SDUI.Controls.Button();

            // Tab 2: Choreography Controls
            groupFormations = new SDUI.Controls.GroupBox();
            btnOrbit = new SDUI.Controls.Button();
            btnCounterOrbit = new SDUI.Controls.Button();
            btnLine = new SDUI.Controls.Button();
            btnWedge = new SDUI.Controls.Button();
            btnCircle = new SDUI.Controls.Button();
            btnSquare = new SDUI.Controls.Button();
            btnColumn = new SDUI.Controls.Button();
            btnPulse = new SDUI.Controls.Button();

            groupEmotes = new SDUI.Controls.GroupBox();
            btnDance = new SDUI.Controls.Button();
            btnWave = new SDUI.Controls.Button();
            btnCheer = new SDUI.Controls.Button();
            btnBow = new SDUI.Controls.Button();
            btnClap = new SDUI.Controls.Button();
            btnHalt = new SDUI.Controls.Button();
            btnStack = new SDUI.Controls.Button();
            btnScatter = new SDUI.Controls.Button();
            btnCome = new SDUI.Controls.Button();
            btnMimic = new SDUI.Controls.Button();

            groupChoreoSettings = new SDUI.Controls.GroupBox();
            lblChoreoLeader = new SDUI.Controls.Label();
            cboChoreoLeader = new SDUI.Controls.ComboBox();
            lblChoreoRadius = new SDUI.Controls.Label();
            cboChoreoRadius = new SDUI.Controls.ComboBox();
            lblChoreoSpeed = new SDUI.Controls.Label();
            cboChoreoSpeed = new SDUI.Controls.ComboBox();
            lblChoreoSlot = new SDUI.Controls.Label();
            cboChoreoSlot = new SDUI.Controls.ComboBox();
            chkBroadcastParty = new SDUI.Controls.CheckBox();
            lblChoreoStatusTitle = new SDUI.Controls.Label();
            txtChoreoStatus = new System.Windows.Forms.RichTextBox();

            // Tab 3: Target Support Controls
            groupTargetSettings = new SDUI.Controls.GroupBox();
            chkTargetEnabled = new SDUI.Controls.CheckBox();
            chkDefensive = new SDUI.Controls.CheckBox();
            chkAutoAttack = new SDUI.Controls.CheckBox();
            lblTargetDescription = new SDUI.Controls.Label();

            groupTargetLeaders = new SDUI.Controls.GroupBox();
            lblTargetLeaderPrompt = new SDUI.Controls.Label();
            cboTargetLeaderName = new SDUI.Controls.ComboBox();
            btnTargetAddLeader = new SDUI.Controls.Button();
            btnTargetRemoveLeader = new SDUI.Controls.Button();
            btnTargetGetPartyLeader = new SDUI.Controls.Button();
            lstTargetLeaders = new SDUI.Controls.ListView();
            colTargetLeaderName = new System.Windows.Forms.ColumnHeader();

            groupTargetStatus = new SDUI.Controls.GroupBox();
            lblTargetStatusTitle = new SDUI.Controls.Label();
            lblTargetStatus = new SDUI.Controls.Label();
            lblCurrentTargetTitle = new SDUI.Controls.Label();
            lblCurrentTarget = new SDUI.Controls.Label();

            // Tab 4: Guide controls
            panelGuideTop = new SDUI.Controls.Panel();
            lblSearch = new SDUI.Controls.Label();
            txtSearch = new SDUI.Controls.TextBox();
            lblCategory = new SDUI.Controls.Label();
            cboCategory = new SDUI.Controls.ComboBox();
            btnResetSearch = new SDUI.Controls.Button();
            lstCommands = new SDUI.Controls.ListView();
            colCmdName = new System.Windows.Forms.ColumnHeader();
            colCmdChannel = new System.Windows.Forms.ColumnHeader();
            colCmdCategory = new System.Windows.Forms.ColumnHeader();
            colCmdDescAr = new System.Windows.Forms.ColumnHeader();
            colCmdDescEn = new System.Windows.Forms.ColumnHeader();
            colCmdExample = new System.Windows.Forms.ColumnHeader();

            tabControl.SuspendLayout();
            tabLeaders.SuspendLayout();
            tabChoreography.SuspendLayout();
            tabTargetSupport.SuspendLayout();
            tabCommandsGuide.SuspendLayout();
            groupSettings.SuspendLayout();
            groupLeaders.SuspendLayout();
            groupLogs.SuspendLayout();
            groupFormations.SuspendLayout();
            groupEmotes.SuspendLayout();
            groupChoreoSettings.SuspendLayout();
            groupTargetSettings.SuspendLayout();
            groupTargetLeaders.SuspendLayout();
            groupTargetStatus.SuspendLayout();
            panelGuideTop.SuspendLayout();
            SuspendLayout();

            // 
            // tabControl
            // 
            tabControl.Controls.Add(tabLeaders);
            tabControl.Controls.Add(tabChoreography);
            tabControl.Controls.Add(tabTargetSupport);
            tabControl.Controls.Add(tabCommandsGuide);
            tabControl.Dock = System.Windows.Forms.DockStyle.Fill;
            tabControl.Location = new System.Drawing.Point(0, 0);
            tabControl.Name = "tabControl";
            tabControl.SelectedIndex = 0;
            tabControl.Size = new System.Drawing.Size(940, 480);
            tabControl.TabIndex = 0;

            // 
            // tabLeaders
            // 
            tabLeaders.Controls.Add(groupSettings);
            tabLeaders.Controls.Add(groupLeaders);
            tabLeaders.Controls.Add(groupLogs);
            tabLeaders.Location = new System.Drawing.Point(4, 24);
            tabLeaders.Name = "tabLeaders";
            tabLeaders.Padding = new System.Windows.Forms.Padding(3);
            tabLeaders.Size = new System.Drawing.Size(932, 452);
            tabLeaders.Text = "التحكم والأوامر (Control)";
            tabLeaders.UseVisualStyleBackColor = true;

            // groupSettings
            groupSettings.Controls.Add(chkEnabled);
            groupSettings.Controls.Add(lblCommandsGuide);
            groupSettings.Location = new System.Drawing.Point(6, 6);
            groupSettings.Name = "groupSettings";
            groupSettings.Size = new System.Drawing.Size(430, 95);
            groupSettings.TabIndex = 0;
            groupSettings.TabStop = false;
            groupSettings.Text = "الإعدادات العامة (General Settings)";

            // chkEnabled
            chkEnabled.AutoSize = false;
            chkEnabled.Location = new System.Drawing.Point(12, 22);
            chkEnabled.Name = "chkEnabled";
            chkEnabled.Size = new System.Drawing.Size(400, 24);
            chkEnabled.TabIndex = 0;
            chkEnabled.Text = "تفعيل الاستجابة لأوامر التحكم من القادة (Enable Commands)";
            chkEnabled.UseVisualStyleBackColor = true;
            chkEnabled.CheckedChanged += chkEnabled_CheckedChanged;

            // lblCommandsGuide
            lblCommandsGuide.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            lblCommandsGuide.ForeColor = System.Drawing.Color.FromArgb(16, 185, 129);
            lblCommandsGuide.Location = new System.Drawing.Point(12, 50);
            lblCommandsGuide.Name = "lblCommandsGuide";
            lblCommandsGuide.Size = new System.Drawing.Size(405, 38);
            lblCommandsGuide.TabIndex = 1;
            lblCommandsGuide.Text = "يدعم الأوامر عن بعد عبر شات البارتي أو الهمس الخاص: START, STOP, TRACE, FOLLOW, RETURN, ZERK, DIS, TARGET ON/OFF...";

            // groupLeaders
            groupLeaders.Controls.Add(lblLeader);
            groupLeaders.Controls.Add(cboLeaderName);
            groupLeaders.Controls.Add(btnAddLeader);
            groupLeaders.Controls.Add(btnRemoveLeader);
            groupLeaders.Controls.Add(btnGetPartyLeader);
            groupLeaders.Controls.Add(lstLeaders);
            groupLeaders.Location = new System.Drawing.Point(6, 107);
            groupLeaders.Name = "groupLeaders";
            groupLeaders.Size = new System.Drawing.Size(430, 335);
            groupLeaders.TabIndex = 1;
            groupLeaders.TabStop = false;
            groupLeaders.Text = "قائمة القادة المصرح لهم (Authorized Leaders)";

            // lblLeader
            lblLeader.AutoSize = true;
            lblLeader.Location = new System.Drawing.Point(10, 24);
            lblLeader.Name = "lblLeader";
            lblLeader.Size = new System.Drawing.Size(65, 15);
            lblLeader.TabIndex = 0;
            lblLeader.Text = "اسم القائد:";

            // cboLeaderName
            cboLeaderName.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDown;
            cboLeaderName.FormattingEnabled = true;
            cboLeaderName.Location = new System.Drawing.Point(80, 20);
            cboLeaderName.Name = "cboLeaderName";
            cboLeaderName.Size = new System.Drawing.Size(180, 23);
            cboLeaderName.TabIndex = 1;
            cboLeaderName.DropDown += cboLeaderName_DropDown;

            // btnAddLeader
            btnAddLeader.Location = new System.Drawing.Point(268, 18);
            btnAddLeader.Name = "btnAddLeader";
            btnAddLeader.Size = new System.Drawing.Size(72, 26);
            btnAddLeader.TabIndex = 2;
            btnAddLeader.Text = "إضافة";
            btnAddLeader.UseVisualStyleBackColor = true;
            btnAddLeader.Click += btnAddLeader_Click;

            // btnRemoveLeader
            btnRemoveLeader.Location = new System.Drawing.Point(346, 18);
            btnRemoveLeader.Name = "btnRemoveLeader";
            btnRemoveLeader.Size = new System.Drawing.Size(72, 26);
            btnRemoveLeader.TabIndex = 3;
            btnRemoveLeader.Text = "حذف";
            btnRemoveLeader.UseVisualStyleBackColor = true;
            btnRemoveLeader.Click += btnRemoveLeader_Click;

            // btnGetPartyLeader
            btnGetPartyLeader.Location = new System.Drawing.Point(10, 52);
            btnGetPartyLeader.Name = "btnGetPartyLeader";
            btnGetPartyLeader.Size = new System.Drawing.Size(408, 26);
            btnGetPartyLeader.TabIndex = 4;
            btnGetPartyLeader.Text = "جلب ليدر البارتي الحالي تلقائياً";
            btnGetPartyLeader.UseVisualStyleBackColor = true;
            btnGetPartyLeader.Click += btnGetPartyLeader_Click;

            // lstLeaders
            lstLeaders.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] { colLeaderName });
            lstLeaders.FullRowSelect = true;
            lstLeaders.Location = new System.Drawing.Point(10, 85);
            lstLeaders.Name = "lstLeaders";
            lstLeaders.Size = new System.Drawing.Size(408, 240);
            lstLeaders.TabIndex = 5;
            lstLeaders.UseCompatibleStateImageBehavior = false;
            lstLeaders.View = System.Windows.Forms.View.Details;

            // colLeaderName
            colLeaderName.Text = "اسم القائد (Leader Name)";
            colLeaderName.Width = 380;

            // groupLogs
            groupLogs.Controls.Add(txtLogs);
            groupLogs.Controls.Add(btnClearLogs);
            groupLogs.Location = new System.Drawing.Point(445, 6);
            groupLogs.Name = "groupLogs";
            groupLogs.Size = new System.Drawing.Size(480, 436);
            groupLogs.TabIndex = 2;
            groupLogs.TabStop = false;
            groupLogs.Text = "سجل الأوامر المنفذة (Commands Execution Log)";

            // txtLogs
            txtLogs.BackColor = System.Drawing.Color.FromArgb(20, 26, 36);
            txtLogs.BorderStyle = System.Windows.Forms.BorderStyle.None;
            txtLogs.ForeColor = System.Drawing.Color.FromArgb(230, 235, 245);
            txtLogs.Location = new System.Drawing.Point(10, 22);
            txtLogs.Name = "txtLogs";
            txtLogs.ReadOnly = true;
            txtLogs.Size = new System.Drawing.Size(460, 375);
            txtLogs.TabIndex = 0;
            txtLogs.Text = "";

            // btnClearLogs
            btnClearLogs.Location = new System.Drawing.Point(375, 403);
            btnClearLogs.Name = "btnClearLogs";
            btnClearLogs.Size = new System.Drawing.Size(95, 26);
            btnClearLogs.TabIndex = 1;
            btnClearLogs.Text = "مسح السجل";
            btnClearLogs.UseVisualStyleBackColor = true;
            btnClearLogs.Click += btnClearLogs_Click;

            // 
            // tabChoreography
            // 
            tabChoreography.Controls.Add(groupFormations);
            tabChoreography.Controls.Add(groupEmotes);
            tabChoreography.Controls.Add(groupChoreoSettings);
            tabChoreography.Location = new System.Drawing.Point(4, 24);
            tabChoreography.Name = "tabChoreography";
            tabChoreography.Padding = new System.Windows.Forms.Padding(3);
            tabChoreography.Size = new System.Drawing.Size(932, 452);
            tabChoreography.Text = "التشكيلات والاستعراض (Choreography)";
            tabChoreography.UseVisualStyleBackColor = true;

            // groupFormations
            groupFormations.Controls.Add(btnOrbit);
            groupFormations.Controls.Add(btnCounterOrbit);
            groupFormations.Controls.Add(btnLine);
            groupFormations.Controls.Add(btnWedge);
            groupFormations.Controls.Add(btnCircle);
            groupFormations.Controls.Add(btnSquare);
            groupFormations.Controls.Add(btnColumn);
            groupFormations.Controls.Add(btnPulse);
            groupFormations.Location = new System.Drawing.Point(6, 6);
            groupFormations.Name = "groupFormations";
            groupFormations.Size = new System.Drawing.Size(450, 155);
            groupFormations.TabIndex = 0;
            groupFormations.TabStop = false;
            groupFormations.Text = "التشكيلات الحركية (Moving Formations)";

            btnOrbit.Location = new System.Drawing.Point(10, 24);
            btnOrbit.Name = "btnOrbit";
            btnOrbit.Size = new System.Drawing.Size(100, 32);
            btnOrbit.TabIndex = 0;
            btnOrbit.Text = "🔄 دوران Orbit";
            btnOrbit.Click += (s, e) => StartFormation(FormationType.Orbit);

            btnCounterOrbit.Location = new System.Drawing.Point(118, 24);
            btnCounterOrbit.Name = "btnCounterOrbit";
            btnCounterOrbit.Size = new System.Drawing.Size(100, 32);
            btnCounterOrbit.TabIndex = 1;
            btnCounterOrbit.Text = "🌀 حلزوني 2X";
            btnCounterOrbit.Click += (s, e) => StartFormation(FormationType.CounterOrbit);

            btnLine.Location = new System.Drawing.Point(226, 24);
            btnLine.Name = "btnLine";
            btnLine.Size = new System.Drawing.Size(100, 32);
            btnLine.TabIndex = 2;
            btnLine.Text = "━ صف أفقي Line";
            btnLine.Click += (s, e) => StartFormation(FormationType.Line);

            btnWedge.Location = new System.Drawing.Point(334, 24);
            btnWedge.Name = "btnWedge";
            btnWedge.Size = new System.Drawing.Size(100, 32);
            btnWedge.TabIndex = 3;
            btnWedge.Text = "🔻 رأس سهم V";
            btnWedge.Click += (s, e) => StartFormation(FormationType.Wedge);

            btnCircle.Location = new System.Drawing.Point(10, 65);
            btnCircle.Name = "btnCircle";
            btnCircle.Size = new System.Drawing.Size(100, 32);
            btnCircle.TabIndex = 4;
            btnCircle.Text = "⭕ حلقة Circle";
            btnCircle.Click += (s, e) => StartFormation(FormationType.Circle);

            btnSquare.Location = new System.Drawing.Point(118, 65);
            btnSquare.Name = "btnSquare";
            btnSquare.Size = new System.Drawing.Size(100, 32);
            btnSquare.TabIndex = 5;
            btnSquare.Text = "⏹️ مربع Square";
            btnSquare.Click += (s, e) => StartFormation(FormationType.Square);

            btnColumn.Location = new System.Drawing.Point(226, 65);
            btnColumn.Name = "btnColumn";
            btnColumn.Size = new System.Drawing.Size(100, 32);
            btnColumn.TabIndex = 6;
            btnColumn.Text = "❙ طابور Column";
            btnColumn.Click += (s, e) => StartFormation(FormationType.Column);

            btnPulse.Location = new System.Drawing.Point(334, 65);
            btnPulse.Name = "btnPulse";
            btnPulse.Size = new System.Drawing.Size(100, 32);
            btnPulse.TabIndex = 7;
            btnPulse.Text = "💓 نبض Pulse";
            btnPulse.Click += (s, e) => StartFormation(FormationType.Pulse);

            // groupEmotes
            groupEmotes.Controls.Add(btnDance);
            groupEmotes.Controls.Add(btnWave);
            groupEmotes.Controls.Add(btnCheer);
            groupEmotes.Controls.Add(btnBow);
            groupEmotes.Controls.Add(btnClap);
            groupEmotes.Controls.Add(btnHalt);
            groupEmotes.Controls.Add(btnStack);
            groupEmotes.Controls.Add(btnScatter);
            groupEmotes.Controls.Add(btnCome);
            groupEmotes.Controls.Add(btnMimic);
            groupEmotes.Location = new System.Drawing.Point(6, 168);
            groupEmotes.Name = "groupEmotes";
            groupEmotes.Size = new System.Drawing.Size(450, 275);
            groupEmotes.TabIndex = 1;
            groupEmotes.TabStop = false;
            groupEmotes.Text = "الحركات الاستعراضية والتطابق (Ghost Stack & Emotes)";

            btnStack.Location = new System.Drawing.Point(10, 25);
            btnStack.Name = "btnStack";
            btnStack.Size = new System.Drawing.Size(208, 36);
            btnStack.TabIndex = 0;
            btnStack.Text = "👤 المشي داخل بعض (Ghost Stack)";
            btnStack.Click += (s, e) => StartFormation(FormationType.Stack, 0f);

            btnScatter.Location = new System.Drawing.Point(226, 25);
            btnScatter.Name = "btnScatter";
            btnScatter.Size = new System.Drawing.Size(208, 36);
            btnScatter.TabIndex = 1;
            btnScatter.Text = "💥 تشتت عشوائي (Scatter)";
            btnScatter.Click += (s, e) => StartFormation(FormationType.Scatter);

            btnCome.Location = new System.Drawing.Point(10, 68);
            btnCome.Name = "btnCome";
            btnCome.Size = new System.Drawing.Size(208, 34);
            btnCome.TabIndex = 2;
            btnCome.Text = "🏃 اجتماع عند الليدر (Come / GEL)";
            btnCome.Click += (s, e) => ChoreographyManager.Instance.ComeToLeader();

            btnMimic.Location = new System.Drawing.Point(226, 68);
            btnMimic.Name = "btnMimic";
            btnMimic.Size = new System.Drawing.Size(208, 34);
            btnMimic.TabIndex = 3;
            btnMimic.Text = "🎭 تقليد القائد (Mimic Leader)";
            btnMimic.Click += btnMimic_Click;

            btnDance.Location = new System.Drawing.Point(10, 110);
            btnDance.Name = "btnDance";
            btnDance.Size = new System.Drawing.Size(100, 32);
            btnDance.TabIndex = 4;
            btnDance.Text = "💃 رقص Dance";
            btnDance.Click += (s, e) => TriggerEmote(EmoteActionType.Dance);

            btnWave.Location = new System.Drawing.Point(118, 110);
            btnWave.Name = "btnWave";
            btnWave.Size = new System.Drawing.Size(100, 32);
            btnWave.TabIndex = 5;
            btnWave.Text = "🌊 موجة Wave";
            btnWave.Click += (s, e) => ChoreographyManager.Instance.PerformEmoteWave(EmoteActionType.Cheer);

            btnCheer.Location = new System.Drawing.Point(226, 110);
            btnCheer.Name = "btnCheer";
            btnCheer.Size = new System.Drawing.Size(100, 32);
            btnCheer.TabIndex = 6;
            btnCheer.Text = "🙌 هتاف Cheer";
            btnCheer.Click += (s, e) => TriggerEmote(EmoteActionType.Cheer);

            btnBow.Location = new System.Drawing.Point(334, 110);
            btnBow.Name = "btnBow";
            btnBow.Size = new System.Drawing.Size(100, 32);
            btnBow.TabIndex = 7;
            btnBow.Text = "🙇 انحناءة Bow";
            btnBow.Click += (s, e) => TriggerEmote(EmoteActionType.Bow);

            btnClap.Location = new System.Drawing.Point(10, 150);
            btnClap.Name = "btnClap";
            btnClap.Size = new System.Drawing.Size(208, 32);
            btnClap.TabIndex = 8;
            btnClap.Text = "👏 تصفيق جماعي (Clap)";
            btnClap.Click += (s, e) => TriggerEmote(EmoteActionType.Clap);

            btnHalt.Color = System.Drawing.Color.FromArgb(215, 55, 50);
            btnHalt.ForeColor = System.Drawing.Color.White;
            btnHalt.Location = new System.Drawing.Point(226, 150);
            btnHalt.Name = "btnHalt";
            btnHalt.Size = new System.Drawing.Size(208, 32);
            btnHalt.TabIndex = 9;
            btnHalt.Text = "🛑 إيقاف التشكيل (HALT)";
            btnHalt.UseVisualStyleBackColor = false;
            btnHalt.Click += (s, e) => ChoreographyManager.Instance.StopFormation();

            // groupChoreoSettings
            groupChoreoSettings.Controls.Add(lblChoreoLeader);
            groupChoreoSettings.Controls.Add(cboChoreoLeader);
            groupChoreoSettings.Controls.Add(lblChoreoRadius);
            groupChoreoSettings.Controls.Add(cboChoreoRadius);
            groupChoreoSettings.Controls.Add(lblChoreoSpeed);
            groupChoreoSettings.Controls.Add(cboChoreoSpeed);
            groupChoreoSettings.Controls.Add(lblChoreoSlot);
            groupChoreoSettings.Controls.Add(cboChoreoSlot);
            groupChoreoSettings.Controls.Add(chkBroadcastParty);
            groupChoreoSettings.Controls.Add(lblChoreoStatusTitle);
            groupChoreoSettings.Controls.Add(txtChoreoStatus);
            groupChoreoSettings.Location = new System.Drawing.Point(465, 6);
            groupChoreoSettings.Name = "groupChoreoSettings";
            groupChoreoSettings.Size = new System.Drawing.Size(460, 437);
            groupChoreoSettings.TabIndex = 2;
            groupChoreoSettings.TabStop = false;
            groupChoreoSettings.Text = "إعدادات العرض والحالة (Choreography Settings)";

            lblChoreoLeader.AutoSize = true;
            lblChoreoLeader.Location = new System.Drawing.Point(12, 28);
            lblChoreoLeader.Name = "lblChoreoLeader";
            lblChoreoLeader.Size = new System.Drawing.Size(85, 15);
            lblChoreoLeader.TabIndex = 0;
            lblChoreoLeader.Text = "قائد التشكيل:";

            cboChoreoLeader.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDown;
            cboChoreoLeader.FormattingEnabled = true;
            cboChoreoLeader.Location = new System.Drawing.Point(105, 24);
            cboChoreoLeader.Name = "cboChoreoLeader";
            cboChoreoLeader.Size = new System.Drawing.Size(200, 23);
            cboChoreoLeader.TabIndex = 1;
            cboChoreoLeader.DropDown += cboChoreoLeader_DropDown;
            cboChoreoLeader.TextChanged += cboChoreoLeader_TextChanged;

            lblChoreoRadius.AutoSize = true;
            lblChoreoRadius.Location = new System.Drawing.Point(12, 62);
            lblChoreoRadius.Name = "lblChoreoRadius";
            lblChoreoRadius.Size = new System.Drawing.Size(80, 15);
            lblChoreoRadius.TabIndex = 2;
            lblChoreoRadius.Text = "نصف القطر (م):";

            cboChoreoRadius.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            cboChoreoRadius.FormattingEnabled = true;
            cboChoreoRadius.Items.AddRange(new object[] { "2 متر", "4 متر", "6 متر (مثالي)", "8 متر", "10 متر", "15 متر" });
            cboChoreoRadius.Location = new System.Drawing.Point(105, 58);
            cboChoreoRadius.Name = "cboChoreoRadius";
            cboChoreoRadius.Size = new System.Drawing.Size(200, 23);
            cboChoreoRadius.TabIndex = 3;
            cboChoreoRadius.SelectedIndex = 2;
            cboChoreoRadius.SelectedIndexChanged += cboChoreoRadius_SelectedIndexChanged;

            lblChoreoSpeed.AutoSize = true;
            lblChoreoSpeed.Location = new System.Drawing.Point(12, 96);
            lblChoreoSpeed.Name = "lblChoreoSpeed";
            lblChoreoSpeed.Size = new System.Drawing.Size(75, 15);
            lblChoreoSpeed.TabIndex = 4;
            lblChoreoSpeed.Text = "سرعة التحديث:";

            cboChoreoSpeed.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            cboChoreoSpeed.FormattingEnabled = true;
            cboChoreoSpeed.Items.AddRange(new object[] { "سريع جداً (400ms)", "سريع (600ms)", "عادي (800ms)", "هادئ (1200ms)" });
            cboChoreoSpeed.Location = new System.Drawing.Point(105, 92);
            cboChoreoSpeed.Name = "cboChoreoSpeed";
            cboChoreoSpeed.Size = new System.Drawing.Size(200, 23);
            cboChoreoSpeed.TabIndex = 5;
            cboChoreoSpeed.SelectedIndex = 1;
            cboChoreoSpeed.SelectedIndexChanged += cboChoreoSpeed_SelectedIndexChanged;

            lblChoreoSlot.AutoSize = true;
            lblChoreoSlot.Location = new System.Drawing.Point(12, 130);
            lblChoreoSlot.Name = "lblChoreoSlot";
            lblChoreoSlot.Size = new System.Drawing.Size(84, 15);
            lblChoreoSlot.TabIndex = 6;
            lblChoreoSlot.Text = "مقعد الشخصية:";

            cboChoreoSlot.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            cboChoreoSlot.FormattingEnabled = true;
            cboChoreoSlot.Items.AddRange(new object[] { "تلقائي (Auto Slot)", "مقعد 1", "مقعد 2", "مقعد 3", "مقعد 4", "مقعد 5", "مقعد 6", "مقعد 7", "مقعد 8" });
            cboChoreoSlot.Location = new System.Drawing.Point(105, 126);
            cboChoreoSlot.Name = "cboChoreoSlot";
            cboChoreoSlot.Size = new System.Drawing.Size(200, 23);
            cboChoreoSlot.TabIndex = 7;
            cboChoreoSlot.SelectedIndex = 0;
            cboChoreoSlot.SelectedIndexChanged += cboChoreoSlot_SelectedIndexChanged;

            chkBroadcastParty.AutoSize = false;
            chkBroadcastParty.Location = new System.Drawing.Point(15, 162);
            chkBroadcastParty.Name = "chkBroadcastParty";
            chkBroadcastParty.Size = new System.Drawing.Size(430, 26);
            chkBroadcastParty.TabIndex = 8;
            chkBroadcastParty.Text = "بث أوامر التشكيل لشات البارتي عند الضغط على الأزرار (Party Broadcast)";
            chkBroadcastParty.UseVisualStyleBackColor = true;
            chkBroadcastParty.Checked = true;
            chkBroadcastParty.CheckedChanged += chkBroadcastParty_CheckedChanged;

            lblChoreoStatusTitle.AutoSize = true;
            lblChoreoStatusTitle.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            lblChoreoStatusTitle.Location = new System.Drawing.Point(12, 200);
            lblChoreoStatusTitle.Name = "lblChoreoStatusTitle";
            lblChoreoStatusTitle.Size = new System.Drawing.Size(126, 15);
            lblChoreoStatusTitle.TabIndex = 9;
            lblChoreoStatusTitle.Text = "حالة الاستعراض المباشرة:";

            txtChoreoStatus.BackColor = System.Drawing.Color.FromArgb(20, 26, 36);
            txtChoreoStatus.BorderStyle = System.Windows.Forms.BorderStyle.None;
            txtChoreoStatus.ForeColor = System.Drawing.Color.FromArgb(16, 185, 129);
            txtChoreoStatus.Location = new System.Drawing.Point(15, 222);
            txtChoreoStatus.Name = "txtChoreoStatus";
            txtChoreoStatus.ReadOnly = true;
            txtChoreoStatus.Size = new System.Drawing.Size(430, 195);
            txtChoreoStatus.TabIndex = 10;
            txtChoreoStatus.Text = "جاهز للإطلاق (Idle)";

            // 
            // tabTargetSupport
            // 
            tabTargetSupport.Controls.Add(groupTargetSettings);
            tabTargetSupport.Controls.Add(groupTargetLeaders);
            tabTargetSupport.Controls.Add(groupTargetStatus);
            tabTargetSupport.Location = new System.Drawing.Point(4, 24);
            tabTargetSupport.Name = "tabTargetSupport";
            tabTargetSupport.Padding = new System.Windows.Forms.Padding(3);
            tabTargetSupport.Size = new System.Drawing.Size(932, 452);
            tabTargetSupport.Text = "مساعد الاستهداف (Target Support)";
            tabTargetSupport.UseVisualStyleBackColor = true;

            // groupTargetSettings
            groupTargetSettings.Controls.Add(chkTargetEnabled);
            groupTargetSettings.Controls.Add(chkDefensive);
            groupTargetSettings.Controls.Add(chkAutoAttack);
            groupTargetSettings.Controls.Add(lblTargetDescription);
            groupTargetSettings.Location = new System.Drawing.Point(6, 6);
            groupTargetSettings.Name = "groupTargetSettings";
            groupTargetSettings.Size = new System.Drawing.Size(430, 200);
            groupTargetSettings.TabIndex = 0;
            groupTargetSettings.TabStop = false;
            groupTargetSettings.Text = "إعدادات مزامنة الهدف (Target Synchronization)";

            chkTargetEnabled.AutoSize = false;
            chkTargetEnabled.Location = new System.Drawing.Point(12, 24);
            chkTargetEnabled.Name = "chkTargetEnabled";
            chkTargetEnabled.Size = new System.Drawing.Size(400, 26);
            chkTargetEnabled.TabIndex = 0;
            chkTargetEnabled.Text = "تفعيل مزامنة الهدف مع القائد (Target Support)";
            chkTargetEnabled.UseVisualStyleBackColor = true;
            chkTargetEnabled.CheckedChanged += chkTargetEnabled_CheckedChanged;

            chkDefensive.AutoSize = false;
            chkDefensive.Location = new System.Drawing.Point(12, 56);
            chkDefensive.Name = "chkDefensive";
            chkDefensive.Size = new System.Drawing.Size(400, 26);
            chkDefensive.TabIndex = 1;
            chkDefensive.Text = "الوضع الدفاعي (الدفاع عن القائد واستهداف مهاجمه)";
            chkDefensive.UseVisualStyleBackColor = true;
            chkDefensive.CheckedChanged += chkDefensive_CheckedChanged;

            chkAutoAttack.AutoSize = false;
            chkAutoAttack.Location = new System.Drawing.Point(12, 88);
            chkAutoAttack.Name = "chkAutoAttack";
            chkAutoAttack.Size = new System.Drawing.Size(400, 26);
            chkAutoAttack.TabIndex = 2;
            chkAutoAttack.Text = "هجوم تلقائي فوري على هدف القائد المتزامن";
            chkAutoAttack.UseVisualStyleBackColor = true;
            chkAutoAttack.CheckedChanged += chkAutoAttack_CheckedChanged;

            lblTargetDescription.ForeColor = System.Drawing.Color.FromArgb(16, 185, 129);
            lblTargetDescription.Location = new System.Drawing.Point(12, 122);
            lblTargetDescription.Name = "lblTargetDescription";
            lblTargetDescription.Size = new System.Drawing.Size(400, 68);
            lblTargetDescription.TabIndex = 3;
            lblTargetDescription.Text = "أوامر الشات السريعة للقائد:\n• TARGET ON - تفعيل المزامنة الفورية للهدف\n• TARGET OFF - إيقاف المزامنة\nيقوم البوت باستهداف نفس وحش/لاعب القائد فوراً ومساعدته في القتال.";

            // groupTargetLeaders
            groupTargetLeaders.Controls.Add(lblTargetLeaderPrompt);
            groupTargetLeaders.Controls.Add(cboTargetLeaderName);
            groupTargetLeaders.Controls.Add(btnTargetAddLeader);
            groupTargetLeaders.Controls.Add(btnTargetRemoveLeader);
            groupTargetLeaders.Controls.Add(btnTargetGetPartyLeader);
            groupTargetLeaders.Controls.Add(lstTargetLeaders);
            groupTargetLeaders.Location = new System.Drawing.Point(445, 6);
            groupTargetLeaders.Name = "groupTargetLeaders";
            groupTargetLeaders.Size = new System.Drawing.Size(480, 275);
            groupTargetLeaders.TabIndex = 1;
            groupTargetLeaders.TabStop = false;
            groupTargetLeaders.Text = "قادة الاستهداف (Target Support Leaders)";

            lblTargetLeaderPrompt.AutoSize = true;
            lblTargetLeaderPrompt.Location = new System.Drawing.Point(10, 24);
            lblTargetLeaderPrompt.Name = "lblTargetLeaderPrompt";
            lblTargetLeaderPrompt.Size = new System.Drawing.Size(65, 15);
            lblTargetLeaderPrompt.TabIndex = 0;
            lblTargetLeaderPrompt.Text = "اسم القائد:";

            cboTargetLeaderName.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDown;
            cboTargetLeaderName.FormattingEnabled = true;
            cboTargetLeaderName.Location = new System.Drawing.Point(80, 20);
            cboTargetLeaderName.Name = "cboTargetLeaderName";
            cboTargetLeaderName.Size = new System.Drawing.Size(200, 23);
            cboTargetLeaderName.TabIndex = 1;
            cboTargetLeaderName.DropDown += cboTargetLeaderName_DropDown;

            btnTargetAddLeader.Location = new System.Drawing.Point(290, 18);
            btnTargetAddLeader.Name = "btnTargetAddLeader";
            btnTargetAddLeader.Size = new System.Drawing.Size(85, 26);
            btnTargetAddLeader.TabIndex = 2;
            btnTargetAddLeader.Text = "إضافة";
            btnTargetAddLeader.UseVisualStyleBackColor = true;
            btnTargetAddLeader.Click += btnTargetAddLeader_Click;

            btnTargetRemoveLeader.Location = new System.Drawing.Point(385, 18);
            btnTargetRemoveLeader.Name = "btnTargetRemoveLeader";
            btnTargetRemoveLeader.Size = new System.Drawing.Size(85, 26);
            btnTargetRemoveLeader.TabIndex = 3;
            btnTargetRemoveLeader.Text = "حذف";
            btnTargetRemoveLeader.UseVisualStyleBackColor = true;
            btnTargetRemoveLeader.Click += btnTargetRemoveLeader_Click;

            btnTargetGetPartyLeader.Location = new System.Drawing.Point(10, 52);
            btnTargetGetPartyLeader.Name = "btnTargetGetPartyLeader";
            btnTargetGetPartyLeader.Size = new System.Drawing.Size(460, 26);
            btnTargetGetPartyLeader.TabIndex = 4;
            btnTargetGetPartyLeader.Text = "إضافة ليدر البارتي الحالي كقائد استهداف";
            btnTargetGetPartyLeader.UseVisualStyleBackColor = true;
            btnTargetGetPartyLeader.Click += btnTargetGetPartyLeader_Click;

            lstTargetLeaders.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] { colTargetLeaderName });
            lstTargetLeaders.FullRowSelect = true;
            lstTargetLeaders.Location = new System.Drawing.Point(10, 85);
            lstTargetLeaders.Name = "lstTargetLeaders";
            lstTargetLeaders.Size = new System.Drawing.Size(460, 180);
            lstTargetLeaders.TabIndex = 5;
            lstTargetLeaders.UseCompatibleStateImageBehavior = false;
            lstTargetLeaders.View = System.Windows.Forms.View.Details;

            colTargetLeaderName.Text = "اسم القائد المتزامن معه (Target Leader)";
            colTargetLeaderName.Width = 430;

            // groupTargetStatus
            groupTargetStatus.Controls.Add(lblTargetStatusTitle);
            groupTargetStatus.Controls.Add(lblTargetStatus);
            groupTargetStatus.Controls.Add(lblCurrentTargetTitle);
            groupTargetStatus.Controls.Add(lblCurrentTarget);
            groupTargetStatus.Location = new System.Drawing.Point(6, 212);
            groupTargetStatus.Name = "groupTargetStatus";
            groupTargetStatus.Size = new System.Drawing.Size(430, 230);
            groupTargetStatus.TabIndex = 2;
            groupTargetStatus.TabStop = false;
            groupTargetStatus.Text = "حالة الاستهداف المباشرة (Live Target Status)";

            lblTargetStatusTitle.AutoSize = true;
            lblTargetStatusTitle.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            lblTargetStatusTitle.Location = new System.Drawing.Point(12, 30);
            lblTargetStatusTitle.Name = "lblTargetStatusTitle";
            lblTargetStatusTitle.Size = new System.Drawing.Size(77, 15);
            lblTargetStatusTitle.TabIndex = 0;
            lblTargetStatusTitle.Text = "حالة المزامنة:";

            lblTargetStatus.AutoSize = true;
            lblTargetStatus.ForeColor = System.Drawing.Color.FromArgb(16, 185, 129);
            lblTargetStatus.Location = new System.Drawing.Point(110, 30);
            lblTargetStatus.Name = "lblTargetStatus";
            lblTargetStatus.Size = new System.Drawing.Size(65, 15);
            lblTargetStatus.TabIndex = 1;
            lblTargetStatus.Text = "جاهز (Idle)";

            lblCurrentTargetTitle.AutoSize = true;
            lblCurrentTargetTitle.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            lblCurrentTargetTitle.Location = new System.Drawing.Point(12, 70);
            lblCurrentTargetTitle.Name = "lblCurrentTargetTitle";
            lblCurrentTargetTitle.Size = new System.Drawing.Size(79, 15);
            lblCurrentTargetTitle.TabIndex = 2;
            lblCurrentTargetTitle.Text = "الهدف الحالي:";

            lblCurrentTarget.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            lblCurrentTarget.ForeColor = System.Drawing.Color.DodgerBlue;
            lblCurrentTarget.Location = new System.Drawing.Point(110, 70);
            lblCurrentTarget.Name = "lblCurrentTarget";
            lblCurrentTarget.Size = new System.Drawing.Size(305, 50);
            lblCurrentTarget.TabIndex = 3;
            lblCurrentTarget.Text = "لا يوجد هدف محدد حالياً (None)";

            // 
            // tabCommandsGuide
            // 
            tabCommandsGuide.Controls.Add(lstCommands);
            tabCommandsGuide.Controls.Add(panelGuideTop);
            tabCommandsGuide.Location = new System.Drawing.Point(4, 24);
            tabCommandsGuide.Name = "tabCommandsGuide";
            tabCommandsGuide.Padding = new System.Windows.Forms.Padding(3);
            tabCommandsGuide.Size = new System.Drawing.Size(932, 452);
            tabCommandsGuide.Text = "دليل الأوامر (Commands Guide)";
            tabCommandsGuide.UseVisualStyleBackColor = true;

            // panelGuideTop
            panelGuideTop.Controls.Add(lblSearch);
            panelGuideTop.Controls.Add(txtSearch);
            panelGuideTop.Controls.Add(lblCategory);
            panelGuideTop.Controls.Add(cboCategory);
            panelGuideTop.Controls.Add(btnResetSearch);
            panelGuideTop.Dock = System.Windows.Forms.DockStyle.Top;
            panelGuideTop.Location = new System.Drawing.Point(3, 3);
            panelGuideTop.Name = "panelGuideTop";
            panelGuideTop.Size = new System.Drawing.Size(926, 42);
            panelGuideTop.TabIndex = 0;

            lblSearch.AutoSize = true;
            lblSearch.Location = new System.Drawing.Point(10, 13);
            lblSearch.Name = "lblSearch";
            lblSearch.Size = new System.Drawing.Size(32, 15);
            lblSearch.TabIndex = 0;
            lblSearch.Text = "بحث:";

            txtSearch.Location = new System.Drawing.Point(48, 10);
            txtSearch.Name = "txtSearch";
            txtSearch.Size = new System.Drawing.Size(220, 23);
            txtSearch.TabIndex = 1;
            txtSearch.TextChanged += txtSearch_TextChanged;

            lblCategory.AutoSize = true;
            lblCategory.Location = new System.Drawing.Point(285, 13);
            lblCategory.Name = "lblCategory";
            lblCategory.Size = new System.Drawing.Size(43, 15);
            lblCategory.TabIndex = 2;
            lblCategory.Text = "التصنيف:";

            cboCategory.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            cboCategory.FormattingEnabled = true;
            cboCategory.Location = new System.Drawing.Point(335, 10);
            cboCategory.Name = "cboCategory";
            cboCategory.Size = new System.Drawing.Size(220, 23);
            cboCategory.TabIndex = 3;
            cboCategory.SelectedIndexChanged += cboCategory_SelectedIndexChanged;

            btnResetSearch.Location = new System.Drawing.Point(565, 9);
            btnResetSearch.Name = "btnResetSearch";
            btnResetSearch.Size = new System.Drawing.Size(85, 25);
            btnResetSearch.TabIndex = 4;
            btnResetSearch.Text = "إعادة ضبط";
            btnResetSearch.UseVisualStyleBackColor = true;
            btnResetSearch.Click += btnResetSearch_Click;

            // lstCommands
            lstCommands.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
                colCmdName, colCmdChannel, colCmdCategory, colCmdDescAr, colCmdDescEn, colCmdExample
            });
            lstCommands.Dock = System.Windows.Forms.DockStyle.Fill;
            lstCommands.FullRowSelect = true;
            lstCommands.GridLines = true;
            lstCommands.Location = new System.Drawing.Point(3, 45);
            lstCommands.Name = "lstCommands";
            lstCommands.Size = new System.Drawing.Size(926, 404);
            lstCommands.TabIndex = 1;
            lstCommands.UseCompatibleStateImageBehavior = false;
            lstCommands.View = System.Windows.Forms.View.Details;

            colCmdName.Text = "الأمر (Command)";
            colCmdName.Width = 140;

            colCmdChannel.Text = "القناة (Channel)";
            colCmdChannel.Width = 110;

            colCmdCategory.Text = "التصنيف (Category)";
            colCmdCategory.Width = 160;

            colCmdDescAr.Text = "الوصف بالعربية";
            colCmdDescAr.Width = 260;

            colCmdDescEn.Text = "English Description";
            colCmdDescEn.Width = 220;

            colCmdExample.Text = "مثال (Example)";
            colCmdExample.Width = 100;

            // Main
            AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            Controls.Add(tabControl);
            Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            Name = "Main";
            Size = new System.Drawing.Size(940, 480);
            Load += Main_Load;

            tabControl.ResumeLayout(false);
            tabLeaders.ResumeLayout(false);
            tabChoreography.ResumeLayout(false);
            tabTargetSupport.ResumeLayout(false);
            tabCommandsGuide.ResumeLayout(false);
            groupSettings.ResumeLayout(false);
            groupLeaders.ResumeLayout(false);
            groupLeaders.PerformLayout();
            groupLogs.ResumeLayout(false);
            groupFormations.ResumeLayout(false);
            groupEmotes.ResumeLayout(false);
            groupChoreoSettings.ResumeLayout(false);
            groupChoreoSettings.PerformLayout();
            groupTargetSettings.ResumeLayout(false);
            groupTargetLeaders.ResumeLayout(false);
            groupTargetLeaders.PerformLayout();
            groupTargetStatus.ResumeLayout(false);
            groupTargetStatus.PerformLayout();
            panelGuideTop.ResumeLayout(false);
            panelGuideTop.PerformLayout();
            ResumeLayout(false);
        }

        #endregion

        private SDUI.Controls.TabControl tabControl;
        private System.Windows.Forms.TabPage tabLeaders;
        private System.Windows.Forms.TabPage tabChoreography;
        private System.Windows.Forms.TabPage tabTargetSupport;
        private System.Windows.Forms.TabPage tabCommandsGuide;

        // Leaders Tab
        private SDUI.Controls.GroupBox groupSettings;
        private SDUI.Controls.CheckBox chkEnabled;
        private SDUI.Controls.Label lblCommandsGuide;

        private SDUI.Controls.GroupBox groupLeaders;
        private SDUI.Controls.Label lblLeader;
        private SDUI.Controls.ComboBox cboLeaderName;
        private SDUI.Controls.Button btnAddLeader;
        private SDUI.Controls.Button btnRemoveLeader;
        private SDUI.Controls.Button btnGetPartyLeader;
        private SDUI.Controls.ListView lstLeaders;
        private System.Windows.Forms.ColumnHeader colLeaderName;

        private SDUI.Controls.GroupBox groupLogs;
        private System.Windows.Forms.RichTextBox txtLogs;
        private SDUI.Controls.Button btnClearLogs;

        // Choreography Tab
        private SDUI.Controls.GroupBox groupFormations;
        private SDUI.Controls.Button btnOrbit;
        private SDUI.Controls.Button btnCounterOrbit;
        private SDUI.Controls.Button btnLine;
        private SDUI.Controls.Button btnWedge;
        private SDUI.Controls.Button btnCircle;
        private SDUI.Controls.Button btnSquare;
        private SDUI.Controls.Button btnColumn;
        private SDUI.Controls.Button btnPulse;

        private SDUI.Controls.GroupBox groupEmotes;
        private SDUI.Controls.Button btnDance;
        private SDUI.Controls.Button btnWave;
        private SDUI.Controls.Button btnCheer;
        private SDUI.Controls.Button btnBow;
        private SDUI.Controls.Button btnClap;
        private SDUI.Controls.Button btnHalt;
        private SDUI.Controls.Button btnStack;
        private SDUI.Controls.Button btnScatter;
        private SDUI.Controls.Button btnCome;
        private SDUI.Controls.Button btnMimic;

        private SDUI.Controls.GroupBox groupChoreoSettings;
        private SDUI.Controls.Label lblChoreoLeader;
        private SDUI.Controls.ComboBox cboChoreoLeader;
        private SDUI.Controls.Label lblChoreoRadius;
        private SDUI.Controls.ComboBox cboChoreoRadius;
        private SDUI.Controls.Label lblChoreoSpeed;
        private SDUI.Controls.ComboBox cboChoreoSpeed;
        private SDUI.Controls.Label lblChoreoSlot;
        private SDUI.Controls.ComboBox cboChoreoSlot;
        private SDUI.Controls.CheckBox chkBroadcastParty;
        private SDUI.Controls.Label lblChoreoStatusTitle;
        private System.Windows.Forms.RichTextBox txtChoreoStatus;

        // Target Support Tab
        private SDUI.Controls.GroupBox groupTargetSettings;
        private SDUI.Controls.CheckBox chkTargetEnabled;
        private SDUI.Controls.CheckBox chkDefensive;
        private SDUI.Controls.CheckBox chkAutoAttack;
        private SDUI.Controls.Label lblTargetDescription;

        private SDUI.Controls.GroupBox groupTargetLeaders;
        private SDUI.Controls.Label lblTargetLeaderPrompt;
        private SDUI.Controls.ComboBox cboTargetLeaderName;
        private SDUI.Controls.Button btnTargetAddLeader;
        private SDUI.Controls.Button btnTargetRemoveLeader;
        private SDUI.Controls.Button btnTargetGetPartyLeader;
        private SDUI.Controls.ListView lstTargetLeaders;
        private System.Windows.Forms.ColumnHeader colTargetLeaderName;

        private SDUI.Controls.GroupBox groupTargetStatus;
        private SDUI.Controls.Label lblTargetStatusTitle;
        private SDUI.Controls.Label lblTargetStatus;
        private SDUI.Controls.Label lblCurrentTargetTitle;
        private SDUI.Controls.Label lblCurrentTarget;

        // Guide Tab
        private SDUI.Controls.Panel panelGuideTop;
        private SDUI.Controls.Label lblSearch;
        private SDUI.Controls.TextBox txtSearch;
        private SDUI.Controls.Label lblCategory;
        private SDUI.Controls.ComboBox cboCategory;
        private SDUI.Controls.Button btnResetSearch;
        private SDUI.Controls.ListView lstCommands;
        private System.Windows.Forms.ColumnHeader colCmdName;
        private System.Windows.Forms.ColumnHeader colCmdChannel;
        private System.Windows.Forms.ColumnHeader colCmdCategory;
        private System.Windows.Forms.ColumnHeader colCmdDescAr;
        private System.Windows.Forms.ColumnHeader colCmdDescEn;
        private System.Windows.Forms.ColumnHeader colCmdExample;
    }
}
