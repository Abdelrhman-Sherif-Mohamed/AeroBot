using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using RSBot.Core;
using SDUI;
using SDUI.Controls;
using CheckBox = SDUI.Controls.CheckBox;

namespace Wolves.Notify.Views;

[ToolboxItem(false)]
public partial class Main : DoubleBufferedControl
{
    private static Main _instance;
    public static Main Instance => _instance ??= new Main();

    private readonly Dictionary<string, CheckBox> _eventChecks = new(StringComparer.OrdinalIgnoreCase);

    public Main()
    {
        CheckForIllegalCrossThreadCalls = false;
        InitializeComponent();
        BuildEventCheckboxes();
        ApplyTheme();

        NotifyConfig.Load();
        LoadConfigToControls();

        NotifySender.OnHistoryAdded += OnHistoryAdded;
        Translate();
    }

    private bool IsArabic =>
        Kernel.Language != null && Kernel.Language.StartsWith("ar", StringComparison.OrdinalIgnoreCase);

    private void BuildEventCheckboxes()
    {
        tlpEvents.SuspendLayout();
        tlpEvents.Controls.Clear();
        _eventChecks.Clear();

        for (var i = 0; i < NotifyEvents.All.Count; i++)
        {
            var meta = NotifyEvents.All[i];
            var chk = new CheckBox
            {
                AutoSize = true,
                Dock = DockStyle.Fill,
                Margin = new Padding(4, 2, 4, 2),
                Tag = meta.Key,
                Text = meta.GetLocalizedTitle()
            };
            _eventChecks[meta.Key] = chk;
            tlpEvents.Controls.Add(chk, i % 4, i / 4);
        }

        tlpEvents.ResumeLayout();
    }

    public void Translate()
    {
        var ar = IsArabic;
        RightToLeft = ar ? RightToLeft.Yes : RightToLeft.No;

        grpChannels.Text = ar
            ? "قنوات الإشعارات والربط (Telegram / Discord)"
            : "Channels & Credentials (Telegram / Discord)";
        grpEvents.Text = ar
            ? "الأحداث التي يتم إرسال إشعار بها"
            : "Alert Events";
        grpHistory.Text = ar
            ? "سجل الإشعارات المرسلة"
            : "Recent Notifications Log";

        chkMasterEnabled.Text = ar ? "تفعيل الإشعارات" : "Enable Notifications";
        chkRichCards.Text = ar ? "بطاقات منسقة (HTML / Embed)" : "Rich formatted cards";
        lblCooldown.Text = ar ? "فترة الانتظار (ث):" : "Cooldown (s):";
        btnSave.Text = ar ? "حفظ الإعدادات" : "Save Settings";

        chkTelegramEnabled.Text = "Telegram";
        lblTelegramToken.Text = ar ? "توكن البوت:" : "Bot Token:";
        lblTelegramChatId.Text = "Chat ID:";
        btnTestTelegram.Text = ar ? "اختبار تليجرام" : "Test Telegram";

        chkDiscordEnabled.Text = "Discord";
        lblDiscordWebhook.Text = "Webhook:";
        btnTestDiscord.Text = ar ? "اختبار ديسكورد" : "Test Discord";

        lblUniqueKeywords.Text = ar ? "كلمات اليونيك:" : "Unique Keywords:";
        btnSelectCoreEvents.Text = ar ? "الأحداث الأساسية" : "Core Alerts Only";
        btnSelectAllEvents.Text = ar ? "تحديد الكل" : "Enable All Events";
        btnClearHistory.Text = ar ? "مسح السجل" : "Clear Log";

        foreach (var meta in NotifyEvents.All)
        {
            if (_eventChecks.TryGetValue(meta.Key, out var chk))
                chk.Text = $"{meta.Emoji} {meta.GetLocalizedTitle()}";
        }

        ApplyTheme();
    }

    private void ApplyTheme()
    {
        txtHistory.BackColor = ColorScheme.BackColor;
        txtHistory.ForeColor = ColorScheme.ForeColor;
    }

    private void LoadConfigToControls()
    {
        var cfg = NotifyConfig.Current;
        chkMasterEnabled.Checked = cfg.Enabled;
        chkRichCards.Checked = cfg.RichCards;
        numCooldown.Value = Math.Clamp(cfg.CooldownSeconds, 0, 3600);

        chkTelegramEnabled.Checked = cfg.TelegramEnabled;
        txtTelegramToken.Text = NotifyConfig.MaskSecret(cfg.TelegramToken);
        txtTelegramChatId.Text = cfg.TelegramChatId ?? string.Empty;

        chkDiscordEnabled.Checked = cfg.DiscordEnabled;
        txtDiscordWebhook.Text = NotifyConfig.MaskSecret(cfg.DiscordWebhook);

        txtUniqueKeywords.Text = cfg.UniqueKeywords ?? NotifyConfig.DefaultUniqueKeywords;

        foreach (var pair in _eventChecks)
        {
            pair.Value.Checked = cfg.Events != null &&
                                 cfg.Events.TryGetValue(pair.Key, out var enabled) &&
                                 enabled;
        }
    }

    private NotifyConfig CollectConfigFromControls()
    {
        var existing = NotifyConfig.Current;
        var next = new NotifyConfig
        {
            Enabled = chkMasterEnabled.Checked,
            RichCards = chkRichCards.Checked,
            CooldownSeconds = (int)numCooldown.Value,
            TelegramEnabled = chkTelegramEnabled.Checked,
            TelegramToken = NotifyConfig.ResolveSecret(txtTelegramToken.Text, existing.TelegramToken),
            TelegramChatId = (txtTelegramChatId.Text ?? string.Empty).Trim(),
            DiscordEnabled = chkDiscordEnabled.Checked,
            DiscordWebhook = NotifyConfig.ResolveSecret(txtDiscordWebhook.Text, existing.DiscordWebhook),
            UniqueKeywords = string.IsNullOrWhiteSpace(txtUniqueKeywords.Text)
                ? NotifyConfig.DefaultUniqueKeywords
                : txtUniqueKeywords.Text.Trim(),
            Events = new Dictionary<string, bool>(StringComparer.OrdinalIgnoreCase)
        };

        foreach (var pair in _eventChecks)
            next.Events[pair.Key] = pair.Value.Checked;

        return next;
    }

    private void SetStatus(string text, bool isError = false)
    {
        if (IsDisposed || !IsHandleCreated)
            return;

        if (InvokeRequired)
        {
            BeginInvoke(() => SetStatus(text, isError));
            return;
        }

        lblStatus.Text = text;
        lblStatus.ForeColor = isError ? Color.IndianRed : ColorScheme.ForeColor;
    }

    private void OnHistoryAdded(string line)
    {
        if (IsDisposed || !IsHandleCreated)
            return;

        if (InvokeRequired)
        {
            BeginInvoke(() => OnHistoryAdded(line));
            return;
        }

        if (txtHistory.Lines.Length > 200)
            txtHistory.Clear();

        txtHistory.AppendText(line + Environment.NewLine);
        txtHistory.ScrollToCaret();
    }

    private void btnSave_Click(object sender, EventArgs e)
    {
        var cfg = CollectConfigFromControls();
        NotifyConfig.Save(cfg);

        txtTelegramToken.Text = NotifyConfig.MaskSecret(cfg.TelegramToken);
        txtDiscordWebhook.Text = NotifyConfig.MaskSecret(cfg.DiscordWebhook);

        SetStatus(IsArabic ? "الحالة: تم حفظ الإعدادات بنجاح." : "Status: Settings saved.");
    }

    private async void btnTestTelegram_Click(object sender, EventArgs e)
    {
        var cfg = CollectConfigFromControls();
        cfg.TelegramEnabled = true;
        cfg.DiscordEnabled = false;
        await RunChannelTestAsync(cfg);
    }

    private async void btnTestDiscord_Click(object sender, EventArgs e)
    {
        var cfg = CollectConfigFromControls();
        cfg.TelegramEnabled = false;
        cfg.DiscordEnabled = true;
        await RunChannelTestAsync(cfg);
    }

    private async System.Threading.Tasks.Task RunChannelTestAsync(NotifyConfig testConfig)
    {
        SetStatus(IsArabic ? "الحالة: جاري إرسال رسالة تجريبية..." : "Status: Sending test message...");
        var charName = string.IsNullOrWhiteSpace(Game.Player?.Name) ? "WolvesBot" : Game.Player.Name;

        var payload = new NotifyPayload(
            NotifyEvents.Unique,
            charName,
            "WolvesBot Notify test alert — connection working!",
            new[]
            {
                new NotifyField("Status", "Connected"),
                new NotifyField("Time", DateTime.Now.ToString("HH:mm:ss"))
            });

        var (success, summary) = await NotifySender.SendDirectTestAsync(testConfig, payload);
        SetStatus($"Status: {summary}", !success);
    }

    private void btnSelectCoreEvents_Click(object sender, EventArgs e)
    {
        var defaults = NotifyConfig.CreateDefaultEvents();
        foreach (var pair in _eventChecks)
            pair.Value.Checked = defaults.TryGetValue(pair.Key, out var value) && value;
    }

    private void btnSelectAllEvents_Click(object sender, EventArgs e)
    {
        foreach (var pair in _eventChecks)
            pair.Value.Checked = true;
    }

    private void btnClearHistory_Click(object sender, EventArgs e)
    {
        txtHistory.Clear();
    }
}
