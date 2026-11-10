using System;

namespace RSBot.Map.Views
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
            label4 = new SDUI.Controls.Label();
            comboViewType = new SDUI.Controls.ComboBox();
            trmInterval = new System.Windows.Forms.Timer(components);
            lvMonster = new SDUI.Controls.ListView();
            colName = new System.Windows.Forms.ColumnHeader();
            colType = new System.Windows.Forms.ColumnHeader();
            colLevel = new System.Windows.Forms.ColumnHeader();
            colPosition = new System.Windows.Forms.ColumnHeader();
            checkBoxAutoSelectUniques = new SDUI.Controls.CheckBox();
            label3 = new SDUI.Controls.Label();
            timerUniqueChecker = new System.Windows.Forms.Timer(components);
            checkEnableCollisions = new SDUI.Controls.CheckBox();
            tabControl1 = new SDUI.Controls.TabControl();
            mapCanvas = new System.Windows.Forms.TabPage();
            tabNavMeshViewer = new System.Windows.Forms.TabPage();
            btnNvmResetToPlayer = new SDUI.Controls.Button();
            labelSectorInfo = new SDUI.Controls.Label();
            panelNavMeshRendererCanvas = new System.Windows.Forms.Panel();
            panel1 = new System.Windows.Forms.Panel();
            panel2 = new System.Windows.Forms.Panel();
            panel3 = new System.Windows.Forms.Panel();
            tabWorldMap = new System.Windows.Forms.TabPage();
            panelWorldMapTop = new System.Windows.Forms.Panel();
            btnWorldMapCenterPlayer = new SDUI.Controls.Button();
            chkWorldMapAutoTrack = new SDUI.Controls.CheckBox();
            btnWorldMapReload = new SDUI.Controls.Button();
            btnWorldMapOpenExternal = new SDUI.Controls.Button();
            webViewWorldMap = new Microsoft.Web.WebView2.WinForms.WebView2();
            tabControl1.SuspendLayout();
            tabNavMeshViewer.SuspendLayout();
            tabWorldMap.SuspendLayout();
            panelWorldMapTop.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)webViewWorldMap).BeginInit();
            panel1.SuspendLayout();
            panel2.SuspendLayout();
            panel3.SuspendLayout();
            SuspendLayout();
            // 
            // label4
            // 
            label4.ApplyGradient = false;
            label4.AutoSize = true;
            label4.ForeColor = System.Drawing.Color.FromArgb(0, 0, 0);
            label4.Gradient = new System.Drawing.Color[]
    {
    System.Drawing.Color.Gray,
    System.Drawing.Color.Black
    };
            label4.GradientAnimation = false;
            label4.Location = new System.Drawing.Point(23, 15);
            label4.Name = "label4";
            label4.Size = new System.Drawing.Size(48, 20);
            label4.TabIndex = 9;
            label4.Text = "Show:";
            // 
            // comboViewType
            // 
            comboViewType.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawFixed;
            comboViewType.DropDownHeight = 100;
            comboViewType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            comboViewType.IntegralHeight = false;
            comboViewType.ItemHeight = 17;
            comboViewType.Items.AddRange(new object[] { "Monsters", "Players", "Party", "NPCs", "COS", "Portals", "All" });
            comboViewType.Location = new System.Drawing.Point(81, 14);
            comboViewType.Name = "comboViewType";
            comboViewType.Radius = 5;
            comboViewType.ShadowDepth = 4F;
            comboViewType.Size = new System.Drawing.Size(197, 23);
            comboViewType.TabIndex = 10;
            // 
            // trmInterval
            // 
            trmInterval.Enabled = true;
            trmInterval.Interval = 50;
            trmInterval.Tick += trmInterval_Tick;
            // 
            // lvMonster
            // 
            lvMonster.BackColor = System.Drawing.Color.White;
            lvMonster.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] { colName, colType, colLevel, colPosition });
            lvMonster.Dock = System.Windows.Forms.DockStyle.Top;
            lvMonster.ForeColor = System.Drawing.Color.FromArgb(0, 0, 0);
            lvMonster.FullRowSelect = true;
            lvMonster.Location = new System.Drawing.Point(0, 49);
            lvMonster.Name = "lvMonster";
            lvMonster.Size = new System.Drawing.Size(384, 291);
            lvMonster.TabIndex = 8;
            lvMonster.UseCompatibleStateImageBehavior = false;
            lvMonster.View = System.Windows.Forms.View.Details;
            // 
            // colName
            // 
            colName.Text = "Name";
            colName.Width = 130;
            // 
            // colType
            // 
            colType.Text = "Type";
            colType.Width = 64;
            // 
            // colLevel
            // 
            colLevel.Text = "Level";
            colLevel.Width = 50;
            // 
            // colPosition
            // 
            colPosition.Text = "Position";
            colPosition.Width = 125;
            // 
            // checkBoxAutoSelectUniques
            // 
            checkBoxAutoSelectUniques.AutoSize = true;
            checkBoxAutoSelectUniques.BackColor = System.Drawing.Color.Transparent;
            checkBoxAutoSelectUniques.Depth = 0;
            checkBoxAutoSelectUniques.Location = new System.Drawing.Point(23, 383);
            checkBoxAutoSelectUniques.Margin = new System.Windows.Forms.Padding(0);
            checkBoxAutoSelectUniques.MouseLocation = new System.Drawing.Point(-1, -1);
            checkBoxAutoSelectUniques.Name = "checkBoxAutoSelectUniques";
            checkBoxAutoSelectUniques.Ripple = true;
            checkBoxAutoSelectUniques.Size = new System.Drawing.Size(225, 30);
            checkBoxAutoSelectUniques.TabIndex = 17;
            checkBoxAutoSelectUniques.Text = "Automatically select uniques";
            checkBoxAutoSelectUniques.UseVisualStyleBackColor = false;
            checkBoxAutoSelectUniques.CheckedChanged += checkBoxAutoSelectUniques_CheckedChanged;
            // 
            // label3
            // 
            label3.ApplyGradient = false;
            label3.AutoSize = true;
            label3.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Italic, System.Drawing.GraphicsUnit.Point);
            label3.ForeColor = System.Drawing.Color.FromArgb(0, 0, 0);
            label3.Gradient = new System.Drawing.Color[]
    {
    System.Drawing.Color.Gray,
    System.Drawing.Color.Black
    };
            label3.GradientAnimation = false;
            label3.Location = new System.Drawing.Point(23, 416);
            label3.Name = "label3";
            label3.Size = new System.Drawing.Size(325, 57);
            label3.TabIndex = 18;
            label3.Text = "This function does not work while the bot is active.\r\n If you want the bot to auto attack while active,\r\n set Avoidance.";
            // 
            // timerUniqueChecker
            // 
            timerUniqueChecker.Enabled = true;
            timerUniqueChecker.Interval = 2500;
            timerUniqueChecker.Tick += timerUniqueChecker_Tick;
            // 
            // checkEnableCollisions
            // 
            checkEnableCollisions.AutoSize = true;
            checkEnableCollisions.BackColor = System.Drawing.Color.Transparent;
            checkEnableCollisions.Checked = true;
            checkEnableCollisions.CheckState = System.Windows.Forms.CheckState.Checked;
            checkEnableCollisions.Depth = 0;
            checkEnableCollisions.Location = new System.Drawing.Point(23, 355);
            checkEnableCollisions.Margin = new System.Windows.Forms.Padding(0);
            checkEnableCollisions.MouseLocation = new System.Drawing.Point(-1, -1);
            checkEnableCollisions.Name = "checkEnableCollisions";
            checkEnableCollisions.Ripple = true;
            checkEnableCollisions.Size = new System.Drawing.Size(204, 30);
            checkEnableCollisions.TabIndex = 19;
            checkEnableCollisions.Text = "Enable collision detection";
            checkEnableCollisions.UseVisualStyleBackColor = false;
            checkEnableCollisions.CheckedChanged += checkEnableCollisions_CheckedChanged;
            // 
            // tabControl1
            // 
            tabControl1.Controls.Add(mapCanvas);
            tabControl1.Controls.Add(tabNavMeshViewer);
            tabControl1.Controls.Add(tabWorldMap);
            tabControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            tabControl1.ItemSize = new System.Drawing.Size(80, 24);
            tabControl1.Location = new System.Drawing.Point(0, 0);
            tabControl1.Name = "tabControl1";
            tabControl1.Radius = new System.Windows.Forms.Padding(4);
            tabControl1.SelectedIndex = 0;
            tabControl1.Size = new System.Drawing.Size(366, 501);
            tabControl1.TabIndex = 20;
            // 
            // mapCanvas
            // 
            mapCanvas.BackColor = System.Drawing.Color.White;
            mapCanvas.Location = new System.Drawing.Point(4, 28);
            mapCanvas.Name = "mapCanvas";
            mapCanvas.Padding = new System.Windows.Forms.Padding(3);
            mapCanvas.Size = new System.Drawing.Size(358, 469);
            mapCanvas.TabIndex = 0;
            mapCanvas.Text = "Minimap";
            mapCanvas.Paint += tabMinimap_Paint;
            mapCanvas.MouseClick += mapCanvas_MouseClick;
            // 
            // tabNavMeshViewer
            // 
            tabNavMeshViewer.BackColor = System.Drawing.Color.White;
            tabNavMeshViewer.Controls.Add(panelNavMeshRendererCanvas);
            tabNavMeshViewer.Controls.Add(panel2);
            tabNavMeshViewer.Location = new System.Drawing.Point(4, 28);
            tabNavMeshViewer.Name = "tabNavMeshViewer";
            tabNavMeshViewer.Padding = new System.Windows.Forms.Padding(3);
            tabNavMeshViewer.Size = new System.Drawing.Size(358, 469);
            tabNavMeshViewer.TabIndex = 1;
            tabNavMeshViewer.Text = "NavMesh Viewer";
            // 
            // tabWorldMap
            // 
            tabWorldMap.BackColor = System.Drawing.Color.White;
            tabWorldMap.Controls.Add(webViewWorldMap);
            tabWorldMap.Controls.Add(panelWorldMapTop);
            tabWorldMap.Location = new System.Drawing.Point(4, 28);
            tabWorldMap.Name = "tabWorldMap";
            tabWorldMap.Padding = new System.Windows.Forms.Padding(0);
            tabWorldMap.Size = new System.Drawing.Size(358, 469);
            tabWorldMap.TabIndex = 2;
            tabWorldMap.Text = "خريطة العالم";
            // 
            // panelWorldMapTop
            // 
            panelWorldMapTop.BackColor = System.Drawing.Color.FromArgb(240, 240, 240);
            panelWorldMapTop.Controls.Add(btnWorldMapCenterPlayer);
            panelWorldMapTop.Controls.Add(chkWorldMapAutoTrack);
            panelWorldMapTop.Controls.Add(btnWorldMapReload);
            panelWorldMapTop.Controls.Add(btnWorldMapOpenExternal);
            panelWorldMapTop.Dock = System.Windows.Forms.DockStyle.Top;
            panelWorldMapTop.Location = new System.Drawing.Point(0, 0);
            panelWorldMapTop.Name = "panelWorldMapTop";
            panelWorldMapTop.Size = new System.Drawing.Size(358, 34);
            panelWorldMapTop.TabIndex = 1;
            // 
            // btnWorldMapCenterPlayer
            // 
            btnWorldMapCenterPlayer.Color = System.Drawing.Color.Transparent;
            btnWorldMapCenterPlayer.Location = new System.Drawing.Point(4, 5);
            btnWorldMapCenterPlayer.Name = "btnWorldMapCenterPlayer";
            btnWorldMapCenterPlayer.Radius = 4;
            btnWorldMapCenterPlayer.ShadowDepth = 2F;
            btnWorldMapCenterPlayer.Size = new System.Drawing.Size(85, 24);
            btnWorldMapCenterPlayer.TabIndex = 0;
            btnWorldMapCenterPlayer.Text = "توسيط اللاعب";
            btnWorldMapCenterPlayer.UseVisualStyleBackColor = true;
            btnWorldMapCenterPlayer.Click += btnWorldMapCenterPlayer_Click;
            // 
            // chkWorldMapAutoTrack
            // 
            chkWorldMapAutoTrack.AutoSize = true;
            chkWorldMapAutoTrack.Checked = true;
            chkWorldMapAutoTrack.CheckState = System.Windows.Forms.CheckState.Checked;
            chkWorldMapAutoTrack.Depth = 0;
            chkWorldMapAutoTrack.Location = new System.Drawing.Point(92, 5);
            chkWorldMapAutoTrack.Margin = new System.Windows.Forms.Padding(0);
            chkWorldMapAutoTrack.MouseLocation = new System.Drawing.Point(-1, -1);
            chkWorldMapAutoTrack.Name = "chkWorldMapAutoTrack";
            chkWorldMapAutoTrack.Ripple = true;
            chkWorldMapAutoTrack.Size = new System.Drawing.Size(75, 24);
            chkWorldMapAutoTrack.TabIndex = 1;
            chkWorldMapAutoTrack.Text = "تتبع مباشر";
            chkWorldMapAutoTrack.UseVisualStyleBackColor = true;
            // 
            // btnWorldMapReload
            // 
            btnWorldMapReload.Color = System.Drawing.Color.Transparent;
            btnWorldMapReload.Location = new System.Drawing.Point(170, 5);
            btnWorldMapReload.Name = "btnWorldMapReload";
            btnWorldMapReload.Radius = 4;
            btnWorldMapReload.ShadowDepth = 2F;
            btnWorldMapReload.Size = new System.Drawing.Size(55, 24);
            btnWorldMapReload.TabIndex = 2;
            btnWorldMapReload.Text = "تحديث";
            btnWorldMapReload.UseVisualStyleBackColor = true;
            btnWorldMapReload.Click += btnWorldMapReload_Click;
            // 
            // btnWorldMapOpenExternal
            // 
            btnWorldMapOpenExternal.Color = System.Drawing.Color.Transparent;
            btnWorldMapOpenExternal.Location = new System.Drawing.Point(230, 5);
            btnWorldMapOpenExternal.Name = "btnWorldMapOpenExternal";
            btnWorldMapOpenExternal.Radius = 4;
            btnWorldMapOpenExternal.ShadowDepth = 2F;
            btnWorldMapOpenExternal.Size = new System.Drawing.Size(65, 24);
            btnWorldMapOpenExternal.TabIndex = 3;
            btnWorldMapOpenExternal.Text = "متصفح";
            btnWorldMapOpenExternal.UseVisualStyleBackColor = true;
            btnWorldMapOpenExternal.Click += btnWorldMapOpenExternal_Click;
            // 
            // webViewWorldMap
            // 
            webViewWorldMap.Dock = System.Windows.Forms.DockStyle.Fill;
            webViewWorldMap.Location = new System.Drawing.Point(0, 34);
            webViewWorldMap.Name = "webViewWorldMap";
            webViewWorldMap.Size = new System.Drawing.Size(358, 435);
            webViewWorldMap.TabIndex = 0;
            // 
            // btnNvmResetToPlayer
            // 
            btnNvmResetToPlayer.Color = System.Drawing.Color.Transparent;
            btnNvmResetToPlayer.Location = new System.Drawing.Point(14, 7);
            btnNvmResetToPlayer.Name = "btnNvmResetToPlayer";
            btnNvmResetToPlayer.Radius = 6;
            btnNvmResetToPlayer.ShadowDepth = 4F;
            btnNvmResetToPlayer.Size = new System.Drawing.Size(122, 23);
            btnNvmResetToPlayer.TabIndex = 16;
            btnNvmResetToPlayer.Text = "Reset to player";
            btnNvmResetToPlayer.UseVisualStyleBackColor = true;
            btnNvmResetToPlayer.Click += btnNvmResetToPlayer_Click;
            // 
            // labelSectorInfo
            // 
            labelSectorInfo.ApplyGradient = false;
            labelSectorInfo.ForeColor = System.Drawing.Color.FromArgb(0, 0, 0);
            labelSectorInfo.Gradient = new System.Drawing.Color[]
    {
    System.Drawing.Color.Gray,
    System.Drawing.Color.Black
    };
            labelSectorInfo.GradientAnimation = false;
            labelSectorInfo.Location = new System.Drawing.Point(619, 10);
            labelSectorInfo.Name = "labelSectorInfo";
            labelSectorInfo.Size = new System.Drawing.Size(128, 17);
            labelSectorInfo.TabIndex = 15;
            labelSectorInfo.Text = "000x000";
            labelSectorInfo.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // panelNavMeshRendererCanvas
            // 
            panelNavMeshRendererCanvas.BackColor = System.Drawing.Color.FromArgb(64, 64, 64);
            panelNavMeshRendererCanvas.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            panelNavMeshRendererCanvas.Dock = System.Windows.Forms.DockStyle.Fill;
            panelNavMeshRendererCanvas.Location = new System.Drawing.Point(3, 3);
            panelNavMeshRendererCanvas.Name = "panelNavMeshRendererCanvas";
            panelNavMeshRendererCanvas.Size = new System.Drawing.Size(352, 427);
            panelNavMeshRendererCanvas.TabIndex = 1;
            // 
            // panel1
            // 
            panel1.Controls.Add(lvMonster);
            panel1.Controls.Add(panel3);
            panel1.Controls.Add(checkEnableCollisions);
            panel1.Controls.Add(label3);
            panel1.Controls.Add(checkBoxAutoSelectUniques);
            panel1.Dock = System.Windows.Forms.DockStyle.Right;
            panel1.Location = new System.Drawing.Point(366, 0);
            panel1.Name = "panel1";
            panel1.Size = new System.Drawing.Size(384, 501);
            panel1.TabIndex = 21;
            // 
            // panel2
            // 
            panel2.BackColor = System.Drawing.Color.Transparent;
            panel2.Controls.Add(btnNvmResetToPlayer);
            panel2.Controls.Add(labelSectorInfo);
            panel2.Dock = System.Windows.Forms.DockStyle.Bottom;
            panel2.Location = new System.Drawing.Point(3, 430);
            panel2.Name = "panel2";
            panel2.Size = new System.Drawing.Size(352, 36);
            panel2.TabIndex = 17;
            // 
            // panel3
            // 
            panel3.Controls.Add(comboViewType);
            panel3.Controls.Add(label4);
            panel3.Dock = System.Windows.Forms.DockStyle.Top;
            panel3.Location = new System.Drawing.Point(0, 0);
            panel3.Name = "panel3";
            panel3.Size = new System.Drawing.Size(384, 49);
            panel3.TabIndex = 20;
            // 
            // Main
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(120F, 120F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            Controls.Add(tabControl1);
            Controls.Add(panel1);
            Name = "Main";
            Size = new System.Drawing.Size(750, 501);
            Load += Main_Load;
            tabControl1.ResumeLayout(false);
            tabNavMeshViewer.ResumeLayout(false);
            tabWorldMap.ResumeLayout(false);
            panelWorldMapTop.ResumeLayout(false);
            panelWorldMapTop.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)webViewWorldMap).EndInit();
            panel1.ResumeLayout(false);
            panel1.PerformLayout();
            panel2.ResumeLayout(false);
            panel3.ResumeLayout(false);
            panel3.PerformLayout();
            ResumeLayout(false);
        }

        #endregion
        private SDUI.Controls.ListView lvMonster;
        private System.Windows.Forms.ColumnHeader colName;
        private System.Windows.Forms.ColumnHeader colType;
        private System.Windows.Forms.ColumnHeader colLevel;
        private SDUI.Controls.Label label4;
        private SDUI.Controls.ComboBox comboViewType;
        private System.Windows.Forms.ColumnHeader colPosition;
        private System.Windows.Forms.Timer trmInterval;
        private SDUI.Controls.CheckBox checkBoxAutoSelectUniques;
        private SDUI.Controls.Label label3;
        private System.Windows.Forms.Timer timerUniqueChecker;
        private SDUI.Controls.CheckBox checkEnableCollisions;
        private SDUI.Controls.TabControl tabControl1;
        private System.Windows.Forms.TabPage mapCanvas;
        private System.Windows.Forms.TabPage tabNavMeshViewer;
        private System.Windows.Forms.TabPage tabWorldMap;
        private System.Windows.Forms.Panel panelWorldMapTop;
        private SDUI.Controls.Button btnWorldMapCenterPlayer;
        private SDUI.Controls.CheckBox chkWorldMapAutoTrack;
        private SDUI.Controls.Button btnWorldMapReload;
        private SDUI.Controls.Button btnWorldMapOpenExternal;
        private Microsoft.Web.WebView2.WinForms.WebView2 webViewWorldMap;
        private System.Windows.Forms.Panel panelNavMeshRendererCanvas;
        private SDUI.Controls.Label labelSectorInfo;
        private SDUI.Controls.Button btnNvmResetToPlayer;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.Panel panel3;
    }
}
