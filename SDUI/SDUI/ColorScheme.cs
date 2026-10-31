using System;
using System.Drawing;
using System.IO;
using System.Text.Json;

namespace SDUI;

public enum ThemePreset
{
    CyberDark,
    EmeraldNeon,
    NeonViolet,
    CrimsonAlloy,
    ModernLight,
    MidnightGold,
    DeepSpace
}

/// <summary>
///     Theme definition model for serializing and deserializing themes to JSON.
/// </summary>
public class ThemeDefinition
{
    public string Name { get; set; } = "Custom";
    public string BackColor { get; set; }
    public string CardColor { get; set; }
    public string CardColor2 { get; set; }
    public string ForeColor { get; set; }
    public string SecondaryForeColor { get; set; }
    public string InputBackColor { get; set; }
    public string BorderColor { get; set; }
    public string ButtonColor { get; set; }
    public string ButtonForeColor { get; set; }
    public string ButtonHoverColor { get; set; }
    public string TableBackColor { get; set; }
    public string AccentColor { get; set; }
    public string SelectionColor { get; set; }
    public string GlobalFontFamily { get; set; }
    public float GlobalFontSizeOffset { get; set; }
}

public class ColorScheme
{
    private static Color _backColor = Color.FromArgb(15, 20, 28);

    // Custom Overrides (null means computed dynamically)
    public static Color? CustomBackColor { get; set; }
    public static Color? CustomCardColor { get; set; }
    public static Color? CustomCardColor2 { get; set; }
    public static Color? CustomForeColor { get; set; }
    public static Color? CustomSecondaryForeColor { get; set; }
    public static Color? CustomInputBackColor { get; set; }
    public static Color? CustomBorderColor { get; set; }
    public static Color? CustomButtonColor { get; set; }
    public static Color? CustomButtonForeColor { get; set; }
    public static Color? CustomButtonHoverColor { get; set; }
    public static Color? CustomTableBackColor { get; set; }
    public static Color? CustomAccentColor { get; set; }
    public static Color? CustomSelectionColor { get; set; }
    public static string GlobalFontFamily { get; set; }
    public static float GlobalFontSizeOffset { get; set; }

    /// <summary>
    /// Base Window Background: #0f141c
    /// </summary>
    public static Color BackColor
    {
        get => CustomBackColor ?? _backColor;
        set => _backColor = value;
    }

    /// <summary>
    /// Card / Container Background: #181f2a
    /// </summary>
    public static Color CardColor => CustomCardColor ?? (BackColor.IsDark() ? Color.FromArgb(24, 31, 42) : Color.FromArgb(255, 255, 255));

    /// <summary>
    /// Secondary Card / Inner Container Background: #1e2530
    /// </summary>
    public static Color CardColor2 => CustomCardColor2 ?? (BackColor.IsDark() ? Color.FromArgb(30, 37, 48) : Color.FromArgb(248, 250, 252));

    /// <summary>
    /// Primary Text: Pure White in Dark, Pure Black in Light
    /// </summary>
    public static Color ForeColor => CustomForeColor ?? (BackColor.IsDark() ? Color.White : Color.Black);

    /// <summary>
    /// Secondary / Label Text: Pure White in Dark, Pure Black in Light
    /// </summary>
    public static Color SecondaryForeColor => CustomSecondaryForeColor ?? (BackColor.IsDark() ? Color.White : Color.Black);

    /// <summary>
    /// Resolves any explicit color to pure White in Dark mode and pure Black in Light mode
    /// unless it represents an intentional non-monochrome status/accent color.
    /// </summary>
    public static Color ResolveTextColor(Color explicitColor)
    {
        bool isDark = BackColor.IsDark();
        if (explicitColor == Color.Empty || explicitColor.A == 0 || explicitColor == Color.Transparent || explicitColor == SystemColors.ControlText)
            return isDark ? Color.White : Color.Black;

        if (isDark)
        {
            if (explicitColor == Color.Black || explicitColor == Color.FromArgb(0, 0, 0) || explicitColor == Color.FromArgb(15, 20, 28) || explicitColor.IsDark())
                return Color.White;
        }
        else
        {
            if (explicitColor == Color.White || explicitColor == Color.FromArgb(255, 255, 255) || explicitColor == Color.FromArgb(241, 245, 249) || !explicitColor.IsDark())
                return Color.Black;
        }

        return explicitColor;
    }

    /// <summary>
    /// Input Fields & ComboBoxes Background: #232d3d
    /// </summary>
    public static Color InputBackColor => CustomInputBackColor ?? (BackColor.IsDark() ? Color.FromArgb(35, 45, 61) : Color.FromArgb(255, 255, 255));

    /// <summary>
    /// Input Fields & Containers Border: #3b485d
    /// </summary>
    public static Color BorderColor => CustomBorderColor ?? (BackColor.IsDark() ? Color.FromArgb(59, 72, 93) : Color.FromArgb(226, 232, 240));

    /// <summary>
    /// Button Background Color
    /// </summary>
    public static Color ButtonColor => CustomButtonColor ?? (BackColor.IsDark() ? Color.FromArgb(37, 99, 235) : Color.FromArgb(59, 130, 246));

    /// <summary>
    /// Button Text Color
    /// </summary>
    public static Color ButtonForeColor => CustomButtonForeColor ?? (ButtonColor.IsDark() ? Color.White : Color.FromArgb(17, 24, 39));

    /// <summary>
    /// Button Hover Background Color
    /// </summary>
    public static Color ButtonHoverColor => CustomButtonHoverColor ?? (ButtonColor.IsDark() ? ButtonColor.Brightness(0.2f) : ButtonColor.Brightness(-0.15f));

    /// <summary>
    /// Container back color 2
    /// </summary>
    public static Color BackColor2 => CardColor;

    /// <summary>
    /// Theme shadow color
    /// </summary>
    public static Color ShadowColor => ForeColor.Alpha(10);

    /// <summary>
    /// Tables Background: #151b24
    /// </summary>
    public static Color TableBackColor => CustomTableBackColor ?? (BackColor.IsDark() ? Color.FromArgb(21, 27, 36) : Color.FromArgb(255, 255, 255));

    /// <summary>
    /// Tables Alternate Row Background: #1a222e
    /// </summary>
    public static Color TableAlternateRowColor => BackColor.IsDark() ? Color.FromArgb(26, 34, 46) : Color.FromArgb(248, 250, 252);

    /// <summary>
    /// Tables Header Background: #202a39
    /// </summary>
    public static Color TableHeaderBackColor => BackColor.IsDark() ? Color.FromArgb(32, 42, 57) : Color.FromArgb(241, 245, 249);

    /// <summary>
    /// Tables Header Text: Pure White in Dark, Pure Black in Light
    /// </summary>
    public static Color TableHeaderForeColor => BackColor.IsDark() ? Color.White : Color.Black;

    /// <summary>
    /// Active Selection Color: #2563eb
    /// </summary>
    public static Color SelectionColor => CustomSelectionColor ?? Color.FromArgb(37, 99, 235);

    /// <summary>
    /// Accent Color: Emerald green #10b981 or custom
    /// </summary>
    public static Color AccentColor
    {
        get
        {
            if (CustomAccentColor.HasValue)
                return CustomAccentColor.Value;

            return BackColor.IsDark() ? Color.FromArgb(16, 185, 129) : Color.FromArgb(2, 132, 199);
        }
    }

    /// <summary>
    /// Gets or sets the debug borders
    /// </summary>
    public static bool DrawDebugBorders;

    /// <summary>
    /// Resets all custom colors to defaults.
    /// </summary>
    public static void ResetToDefault()
    {
        CustomBackColor = null;
        CustomCardColor = null;
        CustomCardColor2 = null;
        CustomForeColor = null;
        CustomSecondaryForeColor = null;
        CustomInputBackColor = null;
        CustomBorderColor = null;
        CustomButtonColor = null;
        CustomButtonForeColor = null;
        CustomButtonHoverColor = null;
        CustomTableBackColor = null;
        CustomAccentColor = null;
        CustomSelectionColor = null;
        GlobalFontFamily = null;
        GlobalFontSizeOffset = 0f;
    }

    /// <summary>
    /// Applies a pre-defined theme preset
    /// </summary>
    public static void ApplyPreset(ThemePreset preset)
    {
        ResetToDefault();

        switch (preset)
        {
            case ThemePreset.CyberDark:
                BackColor = Color.FromArgb(15, 20, 28);
                CustomAccentColor = Color.FromArgb(16, 185, 129); // Emerald Green
                CustomButtonColor = Color.FromArgb(37, 99, 235); // Royal Blue
                break;
            case ThemePreset.EmeraldNeon:
                BackColor = Color.FromArgb(6, 78, 59);
                CustomAccentColor = Color.FromArgb(16, 185, 129); // Emerald Green
                CustomButtonColor = Color.FromArgb(5, 150, 105);
                break;
            case ThemePreset.NeonViolet:
                BackColor = Color.FromArgb(30, 27, 75);
                CustomAccentColor = Color.FromArgb(139, 92, 246); // Neon Violet
                CustomButtonColor = Color.FromArgb(124, 58, 237);
                break;
            case ThemePreset.CrimsonAlloy:
                BackColor = Color.FromArgb(24, 24, 27);
                CustomAccentColor = Color.FromArgb(239, 68, 68); // Crimson Red
                CustomButtonColor = Color.FromArgb(220, 38, 38);
                break;
            case ThemePreset.ModernLight:
                BackColor = Color.FromArgb(248, 250, 252);
                CustomAccentColor = Color.FromArgb(2, 132, 199); // Sky Blue
                CustomButtonColor = Color.FromArgb(37, 99, 235);
                break;
            case ThemePreset.MidnightGold:
                BackColor = Color.FromArgb(18, 16, 22);
                CustomAccentColor = Color.FromArgb(245, 158, 11); // Amber / Gold
                CustomButtonColor = Color.FromArgb(217, 119, 6);
                CustomBorderColor = Color.FromArgb(66, 52, 34);
                break;
            case ThemePreset.DeepSpace:
                BackColor = Color.FromArgb(8, 10, 15);
                CustomAccentColor = Color.FromArgb(56, 189, 248); // Ice Cyan
                CustomButtonColor = Color.FromArgb(14, 116, 144);
                break;
        }
    }

    /// <summary>
    /// Loads custom theme definitions from a JSON file.
    /// </summary>
    public static bool LoadTheme(string filePath)
    {
        try
        {
            if (!File.Exists(filePath)) return false;
            var json = File.ReadAllText(filePath);
            var def = JsonSerializer.Deserialize<ThemeDefinition>(json);
            if (def == null) return false;

            ApplyThemeDefinition(def);
            return true;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Saves current theme customizations to a JSON file.
    /// </summary>
    public static bool SaveTheme(string filePath, string themeName = "Custom")
    {
        try
        {
            var dir = Path.GetDirectoryName(filePath);
            if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
                Directory.CreateDirectory(dir);

            var def = ToThemeDefinition(themeName);
            var options = new JsonSerializerOptions { WriteIndented = true };
            var json = JsonSerializer.Serialize(def, options);
            File.WriteAllText(filePath, json);
            return true;
        }
        catch
        {
            return false;
        }
    }

    public static ThemeDefinition ToThemeDefinition(string themeName = "Custom")
    {
        return new ThemeDefinition
        {
            Name = themeName,
            BackColor = ColorToHex(BackColor),
            CardColor = ColorToHex(CardColor),
            CardColor2 = ColorToHex(CardColor2),
            ForeColor = ColorToHex(ForeColor),
            SecondaryForeColor = ColorToHex(SecondaryForeColor),
            InputBackColor = ColorToHex(InputBackColor),
            BorderColor = ColorToHex(BorderColor),
            ButtonColor = ColorToHex(ButtonColor),
            ButtonForeColor = ColorToHex(ButtonForeColor),
            ButtonHoverColor = ColorToHex(ButtonHoverColor),
            TableBackColor = ColorToHex(TableBackColor),
            AccentColor = ColorToHex(AccentColor),
            SelectionColor = ColorToHex(SelectionColor),
            GlobalFontFamily = GlobalFontFamily,
            GlobalFontSizeOffset = GlobalFontSizeOffset
        };
    }

    public static void ApplyThemeDefinition(ThemeDefinition def)
    {
        if (def == null) return;

        if (!string.IsNullOrEmpty(def.BackColor)) CustomBackColor = HexToColor(def.BackColor, BackColor);
        if (!string.IsNullOrEmpty(def.CardColor)) CustomCardColor = HexToColor(def.CardColor, CardColor);
        if (!string.IsNullOrEmpty(def.CardColor2)) CustomCardColor2 = HexToColor(def.CardColor2, CardColor2);
        if (!string.IsNullOrEmpty(def.ForeColor)) CustomForeColor = HexToColor(def.ForeColor, ForeColor);
        if (!string.IsNullOrEmpty(def.SecondaryForeColor)) CustomSecondaryForeColor = HexToColor(def.SecondaryForeColor, SecondaryForeColor);
        if (!string.IsNullOrEmpty(def.InputBackColor)) CustomInputBackColor = HexToColor(def.InputBackColor, InputBackColor);
        if (!string.IsNullOrEmpty(def.BorderColor)) CustomBorderColor = HexToColor(def.BorderColor, BorderColor);
        if (!string.IsNullOrEmpty(def.ButtonColor)) CustomButtonColor = HexToColor(def.ButtonColor, ButtonColor);
        if (!string.IsNullOrEmpty(def.ButtonForeColor)) CustomButtonForeColor = HexToColor(def.ButtonForeColor, ButtonForeColor);
        if (!string.IsNullOrEmpty(def.ButtonHoverColor)) CustomButtonHoverColor = HexToColor(def.ButtonHoverColor, ButtonHoverColor);
        if (!string.IsNullOrEmpty(def.TableBackColor)) CustomTableBackColor = HexToColor(def.TableBackColor, TableBackColor);
        if (!string.IsNullOrEmpty(def.AccentColor)) CustomAccentColor = HexToColor(def.AccentColor, AccentColor);
        if (!string.IsNullOrEmpty(def.SelectionColor)) CustomSelectionColor = HexToColor(def.SelectionColor, SelectionColor);

        GlobalFontFamily = def.GlobalFontFamily;
        GlobalFontSizeOffset = def.GlobalFontSizeOffset;
    }

    public static string ColorToHex(Color c)
    {
        return $"#{c.R:X2}{c.G:X2}{c.B:X2}";
    }

    public static Color HexToColor(string hex, Color fallback)
    {
        if (string.IsNullOrWhiteSpace(hex)) return fallback;
        hex = hex.Trim().TrimStart('#');
        if (hex.Length == 6)
        {
            if (int.TryParse(hex.Substring(0, 2), System.Globalization.NumberStyles.HexNumber, null, out var r) &&
                int.TryParse(hex.Substring(2, 2), System.Globalization.NumberStyles.HexNumber, null, out var g) &&
                int.TryParse(hex.Substring(4, 2), System.Globalization.NumberStyles.HexNumber, null, out var b))
            {
                return Color.FromArgb(r, g, b);
            }
        }
        else if (hex.Length == 8)
        {
            if (int.TryParse(hex.Substring(0, 2), System.Globalization.NumberStyles.HexNumber, null, out var a) &&
                int.TryParse(hex.Substring(2, 2), System.Globalization.NumberStyles.HexNumber, null, out var r) &&
                int.TryParse(hex.Substring(4, 2), System.Globalization.NumberStyles.HexNumber, null, out var g) &&
                int.TryParse(hex.Substring(6, 2), System.Globalization.NumberStyles.HexNumber, null, out var b))
            {
                return Color.FromArgb(a, r, g, b);
            }
        }
        return fallback;
    }
}
