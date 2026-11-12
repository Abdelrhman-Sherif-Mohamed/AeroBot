using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using RSBot.Core;
using RSBot.Core.Client;
using RSBot.Core.Event;
using RSBot.General.Components;
using RSBot.ServerInfo.Components;
using SDUI;
using SDUI.Controls;

namespace RSBot.ServerInfo.Views;

[ToolboxItem(false)]
public partial class Main : DoubleBufferedControl
{
    private readonly Action _serverListUpdatedHandler;
    private readonly Action _initializedHandler;
    private readonly Action _agentConnectedHandler;
    private readonly Action<DivisionInfo> _divisionLoadedHandler;
    private readonly Action<bool> _themeChangedHandler;

    private bool _eventsSubscribed;
    private bool _isRefreshing;
    private DateTime? _lastUpdatedAt;
    private string _customStatusText;
    private bool _autoRefreshAttempted;

    public Main()
    {
        InitializeComponent();

        _serverListUpdatedHandler = OnServerListUpdated;
        _initializedHandler = OnCoreInitialized;
        _agentConnectedHandler = OnAgentConnected;
        _divisionLoadedHandler = OnDivisionLoaded;
        _themeChangedHandler = OnThemeChanged;

        SubscribeEvents();
        ApplyTheme();
        ApplyLocalization();
        UpdateServerInfo();
    }

    private void SubscribeEvents()
    {
        if (_eventsSubscribed)
            return;

        _eventsSubscribed = true;
        EventManager.SubscribeEvent("OnServerListUpdated", _serverListUpdatedHandler);
        EventManager.SubscribeEvent("OnInitialized", _initializedHandler);
        EventManager.SubscribeEvent("OnAgentServerConnected", _agentConnectedHandler);
        EventManager.SubscribeEvent("OnLoadDivisionInfo", _divisionLoadedHandler);
        EventManager.SubscribeEvent("OnChangeTheme", _themeChangedHandler);
    }

    private void UnsubscribeEvents()
    {
        if (!_eventsSubscribed)
            return;

        _eventsSubscribed = false;
        EventManager.UnsubscribeEvent("OnServerListUpdated", _serverListUpdatedHandler);
        EventManager.UnsubscribeEvent("OnInitialized", _initializedHandler);
        EventManager.UnsubscribeEvent("OnAgentServerConnected", _agentConnectedHandler);
        EventManager.UnsubscribeEvent("OnLoadDivisionInfo", _divisionLoadedHandler);
        EventManager.UnsubscribeEvent("OnChangeTheme", _themeChangedHandler);
    }

    protected override void OnHandleCreated(EventArgs e)
    {
        base.OnHandleCreated(e);
        ApplyTheme();
        ApplyLocalization();
        UpdateServerInfo();
        ResizeColumns();
    }

    protected override void OnVisibleChanged(EventArgs e)
    {
        base.OnVisibleChanged(e);
        if (!Visible || IsDisposed)
            return;

        ApplyTheme();
        ApplyLocalization();
        UpdateServerInfo();
        ResizeColumns();

        if (!_autoRefreshAttempted && (Serverlist.Servers == null || Serverlist.Servers.Count == 0))
        {
            _autoRefreshAttempted = true;
            _ = RefreshServersAsync(silentWhenNotReady: true);
        }
    }

    protected override void OnResize(EventArgs e)
    {
        base.OnResize(e);
        ResizeColumns();
    }

    private void RunOnUi(Action action)
    {
        if (IsDisposed || Disposing || !IsHandleCreated)
            return;

        if (InvokeRequired)
        {
            try
            {
                BeginInvoke(action);
            }
            catch (ObjectDisposedException)
            {
            }
            catch (InvalidOperationException)
            {
            }
        }
        else
        {
            action();
        }
    }

    private void OnServerListUpdated()
    {
        _lastUpdatedAt = DateTime.Now;
        _customStatusText = null;
        RunOnUi(UpdateServerInfo);
    }

    private void OnCoreInitialized()
    {
        RunOnUi(() =>
        {
            ApplyLocalization();
            UpdateServerInfo();
        });
    }

    private void OnAgentConnected()
    {
        RunOnUi(UpdateServerInfo);
    }

    private void OnDivisionLoaded(DivisionInfo divisionInfo)
    {
        if (Serverlist.Servers == null || Serverlist.Servers.Count == 0)
            _ = RefreshServersAsync(silentWhenNotReady: true);
    }

    private void OnThemeChanged(bool _)
    {
        RunOnUi(ApplyTheme);
    }

    public void ApplyTheme()
    {
        var back = ColorScheme.BackColor;
        var card = ColorScheme.CardColor;
        var fore = ColorScheme.ForeColor;
        var accent = ColorScheme.AccentColor;

        BackColor = back;
        ForeColor = fore;

        _headerPanel.BackColor = card;
        _lblStatus.BackColor = card;
        _lblStatus.ForeColor = fore;

        _btnRefresh.BackColor = accent;
        _btnRefresh.ForeColor = Color.White;
        _btnRefresh.Invalidate();

        lvServerInfo.BackColor = back;
        lvServerInfo.ForeColor = fore;
        lvServerInfo.Invalidate();
    }

    public void ApplyLocalization()
    {
        var isArabic = IsArabicLanguage();

        colServer.Text = isArabic ? "السيرفر" : "Server";
        colCapacity.Text = isArabic ? "السعة / اللاعبون" : "Capacity";
        colStatus.Text = isArabic ? "الحالة" : "Status";

        _lblStatus.TextAlign = isArabic ? ContentAlignment.MiddleRight : ContentAlignment.MiddleLeft;
        _btnRefresh.Text = _isRefreshing
            ? (isArabic ? "جاري التحديث..." : "Refreshing...")
            : (isArabic ? "تحديث" : "Refresh");

        UpdateStatusLabel();
        UpdateServerInfo();
    }

    private async void OnRefreshClicked(object sender, EventArgs e)
    {
        await RefreshServersAsync(silentWhenNotReady: false);
    }

    private async System.Threading.Tasks.Task RefreshServersAsync(bool silentWhenNotReady)
    {
        if (_isRefreshing || IsDisposed)
            return;

        _isRefreshing = true;
        var isArabic = IsArabicLanguage();
        _customStatusText = isArabic ? "جاري جلب معلومات السيرفر..." : "Querying gateway server...";

        RunOnUi(() =>
        {
            _btnRefresh.Enabled = false;
            _btnRefresh.Text = isArabic ? "جاري التحديث..." : "Refreshing...";
            UpdateStatusLabel();
        });

        try
        {
            var result = await GatewayQuery.QueryServerListAsync();
            if (IsDisposed)
                return;

            isArabic = IsArabicLanguage();
            switch (result.Status)
            {
                case GatewayQueryStatus.Success:
                    _lastUpdatedAt = DateTime.Now;
                    _customStatusText = null;
                    break;
                case GatewayQueryStatus.RequestedThroughProxy:
                    _customStatusText = isArabic ? "تم إرسال طلب التحديث للسيرفر..." : "Server list request sent...";
                    break;
                case GatewayQueryStatus.ArchiveNotReady:
                    _customStatusText = silentWhenNotReady
                        ? null
                        : (isArabic ? "يرجى تحديد مسار اللعبة أولاً" : "Please configure Silkroad directory first");
                    break;
                case GatewayQueryStatus.InvalidGateway:
                    _customStatusText = isArabic ? "بيانات البوابة غير صالحة" : "Invalid gateway configuration";
                    break;
                case GatewayQueryStatus.PatchRequired:
                    _customStatusText = isArabic ? "إصدار اللعبة غير مطابق أو السيرفر قيد الصيانة" : "Version mismatch or server under check";
                    break;
                case GatewayQueryStatus.Timeout:
                    _customStatusText = isArabic ? "انتهت مهلة الاتصال بالسيرفر" : "Gateway query timed out";
                    break;
                default:
                    _customStatusText = string.IsNullOrWhiteSpace(result.Detail)
                        ? (isArabic ? "تعذر الاتصال بالسيرفر" : "Could not reach gateway server")
                        : result.Detail;
                    break;
            }
        }
        finally
        {
            _isRefreshing = false;
            RunOnUi(() =>
            {
                var arabic = IsArabicLanguage();
                _btnRefresh.Enabled = true;
                _btnRefresh.Text = arabic ? "تحديث" : "Refresh";
                UpdateServerInfo();
            });
        }
    }

    private void UpdateServerInfo()
    {
        if (IsDisposed)
            return;

        var isArabic = IsArabicLanguage();
        var fore = ColorScheme.ForeColor;
        var isDark = ColorScheme.BackColor.IsDark();
        var onlineColor = isDark ? Color.FromArgb(74, 222, 128) : Color.FromArgb(22, 163, 74);
        var offlineColor = isDark ? Color.FromArgb(248, 113, 113) : Color.FromArgb(220, 38, 38);

        lvServerInfo.BeginUpdate();
        try
        {
            lvServerInfo.Items.Clear();

            var servers = Serverlist.Servers;
            if (servers != null && servers.Count > 0)
            {
                foreach (var server in servers)
                {
                    var isJoining = Serverlist.Joining != null &&
                                    (Serverlist.Joining.Id == server.Id ||
                                     string.Equals(Serverlist.Joining.Name, server.Name, StringComparison.OrdinalIgnoreCase));

                    var displayName = isJoining
                        ? $"{server.Name} ({(isArabic ? "الحالي" : "Active")})"
                        : server.Name;

                    var statusText = server.Status
                        ? (isArabic ? "متصل" : "Online")
                        : (isArabic ? "مغلق / فحص" : "Offline / Check");

                    var item = new ListViewItem(new[]
                    {
                        displayName,
                        server.State ?? "-",
                        statusText
                    })
                    {
                        UseItemStyleForSubItems = false,
                        ForeColor = isJoining ? ColorScheme.AccentColor : fore
                    };

                    item.SubItems[1].ForeColor = fore;
                    item.SubItems[2].ForeColor = server.Status ? onlineColor : offlineColor;
                    lvServerInfo.Items.Add(item);
                }
            }
        }
        finally
        {
            lvServerInfo.EndUpdate();
        }

        UpdateStatusLabel();
    }

    private void UpdateStatusLabel()
    {
        var isArabic = IsArabicLanguage();

        if (!string.IsNullOrEmpty(_customStatusText))
        {
            _lblStatus.Text = _customStatusText;
            return;
        }

        var count = Serverlist.Servers?.Count ?? 0;
        if (count > 0)
        {
            var timeText = _lastUpdatedAt?.ToString("HH:mm:ss") ?? DateTime.Now.ToString("HH:mm:ss");
            _lblStatus.Text = isArabic
                ? $"عدد السيرفرات: {count}  •  آخر تحديث: {timeText}"
                : $"Servers: {count}  •  Updated: {timeText}";
            return;
        }

        _lblStatus.Text = isArabic
            ? "لا توجد بيانات بعد — اضغط تحديث لجلب حالة السيرفر"
            : "No server data yet — click Refresh to query gateway";
    }

    private void ResizeColumns()
    {
        if (lvServerInfo.ClientSize.Width <= 0 || lvServerInfo.Columns.Count < 3)
            return;

        var total = Math.Max(240, lvServerInfo.ClientSize.Width - 6);
        colServer.Width = (int)(total * 0.42);
        colCapacity.Width = (int)(total * 0.30);
        colStatus.Width = Math.Max(80, total - colServer.Width - colCapacity.Width);
    }

    private static bool IsArabicLanguage()
    {
        return Kernel.Language != null && Kernel.Language.StartsWith("ar", StringComparison.OrdinalIgnoreCase);
    }
}
