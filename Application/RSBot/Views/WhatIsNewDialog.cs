using System;
using System.Drawing;
using System.IO;
using System.Text;
using System.Windows.Forms;
using RSBot.Core.Components;
using SDUI.Controls;

namespace RSBot.Views;

public partial class WhatIsNewDialog : UIWindowBase
{
    private readonly AppUpdateInfo _updateInfo;
    private readonly bool _isArabic;

    public WhatIsNewDialog(AppUpdateInfo updateInfo)
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
            Text = "Aerobot - تم التحديث بنجاح";
            lblTitle.Text = $"تم تحديث Aerobot بنجاح إلى الإصدار ({_updateInfo.Version})";
            lblSub.Text = "أهلاً بك في النسخة الجديدة! إليك قائمة التغييرات والميزات المضافة:";
            lblChangelogTitle.Text = "ما الجديد في هذا التحديث:";
            txtChangelog.RightToLeft = RightToLeft.Yes;
            txtChangelog.TextAlign = HorizontalAlignment.Right;
            txtChangelog.Text = UpdateAvailableDialog.FormatChangelogForDisplay(_updateInfo.GetChangelog(true));
            btnOk.Text = "حسناً، متابعة التشغيل";
        }
        else
        {
            RightToLeft = RightToLeft.No;
            RightToLeftLayout = false;
            Text = "Aerobot - Update Successful";
            lblTitle.Text = $"Aerobot successfully updated to version ({_updateInfo.Version})";
            lblSub.Text = "Welcome to the new release! Here is what's new and improved:";
            lblChangelogTitle.Text = "What's new in this update:";
            txtChangelog.RightToLeft = RightToLeft.No;
            txtChangelog.TextAlign = HorizontalAlignment.Left;
            txtChangelog.Text = UpdateAvailableDialog.FormatChangelogForDisplay(_updateInfo.GetChangelog(false));
            btnOk.Text = "OK, Continue";
        }
    }

    private void btnOk_Click(object sender, EventArgs e)
    {
        DialogResult = DialogResult.OK;
        Close();
    }
}
