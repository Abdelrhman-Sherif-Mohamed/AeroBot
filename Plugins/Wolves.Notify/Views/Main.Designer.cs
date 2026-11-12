namespace Wolves.Notify.Views
{
    partial class Main
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
                components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            rootLayout = new System.Windows.Forms.TableLayoutPanel();
            grpChannels = new SDUI.Controls.GroupBox();
            grpEvents = new SDUI.Controls.GroupBox();
            grpHistory = new SDUI.Controls.GroupBox();

            chkMasterEnabled = new SDUI.Controls.CheckBox();
            chkRichCards = new SDUI.Controls.CheckBox();
            lblCooldown = new SDUI.Controls.Label();
            numCooldown = new SDUI.Controls.NumUpDown();

            chkTelegramEnabled = new SDUI.Controls.CheckBox();
            lblTelegramToken = new SDUI.Controls.Label();
            txtTelegramToken = new SDUI.Controls.TextBox();
            lblTelegramChatId = new SDUI.Controls.Label();
            txtTelegramChatId = new SDUI.Controls.TextBox();
            btnTestTelegram = new SDUI.Controls.Button();

            chkDiscordEnabled = new SDUI.Controls.CheckBox();
            lblDiscordWebhook = new SDUI.Controls.Label();
            txtDiscordWebhook = new SDUI.Controls.TextBox();
            btnTestDiscord = new SDUI.Controls.Button();

            lblUniqueKeywords = new SDUI.Controls.Label();
            txtUniqueKeywords = new SDUI.Controls.TextBox();

            btnSave = new SDUI.Controls.Button();
            btnSelectAllEvents = new SDUI.Controls.Button();
            btnSelectCoreEvents = new SDUI.Controls.Button();
            lblStatus = new SDUI.Controls.Label();

            tlpEvents = new System.Windows.Forms.TableLayoutPanel();
            txtHistory = new System.Windows.Forms.RichTextBox();
            btnClearHistory = new SDUI.Controls.Button();

            SuspendLayout();
            rootLayout.SuspendLayout();
            grpChannels.SuspendLayout();
            grpEvents.SuspendLayout();
            grpHistory.SuspendLayout();

            // Root layout
            rootLayout.ColumnCount = 1;
            rootLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            rootLayout.RowCount = 3;
            rootLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 265F));
            rootLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 195F));
            rootLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            rootLayout.Dock = System.Windows.Forms.DockStyle.Fill;
            rootLayout.Padding = new System.Windows.Forms.Padding(10);
            rootLayout.Controls.Add(grpChannels, 0, 0);
            rootLayout.Controls.Add(grpEvents, 0, 1);
            rootLayout.Controls.Add(grpHistory, 0, 2);

            // Group: Channels
            grpChannels.Dock = System.Windows.Forms.DockStyle.Fill;
            grpChannels.Padding = new System.Windows.Forms.Padding(12, 24, 12, 10);
            grpChannels.Text = "Channels & Credentials (Telegram / Discord)";
            grpChannels.Controls.Add(chkMasterEnabled);
            grpChannels.Controls.Add(chkRichCards);
            grpChannels.Controls.Add(lblCooldown);
            grpChannels.Controls.Add(numCooldown);
            grpChannels.Controls.Add(chkTelegramEnabled);
            grpChannels.Controls.Add(lblTelegramToken);
            grpChannels.Controls.Add(txtTelegramToken);
            grpChannels.Controls.Add(lblTelegramChatId);
            grpChannels.Controls.Add(txtTelegramChatId);
            grpChannels.Controls.Add(btnTestTelegram);
            grpChannels.Controls.Add(chkDiscordEnabled);
            grpChannels.Controls.Add(lblDiscordWebhook);
            grpChannels.Controls.Add(txtDiscordWebhook);
            grpChannels.Controls.Add(btnTestDiscord);
            grpChannels.Controls.Add(lblUniqueKeywords);
            grpChannels.Controls.Add(txtUniqueKeywords);
            grpChannels.Controls.Add(btnSave);
            grpChannels.Controls.Add(lblStatus);

            // Row 1: Global controls
            chkMasterEnabled.AutoSize = true;
            chkMasterEnabled.Location = new System.Drawing.Point(16, 28);
            chkMasterEnabled.Text = "Enable Notifications";

            chkRichCards.AutoSize = true;
            chkRichCards.Location = new System.Drawing.Point(185, 28);
            chkRichCards.Text = "Rich formatted cards";

            lblCooldown.AutoSize = true;
            lblCooldown.Location = new System.Drawing.Point(375, 31);
            lblCooldown.Text = "Cooldown (s):";

            numCooldown.Location = new System.Drawing.Point(475, 26);
            numCooldown.Size = new System.Drawing.Size(75, 25);
            numCooldown.Maximum = 3600M;
            numCooldown.Minimum = 0M;
            numCooldown.Value = 10M;

            btnSave.Location = new System.Drawing.Point(570, 25);
            btnSave.Size = new System.Drawing.Size(130, 27);
            btnSave.Radius = 6;
            btnSave.Text = "Save Settings";
            btnSave.Click += btnSave_Click;

            // Row 2: Telegram
            chkTelegramEnabled.AutoSize = true;
            chkTelegramEnabled.Location = new System.Drawing.Point(16, 65);
            chkTelegramEnabled.Text = "Telegram";

            lblTelegramToken.AutoSize = true;
            lblTelegramToken.Location = new System.Drawing.Point(115, 68);
            lblTelegramToken.Text = "Bot Token:";

            txtTelegramToken.Location = new System.Drawing.Point(195, 63);
            txtTelegramToken.Size = new System.Drawing.Size(215, 24);
            txtTelegramToken.UseSystemPasswordChar = true;
            txtTelegramToken.PassFocusShow = true;

            lblTelegramChatId.AutoSize = true;
            lblTelegramChatId.Location = new System.Drawing.Point(420, 68);
            lblTelegramChatId.Text = "Chat ID:";

            txtTelegramChatId.Location = new System.Drawing.Point(480, 63);
            txtTelegramChatId.Size = new System.Drawing.Size(125, 24);

            btnTestTelegram.Location = new System.Drawing.Point(615, 62);
            btnTestTelegram.Size = new System.Drawing.Size(115, 26);
            btnTestTelegram.Radius = 6;
            btnTestTelegram.Text = "Test Telegram";
            btnTestTelegram.Click += btnTestTelegram_Click;

            // Row 3: Discord
            chkDiscordEnabled.AutoSize = true;
            chkDiscordEnabled.Location = new System.Drawing.Point(16, 102);
            chkDiscordEnabled.Text = "Discord";

            lblDiscordWebhook.AutoSize = true;
            lblDiscordWebhook.Location = new System.Drawing.Point(115, 105);
            lblDiscordWebhook.Text = "Webhook:";

            txtDiscordWebhook.Location = new System.Drawing.Point(195, 100);
            txtDiscordWebhook.Size = new System.Drawing.Size(410, 24);
            txtDiscordWebhook.UseSystemPasswordChar = true;
            txtDiscordWebhook.PassFocusShow = true;

            btnTestDiscord.Location = new System.Drawing.Point(615, 99);
            btnTestDiscord.Size = new System.Drawing.Size(115, 26);
            btnTestDiscord.Radius = 6;
            btnTestDiscord.Text = "Test Discord";
            btnTestDiscord.Click += btnTestDiscord_Click;

            // Row 4: Unique Keywords
            lblUniqueKeywords.AutoSize = true;
            lblUniqueKeywords.Location = new System.Drawing.Point(16, 140);
            lblUniqueKeywords.Text = "Unique Keywords:";

            txtUniqueKeywords.Location = new System.Drawing.Point(195, 136);
            txtUniqueKeywords.Size = new System.Drawing.Size(535, 24);

            // Row 5: Status & Quick Event Presets
            lblStatus.AutoSize = true;
            lblStatus.Location = new System.Drawing.Point(16, 178);
            lblStatus.Text = "Status: Ready";

            btnSelectCoreEvents.Location = new System.Drawing.Point(475, 218);
            btnSelectCoreEvents.Size = new System.Drawing.Size(120, 26);
            btnSelectCoreEvents.Radius = 6;
            btnSelectCoreEvents.Text = "Core Alerts Only";
            btnSelectCoreEvents.Click += btnSelectCoreEvents_Click;

            btnSelectAllEvents.Location = new System.Drawing.Point(605, 218);
            btnSelectAllEvents.Size = new System.Drawing.Size(125, 26);
            btnSelectAllEvents.Radius = 6;
            btnSelectAllEvents.Text = "Enable All Events";
            btnSelectAllEvents.Click += btnSelectAllEvents_Click;

            grpChannels.Controls.Add(btnSelectCoreEvents);
            grpChannels.Controls.Add(btnSelectAllEvents);

            // Group: Events
            grpEvents.Dock = System.Windows.Forms.DockStyle.Fill;
            grpEvents.Padding = new System.Windows.Forms.Padding(12, 24, 12, 8);
            grpEvents.Text = "Alert Events";

            tlpEvents.ColumnCount = 4;
            tlpEvents.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 25F));
            tlpEvents.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 25F));
            tlpEvents.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 25F));
            tlpEvents.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 25F));
            tlpEvents.RowCount = 5;
            for (var r = 0; r < 5; r++)
                tlpEvents.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 20F));
            tlpEvents.Dock = System.Windows.Forms.DockStyle.Fill;
            grpEvents.Controls.Add(tlpEvents);

            // Group: Recent Notifications History
            grpHistory.Dock = System.Windows.Forms.DockStyle.Fill;
            grpHistory.Padding = new System.Windows.Forms.Padding(12, 24, 12, 10);
            grpHistory.Text = "Recent Notifications Log";

            txtHistory.Dock = System.Windows.Forms.DockStyle.Fill;
            txtHistory.BorderStyle = System.Windows.Forms.BorderStyle.None;
            txtHistory.ReadOnly = true;
            txtHistory.Font = new System.Drawing.Font("Consolas", 9F);

            var historyToolbar = new System.Windows.Forms.Panel
            {
                Dock = System.Windows.Forms.DockStyle.Bottom,
                Height = 32
            };
            btnClearHistory.Dock = System.Windows.Forms.DockStyle.Right;
            btnClearHistory.Width = 110;
            btnClearHistory.Radius = 6;
            btnClearHistory.Text = "Clear Log";
            btnClearHistory.Click += btnClearHistory_Click;
            historyToolbar.Controls.Add(btnClearHistory);

            grpHistory.Controls.Add(txtHistory);
            grpHistory.Controls.Add(historyToolbar);

            Controls.Add(rootLayout);
            Name = "Main";
            Size = new System.Drawing.Size(780, 620);
            AutoScroll = true;

            grpHistory.ResumeLayout(false);
            grpEvents.ResumeLayout(false);
            grpChannels.ResumeLayout(false);
            grpChannels.PerformLayout();
            rootLayout.ResumeLayout(false);
            ResumeLayout(false);
        }

        private System.Windows.Forms.TableLayoutPanel rootLayout;
        private SDUI.Controls.GroupBox grpChannels;
        private SDUI.Controls.GroupBox grpEvents;
        private SDUI.Controls.GroupBox grpHistory;

        private SDUI.Controls.CheckBox chkMasterEnabled;
        private SDUI.Controls.CheckBox chkRichCards;
        private SDUI.Controls.Label lblCooldown;
        private SDUI.Controls.NumUpDown numCooldown;

        private SDUI.Controls.CheckBox chkTelegramEnabled;
        private SDUI.Controls.Label lblTelegramToken;
        private SDUI.Controls.TextBox txtTelegramToken;
        private SDUI.Controls.Label lblTelegramChatId;
        private SDUI.Controls.TextBox txtTelegramChatId;
        private SDUI.Controls.Button btnTestTelegram;

        private SDUI.Controls.CheckBox chkDiscordEnabled;
        private SDUI.Controls.Label lblDiscordWebhook;
        private SDUI.Controls.TextBox txtDiscordWebhook;
        private SDUI.Controls.Button btnTestDiscord;

        private SDUI.Controls.Label lblUniqueKeywords;
        private SDUI.Controls.TextBox txtUniqueKeywords;

        private SDUI.Controls.Button btnSave;
        private SDUI.Controls.Button btnSelectCoreEvents;
        private SDUI.Controls.Button btnSelectAllEvents;
        private SDUI.Controls.Label lblStatus;

        private System.Windows.Forms.TableLayoutPanel tlpEvents;
        private System.Windows.Forms.RichTextBox txtHistory;
        private SDUI.Controls.Button btnClearHistory;
    }
}
