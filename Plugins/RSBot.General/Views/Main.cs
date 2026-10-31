using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using RSBot.Core;
using RSBot.Core.Client;
using RSBot.Core.Components;
using RSBot.Core.Event;
using RSBot.General.Components;
using RSBot.General.Models;
using SDUI.Controls;

namespace RSBot.General.Views;

[ToolboxItem(false)]
internal partial class Main : DoubleBufferedControl
{
    private bool _clientVisible;

    /// <summary>
    ///     Initializes a new instance of the <see cref="Main" /> class.
    /// </summary>
    public Main()
    {
        CheckForIllegalCrossThreadCalls = false;

        InitializeComponent();
        SubscribeEvents();
        ApplyGeneralLayout();

        //btnStartClient.SetUseAsync(true);
        //btnStartClientless.SetUseAsync(true);
    }

    private bool _applyingGeneralLayout;

    protected override void OnResize(EventArgs e)
    {
        base.OnResize(e);
        ApplyGeneralLayout();
    }

    protected override void OnLoad(EventArgs e)
    {
        base.OnLoad(e);
        ApplyGeneralLayout();
    }

    private int S(float value)
    {
        return (int)Math.Round(value * DeviceDpi / 96f);
    }

    private void Place(Control control, int x, int y, int width, int height)
    {
        if (control == null)
            return;

        control.AutoSize = false;
        control.SetBounds(S(x), S(y), Math.Max(1, S(width)), Math.Max(1, S(height)));
    }

    private static void StyleInnerPanel(System.Windows.Forms.Panel panel)
    {
        if (panel == null)
            return;

        // Plain panels (unlike SDUI panels) have no rounded border or shadow, so
        // matching the card color makes them disappear into the card surface.
        panel.BackColor = SDUI.ColorScheme.CardColor;
    }

    /// <summary>
    ///     Rebuilds the General page layout for the current surface size. The
    ///     page uses a fixed two-column grid (cards keep their own padding) so
    ///     gaps stay identical on every DPI and on both sidebar states.
    /// </summary>
    private void ApplyGeneralLayout()
    {
        if (_applyingGeneralLayout || IsDisposed || groupBox1 == null)
            return;

        _applyingGeneralLayout = true;
        SuspendLayout();

        try
        {
            var scale = DeviceDpi / 96f;
            var width = Math.Max((int)(ClientSize.Width / scale), 780);
            var height = Math.Max((int)(ClientSize.Height / scale), 545);

            AutoScroll = true;
            AutoScrollMinSize = new Size(S(780), S(545));

            // Inner panels share the card surface; letting them paint their own
            // rounded background would create nested boxes inside the card.
            StyleInnerPanel(panel3);
            StyleInnerPanel(captchaPanel);
            StyleInnerPanel(autoLoginTopPanel);
            panel3.MinimumSize = Size.Empty;
            groupBox1.AutoSize = false;

            const int margin = 14;
            const int headerTop = 12;
            const int headerH = 30;
            const int columnGap = 14;

            var right = width - margin;

            // ---- Header toolbar: path, client type, version -----------------
            Place(label1, margin, headerTop, 165, headerH);
            label1.TextAlign = ContentAlignment.MiddleLeft;

            const int versionW = 58;
            var versionX = right - versionW;
            Place(lblVersion, versionX, headerTop, versionW, headerH);
            lblVersion.TextAlign = ContentAlignment.MiddleRight;

            const int clientComboW = 140;
            var clientComboX = versionX - 10 - clientComboW;
            Place(comboBoxClientType, clientComboX, headerTop + 2, clientComboW, headerH - 4);

            const int clientLabelW = 94;
            var clientLabelX = clientComboX - 6 - clientLabelW;
            Place(lblClientType, clientLabelX, headerTop, clientLabelW, headerH);
            lblClientType.TextAlign = ContentAlignment.MiddleLeft;

            const int browseW = 36;
            var browseX = clientLabelX - 12 - browseW;
            Place(btnBrowseSilkroadPath, browseX, headerTop + 1, browseW, headerH - 2);

            var pathX = margin + 165 + 8;
            var pathW = Math.Max(150, browseX - 8 - pathX);
            Place(txtSilkroadPath, pathX, headerTop, pathW, headerH);
            txtSilkroadPath.TextAlignment = HorizontalAlignment.Left;

            // ---- Two-column card grid ---------------------------------------
            const int columnTop = 54;
            var contentW = width - margin * 2;
            var leftW = (int)(contentW * 0.53f);
            var rightW = contentW - leftW - columnGap;
            var leftX = margin;
            var rightX = leftX + leftW + columnGap;
            var bottom = height - margin;

            // Left column: quick actions + automated login
            const int startCardH = 115;
            Place(groupBox2, leftX, columnTop, leftW, startCardH);

            var loginY = columnTop + startCardH + 12;
            var loginH = Math.Max(320, bottom - loginY);
            Place(groupBox1, leftX, loginY, leftW, loginH);

            const int btnPad = 16;
            const int btnGap = 10;
            var btnW = (leftW - btnPad * 2 - btnGap) / 2;
            const int btnH = 30;
            Place(btnStartClient, btnPad, 34, btnW, btnH);
            Place(btnStartClientless, btnPad + btnW + btnGap, 34, btnW, btnH);
            Place(btnClientHideShow, btnPad, 74, btnW, btnH);
            Place(btnGoClientless, btnPad + btnW + btnGap, 74, btnW, btnH);

            // Automated login card: accounts and character rows, then options.
            var panelW = leftW - 6;

            autoLoginTopPanel.Height = S(104);
            Place(label4, 14, 4, 60, 30);
            label4.TextAlign = ContentAlignment.MiddleLeft;
            Place(comboAccounts, 84, 4, panelW - 98, 30);
            Place(label7, 14, 38, 60, 30);
            label7.TextAlign = ContentAlignment.MiddleLeft;
            Place(comboCharacter, 84, 38, panelW - 98, 30);
            Place(checkEnableAutoLogin, 84, 71, 200, 26);
            Place(btnAutoLoginSettings, panelW - 14 - 84, 69, 84, 30);

            captchaPanel.Height = S(92);
            Place(label5, 14, 8, 64, 28);
            label5.TextAlign = ContentAlignment.MiddleLeft;
            Place(txtStaticCaptcha, 84, 6, panelW - 98, 30);
            txtStaticCaptcha.TextAlignment = HorizontalAlignment.Left;
            Place(checkEnableStaticCaptcha, 84, 40, panelW - 98, 26);
            Place(label6, 84, 66, panelW - 98, 22);
            label6.TextAlign = ContentAlignment.MiddleLeft;

            var half = (panelW - 16 - 12) / 2;
            const int rowH = 28;
            Place(checkEnableLoginDelay, 8, 4, half, rowH);
            Place(numLoginDelay, panelW - 8 - 76 - 6 - 72, 4, 72, rowH);
            Place(lblLoginDelaySeconds, panelW - 8 - 76, 4, 76, rowH);
            lblLoginDelaySeconds.TextAlign = ContentAlignment.MiddleLeft;

            Place(checkStartBot, 8, 34, half, rowH);
            Place(checkUseReturnScroll, 8 + half + 12, 34, half, rowH);
            Place(checkHideClient, 8, 64, half, rowH);
            Place(checkCharAutoSelect, 8 + half + 12, 64, half, rowH);
            Place(radioAutoSelectFirst, 8, 94, half, rowH);
            Place(radioAutoSelectHigher, 8 + half + 12, 94, half, rowH);

            // Right column: client, bot and pending-server settings
            const int infoCardH = 112;
            Place(groupBox3, rightX, columnTop, rightW, infoCardH);
            Place(groupBox4, rightX, columnTop + infoCardH + 12, rightW, infoCardH);

            var queueY = columnTop + infoCardH * 2 + 24;
            var queueH = Math.Max(190, bottom - queueY);
            Place(groupBox5, rightX, queueY, rightW, queueH);

            Place(checkStayConnected, 16, 30, rightW - 32, rowH);
            Place(label2, 18, 62, rightW - 36, 40);
            label2.TextAlign = ContentAlignment.MiddleLeft;

            Place(checkBoxBotTrayMinimized, 16, 30, rightW - 32, rowH);
            Place(label8, 18, 62, rightW - 36, 40);
            label8.TextAlign = ContentAlignment.MiddleLeft;

            const int queueRowH = 28;
            const int queueBtnH = 32;
            var queueGap = Math.Max(6, Math.Min(20, (queueH - 30 - 14 - (queueRowH * 4 + queueBtnH)) / 4));
            var queueStep = queueRowH + queueGap;
            var rowY = 30;

            Place(checkAutoHidePendingWindow, 16, rowY, rightW - 32, queueRowH);
            rowY += queueStep;
            Place(checkEnableQueueLogs, 16, rowY, rightW - 32, queueRowH);
            rowY += queueStep;
            Place(checkEnableQueueNotification, 16, rowY, rightW - 32, queueRowH);
            rowY += queueStep;

            const int queueLabelW = 100;
            const int queueNumW = 80;
            var queueLabelX = rightW - 16 - queueLabelW;
            Place(numQueueLeft, queueLabelX - 6 - queueNumW, rowY, queueNumW, queueRowH);
            Place(label3, queueLabelX, rowY, queueLabelW, queueRowH);
            label3.TextAlign = ContentAlignment.MiddleLeft;

            rowY += queueStep;
            Place(btnShowPending, 16, rowY, rightW - 32, queueBtnH);

            AutoScrollMinSize = new Size(S(width), S(bottom + margin));
        }
        finally
        {
            ResumeLayout(true);
            _applyingGeneralLayout = false;
        }
    }

    /// <summary>
    ///     Subscribes the events.
    /// </summary>
    private void SubscribeEvents()
    {
        EventManager.SubscribeEvent("OnLoadVersionInfo", new Action<VersionInfo>(OnLoadVersionInfo));
        EventManager.SubscribeEvent("OnAgentServerConnected", OnAgentServerConnected);
        EventManager.SubscribeEvent("OnAgentServerDisconnected", OnAgentServerDisconnected);
        EventManager.SubscribeEvent("OnGatewayServerDisconnected", OnGatewayServerDisconnected);
        EventManager.SubscribeEvent("OnClientConnected", OnClientConnected);
        EventManager.SubscribeEvent("OnEnterGame", OnEnterGame);
        EventManager.SubscribeEvent("OnStartClient", OnStartClient);
        EventManager.SubscribeEvent("OnExitClient", OnExitClient);
        EventManager.SubscribeEvent("OnCharacterListReceived", OnCharacterListReceived);
        EventManager.SubscribeEvent("OnInitialized", OnInitialized);
        EventManager.SubscribeEvent("OnProfileChanged", OnProfileChanged);
    }

    private void OnProfileChanged()
    {
        Accounts.Load();
        LoadAccounts();
    }

    /// <summary>
    ///     Called when gateway server disconnected.
    /// </summary>
    private void OnGatewayServerDisconnected()
    {
        AutoLogin.Pending = false;
        View.PendingWindow?.Hide();

        if (!Kernel.Proxy.IsConnectedToAgentserver)
        {
            Game.Clientless = false;

            btnStartClient.Enabled = true;
            btnStartClientless.Enabled = true;
            btnStartClientless.Text = LanguageManager.GetLang("StartClientless");
            Log.StatusLang("Ready");
            Kernel.Proxy.Shutdown();
        }
    }

    /// <summary>
    ///     Called when main window loaded.
    /// </summary>
    private void OnInitialized()
    {
        comboBoxClientType.Items.AddRange(Enum.GetNames(typeof(GameClientType)));
        comboCharacter.SelectedIndex = 0;

        Accounts.Load();
        LoadAccounts();

        //Load and display config

        txtSilkroadPath.Text = Path.Combine(GlobalConfig.Get<string>("RSBot.SilkroadDirectory"),
            GlobalConfig.Get<string>("RSBot.SilkroadExecutable"));
        checkEnableStaticCaptcha.Checked = GlobalConfig.Get<bool>("RSBot.General.EnableStaticCaptcha");
        checkEnableAutoLogin.Checked = GlobalConfig.Get<bool>("RSBot.General.EnableAutomatedLogin");
        checkStartBot.Checked = GlobalConfig.Get<bool>("RSBot.General.StartBot");
        checkUseReturnScroll.Checked = GlobalConfig.Get<bool>("RSBot.General.UseReturnScroll");
        checkStayConnected.Checked = GlobalConfig.Get<bool>("RSBot.General.StayConnected");
        checkBoxBotTrayMinimized.Checked = GlobalConfig.Get<bool>("RSBot.General.TrayWhenMinimize");
        txtStaticCaptcha.Text = GlobalConfig.Get<string>("RSBot.General.StaticCaptcha");
        checkEnableLoginDelay.Checked = GlobalConfig.Get<bool>("RSBot.General.EnableLoginDelay");
        numLoginDelay.Value = GlobalConfig.Get("RSBot.General.LoginDelay", 10);
        checkHideClient.Checked = GlobalConfig.Get<bool>("RSBot.General.HideOnStartClient");
        checkCharAutoSelect.Checked = GlobalConfig.Get<bool>("RSBot.General.CharacterAutoSelect");
        radioAutoSelectFirst.Checked = GlobalConfig.Get<bool>("RSBot.General.CharacterAutoSelectFirst");
        radioAutoSelectHigher.Checked = GlobalConfig.Get<bool>("RSBot.General.CharacterAutoSelectHigher");
        checkAutoHidePendingWindow.Checked = GlobalConfig.Get<bool>("RSBot.General.AutoHidePendingWindow");
        checkEnableQueueLogs.Checked = GlobalConfig.Get<bool>("RSBot.General.PendingEnableQueueLogs");
        checkEnableQueueNotification.Checked = GlobalConfig.Get<bool>("RSBot.General.EnableQueueNotification");
        numQueueLeft.Value = GlobalConfig.Get("RSBot.General.QueueLeft", 30);

        if (GlobalConfig.Get<bool>("RSBot.General.CharacterAutoSelect"))
        {
            radioAutoSelectFirst.Enabled = true;
            radioAutoSelectHigher.Enabled = true;
        }

        comboBoxClientType.SelectedIndex = (int)Game.ClientType;

        if (Game.IsValidClientDirectory(GlobalConfig.Get<string>("RSBot.SilkroadDirectory")))
            return;

        txtSilkroadPath.BackColor = Color.Red;
    }

    /// <summary>
    ///     Called when account character list updated
    /// </summary>
    private void OnCharacterListReceived()
    {
        LoadAccounts();
    }

    /// <summary>
    ///     Loads the accounts.
    /// </summary>
    private void LoadAccounts()
    {
        comboAccounts.Items.Clear();
        comboAccounts.Items.Add(LanguageManager.GetLang("NoSelected"));

        var autoLoginUserName = GlobalConfig.Get<string>("RSBot.General.AutoLoginAccountUsername");
        foreach (var account in Accounts.SavedAccounts)
        {
            var index = comboAccounts.Items.Add(account);
            if (account.Username == autoLoginUserName)
                comboAccounts.SelectedIndex = index;
        }

        if (comboAccounts.SelectedIndex == -1)
            comboAccounts.SelectedIndex = 0;
    }

    /// <summary>
    ///     Fill the combobox on the form
    /// </summary>
    private void LoadCharacters()
    {
        comboCharacter.Items.Clear();
        comboCharacter.Items.Add(LanguageManager.GetLang("NoSelected"));

        var selectedAccount = comboAccounts.SelectedItem as Account;
        if (selectedAccount?.Characters == null)
        {
            comboCharacter.SelectedIndex = 0;
            return;
        }

        foreach (var character in selectedAccount.Characters.Where(n => n != null))
        {
            var index = comboCharacter.Items.Add(character);
            if (character == selectedAccount.SelectedCharacter)
                comboCharacter.SelectedIndex = index;
        }

        if (comboCharacter.SelectedIndex == -1 ||
            string.IsNullOrWhiteSpace(selectedAccount.SelectedCharacter))
            comboCharacter.SelectedIndex = 0;
    }

    /// <summary>
    ///     Starts the client process.
    /// </summary>
    private async Task StartClientProcess()
    {
        btnStartClient.Enabled = false;
        Game.Start();

        await Task.Run(async () =>
        {
            var startedResult = await ClientManager.Start();
            if (!startedResult)
            {
                OnExitClient();
                Log.WarnLang("ClientStartingError");
            }
        });
    }

    /// <summary>
    ///     Called when [start client].
    /// </summary>
    private void OnStartClient()
    {
        btnStartClient.Enabled = false;
        btnStartClientless.Enabled = false;
        _clientVisible = true;
        btnClientHideShow.Enabled = true;

        if (GlobalConfig.Get<bool>("RSBot.General.HideOnStartClient"))
            ClientManager.SetVisible(false);
    }

    /// <summary>
    ///     Called when [exit client].
    /// </summary>
    private void OnExitClient()
    {
        Log.StatusLang("Ready");
        _clientVisible = false;
        btnStartClient.Text = LanguageManager.GetLang("StartClient");

        if (Game.Clientless)
            return;

        btnStartClient.Enabled = true;
        btnStartClientless.Enabled = true;
        btnClientHideShow.Enabled = false;

        if (!GlobalConfig.Get<bool>("RSBot.General.StayConnected"))
        {
            Kernel.Proxy.Shutdown();
        }
        else
        {
            if (!Kernel.Proxy.IsConnectedToAgentserver)
                return;

            btnStartClient.Enabled = false;

            ClientlessManager.GoClientless();

            btnGoClientless.Enabled = false;
            btnStartClientless.Text = LanguageManager.GetLang("Disconnect");

            Log.NotifyLang("ClientlessModeActivated");
        }
    }

    /// <summary>
    ///     Called when [enter game].
    /// </summary>
    private async void OnEnterGame()
    {
        if (!Game.Clientless)
        {
            btnClientHideShow.Enabled = true;
            btnClientHideShow.Text = LanguageManager.GetLang("HideClient");
            btnStartClient.Enabled = true;
            btnStartClient.Text = LanguageManager.GetLang("KillClient");
            btnGoClientless.Enabled = true;
        }

        //Wait for the game to be ready!
        while (!Game.Ready)
            await Task.Delay(100);

        var startBot = GlobalConfig.Get<bool>("RSBot.General.StartBot");
        var useReturnScroll = GlobalConfig.Get<bool>("RSBot.General.UseReturnScroll");

        if (useReturnScroll)
            Game.Player.UseReturnScroll();

        if (startBot)
            Kernel.Bot.Start();
    }

    /// <summary>
    ///     The on load version information.
    /// </summary>
    /// <param name="info">The information.</param>
    private void OnLoadVersionInfo(VersionInfo info)
    {
        lblVersion.Text = "v" + ((1000f + info.Version) / 1000f).ToString("0.000", CultureInfo.InvariantCulture);
    }

    /// <summary>
    ///     Called when [agent server connected].
    /// </summary>
    private void OnAgentServerConnected()
    {
        //if (!Game.Clientless)
        //    btnGoClientless.Enabled = true;
    }

    /// <summary>
    ///     Called when [agent server disconnected].
    /// </summary>
    private async void OnAgentServerDisconnected()
    {
        Kernel.Bot.Stop();

        // Skiped: Cuz managing from ClientlessManager
        if (Game.Clientless)
            return;

        // If user disconnected with manual from clientless, we dont need open the client automatically again.
        //if (!Kernel.Proxy.ClientConnected)
        //return;

        ClientManager.Kill();

        if (GlobalConfig.Get<bool>("RSBot.General.EnableAutomatedLogin"))
        {
            btnStartClient.Enabled = false;
            btnStartClientless.Enabled = false;

            Thread.Sleep(2000);

            await StartClientProcess().ConfigureAwait(false);
            return;
        }

        btnGoClientless.Enabled = false;
        btnStartClient.Enabled = true;
        btnStartClientless.Enabled = true;
    }

    /// <summary>
    ///     Called when [client connected].
    /// </summary>
    private void OnClientConnected()
    {
        btnStartClientless.Enabled = false;
    }

    /// <summary>
    ///     Handles the Click event of the btnBrowseSilkroadPath control.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
    private void btnBrowseSilkroadPath_Click(object sender, EventArgs e)
    {
        using (var dialog = new OpenFileDialog())
        {
            var title = LanguageManager.GetLang("BrowseSilkroadPathDialogTitle");

            var msgBoxTitle = LanguageManager.GetLang("BrowseSilkroadPathMsgBoxTitle");
            var msgBoxContent = LanguageManager.GetLang("BrowseSilkroadPathMsgBoxContent");

            dialog.Title = title;
            dialog.Filter = "App (*.exe)|*.exe";
            dialog.FileName = "sro_client.exe";

            var result = dialog.ShowDialog();
            if (result != DialogResult.OK)
                return;

            txtSilkroadPath.Text = dialog.FileName;
            GlobalConfig.Set("RSBot.SilkroadDirectory", Path.GetDirectoryName(dialog.FileName));
            GlobalConfig.Set("RSBot.SilkroadExecutable", Path.GetFileName(dialog.FileName));

            result = MessageBox.Show(msgBoxContent, msgBoxTitle, MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (result == DialogResult.No)
                return;

            GlobalConfig.Save();
        }

        Process.Start(Application.ExecutablePath);
        Application.Exit();
    }

    /// <summary>
    ///     Handles the CheckedChanged event of the checkStartBot control.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
    private void checkAutoStartBot_CheckedChanged(object sender, EventArgs e)
    {
        GlobalConfig.Set("RSBot.General.StartBot", checkStartBot.Checked);
    }

    /// <summary>
    ///     Handles the CheckedChanged event of the checkUseReturnScroll control.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
    private void checkUseReturnScroll_CheckedChanged(object sender, EventArgs e)
    {
        GlobalConfig.Set("RSBot.General.UseReturnScroll", checkUseReturnScroll.Checked);
    }

    /// <summary>
    ///     Handles the Click event of the btnAutoLoginSettings control.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
    private void btnAutoLoginSettings_Click(object sender, EventArgs e)
    {
        if (View.AccountsWindow.ShowDialog() == DialogResult.OK)
            LoadAccounts();
    }

    /// <summary>
    ///     Handles the CheckedChanged event of the checkEnableAutoLogin control.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
    private void checkEnableAutoLogin_CheckedChanged(object sender, EventArgs e)
    {
        GlobalConfig.Set("RSBot.General.EnableAutomatedLogin", checkEnableAutoLogin.Checked.ToString());
    }

    /// <summary>
    ///     Handles the SelectedIndexChanged event of the comboAccounts control.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
    private void comboAccounts_SelectedIndexChanged(object sender, EventArgs e)
    {
        var selectedAccount = comboAccounts.SelectedIndex == 0 ? string.Empty : comboAccounts.SelectedItem.ToString();

        GlobalConfig.Set("RSBot.General.AutoLoginAccountUsername", selectedAccount);

        LoadCharacters();
    }

    /// <summary>
    ///     Handles the TextChanged event of the txtStaticCaptcha control.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
    private void txtStaticCaptcha_TextChanged(object sender, EventArgs e)
    {
        GlobalConfig.Set("RSBot.General.StaticCaptcha", txtStaticCaptcha.Text);
    }

    /// <summary>
    ///     Handles the CheckedChanged event of the checkEnableStaticCaptcha control.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
    private void checkEnableStaticCaptcha_CheckedChanged(object sender, EventArgs e)
    {
        GlobalConfig.Set("RSBot.General.EnableStaticCaptcha", checkEnableStaticCaptcha.Checked.ToString());
    }

    /// <summary>
    ///     Handles the SelectedIndexChanged event of the comboCharacter control.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
    private void comboCharacter_SelectedIndexChanged(object sender, EventArgs e)
    {
        if (comboAccounts.SelectedIndex == 0)
            return;

        checkCharAutoSelect.Enabled = comboCharacter.SelectedIndex == 0;

        var selectedAccount = comboAccounts.SelectedItem as Account;
        if (selectedAccount == null)
            return;

        selectedAccount.SelectedCharacter =
            comboCharacter.SelectedIndex == 0 ? string.Empty : comboCharacter.SelectedItem.ToString();

        Accounts.Save();
    }

    /// <summary>
    ///     Handles the Click event of the btnGoClientless control.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
    private void btnGoClientless_Click(object sender, EventArgs e)
    {
        if (Game.Clientless)
            return;

        var msgBoxTitle = LanguageManager.GetLang("GoClientlessMsgBoxTitle");
        var msgBoxContent = LanguageManager.GetLang("GoClientlessMsgBoxContent");

        if (MessageBox.Show(msgBoxContent, msgBoxTitle, MessageBoxButtons.YesNo, MessageBoxIcon.Warning) !=
            DialogResult.Yes) return;

        ClientlessManager.GoClientless();
        ClientManager.Kill();

        btnStartClientless.Text = LanguageManager.GetLang("Disconnect");
        btnGoClientless.Enabled = false;
        btnStartClientless.Enabled = true;
        btnStartClient.Enabled = false;
        btnClientHideShow.Enabled = false;
    }

    /// <summary>
    ///     Handles the Click event of the btnStartClientless control.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
    private async void btnStartClientless_Click(object sender, EventArgs e)
    {
        await Task.Run(() =>
        {
            if (!Game.Clientless)
            {
                if (!checkEnableAutoLogin.Checked || comboAccounts.SelectedIndex <= 0)
                {
                    var msgBoxTitle = LanguageManager.GetLang("StartClientlessMsgBoxTitle");
                    var msgBoxContent = LanguageManager.GetLang("StartClientlessMsgBoxContent");

                    MessageBox.Show(msgBoxContent, msgBoxTitle, MessageBoxButtons.OK, MessageBoxIcon.Warning);

                    return;
                }

                btnStartClient.Enabled = false;
                btnClientHideShow.Enabled = false;

                Game.Clientless = true;
                Log.StatusLang("StartingClientless");
                btnStartClientless.Text = LanguageManager.GetLang("Disconnect");
                Game.Start();
            }
            else
            {
                var msgBoxTitle = LanguageManager.GetLang("MsgBoxDisconnectDialogTitle");
                var msgBoxContent = LanguageManager.GetLang("MsgBoxDisconnectDialogContent");

                var result = MessageBox.Show(msgBoxContent, msgBoxTitle, MessageBoxButtons.YesNo,
                    MessageBoxIcon.Warning);
                if (result == DialogResult.No)
                    return;

                Game.Clientless = false;

                btnStartClient.Enabled = true;
                btnStartClientless.Enabled = true;
                btnStartClientless.Text = LanguageManager.GetLang("StartClientless");

                Kernel.Proxy.Shutdown();
            }
        });
    }

    /// <summary>
    ///     Handles the Click event of the btnStartClient control.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
    private void btnStartClient_Click(object sender, EventArgs e)
    {
        if (!Game.Clientless && Kernel.Proxy != null && Kernel.Proxy.IsConnectedToAgentserver)
        {
            var extraStr = LanguageManager.GetLang("KillClientWarnMsgBoxSplit1");
            if (!GlobalConfig.Get<bool>("RSBot.General.StayConnected"))
                extraStr = LanguageManager.GetLang("KillClientWarnMsgBoxSplit2");

            var title = LanguageManager.GetLang("Warning");
            var content = LanguageManager.GetLang("KillClientWarnMsgBoxContent", extraStr);

            if (MessageBox.Show(content, title, MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
                ClientManager.Kill();

            return;
        }

        StartClientProcess();
    }

    /// <summary>
    ///     Handles the CheckedChanged event of the checkStayConnected control.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
    private void checkStayConnected_CheckedChanged(object sender, EventArgs e)
    {
        GlobalConfig.Set("RSBot.General.StayConnected", checkStayConnected.Checked);
    }

    /// <summary>
    ///     Handles the CheckedChanged event of the checkBoxBotTrayMinimized control
    /// </summary>
    private void checkBoxBotTrayMinimized_CheckedChanged(object sender, EventArgs e)
    {
        GlobalConfig.Set("RSBot.General.TrayWhenMinimize", checkBoxBotTrayMinimized.Checked);
    }

    private void btnClientHideShow_Click(object sender, EventArgs e)
    {
        if (!ClientManager.IsRunning)
            return;

        if (!_clientVisible)
        {
            _clientVisible = true;
            ClientManager.SetVisible(true);
            btnClientHideShow.Text = LanguageManager.GetLang("HideClient");
        }
        else
        {
            _clientVisible = false;
            ClientManager.SetVisible(false);
            btnClientHideShow.Text = LanguageManager.GetLang("ShowClient");
        }
    }

    /// <summary>
    ///     Handles the SelectedIndexChanged event of the comboBoxClientType control.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
    private void comboBoxClientType_SelectedIndexChanged(object sender, EventArgs e)
    {
        // Created from Activator.CreateInstance easy fix ^^
        if (comboBoxClientType.Parent.Parent == null)
            return;

        if (Game.Player != null)
        {
            MessageBox.Show(LanguageManager.GetLang("MsgBoxClientTypeWarn"));
            return;
        }

        var clientType = (GameClientType)comboBoxClientType.SelectedIndex;

        GlobalConfig.Set("RSBot.Game.ClientType", clientType);
        Game.ClientType = clientType;
        GlobalConfig.Save();

        if (clientType.ToString().StartsWith("Vietnam"))
            captchaPanel.Visible = true;
        else
            captchaPanel.Visible = false;

        ApplyGeneralLayout();
    }

    /// <summary>
    ///     Handles the CheckedChanged event of the checkEnableLoginDelay control.
    /// </summary>
    /// <param name="sender">
    ///     The source of the event.
    /// </param>
    /// <param name="e">
    ///     The <see cref="EventArgs" /> instance containing the event data.
    /// </param>
    private void checkEnableLoginDelay_CheckedChanged(object sender, EventArgs e)
    {
        GlobalConfig.Set("RSBot.General.EnableLoginDelay", checkEnableLoginDelay.Checked);
    }

    /// <summary>
    ///     Handles the ValueChanged event of the numLoginDelay control.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
    private void numLoginDelay_ValueChanged(object sender, EventArgs e)
    {
        GlobalConfig.Set("RSBot.General.LoginDelay", numLoginDelay.Value);
    }

    /// <summary>
    ///     Handles the CheckedChanged event of the checkHideClient control.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void checkHideClient_CheckedChanged(object sender, EventArgs e)
    {
        GlobalConfig.Set("RSBot.General.HideOnStartClient", checkHideClient.Checked);
    }

    /// <summary>
    ///     Handles the CheckedChanged event of the checkCharAutoSelect control.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void checkCharAutoSelect_CheckedChanged(object sender, EventArgs e)
    {
        if (!GlobalConfig.Get<bool>("RSBot.General.CharacterAutoSelect"))
        {
            radioAutoSelectFirst.Enabled = true;
            radioAutoSelectHigher.Enabled = true;
        }
        else
        {
            radioAutoSelectFirst.Enabled = false;
            radioAutoSelectHigher.Enabled = false;
        }


        GlobalConfig.Set("RSBot.General.CharacterAutoSelect", checkCharAutoSelect.Checked);
    }

    /// <summary>
    ///     Handles the CheckedChanged event of the radioAutoSelectFirst control.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void radioAutoSelectFirst_CheckedChanged(object sender, EventArgs e)
    {
        GlobalConfig.Set("RSBot.General.CharacterAutoSelectFirst", radioAutoSelectFirst.Checked);
    }

    /// <summary>
    ///     Handles the CheckedChanged event of the radioAutoSelectHigher control.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void radioAutoSelectHigher_CheckedChanged(object sender, EventArgs e)
    {
        GlobalConfig.Set("RSBot.General.CharacterAutoSelectHigher", radioAutoSelectHigher.Checked);
    }

    /// <summary>
    ///     Handles the CheckedChanged event of the checkDontShowPendingOnStartClient control.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void checkDontShowPendingOnStartClient_CheckedChanged(object sender, EventArgs e)
    {
        GlobalConfig.Set("RSBot.General.AutoHidePendingWindow", checkAutoHidePendingWindow.Checked);
    }

    /// <summary>
    ///     Handles the CheckedChanged event of the checkEnableQuequeLogs control.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void checkEnableQueueLogs_CheckedChanged(object sender, EventArgs e)
    {
        GlobalConfig.Set("RSBot.General.PendingEnableQueueLogs", checkEnableQueueLogs.Checked);
    }

    /// <summary>
    ///     Handles the Click event of the btnShowPending control.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void btnShowPending_Click(object sender, EventArgs e)
    {
        if (!AutoLogin.Pending)
            return;

        if (View.PendingWindow?.Visible == false)
            View.PendingWindow.ShowAtTop(View.Instance);
        else
            View.PendingWindow.Hide();
    }

    /// <summary>
    ///     Handles the CheckedChanged event of the checkEnableQueueNotification control.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void checkEnableQueueNotification_CheckedChanged(object sender, EventArgs e)
    {
        GlobalConfig.Set("RSBot.General.EnableQueueNotification", checkEnableQueueNotification.Checked);
    }

    /// <summary>
    ///     Handles the ValueChanged event of the numQuequeLeft control.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void numQueueLeft_ValueChanged(object sender, EventArgs e)
    {
        GlobalConfig.Set("RSBot.General.QueueLeft", numQueueLeft.Value);
    }
}