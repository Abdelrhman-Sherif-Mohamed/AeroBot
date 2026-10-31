namespace Wolves.AvatarTester.Views
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
            pnlNav = new SDUI.Controls.Panel();
            btnTabAvatars = new SDUI.Controls.Button();
            btnTabSwitches = new SDUI.Controls.Button();

            // Avatars Tab Controls
            pnlAvatars = new SDUI.Controls.Panel();
            groupSearch = new SDUI.Controls.GroupBox();
            lblSearch = new SDUI.Controls.Label();
            txtSearch = new SDUI.Controls.TextBox();
            btnSearch = new SDUI.Controls.Button();
            lstSearchResults = new SDUI.Controls.ListView();
            colName = new System.Windows.Forms.ColumnHeader();
            colType = new System.Windows.Forms.ColumnHeader();
            colId = new System.Windows.Forms.ColumnHeader();
            colCode = new System.Windows.Forms.ColumnHeader();
            btnSetHat = new SDUI.Controls.Button();
            btnSetDress = new SDUI.Controls.Button();
            btnSetAccessory = new SDUI.Controls.Button();
            btnSetFlag = new SDUI.Controls.Button();

            groupSelected = new SDUI.Controls.GroupBox();
            lblHat = new SDUI.Controls.Label();
            lblSelectedHat = new SDUI.Controls.Label();
            btnRemoveHat = new SDUI.Controls.Button();
            lblDress = new SDUI.Controls.Label();
            lblSelectedDress = new SDUI.Controls.Label();
            btnRemoveDress = new SDUI.Controls.Button();
            lblAccessory = new SDUI.Controls.Label();
            lblSelectedAccessory = new SDUI.Controls.Label();
            btnRemoveAccessory = new SDUI.Controls.Button();
            lblFlag = new SDUI.Controls.Label();
            lblSelectedFlag = new SDUI.Controls.Label();
            btnRemoveFlag = new SDUI.Controls.Button();
            btnSpawnPreview = new SDUI.Controls.Button();
            btnClearPreview = new SDUI.Controls.Button();
            lblPreviewStatus = new SDUI.Controls.Label();

            // Switches Tab Controls
            pnlSwitches = new SDUI.Controls.Panel();
            groupSwitches = new SDUI.Controls.GroupBox();
            btnFilterAll = new SDUI.Controls.Button();
            btnFilterWepGlow = new SDUI.Controls.Button();
            btnFilterShGlow = new SDUI.Controls.Button();
            btnFilterD1D16 = new SDUI.Controls.Button();
            btnFilterSpecial = new SDUI.Controls.Button();
            lblSwitchSearch = new SDUI.Controls.Label();
            txtSwitchSearch = new SDUI.Controls.TextBox();
            btnSwitchSearch = new SDUI.Controls.Button();
            lblSwitchCount = new SDUI.Controls.Label();
            lstSwitches = new SDUI.Controls.ListView();
            colSwName = new System.Windows.Forms.ColumnHeader();
            colSwCategory = new System.Windows.Forms.ColumnHeader();
            colSwId = new System.Windows.Forms.ColumnHeader();
            colSwCode = new System.Windows.Forms.ColumnHeader();
            btnTestWeapon = new SDUI.Controls.Button();
            btnTestShield = new SDUI.Controls.Button();
            lblOptLevel = new SDUI.Controls.Label();
            cmbOptLevel = new SDUI.Controls.ComboBox();
            btnUseFromInv = new SDUI.Controls.Button();
            btnRestoreWep = new SDUI.Controls.Button();
            lblSwitchStatus = new SDUI.Controls.Label();

            pnlNav.SuspendLayout();
            pnlAvatars.SuspendLayout();
            groupSearch.SuspendLayout();
            groupSelected.SuspendLayout();
            pnlSwitches.SuspendLayout();
            groupSwitches.SuspendLayout();
            SuspendLayout();

            // 
            // pnlNav
            // 
            pnlNav.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            pnlNav.Controls.Add(btnTabAvatars);
            pnlNav.Controls.Add(btnTabSwitches);
            pnlNav.Location = new System.Drawing.Point(10, 6);
            pnlNav.Name = "pnlNav";
            pnlNav.Size = new System.Drawing.Size(960, 36);
            pnlNav.TabIndex = 0;

            // 
            // btnTabAvatars
            // 
            btnTabAvatars.Location = new System.Drawing.Point(4, 2);
            btnTabAvatars.Name = "btnTabAvatars";
            btnTabAvatars.Size = new System.Drawing.Size(185, 32);
            btnTabAvatars.TabIndex = 0;
            btnTabAvatars.Text = "👕 الأزياء والافتارات (Avatars)";
            btnTabAvatars.UseVisualStyleBackColor = true;
            btnTabAvatars.Click += btnTabAvatars_Click;

            // 
            // btnTabSwitches
            // 
            btnTabSwitches.Location = new System.Drawing.Point(195, 2);
            btnTabSwitches.Name = "btnTabSwitches";
            btnTabSwitches.Size = new System.Drawing.Size(235, 32);
            btnTabSwitches.TabIndex = 1;
            btnTabSwitches.Text = "⚔️ سوتشات السلاح والتأثيرات (Switches)";
            btnTabSwitches.UseVisualStyleBackColor = true;
            btnTabSwitches.Click += btnTabSwitches_Click;

            // 
            // pnlAvatars
            // 
            pnlAvatars.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            pnlAvatars.Controls.Add(groupSearch);
            pnlAvatars.Controls.Add(groupSelected);
            pnlAvatars.Location = new System.Drawing.Point(10, 46);
            pnlAvatars.Name = "pnlAvatars";
            pnlAvatars.Size = new System.Drawing.Size(960, 416);
            pnlAvatars.TabIndex = 1;

            // 
            // groupSearch
            // 
            groupSearch.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            groupSearch.Controls.Add(lblSearch);
            groupSearch.Controls.Add(txtSearch);
            groupSearch.Controls.Add(btnSearch);
            groupSearch.Controls.Add(lstSearchResults);
            groupSearch.Controls.Add(btnSetHat);
            groupSearch.Controls.Add(btnSetDress);
            groupSearch.Controls.Add(btnSetAccessory);
            groupSearch.Controls.Add(btnSetFlag);
            groupSearch.Location = new System.Drawing.Point(0, 0);
            groupSearch.Name = "groupSearch";
            groupSearch.Size = new System.Drawing.Size(960, 246);
            groupSearch.TabIndex = 0;
            groupSearch.TabStop = false;
            groupSearch.Text = "البحث عن أزياء وافتارات (Avatar Search)";

            // 
            // lblSearch
            // 
            lblSearch.AutoSize = true;
            lblSearch.Location = new System.Drawing.Point(12, 26);
            lblSearch.Name = "lblSearch";
            lblSearch.Size = new System.Drawing.Size(35, 15);
            lblSearch.TabIndex = 0;
            lblSearch.Text = "بحث:";

            // 
            // txtSearch
            // 
            txtSearch.Location = new System.Drawing.Point(55, 23);
            txtSearch.Name = "txtSearch";
            txtSearch.Size = new System.Drawing.Size(240, 23);
            txtSearch.TabIndex = 1;

            // 
            // btnSearch
            // 
            btnSearch.Location = new System.Drawing.Point(305, 22);
            btnSearch.Name = "btnSearch";
            btnSearch.Size = new System.Drawing.Size(75, 24);
            btnSearch.TabIndex = 2;
            btnSearch.Text = "بحث";
            btnSearch.UseVisualStyleBackColor = true;
            btnSearch.Click += btnSearch_Click;

            // 
            // lstSearchResults
            // 
            lstSearchResults.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            lstSearchResults.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] { colName, colType, colId, colCode });
            lstSearchResults.FullRowSelect = true;
            lstSearchResults.Location = new System.Drawing.Point(12, 54);
            lstSearchResults.Name = "lstSearchResults";
            lstSearchResults.Size = new System.Drawing.Size(936, 152);
            lstSearchResults.TabIndex = 3;
            lstSearchResults.UseCompatibleStateImageBehavior = false;
            lstSearchResults.View = System.Windows.Forms.View.Details;
            lstSearchResults.DoubleClick += lstSearchResults_DoubleClick;

            colName.Text = "الأداة / الاسم";
            colName.Width = 300;
            colType.Text = "نوع القطعة";
            colType.Width = 140;
            colId.Text = "ID";
            colId.Width = 75;
            colCode.Text = "كود الأداة (Code Name)";
            colCode.Width = 400;

            // 
            // btnSetHat
            // 
            btnSetHat.Location = new System.Drawing.Point(12, 212);
            btnSetHat.Name = "btnSetHat";
            btnSetHat.Size = new System.Drawing.Size(115, 26);
            btnSetHat.TabIndex = 4;
            btnSetHat.Text = "تعيين كـ قبعة";
            btnSetHat.UseVisualStyleBackColor = true;
            btnSetHat.Click += btnSetHat_Click;

            // 
            // btnSetDress
            // 
            btnSetDress.Location = new System.Drawing.Point(132, 212);
            btnSetDress.Name = "btnSetDress";
            btnSetDress.Size = new System.Drawing.Size(115, 26);
            btnSetDress.TabIndex = 5;
            btnSetDress.Text = "تعيين كـ فستان";
            btnSetDress.UseVisualStyleBackColor = true;
            btnSetDress.Click += btnSetDress_Click;

            // 
            // btnSetAccessory
            // 
            btnSetAccessory.Location = new System.Drawing.Point(252, 212);
            btnSetAccessory.Name = "btnSetAccessory";
            btnSetAccessory.Size = new System.Drawing.Size(115, 26);
            btnSetAccessory.TabIndex = 6;
            btnSetAccessory.Text = "تعيين كـ ملحق";
            btnSetAccessory.UseVisualStyleBackColor = true;
            btnSetAccessory.Click += btnSetAccessory_Click;

            // 
            // btnSetFlag
            // 
            btnSetFlag.Location = new System.Drawing.Point(372, 212);
            btnSetFlag.Name = "btnSetFlag";
            btnSetFlag.Size = new System.Drawing.Size(115, 26);
            btnSetFlag.TabIndex = 7;
            btnSetFlag.Text = "تعيين كـ علم";
            btnSetFlag.UseVisualStyleBackColor = true;
            btnSetFlag.Click += btnSetFlag_Click;

            // 
            // groupSelected
            // 
            groupSelected.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            groupSelected.Controls.Add(lblHat);
            groupSelected.Controls.Add(lblSelectedHat);
            groupSelected.Controls.Add(btnRemoveHat);
            groupSelected.Controls.Add(lblDress);
            groupSelected.Controls.Add(lblSelectedDress);
            groupSelected.Controls.Add(btnRemoveDress);
            groupSelected.Controls.Add(lblAccessory);
            groupSelected.Controls.Add(lblSelectedAccessory);
            groupSelected.Controls.Add(btnRemoveAccessory);
            groupSelected.Controls.Add(lblFlag);
            groupSelected.Controls.Add(lblSelectedFlag);
            groupSelected.Controls.Add(btnRemoveFlag);
            groupSelected.Controls.Add(btnSpawnPreview);
            groupSelected.Controls.Add(btnClearPreview);
            groupSelected.Controls.Add(lblPreviewStatus);
            groupSelected.Location = new System.Drawing.Point(0, 252);
            groupSelected.Name = "groupSelected";
            groupSelected.Size = new System.Drawing.Size(960, 160);
            groupSelected.TabIndex = 1;
            groupSelected.TabStop = false;
            groupSelected.Text = "معاينة وتركيب الافتار على الشخصية";

            // Hat
            lblHat.Location = new System.Drawing.Point(12, 22);
            lblHat.Name = "lblHat";
            lblHat.Size = new System.Drawing.Size(60, 20);
            lblHat.Text = "القبعة:";
            lblSelectedHat.Location = new System.Drawing.Point(75, 22);
            lblSelectedHat.Name = "lblSelectedHat";
            lblSelectedHat.Size = new System.Drawing.Size(210, 20);
            lblSelectedHat.Text = "(لا يوجد)";
            btnRemoveHat.Location = new System.Drawing.Point(290, 19);
            btnRemoveHat.Name = "btnRemoveHat";
            btnRemoveHat.Size = new System.Drawing.Size(50, 24);
            btnRemoveHat.Text = "مسح";
            btnRemoveHat.Click += (s, e) => { AvatarTesterManager.Instance.SelectedHatId = 0; AvatarTesterManager.Instance.UnequipAvatarFromPlayer(0); UpdateUI(); };

            // Dress
            lblDress.Location = new System.Drawing.Point(360, 22);
            lblDress.Name = "lblDress";
            lblDress.Size = new System.Drawing.Size(60, 20);
            lblDress.Text = "الفستان:";
            lblSelectedDress.Location = new System.Drawing.Point(425, 22);
            lblSelectedDress.Name = "lblSelectedDress";
            lblSelectedDress.Size = new System.Drawing.Size(210, 20);
            lblSelectedDress.Text = "(لا يوجد)";
            btnRemoveDress.Location = new System.Drawing.Point(640, 19);
            btnRemoveDress.Name = "btnRemoveDress";
            btnRemoveDress.Size = new System.Drawing.Size(50, 24);
            btnRemoveDress.Text = "مسح";
            btnRemoveDress.Click += (s, e) => { AvatarTesterManager.Instance.SelectedDressId = 0; AvatarTesterManager.Instance.UnequipAvatarFromPlayer(1); UpdateUI(); };

            // Accessory
            lblAccessory.Location = new System.Drawing.Point(12, 52);
            lblAccessory.Name = "lblAccessory";
            lblAccessory.Size = new System.Drawing.Size(60, 20);
            lblAccessory.Text = "الملحق:";
            lblSelectedAccessory.Location = new System.Drawing.Point(75, 52);
            lblSelectedAccessory.Name = "lblSelectedAccessory";
            lblSelectedAccessory.Size = new System.Drawing.Size(210, 20);
            lblSelectedAccessory.Text = "(لا يوجد)";
            btnRemoveAccessory.Location = new System.Drawing.Point(290, 49);
            btnRemoveAccessory.Name = "btnRemoveAccessory";
            btnRemoveAccessory.Size = new System.Drawing.Size(50, 24);
            btnRemoveAccessory.Text = "مسح";
            btnRemoveAccessory.Click += (s, e) => { AvatarTesterManager.Instance.SelectedAccessoryId = 0; AvatarTesterManager.Instance.UnequipAvatarFromPlayer(2); UpdateUI(); };

            // Flag
            lblFlag.Location = new System.Drawing.Point(360, 52);
            lblFlag.Name = "lblFlag";
            lblFlag.Size = new System.Drawing.Size(60, 20);
            lblFlag.Text = "العلم:";
            lblSelectedFlag.Location = new System.Drawing.Point(425, 52);
            lblSelectedFlag.Name = "lblSelectedFlag";
            lblSelectedFlag.Size = new System.Drawing.Size(210, 20);
            lblSelectedFlag.Text = "(لا يوجد)";
            btnRemoveFlag.Location = new System.Drawing.Point(640, 49);
            btnRemoveFlag.Name = "btnRemoveFlag";
            btnRemoveFlag.Size = new System.Drawing.Size(50, 24);
            btnRemoveFlag.Text = "مسح";
            btnRemoveFlag.Click += (s, e) => { AvatarTesterManager.Instance.SelectedFlagId = 0; AvatarTesterManager.Instance.UnequipAvatarFromPlayer(3); UpdateUI(); };

            // Preview buttons
            btnSpawnPreview.Location = new System.Drawing.Point(12, 84);
            btnSpawnPreview.Name = "btnSpawnPreview";
            btnSpawnPreview.Size = new System.Drawing.Size(170, 30);
            btnSpawnPreview.TabIndex = 8;
            btnSpawnPreview.Text = "تطبيق على الشخصية";
            btnSpawnPreview.UseVisualStyleBackColor = true;
            btnSpawnPreview.Click += btnSpawnPreview_Click;

            btnClearPreview.Location = new System.Drawing.Point(190, 84);
            btnClearPreview.Name = "btnClearPreview";
            btnClearPreview.Size = new System.Drawing.Size(170, 30);
            btnClearPreview.TabIndex = 9;
            btnClearPreview.Text = "مسح الكل (استعادة الأصل)";
            btnClearPreview.UseVisualStyleBackColor = true;
            btnClearPreview.Click += btnClearPreview_Click;

            lblPreviewStatus.Location = new System.Drawing.Point(370, 90);
            lblPreviewStatus.Name = "lblPreviewStatus";
            lblPreviewStatus.Size = new System.Drawing.Size(330, 20);
            lblPreviewStatus.Text = "الحالة: المظهر الأصلي للشخصية";

            // 
            // pnlSwitches
            // 
            pnlSwitches.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            pnlSwitches.Controls.Add(groupSwitches);
            pnlSwitches.Location = new System.Drawing.Point(10, 46);
            pnlSwitches.Name = "pnlSwitches";
            pnlSwitches.Size = new System.Drawing.Size(960, 416);
            pnlSwitches.TabIndex = 2;
            pnlSwitches.Visible = false;

            // 
            // groupSwitches
            // 
            groupSwitches.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            groupSwitches.Controls.Add(btnFilterAll);
            groupSwitches.Controls.Add(btnFilterWepGlow);
            groupSwitches.Controls.Add(btnFilterShGlow);
            groupSwitches.Controls.Add(btnFilterD1D16);
            groupSwitches.Controls.Add(btnFilterSpecial);
            groupSwitches.Controls.Add(lblSwitchSearch);
            groupSwitches.Controls.Add(txtSwitchSearch);
            groupSwitches.Controls.Add(btnSwitchSearch);
            groupSwitches.Controls.Add(lblSwitchCount);
            groupSwitches.Controls.Add(lstSwitches);
            groupSwitches.Controls.Add(btnTestWeapon);
            groupSwitches.Controls.Add(btnTestShield);
            groupSwitches.Controls.Add(lblOptLevel);
            groupSwitches.Controls.Add(cmbOptLevel);
            groupSwitches.Controls.Add(btnUseFromInv);
            groupSwitches.Controls.Add(btnRestoreWep);
            groupSwitches.Controls.Add(lblSwitchStatus);
            groupSwitches.Location = new System.Drawing.Point(0, 0);
            groupSwitches.Name = "groupSwitches";
            groupSwitches.Size = new System.Drawing.Size(960, 412);
            groupSwitches.TabIndex = 0;
            groupSwitches.TabStop = false;
            groupSwitches.Text = "سوتشات وتأثيرات الأسلحة والدروع (Weapon & Shield Switches)";

            // Filter Buttons
            btnFilterAll.Location = new System.Drawing.Point(12, 20);
            btnFilterAll.Name = "btnFilterAll";
            btnFilterAll.Size = new System.Drawing.Size(65, 26);
            btnFilterAll.TabIndex = 0;
            btnFilterAll.Text = "الكل";
            btnFilterAll.UseVisualStyleBackColor = true;
            btnFilterAll.Click += btnFilterAll_Click;

            btnFilterWepGlow.Location = new System.Drawing.Point(82, 20);
            btnFilterWepGlow.Name = "btnFilterWepGlow";
            btnFilterWepGlow.Size = new System.Drawing.Size(115, 26);
            btnFilterWepGlow.TabIndex = 1;
            btnFilterWepGlow.Text = "✨ توهج سلاح";
            btnFilterWepGlow.UseVisualStyleBackColor = true;
            btnFilterWepGlow.Click += btnFilterWepGlow_Click;

            btnFilterShGlow.Location = new System.Drawing.Point(202, 20);
            btnFilterShGlow.Name = "btnFilterShGlow";
            btnFilterShGlow.Size = new System.Drawing.Size(115, 26);
            btnFilterShGlow.TabIndex = 2;
            btnFilterShGlow.Text = "🛡️ توهج درع";
            btnFilterShGlow.UseVisualStyleBackColor = true;
            btnFilterShGlow.Click += btnFilterShGlow_Click;

            btnFilterD1D16.Location = new System.Drawing.Point(322, 20);
            btnFilterD1D16.Name = "btnFilterD1D16";
            btnFilterD1D16.Size = new System.Drawing.Size(125, 26);
            btnFilterD1D16.TabIndex = 3;
            btnFilterD1D16.Text = "⚔️ سوتشر D1-D16";
            btnFilterD1D16.UseVisualStyleBackColor = true;
            btnFilterD1D16.Click += btnFilterD1D16_Click;

            btnFilterSpecial.Location = new System.Drawing.Point(452, 20);
            btnFilterSpecial.Name = "btnFilterSpecial";
            btnFilterSpecial.Size = new System.Drawing.Size(115, 26);
            btnFilterSpecial.TabIndex = 4;
            btnFilterSpecial.Text = "🌟 سوتشر مميز";
            btnFilterSpecial.UseVisualStyleBackColor = true;
            btnFilterSpecial.Click += btnFilterSpecial_Click;

            // Switch Search
            lblSwitchSearch.AutoSize = true;
            lblSwitchSearch.Location = new System.Drawing.Point(12, 55);
            lblSwitchSearch.Name = "lblSwitchSearch";
            lblSwitchSearch.Size = new System.Drawing.Size(35, 15);
            lblSwitchSearch.TabIndex = 5;
            lblSwitchSearch.Text = "بحث:";

            txtSwitchSearch.Location = new System.Drawing.Point(52, 52);
            txtSwitchSearch.Name = "txtSwitchSearch";
            txtSwitchSearch.Size = new System.Drawing.Size(240, 23);
            txtSwitchSearch.TabIndex = 6;
            txtSwitchSearch.KeyDown += txtSwitchSearch_KeyDown;

            btnSwitchSearch.Location = new System.Drawing.Point(298, 52);
            btnSwitchSearch.Name = "btnSwitchSearch";
            btnSwitchSearch.Size = new System.Drawing.Size(75, 24);
            btnSwitchSearch.TabIndex = 7;
            btnSwitchSearch.Text = "بحث";
            btnSwitchSearch.UseVisualStyleBackColor = true;
            btnSwitchSearch.Click += btnSwitchSearch_Click;

            lblSwitchCount.Location = new System.Drawing.Point(385, 55);
            lblSwitchCount.Name = "lblSwitchCount";
            lblSwitchCount.Size = new System.Drawing.Size(200, 20);
            lblSwitchCount.Text = "العدد: 0 أداة";

            // lstSwitches
            lstSwitches.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            lstSwitches.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] { colSwName, colSwCategory, colSwId, colSwCode });
            lstSwitches.FullRowSelect = true;
            lstSwitches.Location = new System.Drawing.Point(12, 82);
            lstSwitches.Name = "lstSwitches";
            lstSwitches.Size = new System.Drawing.Size(936, 240);
            lstSwitches.TabIndex = 8;
            lstSwitches.UseCompatibleStateImageBehavior = false;
            lstSwitches.View = System.Windows.Forms.View.Details;
            lstSwitches.DoubleClick += lstSwitches_DoubleClick;

            colSwName.Text = "الأداة / الاسم";
            colSwName.Width = 320;
            colSwCategory.Text = "القسم / النوع";
            colSwCategory.Width = 180;
            colSwId.Text = "ID";
            colSwId.Width = 75;
            colSwCode.Text = "كود الأداة (Code Name)";
            colSwCode.Width = 340;

            // Actions
            btnTestWeapon.Location = new System.Drawing.Point(12, 328);
            btnTestWeapon.Name = "btnTestWeapon";
            btnTestWeapon.Size = new System.Drawing.Size(125, 28);
            btnTestWeapon.TabIndex = 9;
            btnTestWeapon.Text = "معاينة على السلاح";
            btnTestWeapon.UseVisualStyleBackColor = true;
            btnTestWeapon.Click += btnTestWeapon_Click;

            btnTestShield.Location = new System.Drawing.Point(142, 328);
            btnTestShield.Name = "btnTestShield";
            btnTestShield.Size = new System.Drawing.Size(125, 28);
            btnTestShield.TabIndex = 10;
            btnTestShield.Text = "معاينة على الدرع";
            btnTestShield.UseVisualStyleBackColor = true;
            btnTestShield.Click += btnTestShield_Click;

            lblOptLevel.AutoSize = true;
            lblOptLevel.Location = new System.Drawing.Point(274, 333);
            lblOptLevel.Name = "lblOptLevel";
            lblOptLevel.Size = new System.Drawing.Size(35, 15);
            lblOptLevel.TabIndex = 11;
            lblOptLevel.Text = "بلس:";

            cmbOptLevel.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            cmbOptLevel.Items.AddRange(new object[] { "+0", "+7", "+9", "+11", "+12", "+15", "+16" });
            cmbOptLevel.Location = new System.Drawing.Point(312, 330);
            cmbOptLevel.Name = "cmbOptLevel";
            cmbOptLevel.Size = new System.Drawing.Size(65, 23);
            cmbOptLevel.TabIndex = 12;

            btnUseFromInv.Location = new System.Drawing.Point(385, 328);
            btnUseFromInv.Name = "btnUseFromInv";
            btnUseFromInv.Size = new System.Drawing.Size(130, 28);
            btnUseFromInv.TabIndex = 13;
            btnUseFromInv.Text = "استخدام من الحقيبة";
            btnUseFromInv.UseVisualStyleBackColor = true;
            btnUseFromInv.Click += btnUseFromInv_Click;

            btnRestoreWep.Location = new System.Drawing.Point(522, 328);
            btnRestoreWep.Name = "btnRestoreWep";
            btnRestoreWep.Size = new System.Drawing.Size(105, 28);
            btnRestoreWep.TabIndex = 14;
            btnRestoreWep.Text = "استعادة الأصل";
            btnRestoreWep.UseVisualStyleBackColor = true;
            btnRestoreWep.Click += btnRestoreWep_Click;

            lblSwitchStatus.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            lblSwitchStatus.Location = new System.Drawing.Point(12, 362);
            lblSwitchStatus.Name = "lblSwitchStatus";
            lblSwitchStatus.Size = new System.Drawing.Size(936, 20);
            lblSwitchStatus.Text = "الحالة: جاهز للمعاينة";

            // 
            // Main
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            Controls.Add(pnlNav);
            Controls.Add(pnlAvatars);
            Controls.Add(pnlSwitches);
            Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            Name = "Main";
            Size = new System.Drawing.Size(980, 468);
            Load += Main_Load;
            pnlNav.ResumeLayout(false);
            pnlAvatars.ResumeLayout(false);
            groupSearch.ResumeLayout(false);
            groupSearch.PerformLayout();
            groupSelected.ResumeLayout(false);
            pnlSwitches.ResumeLayout(false);
            groupSwitches.ResumeLayout(false);
            groupSwitches.PerformLayout();
            ResumeLayout(false);
        }

        #endregion

        private SDUI.Controls.Panel pnlNav;
        private SDUI.Controls.Button btnTabAvatars;
        private SDUI.Controls.Button btnTabSwitches;

        // Avatars Tab
        private SDUI.Controls.Panel pnlAvatars;
        private SDUI.Controls.GroupBox groupSearch;
        private SDUI.Controls.Label lblSearch;
        private SDUI.Controls.TextBox txtSearch;
        private SDUI.Controls.Button btnSearch;
        private SDUI.Controls.ListView lstSearchResults;
        private System.Windows.Forms.ColumnHeader colName;
        private System.Windows.Forms.ColumnHeader colType;
        private System.Windows.Forms.ColumnHeader colId;
        private System.Windows.Forms.ColumnHeader colCode;
        private SDUI.Controls.Button btnSetHat;
        private SDUI.Controls.Button btnSetDress;
        private SDUI.Controls.Button btnSetAccessory;
        private SDUI.Controls.Button btnSetFlag;

        private SDUI.Controls.GroupBox groupSelected;
        private SDUI.Controls.Label lblHat;
        private SDUI.Controls.Label lblSelectedHat;
        private SDUI.Controls.Button btnRemoveHat;
        private SDUI.Controls.Label lblDress;
        private SDUI.Controls.Label lblSelectedDress;
        private SDUI.Controls.Button btnRemoveDress;
        private SDUI.Controls.Label lblAccessory;
        private SDUI.Controls.Label lblSelectedAccessory;
        private SDUI.Controls.Button btnRemoveAccessory;
        private SDUI.Controls.Label lblFlag;
        private SDUI.Controls.Label lblSelectedFlag;
        private SDUI.Controls.Button btnRemoveFlag;
        private SDUI.Controls.Button btnSpawnPreview;
        private SDUI.Controls.Button btnClearPreview;
        private SDUI.Controls.Label lblPreviewStatus;

        // Switches Tab
        private SDUI.Controls.Panel pnlSwitches;
        private SDUI.Controls.GroupBox groupSwitches;
        private SDUI.Controls.Button btnFilterAll;
        private SDUI.Controls.Button btnFilterWepGlow;
        private SDUI.Controls.Button btnFilterShGlow;
        private SDUI.Controls.Button btnFilterD1D16;
        private SDUI.Controls.Button btnFilterSpecial;
        private SDUI.Controls.Label lblSwitchSearch;
        private SDUI.Controls.TextBox txtSwitchSearch;
        private SDUI.Controls.Button btnSwitchSearch;
        private SDUI.Controls.Label lblSwitchCount;
        private SDUI.Controls.ListView lstSwitches;
        private System.Windows.Forms.ColumnHeader colSwName;
        private System.Windows.Forms.ColumnHeader colSwCategory;
        private System.Windows.Forms.ColumnHeader colSwId;
        private System.Windows.Forms.ColumnHeader colSwCode;
        private SDUI.Controls.Button btnTestWeapon;
        private SDUI.Controls.Button btnTestShield;
        private SDUI.Controls.Label lblOptLevel;
        private SDUI.Controls.ComboBox cmbOptLevel;
        private SDUI.Controls.Button btnUseFromInv;
        private SDUI.Controls.Button btnRestoreWep;
        private SDUI.Controls.Label lblSwitchStatus;
    }
}
