namespace RSBot.Views;

partial class AdminForm
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
        System.Windows.Forms.DataGridViewCellStyle dgvAccountsHeaderStyle = new System.Windows.Forms.DataGridViewCellStyle();
        System.Windows.Forms.DataGridViewCellStyle dgvAccountsRowStyle = new System.Windows.Forms.DataGridViewCellStyle();
        System.Windows.Forms.DataGridViewCellStyle dgvLogsHeaderStyle = new System.Windows.Forms.DataGridViewCellStyle();
        System.Windows.Forms.DataGridViewCellStyle dgvLogsRowStyle = new System.Windows.Forms.DataGridViewCellStyle();

        panelTop = new System.Windows.Forms.Panel();
        panelStats = new System.Windows.Forms.Panel();
        lblBlockedCount = new System.Windows.Forms.Label();
        lblExpiredCount = new System.Windows.Forms.Label();
        lblActiveCount = new System.Windows.Forms.Label();
        lblTotalAccounts = new System.Windows.Forms.Label();
        lblServerStatus = new System.Windows.Forms.Label();
        lblSubtitle = new System.Windows.Forms.Label();
        lblTitle = new System.Windows.Forms.Label();
        picLogo = new System.Windows.Forms.PictureBox();

        tabControl = new System.Windows.Forms.TabControl();
        tabAccounts = new System.Windows.Forms.TabPage();
        dgvAccounts = new System.Windows.Forms.DataGridView();
        colIndex = new System.Windows.Forms.DataGridViewTextBoxColumn();
        colUser = new System.Windows.Forms.DataGridViewTextBoxColumn();
        colStatus = new System.Windows.Forms.DataGridViewTextBoxColumn();
        colSubscription = new System.Windows.Forms.DataGridViewTextBoxColumn();
        colExpiry = new System.Windows.Forms.DataGridViewTextBoxColumn();
        colReason = new System.Windows.Forms.DataGridViewTextBoxColumn();
        colHwid = new System.Windows.Forms.DataGridViewTextBoxColumn();
        colRegIp = new System.Windows.Forms.DataGridViewTextBoxColumn();
        colLastIp = new System.Windows.Forms.DataGridViewTextBoxColumn();
        colCreated = new System.Windows.Forms.DataGridViewTextBoxColumn();
        colLastLogin = new System.Windows.Forms.DataGridViewTextBoxColumn();
        colNotes = new System.Windows.Forms.DataGridViewTextBoxColumn();

        cmsAccounts = new System.Windows.Forms.ContextMenuStrip();
        tsmiExtendSub = new System.Windows.Forms.ToolStripMenuItem();
        tsmiAdd1Day = new System.Windows.Forms.ToolStripMenuItem();
        tsmiAdd7Days = new System.Windows.Forms.ToolStripMenuItem();
        tsmiAdd30Days = new System.Windows.Forms.ToolStripMenuItem();
        tsmiAdd60Days = new System.Windows.Forms.ToolStripMenuItem();
        tsmiAdd90Days = new System.Windows.Forms.ToolStripMenuItem();
        tsmiAdd365Days = new System.Windows.Forms.ToolStripMenuItem();
        tsmiAddCustom = new System.Windows.Forms.ToolStripMenuItem();
        tsmiSetDate = new System.Windows.Forms.ToolStripMenuItem();
        tsmiLifetime = new System.Windows.Forms.ToolStripMenuItem();
        tsmiExpireNow = new System.Windows.Forms.ToolStripMenuItem();
        tsmiSep1 = new System.Windows.Forms.ToolStripSeparator();
        tsmiResetHwid = new System.Windows.Forms.ToolStripMenuItem();
        tsmiChangePassword = new System.Windows.Forms.ToolStripMenuItem();
        tsmiBlock = new System.Windows.Forms.ToolStripMenuItem();
        tsmiUnblock = new System.Windows.Forms.ToolStripMenuItem();
        tsmiSep2 = new System.Windows.Forms.ToolStripSeparator();
        tsmiDeleteAccount = new System.Windows.Forms.ToolStripMenuItem();

        panelAccountActions = new System.Windows.Forms.Panel();
        lblSubHeader = new System.Windows.Forms.Label();
        btnAdd1Day = new System.Windows.Forms.Button();
        btnAdd7Days = new System.Windows.Forms.Button();
        btnAdd30Days = new System.Windows.Forms.Button();
        btnAdd60Days = new System.Windows.Forms.Button();
        btnAdd90Days = new System.Windows.Forms.Button();
        btnAddCustom = new System.Windows.Forms.Button();
        btnSetDate = new System.Windows.Forms.Button();
        btnLifetime = new System.Windows.Forms.Button();
        btnExpireNow = new System.Windows.Forms.Button();

        btnRefresh = new System.Windows.Forms.Button();
        btnUnblock = new System.Windows.Forms.Button();
        btnBlock = new System.Windows.Forms.Button();
        btnResetHwid = new System.Windows.Forms.Button();
        btnChangePassword = new System.Windows.Forms.Button();
        btnAddAccount = new System.Windows.Forms.Button();
        btnDeleteAccount = new System.Windows.Forms.Button();
        btnToggleAdmin = new System.Windows.Forms.Button();
        tsmiToggleAdmin = new System.Windows.Forms.ToolStripMenuItem();
        chkAutoRefresh = new System.Windows.Forms.CheckBox();

        tabLogs = new System.Windows.Forms.TabPage();
        dgvLogs = new System.Windows.Forms.DataGridView();
        colLogTime = new System.Windows.Forms.DataGridViewTextBoxColumn();
        colLogUser = new System.Windows.Forms.DataGridViewTextBoxColumn();
        colLogAction = new System.Windows.Forms.DataGridViewTextBoxColumn();
        colLogHwid = new System.Windows.Forms.DataGridViewTextBoxColumn();
        colLogIp = new System.Windows.Forms.DataGridViewTextBoxColumn();
        colLogDetails = new System.Windows.Forms.DataGridViewTextBoxColumn();
        panelLogActions = new System.Windows.Forms.Panel();
        btnRefreshLogs = new System.Windows.Forms.Button();

        tabSettings = new System.Windows.Forms.TabPage();
        panelSettingsCard = new System.Windows.Forms.Panel();
        lblInstructions = new System.Windows.Forms.Label();
        btnSaveSettings = new System.Windows.Forms.Button();
        btnTestConnection = new System.Windows.Forms.Button();
        numMaxAccounts = new System.Windows.Forms.NumericUpDown();
        lblMaxAccounts = new System.Windows.Forms.Label();
        txtAuthSecret = new System.Windows.Forms.TextBox();
        lblAuthSecret = new System.Windows.Forms.Label();
        txtFirebaseUrl = new System.Windows.Forms.TextBox();
        lblFirebaseUrl = new System.Windows.Forms.Label();

        panelWhatsAppCard = new System.Windows.Forms.Panel();
        lblWhatsAppHeader = new System.Windows.Forms.Label();
        lblWhatsAppNumber = new System.Windows.Forms.Label();
        txtWhatsAppNumber = new System.Windows.Forms.TextBox();
        lblWhatsAppUrl = new System.Windows.Forms.Label();
        txtWhatsAppUrl = new System.Windows.Forms.TextBox();
        lblRechargeMessage = new System.Windows.Forms.Label();
        txtRechargeMessage = new System.Windows.Forms.TextBox();
        btnSaveWhatsApp = new System.Windows.Forms.Button();
        lblWhatsAppHint = new System.Windows.Forms.Label();

        tabUpdates = new System.Windows.Forms.TabPage();
        tabPermissions = new System.Windows.Forms.TabPage();
        panelPermCard = new System.Windows.Forms.Panel();
        lblPermHeader = new System.Windows.Forms.Label();
        lblPermDesc = new System.Windows.Forms.Label();
        chkFreeTierEnabled = new System.Windows.Forms.CheckBox();
        grpBotbasePerms = new System.Windows.Forms.GroupBox();
        chkAllowGrinding = new System.Windows.Forms.CheckBox();
        chkAllowTrade = new System.Windows.Forms.CheckBox();
        chkAllowAlchemy = new System.Windows.Forms.CheckBox();
        chkAllowLure = new System.Windows.Forms.CheckBox();
        grpAutoPerms = new System.Windows.Forms.GroupBox();
        chkAllowTownLoop = new System.Windows.Forms.CheckBox();
        chkAllowAutoParty = new System.Windows.Forms.CheckBox();
        chkAllowAutoPotion = new System.Windows.Forms.CheckBox();
        chkAllowAutoPickup = new System.Windows.Forms.CheckBox();
        chkAllowAutoResurrect = new System.Windows.Forms.CheckBox();
        chkAllowStall = new System.Windows.Forms.CheckBox();
        grpLimitsPerms = new System.Windows.Forms.GroupBox();
        lblMaxClients = new System.Windows.Forms.Label();
        numMaxClients = new System.Windows.Forms.NumericUpDown();
        lblSessionMinutes = new System.Windows.Forms.Label();
        numSessionMinutes = new System.Windows.Forms.NumericUpDown();
        lblFreeNotice = new System.Windows.Forms.Label();
        txtFreeNotice = new System.Windows.Forms.TextBox();
        btnSavePermissions = new System.Windows.Forms.Button();
        btnReloadPermissions = new System.Windows.Forms.Button();
        lblPermStatus = new System.Windows.Forms.Label();
        panelUpdateCard = new System.Windows.Forms.Panel();
        lblUpdateHeader = new System.Windows.Forms.Label();
        lblUpdateVersion = new System.Windows.Forms.Label();
        txtUpdateVersion = new System.Windows.Forms.TextBox();
        lblUpdateUrl = new System.Windows.Forms.Label();
        txtUpdateUrl = new System.Windows.Forms.TextBox();
        chkUpdateMandatory = new System.Windows.Forms.CheckBox();
        lblChangelogAr = new System.Windows.Forms.Label();
        txtChangelogAr = new System.Windows.Forms.TextBox();
        lblChangelogEn = new System.Windows.Forms.Label();
        txtChangelogEn = new System.Windows.Forms.TextBox();
        btnPublishUpdate = new System.Windows.Forms.Button();
        btnCheckUpdate = new System.Windows.Forms.Button();
        lblUpdateStatus = new System.Windows.Forms.Label();

        panelBottomStatus = new System.Windows.Forms.Panel();
        lblStatusMessage = new System.Windows.Forms.Label();

        panelTop.SuspendLayout();
        panelStats.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)picLogo).BeginInit();
        tabControl.SuspendLayout();
        tabAccounts.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)dgvAccounts).BeginInit();
        cmsAccounts.SuspendLayout();
        panelAccountActions.SuspendLayout();
        tabLogs.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)dgvLogs).BeginInit();
        panelLogActions.SuspendLayout();
        tabSettings.SuspendLayout();
        panelSettingsCard.SuspendLayout();
        panelWhatsAppCard.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)numMaxAccounts).BeginInit();
        tabUpdates.SuspendLayout();
        panelUpdateCard.SuspendLayout();
        panelBottomStatus.SuspendLayout();
        SuspendLayout();

        // 
        // panelTop
        // 
        panelTop.BackColor = System.Drawing.Color.FromArgb(20, 20, 24);
        panelTop.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
        panelTop.Controls.Add(panelStats);
        panelTop.Controls.Add(lblServerStatus);
        panelTop.Controls.Add(lblSubtitle);
        panelTop.Controls.Add(lblTitle);
        panelTop.Controls.Add(picLogo);
        panelTop.Dock = System.Windows.Forms.DockStyle.Top;
        panelTop.Location = new System.Drawing.Point(0, 0);
        panelTop.Name = "panelTop";
        panelTop.Size = new System.Drawing.Size(1160, 85);
        panelTop.TabIndex = 0;

        // 
        // panelStats
        // 
        panelStats.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
        panelStats.BackColor = System.Drawing.Color.FromArgb(28, 28, 34);
        panelStats.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
        panelStats.Controls.Add(lblBlockedCount);
        panelStats.Controls.Add(lblExpiredCount);
        panelStats.Controls.Add(lblActiveCount);
        panelStats.Controls.Add(lblTotalAccounts);
        panelStats.Location = new System.Drawing.Point(580, 10);
        panelStats.Name = "panelStats";
        panelStats.Size = new System.Drawing.Size(565, 62);
        panelStats.TabIndex = 4;

        // 
        // lblBlockedCount
        // 
        lblBlockedCount.Font = new System.Drawing.Font("Segoe UI", 9.5F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
        lblBlockedCount.ForeColor = System.Drawing.Color.FromArgb(240, 80, 80);
        lblBlockedCount.Location = new System.Drawing.Point(8, 20);
        lblBlockedCount.Name = "lblBlockedCount";
        lblBlockedCount.Size = new System.Drawing.Size(125, 22);
        lblBlockedCount.TabIndex = 3;
        lblBlockedCount.Text = "🚫 محظور: 0";
        lblBlockedCount.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;

        // 
        // lblExpiredCount
        // 
        lblExpiredCount.Font = new System.Drawing.Font("Segoe UI", 9.5F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
        lblExpiredCount.ForeColor = System.Drawing.Color.FromArgb(255, 140, 60);
        lblExpiredCount.Location = new System.Drawing.Point(135, 20);
        lblExpiredCount.Name = "lblExpiredCount";
        lblExpiredCount.Size = new System.Drawing.Size(130, 22);
        lblExpiredCount.TabIndex = 2;
        lblExpiredCount.Text = "⏳ منتهي: 0";
        lblExpiredCount.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;

        // 
        // lblActiveCount
        // 
        lblActiveCount.Font = new System.Drawing.Font("Segoe UI", 9.5F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
        lblActiveCount.ForeColor = System.Drawing.Color.FromArgb(70, 210, 120);
        lblActiveCount.Location = new System.Drawing.Point(270, 20);
        lblActiveCount.Name = "lblActiveCount";
        lblActiveCount.Size = new System.Drawing.Size(130, 22);
        lblActiveCount.TabIndex = 1;
        lblActiveCount.Text = "🟢 نشط: 0";
        lblActiveCount.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;

        // 
        // lblTotalAccounts
        // 
        lblTotalAccounts.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
        lblTotalAccounts.ForeColor = System.Drawing.Color.FromArgb(0, 190, 255);
        lblTotalAccounts.Location = new System.Drawing.Point(405, 20);
        lblTotalAccounts.Name = "lblTotalAccounts";
        lblTotalAccounts.Size = new System.Drawing.Size(150, 22);
        lblTotalAccounts.TabIndex = 0;
        lblTotalAccounts.Text = "👥 الحسابات: 0 / 16";
        lblTotalAccounts.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;

        // 
        // lblServerStatus
        // 
        lblServerStatus.AutoSize = true;
        lblServerStatus.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
        lblServerStatus.ForeColor = System.Drawing.Color.Orange;
        lblServerStatus.Location = new System.Drawing.Point(82, 50);
        lblServerStatus.Name = "lblServerStatus";
        lblServerStatus.Size = new System.Drawing.Size(126, 15);
        lblServerStatus.TabIndex = 3;
        lblServerStatus.Text = "السيرفر: جارٍ الفحص...";

        // 
        // lblSubtitle
        // 
        lblSubtitle.AutoSize = true;
        lblSubtitle.Font = new System.Drawing.Font("Segoe UI", 8.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
        lblSubtitle.ForeColor = System.Drawing.Color.Gray;
        lblSubtitle.Location = new System.Drawing.Point(260, 20);
        lblSubtitle.Name = "lblSubtitle";
        lblSubtitle.Size = new System.Drawing.Size(260, 15);
        lblSubtitle.TabIndex = 2;
        lblSubtitle.Text = "لوحة التحكم الشاملة - إدارة الاشتراكات باليوم والشهر والأجهزة";

        // 
        // lblTitle
        // 
        lblTitle.AutoSize = true;
        lblTitle.Font = new System.Drawing.Font("Segoe UI", 15F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
        lblTitle.ForeColor = System.Drawing.Color.White;
        lblTitle.Location = new System.Drawing.Point(80, 12);
        lblTitle.Name = "lblTitle";
        lblTitle.Size = new System.Drawing.Size(175, 28);
        lblTitle.TabIndex = 1;
        lblTitle.Text = "WOLVES ADMIN";

        // 
        // picLogo
        // 
        picLogo.Location = new System.Drawing.Point(14, 12);
        picLogo.Name = "picLogo";
        picLogo.Size = new System.Drawing.Size(58, 58);
        picLogo.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
        picLogo.TabIndex = 0;
        picLogo.TabStop = false;

        // 
        // tabControl
        // 
        tabControl.Controls.Add(tabAccounts);
        tabControl.Controls.Add(tabLogs);
        tabControl.Controls.Add(tabSettings);
        tabControl.Controls.Add(tabUpdates);
        tabControl.Controls.Add(tabPermissions);
        tabControl.Dock = System.Windows.Forms.DockStyle.Fill;
        tabControl.Font = new System.Drawing.Font("Segoe UI", 9.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
        tabControl.Location = new System.Drawing.Point(0, 85);
        tabControl.Name = "tabControl";
        tabControl.SelectedIndex = 0;
        tabControl.Size = new System.Drawing.Size(1160, 560);
        tabControl.TabIndex = 1;

        // 
        // tabAccounts
        // 
        tabAccounts.BackColor = System.Drawing.Color.FromArgb(28, 29, 33);
        tabAccounts.Controls.Add(dgvAccounts);
        tabAccounts.Controls.Add(panelAccountActions);
        tabAccounts.Location = new System.Drawing.Point(4, 26);
        tabAccounts.Name = "tabAccounts";
        tabAccounts.Padding = new System.Windows.Forms.Padding(6);
        tabAccounts.Size = new System.Drawing.Size(1152, 530);
        tabAccounts.TabIndex = 0;
        tabAccounts.Text = "📋 إدارة الحسابات والاشتراكات (Accounts)";

        // 
        // dgvAccounts
        // 
        dgvAccounts.AllowUserToAddRows = false;
        dgvAccounts.AllowUserToDeleteRows = false;
        dgvAccounts.AllowUserToResizeRows = false;
        dgvAccounts.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
        dgvAccounts.BackgroundColor = System.Drawing.Color.FromArgb(24, 25, 29);
        dgvAccounts.BorderStyle = System.Windows.Forms.BorderStyle.None;
        dgvAccounts.ColumnHeadersBorderStyle = System.Windows.Forms.DataGridViewHeaderBorderStyle.Single;
        dgvAccountsHeaderStyle.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
        dgvAccountsHeaderStyle.BackColor = System.Drawing.Color.FromArgb(36, 38, 44);
        dgvAccountsHeaderStyle.Font = new System.Drawing.Font("Segoe UI", 9.5F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
        dgvAccountsHeaderStyle.ForeColor = System.Drawing.Color.White;
        dgvAccountsHeaderStyle.SelectionBackColor = System.Drawing.Color.FromArgb(48, 50, 58);
        dgvAccountsHeaderStyle.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
        dgvAccountsHeaderStyle.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
        dgvAccounts.ColumnHeadersDefaultCellStyle = dgvAccountsHeaderStyle;
        dgvAccounts.ColumnHeadersHeight = 35;
        dgvAccounts.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            colIndex,
            colUser,
            colStatus,
            colSubscription,
            colExpiry,
            colReason,
            colHwid,
            colRegIp,
            colLastIp,
            colCreated,
            colLastLogin,
            colNotes
        });
        dgvAccounts.ContextMenuStrip = cmsAccounts;
        dgvAccountsRowStyle.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
        dgvAccountsRowStyle.BackColor = System.Drawing.Color.FromArgb(28, 29, 33);
        dgvAccountsRowStyle.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
        dgvAccountsRowStyle.ForeColor = System.Drawing.Color.Gainsboro;
        dgvAccountsRowStyle.SelectionBackColor = System.Drawing.Color.FromArgb(0, 122, 204);
        dgvAccountsRowStyle.SelectionForeColor = System.Drawing.Color.White;
        dgvAccountsRowStyle.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
        dgvAccounts.DefaultCellStyle = dgvAccountsRowStyle;
        dgvAccounts.Dock = System.Windows.Forms.DockStyle.Fill;
        dgvAccounts.EnableHeadersVisualStyles = false;
        dgvAccounts.GridColor = System.Drawing.Color.FromArgb(44, 46, 54);
        dgvAccounts.Location = new System.Drawing.Point(6, 6);
        dgvAccounts.MultiSelect = false;
        dgvAccounts.Name = "dgvAccounts";
        dgvAccounts.ReadOnly = true;
        dgvAccounts.RowHeadersVisible = false;
        dgvAccounts.RowTemplate.Height = 30;
        dgvAccounts.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
        dgvAccounts.Size = new System.Drawing.Size(1140, 420);
        dgvAccounts.TabIndex = 0;

        // 
        // colIndex
        // 
        colIndex.FillWeight = 30F;
        colIndex.HeaderText = "#";
        colIndex.Name = "colIndex";
        colIndex.ReadOnly = true;

        // 
        // colUser
        // 
        colUser.FillWeight = 85F;
        colUser.HeaderText = "اسم المستخدم";
        colUser.Name = "colUser";
        colUser.ReadOnly = true;

        // 
        // colStatus
        // 
        colStatus.FillWeight = 65F;
        colStatus.HeaderText = "الحالة";
        colStatus.Name = "colStatus";
        colStatus.ReadOnly = true;

        // 
        // colSubscription
        // 
        colSubscription.FillWeight = 115F;
        colSubscription.HeaderText = "الاشتراك / الوقت المتبقي";
        colSubscription.Name = "colSubscription";
        colSubscription.ReadOnly = true;

        // 
        // colExpiry
        // 
        colExpiry.FillWeight = 100F;
        colExpiry.HeaderText = "تاريخ الانتهاء";
        colExpiry.Name = "colExpiry";
        colExpiry.ReadOnly = true;

        // 
        // colReason
        // 
        colReason.FillWeight = 120F;
        colReason.HeaderText = "سبب الحظر";
        colReason.Name = "colReason";
        colReason.ReadOnly = true;

        // 
        // colHwid
        // 
        colHwid.FillWeight = 120F;
        colHwid.HeaderText = "بصمة الجهاز (HWID)";
        colHwid.Name = "colHwid";
        colHwid.ReadOnly = true;

        // 
        // colRegIp
        // 
        colRegIp.FillWeight = 80F;
        colRegIp.HeaderText = "IP التسجيل";
        colRegIp.Name = "colRegIp";
        colRegIp.ReadOnly = true;

        // 
        // colLastIp
        // 
        colLastIp.FillWeight = 80F;
        colLastIp.HeaderText = "آخر IP";
        colLastIp.Name = "colLastIp";
        colLastIp.ReadOnly = true;

        // 
        // colCreated
        // 
        colCreated.FillWeight = 95F;
        colCreated.HeaderText = "تاريخ التسجيل";
        colCreated.Name = "colCreated";
        colCreated.ReadOnly = true;

        // 
        // colLastLogin
        // 
        colLastLogin.FillWeight = 95F;
        colLastLogin.HeaderText = "آخر دخول";
        colLastLogin.Name = "colLastLogin";
        colLastLogin.ReadOnly = true;

        // 
        // colNotes
        // 
        colNotes.FillWeight = 80F;
        colNotes.HeaderText = "ملاحظات";
        colNotes.Name = "colNotes";
        colNotes.ReadOnly = true;

        // 
        // cmsAccounts
        // 
        cmsAccounts.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            tsmiExtendSub,
            tsmiLifetime,
            tsmiExpireNow,
            tsmiSep1,
            tsmiResetHwid,
            tsmiChangePassword,
            tsmiBlock,
            tsmiUnblock,
            tsmiSep2,
            tsmiDeleteAccount,
            tsmiToggleAdmin
        });
        cmsAccounts.Name = "cmsAccounts";
        cmsAccounts.Size = new System.Drawing.Size(225, 230);

        // 
        // tsmiExtendSub
        // 
        tsmiExtendSub.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            tsmiAdd1Day,
            tsmiAdd7Days,
            tsmiAdd30Days,
            tsmiAdd60Days,
            tsmiAdd90Days,
            tsmiAdd365Days,
            tsmiAddCustom,
            tsmiSetDate
        });
        tsmiExtendSub.Text = "⏱️ تمديد مدة الاشتراك";

        tsmiAdd1Day.Text = "➕ إضافة 1 يوم (+1 Day)";
        tsmiAdd1Day.Click += btnAdd1Day_Click;

        tsmiAdd7Days.Text = "➕ إضافة 7 أيام (أسبوع)";
        tsmiAdd7Days.Click += btnAdd7Days_Click;

        tsmiAdd30Days.Text = "➕ إضافة 30 يوم (شهر)";
        tsmiAdd30Days.Click += btnAdd30Days_Click;

        tsmiAdd60Days.Text = "➕ إضافة شهرين (60 يوم)";
        tsmiAdd60Days.Click += btnAdd60Days_Click;

        tsmiAdd90Days.Text = "➕ إضافة 3 شهور (90 يوم)";
        tsmiAdd90Days.Click += btnAdd90Days_Click;

        tsmiAdd365Days.Text = "➕ إضافة سنة كاملة (365 يوم)";
        tsmiAdd365Days.Click += tsmiAdd365Days_Click;

        tsmiAddCustom.Text = "📅 تحديد عدد مخصص (أيام أو شهور)...";
        tsmiAddCustom.Click += btnAddCustom_Click;

        tsmiSetDate.Text = "⏱️ تحديد تاريخ ووقت انتهاء محدد...";
        tsmiSetDate.Click += btnSetDate_Click;

        tsmiLifetime.Text = "♾️ تفعيل اشتراك دائم (Lifetime)";
        tsmiLifetime.Click += btnLifetime_Click;

        tsmiExpireNow.Text = "⏳ إنهاء الاشتراك فوراً (Expire Now)";
        tsmiExpireNow.Click += btnExpireNow_Click;

        tsmiResetHwid.Text = "🔄 فك ربط بصمة الجهاز (Reset HWID)";
        tsmiResetHwid.Click += btnResetHwid_Click;

        tsmiChangePassword.Text = "🔑 تغيير كلمة المرور";
        tsmiChangePassword.Click += btnChangePassword_Click;

        tsmiBlock.Text = "🔒 حظر الحساب";
        tsmiBlock.Click += btnBlock_Click;

        tsmiUnblock.Text = "🔓 فك الحظر";
        tsmiUnblock.Click += btnUnblock_Click;

        tsmiDeleteAccount.Text = "🗑️ حذف الحساب نهائياً";
        tsmiDeleteAccount.Click += btnDeleteAccount_Click;

        tsmiToggleAdmin.Text = "⭐ تعيين / إلغاء صلاحيات الأدمن";
        tsmiToggleAdmin.Click += btnToggleAdmin_Click;

        // 
        // panelAccountActions
        // 
        panelAccountActions.BackColor = System.Drawing.Color.FromArgb(22, 23, 27);
        panelAccountActions.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
        panelAccountActions.Controls.Add(lblSubHeader);
        panelAccountActions.Controls.Add(btnAdd1Day);
        panelAccountActions.Controls.Add(btnAdd7Days);
        panelAccountActions.Controls.Add(btnAdd30Days);
        panelAccountActions.Controls.Add(btnAdd60Days);
        panelAccountActions.Controls.Add(btnAdd90Days);
        panelAccountActions.Controls.Add(btnAddCustom);
        panelAccountActions.Controls.Add(btnSetDate);
        panelAccountActions.Controls.Add(btnLifetime);
        panelAccountActions.Controls.Add(btnExpireNow);

        panelAccountActions.Controls.Add(btnRefresh);
        panelAccountActions.Controls.Add(btnUnblock);
        panelAccountActions.Controls.Add(btnBlock);
        panelAccountActions.Controls.Add(btnResetHwid);
        panelAccountActions.Controls.Add(btnChangePassword);
        panelAccountActions.Controls.Add(btnAddAccount);
        panelAccountActions.Controls.Add(btnDeleteAccount);
        panelAccountActions.Controls.Add(btnToggleAdmin);
        panelAccountActions.Controls.Add(chkAutoRefresh);

        panelAccountActions.Dock = System.Windows.Forms.DockStyle.Bottom;
        panelAccountActions.Location = new System.Drawing.Point(6, 426);
        panelAccountActions.Name = "panelAccountActions";
        panelAccountActions.Size = new System.Drawing.Size(1140, 98);
        panelAccountActions.TabIndex = 1;

        // --- ROW 1: SUBSCRIPTION ACTIONS (اليوم والشهر) ---
        lblSubHeader.AutoSize = true;
        lblSubHeader.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
        lblSubHeader.ForeColor = System.Drawing.Color.Gold;
        lblSubHeader.Location = new System.Drawing.Point(8, 14);
        lblSubHeader.Name = "lblSubHeader";
        lblSubHeader.Size = new System.Drawing.Size(78, 15);
        lblSubHeader.Text = "⏱️ مدة الاشتراك:";

        btnAdd1Day.BackColor = System.Drawing.Color.FromArgb(38, 70, 83);
        btnAdd1Day.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
        btnAdd1Day.Font = new System.Drawing.Font("Segoe UI", 8.5F, System.Drawing.FontStyle.Bold);
        btnAdd1Day.ForeColor = System.Drawing.Color.White;
        btnAdd1Day.Location = new System.Drawing.Point(92, 7);
        btnAdd1Day.Name = "btnAdd1Day";
        btnAdd1Day.Size = new System.Drawing.Size(72, 30);
        btnAdd1Day.TabIndex = 10;
        btnAdd1Day.Text = "+1 يوم";
        btnAdd1Day.UseVisualStyleBackColor = false;
        btnAdd1Day.Click += btnAdd1Day_Click;

        btnAdd7Days.BackColor = System.Drawing.Color.FromArgb(42, 100, 100);
        btnAdd7Days.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
        btnAdd7Days.Font = new System.Drawing.Font("Segoe UI", 8.5F, System.Drawing.FontStyle.Bold);
        btnAdd7Days.ForeColor = System.Drawing.Color.White;
        btnAdd7Days.Location = new System.Drawing.Point(168, 7);
        btnAdd7Days.Name = "btnAdd7Days";
        btnAdd7Days.Size = new System.Drawing.Size(72, 30);
        btnAdd7Days.TabIndex = 11;
        btnAdd7Days.Text = "+7 أيام";
        btnAdd7Days.UseVisualStyleBackColor = false;
        btnAdd7Days.Click += btnAdd7Days_Click;

        btnAdd30Days.BackColor = System.Drawing.Color.FromArgb(0, 120, 130);
        btnAdd30Days.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
        btnAdd30Days.Font = new System.Drawing.Font("Segoe UI", 8.5F, System.Drawing.FontStyle.Bold);
        btnAdd30Days.ForeColor = System.Drawing.Color.White;
        btnAdd30Days.Location = new System.Drawing.Point(244, 7);
        btnAdd30Days.Name = "btnAdd30Days";
        btnAdd30Days.Size = new System.Drawing.Size(108, 30);
        btnAdd30Days.TabIndex = 12;
        btnAdd30Days.Text = "+1 شهر (30 يوم)";
        btnAdd30Days.UseVisualStyleBackColor = false;
        btnAdd30Days.Click += btnAdd30Days_Click;

        btnAdd60Days.BackColor = System.Drawing.Color.FromArgb(0, 140, 120);
        btnAdd60Days.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
        btnAdd60Days.Font = new System.Drawing.Font("Segoe UI", 8.5F, System.Drawing.FontStyle.Bold);
        btnAdd60Days.ForeColor = System.Drawing.Color.White;
        btnAdd60Days.Location = new System.Drawing.Point(356, 7);
        btnAdd60Days.Name = "btnAdd60Days";
        btnAdd60Days.Size = new System.Drawing.Size(108, 30);
        btnAdd60Days.TabIndex = 13;
        btnAdd60Days.Text = "+ شهرين (60 يوم)";
        btnAdd60Days.UseVisualStyleBackColor = false;
        btnAdd60Days.Click += btnAdd60Days_Click;

        btnAdd90Days.BackColor = System.Drawing.Color.FromArgb(46, 125, 50);
        btnAdd90Days.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
        btnAdd90Days.Font = new System.Drawing.Font("Segoe UI", 8.5F, System.Drawing.FontStyle.Bold);
        btnAdd90Days.ForeColor = System.Drawing.Color.White;
        btnAdd90Days.Location = new System.Drawing.Point(468, 7);
        btnAdd90Days.Name = "btnAdd90Days";
        btnAdd90Days.Size = new System.Drawing.Size(80, 30);
        btnAdd90Days.TabIndex = 14;
        btnAdd90Days.Text = "+3 شهور";
        btnAdd90Days.UseVisualStyleBackColor = false;
        btnAdd90Days.Click += btnAdd90Days_Click;

        btnAddCustom.BackColor = System.Drawing.Color.FromArgb(100, 70, 150);
        btnAddCustom.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
        btnAddCustom.Font = new System.Drawing.Font("Segoe UI", 8.5F, System.Drawing.FontStyle.Bold);
        btnAddCustom.ForeColor = System.Drawing.Color.White;
        btnAddCustom.Location = new System.Drawing.Point(552, 7);
        btnAddCustom.Name = "btnAddCustom";
        btnAddCustom.Size = new System.Drawing.Size(170, 30);
        btnAddCustom.TabIndex = 15;
        btnAddCustom.Text = "➕ مدة مخصصة (أيام/شهور)...";
        btnAddCustom.UseVisualStyleBackColor = false;
        btnAddCustom.Click += btnAddCustom_Click;

        btnSetDate.BackColor = System.Drawing.Color.FromArgb(70, 80, 140);
        btnSetDate.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
        btnSetDate.Font = new System.Drawing.Font("Segoe UI", 8.5F, System.Drawing.FontStyle.Regular);
        btnSetDate.ForeColor = System.Drawing.Color.White;
        btnSetDate.Location = new System.Drawing.Point(726, 7);
        btnSetDate.Name = "btnSetDate";
        btnSetDate.Size = new System.Drawing.Size(110, 30);
        btnSetDate.TabIndex = 16;
        btnSetDate.Text = "📅 تاريخ محدد...";
        btnSetDate.UseVisualStyleBackColor = false;
        btnSetDate.Click += btnSetDate_Click;

        btnLifetime.BackColor = System.Drawing.Color.FromArgb(180, 100, 30);
        btnLifetime.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
        btnLifetime.Font = new System.Drawing.Font("Segoe UI", 8.5F, System.Drawing.FontStyle.Bold);
        btnLifetime.ForeColor = System.Drawing.Color.White;
        btnLifetime.Location = new System.Drawing.Point(840, 7);
        btnLifetime.Name = "btnLifetime";
        btnLifetime.Size = new System.Drawing.Size(115, 30);
        btnLifetime.TabIndex = 17;
        btnLifetime.Text = "♾️ دائم (Lifetime)";
        btnLifetime.UseVisualStyleBackColor = false;
        btnLifetime.Click += btnLifetime_Click;

        btnExpireNow.BackColor = System.Drawing.Color.FromArgb(150, 45, 45);
        btnExpireNow.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
        btnExpireNow.Font = new System.Drawing.Font("Segoe UI", 8.5F, System.Drawing.FontStyle.Bold);
        btnExpireNow.ForeColor = System.Drawing.Color.White;
        btnExpireNow.Location = new System.Drawing.Point(959, 7);
        btnExpireNow.Name = "btnExpireNow";
        btnExpireNow.Size = new System.Drawing.Size(95, 30);
        btnExpireNow.TabIndex = 18;
        btnExpireNow.Text = "⏳ إنهاء الآن";
        btnExpireNow.UseVisualStyleBackColor = false;
        btnExpireNow.Click += btnExpireNow_Click;

        // --- ROW 2: ACCOUNT MANAGEMENT ACTIONS ---
        btnRefresh.BackColor = System.Drawing.Color.FromArgb(50, 54, 62);
        btnRefresh.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
        btnRefresh.Font = new System.Drawing.Font("Segoe UI", 9F);
        btnRefresh.ForeColor = System.Drawing.Color.White;
        btnRefresh.Location = new System.Drawing.Point(8, 48);
        btnRefresh.Name = "btnRefresh";
        btnRefresh.Size = new System.Drawing.Size(76, 32);
        btnRefresh.TabIndex = 0;
        btnRefresh.Text = "🔄 تحديث";
        btnRefresh.UseVisualStyleBackColor = false;
        btnRefresh.Click += btnRefresh_Click;

        btnUnblock.BackColor = System.Drawing.Color.FromArgb(35, 130, 70);
        btnUnblock.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
        btnUnblock.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
        btnUnblock.ForeColor = System.Drawing.Color.White;
        btnUnblock.Location = new System.Drawing.Point(88, 48);
        btnUnblock.Name = "btnUnblock";
        btnUnblock.Size = new System.Drawing.Size(90, 32);
        btnUnblock.TabIndex = 1;
        btnUnblock.Text = "🔓 فك الحظر";
        btnUnblock.UseVisualStyleBackColor = false;
        btnUnblock.Click += btnUnblock_Click;

        btnBlock.BackColor = System.Drawing.Color.FromArgb(140, 50, 50);
        btnBlock.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
        btnBlock.Font = new System.Drawing.Font("Segoe UI", 9F);
        btnBlock.ForeColor = System.Drawing.Color.White;
        btnBlock.Location = new System.Drawing.Point(182, 48);
        btnBlock.Name = "btnBlock";
        btnBlock.Size = new System.Drawing.Size(80, 32);
        btnBlock.TabIndex = 2;
        btnBlock.Text = "🔒 حظر";
        btnBlock.UseVisualStyleBackColor = false;
        btnBlock.Click += btnBlock_Click;

        btnResetHwid.BackColor = System.Drawing.Color.FromArgb(180, 120, 30);
        btnResetHwid.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
        btnResetHwid.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
        btnResetHwid.ForeColor = System.Drawing.Color.White;
        btnResetHwid.Location = new System.Drawing.Point(266, 48);
        btnResetHwid.Name = "btnResetHwid";
        btnResetHwid.Size = new System.Drawing.Size(140, 32);
        btnResetHwid.TabIndex = 3;
        btnResetHwid.Text = "🔄 فك ربط الجهاز (Reset)";
        btnResetHwid.UseVisualStyleBackColor = false;
        btnResetHwid.Click += btnResetHwid_Click;

        btnChangePassword.BackColor = System.Drawing.Color.FromArgb(60, 64, 72);
        btnChangePassword.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
        btnChangePassword.Font = new System.Drawing.Font("Segoe UI", 9F);
        btnChangePassword.ForeColor = System.Drawing.Color.White;
        btnChangePassword.Location = new System.Drawing.Point(410, 48);
        btnChangePassword.Name = "btnChangePassword";
        btnChangePassword.Size = new System.Drawing.Size(120, 32);
        btnChangePassword.TabIndex = 4;
        btnChangePassword.Text = "🔑 كلمة المرور";
        btnChangePassword.UseVisualStyleBackColor = false;
        btnChangePassword.Click += btnChangePassword_Click;

        btnAddAccount.BackColor = System.Drawing.Color.FromArgb(40, 110, 180);
        btnAddAccount.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
        btnAddAccount.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
        btnAddAccount.ForeColor = System.Drawing.Color.White;
        btnAddAccount.Location = new System.Drawing.Point(534, 48);
        btnAddAccount.Name = "btnAddAccount";
        btnAddAccount.Size = new System.Drawing.Size(120, 32);
        btnAddAccount.TabIndex = 5;
        btnAddAccount.Text = "➕ إضافة حساب";
        btnAddAccount.UseVisualStyleBackColor = false;
        btnAddAccount.Click += btnAddAccount_Click;

        btnDeleteAccount.BackColor = System.Drawing.Color.FromArgb(160, 40, 40);
        btnDeleteAccount.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
        btnDeleteAccount.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
        btnDeleteAccount.ForeColor = System.Drawing.Color.White;
        btnDeleteAccount.Location = new System.Drawing.Point(658, 48);
        btnDeleteAccount.Name = "btnDeleteAccount";
        btnDeleteAccount.Size = new System.Drawing.Size(110, 32);
        btnDeleteAccount.TabIndex = 6;
        btnDeleteAccount.Text = "🗑️ حذف الحساب";
        btnDeleteAccount.UseVisualStyleBackColor = false;
        btnDeleteAccount.Click += btnDeleteAccount_Click;

        btnToggleAdmin.BackColor = System.Drawing.Color.FromArgb(170, 115, 20);
        btnToggleAdmin.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
        btnToggleAdmin.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
        btnToggleAdmin.ForeColor = System.Drawing.Color.White;
        btnToggleAdmin.Location = new System.Drawing.Point(772, 48);
        btnToggleAdmin.Name = "btnToggleAdmin";
        btnToggleAdmin.Size = new System.Drawing.Size(155, 32);
        btnToggleAdmin.TabIndex = 7;
        btnToggleAdmin.Text = "⭐ تعيين / إلغاء مسؤول";
        btnToggleAdmin.UseVisualStyleBackColor = false;
        btnToggleAdmin.Click += btnToggleAdmin_Click;

        chkAutoRefresh.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
        chkAutoRefresh.AutoSize = true;
        chkAutoRefresh.Checked = true;
        chkAutoRefresh.CheckState = System.Windows.Forms.CheckState.Checked;
        chkAutoRefresh.Font = new System.Drawing.Font("Segoe UI", 9F);
        chkAutoRefresh.ForeColor = System.Drawing.Color.Silver;
        chkAutoRefresh.Location = new System.Drawing.Point(970, 56);
        chkAutoRefresh.Name = "chkAutoRefresh";
        chkAutoRefresh.Size = new System.Drawing.Size(142, 19);
        chkAutoRefresh.TabIndex = 7;
        chkAutoRefresh.Text = "تحديث تلقائي (10 ثوانٍ)";
        chkAutoRefresh.UseVisualStyleBackColor = true;

        // 
        // tabLogs
        // 
        tabLogs.BackColor = System.Drawing.Color.FromArgb(28, 29, 33);
        tabLogs.Controls.Add(dgvLogs);
        tabLogs.Controls.Add(panelLogActions);
        tabLogs.Location = new System.Drawing.Point(4, 26);
        tabLogs.Name = "tabLogs";
        tabLogs.Padding = new System.Windows.Forms.Padding(6);
        tabLogs.Size = new System.Drawing.Size(1152, 530);
        tabLogs.TabIndex = 1;
        tabLogs.Text = "🚨 سجل الأمان والعمليات (Audit Logs)";

        // 
        // dgvLogs
        // 
        dgvLogs.AllowUserToAddRows = false;
        dgvLogs.AllowUserToDeleteRows = false;
        dgvLogs.AllowUserToResizeRows = false;
        dgvLogs.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
        dgvLogs.BackgroundColor = System.Drawing.Color.FromArgb(24, 25, 29);
        dgvLogs.BorderStyle = System.Windows.Forms.BorderStyle.None;
        dgvLogs.ColumnHeadersBorderStyle = System.Windows.Forms.DataGridViewHeaderBorderStyle.Single;
        dgvLogsHeaderStyle.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
        dgvLogsHeaderStyle.BackColor = System.Drawing.Color.FromArgb(36, 38, 44);
        dgvLogsHeaderStyle.Font = new System.Drawing.Font("Segoe UI", 9.5F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
        dgvLogsHeaderStyle.ForeColor = System.Drawing.Color.White;
        dgvLogsHeaderStyle.SelectionBackColor = System.Drawing.Color.FromArgb(48, 50, 58);
        dgvLogsHeaderStyle.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
        dgvLogsHeaderStyle.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
        dgvLogs.ColumnHeadersDefaultCellStyle = dgvLogsHeaderStyle;
        dgvLogs.ColumnHeadersHeight = 35;
        dgvLogs.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            colLogTime,
            colLogUser,
            colLogAction,
            colLogHwid,
            colLogIp,
            colLogDetails
        });
        dgvLogsRowStyle.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
        dgvLogsRowStyle.BackColor = System.Drawing.Color.FromArgb(28, 29, 33);
        dgvLogsRowStyle.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
        dgvLogsRowStyle.ForeColor = System.Drawing.Color.Gainsboro;
        dgvLogsRowStyle.SelectionBackColor = System.Drawing.Color.FromArgb(0, 122, 204);
        dgvLogsRowStyle.SelectionForeColor = System.Drawing.Color.White;
        dgvLogsRowStyle.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
        dgvLogs.DefaultCellStyle = dgvLogsRowStyle;
        dgvLogs.Dock = System.Windows.Forms.DockStyle.Fill;
        dgvLogs.EnableHeadersVisualStyles = false;
        dgvLogs.GridColor = System.Drawing.Color.FromArgb(44, 46, 54);
        dgvLogs.Location = new System.Drawing.Point(6, 6);
        dgvLogs.MultiSelect = false;
        dgvLogs.Name = "dgvLogs";
        dgvLogs.ReadOnly = true;
        dgvLogs.RowHeadersVisible = false;
        dgvLogs.RowTemplate.Height = 28;
        dgvLogs.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
        dgvLogs.Size = new System.Drawing.Size(1140, 466);
        dgvLogs.TabIndex = 0;

        colLogTime.FillWeight = 85F;
        colLogTime.HeaderText = "الوقت والتاريخ";
        colLogTime.Name = "colLogTime";
        colLogTime.ReadOnly = true;

        colLogUser.FillWeight = 80F;
        colLogUser.HeaderText = "اسم المستخدم";
        colLogUser.Name = "colLogUser";
        colLogUser.ReadOnly = true;

        colLogAction.FillWeight = 90F;
        colLogAction.HeaderText = "الحدث / العملية";
        colLogAction.Name = "colLogAction";
        colLogAction.ReadOnly = true;

        colLogHwid.FillWeight = 110F;
        colLogHwid.HeaderText = "HWID الجهاز";
        colLogHwid.Name = "colLogHwid";
        colLogHwid.ReadOnly = true;

        colLogIp.FillWeight = 75F;
        colLogIp.HeaderText = "عنوان IP";
        colLogIp.Name = "colLogIp";
        colLogIp.ReadOnly = true;

        colLogDetails.FillWeight = 160F;
        colLogDetails.HeaderText = "التفاصيل";
        colLogDetails.Name = "colLogDetails";
        colLogDetails.ReadOnly = true;

        panelLogActions.BackColor = System.Drawing.Color.FromArgb(22, 23, 27);
        panelLogActions.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
        panelLogActions.Controls.Add(btnRefreshLogs);
        panelLogActions.Dock = System.Windows.Forms.DockStyle.Bottom;
        panelLogActions.Location = new System.Drawing.Point(6, 472);
        panelLogActions.Name = "panelLogActions";
        panelLogActions.Size = new System.Drawing.Size(1140, 52);
        panelLogActions.TabIndex = 1;

        btnRefreshLogs.BackColor = System.Drawing.Color.FromArgb(50, 54, 62);
        btnRefreshLogs.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
        btnRefreshLogs.Font = new System.Drawing.Font("Segoe UI", 9F);
        btnRefreshLogs.ForeColor = System.Drawing.Color.White;
        btnRefreshLogs.Location = new System.Drawing.Point(10, 9);
        btnRefreshLogs.Name = "btnRefreshLogs";
        btnRefreshLogs.Size = new System.Drawing.Size(140, 32);
        btnRefreshLogs.TabIndex = 0;
        btnRefreshLogs.Text = "🔄 تحديث سجل الأمان";
        btnRefreshLogs.UseVisualStyleBackColor = false;
        btnRefreshLogs.Click += btnRefreshLogs_Click;

        // 
        // tabSettings
        // 
        tabSettings.BackColor = System.Drawing.Color.FromArgb(28, 29, 33);
        tabSettings.Controls.Add(panelWhatsAppCard);
        tabSettings.Controls.Add(panelSettingsCard);
        tabSettings.Location = new System.Drawing.Point(4, 26);
        tabSettings.Name = "tabSettings";
        tabSettings.Padding = new System.Windows.Forms.Padding(6);
        tabSettings.Size = new System.Drawing.Size(1152, 530);
        tabSettings.TabIndex = 2;
        tabSettings.Text = "⚙️ إعدادات السيرفر والواتساب (Settings)";

        // 
        // panelSettingsCard
        // 
        panelSettingsCard.BackColor = System.Drawing.Color.FromArgb(34, 35, 40);
        panelSettingsCard.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
        panelSettingsCard.Controls.Add(lblInstructions);
        panelSettingsCard.Controls.Add(btnSaveSettings);
        panelSettingsCard.Controls.Add(btnTestConnection);
        panelSettingsCard.Controls.Add(numMaxAccounts);
        panelSettingsCard.Controls.Add(lblMaxAccounts);
        panelSettingsCard.Controls.Add(txtAuthSecret);
        panelSettingsCard.Controls.Add(lblAuthSecret);
        panelSettingsCard.Controls.Add(txtFirebaseUrl);
        panelSettingsCard.Controls.Add(lblFirebaseUrl);
        panelSettingsCard.Location = new System.Drawing.Point(20, 20);
        panelSettingsCard.Name = "panelSettingsCard";
        panelSettingsCard.Size = new System.Drawing.Size(590, 470);
        panelSettingsCard.TabIndex = 0;

        lblFirebaseUrl.AutoSize = true;
        lblFirebaseUrl.Font = new System.Drawing.Font("Segoe UI", 9.5F, System.Drawing.FontStyle.Bold);
        lblFirebaseUrl.ForeColor = System.Drawing.Color.Gainsboro;
        lblFirebaseUrl.Location = new System.Drawing.Point(20, 20);
        lblFirebaseUrl.Name = "lblFirebaseUrl";
        lblFirebaseUrl.Size = new System.Drawing.Size(160, 17);
        lblFirebaseUrl.TabIndex = 0;
        lblFirebaseUrl.Text = "رابط Firebase Database:";

        txtFirebaseUrl.BackColor = System.Drawing.Color.FromArgb(48, 50, 56);
        txtFirebaseUrl.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
        txtFirebaseUrl.Font = new System.Drawing.Font("Segoe UI", 10F);
        txtFirebaseUrl.ForeColor = System.Drawing.Color.White;
        txtFirebaseUrl.Location = new System.Drawing.Point(20, 45);
        txtFirebaseUrl.Name = "txtFirebaseUrl";
        txtFirebaseUrl.Size = new System.Drawing.Size(545, 25);
        txtFirebaseUrl.TabIndex = 1;

        lblAuthSecret.AutoSize = true;
        lblAuthSecret.Font = new System.Drawing.Font("Segoe UI", 9.5F, System.Drawing.FontStyle.Bold);
        lblAuthSecret.ForeColor = System.Drawing.Color.Gainsboro;
        lblAuthSecret.Location = new System.Drawing.Point(20, 85);
        lblAuthSecret.Name = "lblAuthSecret";
        lblAuthSecret.Size = new System.Drawing.Size(262, 17);
        lblAuthSecret.TabIndex = 2;
        lblAuthSecret.Text = "مفتاح المصادقة السري (Auth Secret) - اختياري:";

        txtAuthSecret.BackColor = System.Drawing.Color.FromArgb(48, 50, 56);
        txtAuthSecret.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
        txtAuthSecret.Font = new System.Drawing.Font("Segoe UI", 10F);
        txtAuthSecret.ForeColor = System.Drawing.Color.White;
        txtAuthSecret.Location = new System.Drawing.Point(20, 110);
        txtAuthSecret.Name = "txtAuthSecret";
        txtAuthSecret.Size = new System.Drawing.Size(545, 25);
        txtAuthSecret.TabIndex = 3;

        lblMaxAccounts.AutoSize = true;
        lblMaxAccounts.Font = new System.Drawing.Font("Segoe UI", 9.5F, System.Drawing.FontStyle.Bold);
        lblMaxAccounts.ForeColor = System.Drawing.Color.Gainsboro;
        lblMaxAccounts.Location = new System.Drawing.Point(20, 150);
        lblMaxAccounts.Name = "lblMaxAccounts";
        lblMaxAccounts.Size = new System.Drawing.Size(185, 17);
        lblMaxAccounts.TabIndex = 4;
        lblMaxAccounts.Text = "الحد الأقصى لعدد الحسابات:";

        numMaxAccounts.BackColor = System.Drawing.Color.FromArgb(48, 50, 56);
        numMaxAccounts.Font = new System.Drawing.Font("Segoe UI", 10F);
        numMaxAccounts.ForeColor = System.Drawing.Color.White;
        numMaxAccounts.Location = new System.Drawing.Point(20, 175);
        numMaxAccounts.Maximum = new decimal(new int[] { 100, 0, 0, 0 });
        numMaxAccounts.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
        numMaxAccounts.Name = "numMaxAccounts";
        numMaxAccounts.Size = new System.Drawing.Size(120, 25);
        numMaxAccounts.TabIndex = 5;
        numMaxAccounts.Value = new decimal(new int[] { 16, 0, 0, 0 });

        lblInstructions.Font = new System.Drawing.Font("Segoe UI", 8.5F);
        lblInstructions.ForeColor = System.Drawing.Color.FromArgb(240, 180, 80);
        lblInstructions.Location = new System.Drawing.Point(20, 220);
        lblInstructions.Name = "lblInstructions";
        lblInstructions.Size = new System.Drawing.Size(545, 75);
        lblInstructions.TabIndex = 8;
        lblInstructions.Text = "💡 ملاحظة هامة:\r\n1. ضع رابط Realtime Database من مشروعك في Firebase.\r\n2. تأكد من ضبط قواعد Realtime Database على read: true, write: true.\r\n3. عند الحفظ يتم المزامنة محلياً وعلى قاعدة البيانات السحابية مباشرة.";

        btnTestConnection.BackColor = System.Drawing.Color.FromArgb(0, 122, 204);
        btnTestConnection.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
        btnTestConnection.Font = new System.Drawing.Font("Segoe UI", 9.5F, System.Drawing.FontStyle.Bold);
        btnTestConnection.ForeColor = System.Drawing.Color.White;
        btnTestConnection.Location = new System.Drawing.Point(20, 310);
        btnTestConnection.Name = "btnTestConnection";
        btnTestConnection.Size = new System.Drawing.Size(160, 38);
        btnTestConnection.TabIndex = 6;
        btnTestConnection.Text = "🔌 اختبار الاتصال بالسيرفر";
        btnTestConnection.UseVisualStyleBackColor = false;
        btnTestConnection.Click += btnTestConnection_Click;

        btnSaveSettings.BackColor = System.Drawing.Color.FromArgb(40, 130, 80);
        btnSaveSettings.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
        btnSaveSettings.Font = new System.Drawing.Font("Segoe UI", 9.5F, System.Drawing.FontStyle.Bold);
        btnSaveSettings.ForeColor = System.Drawing.Color.White;
        btnSaveSettings.Location = new System.Drawing.Point(190, 310);
        btnSaveSettings.Name = "btnSaveSettings";
        btnSaveSettings.Size = new System.Drawing.Size(150, 38);
        btnSaveSettings.TabIndex = 7;
        btnSaveSettings.Text = "💾 حفظ إعدادات السيرفر";
        btnSaveSettings.UseVisualStyleBackColor = false;
        btnSaveSettings.Click += btnSaveSettings_Click;

        // 
        // panelWhatsAppCard
        // 
        panelWhatsAppCard.BackColor = System.Drawing.Color.FromArgb(34, 35, 40);
        panelWhatsAppCard.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
        panelWhatsAppCard.Controls.Add(lblWhatsAppHeader);
        panelWhatsAppCard.Controls.Add(lblWhatsAppNumber);
        panelWhatsAppCard.Controls.Add(txtWhatsAppNumber);
        panelWhatsAppCard.Controls.Add(lblWhatsAppUrl);
        panelWhatsAppCard.Controls.Add(txtWhatsAppUrl);
        panelWhatsAppCard.Controls.Add(lblRechargeMessage);
        panelWhatsAppCard.Controls.Add(txtRechargeMessage);
        panelWhatsAppCard.Controls.Add(btnSaveWhatsApp);
        panelWhatsAppCard.Controls.Add(lblWhatsAppHint);
        panelWhatsAppCard.Location = new System.Drawing.Point(625, 20);
        panelWhatsAppCard.Name = "panelWhatsAppCard";
        panelWhatsAppCard.Size = new System.Drawing.Size(500, 470);
        panelWhatsAppCard.TabIndex = 1;

        lblWhatsAppHeader.AutoSize = true;
        lblWhatsAppHeader.Font = new System.Drawing.Font("Segoe UI", 11F, System.Drawing.FontStyle.Bold);
        lblWhatsAppHeader.ForeColor = System.Drawing.Color.LightGreen;
        lblWhatsAppHeader.Location = new System.Drawing.Point(20, 15);
        lblWhatsAppHeader.Name = "lblWhatsAppHeader";
        lblWhatsAppHeader.Size = new System.Drawing.Size(275, 20);
        lblWhatsAppHeader.TabIndex = 0;
        lblWhatsAppHeader.Text = "📱 إعدادات الواتساب وتجديد وشحن البوت";

        lblWhatsAppNumber.AutoSize = true;
        lblWhatsAppNumber.Font = new System.Drawing.Font("Segoe UI", 9.5F, System.Drawing.FontStyle.Bold);
        lblWhatsAppNumber.ForeColor = System.Drawing.Color.Gainsboro;
        lblWhatsAppNumber.Location = new System.Drawing.Point(20, 55);
        lblWhatsAppNumber.Name = "lblWhatsAppNumber";
        lblWhatsAppNumber.Size = new System.Drawing.Size(225, 17);
        lblWhatsAppNumber.TabIndex = 1;
        lblWhatsAppNumber.Text = "رقم الواتساب (مع كود الدولة مثل مصر 20+):";

        txtWhatsAppNumber.BackColor = System.Drawing.Color.FromArgb(48, 50, 56);
        txtWhatsAppNumber.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
        txtWhatsAppNumber.Font = new System.Drawing.Font("Segoe UI", 10F);
        txtWhatsAppNumber.ForeColor = System.Drawing.Color.White;
        txtWhatsAppNumber.Location = new System.Drawing.Point(20, 78);
        txtWhatsAppNumber.Name = "txtWhatsAppNumber";
        txtWhatsAppNumber.Size = new System.Drawing.Size(455, 25);
        txtWhatsAppNumber.TabIndex = 2;
        txtWhatsAppNumber.Text = "+201150238481";

        lblWhatsAppUrl.AutoSize = true;
        lblWhatsAppUrl.Font = new System.Drawing.Font("Segoe UI", 9.5F, System.Drawing.FontStyle.Bold);
        lblWhatsAppUrl.ForeColor = System.Drawing.Color.Gainsboro;
        lblWhatsAppUrl.Location = new System.Drawing.Point(20, 120);
        lblWhatsAppUrl.Name = "lblWhatsAppUrl";
        lblWhatsAppUrl.Size = new System.Drawing.Size(185, 17);
        lblWhatsAppUrl.TabIndex = 3;
        lblWhatsAppUrl.Text = "رابط محادثة الواتساب المباشر:";

        txtWhatsAppUrl.BackColor = System.Drawing.Color.FromArgb(48, 50, 56);
        txtWhatsAppUrl.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
        txtWhatsAppUrl.Font = new System.Drawing.Font("Segoe UI", 10F);
        txtWhatsAppUrl.ForeColor = System.Drawing.Color.White;
        txtWhatsAppUrl.Location = new System.Drawing.Point(20, 143);
        txtWhatsAppUrl.Name = "txtWhatsAppUrl";
        txtWhatsAppUrl.Size = new System.Drawing.Size(455, 25);
        txtWhatsAppUrl.TabIndex = 4;
        txtWhatsAppUrl.Text = "https://wa.me/201150238481";

        lblRechargeMessage.AutoSize = true;
        lblRechargeMessage.Font = new System.Drawing.Font("Segoe UI", 9.5F, System.Drawing.FontStyle.Bold);
        lblRechargeMessage.ForeColor = System.Drawing.Color.Gainsboro;
        lblRechargeMessage.Location = new System.Drawing.Point(20, 185);
        lblRechargeMessage.Name = "lblRechargeMessage";
        lblRechargeMessage.Size = new System.Drawing.Size(265, 17);
        lblRechargeMessage.TabIndex = 5;
        lblRechargeMessage.Text = "رسالة التنبيه المعروضة للمستخدم عند انتهاء وقته:";

        txtRechargeMessage.BackColor = System.Drawing.Color.FromArgb(48, 50, 56);
        txtRechargeMessage.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
        txtRechargeMessage.Font = new System.Drawing.Font("Segoe UI", 10F);
        txtRechargeMessage.ForeColor = System.Drawing.Color.White;
        txtRechargeMessage.Location = new System.Drawing.Point(20, 208);
        txtRechargeMessage.Name = "txtRechargeMessage";
        txtRechargeMessage.Size = new System.Drawing.Size(455, 25);
        txtRechargeMessage.TabIndex = 6;
        txtRechargeMessage.Text = "برجاء إرسال رسالة لنا لإعادة شحن البوت";

        lblWhatsAppHint.Font = new System.Drawing.Font("Segoe UI", 8.5F);
        lblWhatsAppHint.ForeColor = System.Drawing.Color.FromArgb(140, 200, 255);
        lblWhatsAppHint.Location = new System.Drawing.Point(20, 250);
        lblWhatsAppHint.Name = "lblWhatsAppHint";
        lblWhatsAppHint.Size = new System.Drawing.Size(455, 80);
        lblWhatsAppHint.TabIndex = 7;
        lblWhatsAppHint.Text = "💬 ملاحظة:\r\nعند انتهاء اشتراك أي مستخدم (سواء انتهت الأيام أو الشهور المحددة)، يقوم البوت بالإغلاق التلقائي ويظهر للمستخدم نافذة تنبيه تحتوي رقم ورابط الواتساب وزر لفتح المحادثة ونسخ الرقم فوراً.";

        btnSaveWhatsApp.BackColor = System.Drawing.Color.FromArgb(46, 125, 50);
        btnSaveWhatsApp.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
        btnSaveWhatsApp.Font = new System.Drawing.Font("Segoe UI", 9.5F, System.Drawing.FontStyle.Bold);
        btnSaveWhatsApp.ForeColor = System.Drawing.Color.White;
        btnSaveWhatsApp.Location = new System.Drawing.Point(20, 350);
        btnSaveWhatsApp.Name = "btnSaveWhatsApp";
        btnSaveWhatsApp.Size = new System.Drawing.Size(200, 38);
        btnSaveWhatsApp.TabIndex = 8;
        btnSaveWhatsApp.Text = "💾 حفظ إعدادات الواتساب";
        btnSaveWhatsApp.UseVisualStyleBackColor = false;
        btnSaveWhatsApp.Click += btnSaveWhatsApp_Click;

        // 
        // tabUpdates
        // 
        tabUpdates.BackColor = System.Drawing.Color.FromArgb(28, 29, 33);
        tabUpdates.Controls.Add(panelUpdateCard);
        tabUpdates.Location = new System.Drawing.Point(4, 26);
        tabUpdates.Name = "tabUpdates";
        tabUpdates.Padding = new System.Windows.Forms.Padding(6);
        tabUpdates.Size = new System.Drawing.Size(1152, 530);
        tabUpdates.TabIndex = 3;
        tabUpdates.Text = "🚀 إدارة التحديثات التلقائية (Updates)";

        panelUpdateCard.BackColor = System.Drawing.Color.FromArgb(34, 35, 40);
        panelUpdateCard.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
        panelUpdateCard.Controls.Add(lblUpdateHeader);
        panelUpdateCard.Controls.Add(lblUpdateVersion);
        panelUpdateCard.Controls.Add(txtUpdateVersion);
        panelUpdateCard.Controls.Add(lblUpdateUrl);
        panelUpdateCard.Controls.Add(txtUpdateUrl);
        panelUpdateCard.Controls.Add(chkUpdateMandatory);
        panelUpdateCard.Controls.Add(lblChangelogAr);
        panelUpdateCard.Controls.Add(txtChangelogAr);
        panelUpdateCard.Controls.Add(lblChangelogEn);
        panelUpdateCard.Controls.Add(txtChangelogEn);
        panelUpdateCard.Controls.Add(btnPublishUpdate);
        panelUpdateCard.Controls.Add(btnCheckUpdate);
        panelUpdateCard.Controls.Add(lblUpdateStatus);
        panelUpdateCard.Location = new System.Drawing.Point(20, 20);
        panelUpdateCard.Name = "panelUpdateCard";
        panelUpdateCard.Size = new System.Drawing.Size(750, 480);
        panelUpdateCard.TabIndex = 0;

        lblUpdateHeader.AutoSize = true;
        lblUpdateHeader.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Bold);
        lblUpdateHeader.ForeColor = System.Drawing.Color.White;
        lblUpdateHeader.Location = new System.Drawing.Point(20, 15);
        lblUpdateHeader.Name = "lblUpdateHeader";
        lblUpdateHeader.Size = new System.Drawing.Size(315, 21);
        lblUpdateHeader.TabIndex = 0;
        lblUpdateHeader.Text = "نشر وتوزيع تحديث جديد للعملاء تلقائياً";

        lblUpdateVersion.AutoSize = true;
        lblUpdateVersion.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
        lblUpdateVersion.ForeColor = System.Drawing.Color.Gainsboro;
        lblUpdateVersion.Location = new System.Drawing.Point(20, 50);
        lblUpdateVersion.Name = "lblUpdateVersion";
        lblUpdateVersion.Size = new System.Drawing.Size(120, 15);
        lblUpdateVersion.TabIndex = 1;
        lblUpdateVersion.Text = "رقم الإصدار (Version):";

        txtUpdateVersion.BackColor = System.Drawing.Color.FromArgb(48, 50, 56);
        txtUpdateVersion.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
        txtUpdateVersion.Font = new System.Drawing.Font("Segoe UI", 10F);
        txtUpdateVersion.ForeColor = System.Drawing.Color.White;
        txtUpdateVersion.Location = new System.Drawing.Point(20, 70);
        txtUpdateVersion.Name = "txtUpdateVersion";
        txtUpdateVersion.Size = new System.Drawing.Size(200, 25);
        txtUpdateVersion.TabIndex = 2;

        lblUpdateUrl.AutoSize = true;
        lblUpdateUrl.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
        lblUpdateUrl.ForeColor = System.Drawing.Color.Gainsboro;
        lblUpdateUrl.Location = new System.Drawing.Point(20, 110);
        lblUpdateUrl.Name = "lblUpdateUrl";
        lblUpdateUrl.Size = new System.Drawing.Size(200, 15);
        lblUpdateUrl.TabIndex = 3;
        lblUpdateUrl.Text = "رابط تحميل التحديث المباشر (.zip):";

        txtUpdateUrl.BackColor = System.Drawing.Color.FromArgb(48, 50, 56);
        txtUpdateUrl.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
        txtUpdateUrl.Font = new System.Drawing.Font("Segoe UI", 10F);
        txtUpdateUrl.ForeColor = System.Drawing.Color.White;
        txtUpdateUrl.Location = new System.Drawing.Point(20, 130);
        txtUpdateUrl.Name = "txtUpdateUrl";
        txtUpdateUrl.Size = new System.Drawing.Size(700, 25);
        txtUpdateUrl.TabIndex = 4;

        chkUpdateMandatory.AutoSize = true;
        chkUpdateMandatory.Checked = true;
        chkUpdateMandatory.CheckState = System.Windows.Forms.CheckState.Checked;
        chkUpdateMandatory.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
        chkUpdateMandatory.ForeColor = System.Drawing.Color.FromArgb(255, 200, 80);
        chkUpdateMandatory.Location = new System.Drawing.Point(240, 72);
        chkUpdateMandatory.Name = "chkUpdateMandatory";
        chkUpdateMandatory.Size = new System.Drawing.Size(220, 19);
        chkUpdateMandatory.TabIndex = 5;
        chkUpdateMandatory.Text = "تحديث إجباري (Mandatory Update)";
        chkUpdateMandatory.UseVisualStyleBackColor = true;

        lblChangelogAr.AutoSize = true;
        lblChangelogAr.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
        lblChangelogAr.ForeColor = System.Drawing.Color.Gainsboro;
        lblChangelogAr.Location = new System.Drawing.Point(20, 170);
        lblChangelogAr.Name = "lblChangelogAr";
        lblChangelogAr.Size = new System.Drawing.Size(185, 15);
        lblChangelogAr.TabIndex = 6;
        lblChangelogAr.Text = "سجل التغييرات بالعربي (Changelog):";

        txtChangelogAr.BackColor = System.Drawing.Color.FromArgb(48, 50, 56);
        txtChangelogAr.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
        txtChangelogAr.Font = new System.Drawing.Font("Segoe UI", 9.5F);
        txtChangelogAr.ForeColor = System.Drawing.Color.White;
        txtChangelogAr.Location = new System.Drawing.Point(20, 190);
        txtChangelogAr.Multiline = true;
        txtChangelogAr.Name = "txtChangelogAr";
        txtChangelogAr.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
        txtChangelogAr.Size = new System.Drawing.Size(700, 70);
        txtChangelogAr.TabIndex = 7;

        lblChangelogEn.AutoSize = true;
        lblChangelogEn.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
        lblChangelogEn.ForeColor = System.Drawing.Color.Gainsboro;
        lblChangelogEn.Location = new System.Drawing.Point(20, 275);
        lblChangelogEn.Name = "lblChangelogEn";
        lblChangelogEn.Size = new System.Drawing.Size(145, 15);
        lblChangelogEn.TabIndex = 8;
        lblChangelogEn.Text = "English Changelog:";

        txtChangelogEn.BackColor = System.Drawing.Color.FromArgb(48, 50, 56);
        txtChangelogEn.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
        txtChangelogEn.Font = new System.Drawing.Font("Segoe UI", 9.5F);
        txtChangelogEn.ForeColor = System.Drawing.Color.White;
        txtChangelogEn.Location = new System.Drawing.Point(20, 295);
        txtChangelogEn.Multiline = true;
        txtChangelogEn.Name = "txtChangelogEn";
        txtChangelogEn.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
        txtChangelogEn.Size = new System.Drawing.Size(700, 70);
        txtChangelogEn.TabIndex = 9;

        btnPublishUpdate.BackColor = System.Drawing.Color.FromArgb(0, 150, 136);
        btnPublishUpdate.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
        btnPublishUpdate.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
        btnPublishUpdate.ForeColor = System.Drawing.Color.White;
        btnPublishUpdate.Location = new System.Drawing.Point(20, 385);
        btnPublishUpdate.Name = "btnPublishUpdate";
        btnPublishUpdate.Size = new System.Drawing.Size(200, 40);
        btnPublishUpdate.TabIndex = 10;
        btnPublishUpdate.Text = "🚀 نشر التحديث على السيرفر";
        btnPublishUpdate.UseVisualStyleBackColor = false;
        btnPublishUpdate.Click += btnPublishUpdate_Click;

        btnCheckUpdate.BackColor = System.Drawing.Color.FromArgb(60, 64, 72);
        btnCheckUpdate.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
        btnCheckUpdate.Font = new System.Drawing.Font("Segoe UI", 9.5F);
        btnCheckUpdate.ForeColor = System.Drawing.Color.White;
        btnCheckUpdate.Location = new System.Drawing.Point(230, 385);
        btnCheckUpdate.Name = "btnCheckUpdate";
        btnCheckUpdate.Size = new System.Drawing.Size(160, 40);
        btnCheckUpdate.TabIndex = 11;
        btnCheckUpdate.Text = "🔍 فحص التحديث الحالي";
        btnCheckUpdate.UseVisualStyleBackColor = false;
        btnCheckUpdate.Click += btnCheckUpdate_Click;

        lblUpdateStatus.AutoSize = true;
        lblUpdateStatus.Font = new System.Drawing.Font("Segoe UI", 9F);
        lblUpdateStatus.ForeColor = System.Drawing.Color.DarkGray;
        lblUpdateStatus.Location = new System.Drawing.Point(20, 440);
        lblUpdateStatus.Name = "lblUpdateStatus";
        lblUpdateStatus.Size = new System.Drawing.Size(130, 15);
        lblUpdateStatus.TabIndex = 12;
        lblUpdateStatus.Text = "حالة التحديث: جاهز...";

        // 
        // panelBottomStatus
        // 
        panelBottomStatus.BackColor = System.Drawing.Color.FromArgb(18, 18, 22);
        panelBottomStatus.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
        panelBottomStatus.Controls.Add(lblStatusMessage);
        panelBottomStatus.Dock = System.Windows.Forms.DockStyle.Bottom;
        panelBottomStatus.Location = new System.Drawing.Point(0, 645);
        panelBottomStatus.Name = "panelBottomStatus";
        panelBottomStatus.Size = new System.Drawing.Size(1160, 30);
        panelBottomStatus.TabIndex = 2;

        lblStatusMessage.AutoSize = true;
        lblStatusMessage.Font = new System.Drawing.Font("Segoe UI", 9F);
        lblStatusMessage.ForeColor = System.Drawing.Color.FromArgb(160, 175, 195);
        lblStatusMessage.Location = new System.Drawing.Point(10, 6);
        lblStatusMessage.Name = "lblStatusMessage";
        lblStatusMessage.Size = new System.Drawing.Size(76, 15);
        lblStatusMessage.TabIndex = 0;
        lblStatusMessage.Text = "جاهز للعمل...";

        // 
        // AdminForm
        // 
        AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
        AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
        BackColor = System.Drawing.Color.FromArgb(24, 25, 29);
        ClientSize = new System.Drawing.Size(1160, 675);
        Controls.Add(tabControl);
        Controls.Add(panelBottomStatus);
        Controls.Add(panelTop);
        Font = new System.Drawing.Font("Segoe UI", 9F);
        ForeColor = System.Drawing.Color.White;
        MinimumSize = new System.Drawing.Size(1000, 620);
        Name = "AdminForm";
        RightToLeft = System.Windows.Forms.RightToLeft.Yes;
        RightToLeftLayout = true;
        StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
        Text = "Wolves Admin - لوحة تحكم وإدارة الحسابات والاشتراكات الشهرية واليومية";
        Load += AdminForm_Load;

        panelTop.ResumeLayout(false);
        panelTop.PerformLayout();
        panelStats.ResumeLayout(false);
        ((System.ComponentModel.ISupportInitialize)picLogo).EndInit();
        tabControl.ResumeLayout(false);
        tabAccounts.ResumeLayout(false);
        ((System.ComponentModel.ISupportInitialize)dgvAccounts).EndInit();
        cmsAccounts.ResumeLayout(false);
        panelAccountActions.ResumeLayout(false);
        panelAccountActions.PerformLayout();
        tabLogs.ResumeLayout(false);
        ((System.ComponentModel.ISupportInitialize)dgvLogs).EndInit();
        panelLogActions.ResumeLayout(false);
        tabSettings.ResumeLayout(false);
        panelSettingsCard.ResumeLayout(false);
        panelSettingsCard.PerformLayout();
        panelWhatsAppCard.ResumeLayout(false);
        panelWhatsAppCard.PerformLayout();
        ((System.ComponentModel.ISupportInitialize)numMaxAccounts).EndInit();
        tabUpdates.ResumeLayout(false);
        panelUpdateCard.ResumeLayout(false);
        panelUpdateCard.PerformLayout();
        // 
        // tabPermissions
        // 
        tabPermissions.BackColor = System.Drawing.Color.FromArgb(28, 29, 33);
        tabPermissions.Controls.Add(panelPermCard);
        tabPermissions.Location = new System.Drawing.Point(4, 26);
        tabPermissions.Name = "tabPermissions";
        tabPermissions.Padding = new System.Windows.Forms.Padding(6);
        tabPermissions.Size = new System.Drawing.Size(1152, 530);
        tabPermissions.TabIndex = 4;
        tabPermissions.Text = "🛡️ صلاحيات النسخة المجانية (Free Tier)";

        // panelPermCard
        panelPermCard.BackColor = System.Drawing.Color.FromArgb(34, 35, 40);
        panelPermCard.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
        panelPermCard.Controls.Add(lblPermHeader);
        panelPermCard.Controls.Add(lblPermDesc);
        panelPermCard.Controls.Add(chkFreeTierEnabled);
        panelPermCard.Controls.Add(grpBotbasePerms);
        panelPermCard.Controls.Add(grpAutoPerms);
        panelPermCard.Controls.Add(grpLimitsPerms);
        panelPermCard.Controls.Add(btnSavePermissions);
        panelPermCard.Controls.Add(btnReloadPermissions);
        panelPermCard.Controls.Add(lblPermStatus);
        panelPermCard.Location = new System.Drawing.Point(15, 15);
        panelPermCard.Name = "panelPermCard";
        panelPermCard.Size = new System.Drawing.Size(1120, 495);
        panelPermCard.TabIndex = 0;

        // lblPermHeader
        lblPermHeader.AutoSize = true;
        lblPermHeader.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Bold);
        lblPermHeader.ForeColor = System.Drawing.Color.White;
        lblPermHeader.Location = new System.Drawing.Point(20, 12);
        lblPermHeader.Name = "lblPermHeader";
        lblPermHeader.Size = new System.Drawing.Size(420, 21);
        lblPermHeader.Text = "إدارة صلاحيات وشروط النسخة المجانية للبوت (Free Tier)";

        // lblPermDesc
        lblPermDesc.AutoSize = true;
        lblPermDesc.Font = new System.Drawing.Font("Segoe UI", 8.5F);
        lblPermDesc.ForeColor = System.Drawing.Color.Gainsboro;
        lblPermDesc.Location = new System.Drawing.Point(22, 36);
        lblPermDesc.Name = "lblPermDesc";
        lblPermDesc.Size = new System.Drawing.Size(560, 15);
        lblPermDesc.Text = "يمكنك فتح أو قفل أي ميزة محددة للمستخدمين المجانيين، وسيتم تطبيق الصلاحيات فوراً من السيرفر السحابي دون تعديل الكود.";

        // chkFreeTierEnabled
        chkFreeTierEnabled.AutoSize = true;
        chkFreeTierEnabled.Checked = true;
        chkFreeTierEnabled.CheckState = System.Windows.Forms.CheckState.Checked;
        chkFreeTierEnabled.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
        chkFreeTierEnabled.ForeColor = System.Drawing.Color.FromArgb(100, 240, 140);
        chkFreeTierEnabled.Location = new System.Drawing.Point(24, 60);
        chkFreeTierEnabled.Name = "chkFreeTierEnabled";
        chkFreeTierEnabled.Size = new System.Drawing.Size(460, 23);
        chkFreeTierEnabled.Text = "✅ تفعيل النسخة المجانية للبوت (السماح لأي مستخدم بتشغيل البوت مجاناً)";
        chkFreeTierEnabled.UseVisualStyleBackColor = true;

        // grpBotbasePerms
        grpBotbasePerms.BackColor = System.Drawing.Color.FromArgb(38, 40, 46);
        grpBotbasePerms.Controls.Add(chkAllowGrinding);
        grpBotbasePerms.Controls.Add(chkAllowTrade);
        grpBotbasePerms.Controls.Add(chkAllowAlchemy);
        grpBotbasePerms.Controls.Add(chkAllowLure);
        grpBotbasePerms.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
        grpBotbasePerms.ForeColor = System.Drawing.Color.FromArgb(170, 210, 255);
        grpBotbasePerms.Location = new System.Drawing.Point(20, 95);
        grpBotbasePerms.Name = "grpBotbasePerms";
        grpBotbasePerms.Size = new System.Drawing.Size(340, 280);
        grpBotbasePerms.Text = "أنماط البوت المسموح بها للمجاني";

        // chkAllowGrinding
        chkAllowGrinding.AutoSize = true;
        chkAllowGrinding.Checked = true;
        chkAllowGrinding.CheckState = System.Windows.Forms.CheckState.Checked;
        chkAllowGrinding.Font = new System.Drawing.Font("Segoe UI", 9F);
        chkAllowGrinding.ForeColor = System.Drawing.Color.White;
        chkAllowGrinding.Location = new System.Drawing.Point(15, 30);
        chkAllowGrinding.Name = "chkAllowGrinding";
        chkAllowGrinding.Size = new System.Drawing.Size(265, 19);
        chkAllowGrinding.Text = "⚔️ نمط التدريب وصيد الوحوش (Grinding Bot)";
        chkAllowGrinding.UseVisualStyleBackColor = true;

        // chkAllowTrade
        chkAllowTrade.AutoSize = true;
        chkAllowTrade.Font = new System.Drawing.Font("Segoe UI", 9F);
        chkAllowTrade.ForeColor = System.Drawing.Color.White;
        chkAllowTrade.Location = new System.Drawing.Point(15, 68);
        chkAllowTrade.Name = "chkAllowTrade";
        chkAllowTrade.Size = new System.Drawing.Size(260, 19);
        chkAllowTrade.Text = "🐫 نمط التريدة والقوافل (Trade - Hunter/Thief)";
        chkAllowTrade.UseVisualStyleBackColor = true;

        // chkAllowAlchemy
        chkAllowAlchemy.AutoSize = true;
        chkAllowAlchemy.Font = new System.Drawing.Font("Segoe UI", 9F);
        chkAllowAlchemy.ForeColor = System.Drawing.Color.White;
        chkAllowAlchemy.Location = new System.Drawing.Point(15, 106);
        chkAllowAlchemy.Name = "chkAllowAlchemy";
        chkAllowAlchemy.Size = new System.Drawing.Size(245, 19);
        chkAllowAlchemy.Text = "🧪 نمط الدمج والبلص التلقائي (Alchemy Bot)";
        chkAllowAlchemy.UseVisualStyleBackColor = true;

        // chkAllowLure
        chkAllowLure.AutoSize = true;
        chkAllowLure.Font = new System.Drawing.Font("Segoe UI", 9F);
        chkAllowLure.ForeColor = System.Drawing.Color.White;
        chkAllowLure.Location = new System.Drawing.Point(15, 144);
        chkAllowLure.Name = "chkAllowLure";
        chkAllowLure.Size = new System.Drawing.Size(235, 19);
        chkAllowLure.Text = "🎯 نمط سحب وجلب الوحوش (Lure Bot)";
        chkAllowLure.UseVisualStyleBackColor = true;

        // grpAutoPerms
        grpAutoPerms.BackColor = System.Drawing.Color.FromArgb(38, 40, 46);
        grpAutoPerms.Controls.Add(chkAllowTownLoop);
        grpAutoPerms.Controls.Add(chkAllowAutoParty);
        grpAutoPerms.Controls.Add(chkAllowAutoPotion);
        grpAutoPerms.Controls.Add(chkAllowAutoPickup);
        grpAutoPerms.Controls.Add(chkAllowAutoResurrect);
        grpAutoPerms.Controls.Add(chkAllowStall);
        grpAutoPerms.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
        grpAutoPerms.ForeColor = System.Drawing.Color.FromArgb(170, 210, 255);
        grpAutoPerms.Location = new System.Drawing.Point(375, 95);
        grpAutoPerms.Name = "grpAutoPerms";
        grpAutoPerms.Size = new System.Drawing.Size(360, 280);
        grpAutoPerms.Text = "الخدمات والمميزات التلقائية للمجاني";

        // chkAllowTownLoop
        chkAllowTownLoop.AutoSize = true;
        chkAllowTownLoop.Checked = true;
        chkAllowTownLoop.CheckState = System.Windows.Forms.CheckState.Checked;
        chkAllowTownLoop.Font = new System.Drawing.Font("Segoe UI", 9F);
        chkAllowTownLoop.ForeColor = System.Drawing.Color.White;
        chkAllowTownLoop.Location = new System.Drawing.Point(15, 30);
        chkAllowTownLoop.Name = "chkAllowTownLoop";
        chkAllowTownLoop.Size = new System.Drawing.Size(280, 19);
        chkAllowTownLoop.Text = "🛒 دورة المدينة والشراء التلقائي (Town Loop)";
        chkAllowTownLoop.UseVisualStyleBackColor = true;

        // chkAllowAutoParty
        chkAllowAutoParty.AutoSize = true;
        chkAllowAutoParty.Checked = true;
        chkAllowAutoParty.CheckState = System.Windows.Forms.CheckState.Checked;
        chkAllowAutoParty.Font = new System.Drawing.Font("Segoe UI", 9F);
        chkAllowAutoParty.ForeColor = System.Drawing.Color.White;
        chkAllowAutoParty.Location = new System.Drawing.Point(15, 68);
        chkAllowAutoParty.Name = "chkAllowAutoParty";
        chkAllowAutoParty.Size = new System.Drawing.Size(260, 19);
        chkAllowAutoParty.Text = "👥 الانضمام وتشكيل البارتي التلقائي (Auto Party)";
        chkAllowAutoParty.UseVisualStyleBackColor = true;

        // chkAllowAutoPotion
        chkAllowAutoPotion.AutoSize = true;
        chkAllowAutoPotion.Checked = true;
        chkAllowAutoPotion.CheckState = System.Windows.Forms.CheckState.Checked;
        chkAllowAutoPotion.Font = new System.Drawing.Font("Segoe UI", 9F);
        chkAllowAutoPotion.ForeColor = System.Drawing.Color.White;
        chkAllowAutoPotion.Location = new System.Drawing.Point(15, 106);
        chkAllowAutoPotion.Name = "chkAllowAutoPotion";
        chkAllowAutoPotion.Size = new System.Drawing.Size(275, 19);
        chkAllowAutoPotion.Text = "💊 الحماية واستخدام الجرع التلقائي (Auto Potion)";
        chkAllowAutoPotion.UseVisualStyleBackColor = true;

        // chkAllowAutoPickup
        chkAllowAutoPickup.AutoSize = true;
        chkAllowAutoPickup.Checked = true;
        chkAllowAutoPickup.CheckState = System.Windows.Forms.CheckState.Checked;
        chkAllowAutoPickup.Font = new System.Drawing.Font("Segoe UI", 9F);
        chkAllowAutoPickup.ForeColor = System.Drawing.Color.White;
        chkAllowAutoPickup.Location = new System.Drawing.Point(15, 144);
        chkAllowAutoPickup.Name = "chkAllowAutoPickup";
        chkAllowAutoPickup.Size = new System.Drawing.Size(265, 19);
        chkAllowAutoPickup.Text = "🎒 فلتر اللوت والتقاط العناصر (Item Filter)";
        chkAllowAutoPickup.UseVisualStyleBackColor = true;

        // chkAllowAutoResurrect
        chkAllowAutoResurrect.AutoSize = true;
        chkAllowAutoResurrect.Checked = true;
        chkAllowAutoResurrect.CheckState = System.Windows.Forms.CheckState.Checked;
        chkAllowAutoResurrect.Font = new System.Drawing.Font("Segoe UI", 9F);
        chkAllowAutoResurrect.ForeColor = System.Drawing.Color.White;
        chkAllowAutoResurrect.Location = new System.Drawing.Point(15, 182);
        chkAllowAutoResurrect.Name = "chkAllowAutoResurrect";
        chkAllowAutoResurrect.Size = new System.Drawing.Size(240, 19);
        chkAllowAutoResurrect.Text = "⚡ الإنعاش والرجوع بعد الموت (Auto Res)";
        chkAllowAutoResurrect.UseVisualStyleBackColor = true;

        // chkAllowStall
        chkAllowStall.AutoSize = true;
        chkAllowStall.Font = new System.Drawing.Font("Segoe UI", 9F);
        chkAllowStall.ForeColor = System.Drawing.Color.White;
        chkAllowStall.Location = new System.Drawing.Point(15, 220);
        chkAllowStall.Name = "chkAllowStall";
        chkAllowStall.Size = new System.Drawing.Size(200, 19);
        chkAllowStall.Text = "🏪 البسطة التلقائية (Auto Stall)";
        chkAllowStall.UseVisualStyleBackColor = true;

        // grpLimitsPerms
        grpLimitsPerms.BackColor = System.Drawing.Color.FromArgb(38, 40, 46);
        grpLimitsPerms.Controls.Add(lblMaxClients);
        grpLimitsPerms.Controls.Add(numMaxClients);
        grpLimitsPerms.Controls.Add(lblSessionMinutes);
        grpLimitsPerms.Controls.Add(numSessionMinutes);
        grpLimitsPerms.Controls.Add(lblFreeNotice);
        grpLimitsPerms.Controls.Add(txtFreeNotice);
        grpLimitsPerms.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
        grpLimitsPerms.ForeColor = System.Drawing.Color.FromArgb(170, 210, 255);
        grpLimitsPerms.Location = new System.Drawing.Point(750, 95);
        grpLimitsPerms.Name = "grpLimitsPerms";
        grpLimitsPerms.Size = new System.Drawing.Size(350, 280);
        grpLimitsPerms.Text = "القيود والرسائل التنبيهية للمجاني";

        // lblMaxClients
        lblMaxClients.AutoSize = true;
        lblMaxClients.Font = new System.Drawing.Font("Segoe UI", 8.5F, System.Drawing.FontStyle.Bold);
        lblMaxClients.ForeColor = System.Drawing.Color.Gainsboro;
        lblMaxClients.Location = new System.Drawing.Point(15, 25);
        lblMaxClients.Name = "lblMaxClients";
        lblMaxClients.Size = new System.Drawing.Size(260, 15);
        lblMaxClients.Text = "أقصى عدد كلاينت مفتوح في وقت واحد (0 = غير محدود):";

        // numMaxClients
        numMaxClients.BackColor = System.Drawing.Color.FromArgb(48, 50, 56);
        numMaxClients.Font = new System.Drawing.Font("Segoe UI", 9F);
        numMaxClients.ForeColor = System.Drawing.Color.White;
        numMaxClients.Location = new System.Drawing.Point(15, 45);
        numMaxClients.Maximum = new decimal(new int[] { 100, 0, 0, 0 });
        numMaxClients.Name = "numMaxClients";
        numMaxClients.Size = new System.Drawing.Size(120, 23);
        numMaxClients.Value = new decimal(new int[] { 1, 0, 0, 0 });

        // lblSessionMinutes
        lblSessionMinutes.AutoSize = true;
        lblSessionMinutes.Font = new System.Drawing.Font("Segoe UI", 8.5F, System.Drawing.FontStyle.Bold);
        lblSessionMinutes.ForeColor = System.Drawing.Color.Gainsboro;
        lblSessionMinutes.Location = new System.Drawing.Point(15, 78);
        lblSessionMinutes.Name = "lblSessionMinutes";
        lblSessionMinutes.Size = new System.Drawing.Size(235, 15);
        lblSessionMinutes.Text = "مدة الجلسة بالدقائق (0 = غير محدود / مفتوحة):";

        // numSessionMinutes
        numSessionMinutes.BackColor = System.Drawing.Color.FromArgb(48, 50, 56);
        numSessionMinutes.Font = new System.Drawing.Font("Segoe UI", 9F);
        numSessionMinutes.ForeColor = System.Drawing.Color.White;
        numSessionMinutes.Location = new System.Drawing.Point(15, 98);
        numSessionMinutes.Maximum = new decimal(new int[] { 1440, 0, 0, 0 });
        numSessionMinutes.Name = "numSessionMinutes";
        numSessionMinutes.Size = new System.Drawing.Size(120, 23);
        numSessionMinutes.Value = new decimal(new int[] { 0, 0, 0, 0 });

        // lblFreeNotice
        lblFreeNotice.AutoSize = true;
        lblFreeNotice.Font = new System.Drawing.Font("Segoe UI", 8.5F, System.Drawing.FontStyle.Bold);
        lblFreeNotice.ForeColor = System.Drawing.Color.Gainsboro;
        lblFreeNotice.Location = new System.Drawing.Point(15, 130);
        lblFreeNotice.Name = "lblFreeNotice";
        lblFreeNotice.Size = new System.Drawing.Size(220, 15);
        lblFreeNotice.Text = "رسالة التنبيه المعروضة للمستخدم المجاني:";

        // txtFreeNotice
        txtFreeNotice.BackColor = System.Drawing.Color.FromArgb(48, 50, 56);
        txtFreeNotice.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
        txtFreeNotice.Font = new System.Drawing.Font("Segoe UI", 9F);
        txtFreeNotice.ForeColor = System.Drawing.Color.White;
        txtFreeNotice.Location = new System.Drawing.Point(15, 150);
        txtFreeNotice.Multiline = true;
        txtFreeNotice.Name = "txtFreeNotice";
        txtFreeNotice.Size = new System.Drawing.Size(315, 115);
        txtFreeNotice.Text = "أنت تستخدم النسخة المجانية من Wolves Bot. للاشتراك وفتح جميع المميزات تواصل مع الإدارة.";

        // btnSavePermissions
        btnSavePermissions.BackColor = System.Drawing.Color.FromArgb(46, 125, 50);
        btnSavePermissions.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
        btnSavePermissions.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
        btnSavePermissions.ForeColor = System.Drawing.Color.White;
        btnSavePermissions.Location = new System.Drawing.Point(20, 395);
        btnSavePermissions.Name = "btnSavePermissions";
        btnSavePermissions.Size = new System.Drawing.Size(280, 42);
        btnSavePermissions.TabIndex = 6;
        btnSavePermissions.Text = "💾 حفظ وتطبيق الصلاحيات في السيرفر فوراً";
        btnSavePermissions.UseVisualStyleBackColor = false;
        btnSavePermissions.Click += btnSavePermissions_Click;

        // btnReloadPermissions
        btnReloadPermissions.BackColor = System.Drawing.Color.FromArgb(37, 99, 235);
        btnReloadPermissions.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
        btnReloadPermissions.Font = new System.Drawing.Font("Segoe UI", 9.5F, System.Drawing.FontStyle.Bold);
        btnReloadPermissions.ForeColor = System.Drawing.Color.White;
        btnReloadPermissions.Location = new System.Drawing.Point(315, 395);
        btnReloadPermissions.Name = "btnReloadPermissions";
        btnReloadPermissions.Size = new System.Drawing.Size(180, 42);
        btnReloadPermissions.TabIndex = 7;
        btnReloadPermissions.Text = "🔄 استرجاع من السيرفر";
        btnReloadPermissions.UseVisualStyleBackColor = false;
        btnReloadPermissions.Click += btnReloadPermissions_Click;

        // lblPermStatus
        lblPermStatus.AutoSize = true;
        lblPermStatus.Font = new System.Drawing.Font("Segoe UI", 9.5F, System.Drawing.FontStyle.Bold);
        lblPermStatus.ForeColor = System.Drawing.Color.LightGreen;
        lblPermStatus.Location = new System.Drawing.Point(510, 407);
        lblPermStatus.Name = "lblPermStatus";
        lblPermStatus.Size = new System.Drawing.Size(160, 17);
        lblPermStatus.Text = "جاهز لتعديل الصلاحيات";

        panelBottomStatus.ResumeLayout(false);
        panelBottomStatus.PerformLayout();
        ResumeLayout(false);
    }

    #endregion

    private System.Windows.Forms.Panel panelTop;
    private System.Windows.Forms.PictureBox picLogo;
    private System.Windows.Forms.Label lblTitle;
    private System.Windows.Forms.Label lblSubtitle;
    private System.Windows.Forms.Label lblServerStatus;
    private System.Windows.Forms.Panel panelStats;
    private System.Windows.Forms.Label lblTotalAccounts;
    private System.Windows.Forms.Label lblActiveCount;
    private System.Windows.Forms.Label lblExpiredCount;
    private System.Windows.Forms.Label lblBlockedCount;

    private System.Windows.Forms.TabControl tabControl;
    private System.Windows.Forms.TabPage tabAccounts;
    private System.Windows.Forms.DataGridView dgvAccounts;
    private System.Windows.Forms.DataGridViewTextBoxColumn colIndex;
    private System.Windows.Forms.DataGridViewTextBoxColumn colUser;
    private System.Windows.Forms.DataGridViewTextBoxColumn colStatus;
    private System.Windows.Forms.DataGridViewTextBoxColumn colSubscription;
    private System.Windows.Forms.DataGridViewTextBoxColumn colExpiry;
    private System.Windows.Forms.DataGridViewTextBoxColumn colReason;
    private System.Windows.Forms.DataGridViewTextBoxColumn colHwid;
    private System.Windows.Forms.DataGridViewTextBoxColumn colRegIp;
    private System.Windows.Forms.DataGridViewTextBoxColumn colLastIp;
    private System.Windows.Forms.DataGridViewTextBoxColumn colCreated;
    private System.Windows.Forms.DataGridViewTextBoxColumn colLastLogin;
    private System.Windows.Forms.DataGridViewTextBoxColumn colNotes;

    private System.Windows.Forms.ContextMenuStrip cmsAccounts;
    private System.Windows.Forms.ToolStripMenuItem tsmiExtendSub;
    private System.Windows.Forms.ToolStripMenuItem tsmiAdd1Day;
    private System.Windows.Forms.ToolStripMenuItem tsmiAdd7Days;
    private System.Windows.Forms.ToolStripMenuItem tsmiAdd30Days;
    private System.Windows.Forms.ToolStripMenuItem tsmiAdd60Days;
    private System.Windows.Forms.ToolStripMenuItem tsmiAdd90Days;
    private System.Windows.Forms.ToolStripMenuItem tsmiAdd365Days;
    private System.Windows.Forms.ToolStripMenuItem tsmiAddCustom;
    private System.Windows.Forms.ToolStripMenuItem tsmiSetDate;
    private System.Windows.Forms.ToolStripMenuItem tsmiLifetime;
    private System.Windows.Forms.ToolStripMenuItem tsmiExpireNow;
    private System.Windows.Forms.ToolStripSeparator tsmiSep1;
    private System.Windows.Forms.ToolStripMenuItem tsmiResetHwid;
    private System.Windows.Forms.ToolStripMenuItem tsmiChangePassword;
    private System.Windows.Forms.ToolStripMenuItem tsmiBlock;
    private System.Windows.Forms.ToolStripMenuItem tsmiUnblock;
    private System.Windows.Forms.ToolStripSeparator tsmiSep2;
    private System.Windows.Forms.ToolStripMenuItem tsmiDeleteAccount;

    private System.Windows.Forms.Panel panelAccountActions;
    private System.Windows.Forms.Label lblSubHeader;
    private System.Windows.Forms.Button btnAdd1Day;
    private System.Windows.Forms.Button btnAdd7Days;
    private System.Windows.Forms.Button btnAdd30Days;
    private System.Windows.Forms.Button btnAdd60Days;
    private System.Windows.Forms.Button btnAdd90Days;
    private System.Windows.Forms.Button btnAddCustom;
    private System.Windows.Forms.Button btnSetDate;
    private System.Windows.Forms.Button btnLifetime;
    private System.Windows.Forms.Button btnExpireNow;

    private System.Windows.Forms.Button btnRefresh;
    private System.Windows.Forms.Button btnUnblock;
    private System.Windows.Forms.Button btnBlock;
    private System.Windows.Forms.Button btnResetHwid;
    private System.Windows.Forms.Button btnChangePassword;
    private System.Windows.Forms.Button btnAddAccount;
    private System.Windows.Forms.Button btnDeleteAccount;
    private System.Windows.Forms.Button btnToggleAdmin;
    private System.Windows.Forms.ToolStripMenuItem tsmiToggleAdmin;
    private System.Windows.Forms.CheckBox chkAutoRefresh;

    private System.Windows.Forms.TabPage tabLogs;
    private System.Windows.Forms.DataGridView dgvLogs;
    private System.Windows.Forms.DataGridViewTextBoxColumn colLogTime;
    private System.Windows.Forms.DataGridViewTextBoxColumn colLogUser;
    private System.Windows.Forms.DataGridViewTextBoxColumn colLogAction;
    private System.Windows.Forms.DataGridViewTextBoxColumn colLogHwid;
    private System.Windows.Forms.DataGridViewTextBoxColumn colLogIp;
    private System.Windows.Forms.DataGridViewTextBoxColumn colLogDetails;
    private System.Windows.Forms.Panel panelLogActions;
    private System.Windows.Forms.Button btnRefreshLogs;

    private System.Windows.Forms.TabPage tabSettings;
    private System.Windows.Forms.Panel panelSettingsCard;
    private System.Windows.Forms.Label lblFirebaseUrl;
    private System.Windows.Forms.TextBox txtFirebaseUrl;
    private System.Windows.Forms.Label lblAuthSecret;
    private System.Windows.Forms.TextBox txtAuthSecret;
    private System.Windows.Forms.Label lblMaxAccounts;
    private System.Windows.Forms.NumericUpDown numMaxAccounts;
    private System.Windows.Forms.Button btnTestConnection;
    private System.Windows.Forms.Button btnSaveSettings;
    private System.Windows.Forms.Label lblInstructions;

    private System.Windows.Forms.Panel panelWhatsAppCard;
    private System.Windows.Forms.Label lblWhatsAppHeader;
    private System.Windows.Forms.Label lblWhatsAppNumber;
    private System.Windows.Forms.TextBox txtWhatsAppNumber;
    private System.Windows.Forms.Label lblWhatsAppUrl;
    private System.Windows.Forms.TextBox txtWhatsAppUrl;
    private System.Windows.Forms.Label lblRechargeMessage;
    private System.Windows.Forms.TextBox txtRechargeMessage;
    private System.Windows.Forms.Button btnSaveWhatsApp;
    private System.Windows.Forms.Label lblWhatsAppHint;

    private System.Windows.Forms.TabPage tabUpdates;
    private System.Windows.Forms.Panel panelUpdateCard;
    private System.Windows.Forms.Label lblUpdateHeader;
    private System.Windows.Forms.Label lblUpdateVersion;
    private System.Windows.Forms.TextBox txtUpdateVersion;
    private System.Windows.Forms.Label lblUpdateUrl;
    private System.Windows.Forms.TextBox txtUpdateUrl;
    private System.Windows.Forms.CheckBox chkUpdateMandatory;
    private System.Windows.Forms.Label lblChangelogAr;
    private System.Windows.Forms.TextBox txtChangelogAr;
    private System.Windows.Forms.Label lblChangelogEn;
    private System.Windows.Forms.TextBox txtChangelogEn;
    private System.Windows.Forms.Button btnPublishUpdate;
    private System.Windows.Forms.Button btnCheckUpdate;
    private System.Windows.Forms.TabPage tabPermissions;
    private System.Windows.Forms.Panel panelPermCard;
    private System.Windows.Forms.Label lblPermHeader;
    private System.Windows.Forms.Label lblPermDesc;
    private System.Windows.Forms.CheckBox chkFreeTierEnabled;
    private System.Windows.Forms.GroupBox grpBotbasePerms;
    private System.Windows.Forms.CheckBox chkAllowGrinding;
    private System.Windows.Forms.CheckBox chkAllowTrade;
    private System.Windows.Forms.CheckBox chkAllowAlchemy;
    private System.Windows.Forms.CheckBox chkAllowLure;
    private System.Windows.Forms.GroupBox grpAutoPerms;
    private System.Windows.Forms.CheckBox chkAllowTownLoop;
    private System.Windows.Forms.CheckBox chkAllowAutoParty;
    private System.Windows.Forms.CheckBox chkAllowAutoPotion;
    private System.Windows.Forms.CheckBox chkAllowAutoPickup;
    private System.Windows.Forms.CheckBox chkAllowAutoResurrect;
    private System.Windows.Forms.CheckBox chkAllowStall;
    private System.Windows.Forms.GroupBox grpLimitsPerms;
    private System.Windows.Forms.Label lblMaxClients;
    private System.Windows.Forms.NumericUpDown numMaxClients;
    private System.Windows.Forms.Label lblSessionMinutes;
    private System.Windows.Forms.NumericUpDown numSessionMinutes;
    private System.Windows.Forms.Label lblFreeNotice;
    private System.Windows.Forms.TextBox txtFreeNotice;
    private System.Windows.Forms.Button btnSavePermissions;
    private System.Windows.Forms.Button btnReloadPermissions;
    private System.Windows.Forms.Label lblPermStatus;

    private System.Windows.Forms.Label lblUpdateStatus;

    private System.Windows.Forms.Panel panelBottomStatus;
    private System.Windows.Forms.Label lblStatusMessage;
}
