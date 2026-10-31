namespace RSBot.Views;

partial class SubscriptionExpiredDialog
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
        lblTitle = new SDUI.Controls.Label();
        lblSubTitle = new SDUI.Controls.Label();
        panelContent = new SDUI.Controls.Panel();
        lblMessage = new SDUI.Controls.Label();
        lblWhatsAppNumber = new SDUI.Controls.Label();
        btnWhatsApp = new SDUI.Controls.Button();
        btnCopyNumber = new SDUI.Controls.Button();
        btnClose = new SDUI.Controls.Button();
        panelHeader.SuspendLayout();
        panelContent.SuspendLayout();
        SuspendLayout();

        // 
        // panelHeader
        // 
        panelHeader.BorderColor = System.Drawing.Color.FromArgb(50, 50, 60);
        panelHeader.Controls.Add(lblSubTitle);
        panelHeader.Controls.Add(lblTitle);
        panelHeader.Dock = System.Windows.Forms.DockStyle.Top;
        panelHeader.Location = new System.Drawing.Point(0, 0);
        panelHeader.Name = "panelHeader";
        panelHeader.Radius = 0;
        panelHeader.ShadowDepth = 0F;
        panelHeader.Size = new System.Drawing.Size(460, 75);
        panelHeader.TabIndex = 0;

        // 
        // lblTitle
        // 
        lblTitle.Dock = System.Windows.Forms.DockStyle.Top;
        lblTitle.Font = new System.Drawing.Font("Segoe UI", 15F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
        lblTitle.ForeColor = System.Drawing.Color.FromArgb(255, 107, 107);
        lblTitle.Location = new System.Drawing.Point(0, 0);
        lblTitle.Name = "lblTitle";
        lblTitle.Size = new System.Drawing.Size(460, 42);
        lblTitle.TabIndex = 0;
        lblTitle.Text = "⏳ انتهت صلاحية اشتراك البوت";
        lblTitle.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;

        // 
        // lblSubTitle
        // 
        lblSubTitle.Dock = System.Windows.Forms.DockStyle.Bottom;
        lblSubTitle.Font = new System.Drawing.Font("Segoe UI", 9.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
        lblSubTitle.ForeColor = System.Drawing.Color.FromArgb(170, 180, 195);
        lblSubTitle.Location = new System.Drawing.Point(0, 42);
        lblSubTitle.Name = "lblSubTitle";
        lblSubTitle.Size = new System.Drawing.Size(460, 33);
        lblSubTitle.TabIndex = 1;
        lblSubTitle.Text = "يرجى شحن الاشتراك للاستمرار في استخدام البوت";
        lblSubTitle.TextAlign = System.Drawing.ContentAlignment.TopCenter;

        // 
        // panelContent
        // 
        panelContent.Controls.Add(btnClose);
        panelContent.Controls.Add(btnCopyNumber);
        panelContent.Controls.Add(btnWhatsApp);
        panelContent.Controls.Add(lblWhatsAppNumber);
        panelContent.Controls.Add(lblMessage);
        panelContent.Dock = System.Windows.Forms.DockStyle.Fill;
        panelContent.Location = new System.Drawing.Point(0, 75);
        panelContent.Name = "panelContent";
        panelContent.Radius = 0;
        panelContent.ShadowDepth = 0F;
        panelContent.Size = new System.Drawing.Size(460, 225);
        panelContent.TabIndex = 1;

        // 
        // lblMessage
        // 
        lblMessage.Font = new System.Drawing.Font("Segoe UI", 10.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
        lblMessage.ForeColor = System.Drawing.Color.FromArgb(230, 235, 245);
        lblMessage.Location = new System.Drawing.Point(20, 10);
        lblMessage.Name = "lblMessage";
        lblMessage.Size = new System.Drawing.Size(420, 45);
        lblMessage.TabIndex = 0;
        lblMessage.Text = "برجاء إرسال رسالة لنا لإعادة شحن البوت";
        lblMessage.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;

        // 
        // lblWhatsAppNumber
        // 
        lblWhatsAppNumber.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
        lblWhatsAppNumber.ForeColor = System.Drawing.Color.FromArgb(37, 211, 102);
        lblWhatsAppNumber.Location = new System.Drawing.Point(20, 55);
        lblWhatsAppNumber.Name = "lblWhatsAppNumber";
        lblWhatsAppNumber.Size = new System.Drawing.Size(420, 28);
        lblWhatsAppNumber.TabIndex = 1;
        lblWhatsAppNumber.Text = "WhatsApp: +201150238481";
        lblWhatsAppNumber.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;

        // 
        // btnWhatsApp
        // 
        btnWhatsApp.Color = System.Drawing.Color.FromArgb(37, 211, 102);
        btnWhatsApp.Cursor = System.Windows.Forms.Cursors.Hand;
        btnWhatsApp.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
        btnWhatsApp.ForeColor = System.Drawing.Color.White;
        btnWhatsApp.Location = new System.Drawing.Point(60, 95);
        btnWhatsApp.Name = "btnWhatsApp";
        btnWhatsApp.Radius = 6;
        btnWhatsApp.ShadowDepth = 3F;
        btnWhatsApp.Size = new System.Drawing.Size(340, 36);
        btnWhatsApp.TabIndex = 2;
        btnWhatsApp.Text = "💬 تواصل معنا عبر واتساب الآن";
        btnWhatsApp.UseVisualStyleBackColor = true;
        btnWhatsApp.Click += btnWhatsApp_Click;

        // 
        // btnCopyNumber
        // 
        btnCopyNumber.Color = System.Drawing.Color.FromArgb(45, 55, 72);
        btnCopyNumber.Cursor = System.Windows.Forms.Cursors.Hand;
        btnCopyNumber.Font = new System.Drawing.Font("Segoe UI", 9.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
        btnCopyNumber.ForeColor = System.Drawing.Color.FromArgb(220, 230, 242);
        btnCopyNumber.Location = new System.Drawing.Point(60, 137);
        btnCopyNumber.Name = "btnCopyNumber";
        btnCopyNumber.Radius = 6;
        btnCopyNumber.ShadowDepth = 2F;
        btnCopyNumber.Size = new System.Drawing.Size(340, 32);
        btnCopyNumber.TabIndex = 3;
        btnCopyNumber.Text = "📋 نسخ رقم الواتساب";
        btnCopyNumber.UseVisualStyleBackColor = true;
        btnCopyNumber.Click += btnCopyNumber_Click;

        // 
        // btnClose
        // 
        btnClose.Color = System.Drawing.Color.FromArgb(35, 40, 50);
        btnClose.Cursor = System.Windows.Forms.Cursors.Hand;
        btnClose.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
        btnClose.ForeColor = System.Drawing.Color.FromArgb(160, 170, 185);
        btnClose.Location = new System.Drawing.Point(140, 178);
        btnClose.Name = "btnClose";
        btnClose.Radius = 5;
        btnClose.ShadowDepth = 2F;
        btnClose.Size = new System.Drawing.Size(180, 28);
        btnClose.TabIndex = 4;
        btnClose.Text = "إغلاق البوت";
        btnClose.UseVisualStyleBackColor = true;
        btnClose.Click += btnClose_Click;

        // 
        // SubscriptionExpiredDialog
        // 
        AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
        AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
        ClientSize = new System.Drawing.Size(460, 300);
        Controls.Add(panelContent);
        Controls.Add(panelHeader);
        FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
        MaximizeBox = false;
        MinimizeBox = false;
        Name = "SubscriptionExpiredDialog";
        StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
        Text = "انتهاء الاشتراك";
        TopMost = true;
        panelHeader.ResumeLayout(false);
        panelContent.ResumeLayout(false);
        ResumeLayout(false);
    }

    #endregion

    private SDUI.Controls.Panel panelHeader;
    private SDUI.Controls.Label lblTitle;
    private SDUI.Controls.Label lblSubTitle;
    private SDUI.Controls.Panel panelContent;
    private SDUI.Controls.Label lblMessage;
    private SDUI.Controls.Label lblWhatsAppNumber;
    private SDUI.Controls.Button btnWhatsApp;
    private SDUI.Controls.Button btnCopyNumber;
    private SDUI.Controls.Button btnClose;
}
