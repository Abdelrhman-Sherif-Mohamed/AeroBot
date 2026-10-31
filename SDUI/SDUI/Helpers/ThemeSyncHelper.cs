using System;
using System.Drawing;
using System.Windows.Forms;

namespace SDUI.Helpers;

/// <summary>
/// Senior WPF / WinForms Interop Theme Synchronization Helper.
/// Recursively walks WindowsFormsHost child control trees and WinForms hierarchies
/// to eliminate solid black (#000000) boxes, stark white ListView/ListBox surfaces in Dark Mode,
/// and properly synchronize BackColor, ForeColor, and BorderStyle across theme switches.
/// </summary>
public static class ThemeSyncHelper
{
    /// <summary>
    /// Recursively synchronizes WinForms controls to the active Light/Dark theme.
    /// </summary>
    /// <param name="control">Root or parent WinForms control (or child of WindowsFormsHost)</param>
    /// <param name="isDark">True for Dark Mode, False for Light Mode</param>
    public static void SyncWinFormsTheme(Control control, bool isDark)
    {
        if (control == null)
            return;

        // Theme palette definitions matching WPF ResourceDictionaries
        var backColor = isDark ? Color.FromArgb(15, 20, 28) : Color.FromArgb(241, 245, 249);
        var surfaceColor = isDark ? Color.FromArgb(24, 32, 44) : Color.FromArgb(255, 255, 255);
        var surfaceAltColor = isDark ? Color.FromArgb(30, 38, 54) : Color.FromArgb(248, 250, 252);
        var foreColor = isDark ? Color.FromArgb(248, 250, 252) : Color.FromArgb(17, 24, 39);
        var borderColor = isDark ? Color.FromArgb(42, 53, 71) : Color.FromArgb(203, 213, 225);
        var buttonBgColor = isDark ? Color.FromArgb(30, 38, 54) : Color.FromArgb(243, 244, 246);
        var buttonHoverColor = isDark ? Color.FromArgb(45, 56, 78) : Color.FromArgb(229, 231, 235);
        var inputBgColor = isDark ? Color.FromArgb(18, 24, 34) : Color.FromArgb(255, 255, 255);

        // 1. CheckBoxes: Must have transparent background to prevent thick black rectangles around labels
        if (control is SDUI.Controls.CheckBox sduiChk)
        {
            sduiChk.BackColor = Color.Transparent;
            sduiChk.ForeColor = foreColor;
            sduiChk.UseVisualStyleBackColor = false;
            sduiChk.Invalidate();
        }
        else if (control is System.Windows.Forms.CheckBox sysChk)
        {
            sysChk.BackColor = Color.Transparent;
            sysChk.ForeColor = foreColor;
            sysChk.FlatStyle = FlatStyle.Flat;
            sysChk.FlatAppearance.BorderColor = borderColor;
            sysChk.UseVisualStyleBackColor = false;
            sysChk.Invalidate();
        }
        // 2. RadioButtons: Must have transparent background to prevent black bounding boxes
        else if (control is SDUI.Controls.Radio sduiRadio)
        {
            sduiRadio.BackColor = Color.Transparent;
            sduiRadio.ForeColor = foreColor;
            sduiRadio.UseVisualStyleBackColor = false;
            sduiRadio.Invalidate();
        }
        else if (control is System.Windows.Forms.RadioButton sysRb)
        {
            sysRb.BackColor = Color.Transparent;
            sysRb.ForeColor = foreColor;
            sysRb.FlatStyle = FlatStyle.Flat;
            sysRb.FlatAppearance.BorderColor = borderColor;
            sysRb.UseVisualStyleBackColor = false;
            sysRb.Invalidate();
        }
        // 3. Buttons: ("Current", "...", "Browse", "IP Bind") must adapt to light/dark surface, NEVER solid black
        else if (control is SDUI.Controls.Button sduiBtn)
        {
            sduiBtn.BackColor = Color.Transparent;
            sduiBtn.Color = Color.Transparent;
            sduiBtn.ForeColor = foreColor;
            sduiBtn.Invalidate();
        }
        else if (control is System.Windows.Forms.Button sysBtn)
        {
            sysBtn.UseVisualStyleBackColor = false;
            sysBtn.FlatStyle = FlatStyle.Flat;
            sysBtn.BackColor = buttonBgColor;
            sysBtn.ForeColor = foreColor;
            sysBtn.FlatAppearance.BorderColor = borderColor;
            sysBtn.FlatAppearance.BorderSize = 1;
            sysBtn.FlatAppearance.MouseOverBackColor = buttonHoverColor;
            sysBtn.Invalidate();
        }
        // 4. ListBox / ListView ("إعدادات التفضيل"): Slate/Navy surface in Dark Mode, crisp white in Light Mode
        else if (control is ListView lv)
        {
            lv.BackColor = isDark ? Color.FromArgb(21, 27, 36) : Color.FromArgb(255, 255, 255);
            lv.ForeColor = foreColor;
            if (WindowsHelper.IsModern)
            {
                try
                {
                    WindowsHelper.UseImmersiveDarkMode(lv.Handle, isDark);
                    NativeMethods.SetWindowTheme(lv.Handle, isDark ? "DarkMode_Explorer" : "Explorer", null);
                }
                catch { }
            }
            lv.Invalidate();
        }
        else if (control is ListBox lb)
        {
            lb.BackColor = isDark ? Color.FromArgb(21, 27, 36) : Color.FromArgb(255, 255, 255);
            lb.ForeColor = foreColor;
            lb.BorderStyle = BorderStyle.FixedSingle;
            lb.Invalidate();
        }
        // 5. TextBoxes & Inputs
        else if (control is SDUI.Controls.TextBox sduiTxt)
        {
            sduiTxt.BackColor = inputBgColor;
            sduiTxt.ForeColor = foreColor;
            sduiTxt.Invalidate();
        }
        else if (control is System.Windows.Forms.TextBox sysTxt)
        {
            sysTxt.BackColor = inputBgColor;
            sysTxt.ForeColor = foreColor;
            sysTxt.BorderStyle = BorderStyle.FixedSingle;
            sysTxt.Invalidate();
        }
        // 6. GroupBoxes & Panels
        else if (control is GroupBox gb)
        {
            gb.BackColor = Color.Transparent;
            gb.ForeColor = foreColor;
            gb.Invalidate();
        }
        else if (control is Panel pnl)
        {
            pnl.BackColor = Color.Transparent;
            pnl.ForeColor = foreColor;
            pnl.Invalidate();
        }
        // 7. Labels
        else if (control is System.Windows.Forms.Label lbl && !(control is SDUI.Controls.Label))
        {
            lbl.BackColor = Color.Transparent;
            lbl.ForeColor = foreColor;
            lbl.Invalidate();
        }

        // Recursively traverse all nested child controls
        foreach (Control child in control.Controls)
        {
            SyncWinFormsTheme(child, isDark);
        }
    }
}
