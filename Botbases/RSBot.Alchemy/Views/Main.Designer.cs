using System.Drawing;
using System.Windows.Forms;

using Button = SDUI.Controls.Button;
using ColumnHeader = System.Windows.Forms.ColumnHeader;
using ComboBox = SDUI.Controls.ComboBox;
using GroupBox = SDUI.Controls.GroupBox;
using Label = SDUI.Controls.Label;
using ListView = SDUI.Controls.ListView;
using NumUpDown = SDUI.Controls.NumUpDown;
using Panel = SDUI.Controls.Panel;
using TabControl = SDUI.Controls.TabControl;
using TabPage = System.Windows.Forms.TabPage;

namespace RSBot.Alchemy.Views
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

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            tabMain = new TabControl();
            tabAlchemy = new TabPage();
            groupSettings = new GroupBox();
            tabEngine = new TabControl();
            tabEnhance = new TabPage();
            panelEnhance = new Panel();
            tabBlues = new TabPage();
            btnSelectAllBlues = new Button();
            panelBlues = new Panel();
            groupItem = new GroupBox();
            lblOptLevel = new Label();
            lblOptLevelText = new Label();
            lblDegree = new Label();
            lblDegreeText = new Label();
            tabItemInfo = new TabControl();
            tabItemBlues = new TabPage();
            lvItemBlues = new ListView();
            colBlueName = new ColumnHeader();
            colBlueValue = new ColumnHeader();
            colBlueMax = new ColumnHeader();
            tabItemStats = new TabPage();
            lvItemStats = new ListView();
            colStatName = new ColumnHeader();
            colStatValue = new ColumnHeader();
            linkRefreshItemList = new Label();
            comboItem = new ComboBox();
            lblItemSelection = new Label();
            groupLog = new GroupBox();
            lvLog = new ListView();
            colItem = new ColumnHeader();
            colResult = new ColumnHeader();
            btnClearLog = new Button();
            tabStats = new TabPage();
            groupStatsTools = new GroupBox();
            lblStatsItem = new Label();
            btnApplyStats = new Button();
            numStatsValue = new NumUpDown();
            lblStatsSet = new Label();
            btnSelectAllStats = new Button();
            panelAttributes = new Panel();
            tabManufacture = new TabPage();
            tabBreak = new TabPage();
            tabMain.SuspendLayout();
            tabAlchemy.SuspendLayout();
            groupSettings.SuspendLayout();
            tabEngine.SuspendLayout();
            tabEnhance.SuspendLayout();
            tabBlues.SuspendLayout();
            groupItem.SuspendLayout();
            tabItemInfo.SuspendLayout();
            tabItemBlues.SuspendLayout();
            tabItemStats.SuspendLayout();
            groupLog.SuspendLayout();
            tabStats.SuspendLayout();
            groupStatsTools.SuspendLayout();
            tabManufacture.SuspendLayout();
            tabBreak.SuspendLayout();
            SuspendLayout();
            // 
            // tabMain
            // 
            tabMain.Controls.Add(tabAlchemy);
            tabMain.Controls.Add(tabStats);
            tabMain.Controls.Add(tabManufacture);
            tabMain.Controls.Add(tabBreak);
            tabMain.ItemSize = new Size(150, 34);
            tabMain.Location = new Point(12, 12);
            tabMain.Name = "tabMain";
            tabMain.Padding = new Point(16, 6);
            tabMain.SelectedIndex = 0;
            tabMain.Size = new Size(1160, 660);
            tabMain.SizeMode = TabSizeMode.Fixed;
            tabMain.TabIndex = 0;
            tabMain.SelectedIndexChanged += tabMain_SelectedIndexChanged;
            // 
            // tabAlchemy
            // 
            tabAlchemy.BackColor = Color.Transparent;
            tabAlchemy.Controls.Add(groupSettings);
            tabAlchemy.Controls.Add(groupItem);
            tabAlchemy.Controls.Add(groupLog);
            tabAlchemy.Location = new Point(4, 38);
            tabAlchemy.Name = "tabAlchemy";
            tabAlchemy.Padding = new Padding(4);
            tabAlchemy.Size = new Size(1152, 618);
            tabAlchemy.TabIndex = 0;
            tabAlchemy.Text = "Alchemy";
            // 
            // groupSettings
            // 
            groupSettings.BackColor = Color.Transparent;
            groupSettings.Controls.Add(tabEngine);
            groupSettings.Location = new Point(420, 10);
            groupSettings.Name = "groupSettings";
            groupSettings.Radius = 10;
            groupSettings.ShadowDepth = 4;
            groupSettings.Size = new Size(720, 440);
            groupSettings.TabIndex = 1;
            groupSettings.TabStop = false;
            groupSettings.Text = "Settings";
            // 
            // tabEngine
            // 
            tabEngine.Controls.Add(tabEnhance);
            tabEngine.Controls.Add(tabBlues);
            tabEngine.ItemSize = new Size(140, 30);
            tabEngine.Location = new Point(12, 30);
            tabEngine.Name = "tabEngine";
            tabEngine.SelectedIndex = 0;
            tabEngine.Size = new Size(696, 398);
            tabEngine.SizeMode = TabSizeMode.Fixed;
            tabEngine.TabIndex = 0;
            tabEngine.SelectedIndexChanged += tabEngine_SelectedIndexChanged;
            // 
            // tabEnhance
            // 
            tabEnhance.BackColor = Color.Transparent;
            tabEnhance.Controls.Add(panelEnhance);
            tabEnhance.Location = new Point(4, 34);
            tabEnhance.Name = "tabEnhance";
            tabEnhance.Padding = new Padding(4);
            tabEnhance.Size = new Size(688, 360);
            tabEnhance.TabIndex = 0;
            tabEnhance.Text = "Enhance +";
            // 
            // panelEnhance
            // 
            panelEnhance.BackColor = Color.Transparent;
            panelEnhance.Border = new Padding(0);
            panelEnhance.BorderColor = Color.Transparent;
            panelEnhance.Dock = DockStyle.Fill;
            panelEnhance.Location = new Point(4, 4);
            panelEnhance.Name = "panelEnhance";
            panelEnhance.Radius = 0;
            panelEnhance.ShadowDepth = 0F;
            panelEnhance.Size = new Size(680, 352);
            panelEnhance.TabIndex = 0;
            // 
            // tabBlues
            // 
            tabBlues.BackColor = Color.Transparent;
            tabBlues.Controls.Add(panelBlues);
            tabBlues.Controls.Add(btnSelectAllBlues);
            tabBlues.Location = new Point(4, 34);
            tabBlues.Name = "tabBlues";
            tabBlues.Padding = new Padding(4);
            tabBlues.Size = new Size(688, 360);
            tabBlues.TabIndex = 1;
            tabBlues.Text = "Blues";
            // 
            // btnSelectAllBlues
            // 
            btnSelectAllBlues.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            btnSelectAllBlues.Location = new Point(462, 322);
            btnSelectAllBlues.Name = "btnSelectAllBlues";
            btnSelectAllBlues.Size = new Size(218, 32);
            btnSelectAllBlues.TabIndex = 1;
            btnSelectAllBlues.Text = "Select all Blues";
            btnSelectAllBlues.UseVisualStyleBackColor = true;
            btnSelectAllBlues.Click += btnSelectAllBlues_Click;
            // 
            // panelBlues
            // 
            panelBlues.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            panelBlues.BackColor = Color.Transparent;
            panelBlues.Border = new Padding(0);
            panelBlues.BorderColor = Color.Transparent;
            panelBlues.Location = new Point(6, 6);
            panelBlues.Name = "panelBlues";
            panelBlues.Radius = 0;
            panelBlues.ShadowDepth = 0F;
            panelBlues.Size = new Size(676, 308);
            panelBlues.TabIndex = 0;
            // 
            // groupItem
            // 
            groupItem.BackColor = Color.Transparent;
            groupItem.Controls.Add(lblOptLevel);
            groupItem.Controls.Add(lblOptLevelText);
            groupItem.Controls.Add(lblDegree);
            groupItem.Controls.Add(lblDegreeText);
            groupItem.Controls.Add(tabItemInfo);
            groupItem.Controls.Add(linkRefreshItemList);
            groupItem.Controls.Add(comboItem);
            groupItem.Controls.Add(lblItemSelection);
            groupItem.Location = new Point(10, 10);
            groupItem.Name = "groupItem";
            groupItem.Radius = 10;
            groupItem.ShadowDepth = 4;
            groupItem.Size = new Size(396, 440);
            groupItem.TabIndex = 0;
            groupItem.TabStop = false;
            groupItem.Text = "Item";
            // 
            // lblOptLevel
            // 
            lblOptLevel.AutoSize = false;
            lblOptLevel.Font = new Font("Segoe UI", 9F, FontStyle.Bold, GraphicsUnit.Point);
            lblOptLevel.Location = new Point(16, 100);
            lblOptLevel.Name = "lblOptLevel";
            lblOptLevel.Size = new Size(140, 22);
            lblOptLevel.TabIndex = 6;
            lblOptLevel.Text = "—";
            lblOptLevel.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // lblOptLevelText
            // 
            lblOptLevelText.AutoSize = false;
            lblOptLevelText.Location = new Point(166, 100);
            lblOptLevelText.Name = "lblOptLevelText";
            lblOptLevelText.Size = new Size(104, 22);
            lblOptLevelText.TabIndex = 5;
            lblOptLevelText.Text = "Enhancement:";
            lblOptLevelText.TextAlign = ContentAlignment.MiddleRight;
            // 
            // lblDegree
            // 
            lblDegree.AutoSize = false;
            lblDegree.Font = new Font("Segoe UI", 9F, FontStyle.Bold, GraphicsUnit.Point);
            lblDegree.Location = new Point(16, 72);
            lblDegree.Name = "lblDegree";
            lblDegree.Size = new Size(140, 22);
            lblDegree.TabIndex = 4;
            lblDegree.Text = "—";
            lblDegree.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // lblDegreeText
            // 
            lblDegreeText.AutoSize = false;
            lblDegreeText.Location = new Point(166, 72);
            lblDegreeText.Name = "lblDegreeText";
            lblDegreeText.Size = new Size(104, 22);
            lblDegreeText.TabIndex = 3;
            lblDegreeText.Text = "Degree:";
            lblDegreeText.TextAlign = ContentAlignment.MiddleRight;
            // 
            // tabItemInfo
            // 
            tabItemInfo.Controls.Add(tabItemBlues);
            tabItemInfo.Controls.Add(tabItemStats);
            tabItemInfo.ItemSize = new Size(130, 28);
            tabItemInfo.Location = new Point(12, 130);
            tabItemInfo.Name = "tabItemInfo";
            tabItemInfo.SelectedIndex = 0;
            tabItemInfo.Size = new Size(372, 296);
            tabItemInfo.SizeMode = TabSizeMode.Fixed;
            tabItemInfo.TabIndex = 7;
            // 
            // tabItemBlues
            // 
            tabItemBlues.BackColor = Color.Transparent;
            tabItemBlues.Controls.Add(lvItemBlues);
            tabItemBlues.Location = new Point(4, 32);
            tabItemBlues.Name = "tabItemBlues";
            tabItemBlues.Padding = new Padding(3);
            tabItemBlues.Size = new Size(364, 260);
            tabItemBlues.TabIndex = 0;
            tabItemBlues.Text = "Blues";
            // 
            // lvItemBlues
            // 
            lvItemBlues.BorderStyle = BorderStyle.FixedSingle;
            lvItemBlues.Columns.AddRange(new ColumnHeader[] { colBlueName, colBlueValue, colBlueMax });
            lvItemBlues.Dock = DockStyle.Fill;
            lvItemBlues.FullRowSelect = true;
            lvItemBlues.HeaderStyle = ColumnHeaderStyle.Nonclickable;
            lvItemBlues.Location = new Point(3, 3);
            lvItemBlues.MultiSelect = false;
            lvItemBlues.Name = "lvItemBlues";
            lvItemBlues.Size = new Size(358, 254);
            lvItemBlues.TabIndex = 0;
            lvItemBlues.UseCompatibleStateImageBehavior = false;
            lvItemBlues.View = View.Details;
            // 
            // colBlueName
            // 
            colBlueName.Text = "Option";
            colBlueName.Width = 210;
            // 
            // colBlueValue
            // 
            colBlueValue.Text = "Value";
            colBlueValue.TextAlign = HorizontalAlignment.Center;
            colBlueValue.Width = 70;
            // 
            // colBlueMax
            // 
            colBlueMax.Text = "Max";
            colBlueMax.TextAlign = HorizontalAlignment.Center;
            colBlueMax.Width = 70;
            // 
            // tabItemStats
            // 
            tabItemStats.BackColor = Color.Transparent;
            tabItemStats.Controls.Add(lvItemStats);
            tabItemStats.Location = new Point(4, 32);
            tabItemStats.Name = "tabItemStats";
            tabItemStats.Padding = new Padding(3);
            tabItemStats.Size = new Size(364, 260);
            tabItemStats.TabIndex = 1;
            tabItemStats.Text = "Stats";
            // 
            // lvItemStats
            // 
            lvItemStats.BorderStyle = BorderStyle.FixedSingle;
            lvItemStats.Columns.AddRange(new ColumnHeader[] { colStatName, colStatValue });
            lvItemStats.Dock = DockStyle.Fill;
            lvItemStats.FullRowSelect = true;
            lvItemStats.HeaderStyle = ColumnHeaderStyle.Nonclickable;
            lvItemStats.Location = new Point(3, 3);
            lvItemStats.MultiSelect = false;
            lvItemStats.Name = "lvItemStats";
            lvItemStats.Size = new Size(358, 254);
            lvItemStats.TabIndex = 0;
            lvItemStats.UseCompatibleStateImageBehavior = false;
            lvItemStats.View = View.Details;
            // 
            // colStatName
            // 
            colStatName.Text = "Attribute";
            colStatName.Width = 250;
            // 
            // colStatValue
            // 
            colStatValue.Text = "Value";
            colStatValue.TextAlign = HorizontalAlignment.Center;
            colStatValue.Width = 100;
            // 
            // linkRefreshItemList
            // 
            linkRefreshItemList.AutoSize = false;
            linkRefreshItemList.Cursor = Cursors.Hand;
            linkRefreshItemList.Font = new Font("Segoe UI", 15.75F, FontStyle.Regular, GraphicsUnit.Point);
            linkRefreshItemList.Location = new Point(12, 30);
            linkRefreshItemList.Name = "linkRefreshItemList";
            linkRefreshItemList.Size = new Size(36, 30);
            linkRefreshItemList.TabIndex = 2;
            linkRefreshItemList.Text = "↻";
            linkRefreshItemList.TextAlign = ContentAlignment.MiddleCenter;
            linkRefreshItemList.Click += linkRefreshItemList_Click;
            // 
            // comboItem
            // 
            comboItem.DrawMode = DrawMode.OwnerDrawFixed;
            comboItem.DropDownHeight = 220;
            comboItem.DropDownStyle = ComboBoxStyle.DropDownList;
            comboItem.FormattingEnabled = true;
            comboItem.IntegralHeight = false;
            comboItem.ItemHeight = 17;
            comboItem.Location = new Point(54, 32);
            comboItem.Name = "comboItem";
            comboItem.Radius = 5;
            comboItem.ShadowDepth = 4F;
            comboItem.Size = new Size(240, 26);
            comboItem.TabIndex = 1;
            comboItem.SelectedIndexChanged += comboItem_SelectedIndexChanged;
            // 
            // lblItemSelection
            // 
            lblItemSelection.AutoSize = false;
            lblItemSelection.Location = new Point(300, 32);
            lblItemSelection.Name = "lblItemSelection";
            lblItemSelection.Size = new Size(80, 26);
            lblItemSelection.TabIndex = 0;
            lblItemSelection.Text = "Item:";
            lblItemSelection.TextAlign = ContentAlignment.MiddleRight;
            // 
            // groupLog
            // 
            groupLog.BackColor = Color.Transparent;
            groupLog.Controls.Add(lvLog);
            groupLog.Controls.Add(btnClearLog);
            groupLog.Location = new Point(10, 462);
            groupLog.Name = "groupLog";
            groupLog.Radius = 10;
            groupLog.ShadowDepth = 4;
            groupLog.Size = new Size(1132, 146);
            groupLog.TabIndex = 2;
            groupLog.TabStop = false;
            groupLog.Text = "Log";
            // 
            // lvLog
            // 
            lvLog.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            lvLog.BorderStyle = BorderStyle.FixedSingle;
            lvLog.Columns.AddRange(new ColumnHeader[] { colItem, colResult });
            lvLog.FullRowSelect = true;
            lvLog.Location = new Point(12, 32);
            lvLog.Name = "lvLog";
            lvLog.Size = new Size(1030, 102);
            lvLog.TabIndex = 0;
            lvLog.UseCompatibleStateImageBehavior = false;
            lvLog.View = View.Details;
            // 
            // colItem
            // 
            colItem.Text = "Item";
            colItem.Width = 280;
            // 
            // colResult
            // 
            colResult.Text = "Message";
            colResult.Width = 720;
            // 
            // btnClearLog
            // 
            btnClearLog.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnClearLog.Location = new Point(1050, 30);
            btnClearLog.Name = "btnClearLog";
            btnClearLog.Size = new Size(72, 28);
            btnClearLog.TabIndex = 1;
            btnClearLog.Text = "Clear";
            btnClearLog.UseVisualStyleBackColor = true;
            btnClearLog.Click += btnClearLog_Click;
            // 
            // tabStats
            // 
            tabStats.BackColor = Color.Transparent;
            tabStats.Controls.Add(groupStatsTools);
            tabStats.Controls.Add(panelAttributes);
            tabStats.Location = new Point(4, 38);
            tabStats.Name = "tabStats";
            tabStats.Padding = new Padding(4);
            tabStats.Size = new Size(1152, 618);
            tabStats.TabIndex = 1;
            tabStats.Text = "Stats";
            // 
            // groupStatsTools
            // 
            groupStatsTools.BackColor = Color.Transparent;
            groupStatsTools.Controls.Add(lblStatsItem);
            groupStatsTools.Controls.Add(btnApplyStats);
            groupStatsTools.Controls.Add(numStatsValue);
            groupStatsTools.Controls.Add(lblStatsSet);
            groupStatsTools.Controls.Add(btnSelectAllStats);
            groupStatsTools.Location = new Point(10, 10);
            groupStatsTools.Name = "groupStatsTools";
            groupStatsTools.Radius = 10;
            groupStatsTools.ShadowDepth = 4;
            groupStatsTools.Size = new Size(1132, 78);
            groupStatsTools.TabIndex = 0;
            groupStatsTools.TabStop = false;
            groupStatsTools.Text = "Stats settings";
            // 
            // lblStatsItem
            // 
            lblStatsItem.AutoSize = false;
            lblStatsItem.Location = new Point(16, 32);
            lblStatsItem.Name = "lblStatsItem";
            lblStatsItem.Size = new Size(320, 28);
            lblStatsItem.TabIndex = 4;
            lblStatsItem.Text = "Item: —";
            lblStatsItem.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // btnApplyStats
            // 
            btnApplyStats.Location = new Point(840, 30);
            btnApplyStats.Name = "btnApplyStats";
            btnApplyStats.Size = new Size(96, 30);
            btnApplyStats.TabIndex = 3;
            btnApplyStats.Text = "Set";
            btnApplyStats.UseVisualStyleBackColor = true;
            btnApplyStats.Click += btnApplyStats_Click;
            // 
            // numStatsValue
            // 
            numStatsValue.Location = new Point(948, 32);
            numStatsValue.Maximum = new decimal(new int[] { 100, 0, 0, 0 });
            numStatsValue.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
            numStatsValue.Name = "numStatsValue";
            numStatsValue.Size = new Size(72, 26);
            numStatsValue.TabIndex = 2;
            numStatsValue.Value = new decimal(new int[] { 22, 0, 0, 0 });
            // 
            // lblStatsSet
            // 
            lblStatsSet.AutoSize = false;
            lblStatsSet.Location = new Point(1026, 32);
            lblStatsSet.Name = "lblStatsSet";
            lblStatsSet.Size = new Size(90, 28);
            lblStatsSet.TabIndex = 1;
            lblStatsSet.Text = "Set all to:";
            lblStatsSet.TextAlign = ContentAlignment.MiddleRight;
            // 
            // btnSelectAllStats
            // 
            btnSelectAllStats.Location = new Point(688, 30);
            btnSelectAllStats.Name = "btnSelectAllStats";
            btnSelectAllStats.Size = new Size(140, 30);
            btnSelectAllStats.TabIndex = 0;
            btnSelectAllStats.Text = "Select all";
            btnSelectAllStats.UseVisualStyleBackColor = true;
            btnSelectAllStats.Click += btnSelectAllStats_Click;
            // 
            // panelAttributes
            // 
            panelAttributes.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            panelAttributes.AutoScroll = true;
            panelAttributes.BackColor = Color.Transparent;
            panelAttributes.Border = new Padding(0);
            panelAttributes.BorderColor = Color.Transparent;
            panelAttributes.Location = new Point(10, 98);
            panelAttributes.Name = "panelAttributes";
            panelAttributes.Radius = 0;
            panelAttributes.ShadowDepth = 0F;
            panelAttributes.Size = new Size(1132, 510);
            panelAttributes.TabIndex = 1;
            // 
            // tabManufacture
            // 
            tabManufacture.BackColor = Color.Transparent;
            tabManufacture.Location = new Point(4, 38);
            tabManufacture.Name = "tabManufacture";
            tabManufacture.Padding = new Padding(4);
            tabManufacture.Size = new Size(1152, 618);
            tabManufacture.TabIndex = 2;
            tabManufacture.Text = "Manufacture";
            // 
            // tabBreak
            // 
            tabBreak.BackColor = Color.Transparent;
            tabBreak.Location = new Point(4, 38);
            tabBreak.Name = "tabBreak";
            tabBreak.Padding = new Padding(4);
            tabBreak.Size = new Size(1152, 618);
            tabBreak.TabIndex = 3;
            tabBreak.Text = "Break";
            // 
            // Main
            // 
            AutoScaleMode = AutoScaleMode.None;
            BackColor = Color.Transparent;
            Controls.Add(tabMain);
            DoubleBuffered = true;
            Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point);
            Margin = new Padding(4, 3, 4, 3);
            Name = "Main";
            Size = new Size(1184, 684);
            tabMain.ResumeLayout(false);
            tabAlchemy.ResumeLayout(false);
            groupSettings.ResumeLayout(false);
            tabEngine.ResumeLayout(false);
            tabEnhance.ResumeLayout(false);
            tabBlues.ResumeLayout(false);
            groupItem.ResumeLayout(false);
            tabItemInfo.ResumeLayout(false);
            tabItemBlues.ResumeLayout(false);
            tabItemStats.ResumeLayout(false);
            groupLog.ResumeLayout(false);
            tabStats.ResumeLayout(false);
            groupStatsTools.ResumeLayout(false);
            tabManufacture.ResumeLayout(false);
            tabBreak.ResumeLayout(false);
            ResumeLayout(false);
        }

        #endregion

        private TabControl tabMain;
        private TabPage tabAlchemy;
        private TabPage tabStats;
        private TabPage tabManufacture;
        private TabPage tabBreak;

        private GroupBox groupItem;
        private Label lblItemSelection;
        private ComboBox comboItem;
        private Label linkRefreshItemList;
        private Label lblDegreeText;
        private Label lblDegree;
        private Label lblOptLevelText;
        private Label lblOptLevel;
        private TabControl tabItemInfo;
        private TabPage tabItemBlues;
        private TabPage tabItemStats;
        private ListView lvItemBlues;
        private ColumnHeader colBlueName;
        private ColumnHeader colBlueValue;
        private ColumnHeader colBlueMax;
        private ListView lvItemStats;
        private ColumnHeader colStatName;
        private ColumnHeader colStatValue;

        private GroupBox groupSettings;
        private TabControl tabEngine;
        private TabPage tabEnhance;
        private TabPage tabBlues;
        private Panel panelEnhance;
        private Panel panelBlues;
        private Button btnSelectAllBlues;

        private GroupBox groupLog;
        private ListView lvLog;
        private ColumnHeader colItem;
        private ColumnHeader colResult;
        private Button btnClearLog;

        private GroupBox groupStatsTools;
        private Button btnSelectAllStats;
        private Label lblStatsSet;
        private NumUpDown numStatsValue;
        private Button btnApplyStats;
        private Label lblStatsItem;
        private Panel panelAttributes;
    }
}
