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
            logoLabel = new SDUI.Controls.Label();
            labelVersion = new SDUI.Controls.Label();
            label2 = new SDUI.Controls.Label();
            label3 = new SDUI.Controls.Label();
            lblLoading = new SDUI.Controls.Label();
            progressLoading = new SDUI.Controls.ProgressBar();
            ((System.ComponentModel.ISupportInitialize)pictureBox).BeginInit();
            SuspendLayout();
            // 
            // pictureBox
            // 
            pictureBox.BackColor = System.Drawing.Color.Transparent;
            pictureBox.Dock = System.Windows.Forms.DockStyle.Fill;
            pictureBox.Image = Properties.Resources.app;
            pictureBox.Location = new System.Drawing.Point(0, 0);
            pictureBox.Margin = new System.Windows.Forms.Padding(0);
            pictureBox.Name = "pictureBox";
            pictureBox.Size = new System.Drawing.Size(650, 376);
            pictureBox.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            pictureBox.TabIndex = 0;
            pictureBox.TabStop = false;
            // 
            // referenceDataLoader
            // 
            referenceDataLoader.WorkerReportsProgress = true;
            referenceDataLoader.DoWork += referenceDataLoader_DoWork;
            referenceDataLoader.ProgressChanged += referenceDataLoader_ProgressChanged;
            // 
            // logoLabel
            // 
            logoLabel.Visible = false;
            // 
            // labelVersion
            // 
            labelVersion.Visible = false;
            // 
            // label2
            // 
            label2.Visible = false;
            // 
            // label3
            // 
            label3.Visible = false;
            // 
            // lblLoading
            // 
            lblLoading.ApplyGradient = false;
            lblLoading.BackColor = System.Drawing.Color.Transparent;
            lblLoading.Dock = System.Windows.Forms.DockStyle.Bottom;
            lblLoading.Font = new System.Drawing.Font("Segoe UI Semibold", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            lblLoading.ForeColor = System.Drawing.Color.White;
            lblLoading.Location = new System.Drawing.Point(0, 348);
            lblLoading.Name = "lblLoading";
            lblLoading.Size = new System.Drawing.Size(650, 20);
            lblLoading.TabIndex = 6;
            lblLoading.Text = "Loading...";
            lblLoading.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // progressLoading
            // 
            progressLoading.BackColor = System.Drawing.Color.Transparent;
            progressLoading.Dock = System.Windows.Forms.DockStyle.Bottom;
            progressLoading.DrawHatch = false;
            progressLoading.Gradient = new System.Drawing.Color[]
            {
                System.Drawing.Color.FromArgb(220, 160, 40),
                System.Drawing.Color.FromArgb(255, 200, 80)
            };
            progressLoading.HatchType = System.Drawing.Drawing2D.HatchStyle.Percent10;
            progressLoading.Location = new System.Drawing.Point(0, 368);
            progressLoading.Maximum = 100L;
            progressLoading.MaxPercentShowValue = 100F;
            progressLoading.Name = "progressLoading";
            progressLoading.PercentIndices = 0;
            progressLoading.Radius = 0;
            progressLoading.ShowAsPercent = false;
            progressLoading.ShowValue = false;
            progressLoading.Size = new System.Drawing.Size(650, 8);
            progressLoading.TabIndex = 7;
            progressLoading.Text = "0 / 100";
            progressLoading.Value = 0L;
            // 
            // SplashScreen
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            BackColor = System.Drawing.Color.Black;
            BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            BorderColor = System.Drawing.Color.Empty;
            ClientSize = new System.Drawing.Size(650, 376);
            ControlBox = false;
            Controls.Add(lblLoading);
            Controls.Add(progressLoading);
            Controls.Add(pictureBox);
            Cursor = System.Windows.Forms.Cursors.AppStarting;
            DrawTitleBorder = false;
            Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            Hatch = System.Drawing.Drawing2D.HatchStyle.Percent30;
            Icon = (System.Drawing.Icon)resources.GetObject("$this.Icon");
            Location = new System.Drawing.Point(0, 0);
            Margin = new System.Windows.Forms.Padding(0);
            Movable = false;
            Name = "SplashScreen";
            Padding = new System.Windows.Forms.Padding(0);
            ShowTitle = false;
            SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            Tag = "";
            TopMost = false;
            Load += SplashScreen_Load;
            ((System.ComponentModel.ISupportInitialize)pictureBox).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private System.Windows.Forms.PictureBox pictureBox;
        private System.ComponentModel.BackgroundWorker referenceDataLoader;
        private SDUI.Controls.Label logoLabel;
        private SDUI.Controls.Label labelVersion;
        private SDUI.Controls.Label label2;
        private SDUI.Controls.Label label3;
        private SDUI.Controls.Label lblLoading;
        private SDUI.Controls.ProgressBar progressLoading;
    }
}