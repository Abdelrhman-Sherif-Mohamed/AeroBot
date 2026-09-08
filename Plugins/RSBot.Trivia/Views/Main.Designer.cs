namespace RSBot.Trivia.Views
{
    partial class Main
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

        #region Component Designer generated code

        private void InitializeComponent()
        {
            groupSettings = new SDUI.Controls.GroupBox();
            chkEnabled = new SDUI.Controls.CheckBox();
            lblQPattern = new SDUI.Controls.Label();
            txtQuestionPattern = new SDUI.Controls.TextBox();
            lblAPattern = new SDUI.Controls.Label();
            txtAnswerPattern = new SDUI.Controls.TextBox();
            lblReplyTo = new SDUI.Controls.Label();
            txtReplyTo = new SDUI.Controls.TextBox();
            btnSaveConfig = new SDUI.Controls.Button();
            lblStatus = new SDUI.Controls.Label();

            groupQA = new SDUI.Controls.GroupBox();
            lblQuestion = new SDUI.Controls.Label();
            txtQuestion = new SDUI.Controls.TextBox();
            lblAnswer = new SDUI.Controls.Label();
            txtAnswer = new SDUI.Controls.TextBox();
            btnAddQA = new SDUI.Controls.Button();
            btnRemoveQA = new SDUI.Controls.Button();
            lstQA = new SDUI.Controls.ListView();
            colQuestion = new System.Windows.Forms.ColumnHeader();
            colAnswer = new System.Windows.Forms.ColumnHeader();
            lblCount = new SDUI.Controls.Label();

            groupSettings.SuspendLayout();
            groupQA.SuspendLayout();
            SuspendLayout();

            // groupSettings
            groupSettings.Controls.Add(chkEnabled);
            groupSettings.Controls.Add(lblQPattern);
            groupSettings.Controls.Add(txtQuestionPattern);
            groupSettings.Controls.Add(lblAPattern);
            groupSettings.Controls.Add(txtAnswerPattern);
            groupSettings.Controls.Add(lblReplyTo);
            groupSettings.Controls.Add(txtReplyTo);
            groupSettings.Controls.Add(btnSaveConfig);
            groupSettings.Controls.Add(lblStatus);
            groupSettings.Location = new System.Drawing.Point(12, 12);
            groupSettings.Name = "groupSettings";
            groupSettings.Size = new System.Drawing.Size(726, 150);
            groupSettings.TabIndex = 0;
            groupSettings.TabStop = false;
            groupSettings.Text = "Settings & Patterns";

            // chkEnabled
            chkEnabled.AutoSize = false;
            chkEnabled.Location = new System.Drawing.Point(15, 22);
            chkEnabled.Name = "chkEnabled";
            chkEnabled.Size = new System.Drawing.Size(220, 26);
            chkEnabled.TabIndex = 0;
            chkEnabled.Text = "Enable Auto Trivia Solver";
            chkEnabled.UseVisualStyleBackColor = true;
            chkEnabled.CheckedChanged += chkEnabled_CheckedChanged;

            // lblQPattern
            lblQPattern.AutoSize = true;
            lblQPattern.Location = new System.Drawing.Point(15, 55);
            lblQPattern.Name = "lblQPattern";
            lblQPattern.Size = new System.Drawing.Size(97, 15);
            lblQPattern.TabIndex = 1;
            lblQPattern.Text = "Question Pattern:";

            // txtQuestionPattern
            txtQuestionPattern.Location = new System.Drawing.Point(120, 52);
            txtQuestionPattern.Name = "txtQuestionPattern";
            txtQuestionPattern.Size = new System.Drawing.Size(320, 23);
            txtQuestionPattern.TabIndex = 2;

            // lblAPattern
            lblAPattern.AutoSize = true;
            lblAPattern.Location = new System.Drawing.Point(15, 85);
            lblAPattern.Name = "lblAPattern";
            lblAPattern.Size = new System.Drawing.Size(89, 15);
            lblAPattern.TabIndex = 3;
            lblAPattern.Text = "Answer Pattern:";

            // txtAnswerPattern
            txtAnswerPattern.Location = new System.Drawing.Point(120, 82);
            txtAnswerPattern.Name = "txtAnswerPattern";
            txtAnswerPattern.Size = new System.Drawing.Size(320, 23);
            txtAnswerPattern.TabIndex = 4;

            // lblReplyTo
            lblReplyTo.AutoSize = true;
            lblReplyTo.Location = new System.Drawing.Point(460, 55);
            lblReplyTo.Name = "lblReplyTo";
            lblReplyTo.Size = new System.Drawing.Size(80, 15);
            lblReplyTo.TabIndex = 5;
            lblReplyTo.Text = "Reply in PM to:";

            // txtReplyTo
            txtReplyTo.Location = new System.Drawing.Point(550, 52);
            txtReplyTo.Name = "txtReplyTo";
            txtReplyTo.Size = new System.Drawing.Size(160, 23);
            txtReplyTo.TabIndex = 6;

            // btnSaveConfig
            btnSaveConfig.Location = new System.Drawing.Point(550, 82);
            btnSaveConfig.Name = "btnSaveConfig";
            btnSaveConfig.Size = new System.Drawing.Size(160, 26);
            btnSaveConfig.TabIndex = 7;
            btnSaveConfig.Text = "Save Settings";
            btnSaveConfig.UseVisualStyleBackColor = true;
            btnSaveConfig.Click += btnSaveConfig_Click;

            // lblStatus
            lblStatus.Location = new System.Drawing.Point(15, 118);
            lblStatus.Name = "lblStatus";
            lblStatus.Size = new System.Drawing.Size(695, 20);
            lblStatus.TabIndex = 8;
            lblStatus.Text = "Status: Idle";

            // groupQA
            groupQA.Controls.Add(lblQuestion);
            groupQA.Controls.Add(txtQuestion);
            groupQA.Controls.Add(lblAnswer);
            groupQA.Controls.Add(txtAnswer);
            groupQA.Controls.Add(btnAddQA);
            groupQA.Controls.Add(btnRemoveQA);
            groupQA.Controls.Add(lstQA);
            groupQA.Controls.Add(lblCount);
            groupQA.Location = new System.Drawing.Point(12, 170);
            groupQA.Name = "groupQA";
            groupQA.Size = new System.Drawing.Size(726, 275);
            groupQA.TabIndex = 1;
            groupQA.TabStop = false;
            groupQA.Text = "Trivia Questions & Answers Database";

            // lblQuestion
            lblQuestion.AutoSize = true;
            lblQuestion.Location = new System.Drawing.Point(15, 25);
            lblQuestion.Name = "lblQuestion";
            lblQuestion.Size = new System.Drawing.Size(58, 15);
            lblQuestion.TabIndex = 0;
            lblQuestion.Text = "Question:";

            // txtQuestion
            txtQuestion.Location = new System.Drawing.Point(78, 22);
            txtQuestion.Name = "txtQuestion";
            txtQuestion.Size = new System.Drawing.Size(250, 23);
            txtQuestion.TabIndex = 1;

            // lblAnswer
            lblAnswer.AutoSize = true;
            lblAnswer.Location = new System.Drawing.Point(340, 25);
            lblAnswer.Name = "lblAnswer";
            lblAnswer.Size = new System.Drawing.Size(49, 15);
            lblAnswer.TabIndex = 2;
            lblAnswer.Text = "Answer:";

            // txtAnswer
            txtAnswer.Location = new System.Drawing.Point(395, 22);
            txtAnswer.Name = "txtAnswer";
            txtAnswer.Size = new System.Drawing.Size(170, 23);
            txtAnswer.TabIndex = 3;

            // btnAddQA
            btnAddQA.Location = new System.Drawing.Point(575, 21);
            btnAddQA.Name = "btnAddQA";
            btnAddQA.Size = new System.Drawing.Size(65, 25);
            btnAddQA.TabIndex = 4;
            btnAddQA.Text = "Add";
            btnAddQA.UseVisualStyleBackColor = true;
            btnAddQA.Click += btnAddQA_Click;

            // btnRemoveQA
            btnRemoveQA.Location = new System.Drawing.Point(645, 21);
            btnRemoveQA.Name = "btnRemoveQA";
            btnRemoveQA.Size = new System.Drawing.Size(65, 25);
            btnRemoveQA.TabIndex = 5;
            btnRemoveQA.Text = "Delete";
            btnRemoveQA.UseVisualStyleBackColor = true;
            btnRemoveQA.Click += btnRemoveQA_Click;

            // lstQA
            lstQA.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] { colQuestion, colAnswer });
            lstQA.FullRowSelect = true;
            lstQA.Location = new System.Drawing.Point(15, 55);
            lstQA.Name = "lstQA";
            lstQA.Size = new System.Drawing.Size(695, 185);
            lstQA.TabIndex = 6;
            lstQA.UseCompatibleStateImageBehavior = false;
            lstQA.View = System.Windows.Forms.View.Details;

            colQuestion.Text = "Question";
            colQuestion.Width = 460;
            colAnswer.Text = "Answer";
            colAnswer.Width = 210;

            // lblCount
            lblCount.Location = new System.Drawing.Point(15, 248);
            lblCount.Name = "lblCount";
            lblCount.Size = new System.Drawing.Size(300, 20);
            lblCount.TabIndex = 7;
            lblCount.Text = "Total Q&A: 0";

            // Main
            AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            Controls.Add(groupSettings);
            Controls.Add(groupQA);
            Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            Name = "Main";
            Size = new System.Drawing.Size(750, 458);
            Load += Main_Load;
            groupSettings.ResumeLayout(false);
            groupSettings.PerformLayout();
            groupQA.ResumeLayout(false);
            groupQA.PerformLayout();
            ResumeLayout(false);
        }

        #endregion

        private SDUI.Controls.GroupBox groupSettings;
        private SDUI.Controls.CheckBox chkEnabled;
        private SDUI.Controls.Label lblQPattern;
        private SDUI.Controls.TextBox txtQuestionPattern;
        private SDUI.Controls.Label lblAPattern;
        private SDUI.Controls.TextBox txtAnswerPattern;
        private SDUI.Controls.Label lblReplyTo;
        private SDUI.Controls.TextBox txtReplyTo;
        private SDUI.Controls.Button btnSaveConfig;
        private SDUI.Controls.Label lblStatus;

        private SDUI.Controls.GroupBox groupQA;
        private SDUI.Controls.Label lblQuestion;
        private SDUI.Controls.TextBox txtQuestion;
        private SDUI.Controls.Label lblAnswer;
        private SDUI.Controls.TextBox txtAnswer;
        private SDUI.Controls.Button btnAddQA;
        private SDUI.Controls.Button btnRemoveQA;
        private SDUI.Controls.ListView lstQA;
        private System.Windows.Forms.ColumnHeader colQuestion;
        private System.Windows.Forms.ColumnHeader colAnswer;
        private SDUI.Controls.Label lblCount;
    }
}
