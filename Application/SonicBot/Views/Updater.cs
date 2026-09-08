using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Net.Http;
using System.Reflection;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Forms;
using RSBot.Core;
using SDUI;

namespace RSBot.Views;

public partial class Updater : Form
{
    private const string VersionCheckUrl = "https://raw.githubusercontent.com/Abdelrhman-Sherif-Mohamed/AeroBot/main/version.json";
    private static readonly HttpClient _httpClient = new() { Timeout = TimeSpan.FromSeconds(7) };

    private string _downloadUrl = "https://github.com/Abdelrhman-Sherif-Mohamed/AeroBot/releases/latest";
    private string _githubUrl = "https://github.com/Abdelrhman-Sherif-Mohamed/AeroBot";

    public class VersionManifest
    {
        public string version { get; set; }
        public string release_date { get; set; }
        public string download_url { get; set; }
        public string github_url { get; set; }
        public string changelog { get; set; }
        public bool required { get; set; }
    }

    public Updater()
    {
        InitializeComponent();
        CheckForIllegalCrossThreadCalls = false;
        ApplyTheme();
        btnSkip.Click += (s, e) => { DialogResult = DialogResult.Cancel; Close(); };
    }

    private void ApplyTheme()
    {
        try
        {
            var isDark = ColorScheme.BackColor.IsDark();
            BackColor = isDark ? Color.FromArgb(15, 23, 42) : Color.FromArgb(248, 250, 252);
            ForeColor = isDark ? Color.FromArgb(241, 245, 249) : Color.FromArgb(30, 41, 59);

            lblInfo.ForeColor = isDark ? Color.White : Color.Black;
            cbChangeLog.ForeColor = isDark ? Color.FromArgb(203, 213, 225) : Color.FromArgb(71, 85, 105);

            rtbUpdateInfo.BackColor = isDark ? Color.FromArgb(30, 41, 59) : Color.White;
            rtbUpdateInfo.ForeColor = isDark ? Color.FromArgb(226, 232, 240) : Color.Black;

            btnDownload.Color = Color.FromArgb(59, 130, 246);
            btnDownload.ForeColor = Color.White;
            btnSkip.Color = isDark ? Color.FromArgb(51, 65, 85) : Color.FromArgb(203, 213, 225);
            btnSkip.ForeColor = isDark ? Color.White : Color.Black;
        }
        catch
        {
        }
    }

    /// <summary>
    /// Current assembly version
    /// </summary>
    private Version CurrentVersion
    {
        get
        {
            var v = Assembly.GetExecutingAssembly().GetName().Version;
            return v ?? new Version(1, 0, 0);
        }
    }

    private void Append(string text, Color color, FontStyle fontStyle = FontStyle.Regular, float emSize = 0)
    {
        rtbUpdateInfo.SuspendLayout();
        rtbUpdateInfo.Select(rtbUpdateInfo.TextLength, 0);
        rtbUpdateInfo.SelectionColor = color;
        rtbUpdateInfo.SelectionFont = new Font(Font.FontFamily, emSize == 0 ? rtbUpdateInfo.Font.Size : emSize, fontStyle);
        rtbUpdateInfo.AppendText(text + Environment.NewLine);
        rtbUpdateInfo.ResumeLayout();
    }

    private void btnDownload_Click(object sender, EventArgs e)
    {
        try
        {
            var target = !string.IsNullOrWhiteSpace(_downloadUrl) ? _downloadUrl : _githubUrl;
            Process.Start(new ProcessStartInfo
            {
                FileName = target,
                UseShellExecute = true
            });
            DialogResult = DialogResult.OK;
            Close();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"تعذر فتح رابط التحميل: {ex.Message}", "AeroBot", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
    }

    private async void cbChangeLog_CheckedChanged(object sender, EventArgs e)
    {
        await Task.Run(() =>
        {
            try
            {
                if (cbChangeLog.Checked)
                {
                    for (var i = Height; i <= 350; i += 8)
                    {
                        if (IsDisposed) return;
                        Invoke(new Action(() => Height = i));
                        System.Threading.Thread.Sleep(5);
                    }
                }
                else
                {
                    for (var i = Height; i >= 78; i -= 8)
                    {
                        if (IsDisposed) return;
                        Invoke(new Action(() => Height = i));
                        System.Threading.Thread.Sleep(5);
                    }
                }
            }
            catch
            {
            }
        });
    }

    /// <summary>
    /// Checks for updates against GitHub repository raw version.json.
    /// </summary>
    /// <param name="manual">If true, shows messages even if no update is available or on error.</param>
    /// <returns>True if a newer version is available.</returns>
    public async Task<bool> Check(bool manual = false)
    {
        try
        {
            var json = await _httpClient.GetStringAsync(VersionCheckUrl);
            if (string.IsNullOrWhiteSpace(json))
                return false;

            var manifest = JsonSerializer.Deserialize<VersionManifest>(json);
            if (manifest == null || string.IsNullOrWhiteSpace(manifest.version))
                return false;

            if (!string.IsNullOrWhiteSpace(manifest.download_url))
                _downloadUrl = manifest.download_url;
            if (!string.IsNullOrWhiteSpace(manifest.github_url))
                _githubUrl = manifest.github_url;

            if (Version.TryParse(manifest.version, out var remoteVersion))
            {
                if (remoteVersion > CurrentVersion)
                {
                    lblInfo.Text = $"تحديث جديد متاح: v{manifest.version}";
                    btnDownload.Text = "تحديث الآن";
                    btnSkip.Text = "لاحقاً";
                    cbChangeLog.Text = "سجل التغييرات";

                    rtbUpdateInfo.Clear();
                    Append($"AeroBot v{manifest.version}", Color.FromArgb(56, 189, 248), FontStyle.Bold, 11);
                    if (!string.IsNullOrWhiteSpace(manifest.release_date))
                        Append($"تاريخ الإصدار: {manifest.release_date}", Color.Gray, FontStyle.Italic, 8.5f);
                    Append(string.Empty, Color.Black);

                    if (!string.IsNullOrWhiteSpace(manifest.changelog))
                    {
                        var lines = manifest.changelog.Split(new[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries);
                        foreach (var line in lines)
                        {
                            Append(line, ColorScheme.BackColor.IsDark() ? Color.FromArgb(226, 232, 240) : Color.DarkSlateGray, FontStyle.Regular, 9.5f);
                        }
                    }

                    rtbUpdateInfo.SelectionStart = 0;
                    return true;
                }
            }

            if (manual)
            {
                MessageBox.Show($"أنت تستخدم أحدث إصدار من AeroBot (v{CurrentVersion.ToString(3)})!\nلا توجد تحديثات جديدة حالياً.", "AeroBot Updater", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }
        catch (Exception ex)
        {
            if (manual)
            {
                MessageBox.Show($"فحص التحديثات غير متاح حالياً:\n{ex.Message}\n\nيمكنك زيارة صفحة المشروع على GitHub يدوياً.", "AeroBot Updater", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        return false;
    }
}
