namespace RSBot.Views;

partial class ManageAccountsDialog
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

    #region Windows Form Designer generated code

    private void InitializeComponent()
    {
        panelHeader = new SDUI.Controls.Panel();
        lblHeaderTitle = new SDUI.Controls.Label();
        lblHeaderSubtitle = new SDUI.Controls.Label();
        panelTopBar = new SDUI.Controls.Panel();
        lblSearch = new SDUI.Controls.Label();
        txtSearch = new SDUI.Controls.TextBox();
        btnRefresh = new SDUI.Controls.Button();
        btnSettings = new SDUI.Controls.Button();
        btnNewAccount = new SDUI.Controls.Button();
        panelMain = new SDUI.Controls.Panel();
        listAccounts = new System.Windows.Forms.ListView();
        colUser = new System.Windows.Forms.ColumnHeader();
        colStatus = new System.Windows.Forms.ColumnHeader();
        colRemaining = new System.Windows.Forms.ColumnHeader();
        colExpires = new System.Windows.Forms.ColumnHeader();
        colHwid = new System.Windows.Forms.ColumnHeader();
        colLastLogin = new System.Windows.Forms.ColumnHeader();
        panelActions = new SDUI.Controls.Panel();
        grpSubscription = new SDUI.Controls.GroupBox();
        btnAdd1Day = new SDUI.Controls.Button();
        btnAdd7Days = new SDUI.Controls.Button();
        btnAdd30Days = new SDUI.Controls.Button();
        btnAdd90Days = new SDUI.Controls.Button();
        btnLifetime = new SDUI.Controls.Button();
        btnExpireNow = new SDUI.Controls.Button();
        dtpCustomExpiry = new System.Windows.Forms.DateTimePicker();
        btnApplyCustomDate = new SDUI.Controls.Button();
        grpAccountOps = new SDUI.Controls.GroupBox();
        btnResetHwid = new SDUI.Controls.Button();
        btnToggleBlock = new SDUI.Controls.Button();
        btnChangePass = new SDUI.Controls.Button();
        btnDeleteAccount = new SDUI.Controls.Button();
        btnToggleAdmin = new SDUI.Controls.Button();
        lblSelectedUser = new SDUI.Controls.Label();
        panelBottom = new SDUI.Controls.Panel();
        lblStatus = new SDUI.Controls.Label();
        btnClose = new SDUI.Controls.Button();

        panelHeader.SuspendLayout();
        panelTopBar.SuspendLayout();
        panelMain.SuspendLayout();
        panelActions.SuspendLayout();
        grpSubscription.SuspendLayout();
        grpAccountOps.SuspendLayout();
        panelBottom.SuspendLayout();
        SuspendLayout();

        // 
        // panelHeader
        // 
        panelHeader.BackColor = System.Drawing.Color.FromArgb(28, 30, 38);
        panelHeader.BorderColor = System.Drawing.Color.FromArgb(50, 55, 70);
        panelHeader.Controls.Add(lblHeaderSubtitle);
        panelHeader.Controls.Add(lblHeaderTitle);
        panelHeader.Dock = System.Windows.Forms.DockStyle.Top;
        panelHeader.Location = new System.Drawing.Point(0, 0);
        panelHeader.Name = "panelHeader";
        panelHeader.Radius = 0;
        panelHeader.ShadowDepth = 0F;
        panelHeader.Size = new System.Drawing.Size(920, 65);
        panelHeader.TabIndex = 0;

        // 
        // lblHeaderTitle
        // 
        lblHeaderTitle.Font = new System.Drawing.Font("Segoe UI", 14F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
        lblHeaderTitle.ForeColor = System.Drawing.Color.FromArgb(245, 215, 100);
        lblHeaderTitle.Location = new System.Drawing.Point(12, 10);
        lblHeaderTitle.Name = "lblHeaderTitle";
        lblHeaderTitle.Size = new System.Drawing.Size(896, 28);
        lblHeaderTitle.TabIndex = 0;
        lblHeaderTitle.Text = "لوحة إدارة الحسابات والاشتراكات (Admin Dashboard)";

        // 
        // lblHeaderSubtitle
        // 
        lblHeaderSubtitle.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
        lblHeaderSubtitle.ForeColor = System.Drawing.Color.FromArgb(160, 175, 195);
        lblHeaderSubtitle.Location = new System.Drawing.Point(14, 38);
        lblHeaderSubtitle.Name = "lblHeaderSubtitle";
        lblHeaderSubtitle.Size = new System.Drawing.Size(894, 20);
        lblHeaderSubtitle.TabIndex = 1;
        lblHeaderSubtitle.Text = "التحكم في مدد الاشتراكات، فك الحظر، إعادة تعيين بصمات الأجهزة، وإعدادات الواتساب";

        // 
        // panelTopBar
        // 
        panelTopBar.BackColor = System.Drawing.Color.FromArgb(24, 25, 32);
        panelTopBar.Controls.Add(btnNewAccount);
        panelTopBar.Controls.Add(btnSettings);
        panelTopBar.Controls.Add(btnRefresh);
        panelTopBar.Controls.Add(txtSearch);
        panelTopBar.Controls.Add(lblSearch);
        panelTopBar.Dock = System.Windows.Forms.DockStyle.Top;
        panelTopBar.Location = new System.Drawing.Point(0, 65);
        panelTopBar.Name = "panelTopBar";
        panelTopBar.Radius = 0;
        panelTopBar.ShadowDepth = 0F;
        panelTopBar.Size = new System.Drawing.Size(920, 45);
        panelTopBar.TabIndex = 1;

        // 
        // lblSearch
        // 
        lblSearch.AutoSize = true;
        lblSearch.ForeColor = System.Drawing.Color.FromArgb(200, 210, 225);
        lblSearch.Location = new System.Drawing.Point(14, 15);
        lblSearch.Name = "lblSearch";
        lblSearch.Size = new System.Drawing.Size(32, 15);
        lblSearch.TabIndex = 0;
        lblSearch.Text = "بحث:";

        // 
        // txtSearch
        // 
        txtSearch.Location = new System.Drawing.Point(52, 11);
        txtSearch.Name = "txtSearch";
        txtSearch.Size = new System.Drawing.Size(180, 23);
        txtSearch.TabIndex = 1;
        txtSearch.TextChanged += txtSearch_TextChanged;

        // 
        // btnRefresh
        // 
        btnRefresh.Color = System.Drawing.Color.FromArgb(37, 99, 235);
        btnRefresh.ForeColor = System.Drawing.Color.White;
        btnRefresh.Location = new System.Drawing.Point(240, 9);
        btnRefresh.Name = "btnRefresh";
        btnRefresh.Radius = 4;
        btnRefresh.ShadowDepth = 2F;
        btnRefresh.Size = new System.Drawing.Size(85, 27);
        btnRefresh.TabIndex = 2;
        btnRefresh.Text = "🔄 تحديث";
        btnRefresh.UseVisualStyleBackColor = true;
        btnRefresh.Click += btnRefresh_Click;

        // 
        // btnNewAccount
        // 
        btnNewAccount.Color = System.Drawing.Color.FromArgb(16, 149, 100);
        btnNewAccount.ForeColor = System.Drawing.Color.White;
        btnNewAccount.Location = new System.Drawing.Point(335, 9);
        btnNewAccount.Name = "btnNewAccount";
        btnNewAccount.Radius = 4;
        btnNewAccount.ShadowDepth = 2F;
        btnNewAccount.Size = new System.Drawing.Size(120, 27);
        btnNewAccount.TabIndex = 3;
        btnNewAccount.Text = "➕ إنشاء حساب";
        btnNewAccount.UseVisualStyleBackColor = true;
        btnNewAccount.Click += btnNewAccount_Click;

        // 
        // btnSettings
        // 
        btnSettings.Color = System.Drawing.Color.FromArgb(55, 65, 81);
        btnSettings.ForeColor = System.Drawing.Color.White;
        btnSettings.Location = new System.Drawing.Point(465, 9);
        btnSettings.Name = "btnSettings";
        btnSettings.Radius = 4;
        btnSettings.ShadowDepth = 2F;
        btnSettings.Size = new System.Drawing.Size(140, 27);
        btnSettings.TabIndex = 4;
        btnSettings.Text = "⚙️ إعدادات الواتساب";
        btnSettings.UseVisualStyleBackColor = true;
        btnSettings.Click += btnSettings_Click;

        // 
        // panelMain
        // 
        panelMain.Controls.Add(listAccounts);
        panelMain.Dock = System.Windows.Forms.DockStyle.Fill;
        panelMain.Location = new System.Drawing.Point(0, 110);
        panelMain.Name = "panelMain";
        panelMain.Radius = 0;
        panelMain.ShadowDepth = 0F;
        panelMain.Size = new System.Drawing.Size(920, 260);
        panelMain.TabIndex = 2;

        // 
        // listAccounts
        // 
        listAccounts.BackColor = System.Drawing.Color.FromArgb(20, 22, 28);
        listAccounts.BorderStyle = System.Windows.Forms.BorderStyle.None;
        listAccounts.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            colUser,
            colStatus,
            colRemaining,
            colExpires,
            colHwid,
            colLastLogin});
        listAccounts.Dock = System.Windows.Forms.DockStyle.Fill;
        listAccounts.Font = new System.Drawing.Font("Segoe UI", 9.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
        listAccounts.ForeColor = System.Drawing.Color.FromArgb(230, 235, 245);
        listAccounts.FullRowSelect = true;
        listAccounts.GridLines = true;
        listAccounts.HideSelection = false;
        listAccounts.Location = new System.Drawing.Point(0, 0);
        listAccounts.MultiSelect = false;
        listAccounts.Name = "listAccounts";
        listAccounts.Size = new System.Drawing.Size(920, 260);
        listAccounts.TabIndex = 0;
        listAccounts.UseCompatibleStateImageBehavior = false;
        listAccounts.View = System.Windows.Forms.View.Details;
        listAccounts.SelectedIndexChanged += listAccounts_SelectedIndexChanged;

        // 
        // colUser
        // 
        colUser.Text = "اسم المستخدم";
        colUser.Width = 120;

        // 
        // colStatus
        // 
        colStatus.Text = "الحالة";
        colStatus.Width = 100;

        // 
        // colRemaining
        // 
        colRemaining.Text = "الوقت المتبقي";
        colRemaining.Width = 160;

        // 
        // colExpires
        // 
        colExpires.Text = "تاريخ الانتهاء";
        colExpires.Width = 150;

        // 
        // colHwid
        // 
        colHwid.Text = "بصمة الجهاز (HWID)";
        colHwid.Width = 180;

        // 
        // colLastLogin
        // 
        colLastLogin.Text = "آخر تسجيل دخول";
        colLastLogin.Width = 150;

        // 
        // panelActions
        // 
        panelActions.BackColor = System.Drawing.Color.FromArgb(24, 26, 33);
        panelActions.Controls.Add(lblSelectedUser);
        panelActions.Controls.Add(grpSubscription);
        panelActions.Controls.Add(grpAccountOps);
        panelActions.Dock = System.Windows.Forms.DockStyle.Bottom;
        panelActions.Location = new System.Drawing.Point(0, 370);
        panelActions.Name = "panelActions";
        panelActions.Radius = 0;
        panelActions.ShadowDepth = 0F;
        panelActions.Size = new System.Drawing.Size(920, 165);
        panelActions.TabIndex = 3;

        // 
        // lblSelectedUser
        // 
        lblSelectedUser.Font = new System.Drawing.Font("Segoe UI", 9.5F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
        lblSelectedUser.ForeColor = System.Drawing.Color.FromArgb(140, 200, 255);
        lblSelectedUser.Location = new System.Drawing.Point(12, 6);
        lblSelectedUser.Name = "lblSelectedUser";
        lblSelectedUser.Size = new System.Drawing.Size(896, 20);
        lblSelectedUser.TabIndex = 0;
        lblSelectedUser.Text = "الحساب المحدد: لم يتم تحديد أي حساب";

        // 
        // grpSubscription
        // 
        grpSubscription.BackColor = System.Drawing.Color.Transparent;
        grpSubscription.Controls.Add(btnApplyCustomDate);
        grpSubscription.Controls.Add(dtpCustomExpiry);
        grpSubscription.Controls.Add(btnExpireNow);
        grpSubscription.Controls.Add(btnLifetime);
        grpSubscription.Controls.Add(btnAdd90Days);
        grpSubscription.Controls.Add(btnAdd30Days);
        grpSubscription.Controls.Add(btnAdd7Days);
        grpSubscription.Controls.Add(btnAdd1Day);
        grpSubscription.ForeColor = System.Drawing.Color.FromArgb(245, 215, 100);
        grpSubscription.Location = new System.Drawing.Point(12, 28);
        grpSubscription.Name = "grpSubscription";
        grpSubscription.Size = new System.Drawing.Size(530, 130);
        grpSubscription.TabIndex = 1;
        grpSubscription.TabStop = false;
        grpSubscription.Text = "إعطاء وقت وشحن الاشتراك (Subscription)";

        // 
        // btnAdd1Day
        // 
        btnAdd1Day.Color = System.Drawing.Color.FromArgb(40, 70, 120);
        btnAdd1Day.ForeColor = System.Drawing.Color.White;
        btnAdd1Day.Location = new System.Drawing.Point(12, 24);
        btnAdd1Day.Name = "btnAdd1Day";
        btnAdd1Day.Radius = 4;
        btnAdd1Day.ShadowDepth = 2F;
        btnAdd1Day.Size = new System.Drawing.Size(75, 28);
        btnAdd1Day.TabIndex = 0;
        btnAdd1Day.Text = "+1 يوم";
        btnAdd1Day.UseVisualStyleBackColor = true;
        btnAdd1Day.Click += btnAdd1Day_Click;

        // 
        // btnAdd7Days
        // 
        btnAdd7Days.Color = System.Drawing.Color.FromArgb(40, 70, 120);
        btnAdd7Days.ForeColor = System.Drawing.Color.White;
        btnAdd7Days.Location = new System.Drawing.Point(93, 24);
        btnAdd7Days.Name = "btnAdd7Days";
        btnAdd7Days.Radius = 4;
        btnAdd7Days.ShadowDepth = 2F;
        btnAdd7Days.Size = new System.Drawing.Size(75, 28);
        btnAdd7Days.TabIndex = 1;
        btnAdd7Days.Text = "+7 أيام";
        btnAdd7Days.UseVisualStyleBackColor = true;
        btnAdd7Days.Click += btnAdd7Days_Click;

        // 
        // btnAdd30Days
        // 
        btnAdd30Days.Color = System.Drawing.Color.FromArgb(37, 99, 235);
        btnAdd30Days.ForeColor = System.Drawing.Color.White;
        btnAdd30Days.Location = new System.Drawing.Point(174, 24);
        btnAdd30Days.Name = "btnAdd30Days";
        btnAdd30Days.Radius = 4;
        btnAdd30Days.ShadowDepth = 2F;
        btnAdd30Days.Size = new System.Drawing.Size(85, 28);
        btnAdd30Days.TabIndex = 2;
        btnAdd30Days.Text = "+30 يوم (شهر)";
        btnAdd30Days.UseVisualStyleBackColor = true;
        btnAdd30Days.Click += btnAdd30Days_Click;

        // 
        // btnAdd90Days
        // 
        btnAdd90Days.Color = System.Drawing.Color.FromArgb(37, 99, 235);
        btnAdd90Days.ForeColor = System.Drawing.Color.White;
        btnAdd90Days.Location = new System.Drawing.Point(265, 24);
        btnAdd90Days.Name = "btnAdd90Days";
        btnAdd90Days.Radius = 4;
        btnAdd90Days.ShadowDepth = 2F;
        btnAdd90Days.Size = new System.Drawing.Size(95, 28);
        btnAdd90Days.TabIndex = 3;
        btnAdd90Days.Text = "+90 يوم (3 شهور)";
        btnAdd90Days.UseVisualStyleBackColor = true;
        btnAdd90Days.Click += btnAdd90Days_Click;

        // 
        // btnLifetime
        // 
        btnLifetime.Color = System.Drawing.Color.FromArgb(16, 149, 100);
        btnLifetime.ForeColor = System.Drawing.Color.White;
        btnLifetime.Location = new System.Drawing.Point(366, 24);
        btnLifetime.Name = "btnLifetime";
        btnLifetime.Radius = 4;
        btnLifetime.ShadowDepth = 2F;
        btnLifetime.Size = new System.Drawing.Size(85, 28);
        btnLifetime.TabIndex = 4;
        btnLifetime.Text = "اشتراك دائم";
        btnLifetime.UseVisualStyleBackColor = true;
        btnLifetime.Click += btnLifetime_Click;

        // 
        // btnExpireNow
        // 
        btnExpireNow.Color = System.Drawing.Color.FromArgb(180, 40, 40);
        btnExpireNow.ForeColor = System.Drawing.Color.White;
        btnExpireNow.Location = new System.Drawing.Point(457, 24);
        btnExpireNow.Name = "btnExpireNow";
        btnExpireNow.Radius = 4;
        btnExpireNow.ShadowDepth = 2F;
        btnExpireNow.Size = new System.Drawing.Size(65, 28);
        btnExpireNow.TabIndex = 5;
        btnExpireNow.Text = "تصفير";
        btnExpireNow.UseVisualStyleBackColor = true;
        btnExpireNow.Click += btnExpireNow_Click;

        // 
        // dtpCustomExpiry
        // 
        dtpCustomExpiry.CustomFormat = "yyyy-MM-dd HH:mm";
        dtpCustomExpiry.Format = System.Windows.Forms.DateTimePickerFormat.Custom;
        dtpCustomExpiry.Location = new System.Drawing.Point(12, 70);
        dtpCustomExpiry.Name = "dtpCustomExpiry";
        dtpCustomExpiry.Size = new System.Drawing.Size(180, 23);
        dtpCustomExpiry.TabIndex = 6;

        // 
        // btnApplyCustomDate
        // 
        btnApplyCustomDate.Color = System.Drawing.Color.FromArgb(70, 80, 100);
        btnApplyCustomDate.ForeColor = System.Drawing.Color.White;
        btnApplyCustomDate.Location = new System.Drawing.Point(202, 68);
        btnApplyCustomDate.Name = "btnApplyCustomDate";
        btnApplyCustomDate.Radius = 4;
        btnApplyCustomDate.ShadowDepth = 2F;
        btnApplyCustomDate.Size = new System.Drawing.Size(130, 27);
        btnApplyCustomDate.TabIndex = 7;
        btnApplyCustomDate.Text = "تطبيق التاريخ المحدد";
        btnApplyCustomDate.UseVisualStyleBackColor = true;
        btnApplyCustomDate.Click += btnApplyCustomDate_Click;

        // 
        // grpAccountOps
        // 
        grpAccountOps.BackColor = System.Drawing.Color.Transparent;
        grpAccountOps.Controls.Add(btnToggleAdmin);
        grpAccountOps.Controls.Add(btnDeleteAccount);
        grpAccountOps.Controls.Add(btnChangePass);
        grpAccountOps.Controls.Add(btnToggleBlock);
        grpAccountOps.Controls.Add(btnResetHwid);
        grpAccountOps.ForeColor = System.Drawing.Color.FromArgb(170, 200, 240);
        grpAccountOps.Location = new System.Drawing.Point(555, 28);
        grpAccountOps.Name = "grpAccountOps";
        grpAccountOps.Size = new System.Drawing.Size(353, 130);
        grpAccountOps.TabIndex = 2;
        grpAccountOps.TabStop = false;
        grpAccountOps.Text = "عمليات الحساب والأمان";

        // 
        // btnResetHwid
        // 
        btnResetHwid.Color = System.Drawing.Color.FromArgb(45, 60, 80);
        btnResetHwid.ForeColor = System.Drawing.Color.White;
        btnResetHwid.Location = new System.Drawing.Point(12, 24);
        btnResetHwid.Name = "btnResetHwid";
        btnResetHwid.Radius = 4;
        btnResetHwid.ShadowDepth = 2F;
        btnResetHwid.Size = new System.Drawing.Size(110, 28);
        btnResetHwid.TabIndex = 0;
        btnResetHwid.Text = "تصفير الجهاز (HWID)";
        btnResetHwid.UseVisualStyleBackColor = true;
        btnResetHwid.Click += btnResetHwid_Click;

        // 
        // btnToggleBlock
        // 
        btnToggleBlock.Color = System.Drawing.Color.FromArgb(160, 60, 60);
        btnToggleBlock.ForeColor = System.Drawing.Color.White;
        btnToggleBlock.Location = new System.Drawing.Point(128, 24);
        btnToggleBlock.Name = "btnToggleBlock";
        btnToggleBlock.Radius = 4;
        btnToggleBlock.ShadowDepth = 2F;
        btnToggleBlock.Size = new System.Drawing.Size(95, 28);
        btnToggleBlock.TabIndex = 1;
        btnToggleBlock.Text = "حظر / فك حظر";
        btnToggleBlock.UseVisualStyleBackColor = true;
        btnToggleBlock.Click += btnToggleBlock_Click;

        // 
        // btnChangePass
        // 
        btnChangePass.Color = System.Drawing.Color.FromArgb(50, 65, 85);
        btnChangePass.ForeColor = System.Drawing.Color.White;
        btnChangePass.Location = new System.Drawing.Point(229, 24);
        btnChangePass.Name = "btnChangePass";
        btnChangePass.Radius = 4;
        btnChangePass.ShadowDepth = 2F;
        btnChangePass.Size = new System.Drawing.Size(110, 28);
        btnChangePass.TabIndex = 2;
        btnChangePass.Text = "تغيير كلمة المرور";
        btnChangePass.UseVisualStyleBackColor = true;
        btnChangePass.Click += btnChangePass_Click;

        // 
        // btnDeleteAccount
        // 
        btnDeleteAccount.Color = System.Drawing.Color.FromArgb(120, 30, 30);
        btnDeleteAccount.ForeColor = System.Drawing.Color.White;
        btnDeleteAccount.Location = new System.Drawing.Point(12, 68);
        btnDeleteAccount.Name = "btnDeleteAccount";
        btnDeleteAccount.Radius = 4;
        btnDeleteAccount.ShadowDepth = 2F;
        btnDeleteAccount.Size = new System.Drawing.Size(110, 28);
        btnDeleteAccount.TabIndex = 3;
        btnDeleteAccount.Text = "🗑️ حذف الحساب";
        btnDeleteAccount.UseVisualStyleBackColor = true;
        btnDeleteAccount.Click += btnDeleteAccount_Click;

        // 
        // btnToggleAdmin
        // 
        btnToggleAdmin.Color = System.Drawing.Color.FromArgb(70, 70, 95);
        btnToggleAdmin.ForeColor = System.Drawing.Color.White;
        btnToggleAdmin.Location = new System.Drawing.Point(128, 68);
        btnToggleAdmin.Name = "btnToggleAdmin";
        btnToggleAdmin.Radius = 4;
        btnToggleAdmin.ShadowDepth = 2F;
        btnToggleAdmin.Size = new System.Drawing.Size(115, 28);
        btnToggleAdmin.TabIndex = 4;
        btnToggleAdmin.Text = "تعيين / إلغاء أدمن";
        btnToggleAdmin.UseVisualStyleBackColor = true;
        btnToggleAdmin.Click += btnToggleAdmin_Click;

        // 
        // panelBottom
        // 
        panelBottom.BackColor = System.Drawing.Color.FromArgb(20, 21, 26);
        panelBottom.Controls.Add(btnClose);
        panelBottom.Controls.Add(lblStatus);
        panelBottom.Dock = System.Windows.Forms.DockStyle.Bottom;
        panelBottom.Location = new System.Drawing.Point(0, 535);
        panelBottom.Name = "panelBottom";
        panelBottom.Radius = 0;
        panelBottom.ShadowDepth = 0F;
        panelBottom.Size = new System.Drawing.Size(920, 45);
        panelBottom.TabIndex = 4;

        // 
        // lblStatus
        // 
        lblStatus.ForeColor = System.Drawing.Color.FromArgb(160, 175, 195);
        lblStatus.Location = new System.Drawing.Point(14, 12);
        lblStatus.Name = "lblStatus";
        lblStatus.Size = new System.Drawing.Size(780, 20);
        lblStatus.TabIndex = 0;
        lblStatus.Text = "جاهز";

        // 
        // btnClose
        // 
        btnClose.Color = System.Drawing.Color.FromArgb(40, 45, 55);
        btnClose.ForeColor = System.Drawing.Color.White;
        btnClose.Location = new System.Drawing.Point(825, 9);
        btnClose.Name = "btnClose";
        btnClose.Radius = 4;
        btnClose.ShadowDepth = 2F;
        btnClose.Size = new System.Drawing.Size(83, 27);
        btnClose.TabIndex = 1;
        btnClose.Text = "إغلاق";
        btnClose.UseVisualStyleBackColor = true;
        btnClose.Click += btnClose_Click;

        // 
        // ManageAccountsDialog
        // 
        AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
        AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
        ClientSize = new System.Drawing.Size(920, 580);
        Controls.Add(panelMain);
        Controls.Add(panelActions);
        Controls.Add(panelBottom);
        Controls.Add(panelTopBar);
        Controls.Add(panelHeader);
        FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
        MinimumSize = new System.Drawing.Size(850, 500);
        Name = "ManageAccountsDialog";
        StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
        Text = "لوحة إدارة الحسابات والاشتراكات";
        panelHeader.ResumeLayout(false);
        panelTopBar.ResumeLayout(false);
        panelTopBar.PerformLayout();
        panelMain.ResumeLayout(false);
        panelActions.ResumeLayout(false);
        grpSubscription.ResumeLayout(false);
        grpAccountOps.ResumeLayout(false);
        panelBottom.ResumeLayout(false);
        ResumeLayout(false);
    }

    #endregion

    private SDUI.Controls.Panel panelHeader;
    private SDUI.Controls.Label lblHeaderTitle;
    private SDUI.Controls.Label lblHeaderSubtitle;
    private SDUI.Controls.Panel panelTopBar;
    private SDUI.Controls.Label lblSearch;
    private SDUI.Controls.TextBox txtSearch;
    private SDUI.Controls.Button btnRefresh;
    private SDUI.Controls.Button btnNewAccount;
    private SDUI.Controls.Button btnSettings;
    private SDUI.Controls.Panel panelMain;
    private System.Windows.Forms.ListView listAccounts;
    private System.Windows.Forms.ColumnHeader colUser;
    private System.Windows.Forms.ColumnHeader colStatus;
    private System.Windows.Forms.ColumnHeader colRemaining;
    private System.Windows.Forms.ColumnHeader colExpires;
    private System.Windows.Forms.ColumnHeader colHwid;
    private System.Windows.Forms.ColumnHeader colLastLogin;
    private SDUI.Controls.Panel panelActions;
    private SDUI.Controls.Label lblSelectedUser;
    private SDUI.Controls.GroupBox grpSubscription;
    private SDUI.Controls.Button btnAdd1Day;
    private SDUI.Controls.Button btnAdd7Days;
    private SDUI.Controls.Button btnAdd30Days;
    private SDUI.Controls.Button btnAdd90Days;
    private SDUI.Controls.Button btnLifetime;
    private SDUI.Controls.Button btnExpireNow;
    private System.Windows.Forms.DateTimePicker dtpCustomExpiry;
    private SDUI.Controls.Button btnApplyCustomDate;
    private SDUI.Controls.GroupBox grpAccountOps;
    private SDUI.Controls.Button btnResetHwid;
    private SDUI.Controls.Button btnToggleBlock;
    private SDUI.Controls.Button btnChangePass;
    private SDUI.Controls.Button btnDeleteAccount;
    private SDUI.Controls.Button btnToggleAdmin;
    private SDUI.Controls.Panel panelBottom;
    private SDUI.Controls.Label lblStatus;
    private SDUI.Controls.Button btnClose;
}
