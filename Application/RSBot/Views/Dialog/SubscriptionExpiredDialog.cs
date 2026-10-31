using System;
using System.Diagnostics;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;
using RSBot.Core.Security;
using SDUI;
using SDUI.Controls;

namespace RSBot.Views;

public partial class SubscriptionExpiredDialog : UIWindowBase
{
    private string _whatsAppNumber = "+201150238481";
    private string _whatsAppUrl = "https://wa.me/201150238481";
    private string _rechargeMessage = "برجاء إرسال رسالة لنا لإعادة شحن البوت";
    private readonly bool _shouldExitOnClose;

    public SubscriptionExpiredDialog(bool shouldExitOnClose = true)
    {
        _shouldExitOnClose = shouldExitOnClose;
        InitializeComponent();

        try
        {
            DoubleBuffered = true;
            ColorScheme.BackColor = Color.FromArgb(24, 24, 28);
            BackColor = Color.FromArgb(24, 24, 28);
        }
        catch { }

        Load += SubscriptionExpiredDialog_Load;
        FormClosed += SubscriptionExpiredDialog_FormClosed;
    }

    private async void SubscriptionExpiredDialog_Load(object sender, EventArgs e)
    {
        try
        {
            var settings = await AuthService.Instance.GetSettingsAsync();
            if (!string.IsNullOrWhiteSpace(settings.WhatsAppNumber))
                _whatsAppNumber = settings.WhatsAppNumber;

            if (!string.IsNullOrWhiteSpace(settings.WhatsAppUrl))
                _whatsAppUrl = settings.WhatsAppUrl;
            else
                _whatsAppUrl = $"https://wa.me/{_whatsAppNumber.Replace("+", "").Replace(" ", "")}";

            if (!string.IsNullOrWhiteSpace(settings.RechargeMessage))
                _rechargeMessage = settings.RechargeMessage;

            lblMessage.Text = _rechargeMessage;
            lblWhatsAppNumber.Text = $"WhatsApp: {_whatsAppNumber}";
        }
        catch { }
    }

    private void btnWhatsApp_Click(object sender, EventArgs e)
    {
        try
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = _whatsAppUrl,
                UseShellExecute = true
            });
        }
        catch (Exception ex)
        {
            MessageBox.Show(this, $"WhatsApp: {_whatsAppNumber}\n\n{ex.Message}", "WhatsApp", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
    }

    private void btnCopyNumber_Click(object sender, EventArgs e)
    {
        try
        {
            Clipboard.SetText(_whatsAppNumber);
            btnCopyNumber.Text = "تم نسخ الرقم بنجاح! ✔";
            btnCopyNumber.Color = Color.FromArgb(40, 120, 70);
        }
        catch
        {
            btnCopyNumber.Text = _whatsAppNumber;
        }
    }

    private void btnClose_Click(object sender, EventArgs e)
    {
        DialogResult = DialogResult.OK;
        Close();
    }

    private void SubscriptionExpiredDialog_FormClosed(object sender, FormClosedEventArgs e)
    {
        if (_shouldExitOnClose)
        {
            Environment.Exit(0);
        }
    }
}
