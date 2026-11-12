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
    private bool _sidebarVisible;
    private ExtensionsHubControl _extensionsHub;
    private AccountPageControl _accountPage;
    private System.Windows.Forms.Timer _accountRefreshTimer;
    private ToolStripMenuItem _gearAccountItem;




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

        ExtendBox = true;
        MainMenuStrip = menuStrip;
        if (menuStrip != null)
        {
            menuStrip.MouseLeave += (s, e) => ClearMenuSelection();
            if (menuAccountSummary != null)
                menuAccountSummary.Click += (s, e) => OpenAccountPage();
        }

        InitializeGearMenu();
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
    ///     Preferred order of the sidebar pages.
    /// </summary>
    private static readonly string[] NavigationOrder =
    {
        "RSBot.Account",
        "RSBot.Statistics",
        "RSBot.General",
        "RSBot.Protection",
        "RSBot.Party",
        "RSBot.Skills",
        "RSBot.Mastery",
        "RSBot.Inventory",
        "RSBot.Items",
        "RSBot.Alchemy",
        "RSBot.Default",
        "RSBot.Trade",
        "RSBot.Map",
        "Wolves.Stall",
        "RSBot.QuestLog",
        "RSBot.ExtensionsHub",
        "Wolves.Players",
        "RSBot.Chat",
        "RSBot.CommandCenter",
        "RSBot.Log"
    };

    /// <summary>
    ///     Secondary tools grouped inside the unified "Extensions" ("الإضافات") tab.
    /// </summary>
    private static readonly string[] ExtensionsHubToolKeys =
    {
        "RSBot.Lure",
        "Wolves.Fortress",
        "Wolves.Control",
        "Wolves.Notify",
        "Wolves.AvatarTester",
        "SonicBot.MobSelector",
        "RSBot.ServerInfo"
    };

    /// <summary>
    ///     Plugins excluded from the sidebar navigation rail.
    /// </summary>
    private static readonly HashSet<string> HiddenSidebarPlugins = new(StringComparer.OrdinalIgnoreCase)
    {
        "SonicBot.TargetSupport",
        "SonicBot.Trivia",
        "AeroBot.TargetSupport",
        "AeroBot.Trivia"
    };

    private static int GetExtensionsHubOrder(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            return -1;

        var exact = Array.IndexOf(ExtensionsHubToolKeys, name);
        if (exact >= 0)
            return exact;

        var suffix = name.Contains('.') ? name.Substring(name.IndexOf('.') + 1) : name;
        for (var i = 0; i < ExtensionsHubToolKeys.Length; i++)
        {
            var toolSuffix = ExtensionsHubToolKeys[i].Contains('.')
                ? ExtensionsHubToolKeys[i].Substring(ExtensionsHubToolKeys[i].IndexOf('.') + 1)
                : ExtensionsHubToolKeys[i];

            if (string.Equals(suffix, toolSuffix, StringComparison.OrdinalIgnoreCase))
                return i;
        }

        return -1;
    }

    private static string GetExtensionsHubTitle()
    {
        var fallback = Kernel.Language == "ar_AR" ? "الإضافات" : "Extensions";
        return LanguageManager.GetLangBySpecificKey("RSBot.ExtensionsHub", "DisplayName", fallback);
    }

    private ExtensionsHubControl EnsureExtensionsHub()
    {
        if (_extensionsHub != null && !_extensionsHub.IsDisposed)
            return _extensionsHub;

        _extensionsHub = new ExtensionsHubControl
        {
            Name = "RSBot.ExtensionsHub",
            Text = GetExtensionsHubTitle(),
            Enabled = true,
            Dock = DockStyle.Fill
        };

        if (windowPageControl != null && !windowPageControl.Controls.ContainsKey(_extensionsHub.Name))
        {
            windowPageControl.Controls.Add(_extensionsHub);
            ChangeControlsTheme(_extensionsHub);
        }

        return _extensionsHub;
    }

    private static string GetAccountTabTitle()
    {
        var fallback = Kernel.Language == "ar_AR" ? "الحساب" : "Account";
        return LanguageManager.GetLangBySpecificKey("RSBot.Account", "DisplayName", fallback);
    }

    private AccountPageControl EnsureAccountPage()
    {
        if (_accountPage != null && !_accountPage.IsDisposed)
            return _accountPage;

        _accountPage = new AccountPageControl
        {
            Name = "RSBot.Account",
            Text = GetAccountTabTitle(),
            Enabled = true,
            Dock = DockStyle.Fill
        };
        _accountPage.AccountRefreshed += (_, _) => UpdateAccountMenuSummary();

        if (windowPageControl != null && !windowPageControl.Controls.ContainsKey(_accountPage.Name))
        {
            windowPageControl.Controls.Add(_accountPage);
            ChangeControlsTheme(_accountPage);
            _accountPage.ApplyTheme();
        }

        return _accountPage;
    }

    private void EnsureAccountRefreshTimer()
    {
        if (_accountRefreshTimer != null)
            return;

        _accountRefreshTimer = new System.Windows.Forms.Timer { Interval = 30000 };
        _accountRefreshTimer.Tick += (_, _) =>
        {
            if (IsDisposed || Disposing)
                return;

            _accountPage?.RefreshData();
            UpdateAccountMenuSummary();
        };
        FormClosing += (_, _) =>
        {
            if (_accountRefreshTimer != null)
            {
                _accountRefreshTimer.Stop();
                _accountRefreshTimer.Dispose();
                _accountRefreshTimer = null;
            }
        };
        _accountRefreshTimer.Start();
    }

    private void OpenAccountPage()
    {
        var page = EnsureAccountPage();
        if (page == null)
            return;

        page.RefreshData();

        if (windowPageControl == null)
            return;

        var pageIndex = windowPageControl.IndexOfPage(page);
        if (pageIndex >= 0)
            windowPageControl.SelectedIndex = pageIndex;
    }

    private void UpdateAccountMenuSummary()
    {
        if (menuAccountSummary == null)
            return;

        if (InvokeRequired)
        {
            try { BeginInvoke(new Action(UpdateAccountMenuSummary)); } catch { }
            return;
        }

        bool ar = Kernel.Language == "ar_AR";
        var user = AuthService.Instance.CurrentUser;
        if (user == null)
        {
            menuAccountSummary.Text = ar ? "👤 الحساب" : "👤 Account";
            menuAccountSummary.ToolTipText = ar ? "عرض تفاصيل الحساب والاشتراك" : "View account and subscription details";
            menuAccountSummary.ForeColor = ColorScheme.AccentColor;
            if (_gearAccountItem != null)
                _gearAccountItem.Text = menuAccountSummary.Text;
            return;
        }

        string remaining = AccountPageControl.FormatRemaining(user, ar);
        menuAccountSummary.Text = $"👤 {user.Username}  •  ⏳ {remaining}";
        menuAccountSummary.ToolTipText = ar
            ? "انقر لفتح صفحة الحساب وتفاصيل الاشتراك"
            : "Click to open the Account & Subscription page";

        if (user.IsBlocked || user.IsExpired)
            menuAccountSummary.ForeColor = Color.FromArgb(220, 38, 38);
        else if (!user.IsLifetime && user.RemainingTime is { TotalDays: <= 3 })
            menuAccountSummary.ForeColor = Color.FromArgb(217, 119, 6);
        else
            menuAccountSummary.ForeColor = ColorScheme.AccentColor;

        if (_gearAccountItem != null)
            _gearAccountItem.Text = menuAccountSummary.Text;
    }

    /// <summary>
    ///     Applies a fixed, deterministic sidebar order so pages never jump around.
    /// </summary>
    private void ApplyNavigationOrder()
    {
        if (windowPageControl == null)
            return;

        windowPageControl.GroupBreakBefore.Clear();

        var pages = new List<Control>(windowPageControl.Count);
        for (int i = 0; i < windowPageControl.Count; i++)
            pages.Add(windowPageControl.GetPage(i));

        windowPageControl.SetPageOrder(pages.OrderBy(GetNavigationRank).ToList());
    }

    private static int GetNavigationRank(Control control)
    {
        if (control == null)
            return int.MaxValue;

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

        return index < 0 ? 100 : index;
    }

    /// <summary>
    ///     Applies the vBot-like light chrome to the main window.
    /// </summary>
    private void ApplyVisualStyle()
    {
        BackColor = ColorScheme.BackColor;
        TitleHeight = 32;
        VerticalTabs = true;
        _extensionsHub?.ApplyTheme();
        _accountPage?.ApplyTheme();

        if (menuStrip != null)
        {
            ApplyThemeToToolStrip(menuStrip);
            UpdateAccountMenuSummary();
        }

        if (_gearMenu != null && !_gearMenu.Visible)
        {
            RebuildGearMenu();
        }
        else if (ExtendMenu != null)
        {
            ApplyThemeToToolStrip(ExtendMenu);
        }

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

        if (logPanel != null)
        {
            logPanel.BackColor = ColorScheme.ChromeColor;
            logPanel.Border = new Padding(0, 1, 0, 0);
            logPanel.BorderColor = ColorScheme.BorderColor;
        }

        if (pnlLogHeader != null)
            pnlLogHeader.BackColor = ColorScheme.ChromeColor;

        if (lblLogTitle != null)
            lblLogTitle.ForeColor = ColorScheme.SecondaryForeColor;

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

            if (lstLog.ContextMenuStrip == null)
            {
                var contextMenu = new System.Windows.Forms.ContextMenuStrip();
                var copyItem = new ToolStripMenuItem("نسخ السطر المحدد (Ctrl+C)", null, (s, e) =>
                {
                    if (lstLog.SelectedItems.Count > 0)
                    {
                        var lines = lstLog.SelectedItems.Cast<ListViewItem>()
                            .Select(i => $"{i.SubItems[0].Text} {i.SubItems[1].Text} {i.SubItems[2].Text}");
                        Clipboard.SetText(string.Join(Environment.NewLine, lines));
                    }
                });
                var openLogItem = new ToolStripMenuItem("فتح ملف الباكتات (packets.log)", null, (s, e) =>
                {
                    try
                    {
                        var path = RSBot.Core.Network.PacketLogger.LocalLogPath;
                        if (System.IO.File.Exists(path))
                            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(path) { UseShellExecute = true });
                    }
                    catch { }
                });
                var clearItem = new ToolStripMenuItem("مسح السجلات", null, (s, e) => lstLog.Items.Clear());

                contextMenu.Items.Add(copyItem);
                contextMenu.Items.Add(openLogItem);
                contextMenu.Items.Add(new ToolStripSeparator());
                contextMenu.Items.Add(clearItem);
                lstLog.ContextMenuStrip = contextMenu;

                lstLog.KeyDown += (s, e) =>
                {
                    if (e.Control && e.KeyCode == Keys.C && lstLog.SelectedItems.Count > 0)
                    {
                        var lines = lstLog.SelectedItems.Cast<ListViewItem>()
                            .Select(i => $"{i.SubItems[0].Text} {i.SubItems[1].Text} {i.SubItems[2].Text}");
                        Clipboard.SetText(string.Join(Environment.NewLine, lines));
                    }
                };
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
        if (btnSave != null)
            btnSave.Text = ar ? "حفظ الإعدادات" : "Save Settings";
        UpdateShowClientButton(ClientManager.IsVisible);

        if (lblLogTitle != null)
            lblLogTitle.Text = ar ? "السجل" : "Log";
        if (btnClearLog != null)
            btnClearLog.Text = ar ? "مسح" : "Clear";

        var neutralBack = ColorScheme.BackColor.IsDark() ? ColorScheme.CardColor2 : Color.White;
        var neutralFore = ColorScheme.BackColor.IsDark() ? ColorScheme.ForeColor : Color.FromArgb(55, 65, 81);

        StyleActionButton(btnStartBot, Color.FromArgb(34, 160, 107), Color.White);
        StyleActionButton(btnStopBot, neutralBack, neutralFore);
        StyleActionButton(btnShowClient, neutralBack, neutralFore);
        if (btnSave != null)
            StyleActionButton(btnSave, Color.FromArgb(41, 128, 185), Color.White);
        if (btnClearLog != null && btnClearLog.Parent != null)
            StyleActionButton(btnClearLog, neutralBack, neutralFore);

        bool running = Kernel.Bot?.Running == true;
        if (btnStartBot != null)
            btnStartBot.Enabled = !running;
        if (btnStopBot != null)
            btnStopBot.Enabled = running;
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

        if (_extensionsHub != null && !_extensionsHub.IsDisposed)
        {
            _extensionsHub.Text = GetExtensionsHubTitle();
            _extensionsHub.RefreshLocalization();
        }

        if (_accountPage != null && !_accountPage.IsDisposed)
        {
            _accountPage.Text = GetAccountTabTitle();
            _accountPage.ApplyLanguage();
        }

        UpdateAccountMenuSummary();
        RebuildGearMenu();
    }
    private SDUI.Controls.ContextMenuStrip _gearMenu;

    private void InitializeGearMenu()
    {
        _gearMenu = new SDUI.Controls.ContextMenuStrip
        {
            Renderer = new SDUI.Renderers.MenuRenderer()
        };
        ExtendMenu = _gearMenu;
        RebuildGearMenu();
    }

    private void RebuildGearMenu()
    {
        if (_gearMenu == null) return;
        _gearMenu.Items.Clear();

        bool ar = Kernel.Language == "ar_AR";

        // File
        var mFile = new ToolStripMenuItem(ar ? "ملف" : "File");
        var mSave = new ToolStripMenuItem(ar ? "💾 حفظ الإعدادات" : "💾 Save Settings", null, btnSave_Click)
        {
            ShortcutKeys = Keys.Control | Keys.S
        };
        var mSelProfile = new ToolStripMenuItem(ar ? "اختيار الملف الشخصي..." : "Select Profile...", null, menuSelectProfile_Click);
        var mProxy = new ToolStripMenuItem(ar ? "إعدادات البروكسي" : "Proxy Config", null, networkConfigToolStripMenuItem_Click);
        var mExit = new ToolStripMenuItem(ar ? "خروج" : "Exit", null, menuItemExit_Click);
        mFile.DropDownItems.AddRange(new ToolStripItem[] { mSave, new ToolStripSeparator(), mSelProfile, mProxy, mExit });

        // View
        var mView = new ToolStripMenuItem(ar ? "عرض" : "View");
        var mSidebar = new ToolStripMenuItem(ar ? "الشريط الجانبي" : "Sidebar", null, menuSidebar_Click)
        {
            Checked = _sidebarVisible
        };
        mView.DropDownItems.Add(mSidebar);

        // Tools
        var mTools = new ToolStripMenuItem(ar ? "أدوات" : "Tools");
        var mRecorder = new ToolStripMenuItem(ar ? "مسجل السكربت" : "Script Recorder", null, menuScriptRecorder_Click);
        mTools.DropDownItems.Add(mRecorder);

        // Language
        var mLang = new ToolStripMenuItem(ar ? "اللغة" : "Language");
        foreach (var lang in LanguageManager.GetLanguages())
        {
            var langItem = new ToolStripMenuItem(lang.Value)
            {
                Tag = lang.Key,
                Checked = string.Equals(Kernel.Language, lang.Key, StringComparison.OrdinalIgnoreCase)
            };
            langItem.Click += LanguageDropdown_Click;
            mLang.DropDownItems.Add(langItem);
        }

        // Theme
        bool isAutoTheme = GlobalConfig.Get("RSBot.Theme.Auto", true);
        bool isDarkTheme = !isAutoTheme && ColorScheme.BackColor == DarkThemeColor;
        bool isLightTheme = !isAutoTheme && ColorScheme.BackColor == LightThemeColor;
        var mTheme = new ToolStripMenuItem(ar ? "المظهر" : "Theme");
        var mDark = new ToolStripMenuItem(ar ? "داكن" : "Dark", null, darkToolStripMenuItem_Click) { Checked = isDarkTheme };
        var mLight = new ToolStripMenuItem(ar ? "فاتح" : "Light", null, lightToolStripMenuItem_Click) { Checked = isLightTheme };
        var mAuto = new ToolStripMenuItem(ar ? "تلقائي" : "Auto", null, autoToolStripMenuItem_Click) { Checked = isAutoTheme };
        var mColor = new ToolStripMenuItem(ar ? "اختيار لون" : "Choose a color", null, coloredToolStripMenuItem_Click)
        {
            Checked = !isAutoTheme && !isDarkTheme && !isLightTheme
        };
        mTheme.DropDownItems.AddRange(new ToolStripItem[] { mDark, mLight, mAuto, mColor });

        // About
        var mAbout = new ToolStripMenuItem(ar ? "حول" : "About");
        var mThis = new ToolStripMenuItem(ar ? "هذا البرنامج" : "This", null, menuItemThis_Click);
        mAbout.DropDownItems.Add(mThis);

        var accountLabel = !string.IsNullOrWhiteSpace(menuAccountSummary?.Text)
            ? menuAccountSummary.Text
            : (ar ? "👤 الحساب" : "👤 Account");
        _gearAccountItem = new ToolStripMenuItem(accountLabel, null, (s, e) => OpenAccountPage());

        _gearMenu.Items.AddRange(new ToolStripItem[]
        {
            mFile, mView, mTools, mLang, mTheme, mAbout,
            new ToolStripSeparator(),
            _gearAccountItem
        });

        ApplyThemeToToolStrip(_gearMenu);
    }

    private void ApplyThemeToToolStrip(System.Windows.Forms.ToolStrip strip)
    {
        if (strip == null) return;
        strip.BackColor = ColorScheme.BackColor.IsDark()
            ? Color.FromArgb(20, 26, 38)
            : Color.FromArgb(240, 242, 245);
        strip.ForeColor = ColorScheme.ForeColor;
        strip.Renderer = new SDUI.Renderers.MenuRenderer();

        foreach (ToolStripItem item in strip.Items)
        {
            ApplyThemeToItem(item);
        }
    }

    private void ApplyThemeToItem(ToolStripItem item)
    {
        if (item == null) return;
        if (item.Tag?.ToString() != "private")
        {
            item.ForeColor = ColorScheme.ForeColor;
        }

        if (item is ToolStripDropDownItem dropDownItem)
        {
            dropDownItem.DropDown.BackColor = ColorScheme.BackColor.IsDark()
                ? Color.FromArgb(24, 30, 44)
                : Color.FromArgb(250, 250, 250);
            dropDownItem.DropDown.ForeColor = ColorScheme.ForeColor;
            dropDownItem.DropDown.Renderer = new SDUI.Renderers.MenuRenderer();

            foreach (ToolStripItem subItem in dropDownItem.DropDownItems)
            {
                ApplyThemeToItem(subItem);
            }
        }
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
    ///     Switches the sidebar tab to the requested botbase page without altering the Training botbase.
    /// </summary>
    /// <param name="name">The botbase name.</param>
    private void SelectBotbase(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            return;

        if (windowPageControl != null && windowPageControl.Controls.ContainsKey(name))
        {
            var page = windowPageControl.Controls[name];
            var pageIndex = windowPageControl.IndexOfPage(page);
            if (pageIndex >= 0)
                windowPageControl.SelectedIndex = pageIndex;
        }
        else if (_extensionsHub != null && _extensionsHub.ContainsTool(name))
        {
            if (windowPageControl != null && windowPageControl.Controls.ContainsKey(_extensionsHub.Name))
            {
                var hubIndex = windowPageControl.IndexOfPage(_extensionsHub);
                if (hubIndex >= 0)
                    windowPageControl.SelectedIndex = hubIndex;
            }

            _extensionsHub.SelectTool(name);
        }
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

        foreach (var extension in extensions.Where(extension =>
                     !HiddenSidebarPlugins.Contains(extension.Value.InternalName) &&
                     (extension.Value.DisplayAsTab || GetExtensionsHubOrder(extension.Value.InternalName) >= 0)))
        {
            try
            {
                extension.Value.Translate();

                var control = extension.Value.View;
                if (control == null)
                    continue;

                var hubOrder = GetExtensionsHubOrder(extension.Value.InternalName);
                if (hubOrder >= 0)
                {
                    var ext = extension.Value;
                    EnsureExtensionsHub().AddTool(
                        ext.InternalName,
                        () => LanguageManager.GetLangBySpecificKey(ext.InternalName, "DisplayName", ext.DisplayName),
                        control,
                        hubOrder);
                    ChangeControlsTheme(control);
                    continue;
                }

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

        ApplyNavigationOrder();
        RebuildGearMenu();
    }

    /// <summary>
    ///     Shows or hides the character sidebar.
    /// </summary>
    private void ConfigureSidebar()
    {
        pSidebar.Visible = _sidebarVisible;
    }

    /// <summary>
    ///     Shows or hides the log panel. The page area automatically reclaims
    ///     the space because every section is docked.
    /// </summary>
    private void ConfigureLog()
    {
        if (logPanel != null)
            logPanel.Visible = false;
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
        try
        {
            // 1. Ensure profile selection is persisted
            if (!string.IsNullOrWhiteSpace(ProfileManager.SelectedProfile))
            {
                ProfileManager.SetSelectedProfile(ProfileManager.SelectedProfile);
            }

            // 2. Save Global configuration
            GlobalConfig.Save();

            // 3. Save Player configuration if player is in game or loaded
            if (PlayerConfig.IsLoaded)
            {
                PlayerConfig.Save();
            }
            else if (Game.Player != null && !string.IsNullOrWhiteSpace(Game.Player.Name))
            {
                PlayerConfig.Load(Game.Player.Name);
                PlayerConfig.Save();
            }

            // 4. Notify all plugins / botbases
            EventManager.FireEvent("OnSaveSettings");
            EventManager.FireEvent("OnSavePlayerConfig");
            EventManager.FireEvent("OnSaveGlobalConfig");

            bool ar = Kernel.Language == "ar_AR";
            Log.Notify(ar ? "[AeroBot] تم حفظ جميع الإعدادات بنجاح!" : "[AeroBot] All settings have been saved successfully!");

            // Visual feedback on the button
            if (btnSave != null && !btnSave.IsDisposed)
            {
                var originalText = ar ? "حفظ الإعدادات" : "Save Settings";
                btnSave.Text = ar ? "تم الحفظ ✓" : "Saved ✓";
                btnSave.Color = Color.FromArgb(46, 171, 107);

                var timer = new System.Windows.Forms.Timer { Interval = 1500 };
                timer.Tick += (s, ev) =>
                {
                    timer.Stop();
                    timer.Dispose();
                    if (btnSave != null && !btnSave.IsDisposed)
                    {
                        btnSave.Text = originalText;
                        btnSave.Color = Color.FromArgb(41, 128, 185);
                    }
                };
                timer.Start();
            }
        }
        catch (Exception ex)
        {
            Log.Warn($"[AeroBot] Could not save settings: {ex.Message}");
            MessageBox.Show(this, $"Error saving settings:\n{ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
    {
        if (keyData == (Keys.Control | Keys.S))
        {
            btnSave_Click(btnSave, EventArgs.Empty);
            return true;
        }
        return base.ProcessCmdKey(ref msg, keyData);
    }

    /// <summary>
    ///     Handles the Click event of the menuSidebar control.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
    private void menuSidebar_Click(object sender, EventArgs e)
    {
        _sidebarVisible = !_sidebarVisible;
        GlobalConfig.Set("RSBot.ShowSidebar", _sidebarVisible.ToString());
        ConfigureSidebar();
        RebuildGearMenu();
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
        _sidebarVisible = GlobalConfig.Get("RSBot.ShowSidebar", false);
        GlobalConfig.Set("RSBot.ShowLog", "false");

        ConfigureSidebar();
        ConfigureLog();
        VerticalTabs = true;
        WindowPageControl = windowPageControl;

        EnsureAccountPage();
        ApplyNavigationOrder();
        ApplyCurrentLanguage();
        ApplyVisualStyle();
        _accountPage?.RefreshData();
        UpdateAccountMenuSummary();
        RebuildGearMenu();
        EnsureAccountRefreshTimer();

        ClearMenuSelection();

        EventManager.FireEvent("OnInitialized");
    }

    /// <summary>
    ///     Removes the focus/hover highlight that the menu strip shows on the
    ///     first item right after startup.
    /// </summary>
    private void ClearMenuSelection()
    {
        if (menuStrip == null)
            return;

        // MenuStrip keeps the first item highlighted while it owns the focus.
        // Releasing the active control lets the page area own the focus again.
        ActiveControl = null;
        menuStrip.Invalidate();
    }

    /// <summary>
    private void LanguageDropdown_Click(object sender, EventArgs e)
    {
        var dropdown = sender as ToolStripMenuItem;
        if (dropdown == null || dropdown.Checked || dropdown.Tag == null)
            return;

        LanguageManager.ClearCache();
        Kernel.Language = dropdown.Tag.ToString();

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

                var localizedTabText = LanguageManager.GetLangBySpecificKey(botbase.Key, "TabText", botbase.Value.TabText);

                if (windowPageControl.Controls.ContainsKey(botbase.Key))
                    windowPageControl.Controls[botbase.Key].Text = localizedTabText;
            }
            catch (Exception ex)
            {
                Log.Error($"[LanguageDropdown_Click] Error translating botbase [{botbase.Key}]: {ex.Message}");
            }
        }

        LanguageManager.Translate(this, Kernel.Language);

        if (_accountPage != null && !_accountPage.IsDisposed)
        {
            _accountPage.Text = GetAccountTabTitle();
            _accountPage.ApplyLanguage();
        }

        UpdateAccountMenuSummary();
        
        ApplyCurrentLanguage();

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
        if (Kernel.Bot == null || !Kernel.Bot.Running)
            StartBot();
        else
            StopBot();
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
            SaveAllOnExit();
            Environment.Exit(0);
        }

        var exitDialog = new ExitDialog();
        if (exitDialog.ShowDialog(this) != DialogResult.Yes)
        {
            e.Cancel = true;
            return;
        }

        SaveAllOnExit();
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

    private void SaveAllOnExit()
    {
        try
        {
            if (!string.IsNullOrWhiteSpace(ProfileManager.SelectedProfile))
                ProfileManager.SetSelectedProfile(ProfileManager.SelectedProfile);

            GlobalConfig.Save();

            if (PlayerConfig.IsLoaded)
                PlayerConfig.Save();
            else if (Game.Player != null && !string.IsNullOrWhiteSpace(Game.Player.Name))
            {
                PlayerConfig.Load(Game.Player.Name);
                PlayerConfig.Save();
            }

            EventManager.FireEvent("OnSaveSettings");
            EventManager.FireEvent("OnSavePlayerConfig");
            EventManager.FireEvent("OnSaveGlobalConfig");
        }
        catch { }
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
            SaveAllOnExit();
            Environment.Exit(0);
        }

        var exitDialog = new ExitDialog();
        if (exitDialog.ShowDialog(this) != DialogResult.Yes)
            return;

        SaveAllOnExit();
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
            try
            {
                bot.Value.Translate();
            }
            catch (Exception ex)
            {
                Log.Error($"[OnLoadBotbases] Error translating botbase [{bot.Key}]: {ex.Message}");
            }

            var control = bot.Value.View;
            var localizedTabText = LanguageManager.GetLangBySpecificKey(bot.Value.Name, "TabText", bot.Value.TabText);

            if (control != null)
            {
                var hubOrder = GetExtensionsHubOrder(bot.Value.Name);
                if (hubOrder >= 0)
                {
                    var currentBot = bot.Value;
                    EnsureExtensionsHub().AddTool(
                        currentBot.Name,
                        () => LanguageManager.GetLangBySpecificKey(currentBot.Name, "TabText", currentBot.TabText),
                        control,
                        hubOrder);
                    ChangeControlsTheme(control);
                }
                else
                {
                    control.Name = bot.Value.Name;
                    control.Text = localizedTabText;
                    control.Enabled = true;
                    control.Dock = DockStyle.Fill;

                    if (!windowPageControl.Controls.ContainsKey(control.Name))
                    {
                        windowPageControl.Controls.Add(control);
                        ChangeControlsTheme(control);
                    }
                }
            }

        }

        var trainingBotbase = Kernel.BotbaseManager.Bots.FirstOrDefault(bot => bot.Key == "RSBot.Default").Value
                              ?? Kernel.BotbaseManager.Bots.Values.FirstOrDefault();

        if (trainingBotbase != null)
        {
            Kernel.Bot?.SetBotbase(trainingBotbase);
            GlobalConfig.Set("RSBot.BotName", trainingBotbase.Name);
        }

        ApplyNavigationOrder();
        RebuildGearMenu();
    }

    /// <summary>
    ///     Handles the Click event of the MenuItem control.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
    /// <exception cref="System.NotImplementedException"></exception>


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

        _extensionsHub?.SetIngameState(false);

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

        _extensionsHub?.SetIngameState(true);

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

    private bool EnsureTrainingBotbase()
    {
        if (Kernel.Bot == null)
            return false;

        if (Kernel.Bot.Botbase?.Name == "RSBot.Default")
            return true;

        var trainingBotbase = Kernel.BotbaseManager?.Bots?
            .FirstOrDefault(bot => bot.Key == "RSBot.Default").Value;
        if (trainingBotbase == null)
        {
            Log.NotifyLang("NotifyPleaseSelectProperBotBase");
            return false;
        }

        Kernel.Bot.SetBotbase(trainingBotbase);
        return true;
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
            if (!EnsureTrainingBotbase())
                return;

            if (!Kernel.Bot.TryStart(out var errorMessage))
            {
                if (!string.IsNullOrWhiteSpace(errorMessage))
                    MessageBox.Show(errorMessage, "AeroBot", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            Log.StatusLang("Running");
        }
    }

    private void StopBot()
    {
        if (Kernel.Bot != null && Kernel.Bot.Running)
        {
            Log.NotifyLang("StopingBot", Kernel.Bot.Botbase?.DisplayName ?? "Training");
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
