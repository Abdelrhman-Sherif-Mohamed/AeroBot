namespace RSBot.Views
{
    partial class AboutDialog
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
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
            pictureBox1 = new System.Windows.Forms.PictureBox();
            buttonOk = new SDUI.Controls.Button();
            btnWhatsApp = new SDUI.Controls.Button();
            labelName = new SDUI.Controls.Label();
            labelDescription = new SDUI.Controls.Label();
            labelVersion = new SDUI.Controls.Label();
            labelDeveloper = new SDUI.Controls.Label();
            ((System.ComponentModel.ISupportInitialize)pictureBox1).BeginInit();
            SuspendLayout();
            // 
            // pictureBox1
            // 
            pictureBox1.BackColor = System.Drawing.Color.Transparent;
            pictureBox1.Location = new System.Drawing.Point(16, 16);
            pictureBox1.Name = "pictureBox1";
            pictureBox1.Size = new System.Drawing.Size(160, 166);
            pictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            pictureBox1.TabIndex = 0;
            pictureBox1.TabStop = false;
            // 
            // labelName
            // 
            labelName.ApplyGradient = false;
            labelName.AutoSize = true;
            labelName.Font = new System.Drawing.Font("Segoe UI", 16F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            labelName.ForeColor = System.Drawing.Color.FromArgb(245, 215, 100);
            labelName.Location = new System.Drawing.Point(192, 14);
            labelName.Name = "labelName";
            labelName.Size = new System.Drawing.Size(98, 30);
            labelName.TabIndex = 1;
            labelName.Text = "AeroBot";
            // 
            // labelVersion
            // 
            labelVersion.ApplyGradient = false;
            labelVersion.AutoSize = true;
            labelVersion.Font = new System.Drawing.Font("Segoe UI", 9.5F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            labelVersion.ForeColor = System.Drawing.Color.FromArgb(90, 210, 255);
            labelVersion.Location = new System.Drawing.Point(300, 22);
            labelVersion.Name = "labelVersion";
            labelVersion.Size = new System.Drawing.Size(46, 17);
            labelVersion.TabIndex = 2;
            labelVersion.Text = "v2.9.5";
            // 
            // labelDescription
            // 
            labelDescription.ApplyGradient = false;
            labelDescription.AutoSize = true;
            labelDescription.Font = new System.Drawing.Font("Segoe UI", 9.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            labelDescription.ForeColor = System.Drawing.Color.FromArgb(210, 210, 210);
            labelDescription.Location = new System.Drawing.Point(194, 46);
            labelDescription.Name = "labelDescription";
            labelDescription.Size = new System.Drawing.Size(205, 17);
            labelDescription.TabIndex = 3;
            labelDescription.Text = "Advanced Bot for Silkroad Online";
            // 
            // labelDeveloper
            // 
            labelDeveloper.ApplyGradient = false;
            labelDeveloper.AutoSize = true;
            labelDeveloper.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            labelDeveloper.ForeColor = System.Drawing.Color.FromArgb(160, 168, 180);
            labelDeveloper.Location = new System.Drawing.Point(194, 69);
            labelDeveloper.Name = "labelDeveloper";
            labelDeveloper.Size = new System.Drawing.Size(188, 15);
            labelDeveloper.TabIndex = 4;
            labelDeveloper.Text = "(c) Developed by Abdelrhman Sherif";
            // 
            // btnWhatsApp
            // 
            btnWhatsApp.Color = System.Drawing.Color.FromArgb(37, 211, 102);
            btnWhatsApp.Cursor = System.Windows.Forms.Cursors.Hand;
            btnWhatsApp.Font = new System.Drawing.Font("Segoe UI Semibold", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            btnWhatsApp.ForeColor = System.Drawing.Color.White;
            btnWhatsApp.Location = new System.Drawing.Point(194, 98);
            btnWhatsApp.Name = "btnWhatsApp";
            btnWhatsApp.Radius = 6;
            btnWhatsApp.ShadowDepth = 3F;
            btnWhatsApp.Size = new System.Drawing.Size(280, 36);
            btnWhatsApp.TabIndex = 5;
            btnWhatsApp.Text = "💬 تواصل عبر واتساب (+201150238481)";
            btnWhatsApp.UseVisualStyleBackColor = true;
            btnWhatsApp.Click += btnWhatsApp_Click;
            // 
            // buttonOk
            // 
            buttonOk.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
            buttonOk.Color = System.Drawing.Color.FromArgb(50, 60, 75);
            buttonOk.Font = new System.Drawing.Font("Segoe UI Semibold", 9.5F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            buttonOk.ForeColor = System.Drawing.Color.White;
            buttonOk.Location = new System.Drawing.Point(384, 150);
            buttonOk.Name = "buttonOk";
            buttonOk.Radius = 6;
            buttonOk.ShadowDepth = 2F;
            buttonOk.Size = new System.Drawing.Size(90, 32);
            buttonOk.TabIndex = 6;
            buttonOk.Text = "OK";
            buttonOk.UseVisualStyleBackColor = true;
            buttonOk.Click += buttonOk_Click;
            // 
            // AboutDialog
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            BackColor = System.Drawing.Color.FromArgb(17, 24, 39);
            ClientSize = new System.Drawing.Size(496, 200);
            Controls.Add(buttonOk);
            Controls.Add(btnWhatsApp);
            Controls.Add(labelDeveloper);
            Controls.Add(labelDescription);
            Controls.Add(labelVersion);
            Controls.Add(labelName);
            Controls.Add(pictureBox1);
            Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            ForeColor = System.Drawing.Color.White;
            FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MaximumSize = new System.Drawing.Size(512, 240);
            MinimizeBox = false;
            MinimumSize = new System.Drawing.Size(496, 200);
            Name = "AboutDialog";
            ShowIcon = false;
            ShowInTaskbar = false;
            StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            Text = "About AeroBot";
            ((System.ComponentModel.ISupportInitialize)pictureBox1).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private System.Windows.Forms.PictureBox pictureBox1;
        private SDUI.Controls.Button buttonOk;
        private SDUI.Controls.Button btnWhatsApp;
        private SDUI.Controls.Label labelName;
        private SDUI.Controls.Label labelDescription;
        private SDUI.Controls.Label labelVersion;
        private SDUI.Controls.Label labelDeveloper;
    }
}
