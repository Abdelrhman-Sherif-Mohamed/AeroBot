using RSBot.Views.Controls;
using System.Windows.Forms;

namespace RSBot.Views
{
    partial class Main
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Main));
            stripStatus = new StatusStrip();
            lblIngameStatus = new ToolStripStatusLabel();
            menuCurrentProfile = new ToolStripMenuItem();
            cosController = new RSBot.Views.Controls.Cos.CosController();
                        lstLog = new ListView();
            colTime = new ColumnHeader();
            colType = new ColumnHeader();
            colMsg = new ColumnHeader();
            pnlBottomActions = new SDUI.Controls.Panel();
            btnStartBot = new SDUI.Controls.Button();
            btnStopBot = new SDUI.Controls.Button();
            btnShowClient = new SDUI.Controls.Button();
            btnClearLog = new SDUI.Controls.Button();
            bottomPanel = new SDUI.Controls.Panel();
            buttonConfig = new SDUI.Controls.Button();
            comboServer = new SDUI.Controls.ComboBox();
            comboDivision = new SDUI.Controls.ComboBox();
            btnSave = new SDUI.Controls.Button();
            btnStartStop = new SDUI.Controls.Button();
            pSidebar = new SDUI.Controls.Panel();
            pSidebarCustom = new SDUI.Controls.Panel();
            entity1 = new Entity();
            botbase1ToolStripMenuItem1 = new ToolStripMenuItem();
            botbase1ToolStripMenuItem = new ToolStripMenuItem();
            notifyIcon = new NotifyIcon(components);
            fileToolStripMenuItem = new ToolStripMenuItem();
            menuSelectProfile = new ToolStripMenuItem();
            networkConfigToolStripMenuItem = new ToolStripMenuItem();
            menuItemExit = new ToolStripMenuItem();
            viewToolStripMenuItem = new ToolStripMenuItem();
            menuSidebar = new ToolStripMenuItem();
            toolsToolStripMenuItem = new ToolStripMenuItem();
            menuScriptRecorder = new ToolStripMenuItem();
            menuPlugins = new ToolStripMenuItem();
            aboutToolStripMenuItem = new ToolStripMenuItem();
            thisToolStripMenuItem = new ToolStripMenuItem();
            menuStrip = new SDUI.Controls.ContextMenuStrip();
            botsToolStripMenuItem = new ToolStripMenuItem();
            languageToolStripMenuItem = new ToolStripMenuItem();
            themeToolStripMenuItem = new ToolStripMenuItem();
            darkToolStripMenuItem = new ToolStripMenuItem();
            lightToolStripMenuItem = new ToolStripMenuItem();
            autoToolStripMenuItem = new ToolStripMenuItem();
            coloredToolStripMenuItem = new ToolStripMenuItem();
            topCharacter = new Character();
            windowPageControl = new SDUI.Controls.WindowPageControl();
            stripStatus.SuspendLayout();
            bottomPanel.SuspendLayout();
            pnlBottomActions.SuspendLayout();
            pSidebar.SuspendLayout();
            menuStrip.SuspendLayout();
            SuspendLayout();
            // 
            // stripStatus
            // 
            stripStatus.BackColor = System.Drawing.Color.FromArgb(33, 150, 243);
            stripStatus.GripMargin = new Padding(2, 2, 0, 2);
            stripStatus.ImageScalingSize = new System.Drawing.Size(17, 17);
            stripStatus.Items.AddRange(new ToolStripItem[] { lblIngameStatus, menuCurrentProfile });
            stripStatus.LayoutStyle = ToolStripLayoutStyle.HorizontalStackWithOverflow;
            stripStatus.Location = new System.Drawing.Point(0, 806);
            stripStatus.Name = "stripStatus";
            stripStatus.Padding = new Padding(1, 0, 18, 0);
            stripStatus.Size = new System.Drawing.Size(1022, 26);
            stripStatus.SizingGrip = false;
            stripStatus.TabIndex = 0;
            stripStatus.Tag = "private";
            // 
            // lblIngameStatus
            // 
            lblIngameStatus.BackColor = System.Drawing.Color.Transparent;
            lblIngameStatus.ForeColor = System.Drawing.Color.White;
            lblIngameStatus.Name = "lblIngameStatus";
            lblIngameStatus.Size = new System.Drawing.Size(92, 20);
            lblIngameStatus.Text = "Not in game";
            // 
            // menuCurrentProfile
            // 
            menuCurrentProfile.Alignment = ToolStripItemAlignment.Right;
            menuCurrentProfile.ForeColor = System.Drawing.Color.White;
            menuCurrentProfile.Name = "menuCurrentProfile";
            menuCurrentProfile.Size = new System.Drawing.Size(122, 26);
            menuCurrentProfile.Text = "Profile: Default";
            menuCurrentProfile.Click += menuSelectProfile_Click;
            // 
            // cosController
            // 
            cosController.AutoSize = true;
            cosController.Dock = DockStyle.Top;
            cosController.Location = new System.Drawing.Point(0, 0);
            cosController.Margin = new Padding(5, 5, 5, 5);
            cosController.Name = "cosController";
            cosController.Padding = new Padding(4, 4, 4, 4);
            cosController.Size = new System.Drawing.Size(312, 88);
            cosController.TabIndex = 9;
            cosController.Visible = false;
            // 
            // bottomPanel
            // 
            bottomPanel.BackColor = System.Drawing.Color.Transparent;
            bottomPanel.Border = new Padding(0, 1, 0, 0);
            bottomPanel.BorderColor = System.Drawing.Color.Transparent;
            bottomPanel.Controls.Add(lstLog);
            bottomPanel.Controls.Add(pnlBottomActions);
            bottomPanel.Controls.Add(buttonConfig);
            bottomPanel.Controls.Add(comboServer);
            bottomPanel.Controls.Add(comboDivision);
            bottomPanel.Controls.Add(btnSave);
            bottomPanel.Controls.Add(btnStartStop);
            bottomPanel.Dock = DockStyle.Bottom;
            bottomPanel.Location = new System.Drawing.Point(0, 420);
            bottomPanel.Margin = new Padding(0);
            bottomPanel.Name = "bottomPanel";
            bottomPanel.Radius = 0;
            bottomPanel.ShadowDepth = 0F;
            bottomPanel.Size = new System.Drawing.Size(1022, 140);
            bottomPanel.TabIndex = 2;
            // 
            // lstLog
            // 
            lstLog.BackColor = System.Drawing.Color.FromArgb(18, 22, 33);
            lstLog.ForeColor = System.Drawing.Color.FromArgb(200, 210, 225);
            lstLog.BorderStyle = BorderStyle.None;
            lstLog.Columns.AddRange(new ColumnHeader[] { colTime, colType, colMsg });
            lstLog.Dock = DockStyle.Fill;
            lstLog.FullRowSelect = true;
            lstLog.HeaderStyle = ColumnHeaderStyle.None;
            lstLog.Location = new System.Drawing.Point(0, 0);
            lstLog.Margin = new Padding(0);
            lstLog.Name = "lstLog";
            lstLog.Size = new System.Drawing.Size(882, 140);
            lstLog.TabIndex = 0;
            lstLog.UseCompatibleStateImageBehavior = false;
            lstLog.View = View.Details;
            colTime.Width = 85;
            colType.Width = 75;
            colMsg.Width = 650;
            // 
            // pnlBottomActions
            // 
            pnlBottomActions.BackColor = System.Drawing.Color.Transparent;
            pnlBottomActions.Border = new Padding(1, 0, 0, 0);
            pnlBottomActions.BorderColor = System.Drawing.Color.Transparent;
            pnlBottomActions.Controls.Add(btnStartBot);
            pnlBottomActions.Controls.Add(btnStopBot);
            pnlBottomActions.Controls.Add(btnShowClient);
            pnlBottomActions.Controls.Add(btnClearLog);
            pnlBottomActions.Dock = DockStyle.Right;
            pnlBottomActions.Location = new System.Drawing.Point(882, 0);
            pnlBottomActions.Margin = new Padding(0);
            pnlBottomActions.Name = "pnlBottomActions";
            pnlBottomActions.Radius = 0;
            pnlBottomActions.ShadowDepth = 0F;
            pnlBottomActions.Size = new System.Drawing.Size(140, 140);
            pnlBottomActions.TabIndex = 1;
            // 
            // btnStartBot
            // 
            btnStartBot.Color = System.Drawing.Color.FromArgb(46, 171, 107);
            btnStartBot.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            btnStartBot.ForeColor = System.Drawing.Color.White;
            btnStartBot.Location = new System.Drawing.Point(8, 6);
            btnStartBot.Name = "btnStartBot";
            btnStartBot.Radius = 4;
            btnStartBot.Size = new System.Drawing.Size(124, 28);
            btnStartBot.TabIndex = 0;
            btnStartBot.Text = "شغل البوت";
            btnStartBot.UseVisualStyleBackColor = true;
            btnStartBot.Click += btnStartBot_Click;
            // 
            // btnStopBot
            // 
            btnStopBot.Color = System.Drawing.Color.FromArgb(217, 83, 79);
            btnStopBot.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            btnStopBot.ForeColor = System.Drawing.Color.White;
            btnStopBot.Location = new System.Drawing.Point(8, 38);
            btnStopBot.Name = "btnStopBot";
            btnStopBot.Radius = 4;
            btnStopBot.Size = new System.Drawing.Size(124, 28);
            btnStopBot.TabIndex = 1;
            btnStopBot.Text = "وقف البوت";
            btnStopBot.UseVisualStyleBackColor = true;
            btnStopBot.Click += btnStopBot_Click;
            // 
            // btnShowClient
            // 
            btnShowClient.Color = System.Drawing.Color.Transparent;
            btnShowClient.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            btnShowClient.Location = new System.Drawing.Point(8, 70);
            btnShowClient.Name = "btnShowClient";
            btnShowClient.Radius = 4;
            btnShowClient.Size = new System.Drawing.Size(124, 28);
            btnShowClient.TabIndex = 2;
            btnShowClient.Text = "إظهار الكلاينت";
            btnShowClient.UseVisualStyleBackColor = true;
            btnShowClient.Click += btnShowClient_Click;
            // 
            // btnClearLog
            // 
            btnClearLog.Color = System.Drawing.Color.Transparent;
            btnClearLog.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            btnClearLog.Location = new System.Drawing.Point(8, 102);
            btnClearLog.Name = "btnClearLog";
            btnClearLog.Radius = 4;
            btnClearLog.Size = new System.Drawing.Size(124, 28);
            btnClearLog.TabIndex = 3;
            btnClearLog.Text = "مسح السجلات";
            btnClearLog.UseVisualStyleBackColor = true;
            btnClearLog.Click += btnClearLog_Click;
            // 
            // buttonConfig
            // 
            buttonConfig.Location = new System.Drawing.Point(0, 0);
            buttonConfig.Name = "buttonConfig";
            buttonConfig.Size = new System.Drawing.Size(0, 0);
            buttonConfig.Visible = false;
            buttonConfig.Click += buttonConfig_Click;
            // 
            // comboServer
            // 
            comboServer.Location = new System.Drawing.Point(0, 0);
            comboServer.Name = "comboServer";
            comboServer.Size = new System.Drawing.Size(0, 0);
            comboServer.Visible = false;
            comboServer.SelectedIndexChanged += comboServer_SelectedIndexChanged;
            // 
            // comboDivision
            // 
            comboDivision.Location = new System.Drawing.Point(0, 0);
            comboDivision.Name = "comboDivision";
            comboDivision.Size = new System.Drawing.Size(0, 0);
            comboDivision.Visible = false;
            comboDivision.SelectedIndexChanged += comboDivision_SelectedIndexChanged;
            // 
            // btnSave
            // 
            btnSave.Location = new System.Drawing.Point(0, 0);
            btnSave.Name = "btnSave";
            btnSave.Size = new System.Drawing.Size(0, 0);
            btnSave.Visible = false;
            btnSave.Click += btnSave_Click;
            // 
            // btnStartStop
            // 
            btnStartStop.Location = new System.Drawing.Point(0, 0);
            btnStartStop.Name = "btnStartStop";
            btnStartStop.Size = new System.Drawing.Size(0, 0);
            btnStartStop.Visible = false;
            btnStartStop.Click += btnStartStop_Click;
            // 
            // pSidebar
            // 
            pSidebar.BackColor = System.Drawing.Color.Transparent;
            pSidebar.Border = new Padding(1, 0, 0, 0);
            pSidebar.BorderColor = System.Drawing.Color.Transparent;
            pSidebar.Controls.Add(pSidebarCustom);
            pSidebar.Controls.Add(entity1);
            pSidebar.Controls.Add(cosController);
            pSidebar.Dock = DockStyle.Right;
            pSidebar.Location = new System.Drawing.Point(943, 40);
            pSidebar.Margin = new Padding(4, 4, 4, 4);
            pSidebar.Name = "pSidebar";
            pSidebar.Radius = 0;
            pSidebar.ShadowDepth = 0F;
            pSidebar.Size = new System.Drawing.Size(260, 443);
            pSidebar.Visible = false;
            pSidebar.TabIndex = 6;
            // 
            // pSidebarCustom
            // 
            pSidebarCustom.Dock = DockStyle.Fill;
            pSidebarCustom.Location = new System.Drawing.Point(0, 192);
            pSidebarCustom.Margin = new Padding(4, 38, 4, 4);
            pSidebarCustom.Name = "pSidebarCustom";
            pSidebarCustom.Padding = new Padding(0, 20, 0, 0);
            pSidebarCustom.Size = new System.Drawing.Size(312, 509);
            pSidebarCustom.TabIndex = 14;
            pSidebarCustom.ShadowDepth = 0;
            pSidebarCustom.Radius = 0;
            // 
            // entity1
            // 
            entity1.Dock = DockStyle.Top;
            entity1.Location = new System.Drawing.Point(0, 88);
            entity1.Margin = new Padding(5, 5, 5, 5);
            entity1.MinimumSize = new System.Drawing.Size(312, 95);
            entity1.Name = "entity1";
            entity1.Size = new System.Drawing.Size(312, 104);
            entity1.TabIndex = 10;
            // 
            // botbase1ToolStripMenuItem1
            // 
            botbase1ToolStripMenuItem1.Name = "botbase1ToolStripMenuItem1";
            botbase1ToolStripMenuItem1.Size = new System.Drawing.Size(132, 22);
            botbase1ToolStripMenuItem1.Text = "Botbase #2";
            // 
            // botbase1ToolStripMenuItem
            // 
            botbase1ToolStripMenuItem.Name = "botbase1ToolStripMenuItem";
            botbase1ToolStripMenuItem.Size = new System.Drawing.Size(132, 22);
            botbase1ToolStripMenuItem.Text = "Botbase #1";
            // 
            // notifyIcon
            // 
            notifyIcon.BalloonTipIcon = ToolTipIcon.Info;
            notifyIcon.BalloonTipText = "Aerobot is currently running in the system tray.";
            notifyIcon.BalloonTipTitle = "Aerobot";
            notifyIcon.Icon = (System.Drawing.Icon)resources.GetObject("notifyIcon.Icon");
            notifyIcon.Text = "Aerobot";
            notifyIcon.Visible = true;
            notifyIcon.Click += notifyIcon_Click;
            // 
            // fileToolStripMenuItem
            // 
            fileToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { menuSelectProfile, networkConfigToolStripMenuItem, menuItemExit });
            fileToolStripMenuItem.ForeColor = System.Drawing.Color.FromArgb(0, 0, 0);
            fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            fileToolStripMenuItem.Size = new System.Drawing.Size(148, 26);
            fileToolStripMenuItem.Text = "File";
            // 
            // menuSelectProfile
            // 
            menuSelectProfile.ForeColor = System.Drawing.Color.FromArgb(0, 0, 0);
            menuSelectProfile.Name = "menuSelectProfile";
            menuSelectProfile.Size = new System.Drawing.Size(193, 26);
            menuSelectProfile.Text = "Select Profile...";
            menuSelectProfile.Click += menuSelectProfile_Click;
            // 
            // networkConfigToolStripMenuItem
            // 
            networkConfigToolStripMenuItem.ForeColor = System.Drawing.Color.FromArgb(0, 0, 0);
            networkConfigToolStripMenuItem.Name = "networkConfigToolStripMenuItem";
            networkConfigToolStripMenuItem.Size = new System.Drawing.Size(193, 26);
            networkConfigToolStripMenuItem.Text = "Proxy Config";
            networkConfigToolStripMenuItem.Click += networkConfigToolStripMenuItem_Click;
            // 
            // menuItemExit
            // 
            menuItemExit.ForeColor = System.Drawing.Color.FromArgb(0, 0, 0);
            menuItemExit.Name = "menuItemExit";
            menuItemExit.Size = new System.Drawing.Size(193, 26);
            menuItemExit.Text = "Exit";
            menuItemExit.Click += menuItemExit_Click;
            // 
            // viewToolStripMenuItem
            // 
            viewToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { menuSidebar });
            viewToolStripMenuItem.ForeColor = System.Drawing.Color.FromArgb(0, 0, 0);
            viewToolStripMenuItem.Name = "viewToolStripMenuItem";
            viewToolStripMenuItem.Size = new System.Drawing.Size(148, 26);
            viewToolStripMenuItem.Text = "View";
            // 
            // menuSidebar
            // 
            menuSidebar.Name = "menuSidebar";
            menuSidebar.Size = new System.Drawing.Size(147, 26);
            menuSidebar.Text = "Sidebar";
            menuSidebar.Click += menuSidebar_Click;
            // 
            // toolsToolStripMenuItem
            // 
            toolsToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { menuScriptRecorder });
            toolsToolStripMenuItem.ForeColor = System.Drawing.Color.FromArgb(0, 0, 0);
            toolsToolStripMenuItem.Name = "toolsToolStripMenuItem";
            toolsToolStripMenuItem.Size = new System.Drawing.Size(148, 26);
            toolsToolStripMenuItem.Text = "Tools";
            // 
            // menuScriptRecorder
            // 
            menuScriptRecorder.ForeColor = System.Drawing.Color.FromArgb(0, 0, 0);
            menuScriptRecorder.Name = "menuScriptRecorder";
            menuScriptRecorder.Size = new System.Drawing.Size(201, 26);
            menuScriptRecorder.Text = "Script Recorder";
            menuScriptRecorder.Click += menuScriptRecorder_Click;
            // 
            // menuPlugins
            // 
            menuPlugins.ForeColor = System.Drawing.Color.FromArgb(0, 0, 0);
            menuPlugins.Name = "menuPlugins";
            menuPlugins.Size = new System.Drawing.Size(148, 26);
            menuPlugins.Text = "Plugins";
            // 
            // aboutToolStripMenuItem
            // 
            aboutToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { thisToolStripMenuItem });
            aboutToolStripMenuItem.ForeColor = System.Drawing.Color.FromArgb(0, 0, 0);
            aboutToolStripMenuItem.Name = "aboutToolStripMenuItem";
            aboutToolStripMenuItem.Size = new System.Drawing.Size(148, 26);
            aboutToolStripMenuItem.Text = "About";
            // 
            // thisToolStripMenuItem
            // 
            thisToolStripMenuItem.ForeColor = System.Drawing.Color.FromArgb(0, 0, 0);
            thisToolStripMenuItem.Name = "thisToolStripMenuItem";
            thisToolStripMenuItem.Size = new System.Drawing.Size(122, 26);
            thisToolStripMenuItem.Text = "This";
            thisToolStripMenuItem.Click += menuItemThis_Click;
            // 
            // menuStrip
            // 
            menuStrip.BackColor = System.Drawing.Color.FromArgb(0, 0, 0, 0);
            menuStrip.Font = new System.Drawing.Font("Segoe UI", 9.25F);
            menuStrip.ImageScalingSize = new System.Drawing.Size(24, 24);
            menuStrip.Items.AddRange(new ToolStripItem[] { fileToolStripMenuItem, viewToolStripMenuItem, toolsToolStripMenuItem, menuPlugins, botsToolStripMenuItem, languageToolStripMenuItem, themeToolStripMenuItem, aboutToolStripMenuItem });
            menuStrip.Name = "menuStrip";
            menuStrip.Size = new System.Drawing.Size(149, 212);
            // 
            // botsToolStripMenuItem
            // 
            botsToolStripMenuItem.ForeColor = System.Drawing.Color.FromArgb(0, 0, 0);
            botsToolStripMenuItem.Name = "botsToolStripMenuItem";
            botsToolStripMenuItem.Size = new System.Drawing.Size(148, 26);
            botsToolStripMenuItem.Text = "Bots";
            // 
            // languageToolStripMenuItem
            // 
            languageToolStripMenuItem.ForeColor = System.Drawing.Color.FromArgb(0, 0, 0);
            languageToolStripMenuItem.Name = "languageToolStripMenuItem";
            languageToolStripMenuItem.Size = new System.Drawing.Size(148, 26);
            languageToolStripMenuItem.Text = "Language";
            // 
            // themeToolStripMenuItem
            // 
            themeToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { darkToolStripMenuItem, lightToolStripMenuItem, autoToolStripMenuItem, coloredToolStripMenuItem });
            themeToolStripMenuItem.ForeColor = System.Drawing.Color.FromArgb(0, 0, 0);
            themeToolStripMenuItem.Name = "themeToolStripMenuItem";
            themeToolStripMenuItem.Size = new System.Drawing.Size(148, 26);
            themeToolStripMenuItem.Text = "Theme";
            // 
            // darkToolStripMenuItem
            // 
            darkToolStripMenuItem.ForeColor = System.Drawing.Color.FromArgb(0, 0, 0);
            darkToolStripMenuItem.Name = "darkToolStripMenuItem";
            darkToolStripMenuItem.Size = new System.Drawing.Size(197, 26);
            darkToolStripMenuItem.Text = "Dark";
            darkToolStripMenuItem.Click += darkToolStripMenuItem_Click;
            // 
            // lightToolStripMenuItem
            // 
            lightToolStripMenuItem.ForeColor = System.Drawing.Color.FromArgb(0, 0, 0);
            lightToolStripMenuItem.Name = "lightToolStripMenuItem";
            lightToolStripMenuItem.Size = new System.Drawing.Size(197, 26);
            lightToolStripMenuItem.Text = "Light";
            lightToolStripMenuItem.Click += lightToolStripMenuItem_Click;
            // 
            // autoToolStripMenuItem
            // 
            autoToolStripMenuItem.ForeColor = System.Drawing.Color.FromArgb(0, 0, 0);
            autoToolStripMenuItem.Name = "autoToolStripMenuItem";
            autoToolStripMenuItem.Size = new System.Drawing.Size(197, 26);
            autoToolStripMenuItem.Text = "Auto";
            autoToolStripMenuItem.Click += autoToolStripMenuItem_Click;
            // 
            // coloredToolStripMenuItem
            // 
            coloredToolStripMenuItem.ForeColor = System.Drawing.Color.FromArgb(0, 0, 0);
            coloredToolStripMenuItem.Name = "coloredToolStripMenuItem";
            coloredToolStripMenuItem.Size = new System.Drawing.Size(197, 26);
            coloredToolStripMenuItem.Text = "Choose a color";
            coloredToolStripMenuItem.Click += coloredToolStripMenuItem_Click;
            // 
            // topCharacter
            // 
            topCharacter.BackColor = System.Drawing.Color.Transparent;
            topCharacter.Dock = DockStyle.Top;
            topCharacter.Font = new System.Drawing.Font("Segoe UI", 9.25F);
            topCharacter.Location = new System.Drawing.Point(0, 40);
            topCharacter.Margin = new Padding(5, 5, 5, 5);
            topCharacter.Name = "topCharacter";
            topCharacter.Visible = false;
            topCharacter.Height = 0;
            topCharacter.Size = new System.Drawing.Size(1022, 82);
            topCharacter.TabIndex = 7;
            // 
            // windowPageControl
            // 
            windowPageControl.AutoScroll = false;
            windowPageControl.Dock = DockStyle.Fill;
            windowPageControl.Location = new System.Drawing.Point(0, 132);
            windowPageControl.Margin = new Padding(4, 4, 4, 4);
            windowPageControl.Name = "windowPageControl";
            windowPageControl.SelectedIndex = -1;
            windowPageControl.Size = new System.Drawing.Size(1022, 521);
            windowPageControl.TabIndex = 13;
            // 
            // Main
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            AutoScaleMode = AutoScaleMode.None;
            ClientSize = new System.Drawing.Size(1022, 716);
            MaximizeBox = true;
            Resizable = true;
            VerticalTabs = true;
            ControlBox = false;
            Controls.Add(windowPageControl);
            Controls.Add(topCharacter);
            Controls.Add(pSidebar);
            Controls.Add(bottomPanel);
            Controls.Add(stripStatus);
            DwmMargin = 0;
            ExtendBox = true;
            ExtendMenu = menuStrip;
            Hatch = System.Drawing.Drawing2D.HatchStyle.Percent20;
            Icon = (System.Drawing.Icon)resources.GetObject("$this.Icon");
            Location = new System.Drawing.Point(0, 0);
            Margin = new Padding(4, 4, 4, 4);
            Name = "Main";
            StartPosition = FormStartPosition.CenterScreen;
            Tag = "";
            WindowPageControl = windowPageControl;
            FormClosing += Main_FormClosing;
            Load += Main_Load;
            Resize += Main_Resize;
            stripStatus.ResumeLayout(false);
            stripStatus.PerformLayout();
            bottomPanel.ResumeLayout(false);
            pnlBottomActions.ResumeLayout(false);
            bottomPanel.PerformLayout();
            pSidebar.ResumeLayout(false);
            pSidebar.PerformLayout();
            menuStrip.ResumeLayout(false);
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion
        private System.Windows.Forms.StatusStrip stripStatus;
                private System.Windows.Forms.ListView lstLog;
        private System.Windows.Forms.ColumnHeader colTime;
        private System.Windows.Forms.ColumnHeader colType;
        private System.Windows.Forms.ColumnHeader colMsg;
        private SDUI.Controls.Panel pnlBottomActions;
        private SDUI.Controls.Button btnStartBot;
        private SDUI.Controls.Button btnStopBot;
        private SDUI.Controls.Button btnShowClient;
        private SDUI.Controls.Button btnClearLog;
        private SDUI.Controls.Panel bottomPanel;
        private SDUI.Controls.Panel pSidebar;
        private System.Windows.Forms.ToolStripStatusLabel lblIngameStatus;
        private SDUI.Controls.Button btnStartStop;
        private SDUI.Controls.Button btnSave;
        private Controls.Character topCharacter;
        private Controls.Cos.CosController cosController;
        private SDUI.Controls.ComboBox comboServer;
        private SDUI.Controls.ComboBox comboDivision;
        private System.Windows.Forms.ToolStripMenuItem botbase1ToolStripMenuItem1;
        private System.Windows.Forms.ToolStripMenuItem botbase1ToolStripMenuItem;
        private System.Windows.Forms.NotifyIcon notifyIcon;
        private System.Windows.Forms.ToolStripMenuItem fileToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem menuItemExit;
        private System.Windows.Forms.ToolStripMenuItem viewToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem menuSidebar;
        private System.Windows.Forms.ToolStripMenuItem toolsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem menuScriptRecorder;
        private System.Windows.Forms.ToolStripMenuItem menuPlugins;
        private System.Windows.Forms.ToolStripMenuItem aboutToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem thisToolStripMenuItem;
        private SDUI.Controls.ContextMenuStrip menuStrip;
        private System.Windows.Forms.ToolStripMenuItem themeToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem darkToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem lightToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem coloredToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem languageToolStripMenuItem;
        private ToolStripMenuItem menuSelectProfile;
        private ToolStripMenuItem botsToolStripMenuItem;
        private ToolStripMenuItem autoToolStripMenuItem;
        private SDUI.Controls.Button buttonConfig;
        private ToolStripMenuItem networkConfigToolStripMenuItem;
        private ToolStripMenuItem menuCurrentProfile;
        private SDUI.Controls.WindowPageControl windowPageControl;
        private Entity entity1;
        private SDUI.Controls.Panel pSidebarCustom;
    }
}

