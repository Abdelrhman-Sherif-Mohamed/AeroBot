using System;
using System.Drawing;
using System.IO;
using System.Text;
using System.Windows.Forms;
using RSBot.Core.Components;
using SDUI.Controls;

namespace RSBot.Views;

public partial class UpdateAvailableDialog : UIWindowBase
{
    private readonly AppUpdateInfo _updateInfo;
    private readonly bool _isArabic;

    public UpdateAvailableDialog(AppUpdateInfo updateInfo)
    {
        _updateInfo = updateInfo ?? throw new ArgumentNullException(nameof(updateInfo));
        _isArabic = UpdateManager.IsArabic;

        InitializeComponent();

        try
        {
            var iconPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "app.ico");
            if (File.Exists(iconPath))
                Icon = new Icon(iconPath);
        }
        catch { }

        ApplyLanguage();
    }

    private void ApplyLanguage()
    {
        if (_isArabic)
        {
            RightToLeft = RightToLeft.Yes;
            RightToLeftLayout = true;
            Text = "Aerobot - تحديث جديد متاح";
            lblTitle.Text = $"يتوفر إصدار جديد من Aerobot ({_updateInfo.Version})";
            lblVersionInfo.Text = $"الإصدار الحالي: {UpdateManager.CurrentVersion}   ←   الإصدار الجديد: {_updateInfo.Version}";
            lblChangelogTitle.Text = "ما الجديد في هذا الإصدار:";
            txtChangelog.RightToLeft = RightToLeft.Yes;
            txtChangelog.TextAlign = HorizontalAlignment.Right;
            txtChangelog.Text = FormatChangelogForDisplay(_updateInfo.GetChangelog(true));
            btnUpdate.Text = "تحديث الآن";
            btnLater.Text = "لاحقاً";
        }
        else
        {
            RightToLeft = RightToLeft.No;
            RightToLeftLayout = false;
            Text = "Aerobot - New Update Available";
            lblTitle.Text = $"A new version of Aerobot is available ({_updateInfo.Version})";
            lblVersionInfo.Text = $"Current: {UpdateManager.CurrentVersion}   →   New: {_updateInfo.Version}";
            lblChangelogTitle.Text = "What's new in this release:";
            txtChangelog.RightToLeft = RightToLeft.No;
            txtChangelog.TextAlign = HorizontalAlignment.Left;
            txtChangelog.Text = FormatChangelogForDisplay(_updateInfo.GetChangelog(false));
            btnUpdate.Text = "Update Now";
            btnLater.Text = "Later";
        }

        if (_updateInfo.Mandatory)
        {
            btnLater.Visible = false;
            btnUpdate.Dock = DockStyle.Fill;
        }
    }

    public static string FormatChangelogForDisplay(string raw)
    {
        if (string.IsNullOrWhiteSpace(raw)) return string.Empty;

        var normalized = raw.Replace("\r\n", "\n").Replace("\r", "\n");
        var lines = normalized.Split('\n');
        var sb = new StringBuilder();

        foreach (var rawLine in lines)
        {
            var line = rawLine?.Trim();
            if (string.IsNullOrWhiteSpace(line)) continue;

            // Replace leading digits like "1. ", "2. ", "1 - " with clean bullets
            if (char.IsDigit(line[0]) && line.Length > 2)
            {
                var idx = line.IndexOfAny(new[] { '.', '-', ':' });
                if (idx > 0 && idx < 4)
                    line = line.Substring(idx + 1).Trim();
            }

            if (!line.StartsWith("• ") && !line.StartsWith("- ") && !line.StartsWith("* "))
            {
                line = "•  " + line;
            }

            sb.AppendLine(line);
            sb.AppendLine(); // Blank line between points for optimal readability
        }

        return sb.ToString().TrimEnd();
    }

    private void btnUpdate_Click(object sender, EventArgs e)
    {
        DialogResult = DialogResult.OK;
        Close();
    }

    private void btnLater_Click(object sender, EventArgs e)
    {
        DialogResult = DialogResult.Cancel;
        Close();
    }
}
