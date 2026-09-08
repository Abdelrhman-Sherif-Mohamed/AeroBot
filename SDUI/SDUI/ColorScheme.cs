using System.Drawing;

namespace SDUI;

public class ColorScheme
{
    /// <summary>
    /// Gets or Sets to the theme back color
    /// </summary>
    public static Color BackColor = Color.White;

    /// <summary>
    /// Get Determined theme forecolor from backcolor
    /// </summary>
    public static Color ForeColor => BackColor.Determine();

    /// <summary>
    /// Gets theme border color
    /// </summary>
    public static Color BorderColor => BackColor.IsDark() ? Color.FromArgb(30, 41, 59) : ForeColor.Alpha(40);

    /// <summary>
    /// Gets theme back color 2
    /// </summary>
    public static Color BackColor2 => ForeColor.Alpha(10);

    /// <summary>
    /// Gets theme back color 2
    /// </summary>
    public static Color ShadowColor => ForeColor.Alpha(10);

    /// <summary>
    /// Gets theme accent color (vibrant sky-blue/cyan on dark themes, blue on light themes)
    /// </summary>
    public static Color AccentColor => BackColor.IsDark() ? Color.FromArgb(56, 189, 248) : Color.FromArgb(0, 114, 245);

    /// <summary>
    /// Gets or sets the debug borders
    /// </summary>
    public static bool DrawDebugBorders;
}
