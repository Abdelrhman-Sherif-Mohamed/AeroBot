namespace RSBot.Quest.Views
{
    partial class Main
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
            treeQuests = new System.Windows.Forms.TreeView();
            checkShowCompleted = new SDUI.Controls.CheckBox();
            btnRefresh = new SDUI.Controls.Button();
            btnWatch = new SDUI.Controls.Button();
            btnAbandon = new SDUI.Controls.Button();
            contextQuest = new SDUI.Controls.ContextMenuStrip();
            watchQuestToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            abandonToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            contextQuest.SuspendLayout();
            SuspendLayout();
            // 
            // treeQuests
            // 
            treeQuests.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            treeQuests.FullRowSelect = true;
            treeQuests.HideSelection = false;
            treeQuests.Location = new System.Drawing.Point(12, 12);
            treeQuests.Name = "treeQuests";
            treeQuests.Size = new System.Drawing.Size(680, 480);
            treeQuests.TabIndex = 0;
            treeQuests.NodeMouseClick += treeQuests_NodeMouseClick;
            // 
            // checkShowCompleted
            // 
            checkShowCompleted.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left;
            checkShowCompleted.AutoSize = true;
            checkShowCompleted.Depth = 0;
            checkShowCompleted.Location = new System.Drawing.Point(12, 502);
            checkShowCompleted.Margin = new System.Windows.Forms.Padding(0);
            checkShowCompleted.MouseLocation = new System.Drawing.Point(-1, -1);
            checkShowCompleted.Name = "checkShowCompleted";
            checkShowCompleted.Ripple = true;
            checkShowCompleted.Size = new System.Drawing.Size(145, 30);
            checkShowCompleted.TabIndex = 1;
            checkShowCompleted.Text = "Show completed";
            checkShowCompleted.UseVisualStyleBackColor = true;
            checkShowCompleted.CheckedChanged += checkShowCompleted_CheckedChanged;
            // 
            // btnRefresh
            // 
            btnRefresh.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left;
            btnRefresh.Location = new System.Drawing.Point(230, 502);
            btnRefresh.Name = "btnRefresh";
            btnRefresh.Radius = 4;
            btnRefresh.Size = new System.Drawing.Size(120, 30);
            btnRefresh.TabIndex = 2;
            btnRefresh.Text = "Refresh";
            btnRefresh.UseVisualStyleBackColor = true;
            btnRefresh.Click += btnRefresh_Click;
            // 
            // btnWatch
            // 
            btnWatch.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left;
            btnWatch.Location = new System.Drawing.Point(360, 502);
            btnWatch.Name = "btnWatch";
            btnWatch.Radius = 4;
            btnWatch.Size = new System.Drawing.Size(140, 30);
            btnWatch.TabIndex = 3;
            btnWatch.Text = "Watch / Unwatch";
            btnWatch.UseVisualStyleBackColor = true;
            btnWatch.Click += watchQuestToolStripMenuItem_Click;
            // 
            // btnAbandon
            // 
            btnAbandon.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left;
            btnAbandon.Location = new System.Drawing.Point(510, 502);
            btnAbandon.Name = "btnAbandon";
            btnAbandon.Radius = 4;
            btnAbandon.Size = new System.Drawing.Size(130, 30);
            btnAbandon.TabIndex = 4;
            btnAbandon.Text = "Abandon Quest";
            btnAbandon.UseVisualStyleBackColor = true;
            btnAbandon.Click += abandonToolStripMenuItem_Click;
            // 
            // contextQuest
            // 
            contextQuest.Items.AddRange(new System.Windows.Forms.ToolStripItem[] { watchQuestToolStripMenuItem, toolStripSeparator1, abandonToolStripMenuItem });
            contextQuest.Name = "contextMenuStrip1";
            contextQuest.Size = new System.Drawing.Size(181, 76);
            // 
            // watchQuestToolStripMenuItem
            // 
            watchQuestToolStripMenuItem.ForeColor = System.Drawing.Color.FromArgb(0, 0, 0);
            watchQuestToolStripMenuItem.Name = "watchQuestToolStripMenuItem";
            watchQuestToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
            watchQuestToolStripMenuItem.Text = "Watch / unwatch";
            watchQuestToolStripMenuItem.Click += watchQuestToolStripMenuItem_Click;
            // 
            // toolStripSeparator1
            // 
            toolStripSeparator1.Name = "toolStripSeparator1";
            toolStripSeparator1.Size = new System.Drawing.Size(177, 6);
            // 
            // abandonToolStripMenuItem
            // 
            abandonToolStripMenuItem.ForeColor = System.Drawing.Color.FromArgb(0, 0, 0);
            abandonToolStripMenuItem.Name = "abandonToolStripMenuItem";
            abandonToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
            abandonToolStripMenuItem.Text = "Abandon";
            abandonToolStripMenuItem.Click += abandonToolStripMenuItem_Click;
            // 
            // Main
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            Controls.Add(btnAbandon);
            Controls.Add(btnWatch);
            Controls.Add(btnRefresh);
            Controls.Add(checkShowCompleted);
            Controls.Add(treeQuests);
            Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            Name = "Main";
            Size = new System.Drawing.Size(704, 545);
            Load += Main_Load;
            contextQuest.ResumeLayout(false);
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private System.Windows.Forms.TreeView treeQuests;
        private SDUI.Controls.CheckBox checkShowCompleted;
        private SDUI.Controls.Button btnRefresh;
        private SDUI.Controls.Button btnWatch;
        private SDUI.Controls.Button btnAbandon;
        private SDUI.Controls.ContextMenuStrip contextQuest;
        private System.Windows.Forms.ToolStripMenuItem watchQuestToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem abandonToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
    }
}
