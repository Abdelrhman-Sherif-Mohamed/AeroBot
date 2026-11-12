using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Globalization;
using System.Threading.Tasks;
using System.Windows.Forms;
using RSBot.Core;
using RSBot.Core.Components;
using RSBot.Core.Security;
using SDUI;
using Button = SDUI.Controls.Button;
using GroupBox = SDUI.Controls.GroupBox;
using Label = SDUI.Controls.Label;
using Panel = SDUI.Controls.Panel;

namespace RSBot.Views.Controls;

/// <summary>
///     Account dashboard page displayed in the sidebar showing username,
///     subscription duration/expiry, hardware binding, and quick actions.
/// </summary>
[ToolboxItem(false)]
public class AccountPageControl : UserControl
{
    public event EventHandler AccountRefreshed;

    private readonly System.Windows.Forms.Panel _scrollContainer;

    // Profile header card
    private readonly GroupBox _grpProfile;
    private readonly PictureBox _picAvatar;
    private readonly Label _lblUsername;
    private readonly System.Windows.Forms.Label _lblStatusBadge;
    private readonly Label _lblRoleAndVersion;
    private readonly Button _btnCopyUsername;

    // Subscription card
    private readonly GroupBox _grpSubscription;
    private readonly Label _lblRemainingCaption;
    private readonly Label _lblRemainingValue;
    private readonly Label _lblExpiresCaption;
    private readonly Label _lblExpiresValue;
    private readonly System.Windows.Forms.Panel _healthTrack;
    private readonly System.Windows.Forms.Panel _healthFill;
    private readonly System.Windows.Forms.Label _lblAlertBanner;

    // Details card
    private readonly GroupBox _grpSecurity;
    private readonly Label _lblHwidCaption;
    private readonly System.Windows.Forms.Label _lblHwidValue;
    private readonly Button _btnCopyHwid;
    private readonly Label _lblCreatedCaption;
    private readonly Label _lblCreatedValue;
    private readonly Label _lblLastLoginCaption;
    private readonly Label _lblLastLoginValue;
    private readonly Label _lblLastIpCaption;
    private readonly Label _lblLastIpValue;
    private readonly Label _lblNotesCaption;
    private readonly Label _lblNotesValue;

    // Actions row
    private readonly FlowLayoutPanel _actionsPanel;
    private readonly Button _btnRefresh;
    private readonly Button _btnRenewWhatsApp;
    private readonly Button _btnCopySupport;
    private readonly Button _btnAdminPanel;
    private readonly Label _lblActionFeedback;

    private string _whatsAppNumber = "+201150238481";
    private string _whatsAppUrl = "https://wa.me/201150238481";
    private bool _applyingTheme;
    private bool _isRefreshing;

    public AccountPageControl()
    {
        Font = new Font("Segoe UI", 9.25f, FontStyle.Regular);
        Size = new Size(762, 600);
        AutoScroll = true;
        Padding = new Padding(16);

        _scrollContainer = new System.Windows.Forms.Panel
        {
            Size = new Size(730, 560),
            Dock = DockStyle.Top,
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink,
            Padding = new Padding(0, 0, 0, 12),
            BackColor = Color.Transparent
        };

        // 1. Profile header card
        _grpProfile = new GroupBox
        {
            Dock = DockStyle.Top,
            Height = 112,
            Radius = 10,
            ShadowDepth = 4,
            Padding = new Padding(16, 22, 16, 12)
        };

        _picAvatar = new PictureBox
        {
            Location = new Point(18, 28),
            Size = new Size(58, 58),
            BackColor = Color.Transparent
        };
        _picAvatar.Paint += PicAvatar_Paint;

        _lblUsername = new Label
        {
            Location = new Point(90, 26),
            Size = new Size(340, 28),
            AutoEllipsis = true,
            Font = new Font("Segoe UI", 14f, FontStyle.Bold)
        };

        _lblStatusBadge = new System.Windows.Forms.Label
        {
            Location = new Point(90, 58),
            AutoSize = true,
            Padding = new Padding(8, 3, 8, 3),
            Font = new Font("Segoe UI", 8.75f, FontStyle.Bold),
            ForeColor = Color.White,
            BackColor = Color.FromArgb(22, 163, 74)
        };

        _lblRoleAndVersion = new Label
        {
            Location = new Point(220, 61),
            Size = new Size(340, 20),
            AutoEllipsis = true
        };

        _btnCopyUsername = new Button
        {
            Anchor = AnchorStyles.Top | AnchorStyles.Right,
            Location = new Point(590, 38),
            Size = new Size(124, 32),
            Radius = 6,
            ShadowDepth = 2f,
            Color = Color.Transparent
        };
        _btnCopyUsername.Click += BtnCopyUsername_Click;

        _grpProfile.Size = new Size(730, 112);
        _grpProfile.Controls.AddRange(new Control[]
        {
            _picAvatar, _lblUsername, _lblStatusBadge, _lblRoleAndVersion, _btnCopyUsername
        });

        // 2. Subscription card
        _grpSubscription = new GroupBox
        {
            Dock = DockStyle.Top,
            Height = 152,
            Radius = 10,
            ShadowDepth = 4,
            Padding = new Padding(16, 26, 16, 12)
        };

        _lblRemainingCaption = new Label
        {
            Location = new Point(18, 32),
            Size = new Size(190, 22),
            Font = new Font("Segoe UI", 9.5f, FontStyle.Bold)
        };

        _lblRemainingValue = new Label
        {
            Location = new Point(214, 28),
            Size = new Size(495, 28),
            Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right,
            AutoEllipsis = true,
            Tag = "CustomFore",
            Font = new Font("Segoe UI", 12.5f, FontStyle.Bold)
        };

        _lblExpiresCaption = new Label
        {
            Location = new Point(18, 66),
            Size = new Size(190, 22)
        };

        _lblExpiresValue = new Label
        {
            Location = new Point(214, 66),
            Size = new Size(495, 22),
            Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right,
            AutoEllipsis = true
        };

        _healthTrack = new System.Windows.Forms.Panel
        {
            Location = new Point(18, 98),
            Size = new Size(692, 10),
            Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right,
            BackColor = Color.FromArgb(45, 52, 66)
        };

        _healthFill = new System.Windows.Forms.Panel
        {
            Dock = DockStyle.Left,
            Width = 692,
            BackColor = Color.FromArgb(22, 163, 74)
        };
        _healthTrack.Controls.Add(_healthFill);

        _lblAlertBanner = new System.Windows.Forms.Label
        {
            Location = new Point(18, 116),
            Size = new Size(692, 26),
            Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right,
            Font = new Font("Segoe UI", 9f, FontStyle.Bold),
            ForeColor = Color.FromArgb(239, 68, 68),
            BackColor = Color.Transparent,
            Visible = false
        };

        _grpSubscription.Size = new Size(730, 152);
        _grpSubscription.Controls.AddRange(new Control[]
        {
            _lblRemainingCaption, _lblRemainingValue,
            _lblExpiresCaption, _lblExpiresValue,
            _healthTrack, _lblAlertBanner
        });

        // 3. Security & Account details card
        _grpSecurity = new GroupBox
        {
            Dock = DockStyle.Top,
            Height = 192,
            Radius = 10,
            ShadowDepth = 4,
            Padding = new Padding(16, 26, 16, 12)
        };

        _lblHwidCaption = new Label { Location = new Point(18, 32), Size = new Size(190, 22) };
        _lblHwidValue = new System.Windows.Forms.Label
        {
            Location = new Point(214, 32),
            Size = new Size(390, 22),
            Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right,
            Font = new Font("Consolas", 9.5f, FontStyle.Regular),
            BackColor = Color.Transparent
        };
        _btnCopyHwid = new Button
        {
            Anchor = AnchorStyles.Top | AnchorStyles.Right,
            Location = new Point(612, 28),
            Size = new Size(98, 28),
            Radius = 6,
            ShadowDepth = 2f,
            Color = Color.Transparent
        };
        _btnCopyHwid.Click += BtnCopyHwid_Click;

        _lblCreatedCaption = new Label { Location = new Point(18, 64), Size = new Size(190, 22) };
        _lblCreatedValue = new Label
        {
            Location = new Point(214, 64),
            Size = new Size(496, 22),
            Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
        };

        _lblLastLoginCaption = new Label { Location = new Point(18, 94), Size = new Size(190, 22) };
        _lblLastLoginValue = new Label
        {
            Location = new Point(214, 94),
            Size = new Size(496, 22),
            Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
        };

        _lblLastIpCaption = new Label { Location = new Point(18, 124), Size = new Size(190, 22) };
        _lblLastIpValue = new Label
        {
            Location = new Point(214, 124),
            Size = new Size(496, 22),
            Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
        };

        _lblNotesCaption = new Label { Location = new Point(18, 154), Size = new Size(190, 22) };
        _lblNotesValue = new Label
        {
            Location = new Point(214, 154),
            Size = new Size(496, 22),
            Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right,
            AutoEllipsis = true
        };

        _grpSecurity.Size = new Size(730, 192);
        _grpSecurity.Controls.AddRange(new Control[]
        {
            _lblHwidCaption, _lblHwidValue, _btnCopyHwid,
            _lblCreatedCaption, _lblCreatedValue,
            _lblLastLoginCaption, _lblLastLoginValue,
            _lblLastIpCaption, _lblLastIpValue,
            _lblNotesCaption, _lblNotesValue
        });

        // 4. Actions bar
        _actionsPanel = new FlowLayoutPanel
        {
            Dock = DockStyle.Top,
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink,
            WrapContents = true,
            Padding = new Padding(0, 8, 0, 4),
            BackColor = Color.Transparent
        };

        _btnRefresh = new Button
        {
            Size = new Size(165, 36),
            Margin = new Padding(0, 0, 8, 8),
            Radius = 7,
            ShadowDepth = 3f,
            Color = Color.Transparent
        };
        _btnRefresh.Click += async (_, _) => await RefreshFromServerAsync();

        _btnRenewWhatsApp = new Button
        {
            Size = new Size(185, 36),
            Margin = new Padding(0, 0, 8, 8),
            Radius = 7,
            ShadowDepth = 3f,
            Color = Color.FromArgb(22, 163, 74),
            ForeColor = Color.White,
            Tag = "PreserveColor"
        };
        _btnRenewWhatsApp.Click += BtnRenewWhatsApp_Click;

        _btnCopySupport = new Button
        {
            Size = new Size(175, 36),
            Margin = new Padding(0, 0, 8, 8),
            Radius = 7,
            ShadowDepth = 3f,
            Color = Color.Transparent
        };
        _btnCopySupport.Click += BtnCopySupport_Click;

        _btnAdminPanel = new Button
        {
            Size = new Size(160, 36),
            Margin = new Padding(0, 0, 8, 8),
            Radius = 7,
            ShadowDepth = 3f,
            Color = Color.FromArgb(37, 99, 235),
            ForeColor = Color.White,
            Tag = "PreserveColor",
            Visible = false
        };
        _btnAdminPanel.Click += BtnAdminPanel_Click;

        _lblActionFeedback = new Label
        {
            AutoSize = true,
            Tag = "CustomFore",
            Padding = new Padding(4, 9, 4, 4)
        };

        _actionsPanel.Controls.AddRange(new Control[]
        {
            _btnRefresh, _btnRenewWhatsApp, _btnCopySupport, _btnAdminPanel, _lblActionFeedback
        });

        // Reverse add order because Dock = DockStyle.Top stacks last-added at the top.
        _scrollContainer.Controls.Add(_actionsPanel);
        _scrollContainer.Controls.Add(CreateSpacer(10));
        _scrollContainer.Controls.Add(_grpSecurity);
        _scrollContainer.Controls.Add(CreateSpacer(10));
        _scrollContainer.Controls.Add(_grpSubscription);
        _scrollContainer.Controls.Add(CreateSpacer(10));
        _scrollContainer.Controls.Add(_grpProfile);

        Controls.Add(_scrollContainer);

        ApplyLanguage();
        ApplyTheme();
        RefreshData();
    }

    private static bool IsArabic => string.Equals(Kernel.Language, "ar_AR", StringComparison.OrdinalIgnoreCase);

    public static string FormatRemaining(UserAccount account, bool arabic)
    {
        if (account == null)
            return arabic ? "غير مسجل الدخول" : "Not logged in";

        if (account.IsBlocked)
            return arabic ? "محظور (Blocked)" : "Blocked";

        if (account.IsLifetime)
            return arabic ? "اشتراك دائم (Lifetime)" : "Lifetime";

        if (account.IsAdmin)
            return arabic ? "حساب مسؤول (Admin)" : "Admin Account";

        if (account.IsFreeTier)
            return arabic ? "نسخة مجانية" : "Free Tier";

        if (account.IsExpired)
            return arabic ? "منتهي الصلاحية" : "Expired";

        var remaining = account.RemainingTime;
        if (remaining.HasValue && remaining.Value > TimeSpan.Zero)
        {
            var diff = remaining.Value;
            if (diff.TotalDays >= 1)
            {
                var days = (int)diff.TotalDays;
                return arabic ? $"{days} يوم و {diff.Hours} ساعة" : $"{days}d {diff.Hours}h";
            }

            if (diff.TotalHours >= 1)
            {
                var hours = (int)diff.TotalHours;
                return arabic ? $"{hours} ساعة و {diff.Minutes} دقيقة" : $"{hours}h {diff.Minutes}m";
            }

            var mins = Math.Max(1, (int)diff.TotalMinutes);
            return arabic ? $"{mins} دقيقة" : $"{mins} min";
        }

        return arabic ? "غير مفعل" : "Inactive";
    }

    public void ApplyLanguage()
    {
        if (IsDisposed || Disposing)
            return;

        if (InvokeRequired)
        {
            BeginInvoke(new Action(ApplyLanguage));
            return;
        }

        var ar = IsArabic;
        _grpProfile.Text = ar ? "ملف الحساب" : "Account Profile";
        _btnCopyUsername.Text = ar ? "نسخ الاسم" : "Copy Username";

        _grpSubscription.Text = ar ? "تفاصيل الاشتراك" : "Subscription Details";
        _lblRemainingCaption.Text = ar ? "مدة الاشتراك المتبقية:" : "Remaining Time:";
        _lblExpiresCaption.Text = ar ? "تاريخ الانتهاء:" : "Expiration Date:";

        _grpSecurity.Text = ar ? "بيانات الحساب والأمان" : "Account & Security";
        _lblHwidCaption.Text = ar ? "بصمة الجهاز (HWID):" : "Hardware ID (HWID):";
        _btnCopyHwid.Text = ar ? "نسخ" : "Copy";
        _lblCreatedCaption.Text = ar ? "تاريخ الإنشاء:" : "Created At:";
        _lblLastLoginCaption.Text = ar ? "آخر تسجيل دخول:" : "Last Login:";
        _lblLastIpCaption.Text = ar ? "آخر عنوان IP:" : "Last Login IP:";
        _lblNotesCaption.Text = ar ? "ملاحظات الحساب:" : "Account Notes:";

        _btnRefresh.Text = _isRefreshing
            ? (ar ? "جارٍ التحديث..." : "Refreshing...")
            : (ar ? "تحديث من السيرفر" : "Refresh from Server");
        _btnRenewWhatsApp.Text = ar ? "تجديد الاشتراك (واتساب)" : "Renew Subscription (WhatsApp)";
        _btnCopySupport.Text = ar ? "نسخ بيانات الدعم" : "Copy Support Info";
        _btnAdminPanel.Text = ar ? "لوحة الإدارة (Admin)" : "Admin Panel";

        RefreshData();
    }

    public void ApplyTheme()
    {
        if (_applyingTheme || IsDisposed || Disposing)
            return;

        _applyingTheme = true;
        try
        {
            var isDark = ColorScheme.BackColor.IsDark();
            var bg = ColorScheme.BackColor;
            var fg = isDark ? ColorScheme.ForeColor : Color.FromArgb(24, 28, 36);
            var secondaryFg = isDark ? ColorScheme.SecondaryForeColor : Color.FromArgb(95, 104, 118);

            BackColor = bg;
            _scrollContainer.BackColor = bg;

            foreach (var lbl in new Label[]
                     {
                         _lblUsername, _lblRemainingCaption, _lblRemainingValue,
                         _lblExpiresValue, _lblCreatedValue, _lblLastLoginValue,
                         _lblLastIpValue, _lblNotesValue, _lblActionFeedback
                     })
                lbl.ForeColor = fg;

            foreach (var lbl in new Label[]
                     {
                         _lblRoleAndVersion, _lblExpiresCaption, _lblHwidCaption,
                         _lblCreatedCaption, _lblLastLoginCaption, _lblLastIpCaption, _lblNotesCaption
                     })
                lbl.ForeColor = secondaryFg;

            _lblHwidValue.ForeColor = fg;
            _healthTrack.BackColor = isDark ? Color.FromArgb(40, 46, 58) : Color.FromArgb(224, 229, 236);

            // Preserve intentional colored highlight on remaining time & buttons
            RefreshData();
            _picAvatar.Invalidate();
        }
        finally
        {
            _applyingTheme = false;
        }
    }

    public void RefreshData()
    {
        if (IsDisposed || Disposing)
            return;

        var ar = IsArabic;
        var user = AuthService.Instance.CurrentUser;
        var isAdmin = AuthService.Instance.IsAdmin;
        var localHwid = HardwareId.GetHwid();

        _btnAdminPanel.Visible = isAdmin;
        _lblStatusBadge.ForeColor = Color.White;
        _lblAlertBanner.ForeColor = Color.FromArgb(239, 68, 68);

        if (user == null)
        {
            _lblUsername.Text = ar ? "غير مسجل" : "Not Logged In";
            _lblStatusBadge.Text = ar ? "غير متصل" : "Offline";
            _lblStatusBadge.BackColor = Color.FromArgb(107, 114, 128);
            _lblRoleAndVersion.Left = _lblStatusBadge.Right + 12;
            _lblRoleAndVersion.Text = $"Aerobot v{UpdateManager.DisplayVersion}";

            _lblRemainingValue.Text = ar ? "لا يوجد اشتراك نشط" : "No active subscription";
            _lblRemainingValue.ForeColor = Color.FromArgb(239, 68, 68);
            _lblExpiresValue.Text = "-";
            _lblAlertBanner.Visible = false;
            UpdateHealthBar(0, Color.FromArgb(107, 114, 128));

            _lblHwidValue.Text = localHwid;
            _lblCreatedValue.Text = "-";
            _lblLastLoginValue.Text = "-";
            _lblLastIpValue.Text = "-";
            _lblNotesValue.Text = "-";
            return;
        }

        _lblUsername.Text = user.Username;
        _lblRoleAndVersion.Text = ar
            ? $"{(isAdmin ? "مسؤول النظام" : "حساب مستخدم")}  •  الإصدار v{UpdateManager.DisplayVersion}"
            : $"{(isAdmin ? "Administrator" : "Standard User")}  •  v{UpdateManager.DisplayVersion}";

        if (user.IsBlocked)
        {
            _lblStatusBadge.Text = ar ? "محظور" : "Blocked";
            _lblStatusBadge.BackColor = Color.FromArgb(220, 38, 38);
            _lblRemainingValue.ForeColor = Color.FromArgb(239, 68, 68);
            UpdateHealthBar(0, Color.FromArgb(220, 38, 38));
            _lblAlertBanner.Text = string.IsNullOrWhiteSpace(user.BlockedReason)
                ? (ar ? "هذا الحساب محظور. يرجى التواصل مع الإدارة." : "This account is blocked. Contact support.")
                : user.BlockedReason.Replace("\r", " ").Replace("\n", " ");
            _lblAlertBanner.Visible = true;
        }
        else if (user.IsLifetime || user.IsAdmin)
        {
            _lblStatusBadge.Text = user.IsAdmin ? (ar ? "أدمن" : "Admin") : (ar ? "دائم" : "Lifetime");
            _lblStatusBadge.BackColor = Color.FromArgb(37, 99, 235);
            _lblRemainingValue.ForeColor = Color.FromArgb(34, 197, 94);
            UpdateHealthBar(100, Color.FromArgb(34, 197, 94));
            _lblAlertBanner.Visible = false;
        }
        else if (user.IsExpired)
        {
            _lblStatusBadge.Text = ar ? "منتهي" : "Expired";
            _lblStatusBadge.BackColor = Color.FromArgb(220, 38, 38);
            _lblRemainingValue.ForeColor = Color.FromArgb(239, 68, 68);
            UpdateHealthBar(0, Color.FromArgb(220, 38, 38));
            _lblAlertBanner.Text = ar
                ? "انتهت صلاحية الاشتراك — اضغط على زر «تجديد الاشتراك (واتساب)» للتجديد."
                : "Subscription expired — click Renew Subscription (WhatsApp) to extend.";
            _lblAlertBanner.Visible = true;
        }
        else
        {
            var rem = user.RemainingTime ?? TimeSpan.Zero;
            var isLow = rem.TotalDays < 3;
            _lblStatusBadge.Text = isLow ? (ar ? "قرب الانتهاء" : "Expiring Soon") : (ar ? "نشط" : "Active");
            _lblStatusBadge.BackColor = isLow ? Color.FromArgb(217, 119, 6) : Color.FromArgb(22, 163, 74);
            _lblRemainingValue.ForeColor = isLow ? Color.FromArgb(245, 158, 11) : Color.FromArgb(34, 197, 94);

            var pct = (int)Math.Max(6, Math.Min(100, rem.TotalDays / 30.0 * 100.0));
            UpdateHealthBar(pct, _lblStatusBadge.BackColor);
            _lblAlertBanner.Visible = false;
        }

        _lblRemainingValue.Text = FormatRemaining(user, ar);

        if (user.IsLifetime)
        {
            _lblExpiresValue.Text = ar ? "دائم (بدون تاريخ انتهاء)" : "Lifetime (Never Expires)";
        }
        else if (user.ExpirationDate.HasValue)
        {
            var localExp = user.ExpirationDate.Value.ToLocalTime();
            _lblExpiresValue.Text = $"{localExp:yyyy-MM-dd HH:mm}";
        }
        else
        {
            _lblExpiresValue.Text = user.IsAdmin
                ? (ar ? "غير محدود (حساب مسؤول)" : "Unlimited (Admin)")
                : (ar ? "غير محدد" : "Not set");
        }

        _lblHwidValue.Text = string.IsNullOrWhiteSpace(user.HWID) ? localHwid : user.HWID;
        _lblCreatedValue.Text = FormatTimestamp(user.CreatedAt);
        _lblLastLoginValue.Text = FormatTimestamp(user.LastLoginAt);
        _lblLastIpValue.Text = string.IsNullOrWhiteSpace(user.LastLoginIP) ? "-" : user.LastLoginIP;
        _lblNotesValue.Text = string.IsNullOrWhiteSpace(user.Notes) ? "-" : user.Notes;
        _lblRoleAndVersion.Left = _lblStatusBadge.Right + 12;
        _picAvatar.Invalidate();
    }

    public async Task RefreshFromServerAsync()
    {
        if (_isRefreshing || IsDisposed)
            return;

        _isRefreshing = true;
        var ar = IsArabic;
        _btnRefresh.Enabled = false;
        _btnRefresh.Text = ar ? "جارٍ التحديث..." : "Refreshing...";
        _lblActionFeedback.Text = string.Empty;

        try
        {
            var username = AuthService.Instance.CurrentUser?.Username;
            if (!string.IsNullOrWhiteSpace(username))
            {
                await AuthService.Instance.CheckCurrentSubscriptionAsync();

                var settings = await AuthService.Instance.GetSettingsAsync();
                if (settings != null)
                {
                    if (!string.IsNullOrWhiteSpace(settings.WhatsAppNumber))
                        _whatsAppNumber = settings.WhatsAppNumber;
                    if (!string.IsNullOrWhiteSpace(settings.WhatsAppUrl))
                        _whatsAppUrl = settings.WhatsAppUrl;
                    else
                        _whatsAppUrl = $"https://wa.me/{_whatsAppNumber.Replace("+", "").Replace(" ", "")}";
                }
            }

            RefreshData();
            AccountRefreshed?.Invoke(this, EventArgs.Empty);
            ShowFeedback(ar ? "تم تحديث بيانات الحساب والاشتراك ✓" : "Account & subscription refreshed ✓",
                Color.FromArgb(34, 197, 94));
        }
        catch (Exception ex)
        {
            ShowFeedback(ar ? $"فشل التحديث: {ex.Message}" : $"Refresh failed: {ex.Message}",
                Color.FromArgb(239, 68, 68));
        }
        finally
        {
            _isRefreshing = false;
            if (!IsDisposed)
            {
                _btnRefresh.Enabled = true;
                _btnRefresh.Text = IsArabic ? "تحديث من السيرفر" : "Refresh from Server";
            }
        }
    }

    protected override void OnVisibleChanged(EventArgs e)
    {
        base.OnVisibleChanged(e);
        if (Visible)
        {
            ApplyLanguage();
            ApplyTheme();
        }
    }

    protected override void OnBackColorChanged(EventArgs e)
    {
        base.OnBackColorChanged(e);
        if (!_applyingTheme && IsHandleCreated)
            ApplyTheme();
    }

    protected override void OnSizeChanged(EventArgs e)
    {
        base.OnSizeChanged(e);
        if (_healthTrack != null && _healthFill != null)
            RefreshHealthWidth();
    }

    private int _lastHealthPercent = 100;

    private void UpdateHealthBar(int percent, Color fillColor)
    {
        _lastHealthPercent = Math.Max(0, Math.Min(100, percent));
        _healthFill.BackColor = fillColor;
        RefreshHealthWidth();
    }

    private void RefreshHealthWidth()
    {
        var trackWidth = Math.Max(0, _healthTrack.Width);
        _healthFill.Width = (int)(trackWidth * (_lastHealthPercent / 100.0));
    }

    private static string FormatTimestamp(string rawUtc)
    {
        if (string.IsNullOrWhiteSpace(rawUtc))
            return "-";

        if (DateTime.TryParse(rawUtc, CultureInfo.InvariantCulture,
                DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal, out var dt))
            return dt.ToLocalTime().ToString("yyyy-MM-dd HH:mm");

        return rawUtc;
    }

    private void PicAvatar_Paint(object sender, PaintEventArgs e)
    {
        e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
        var rect = new Rectangle(2, 2, _picAvatar.Width - 4, _picAvatar.Height - 4);
        using var bgBrush = new SolidBrush(ColorScheme.AccentColor);
        e.Graphics.FillEllipse(bgBrush, rect);

        var username = AuthService.Instance.CurrentUser?.Username;
        var initial = !string.IsNullOrWhiteSpace(username)
            ? char.ToUpperInvariant(username.Trim()[0]).ToString()
            : "A";

        using var font = new Font("Segoe UI", 18f, FontStyle.Bold);
        using var sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
        e.Graphics.DrawString(initial, font, Brushes.White, rect, sf);
    }

    private void BtnCopyUsername_Click(object sender, EventArgs e)
    {
        var username = AuthService.Instance.CurrentUser?.Username;
        if (string.IsNullOrWhiteSpace(username))
            return;

        CopyText(username, IsArabic ? "تم نسخ اسم المستخدم ✓" : "Username copied ✓");
    }

    private void BtnCopyHwid_Click(object sender, EventArgs e)
    {
        var hwid = _lblHwidValue.Text;
        if (!string.IsNullOrWhiteSpace(hwid))
            CopyText(hwid, IsArabic ? "تم نسخ بصمة الجهاز ✓" : "HWID copied ✓");
    }

    private void BtnCopySupport_Click(object sender, EventArgs e)
    {
        var user = AuthService.Instance.CurrentUser;
        var username = user?.Username ?? "-";
        var hwid = string.IsNullOrWhiteSpace(user?.HWID) ? HardwareId.GetHwid() : user.HWID;
        var sub = FormatRemaining(user, IsArabic);

        var summary = $"Username: {username}\r\nHWID: {hwid}\r\nSubscription: {sub}";
        CopyText(summary, IsArabic ? "تم نسخ بيانات الدعم ✓" : "Support info copied ✓");
    }

    private async void BtnRenewWhatsApp_Click(object sender, EventArgs e)
    {
        try
        {
            var settings = await AuthService.Instance.GetSettingsAsync();
            if (settings != null && !string.IsNullOrWhiteSpace(settings.WhatsAppUrl))
                _whatsAppUrl = settings.WhatsAppUrl;

            Process.Start(new ProcessStartInfo
            {
                FileName = _whatsAppUrl,
                UseShellExecute = true
            });
        }
        catch (Exception ex)
        {
            MessageBox.Show(this, $"WhatsApp: {_whatsAppNumber}\n\n{ex.Message}", "WhatsApp",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
    }

    private async void BtnAdminPanel_Click(object sender, EventArgs e)
    {
        if (!AuthService.Instance.IsAdmin)
            return;

        using var adminForm = new AdminForm();
        adminForm.ShowDialog(FindForm());
        await RefreshFromServerAsync();
    }

    private void CopyText(string text, string feedback)
    {
        try
        {
            Clipboard.SetText(text);
            ShowFeedback(feedback, Color.FromArgb(34, 197, 94));
        }
        catch
        {
        }
    }

    private void ShowFeedback(string text, Color color)
    {
        _lblActionFeedback.Text = text;
        _lblActionFeedback.ForeColor = color;
    }

    private static System.Windows.Forms.Panel CreateSpacer(int height)
    {
        return new System.Windows.Forms.Panel
        {
            Dock = DockStyle.Top,
            Height = height,
            BackColor = Color.Transparent
        };
    }
}
