namespace RSBot.Lure.Views
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

        #region Vom Komponenten-Designer generierter Code

        private void InitializeComponent()
        {
            groupBoxParty = new SDUI.Controls.GroupBox();
            lblPartyXTitle = new SDUI.Controls.Label();
            lblPartyX = new SDUI.Controls.Label();
            lblPartyYTitle = new SDUI.Controls.Label();
            lblPartyY = new SDUI.Controls.Label();
            lblPartyRadiusTitle = new SDUI.Controls.Label();
            numPartyRadius = new SDUI.Controls.NumUpDown();
            btnSetPartySpot = new SDUI.Controls.Button();
            lblPartyWaitTitle = new SDUI.Controls.Label();
            numPartyWaitSeconds = new SDUI.Controls.NumUpDown();
            lblWalkscriptTitle = new SDUI.Controls.Label();
            txtWalkbackScript = new SDUI.Controls.TextBox();
            btnBrowseWalkscript = new SDUI.Controls.Button();

            groupBoxLureSpots = new SDUI.Controls.GroupBox();
            btnAddLureSpot = new SDUI.Controls.Button();
            lstLureSpots = new SDUI.Controls.ListView();
            colIndex = new System.Windows.Forms.ColumnHeader();
            colX = new System.Windows.Forms.ColumnHeader();
            colY = new System.Windows.Forms.ColumnHeader();
            colRadius = new System.Windows.Forms.ColumnHeader();
            lblSpotRadiusTitle = new SDUI.Controls.Label();
            numSpotRadius = new SDUI.Controls.NumUpDown();
            btnDeleteLureSpot = new SDUI.Controls.Button();
            btnClearLureSpots = new SDUI.Controls.Button();
            checkWalkRandomly = new SDUI.Controls.CheckBox();

            groupBox3 = new SDUI.Controls.GroupBox();
            checkUseHowlingShout = new SDUI.Controls.CheckBox();
            checkNoHowlingAtCenter = new SDUI.Controls.CheckBox();
            checkUseNormalAttack = new SDUI.Controls.CheckBox();
            checkUseAttackingSkills = new SDUI.Controls.CheckBox();
            separator1 = new SDUI.Controls.Separator();
            checkStopPartyMemberDead = new SDUI.Controls.CheckBox();
            numPartyMemberDead = new SDUI.Controls.NumUpDown();
            checkStopPartyMember = new SDUI.Controls.CheckBox();
            numPartyMember = new SDUI.Controls.NumUpDown();
            separator2 = new SDUI.Controls.Separator();
            checkStopMonsterType = new SDUI.Controls.CheckBox();
            comboMonsterType = new SDUI.Controls.ComboBox();
            label6 = new SDUI.Controls.Label();
            numMonsterType = new SDUI.Controls.NumUpDown();

            groupBoxParty.SuspendLayout();
            groupBoxLureSpots.SuspendLayout();
            groupBox3.SuspendLayout();
            SuspendLayout();

            // 
            // groupBoxParty
            // 
            groupBoxParty.BackColor = System.Drawing.Color.Transparent;
            groupBoxParty.Controls.Add(lblPartyXTitle);
            groupBoxParty.Controls.Add(lblPartyX);
            groupBoxParty.Controls.Add(lblPartyYTitle);
            groupBoxParty.Controls.Add(lblPartyY);
            groupBoxParty.Controls.Add(lblPartyRadiusTitle);
            groupBoxParty.Controls.Add(numPartyRadius);
            groupBoxParty.Controls.Add(btnSetPartySpot);
            groupBoxParty.Controls.Add(lblPartyWaitTitle);
            groupBoxParty.Controls.Add(numPartyWaitSeconds);
            groupBoxParty.Controls.Add(lblWalkscriptTitle);
            groupBoxParty.Controls.Add(txtWalkbackScript);
            groupBoxParty.Controls.Add(btnBrowseWalkscript);
            groupBoxParty.Location = new System.Drawing.Point(12, 10);
            groupBoxParty.Name = "groupBoxParty";
            groupBoxParty.Padding = new System.Windows.Forms.Padding(3, 8, 3, 3);
            groupBoxParty.Radius = 10;
            groupBoxParty.ShadowDepth = 4;
            groupBoxParty.Size = new System.Drawing.Size(400, 192);
            groupBoxParty.TabIndex = 0;
            groupBoxParty.TabStop = false;
            groupBoxParty.Text = "مكان البارتي (منطقة العودة)";

            // 
            // lblPartyXTitle
            // 
            lblPartyXTitle.AutoSize = true;
            lblPartyXTitle.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            lblPartyXTitle.Location = new System.Drawing.Point(16, 26);
            lblPartyXTitle.Name = "lblPartyXTitle";
            lblPartyXTitle.Size = new System.Drawing.Size(20, 15);
            lblPartyXTitle.TabIndex = 0;
            lblPartyXTitle.Text = "X:";

            // 
            // lblPartyX
            // 
            lblPartyX.AutoSize = true;
            lblPartyX.Location = new System.Drawing.Point(38, 26);
            lblPartyX.Name = "lblPartyX";
            lblPartyX.Size = new System.Drawing.Size(22, 15);
            lblPartyX.TabIndex = 1;
            lblPartyX.Text = "0.0";

            // 
            // lblPartyYTitle
            // 
            lblPartyYTitle.AutoSize = true;
            lblPartyYTitle.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            lblPartyYTitle.Location = new System.Drawing.Point(115, 26);
            lblPartyYTitle.Name = "lblPartyYTitle";
            lblPartyYTitle.Size = new System.Drawing.Size(20, 15);
            lblPartyYTitle.TabIndex = 2;
            lblPartyYTitle.Text = "Y:";

            // 
            // lblPartyY
            // 
            lblPartyY.AutoSize = true;
            lblPartyY.Location = new System.Drawing.Point(137, 26);
            lblPartyY.Name = "lblPartyY";
            lblPartyY.Size = new System.Drawing.Size(22, 15);
            lblPartyY.TabIndex = 3;
            lblPartyY.Text = "0.0";

            // 
            // lblPartyRadiusTitle
            // 
            lblPartyRadiusTitle.AutoSize = true;
            lblPartyRadiusTitle.Location = new System.Drawing.Point(215, 26);
            lblPartyRadiusTitle.Name = "lblPartyRadiusTitle";
            lblPartyRadiusTitle.Size = new System.Drawing.Size(46, 15);
            lblPartyRadiusTitle.TabIndex = 4;
            lblPartyRadiusTitle.Text = "Radius:";

            // 
            // numPartyRadius
            // 
            numPartyRadius.BackColor = System.Drawing.Color.Transparent;
            numPartyRadius.Location = new System.Drawing.Point(280, 22);
            numPartyRadius.Maximum = new decimal(new int[] { 300, 0, 0, 0 });
            numPartyRadius.Minimum = new decimal(new int[] { 5, 0, 0, 0 });
            numPartyRadius.Name = "numPartyRadius";
            numPartyRadius.Size = new System.Drawing.Size(100, 24);
            numPartyRadius.TabIndex = 5;
            numPartyRadius.Value = new decimal(new int[] { 50, 0, 0, 0 });
            numPartyRadius.ValueChanged += OnSettingsChanged;

            // 
            // btnSetPartySpot
            // 
            btnSetPartySpot.Color = System.Drawing.Color.FromArgb(46, 171, 107);
            btnSetPartySpot.ForeColor = System.Drawing.Color.White;
            btnSetPartySpot.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            btnSetPartySpot.Location = new System.Drawing.Point(16, 54);
            btnSetPartySpot.Name = "btnSetPartySpot";
            btnSetPartySpot.Radius = 6;
            btnSetPartySpot.ShadowDepth = 4F;
            btnSetPartySpot.Size = new System.Drawing.Size(364, 30);
            btnSetPartySpot.TabIndex = 6;
            btnSetPartySpot.Text = "تحديد مكان البارتي (الموقع الحالي)";
            btnSetPartySpot.UseVisualStyleBackColor = true;
            btnSetPartySpot.Click += btnSetPartySpot_Click;

            // 
            // lblPartyWaitTitle
            // 
            lblPartyWaitTitle.AutoSize = true;
            lblPartyWaitTitle.Location = new System.Drawing.Point(16, 94);
            lblPartyWaitTitle.Name = "lblPartyWaitTitle";
            lblPartyWaitTitle.Size = new System.Drawing.Size(155, 15);
            lblPartyWaitTitle.TabIndex = 7;
            lblPartyWaitTitle.Text = "الانتظار عند البارتي (ثواني):";

            // 
            // numPartyWaitSeconds
            // 
            numPartyWaitSeconds.BackColor = System.Drawing.Color.Transparent;
            numPartyWaitSeconds.Location = new System.Drawing.Point(280, 90);
            numPartyWaitSeconds.Maximum = new decimal(new int[] { 60, 0, 0, 0 });
            numPartyWaitSeconds.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
            numPartyWaitSeconds.Name = "numPartyWaitSeconds";
            numPartyWaitSeconds.Size = new System.Drawing.Size(100, 24);
            numPartyWaitSeconds.TabIndex = 8;
            numPartyWaitSeconds.Value = new decimal(new int[] { 3, 0, 0, 0 });
            numPartyWaitSeconds.ValueChanged += OnSettingsChanged;

            // 
            // lblWalkscriptTitle
            // 
            lblWalkscriptTitle.AutoSize = true;
            lblWalkscriptTitle.Location = new System.Drawing.Point(16, 126);
            lblWalkscriptTitle.Name = "lblWalkscriptTitle";
            lblWalkscriptTitle.Size = new System.Drawing.Size(170, 15);
            lblWalkscriptTitle.TabIndex = 9;
            lblWalkscriptTitle.Text = "مسار السير للبارتي (Walkscript):";

            // 
            // txtWalkbackScript
            // 
            txtWalkbackScript.Location = new System.Drawing.Point(16, 148);
            txtWalkbackScript.MaxLength = 32767;
            txtWalkbackScript.MultiLine = false;
            txtWalkbackScript.Name = "txtWalkbackScript";
            txtWalkbackScript.PassFocusShow = false;
            txtWalkbackScript.Radius = 4;
            txtWalkbackScript.Size = new System.Drawing.Size(320, 24);
            txtWalkbackScript.TabIndex = 10;
            txtWalkbackScript.TextAlignment = System.Windows.Forms.HorizontalAlignment.Left;
            txtWalkbackScript.UseSystemPasswordChar = false;

            // 
            // btnBrowseWalkscript
            // 
            btnBrowseWalkscript.Color = System.Drawing.Color.Transparent;
            btnBrowseWalkscript.Location = new System.Drawing.Point(342, 148);
            btnBrowseWalkscript.Name = "btnBrowseWalkscript";
            btnBrowseWalkscript.Radius = 4;
            btnBrowseWalkscript.ShadowDepth = 4F;
            btnBrowseWalkscript.Size = new System.Drawing.Size(38, 24);
            btnBrowseWalkscript.TabIndex = 11;
            btnBrowseWalkscript.Text = "...";
            btnBrowseWalkscript.UseVisualStyleBackColor = true;
            btnBrowseWalkscript.Click += btnBrowseWalkscript_Click;

            // 
            // groupBoxLureSpots
            // 
            groupBoxLureSpots.BackColor = System.Drawing.Color.Transparent;
            groupBoxLureSpots.Controls.Add(btnAddLureSpot);
            groupBoxLureSpots.Controls.Add(lstLureSpots);
            groupBoxLureSpots.Controls.Add(lblSpotRadiusTitle);
            groupBoxLureSpots.Controls.Add(numSpotRadius);
            groupBoxLureSpots.Controls.Add(btnDeleteLureSpot);
            groupBoxLureSpots.Controls.Add(btnClearLureSpots);
            groupBoxLureSpots.Controls.Add(checkWalkRandomly);
            groupBoxLureSpots.Location = new System.Drawing.Point(12, 208);
            groupBoxLureSpots.Name = "groupBoxLureSpots";
            groupBoxLureSpots.Padding = new System.Windows.Forms.Padding(3, 8, 3, 3);
            groupBoxLureSpots.Radius = 10;
            groupBoxLureSpots.ShadowDepth = 4;
            groupBoxLureSpots.Size = new System.Drawing.Size(400, 278);
            groupBoxLureSpots.TabIndex = 1;
            groupBoxLureSpots.TabStop = false;
            groupBoxLureSpots.Text = "أماكن سحب الوحوش (Lure Spots)";

            // 
            // btnAddLureSpot
            // 
            btnAddLureSpot.Color = System.Drawing.Color.FromArgb(33, 150, 243);
            btnAddLureSpot.ForeColor = System.Drawing.Color.White;
            btnAddLureSpot.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            btnAddLureSpot.Location = new System.Drawing.Point(16, 24);
            btnAddLureSpot.Name = "btnAddLureSpot";
            btnAddLureSpot.Radius = 6;
            btnAddLureSpot.ShadowDepth = 4F;
            btnAddLureSpot.Size = new System.Drawing.Size(364, 30);
            btnAddLureSpot.TabIndex = 0;
            btnAddLureSpot.Text = "+ إضافة موقع السحب (الموقع الحالي)";
            btnAddLureSpot.UseVisualStyleBackColor = true;
            btnAddLureSpot.Click += btnAddLureSpot_Click;

            // 
            // lstLureSpots
            // 
            lstLureSpots.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            lstLureSpots.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] { colIndex, colX, colY, colRadius });
            lstLureSpots.FullRowSelect = true;
            lstLureSpots.GridLines = true;
            lstLureSpots.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
            lstLureSpots.Location = new System.Drawing.Point(16, 60);
            lstLureSpots.MultiSelect = false;
            lstLureSpots.Name = "lstLureSpots";
            lstLureSpots.Size = new System.Drawing.Size(364, 130);
            lstLureSpots.TabIndex = 1;
            lstLureSpots.UseCompatibleStateImageBehavior = false;
            lstLureSpots.View = System.Windows.Forms.View.Details;

            // 
            // colIndex
            // 
            colIndex.Text = "#";
            colIndex.Width = 42;

            // 
            // colX
            // 
            colX.Text = "X";
            colX.Width = 100;

            // 
            // colY
            // 
            colY.Text = "Y";
            colY.Width = 100;

            // 
            // colRadius
            // 
            colRadius.Text = "Radius";
            colRadius.Width = 100;

            // 
            // lblSpotRadiusTitle
            // 
            lblSpotRadiusTitle.AutoSize = true;
            lblSpotRadiusTitle.Location = new System.Drawing.Point(16, 202);
            lblSpotRadiusTitle.Name = "lblSpotRadiusTitle";
            lblSpotRadiusTitle.Size = new System.Drawing.Size(69, 15);
            lblSpotRadiusTitle.TabIndex = 2;
            lblSpotRadiusTitle.Text = "نصف القطر:";

            // 
            // numSpotRadius
            // 
            numSpotRadius.BackColor = System.Drawing.Color.Transparent;
            numSpotRadius.Location = new System.Drawing.Point(92, 198);
            numSpotRadius.Maximum = new decimal(new int[] { 150, 0, 0, 0 });
            numSpotRadius.Minimum = new decimal(new int[] { 5, 0, 0, 0 });
            numSpotRadius.Name = "numSpotRadius";
            numSpotRadius.Size = new System.Drawing.Size(70, 24);
            numSpotRadius.TabIndex = 3;
            numSpotRadius.Value = new decimal(new int[] { 30, 0, 0, 0 });

            // 
            // btnDeleteLureSpot
            // 
            btnDeleteLureSpot.Color = System.Drawing.Color.FromArgb(217, 83, 79);
            btnDeleteLureSpot.ForeColor = System.Drawing.Color.White;
            btnDeleteLureSpot.Location = new System.Drawing.Point(180, 196);
            btnDeleteLureSpot.Name = "btnDeleteLureSpot";
            btnDeleteLureSpot.Radius = 4;
            btnDeleteLureSpot.ShadowDepth = 4F;
            btnDeleteLureSpot.Size = new System.Drawing.Size(95, 26);
            btnDeleteLureSpot.TabIndex = 4;
            btnDeleteLureSpot.Text = "حذف المحدد";
            btnDeleteLureSpot.UseVisualStyleBackColor = true;
            btnDeleteLureSpot.Click += btnDeleteLureSpot_Click;

            // 
            // btnClearLureSpots
            // 
            btnClearLureSpots.Color = System.Drawing.Color.Transparent;
            btnClearLureSpots.Location = new System.Drawing.Point(285, 196);
            btnClearLureSpots.Name = "btnClearLureSpots";
            btnClearLureSpots.Radius = 4;
            btnClearLureSpots.ShadowDepth = 4F;
            btnClearLureSpots.Size = new System.Drawing.Size(95, 26);
            btnClearLureSpots.TabIndex = 5;
            btnClearLureSpots.Text = "مسح الكل";
            btnClearLureSpots.UseVisualStyleBackColor = true;
            btnClearLureSpots.Click += btnClearLureSpots_Click;

            // 
            // checkWalkRandomly
            // 
            checkWalkRandomly.AutoSize = true;
            checkWalkRandomly.BackColor = System.Drawing.Color.Transparent;
            checkWalkRandomly.Depth = 0;
            checkWalkRandomly.Location = new System.Drawing.Point(16, 235);
            checkWalkRandomly.Margin = new System.Windows.Forms.Padding(0);
            checkWalkRandomly.MouseLocation = new System.Drawing.Point(-1, -1);
            checkWalkRandomly.Name = "checkWalkRandomly";
            checkWalkRandomly.Ripple = true;
            checkWalkRandomly.Size = new System.Drawing.Size(225, 30);
            checkWalkRandomly.TabIndex = 6;
            checkWalkRandomly.Text = "سحب عشوائي إذا لم يتم تحديد مواقع";
            checkWalkRandomly.UseVisualStyleBackColor = false;
            checkWalkRandomly.CheckedChanged += OnSettingsChanged;

            // 
            // groupBox3
            // 
            groupBox3.BackColor = System.Drawing.Color.Transparent;
            groupBox3.Controls.Add(checkUseHowlingShout);
            groupBox3.Controls.Add(checkNoHowlingAtCenter);
            groupBox3.Controls.Add(checkUseNormalAttack);
            groupBox3.Controls.Add(checkUseAttackingSkills);
            groupBox3.Controls.Add(separator1);
            groupBox3.Controls.Add(checkStopPartyMemberDead);
            groupBox3.Controls.Add(numPartyMemberDead);
            groupBox3.Controls.Add(checkStopPartyMember);
            groupBox3.Controls.Add(numPartyMember);
            groupBox3.Controls.Add(separator2);
            groupBox3.Controls.Add(checkStopMonsterType);
            groupBox3.Controls.Add(comboMonsterType);
            groupBox3.Controls.Add(label6);
            groupBox3.Controls.Add(numMonsterType);
            groupBox3.Location = new System.Drawing.Point(422, 10);
            groupBox3.Name = "groupBox3";
            groupBox3.Padding = new System.Windows.Forms.Padding(3, 8, 3, 3);
            groupBox3.Radius = 10;
            groupBox3.ShadowDepth = 4;
            groupBox3.Size = new System.Drawing.Size(380, 476);
            groupBox3.TabIndex = 2;
            groupBox3.TabStop = false;
            groupBox3.Text = "إعدادات السحب والمهارات";

            // 
            // checkUseHowlingShout
            // 
            checkUseHowlingShout.AutoSize = true;
            checkUseHowlingShout.BackColor = System.Drawing.Color.Transparent;
            checkUseHowlingShout.Depth = 0;
            checkUseHowlingShout.Location = new System.Drawing.Point(18, 28);
            checkUseHowlingShout.Margin = new System.Windows.Forms.Padding(0);
            checkUseHowlingShout.MouseLocation = new System.Drawing.Point(-1, -1);
            checkUseHowlingShout.Name = "checkUseHowlingShout";
            checkUseHowlingShout.Ripple = true;
            checkUseHowlingShout.Size = new System.Drawing.Size(270, 30);
            checkUseHowlingShout.TabIndex = 0;
            checkUseHowlingShout.Text = "استخدام Howling Shout (محارب أوروبي فقط)";
            checkUseHowlingShout.UseVisualStyleBackColor = false;
            checkUseHowlingShout.CheckedChanged += OnSettingsChanged;

            // 
            // checkNoHowlingAtCenter
            // 
            checkNoHowlingAtCenter.AutoSize = true;
            checkNoHowlingAtCenter.BackColor = System.Drawing.Color.Transparent;
            checkNoHowlingAtCenter.Checked = true;
            checkNoHowlingAtCenter.CheckState = System.Windows.Forms.CheckState.Checked;
            checkNoHowlingAtCenter.Depth = 0;
            checkNoHowlingAtCenter.Location = new System.Drawing.Point(38, 62);
            checkNoHowlingAtCenter.Margin = new System.Windows.Forms.Padding(0);
            checkNoHowlingAtCenter.MouseLocation = new System.Drawing.Point(-1, -1);
            checkNoHowlingAtCenter.Name = "checkNoHowlingAtCenter";
            checkNoHowlingAtCenter.Ripple = true;
            checkNoHowlingAtCenter.Size = new System.Drawing.Size(248, 30);
            checkNoHowlingAtCenter.TabIndex = 1;
            checkNoHowlingAtCenter.Text = "عدم تفعيل Howling Shout قرب البارتي";
            checkNoHowlingAtCenter.UseVisualStyleBackColor = false;
            checkNoHowlingAtCenter.CheckedChanged += OnSettingsChanged;

            // 
            // checkUseNormalAttack
            // 
            checkUseNormalAttack.AutoSize = true;
            checkUseNormalAttack.BackColor = System.Drawing.Color.Transparent;
            checkUseNormalAttack.Depth = 0;
            checkUseNormalAttack.Location = new System.Drawing.Point(18, 100);
            checkUseNormalAttack.Margin = new System.Windows.Forms.Padding(0);
            checkUseNormalAttack.MouseLocation = new System.Drawing.Point(-1, -1);
            checkUseNormalAttack.Name = "checkUseNormalAttack";
            checkUseNormalAttack.Ripple = true;
            checkUseNormalAttack.Size = new System.Drawing.Size(245, 30);
            checkUseNormalAttack.TabIndex = 2;
            checkUseNormalAttack.Text = "ضرب الوحوش بالضربة العادية لسحبها";
            checkUseNormalAttack.UseVisualStyleBackColor = false;
            checkUseNormalAttack.CheckedChanged += OnSettingsChanged;

            // 
            // checkUseAttackingSkills
            // 
            checkUseAttackingSkills.AutoSize = true;
            checkUseAttackingSkills.BackColor = System.Drawing.Color.Transparent;
            checkUseAttackingSkills.Depth = 0;
            checkUseAttackingSkills.Location = new System.Drawing.Point(18, 134);
            checkUseAttackingSkills.Margin = new System.Windows.Forms.Padding(0);
            checkUseAttackingSkills.MouseLocation = new System.Drawing.Point(-1, -1);
            checkUseAttackingSkills.Name = "checkUseAttackingSkills";
            checkUseAttackingSkills.Ripple = true;
            checkUseAttackingSkills.Size = new System.Drawing.Size(245, 30);
            checkUseAttackingSkills.TabIndex = 3;
            checkUseAttackingSkills.Text = "استخدام مهارة هجومية لسحب الوحوش";
            checkUseAttackingSkills.UseVisualStyleBackColor = false;
            checkUseAttackingSkills.CheckedChanged += OnSettingsChanged;

            // 
            // separator1
            // 
            separator1.IsVertical = false;
            separator1.Location = new System.Drawing.Point(18, 175);
            separator1.Name = "separator1";
            separator1.Size = new System.Drawing.Size(344, 10);
            separator1.TabIndex = 4;
            separator1.Text = "separator1";

            // 
            // checkStopPartyMemberDead
            // 
            checkStopPartyMemberDead.AutoSize = true;
            checkStopPartyMemberDead.BackColor = System.Drawing.Color.Transparent;
            checkStopPartyMemberDead.Depth = 0;
            checkStopPartyMemberDead.Location = new System.Drawing.Point(18, 195);
            checkStopPartyMemberDead.Margin = new System.Windows.Forms.Padding(0);
            checkStopPartyMemberDead.MouseLocation = new System.Drawing.Point(-1, -1);
            checkStopPartyMemberDead.Name = "checkStopPartyMemberDead";
            checkStopPartyMemberDead.Ripple = true;
            checkStopPartyMemberDead.Size = new System.Drawing.Size(235, 30);
            checkStopPartyMemberDead.TabIndex = 5;
            checkStopPartyMemberDead.Text = "إيقاف السحب إذا مات أعضاء >=";
            checkStopPartyMemberDead.UseVisualStyleBackColor = false;
            checkStopPartyMemberDead.CheckedChanged += OnSettingsChanged;

            // 
            // numPartyMemberDead
            // 
            numPartyMemberDead.BackColor = System.Drawing.Color.Transparent;
            numPartyMemberDead.Location = new System.Drawing.Point(282, 198);
            numPartyMemberDead.Maximum = new decimal(new int[] { 8, 0, 0, 0 });
            numPartyMemberDead.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
            numPartyMemberDead.Name = "numPartyMemberDead";
            numPartyMemberDead.Size = new System.Drawing.Size(80, 24);
            numPartyMemberDead.TabIndex = 6;
            numPartyMemberDead.Value = new decimal(new int[] { 3, 0, 0, 0 });
            numPartyMemberDead.ValueChanged += OnSettingsChanged;

            // 
            // checkStopPartyMember
            // 
            checkStopPartyMember.AutoSize = true;
            checkStopPartyMember.BackColor = System.Drawing.Color.Transparent;
            checkStopPartyMember.Depth = 0;
            checkStopPartyMember.Location = new System.Drawing.Point(18, 235);
            checkStopPartyMember.Margin = new System.Windows.Forms.Padding(0);
            checkStopPartyMember.MouseLocation = new System.Drawing.Point(-1, -1);
            checkStopPartyMember.Name = "checkStopPartyMember";
            checkStopPartyMember.Ripple = true;
            checkStopPartyMember.Size = new System.Drawing.Size(250, 30);
            checkStopPartyMember.TabIndex = 7;
            checkStopPartyMember.Text = "إيقاف السحب إذا قل أعضاء البارتي <=";
            checkStopPartyMember.UseVisualStyleBackColor = false;
            checkStopPartyMember.CheckedChanged += OnSettingsChanged;

            // 
            // numPartyMember
            // 
            numPartyMember.BackColor = System.Drawing.Color.Transparent;
            numPartyMember.Location = new System.Drawing.Point(282, 238);
            numPartyMember.Maximum = new decimal(new int[] { 8, 0, 0, 0 });
            numPartyMember.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
            numPartyMember.Name = "numPartyMember";
            numPartyMember.Size = new System.Drawing.Size(80, 24);
            numPartyMember.TabIndex = 8;
            numPartyMember.Value = new decimal(new int[] { 3, 0, 0, 0 });
            numPartyMember.ValueChanged += OnSettingsChanged;

            // 
            // separator2
            // 
            separator2.IsVertical = false;
            separator2.Location = new System.Drawing.Point(18, 278);
            separator2.Name = "separator2";
            separator2.Size = new System.Drawing.Size(344, 10);
            separator2.TabIndex = 9;
            separator2.Text = "separator2";

            // 
            // checkStopMonsterType
            // 
            checkStopMonsterType.AutoSize = true;
            checkStopMonsterType.BackColor = System.Drawing.Color.Transparent;
            checkStopMonsterType.Depth = 0;
            checkStopMonsterType.Location = new System.Drawing.Point(18, 298);
            checkStopMonsterType.Margin = new System.Windows.Forms.Padding(0);
            checkStopMonsterType.MouseLocation = new System.Drawing.Point(-1, -1);
            checkStopMonsterType.Name = "checkStopMonsterType";
            checkStopMonsterType.Ripple = true;
            checkStopMonsterType.Size = new System.Drawing.Size(240, 30);
            checkStopMonsterType.TabIndex = 10;
            checkStopMonsterType.Text = "إيقاف السحب إذا زاد عدد وحوش >=";
            checkStopMonsterType.UseVisualStyleBackColor = false;
            checkStopMonsterType.CheckedChanged += OnSettingsChanged;

            // 
            // comboMonsterType
            // 
            comboMonsterType.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawFixed;
            comboMonsterType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            comboMonsterType.FormattingEnabled = true;
            comboMonsterType.Items.AddRange(new object[] { "General (Default)", "Champion", "Giant", "General (Party)", "Champion (Party)", "Giant (Party)", "Elite", "Strong", "Unique", "Event" });
            comboMonsterType.Location = new System.Drawing.Point(18, 335);
            comboMonsterType.Name = "comboMonsterType";
            comboMonsterType.Radius = 5;
            comboMonsterType.ShadowDepth = 4F;
            comboMonsterType.Size = new System.Drawing.Size(180, 24);
            comboMonsterType.TabIndex = 11;
            comboMonsterType.SelectedIndexChanged += comboMonsterType_SelectedIndexChanged;

            // 
            // label6
            // 
            label6.AutoSize = true;
            label6.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            label6.Location = new System.Drawing.Point(215, 338);
            label6.Name = "label6";
            label6.Size = new System.Drawing.Size(23, 15);
            label6.TabIndex = 12;
            label6.Text = ">=";

            // 
            // numMonsterType
            // 
            numMonsterType.BackColor = System.Drawing.Color.Transparent;
            numMonsterType.Location = new System.Drawing.Point(282, 335);
            numMonsterType.Maximum = new decimal(new int[] { 100, 0, 0, 0 });
            numMonsterType.Minimum = new decimal(new int[] { 0, 0, 0, 0 });
            numMonsterType.Name = "numMonsterType";
            numMonsterType.Size = new System.Drawing.Size(80, 24);
            numMonsterType.TabIndex = 13;
            numMonsterType.Value = new decimal(new int[] { 3, 0, 0, 0 });
            numMonsterType.ValueChanged += OnSettingsChanged;

            // 
            // Main
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            Controls.Add(groupBoxParty);
            Controls.Add(groupBoxLureSpots);
            Controls.Add(groupBox3);
            Name = "Main";
            Size = new System.Drawing.Size(820, 500);
            Load += Main_Load;
            groupBoxParty.ResumeLayout(false);
            groupBoxParty.PerformLayout();
            groupBoxLureSpots.ResumeLayout(false);
            groupBoxLureSpots.PerformLayout();
            groupBox3.ResumeLayout(false);
            groupBox3.PerformLayout();
            ResumeLayout(false);
        }

        #endregion

        private SDUI.Controls.GroupBox groupBoxParty;
        private SDUI.Controls.Label lblPartyXTitle;
        private SDUI.Controls.Label lblPartyX;
        private SDUI.Controls.Label lblPartyYTitle;
        private SDUI.Controls.Label lblPartyY;
        private SDUI.Controls.Label lblPartyRadiusTitle;
        private SDUI.Controls.NumUpDown numPartyRadius;
        private SDUI.Controls.Button btnSetPartySpot;
        private SDUI.Controls.Label lblPartyWaitTitle;
        private SDUI.Controls.NumUpDown numPartyWaitSeconds;
        private SDUI.Controls.Label lblWalkscriptTitle;
        private SDUI.Controls.TextBox txtWalkbackScript;
        private SDUI.Controls.Button btnBrowseWalkscript;

        private SDUI.Controls.GroupBox groupBoxLureSpots;
        private SDUI.Controls.Button btnAddLureSpot;
        private SDUI.Controls.ListView lstLureSpots;
        private System.Windows.Forms.ColumnHeader colIndex;
        private System.Windows.Forms.ColumnHeader colX;
        private System.Windows.Forms.ColumnHeader colY;
        private System.Windows.Forms.ColumnHeader colRadius;
        private SDUI.Controls.Label lblSpotRadiusTitle;
        private SDUI.Controls.NumUpDown numSpotRadius;
        private SDUI.Controls.Button btnDeleteLureSpot;
        private SDUI.Controls.Button btnClearLureSpots;
        private SDUI.Controls.CheckBox checkWalkRandomly;

        private SDUI.Controls.GroupBox groupBox3;
        private SDUI.Controls.CheckBox checkUseHowlingShout;
        private SDUI.Controls.CheckBox checkNoHowlingAtCenter;
        private SDUI.Controls.CheckBox checkUseNormalAttack;
        private SDUI.Controls.CheckBox checkUseAttackingSkills;
        private SDUI.Controls.Separator separator1;
        private SDUI.Controls.CheckBox checkStopPartyMemberDead;
        private SDUI.Controls.NumUpDown numPartyMemberDead;
        private SDUI.Controls.CheckBox checkStopPartyMember;
        private SDUI.Controls.NumUpDown numPartyMember;
        private SDUI.Controls.Separator separator2;
        private SDUI.Controls.CheckBox checkStopMonsterType;
        private SDUI.Controls.ComboBox comboMonsterType;
        private SDUI.Controls.Label label6;
        private SDUI.Controls.NumUpDown numMonsterType;
    }
}
