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
            lblHappyDelay = new SDUI.Controls.Label();
            numHappyDelay = new SDUI.Controls.NumUpDown();

            checkAutoHappy = new SDUI.Controls.CheckBox();

            btnStartHappy = new SDUI.Controls.Button();

            btnStopHappy = new SDUI.Controls.Button();

            timerHappy = new System.Windows.Forms.Timer();

            pbInventoryStatus = new SDUI.Controls.ShapeProgressBar();

            checkAutoSort = new SDUI.Controls.CheckBox();

            checkAutoOrder = new SDUI.Controls.CheckBox();

            btnSort = new SDUI.Controls.Button();

            btnOrder = new SDUI.Controls.Button();

            buttonInventory = new SDUI.Controls.Button();
            buttonStorageOrder = new SDUI.Controls.Button();
            panelStorageOrder = new SDUI.Controls.Panel();
            lblStorageOrderTitle = new SDUI.Controls.Label();
            lblPagesHeader = new SDUI.Controls.Label();
            lblPage1 = new SDUI.Controls.Label();
            lblPage2 = new SDUI.Controls.Label();
            lblPage3 = new SDUI.Controls.Label();
            lblPage4 = new SDUI.Controls.Label();
            lblPage5 = new SDUI.Controls.Label();
            lblOverflowNote = new SDUI.Controls.Label();
            lblSettingsHeader = new SDUI.Controls.Label();
            checkStorageSplitPages = new SDUI.Controls.CheckBox();
            checkStorageAutoOrder = new SDUI.Controls.CheckBox();
            checkStorageAutoSort = new SDUI.Controls.CheckBox();
            checkGuildStorageAutoOrder = new SDUI.Controls.CheckBox();
            checkGuildStorageAutoSort = new SDUI.Controls.CheckBox();
            lblActionsHeader = new SDUI.Controls.Label();
            btnOrderStorageNow = new SDUI.Controls.Button();
            btnSortStorageNow = new SDUI.Controls.Button();
            btnOrderGuildStorageNow = new SDUI.Controls.Button();
            lblStorageOrderStatus = new SDUI.Controls.Label();

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

            buttonHappy = new SDUI.Controls.Button();

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
            panelHappy.Controls.Add(lblHappyDelay);
            panelHappy.Controls.Add(numHappyDelay);

            panelHappy.Controls.Add(btnStartHappy);

            panelHappy.Controls.Add(btnStopHappy);

            panelHappy.Dock = System.Windows.Forms.DockStyle.Fill;

            panelHappy.Location = new System.Drawing.Point(0, 0);

            panelHappy.Name = "panelHappy";

            panelHappy.Padding = new System.Windows.Forms.Padding(4);

            panelHappy.Radius = 0;

            panelHappy.ShadowDepth = 2F;

            panelHappy.Size = new System.Drawing.Size(792, 458);

            panelHappy.TabIndex = 7;

            panelHappy.Visible = false;

            // 

            // lblHappyTitle

            // 

            lblHappyTitle.AutoSize = true;

            lblHappyTitle.Font = new System.Drawing.Font("Segoe UI", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);

            lblHappyTitle.Location = new System.Drawing.Point(28, 28);

            lblHappyTitle.Name = "lblHappyTitle";

            lblHappyTitle.Size = new System.Drawing.Size(160, 25);

            lblHappyTitle.TabIndex = 0;

            lblHappyTitle.Text = "Happy Event (So-Ok)";

            // 

            // lblHappyCounts

            // 

            lblHappyCounts.AutoSize = true;

            lblHappyCounts.Font = new System.Drawing.Font("Segoe UI", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);

            lblHappyCounts.Location = new System.Drawing.Point(30, 70);

            lblHappyCounts.Name = "lblHappyCounts";

            lblHappyCounts.Size = new System.Drawing.Size(220, 20);

            lblHappyCounts.TabIndex = 1;

            lblHappyCounts.Text = "H:0  A:0  P1:0  P2:0  Y:0 | Jewel:0";

            // 

            // lblHappyStatus

            // 

            lblHappyStatus.AutoSize = true;

            lblHappyStatus.Font = new System.Drawing.Font("Segoe UI Semibold", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);

            lblHappyStatus.ForeColor = System.Drawing.Color.Gray;

            lblHappyStatus.Location = new System.Drawing.Point(31, 98);

            lblHappyStatus.Name = "lblHappyStatus";

            lblHappyStatus.Size = new System.Drawing.Size(35, 19);

            lblHappyStatus.TabIndex = 2;

            lblHappyStatus.Text = "Idle";

            // 

            // checkAutoHappy

            // 

            checkAutoHappy.BackColor = System.Drawing.Color.Transparent;

            checkAutoHappy.Depth = 0;

            checkAutoHappy.Location = new System.Drawing.Point(30, 134);

            checkAutoHappy.Margin = new System.Windows.Forms.Padding(0);

            checkAutoHappy.MouseLocation = new System.Drawing.Point(-1, -1);

            checkAutoHappy.Name = "checkAutoHappy";

            checkAutoHappy.Ripple = true;

            checkAutoHappy.Size = new System.Drawing.Size(120, 24);

            checkAutoHappy.TabIndex = 3;

            checkAutoHappy.Text = "Auto Loop";

            checkAutoHappy.UseVisualStyleBackColor = false;

            checkAutoHappy.CheckedChanged += checkAutoHappy_CheckedChanged;
            // 
            // lblHappyDelay
            // 
            lblHappyDelay.AutoSize = true;
            lblHappyDelay.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            lblHappyDelay.Location = new System.Drawing.Point(175, 137);
            lblHappyDelay.Name = "lblHappyDelay";
            lblHappyDelay.Size = new System.Drawing.Size(120, 15);
            lblHappyDelay.TabIndex = 6;
            lblHappyDelay.Text = "NPC Delay (sec):";
            // 
            // numHappyDelay
            // 
            numHappyDelay.BackColor = System.Drawing.Color.Transparent;
            numHappyDelay.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            numHappyDelay.ForeColor = System.Drawing.Color.FromArgb(0, 0, 0);
            numHappyDelay.Location = new System.Drawing.Point(305, 132);
            numHappyDelay.Maximum = new decimal(new int[] { 30, 0, 0, 0 });
            numHappyDelay.Minimum = new decimal(new int[] { 0, 0, 0, 0 });
            numHappyDelay.MinimumSize = new System.Drawing.Size(65, 25);
            numHappyDelay.Name = "numHappyDelay";
            numHappyDelay.Size = new System.Drawing.Size(65, 25);
            numHappyDelay.TabIndex = 7;
            numHappyDelay.Value = new decimal(new int[] { 1, 0, 0, 0 });
            numHappyDelay.ValueChanged += numHappyDelay_ValueChanged;

            // 

            // btnStartHappy

            // 

            btnStartHappy.Color = System.Drawing.Color.FromArgb(200, 130, 25);

            btnStartHappy.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);

            btnStartHappy.ForeColor = System.Drawing.Color.White;

            btnStartHappy.Location = new System.Drawing.Point(30, 172);

            btnStartHappy.Name = "btnStartHappy";

            btnStartHappy.Radius = 6;

            btnStartHappy.ShadowDepth = 4F;

            btnStartHappy.Size = new System.Drawing.Size(150, 34);

            btnStartHappy.TabIndex = 4;

            btnStartHappy.Text = "Start Exchange";

            btnStartHappy.UseVisualStyleBackColor = false;

            btnStartHappy.Click += btnStartHappy_Click;

            // 

            // btnStopHappy

            // 

            btnStopHappy.Color = System.Drawing.Color.FromArgb(215, 55, 50);

            btnStopHappy.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);

            btnStopHappy.ForeColor = System.Drawing.Color.White;

            btnStopHappy.Location = new System.Drawing.Point(188, 172);

            btnStopHappy.Name = "btnStopHappy";

            btnStopHappy.Radius = 6;

            btnStopHappy.ShadowDepth = 4F;

            btnStopHappy.Size = new System.Drawing.Size(110, 34);

            btnStopHappy.TabIndex = 5;

            btnStopHappy.Text = "Stop";

            btnStopHappy.UseVisualStyleBackColor = false;

            btnStopHappy.Click += btnStopHappy_Click;

            // 
            // panelStorageOrder
            // 
            panelStorageOrder.BackColor = System.Drawing.Color.Transparent;
            panelStorageOrder.Border = new System.Windows.Forms.Padding(0, 1, 0, 1);
            panelStorageOrder.BorderColor = System.Drawing.Color.FromArgb(40, 0, 0, 0);
            panelStorageOrder.Controls.Add(lblStorageOrderTitle);
            panelStorageOrder.Controls.Add(lblPagesHeader);
            panelStorageOrder.Controls.Add(lblPage1);
            panelStorageOrder.Controls.Add(lblPage2);
            panelStorageOrder.Controls.Add(lblPage3);
            panelStorageOrder.Controls.Add(lblPage4);
            panelStorageOrder.Controls.Add(lblPage5);
            panelStorageOrder.Controls.Add(lblOverflowNote);
            panelStorageOrder.Controls.Add(lblSettingsHeader);
            panelStorageOrder.Controls.Add(checkStorageSplitPages);
            panelStorageOrder.Controls.Add(checkStorageAutoOrder);
            panelStorageOrder.Controls.Add(checkStorageAutoSort);
            panelStorageOrder.Controls.Add(checkGuildStorageAutoOrder);
            panelStorageOrder.Controls.Add(checkGuildStorageAutoSort);
            panelStorageOrder.Controls.Add(lblActionsHeader);
            panelStorageOrder.Controls.Add(btnOrderStorageNow);
            panelStorageOrder.Controls.Add(btnSortStorageNow);
            panelStorageOrder.Controls.Add(btnOrderGuildStorageNow);
            panelStorageOrder.Controls.Add(lblStorageOrderStatus);
            panelStorageOrder.Dock = System.Windows.Forms.DockStyle.Fill;
            panelStorageOrder.Location = new System.Drawing.Point(0, 0);
            panelStorageOrder.Name = "panelStorageOrder";
            panelStorageOrder.Padding = new System.Windows.Forms.Padding(4);
            panelStorageOrder.Radius = 0;
            panelStorageOrder.ShadowDepth = 2F;
            panelStorageOrder.Size = new System.Drawing.Size(792, 458);
            panelStorageOrder.TabIndex = 8;
            panelStorageOrder.Visible = false;

            // 
            // lblStorageOrderTitle
            // 
            lblStorageOrderTitle.AutoSize = true;
            lblStorageOrderTitle.Font = new System.Drawing.Font("Segoe UI", 13F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            lblStorageOrderTitle.Location = new System.Drawing.Point(28, 20);
            lblStorageOrderTitle.Name = "lblStorageOrderTitle";
            lblStorageOrderTitle.Size = new System.Drawing.Size(320, 25);
            lblStorageOrderTitle.TabIndex = 0;
            lblStorageOrderTitle.Text = "Storage Order (ترتيب وتقسيم المستودع)";

            // 
            // lblPagesHeader
            // 
            lblPagesHeader.AutoSize = true;
            lblPagesHeader.Font = new System.Drawing.Font("Segoe UI Semibold", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            lblPagesHeader.Location = new System.Drawing.Point(30, 55);
            lblPagesHeader.Name = "lblPagesHeader";
            lblPagesHeader.Size = new System.Drawing.Size(300, 17);
            lblPagesHeader.TabIndex = 1;
            lblPagesHeader.Text = "Pages Layout (تقسيم الصفحات - كل صفحة 30 خانة):";

            // 
            // lblPage1
            // 
            lblPage1.AutoSize = true;
            lblPage1.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            lblPage1.Location = new System.Drawing.Point(35, 80);
            lblPage1.Name = "lblPage1";
            lblPage1.Size = new System.Drawing.Size(320, 15);
            lblPage1.TabIndex = 2;
            lblPage1.Text = "• Page 1: Weapons & Equipment (الأسلحة والعتاد والإكسسوارات)";

            // 
            // lblPage2
            // 
            lblPage2.AutoSize = true;
            lblPage2.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            lblPage2.Location = new System.Drawing.Point(35, 105);
            lblPage2.Name = "lblPage2";
            lblPage2.Size = new System.Drawing.Size(320, 15);
            lblPage2.TabIndex = 3;
            lblPage2.Text = "• Page 2: Elixirs & Enhancement (إكسيرات الأسلحة والدروع)";

            // 
            // lblPage3
            // 
            lblPage3.AutoSize = true;
            lblPage3.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            lblPage3.Location = new System.Drawing.Point(35, 130);
            lblPage3.Name = "lblPage3";
            lblPage3.Size = new System.Drawing.Size(320, 15);
            lblPage3.TabIndex = 4;
            lblPage3.Text = "• Page 3: Magic & Attribute Stones (أحجار الماجيك والأتربيوت)";

            // 
            // lblPage4
            // 
            lblPage4.AutoSize = true;
            lblPage4.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            lblPage4.Location = new System.Drawing.Point(35, 155);
            lblPage4.Name = "lblPage4";
            lblPage4.Size = new System.Drawing.Size(320, 15);
            lblPage4.TabIndex = 5;
            lblPage4.Text = "• Page 4: Recovery Potions & Pills (جرعات HP / MP / Pills)";

            // 
            // lblPage5
            // 
            lblPage5.AutoSize = true;
            lblPage5.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            lblPage5.Location = new System.Drawing.Point(35, 180);
            lblPage5.Name = "lblPage5";
            lblPage5.Size = new System.Drawing.Size(320, 15);
            lblPage5.TabIndex = 6;
            lblPage5.Text = "• Page 5: Scrolls & Other Items (مخطوطات العودة وباقي العناصر)";

            // 
            // lblOverflowNote
            // 
            lblOverflowNote.AutoSize = true;
            lblOverflowNote.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Italic, System.Drawing.GraphicsUnit.Point);
            lblOverflowNote.ForeColor = System.Drawing.Color.Gray;
            lblOverflowNote.Location = new System.Drawing.Point(35, 205);
            lblOverflowNote.Name = "lblOverflowNote";
            lblOverflowNote.Size = new System.Drawing.Size(340, 13);
            lblOverflowNote.TabIndex = 7;
            lblOverflowNote.Text = "* عند امتلاء الصفحة، تذهب العناصر الفائضة لأول خانة فارغة متاحة.";

            // 
            // lblSettingsHeader
            // 
            lblSettingsHeader.AutoSize = true;
            lblSettingsHeader.Font = new System.Drawing.Font("Segoe UI Semibold", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            lblSettingsHeader.Location = new System.Drawing.Point(440, 20);
            lblSettingsHeader.Name = "lblSettingsHeader";
            lblSettingsHeader.Size = new System.Drawing.Size(120, 17);
            lblSettingsHeader.TabIndex = 8;
            lblSettingsHeader.Text = "Settings (الإعدادات):";

            // 
            // checkStorageSplitPages
            // 
            checkStorageSplitPages.BackColor = System.Drawing.Color.Transparent;
            checkStorageSplitPages.Depth = 0;
            checkStorageSplitPages.Location = new System.Drawing.Point(440, 45);
            checkStorageSplitPages.Margin = new System.Windows.Forms.Padding(0);
            checkStorageSplitPages.MouseLocation = new System.Drawing.Point(-1, -1);
            checkStorageSplitPages.Name = "checkStorageSplitPages";
            checkStorageSplitPages.Ripple = true;
            checkStorageSplitPages.Size = new System.Drawing.Size(330, 24);
            checkStorageSplitPages.TabIndex = 9;
            checkStorageSplitPages.Text = "Split Storage by Pages (تقسيم صفحات المستودع)";
            checkStorageSplitPages.UseVisualStyleBackColor = false;
            checkStorageSplitPages.CheckedChanged += checkStorageSplitPages_CheckedChanged;

            // 
            // checkStorageAutoOrder
            // 
            checkStorageAutoOrder.BackColor = System.Drawing.Color.Transparent;
            checkStorageAutoOrder.Depth = 0;
            checkStorageAutoOrder.Location = new System.Drawing.Point(440, 72);
            checkStorageAutoOrder.Margin = new System.Windows.Forms.Padding(0);
            checkStorageAutoOrder.MouseLocation = new System.Drawing.Point(-1, -1);
            checkStorageAutoOrder.Name = "checkStorageAutoOrder";
            checkStorageAutoOrder.Ripple = true;
            checkStorageAutoOrder.Size = new System.Drawing.Size(330, 24);
            checkStorageAutoOrder.TabIndex = 10;
            checkStorageAutoOrder.Text = "Auto Order Storage (ترتيب تلقائي للمستودع)";
            checkStorageAutoOrder.UseVisualStyleBackColor = false;
            checkStorageAutoOrder.CheckedChanged += checkStorageAutoOrder_CheckedChanged;

            // 
            // checkStorageAutoSort
            // 
            checkStorageAutoSort.BackColor = System.Drawing.Color.Transparent;
            checkStorageAutoSort.Depth = 0;
            checkStorageAutoSort.Location = new System.Drawing.Point(440, 99);
            checkStorageAutoSort.Margin = new System.Windows.Forms.Padding(0);
            checkStorageAutoSort.MouseLocation = new System.Drawing.Point(-1, -1);
            checkStorageAutoSort.Name = "checkStorageAutoSort";
            checkStorageAutoSort.Ripple = true;
            checkStorageAutoSort.Size = new System.Drawing.Size(330, 24);
            checkStorageAutoSort.TabIndex = 11;
            checkStorageAutoSort.Text = "Auto Sort Storage (دمج وتكديس المستودع)";
            checkStorageAutoSort.UseVisualStyleBackColor = false;
            checkStorageAutoSort.CheckedChanged += checkStorageAutoSort_CheckedChanged;

            // 
            // checkGuildStorageAutoOrder
            // 
            checkGuildStorageAutoOrder.BackColor = System.Drawing.Color.Transparent;
            checkGuildStorageAutoOrder.Depth = 0;
            checkGuildStorageAutoOrder.Location = new System.Drawing.Point(440, 126);
            checkGuildStorageAutoOrder.Margin = new System.Windows.Forms.Padding(0);
            checkGuildStorageAutoOrder.MouseLocation = new System.Drawing.Point(-1, -1);
            checkGuildStorageAutoOrder.Name = "checkGuildStorageAutoOrder";
            checkGuildStorageAutoOrder.Ripple = true;
            checkGuildStorageAutoOrder.Size = new System.Drawing.Size(330, 24);
            checkGuildStorageAutoOrder.TabIndex = 12;
            checkGuildStorageAutoOrder.Text = "Auto Order Guild Storage (ترتيب تلقائي للجيلد)";
            checkGuildStorageAutoOrder.UseVisualStyleBackColor = false;
            checkGuildStorageAutoOrder.CheckedChanged += checkGuildStorageAutoOrder_CheckedChanged;

            // 
            // checkGuildStorageAutoSort
            // 
            checkGuildStorageAutoSort.BackColor = System.Drawing.Color.Transparent;
            checkGuildStorageAutoSort.Depth = 0;
            checkGuildStorageAutoSort.Location = new System.Drawing.Point(440, 153);
            checkGuildStorageAutoSort.Margin = new System.Windows.Forms.Padding(0);
            checkGuildStorageAutoSort.MouseLocation = new System.Drawing.Point(-1, -1);
            checkGuildStorageAutoSort.Name = "checkGuildStorageAutoSort";
            checkGuildStorageAutoSort.Ripple = true;
            checkGuildStorageAutoSort.Size = new System.Drawing.Size(330, 24);
            checkGuildStorageAutoSort.TabIndex = 13;
            checkGuildStorageAutoSort.Text = "Auto Sort Guild Storage (دمج وتكديس الجيلد)";
            checkGuildStorageAutoSort.UseVisualStyleBackColor = false;
            checkGuildStorageAutoSort.CheckedChanged += checkGuildStorageAutoSort_CheckedChanged;

            // 
            // lblActionsHeader
            // 
            lblActionsHeader.AutoSize = true;
            lblActionsHeader.Font = new System.Drawing.Font("Segoe UI Semibold", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            lblActionsHeader.Location = new System.Drawing.Point(30, 235);
            lblActionsHeader.Name = "lblActionsHeader";
            lblActionsHeader.Size = new System.Drawing.Size(160, 17);
            lblActionsHeader.TabIndex = 14;
            lblActionsHeader.Text = "Manual Actions (يدوي):";

            // 
            // btnOrderStorageNow
            // 
            btnOrderStorageNow.Color = System.Drawing.Color.FromArgb(45, 140, 220);
            btnOrderStorageNow.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            btnOrderStorageNow.ForeColor = System.Drawing.Color.White;
            btnOrderStorageNow.Location = new System.Drawing.Point(30, 260);
            btnOrderStorageNow.Name = "btnOrderStorageNow";
            btnOrderStorageNow.Radius = 6;
            btnOrderStorageNow.ShadowDepth = 4F;
            btnOrderStorageNow.Size = new System.Drawing.Size(155, 32);
            btnOrderStorageNow.TabIndex = 15;
            btnOrderStorageNow.Text = "Order Storage (ترتيب)";
            btnOrderStorageNow.UseVisualStyleBackColor = false;
            btnOrderStorageNow.Click += btnOrderStorageNow_Click;

            // 
            // btnSortStorageNow
            // 
            btnSortStorageNow.Color = System.Drawing.Color.FromArgb(70, 160, 90);
            btnSortStorageNow.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            btnSortStorageNow.ForeColor = System.Drawing.Color.White;
            btnSortStorageNow.Location = new System.Drawing.Point(195, 260);
            btnSortStorageNow.Name = "btnSortStorageNow";
            btnSortStorageNow.Radius = 6;
            btnSortStorageNow.ShadowDepth = 4F;
            btnSortStorageNow.Size = new System.Drawing.Size(155, 32);
            btnSortStorageNow.TabIndex = 16;
            btnSortStorageNow.Text = "Sort Storage (دمج)";
            btnSortStorageNow.UseVisualStyleBackColor = false;
            btnSortStorageNow.Click += btnSortStorageNow_Click;

            // 
            // btnOrderGuildStorageNow
            // 
            btnOrderGuildStorageNow.Color = System.Drawing.Color.FromArgb(170, 90, 210);
            btnOrderGuildStorageNow.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            btnOrderGuildStorageNow.ForeColor = System.Drawing.Color.White;
            btnOrderGuildStorageNow.Location = new System.Drawing.Point(360, 260);
            btnOrderGuildStorageNow.Name = "btnOrderGuildStorageNow";
            btnOrderGuildStorageNow.Radius = 6;
            btnOrderGuildStorageNow.ShadowDepth = 4F;
            btnOrderGuildStorageNow.Size = new System.Drawing.Size(175, 32);
            btnOrderGuildStorageNow.TabIndex = 17;
            btnOrderGuildStorageNow.Text = "Order Guild (ترتيب الجيلد)";
            btnOrderGuildStorageNow.UseVisualStyleBackColor = false;
            btnOrderGuildStorageNow.Click += btnOrderGuildStorageNow_Click;

            // 
            // lblStorageOrderStatus
            // 
            lblStorageOrderStatus.AutoSize = true;
            lblStorageOrderStatus.Font = new System.Drawing.Font("Segoe UI Semibold", 9.5F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            lblStorageOrderStatus.ForeColor = System.Drawing.Color.Gray;
            lblStorageOrderStatus.Location = new System.Drawing.Point(32, 305);
            lblStorageOrderStatus.Name = "lblStorageOrderStatus";
            lblStorageOrderStatus.Size = new System.Drawing.Size(44, 17);
            lblStorageOrderStatus.TabIndex = 18;
            lblStorageOrderStatus.Text = "Ready";

            // 

            // panel1

            // 

            panel1.BackColor = System.Drawing.Color.Transparent;

            panel1.Border = new System.Windows.Forms.Padding(0, 1, 0, 0);

            panel1.BorderColor = System.Drawing.Color.Transparent;

            panel1.Controls.Add(pbInventoryStatus);

            panel1.Controls.Add(lblFreeSlots);

            panel1.Controls.Add(checkAutoSort);

            panel1.Controls.Add(checkAutoOrder);

            panel1.Controls.Add(btnSort);

            panel1.Controls.Add(btnOrder);

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

            checkAutoSort.Location = new System.Drawing.Point(346, 10);

            checkAutoSort.Margin = new System.Windows.Forms.Padding(0);

            checkAutoSort.MouseLocation = new System.Drawing.Point(-1, -1);

            checkAutoSort.Name = "checkAutoSort";

            checkAutoSort.Ripple = true;

            checkAutoSort.Size = new System.Drawing.Size(130, 24);

            checkAutoSort.TabIndex = 6;

            checkAutoSort.Text = "Auto sort";

            checkAutoSort.UseVisualStyleBackColor = false;

            checkAutoSort.CheckedChanged += checkAutoSort_CheckedChanged;

            // 

            // checkAutoOrder

            // 

            checkAutoOrder.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;

            checkAutoOrder.BackColor = System.Drawing.Color.Transparent;

            checkAutoOrder.Depth = 0;

            checkAutoOrder.Location = new System.Drawing.Point(482, 10);

            checkAutoOrder.Margin = new System.Windows.Forms.Padding(0);

            checkAutoOrder.MouseLocation = new System.Drawing.Point(-1, -1);

            checkAutoOrder.Name = "checkAutoOrder";

            checkAutoOrder.Ripple = true;

            checkAutoOrder.Size = new System.Drawing.Size(134, 24);

            checkAutoOrder.TabIndex = 7;

            checkAutoOrder.Text = "Auto order";

            checkAutoOrder.UseVisualStyleBackColor = false;

            checkAutoOrder.CheckedChanged += checkAutoOrder_CheckedChanged;

            // 

            // btnSort

            // 

            btnSort.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;

            btnSort.Color = System.Drawing.Color.FromArgb(70, 130, 180);

            btnSort.ForeColor = System.Drawing.Color.White;

            btnSort.Location = new System.Drawing.Point(623, 9);

            btnSort.Name = "btnSort";

            btnSort.Radius = 6;

            btnSort.ShadowDepth = 4F;

            btnSort.Size = new System.Drawing.Size(75, 26);

            btnSort.TabIndex = 5;

            btnSort.Text = "Sort";

            btnSort.UseVisualStyleBackColor = false;

            btnSort.Click += btnSort_Click;

            // 

            // btnOrder

            // 

            btnOrder.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;

            btnOrder.Color = System.Drawing.Color.FromArgb(56, 155, 90);

            btnOrder.ForeColor = System.Drawing.Color.White;

            btnOrder.Location = new System.Drawing.Point(704, 9);

            btnOrder.Name = "btnOrder";

            btnOrder.Radius = 6;

            btnOrder.ShadowDepth = 4F;

            btnOrder.Size = new System.Drawing.Size(78, 26);

            btnOrder.TabIndex = 8;

            btnOrder.Text = "Order";

            btnOrder.UseVisualStyleBackColor = false;

            btnOrder.Click += btnOrder_Click;

            // 

            // buttonInventory

            // 

            buttonInventory.Color = System.Drawing.Color.Transparent;

            buttonInventory.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);

            buttonInventory.Location = new System.Drawing.Point(6, 6);

            buttonInventory.Name = "buttonInventory";

            buttonInventory.Radius = 6;

            buttonInventory.ShadowDepth = 4F;

            buttonInventory.Size = new System.Drawing.Size(60, 25);

            buttonInventory.TabIndex = 0;

            buttonInventory.Text = "Inventory";

            buttonInventory.UseVisualStyleBackColor = true;

            buttonInventory.Click += ButtonSwitcher;

            // 

            // buttonEquipment

            // 

            buttonEquipment.Color = System.Drawing.Color.Transparent;

            buttonEquipment.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);

            buttonEquipment.Location = new System.Drawing.Point(68, 6);

            buttonEquipment.Name = "buttonEquipment";

            buttonEquipment.Radius = 6;

            buttonEquipment.ShadowDepth = 4F;

            buttonEquipment.Size = new System.Drawing.Size(68, 25);

            buttonEquipment.TabIndex = 1;

            buttonEquipment.Text = "Equipment";

            buttonEquipment.UseVisualStyleBackColor = true;

            buttonEquipment.Click += ButtonSwitcher;

            // 

            // buttonAvatars

            // 

            buttonAvatars.Color = System.Drawing.Color.Transparent;

            buttonAvatars.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);

            buttonAvatars.Location = new System.Drawing.Point(228, 6);

            buttonAvatars.Name = "buttonAvatars";

            buttonAvatars.Radius = 6;

            buttonAvatars.ShadowDepth = 4F;

            buttonAvatars.Size = new System.Drawing.Size(50, 25);

            buttonAvatars.TabIndex = 2;

            buttonAvatars.Text = "Avatars";

            buttonAvatars.UseVisualStyleBackColor = true;

            buttonAvatars.Click += ButtonSwitcher;

            // 

            // buttonGrabpet

            // 

            buttonGrabpet.Color = System.Drawing.Color.Transparent;

            buttonGrabpet.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);

            buttonGrabpet.Location = new System.Drawing.Point(280, 6);

            buttonGrabpet.Name = "buttonGrabpet";

            buttonGrabpet.Radius = 6;

            buttonGrabpet.ShadowDepth = 4F;

            buttonGrabpet.Size = new System.Drawing.Size(56, 25);

            buttonGrabpet.TabIndex = 3;

            buttonGrabpet.Text = "Grab Pet";

            buttonGrabpet.UseVisualStyleBackColor = true;

            buttonGrabpet.Click += ButtonSwitcher;

            // 

            // topPanel

            // 

            topPanel.BackColor = System.Drawing.Color.FromArgb(20, 26, 38);

            topPanel.Border = new System.Windows.Forms.Padding(0, 0, 0, 1);

            topPanel.BorderColor = System.Drawing.Color.FromArgb(35, 45, 60);

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

            topPanel.Controls.Add(buttonHappy);

            topPanel.Controls.Add(buttonStorageOrder);

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

            buttonSpecialty.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);

            buttonSpecialty.Location = new System.Drawing.Point(484, 6);

            buttonSpecialty.Name = "buttonSpecialty";

            buttonSpecialty.Radius = 6;

            buttonSpecialty.ShadowDepth = 4F;

            buttonSpecialty.Size = new System.Drawing.Size(56, 25);

            buttonSpecialty.TabIndex = 7;

            buttonSpecialty.Text = "Specialty";

            buttonSpecialty.UseVisualStyleBackColor = true;

            buttonSpecialty.Click += ButtonSwitcher;

            // 

            // buttonGuildStorage

            // 

            buttonGuildStorage.Color = System.Drawing.Color.Transparent;

            buttonGuildStorage.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);

            buttonGuildStorage.Location = new System.Drawing.Point(594, 6);

            buttonGuildStorage.Name = "buttonGuildStorage";

            buttonGuildStorage.Radius = 6;

            buttonGuildStorage.ShadowDepth = 4F;

            buttonGuildStorage.Size = new System.Drawing.Size(78, 25);

            buttonGuildStorage.TabIndex = 5;

            buttonGuildStorage.Text = "Guild Storage";

            buttonGuildStorage.UseVisualStyleBackColor = true;

            buttonGuildStorage.Click += ButtonSwitcher;

            // 

            // buttonHappy

            // 

            buttonHappy.Color = System.Drawing.Color.Transparent;

            buttonHappy.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);

            buttonHappy.Location = new System.Drawing.Point(674, 6);

            buttonHappy.Name = "buttonHappy";

            buttonHappy.Radius = 6;

            buttonHappy.ShadowDepth = 4F;

            buttonHappy.Size = new System.Drawing.Size(44, 25);

            buttonHappy.TabIndex = 10;

            buttonHappy.Text = "Happy";

            buttonHappy.UseVisualStyleBackColor = true;

            buttonHappy.Click += ButtonSwitcher;

            // 
            // buttonStorageOrder
            // 
            buttonStorageOrder.Color = System.Drawing.Color.Transparent;
            buttonStorageOrder.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            buttonStorageOrder.Location = new System.Drawing.Point(720, 6);
            buttonStorageOrder.Name = "buttonStorageOrder";
            buttonStorageOrder.Radius = 6;
            buttonStorageOrder.ShadowDepth = 4F;
            buttonStorageOrder.Size = new System.Drawing.Size(65, 25);
            buttonStorageOrder.TabIndex = 11;
            buttonStorageOrder.Text = "Order";
            buttonStorageOrder.UseVisualStyleBackColor = true;
            buttonStorageOrder.Click += ButtonSwitcher;

            // 

            // buttonFellowPet

            // 

            buttonFellowPet.Color = System.Drawing.Color.Transparent;

            buttonFellowPet.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);

            buttonFellowPet.Location = new System.Drawing.Point(338, 6);

            buttonFellowPet.Name = "buttonFellowPet";

            buttonFellowPet.Radius = 6;

            buttonFellowPet.ShadowDepth = 4F;

            buttonFellowPet.Size = new System.Drawing.Size(62, 25);

            buttonFellowPet.TabIndex = 9;

            buttonFellowPet.Text = "Fellow Pet";

            buttonFellowPet.UseVisualStyleBackColor = true;

            buttonFellowPet.Click += ButtonSwitcher;

            // 

            // buttonJobTransport

            // 

            buttonJobTransport.Color = System.Drawing.Color.Transparent;

            buttonJobTransport.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);

            buttonJobTransport.Location = new System.Drawing.Point(402, 6);

            buttonJobTransport.Name = "buttonJobTransport";

            buttonJobTransport.Radius = 6;

            buttonJobTransport.ShadowDepth = 4F;

            buttonJobTransport.Size = new System.Drawing.Size(80, 25);

            buttonJobTransport.TabIndex = 6;

            buttonJobTransport.Text = "Job Transport";

            buttonJobTransport.UseVisualStyleBackColor = true;

            buttonJobTransport.Click += ButtonSwitcher;

            // 

            // buttonStorage

            // 

            buttonStorage.Color = System.Drawing.Color.Transparent;

            buttonStorage.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);

            buttonStorage.Location = new System.Drawing.Point(542, 6);

            buttonStorage.Name = "buttonStorage";

            buttonStorage.Radius = 6;

            buttonStorage.ShadowDepth = 4F;

            buttonStorage.Size = new System.Drawing.Size(50, 25);

            buttonStorage.TabIndex = 4;

            buttonStorage.Text = "Storage";

            buttonStorage.UseVisualStyleBackColor = true;

            buttonStorage.Click += ButtonSwitcher;

            // 

            // buttonJobEquipment

            // 

            buttonJobEquipment.Color = System.Drawing.Color.Transparent;

            buttonJobEquipment.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);

            buttonJobEquipment.Location = new System.Drawing.Point(138, 6);

            buttonJobEquipment.Name = "buttonJobEquipment";

            buttonJobEquipment.Radius = 6;

            buttonJobEquipment.ShadowDepth = 4F;

            buttonJobEquipment.Size = new System.Drawing.Size(88, 25);

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

            Controls.Add(panelStorageOrder);

            Controls.Add(panel1);

            Controls.Add(topPanel);

            Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);

            Name = "Main";

            Size = new System.Drawing.Size(792, 458);

            Load += Main_Load;

            VisibleChanged += Main_VisibleChanged;

            contextMenuStrip.ResumeLayout(false);

            panelStorageOrder.ResumeLayout(false);

            panelStorageOrder.PerformLayout();

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

                private SDUI.Controls.Button buttonStorageOrder;
        private SDUI.Controls.Panel panelStorageOrder;
        private SDUI.Controls.Label lblStorageOrderTitle;
        private SDUI.Controls.Label lblPagesHeader;
        private SDUI.Controls.Label lblPage1;
        private SDUI.Controls.Label lblPage2;
        private SDUI.Controls.Label lblPage3;
        private SDUI.Controls.Label lblPage4;
        private SDUI.Controls.Label lblPage5;
        private SDUI.Controls.Label lblOverflowNote;
        private SDUI.Controls.Label lblSettingsHeader;
        private SDUI.Controls.CheckBox checkStorageSplitPages;
        private SDUI.Controls.CheckBox checkStorageAutoOrder;
        private SDUI.Controls.CheckBox checkStorageAutoSort;
        private SDUI.Controls.CheckBox checkGuildStorageAutoOrder;
        private SDUI.Controls.CheckBox checkGuildStorageAutoSort;
        private SDUI.Controls.Label lblActionsHeader;
        private SDUI.Controls.Button btnOrderStorageNow;
        private SDUI.Controls.Button btnSortStorageNow;
        private SDUI.Controls.Button btnOrderGuildStorageNow;
        private SDUI.Controls.Label lblStorageOrderStatus;
        private SDUI.Controls.Panel panelHappy;

        private SDUI.Controls.Label lblHappyTitle;

        private SDUI.Controls.Label lblHappyCounts;

        private SDUI.Controls.Label lblHappyStatus;

        private SDUI.Controls.CheckBox checkAutoHappy;

        private SDUI.Controls.Button btnStartHappy;

        private SDUI.Controls.Button btnStopHappy;
        private SDUI.Controls.Label lblHappyDelay;
        private SDUI.Controls.NumUpDown numHappyDelay;

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

        private SDUI.Controls.Button buttonHappy;

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

        private SDUI.Controls.Button btnOrder;

        private SDUI.Controls.CheckBox checkAutoSort;

        private SDUI.Controls.CheckBox checkAutoOrder;

        private System.Windows.Forms.ToolStripMenuItem useItemAtTrainingPlaceMenuItem;

        private System.Windows.Forms.ToolStripMenuItem autoUseAccordingToPurposeToolStripMenuItem;

        private SDUI.Controls.ShapeProgressBar pbInventoryStatus;

    }

}

