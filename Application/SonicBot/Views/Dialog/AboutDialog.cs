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
        labelName.Text = Program.AssemblyTitle;
        labelDescription.Text = Program.AssemblyDescription;
        labelVersion.Text = Program.AssemblyVersion;

        try
        {
            var sprite = SonicBranding.Sprite;
            if (sprite != null)
                pictureBox1.Image = sprite;
            else
                pictureBox1.Image = Properties.Resources.app;
        }
        catch
        {
            pictureBox1.Image = Properties.Resources.app;
        }
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

