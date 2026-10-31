namespace RSBot.Inventory.Views
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
            lblFreeSlots = new SDUI.Controls.Label();
            listViewMain = new SDUI.Controls.ListView();
            colName = new System.Windows.Forms.ColumnHeader();
            colAmount = new System.Windows.Forms.ColumnHeader();
            colGenderRace = new System.Windows.Forms.ColumnHeader();
            contextMenuStrip = new SDUI.Controls.ContextMenuStrip();
            useToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            dropToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            moveToPetToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            moveToPlayerToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            moveToLastDeathPositionToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            moveToLastRecallPositionToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            selectMapLocationToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            useItemAtTrainingPlaceMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            autoUseAccordingToPurposeToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            panel1 = new SDUI.Controls.Panel();
            panelHappy = new SDUI.Controls.Panel();
            lblHappyTitle = new SDUI.Controls.Label();
            lblHappyCounts = new SDUI.Controls.Label();
            lblHappyStatus = new SDUI.Controls.Label();
            checkAutoHappy = new SDUI.Controls.CheckBox();
            btnStartHappy = new SDUI.Controls.Button();
            btnStopHappy = new SDUI.Controls.Button();
            timerHappy = new System.Windows.Forms.Timer();
            pbInventoryStatus = new SDUI.Controls.ShapeProgressBar();
            checkAutoSort = new SDUI.Controls.CheckBox();
            btnSort = new SDUI.Controls.Button();
            buttonInventory = new SDUI.Controls.Button();
            buttonEquipment = new SDUI.Controls.Button();
            buttonAvatars = new SDUI.Controls.Button();
            buttonGrabpet = new SDUI.Controls.Button();
            topPanel = new SDUI.Controls.Panel();
            buttonSpecialty = new SDUI.Controls.Button();
            buttonGuildStorage = new SDUI.Controls.Button();
            buttonFellowPet = new SDUI.Controls.Button();
            buttonJobTransport = new SDUI.Controls.Button();
            buttonStorage = new SDUI.Controls.Button();
            buttonJobEquipment = new SDUI.Controls.Button();
            contextMenuStrip.SuspendLayout();
            panel1.SuspendLayout();
            topPanel.SuspendLayout();
            SuspendLayout();
            // 
            // lblFreeSlots
            // 
            lblFreeSlots.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left;
            lblFreeSlots.ApplyGradient = false;
            lblFreeSlots.AutoSize = true;
            lblFreeSlots.BackColor = System.Drawing.Color.Transparent;
            lblFreeSlots.Font = new System.Drawing.Font("Segoe UI", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            lblFreeSlots.ForeColor = System.Drawing.Color.FromArgb(0, 0, 0);
            lblFreeSlots.Gradient = new System.Drawing.Color[] { System.Drawing.Color.Gray, System.Drawing.Color.Black };
            lblFreeSlots.GradientAnimation = false;
            lblFreeSlots.Location = new System.Drawing.Point(47, 13);
            lblFreeSlots.Name = "lblFreeSlots";
            lblFreeSlots.Size = new System.Drawing.Size(17, 20);
            lblFreeSlots.TabIndex = 4;
            lblFreeSlots.Text = "0";
            // 
            // listViewMain
            // 
            listViewMain.BackColor = System.Drawing.Color.FromArgb(21, 27, 36);
            listViewMain.BorderStyle = System.Windows.Forms.BorderStyle.None;
            listViewMain.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] { colName, colAmount, colGenderRace });
            listViewMain.ContextMenuStrip = contextMenuStrip;
            listViewMain.Dock = System.Windows.Forms.DockStyle.Fill;
            listViewMain.ForeColor = System.Drawing.Color.White;
            listViewMain.FullRowSelect = true;
            listViewMain.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
            listViewMain.Location = new System.Drawing.Point(0, 38);
            listViewMain.MultiSelect = false;
            listViewMain.Name = "listViewMain";
            listViewMain.Size = new System.Drawing.Size(792, 375);
            listViewMain.TabIndex = 2;
            listViewMain.UseCompatibleStateImageBehavior = false;
            listViewMain.View = System.Windows.Forms.View.Details;
            listViewMain.MouseDoubleClick += listViewMain_MouseDoubleClick;
            // 
            // colName
            // 
            colName.Text = "Name";
            colName.Width = 381;
            // 
            // colAmount
            // 
            colAmount.Text = "Amount";
            colAmount.Width = 62;
            // 
            // colGenderRace
            // 
            colGenderRace.Text = "Rarity";
            colGenderRace.Width = 153;
            // 
            // contextMenuStrip
            // 
            contextMenuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] { useToolStripMenuItem, dropToolStripMenuItem, moveToPetToolStripMenuItem, moveToPlayerToolStripMenuItem, moveToLastDeathPositionToolStripMenuItem, moveToLastRecallPositionToolStripMenuItem, selectMapLocationToolStripMenuItem, useItemAtTrainingPlaceMenuItem, autoUseAccordingToPurposeToolStripMenuItem });
            contextMenuStrip.Name = "contextMenuStrip";
            contextMenuStrip.Size = new System.Drawing.Size(238, 202);
            contextMenuStrip.Opening += contextMenuStrip_Opening;
            // 
            // useToolStripMenuItem
            // 
            useToolStripMenuItem.ForeColor = System.Drawing.Color.FromArgb(0, 0, 0);
            useToolStripMenuItem.Name = "useToolStripMenuItem";
            useToolStripMenuItem.Size = new System.Drawing.Size(237, 22);
            useToolStripMenuItem.Text = "Use";
            useToolStripMenuItem.Click += buttonUseItem_Click;
            // 
            // dropToolStripMenuItem
            // 
            dropToolStripMenuItem.ForeColor = System.Drawing.Color.FromArgb(0, 0, 0);
            dropToolStripMenuItem.Name = "dropToolStripMenuItem";
            dropToolStripMenuItem.Size = new System.Drawing.Size(237, 22);
            dropToolStripMenuItem.Text = "Drop";
            dropToolStripMenuItem.Click += dropToolStripMenuItem_Click;
            // 
            // moveToPetToolStripMenuItem
            // 
            moveToPetToolStripMenuItem.ForeColor = System.Drawing.Color.FromArgb(0, 0, 0);
            moveToPetToolStripMenuItem.Name = "moveToPetToolStripMenuItem";
            moveToPetToolStripMenuItem.Size = new System.Drawing.Size(237, 22);
            moveToPetToolStripMenuItem.Text = "Move to active pet";
            moveToPetToolStripMenuItem.Click += moveToPetToolStripMenuItem_Click;
            // 
            // moveToPlayerToolStripMenuItem
            // 
            moveToPlayerToolStripMenuItem.ForeColor = System.Drawing.Color.FromArgb(0, 0, 0);
            moveToPlayerToolStripMenuItem.Name = "moveToPlayerToolStripMenuItem";
            moveToPlayerToolStripMenuItem.Size = new System.Drawing.Size(237, 22);
            moveToPlayerToolStripMenuItem.Text = "Move to player";
            moveToPlayerToolStripMenuItem.Click += moveToPlayerToolStripMenuItem_Click;
            // 
            // moveToLastDeathPositionToolStripMenuItem
            // 
            moveToLastDeathPositionToolStripMenuItem.ForeColor = System.Drawing.Color.FromArgb(0, 0, 0);
            moveToLastDeathPositionToolStripMenuItem.Name = "moveToLastDeathPositionToolStripMenuItem";
            moveToLastDeathPositionToolStripMenuItem.Size = new System.Drawing.Size(237, 22);
            moveToLastDeathPositionToolStripMenuItem.Text = "Move to last death position";
            moveToLastDeathPositionToolStripMenuItem.Click += moveToLastDeathPositionToolStripMenuItem_Click;
            // 
            // moveToLastRecallPositionToolStripMenuItem
            // 
            moveToLastRecallPositionToolStripMenuItem.ForeColor = System.Drawing.Color.FromArgb(0, 0, 0);
            moveToLastRecallPositionToolStripMenuItem.Name = "moveToLastRecallPositionToolStripMenuItem";
            moveToLastRecallPositionToolStripMenuItem.Size = new System.Drawing.Size(237, 22);
            moveToLastRecallPositionToolStripMenuItem.Text = "Move to Last recall position";
            moveToLastRecallPositionToolStripMenuItem.Click += moveToLastRecallPositionToolStripMenuItem_Click;
            // 
            // selectMapLocationToolStripMenuItem
            // 
            selectMapLocationToolStripMenuItem.ForeColor = System.Drawing.Color.FromArgb(0, 0, 0);
            selectMapLocationToolStripMenuItem.Name = "selectMapLocationToolStripMenuItem";
            selectMapLocationToolStripMenuItem.Size = new System.Drawing.Size(237, 22);
            selectMapLocationToolStripMenuItem.Text = "Select Map Location";
            // 
            // useItemAtTrainingPlaceMenuItem
            // 
            useItemAtTrainingPlaceMenuItem.ForeColor = System.Drawing.Color.FromArgb(0, 0, 0);
            useItemAtTrainingPlaceMenuItem.Name = "useItemAtTrainingPlaceMenuItem";
            useItemAtTrainingPlaceMenuItem.Size = new System.Drawing.Size(237, 22);
            useItemAtTrainingPlaceMenuItem.Text = "Use item at training place";
            useItemAtTrainingPlaceMenuItem.Click += useItemAtTrainingPlaceMenuItem_Click;
            // 
            // autoUseAccordingToPurposeToolStripMenuItem
            // 
            autoUseAccordingToPurposeToolStripMenuItem.ForeColor = System.Drawing.Color.FromArgb(0, 0, 0);
            autoUseAccordingToPurposeToolStripMenuItem.Name = "autoUseAccordingToPurposeToolStripMenuItem";
            autoUseAccordingToPurposeToolStripMenuItem.Size = new System.Drawing.Size(237, 22);
            autoUseAccordingToPurposeToolStripMenuItem.Text = "Auto use according to purpose";
            autoUseAccordingToPurposeToolStripMenuItem.Click += autoUseAccordingToPurposeToolStripMenuItem_Click;
            // 
            // panelHappy
            // 
            panelHappy.BackColor = System.Drawing.Color.Transparent;
            panelHappy.Border = new System.Windows.Forms.Padding(0, 1, 0, 1);
            panelHappy.BorderColor = System.Drawing.Color.FromArgb(40, 0, 0, 0);
            panelHappy.Controls.Add(lblHappyTitle);
            panelHappy.Controls.Add(lblHappyCounts);
            panelHappy.Controls.Add(lblHappyStatus);
            panelHappy.Controls.Add(checkAutoHappy);
            panelHappy.Controls.Add(btnStartHappy);
            panelHappy.Controls.Add(btnStopHappy);
            panelHappy.Dock = System.Windows.Forms.DockStyle.Bottom;
            panelHappy.Location = new System.Drawing.Point(0, 369);
            panelHappy.Name = "panelHappy";
            panelHappy.Padding = new System.Windows.Forms.Padding(4);
            panelHappy.Radius = 0;
            panelHappy.ShadowDepth = 2F;
            panelHappy.Size = new System.Drawing.Size(792, 44);
            panelHappy.TabIndex = 7;
            // 
            // lblHappyTitle
            // 
            lblHappyTitle.AutoSize = true;
            lblHappyTitle.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            lblHappyTitle.Location = new System.Drawing.Point(9, 13);
            lblHappyTitle.Name = "lblHappyTitle";
            lblHappyTitle.Size = new System.Drawing.Size(76, 15);
            lblHappyTitle.TabIndex = 0;
            lblHappyTitle.Text = "Event So-Ok:";
            // 
            // lblHappyCounts
            // 
            lblHappyCounts.AutoSize = true;
            lblHappyCounts.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            lblHappyCounts.Location = new System.Drawing.Point(88, 14);
            lblHappyCounts.Name = "lblHappyCounts";
            lblHappyCounts.Size = new System.Drawing.Size(165, 13);
            lblHappyCounts.TabIndex = 1;
            lblHappyCounts.Text = "H:0  A:0  P1:0  P2:0  Y:0 | Jewel:0";
            // 
            // lblHappyStatus
            // 
            lblHappyStatus.AutoSize = true;
            lblHappyStatus.Font = new System.Drawing.Font("Segoe UI Semibold", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            lblHappyStatus.ForeColor = System.Drawing.Color.Gray;
            lblHappyStatus.Location = new System.Drawing.Point(256, 14);
            lblHappyStatus.Name = "lblHappyStatus";
            lblHappyStatus.Size = new System.Drawing.Size(35, 13);
            lblHappyStatus.TabIndex = 2;
            lblHappyStatus.Text = "Idle";
            // 
            // checkAutoHappy
            // 
            checkAutoHappy.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
            checkAutoHappy.BackColor = System.Drawing.Color.Transparent;
            checkAutoHappy.Depth = 0;
            checkAutoHappy.Location = new System.Drawing.Point(465, 11);
            checkAutoHappy.Margin = new System.Windows.Forms.Padding(0);
            checkAutoHappy.MouseLocation = new System.Drawing.Point(-1, -1);
            checkAutoHappy.Name = "checkAutoHappy";
            checkAutoHappy.Ripple = true;
            checkAutoHappy.Size = new System.Drawing.Size(95, 20);
            checkAutoHappy.TabIndex = 3;
            checkAutoHappy.Text = "Auto Loop";
            checkAutoHappy.UseVisualStyleBackColor = false;
            checkAutoHappy.CheckedChanged += checkAutoHappy_CheckedChanged;
            // 
            // btnStartHappy
            // 
            btnStartHappy.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
            btnStartHappy.Color = System.Drawing.Color.FromArgb(200, 130, 25);
            btnStartHappy.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            btnStartHappy.ForeColor = System.Drawing.Color.White;
            btnStartHappy.Location = new System.Drawing.Point(568, 8);
            btnStartHappy.Name = "btnStartHappy";
            btnStartHappy.Radius = 6;
            btnStartHappy.ShadowDepth = 4F;
            btnStartHappy.Size = new System.Drawing.Size(125, 26);
            btnStartHappy.TabIndex = 4;
            btnStartHappy.Text = "Start Exchange";
            btnStartHappy.UseVisualStyleBackColor = false;
            btnStartHappy.Click += btnStartHappy_Click;
            // 
            // btnStopHappy
            // 
            btnStopHappy.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
            btnStopHappy.Color = System.Drawing.Color.FromArgb(215, 55, 50);
            btnStopHappy.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            btnStopHappy.ForeColor = System.Drawing.Color.White;
            btnStopHappy.Location = new System.Drawing.Point(702, 8);
            btnStopHappy.Name = "btnStopHappy";
            btnStopHappy.Radius = 6;
            btnStopHappy.ShadowDepth = 4F;
            btnStopHappy.Size = new System.Drawing.Size(80, 26);
            btnStopHappy.TabIndex = 5;
            btnStopHappy.Text = "Stop";
            btnStopHappy.UseVisualStyleBackColor = false;
            btnStopHappy.Click += btnStopHappy_Click;
            // 
            // panel1
            // 
            panel1.BackColor = System.Drawing.Color.Transparent;
            panel1.Border = new System.Windows.Forms.Padding(0, 1, 0, 0);
            panel1.BorderColor = System.Drawing.Color.Transparent;
            panel1.Controls.Add(pbInventoryStatus);
            panel1.Controls.Add(lblFreeSlots);
            panel1.Controls.Add(checkAutoSort);
            panel1.Controls.Add(btnSort);
            panel1.Dock = System.Windows.Forms.DockStyle.Bottom;
            panel1.Location = new System.Drawing.Point(0, 413);
            panel1.Name = "panel1";
            panel1.Padding = new System.Windows.Forms.Padding(3);
            panel1.Radius = 0;
            panel1.ShadowDepth = 4F;
            panel1.Size = new System.Drawing.Size(792, 45);
            panel1.TabIndex = 6;
            // 
            // pbInventoryStatus
            // 
            pbInventoryStatus.BackColor = System.Drawing.Color.Transparent;
            pbInventoryStatus.DrawHatch = false;
            pbInventoryStatus.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            pbInventoryStatus.Gradient = new System.Drawing.Color[] { System.Drawing.Color.FromArgb(224, 224, 224), System.Drawing.Color.Teal };
            pbInventoryStatus.HatchType = System.Drawing.Drawing2D.HatchStyle.Horizontal;
            pbInventoryStatus.Location = new System.Drawing.Point(9, 7);
            pbInventoryStatus.Maximum = 100L;
            pbInventoryStatus.Name = "pbInventoryStatus";
            pbInventoryStatus.Size = new System.Drawing.Size(32, 32);
            pbInventoryStatus.TabIndex = 7;
            pbInventoryStatus.Value = 50L;
            pbInventoryStatus.Weight = 4F;
            // 
            // checkAutoSort
            // 
            checkAutoSort.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
            checkAutoSort.BackColor = System.Drawing.Color.Transparent;
            checkAutoSort.Depth = 0;
            checkAutoSort.Location = new System.Drawing.Point(595, 11);
            checkAutoSort.Margin = new System.Windows.Forms.Padding(0);
            checkAutoSort.MouseLocation = new System.Drawing.Point(-1, -1);
            checkAutoSort.Name = "checkAutoSort";
            checkAutoSort.Ripple = true;
            checkAutoSort.Size = new System.Drawing.Size(95, 22);
            checkAutoSort.TabIndex = 6;
            checkAutoSort.Text = "Auto sort";
            checkAutoSort.UseVisualStyleBackColor = false;
            checkAutoSort.CheckedChanged += checkAutoSort_CheckedChanged;
            // 
            // btnSort
            // 
            btnSort.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
            btnSort.Color = System.Drawing.Color.FromArgb(56, 155, 90);
            btnSort.ForeColor = System.Drawing.Color.White;
            btnSort.Location = new System.Drawing.Point(700, 9);
            btnSort.Name = "btnSort";
            btnSort.Radius = 6;
            btnSort.ShadowDepth = 4F;
            btnSort.Size = new System.Drawing.Size(80, 26);
            btnSort.TabIndex = 5;
            btnSort.Text = "Sort";
            btnSort.UseVisualStyleBackColor = false;
            btnSort.Click += btnSort_Click;
            // 
            // buttonInventory
            // 
            buttonInventory.Color = System.Drawing.Color.Transparent;
            buttonInventory.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            buttonInventory.Location = new System.Drawing.Point(8, 6);
            buttonInventory.Name = "buttonInventory";
            buttonInventory.Radius = 6;
            buttonInventory.ShadowDepth = 4F;
            buttonInventory.Size = new System.Drawing.Size(68, 25);
            buttonInventory.TabIndex = 0;
            buttonInventory.Text = "Inventory";
            buttonInventory.UseVisualStyleBackColor = true;
            buttonInventory.Click += ButtonSwitcher;
            // 
            // buttonEquipment
            // 
            buttonEquipment.Color = System.Drawing.Color.Transparent;
            buttonEquipment.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            buttonEquipment.Location = new System.Drawing.Point(80, 6);
            buttonEquipment.Name = "buttonEquipment";
            buttonEquipment.Radius = 6;
            buttonEquipment.ShadowDepth = 4F;
            buttonEquipment.Size = new System.Drawing.Size(74, 25);
            buttonEquipment.TabIndex = 1;
            buttonEquipment.Text = "Equipment";
            buttonEquipment.UseVisualStyleBackColor = true;
            buttonEquipment.Click += ButtonSwitcher;
            // 
            // buttonAvatars
            // 
            buttonAvatars.Color = System.Drawing.Color.Transparent;
            buttonAvatars.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            buttonAvatars.Location = new System.Drawing.Point(258, 6);
            buttonAvatars.Name = "buttonAvatars";
            buttonAvatars.Radius = 6;
            buttonAvatars.ShadowDepth = 4F;
            buttonAvatars.Size = new System.Drawing.Size(58, 25);
            buttonAvatars.TabIndex = 2;
            buttonAvatars.Text = "Avatars";
            buttonAvatars.UseVisualStyleBackColor = true;
            buttonAvatars.Click += ButtonSwitcher;
            // 
            // buttonGrabpet
            // 
            buttonGrabpet.Color = System.Drawing.Color.Transparent;
            buttonGrabpet.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            buttonGrabpet.Location = new System.Drawing.Point(320, 6);
            buttonGrabpet.Name = "buttonGrabpet";
            buttonGrabpet.Radius = 6;
            buttonGrabpet.ShadowDepth = 4F;
            buttonGrabpet.Size = new System.Drawing.Size(66, 25);
            buttonGrabpet.TabIndex = 3;
            buttonGrabpet.Text = "Grab Pet";
            buttonGrabpet.UseVisualStyleBackColor = true;
            buttonGrabpet.Click += ButtonSwitcher;
            // 
            // topPanel
            // 
            topPanel.BackColor = System.Drawing.Color.Transparent;
            topPanel.Border = new System.Windows.Forms.Padding(0, 0, 0, 1);
            topPanel.BorderColor = System.Drawing.Color.Transparent;
            topPanel.Controls.Add(buttonSpecialty);
            topPanel.Controls.Add(buttonGuildStorage);
            topPanel.Controls.Add(buttonGrabpet);
            topPanel.Controls.Add(buttonInventory);
            topPanel.Controls.Add(buttonFellowPet);
            topPanel.Controls.Add(buttonJobTransport);
            topPanel.Controls.Add(buttonStorage);
            topPanel.Controls.Add(buttonJobEquipment);
            topPanel.Controls.Add(buttonEquipment);
            topPanel.Controls.Add(buttonAvatars);
            topPanel.Dock = System.Windows.Forms.DockStyle.Top;
            topPanel.Location = new System.Drawing.Point(0, 0);
            topPanel.Name = "topPanel";
            topPanel.Padding = new System.Windows.Forms.Padding(8);
            topPanel.Radius = 0;
            topPanel.ShadowDepth = 4F;
            topPanel.Size = new System.Drawing.Size(792, 38);
            topPanel.TabIndex = 8;
            // 
            // buttonSpecialty
            // 
            buttonSpecialty.Color = System.Drawing.Color.Transparent;
            buttonSpecialty.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            buttonSpecialty.Location = new System.Drawing.Point(564, 6);
            buttonSpecialty.Name = "buttonSpecialty";
            buttonSpecialty.Radius = 6;
            buttonSpecialty.ShadowDepth = 4F;
            buttonSpecialty.Size = new System.Drawing.Size(66, 25);
            buttonSpecialty.TabIndex = 7;
            buttonSpecialty.Text = "Specialty";
            buttonSpecialty.UseVisualStyleBackColor = true;
            buttonSpecialty.Click += ButtonSwitcher;
            // 
            // buttonGuildStorage
            // 
            buttonGuildStorage.Color = System.Drawing.Color.Transparent;
            buttonGuildStorage.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            buttonGuildStorage.Location = new System.Drawing.Point(696, 6);
            buttonGuildStorage.Name = "buttonGuildStorage";
            buttonGuildStorage.Radius = 6;
            buttonGuildStorage.ShadowDepth = 4F;
            buttonGuildStorage.Size = new System.Drawing.Size(92, 25);
            buttonGuildStorage.TabIndex = 5;
            buttonGuildStorage.Text = "Guild Storage";
            buttonGuildStorage.UseVisualStyleBackColor = true;
            buttonGuildStorage.Click += ButtonSwitcher;
            // 
            // buttonFellowPet
            // 
            buttonFellowPet.Color = System.Drawing.Color.Transparent;
            buttonFellowPet.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            buttonFellowPet.Location = new System.Drawing.Point(390, 6);
            buttonFellowPet.Name = "buttonFellowPet";
            buttonFellowPet.Radius = 6;
            buttonFellowPet.ShadowDepth = 4F;
            buttonFellowPet.Size = new System.Drawing.Size(74, 25);
            buttonFellowPet.TabIndex = 9;
            buttonFellowPet.Text = "Fellow Pet";
            buttonFellowPet.UseVisualStyleBackColor = true;
            buttonFellowPet.Click += ButtonSwitcher;
            // 
            // buttonJobTransport
            // 
            buttonJobTransport.Color = System.Drawing.Color.Transparent;
            buttonJobTransport.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            buttonJobTransport.Location = new System.Drawing.Point(468, 6);
            buttonJobTransport.Name = "buttonJobTransport";
            buttonJobTransport.Radius = 6;
            buttonJobTransport.ShadowDepth = 4F;
            buttonJobTransport.Size = new System.Drawing.Size(92, 25);
            buttonJobTransport.TabIndex = 6;
            buttonJobTransport.Text = "Job Transport";
            buttonJobTransport.UseVisualStyleBackColor = true;
            buttonJobTransport.Click += ButtonSwitcher;
            // 
            // buttonStorage
            // 
            buttonStorage.Color = System.Drawing.Color.Transparent;
            buttonStorage.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            buttonStorage.Location = new System.Drawing.Point(634, 6);
            buttonStorage.Name = "buttonStorage";
            buttonStorage.Radius = 6;
            buttonStorage.ShadowDepth = 4F;
            buttonStorage.Size = new System.Drawing.Size(58, 25);
            buttonStorage.TabIndex = 4;
            buttonStorage.Text = "Storage";
            buttonStorage.UseVisualStyleBackColor = true;
            buttonStorage.Click += ButtonSwitcher;
            // 
            // buttonJobEquipment
            // 
            buttonJobEquipment.Color = System.Drawing.Color.Transparent;
            buttonJobEquipment.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            buttonJobEquipment.Location = new System.Drawing.Point(158, 6);
            buttonJobEquipment.Name = "buttonJobEquipment";
            buttonJobEquipment.Radius = 6;
            buttonJobEquipment.ShadowDepth = 4F;
            buttonJobEquipment.Size = new System.Drawing.Size(96, 25);
            buttonJobEquipment.TabIndex = 8;
            buttonJobEquipment.Text = "Job Equipment";
            buttonJobEquipment.UseVisualStyleBackColor = true;
            buttonJobEquipment.Click += ButtonSwitcher;
            // 
            // Main
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            Controls.Add(listViewMain);
            Controls.Add(panelHappy);
            Controls.Add(topPanel);
            Controls.Add(panel1);
            Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            Name = "Main";
            Size = new System.Drawing.Size(792, 458);
            Load += Main_Load;
            VisibleChanged += Main_VisibleChanged;
            contextMenuStrip.ResumeLayout(false);
            panelHappy.ResumeLayout(false);
            panelHappy.PerformLayout();
            panel1.ResumeLayout(false);
            panel1.PerformLayout();
            topPanel.ResumeLayout(false);
            ResumeLayout(false);
        }

        #endregion
        private SDUI.Controls.ListView listViewMain;
        private System.Windows.Forms.ColumnHeader colName;
        private System.Windows.Forms.ColumnHeader colAmount;
        private SDUI.Controls.Label label2;
        private SDUI.Controls.Label lblFreeSlots;
        private System.Windows.Forms.ColumnHeader colGenderRace;
        private SDUI.Controls.Panel panel1;
        private SDUI.Controls.Panel panelHappy;
        private SDUI.Controls.Label lblHappyTitle;
        private SDUI.Controls.Label lblHappyCounts;
        private SDUI.Controls.Label lblHappyStatus;
        private SDUI.Controls.CheckBox checkAutoHappy;
        private SDUI.Controls.Button btnStartHappy;
        private SDUI.Controls.Button btnStopHappy;
        private System.Windows.Forms.Timer timerHappy;
        private SDUI.Controls.Button buttonInventory;
        private SDUI.Controls.Button buttonEquipment;
        private SDUI.Controls.Button buttonAvatars;
        private SDUI.Controls.Button buttonGrabpet;
        private SDUI.Controls.Panel topPanel;
        private SDUI.Controls.Button buttonSpecialty;
        private SDUI.Controls.Button buttonGuildStorage;
        private SDUI.Controls.Button buttonJobTransport;
        private SDUI.Controls.Button buttonStorage;
        private SDUI.Controls.Button buttonJobEquipment;
        private SDUI.Controls.ContextMenuStrip contextMenuStrip;
        private System.Windows.Forms.ToolStripMenuItem useToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem dropToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem moveToLastDeathPositionToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem moveToLastRecallPositionToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem selectMapLocationToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem moveToPetToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem moveToPlayerToolStripMenuItem;
        private SDUI.Controls.Button buttonFellowPet;
        private SDUI.Controls.Button btnSort;
        private SDUI.Controls.CheckBox checkAutoSort;
        private System.Windows.Forms.ToolStripMenuItem useItemAtTrainingPlaceMenuItem;
        private System.Windows.Forms.ToolStripMenuItem autoUseAccordingToPurposeToolStripMenuItem;
        private SDUI.Controls.ShapeProgressBar pbInventoryStatus;
    }
}
