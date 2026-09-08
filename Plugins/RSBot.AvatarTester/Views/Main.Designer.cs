namespace RSBot.AvatarTester.Views
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
            groupSearch = new SDUI.Controls.GroupBox();
            lblSearch = new SDUI.Controls.Label();
            txtSearch = new SDUI.Controls.TextBox();
            btnSearch = new SDUI.Controls.Button();
            lstSearchResults = new SDUI.Controls.ListView();
            colId = new System.Windows.Forms.ColumnHeader();
            colType = new System.Windows.Forms.ColumnHeader();
            colName = new System.Windows.Forms.ColumnHeader();
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

            groupSearch.SuspendLayout();
            groupSelected.SuspendLayout();
            SuspendLayout();

            // groupSearch
            groupSearch.Controls.Add(lblSearch);
            groupSearch.Controls.Add(txtSearch);
            groupSearch.Controls.Add(btnSearch);
            groupSearch.Controls.Add(lstSearchResults);
            groupSearch.Controls.Add(btnSetHat);
            groupSearch.Controls.Add(btnSetDress);
            groupSearch.Controls.Add(btnSetAccessory);
            groupSearch.Controls.Add(btnSetFlag);
            groupSearch.Location = new System.Drawing.Point(12, 12);
            groupSearch.Name = "groupSearch";
            groupSearch.Size = new System.Drawing.Size(726, 250);
            groupSearch.TabIndex = 0;
            groupSearch.TabStop = false;
            groupSearch.Text = "Avatar Item Search";

            // lblSearch
            lblSearch.AutoSize = true;
            lblSearch.Location = new System.Drawing.Point(15, 25);
            lblSearch.Name = "lblSearch";
            lblSearch.Size = new System.Drawing.Size(45, 15);
            lblSearch.TabIndex = 0;
            lblSearch.Text = "Search:";

            // txtSearch
            txtSearch.Location = new System.Drawing.Point(65, 22);
            txtSearch.Name = "txtSearch";
            txtSearch.Size = new System.Drawing.Size(250, 23);
            txtSearch.TabIndex = 1;

            // btnSearch
            btnSearch.Location = new System.Drawing.Point(325, 22);
            btnSearch.Name = "btnSearch";
            btnSearch.Size = new System.Drawing.Size(75, 24);
            btnSearch.TabIndex = 2;
            btnSearch.Text = "Search";
            btnSearch.UseVisualStyleBackColor = true;
            btnSearch.Click += btnSearch_Click;

            // lstSearchResults
            lstSearchResults.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] { colId, colType, colName, colCode });
            lstSearchResults.FullRowSelect = true;
            lstSearchResults.Location = new System.Drawing.Point(15, 55);
            lstSearchResults.Name = "lstSearchResults";
            lstSearchResults.Size = new System.Drawing.Size(695, 150);
            lstSearchResults.TabIndex = 3;
            lstSearchResults.UseCompatibleStateImageBehavior = false;
            lstSearchResults.View = System.Windows.Forms.View.Details;

            colId.Text = "ID";
            colId.Width = 60;
            colType.Text = "Slot Type";
            colType.Width = 90;
            colName.Text = "Real Name";
            colName.Width = 240;
            colCode.Text = "Code Name";
            colCode.Width = 280;

            // btnSetHat
            btnSetHat.Location = new System.Drawing.Point(15, 212);
            btnSetHat.Name = "btnSetHat";
            btnSetHat.Size = new System.Drawing.Size(120, 26);
            btnSetHat.TabIndex = 4;
            btnSetHat.Text = "Set as Hat";
            btnSetHat.UseVisualStyleBackColor = true;
            btnSetHat.Click += btnSetHat_Click;

            // btnSetDress
            btnSetDress.Location = new System.Drawing.Point(145, 212);
            btnSetDress.Name = "btnSetDress";
            btnSetDress.Size = new System.Drawing.Size(120, 26);
            btnSetDress.TabIndex = 5;
            btnSetDress.Text = "Set as Dress";
            btnSetDress.UseVisualStyleBackColor = true;
            btnSetDress.Click += btnSetDress_Click;

            // btnSetAccessory
            btnSetAccessory.Location = new System.Drawing.Point(275, 212);
            btnSetAccessory.Name = "btnSetAccessory";
            btnSetAccessory.Size = new System.Drawing.Size(120, 26);
            btnSetAccessory.TabIndex = 6;
            btnSetAccessory.Text = "Set as Accessory";
            btnSetAccessory.UseVisualStyleBackColor = true;
            btnSetAccessory.Click += btnSetAccessory_Click;

            // btnSetFlag
            btnSetFlag.Location = new System.Drawing.Point(405, 212);
            btnSetFlag.Name = "btnSetFlag";
            btnSetFlag.Size = new System.Drawing.Size(120, 26);
            btnSetFlag.TabIndex = 7;
            btnSetFlag.Text = "Set as Flag";
            btnSetFlag.UseVisualStyleBackColor = true;
            btnSetFlag.Click += btnSetFlag_Click;

            // groupSelected
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
            groupSelected.Location = new System.Drawing.Point(12, 268);
            groupSelected.Name = "groupSelected";
            groupSelected.Size = new System.Drawing.Size(726, 175);
            groupSelected.TabIndex = 1;
            groupSelected.TabStop = false;
            groupSelected.Text = "Avatar Assembly & Preview";

            // Hat
            lblHat.Location = new System.Drawing.Point(15, 25);
            lblHat.Name = "lblHat";
            lblHat.Size = new System.Drawing.Size(70, 20);
            lblHat.Text = "Hat:";
            lblSelectedHat.Location = new System.Drawing.Point(90, 25);
            lblSelectedHat.Name = "lblSelectedHat";
            lblSelectedHat.Size = new System.Drawing.Size(200, 20);
            lblSelectedHat.Text = "(None)";
            btnRemoveHat.Location = new System.Drawing.Point(295, 22);
            btnRemoveHat.Name = "btnRemoveHat";
            btnRemoveHat.Size = new System.Drawing.Size(55, 23);
            btnRemoveHat.Text = "Clear";
            btnRemoveHat.Click += (s, e) => { AvatarTesterManager.Instance.SelectedHatId = 0; UpdateUI(); };

            // Dress
            lblDress.Location = new System.Drawing.Point(370, 25);
            lblDress.Name = "lblDress";
            lblDress.Size = new System.Drawing.Size(70, 20);
            lblDress.Text = "Dress:";
            lblSelectedDress.Location = new System.Drawing.Point(445, 25);
            lblSelectedDress.Name = "lblSelectedDress";
            lblSelectedDress.Size = new System.Drawing.Size(200, 20);
            lblSelectedDress.Text = "(None)";
            btnRemoveDress.Location = new System.Drawing.Point(650, 22);
            btnRemoveDress.Name = "btnRemoveDress";
            btnRemoveDress.Size = new System.Drawing.Size(55, 23);
            btnRemoveDress.Text = "Clear";
            btnRemoveDress.Click += (s, e) => { AvatarTesterManager.Instance.SelectedDressId = 0; UpdateUI(); };

            // Accessory
            lblAccessory.Location = new System.Drawing.Point(15, 60);
            lblAccessory.Name = "lblAccessory";
            lblAccessory.Size = new System.Drawing.Size(70, 20);
            lblAccessory.Text = "Accessory:";
            lblSelectedAccessory.Location = new System.Drawing.Point(90, 60);
            lblSelectedAccessory.Name = "lblSelectedAccessory";
            lblSelectedAccessory.Size = new System.Drawing.Size(200, 20);
            lblSelectedAccessory.Text = "(None)";
            btnRemoveAccessory.Location = new System.Drawing.Point(295, 57);
            btnRemoveAccessory.Name = "btnRemoveAccessory";
            btnRemoveAccessory.Size = new System.Drawing.Size(55, 23);
            btnRemoveAccessory.Text = "Clear";
            btnRemoveAccessory.Click += (s, e) => { AvatarTesterManager.Instance.SelectedAccessoryId = 0; UpdateUI(); };

            // Flag
            lblFlag.Location = new System.Drawing.Point(370, 60);
            lblFlag.Name = "lblFlag";
            lblFlag.Size = new System.Drawing.Size(70, 20);
            lblFlag.Text = "Flag:";
            lblSelectedFlag.Location = new System.Drawing.Point(445, 60);
            lblSelectedFlag.Name = "lblSelectedFlag";
            lblSelectedFlag.Size = new System.Drawing.Size(200, 20);
            lblSelectedFlag.Text = "(None)";
            btnRemoveFlag.Location = new System.Drawing.Point(650, 57);
            btnRemoveFlag.Name = "btnRemoveFlag";
            btnRemoveFlag.Size = new System.Drawing.Size(55, 23);
            btnRemoveFlag.Text = "Clear";
            btnRemoveFlag.Click += (s, e) => { AvatarTesterManager.Instance.SelectedFlagId = 0; UpdateUI(); };

            // Preview buttons
            btnSpawnPreview.Location = new System.Drawing.Point(15, 110);
            btnSpawnPreview.Name = "btnSpawnPreview";
            btnSpawnPreview.Size = new System.Drawing.Size(180, 32);
            btnSpawnPreview.TabIndex = 8;
            btnSpawnPreview.Text = "Spawn Avatar Preview";
            btnSpawnPreview.UseVisualStyleBackColor = true;
            btnSpawnPreview.Click += btnSpawnPreview_Click;

            btnClearPreview.Location = new System.Drawing.Point(210, 110);
            btnClearPreview.Name = "btnClearPreview";
            btnClearPreview.Size = new System.Drawing.Size(130, 32);
            btnClearPreview.TabIndex = 9;
            btnClearPreview.Text = "Clear Preview";
            btnClearPreview.UseVisualStyleBackColor = true;
            btnClearPreview.Click += btnClearPreview_Click;

            lblPreviewStatus.Location = new System.Drawing.Point(370, 117);
            lblPreviewStatus.Name = "lblPreviewStatus";
            lblPreviewStatus.Size = new System.Drawing.Size(330, 20);
            lblPreviewStatus.Text = "Preview Status: Not spawned";

            // Main
            AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            Controls.Add(groupSearch);
            Controls.Add(groupSelected);
            Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            Name = "Main";
            Size = new System.Drawing.Size(750, 458);
            Load += Main_Load;
            groupSearch.ResumeLayout(false);
            groupSearch.PerformLayout();
            groupSelected.ResumeLayout(false);
            ResumeLayout(false);
        }

        #endregion

        private SDUI.Controls.GroupBox groupSearch;
        private SDUI.Controls.Label lblSearch;
        private SDUI.Controls.TextBox txtSearch;
        private SDUI.Controls.Button btnSearch;
        private SDUI.Controls.ListView lstSearchResults;
        private System.Windows.Forms.ColumnHeader colId;
        private System.Windows.Forms.ColumnHeader colType;
        private System.Windows.Forms.ColumnHeader colName;
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
    }
}
