namespace RSBot.Views;

partial class AuthWindow
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
        lblAppTitle = new SDUI.Controls.Label();
        lblAppSub = new SDUI.Controls.Label();
        panelTabs = new SDUI.Controls.Panel();
        btnTabLogin = new SDUI.Controls.Button();
        btnTabRegister = new SDUI.Controls.Button();
        panelLogin = new System.Windows.Forms.Panel();
        lblLoginUser = new SDUI.Controls.Label();
        txtLoginUser = new SDUI.Controls.TextBox();
        lblLoginPass = new SDUI.Controls.Label();
        txtLoginPass = new SDUI.Controls.TextBox();
        chkRememberMe = new SDUI.Controls.CheckBox();
        chkAutoLogin = new SDUI.Controls.CheckBox();
        btnLogin = new SDUI.Controls.Button();
        btnFreeMode = new SDUI.Controls.Button();
        panelRegister = new System.Windows.Forms.Panel();
        lblRegNotice = new SDUI.Controls.Label();
        lblRegUser = new SDUI.Controls.Label();
        txtRegUser = new SDUI.Controls.TextBox();
        lblRegPass = new SDUI.Controls.Label();
        txtRegPass = new SDUI.Controls.TextBox();
        lblRegPassConfirm = new SDUI.Controls.Label();
        txtRegPassConfirm = new SDUI.Controls.TextBox();
        btnRegister = new SDUI.Controls.Button();
        panelBottom = new SDUI.Controls.Panel();
        progressBar = new SDUI.Controls.ProgressBar();
        lblStatus = new SDUI.Controls.Label();
        lblHwid = new SDUI.Controls.Label();
        lblServer = new SDUI.Controls.Label();
        btnAdminPanel = new SDUI.Controls.Button();

        panelHeader.SuspendLayout();
        panelTabs.SuspendLayout();
        panelLogin.SuspendLayout();
        panelRegister.SuspendLayout();
        panelBottom.SuspendLayout();
        SuspendLayout();

        // 
        // panelHeader
        // 
        panelHeader.Controls.Add(lblAppSub);
        panelHeader.Controls.Add(lblAppTitle);
        panelHeader.Dock = System.Windows.Forms.DockStyle.Top;
        panelHeader.Location = new System.Drawing.Point(0, 0);
        panelHeader.Name = "panelHeader";
        panelHeader.Radius = 0;
        panelHeader.ShadowDepth = 0F;
        panelHeader.Size = new System.Drawing.Size(430, 80);
        panelHeader.TabIndex = 0;

        // 
        // lblAppTitle
        // 
        lblAppTitle.Font = new System.Drawing.Font("Segoe UI", 16F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
        lblAppTitle.ForeColor = System.Drawing.Color.FromArgb(245, 215, 100);
        lblAppTitle.Location = new System.Drawing.Point(0, 16);
        lblAppTitle.Name = "lblAppTitle";
        lblAppTitle.Size = new System.Drawing.Size(430, 32);
        lblAppTitle.TabIndex = 1;
        lblAppTitle.Text = "Aerobot";
        lblAppTitle.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;

        // 
        // lblAppSub
        // 
        lblAppSub.Font = new System.Drawing.Font("Segoe UI", 9.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
        lblAppSub.ForeColor = System.Drawing.Color.FromArgb(180, 190, 205);
        lblAppSub.Location = new System.Drawing.Point(0, 50);
        lblAppSub.Name = "lblAppSub";
        lblAppSub.Size = new System.Drawing.Size(430, 20);
        lblAppSub.TabIndex = 2;
        lblAppSub.Text = "نظام تسجيل الدخول والترخيص السحابي";
        lblAppSub.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;

        // 
        // panelTabs
        // 
        panelTabs.Controls.Add(btnTabRegister);
        panelTabs.Controls.Add(btnTabLogin);
        panelTabs.Dock = System.Windows.Forms.DockStyle.Top;
        panelTabs.Location = new System.Drawing.Point(0, 80);
        panelTabs.Name = "panelTabs";
        panelTabs.Radius = 0;
        panelTabs.ShadowDepth = 0F;
        panelTabs.Size = new System.Drawing.Size(430, 42);
        panelTabs.TabIndex = 1;

        // 
        // btnTabLogin
        // 
        btnTabLogin.Location = new System.Drawing.Point(22, 5);
        btnTabLogin.Name = "btnTabLogin";
        btnTabLogin.Radius = 4;
        btnTabLogin.ShadowDepth = 2F;
        btnTabLogin.Size = new System.Drawing.Size(185, 32);
        btnTabLogin.TabIndex = 0;
        btnTabLogin.Text = "تسجيل الدخول";
        btnTabLogin.UseVisualStyleBackColor = true;
        btnTabLogin.Click += btnTabLogin_Click;

        // 
        // btnTabRegister
        // 
        btnTabRegister.Location = new System.Drawing.Point(223, 5);
        btnTabRegister.Name = "btnTabRegister";
        btnTabRegister.Radius = 4;
        btnTabRegister.ShadowDepth = 2F;
        btnTabRegister.Size = new System.Drawing.Size(185, 32);
        btnTabRegister.TabIndex = 1;
        btnTabRegister.Text = "حساب جديد";
        btnTabRegister.UseVisualStyleBackColor = true;
        btnTabRegister.Click += btnTabRegister_Click;

        // 
        // panelLogin
        // 
        panelLogin.Controls.Add(btnLogin);
        panelLogin.Controls.Add(btnFreeMode);
        panelLogin.Controls.Add(chkAutoLogin);
        panelLogin.Controls.Add(chkRememberMe);
        panelLogin.Controls.Add(txtLoginPass);
        panelLogin.Controls.Add(lblLoginPass);
        panelLogin.Controls.Add(txtLoginUser);
        panelLogin.Controls.Add(lblLoginUser);
        panelLogin.Location = new System.Drawing.Point(12, 128);
        panelLogin.Name = "panelLogin";
        panelLogin.Size = new System.Drawing.Size(406, 285);
        panelLogin.TabIndex = 2;

        // 
        // lblLoginUser
        // 
        lblLoginUser.AutoSize = true;
        lblLoginUser.ForeColor = System.Drawing.Color.FromArgb(235, 240, 245);
        lblLoginUser.Location = new System.Drawing.Point(15, 12);
        lblLoginUser.Name = "lblLoginUser";
        lblLoginUser.Size = new System.Drawing.Size(130, 15);
        lblLoginUser.TabIndex = 0;
        lblLoginUser.Text = "اسم المستخدم (Username):";

        // 
        // txtLoginUser
        // 
        txtLoginUser.Location = new System.Drawing.Point(15, 32);
        txtLoginUser.Name = "txtLoginUser";
        txtLoginUser.Radius = 4;
        txtLoginUser.Size = new System.Drawing.Size(376, 30);
        txtLoginUser.TabIndex = 1;

        // 
        // lblLoginPass
        // 
        lblLoginPass.AutoSize = true;
        lblLoginPass.ForeColor = System.Drawing.Color.FromArgb(235, 240, 245);
        lblLoginPass.Location = new System.Drawing.Point(15, 75);
        lblLoginPass.Name = "lblLoginPass";
        lblLoginPass.Size = new System.Drawing.Size(126, 15);
        lblLoginPass.TabIndex = 2;
        lblLoginPass.Text = "كلمة المرور (Password):";

        // 
        // txtLoginPass
        // 
        txtLoginPass.Location = new System.Drawing.Point(15, 95);
        txtLoginPass.Name = "txtLoginPass";
        txtLoginPass.Radius = 4;
        txtLoginPass.Size = new System.Drawing.Size(376, 30);
        txtLoginPass.TabIndex = 3;
        txtLoginPass.UseSystemPasswordChar = true;

        // 
        // chkRememberMe
        // 
        chkRememberMe.AutoSize = true;
        chkRememberMe.Checked = true;
        chkRememberMe.CheckState = System.Windows.Forms.CheckState.Checked;
        chkRememberMe.ForeColor = System.Drawing.Color.Silver;
        chkRememberMe.Location = new System.Drawing.Point(15, 140);
        chkRememberMe.Name = "chkRememberMe";
        chkRememberMe.Size = new System.Drawing.Size(148, 19);
        chkRememberMe.TabIndex = 4;
        chkRememberMe.Text = "تذكر بياناتي على هذا الجهاز";
        chkRememberMe.UseVisualStyleBackColor = true;

        // 
        // chkAutoLogin
        // 
        chkAutoLogin.AutoSize = true;
        chkAutoLogin.ForeColor = System.Drawing.Color.Silver;
        chkAutoLogin.Location = new System.Drawing.Point(220, 140);
        chkAutoLogin.Name = "chkAutoLogin";
        chkAutoLogin.Size = new System.Drawing.Size(147, 19);
        chkAutoLogin.TabIndex = 5;
        chkAutoLogin.Text = "تسجيل دخول تلقائي لاحقاً";
        chkAutoLogin.UseVisualStyleBackColor = true;

        // 
        // btnLogin
        // 
        btnLogin.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
        btnLogin.Location = new System.Drawing.Point(15, 185);
        btnLogin.Name = "btnLogin";
        btnLogin.Radius = 5;
        btnLogin.ShadowDepth = 3F;
        btnLogin.Size = new System.Drawing.Size(376, 42);
        btnLogin.TabIndex = 6;
        btnLogin.Text = "تسجيل الدخول ➔";
        btnLogin.UseVisualStyleBackColor = true;
        btnLogin.Click += btnLogin_Click;
        // 
        // btnFreeMode
        // 
        btnFreeMode.Color = System.Drawing.Color.FromArgb(16, 120, 80);
        btnFreeMode.Cursor = System.Windows.Forms.Cursors.Hand;
        btnFreeMode.Font = new System.Drawing.Font("Segoe UI", 9.5F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
        btnFreeMode.ForeColor = System.Drawing.Color.White;
        btnFreeMode.Location = new System.Drawing.Point(15, 235);
        btnFreeMode.Name = "btnFreeMode";
        btnFreeMode.Radius = 5;
        btnFreeMode.ShadowDepth = 2F;
        btnFreeMode.Size = new System.Drawing.Size(376, 38);
        btnFreeMode.TabIndex = 7;
        btnFreeMode.Text = "🆓 الدخول بالنسخة المجانية (Free Mode)";
        btnFreeMode.UseVisualStyleBackColor = true;
        btnFreeMode.Click += btnFreeMode_Click;

        // 
        // panelRegister
        // 
        panelRegister.Controls.Add(btnRegister);
        panelRegister.Controls.Add(txtRegPassConfirm);
        panelRegister.Controls.Add(lblRegPassConfirm);
        panelRegister.Controls.Add(txtRegPass);
        panelRegister.Controls.Add(lblRegPass);
        panelRegister.Controls.Add(txtRegUser);
        panelRegister.Controls.Add(lblRegUser);
        panelRegister.Controls.Add(lblRegNotice);
        panelRegister.Location = new System.Drawing.Point(12, 128);
        panelRegister.Name = "panelRegister";
        panelRegister.Size = new System.Drawing.Size(406, 285);
        panelRegister.TabIndex = 3;
        panelRegister.Visible = false;

        // 
        // lblRegNotice
        // 
        lblRegNotice.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
        lblRegNotice.ForeColor = System.Drawing.Color.FromArgb(245, 175, 65);
        lblRegNotice.Location = new System.Drawing.Point(10, 5);
        lblRegNotice.Name = "lblRegNotice";
        lblRegNotice.Size = new System.Drawing.Size(386, 30);
        lblRegNotice.TabIndex = 0;
        lblRegNotice.Text = "⚠️ الحد الأقصى للنظام 16 حساباً فقط. الحساب يرتبط بجهازك تلقائياً ويُمنع تشغيله على أكثر من جهاز.";

        // 
        // lblRegUser
        // 
        lblRegUser.AutoSize = true;
        lblRegUser.ForeColor = System.Drawing.Color.FromArgb(235, 240, 245);
        lblRegUser.Location = new System.Drawing.Point(15, 40);
        lblRegUser.Name = "lblRegUser";
        lblRegUser.Size = new System.Drawing.Size(107, 15);
        lblRegUser.TabIndex = 1;
        lblRegUser.Text = "اسم المستخدم الجديد:";

        // 
        // txtRegUser
        // 
        txtRegUser.Location = new System.Drawing.Point(15, 58);
        txtRegUser.Name = "txtRegUser";
        txtRegUser.Radius = 4;
        txtRegUser.Size = new System.Drawing.Size(376, 28);
        txtRegUser.TabIndex = 2;

        // 
        // lblRegPass
        // 
        lblRegPass.AutoSize = true;
        lblRegPass.ForeColor = System.Drawing.Color.FromArgb(235, 240, 245);
        lblRegPass.Location = new System.Drawing.Point(15, 93);
        lblRegPass.Name = "lblRegPass";
        lblRegPass.Size = new System.Drawing.Size(65, 15);
        lblRegPass.TabIndex = 3;
        lblRegPass.Text = "كلمة المرور:";

        // 
        // txtRegPass
        // 
        txtRegPass.Location = new System.Drawing.Point(15, 111);
        txtRegPass.Name = "txtRegPass";
        txtRegPass.Radius = 4;
        txtRegPass.Size = new System.Drawing.Size(376, 28);
        txtRegPass.TabIndex = 4;
        txtRegPass.UseSystemPasswordChar = true;

        // 
        // lblRegPassConfirm
        // 
        lblRegPassConfirm.AutoSize = true;
        lblRegPassConfirm.ForeColor = System.Drawing.Color.FromArgb(235, 240, 245);
        lblRegPassConfirm.Location = new System.Drawing.Point(15, 146);
        lblRegPassConfirm.Name = "lblRegPassConfirm";
        lblRegPassConfirm.Size = new System.Drawing.Size(95, 15);
        lblRegPassConfirm.TabIndex = 5;
        lblRegPassConfirm.Text = "تأكيد كلمة المرور:";

        // 
        // txtRegPassConfirm
        // 
        txtRegPassConfirm.Location = new System.Drawing.Point(15, 164);
        txtRegPassConfirm.Name = "txtRegPassConfirm";
        txtRegPassConfirm.Radius = 4;
        txtRegPassConfirm.Size = new System.Drawing.Size(376, 28);
        txtRegPassConfirm.TabIndex = 6;
        txtRegPassConfirm.UseSystemPasswordChar = true;

        // 
        // btnRegister
        // 
        btnRegister.Font = new System.Drawing.Font("Segoe UI", 9.5F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
        btnRegister.Location = new System.Drawing.Point(15, 204);
        btnRegister.Name = "btnRegister";
        btnRegister.Radius = 5;
        btnRegister.ShadowDepth = 3F;
        btnRegister.Size = new System.Drawing.Size(376, 38);
        btnRegister.TabIndex = 7;
        btnRegister.Text = "إنشاء الحساب وربط هذا الجهاز ➔";
        btnRegister.UseVisualStyleBackColor = true;
        btnRegister.Click += btnRegister_Click;

        // 
        // panelBottom
        // 
        panelBottom.Controls.Add(btnAdminPanel);
        panelBottom.Controls.Add(lblServer);
        panelBottom.Controls.Add(lblHwid);
        panelBottom.Controls.Add(lblStatus);
        panelBottom.Controls.Add(progressBar);
        panelBottom.Dock = System.Windows.Forms.DockStyle.Bottom;
        panelBottom.Location = new System.Drawing.Point(0, 420);
        panelBottom.Name = "panelBottom";
        panelBottom.Radius = 0;
        panelBottom.ShadowDepth = 0F;
        panelBottom.Size = new System.Drawing.Size(430, 94);
        panelBottom.TabIndex = 4;

        // 
        // progressBar
        // 
        progressBar.Dock = System.Windows.Forms.DockStyle.Top;
        progressBar.Location = new System.Drawing.Point(0, 0);
        progressBar.Name = "progressBar";
        progressBar.Size = new System.Drawing.Size(430, 4);
        progressBar.TabIndex = 0;
        progressBar.Visible = false;

        // 
        // lblStatus
        // 
        lblStatus.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
        lblStatus.ForeColor = System.Drawing.Color.LightCoral;
        lblStatus.Location = new System.Drawing.Point(12, 6);
        lblStatus.Name = "lblStatus";
        lblStatus.Size = new System.Drawing.Size(406, 44);
        lblStatus.TabIndex = 1;
        lblStatus.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;

        // 
        // lblHwid
        // 
        lblHwid.AutoSize = true;
        lblHwid.Font = new System.Drawing.Font("Segoe UI", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
        lblHwid.ForeColor = System.Drawing.Color.Gray;
        lblHwid.Location = new System.Drawing.Point(12, 58);
        lblHwid.Name = "lblHwid";
        lblHwid.Size = new System.Drawing.Size(65, 13);
        lblHwid.TabIndex = 2;
        lblHwid.Text = "بصمة الجهاز: ...";

        // 
        // lblServer
        // 
        lblServer.Font = new System.Drawing.Font("Segoe UI", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
        lblServer.ForeColor = System.Drawing.Color.FromArgb(160, 175, 185);
        lblServer.Location = new System.Drawing.Point(245, 56);
        lblServer.Name = "lblServer";
        lblServer.Size = new System.Drawing.Size(170, 20);
        lblServer.TabIndex = 3;
        lblServer.Text = "السيرفر: متصل";
        lblServer.TextAlign = System.Drawing.ContentAlignment.MiddleRight;

        // 
        // btnAdminPanel
        // 
        btnAdminPanel.Color = System.Drawing.Color.FromArgb(40, 45, 60);
        btnAdminPanel.Cursor = System.Windows.Forms.Cursors.Hand;
        btnAdminPanel.Font = new System.Drawing.Font("Segoe UI", 7.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
        btnAdminPanel.ForeColor = System.Drawing.Color.FromArgb(170, 190, 220);
        btnAdminPanel.Location = new System.Drawing.Point(145, 54);
        btnAdminPanel.Name = "btnAdminPanel";
        btnAdminPanel.Radius = 3;
        btnAdminPanel.ShadowDepth = 1F;
        btnAdminPanel.Size = new System.Drawing.Size(95, 23);
        btnAdminPanel.TabIndex = 4;
        btnAdminPanel.Text = "لوحة الأدمن ⚙️";
        btnAdminPanel.UseVisualStyleBackColor = true;
        btnAdminPanel.Click += btnAdminPanel_Click;

        // 
        // AuthWindow
        // 
        AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
        AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
        ClientSize = new System.Drawing.Size(430, 514);
        Controls.Add(panelLogin);
        Controls.Add(panelRegister);
        Controls.Add(panelBottom);
        Controls.Add(panelTabs);
        Controls.Add(panelHeader);
        Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
        FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = true;
        Name = "AuthWindow";
        StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
        Text = "Aerobot - تسجيل الدخول والترخيص";
        Load += AuthWindow_Load;
        panelHeader.ResumeLayout(false);
        panelHeader.PerformLayout();
        panelTabs.ResumeLayout(false);
        panelLogin.ResumeLayout(false);
        panelLogin.PerformLayout();
        panelRegister.ResumeLayout(false);
        panelRegister.PerformLayout();
        panelBottom.ResumeLayout(false);
        panelBottom.PerformLayout();
        ResumeLayout(false);
    }

    #endregion

    private SDUI.Controls.Panel panelHeader;
    private SDUI.Controls.Label lblAppTitle;
    private SDUI.Controls.Label lblAppSub;
    private SDUI.Controls.Panel panelTabs;
    private SDUI.Controls.Button btnTabLogin;
    private SDUI.Controls.Button btnTabRegister;
    private System.Windows.Forms.Panel panelLogin;
    private SDUI.Controls.Label lblLoginUser;
    private SDUI.Controls.TextBox txtLoginUser;
    private SDUI.Controls.Label lblLoginPass;
    private SDUI.Controls.TextBox txtLoginPass;
    private SDUI.Controls.CheckBox chkRememberMe;
    private SDUI.Controls.CheckBox chkAutoLogin;
    private SDUI.Controls.Button btnLogin;
    private SDUI.Controls.Button btnFreeMode;
    private System.Windows.Forms.Panel panelRegister;
    private SDUI.Controls.Label lblRegNotice;
    private SDUI.Controls.Label lblRegUser;
    private SDUI.Controls.TextBox txtRegUser;
    private SDUI.Controls.Label lblRegPass;
    private SDUI.Controls.TextBox txtRegPass;
    private SDUI.Controls.Label lblRegPassConfirm;
    private SDUI.Controls.TextBox txtRegPassConfirm;
    private SDUI.Controls.Button btnRegister;
    private SDUI.Controls.Panel panelBottom;
    private SDUI.Controls.ProgressBar progressBar;
    private SDUI.Controls.Label lblStatus;
    private SDUI.Controls.Label lblHwid;
    private SDUI.Controls.Label lblServer;
    private SDUI.Controls.Button btnAdminPanel;
}
