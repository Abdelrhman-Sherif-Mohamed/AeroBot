using Microsoft.Win32;
using RSBot.Core;
using RSBot.Core.Client;
using RSBot.Core.Components;
using RSBot.Core.Event;
using RSBot.Core.Plugins;
using RSBot.Core.Security;
using RSBot.Views.Controls;
using RSBot.Views.Dialog;
using SDUI;
using SDUI.Controls;
using SDUI.Helpers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Windows.Forms;

namespace RSBot.Views;

public partial class Main : UIWindow
{
    public static readonly Color LightThemeColor = Color.FromArgb(255, 255, 255);
    public static readonly Color DarkThemeColor = Color.FromArgb(16, 16, 16);

    #region Members

    /// <summary>
    ///     Bot player name [_cached]
    /// </summary>
    private string _playerName;
    private readonly Dictionary<string, UIWindow> _pluginWindows = new(8);




    #endregion Members

    #region Constructor

    /// <summary>
    ///     Initializes a new instance of the <see cref="Main" /> class.
    /// </summary>
    public Main()
    {
        InitializeComponent();
        CheckForIllegalCrossThreadCalls = false;
        SystemEvents.UserPreferenceChanged += SystemEvents_UserPreferenceChanged;
        RegisterEvents();

        Text = "Aerobot - Ultimate Edition";

        // Responsive window sizing: fits every desktop, never larger than the working area.
        Resizable = true;
        MaximizeBox = true;
        StartPosition = FormStartPosition.CenterScreen;
        TitleHeight = 32;
        VerticalTabs = true;
        WindowPageControl = windowPageControl;
        ApplyDefaultWindowSize();

        try
        {
            var iconPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "app.ico");
            if (System.IO.File.Exists(iconPath))
            {
                using var stream = new System.IO.FileStream(iconPath, System.IO.FileMode.Open, System.IO.FileAccess.Read);
                var botIcon = new Icon(stream);
                Icon = botIcon;
                notifyIcon.Icon = botIcon;
            }
        }
        catch { }

        ApplyCurrentLanguage();
    }

    /// <summary>
    ///     Picks a window size that fits the current monitor and keeps the layout usable.
    /// </summary>
    private void ApplyDefaultWindowSize()
    {
        var area = Screen.PrimaryScreen?.WorkingArea ?? new Rectangle(0, 0, 1366, 768);

        int width = Math.Min(area.Width - 16, Math.Max(920, Math.Min(1180, (int)(area.Width * 0.72))));
        int height = Math.Min(area.Height - 16, Math.Max(640, Math.Min(780, (int)(area.Height * 0.82))));

        Size = new Size(width, height);
        MinimumSize = new Size(Math.Min(920, width), Math.Min(640, height));
        MaximumSize = Size.Empty;
    }

    /// <summary>
    ///     Preferred order of the sidebar pages. The current botbase takes the third slot.
    /// </summary>
    private static readonly string[] NavigationOrder =
    {
        "RSBot.General",
        "RSBot.Items",
        "RSBot.Skills",
        "RSBot.Protection",
        "RSBot.Party",
        "RSBot.Inventory",
        "RSBot.Map",
        "Wolves.Fortress",
        "Wolves.Control",
        "Wolves.Stall",
        "Wolves.AvatarTester",
        "Wolves.Players",
        "RSBot.Statistics",
        "RSBot.Chat",
        "RSBot.Log"
    };

    /// <summary>
    ///     Applies a fixed, deterministic sidebar order so pages never jump around.
    /// </summary>
    private void ApplyNavigationOrder()
    {
        if (windowPageControl == null)
            return;

        var pages = new List<Control>(windowPageControl.Count);
        for (int i = 0; i < windowPageControl.Count; i++)
            pages.Add(windowPageControl.GetPage(i));

        windowPageControl.SetPageOrder(pages.OrderBy(GetNavigationRank).ToList());
    }

    private static int GetNavigationRank(Control control)
    {
        if (control == null)
            return int.MaxValue;

        // The active botbase page always sits right after "General" and "Items".
        if (Kernel.BotbaseManager?.Bots != null && Kernel.BotbaseManager.Bots.ContainsKey(control.Name))
            return 2;

        var name = control.Name ?? string.Empty;
        var index = Array.IndexOf(NavigationOrder, name);
        if (index < 0)
        {
            // Suffix match for AeroBot <-> RSBot <-> Wolves
            var suffix = name.Contains('.') ? name.Substring(name.IndexOf('.') + 1) : name;
            for (var i = 0; i < NavigationOrder.Length; i++)
            {
                var navSuffix = NavigationOrder[i].Contains('.') ? NavigationOrder[i].Substring(NavigationOrder[i].IndexOf('.') + 1) : NavigationOrder[i];
                if (string.Equals(suffix, navSuffix, StringComparison.OrdinalIgnoreCase))
                {
                    index = i;
                    break;
                }
            }
        }

        if (index < 0)
            return 100;

        return index < 2 ? index : index + 1;
    }

    /// <summary>
    ///     Applies the vBot-like light chrome to the main window.
    /// </summary>
    private void ApplyVisualStyle()
    {
        BackColor = ColorScheme.BackColor;
        TitleHeight = 32;
        VerticalTabs = true;

        if (stripStatus != null)
        {
            stripStatus.BackColor = ColorScheme.ChromeColor;
            stripStatus.ForeColor = ColorScheme.SecondaryForeColor;
            stripStatus.Padding = new Padding(12, 0, 14, 0);
        }

        if (lblIngameStatus != null)
            lblIngameStatus.ForeColor = ColorScheme.SecondaryForeColor;

        if (menuCurrentProfile != null)
            menuCurrentProfile.ForeColor = ColorScheme.SecondaryForeColor;

        if (bottomPanel != null)
        {
            bottomPanel.BackColor = ColorScheme.ChromeColor;
            bottomPanel.Border = new Padding(0, 1, 0, 0);
            bottomPanel.BorderColor = ColorScheme.BorderColor;
            bottomPanel.Padding = new Padding(0);
        }

        if (pnlBottomActions != null)
        {
            pnlBottomActions.BackColor = Color.Transparent;
            pnlBottomActions.Border = new Padding(0);
            pnlBottomActions.BorderColor = ColorScheme.BorderColor;
        }

        if (lstLog != null && lstLog.Parent != null)
        {
            lstLog.BackColor = ColorScheme.BackColor.IsDark()
                ? Color.FromArgb(15, 23, 42)
                : Color.FromArgb(244, 245, 247);
            lstLog.ForeColor = ColorScheme.ForeColor;
            lstLog.BorderStyle = BorderStyle.FixedSingle;
            lstLog.Font = new Font("Segoe UI", 8.5F, FontStyle.Regular, GraphicsUnit.Point);

            if (lstLog.SmallImageList == null)
            {
                var spacer = new Bitmap(1, 20);
                lstLog.SmallImageList = new ImageList { ImageSize = new Size(1, 20) };
                lstLog.SmallImageList.Images.Add(spacer);
            }
        }

        StyleActionButtons();
        AdjustLogColumns();
    }

    /// <summary>
    ///     Restores the vBot action button styling after the theme engine runs.
    /// </summary>
    private void StyleActionButtons()
    {
        bool ar = Kernel.Language == "ar_AR";

        btnStartBot.Text = ar ? "شغل البوت" : "Start Bot";
        btnStopBot.Text = ar ? "وقف البوت" : "Stop Bot";
        UpdateShowClientButton(ClientManager.IsVisible);

        var neutralBack = ColorScheme.BackColor.IsDark() ? ColorScheme.CardColor2 : Color.White;
        var neutralFore = ColorScheme.BackColor.IsDark() ? ColorScheme.ForeColor : Color.FromArgb(55, 65, 81);

        StyleActionButton(btnStartBot, Color.FromArgb(34, 160, 107), Color.White);
        StyleActionButton(btnStopBot, neutralBack, neutralFore);
        StyleActionButton(btnShowClient, neutralBack, neutralFore);
        if (btnClearLog != null && btnClearLog.Parent != null)
            StyleActionButton(btnClearLog, neutralBack, neutralFore);
    }

    private static void StyleActionButton(SDUI.Controls.Button button, Color backColor, Color foreColor)
    {
        if (button == null)
            return;

        button.Color = backColor;
        button.BackColor = Color.Transparent;
        button.ForeColor = foreColor;
        button.Radius = 4;
        button.ShadowDepth = 0;
        button.Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point);
        button.Invalidate();
    }

    /// <summary>
    ///     Keeps the log columns proportional when the window is resized.
    /// </summary>
    private void AdjustLogColumns()
    {
        if (lstLog == null || lstLog.IsDisposed || lstLog.Parent == null)
            return;

        int width = Math.Max(220, lstLog.ClientSize.Width - 4);
        colTime.Width = 78;
        colType.Width = 82;
        colMsg.Width = Math.Max(80, width - colTime.Width - colType.Width);
    }

    private void ApplyCurrentLanguage()
    {
        ApplyVisualStyle();

        bool ar = Kernel.Language == "ar_AR";

        menuCurrentProfile.Text = ar
            ? $"الملف: {ProfileManager.SelectedProfile}"
            : $"Profile: {ProfileManager.SelectedProfile}";

        if (Kernel.Proxy == null || !Kernel.Proxy.IsConnectedToAgentserver)
            lblIngameStatus.Text = ar ? "غير متصل" : "Not in game";
        else if (Kernel.Bot != null && Kernel.Bot.Running)
            lblIngameStatus.Text = ar ? "يعمل" : "Running";
    }

    #endregion Constructor

    #region Events

    public static event UserPreferenceChangingEventHandler UserPreferenceChanging;

    #endregion

    #region Methods

    private void donateButton_Click(object sender, EventArgs e)
    {
        Process.Start(new ProcessStartInfo { FileName = "https://buymeacoffee.com/sdclowen", UseShellExecute = true });
        Process.Start(
            new ProcessStartInfo { FileName = "https://github.com/sponsors/SDClowen", UseShellExecute = true });
        Process.Start(new ProcessStartInfo { FileName = "https://www.patreon.com/sdclowen", UseShellExecute = true });
    }

    /// <summary>
    ///     Called when user preference changing
    /// </summary>
    /// <param name="sender">The sender</param>
    /// <param name="e">The event args</param>
    private void SystemEvents_UserPreferenceChanged(object sender, UserPreferenceChangedEventArgs e)
    {
        if (BackColor.IsDark() == WindowsHelper.IsDark())
            return;

        var detectDarkLight = GlobalConfig.Get("RSBot.Theme.Auto", true);
        if (!detectDarkLight)
            return;

        if (WindowsHelper.IsDark())
            SetThemeColor(DarkThemeColor);
        else
            SetThemeColor(LightThemeColor);
    }

    /// <summary>
    ///     Set theme color
    /// </summary>
    /// <param name="color">The color</param>
    private void SetThemeColor(Color color)
    {
        GlobalConfig.Set("SDUI.Color", color.ToArgb());
        ColorScheme.BackColor = color;
        RefreshTheme();
    }

    /// <summary>
    ///     Refreshes the theme.
    /// </summary>
    public void RefreshTheme(bool save = true)
    {
        BackColor = ColorScheme.BackColor;

        ChangeControlsTheme(this);
        ApplyVisualStyle();

        Invalidate(true);

        // Notify plugins and WPF controls of theme change
        EventManager.FireEvent("OnChangeTheme", BackColor.IsDark());

        if (save)
            GlobalConfig.Save();
    }

    /// <summary>
    ///     Registers the events.
    /// </summary>
    private void RegisterEvents()
    {
        EventManager.SubscribeEvent("OnChangeStatusText", new Action<string>(OnChangeStatusText));
        EventManager.SubscribeEvent("OnShowBotWindow", OnShowBotWindow);
        EventManager.SubscribeEvent("OnLoadPlugins", OnLoadPlugins);
        EventManager.SubscribeEvent("OnLoadDivisionInfo", new Action<DivisionInfo>(OnLoadDivisionInfo));
        EventManager.SubscribeEvent("OnLoadBotbases", OnLoadBotbases);
        EventManager.SubscribeEvent("OnLoadCharacter", OnLoadCharacter);
        EventManager.SubscribeEvent("OnStartBot", OnStartBot);
        EventManager.SubscribeEvent("OnStopBot", OnStopBot);
        EventManager.SubscribeEvent("OnAddLog", OnAddLog);
        EventManager.SubscribeEvent("OnAgentServerDisconnected", OnAgentServerDisconnected);
        EventManager.SubscribeEvent("OnShowScriptRecorder", new Action<int, bool>(OnShowScriptRecorder));
        EventManager.SubscribeEvent("OnAddSidebarElement", new Action<Control>(OnAddSidebarElement));
        EventManager.SubscribeEvent("OnStartClient", OnClientStarted);
        EventManager.SubscribeEvent("OnExitClient", OnClientExited);
    }

    private void OnClientStarted()
    {
        if (InvokeRequired)
            BeginInvoke(new Action(OnClientStarted));
        else
            UpdateShowClientButton(true);
    }

    private void OnClientExited()
    {
        if (InvokeRequired)
            BeginInvoke(new Action(OnClientExited));
        else
            UpdateShowClientButton(false);
    }

    private void OnAddSidebarElement(Control obj)
    {
        pSidebarCustom.Controls.Add(obj);
    }

    private void OnShowScriptRecorder(int ownerId, bool startRecording)
    {
        var recorder = new ScriptRecorder(ownerId, startRecording);
        recorder.Show();
    }

    /// <summary>
    ///     Forces to show the bot window
    /// </summary>
    private void OnShowBotWindow()
    {
        if (WindowState == FormWindowState.Minimized)
            WindowState = FormWindowState.Normal;

        TopMost = true;

        BringToFront();
        Activate();

        TopMost = false;
    }

    /// <summary>
    ///     Selects the botbase.
    /// </summary>
    /// <param name="index">The index.</param>
    private void SelectBotbase(string name)
    {
        if (Kernel.Bot.Running)
            return;

        var oldBotbaseName = Kernel.Bot?.Botbase?.Name;
        var newBotbase = Kernel.BotbaseManager.Bots.FirstOrDefault(bot => bot.Value.Name == name);
        if (newBotbase.Value == null)
        {
            Log.Error($"Botbase [{name}] could not be found!");

            return;
        }

        try
        {
            newBotbase.Value.Translate();
        }
        catch (Exception ex)
        {
            Log.Error($"[SelectBotbase] Error translating botbase [{name}]: {ex.Message}");
        }

        var control = newBotbase.Value.View;
        control.Name = newBotbase.Value.Name;
        control.Text = LanguageManager.GetLangBySpecificKey(newBotbase.Value.Name, "TabText", newBotbase.Value.TabText);
        control.Enabled = true;

        var oldBotbaseControl = !string.IsNullOrWhiteSpace(oldBotbaseName)
            ? windowPageControl.Controls[oldBotbaseName]
            : null;

        windowPageControl.Controls.Add(control);
        windowPageControl.MovePageTo(control, 2);
        ChangeControlsTheme(control);


        Kernel.Bot?.SetBotbase(newBotbase.Value);
        GlobalConfig.Set("RSBot.BotName", newBotbase.Value.Name);

        if (Game.Player != null)
            EventManager.FireEvent("OnLoadCharacter");

        foreach (ToolStripMenuItem item in botsToolStripMenuItem.DropDown.Items)
            item.Checked = newBotbase.Value.Name == item.Name;

        if (oldBotbaseControl != null && !ReferenceEquals(oldBotbaseControl, control))
            windowPageControl.Controls.Remove(oldBotbaseControl);
    }

    /// <summary>
    ///     Loads the extensions.
    /// </summary>
    private void LoadExtensions()
    {
        foreach (var plugin in Kernel.PluginManager.Extensions.Values)
        {
            try
            {
                plugin.Initialize();
            }
            catch (Exception ex)
            {
                Log.Error($"[LoadExtensions] Error initializing plugin [{plugin.DisplayName}]: {ex.Message}");
            }
        }

        var extensions =
            Kernel.PluginManager.Extensions.OrderBy(entry => entry.Value.Index)
                .ToDictionary(x => x.Key, x => x.Value);

        foreach (var extension in extensions.Where(extension => extension.Value.DisplayAsTab))
        {
            try
            {
                extension.Value.Translate();

                var control = extension.Value.View;
                control.Name = extension.Value.InternalName;
                control.Text = LanguageManager.GetLangBySpecificKey(extension.Value.InternalName, "DisplayName",
                    extension.Value.DisplayName);
                control.Enabled = true;
                control.Dock = DockStyle.Fill;

                windowPageControl.Controls.Add(control);
                ChangeControlsTheme(control);
            }
            catch (Exception ex)
            {
                Log.Error($"[LoadExtensions] Error adding tab [{extension.Key}]: {ex.Message}");
            }
        }

        foreach (var extension in extensions.Where(extension => !extension.Value.DisplayAsTab))
        {
            try
            {
                var menuItemText = LanguageManager.GetLangBySpecificKey(extension.Value.InternalName, "DisplayName",
                    extension.Value.DisplayName);
                var menuItem = new ToolStripMenuItem(menuItemText)
                {
                    Enabled = true
                };
                menuItem.Click += PluginMenuItem_Click;
                menuItem.Tag = extension.Value;

                menuPlugins.DropDownItems.Add(menuItem);
            }
            catch (Exception ex)
            {
                Log.Error($"[LoadExtensions] Error adding menu item [{extension.Key}]: {ex.Message}");
            }
        }
    }

        /// <summary>
    ///     Shows or hides the character sidebar.
    /// </summary>
    private void ConfigureSidebar()
    {
        pSidebar.Visible = menuSidebar.Checked;
    }

    private void PopulateServerCombobox(DivisionInfo info)
    {
        comboServer.Items.Clear();
        foreach (var item in info.Divisions[comboDivision.SelectedIndex].GatewayServers)
            comboServer.Items.Add(item);

        var gatewayIndex = GlobalConfig.Get<int>("RSBot.GatewayIndex");

        if (comboServer.Items.Count > 0)
            comboServer.SelectedIndex = comboServer.Items.Count - 1 >= gatewayIndex ? gatewayIndex : 0;

        GlobalConfig.Set("RSBot.GatewayIndex", comboServer.SelectedIndex.ToString());
    }

    #endregion Methods

    #region Form events

    /// <summary>
    ///     Handles the Click event of the MenuItem control.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
    /// <exception cref="System.NotImplementedException"></exception>
    private void PluginMenuItem_Click(object sender, EventArgs e)
    {
        var menuItem = (ToolStripMenuItem)sender;
        var plugin = (IPlugin)menuItem.Tag;

        if (!_pluginWindows.TryGetValue(plugin.InternalName, out var pluginWindow) || pluginWindow.IsDisposed)
        {
            pluginWindow = new UIWindow
            {
                Text = plugin.DisplayName,
                Name = plugin.InternalName,
                MaximizeBox = false,
                FormBorderStyle = FormBorderStyle.FixedDialog,
                Icon = Icon,
                StartPosition = FormStartPosition.CenterParent,
                ShowTitle = true
            };

            var content = plugin.View;
            content.Dock = DockStyle.Fill;

            plugin.Translate();

            pluginWindow.Size = new Size(content.Size.Width + 16, content.Size.Height + 32);
            pluginWindow.Controls.Add(content);

            _pluginWindows[plugin.InternalName] = pluginWindow;
        }

        if (!pluginWindow.Visible)
            pluginWindow.Show();

    }

    /// <summary>
    ///     Handles the Click event of the menuScriptRecorder control.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
    private void menuScriptRecorder_Click(object sender, EventArgs e)
    {
        var scriptRecorder = new ScriptRecorder();
        scriptRecorder.Show();
    }

    private void btnSave_Click(object sender, EventArgs e)
    {
        GlobalConfig.Save();
        PlayerConfig.Save();
    }

    /// <summary>
    ///     Handles the Click event of the menuSidebar control.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
    private void menuSidebar_Click(object sender, EventArgs e)
    {
        menuSidebar.Checked = !menuSidebar.Checked;
        GlobalConfig.Set("RSBot.ShowSidebar", menuSidebar.Checked.ToString());
        ConfigureSidebar();
    }

    /// <summary>
    ///     Handles the SelectedIndexChanged event of the comboDivision control.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
    private void comboDivision_SelectedIndexChanged(object sender, EventArgs e)
    {
        GlobalConfig.Set("RSBot.DivisionIndex", comboDivision.SelectedIndex.ToString());

        if (Game.ReferenceManager.DivisionInfo != null)
            PopulateServerCombobox(Game.ReferenceManager.DivisionInfo);
    }

    /// <summary>
    ///     Handles the SelectedIndexChanged event of the comboServer control.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
    private void comboServer_SelectedIndexChanged(object sender, EventArgs e)
    {
        GlobalConfig.Set("RSBot.GatewayIndex", comboServer.SelectedIndex.ToString());
    }

    /// <summary>
    ///     Handles the Load event of the Main window.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
    private void Main_Load(object sender, EventArgs e)
    {
        menuSidebar.Checked = GlobalConfig.Get("RSBot.ShowSidebar", false);

        foreach (var item in LanguageManager.GetLanguages())
        {
            var dropdown = new ToolStripMenuItem(item.Value);
            dropdown.Click += LanguageDropdown_Click;
            dropdown.Tag = item.Key;
            languageToolStripMenuItem.DropDownItems.Add(dropdown);

            if (Kernel.Language == dropdown.Tag?.ToString())
                dropdown.Checked = true;
        }

        ConfigureSidebar();
        VerticalTabs = true;
        WindowPageControl = windowPageControl;

        ApplyNavigationOrder();
        ApplyCurrentLanguage();
        ApplyVisualStyle();

        EventManager.FireEvent("OnInitialized");
    }

    /// <summary>
    private void LanguageDropdown_Click(object sender, EventArgs e)
    {
        var dropdown = sender as ToolStripMenuItem;
        if (dropdown.Checked)
            return;

        LanguageManager.ClearCache();
        Kernel.Language = dropdown.Tag.ToString();

        foreach (ToolStripMenuItem item in languageToolStripMenuItem.DropDownItems)
            item.Checked = false;

        foreach (var plugin in Kernel.PluginManager.Extensions)
        {
            try
            {
                plugin.Value.Translate();

                var tabpage = windowPageControl.Controls[plugin.Key];
                if (tabpage == null)
                    continue;

                tabpage.Text = LanguageManager.GetLangBySpecificKey(plugin.Key, "DisplayName", plugin.Value.DisplayName);
            }
            catch (Exception ex)
            {
                Log.Error($"[LanguageDropdown_Click] Error translating plugin [{plugin.Key}]: {ex.Message}");
            }
        }

        foreach (var botbase in Kernel.BotbaseManager.Bots)
        {
            try
            {
                botbase.Value.Translate();

                if (!windowPageControl.Controls.ContainsKey(botbase.Key))
                    continue;

                var tabpage = windowPageControl.Controls[botbase.Key];
                tabpage.Text = LanguageManager.GetLangBySpecificKey(botbase.Key, "TabText", botbase.Value.TabText);
            }
            catch (Exception ex)
            {
                Log.Error($"[LanguageDropdown_Click] Error translating botbase [{botbase.Key}]: {ex.Message}");
            }
        }

        LanguageManager.Translate(this, Kernel.Language);
        
        ApplyCurrentLanguage();

        dropdown.Checked = true;

        GlobalConfig.Set("RSBot.Language", Kernel.Language);
        GlobalConfig.Save();

        Invalidate(true);
        windowPageControl.Invalidate(true);
    }

    /// <summary>
    ///     Handles the Click event of the btnStartStop control.
    /// </summary>
    private void btnStartStop_Click(object sender, EventArgs e)
    {
        if (Kernel.Proxy == null)
            return;

        if (!Kernel.Proxy.IsConnectedToAgentserver)
            return;

        if (Kernel.Bot == null)
        {
            Log.NotifyLang("NotifyPleaseSelectProperBotBase");
            return;
        }

        if (Game.Player == null)
        {
            Log.WarnLang("NotifyPlayerWasNull");
            return;
        }

        if (!Kernel.Bot.Running)
        {
            Kernel.Bot.Start();

            Log.StatusLang("Running");
        }
        else
        {
            Log.NotifyLang("StopingBot", Kernel.Bot.Botbase.DisplayName);

            Kernel.Bot.Stop();
            Log.StatusLang("Ready");
        }
    }

    /// <summary>
    ///     Handles the FormClosing event of the Main control.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="FormClosingEventArgs" /> instance containing the event data.</param>
    private void Main_FormClosing(object sender, FormClosingEventArgs e)
    {
        if (Kernel.Proxy == null || !Kernel.Proxy.ClientConnected || !GlobalConfig.Get("RSBot.showExitDialog", true))
        {
            GlobalConfig.Save();
            PlayerConfig.Save();

            Environment.Exit(0);
        }

        var exitDialog = new ExitDialog();
        if (exitDialog.ShowDialog(this) != DialogResult.Yes)
        {
            e.Cancel = true;
            return;
        }

        GlobalConfig.Save();
        PlayerConfig.Save();
        ClientManager.Kill();

        Environment.Exit(0);
    }

    /// <summary>
    ///     Handles the Click event of the notifyIcon control.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
    private void notifyIcon_Click(object sender, EventArgs e)
    {
        if (WindowState == FormWindowState.Normal)
            return;

        /*notifyIcon.Visible = true;
        notifyIcon.ShowBalloonTip(1000, "RSBot", "RSBot visible mode", ToolTipIcon.Info);*/

        Show();
        WindowState = FormWindowState.Normal;
    }

    /// <summary>
    ///     Handles the Click event of the menuItemExit control.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
    private void menuItemExit_Click(object sender, EventArgs e)
    {
        if (Kernel.Proxy == null || !Kernel.Proxy.ClientConnected || !GlobalConfig.Get("RSBot.showExitDialog", true))
        {
            GlobalConfig.Save();
            PlayerConfig.Save();

            Environment.Exit(0);
        }

        var exitDialog = new ExitDialog();
        if (exitDialog.ShowDialog(this) != DialogResult.Yes)
            return;

        GlobalConfig.Save();
        PlayerConfig.Save();
        ClientManager.Kill();

        Environment.Exit(0);
    }

    /// <summary>
    ///     Handles the Resize event of the Main control.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
    private void Main_Resize(object sender, EventArgs e)
    {
        if (WindowState == FormWindowState.Minimized)
        {
            if (!GlobalConfig.Get<bool>("RSBot.General.TrayWhenMinimize"))
                return;

            notifyIcon.Visible = true;
            notifyIcon.ShowBalloonTip(1000);

            Hide();
            return;
        }

        AdjustLogColumns();
    }

    protected override void OnShown(EventArgs e)
    {
        base.OnShown(e);
    }

    private void menuItemThis_Click(object sender, EventArgs e)
    {
        new AboutDialog().ShowDialog();
    }

    /// <summary>
    ///     Handles the Click event of the networkConfigToolStripMenuItem control.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
    private void networkConfigToolStripMenuItem_Click(object sender, EventArgs e)
    {
        using var configDialog = new ConfigDialog();
        configDialog.ShowDialog();
    }

    /// <summary>
    ///     Handles the Click event of the darkToolStripMenuItem control.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
    private void darkToolStripMenuItem_Click(object sender, EventArgs e)
    {
        GlobalConfig.Set("RSBot.Theme.Auto", false);
        SetThemeColor(DarkThemeColor);
    }

    /// <summary>
    ///     Handles the Click event of the lightToolStripMenuItem control.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
    private void lightToolStripMenuItem_Click(object sender, EventArgs e)
    {
        GlobalConfig.Set("RSBot.Theme.Auto", false);
        SetThemeColor(LightThemeColor);
    }

    /// <summary>
    ///     Handles the Click event of the autoToolStripMenuItem control.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
    private void autoToolStripMenuItem_Click(object sender, EventArgs e)
    {
        if (WindowsHelper.IsModern)
        {
            GlobalConfig.Set("RSBot.Theme.Auto", true);
            SystemEvents_UserPreferenceChanged(null,
                new UserPreferenceChangedEventArgs(UserPreferenceCategory.Color));

            return;
        }

        MessageBox.Show(
            "Unfortunately, it does not support this mode because your operating system is outdated!",
            "Warning",
            MessageBoxButtons.OK,
            MessageBoxIcon.Warning
        );
    }

    /// <summary>
    ///     Handles the Click event of the coloredToolStripMenuItem control.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
    private void coloredToolStripMenuItem_Click(object sender, EventArgs e)
    {
        var colorDialog = new ColorDialog
        {
            CustomColors = GlobalConfig.GetArray<int>("SDUI.CustomColors")
        };

        if (colorDialog.ShowDialog() == DialogResult.OK)
        {
            GlobalConfig.SetArray("SDUI.CustomColors", colorDialog.CustomColors);
            SetThemeColor(colorDialog.Color);
        }
    }

    /// <summary>
    ///     Handles the Click event of the menuSelectProfile control.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
    private void menuSelectProfile_Click(object sender, EventArgs e)
    {
        var dialog = new ProfileSelectionDialog();
        dialog.StartPosition = FormStartPosition.CenterParent;
        dialog.ShowInTaskbar = false;
        if (dialog.ShowDialog() != DialogResult.OK)
            return;

        if (dialog.SelectedProfile == ProfileManager.SelectedProfile)
            return;

        var oldSroPath = GlobalConfig.Get("RSBot.SilkroadDirectory", "");

        //We need this to check if the sro directories are different
        var tempNewConfig = new Config(ProfileManager.GetProfileFile(dialog.SelectedProfile));

        if (oldSroPath != tempNewConfig.Get("RSBot.SilkroadDirectory", ""))
            if (MessageBox.Show("This profile references to a different client, do you want to restart the bot?",
                    "Restart bot?", MessageBoxButtons.OKCancel, MessageBoxIcon.Warning) == DialogResult.OK)
                Application.Restart();

        ProfileManager.SetSelectedProfile(dialog.SelectedProfile);
        GlobalConfig.Load();

        EventManager.FireEvent("OnProfileChanged");
        menuCurrentProfile.Text = dialog.SelectedProfile;

        if (Game.Player == null)
            return;

        //Reload player config
        PlayerConfig.Load(Game.Player.Name);

        //A little hack to tell all plugins to reload their UI
        EventManager.FireEvent("OnLoadCharacter");
    }

    /// <summary>
    ///     Handles the Click event of the buttonConfig control.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
    private void buttonConfig_Click(object sender, EventArgs e)
    {
        const string title = "IP Bind";

        var currentBind = GlobalConfig.Get("RSBot.Network.BindIp", "0.0.0.0");

        const string message =
            "Use your custom interface ip for connect to game.\nEnter your interface Ip:\t(default: 0.0.0.0)";

        var dialog = new InputDialog(title, title, message, defaultValue: currentBind);
        if (dialog.ShowDialog() != DialogResult.OK)
            return;

        if (!IPAddress.TryParse(dialog.Value.ToString(), out var ipAddress))
        {
            const string errorMessage = "The IP address is incorrect or cannot be verified.You can try like '0.0.0.0'.";
            MessageBox.Show(errorMessage);

            return;
        }

        GlobalConfig.Set("RSBot.Network.BindIp", ipAddress.ToString());
    }

    #endregion Form events

    #region Core events

    /// <summary>
    ///     Called when [start bot].
    /// </summary>
    private void OnStartBot()
    {
        if (InvokeRequired)
        {
            BeginInvoke(new Action(OnStartBot));
            return;
        }

        var lang = LanguageManager.GetLang("StopBot");
        btnStartStop.Text = !string.IsNullOrWhiteSpace(lang) ? lang : "STOP BOT";
        btnStartStop.Color = System.Drawing.Color.FromArgb(220, 53, 69);
        btnStartStop.BackColor = System.Drawing.Color.FromArgb(220, 53, 69);

        if (btnStartBot != null) btnStartBot.Enabled = false;
        if (btnStopBot != null) btnStopBot.Enabled = true;
    }

    /// <summary>
    ///     Called when [stop bot].
    /// </summary>
    private void OnStopBot()
    {
        if (InvokeRequired)
        {
            BeginInvoke(new Action(OnStopBot));
            return;
        }

        var lang = LanguageManager.GetLang("StartBot");
        btnStartStop.Text = !string.IsNullOrWhiteSpace(lang) ? lang : "START BOT";
        btnStartStop.Color = System.Drawing.Color.FromArgb(33, 150, 243);
        btnStartStop.BackColor = System.Drawing.Color.FromArgb(33, 150, 243);

        if (btnStartBot != null) btnStartBot.Enabled = true;
        if (btnStopBot != null) btnStopBot.Enabled = false;
    }

    /// <summary>
    ///     Called when [load botbases].
    /// </summary>
    private void OnLoadBotbases()
    {
        if (Kernel.BotbaseManager.Bots == null || Kernel.BotbaseManager.Bots.Count == 0)
        {
            var title = LanguageManager.GetLang("NoBotbaseDetected");
            var message = LanguageManager.GetLang("NoBotbaseDetectedDesc");
            var messageResult =
                MessageBox.Show(message, title, MessageBoxButtons.AbortRetryIgnore, MessageBoxIcon.Error);

            if (messageResult == DialogResult.Retry)
                Kernel.BotbaseManager.LoadAssemblies();
            else if (messageResult == DialogResult.Abort)
                Environment.Exit(-1);
        }

        foreach (var bot in Kernel.BotbaseManager.Bots)
        {
            var item = new ToolStripMenuItem
            {
                Name = bot.Value.Name,
                Text = bot.Value.DisplayName
            };
            item.Click += Item_Click;
            botsToolStripMenuItem.DropDown.Items.Add(item);
        }

        SelectBotbase(GlobalConfig.Get("RSBot.BotName", "RSBot.Default"));
    }

    /// <summary>
    ///     Handles the Click event of the MenuItem control.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
    /// <exception cref="System.NotImplementedException"></exception>
    private void Item_Click(object sender, EventArgs e)
    {
        var item = sender as ToolStripMenuItem;
        SelectBotbase(item.Name);
    }

    /// <summary>
    ///     Reset UI after character disconnect
    /// </summary>
    private void OnAgentServerDisconnected()
    {
        foreach (Control control in windowPageControl.Controls)
        {
            if (!control.Controls.ContainsKey("overlay"))
                continue;

            control.Enabled = false;
            control.Controls["overlay"].Show();

        }

        var disconnectedText = LanguageManager.GetLang("Disconnected");
        if (!Text.EndsWith(disconnectedText))
        {
            Text = $@"Aerobot - {_playerName} - {disconnectedText}";
            notifyIcon.Text = Text;
        }
    }

    /// <summary>
    ///     Called when [change status text].
    /// </summary>
    /// <param name="text">The text.</param>
    private void OnChangeStatusText(string text)
    {
        if (InvokeRequired)
        {
            BeginInvoke(new Action(() => lblIngameStatus.Text = text));
            return;
        }

        lblIngameStatus.Text = text;
    }

    /// <summary>
    ///     Called when [load plugins].
    /// </summary>
    private void OnLoadPlugins()
    {
        LoadExtensions();
    }

    /// <summary>
    ///     Called when [load division information].
    /// </summary>
    /// <param name="info">The information.</param>
    private void OnLoadDivisionInfo(DivisionInfo info)
    {
        comboDivision.Items.Clear();
        foreach (var divInfo in info.Divisions)
            comboDivision.Items.Add(divInfo.Name);

        var divisionIndex = GlobalConfig.Get<int>("RSBot.DivisionIndex");

        if (comboDivision.Items.Count >= info.Divisions.Count)
            comboDivision.SelectedIndex = comboDivision.SelectedIndex =
                comboDivision.Items.Count - 1 >= divisionIndex ? divisionIndex : 0;

        PopulateServerCombobox(info);
    }

    /// <summary>
    ///     Called when [load character].
    /// </summary>
    private void OnLoadCharacter()
    {
        foreach (Control control in windowPageControl.Controls)
        {
            control.Enabled = true;

            control.Controls["overlay"]?.Hide();
        }

        foreach (ToolStripItem item in menuPlugins.DropDownItems)
            item.Enabled = true;

        _playerName = Game.Player.Name;
        Text = $@"Aerobot - {_playerName}";
        notifyIcon.Text = Text;

        if (Game.Clientless)
            Text += " [Clientless]";

        if (Kernel.Debug)
            Text += $@" [JID = {Game.Player.JID}]";
    }

    #endregion Core events
    #region Bottom Panel Controls and Logging

    private void btnStartBot_Click(object sender, EventArgs e)
    {
        StartBot();
    }

    private void btnStopBot_Click(object sender, EventArgs e)
    {
        StopBot();
    }

    private void btnShowClient_Click(object sender, EventArgs e)
    {
        if (!ClientManager.IsRunning)
            return;

        bool isVisible = ClientManager.ToggleVisible();
        UpdateShowClientButton(isVisible);
    }

    private void UpdateShowClientButton(bool isVisible)
    {
        if (btnShowClient == null || btnShowClient.IsDisposed)
            return;

        bool ar = Kernel.Language == "ar_AR";
        btnShowClient.Text = isVisible
            ? (ar ? "إخفاء الكلاينت" : "Hide Client")
            : (ar ? "إظهار الكلاينت" : "Show Client");
    }

    private void btnClearLog_Click(object sender, EventArgs e)
    {
        lstLog?.Items.Clear();
    }

    private void StartBot()
    {
        if (Kernel.Proxy == null || !Kernel.Proxy.IsConnectedToAgentserver)
            return;

        if (Kernel.Bot == null)
        {
            Log.NotifyLang("NotifyPleaseSelectProperBotBase");
            return;
        }

        if (Game.Player == null)
        {
            Log.WarnLang("NotifyPlayerWasNull");
            return;
        }

        if (!Kernel.Bot.Running)
        {
            Kernel.Bot.Start();
            Log.StatusLang("Running");
        }
    }

    private void StopBot()
    {
        if (Kernel.Bot != null && Kernel.Bot.Running)
        {
            Log.NotifyLang("StopingBot", Kernel.Bot.Botbase.DisplayName);
            Kernel.Bot.Stop();
            Log.StatusLang("Ready");
        }
    }

    private void OnAddLog(object[] args)
    {
        if (args == null || args.Length == 0 || IsDisposed || !IsHandleCreated)
            return;

        var message = args[0]?.ToString() ?? string.Empty;
        var level = args.Length > 1 && args[1] is LogLevel l ? l : LogLevel.Notify;

        try
        {
            BeginInvoke((MethodInvoker)delegate
            {
                if (lstLog == null || lstLog.IsDisposed)
                    return;

                var timeStr = DateTime.Now.ToString("[HH:mm:ss]");
                var levelStr = $"<{level}>";
                var item = new ListViewItem(timeStr);
                item.SubItems.Add(levelStr);
                item.SubItems.Add(message);

                if (level == LogLevel.Error)
                    item.ForeColor = Color.FromArgb(239, 68, 68);
                else if (level == LogLevel.Warning)
                    item.ForeColor = Color.FromArgb(245, 158, 11);
                else if (level == LogLevel.Notify)
                    item.ForeColor = BackColor.IsDark() ? Color.FromArgb(52, 211, 153) : Color.FromArgb(16, 149, 90);

                lstLog.Items.Add(item);

                if (lstLog.Items.Count > 1000)
                    lstLog.Items.RemoveAt(0);

                lstLog.EnsureVisible(lstLog.Items.Count - 1);
            });
        }
        catch { }
    }

    #endregion

}
