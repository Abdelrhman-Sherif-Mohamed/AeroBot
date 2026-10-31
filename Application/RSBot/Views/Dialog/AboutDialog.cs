using System;
using System.Diagnostics;
using System.Windows.Forms;
using SDUI.Controls;

namespace RSBot.Views;

internal partial class AboutDialog : UIWindowBase
{
    public AboutDialog()
    {
        InitializeComponent();
        labelName.Text = "Aerobot";
        labelDescription.Text = "Advanced Bot for Silkroad Online";
        labelVersion.Text = $"v{RSBot.Core.Components.UpdateManager.DisplayVersion}";
        labelDeveloper.Text = "(c) Developed by Abdelrhman Sherif";
    }

    private void btnWhatsApp_Click(object sender, EventArgs e)
    {
        try
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = "https://wa.me/201150238481",
                UseShellExecute = true
            });
        }
        catch (Exception ex)
        {
            MessageBox.Show(this, "WhatsApp Link: https://wa.me/201150238481\r\n" + ex.Message, "WhatsApp", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
    }

    private void buttonOk_Click(object sender, EventArgs e)
    {
        Close();
    }
}