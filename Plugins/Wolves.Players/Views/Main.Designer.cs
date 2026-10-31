namespace Wolves.Players.Views
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
            components = new System.ComponentModel.Container();

            tabControl = new SDUI.Controls.TabControl();
            tabInspect = new System.Windows.Forms.TabPage();
            tabPlayers = new System.Windows.Forms.TabPage();

            // Inspect tab
            groupNearbyInspect = new SDUI.Controls.GroupBox();
            listViewInspect = new SDUI.Controls.ListView();
            colInspectName = new System.Windows.Forms.ColumnHeader();
            colInspectGuild = new System.Windows.Forms.ColumnHeader();

            groupPlayerInfo = new SDUI.Controls.GroupBox();
            lblInspectNameTitle = new SDUI.Controls.Label();
            lblInspectNameValue = new SDUI.Controls.Label();
            lblInspectGuildTitle = new SDUI.Controls.Label();
            lblInspectGuildValue = new SDUI.Controls.Label();
            lblInspectMemberTitle = new SDUI.Controls.Label();
            lblInspectMemberValue = new SDUI.Controls.Label();
            lblInspectGuildIdTitle = new SDUI.Controls.Label();
            lblInspectGuildIdValue = new SDUI.Controls.Label();
            lblInspectUnionIdTitle = new SDUI.Controls.Label();
            lblInspectUnionIdValue = new SDUI.Controls.Label();
            lblInspectStatusTitle = new SDUI.Controls.Label();
            lblInspectStatusValue = new SDUI.Controls.Label();
            lblInspectHint = new SDUI.Controls.Label();

            groupEquipment = new SDUI.Controls.GroupBox();
            listViewEquipment = new SDUI.Controls.ListView();
            colEquipName = new System.Windows.Forms.ColumnHeader();
            colEquipPlus = new System.Windows.Forms.ColumnHeader();
            colEquipType = new System.Windows.Forms.ColumnHeader();
            colEquipRarity = new System.Windows.Forms.ColumnHeader();

            groupAvatars = new SDUI.Controls.GroupBox();
            listViewAvatars = new SDUI.Controls.ListView();
            colAvatarName = new System.Windows.Forms.ColumnHeader();
            colAvatarPlus = new System.Windows.Forms.ColumnHeader();
            colAvatarType = new System.Windows.Forms.ColumnHeader();
            colAvatarRarity = new System.Windows.Forms.ColumnHeader();

            // Players tab
            groupTrace = new SDUI.Controls.GroupBox();
            lblTraceType = new SDUI.Controls.Label();
            cboTraceTarget = new SDUI.Controls.ComboBox();
            lblTraceDistance = new SDUI.Controls.Label();
            numTraceDistance = new SDUI.Controls.NumUpDown();
            btnTraceStart = new SDUI.Controls.Button();
            btnTraceStop = new SDUI.Controls.Button();
            btnTraceReset = new SDUI.Controls.Button();
            chkInGameTrace = new SDUI.Controls.CheckBox();
            chkBuff = new SDUI.Controls.CheckBox();
            chkPartyBuff = new SDUI.Controls.CheckBox();
            chkAttackMonsters = new SDUI.Controls.CheckBox();
            lblAttackRange = new SDUI.Controls.Label();
            numAttackRange = new SDUI.Controls.NumUpDown();
            lblTraceStatus = new SDUI.Controls.Label();
            lblTraceHint = new SDUI.Controls.Label();

            groupPlayers = new SDUI.Controls.GroupBox();
            listViewPlayers = new SDUI.Controls.ListView();
            colPlayerName = new System.Windows.Forms.ColumnHeader();
            colPlayerLevel = new System.Windows.Forms.ColumnHeader();
            colPlayerDistance = new System.Windows.Forms.ColumnHeader();
            colPlayerJob = new System.Windows.Forms.ColumnHeader();
            colPlayerSuit = new System.Windows.Forms.ColumnHeader();
            colPlayerStatus = new System.Windows.Forms.ColumnHeader();
            btnRefreshPlayers = new SDUI.Controls.Button();
            lblRefreshTime = new SDUI.Controls.Label();

            groupStats = new SDUI.Controls.GroupBox();
            lblCurrentTraceTitle = new SDUI.Controls.Label();
            lblCurrentTraceValue = new SDUI.Controls.Label();
            lblPlayersTitle = new SDUI.Controls.Label();
            lblPlayersValue = new SDUI.Controls.Label();
            lblThievesTitle = new SDUI.Controls.Label();
            lblThievesValue = new SDUI.Controls.Label();
            lblHtTitle = new SDUI.Controls.Label();
            lblHtValue = new SDUI.Controls.Label();
            lblStatsHint = new SDUI.Controls.Label();

            contextMenuPlayers = new SDUI.Controls.ContextMenuStrip();
            menuTracePlayer = new System.Windows.Forms.ToolStripMenuItem();
            menuInspectPlayer = new System.Windows.Forms.ToolStripMenuItem();

            timerRefresh = new System.Windows.Forms.Timer(components);

            tabControl.SuspendLayout();
            tabInspect.SuspendLayout();
            tabPlayers.SuspendLayout();
            groupNearbyInspect.SuspendLayout();
            groupPlayerInfo.SuspendLayout();
            groupEquipment.SuspendLayout();
            groupAvatars.SuspendLayout();
            groupTrace.SuspendLayout();
            groupPlayers.SuspendLayout();
            groupStats.SuspendLayout();
            contextMenuPlayers.SuspendLayout();
            SuspendLayout();

            // 
            // tabControl
            // 
            tabControl.Controls.Add(tabInspect);
            tabControl.Controls.Add(tabPlayers);
            tabControl.Dock = System.Windows.Forms.DockStyle.Fill;
            tabControl.ItemSize = new System.Drawing.Size(110, 26);
            tabControl.Location = new System.Drawing.Point(0, 0);
            tabControl.Name = "tabControl";
            tabControl.SelectedIndex = 0;
            tabControl.Size = new System.Drawing.Size(940, 600);
            tabControl.TabIndex = 0;

            // 
            // tabInspect
            // 
            tabInspect.Controls.Add(groupNearbyInspect);
            tabInspect.Controls.Add(groupPlayerInfo);
            tabInspect.Controls.Add(groupEquipment);
            tabInspect.Controls.Add(groupAvatars);
            tabInspect.Location = new System.Drawing.Point(4, 30);
            tabInspect.Name = "tabInspect";
            tabInspect.Padding = new System.Windows.Forms.Padding(3);
            tabInspect.Size = new System.Drawing.Size(932, 566);
            tabInspect.TabIndex = 0;
            tabInspect.Text = "فحص";
            tabInspect.UseVisualStyleBackColor = true;

            // 
            // groupNearbyInspect
            // 
            groupNearbyInspect.Controls.Add(listViewInspect);
            groupNearbyInspect.Font = new System.Drawing.Font("Segoe UI Semibold", 9.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            groupNearbyInspect.Location = new System.Drawing.Point(6, 6);
            groupNearbyInspect.Name = "groupNearbyInspect";
            groupNearbyInspect.Size = new System.Drawing.Size(300, 544);
            groupNearbyInspect.TabIndex = 0;
            groupNearbyInspect.TabStop = false;
            groupNearbyInspect.Text = "اللاعبون القريبون";

            // 
            // listViewInspect
            // 
            listViewInspect.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] { colInspectName, colInspectGuild });
            listViewInspect.FullRowSelect = true;
            listViewInspect.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
            listViewInspect.Location = new System.Drawing.Point(10, 24);
            listViewInspect.MultiSelect = false;
            listViewInspect.Name = "listViewInspect";
            listViewInspect.Size = new System.Drawing.Size(280, 510);
            listViewInspect.TabIndex = 0;
            listViewInspect.UseCompatibleStateImageBehavior = false;
            listViewInspect.View = System.Windows.Forms.View.Details;
            listViewInspect.SelectedIndexChanged += listViewInspect_SelectedIndexChanged;
            listViewInspect.MouseDown += listViewPlayers_MouseDown;

            // 
            // colInspectName
            // 
            colInspectName.Text = "الاسم";
            colInspectName.Width = 160;

            // 
            // colInspectGuild
            // 
            colInspectGuild.Text = "النقابة";
            colInspectGuild.Width = 110;

            // 
            // groupPlayerInfo
            // 
            groupPlayerInfo.Controls.Add(lblInspectNameTitle);
            groupPlayerInfo.Controls.Add(lblInspectNameValue);
            groupPlayerInfo.Controls.Add(lblInspectGuildTitle);
            groupPlayerInfo.Controls.Add(lblInspectGuildValue);
            groupPlayerInfo.Controls.Add(lblInspectMemberTitle);
            groupPlayerInfo.Controls.Add(lblInspectMemberValue);
            groupPlayerInfo.Controls.Add(lblInspectGuildIdTitle);
            groupPlayerInfo.Controls.Add(lblInspectGuildIdValue);
            groupPlayerInfo.Controls.Add(lblInspectUnionIdTitle);
            groupPlayerInfo.Controls.Add(lblInspectUnionIdValue);
            groupPlayerInfo.Controls.Add(lblInspectStatusTitle);
            groupPlayerInfo.Controls.Add(lblInspectStatusValue);
            groupPlayerInfo.Controls.Add(lblInspectHint);
            groupPlayerInfo.Font = new System.Drawing.Font("Segoe UI Semibold", 9.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            groupPlayerInfo.Location = new System.Drawing.Point(312, 6);
            groupPlayerInfo.Name = "groupPlayerInfo";
            groupPlayerInfo.Size = new System.Drawing.Size(616, 124);
            groupPlayerInfo.TabIndex = 1;
            groupPlayerInfo.TabStop = false;
            groupPlayerInfo.Text = "معلومات اللاعب";

            // 
            // lblInspectNameTitle
            // 
            lblInspectNameTitle.AutoSize = true;
            lblInspectNameTitle.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            lblInspectNameTitle.Location = new System.Drawing.Point(12, 26);
            lblInspectNameTitle.Name = "lblInspectNameTitle";
            lblInspectNameTitle.Size = new System.Drawing.Size(42, 20);
            lblInspectNameTitle.TabIndex = 0;
            lblInspectNameTitle.Text = "اللاعب:";

            // 
            // lblInspectNameValue
            // 
            lblInspectNameValue.AutoSize = false;
            lblInspectNameValue.Font = new System.Drawing.Font("Segoe UI Semibold", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            lblInspectNameValue.Location = new System.Drawing.Point(70, 26);
            lblInspectNameValue.Name = "lblInspectNameValue";
            lblInspectNameValue.Size = new System.Drawing.Size(155, 20);
            lblInspectNameValue.TabIndex = 1;
            lblInspectNameValue.Text = "-";

            // 
            // lblInspectGuildTitle
            // 
            lblInspectGuildTitle.AutoSize = true;
            lblInspectGuildTitle.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            lblInspectGuildTitle.Location = new System.Drawing.Point(235, 26);
            lblInspectGuildTitle.Name = "lblInspectGuildTitle";
            lblInspectGuildTitle.Size = new System.Drawing.Size(48, 20);
            lblInspectGuildTitle.TabIndex = 2;
            lblInspectGuildTitle.Text = "النقابة:";

            // 
            // lblInspectGuildValue
            // 
            lblInspectGuildValue.AutoSize = false;
            lblInspectGuildValue.Font = new System.Drawing.Font("Segoe UI Semibold", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            lblInspectGuildValue.Location = new System.Drawing.Point(292, 26);
            lblInspectGuildValue.Name = "lblInspectGuildValue";
            lblInspectGuildValue.Size = new System.Drawing.Size(150, 20);
            lblInspectGuildValue.TabIndex = 3;
            lblInspectGuildValue.Text = "-";

            // 
            // lblInspectMemberTitle
            // 
            lblInspectMemberTitle.AutoSize = true;
            lblInspectMemberTitle.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            lblInspectMemberTitle.Location = new System.Drawing.Point(455, 26);
            lblInspectMemberTitle.Name = "lblInspectMemberTitle";
            lblInspectMemberTitle.Size = new System.Drawing.Size(70, 20);
            lblInspectMemberTitle.TabIndex = 4;
            lblInspectMemberTitle.Text = "اللقب:";

            // 
            // lblInspectMemberValue
            // 
            lblInspectMemberValue.AutoSize = false;
            lblInspectMemberValue.Font = new System.Drawing.Font("Segoe UI Semibold", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            lblInspectMemberValue.Location = new System.Drawing.Point(500, 26);
            lblInspectMemberValue.Name = "lblInspectMemberValue";
            lblInspectMemberValue.Size = new System.Drawing.Size(70, 20);
            lblInspectMemberValue.TabIndex = 5;
            lblInspectMemberValue.Text = "-";

            // 
            // lblInspectGuildIdTitle
            // 
            lblInspectGuildIdTitle.AutoSize = true;
            lblInspectGuildIdTitle.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            lblInspectGuildIdTitle.Location = new System.Drawing.Point(12, 56);
            lblInspectGuildIdTitle.Name = "lblInspectGuildIdTitle";
            lblInspectGuildIdTitle.Size = new System.Drawing.Size(70, 20);
            lblInspectGuildIdTitle.TabIndex = 6;
            lblInspectGuildIdTitle.Text = "رقم النقابة:";

            // 
            // lblInspectGuildIdValue
            // 
            lblInspectGuildIdValue.AutoSize = false;
            lblInspectGuildIdValue.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            lblInspectGuildIdValue.Location = new System.Drawing.Point(92, 56);
            lblInspectGuildIdValue.Name = "lblInspectGuildIdValue";
            lblInspectGuildIdValue.Size = new System.Drawing.Size(90, 20);
            lblInspectGuildIdValue.TabIndex = 7;
            lblInspectGuildIdValue.Text = "-";

            // 
            // lblInspectUnionIdTitle
            // 
            lblInspectUnionIdTitle.AutoSize = true;
            lblInspectUnionIdTitle.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            lblInspectUnionIdTitle.Location = new System.Drawing.Point(190, 56);
            lblInspectUnionIdTitle.Name = "lblInspectUnionIdTitle";
            lblInspectUnionIdTitle.Size = new System.Drawing.Size(74, 20);
            lblInspectUnionIdTitle.TabIndex = 8;
            lblInspectUnionIdTitle.Text = "رقم الاتحاد:";

            // 
            // lblInspectUnionIdValue
            // 
            lblInspectUnionIdValue.AutoSize = false;
            lblInspectUnionIdValue.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            lblInspectUnionIdValue.Location = new System.Drawing.Point(270, 56);
            lblInspectUnionIdValue.Name = "lblInspectUnionIdValue";
            lblInspectUnionIdValue.Size = new System.Drawing.Size(90, 20);
            lblInspectUnionIdValue.TabIndex = 9;
            lblInspectUnionIdValue.Text = "-";

            // 
            // lblInspectStatusTitle
            // 
            lblInspectStatusTitle.AutoSize = true;
            lblInspectStatusTitle.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            lblInspectStatusTitle.Location = new System.Drawing.Point(370, 56);
            lblInspectStatusTitle.Name = "lblInspectStatusTitle";
            lblInspectStatusTitle.Size = new System.Drawing.Size(49, 20);
            lblInspectStatusTitle.TabIndex = 10;
            lblInspectStatusTitle.Text = "الحالة:";

            // 
            // lblInspectStatusValue
            // 
            lblInspectStatusValue.AutoSize = false;
            lblInspectStatusValue.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            lblInspectStatusValue.Location = new System.Drawing.Point(425, 56);
            lblInspectStatusValue.Name = "lblInspectStatusValue";
            lblInspectStatusValue.Size = new System.Drawing.Size(170, 20);
            lblInspectStatusValue.TabIndex = 11;
            lblInspectStatusValue.Text = "-";

            // 
            // lblInspectHint
            // 
            lblInspectHint.AutoSize = false;
            lblInspectHint.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Italic, System.Drawing.GraphicsUnit.Point);
            lblInspectHint.Location = new System.Drawing.Point(12, 88);
            lblInspectHint.Name = "lblInspectHint";
            lblInspectHint.Size = new System.Drawing.Size(590, 24);
            lblInspectHint.TabIndex = 12;
            lblInspectHint.Text = "اختر لاعباً من القائمة لعرض بيانات النقابة والمعدات، أو استخدم الزر الأيمن للتتبع أو الفحص.";

            // 
            // groupEquipment
            // 
            groupEquipment.Controls.Add(listViewEquipment);
            groupEquipment.Font = new System.Drawing.Font("Segoe UI Semibold", 9.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            groupEquipment.Location = new System.Drawing.Point(312, 136);
            groupEquipment.Name = "groupEquipment";
            groupEquipment.Size = new System.Drawing.Size(616, 190);
            groupEquipment.TabIndex = 2;
            groupEquipment.TabStop = false;
            groupEquipment.Text = "المعدات";

            // 
            // listViewEquipment
            // 
            listViewEquipment.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] { colEquipName, colEquipPlus, colEquipType, colEquipRarity });
            listViewEquipment.FullRowSelect = true;
            listViewEquipment.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
            listViewEquipment.Location = new System.Drawing.Point(10, 24);
            listViewEquipment.MultiSelect = false;
            listViewEquipment.Name = "listViewEquipment";
            listViewEquipment.Size = new System.Drawing.Size(596, 156);
            listViewEquipment.TabIndex = 0;
            listViewEquipment.UseCompatibleStateImageBehavior = false;
            listViewEquipment.View = System.Windows.Forms.View.Details;

            // 
            // colEquipName
            // 
            colEquipName.Text = "الاسم";
            colEquipName.Width = 280;

            // 
            // colEquipPlus
            // 
            colEquipPlus.Text = "+";
            colEquipPlus.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            colEquipPlus.Width = 50;

            // 
            // colEquipType
            // 
            colEquipType.Text = "النوع";
            colEquipType.Width = 120;

            // 
            // colEquipRarity
            // 
            colEquipRarity.Text = "الندرة";
            colEquipRarity.Width = 115;

            // 
            // groupAvatars
            // 
            groupAvatars.Controls.Add(listViewAvatars);
            groupAvatars.Font = new System.Drawing.Font("Segoe UI Semibold", 9.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            groupAvatars.Location = new System.Drawing.Point(312, 332);
            groupAvatars.Name = "groupAvatars";
            groupAvatars.Size = new System.Drawing.Size(616, 218);
            groupAvatars.TabIndex = 3;
            groupAvatars.TabStop = false;
            groupAvatars.Text = "الأفاتارات";

            // 
            // listViewAvatars
            // 
            listViewAvatars.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] { colAvatarName, colAvatarPlus, colAvatarType, colAvatarRarity });
            listViewAvatars.FullRowSelect = true;
            listViewAvatars.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
            listViewAvatars.Location = new System.Drawing.Point(10, 24);
            listViewAvatars.MultiSelect = false;
            listViewAvatars.Name = "listViewAvatars";
            listViewAvatars.Size = new System.Drawing.Size(596, 184);
            listViewAvatars.TabIndex = 0;
            listViewAvatars.UseCompatibleStateImageBehavior = false;
            listViewAvatars.View = System.Windows.Forms.View.Details;

            // 
            // colAvatarName
            // 
            colAvatarName.Text = "الاسم";
            colAvatarName.Width = 280;

            // 
            // colAvatarPlus
            // 
            colAvatarPlus.Text = "+";
            colAvatarPlus.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            colAvatarPlus.Width = 50;

            // 
            // colAvatarType
            // 
            colAvatarType.Text = "النوع";
            colAvatarType.Width = 120;

            // 
            // colAvatarRarity
            // 
            colAvatarRarity.Text = "الندرة";
            colAvatarRarity.Width = 115;

            // 
            // tabPlayers
            // 
            tabPlayers.Controls.Add(groupTrace);
            tabPlayers.Controls.Add(groupPlayers);
            tabPlayers.Controls.Add(groupStats);
            tabPlayers.Location = new System.Drawing.Point(4, 30);
            tabPlayers.Name = "tabPlayers";
            tabPlayers.Padding = new System.Windows.Forms.Padding(3);
            tabPlayers.Size = new System.Drawing.Size(932, 566);
            tabPlayers.TabIndex = 1;
            tabPlayers.Text = "اللاعبين";
            tabPlayers.UseVisualStyleBackColor = true;

            // 
            // groupTrace
            // 
            groupTrace.Controls.Add(lblTraceType);
            groupTrace.Controls.Add(cboTraceTarget);
            groupTrace.Controls.Add(lblTraceDistance);
            groupTrace.Controls.Add(numTraceDistance);
            groupTrace.Controls.Add(btnTraceStart);
            groupTrace.Controls.Add(btnTraceStop);
            groupTrace.Controls.Add(btnTraceReset);
            groupTrace.Controls.Add(chkInGameTrace);
            groupTrace.Controls.Add(chkBuff);
            groupTrace.Controls.Add(chkPartyBuff);
            groupTrace.Controls.Add(chkAttackMonsters);
            groupTrace.Controls.Add(lblAttackRange);
            groupTrace.Controls.Add(numAttackRange);
            groupTrace.Controls.Add(lblTraceStatus);
            groupTrace.Controls.Add(lblTraceHint);
            groupTrace.Font = new System.Drawing.Font("Segoe UI Semibold", 9.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            groupTrace.Location = new System.Drawing.Point(6, 6);
            groupTrace.Name = "groupTrace";
            groupTrace.Size = new System.Drawing.Size(920, 180);
            groupTrace.TabIndex = 0;
            groupTrace.TabStop = false;
            groupTrace.Text = "التتبع";

            // 
            // lblTraceType
            // 
            lblTraceType.AutoSize = true;
            lblTraceType.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            lblTraceType.Location = new System.Drawing.Point(14, 29);
            lblTraceType.Name = "lblTraceType";
            lblTraceType.Size = new System.Drawing.Size(72, 20);
            lblTraceType.TabIndex = 0;
            lblTraceType.Text = "نوع التتبع:";

            // 
            // cboTraceTarget
            // 
            cboTraceTarget.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawFixed;
            cboTraceTarget.DropDownHeight = 200;
            cboTraceTarget.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            cboTraceTarget.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            cboTraceTarget.IntegralHeight = false;
            cboTraceTarget.ItemHeight = 17;
            cboTraceTarget.Location = new System.Drawing.Point(100, 26);
            cboTraceTarget.Name = "cboTraceTarget";
            cboTraceTarget.Radius = 5;
            cboTraceTarget.ShadowDepth = 4F;
            cboTraceTarget.Size = new System.Drawing.Size(230, 23);
            cboTraceTarget.TabIndex = 1;

            // 
            // lblTraceDistance
            // 
            lblTraceDistance.AutoSize = true;
            lblTraceDistance.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            lblTraceDistance.Location = new System.Drawing.Point(350, 29);
            lblTraceDistance.Name = "lblTraceDistance";
            lblTraceDistance.Size = new System.Drawing.Size(55, 20);
            lblTraceDistance.TabIndex = 2;
            lblTraceDistance.Text = "المسافة:";

            // 
            // numTraceDistance
            // 
            numTraceDistance.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            numTraceDistance.Location = new System.Drawing.Point(420, 26);
            numTraceDistance.Maximum = new decimal(new int[] { 50, 0, 0, 0 });
            numTraceDistance.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
            numTraceDistance.Name = "numTraceDistance";
            numTraceDistance.Size = new System.Drawing.Size(65, 24);
            numTraceDistance.TabIndex = 3;
            numTraceDistance.Value = new decimal(new int[] { 5, 0, 0, 0 });
            numTraceDistance.ValueChanged += TraceSettingChanged;

            // 
            // btnTraceStart
            // 
            btnTraceStart.Color = System.Drawing.Color.Transparent;
            btnTraceStart.Font = new System.Drawing.Font("Segoe UI Semibold", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            btnTraceStart.Location = new System.Drawing.Point(505, 22);
            btnTraceStart.Name = "btnTraceStart";
            btnTraceStart.Radius = 6;
            btnTraceStart.ShadowDepth = 4F;
            btnTraceStart.Size = new System.Drawing.Size(120, 30);
            btnTraceStart.TabIndex = 4;
            btnTraceStart.Text = "بدء التتبع";
            btnTraceStart.Click += btnTraceStart_Click;

            // 
            // btnTraceStop
            // 
            btnTraceStop.Color = System.Drawing.Color.Transparent;
            btnTraceStop.Enabled = false;
            btnTraceStop.Font = new System.Drawing.Font("Segoe UI Semibold", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            btnTraceStop.Location = new System.Drawing.Point(635, 22);
            btnTraceStop.Name = "btnTraceStop";
            btnTraceStop.Radius = 6;
            btnTraceStop.ShadowDepth = 4F;
            btnTraceStop.Size = new System.Drawing.Size(120, 30);
            btnTraceStop.TabIndex = 5;
            btnTraceStop.Text = "إيقاف التتبع";
            btnTraceStop.Click += btnTraceStop_Click;

            // 
            // btnTraceReset
            // 
            btnTraceReset.Color = System.Drawing.Color.Transparent;
            btnTraceReset.Font = new System.Drawing.Font("Segoe UI Semibold", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            btnTraceReset.Location = new System.Drawing.Point(765, 22);
            btnTraceReset.Name = "btnTraceReset";
            btnTraceReset.Radius = 6;
            btnTraceReset.ShadowDepth = 4F;
            btnTraceReset.Size = new System.Drawing.Size(120, 30);
            btnTraceReset.TabIndex = 6;
            btnTraceReset.Text = "إعادة تعيين";
            btnTraceReset.Click += btnTraceReset_Click;

            // 
            // chkInGameTrace
            // 
            chkInGameTrace.AutoSize = true;
            chkInGameTrace.BackColor = System.Drawing.Color.Transparent;
            chkInGameTrace.Checked = true;
            chkInGameTrace.CheckState = System.Windows.Forms.CheckState.Checked;
            chkInGameTrace.Depth = 0;
            chkInGameTrace.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            chkInGameTrace.Location = new System.Drawing.Point(14, 66);
            chkInGameTrace.Margin = new System.Windows.Forms.Padding(0);
            chkInGameTrace.MouseLocation = new System.Drawing.Point(-1, -1);
            chkInGameTrace.Name = "chkInGameTrace";
            chkInGameTrace.Ripple = true;
            chkInGameTrace.Size = new System.Drawing.Size(150, 24);
            chkInGameTrace.TabIndex = 7;
            chkInGameTrace.Text = "تتبع داخل اللعبة";
            chkInGameTrace.UseVisualStyleBackColor = false;
            chkInGameTrace.CheckedChanged += TraceSettingChanged;

            // 
            // chkBuff
            // 
            chkBuff.AutoSize = true;
            chkBuff.BackColor = System.Drawing.Color.Transparent;
            chkBuff.Depth = 0;
            chkBuff.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            chkBuff.Location = new System.Drawing.Point(190, 66);
            chkBuff.Margin = new System.Windows.Forms.Padding(0);
            chkBuff.MouseLocation = new System.Drawing.Point(-1, -1);
            chkBuff.Name = "chkBuff";
            chkBuff.Ripple = true;
            chkBuff.Size = new System.Drawing.Size(115, 24);
            chkBuff.TabIndex = 8;
            chkBuff.Text = "تفعيل البفات";
            chkBuff.UseVisualStyleBackColor = false;
            chkBuff.CheckedChanged += TraceSettingChanged;

            // 
            // chkPartyBuff
            // 
            chkPartyBuff.AutoSize = true;
            chkPartyBuff.BackColor = System.Drawing.Color.Transparent;
            chkPartyBuff.Depth = 0;
            chkPartyBuff.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            chkPartyBuff.Location = new System.Drawing.Point(315, 66);
            chkPartyBuff.Margin = new System.Windows.Forms.Padding(0);
            chkPartyBuff.MouseLocation = new System.Drawing.Point(-1, -1);
            chkPartyBuff.Name = "chkPartyBuff";
            chkPartyBuff.Ripple = true;
            chkPartyBuff.Size = new System.Drawing.Size(125, 24);
            chkPartyBuff.TabIndex = 9;
            chkPartyBuff.Text = "بفات البارتي";
            chkPartyBuff.UseVisualStyleBackColor = false;
            chkPartyBuff.CheckedChanged += TraceSettingChanged;

            // 
            // chkAttackMonsters
            // 
            chkAttackMonsters.AutoSize = true;
            chkAttackMonsters.BackColor = System.Drawing.Color.Transparent;
            chkAttackMonsters.Depth = 0;
            chkAttackMonsters.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            chkAttackMonsters.Location = new System.Drawing.Point(450, 66);
            chkAttackMonsters.Margin = new System.Windows.Forms.Padding(0);
            chkAttackMonsters.MouseLocation = new System.Drawing.Point(-1, -1);
            chkAttackMonsters.Name = "chkAttackMonsters";
            chkAttackMonsters.Ripple = true;
            chkAttackMonsters.Size = new System.Drawing.Size(150, 24);
            chkAttackMonsters.TabIndex = 10;
            chkAttackMonsters.Text = "مهاجمة الوحوش";
            chkAttackMonsters.UseVisualStyleBackColor = false;
            chkAttackMonsters.CheckedChanged += TraceSettingChanged;

            // 
            // lblAttackRange
            // 
            lblAttackRange.AutoSize = true;
            lblAttackRange.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            lblAttackRange.Location = new System.Drawing.Point(615, 69);
            lblAttackRange.Name = "lblAttackRange";
            lblAttackRange.Size = new System.Drawing.Size(74, 20);
            lblAttackRange.TabIndex = 11;
            lblAttackRange.Text = "نطاق الهجوم:";

            // 
            // numAttackRange
            // 
            numAttackRange.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            numAttackRange.Location = new System.Drawing.Point(700, 66);
            numAttackRange.Maximum = new decimal(new int[] { 100, 0, 0, 0 });
            numAttackRange.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
            numAttackRange.Name = "numAttackRange";
            numAttackRange.Size = new System.Drawing.Size(70, 24);
            numAttackRange.TabIndex = 12;
            numAttackRange.Value = new decimal(new int[] { 30, 0, 0, 0 });
            numAttackRange.ValueChanged += TraceSettingChanged;

            // 
            // lblTraceStatus
            // 
            lblTraceStatus.AutoSize = false;
            lblTraceStatus.Font = new System.Drawing.Font("Segoe UI Semibold", 9.5F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            lblTraceStatus.ForeColor = System.Drawing.Color.Gray;
            lblTraceStatus.Location = new System.Drawing.Point(14, 102);
            lblTraceStatus.Name = "lblTraceStatus";
            lblTraceStatus.Size = new System.Drawing.Size(700, 22);
            lblTraceStatus.TabIndex = 13;
            lblTraceStatus.Text = "الحالة: متوقف";

            // 
            // lblTraceHint
            // 
            lblTraceHint.AutoSize = false;
            lblTraceHint.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Italic, System.Drawing.GraphicsUnit.Point);
            lblTraceHint.Location = new System.Drawing.Point(14, 132);
            lblTraceHint.Name = "lblTraceHint";
            lblTraceHint.Size = new System.Drawing.Size(900, 36);
            lblTraceHint.TabIndex = 14;
            lblTraceHint.Text = "يتم إيقاف البوت تلقائياً عند بدء التتبع. استخدم الزر الأيمن على أي لاعب للتتبع أو الفحص.";

            // 
            // groupPlayers
            // 
            groupPlayers.Controls.Add(listViewPlayers);
            groupPlayers.Controls.Add(btnRefreshPlayers);
            groupPlayers.Controls.Add(lblRefreshTime);
            groupPlayers.Font = new System.Drawing.Font("Segoe UI Semibold", 9.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            groupPlayers.Location = new System.Drawing.Point(6, 196);
            groupPlayers.Name = "groupPlayers";
            groupPlayers.Size = new System.Drawing.Size(620, 348);
            groupPlayers.TabIndex = 1;
            groupPlayers.TabStop = false;
            groupPlayers.Text = "اللاعبون القريبون";

            // 
            // listViewPlayers
            // 
            listViewPlayers.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] { colPlayerName, colPlayerLevel, colPlayerDistance, colPlayerJob, colPlayerSuit, colPlayerStatus });
            listViewPlayers.FullRowSelect = true;
            listViewPlayers.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
            listViewPlayers.Location = new System.Drawing.Point(10, 24);
            listViewPlayers.MultiSelect = false;
            listViewPlayers.Name = "listViewPlayers";
            listViewPlayers.Size = new System.Drawing.Size(600, 282);
            listViewPlayers.TabIndex = 0;
            listViewPlayers.UseCompatibleStateImageBehavior = false;
            listViewPlayers.View = System.Windows.Forms.View.Details;
            listViewPlayers.MouseDown += listViewPlayers_MouseDown;

            // 
            // colPlayerName
            // 
            colPlayerName.Text = "الاسم";
            colPlayerName.Width = 145;

            // 
            // colPlayerLevel
            // 
            colPlayerLevel.Text = "المستوى";
            colPlayerLevel.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            colPlayerLevel.Width = 85;

            // 
            // colPlayerDistance
            // 
            colPlayerDistance.Text = "المسافة";
            colPlayerDistance.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            colPlayerDistance.Width = 75;

            // 
            // colPlayerJob
            // 
            colPlayerJob.Text = "الوظيفة";
            colPlayerJob.Width = 75;

            // 
            // colPlayerSuit
            // 
            colPlayerSuit.Text = "الزي";
            colPlayerSuit.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            colPlayerSuit.Width = 55;

            // 
            // colPlayerStatus
            // 
            colPlayerStatus.Text = "الحالة";
            colPlayerStatus.Width = 140;

            // 
            // btnRefreshPlayers
            // 
            btnRefreshPlayers.Color = System.Drawing.Color.Transparent;
            btnRefreshPlayers.Font = new System.Drawing.Font("Segoe UI Semibold", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            btnRefreshPlayers.Location = new System.Drawing.Point(10, 314);
            btnRefreshPlayers.Name = "btnRefreshPlayers";
            btnRefreshPlayers.Radius = 6;
            btnRefreshPlayers.ShadowDepth = 4F;
            btnRefreshPlayers.Size = new System.Drawing.Size(160, 28);
            btnRefreshPlayers.TabIndex = 1;
            btnRefreshPlayers.Text = "تحديث اللاعبين";
            btnRefreshPlayers.Click += btnRefreshPlayers_Click;

            // 
            // lblRefreshTime
            // 
            lblRefreshTime.AutoSize = false;
            lblRefreshTime.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            lblRefreshTime.ForeColor = System.Drawing.Color.Gray;
            lblRefreshTime.Location = new System.Drawing.Point(180, 318);
            lblRefreshTime.Name = "lblRefreshTime";
            lblRefreshTime.Size = new System.Drawing.Size(440, 20);
            lblRefreshTime.TabIndex = 2;
            lblRefreshTime.Text = "آخر تحديث: --";

            // 
            // groupStats
            // 
            groupStats.Controls.Add(lblCurrentTraceTitle);
            groupStats.Controls.Add(lblCurrentTraceValue);
            groupStats.Controls.Add(lblPlayersTitle);
            groupStats.Controls.Add(lblPlayersValue);
            groupStats.Controls.Add(lblThievesTitle);
            groupStats.Controls.Add(lblThievesValue);
            groupStats.Controls.Add(lblHtTitle);
            groupStats.Controls.Add(lblHtValue);
            groupStats.Controls.Add(lblStatsHint);
            groupStats.Font = new System.Drawing.Font("Segoe UI Semibold", 9.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            groupStats.Location = new System.Drawing.Point(638, 196);
            groupStats.Name = "groupStats";
            groupStats.Size = new System.Drawing.Size(290, 348);
            groupStats.TabIndex = 2;
            groupStats.TabStop = false;
            groupStats.Text = "إحصائيات التتبع";

            // 
            // lblCurrentTraceTitle
            // 
            lblCurrentTraceTitle.AutoSize = true;
            lblCurrentTraceTitle.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            lblCurrentTraceTitle.Location = new System.Drawing.Point(12, 30);
            lblCurrentTraceTitle.Name = "lblCurrentTraceTitle";
            lblCurrentTraceTitle.Size = new System.Drawing.Size(87, 20);
            lblCurrentTraceTitle.TabIndex = 0;
            lblCurrentTraceTitle.Text = "التتبع الحالي:";

            // 
            // lblCurrentTraceValue
            // 
            lblCurrentTraceValue.AutoSize = false;
            lblCurrentTraceValue.Font = new System.Drawing.Font("Segoe UI Semibold", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            lblCurrentTraceValue.Location = new System.Drawing.Point(125, 30);
            lblCurrentTraceValue.Name = "lblCurrentTraceValue";
            lblCurrentTraceValue.Size = new System.Drawing.Size(155, 20);
            lblCurrentTraceValue.TabIndex = 1;
            lblCurrentTraceValue.Text = "-";

            // 
            // lblPlayersTitle
            // 
            lblPlayersTitle.AutoSize = true;
            lblPlayersTitle.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            lblPlayersTitle.Location = new System.Drawing.Point(12, 66);
            lblPlayersTitle.Name = "lblPlayersTitle";
            lblPlayersTitle.Size = new System.Drawing.Size(63, 20);
            lblPlayersTitle.TabIndex = 2;
            lblPlayersTitle.Text = "اللاعبون:";

            // 
            // lblPlayersValue
            // 
            lblPlayersValue.AutoSize = false;
            lblPlayersValue.Font = new System.Drawing.Font("Segoe UI Semibold", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            lblPlayersValue.Location = new System.Drawing.Point(125, 66);
            lblPlayersValue.Name = "lblPlayersValue";
            lblPlayersValue.Size = new System.Drawing.Size(155, 20);
            lblPlayersValue.TabIndex = 3;
            lblPlayersValue.Text = "0";

            // 
            // lblThievesTitle
            // 
            lblThievesTitle.AutoSize = true;
            lblThievesTitle.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            lblThievesTitle.Location = new System.Drawing.Point(12, 102);
            lblThievesTitle.Name = "lblThievesTitle";
            lblThievesTitle.Size = new System.Drawing.Size(49, 20);
            lblThievesTitle.TabIndex = 4;
            lblThievesTitle.Text = "اللصوص:";

            // 
            // lblThievesValue
            // 
            lblThievesValue.AutoSize = false;
            lblThievesValue.Font = new System.Drawing.Font("Segoe UI Semibold", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            lblThievesValue.Location = new System.Drawing.Point(125, 102);
            lblThievesValue.Name = "lblThievesValue";
            lblThievesValue.Size = new System.Drawing.Size(155, 20);
            lblThievesValue.TabIndex = 5;
            lblThievesValue.Text = "0";

            // 
            // lblHtTitle
            // 
            lblHtTitle.AutoSize = true;
            lblHtTitle.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            lblHtTitle.Location = new System.Drawing.Point(12, 138);
            lblHtTitle.Name = "lblHtTitle";
            lblHtTitle.Size = new System.Drawing.Size(31, 20);
            lblHtTitle.TabIndex = 6;
            lblHtTitle.Text = "H/T:";

            // 
            // lblHtValue
            // 
            lblHtValue.AutoSize = false;
            lblHtValue.Font = new System.Drawing.Font("Segoe UI Semibold", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            lblHtValue.Location = new System.Drawing.Point(125, 138);
            lblHtValue.Name = "lblHtValue";
            lblHtValue.Size = new System.Drawing.Size(155, 20);
            lblHtValue.TabIndex = 7;
            lblHtValue.Text = "0 / 0";

            // 
            // lblStatsHint
            // 
            lblStatsHint.AutoSize = false;
            lblStatsHint.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Italic, System.Drawing.GraphicsUnit.Point);
            lblStatsHint.Location = new System.Drawing.Point(12, 190);
            lblStatsHint.Name = "lblStatsHint";
            lblStatsHint.Size = new System.Drawing.Size(266, 110);
            lblStatsHint.TabIndex = 8;
            lblStatsHint.Text = "H/T تعني عدد الصيادين / عدد اللصوص بين اللاعبين القريبين.\r\n\r\nانقر بزر الفأرة الأيمن على أي لاعب للتتبع أو الفحص.";

            // 
            // contextMenuPlayers
            // 
            contextMenuPlayers.Items.AddRange(new System.Windows.Forms.ToolStripItem[] { menuTracePlayer, menuInspectPlayer });
            contextMenuPlayers.Name = "contextMenuPlayers";
            contextMenuPlayers.Size = new System.Drawing.Size(190, 48);

            // 
            // menuTracePlayer
            // 
            menuTracePlayer.Name = "menuTracePlayer";
            menuTracePlayer.Size = new System.Drawing.Size(189, 22);
            menuTracePlayer.Text = "تتبع هذا اللاعب";
            menuTracePlayer.Click += menuTracePlayer_Click;

            // 
            // menuInspectPlayer
            // 
            menuInspectPlayer.Name = "menuInspectPlayer";
            menuInspectPlayer.Size = new System.Drawing.Size(189, 22);
            menuInspectPlayer.Text = "فحص هذا اللاعب";
            menuInspectPlayer.Click += menuInspectPlayer_Click;

            // 
            // timerRefresh
            // 
            timerRefresh.Enabled = true;
            timerRefresh.Interval = 1000;
            timerRefresh.Tick += timerRefresh_Tick;

            // 
            // Main
            // 
            Controls.Add(tabControl);
            Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            Name = "Main";
            Size = new System.Drawing.Size(940, 600);

            tabControl.ResumeLayout(false);
            tabInspect.ResumeLayout(false);
            tabPlayers.ResumeLayout(false);
            groupNearbyInspect.ResumeLayout(false);
            groupPlayerInfo.ResumeLayout(false);
            groupPlayerInfo.PerformLayout();
            groupEquipment.ResumeLayout(false);
            groupAvatars.ResumeLayout(false);
            groupTrace.ResumeLayout(false);
            groupTrace.PerformLayout();
            groupPlayers.ResumeLayout(false);
            groupStats.ResumeLayout(false);
            groupStats.PerformLayout();
            contextMenuPlayers.ResumeLayout(false);
            ResumeLayout(false);
        }

        #endregion

        private SDUI.Controls.TabControl tabControl;
        private System.Windows.Forms.TabPage tabInspect;
        private System.Windows.Forms.TabPage tabPlayers;

        private SDUI.Controls.GroupBox groupNearbyInspect;
        private SDUI.Controls.ListView listViewInspect;
        private System.Windows.Forms.ColumnHeader colInspectName;
        private System.Windows.Forms.ColumnHeader colInspectGuild;

        private SDUI.Controls.GroupBox groupPlayerInfo;
        private SDUI.Controls.Label lblInspectNameTitle;
        private SDUI.Controls.Label lblInspectNameValue;
        private SDUI.Controls.Label lblInspectGuildTitle;
        private SDUI.Controls.Label lblInspectGuildValue;
        private SDUI.Controls.Label lblInspectMemberTitle;
        private SDUI.Controls.Label lblInspectMemberValue;
        private SDUI.Controls.Label lblInspectGuildIdTitle;
        private SDUI.Controls.Label lblInspectGuildIdValue;
        private SDUI.Controls.Label lblInspectUnionIdTitle;
        private SDUI.Controls.Label lblInspectUnionIdValue;
        private SDUI.Controls.Label lblInspectStatusTitle;
        private SDUI.Controls.Label lblInspectStatusValue;
        private SDUI.Controls.Label lblInspectHint;

        private SDUI.Controls.GroupBox groupEquipment;
        private SDUI.Controls.ListView listViewEquipment;
        private System.Windows.Forms.ColumnHeader colEquipName;
        private System.Windows.Forms.ColumnHeader colEquipPlus;
        private System.Windows.Forms.ColumnHeader colEquipType;
        private System.Windows.Forms.ColumnHeader colEquipRarity;

        private SDUI.Controls.GroupBox groupAvatars;
        private SDUI.Controls.ListView listViewAvatars;
        private System.Windows.Forms.ColumnHeader colAvatarName;
        private System.Windows.Forms.ColumnHeader colAvatarPlus;
        private System.Windows.Forms.ColumnHeader colAvatarType;
        private System.Windows.Forms.ColumnHeader colAvatarRarity;

        private SDUI.Controls.GroupBox groupTrace;
        private SDUI.Controls.Label lblTraceType;
        private SDUI.Controls.ComboBox cboTraceTarget;
        private SDUI.Controls.Label lblTraceDistance;
        private SDUI.Controls.NumUpDown numTraceDistance;
        private SDUI.Controls.Button btnTraceStart;
        private SDUI.Controls.Button btnTraceStop;
        private SDUI.Controls.Button btnTraceReset;
        private SDUI.Controls.CheckBox chkInGameTrace;
        private SDUI.Controls.CheckBox chkBuff;
        private SDUI.Controls.CheckBox chkPartyBuff;
        private SDUI.Controls.CheckBox chkAttackMonsters;
        private SDUI.Controls.Label lblAttackRange;
        private SDUI.Controls.NumUpDown numAttackRange;
        private SDUI.Controls.Label lblTraceStatus;
        private SDUI.Controls.Label lblTraceHint;

        private SDUI.Controls.GroupBox groupPlayers;
        private SDUI.Controls.ListView listViewPlayers;
        private System.Windows.Forms.ColumnHeader colPlayerName;
        private System.Windows.Forms.ColumnHeader colPlayerLevel;
        private System.Windows.Forms.ColumnHeader colPlayerDistance;
        private System.Windows.Forms.ColumnHeader colPlayerJob;
        private System.Windows.Forms.ColumnHeader colPlayerSuit;
        private System.Windows.Forms.ColumnHeader colPlayerStatus;
        private SDUI.Controls.Button btnRefreshPlayers;
        private SDUI.Controls.Label lblRefreshTime;

        private SDUI.Controls.GroupBox groupStats;
        private SDUI.Controls.Label lblCurrentTraceTitle;
        private SDUI.Controls.Label lblCurrentTraceValue;
        private SDUI.Controls.Label lblPlayersTitle;
        private SDUI.Controls.Label lblPlayersValue;
        private SDUI.Controls.Label lblThievesTitle;
        private SDUI.Controls.Label lblThievesValue;
        private SDUI.Controls.Label lblHtTitle;
        private SDUI.Controls.Label lblHtValue;
        private SDUI.Controls.Label lblStatsHint;

        private SDUI.Controls.ContextMenuStrip contextMenuPlayers;
        private System.Windows.Forms.ToolStripMenuItem menuTracePlayer;
        private System.Windows.Forms.ToolStripMenuItem menuInspectPlayer;

        private System.Windows.Forms.Timer timerRefresh;
    }
}
