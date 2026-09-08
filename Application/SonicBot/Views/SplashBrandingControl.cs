using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.Windows.Forms;

namespace RSBot.Views;

/// <summary>
///     Paints the golden "SONIC BOT" title (scalable, vector font) with the
///     version string centered inside the 'O' of "BOT".
/// </summary>
internal sealed class SplashBrandingControl : Control
{
    private string _versionText = "v2.9.5";

    public string VersionText
    {
        get => _versionText;
        set
        {
            _versionText = value;
            Invalidate();
        }
    }

    public SplashBrandingControl()
    {
        SetStyle(
            ControlStyles.AllPaintingInWmPaint |
            ControlStyles.OptimizedDoubleBuffer |
            ControlStyles.UserPaint |
            ControlStyles.ResizeRedraw |
            ControlStyles.SupportsTransparentBackColor,
            true);
        BackColor = Color.Transparent;
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);

        var g = e.Graphics;
        g.SmoothingMode = SmoothingMode.AntiAlias;
        g.TextRenderingHint = TextRenderingHint.AntiAliasGridFit;

        if (Width <= 0 || Height <= 0)
            return;

        const string title = "AEROBOT";
        const string prefix = "AEROB";

        float titlePx = Math.Max(24f, Height * 0.58f);

        using var titleFont = new Font("Segoe UI Black", titlePx, FontStyle.Bold, GraphicsUnit.Pixel);
        using var measureFormat = new StringFormat(StringFormat.GenericTypographic);

        var total = g.MeasureString(title, titleFont, PointF.Empty, measureFormat);
        var prefixSize = g.MeasureString(prefix, titleFont, PointF.Empty, measureFormat);
        var oSize = g.MeasureString("O", titleFont, PointF.Empty, measureFormat);

        float startX = Math.Max(0f, (ClientSize.Width - total.Width) / 2f);
        float startY = Math.Max(0f, (ClientSize.Height - total.Height) / 2f) - 10f;

        // Draw soft drop shadow for depth
        using (var shadowBrush = new SolidBrush(Color.FromArgb(120, 0, 0, 0)))
        {
            g.DrawString(title, titleFont, shadowBrush, startX + 3f, startY + 3f);
        }

        // Draw radiant Silkroad Sun Gold gradient
        using (var gold = new LinearGradientBrush(
                   new RectangleF(startX, startY, total.Width, total.Height),
                   Color.FromArgb(255, 230, 120),
                   Color.FromArgb(195, 145, 30),
                   LinearGradientMode.Vertical))
        {
            g.DrawString(title, titleFont, gold, startX, startY);
        }

        // Subtitle: SILKROAD ONLINE
        const string subtitle = "SILKROAD ONLINE";
        float subPx = titlePx * 0.20f;
        using (var subFont = new Font("Segoe UI", subPx, FontStyle.Bold, GraphicsUnit.Pixel))
        using (var cyanBrush = new SolidBrush(Color.FromArgb(100, 210, 255)))
        {
            var subSize = g.MeasureString(subtitle, subFont, PointF.Empty, measureFormat);
            float subX = Math.Max(0f, (ClientSize.Width - subSize.Width) / 2f);
            float subY = startY + total.Height + 2f;
            g.DrawString(subtitle, subFont, cyanBrush, subX, subY);
        }

        float oCenterX = startX + prefixSize.Width + oSize.Width / 2f;
        float oCenterY = startY + total.Height / 2f;

        if (string.IsNullOrWhiteSpace(_versionText))
            return;

        float versionPx = titlePx * 0.15f;
        using (var versionFont = new Font("Segoe UI", versionPx, FontStyle.Bold, GraphicsUnit.Pixel))
        {
            var versionSize = g.MeasureString(_versionText, versionFont, PointF.Empty, measureFormat);

            float maxWidth = oSize.Width * 0.60f;
            Font finalFont = versionFont;
            if (versionSize.Width > maxWidth)
            {
                versionFont.Dispose();
                versionPx = Math.Max(6f, versionPx * (maxWidth / versionSize.Width) * 0.95f);
                finalFont = new Font("Segoe UI", versionPx, FontStyle.Bold, GraphicsUnit.Pixel);
                versionSize = g.MeasureString(_versionText, finalFont, PointF.Empty, measureFormat);
            }

            using (finalFont)
            using (var white = new SolidBrush(Color.White))
            {
                g.DrawString(
                    _versionText,
                    finalFont,
                    white,
                    oCenterX - versionSize.Width / 2f,
                    oCenterY - versionSize.Height / 2f);
            }
        }
    }
}