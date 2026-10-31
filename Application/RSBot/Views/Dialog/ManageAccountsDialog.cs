using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using RSBot.Core.Security;
using SDUI;
using SDUI.Controls;

namespace RSBot.Views;

public partial class ManageAccountsDialog : UIWindowBase
{
    private readonly AuthService _authService = AuthService.Instance;
    private Dictionary<string, UserAccount> _allAccounts = new(StringComparer.OrdinalIgnoreCase);
    private string _selectedUsername = string.Empty;

    public ManageAccountsDialog()
    {
        InitializeComponent();

        try
        {
            DoubleBuffered = true;
            ColorScheme.BackColor = Color.FromArgb(24, 24, 28);
            BackColor = Color.FromArgb(24, 24, 28);
        }
        catch { }

        Load += ManageAccountsDialog_Load;
    }

    private async void ManageAccountsDialog_Load(object sender, EventArgs e)
    {
        dtpCustomExpiry.Value = DateTime.Now.AddDays(30);
        await RefreshAccountsAsync();
    }

    private async Task RefreshAccountsAsync()
    {
        SetStatus("جارٍ تحميل الحسابات من السيرفر...", Color.DarkGoldenrod);
        btnRefresh.Enabled = false;

        try
        {
            _allAccounts = await _authService.GetAllAccountsAsync();
            PopulateList(txtSearch.Text);
            SetStatus($"تم تحميل {_allAccounts.Count} حساب بنجاح. الحد الأقصى: 16 حساب.", Color.LightGreen);
        }
        catch (Exception ex)
        {
            SetStatus($"فشل تحميل الحسابات: {ex.Message}", Color.LightCoral);
        }
        finally
        {
            btnRefresh.Enabled = true;
        }
    }

    private void PopulateList(string filter = "")
    {
        listAccounts.BeginUpdate();
        listAccounts.Items.Clear();

        filter = (filter ?? string.Empty).Trim().ToLowerInvariant();

        foreach (var pair in _allAccounts.OrderBy(p => p.Key))
        {
            var acc = pair.Value;
            if (!string.IsNullOrEmpty(filter) && !acc.Username.ToLowerInvariant().Contains(filter))
                continue;

            string statusText;
            Color statusColor;

            if (acc.IsBlocked)
            {
                statusText = "محظور 🔴";
                statusColor = Color.LightCoral;
            }
            else if (acc.IsAdmin)
            {
                statusText = "مسؤول ⭐";
                statusColor = Color.Gold;
            }
            else if (acc.IsLifetime)
            {
                statusText = "اشتراك دائم ♾️";
                statusColor = Color.Aquamarine;
            }
            else if (acc.IsExpired)
            {
                statusText = "منتهي ⏳";
                statusColor = Color.Salmon;
            }
            else
            {
                statusText = "نشط 🟢";
                statusColor = Color.LightGreen;
            }

            var item = new ListViewItem(acc.Username)
            {
                Tag = acc,
                ForeColor = statusColor
            };

            item.SubItems.Add(statusText);
            item.SubItems.Add(acc.GetFormattedRemainingTime());
            item.SubItems.Add(string.IsNullOrWhiteSpace(acc.ExpiresAt) ? "غير محدد" : acc.ExpiresAt);
            item.SubItems.Add(string.IsNullOrWhiteSpace(acc.HWID) ? "(جهاز غير مربوط)" : acc.HWID);
            item.SubItems.Add(string.IsNullOrWhiteSpace(acc.LastLoginAt) ? "لم يسجل بعد" : acc.LastLoginAt);

            listAccounts.Items.Add(item);

            if (string.Equals(acc.Username, _selectedUsername, StringComparison.OrdinalIgnoreCase))
            {
                item.Selected = true;
            }
        }

        listAccounts.EndUpdate();
        UpdateSelectedUserUI();
    }

    private void listAccounts_SelectedIndexChanged(object sender, EventArgs e)
    {
        if (listAccounts.SelectedItems.Count > 0)
        {
            _selectedUsername = listAccounts.SelectedItems[0].Text;
        }
        else
        {
            _selectedUsername = string.Empty;
        }

        UpdateSelectedUserUI();
    }

    private void UpdateSelectedUserUI()
    {
        if (string.IsNullOrWhiteSpace(_selectedUsername) || !_allAccounts.TryGetValue(_selectedUsername, out var acc))
        {
            lblSelectedUser.Text = "الحساب المحدد: لم يتم تحديد أي حساب";
            lblSelectedUser.ForeColor = Color.FromArgb(160, 175, 195);
            return;
        }

        var remaining = acc.GetFormattedRemainingTime();
        lblSelectedUser.Text = $"الحساب المحدد: [{acc.Username}] | الصلاحية: {remaining} | الجهاز: {(string.IsNullOrEmpty(acc.HWID) ? "حر" : "مقيد")}";
        lblSelectedUser.ForeColor = Color.FromArgb(140, 205, 255);

        if (acc.ExpirationDate.HasValue)
        {
            dtpCustomExpiry.Value = acc.ExpirationDate.Value.ToLocalTime();
        }
    }

    private void txtSearch_TextChanged(object sender, EventArgs e)
    {
        PopulateList(txtSearch.Text);
    }

    private async void btnRefresh_Click(object sender, EventArgs e)
    {
        await RefreshAccountsAsync();
    }

    private async void btnAdd1Day_Click(object sender, EventArgs e)
    {
        await ExecuteSubscriptionChangeAsync(async user => await _authService.AddSubscriptionDaysAsync(user, 1), "+1 يوم");
    }

    private async void btnAdd7Days_Click(object sender, EventArgs e)
    {
        await ExecuteSubscriptionChangeAsync(async user => await _authService.AddSubscriptionDaysAsync(user, 7), "+7 أيام");
    }

    private async void btnAdd30Days_Click(object sender, EventArgs e)
    {
        await ExecuteSubscriptionChangeAsync(async user => await _authService.AddSubscriptionDaysAsync(user, 30), "+30 يوم (شهر)");
    }

    private async void btnAdd90Days_Click(object sender, EventArgs e)
    {
        await ExecuteSubscriptionChangeAsync(async user => await _authService.AddSubscriptionDaysAsync(user, 90), "+90 يوم (3 شهور)");
    }

    private async void btnLifetime_Click(object sender, EventArgs e)
    {
        await ExecuteSubscriptionChangeAsync(async user => await _authService.SetLifetimeAsync(user, true), "اشتراك دائم (Lifetime)");
    }

    private async void btnExpireNow_Click(object sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(_selectedUsername))
        {
            MessageBox.Show(this, "يرجى تحديد حساب أولاً.", "تنبيه", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        if (MessageBox.Show(this, $"هل أنت متأكد من إنهاء اشتراك الحساب [{_selectedUsername}] فوراً؟", "تأكيد", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
            return;

        await ExecuteSubscriptionChangeAsync(async user => await _authService.ExpireAccountNowAsync(user), "إنهاء الاشتراك فوراً");
    }

    private async void btnApplyCustomDate_Click(object sender, EventArgs e)
    {
        var targetUtc = dtpCustomExpiry.Value.ToUniversalTime();
        await ExecuteSubscriptionChangeAsync(async user => await _authService.SetSubscriptionExpiryAsync(user, targetUtc), $"تحديد التاريخ إلى {targetUtc:yyyy-MM-dd HH:mm}");
    }

    private async Task ExecuteSubscriptionChangeAsync(Func<string, Task<bool>> action, string actionName)
    {
        if (string.IsNullOrWhiteSpace(_selectedUsername))
        {
            MessageBox.Show(this, "يرجى تحديد حساب من القائمة أولاً.", "تنبيه", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        SetStatus($"جارٍ تطبيق ({actionName}) على الحساب [{_selectedUsername}]...", Color.DarkGoldenrod);

        try
        {
            var ok = await action(_selectedUsername);
            if (ok)
            {
                SetStatus($"تم تطبيق ({actionName}) على الحساب [{_selectedUsername}] بنجاح!", Color.LightGreen);
                await RefreshAccountsAsync();
            }
            else
            {
                SetStatus($"فشل تطبيق العملية على [{_selectedUsername}].", Color.LightCoral);
            }
        }
        catch (Exception ex)
        {
            SetStatus($"خطأ: {ex.Message}", Color.LightCoral);
        }
    }

    private async void btnResetHwid_Click(object sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(_selectedUsername))
        {
            MessageBox.Show(this, "يرجى تحديد حساب أولاً.", "تنبيه", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        if (MessageBox.Show(this, $"هل تريد فك ربط جهاز الحساب [{_selectedUsername}]؟\nسيتمكن المستخدم من تسجيل الدخول من جهازه الجديد وسيتم ربطه تلقائياً.", "تصفير الجهاز", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
            return;

        SetStatus($"جارٍ تصفير بصمة الجهاز للحساب [{_selectedUsername}]...", Color.DarkGoldenrod);
        var ok = await _authService.ResetHwidAsync(_selectedUsername);
        if (ok)
        {
            SetStatus($"تم تصفير جهاز [{_selectedUsername}] بنجاح!", Color.LightGreen);
            await RefreshAccountsAsync();
        }
        else
        {
            SetStatus($"فشل تصفير الجهاز.", Color.LightCoral);
        }
    }

    private async void btnToggleBlock_Click(object sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(_selectedUsername) || !_allAccounts.TryGetValue(_selectedUsername, out var acc))
        {
            MessageBox.Show(this, "يرجى تحديد حساب أولاً.", "تنبيه", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        bool ok;
        if (acc.IsBlocked)
        {
            SetStatus($"جارٍ فك الحظر عن [{_selectedUsername}]...", Color.DarkGoldenrod);
            ok = await _authService.UnblockAccountAsync(_selectedUsername);
        }
        else
        {
            SetStatus($"جارٍ حظر [{_selectedUsername}]...", Color.DarkGoldenrod);
            ok = await _authService.BlockAccountAsync(_selectedUsername, "تم الحظر يدوياً بواسطة الأدمن");
        }

        if (ok)
        {
            SetStatus($"تم تحديث حالة الحظر بنجاح!", Color.LightGreen);
            await RefreshAccountsAsync();
        }
        else
        {
            SetStatus($"فشل تحديث حالة الحظر.", Color.LightCoral);
        }
    }

    private async void btnChangePass_Click(object sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(_selectedUsername))
        {
            MessageBox.Show(this, "يرجى تحديد حساب أولاً.", "تنبيه", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        var dlg = new InputDialog("تغيير كلمة المرور", "تغيير كلمة المرور", $"أدخل كلمة المرور الجديدة للحساب [{_selectedUsername}]:");
        if (dlg.ShowDialog(this) == DialogResult.OK && dlg.Value != null && !string.IsNullOrWhiteSpace(dlg.Value.ToString()))
        {
            SetStatus($"جارٍ تغيير كلمة مرور [{_selectedUsername}]...", Color.DarkGoldenrod);
            var ok = await _authService.ChangePasswordAsync(_selectedUsername, dlg.Value.ToString().Trim());
            if (ok)
            {
                SetStatus($"تم تغيير كلمة المرور بنجاح!", Color.LightGreen);
                MessageBox.Show(this, $"تم تغيير كلمة المرور للحساب [{_selectedUsername}] بنجاح!", "نجاح", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
                SetStatus($"فشل تغيير كلمة المرور.", Color.LightCoral);
            }
        }
    }

    private async void btnDeleteAccount_Click(object sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(_selectedUsername))
        {
            MessageBox.Show(this, "يرجى تحديد حساب أولاً.", "تنبيه", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        if (MessageBox.Show(this, $"تحذير: هل أنت متأكد من حذف الحساب [{_selectedUsername}] نهائياً؟\nسيؤدي هذا إلى إفراغ خانة للحسابات المسموح بها.", "حذف الحساب", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes)
            return;

        SetStatus($"جارٍ حذف الحساب [{_selectedUsername}]...", Color.DarkGoldenrod);
        var ok = await _authService.DeleteAccountAsync(_selectedUsername);
        if (ok)
        {
            SetStatus($"تم حذف الحساب بنجاح!", Color.LightGreen);
            _selectedUsername = string.Empty;
            await RefreshAccountsAsync();
        }
        else
        {
            SetStatus($"فشل حذف الحساب.", Color.LightCoral);
        }
    }

    private async void btnToggleAdmin_Click(object sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(_selectedUsername) || !_allAccounts.TryGetValue(_selectedUsername, out var acc))
        {
            MessageBox.Show(this, "يرجى تحديد حساب أولاً.", "تنبيه", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        var newAdminStatus = !acc.IsAdmin;
        SetStatus($"جارٍ تعديل صلاحيات الأدمن للحساب [{_selectedUsername}]...", Color.DarkGoldenrod);
        var ok = await _authService.SetAdminStatusAsync(_selectedUsername, newAdminStatus);
        if (ok)
        {
            SetStatus($"تم تحديث صلاحيات الأدمن بنجاح!", Color.LightGreen);
            await RefreshAccountsAsync();
        }
        else
        {
            SetStatus($"فشل تعديل الصلاحيات.", Color.LightCoral);
        }
    }

    private async void btnNewAccount_Click(object sender, EventArgs e)
    {
        var dlgUser = new InputDialog("إنشاء حساب جديد", "اسم المستخدم", "أدخل اسم المستخدم الجديد:");
        if (dlgUser.ShowDialog(this) != DialogResult.OK || dlgUser.Value == null || string.IsNullOrWhiteSpace(dlgUser.Value.ToString()))
            return;

        var user = dlgUser.Value.ToString().Trim();

        var dlgPass = new InputDialog("كلمة المرور", "كلمة المرور", $"أدخل كلمة المرور للحساب [{user}]:");
        if (dlgPass.ShowDialog(this) != DialogResult.OK || dlgPass.Value == null || string.IsNullOrWhiteSpace(dlgPass.Value.ToString()))
            return;

        var pass = dlgPass.Value.ToString().Trim();

        SetStatus($"جارٍ إنشاء الحساب [{user}]...", Color.DarkGoldenrod);
        var result = await _authService.AdminCreateAccountAsync(user, pass);
        if (result.Success)
        {
            // Give initial 30 days
            await _authService.AddSubscriptionDaysAsync(user, 30);
            SetStatus($"تم إنشاء الحساب [{user}] مع اشتراك شهر بنجاح!", Color.LightGreen);
            MessageBox.Show(this, $"تم إنشاء الحساب [{user}] بنجاح وتم تفعيل اشتراك 30 يوماً له!\nيمكن للمستخدم تسجيل الدخول من جهازه وسيتم ربطه تلقائياً.", "تم الإنشاء", MessageBoxButtons.OK, MessageBoxIcon.Information);
            await RefreshAccountsAsync();
        }
        else
        {
            SetStatus($"فشل إنشاء الحساب: {result.Message}", Color.LightCoral);
            MessageBox.Show(this, result.Message, "خطأ", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
    }

    private async void btnSettings_Click(object sender, EventArgs e)
    {
        var settings = await _authService.GetSettingsAsync();

        var dlgPhone = new InputDialog("رقم الواتساب", "رقم الواتساب", "أدخل رقم الواتساب لإعادة الشحن (مثال: +201150238481):", defaultValue: settings.WhatsAppNumber ?? "+201150238481");
        if (dlgPhone.ShowDialog(this) != DialogResult.OK)
            return;

        var phone = dlgPhone.Value?.ToString()?.Trim();
        if (string.IsNullOrWhiteSpace(phone))
            phone = "+201150238481";

        var cleanPhone = phone.Replace("+", "").Replace(" ", "");

        var dlgMsg = new InputDialog("رسالة إعادة الشحن", "رسالة إعادة الشحن", "أدخل نص الرسالة التي تظهر عند انتهاء الاشتراك:", defaultValue: settings.RechargeMessage ?? "برجاء إرسال رسالة لنا لإعادة شحن البوت");
        if (dlgMsg.ShowDialog(this) != DialogResult.OK)
            return;

        var msg = dlgMsg.Value?.ToString()?.Trim();
        if (string.IsNullOrWhiteSpace(msg))
            msg = "برجاء إرسال رسالة لنا لإعادة شحن البوت";

        settings.WhatsAppNumber = phone;
        settings.WhatsAppUrl = $"https://wa.me/{cleanPhone}";
        settings.RechargeMessage = msg;

        SetStatus("جارٍ حفظ إعدادات الواتساب...", Color.DarkGoldenrod);
        var ok = await _authService.SaveSettingsAsync(settings);
        if (ok)
        {
            SetStatus("تم حفظ إعدادات الواتساب بنجاح!", Color.LightGreen);
            MessageBox.Show(this, $"تم تحديث إعدادات الواتساب:\nالرقم: {phone}\nالرسالة: {msg}", "تم الحفظ", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        else
        {
            SetStatus("فشل حفظ إعدادات الواتساب.", Color.LightCoral);
        }
    }

    private void SetStatus(string text, Color color)
    {
        lblStatus.Text = text;
        lblStatus.ForeColor = color;
    }

    private void btnClose_Click(object sender, EventArgs e)
    {
        Close();
    }
}
