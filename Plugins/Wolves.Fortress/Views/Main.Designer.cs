namespace Wolves.Fortress.Views
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
            timerCooldown = new System.Windows.Forms.Timer(components);

            groupSettings = new SDUI.Controls.GroupBox();
            chkEnabled = new SDUI.Controls.CheckBox();
            chkAutoCast = new SDUI.Controls.CheckBox();
            lblChannel = new SDUI.Controls.Label();
            cboChannel = new SDUI.Controls.ComboBox();
            btnAutoFill = new SDUI.Controls.Button();
            lblMasterInfo = new SDUI.Controls.Label();

            // Slots 1 to 7
            groupSlots = new SDUI.Controls.GroupBox[7];
            chkSlotEnabled = new SDUI.Controls.CheckBox[7];
            lblSlotBadge = new SDUI.Controls.Label[7];
            cboPlayer = new SDUI.Controls.ComboBox[7];
            btnCast = new SDUI.Controls.Button[7];
            cboSkill = new SDUI.Controls.ComboBox[7];
            lblPlayerCaption = new SDUI.Controls.Label[7];
            lblSkillCaption = new SDUI.Controls.Label[7];
            lblStatus = new SDUI.Controls.Label[7];

            for (int i = 0; i < 7; i++)
            {
                groupSlots[i] = new SDUI.Controls.GroupBox();
                chkSlotEnabled[i] = new SDUI.Controls.CheckBox();
                lblSlotBadge[i] = new SDUI.Controls.Label();
                cboPlayer[i] = new SDUI.Controls.ComboBox();
                btnCast[i] = new SDUI.Controls.Button();
                cboSkill[i] = new SDUI.Controls.ComboBox();
                lblPlayerCaption[i] = new SDUI.Controls.Label();
                lblSkillCaption[i] = new SDUI.Controls.Label();
                lblStatus[i] = new SDUI.Controls.Label();
            }

            groupLogs = new SDUI.Controls.GroupBox();
            txtLogs = new FortressLogBox();
            btnClearLogs = new SDUI.Controls.Button();

            groupSettings.SuspendLayout();
            for (int i = 0; i < 7; i++)
                groupSlots[i].SuspendLayout();
            groupLogs.SuspendLayout();
            SuspendLayout();

            // groupSettings
            groupSettings.Controls.Add(chkEnabled);
            groupSettings.Controls.Add(chkAutoCast);
            groupSettings.Controls.Add(lblChannel);
            groupSettings.Controls.Add(cboChannel);
            groupSettings.Controls.Add(btnAutoFill);
            groupSettings.Controls.Add(lblMasterInfo);
            groupSettings.Location = new System.Drawing.Point(8, 6);
            groupSettings.Name = "groupSettings";
            groupSettings.Size = new System.Drawing.Size(912, 76);
            groupSettings.TabIndex = 0;
            groupSettings.TabStop = false;
            groupSettings.Text = "إعدادات حرب القلعة";

            // chkEnabled
            chkEnabled.AutoSize = false;
            chkEnabled.Location = new System.Drawing.Point(12, 20);
            chkEnabled.Name = "chkEnabled";
            chkEnabled.Size = new System.Drawing.Size(220, 24);
            chkEnabled.TabIndex = 0;
            chkEnabled.Text = "تفعيل نظام حرب القلعة";
            chkEnabled.UseVisualStyleBackColor = true;
            chkEnabled.CheckedChanged += chkEnabled_CheckedChanged;

            // chkAutoCast
            chkAutoCast.AutoSize = false;
            chkAutoCast.Location = new System.Drawing.Point(12, 45);
            chkAutoCast.Name = "chkAutoCast";
            chkAutoCast.Size = new System.Drawing.Size(220, 24);
            chkAutoCast.TabIndex = 1;
            chkAutoCast.Text = "إلقاء تلقائي عند الجاهزية";
            chkAutoCast.UseVisualStyleBackColor = true;
            chkAutoCast.CheckedChanged += chkAutoCast_CheckedChanged;

            // lblChannel
            lblChannel.AutoSize = true;
            lblChannel.Location = new System.Drawing.Point(238, 24);
            lblChannel.Name = "lblChannel";
            lblChannel.Size = new System.Drawing.Size(65, 17);
            lblChannel.TabIndex = 2;
            lblChannel.Text = "قناة الرد:";

            // cboChannel
            cboChannel.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            cboChannel.FormattingEnabled = true;
            cboChannel.Items.AddRange(new object[] {
                "شات المجموعة",
                "الهمس الخاص"
            });
            cboChannel.Location = new System.Drawing.Point(308, 20);
            cboChannel.Name = "cboChannel";
            cboChannel.Size = new System.Drawing.Size(160, 25);
            cboChannel.TabIndex = 3;
            cboChannel.SelectedIndex = 0;
            cboChannel.SelectedIndexChanged += cboChannel_SelectedIndexChanged;

            // btnAutoFill
            btnAutoFill.Location = new System.Drawing.Point(480, 18);
            btnAutoFill.Name = "btnAutoFill";
            btnAutoFill.Size = new System.Drawing.Size(242, 28);
            btnAutoFill.TabIndex = 4;
            btnAutoFill.Text = "توزيع أعضاء المجموعة تلقائيًا";
            btnAutoFill.UseVisualStyleBackColor = true;
            btnAutoFill.Click += btnAutoFill_Click;

            // lblMasterInfo
            lblMasterInfo.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            lblMasterInfo.ForeColor = System.Drawing.Color.FromArgb(16, 185, 129);
            lblMasterInfo.Location = new System.Drawing.Point(240, 48);
            lblMasterInfo.Name = "lblMasterInfo";
            lblMasterInfo.Size = new System.Drawing.Size(480, 20);
            lblMasterInfo.TabIndex = 5;
            lblMasterInfo.Text = "صاحب البوت: غير متصل باللعبة";

            // Setup 7 Slot GroupBoxes
            int[] slotX = { 8, 8, 8, 8, 470, 470, 470 };
            int[] slotY = { 86, 158, 230, 302, 86, 158, 230 };

            for (int i = 0; i < 7; i++)
            {
                int slotIndex = i + 1;
                var g = groupSlots[i];
                g.Location = new System.Drawing.Point(slotX[i], slotY[i]);
                g.Name = $"groupSlot{slotIndex}";
                g.Size = new System.Drawing.Size(450, 70);
                g.TabIndex = 6 + i;
                g.TabStop = false;
                g.Text = $"الخانة {slotIndex}";

                // chkSlotEnabled
                var chk = chkSlotEnabled[i];
                chk.AutoSize = true;
                chk.Location = new System.Drawing.Point(8, 16);
                chk.Name = $"chkSlotEnabled{slotIndex}";
                chk.Size = new System.Drawing.Size(55, 21);
                chk.TabIndex = 0;
                chk.Text = "تفعيل";
                chk.UseVisualStyleBackColor = true;
                int capturedIndex = i;
                chk.CheckedChanged += (s, e) => SlotEnabled_Changed(capturedIndex);

                // lblSlotBadge
                var badge = lblSlotBadge[i];
                badge.Font = new System.Drawing.Font("Segoe UI", 9.5F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
                badge.ForeColor = System.Drawing.Color.FromArgb(16, 185, 129);
                badge.Location = new System.Drawing.Point(65, 16);
                badge.Name = $"lblSlotBadge{slotIndex}";
                badge.Size = new System.Drawing.Size(42, 22);
                badge.TabIndex = 1;
                badge.Text = $"[b{slotIndex}]";

                // cboPlayer
                var cboP = cboPlayer[i];
                cboP.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDown;
                cboP.FormattingEnabled = true;
                cboP.Location = new System.Drawing.Point(110, 15);
                cboP.Name = $"cboPlayer{slotIndex}";
                cboP.Size = new System.Drawing.Size(195, 25);
                cboP.TabIndex = 2;
                cboP.TextChanged += (s, e) => SlotPlayer_Changed(capturedIndex);
                cboP.DropDown += (s, e) => RefreshPartyMembersDropDown(cboP);

                // btnCast
                var bCast = btnCast[i];
                bCast.Location = new System.Drawing.Point(315, 14);
                bCast.Name = $"btnCast{slotIndex}";
                bCast.Size = new System.Drawing.Size(125, 26);
                bCast.TabIndex = 3;
                bCast.Text = "إلقاء";
                bCast.UseVisualStyleBackColor = true;
                bCast.Click += (s, e) => CastSlotBuff(capturedIndex);

                // lblPlayerCaption
                var playerCaption = lblPlayerCaption[i];
                playerCaption.AutoSize = false;
                playerCaption.Font = new System.Drawing.Font("Segoe UI", 8.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
                playerCaption.ForeColor = System.Drawing.Color.FromArgb(160, 175, 195);
                playerCaption.Location = new System.Drawing.Point(8, 44);
                playerCaption.Name = $"lblPlayerCaption{slotIndex}";
                playerCaption.Size = new System.Drawing.Size(52, 25);
                playerCaption.TabIndex = 4;
                playerCaption.Text = "اللاعب";
                playerCaption.TextAlign = System.Drawing.ContentAlignment.MiddleRight;

                // cboSkill
                var cboS = cboSkill[i];
                cboS.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
                cboS.FormattingEnabled = true;
                cboS.Location = new System.Drawing.Point(8, 41);
                cboS.Name = $"cboSkill{slotIndex}";
                cboS.Size = new System.Drawing.Size(297, 25);
                cboS.TabIndex = 5;
                cboS.SelectedIndexChanged += (s, e) => SlotSkill_Changed(capturedIndex);
                cboS.DropDown += (s, e) => RefreshPlayerSkillsDropDown(cboS);

                // lblSkillCaption
                var skillCaption = lblSkillCaption[i];
                skillCaption.AutoSize = false;
                skillCaption.Font = new System.Drawing.Font("Segoe UI", 8.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
                skillCaption.ForeColor = System.Drawing.Color.FromArgb(160, 175, 195);
                skillCaption.Location = new System.Drawing.Point(8, 72);
                skillCaption.Name = $"lblSkillCaption{slotIndex}";
                skillCaption.Size = new System.Drawing.Size(52, 25);
                skillCaption.TabIndex = 6;
                skillCaption.Text = "البفة";
                skillCaption.TextAlign = System.Drawing.ContentAlignment.MiddleRight;

                // lblStatus
                var st = lblStatus[i];
                st.Font = new System.Drawing.Font("Segoe UI", 8.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
                st.ForeColor = System.Drawing.Color.FromArgb(160, 175, 195);
                st.Location = new System.Drawing.Point(120, 4);
                st.Name = $"lblStatus{slotIndex}";
                st.Size = new System.Drawing.Size(210, 20);
                st.TabIndex = 7;
                st.Text = "غير محدد";
                st.TextAlign = System.Drawing.ContentAlignment.MiddleRight;

                g.Controls.Add(chk);
                g.Controls.Add(badge);
                g.Controls.Add(cboP);
                g.Controls.Add(bCast);
                g.Controls.Add(playerCaption);
                g.Controls.Add(cboS);
                g.Controls.Add(skillCaption);
                g.Controls.Add(st);
                Controls.Add(g);
            }

            // groupLogs
            groupLogs.Controls.Add(txtLogs);
            groupLogs.Controls.Add(btnClearLogs);
            groupLogs.Location = new System.Drawing.Point(470, 302);
            groupLogs.Name = "groupLogs";
            groupLogs.Size = new System.Drawing.Size(450, 142);
            groupLogs.TabIndex = 13;
            groupLogs.TabStop = false;
            groupLogs.Text = "سجل عمليات القلعة";

            // txtLogs
            txtLogs.BackColor = System.Drawing.Color.FromArgb(20, 26, 36);
            txtLogs.BorderStyle = System.Windows.Forms.BorderStyle.None;
            txtLogs.ForeColor = System.Drawing.Color.FromArgb(230, 235, 245);
            txtLogs.Location = new System.Drawing.Point(10, 22);
            txtLogs.Name = "txtLogs";
            txtLogs.ReadOnly = true;
            txtLogs.Size = new System.Drawing.Size(430, 86);
            txtLogs.TabIndex = 0;
            txtLogs.Text = "";

            // btnClearLogs
            btnClearLogs.Location = new System.Drawing.Point(348, 111);
            btnClearLogs.Name = "btnClearLogs";
            btnClearLogs.Size = new System.Drawing.Size(92, 25);
            btnClearLogs.TabIndex = 1;
            btnClearLogs.Text = "مسح السجل";
            btnClearLogs.UseVisualStyleBackColor = true;
            btnClearLogs.Click += btnClearLogs_Click;

            // timerCooldown
            timerCooldown.Interval = 250;
            timerCooldown.Tick += timerCooldown_Tick;

            // Main
            AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            Controls.Add(groupSettings);
            Controls.Add(groupLogs);
            Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            Name = "Main";
            Size = new System.Drawing.Size(930, 458);
            Load += Main_Load;

            groupSettings.ResumeLayout(false);
            groupSettings.PerformLayout();
            for (int i = 0; i < 7; i++)
                groupSlots[i].ResumeLayout(false);
            groupLogs.ResumeLayout(false);
            ResumeLayout(false);
        }

        #endregion

        private System.Windows.Forms.Timer timerCooldown;

        private SDUI.Controls.GroupBox groupSettings;
        private SDUI.Controls.CheckBox chkEnabled;
        private SDUI.Controls.CheckBox chkAutoCast;
        private SDUI.Controls.Label lblChannel;
        private SDUI.Controls.ComboBox cboChannel;
        private SDUI.Controls.Button btnAutoFill;
        private SDUI.Controls.Label lblMasterInfo;

        private SDUI.Controls.GroupBox[] groupSlots;
        private SDUI.Controls.CheckBox[] chkSlotEnabled;
        private SDUI.Controls.Label[] lblSlotBadge;
        private SDUI.Controls.ComboBox[] cboPlayer;
        private SDUI.Controls.Button[] btnCast;
        private SDUI.Controls.ComboBox[] cboSkill;
        private SDUI.Controls.Label[] lblPlayerCaption;
        private SDUI.Controls.Label[] lblSkillCaption;
        private SDUI.Controls.Label[] lblStatus;

        private SDUI.Controls.GroupBox groupLogs;
        private FortressLogBox txtLogs;
        private SDUI.Controls.Button btnClearLogs;
    }
}
