namespace RSBot.Views;

partial class UpdateAvailableDialog
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
        lblVersionInfo = new SDUI.Controls.Label();
        lblTitle = new SDUI.Controls.Label();
        picLogo = new System.Windows.Forms.PictureBox();
        lblChangelogTitle = new SDUI.Controls.Label();
        txtChangelog = new System.Windows.Forms.TextBox();
        panelButtons = new System.Windows.Forms.Panel();
        btnUpdate = new SDUI.Controls.Button();
        btnLater = new SDUI.Controls.Button();
        panelHeader.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)picLogo).BeginInit();
        panelButtons.SuspendLayout();
        SuspendLayout();
        // 
        // panelHeader
        // 
        panelHeader.BackColor = System.Drawing.Color.FromArgb(24, 25, 38);
        panelHeader.BorderColor = System.Drawing.Color.FromArgb(50, 52, 70);
        panelHeader.Controls.Add(lblVersionInfo);
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
        // lblVersionInfo
        // 
        lblVersionInfo.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
        lblVersionInfo.ForeColor = System.Drawing.Color.FromArgb(220, 170, 50);
        lblVersionInfo.Location = new System.Drawing.Point(74, 43);
        lblVersionInfo.Name = "lblVersionInfo";
        lblVersionInfo.Size = new System.Drawing.Size(420, 22);
        lblVersionInfo.TabIndex = 2;
        lblVersionInfo.Text = "الإصدار الحالي: 2.9.9   ←   الإصدار الجديد: 2.9.10";
        // 
        // lblTitle
        // 
        lblTitle.Font = new System.Drawing.Font("Segoe UI", 11F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
        lblTitle.ForeColor = System.Drawing.Color.White;
        lblTitle.Location = new System.Drawing.Point(74, 16);
        lblTitle.Name = "lblTitle";
        lblTitle.Size = new System.Drawing.Size(420, 26);
        lblTitle.TabIndex = 1;
        lblTitle.Text = "يتوفر إصدار جديد من Aerobot!";
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
        lblChangelogTitle.Text = "ما الجديد في هذا الإصدار:";
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
        panelButtons.Controls.Add(btnUpdate);
        panelButtons.Controls.Add(btnLater);
        panelButtons.Dock = System.Windows.Forms.DockStyle.Bottom;
        panelButtons.Location = new System.Drawing.Point(0, 308);
        panelButtons.Name = "panelButtons";
        panelButtons.Padding = new System.Windows.Forms.Padding(16, 8, 16, 12);
        panelButtons.Size = new System.Drawing.Size(510, 58);
        panelButtons.TabIndex = 3;
        // 
        // btnUpdate
        // 
        btnUpdate.Color = System.Drawing.Color.FromArgb(40, 140, 70);
        btnUpdate.Cursor = System.Windows.Forms.Cursors.Hand;
        btnUpdate.Dock = System.Windows.Forms.DockStyle.Fill;
        btnUpdate.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
        btnUpdate.ForeColor = System.Drawing.Color.White;
        btnUpdate.Location = new System.Drawing.Point(16, 8);
        btnUpdate.Name = "btnUpdate";
        btnUpdate.Radius = 6;
        btnUpdate.ShadowDepth = 0;
        btnUpdate.Size = new System.Drawing.Size(368, 38);
        btnUpdate.TabIndex = 0;
        btnUpdate.Text = "تحديث الآن";
        btnUpdate.UseVisualStyleBackColor = false;
        btnUpdate.Click += btnUpdate_Click;
        // 
        // btnLater
        // 
        btnLater.Color = System.Drawing.Color.FromArgb(50, 53, 72);
        btnLater.Cursor = System.Windows.Forms.Cursors.Hand;
        btnLater.Dock = System.Windows.Forms.DockStyle.Right;
        btnLater.Font = new System.Drawing.Font("Segoe UI", 9.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
        btnLater.ForeColor = System.Drawing.Color.Gainsboro;
        btnLater.Location = new System.Drawing.Point(384, 8);
        btnLater.Name = "btnLater";
        btnLater.Radius = 6;
        btnLater.ShadowDepth = 0;
        btnLater.Size = new System.Drawing.Size(110, 38);
        btnLater.TabIndex = 1;
        btnLater.Text = "لاحقاً";
        btnLater.UseVisualStyleBackColor = false;
        btnLater.Click += btnLater_Click;
        // 
        // UpdateAvailableDialog
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
        Name = "UpdateAvailableDialog";
        StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
        Text = "Aerobot - تحديث جديد متاح";
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
    private SDUI.Controls.Label lblVersionInfo;
    private SDUI.Controls.Label lblChangelogTitle;
    private System.Windows.Forms.TextBox txtChangelog;
    private System.Windows.Forms.Panel panelButtons;
    private SDUI.Controls.Button btnUpdate;
    private SDUI.Controls.Button btnLater;
}
