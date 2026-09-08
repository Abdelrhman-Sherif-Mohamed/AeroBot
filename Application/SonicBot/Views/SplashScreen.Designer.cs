namespace RSBot.Views
{
    partial class SplashScreen
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SplashScreen));
            pictureBox = new System.Windows.Forms.PictureBox();
            referenceDataLoader = new System.ComponentModel.BackgroundWorker();
            brandingControl = new SplashBrandingControl();
            lblItems = new SDUI.Controls.Label();
            label2 = new SDUI.Controls.Label();
            lblLoading = new SDUI.Controls.Label();
            progressLoading = new SDUI.Controls.ProgressBar();
            ((System.ComponentModel.ISupportInitialize)pictureBox).BeginInit();
            SuspendLayout();
            // 
            // pictureBox
            // 
            pictureBox.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            pictureBox.BackColor = System.Drawing.Color.Transparent;
            pictureBox.Location = new System.Drawing.Point(20, 24);
            pictureBox.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            pictureBox.Name = "pictureBox";
            pictureBox.Size = new System.Drawing.Size(1240, 300);
            pictureBox.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            pictureBox.TabIndex = 0;
            pictureBox.TabStop = false;
            // 
            // referenceDataLoader
            // 
            referenceDataLoader.WorkerReportsProgress = true;
            referenceDataLoader.DoWork += referenceDataLoader_DoWork;
            referenceDataLoader.ProgressChanged += referenceDataLoader_ProgressChanged;
            // 
            // brandingControl
            // 
            brandingControl.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            brandingControl.BackColor = System.Drawing.Color.Transparent;
            brandingControl.Location = new System.Drawing.Point(0, 340);
            brandingControl.Name = "brandingControl";
            brandingControl.Size = new System.Drawing.Size(1280, 180);
            brandingControl.TabIndex = 1;
            brandingControl.VersionText = "";
            // 
            // lblItems
            // 
            lblItems.ApplyGradient = false;
            lblItems.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            lblItems.BackColor = System.Drawing.Color.Transparent;
            lblItems.Font = new System.Drawing.Font("Segoe UI", 15.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            lblItems.ForeColor = System.Drawing.Color.FromArgb(232, 232, 232);
            lblItems.Gradient = new System.Drawing.Color[]
    {
    System.Drawing.Color.RosyBrown,
    System.Drawing.Color.FromArgb(74, 74, 74)
    };
            lblItems.GradientAnimation = false;
            lblItems.Location = new System.Drawing.Point(0, 525);
            lblItems.Name = "lblItems";
            lblItems.Size = new System.Drawing.Size(1280, 48);
            lblItems.TabIndex = 2;
            lblItems.Text = "Items جارى التحميل";
            lblItems.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // label2
            // 
            label2.ApplyGradient = false;
            label2.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            label2.BackColor = System.Drawing.Color.Transparent;
            label2.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            label2.ForeColor = System.Drawing.Color.FromArgb(230, 230, 230);
            label2.Gradient = new System.Drawing.Color[]
    {
    System.Drawing.Color.RosyBrown,
    System.Drawing.Color.FromArgb(74, 74, 74)
    };
            label2.GradientAnimation = false;
            label2.Location = new System.Drawing.Point(0, 682);
            label2.Name = "label2";
            label2.Size = new System.Drawing.Size(1280, 38);
            label2.TabIndex = 3;
            label2.Text = "Free powerful bot for Silkroad Online servers";
            label2.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // lblLoading
            // 
            lblLoading.ApplyGradient = false;
            lblLoading.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left;
            lblLoading.BackColor = System.Drawing.Color.Transparent;
            lblLoading.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Italic, System.Drawing.GraphicsUnit.Point);
            lblLoading.ForeColor = System.Drawing.Color.FromArgb(200, 200, 200);
            lblLoading.Gradient = new System.Drawing.Color[]
    {
    System.Drawing.Color.Gray,
    System.Drawing.Color.Black
    };
            lblLoading.GradientAnimation = false;
            lblLoading.Location = new System.Drawing.Point(24, 636);
            lblLoading.Name = "lblLoading";
            lblLoading.Size = new System.Drawing.Size(340, 22);
            lblLoading.TabIndex = 4;
            lblLoading.Text = "Loading";
            lblLoading.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // progressLoading
            // 
            progressLoading.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left;
            progressLoading.BackColor = System.Drawing.Color.Transparent;
            progressLoading.DrawHatch = false;
            progressLoading.Gradient = new System.Drawing.Color[]
    {
    System.Drawing.Color.FromArgb(228, 196, 90),
    System.Drawing.Color.FromArgb(228, 196, 90)
    };
            progressLoading.HatchType = System.Drawing.Drawing2D.HatchStyle.Percent10;
            progressLoading.Location = new System.Drawing.Point(24, 660);
            progressLoading.Maximum = 100L;
            progressLoading.MaxPercentShowValue = 100F;
            progressLoading.Name = "progressLoading";
            progressLoading.PercentIndices = 0;
            progressLoading.Radius = 4;
            progressLoading.ShowAsPercent = false;
            progressLoading.ShowValue = false;
            progressLoading.Size = new System.Drawing.Size(320, 20);
            progressLoading.TabIndex = 5;
            progressLoading.Text = "0 / 100";
            progressLoading.Value = 0L;
            // 
            // SplashScreen
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            BorderColor = System.Drawing.Color.DodgerBlue;
            ClientSize = new System.Drawing.Size(1280, 720);
            Controls.Add(label2);
            Controls.Add(progressLoading);
            Controls.Add(lblLoading);
            Controls.Add(lblItems);
            Controls.Add(brandingControl);
            Controls.Add(pictureBox);
            Cursor = System.Windows.Forms.Cursors.AppStarting;
            DrawTitleBorder = false;
            DwmMargin = -1;
            Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            FullDrawHatch = true;
            Hatch = System.Drawing.Drawing2D.HatchStyle.Percent20;
            Icon = (System.Drawing.Icon)resources.GetObject("$this.Icon");
            Location = new System.Drawing.Point(0, 0);
            Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            MaximizeBox = false;
            MinimizeBox = false;
            Movable = false;
            Name = "SplashScreen";
            Padding = new System.Windows.Forms.Padding(0, 0, 0, 0);
            ShowTitle = false;
            SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            Tag = "";
            TopMost = true;
            Load += SplashScreen_Load;
            ((System.ComponentModel.ISupportInitialize)pictureBox).EndInit();
            ResumeLayout(false);
        }

        #endregion

        private System.Windows.Forms.PictureBox pictureBox;
        private System.ComponentModel.BackgroundWorker referenceDataLoader;
        private SplashBrandingControl brandingControl;
        private SDUI.Controls.Label lblItems;
        private SDUI.Controls.Label label2;
        private SDUI.Controls.Label lblLoading;
        private SDUI.Controls.ProgressBar progressLoading;
    }
}