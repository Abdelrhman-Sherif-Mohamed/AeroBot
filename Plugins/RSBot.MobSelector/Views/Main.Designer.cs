namespace RSBot.MobSelector.Views
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
            groupFilters = new SDUI.Controls.GroupBox();
            chkEnabled = new SDUI.Controls.CheckBox();
            chkTargetUniques = new SDUI.Controls.CheckBox();
            chkTargetTitans = new SDUI.Controls.CheckBox();
            chkTargetGiants = new SDUI.Controls.CheckBox();
            chkTargetElites = new SDUI.Controls.CheckBox();
            lblStatus = new SDUI.Controls.Label();

            groupNearby = new SDUI.Controls.GroupBox();
            lstNearby = new SDUI.Controls.ListView();
            colNearbyName = new System.Windows.Forms.ColumnHeader();
            colNearbyRarity = new System.Windows.Forms.ColumnHeader();
            colNearbyLevel = new System.Windows.Forms.ColumnHeader();
            colNearbyDist = new System.Windows.Forms.ColumnHeader();
            btnRefreshNearby = new SDUI.Controls.Button();
            chkAutoRefreshNearby = new SDUI.Controls.CheckBox();
            btnAddSelectedMob = new SDUI.Controls.Button();

            groupAutoTarget = new SDUI.Controls.GroupBox();
            lstTargetRules = new SDUI.Controls.ListView();
            colRuleName = new System.Windows.Forms.ColumnHeader();
            colRuleRarity = new System.Windows.Forms.ColumnHeader();
            lblCustomName = new SDUI.Controls.Label();
            txtCustomMobName = new SDUI.Controls.TextBox();
            lblCustomRarity = new SDUI.Controls.Label();
            cboCustomMobRarity = new SDUI.Controls.ComboBox();
            btnAddCustomRule = new SDUI.Controls.Button();
            btnRemoveRule = new SDUI.Controls.Button();
            btnClearRules = new SDUI.Controls.Button();

            groupFilters.SuspendLayout();
            groupNearby.SuspendLayout();
            groupAutoTarget.SuspendLayout();
            SuspendLayout();

            // groupFilters
            groupFilters.Controls.Add(chkEnabled);
            groupFilters.Controls.Add(chkTargetUniques);
            groupFilters.Controls.Add(chkTargetTitans);
            groupFilters.Controls.Add(chkTargetGiants);
            groupFilters.Controls.Add(chkTargetElites);
            groupFilters.Controls.Add(lblStatus);
            groupFilters.Location = new System.Drawing.Point(12, 10);
            groupFilters.Name = "groupFilters";
            groupFilters.Size = new System.Drawing.Size(726, 95);
            groupFilters.TabIndex = 0;
            groupFilters.TabStop = false;
            groupFilters.Text = "Scanner Status & Quick Presets";

            // chkEnabled
            chkEnabled.AutoSize = false;
            chkEnabled.Location = new System.Drawing.Point(15, 25);
            chkEnabled.Name = "chkEnabled";
            chkEnabled.Size = new System.Drawing.Size(190, 26);
            chkEnabled.TabIndex = 0;
            chkEnabled.Text = "Enable Auto Mob Selector";
            chkEnabled.UseVisualStyleBackColor = true;
            chkEnabled.CheckedChanged += chkEnabled_CheckedChanged;

            // chkTargetUniques
            chkTargetUniques.AutoSize = false;
            chkTargetUniques.Location = new System.Drawing.Point(220, 25);
            chkTargetUniques.Name = "chkTargetUniques";
            chkTargetUniques.Size = new System.Drawing.Size(150, 26);
            chkTargetUniques.TabIndex = 1;
            chkTargetUniques.Text = "Focus All Uniques";
            chkTargetUniques.UseVisualStyleBackColor = true;
            chkTargetUniques.CheckedChanged += chkTargetUniques_CheckedChanged;

            // chkTargetTitans
            chkTargetTitans.AutoSize = false;
            chkTargetTitans.Location = new System.Drawing.Point(380, 25);
            chkTargetTitans.Name = "chkTargetTitans";
            chkTargetTitans.Size = new System.Drawing.Size(150, 26);
            chkTargetTitans.TabIndex = 2;
            chkTargetTitans.Text = "Focus All Titans";
            chkTargetTitans.UseVisualStyleBackColor = true;
            chkTargetTitans.CheckedChanged += chkTargetTitans_CheckedChanged;

            // chkTargetGiants
            chkTargetGiants.AutoSize = false;
            chkTargetGiants.Location = new System.Drawing.Point(220, 55);
            chkTargetGiants.Name = "chkTargetGiants";
            chkTargetGiants.Size = new System.Drawing.Size(150, 26);
            chkTargetGiants.TabIndex = 3;
            chkTargetGiants.Text = "Focus All Giants";
            chkTargetGiants.UseVisualStyleBackColor = true;
            chkTargetGiants.CheckedChanged += chkTargetGiants_CheckedChanged;

            // chkTargetElites
            chkTargetElites.AutoSize = false;
            chkTargetElites.Location = new System.Drawing.Point(380, 55);
            chkTargetElites.Name = "chkTargetElites";
            chkTargetElites.Size = new System.Drawing.Size(150, 26);
            chkTargetElites.TabIndex = 4;
            chkTargetElites.Text = "Focus All Elites";
            chkTargetElites.UseVisualStyleBackColor = true;
            chkTargetElites.CheckedChanged += chkTargetElites_CheckedChanged;

            // lblStatus
            lblStatus.Location = new System.Drawing.Point(15, 60);
            lblStatus.Name = "lblStatus";
            lblStatus.Size = new System.Drawing.Size(190, 20);
            lblStatus.TabIndex = 5;
            lblStatus.Text = "Status: Idle";

            // groupNearby
            groupNearby.Controls.Add(lstNearby);
            groupNearby.Controls.Add(btnRefreshNearby);
            groupNearby.Controls.Add(chkAutoRefreshNearby);
            groupNearby.Controls.Add(btnAddSelectedMob);
            groupNearby.Location = new System.Drawing.Point(12, 115);
            groupNearby.Name = "groupNearby";
            groupNearby.Size = new System.Drawing.Size(370, 335);
            groupNearby.TabIndex = 1;
            groupNearby.TabStop = false;
            groupNearby.Text = "Monsters Around You";

            // lstNearby
            lstNearby.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] { colNearbyName, colNearbyRarity, colNearbyLevel, colNearbyDist });
            lstNearby.FullRowSelect = true;
            lstNearby.Location = new System.Drawing.Point(12, 25);
            lstNearby.Name = "lstNearby";
            lstNearby.Size = new System.Drawing.Size(346, 260);
            lstNearby.TabIndex = 0;
            lstNearby.UseCompatibleStateImageBehavior = false;
            lstNearby.View = System.Windows.Forms.View.Details;

            colNearbyName.Text = "Name";
            colNearbyName.Width = 140;
            colNearbyRarity.Text = "Rarity";
            colNearbyRarity.Width = 85;
            colNearbyLevel.Text = "Lv";
            colNearbyLevel.Width = 45;
            colNearbyDist.Text = "Dist";
            colNearbyDist.Width = 60;

            // btnRefreshNearby
            btnRefreshNearby.Location = new System.Drawing.Point(12, 295);
            btnRefreshNearby.Name = "btnRefreshNearby";
            btnRefreshNearby.Size = new System.Drawing.Size(75, 26);
            btnRefreshNearby.TabIndex = 1;
            btnRefreshNearby.Text = "Refresh";
            btnRefreshNearby.UseVisualStyleBackColor = true;
            btnRefreshNearby.Click += btnRefreshNearby_Click;

            // chkAutoRefreshNearby
            chkAutoRefreshNearby.AutoSize = false;
            chkAutoRefreshNearby.Location = new System.Drawing.Point(95, 295);
            chkAutoRefreshNearby.Name = "chkAutoRefreshNearby";
            chkAutoRefreshNearby.Size = new System.Drawing.Size(120, 26);
            chkAutoRefreshNearby.TabIndex = 2;
            chkAutoRefreshNearby.Text = "Auto Refresh";
            chkAutoRefreshNearby.UseVisualStyleBackColor = true;
            chkAutoRefreshNearby.CheckedChanged += chkAutoRefreshNearby_CheckedChanged;

            // btnAddSelectedMob
            btnAddSelectedMob.Location = new System.Drawing.Point(235, 295);
            btnAddSelectedMob.Name = "btnAddSelectedMob";
            btnAddSelectedMob.Size = new System.Drawing.Size(123, 26);
            btnAddSelectedMob.TabIndex = 3;
            btnAddSelectedMob.Text = "Add to Targets >>";
            btnAddSelectedMob.UseVisualStyleBackColor = true;
            btnAddSelectedMob.Click += btnAddSelectedMob_Click;

            // groupAutoTarget
            groupAutoTarget.Controls.Add(lstTargetRules);
            groupAutoTarget.Controls.Add(lblCustomName);
            groupAutoTarget.Controls.Add(txtCustomMobName);
            groupAutoTarget.Controls.Add(lblCustomRarity);
            groupAutoTarget.Controls.Add(cboCustomMobRarity);
            groupAutoTarget.Controls.Add(btnAddCustomRule);
            groupAutoTarget.Controls.Add(btnRemoveRule);
            groupAutoTarget.Controls.Add(btnClearRules);
            groupAutoTarget.Location = new System.Drawing.Point(395, 115);
            groupAutoTarget.Name = "groupAutoTarget";
            groupAutoTarget.Size = new System.Drawing.Size(343, 335);
            groupAutoTarget.TabIndex = 2;
            groupAutoTarget.TabStop = false;
            groupAutoTarget.Text = "Priority Target List";

            // lstTargetRules
            lstTargetRules.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] { colRuleName, colRuleRarity });
            lstTargetRules.FullRowSelect = true;
            lstTargetRules.Location = new System.Drawing.Point(12, 25);
            lstTargetRules.Name = "lstTargetRules";
            lstTargetRules.Size = new System.Drawing.Size(319, 210);
            lstTargetRules.TabIndex = 0;
            lstTargetRules.UseCompatibleStateImageBehavior = false;
            lstTargetRules.View = System.Windows.Forms.View.Details;

            colRuleName.Text = "Monster Name";
            colRuleName.Width = 190;
            colRuleRarity.Text = "Rarity";
            colRuleRarity.Width = 110;

            // lblCustomName
            lblCustomName.AutoSize = true;
            lblCustomName.Location = new System.Drawing.Point(12, 245);
            lblCustomName.Name = "lblCustomName";
            lblCustomName.Size = new System.Drawing.Size(42, 15);
            lblCustomName.TabIndex = 1;
            lblCustomName.Text = "Name:";

            // txtCustomMobName
            txtCustomMobName.Location = new System.Drawing.Point(60, 242);
            txtCustomMobName.Name = "txtCustomMobName";
            txtCustomMobName.Size = new System.Drawing.Size(120, 23);
            txtCustomMobName.TabIndex = 2;

            // lblCustomRarity
            lblCustomRarity.AutoSize = true;
            lblCustomRarity.Location = new System.Drawing.Point(185, 245);
            lblCustomRarity.Name = "lblCustomRarity";
            lblCustomRarity.Size = new System.Drawing.Size(40, 15);
            lblCustomRarity.TabIndex = 3;
            lblCustomRarity.Text = "Rarity:";

            // cboCustomMobRarity
            cboCustomMobRarity.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            cboCustomMobRarity.FormattingEnabled = true;
            cboCustomMobRarity.Location = new System.Drawing.Point(230, 242);
            cboCustomMobRarity.Name = "cboCustomMobRarity";
            cboCustomMobRarity.Size = new System.Drawing.Size(101, 23);
            cboCustomMobRarity.TabIndex = 4;

            // btnAddCustomRule
            btnAddCustomRule.Location = new System.Drawing.Point(12, 275);
            btnAddCustomRule.Name = "btnAddCustomRule";
            btnAddCustomRule.Size = new System.Drawing.Size(95, 26);
            btnAddCustomRule.TabIndex = 5;
            btnAddCustomRule.Text = "Add Rule";
            btnAddCustomRule.UseVisualStyleBackColor = true;
            btnAddCustomRule.Click += btnAddCustomRule_Click;

            // btnRemoveRule
            btnRemoveRule.Location = new System.Drawing.Point(120, 275);
            btnRemoveRule.Name = "btnRemoveRule";
            btnRemoveRule.Size = new System.Drawing.Size(95, 26);
            btnRemoveRule.TabIndex = 6;
            btnRemoveRule.Text = "Remove";
            btnRemoveRule.UseVisualStyleBackColor = true;
            btnRemoveRule.Click += btnRemoveRule_Click;

            // btnClearRules
            btnClearRules.Location = new System.Drawing.Point(236, 275);
            btnClearRules.Name = "btnClearRules";
            btnClearRules.Size = new System.Drawing.Size(95, 26);
            btnClearRules.TabIndex = 7;
            btnClearRules.Text = "Clear All";
            btnClearRules.UseVisualStyleBackColor = true;
            btnClearRules.Click += btnClearRules_Click;

            // Main
            AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            Controls.Add(groupFilters);
            Controls.Add(groupNearby);
            Controls.Add(groupAutoTarget);
            Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            Name = "Main";
            Size = new System.Drawing.Size(750, 458);
            Load += Main_Load;
            groupFilters.ResumeLayout(false);
            groupNearby.ResumeLayout(false);
            groupAutoTarget.ResumeLayout(false);
            groupAutoTarget.PerformLayout();
            ResumeLayout(false);
        }

        #endregion

        private SDUI.Controls.GroupBox groupFilters;
        private SDUI.Controls.CheckBox chkEnabled;
        private SDUI.Controls.CheckBox chkTargetUniques;
        private SDUI.Controls.CheckBox chkTargetTitans;
        private SDUI.Controls.CheckBox chkTargetGiants;
        private SDUI.Controls.CheckBox chkTargetElites;
        private SDUI.Controls.Label lblStatus;

        private SDUI.Controls.GroupBox groupNearby;
        private SDUI.Controls.ListView lstNearby;
        private System.Windows.Forms.ColumnHeader colNearbyName;
        private System.Windows.Forms.ColumnHeader colNearbyRarity;
        private System.Windows.Forms.ColumnHeader colNearbyLevel;
        private System.Windows.Forms.ColumnHeader colNearbyDist;
        private SDUI.Controls.Button btnRefreshNearby;
        private SDUI.Controls.CheckBox chkAutoRefreshNearby;
        private SDUI.Controls.Button btnAddSelectedMob;

        private SDUI.Controls.GroupBox groupAutoTarget;
        private SDUI.Controls.ListView lstTargetRules;
        private System.Windows.Forms.ColumnHeader colRuleName;
        private System.Windows.Forms.ColumnHeader colRuleRarity;
        private SDUI.Controls.Label lblCustomName;
        private SDUI.Controls.TextBox txtCustomMobName;
        private SDUI.Controls.Label lblCustomRarity;
        private SDUI.Controls.ComboBox cboCustomMobRarity;
        private SDUI.Controls.Button btnAddCustomRule;
        private SDUI.Controls.Button btnRemoveRule;
        private SDUI.Controls.Button btnClearRules;
    }
}
