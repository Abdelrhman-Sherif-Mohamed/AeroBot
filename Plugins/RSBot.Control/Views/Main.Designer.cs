namespace RSBot.Control.Views
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

            groupSettings.SuspendLayout();
            groupLeaders.SuspendLayout();
            groupLogs.SuspendLayout();
            SuspendLayout();

            // groupSettings
            groupSettings.Controls.Add(chkEnabled);
            groupSettings.Controls.Add(lblCommandsGuide);
            groupSettings.Location = new System.Drawing.Point(12, 12);
            groupSettings.Name = "groupSettings";
            groupSettings.Size = new System.Drawing.Size(350, 235);
            groupSettings.TabIndex = 0;
            groupSettings.TabStop = false;
            groupSettings.Text = "Settings & Commands";

            // chkEnabled
            chkEnabled.AutoSize = false;
            chkEnabled.Location = new System.Drawing.Point(15, 25);
            chkEnabled.Name = "chkEnabled";
            chkEnabled.Size = new System.Drawing.Size(320, 28);
            chkEnabled.TabIndex = 0;
            chkEnabled.Text = "Enable Remote Control via Chat";
            chkEnabled.UseVisualStyleBackColor = true;
            chkEnabled.CheckedChanged += chkEnabled_CheckedChanged;

            // lblCommandsGuide
            lblCommandsGuide.Location = new System.Drawing.Point(15, 60);
            lblCommandsGuide.Name = "lblCommandsGuide";
            lblCommandsGuide.Size = new System.Drawing.Size(320, 165);
            lblCommandsGuide.TabIndex = 1;
            lblCommandsGuide.Text = "Supported Commands (in Party/PM):\n• START / STOP : Start or stop bot\n• TRACE [name] / NOTRACE : Follow leader\n• RETURN : Use Return Scroll / Resurrect\n• ZERK : Activate Berserker\n• GETOUT : Leave Party\n• MOVEON [radius] : Move randomly\n• SETPOS / SETRADIUS : Area settings\n• GETPOS : Reply with coordinates\n• SIT / CAPE / USE [item]";

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

            // groupLogs
            groupLogs.Controls.Add(txtLogs);
            groupLogs.Controls.Add(btnClearLogs);
            groupLogs.Location = new System.Drawing.Point(12, 255);
            groupLogs.Name = "groupLogs";
            groupLogs.Size = new System.Drawing.Size(350, 187);
            groupLogs.TabIndex = 2;
            groupLogs.TabStop = false;
            groupLogs.Text = "Command Execution Log";

            // txtLogs
            txtLogs.BackColor = System.Drawing.Color.FromArgb(28, 30, 36);
            txtLogs.BorderStyle = System.Windows.Forms.BorderStyle.None;
            txtLogs.ForeColor = System.Drawing.Color.Gainsboro;
            txtLogs.Location = new System.Drawing.Point(10, 25);
            txtLogs.Name = "txtLogs";
            txtLogs.ReadOnly = true;
            txtLogs.Size = new System.Drawing.Size(330, 122);
            txtLogs.TabIndex = 0;
            txtLogs.Text = "";

            // btnClearLogs
            btnClearLogs.Location = new System.Drawing.Point(260, 152);
            btnClearLogs.Name = "btnClearLogs";
            btnClearLogs.Size = new System.Drawing.Size(80, 25);
            btnClearLogs.TabIndex = 1;
            btnClearLogs.Text = "Clear Log";
            btnClearLogs.UseVisualStyleBackColor = true;
            btnClearLogs.Click += btnClearLogs_Click;

            // Main
            AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            Controls.Add(groupSettings);
            Controls.Add(groupLeaders);
            Controls.Add(groupLogs);
            Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            Name = "Main";
            Size = new System.Drawing.Size(750, 458);
            Load += Main_Load;
            groupSettings.ResumeLayout(false);
            groupLeaders.ResumeLayout(false);
            groupLeaders.PerformLayout();
            groupLogs.ResumeLayout(false);
            ResumeLayout(false);
        }

        #endregion

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
    }
}
