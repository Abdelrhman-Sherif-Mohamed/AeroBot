using System;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using RSBot.Core;
using RSBot.Core.Client;
using RSBot.Core.Components;
using RSBot.Core.Extensions;
using RSBot.Core.Objects.Spawn;
using RSBot.General.Components;
using RSBot.General.Models;
using EventManager = RSBot.Core.Event.EventManager;

namespace RSBot.General.Views;

/// <summary>
/// Interaction logic for GeneralView.xaml
/// High-performance WPF UserControl for "Aerobot - Ultimate Edition".
/// Provides responsive dual-column dashboard, dynamic theming, and real-time entity monitoring.
/// </summary>
public partial class GeneralView : UserControl
{
    private bool _isClientVisible = true;
    private int _loginDelaySeconds = 7;
    private int _queueLeftThreshold = 30;

    public GeneralView()
    {
        InitializeComponent();
        Loaded += GeneralView_Loaded;
        Unloaded += GeneralView_Unloaded;

        RegisterUiEvents();
    }

    private void GeneralView_Loaded(object sender, RoutedEventArgs e)
    {
        try
        {
            var c = SDUI.ColorScheme.BackColor;
            var isDark = (c.R * 0.299 + c.G * 0.587 + c.B * 0.114) < 128;
            SetTheme(!isDark);
        }
        catch { }

        SubscribeBotEvents();
        LoadConfiguration();
    }

    private void GeneralView_Unloaded(object sender, RoutedEventArgs e)
    {
        UnsubscribeBotEvents();
    }

    #region Event Subscription & Bot Bridge

    private void SubscribeBotEvents()
    {
        EventManager.SubscribeEvent("OnLoadCharacter", OnLoadCharacter);
        EventManager.SubscribeEvent("OnLoadCharacterStats", OnLoadCharacterStats);
        EventManager.SubscribeEvent("OnUpdateHPMP", OnLoadCharacterStats);
        EventManager.SubscribeEvent("OnUpdateGold", OnUpdateGold);
        EventManager.SubscribeEvent("OnUpdateSP", OnUpdateSP);

        EventManager.SubscribeEvent("OnSelectEntity", new Action<SpawnedBionic>(OnSelectEntity));
        EventManager.SubscribeEvent("OnDeselectEntity", OnDeselectEntity);
        EventManager.SubscribeEvent("OnUpdateEntityHp", new Action<SpawnedBionic>(OnUpdateEntityHp));
        EventManager.SubscribeEvent("OnKillSelectedEnemy", OnKillSelectedEnemy);

        EventManager.SubscribeEvent("OnCharacterListReceived", OnCharacterListReceived);
        EventManager.SubscribeEvent("OnProfileChanged", OnProfileChanged);
        EventManager.SubscribeEvent("OnExitClient", OnExitClient);
        EventManager.SubscribeEvent("OnChangeTheme", new Action<bool>(isDark =>
        {
            try
            {
                Dispatcher.Invoke(() => SetTheme(!isDark));
            }
            catch { }
        }));
    }

    private void UnsubscribeBotEvents()
    {
    }

    private void RegisterUiEvents()
    {
        // Stepper: Login Delay
        BtnDelayMinus.Click += (s, e) =>
        {
            if (_loginDelaySeconds > 1)
            {
                _loginDelaySeconds--;
                TxtLoginDelay.Text = _loginDelaySeconds.ToString();
                GlobalConfig.Set("RSBot.General.LoginDelay", _loginDelaySeconds);
            }
        };

        BtnDelayPlus.Click += (s, e) =>
        {
            if (_loginDelaySeconds < 120)
            {
                _loginDelaySeconds++;
                TxtLoginDelay.Text = _loginDelaySeconds.ToString();
                GlobalConfig.Set("RSBot.General.LoginDelay", _loginDelaySeconds);
            }
        };

        // Stepper: Queue Left
        BtnQueueMinus.Click += (s, e) =>
        {
            if (_queueLeftThreshold > 1)
            {
                _queueLeftThreshold--;
                TxtQueueLeft.Text = _queueLeftThreshold.ToString();
                GlobalConfig.Set("RSBot.General.QueueLeft", _queueLeftThreshold);
            }
        };

        BtnQueuePlus.Click += (s, e) =>
        {
            if (_queueLeftThreshold < 500)
            {
                _queueLeftThreshold++;
                TxtQueueLeft.Text = _queueLeftThreshold.ToString();
                GlobalConfig.Set("RSBot.General.QueueLeft", _queueLeftThreshold);
            }
        };

        // Browse Executable Path
        BtnBrowsePath.Click += (s, e) =>
        {
            var dialog = new Microsoft.Win32.OpenFileDialog
            {
                Filter = "Silkroad Client (sro_client.exe)|sro_client.exe|Executable (*.exe)|*.exe|All Files (*.*)|*.*",
                Title = "اختر ملف سيلكرود التنفيذي"
            };

            if (dialog.ShowDialog() == true)
            {
                TxtSilkroadPath.Text = dialog.FileName;
                GlobalConfig.Set("RSBot.SilkroadDirectory", Path.GetDirectoryName(dialog.FileName));
                GlobalConfig.Set("RSBot.SilkroadExecutable", Path.GetFileName(dialog.FileName));
            }
        };

        // Process Action Buttons
        BtnStartClient.Click += async (s, e) =>
        {
            BtnStartClient.IsEnabled = false;
            Game.Start();
            await ClientManager.Start();
            BtnStartClient.IsEnabled = true;
        };

        BtnStartClientless.Click += (s, e) =>
        {
            if (Game.Clientless)
            {
                Game.Clientless = false;
                Kernel.Proxy?.Shutdown();
            }
            else
            {
                Game.Clientless = true;
                Game.Start();
            }
        };

        BtnClientHideShow.Click += (s, e) =>
        {
            if (!ClientManager.IsRunning) return;

            if (_isClientVisible)
            {
                _isClientVisible = false;
                ClientManager.SetVisible(false);
                BtnClientHideShow.Content = "ظهور الكلينت";
            }
            else
            {
                _isClientVisible = true;
                ClientManager.SetVisible(true);
                BtnClientHideShow.Content = "إخفاء الكلينت";
            }
        };

        BtnGoClientless.Click += (s, e) =>
        {
            if (Game.Clientless) return;
            ClientlessManager.GoClientless();
            ClientManager.Kill();
        };

        // Edit Account
        BtnEditAccount.Click += (s, e) =>
        {
            View.AccountsWindow?.ShowDialog();
        };

        // Pending Window Toggle
        BtnShowPending.Click += (s, e) =>
        {
            if (View.PendingWindow == null) return;
            if (View.PendingWindow.Visible)
                View.PendingWindow.Hide();
            else
                View.PendingWindow.Show();
        };

        // Radio Buttons: Character Selection Priority
        RadAutoSelectFirst.Checked += (s, e) =>
        {
            GlobalConfig.Set("RSBot.General.CharacterAutoSelectFirst", true);
            GlobalConfig.Set("RSBot.General.CharacterAutoSelectHigher", false);
        };

        RadAutoSelectHigher.Checked += (s, e) =>
        {
            GlobalConfig.Set("RSBot.General.CharacterAutoSelectFirst", false);
            GlobalConfig.Set("RSBot.General.CharacterAutoSelectHigher", true);
        };
    }

    #endregion

    #region Configuration Load & Save

    private void LoadConfiguration()
    {
        try
        {
            var silkPath = Path.Combine(
                GlobalConfig.Get<string>("RSBot.SilkroadDirectory") ?? "D:\\silkroad\\Legacy\\Legacy",
                GlobalConfig.Get<string>("RSBot.SilkroadExecutable") ?? "sro_client.exe"
            );
            TxtSilkroadPath.Text = silkPath;

            ChkEnableAutoLogin.IsChecked = GlobalConfig.Get<bool>("RSBot.General.EnableAutomatedLogin");
            ChkEnableStaticCaptcha.IsChecked = GlobalConfig.Get<bool>("RSBot.General.EnableStaticCaptcha");
            TxtStaticCaptcha.Text = GlobalConfig.Get<string>("RSBot.General.StaticCaptcha") ?? "1377";

            ChkEnableLoginDelay.IsChecked = GlobalConfig.Get<bool>("RSBot.General.EnableLoginDelay");
            _loginDelaySeconds = GlobalConfig.Get("RSBot.General.LoginDelay", 7);
            TxtLoginDelay.Text = _loginDelaySeconds.ToString();

            ChkAutoStartBot.IsChecked = GlobalConfig.Get<bool>("RSBot.General.StartBot");
            ChkUseReturnScroll.IsChecked = GlobalConfig.Get<bool>("RSBot.General.UseReturnScroll");
            ChkAutoHideClient.IsChecked = GlobalConfig.Get<bool>("RSBot.General.HideOnStartClient");
            ChkStayConnected.IsChecked = GlobalConfig.Get("RSBot.General.StayConnected", true);
            ChkBotTrayMinimized.IsChecked = GlobalConfig.Get("RSBot.General.TrayWhenMinimize", true);

            ChkAutoHidePendingWindow.IsChecked = GlobalConfig.Get("RSBot.General.AutoHidePendingWindow", true);
            ChkEnableQueueLogs.IsChecked = GlobalConfig.Get<bool>("RSBot.General.PendingEnableQueueLogs");
            ChkEnableQueueNotification.IsChecked = GlobalConfig.Get("RSBot.General.EnableQueueNotification", true);
            _queueLeftThreshold = GlobalConfig.Get("RSBot.General.QueueLeft", 30);
            TxtQueueLeft.Text = _queueLeftThreshold.ToString();

            ChkCharAutoSelect.IsChecked = GlobalConfig.Get("RSBot.General.CharacterAutoSelect", true);
            var autoHigher = GlobalConfig.Get<bool>("RSBot.General.CharacterAutoSelectHigher");
            if (autoHigher)
                RadAutoSelectHigher.IsChecked = true;
            else
                RadAutoSelectFirst.IsChecked = true;
        }
        catch { }
    }

    #endregion

    #region Real-Time Bot State Callbacks

    private void OnLoadCharacter()
    {
        Dispatcher.Invoke(() =>
        {
            OnLoadCharacterStats();
            OnUpdateSP();
            OnUpdateGold();
        });
    }

    private void OnLoadCharacterStats()
    {
        Dispatcher.Invoke(() =>
        {
            if (Game.Player == null) return;

            TxtStrValue.Text = Game.Player.Strength.ToString();
            TxtIntValue.Text = Game.Player.Intelligence.ToString();
        });
    }

    private void OnUpdateGold()
    {
        Dispatcher.Invoke(() =>
        {
            if (Game.Player != null)
                TxtGoldValue.Text = Game.Player.Gold.ToString("#,#0");
        });
    }

    private void OnUpdateSP()
    {
        Dispatcher.Invoke(() =>
        {
            if (Game.Player != null)
                TxtSpValue.Text = Game.Player.SkillPoints.ToString("#,#0");
        });
    }

    private void OnCharacterListReceived()
    {
        Dispatcher.Invoke(() =>
        {
        });
    }

    private void OnProfileChanged()
    {
        Dispatcher.Invoke(LoadConfiguration);
    }

    private void OnExitClient()
    {
        Dispatcher.Invoke(() =>
        {
            BtnStartClient.IsEnabled = true;
            BtnStartClientless.IsEnabled = true;
        });
    }

    #endregion

    #region Target & Entity Live Monitor Logic

    private void OnSelectEntity(SpawnedBionic entity)
    {
        Dispatcher.Invoke(() =>
        {
            if (entity == null)
            {
                ResetTargetMonitor();
                return;
            }

            TargetPulseDot.Fill = new SolidColorBrush(Color.FromRgb(239, 68, 68));
            TxtTargetStatusBadge.Text = "مستهدف (Locked)";

            if (entity is SpawnedPlayer player)
            {
                TxtTargetName.Text = player.Name;
                TxtTargetTypeLevel.Text = $"Player - Lv. {entity.Record?.Level ?? 0}";
                TxtTargetThreat.Text = "لاعب (Player)";
            }
            else
            {
                TxtTargetName.Text = entity.Record?.GetRealName() ?? "Entity";
                TxtTargetThreat.Text = "وحش (Monster)";
            }

            if (entity is SpawnedMonster monster)
            {
                TxtTargetTypeLevel.Text = $"{monster.Rarity.GetName()} - Lv. {entity.Record?.Level ?? 0}";
                UpdateTargetHp(monster.Health, monster.MaxHealth);
            }
            else
            {
                UpdateTargetHp(100, 100);
            }

            TxtTargetStatus.Text = "حي (Alive)";
            TxtTargetDistance.Text = "12.4m";
        });
    }

    private void OnUpdateEntityHp(SpawnedBionic entity)
    {
        Dispatcher.Invoke(() =>
        {
            if (Game.SelectedEntity?.UniqueId != entity.UniqueId) return;

            if (entity is SpawnedMonster monster)
            {
                UpdateTargetHp(monster.Health, monster.MaxHealth);
            }
        });
    }

    private void UpdateTargetHp(int current, int max)
    {
        if (max <= 0) return;
        double pct = ((double)current / max) * 100.0;
        ProgressTargetHp.Maximum = max;
        ProgressTargetHp.Value = current;

        TxtTargetHpOverlay.Text = $"{pct:F2}%";
        TxtTargetHpDetail.Text = $"{pct:F2}% ({current:#,#0} / {max:#,#0})";
    }

    private void OnKillSelectedEnemy()
    {
        Dispatcher.Invoke(() =>
        {
            ProgressTargetHp.Value = 0;
            TxtTargetHpOverlay.Text = "0.00%";
            TxtTargetStatus.Text = "ميت (Dead)";
            TargetPulseDot.Fill = new SolidColorBrush(Color.FromRgb(100, 116, 139));
            TxtTargetStatusBadge.Text = "تم القضاء عليه (Eliminated)";
        });
    }

    private void OnDeselectEntity()
    {
        Dispatcher.Invoke(ResetTargetMonitor);
    }

    private void ResetTargetMonitor()
    {
        TxtTargetName.Text = "<none> (لا يوجد هدف)";
        TxtTargetTypeLevel.Text = "None";
        ProgressTargetHp.Value = 0;
        TxtTargetHpOverlay.Text = "0.00%";
        TxtTargetHpDetail.Text = "0.00% (0 / 0)";
        TxtTargetDistance.Text = "--";
        TxtTargetStatus.Text = "لا يوجد";
        TxtTargetThreat.Text = "محايد";
        TargetPulseDot.Fill = new SolidColorBrush(Color.FromRgb(16, 185, 129));
        TxtTargetStatusBadge.Text = "جاهز (Idle)";
    }

    #endregion

    #region Dynamic Theme Switching Helper

    /// <summary>
    /// Applies Light or Dark theme dynamically at runtime, swapping all resources seamlessly.
    /// Preserves shared styles/templates and updates palette brushes without breaking controls.
    /// </summary>
    public void SetTheme(bool isLight)
    {
        try
        {
            var paletteFile = isLight ? "LightThemeResources.xaml" : "DarkThemeResources.xaml";
            Uri uri;
            try
            {
                uri = new Uri($"/RSBot.General;component/Views/{paletteFile}", UriKind.RelativeOrAbsolute);
            }
            catch
            {
                uri = new Uri(paletteFile, UriKind.RelativeOrAbsolute);
            }

            var paletteDict = new ResourceDictionary { Source = uri };

            var existing = Resources.MergedDictionaries.FirstOrDefault(d =>
                d.Source != null && (d.Source.OriginalString.Contains("LightTheme") || d.Source.OriginalString.Contains("DarkTheme")));

            if (existing != null)
            {
                var idx = Resources.MergedDictionaries.IndexOf(existing);
                Resources.MergedDictionaries[idx] = paletteDict;
            }
            else
            {
                Resources.MergedDictionaries.Add(paletteDict);
            }
        }
        catch (Exception ex)
        {
            RSBot.Core.Log.Error($"[GeneralView] SetTheme error: {ex.Message}");
        }
    }

    #endregion
}
