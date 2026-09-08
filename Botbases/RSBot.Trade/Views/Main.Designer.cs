namespace RSBot.Trade.Views
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
            contextMenuRouteList = new SDUI.Controls.ContextMenuStrip();
            menuActivateRoute = new System.Windows.Forms.ToolStripMenuItem();
            menuSetScript = new System.Windows.Forms.ToolStripMenuItem();
            menuRemoveRoute = new System.Windows.Forms.ToolStripMenuItem();
            menuClearRoutes = new System.Windows.Forms.ToolStripMenuItem();
            tabControl1 = new SDUI.Controls.TabControl();
            tabPageRoute = new System.Windows.Forms.TabPage();
            lblStart = new SDUI.Controls.Label();
            comboStartCity = new SDUI.Controls.ComboBox();
            lblEnd = new SDUI.Controls.Label();
            comboEndCity = new SDUI.Controls.ComboBox();
            lblTransport = new SDUI.Controls.Label();
            comboTransport = new SDUI.Controls.ComboBox();
            btnAddRoute = new SDUI.Controls.Button();
            btnStartTrade = new SDUI.Controls.Button();
            btnStopTrade = new SDUI.Controls.Button();
            btnMoveUp = new SDUI.Controls.Button();
            btnMoveDown = new SDUI.Controls.Button();
            checkSellGoods = new SDUI.Controls.CheckBox();
            checkBuyGoods = new SDUI.Controls.CheckBox();
            numAmountGoods = new SDUI.Controls.NumUpDown();
            lblGoods = new SDUI.Controls.Label();
            lblNumGoodsDesc = new SDUI.Controls.Label();
            lvRouteList = new SDUI.Controls.ListView();
            colIndex = new System.Windows.Forms.ColumnHeader();
            colStart = new System.Windows.Forms.ColumnHeader();
            colEnd = new System.Windows.Forms.ColumnHeader();
            colScript = new System.Windows.Forms.ColumnHeader();
            colActive = new System.Windows.Forms.ColumnHeader();
            colLoop = new System.Windows.Forms.ColumnHeader();
            checkDisableAutoActivation = new SDUI.Controls.CheckBox();
            linkTradeGuide = new System.Windows.Forms.LinkLabel();
            txtTradeLog = new System.Windows.Forms.RichTextBox();
            tabPageSettings = new System.Windows.Forms.TabPage();
            groupBoxLoop = new SDUI.Controls.GroupBox();
            checkRepeatLoop = new SDUI.Controls.CheckBox();
            lblRepeatTimes = new SDUI.Controls.Label();
            numRepeatTimes = new SDUI.Controls.NumUpDown();
            checkReturnScroll = new SDUI.Controls.CheckBox();
            checkUnequipJobSuit = new SDUI.Controls.CheckBox();
            checkSkipTownScripts = new SDUI.Controls.CheckBox();
            groupBoxTransport = new SDUI.Controls.GroupBox();
            checkMountTransport = new SDUI.Controls.CheckBox();
            checkProtectTransport = new SDUI.Controls.CheckBox();
            label1 = new SDUI.Controls.Label();
            numMaxDistance = new SDUI.Controls.NumUpDown();
            label3 = new SDUI.Controls.Label();
            groupBoxDefense = new SDUI.Controls.GroupBox();
            checkAttackThiefNpc = new SDUI.Controls.CheckBox();
            checkAttackThiefPlayers = new SDUI.Controls.CheckBox();
            checkCounterAttack = new SDUI.Controls.CheckBox();
            checkCastBuffs = new SDUI.Controls.CheckBox();
            checkWaitForHunter = new SDUI.Controls.CheckBox();
            tabPage1 = new System.Windows.Forms.TabPage();
            lblTradeScale = new SDUI.Controls.Label();
            label2 = new SDUI.Controls.Label();
            separator3 = new SDUI.Controls.Separator();
            label9 = new SDUI.Controls.Label();
            label8 = new SDUI.Controls.Label();
            label7 = new SDUI.Controls.Label();
            lblJobExp = new SDUI.Controls.Label();
            lblJobLevel = new SDUI.Controls.Label();
            lblJobAlias = new SDUI.Controls.Label();

            contextMenuRouteList.SuspendLayout();
            tabControl1.SuspendLayout();
            tabPageRoute.SuspendLayout();
            tabPageSettings.SuspendLayout();
            groupBoxLoop.SuspendLayout();
            groupBoxTransport.SuspendLayout();
            groupBoxDefense.SuspendLayout();
            tabPage1.SuspendLayout();
            SuspendLayout();

            // contextMenuRouteList
            contextMenuRouteList.ImageScalingSize = new System.Drawing.Size(20, 20);
            contextMenuRouteList.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
                menuActivateRoute,
                menuSetScript,
                menuRemoveRoute,
                menuClearRoutes
            });
            contextMenuRouteList.Name = "contextMenuRouteList";
            contextMenuRouteList.Size = new System.Drawing.Size(160, 96);

            // menuActivateRoute
            menuActivateRoute.Name = "menuActivateRoute";
            menuActivateRoute.Size = new System.Drawing.Size(159, 22);
            menuActivateRoute.Text = "Activate Route";
            menuActivateRoute.Click += menuActivateRoute_Click;

            // menuSetScript
            menuSetScript.Name = "menuSetScript";
            menuSetScript.Size = new System.Drawing.Size(159, 22);
            menuSetScript.Text = "Choose Script...";
            menuSetScript.Click += menuSetScript_Click;

            // menuRemoveRoute
            menuRemoveRoute.Name = "menuRemoveRoute";
            menuRemoveRoute.Size = new System.Drawing.Size(159, 22);
            menuRemoveRoute.Text = "Remove";
            menuRemoveRoute.Click += menuRemoveRoute_Click;

            // menuClearRoutes
            menuClearRoutes.Name = "menuClearRoutes";
            menuClearRoutes.Size = new System.Drawing.Size(159, 22);
            menuClearRoutes.Text = "Clear All";
            menuClearRoutes.Click += menuClearRoutes_Click;

            // tabControl1
            tabControl1.Controls.Add(tabPageRoute);
            tabControl1.Controls.Add(tabPageSettings);
            tabControl1.Controls.Add(tabPage1);
            tabControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            tabControl1.Font = new System.Drawing.Font("Segoe UI", 9.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            tabControl1.ItemSize = new System.Drawing.Size(180, 32);
            tabControl1.Location = new System.Drawing.Point(0, 0);
            tabControl1.Name = "tabControl1";
            tabControl1.Radius = new System.Windows.Forms.Padding(4);
            tabControl1.SelectedIndex = 0;
            tabControl1.Size = new System.Drawing.Size(790, 442);
            tabControl1.SizeMode = System.Windows.Forms.TabSizeMode.Fixed;
            tabControl1.TabIndex = 0;

            // tabPageRoute
            tabPageRoute.Controls.Add(lblStart);
            tabPageRoute.Controls.Add(comboStartCity);
            tabPageRoute.Controls.Add(lblEnd);
            tabPageRoute.Controls.Add(comboEndCity);
            tabPageRoute.Controls.Add(lblTransport);
            tabPageRoute.Controls.Add(comboTransport);
            tabPageRoute.Controls.Add(btnAddRoute);
            tabPageRoute.Controls.Add(btnStartTrade);
            tabPageRoute.Controls.Add(btnStopTrade);
            tabPageRoute.Controls.Add(btnMoveUp);
            tabPageRoute.Controls.Add(btnMoveDown);
            tabPageRoute.Controls.Add(checkSellGoods);
            tabPageRoute.Controls.Add(checkBuyGoods);
            tabPageRoute.Controls.Add(numAmountGoods);
            tabPageRoute.Controls.Add(lblGoods);
            tabPageRoute.Controls.Add(lblNumGoodsDesc);
            tabPageRoute.Controls.Add(lvRouteList);
            tabPageRoute.Controls.Add(checkDisableAutoActivation);
            tabPageRoute.Controls.Add(linkTradeGuide);
            tabPageRoute.Controls.Add(txtTradeLog);
            tabPageRoute.Location = new System.Drawing.Point(4, 36);
            tabPageRoute.Name = "tabPageRoute";
            tabPageRoute.Padding = new System.Windows.Forms.Padding(6);
            tabPageRoute.Size = new System.Drawing.Size(782, 402);
            tabPageRoute.TabIndex = 0;
            tabPageRoute.Text = "Route";

            // lblStart
            lblStart.AutoSize = true;
            lblStart.Location = new System.Drawing.Point(8, 12);
            lblStart.Name = "lblStart";
            lblStart.Size = new System.Drawing.Size(38, 17);
            lblStart.Text = "Start:";

            // comboStartCity
            comboStartCity.Location = new System.Drawing.Point(52, 8);
            comboStartCity.Name = "comboStartCity";
            comboStartCity.Size = new System.Drawing.Size(120, 26);
            comboStartCity.TabIndex = 1;

            // lblEnd
            lblEnd.AutoSize = true;
            lblEnd.Location = new System.Drawing.Point(180, 12);
            lblEnd.Name = "lblEnd";
            lblEnd.Size = new System.Drawing.Size(33, 17);
            lblEnd.Text = "End:";

            // comboEndCity
            comboEndCity.Location = new System.Drawing.Point(218, 8);
            comboEndCity.Name = "comboEndCity";
            comboEndCity.Size = new System.Drawing.Size(120, 26);
            comboEndCity.TabIndex = 3;

            // lblTransport
            lblTransport.AutoSize = true;
            lblTransport.Location = new System.Drawing.Point(8, 44);
            lblTransport.Name = "lblTransport";
            lblTransport.Size = new System.Drawing.Size(66, 17);
            lblTransport.Text = "Transport:";

            // comboTransport
            comboTransport.Location = new System.Drawing.Point(78, 40);
            comboTransport.Name = "comboTransport";
            comboTransport.Size = new System.Drawing.Size(130, 26);
            comboTransport.TabIndex = 5;
            comboTransport.SelectedIndexChanged += comboTransport_SelectedIndexChanged;

            // btnAddRoute
            btnAddRoute.Color = System.Drawing.Color.FromArgb(59, 130, 246);
            btnAddRoute.ForeColor = System.Drawing.Color.White;
            btnAddRoute.Location = new System.Drawing.Point(218, 40);
            btnAddRoute.Name = "btnAddRoute";
            btnAddRoute.Size = new System.Drawing.Size(86, 26);
            btnAddRoute.TabIndex = 6;
            btnAddRoute.Text = "Add Route";
            btnAddRoute.Click += btnAddRoute_Click;

            // btnStartTrade
            btnStartTrade.Color = System.Drawing.Color.FromArgb(16, 185, 129);
            btnStartTrade.ForeColor = System.Drawing.Color.White;
            btnStartTrade.Location = new System.Drawing.Point(312, 40);
            btnStartTrade.Name = "btnStartTrade";
            btnStartTrade.Size = new System.Drawing.Size(76, 26);
            btnStartTrade.TabIndex = 7;
            btnStartTrade.Text = "▶ Start";
            btnStartTrade.Click += btnStartTrade_Click;

            // btnStopTrade
            btnStopTrade.Color = System.Drawing.Color.FromArgb(239, 68, 68);
            btnStopTrade.ForeColor = System.Drawing.Color.White;
            btnStopTrade.Location = new System.Drawing.Point(394, 40);
            btnStopTrade.Name = "btnStopTrade";
            btnStopTrade.Size = new System.Drawing.Size(76, 26);
            btnStopTrade.TabIndex = 8;
            btnStopTrade.Text = "■ Stop";
            btnStopTrade.Click += btnStopTrade_Click;

            // btnMoveUp
            btnMoveUp.Location = new System.Drawing.Point(476, 40);
            btnMoveUp.Name = "btnMoveUp";
            btnMoveUp.Size = new System.Drawing.Size(32, 26);
            btnMoveUp.TabIndex = 9;
            btnMoveUp.Text = "▲";
            btnMoveUp.Click += btnMoveUp_Click;

            // btnMoveDown
            btnMoveDown.Location = new System.Drawing.Point(512, 40);
            btnMoveDown.Name = "btnMoveDown";
            btnMoveDown.Size = new System.Drawing.Size(32, 26);
            btnMoveDown.TabIndex = 10;
            btnMoveDown.Text = "▼";
            btnMoveDown.Click += btnMoveDown_Click;

            // checkSellGoods
            checkSellGoods.AutoSize = true;
            checkSellGoods.Location = new System.Drawing.Point(490, 8);
            checkSellGoods.Name = "checkSellGoods";
            checkSellGoods.Size = new System.Drawing.Size(87, 21);
            checkSellGoods.TabIndex = 11;
            checkSellGoods.Text = "Sell goods";
            checkSellGoods.CheckedChanged += checkSellGoods_CheckedChanged;

            // checkBuyGoods
            checkBuyGoods.AutoSize = true;
            checkBuyGoods.Location = new System.Drawing.Point(588, 8);
            checkBuyGoods.Name = "checkBuyGoods";
            checkBuyGoods.Size = new System.Drawing.Size(48, 21);
            checkBuyGoods.TabIndex = 12;
            checkBuyGoods.Text = "Buy";
            checkBuyGoods.CheckedChanged += checkBuyGoods_CheckedChanged;

            // numAmountGoods
            numAmountGoods.Location = new System.Drawing.Point(640, 6);
            numAmountGoods.Maximum = 999999;
            numAmountGoods.Name = "numAmountGoods";
            numAmountGoods.Size = new System.Drawing.Size(65, 24);
            numAmountGoods.TabIndex = 13;
            numAmountGoods.Value = 5000;
            numAmountGoods.ValueChanged += numAmountGoods_ValueChanged;

            // lblGoods
            lblGoods.AutoSize = true;
            lblGoods.Location = new System.Drawing.Point(710, 10);
            lblGoods.Name = "lblGoods";
            lblGoods.Size = new System.Drawing.Size(100, 17);
            lblGoods.Text = "x Special Goods";

            // lblNumGoodsDesc
            lblNumGoodsDesc.AutoSize = true;
            lblNumGoodsDesc.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Italic);
            lblNumGoodsDesc.ForeColor = System.Drawing.Color.Gray;
            lblNumGoodsDesc.Location = new System.Drawing.Point(640, 34);
            lblNumGoodsDesc.Name = "lblNumGoodsDesc";
            lblNumGoodsDesc.Size = new System.Drawing.Size(103, 13);
            lblNumGoodsDesc.Text = "0 = max. possible";

            // lvRouteList
            lvRouteList.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
                colIndex,
                colStart,
                colEnd,
                colScript,
                colActive,
                colLoop
            });
            lvRouteList.ContextMenuStrip = contextMenuRouteList;
            lvRouteList.FullRowSelect = true;
            lvRouteList.GridLines = true;
            lvRouteList.Location = new System.Drawing.Point(8, 72);
            lvRouteList.MultiSelect = false;
            lvRouteList.Name = "lvRouteList";
            lvRouteList.Size = new System.Drawing.Size(766, 175);
            lvRouteList.TabIndex = 14;
            lvRouteList.UseCompatibleStateImageBehavior = false;
            lvRouteList.View = System.Windows.Forms.View.Details;

            // colIndex
            colIndex.Text = "#";
            colIndex.Width = 35;

            // colStart
            colStart.Text = "Start";
            colStart.Width = 105;

            // colEnd
            colEnd.Text = "End";
            colEnd.Width = 105;

            // colScript
            colScript.Text = "Script";
            colScript.Width = 270;

            // colActive
            colActive.Text = "Active";
            colActive.Width = 75;

            // colLoop
            colLoop.Text = "Loop";
            colLoop.Width = 65;

            // checkDisableAutoActivation
            checkDisableAutoActivation.AutoSize = true;
            checkDisableAutoActivation.Location = new System.Drawing.Point(8, 252);
            checkDisableAutoActivation.Name = "checkDisableAutoActivation";
            checkDisableAutoActivation.Size = new System.Drawing.Size(161, 21);
            checkDisableAutoActivation.TabIndex = 15;
            checkDisableAutoActivation.Text = "Disable auto-activation";
            checkDisableAutoActivation.CheckedChanged += checkDisableAutoActivation_CheckedChanged;

            // linkTradeGuide
            linkTradeGuide.AutoSize = true;
            linkTradeGuide.Location = new System.Drawing.Point(695, 253);
            linkTradeGuide.Name = "linkTradeGuide";
            linkTradeGuide.Size = new System.Drawing.Size(78, 17);
            linkTradeGuide.TabIndex = 16;
            linkTradeGuide.TabStop = true;
            linkTradeGuide.Text = "Trade Guide";
            linkTradeGuide.LinkClicked += linkTradeGuide_LinkClicked;

            // txtTradeLog
            txtTradeLog.BackColor = System.Drawing.Color.FromArgb(15, 23, 42);
            txtTradeLog.BorderStyle = System.Windows.Forms.BorderStyle.None;
            txtTradeLog.Font = new System.Drawing.Font("Consolas", 8.5F);
            txtTradeLog.ForeColor = System.Drawing.Color.FromArgb(226, 232, 240);
            txtTradeLog.Location = new System.Drawing.Point(8, 278);
            txtTradeLog.Name = "txtTradeLog";
            txtTradeLog.ReadOnly = true;
            txtTradeLog.ScrollBars = System.Windows.Forms.RichTextBoxScrollBars.Vertical;
            txtTradeLog.Size = new System.Drawing.Size(766, 118);
            txtTradeLog.TabIndex = 17;
            txtTradeLog.Text = "";

            // tabPageSettings
            tabPageSettings.Controls.Add(groupBoxLoop);
            tabPageSettings.Controls.Add(groupBoxTransport);
            tabPageSettings.Controls.Add(groupBoxDefense);
            tabPageSettings.Location = new System.Drawing.Point(4, 36);
            tabPageSettings.Name = "tabPageSettings";
            tabPageSettings.Padding = new System.Windows.Forms.Padding(6);
            tabPageSettings.Size = new System.Drawing.Size(782, 402);
            tabPageSettings.TabIndex = 1;
            tabPageSettings.Text = "Settings";

            // groupBoxLoop
            groupBoxLoop.Controls.Add(checkRepeatLoop);
            groupBoxLoop.Controls.Add(lblRepeatTimes);
            groupBoxLoop.Controls.Add(numRepeatTimes);
            groupBoxLoop.Controls.Add(checkReturnScroll);
            groupBoxLoop.Controls.Add(checkUnequipJobSuit);
            groupBoxLoop.Controls.Add(checkSkipTownScripts);
            groupBoxLoop.Location = new System.Drawing.Point(8, 8);
            groupBoxLoop.Name = "groupBoxLoop";
            groupBoxLoop.Size = new System.Drawing.Size(766, 120);
            groupBoxLoop.TabIndex = 0;
            groupBoxLoop.Text = "Loop & Automation Options";

            // checkRepeatLoop
            checkRepeatLoop.AutoSize = true;
            checkRepeatLoop.Location = new System.Drawing.Point(12, 25);
            checkRepeatLoop.Name = "checkRepeatLoop";
            checkRepeatLoop.Size = new System.Drawing.Size(139, 21);
            checkRepeatLoop.TabIndex = 0;
            checkRepeatLoop.Text = "Repeat trade loop";
            checkRepeatLoop.CheckedChanged += checkRepeatLoop_CheckedChanged;

            // lblRepeatTimes
            lblRepeatTimes.AutoSize = true;
            lblRepeatTimes.Location = new System.Drawing.Point(180, 27);
            lblRepeatTimes.Name = "lblRepeatTimes";
            lblRepeatTimes.Size = new System.Drawing.Size(147, 17);
            lblRepeatTimes.Text = "Times (0 = infinite):";

            // numRepeatTimes
            numRepeatTimes.Location = new System.Drawing.Point(335, 23);
            numRepeatTimes.Maximum = 999;
            numRepeatTimes.Name = "numRepeatTimes";
            numRepeatTimes.Size = new System.Drawing.Size(65, 24);
            numRepeatTimes.TabIndex = 1;
            numRepeatTimes.ValueChanged += numRepeatTimes_ValueChanged;

            // checkReturnScroll
            checkReturnScroll.AutoSize = true;
            checkReturnScroll.Location = new System.Drawing.Point(12, 54);
            checkReturnScroll.Name = "checkReturnScroll";
            checkReturnScroll.Size = new System.Drawing.Size(280, 21);
            checkReturnScroll.TabIndex = 2;
            checkReturnScroll.Text = "Use Return Scroll after completion of loop";
            checkReturnScroll.CheckedChanged += checkReturnScroll_CheckedChanged;

            // checkUnequipJobSuit
            checkUnequipJobSuit.AutoSize = true;
            checkUnequipJobSuit.Location = new System.Drawing.Point(335, 54);
            checkUnequipJobSuit.Name = "checkUnequipJobSuit";
            checkUnequipJobSuit.Size = new System.Drawing.Size(320, 21);
            checkUnequipJobSuit.TabIndex = 3;
            checkUnequipJobSuit.Text = "Dismount and unequip Job Suit when finished";
            checkUnequipJobSuit.CheckedChanged += checkUnequipJobSuit_CheckedChanged;

            // checkSkipTownScripts
            checkSkipTownScripts.AutoSize = true;
            checkSkipTownScripts.Location = new System.Drawing.Point(12, 83);
            checkSkipTownScripts.Name = "checkSkipTownScripts";
            checkSkipTownScripts.Size = new System.Drawing.Size(220, 21);
            checkSkipTownScripts.TabIndex = 4;
            checkSkipTownScripts.Text = "Skip town scripts completely";
            checkSkipTownScripts.CheckedChanged += checkSkipTownScripts_CheckedChanged;

            // groupBoxTransport
            groupBoxTransport.Controls.Add(checkMountTransport);
            groupBoxTransport.Controls.Add(checkProtectTransport);
            groupBoxTransport.Controls.Add(label1);
            groupBoxTransport.Controls.Add(numMaxDistance);
            groupBoxTransport.Controls.Add(label3);
            groupBoxTransport.Location = new System.Drawing.Point(8, 134);
            groupBoxTransport.Name = "groupBoxTransport";
            groupBoxTransport.Size = new System.Drawing.Size(375, 140);
            groupBoxTransport.TabIndex = 1;
            groupBoxTransport.Text = "Transport Vehicle";

            // checkMountTransport
            checkMountTransport.AutoSize = true;
            checkMountTransport.Location = new System.Drawing.Point(12, 28);
            checkMountTransport.Name = "checkMountTransport";
            checkMountTransport.Size = new System.Drawing.Size(167, 21);
            checkMountTransport.TabIndex = 0;
            checkMountTransport.Text = "Mount transport vehicle";
            checkMountTransport.CheckedChanged += checkMountTransport_CheckedChanged;

            // checkProtectTransport
            checkProtectTransport.AutoSize = true;
            checkProtectTransport.Location = new System.Drawing.Point(12, 58);
            checkProtectTransport.Name = "checkProtectTransport";
            checkProtectTransport.Size = new System.Drawing.Size(170, 21);
            checkProtectTransport.TabIndex = 1;
            checkProtectTransport.Text = "Protect transport vehicle";
            checkProtectTransport.CheckedChanged += checkProtectTransport_CheckedChanged;

            // label1
            label1.AutoSize = true;
            label1.Location = new System.Drawing.Point(12, 94);
            label1.Name = "label1";
            label1.Size = new System.Drawing.Size(91, 17);
            label1.Text = "Max. distance:";

            // numMaxDistance
            numMaxDistance.Location = new System.Drawing.Point(110, 90);
            numMaxDistance.Maximum = 100;
            numMaxDistance.Name = "numMaxDistance";
            numMaxDistance.Size = new System.Drawing.Size(65, 24);
            numMaxDistance.TabIndex = 2;
            numMaxDistance.Value = 15;
            numMaxDistance.ValueChanged += numMaxDistance_ValueChanged;

            // label3
            label3.AutoSize = true;
            label3.Location = new System.Drawing.Point(180, 94);
            label3.Name = "label3";
            label3.Size = new System.Drawing.Size(20, 17);
            label3.Text = "m";

            // groupBoxDefense
            groupBoxDefense.Controls.Add(checkAttackThiefNpc);
            groupBoxDefense.Controls.Add(checkAttackThiefPlayers);
            groupBoxDefense.Controls.Add(checkCounterAttack);
            groupBoxDefense.Controls.Add(checkCastBuffs);
            groupBoxDefense.Controls.Add(checkWaitForHunter);
            groupBoxDefense.Location = new System.Drawing.Point(395, 134);
            groupBoxDefense.Name = "groupBoxDefense";
            groupBoxDefense.Size = new System.Drawing.Size(379, 140);
            groupBoxDefense.TabIndex = 2;
            groupBoxDefense.Text = "Combat & Defense";

            // checkAttackThiefNpc
            checkAttackThiefNpc.AutoSize = true;
            checkAttackThiefNpc.Location = new System.Drawing.Point(12, 28);
            checkAttackThiefNpc.Name = "checkAttackThiefNpc";
            checkAttackThiefNpc.Size = new System.Drawing.Size(127, 21);
            checkAttackThiefNpc.TabIndex = 0;
            checkAttackThiefNpc.Text = "Attack Thief NPCs";
            checkAttackThiefNpc.CheckedChanged += checkAttackThiefNpc_CheckedChanged;

            // checkAttackThiefPlayers
            checkAttackThiefPlayers.AutoSize = true;
            checkAttackThiefPlayers.Location = new System.Drawing.Point(170, 28);
            checkAttackThiefPlayers.Name = "checkAttackThiefPlayers";
            checkAttackThiefPlayers.Size = new System.Drawing.Size(142, 21);
            checkAttackThiefPlayers.TabIndex = 1;
            checkAttackThiefPlayers.Text = "Attack Thief Players";
            checkAttackThiefPlayers.CheckedChanged += checkAttackThiefPlayers_CheckedChanged;

            // checkCounterAttack
            checkCounterAttack.AutoSize = true;
            checkCounterAttack.Location = new System.Drawing.Point(12, 58);
            checkCounterAttack.Name = "checkCounterAttack";
            checkCounterAttack.Size = new System.Drawing.Size(111, 21);
            checkCounterAttack.TabIndex = 2;
            checkCounterAttack.Text = "Counter attack";
            checkCounterAttack.CheckedChanged += checkCounterAttack_CheckedChanged;

            // checkCastBuffs
            checkCastBuffs.AutoSize = true;
            checkCastBuffs.Location = new System.Drawing.Point(170, 58);
            checkCastBuffs.Name = "checkCastBuffs";
            checkCastBuffs.Size = new System.Drawing.Size(86, 21);
            checkCastBuffs.TabIndex = 3;
            checkCastBuffs.Text = "Cast Buffs";
            checkCastBuffs.CheckedChanged += checkCastBuffs_CheckedChanged;

            // checkWaitForHunter
            checkWaitForHunter.AutoSize = true;
            checkWaitForHunter.Location = new System.Drawing.Point(12, 88);
            checkWaitForHunter.Name = "checkWaitForHunter";
            checkWaitForHunter.Size = new System.Drawing.Size(161, 21);
            checkWaitForHunter.TabIndex = 4;
            checkWaitForHunter.Text = "Wait for hunter nearby";
            checkWaitForHunter.CheckedChanged += checkWaitForHunter_CheckedChanged;

            // tabPage1
            tabPage1.Controls.Add(lblTradeScale);
            tabPage1.Controls.Add(label2);
            tabPage1.Controls.Add(separator3);
            tabPage1.Controls.Add(label9);
            tabPage1.Controls.Add(label8);
            tabPage1.Controls.Add(label7);
            tabPage1.Controls.Add(lblJobExp);
            tabPage1.Controls.Add(lblJobLevel);
            tabPage1.Controls.Add(lblJobAlias);
            tabPage1.Location = new System.Drawing.Point(4, 36);
            tabPage1.Name = "tabPage1";
            tabPage1.Padding = new System.Windows.Forms.Padding(6);
            tabPage1.Size = new System.Drawing.Size(782, 402);
            tabPage1.TabIndex = 2;
            tabPage1.Text = "Overview";

            // lblTradeScale
            lblTradeScale.AutoSize = true;
            lblTradeScale.Location = new System.Drawing.Point(135, 25);
            lblTradeScale.Name = "lblTradeScale";
            lblTradeScale.Size = new System.Drawing.Size(43, 17);
            lblTradeScale.TabIndex = 2;
            lblTradeScale.Text = "■■■■■";

            // label2
            label2.AutoSize = true;
            label2.Location = new System.Drawing.Point(25, 25);
            label2.Name = "label2";
            label2.Size = new System.Drawing.Size(95, 17);
            label2.TabIndex = 1;
            label2.Text = "Trade difficulty:";

            // separator3
            separator3.Location = new System.Drawing.Point(25, 52);
            separator3.Name = "separator3";
            separator3.Size = new System.Drawing.Size(730, 2);
            separator3.TabIndex = 3;

            // label9
            label9.AutoSize = true;
            label9.Location = new System.Drawing.Point(25, 68);
            label9.Name = "label9";
            label9.Size = new System.Drawing.Size(32, 17);
            label9.TabIndex = 4;
            label9.Text = "EXP:";

            // lblJobExp
            lblJobExp.AutoSize = true;
            lblJobExp.Location = new System.Drawing.Point(135, 68);
            lblJobExp.Name = "lblJobExp";
            lblJobExp.Size = new System.Drawing.Size(15, 17);
            lblJobExp.TabIndex = 7;
            lblJobExp.Text = "0";

            // label8
            label8.AutoSize = true;
            label8.Location = new System.Drawing.Point(25, 96);
            label8.Name = "label8";
            label8.Size = new System.Drawing.Size(95, 17);
            label8.TabIndex = 5;
            label8.Text = "Job Alias / Nick:";

            // lblJobAlias
            lblJobAlias.AutoSize = true;
            lblJobAlias.Location = new System.Drawing.Point(135, 96);
            lblJobAlias.Name = "lblJobAlias";
            lblJobAlias.Size = new System.Drawing.Size(56, 17);
            lblJobAlias.TabIndex = 9;
            lblJobAlias.Text = "<none>";

            // label7
            label7.AutoSize = true;
            label7.Location = new System.Drawing.Point(25, 124);
            label7.Name = "label7";
            label7.Size = new System.Drawing.Size(64, 17);
            label7.TabIndex = 6;
            label7.Text = "Job Level:";

            // lblJobLevel
            lblJobLevel.AutoSize = true;
            lblJobLevel.Location = new System.Drawing.Point(135, 124);
            lblJobLevel.Name = "lblJobLevel";
            lblJobLevel.Size = new System.Drawing.Size(15, 17);
            lblJobLevel.TabIndex = 8;
            lblJobLevel.Text = "0";

            // Main
            AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            Controls.Add(tabControl1);
            Name = "Main";
            Size = new System.Drawing.Size(790, 442);
            Load += Main_Load;

            contextMenuRouteList.ResumeLayout(false);
            tabControl1.ResumeLayout(false);
            tabPageRoute.ResumeLayout(false);
            tabPageRoute.PerformLayout();
            tabPageSettings.ResumeLayout(false);
            groupBoxLoop.ResumeLayout(false);
            groupBoxLoop.PerformLayout();
            groupBoxTransport.ResumeLayout(false);
            groupBoxTransport.PerformLayout();
            groupBoxDefense.ResumeLayout(false);
            groupBoxDefense.PerformLayout();
            tabPage1.ResumeLayout(false);
            tabPage1.PerformLayout();
            ResumeLayout(false);
        }

        #endregion

        private SDUI.Controls.ContextMenuStrip contextMenuRouteList;
        private System.Windows.Forms.ToolStripMenuItem menuActivateRoute;
        private System.Windows.Forms.ToolStripMenuItem menuSetScript;
        private System.Windows.Forms.ToolStripMenuItem menuRemoveRoute;
        private System.Windows.Forms.ToolStripMenuItem menuClearRoutes;
        private SDUI.Controls.TabControl tabControl1;
        private System.Windows.Forms.TabPage tabPageRoute;
        private SDUI.Controls.Label lblStart;
        private SDUI.Controls.ComboBox comboStartCity;
        private SDUI.Controls.Label lblEnd;
        private SDUI.Controls.ComboBox comboEndCity;
        private SDUI.Controls.Label lblTransport;
        private SDUI.Controls.ComboBox comboTransport;
        private SDUI.Controls.Button btnAddRoute;
        private SDUI.Controls.Button btnStartTrade;
        private SDUI.Controls.Button btnStopTrade;
        private SDUI.Controls.Button btnMoveUp;
        private SDUI.Controls.Button btnMoveDown;
        private SDUI.Controls.CheckBox checkSellGoods;
        private SDUI.Controls.CheckBox checkBuyGoods;
        private SDUI.Controls.NumUpDown numAmountGoods;
        private SDUI.Controls.Label lblGoods;
        private SDUI.Controls.Label lblNumGoodsDesc;
        private SDUI.Controls.ListView lvRouteList;
        private System.Windows.Forms.ColumnHeader colIndex;
        private System.Windows.Forms.ColumnHeader colStart;
        private System.Windows.Forms.ColumnHeader colEnd;
        private System.Windows.Forms.ColumnHeader colScript;
        private System.Windows.Forms.ColumnHeader colActive;
        private System.Windows.Forms.ColumnHeader colLoop;
        private SDUI.Controls.CheckBox checkDisableAutoActivation;
        private System.Windows.Forms.LinkLabel linkTradeGuide;
        private System.Windows.Forms.RichTextBox txtTradeLog;
        private System.Windows.Forms.TabPage tabPageSettings;
        private SDUI.Controls.GroupBox groupBoxLoop;
        private SDUI.Controls.CheckBox checkRepeatLoop;
        private SDUI.Controls.Label lblRepeatTimes;
        private SDUI.Controls.NumUpDown numRepeatTimes;
        private SDUI.Controls.CheckBox checkReturnScroll;
        private SDUI.Controls.CheckBox checkUnequipJobSuit;
        private SDUI.Controls.CheckBox checkSkipTownScripts;
        private SDUI.Controls.GroupBox groupBoxTransport;
        private SDUI.Controls.CheckBox checkMountTransport;
        private SDUI.Controls.CheckBox checkProtectTransport;
        private SDUI.Controls.Label label1;
        private SDUI.Controls.NumUpDown numMaxDistance;
        private SDUI.Controls.Label label3;
        private SDUI.Controls.GroupBox groupBoxDefense;
        private SDUI.Controls.CheckBox checkAttackThiefNpc;
        private SDUI.Controls.CheckBox checkAttackThiefPlayers;
        private SDUI.Controls.CheckBox checkCounterAttack;
        private SDUI.Controls.CheckBox checkCastBuffs;
        private SDUI.Controls.CheckBox checkWaitForHunter;
        private System.Windows.Forms.TabPage tabPage1;
        private SDUI.Controls.Label lblTradeScale;
        private SDUI.Controls.Label label2;
        private SDUI.Controls.Separator separator3;
        private SDUI.Controls.Label label9;
        private SDUI.Controls.Label label8;
        private SDUI.Controls.Label label7;
        private SDUI.Controls.Label lblJobExp;
        private SDUI.Controls.Label lblJobLevel;
        private SDUI.Controls.Label lblJobAlias;
    }
}
