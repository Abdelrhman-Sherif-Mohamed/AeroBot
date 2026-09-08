using Microsoft.Win32;
using RSBot.Core;
using RSBot.Core.Client;
using RSBot.Core.Components;
using RSBot.Core.Event;
using RSBot.Core.Plugins;
using RSBot.Views.Dialog;
using SDUI;
using SDUI.Controls;
using SDUI.Helpers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace RSBot.Views;

public partial class Main : UIWindow
{
    public static readonly Color LightThemeColor = Color.FromArgb(255, 255, 255);
    public static readonly Color DarkThemeColor = Color.FromArgb(12, 18, 34);

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
        VerticalTabs = true;
        CheckForIllegalCrossThreadCalls = false;
        SystemEvents.UserPreferenceChanged += SystemEvents_UserPreferenceChanged;
        RegisterEvents();
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

        var isDark = BackColor.IsDark();
        try
        {
            BorderColor = isDark ? Color.FromArgb(30, 41, 59) : Color.FromArgb(203, 213, 225);
        }
        catch
        {
        }

        stripStatus.BackColor = isDark ? Color.FromArgb(15, 23, 42) : Color.FromArgb(241, 245, 249);
        stripStatus.ForeColor = isDark ? Color.FromArgb(226, 232, 240) : Color.FromArgb(30, 41, 59);

        bottomPanel.BackColor = isDark ? Color.FromArgb(15, 23, 42) : Color.FromArgb(248, 250, 252);
        bottomPanel.BorderColor = isDark ? Color.FromArgb(30, 41, 59) : Color.FromArgb(226, 232, 240);

        topCharacter?.RefreshTheme();

        try
        {
            if (Kernel.Bot != null && Kernel.Bot.Running)
            {
                btnStartStop.Color = Color.FromArgb(239, 68, 68);
                btnStartStop.BackColor = Color.FromArgb(239, 68, 68);
            }
            else
            {
                btnStartStop.Color = Color.FromArgb(16, 185, 129);
                btnStartStop.BackColor = Color.FromArgb(16, 185, 129);
            }
            btnStartStop.ForeColor = Color.White;

            btnSave.Color = isDark ? Color.FromArgb(37, 99, 235) : Color.FromArgb(59, 130, 246);
            btnSave.BackColor = isDark ? Color.FromArgb(37, 99, 235) : Color.FromArgb(59, 130, 246);
            btnSave.ForeColor = Color.White;

            buttonConfig.Color = isDark ? Color.FromArgb(51, 65, 85) : Color.FromArgb(203, 213, 225);
            buttonConfig.ForeColor = isDark ? Color.FromArgb(241, 245, 249) : Color.FromArgb(30, 41, 59);
        }
        catch
        {
        }

        if (save)
            GlobalConfig.Save();
    }

    /// <summary>
    ///     Enables double buffering on a control and all of its children (reduces flicker).
    /// </summary>
    private static void EnableDoubleBuffer(Control control)
    {
        try
        {
            typeof(Control).GetProperty("DoubleBuffered",
                BindingFlags.Instance | BindingFlags.NonPublic)?.SetValue(control, true);

            foreach (Control child in control.Controls)
                EnableDoubleBuffer(child);
        }
        catch
        {
        }
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
        EventManager.SubscribeEvent("OnAgentServerDisconnected", OnAgentServerDisconnected);
        EventManager.SubscribeEvent("OnShowScriptRecorder", new Action<int, bool>(OnShowScriptRecorder));
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

        newBotbase.Value.Translate();

        var control = newBotbase.Value.View;
        control.Name = newBotbase.Value.Name;
        control.Text = LanguageManager.GetLangBySpecificKey(newBotbase.Value.Name, "TabText", newBotbase.Value.TabText);
        control.Enabled = true;
        windowPageControl.Controls.Add(control);
        windowPageControl.Controls.SetChildIndex(control, 1);
        EnableDoubleBuffer(control);

        Kernel.Bot?.SetBotbase(newBotbase.Value);
        GlobalConfig.Set("RSBot.BotName", newBotbase.Value.Name);

        if (Game.Player != null)
            EventManager.FireEvent("OnLoadCharacter");

        foreach (ToolStripMenuItem item in botsToolStripMenuItem.DropDown.Items)
            item.Checked = newBotbase.Value.Name == item.Name;

        if (!string.IsNullOrWhiteSpace(oldBotbaseName) && windowPageControl.Controls.ContainsKey(oldBotbaseName))
            windowPageControl.Controls.RemoveByKey(oldBotbaseName);
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
                Log.Error($"Failed to initialize plugin [{plugin.DisplayName}]: {ex.Message}");
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
                if (control == null) continue;

                control.Name = extension.Value.InternalName;
                control.Text = LanguageManager.GetLangBySpecificKey(extension.Value.InternalName, "DisplayName",
                    extension.Value.DisplayName);
                control.Enabled = true;
                control.Dock = DockStyle.Fill;

                windowPageControl.Controls.Add(control);
                EnableDoubleBuffer(control);
            }
            catch (Exception ex)
            {
                Log.Error($"Failed to load tab for plugin [{extension.Value.DisplayName}]: {ex.Message}");
            }
        }

        menuPlugins.DropDownItems.Clear();

        // Action 1: Install Plugin (.dll)...
        var itemInstall = new ToolStripMenuItem(LanguageManager.GetLang("InstallPlugin", "Install Plugin (.dll)..."));
        itemInstall.Click += InstallPlugin_Click;
        menuPlugins.DropDownItems.Add(itemInstall);

        // Action 2: Open Plugins Folder
        var itemOpenFolder = new ToolStripMenuItem(LanguageManager.GetLang("OpenPluginsFolder", "Open Plugins Folder"));
        itemOpenFolder.Click += OpenPluginsFolder_Click;
        menuPlugins.DropDownItems.Add(itemOpenFolder);

        menuPlugins.DropDownItems.Add(new ToolStripSeparator());

        // Action 3: Non-tab plugins (dialogs / standalone tools)
        var nonTabExtensions = extensions.Where(e => !e.Value.DisplayAsTab).ToList();
        if (nonTabExtensions.Count > 0)
        {
            foreach (var extension in nonTabExtensions)
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
        }

        // Action 4: Submenu showing all loaded plugins
        var loadedMenu = new ToolStripMenuItem(LanguageManager.GetLang("LoadedPlugins", $"Loaded Plugins ({extensions.Count})"));
        foreach (var ext in extensions)
        {
            var capturedExt = ext.Value;
            var pluginText = LanguageManager.GetLangBySpecificKey(capturedExt.InternalName, "DisplayName", capturedExt.DisplayName);
            var item = new ToolStripMenuItem(pluginText)
            {
                Tag = capturedExt
            };
            item.Click += (s, e) =>
            {
                if (capturedExt.DisplayAsTab)
                {
                    var tab = windowPageControl.Controls[capturedExt.InternalName];
                    if (tab != null)
                        windowPageControl.SelectedIndex = windowPageControl.Controls.IndexOf(tab);
                }
                else
                {
                    PluginMenuItem_Click(item, e);
                }
            };
            loadedMenu.DropDownItems.Add(item);
        }
        menuPlugins.DropDownItems.Add(loadedMenu);
    }

    private void InstallPlugin_Click(object sender, EventArgs e)
    {
        using var ofd = new OpenFileDialog
        {
            Title = LanguageManager.GetLang("SelectPluginTitle", "Select Plugin DLL to Install"),
            Filter = "Plugin Files (*.dll)|*.dll|All Files (*.*)|*.*",
            Multiselect = true
        };

        if (ofd.ShowDialog(this) != DialogResult.OK)
            return;

        var targetDir = Path.Combine(Kernel.BasePath, "Data", "Plugins");
        if (!Directory.Exists(targetDir))
            Directory.CreateDirectory(targetDir);

        var copied = new List<string>();
        foreach (var sourceFile in ofd.FileNames)
        {
            try
            {
                var destFile = Path.Combine(targetDir, Path.GetFileName(sourceFile));
                File.Copy(sourceFile, destFile, true);
                copied.Add(destFile);

                var pdbFile = Path.ChangeExtension(sourceFile, ".pdb");
                if (File.Exists(pdbFile))
                    File.Copy(pdbFile, Path.Combine(targetDir, Path.GetFileName(pdbFile)), true);

                var depsFile = Path.ChangeExtension(sourceFile, ".deps.json");
                if (File.Exists(depsFile))
                    File.Copy(depsFile, Path.Combine(targetDir, Path.GetFileName(depsFile)), true);
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, $"Failed to copy {Path.GetFileName(sourceFile)}: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        if (copied.Count > 0)
        {
            var newlyLoaded = new List<IPlugin>();
            foreach (var dest in copied)
            {
                var plugins = Kernel.PluginManager.LoadSingleAssembly(dest);
                foreach (var p in plugins)
                {
                    newlyLoaded.Add(p);
                    if (p.DisplayAsTab)
                    {
                        try
                        {
                            p.Translate();
                            var control = p.View;
                            if (control != null)
                            {
                                control.Name = p.InternalName;
                                control.Text = LanguageManager.GetLangBySpecificKey(p.InternalName, "DisplayName", p.DisplayName);
                                control.Enabled = true;
                                control.Dock = DockStyle.Fill;
                                windowPageControl.Controls.Add(control);
                                EnableDoubleBuffer(control);
                            }
                        }
                        catch (Exception ex)
                        {
                            Log.Error($"Error loading tab for new plugin {p.DisplayName}: {ex.Message}");
                        }
                    }
                }
            }

            var msg = newlyLoaded.Count > 0
                ? $"تم تثبيت وتحميل {newlyLoaded.Count} إضافة بنجاح!\nSuccessfully installed and loaded {newlyLoaded.Count} plugin(s)!"
                : $"تم نسخ الملفات إلى مجلد الإضافات بنجاح. يرجى إعادة تشغيل البوت لتفعيلها.\nFiles copied to Plugins directory. Please restart AeroBot to activate.";

            MessageBox.Show(this, msg, "AeroBot Plugins", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
    }

    private void OpenPluginsFolder_Click(object sender, EventArgs e)
    {
        var targetDir = Path.Combine(Kernel.BasePath, "Data", "Plugins");
        if (!Directory.Exists(targetDir))
            Directory.CreateDirectory(targetDir);

        Process.Start(new ProcessStartInfo
        {
            FileName = targetDir,
            UseShellExecute = true
        });
    }

    /// <summary>
    ///     Configures the main window size (sidebar removed, stats live in the Statistics tab).
    /// </summary>
    private void ConfigureSidebar()
    {
        var size = Size;
        size.Width = 1000;
        size.Height = 760;
        Size = size;
        Width = size.Width;
        Height = size.Height;
        MinimumSize = size;
        MaximumSize = size;
    }

    /// <summary>
    ///     Populates the server combobox.
    /// </summary>
    /// <param name="info">The information.</param>
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
        SonicBranding.ApplyWindowIcon(this);

        foreach (var item in LanguageManager.GetLanguages())
        {
            var dropdown = new ToolStripMenuItem(item.Value);
            dropdown.Click += LanguageDropdown_Click;
            dropdown.Tag = item.Key;
            languageToolStripMenuItem.DropDownItems.Add(dropdown);

            if (Kernel.Language == dropdown.Tag.ToString())
                dropdown.Checked = true;
        }

        ConfigureSidebar();
        BackColor = ColorScheme.BackColor;
        ApplyMainTranslations();

        EventManager.FireEvent("OnInitialized");

        Task.Run(async () =>
        {
            try
            {
                await Task.Delay(4000);
                var updater = new Updater();
                var hasUpdate = await updater.Check(manual: false);
                if (hasUpdate && !IsDisposed && IsHandleCreated)
                {
                    Invoke(new Action(() =>
                    {
                        if (!IsDisposed) updater.ShowDialog(this);
                    }));
                }
            }
            catch
            {
                // Silent on auto-check failure
            }
        });
    }

    /// <summary>
    ///     Applies localized strings to Main window controls (save button, start/stop button, profile, status).
    /// </summary>
    private void ApplyMainTranslations()
    {
        var saveText = LanguageManager.GetLang("btnSave");
        if (string.IsNullOrEmpty(saveText)) saveText = LanguageManager.GetLang("RSBot.Main.Panel.btnSave");
        if (!string.IsNullOrEmpty(saveText)) btnSave.Text = saveText;

        if (Kernel.Bot != null && Kernel.Bot.Running)
        {
            var stopText = LanguageManager.GetLang("StopBot");
            if (!string.IsNullOrEmpty(stopText)) btnStartStop.Text = stopText;
        }
        else
        {
            var startText = LanguageManager.GetLang("StartBot");
            if (!string.IsNullOrEmpty(startText)) btnStartStop.Text = startText;
        }

        var profilePrefix = LanguageManager.GetLang("ProfilePrefix");
        menuCurrentProfile.Text = (string.IsNullOrEmpty(profilePrefix) ? "Profile: " : profilePrefix) + ProfileManager.SelectedProfile;

        if (Game.Player == null)
        {
            var notInGameText = LanguageManager.GetLang("LabelPlayerName");
            if (!string.IsNullOrEmpty(notInGameText)) lblIngameStatus.Text = notInGameText;
        }

        topCharacter?.Translate();
    }

    /// <summary>
    ///     Handles the Click event of the MenuItem control.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
    /// <exception cref="System.NotImplementedException"></exception>
    private void LanguageDropdown_Click(object sender, EventArgs e)
    {
        var dropdown = sender as ToolStripMenuItem;
        if (dropdown.Checked)
            return;

        Kernel.Language = dropdown.Tag.ToString();

        foreach (ToolStripMenuItem item in languageToolStripMenuItem.DropDownItems)
            item.Checked = false;

        foreach (var plugin in Kernel.PluginManager.Extensions)
        {
            plugin.Value.Translate();

            var tabpage = windowPageControl.Controls[plugin.Key];
            if (tabpage == null)
                continue;

            tabpage.Text = LanguageManager.GetLangBySpecificKey(plugin.Key, "DisplayName", tabpage.Text);
        }

        foreach (var botbase in Kernel.BotbaseManager.Bots)
        {
            botbase.Value.Translate();

            if (!windowPageControl.Controls.ContainsKey(botbase.Key))
                continue;

            var tabpage = windowPageControl.Controls[botbase.Key];
            tabpage.Text = LanguageManager.GetLangBySpecificKey(botbase.Key, "DisplayName", tabpage.Text);
        }

        LanguageManager.Translate(this, Kernel.Language);
        ApplyMainTranslations();

        dropdown.Checked = true;

        GlobalConfig.Set("RSBot.Language", Kernel.Language);
        GlobalConfig.Save();
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
        notifyIcon.ShowBalloonTip(1000, "Sonic Bot", "Sonic Bot visible mode", ToolTipIcon.Info);*/

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
    ///     Sets the system tray icon (used by SonicBranding).
    /// </summary>
    internal void SetTrayIcon(System.Drawing.Icon icon)
    {
        try
        {
            notifyIcon.Icon = icon;
        }
        catch
        {
        }
    }

    /// <summary>
    ///     Handles the Resize event of the Main control.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
    private void Main_Resize(object sender, EventArgs e)
    {
        if (WindowState == FormWindowState.Normal)
            return;

        if (!GlobalConfig.Get<bool>("RSBot.General.TrayWhenMinimize"))
            return;

        notifyIcon.Visible = true;
        notifyIcon.ShowBalloonTip(1000);

        Hide();
    }

    /// <summary>
    ///     Handles the Click event of the menuCheckUpdates control.
    /// </summary>
    private async void menuCheckUpdates_Click(object sender, EventArgs e)
    {
        menuCheckUpdates.Enabled = false;
        try
        {
            using var updater = new Updater();
            var hasUpdate = await updater.Check(manual: true);
            if (hasUpdate)
            {
                updater.ShowDialog(this);
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show(this, $"خطأ في فحص التحديثات: {ex.Message}", "AeroBot Updater", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
        finally
        {
            menuCheckUpdates.Enabled = true;
        }
    }

    /// <summary>
    ///     Handles the Click event of the menuGitHub control.
    /// </summary>
    private void menuGitHub_Click(object sender, EventArgs e)
    {
        try
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = "https://github.com/Abdelrhman-Sherif-Mohamed/AeroBot",
                UseShellExecute = true
            });
        }
        catch (Exception ex)
        {
            Log.Notify($"Could not open GitHub: {ex.Message}");
        }
    }

    /// <summary>
    ///     Handles the Click event of the menuItemThis control.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
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
        btnStartStop.Text = LanguageManager.GetLang("StopBot");
        btnStartStop.Color = Color.FromArgb(239, 68, 68);
        btnStartStop.BackColor = Color.FromArgb(239, 68, 68);
        btnStartStop.ForeColor = Color.White;
    }

    /// <summary>
    ///     Called when [stop bot].
    /// </summary>
    private void OnStopBot()
    {
        btnStartStop.Text = LanguageManager.GetLang("StartBot");
        btnStartStop.Color = Color.FromArgb(16, 185, 129);
        btnStartStop.BackColor = Color.FromArgb(16, 185, 129);
        btnStartStop.ForeColor = Color.White;
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

        var defaultBot = GlobalConfig.Get("RSBot.BotName", "AeroBot.Default");
        if (defaultBot.StartsWith("SonicBot.") || defaultBot.StartsWith("RSBot."))
            defaultBot = defaultBot.Replace("SonicBot.", "AeroBot.").Replace("RSBot.", "AeroBot.");
        SelectBotbase(defaultBot);
    }

    /// <summary>
    ///     Handles the Click event of the MenuItem control.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
    /// <exception cref="System.NotImplementedException"></exception>
    private void Item_Click(object? sender, EventArgs e)
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
            Text = $@"AeroBot - {_playerName} - {disconnectedText}";
            notifyIcon.Text = Text;
        }
    }

    /// <summary>
    ///     Called when [change status text].
    /// </summary>
    /// <param name="text">The text.</param>
    private void OnChangeStatusText(string text)
    {
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
        Text = $@"AeroBot - {_playerName}";
        notifyIcon.Text = Text;

        if (Game.Clientless)
            Text += " [Clientless]";

        if (Kernel.Debug)
            Text += $@" [JID = {Game.Player.JID}]";
    }

    #endregion Core events
}
