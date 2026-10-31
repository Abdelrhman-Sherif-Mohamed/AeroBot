using System;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using RSBot.Core;
using RSBot.Core.Components;
using RSBot.Core.Security;
using RSBot.Views.Dialog;
using SDUI;
using SDUI.Controls;
using SDUI.Helpers;

namespace RSBot.Views;

public partial class SplashScreen : UIWindow
{
    private readonly Main _mainForm;

    /// <summary>
    ///     Initializes a new instance of the <see cref="SplashScreen" /> class.
    /// </summary>
    public SplashScreen()
    {
        InitializeComponent();

        ColorScheme.BackColor = Main.DarkThemeColor;
        BackColor = Main.DarkThemeColor;
        CheckForIllegalCrossThreadCalls = false;

        _mainForm = new Main();

        labelVersion.Text = Program.AssemblyVersion;

        try
        {
            string splashPath = Path.Combine(AppContext.BaseDirectory, "splash_banner.jpg");
            if (!File.Exists(splashPath))
                splashPath = Path.Combine(AppContext.BaseDirectory, "Resources", "splash_banner.jpg");

            if (File.Exists(splashPath))
            {
                using var stream = new MemoryStream(File.ReadAllBytes(splashPath));
                pictureBox.Image = Image.FromStream(stream);
                pictureBox.SizeMode = PictureBoxSizeMode.StretchImage;
            }
            else
            {
                pictureBox.Image = Properties.Resources.app;
                pictureBox.SizeMode = PictureBoxSizeMode.StretchImage;
            }

            string icoPath = Path.Combine(AppContext.BaseDirectory, "app.ico");
            if (File.Exists(icoPath))
            {
                using var icoStream = new MemoryStream(File.ReadAllBytes(icoPath));
                Icon = new Icon(icoStream);
            }
        }
        catch { }

        referenceDataLoader.RunWorkerCompleted += ReferenceDataLoaderCompleted;
    }

    /// <summary>
    ///     Handles the Load event of the SplashScreen control.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
    private async void SplashScreen_Load(object sender, EventArgs e)
    {
        // 0. Load language configuration early
        Kernel.Language = GlobalConfig.Get("RSBot.Language", "ar_AR");
        var isArabic = UpdateManager.IsArabic;

        // 1. Post-update notification: Show the "What's New" box if an update was just installed
        var justUpdated = UpdateManager.GetJustUpdatedInfo();
        if (justUpdated != null)
        {
            UpdateManager.ClearJustUpdatedMarker();
            using (var whatIsNew = new WhatIsNewDialog(justUpdated))
            {
                whatIsNew.TopMost = true;
                whatIsNew.ShowDialog();
            }
        }
        else
        {
            // 2. Check for new updates from Firebase
            lblLoading.Text = isArabic ? "جارٍ التحقق من التحديثات..." : "Checking for updates...";
            Application.DoEvents();

            var updateInfo = await UpdateManager.CheckForUpdateAsync();
            if (updateInfo != null && updateInfo.HasNewVersion && !string.IsNullOrWhiteSpace(updateInfo.DownloadUrl))
            {
                using (var updateDialog = new UpdateAvailableDialog(updateInfo))
                {
                    updateDialog.TopMost = true;
                    if (updateDialog.ShowDialog() == DialogResult.OK)
                    {
                        // User confirmed update: download in launcher
                        lblLoading.Text = isArabic ? "يتم التحديث... جارٍ تحميل الملفات (0%)" : "Updating... Downloading files (0%)";
                        progressLoading.Value = 0;
                        Application.DoEvents();

                        var tempZip = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "update_temp.zip");
                        var progressReporter = new Progress<int>(percent =>
                        {
                            progressLoading.Value = percent;
                            lblLoading.Text = isArabic ?
                                $"يتم التحديث... جارٍ تحميل الملفات ({percent}%)" :
                                $"Updating... Downloading files ({percent}%)";
                            Application.DoEvents();
                        });

                        var success = await UpdateManager.DownloadUpdateAsync(updateInfo.DownloadUrl, tempZip, progressReporter);
                        if (success)
                        {
                            lblLoading.Text = isArabic ?
                                "يتم التحديث... جارٍ تطبيق التحديث وإعادة التشغيل" :
                                "Updating... Applying update and restarting";
                            Application.DoEvents();
                            await System.Threading.Tasks.Task.Delay(600);

                            UpdateManager.ApplyUpdateAndRestart(tempZip, updateInfo);
                            return;
                        }
                        else
                        {
                            var detail = string.IsNullOrWhiteSpace(UpdateManager.LastError) ? "" : $"\n({UpdateManager.LastError})";
                            var errMsg = isArabic ? $"فشل تحميل التحديث! تأكد من اتصال الإنترنت.{detail}" : $"Failed to download update! Please check internet connection.{detail}";
                            MessageBox.Show(this, errMsg, "Aerobot", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        }
                    }
                    else if (updateInfo.Mandatory)
                    {
                        Environment.Exit(0);
                        return;
                    }
                }
            }
        }

        if (!LoadProfileConfig())
        {
            Environment.Exit(0);
            return;
        }

        LanguageManager.Translate(_mainForm, Kernel.Language);

        var silkroadDirectory = GlobalConfig.Get<string>("RSBot.SilkroadDirectory");

        if (string.IsNullOrWhiteSpace(silkroadDirectory) || !Game.IsValidClientDirectory(silkroadDirectory))
        {
            var dialog = new OpenFileDialog
            {
                Title = LanguageManager.GetLang("OpenFileDialogTitle"),
                Filter = "Silkroad Client / Archives (*.exe;*.pk2;*.ne;*.bin)|*.exe;*.pk2;*.ne;*.bin|All Files (*.*)|*.*",
                FileName = "sro_client.exe"
            };

            var result = dialog.ShowDialog(this);

            silkroadDirectory = Path.GetDirectoryName(dialog.FileName);

            if (result == DialogResult.OK && Game.IsValidClientDirectory(silkroadDirectory))
            {
                GlobalConfig.Set("RSBot.SilkroadDirectory", silkroadDirectory);
                GlobalConfig.Set("RSBot.SilkroadExecutable", Path.GetFileName(dialog.FileName));

                var title = LanguageManager.GetLang("ClientTypeInputDialogTitle");
                var content = LanguageManager.GetLang("ClientTypeInputDialogContent");

                var clientTypeDialog = new InputDialog(title, title, content, InputDialog.InputType.Combobox);
                clientTypeDialog.ShowInTaskbar = true;
                clientTypeDialog.StartPosition = FormStartPosition.CenterScreen;
                clientTypeDialog.Selector.Items.AddRange(Enum.GetNames(typeof(GameClientType)));
                clientTypeDialog.Selector.SelectedIndex = 2;
                clientTypeDialog.TopMost = true;
                clientTypeDialog.StartPosition = FormStartPosition.CenterScreen;

                if (clientTypeDialog.ShowDialog() == DialogResult.OK)
                {
                    if (Enum.TryParse<GameClientType>(clientTypeDialog.Value.ToString(), out var clientType))
                        GlobalConfig.Set("RSBot.Game.ClientType", clientType);
                }
                else
                {
                    MessageBox.Show(LanguageManager.GetLang("ClientTypeNotSelected"));
                    GlobalConfig.Set("RSBot.Game.ClientType", GameClientType.Vietnam);
                }
            }
            else
            {
                MessageBox.Show(LanguageManager.GetLang("SelectSroDirWarn"));
                Environment.Exit(0);
            }
        }

        InitializeBot();

        referenceDataLoader.RunWorkerAsync();
    }

    /// <summary>
    ///     Handles the RunWorkerCompleted event of the Initializer control.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">
    ///     The <see cref="System.ComponentModel.RunWorkerCompletedEventArgs" /> instance containing the event
    ///     data.
    /// </param>
    private void ReferenceDataLoaderCompleted(object sender, RunWorkerCompletedEventArgs e)
    {
        var detectDarkLight = GlobalConfig.Get("RSBot.Theme.Auto", true);
        if (detectDarkLight)
        {
            if (WindowsHelper.IsDark())
                ColorScheme.BackColor = Main.DarkThemeColor;
            else
                ColorScheme.BackColor = Main.LightThemeColor;
        }
        else
        {
            ColorScheme.BackColor = Color.FromArgb(GlobalConfig.Get("SDUI.Color", Color.White.ToArgb()));
        }

        _mainForm.Show(this);
        _mainForm.RefreshTheme();
        _mainForm.BringToFront();

        Hide();
    }

    /// <summary>
    ///     Loads the profile configuration.
    /// </summary>
    private bool LoadProfileConfig()
    {
        if (!ProfileManager.IsProfileLoadedByArgs)
        {
            if (ProfileManager.ShowProfileDialog)
            {
                var dialog = new ProfileSelectionDialog();
                if (dialog.ShowDialog() != DialogResult.Cancel)
                    ProfileManager.SetSelectedProfile(dialog.SelectedProfile);
                else
                    return false;
            }
            else //Load selected profile without dialog
            {
                var selectedProfile = ProfileManager.SelectedProfile;
                var profilePath = ProfileManager.GetProfileFile(selectedProfile);

                //Configured profile could not be found. Fallback to default profile
                if (!string.IsNullOrEmpty(selectedProfile) && !File.Exists(profilePath))
                    selectedProfile = "Default";

                ProfileManager.SetSelectedProfile(selectedProfile);
            }
        }

        GlobalConfig.Load();
        Log.Notify($"Loaded profile {ProfileManager.SelectedProfile}");

        return true;
    }

    /// <summary>
    ///     Initializes the bot.
    /// </summary>
    private void InitializeBot()
    {
        Kernel.Initialize();
        Game.Initialize();

        if (!Kernel.PluginManager.LoadAssemblies())
        {
            MessageBox.Show(@"Failed to load plugins. Process canceled!", @"Initialize Application - Error",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }

        if (!Kernel.BotbaseManager.LoadAssemblies())
        {
            MessageBox.Show(@"Failed to load botbases. Process canceled!", @"Initialize Application - Error",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }

        CommandManager.Initialize();
    }

    /// <summary>
    ///     Handles the DoWork event of the referenceDataLoader control.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="System.ComponentModel.DoWorkEventArgs" /> instance containing the event data.</param>
    private void referenceDataLoader_DoWork(object sender, DoWorkEventArgs e)
    {
        if (!Game.InitializeArchiveFiles())
        {
            MessageBox.Show(@"Failed to load game data. Boot process canceled!", @"Initialize Application - Error",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }

        Game.ReferenceManager.Load(GlobalConfig.Get("RSBot.TranslationIndex", 9), referenceDataLoader);
    }

    private void referenceDataLoader_ProgressChanged(object sender, ProgressChangedEventArgs e)
    {
        lblLoading.Text = $"Loading: {e.UserState}";
        progressLoading.Value = e.ProgressPercentage;
    }
}