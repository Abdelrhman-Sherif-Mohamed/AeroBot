using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;
using RSBot.Core.Components;
using RSBot.Core.Security;

namespace RSBot.Views;

public partial class AdminForm : Form
{
    private readonly AuthService _authService = AuthService.Instance;
    private readonly System.Windows.Forms.Timer _refreshTimer = new();
    private bool _isRefreshing = false;

    public AdminForm()
    {
        InitializeComponent();

        _refreshTimer.Interval = 10000; // 10 seconds
        _refreshTimer.Tick += (s, e) =>
        {
            if (chkAutoRefresh.Checked && !_isRefreshing)
            {
                _ = RefreshDataAsync(silent: true);
            }
        };
    }

    private async void AdminForm_Load(object sender, EventArgs e)
    {
        try
        {
            var iconPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "app.ico");
            if (File.Exists(iconPath))
            {
                Icon = new Icon(iconPath);
                picLogo.Image = Icon.ToBitmap();
            }
        }
        catch { }

        // Load Settings into controls
        await LoadSettingsToUiAsync();
        await LoadPermissionsToUiAsync();

        // Initial Data Refresh
        await RefreshDataAsync(silent: false);
        await RefreshLogsAsync();
        _ = CheckCurrentUpdateInfoAsync();

        // Start Auto Refresh
        _refreshTimer.Start();
    }

    private async Task LoadSettingsToUiAsync()
    {
        try
        {
            var config = AuthConfig.Instance;
            txtFirebaseUrl.Text = config.FirebaseUrl;
            txtAuthSecret.Text = config.AuthSecret;

            var globalSettings = await _authService.GetSettingsAsync();
            if (globalSettings != null)
            {
                txtWhatsAppNumber.Text = string.IsNullOrWhiteSpace(globalSettings.WhatsAppNumber) ? "+201150238481" : globalSettings.WhatsAppNumber;
                txtWhatsAppUrl.Text = string.IsNullOrWhiteSpace(globalSettings.WhatsAppUrl) ? "https://wa.me/201150238481" : globalSettings.WhatsAppUrl;
                txtRechargeMessage.Text = string.IsNullOrWhiteSpace(globalSettings.RechargeMessage) ? "برجاء إرسال رسالة لنا لإعادة شحن البوت" : globalSettings.RechargeMessage;
            }
        }
        catch { }
    }

    private async Task RefreshDataAsync(bool silent = false)
    {
        if (_isRefreshing) return;
        _isRefreshing = true;

        if (!silent)
            lblStatusMessage.Text = "جارٍ جلب وتحديث بيانات الحسابات من السيرفر...";

        try
        {
            // Test connection first
            var connected = await _authService.TestConnectionAsync();
            if (connected)
            {
                lblServerStatus.Text = "السيرفر: متصل 🟢";
                lblServerStatus.ForeColor = Color.LightGreen;
            }
            else
            {
                lblServerStatus.Text = "السيرفر: غير متصل 🔴";
                lblServerStatus.ForeColor = Color.LightCoral;
            }

            var settings = await _authService.GetSettingsAsync();
            var maxCap = settings.MaxAccounts > 0 ? settings.MaxAccounts : 16;
            numMaxAccounts.Value = Math.Clamp(maxCap, 1, 100);

            var accounts = await _authService.GetAllAccountsAsync();

            // Remember selected username
            string selectedUser = null;
            if (dgvAccounts.SelectedRows.Count > 0 && dgvAccounts.SelectedRows[0].Cells[1].Value != null)
            {
                selectedUser = dgvAccounts.SelectedRows[0].Cells[1].Value.ToString();
            }

            dgvAccounts.Rows.Clear();

            int index = 1;
            int activeCount = 0;
            int blockedCount = 0;
            int expiredCount = 0;

            foreach (var kvp in accounts)
            {
                var acc = kvp.Value;
                if (acc == null) continue;

                if (acc.IsBlocked)
                    blockedCount++;
                else if (acc.IsExpired)
                    expiredCount++;
                else
                    activeCount++;

                var statusText = acc.IsBlocked ? "🚫 محظور" : (acc.IsAdmin ? "⭐ مسؤول (Admin)" : "🟢 نشط (عادي)");
                var hwidText = string.IsNullOrWhiteSpace(acc.HWID) ? "⚠️ غير مقيد (متاح للربط)" : acc.HWID;

                string subText;
                if (acc.IsLifetime)
                {
                    subText = "♾️ دائم (Lifetime)";
                }
                else if (acc.IsExpired)
                {
                    subText = "⏳ منتهي الصلاحية";
                }
                else
                {
                    subText = $"🟢 {acc.GetFormattedRemainingTime()}";
                }

                string expiryText = string.IsNullOrWhiteSpace(acc.ExpiresAt) ? "غير محدد" : acc.ExpiresAt;

                int rowIndex = dgvAccounts.Rows.Add(
                    index++,
                    acc.Username,
                    statusText,
                    subText,
                    expiryText,
                    acc.BlockedReason,
                    hwidText,
                    acc.RegisteredIP,
                    acc.LastLoginIP,
                    acc.CreatedAt,
                    acc.LastLoginAt,
                    acc.Notes
                );

                var row = dgvAccounts.Rows[rowIndex];
                if (acc.IsBlocked)
                {
                    row.DefaultCellStyle.ForeColor = Color.FromArgb(255, 120, 120);
                }
                else if (acc.IsExpired)
                {
                    row.DefaultCellStyle.ForeColor = Color.FromArgb(255, 160, 90);
                }
                else if (acc.IsLifetime)
                {
                    row.DefaultCellStyle.ForeColor = Color.FromArgb(120, 240, 240);
                }
                else if (string.IsNullOrWhiteSpace(acc.HWID))
                {
                    row.DefaultCellStyle.ForeColor = Color.FromArgb(255, 210, 80);
                }

                if (string.Equals(acc.Username, selectedUser, StringComparison.OrdinalIgnoreCase))
                {
                    row.Selected = true;
                }
            }

            lblTotalAccounts.Text = $"👥 الحسابات: {accounts.Count} / {maxCap}";
            lblActiveCount.Text = $"🟢 نشط: {activeCount}";
            lblExpiredCount.Text = $"⏳ منتهي: {expiredCount}";
            lblBlockedCount.Text = $"🚫 محظور: {blockedCount}";

            lblStatusMessage.Text = $"آخر تحديث: {DateTime.Now:HH:mm:ss} | إجمالي الحسابات: {accounts.Count} من أصل {maxCap}";
        }
        catch (Exception ex)
        {
            if (!silent)
                lblStatusMessage.Text = $"خطأ في جلب البيانات: {ex.Message}";
        }
        finally
        {
            _isRefreshing = false;
        }
    }

    private async Task RefreshLogsAsync()
    {
        try
        {
            var logs = await _authService.GetLogsAsync();
            dgvLogs.Rows.Clear();

            foreach (var log in logs)
            {
                int rowIndex = dgvLogs.Rows.Add(
                    log.Timestamp,
                    log.Username,
                    log.Action,
                    log.HWID,
                    log.IP,
                    log.Details
                );

                var row = dgvLogs.Rows[rowIndex];
                if (log.Action.Contains("BLOCK", StringComparison.OrdinalIgnoreCase) ||
                    log.Action.Contains("EXPIRED", StringComparison.OrdinalIgnoreCase))
                {
                    row.DefaultCellStyle.ForeColor = Color.FromArgb(255, 110, 110);
                }
                else if (log.Action.Contains("SUCCESS", StringComparison.OrdinalIgnoreCase) ||
                         log.Action.Contains("REGISTER", StringComparison.OrdinalIgnoreCase) ||
                         log.Action.Contains("SUBSCRIPTION", StringComparison.OrdinalIgnoreCase))
                {
                    row.DefaultCellStyle.ForeColor = Color.FromArgb(120, 230, 140);
                }
            }
        }
        catch { }
    }

    private string GetSelectedUsername()
    {
        if (dgvAccounts.SelectedRows.Count == 0 || dgvAccounts.SelectedRows[0].Cells[1].Value == null)
        {
            MessageBox.Show(this, "يرجى تحديد حساب من القائمة أولاً.", "تنبيه", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return null;
        }

        return dgvAccounts.SelectedRows[0].Cells[1].Value.ToString();
    }

    private async Task ExecuteSubscriptionActionAsync(Func<string, Task<bool>> action, string actionDescription)
    {
        var user = GetSelectedUsername();
        if (user == null) return;

        lblStatusMessage.Text = $"جارٍ تنفيذ ({actionDescription}) للحساب [{user}]...";

        try
        {
            var ok = await action(user);
            if (ok)
            {
                lblStatusMessage.Text = $"تم تنفيذ ({actionDescription}) للحساب [{user}] بنجاح!";
                await RefreshDataAsync();
                await RefreshLogsAsync();
            }
            else
            {
                MessageBox.Show(this, $"فشل تنفيذ العملية على الحساب '{user}'.", "خطأ", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show(this, $"حدث خطأ: {ex.Message}", "خطأ", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    // --- SUBSCRIPTION TIME EXTENSION (اليوم والشهر) ---

    private async void btnAdd1Day_Click(object sender, EventArgs e)
    {
        await ExecuteSubscriptionActionAsync(u => _authService.AddSubscriptionDaysAsync(u, 1), "إضافة +1 يوم");
    }

    private async void btnAdd7Days_Click(object sender, EventArgs e)
    {
        await ExecuteSubscriptionActionAsync(u => _authService.AddSubscriptionDaysAsync(u, 7), "إضافة +7 أيام (أسبوع)");
    }

    private async void btnAdd30Days_Click(object sender, EventArgs e)
    {
        await ExecuteSubscriptionActionAsync(u => _authService.AddSubscriptionMonthsAsync(u, 1), "إضافة +1 شهر (30 يوم)");
    }

    private async void btnAdd60Days_Click(object sender, EventArgs e)
    {
        await ExecuteSubscriptionActionAsync(u => _authService.AddSubscriptionMonthsAsync(u, 2), "إضافة + شهرين (60 يوم)");
    }

    private async void btnAdd90Days_Click(object sender, EventArgs e)
    {
        await ExecuteSubscriptionActionAsync(u => _authService.AddSubscriptionMonthsAsync(u, 3), "إضافة +3 شهور (90 يوم)");
    }

    private async void tsmiAdd365Days_Click(object sender, EventArgs e)
    {
        await ExecuteSubscriptionActionAsync(u => _authService.AddSubscriptionMonthsAsync(u, 12), "إضافة + سنة كاملة (12 شهر)");
    }

    private async void btnAddCustom_Click(object sender, EventArgs e)
    {
        var user = GetSelectedUsername();
        if (user == null) return;

        var result = InputPrompt.ShowCustomDurationDialog(user);
        if (result == null) return;

        var (isMonths, count) = result.Value;
        if (count <= 0) return;

        if (isMonths)
        {
            await ExecuteSubscriptionActionAsync(u => _authService.AddSubscriptionMonthsAsync(u, count), $"إضافة +{count} شهر");
        }
        else
        {
            await ExecuteSubscriptionActionAsync(u => _authService.AddSubscriptionDaysAsync(u, count), $"إضافة +{count} يوم");
        }
    }

    private async void btnSetDate_Click(object sender, EventArgs e)
    {
        var user = GetSelectedUsername();
        if (user == null) return;

        var dt = InputPrompt.ShowDatePickerPrompt(
            $"اختر تاريخ ووقت انتهاء اشتراك الحساب [{user}]:",
            "تحديد موعد انتهاء الاشتراك",
            DateTime.Now.AddDays(30));

        if (dt == null) return;

        var utcTarget = dt.Value.ToUniversalTime();
        await ExecuteSubscriptionActionAsync(u => _authService.SetSubscriptionExpiryAsync(u, utcTarget), $"تحديد تاريخ الانتهاء إلى {dt.Value:yyyy-MM-dd HH:mm}");
    }

    private async void btnLifetime_Click(object sender, EventArgs e)
    {
        var user = GetSelectedUsername();
        if (user == null) return;

        var confirm = MessageBox.Show(this, $"هل أنت متأكد من تفعيل اشتراك دائم (Lifetime) للحساب '{user}'؟", "تأكيد الاشتراك الدائم", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
        if (confirm != DialogResult.Yes) return;

        await ExecuteSubscriptionActionAsync(u => _authService.SetLifetimeAsync(u, true), "تفعيل اشتراك دائم (Lifetime)");
    }

    private async void btnExpireNow_Click(object sender, EventArgs e)
    {
        var user = GetSelectedUsername();
        if (user == null) return;

        var confirm = MessageBox.Show(this, $"هل أنت متأكد من إنهاء اشتراك الحساب '{user}' فوراً؟\nسيتوقف البوت عنده فوراً ويطلب منه التجديد.", "تأكيد إنهاء الاشتراك", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
        if (confirm != DialogResult.Yes) return;

        await ExecuteSubscriptionActionAsync(u => _authService.ExpireAccountNowAsync(u), "إنهاء الاشتراك فوراً");
    }

    // --- ACCOUNT ACTIONS ---

    private async void btnRefresh_Click(object sender, EventArgs e)
    {
        await RefreshDataAsync(silent: false);
    }

    private async void btnUnblock_Click(object sender, EventArgs e)
    {
        var user = GetSelectedUsername();
        if (user == null) return;

        var confirm = MessageBox.Show(this, $"هل أنت متأكد من فك الحظر عن الحساب '{user}'؟", "تأكيد فك الحظر", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
        if (confirm != DialogResult.Yes) return;

        lblStatusMessage.Text = $"جارٍ فك الحظر عن {user}...";
        var ok = await _authService.UnblockAccountAsync(user);
        if (ok)
        {
            MessageBox.Show(this, $"تم فك الحظر عن الحساب '{user}' بنجاح.", "تم بنجاح", MessageBoxButtons.OK, MessageBoxIcon.Information);
            await RefreshDataAsync();
            await RefreshLogsAsync();
        }
        else
        {
            MessageBox.Show(this, "فشل فك الحظر. يرجى التأكد من الاتصال بالسيرفر.", "خطأ", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private async void btnBlock_Click(object sender, EventArgs e)
    {
        var user = GetSelectedUsername();
        if (user == null) return;

        var reason = InputPrompt.Show("أدخل سبب الحظر:", "حظر الحساب يدويًا", "مخالفة شروط الاستخدام بواسطة الإدارة");
        if (reason == null) return;

        lblStatusMessage.Text = $"جارٍ حظر الحساب {user}...";
        var ok = await _authService.BlockAccountAsync(user, reason);
        if (ok)
        {
            MessageBox.Show(this, $"تم حظر الحساب '{user}' بنجاح.", "تم بنجاح", MessageBoxButtons.OK, MessageBoxIcon.Information);
            await RefreshDataAsync();
            await RefreshLogsAsync();
        }
        else
        {
            MessageBox.Show(this, "فشل حظر الحساب.", "خطأ", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private async void btnResetHwid_Click(object sender, EventArgs e)
    {
        var user = GetSelectedUsername();
        if (user == null) return;

        var confirm = MessageBox.Show(
            this,
            $"هل أنت متأكد من فك ربط الجهاز (Reset HWID) للحساب '{user}'؟\n\n" +
            "هذا سيسمح للمستخدم بتسجيل الدخول من جهاز جديد، وسيتم ربط الحساب تلقائياً بالجهاز الجديد فور دخوله.",
            "تأكيد فك ربط الجهاز",
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Question);

        if (confirm != DialogResult.Yes) return;

        lblStatusMessage.Text = $"جارٍ فك ربط الجهاز للحساب {user}...";
        var ok = await _authService.ResetHwidAsync(user);
        if (ok)
        {
            MessageBox.Show(this, $"تم فك ربط الجهاز بنجاح للحساب '{user}'.\nيمكن للعميل الآن تسجيل الدخول بجهازه الجديد ليتم ربطه فوراً.", "تم بنجاح", MessageBoxButtons.OK, MessageBoxIcon.Information);
            await RefreshDataAsync();
            await RefreshLogsAsync();
        }
        else
        {
            MessageBox.Show(this, "فشلت العملية.", "خطأ", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private async void btnChangePassword_Click(object sender, EventArgs e)
    {
        var user = GetSelectedUsername();
        if (user == null) return;

        var newPass = InputPrompt.Show("أدخل كلمة المرور الجديدة:", "تغيير كلمة المرور", "");
        if (string.IsNullOrWhiteSpace(newPass)) return;

        if (newPass.Length < 4)
        {
            MessageBox.Show(this, "كلمة المرور يجب أن تكون 4 أحرف على الأقل.", "تنبيه", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        lblStatusMessage.Text = $"جارٍ تغيير كلمة المرور للحساب {user}...";
        var ok = await _authService.ChangePasswordAsync(user, newPass);
        if (ok)
        {
            MessageBox.Show(this, $"تم تغيير كلمة المرور للحساب '{user}' بنجاح.", "تم بنجاح", MessageBoxButtons.OK, MessageBoxIcon.Information);
            await RefreshLogsAsync();
        }
        else
        {
            MessageBox.Show(this, "فشل تغيير كلمة المرور.", "خطأ", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private async void btnAddAccount_Click(object sender, EventArgs e)
    {
        var result = InputPrompt.ShowNewAccountDialog();
        if (result == null) return;

        var (username, password, notes, isAdmin) = result.Value;
        lblStatusMessage.Text = $"جارٍ إضافة الحساب {username}...";

        var res = await _authService.AdminCreateAccountAsync(username, password, notes, isAdmin);
        if (res.Success)
        {
            var roleText = isAdmin ? "مسؤول (Admin) ⭐" : "مستخدم عادي 👤";
            MessageBox.Show(this, $"تمت إضافة الحساب '{username}' برتبة [{roleText}] بنجاح!\nسيتم ربطه بجهاز العميل تلقائياً عند أول تسجيل دخول له.", "تم بنجاح", MessageBoxButtons.OK, MessageBoxIcon.Information);
            await RefreshDataAsync();
            await RefreshLogsAsync();
        }
        else
        {
            MessageBox.Show(this, res.Message, "خطأ في الإضافة", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
    }

    private async void btnToggleAdmin_Click(object sender, EventArgs e)
    {
        var username = GetSelectedUsername();
        if (string.IsNullOrWhiteSpace(username)) return;

        var acc = await _authService.GetAccountAsync(username);
        if (acc == null)
        {
            MessageBox.Show(this, "لم يتم العثور على بيانات الحساب.", "خطأ", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        bool targetAdmin = !acc.IsAdmin;
        var actionPrompt = targetAdmin
            ? $"هل أنت متأكد من ترقية الحساب '{username}' ليصبح مسؤولاً (Admin) بكامل الصلاحيات؟"
            : $"هل أنت متأكد من سحب صلاحيات الأدمن من الحساب '{username}' وتحويله إلى مستخدم عادي؟";

        var confirm = MessageBox.Show(this, actionPrompt, "تأكيد تعديل رتبة الحساب", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
        if (confirm != DialogResult.Yes) return;

        lblStatusMessage.Text = $"جارٍ تعديل رتبة الحساب {username}...";
        var success = await _authService.SetAdminStatusAsync(username, targetAdmin);
        if (success)
        {
            var resultMsg = targetAdmin
                ? $"تمت ترقية الحساب '{username}' إلى مسؤول (Admin) ⭐ بنجاح!"
                : $"تم سحب صلاحيات الأدمن من الحساب '{username}' وأصبح مستخدماً عادياً 👤 بنجاح!";
            MessageBox.Show(this, resultMsg, "تم بنجاح", MessageBoxButtons.OK, MessageBoxIcon.Information);
            await RefreshDataAsync();
            await RefreshLogsAsync();
        }
        else
        {
            MessageBox.Show(this, "فشل حفظ التعديل في السيرفر.", "خطأ", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private async void btnDeleteAccount_Click(object sender, EventArgs e)
    {
        var user = GetSelectedUsername();
        if (user == null) return;

        var confirm = MessageBox.Show(
            this,
            $"⚠️ هل أنت متأكد من حذف الحساب '{user}' نهائياً؟\n\n" +
            "هذا الإجراء سيقوم بحذف الحساب تماماً من قاعدة البيانات وإخلاء خانة من الخانات المتاحة.",
            "تأكيد حذف الحساب",
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Warning);

        if (confirm != DialogResult.Yes) return;

        lblStatusMessage.Text = $"جارٍ حذف الحساب {user}...";
        var ok = await _authService.DeleteAccountAsync(user);
        if (ok)
        {
            MessageBox.Show(this, $"تم حذف الحساب '{user}' بنجاح.", "تم بنجاح", MessageBoxButtons.OK, MessageBoxIcon.Information);
            await RefreshDataAsync();
            await RefreshLogsAsync();
        }
        else
        {
            MessageBox.Show(this, "فشل حذف الحساب.", "خطأ", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private async void btnRefreshLogs_Click(object sender, EventArgs e)
    {
        lblStatusMessage.Text = "جارٍ تحديث سجل الأمان...";
        await RefreshLogsAsync();
        lblStatusMessage.Text = "تم تحديث سجل الأمان بنجاح.";
    }

    // --- SERVER SETTINGS ---

    private async void btnTestConnection_Click(object sender, EventArgs e)
    {
        lblStatusMessage.Text = "جارٍ اختبار الاتصال بسيرفر Firebase...";
        btnTestConnection.Enabled = false;

        var config = AuthConfig.Instance;
        var oldUrl = config.FirebaseUrl;
        var oldSecret = config.AuthSecret;

        config.FirebaseUrl = txtFirebaseUrl.Text.Trim();
        config.AuthSecret = txtAuthSecret.Text.Trim();

        var ok = await _authService.TestConnectionAsync();
        btnTestConnection.Enabled = true;

        if (ok)
        {
            MessageBox.Show(this, "الاتصال بسيرفر Firebase ناجح ويعمل بصورة ممتازة! 🟢", "نجاح الاتصال", MessageBoxButtons.OK, MessageBoxIcon.Information);
            lblServerStatus.Text = "السيرفر: متصل 🟢";
            lblServerStatus.ForeColor = Color.LightGreen;
        }
        else
        {
            MessageBox.Show(
                this,
                "فشل الاتصال بسيرفر Firebase! 🔴\n\n" +
                "يرجى التأكد من:\n" +
                "1. صحة رابط Realtime Database.\n" +
                "2. قواعد السيرفر (Rules) تسمح بالقراءة والكتابة (read: true, write: true).\n" +
                "3. اتصال الإنترنت بالجهاز.",
                "فشل الاتصال",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);

            lblServerStatus.Text = "السيرفر: غير متصل 🔴";
            lblServerStatus.ForeColor = Color.LightCoral;

            config.FirebaseUrl = oldUrl;
            config.AuthSecret = oldSecret;
        }
    }

    private async void btnSaveSettings_Click(object sender, EventArgs e)
    {
        var config = AuthConfig.Instance;
        config.FirebaseUrl = txtFirebaseUrl.Text.Trim();
        config.AuthSecret = txtAuthSecret.Text.Trim();
        config.Save();

        // Also update settings on Firebase
        var settings = await _authService.GetSettingsAsync() ?? new GlobalAuthSettings();
        settings.MaxAccounts = (int)numMaxAccounts.Value;

        var ok = await _authService.SaveSettingsAsync(settings);
        if (ok)
        {
            MessageBox.Show(this, "تم حفظ إعدادات السيرفر بنجاح وتطبيقها محلياً وسحابياً! 💾", "تم الحفظ", MessageBoxButtons.OK, MessageBoxIcon.Information);
            await RefreshDataAsync();
        }
        else
        {
            MessageBox.Show(this, "تم حفظ الإعدادات محلياً، ولكن تعذر تحديثها على السيرفر السحابي.", "تنبيه", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
    }

    private async void btnSaveWhatsApp_Click(object sender, EventArgs e)
    {
        var num = txtWhatsAppNumber.Text.Trim();
        var url = txtWhatsAppUrl.Text.Trim();
        var msg = txtRechargeMessage.Text.Trim();

        if (string.IsNullOrWhiteSpace(num))
        {
            MessageBox.Show(this, "يرجى كتابة رقم الواتساب!", "تنبيه", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        var settings = await _authService.GetSettingsAsync() ?? new GlobalAuthSettings();
        settings.WhatsAppNumber = num;
        settings.WhatsAppUrl = url;
        settings.RechargeMessage = msg;

        lblStatusMessage.Text = "جارٍ حفظ إعدادات الواتساب على السيرفر...";
        var ok = await _authService.SaveSettingsAsync(settings);
        if (ok)
        {
            MessageBox.Show(this, "تم حفظ إعدادات الواتساب بنجاح على سيرفر Firebase! 📱\nستظهر الآن لجميع المستخدمين عند انتهاء مدد اشتراكهم.", "تم الحفظ", MessageBoxButtons.OK, MessageBoxIcon.Information);
            lblStatusMessage.Text = "تم حفظ إعدادات الواتساب بنجاح.";
        }
        else
        {
            MessageBox.Show(this, "فشل حفظ إعدادات الواتساب على السيرفر السحابي. يرجى التأكد من الاتصال.", "خطأ", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    // --- APP UPDATE MANAGEMENT ---

    private async Task CheckCurrentUpdateInfoAsync()
    {
        try
        {
            var update = await UpdateManager.CheckForUpdateAsync();
            if (update != null)
            {
                txtUpdateVersion.Text = update.Version;
                txtUpdateUrl.Text = update.DownloadUrl;
                chkUpdateMandatory.Checked = update.Mandatory;
                txtChangelogAr.Text = update.ChangelogAr;
                txtChangelogEn.Text = update.ChangelogEn;
                lblUpdateStatus.Text = $"الإصدار الحالي على السيرفر: {update.Version} ({update.ReleasedAt})";
                lblUpdateStatus.ForeColor = Color.LightGreen;
            }
            else
            {
                lblUpdateStatus.Text = "لا يوجد تحديث منشور على Firebase حالياً.";
                lblUpdateStatus.ForeColor = Color.DarkGray;
            }
        }
        catch (Exception ex)
        {
            lblUpdateStatus.Text = "خطأ أثناء جلب معلومات التحديث: " + ex.Message;
            lblUpdateStatus.ForeColor = Color.LightCoral;
        }
    }

    private async void btnPublishUpdate_Click(object sender, EventArgs e)
    {
        var version = txtUpdateVersion.Text.Trim();
        var downloadUrl = txtUpdateUrl.Text.Trim();
        var changelogAr = txtChangelogAr.Text.Trim();
        var changelogEn = txtChangelogEn.Text.Trim();

        if (string.IsNullOrWhiteSpace(version))
        {
            MessageBox.Show(this, "يرجى إدخال رقم الإصدار (مثال: 2.9.15)!", "تنبيه", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        if (string.IsNullOrWhiteSpace(downloadUrl))
        {
            MessageBox.Show(this, "يرجى إدخال رابط تحميل ملف التحديث (.zip) المباشر!", "تنبيه", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        var confirm = MessageBox.Show(
            this,
            $"هل أنت متأكد من نشر التحديث الجديد إصدار [{version}] للمستخدمين على السيرفر؟\n\nعند فتح البوت عند أي مستخدم سيظهر له إشعار التحديث وسيقوم بتحديث نفسه تلقائياً.",
            "تأكيد نشر التحديث",
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Question);

        if (confirm != DialogResult.Yes) return;

        btnPublishUpdate.Enabled = false;
        lblUpdateStatus.Text = "جارٍ نشر التحديث على Firebase...";
        lblUpdateStatus.ForeColor = Color.Yellow;

        var updateInfo = new AppUpdateInfo
        {
            Version = version,
            DownloadUrl = downloadUrl,
            Mandatory = chkUpdateMandatory.Checked,
            ChangelogAr = changelogAr,
            ChangelogEn = changelogEn,
            ReleasedAt = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
        };

        var ok = await UpdateManager.PublishUpdateAsync(updateInfo);
        btnPublishUpdate.Enabled = true;

        if (ok)
        {
            lblUpdateStatus.Text = $"تم نشر التحديث [{version}] بنجاح! 🚀";
            lblUpdateStatus.ForeColor = Color.LightGreen;
            MessageBox.Show(this, $"تم نشر التحديث الجديد [{version}] بنجاح على Firebase! 🚀\nجميع المستخدمين سيستلمون التحديث تلقائياً عند تشغيل البوت.", "تم النشر", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        else
        {
            lblUpdateStatus.Text = "فشل نشر التحديث على Firebase!";
            lblUpdateStatus.ForeColor = Color.LightCoral;
            MessageBox.Show(this, "فشل نشر التحديث على Firebase! تأكد من اتصال الإنترنت وصلاحيات قاعدة البيانات.", "خطأ", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private async void btnCheckUpdate_Click(object sender, EventArgs e)
    {
        btnCheckUpdate.Enabled = false;
        lblUpdateStatus.Text = "جارٍ فحص التحديث الحالي من السيرفر...";
        lblUpdateStatus.ForeColor = Color.Yellow;

        await CheckCurrentUpdateInfoAsync();
        btnCheckUpdate.Enabled = true;
    }

    private async Task LoadPermissionsToUiAsync()
    {
        try
        {
            lblPermStatus.Text = "جارٍ تحميل الصلاحيات من السيرفر...";
            lblPermStatus.ForeColor = Color.Yellow;

            var perms = await _authService.GetFreeTierPermissionsAsync();
            if (perms == null)
            {
                perms = new FreeTierPermissions();
            }

            chkAllowGrinding.Checked = perms.AllowGrindingBot;
            chkAllowTrade.Checked = perms.AllowTradeBot;
            chkAllowAlchemy.Checked = perms.AllowAlchemyBot;
            chkAllowLure.Checked = perms.AllowLureBot;

            chkAllowTownLoop.Checked = perms.AllowTownLoop;
            chkAllowAutoParty.Checked = perms.AllowAutoParty;
            chkAllowAutoPotion.Checked = perms.AllowAutoPotion;
            chkAllowAutoPickup.Checked = perms.AllowAutoPickup;
            chkAllowAutoResurrect.Checked = perms.AllowAutoResurrect;
            chkAllowStall.Checked = perms.AllowStall;

            numMaxClients.Value = Math.Clamp(perms.MaxClients, 0, 100);
            numSessionMinutes.Value = Math.Clamp(perms.SessionMinutes, 0, 1440);
            txtFreeNotice.Text = perms.Notice ?? string.Empty;

            lblPermStatus.Text = "تم تحميل الصلاحيات بنجاح 🟢";
            lblPermStatus.ForeColor = Color.LightGreen;
        }
        catch
        {
            lblPermStatus.Text = "فشل تحميل الصلاحيات 🔴";
            lblPermStatus.ForeColor = Color.LightCoral;
        }
    }

    private async void btnSavePermissions_Click(object sender, EventArgs e)
    {
        try
        {
            btnSavePermissions.Enabled = false;
            lblPermStatus.Text = "جارٍ حفظ الصلاحيات في السيرفر...";
            lblPermStatus.ForeColor = Color.Yellow;

            var perms = new FreeTierPermissions
            {
                AllowGrindingBot = chkAllowGrinding.Checked,
                AllowTradeBot = chkAllowTrade.Checked,
                AllowAlchemyBot = chkAllowAlchemy.Checked,
                AllowLureBot = chkAllowLure.Checked,
                AllowTownLoop = chkAllowTownLoop.Checked,
                AllowAutoParty = chkAllowAutoParty.Checked,
                AllowAutoPotion = chkAllowAutoPotion.Checked,
                AllowAutoPickup = chkAllowAutoPickup.Checked,
                AllowAutoResurrect = chkAllowAutoResurrect.Checked,
                AllowStall = chkAllowStall.Checked,
                MaxClients = (int)numMaxClients.Value,
                SessionMinutes = (int)numSessionMinutes.Value,
                Notice = txtFreeNotice.Text.Trim()
            };

            var ok = await _authService.SaveFreeTierPermissionsAsync(perms);
            if (ok)
            {
                lblPermStatus.Text = "تم حفظ وتطبيق الصلاحيات بنجاح! 🚀";
                lblPermStatus.ForeColor = Color.LightGreen;
                MessageBox.Show(this, "تم حفظ وتطبيق صلاحيات النسخة المجانية في السيرفر بنجاح! 🚀\nجميع المستخدمين للنسخة المجانية سيتم تطبيق هذه الصلاحيات عليهم فوراً.", "تم الحفظ بنجاح", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
                lblPermStatus.Text = "فشل حفظ الصلاحيات بالسيرفر 🔴";
                lblPermStatus.ForeColor = Color.LightCoral;
                MessageBox.Show(this, "فشل حفظ الصلاحيات على Firebase! تأكد من الاتصال بالإنترنت.", "خطأ في الحفظ", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        catch (Exception ex)
        {
            lblPermStatus.Text = "حدث خطأ أثناء الحفظ!";
            lblPermStatus.ForeColor = Color.LightCoral;
            MessageBox.Show(this, $"حدث خطأ: {ex.Message}", "خطأ", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        finally
        {
            btnSavePermissions.Enabled = true;
        }
    }

    private async void btnReloadPermissions_Click(object sender, EventArgs e)
    {
        btnReloadPermissions.Enabled = false;
        await LoadPermissionsToUiAsync();
        btnReloadPermissions.Enabled = true;
    }
}
