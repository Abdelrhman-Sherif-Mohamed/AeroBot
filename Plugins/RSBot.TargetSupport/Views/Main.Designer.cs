namespace RSBot.TargetSupport.Views
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
            groupSettings = new SDUI.Controls.GroupBox();
            chkEnabled = new SDUI.Controls.CheckBox();
            chkDefensive = new SDUI.Controls.CheckBox();
            chkAutoAttack = new SDUI.Controls.CheckBox();
            lblDescription = new SDUI.Controls.Label();

            groupLeaders = new SDUI.Controls.GroupBox();
            lblLeader = new SDUI.Controls.Label();
            cboLeaderName = new SDUI.Controls.ComboBox();
            btnAddLeader = new SDUI.Controls.Button();
            btnRemoveLeader = new SDUI.Controls.Button();
            btnGetPartyLeader = new SDUI.Controls.Button();
            lstLeaders = new SDUI.Controls.ListView();
            colLeaderName = new System.Windows.Forms.ColumnHeader();

            groupStatus = new SDUI.Controls.GroupBox();
            lblStatus = new SDUI.Controls.Label();
            lblTarget = new SDUI.Controls.Label();

            groupSettings.SuspendLayout();
            groupLeaders.SuspendLayout();
            groupStatus.SuspendLayout();
            SuspendLayout();

            // groupSettings
            groupSettings.Controls.Add(chkEnabled);
            groupSettings.Controls.Add(chkDefensive);
            groupSettings.Controls.Add(chkAutoAttack);
            groupSettings.Controls.Add(lblDescription);
            groupSettings.Location = new System.Drawing.Point(12, 12);
            groupSettings.Name = "groupSettings";
            groupSettings.Size = new System.Drawing.Size(350, 200);
            groupSettings.TabIndex = 0;
            groupSettings.TabStop = false;
            groupSettings.Text = "Settings";

            // chkEnabled
            chkEnabled.AutoSize = false;
            chkEnabled.Location = new System.Drawing.Point(15, 30);
            chkEnabled.Name = "chkEnabled";
            chkEnabled.Size = new System.Drawing.Size(320, 28);
            chkEnabled.TabIndex = 0;
            chkEnabled.Text = "Enable Target Support";
            chkEnabled.UseVisualStyleBackColor = true;
            chkEnabled.CheckedChanged += chkEnabled_CheckedChanged;

            // chkDefensive
            chkDefensive.AutoSize = false;
            chkDefensive.Location = new System.Drawing.Point(15, 65);
            chkDefensive.Name = "chkDefensive";
            chkDefensive.Size = new System.Drawing.Size(320, 28);
            chkDefensive.TabIndex = 1;
            chkDefensive.Text = "Defensive Mode (Defend Leader)";
            chkDefensive.UseVisualStyleBackColor = true;
            chkDefensive.CheckedChanged += chkDefensive_CheckedChanged;

            // chkAutoAttack
            chkAutoAttack.AutoSize = false;
            chkAutoAttack.Location = new System.Drawing.Point(15, 100);
            chkAutoAttack.Name = "chkAutoAttack";
            chkAutoAttack.Size = new System.Drawing.Size(320, 28);
            chkAutoAttack.TabIndex = 2;
            chkAutoAttack.Text = "Auto-attack Synchronized Target";
            chkAutoAttack.UseVisualStyleBackColor = true;
            chkAutoAttack.CheckedChanged += chkAutoAttack_CheckedChanged;

            // lblDescription
            lblDescription.Location = new System.Drawing.Point(15, 138);
            lblDescription.Name = "lblDescription";
            lblDescription.Size = new System.Drawing.Size(320, 48);
            lblDescription.TabIndex = 3;
            lblDescription.Text = "Chat commands:\nTARGET ON - Enables synchronization\nTARGET OFF - Disables synchronization";

            // groupLeaders
            groupLeaders.Controls.Add(lblLeader);
            groupLeaders.Controls.Add(cboLeaderName);
            groupLeaders.Controls.Add(btnAddLeader);
            groupLeaders.Controls.Add(btnRemoveLeader);
            groupLeaders.Controls.Add(btnGetPartyLeader);
            groupLeaders.Controls.Add(lstLeaders);
            groupLeaders.Location = new System.Drawing.Point(380, 12);
            groupLeaders.Name = "groupLeaders";
            groupLeaders.Size = new System.Drawing.Size(350, 430);
            groupLeaders.TabIndex = 1;
            groupLeaders.TabStop = false;
            groupLeaders.Text = "Authorized Leaders";

            // lblLeader
            lblLeader.AutoSize = true;
            lblLeader.Location = new System.Drawing.Point(15, 30);
            lblLeader.Name = "lblLeader";
            lblLeader.Size = new System.Drawing.Size(160, 15);
            lblLeader.TabIndex = 0;
            lblLeader.Text = "Select Party / Type Name:";

            // cboLeaderName
            cboLeaderName.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDown;
            cboLeaderName.FormattingEnabled = true;
            cboLeaderName.Location = new System.Drawing.Point(15, 50);
            cboLeaderName.Name = "cboLeaderName";
            cboLeaderName.Size = new System.Drawing.Size(180, 23);
            cboLeaderName.TabIndex = 1;

            // btnAddLeader
            btnAddLeader.Location = new System.Drawing.Point(205, 50);
            btnAddLeader.Name = "btnAddLeader";
            btnAddLeader.Size = new System.Drawing.Size(60, 24);
            btnAddLeader.TabIndex = 2;
            btnAddLeader.Text = "Add";
            btnAddLeader.UseVisualStyleBackColor = true;
            btnAddLeader.Click += btnAddLeader_Click;

            // btnRemoveLeader
            btnRemoveLeader.Location = new System.Drawing.Point(270, 50);
            btnRemoveLeader.Name = "btnRemoveLeader";
            btnRemoveLeader.Size = new System.Drawing.Size(65, 24);
            btnRemoveLeader.TabIndex = 3;
            btnRemoveLeader.Text = "Remove";
            btnRemoveLeader.UseVisualStyleBackColor = true;
            btnRemoveLeader.Click += btnRemoveLeader_Click;

            // btnGetPartyLeader
            btnGetPartyLeader.Location = new System.Drawing.Point(15, 80);
            btnGetPartyLeader.Name = "btnGetPartyLeader";
            btnGetPartyLeader.Size = new System.Drawing.Size(320, 26);
            btnGetPartyLeader.TabIndex = 4;
            btnGetPartyLeader.Text = "Add Current Party Leader";
            btnGetPartyLeader.UseVisualStyleBackColor = true;
            btnGetPartyLeader.Click += btnGetPartyLeader_Click;

            // lstLeaders
            lstLeaders.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] { colLeaderName });
            lstLeaders.FullRowSelect = true;
            lstLeaders.Location = new System.Drawing.Point(15, 115);
            lstLeaders.Name = "lstLeaders";
            lstLeaders.Size = new System.Drawing.Size(320, 300);
            lstLeaders.TabIndex = 5;
            lstLeaders.UseCompatibleStateImageBehavior = false;
            lstLeaders.View = System.Windows.Forms.View.Details;

            // colLeaderName
            colLeaderName.Text = "Leader Name";
            colLeaderName.Width = 300;

            // groupStatus
            groupStatus.Controls.Add(lblStatus);
            groupStatus.Controls.Add(lblTarget);
            groupStatus.Location = new System.Drawing.Point(12, 225);
            groupStatus.Name = "groupStatus";
            groupStatus.Size = new System.Drawing.Size(350, 217);
            groupStatus.TabIndex = 2;
            groupStatus.TabStop = false;
            groupStatus.Text = "Status";

            // lblStatus
            lblStatus.Location = new System.Drawing.Point(15, 35);
            lblStatus.Name = "lblStatus";
            lblStatus.Size = new System.Drawing.Size(320, 30);
            lblStatus.TabIndex = 0;
            lblStatus.Text = "Status: Idle";

            // lblTarget
            lblTarget.Location = new System.Drawing.Point(15, 75);
            lblTarget.Name = "lblTarget";
            lblTarget.Size = new System.Drawing.Size(320, 30);
            lblTarget.TabIndex = 1;
            lblTarget.Text = "Current Target: None";

            // Main
            AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            Controls.Add(groupSettings);
            Controls.Add(groupLeaders);
            Controls.Add(groupStatus);
            Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            Name = "Main";
            Size = new System.Drawing.Size(750, 458);
            Load += Main_Load;
            groupSettings.ResumeLayout(false);
            groupLeaders.ResumeLayout(false);
            groupLeaders.PerformLayout();
            groupStatus.ResumeLayout(false);
            ResumeLayout(false);
        }

        #endregion

        private SDUI.Controls.GroupBox groupSettings;
        private SDUI.Controls.CheckBox chkEnabled;
        private SDUI.Controls.CheckBox chkDefensive;
        private SDUI.Controls.CheckBox chkAutoAttack;
        private SDUI.Controls.Label lblDescription;

        private SDUI.Controls.GroupBox groupLeaders;
        private SDUI.Controls.Label lblLeader;
        private SDUI.Controls.ComboBox cboLeaderName;
        private SDUI.Controls.Button btnAddLeader;
        private SDUI.Controls.Button btnRemoveLeader;
        private SDUI.Controls.Button btnGetPartyLeader;
        private SDUI.Controls.ListView lstLeaders;
        private System.Windows.Forms.ColumnHeader colLeaderName;

        private SDUI.Controls.GroupBox groupStatus;
        private SDUI.Controls.Label lblStatus;
        private SDUI.Controls.Label lblTarget;
    }
}
