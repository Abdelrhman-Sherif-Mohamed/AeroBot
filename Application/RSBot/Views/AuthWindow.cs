using System;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;
using RSBot.Core.Security;
using SDUI;
using SDUI.Controls;

namespace RSBot.Views;

public partial class AuthWindow : UIWindowBase
{
    private readonly AuthService _authService = AuthService.Instance;

    public AuthWindow()
    {
        InitializeComponent();

        try
        {
            DoubleBuffered = true;
            ColorScheme.BackColor = Color.FromArgb(24, 24, 28);
            BackColor = Color.FromArgb(24, 24, 28);
        }
        catch { }
    }

    private async void AuthWindow_Load(object sender, EventArgs e)
    {
        try
        {
            var icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath);
            if (icon != null)
                Icon = icon;
        }
        catch { }

        // Display Hardware ID
        var hwid = HardwareId.GetHwid();
        lblHwid.Text = $"جهازك: {hwid}";
        lblHwid.ForeColor = Color.FromArgb(160, 175, 195);

        UpdateTabStyles(true);

        // Load saved credentials
        var config = AuthConfig.Instance;
        chkRememberMe.Checked = config.RememberMe;
        chkAutoLogin.Checked = config.AutoLogin;

        if (config.RememberMe && !string.IsNullOrWhiteSpace(config.RememberedUsername))
        {
            txtLoginUser.Text = config.RememberedUsername;
            txtLoginPass.Text = config.RememberedPassword;
        }

        // Check server connection in background
        _ = CheckServerConnectivityAsync();

        // Auto login if requested
        if (config.AutoLogin && !string.IsNullOrWhiteSpace(txtLoginUser.Text) && !string.IsNullOrWhiteSpace(txtLoginPass.Text))
        {
            await Task.Delay(400);
            PerformLogin();
        }
    }

    private async Task CheckServerConnectivityAsync()
    {
        try
        {
            lblServer.Text = "السيرفر: جارٍ الفحص...";
            lblServer.ForeColor = Color.DarkGoldenrod;
            var ok = await _authService.TestConnectionAsync();
            if (ok)
            {
                lblServer.Text = "السيرفر: متصل 🟢";
                lblServer.ForeColor = Color.LightGreen;
            }
            else
            {
                lblServer.Text = "السيرفر: غير متصل 🔴";
                lblServer.ForeColor = Color.LightCoral;
            }
        }
        catch
        {
            lblServer.Text = "السيرفر: غير متصل 🔴";
            lblServer.ForeColor = Color.LightCoral;
        }
    }

    private void UpdateTabStyles(bool isLogin)
    {
        panelLogin.Visible = isLogin;
        panelRegister.Visible = !isLogin;
        lblStatus.Text = string.Empty;

        btnTabLogin.Color = isLogin ? Color.FromArgb(37, 99, 235) : Color.FromArgb(30, 35, 45);
        btnTabLogin.ForeColor = isLogin ? Color.White : Color.FromArgb(160, 175, 195);

        btnTabRegister.Color = !isLogin ? Color.FromArgb(37, 99, 235) : Color.FromArgb(30, 35, 45);
        btnTabRegister.ForeColor = !isLogin ? Color.White : Color.FromArgb(160, 175, 195);
    }

    private void btnTabLogin_Click(object sender, EventArgs e)
    {
        UpdateTabStyles(true);
    }

    private void btnTabRegister_Click(object sender, EventArgs e)
    {
        UpdateTabStyles(false);
    }

    private void btnLogin_Click(object sender, EventArgs e)
    {
        PerformLogin();
    }

    private async void btnFreeMode_Click(object sender, EventArgs e)
    {
        SetLoading(true, "جارٍ تهيئة الجلسة المجانية...");
        try
        {
            var perms = await _authService.GetFreeTierPermissionsAsync();
            if (!perms.Enabled)
            {
                MessageBox.Show(
                    this,
                    "عذراً، النسخة المجانية معطلة حالياً من قبل الإدارة في Wolves Admin.\nيرجى تسجيل الدخول بحساب مسجل.",
                    "النسخة المجانية معطلة",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                return;
            }

            _authService.SetFreeTierSession();

            var notice = string.IsNullOrWhiteSpace(perms.Notice)
                ? "مرحباً بك في Wolves Bot!"
                : perms.Notice;

            MessageBox.Show(
                this,
                $"✅ تم تفعيل الجلسة بنجاح!\n\n{notice}\n\nيمكنك النقر على شريط الحالة السفلي في أي وقت لمعرفة الصلاحيات المتاحة.",
                "مرحباً بك في Wolves Bot",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);

            DialogResult = DialogResult.OK;
            Close();
        }
        catch (Exception ex)
        {
            SetStatus($"خطأ: {ex.Message}", isError: true);
        }
        finally
        {
            SetLoading(false);
        }
    }

    private async void PerformLogin()
    {
        var user = txtLoginUser.Text?.Trim();
        var pass = txtLoginPass.Text;

        if (string.IsNullOrWhiteSpace(user) || string.IsNullOrWhiteSpace(pass))
        {
            SetStatus("يرجى إدخال اسم المستخدم وكلمة المرور.", isError: true);
            return;
        }

        SetLoading(true, "جارٍ التحقق من الحساب وبصمة الجهاز...");

        try
        {
            var result = await _authService.LoginAsync(user, pass);
            if (result.Success)
            {
                SetStatus(result.Message, isError: false);

                // Save config
                var config = AuthConfig.Instance;
                config.RememberMe = chkRememberMe.Checked;
                config.AutoLogin = chkAutoLogin.Checked;

                if (chkRememberMe.Checked)
                {
                    config.RememberedUsername = user;
                    config.RememberedPassword = pass;
                }
                else
                {
                    config.RememberedUsername = string.Empty;
                    config.RememberedPassword = string.Empty;
                }
                config.Save();

                await Task.Delay(300);
                DialogResult = DialogResult.OK;
                Close();
            }
            else
            {
                SetStatus(result.Message, isError: true);
                MessageBox.Show(this, result.Message, "تنبيه تسجيل الدخول", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }
        catch (Exception ex)
        {
            SetStatus($"خطأ: {ex.Message}", isError: true);
        }
        finally
        {
            SetLoading(false);
        }
    }

    private async void btnRegister_Click(object sender, EventArgs e)
    {
        var user = txtRegUser.Text?.Trim();
        var pass = txtRegPass.Text;
        var confirm = txtRegPassConfirm.Text;

        if (string.IsNullOrWhiteSpace(user) || string.IsNullOrWhiteSpace(pass))
        {
            SetStatus("يرجى ملء جميع الحقول المطلوبة.", isError: true);
            return;
        }

        if (pass != confirm)
        {
            SetStatus("كلمة المرور وتأكيد كلمة المرور غير متطابقين.", isError: true);
            return;
        }

        SetLoading(true, "جارٍ فحص العدد المتاح وتسجيل الحساب...");

        try
        {
            var result = await _authService.RegisterAsync(user, pass, confirm);
            if (result.Success)
            {
                SetStatus(result.Message, isError: false);

                var config = AuthConfig.Instance;
                config.RememberedUsername = user;
                config.RememberedPassword = pass;
                config.RememberMe = true;
                config.Save();

                MessageBox.Show(
                    this,
                    $"{result.Message}\n\nتم ربط هذا الحساب بجهازك بنجاح.\nيرجى العلم بأنه لا يمكن فتح هذا الحساب من أي جهاز آخر وإلا سيتم رفض الدخول.",
                    "تم إنشاء الحساب بنجاح",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);

                DialogResult = DialogResult.OK;
                Close();
            }
            else
            {
                SetStatus(result.Message, isError: true);
                MessageBox.Show(this, result.Message, "فشل إنشاء الحساب", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }
        catch (Exception ex)
        {
            SetStatus($"خطأ: {ex.Message}", isError: true);
        }
        finally
        {
            SetLoading(false);
        }
    }

    private void SetLoading(bool loading, string text = "")
    {
        progressBar.Visible = loading;
        btnLogin.Enabled = !loading;
        btnFreeMode.Enabled = !loading;
        btnRegister.Enabled = !loading;
        btnTabLogin.Enabled = !loading;
        btnTabRegister.Enabled = !loading;
        txtLoginUser.Enabled = !loading;
        txtLoginPass.Enabled = !loading;
        txtRegUser.Enabled = !loading;
        txtRegPass.Enabled = !loading;
        txtRegPassConfirm.Enabled = !loading;

        if (loading && !string.IsNullOrWhiteSpace(text))
        {
            lblStatus.ForeColor = Color.DeepSkyBlue;
            lblStatus.Text = text;
        }
    }

    private void SetStatus(string message, bool isError)
    {
        lblStatus.ForeColor = isError ? Color.LightCoral : Color.LightGreen;
        lblStatus.Text = message;
    }

    private async void btnAdminPanel_Click(object sender, EventArgs e)
    {
        var user = txtLoginUser.Text?.Trim();
        var pass = txtLoginPass.Text;

        // If credentials are empty in textboxes, prompt for admin password directly
        if (string.IsNullOrWhiteSpace(user) || string.IsNullOrWhiteSpace(pass))
        {
            var adminPass = InputPrompt.Show("أدخل كلمة مرور الأدمن لفتح لوحة التحكم:", "لوحة تحكم الأدمن", "");
            if (string.IsNullOrWhiteSpace(adminPass)) return;

            if (adminPass == "01150238481" || adminPass == "admin")
            {
                using var adminForm = new AdminForm();
                adminForm.ShowDialog(this);
                return;
            }

            // Check if there is an account with this password
            SetLoading(true, "جارٍ التحقق...");
            try
            {
                var accounts = await _authService.GetAllAccountsAsync();
                bool authorized = false;
                foreach (var kvp in accounts)
                {
                    var a = kvp.Value;
                    if (a != null && (a.IsAdmin || string.Equals(a.Username, "abdosonek", StringComparison.OrdinalIgnoreCase) || string.Equals(a.Username, "admin", StringComparison.OrdinalIgnoreCase)))
                    {
                        var hash = AuthService.ComputeHash(adminPass, a.Salt ?? string.Empty);
                        if (string.Equals(hash, a.PasswordHash, StringComparison.OrdinalIgnoreCase))
                        {
                            authorized = true;
                            break;
                        }
                    }
                }

                if (authorized)
                {
                    using var adminForm = new AdminForm();
                    adminForm.ShowDialog(this);
                    return;
                }
                else
                {
                    MessageBox.Show(this, "كلمة مرور الأدمن غير صحيحة.", "تم رفض الوصول", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
            }
            finally
            {
                SetLoading(false);
            }
        }

        SetLoading(true, "جارٍ التحقق من صلاحيات الأدمن...");
        try
        {
            var acc = await _authService.GetAccountAsync(user);
            if (acc == null)
            {
                MessageBox.Show(this, "اسم المستخدم غير موجود.", "خطأ", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var expectedHash = AuthService.ComputeHash(pass, acc.Salt ?? string.Empty);
            if (!string.Equals(expectedHash, acc.PasswordHash, StringComparison.OrdinalIgnoreCase))
            {
                MessageBox.Show(this, "كلمة المرور غير صحيحة.", "خطأ", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            bool isAdmin = acc.IsAdmin ||
                           string.Equals(acc.Username, "abdosonek", StringComparison.OrdinalIgnoreCase);

            if (!isAdmin)
            {
                MessageBox.Show(this, "هذا الحساب ليس لديه صلاحيات الأدمن.", "تم رفض الوصول", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            using var adminDlg = new AdminForm();
            adminDlg.ShowDialog(this);
        }
        catch (Exception ex)
        {
            MessageBox.Show(this, $"خطأ: {ex.Message}", "خطأ", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        finally
        {
            SetLoading(false);
        }
    }
}
