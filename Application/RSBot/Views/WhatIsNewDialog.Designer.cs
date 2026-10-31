namespace RSBot.Views;

partial class WhatIsNewDialog
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
        lblSub = new SDUI.Controls.Label();
        lblTitle = new SDUI.Controls.Label();
        picLogo = new System.Windows.Forms.PictureBox();
        lblChangelogTitle = new SDUI.Controls.Label();
        txtChangelog = new System.Windows.Forms.TextBox();
        panelButtons = new System.Windows.Forms.Panel();
        btnOk = new SDUI.Controls.Button();
        panelHeader.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)picLogo).BeginInit();
        panelButtons.SuspendLayout();
        SuspendLayout();
        // 
        // panelHeader
        // 
        panelHeader.BackColor = System.Drawing.Color.FromArgb(24, 25, 38);
        panelHeader.BorderColor = System.Drawing.Color.FromArgb(50, 52, 70);
        panelHeader.Controls.Add(lblSub);
        panelHeader.Controls.Add(lblTitle);
        panelHeader.Controls.Add(picLogo);
        panelHeader.Dock = System.Windows.Forms.DockStyle.Top;
        panelHeader.Location = new System.Drawing.Point(0, 0);
        panelHeader.Name = "panelHeader";
        panelHeader.Radius = 0;
        panelHeader.ShadowDepth = 0;
        panelHeader.Size = new System.Drawing.Size(510, 80);
        panelHeader.TabIndex = 0;
        // 
        // lblSub
        // 
        lblSub.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
        lblSub.ForeColor = System.Drawing.Color.FromArgb(120, 210, 140);
        lblSub.Location = new System.Drawing.Point(74, 43);
        lblSub.Name = "lblSub";
        lblSub.Size = new System.Drawing.Size(420, 22);
        lblSub.TabIndex = 2;
        lblSub.Text = "أهلاً بك في النسخة الجديدة! إليك قائمة التغييرات والميزات المضافة:";
        // 
        // lblTitle
        // 
        lblTitle.Font = new System.Drawing.Font("Segoe UI", 11F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
        lblTitle.ForeColor = System.Drawing.Color.White;
        lblTitle.Location = new System.Drawing.Point(74, 16);
        lblTitle.Name = "lblTitle";
        lblTitle.Size = new System.Drawing.Size(420, 26);
        lblTitle.TabIndex = 1;
        lblTitle.Text = "تم تحديث Aerobot بنجاح!";
        // 
        // picLogo
        // 
        picLogo.Image = Properties.Resources.app;
        picLogo.Location = new System.Drawing.Point(16, 16);
        picLogo.Name = "picLogo";
        picLogo.Size = new System.Drawing.Size(48, 48);
        picLogo.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
        picLogo.TabIndex = 0;
        picLogo.TabStop = false;
        // 
        // lblChangelogTitle
        // 
        lblChangelogTitle.AutoSize = true;
        lblChangelogTitle.Font = new System.Drawing.Font("Segoe UI Semibold", 9.5F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
        lblChangelogTitle.ForeColor = System.Drawing.Color.Gainsboro;
        lblChangelogTitle.Location = new System.Drawing.Point(16, 92);
        lblChangelogTitle.Name = "lblChangelogTitle";
        lblChangelogTitle.Size = new System.Drawing.Size(130, 17);
        lblChangelogTitle.TabIndex = 1;
        lblChangelogTitle.Text = "ما الجديد في هذا التحديث:";
        // 
        // txtChangelog
        // 
        txtChangelog.BackColor = System.Drawing.Color.FromArgb(20, 21, 30);
        txtChangelog.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
        txtChangelog.Font = new System.Drawing.Font("Segoe UI", 9.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
        txtChangelog.ForeColor = System.Drawing.Color.WhiteSmoke;
        txtChangelog.Location = new System.Drawing.Point(16, 115);
        txtChangelog.Multiline = true;
        txtChangelog.Name = "txtChangelog";
        txtChangelog.ReadOnly = true;
        txtChangelog.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
        txtChangelog.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
        txtChangelog.Size = new System.Drawing.Size(478, 180);
        txtChangelog.TabIndex = 2;
        // 
        // panelButtons
        // 
        panelButtons.Controls.Add(btnOk);
        panelButtons.Dock = System.Windows.Forms.DockStyle.Bottom;
        panelButtons.Location = new System.Drawing.Point(0, 308);
        panelButtons.Name = "panelButtons";
        panelButtons.Padding = new System.Windows.Forms.Padding(16, 8, 16, 12);
        panelButtons.Size = new System.Drawing.Size(510, 58);
        panelButtons.TabIndex = 3;
        // 
        // btnOk
        // 
        btnOk.Color = System.Drawing.Color.FromArgb(40, 140, 70);
        btnOk.Cursor = System.Windows.Forms.Cursors.Hand;
        btnOk.Dock = System.Windows.Forms.DockStyle.Fill;
        btnOk.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
        btnOk.ForeColor = System.Drawing.Color.White;
        btnOk.Location = new System.Drawing.Point(16, 8);
        btnOk.Name = "btnOk";
        btnOk.Radius = 6;
        btnOk.ShadowDepth = 0;
        btnOk.Size = new System.Drawing.Size(478, 38);
        btnOk.TabIndex = 0;
        btnOk.Text = "حسناً، متابعة التشغيل";
        btnOk.UseVisualStyleBackColor = false;
        btnOk.Click += btnOk_Click;
        // 
        // WhatIsNewDialog
        // 
        AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
        AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
        BackColor = System.Drawing.Color.FromArgb(16, 17, 24);
        ClientSize = new System.Drawing.Size(510, 366);
        Controls.Add(panelButtons);
        Controls.Add(txtChangelog);
        Controls.Add(lblChangelogTitle);
        Controls.Add(panelHeader);
        Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
        FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        Name = "WhatIsNewDialog";
        StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
        Text = "Aerobot - تم التحديث بنجاح";
        panelHeader.ResumeLayout(false);
        ((System.ComponentModel.ISupportInitialize)picLogo).EndInit();
        panelButtons.ResumeLayout(false);
        ResumeLayout(false);
        PerformLayout();
    }

    #endregion

    private SDUI.Controls.Panel panelHeader;
    private System.Windows.Forms.PictureBox picLogo;
    private SDUI.Controls.Label lblTitle;
    private SDUI.Controls.Label lblSub;
    private SDUI.Controls.Label lblChangelogTitle;
    private System.Windows.Forms.TextBox txtChangelog;
    private System.Windows.Forms.Panel panelButtons;
    private SDUI.Controls.Button btnOk;
}
