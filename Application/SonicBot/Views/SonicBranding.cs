using System;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Windows.Forms;

namespace RSBot.Views;

/// <summary>
///     Sonic branding: exe icon for windows/tray + Sonic art for
///     splash/about pages (embedded sonic_splash.png, may be absent).
/// </summary>
internal static class SonicBranding
{
    private static Image _sprite;
    private static Image _art;
    private static bool _spriteLoaded;
    private static bool _artLoaded;

    /// <summary>
    ///     Full-resolution Sonic pixel-art sprite (embedded resource or sonic.png).
    /// </summary>
    public static Image Sprite
    {
        get
        {
            if (_spriteLoaded)
                return _sprite;

            _spriteLoaded = true;
            _sprite = LoadOriginal();
            return _sprite;
        }
    }

    public static Image Art
    {
        get
        {
            if (_artLoaded)
                return _art;

            _artLoaded = true;
            try
            {
                using var img = LoadOriginal();
                if (img == null)
                    return null;

                // Fit inside the splash strip while keeping the aspect ratio
                float ratio = Math.Min(440f / img.Width, 40f / img.Height);
                var fit = new Size(
                    Math.Max(1, (int)(img.Width * ratio)),
                    Math.Max(1, (int)(img.Height * ratio)));
                _art = new Bitmap(img, fit);
                return _art;
            }
            catch
            {
                return null;
            }
        }
    }

    private static Image LoadOriginal()
    {
        // 1) Try embedded resource (RSBot.sonic_splash.png)
        try
        {
            var asm = Assembly.GetExecutingAssembly();
            using var stream = asm.GetManifestResourceStream("RSBot.sonic_splash.png");
            if (stream != null)
            {
                using var copy = new MemoryStream();
                stream.CopyTo(copy);
                copy.Position = 0;
                using var tmp = Image.FromStream(copy);
                return new Bitmap(tmp);
            }
        }
        catch { /* fall through to file */ }

        // 2) Fallback: load from file next to the executable
        try
        {
            string aeroPath = Path.Combine(AppContext.BaseDirectory, "aerobot.png");
            if (File.Exists(aeroPath))
                return new Bitmap(aeroPath);

            string fallbackPath = Path.Combine(AppContext.BaseDirectory, "sonic.png");
            if (File.Exists(fallbackPath))
                return new Bitmap(fallbackPath);
        }
        catch { /* give up */ }

        return null;
    }

    public static void ApplyWindowIcon(Form form)
    {
        try
        {
            var icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath);
            if (icon == null)
                return;

            form.Icon = icon;

            // Main window tray icon follows the exe icon too
            if (form is Main main)
                main.SetTrayIcon(icon);
        }
        catch
        {
        }
    }
}
