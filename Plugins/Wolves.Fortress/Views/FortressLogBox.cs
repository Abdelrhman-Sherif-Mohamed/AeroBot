using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using SDUI;

namespace Wolves.Fortress.Views;

/// <summary>
///     Read-only log surface with an explicitly themed native RichEdit
///     background. The stock RichTextBox can keep the system-white client area
///     on some Windows themes, which clashes with the dark page cards.
/// </summary>
public class FortressLogBox : RichTextBox
{
    private const int WM_ERASEBKGND = 0x0014;
    private const int EM_SETBKGNDCOLOR = 0x0443;

    [DllImport("user32.dll")]
    private static extern IntPtr SendMessage(IntPtr hWnd, int msg, IntPtr wParam, IntPtr lParam);

    public FortressLogBox()
    {
        BorderStyle = BorderStyle.None;
        ReadOnly = true;
        DetectUrls = false;
        WordWrap = true;
        ScrollBars = RichTextBoxScrollBars.Vertical;
        ApplyTheme();
    }

    public void ApplyTheme()
    {
        var background = ColorScheme.TableBackColor;

        BackColor = background;
        ForeColor = ColorScheme.ForeColor;

        if (IsHandleCreated)
        {
            SendMessage(Handle, EM_SETBKGNDCOLOR, IntPtr.Zero,
                (IntPtr)ColorTranslator.ToWin32(background));
            Invalidate();
        }
    }

    protected override void OnHandleCreated(EventArgs e)
    {
        base.OnHandleCreated(e);
        ApplyTheme();
    }

    protected override void OnBackColorChanged(EventArgs e)
    {
        base.OnBackColorChanged(e);

        if (IsHandleCreated)
        {
            SendMessage(Handle, EM_SETBKGNDCOLOR, IntPtr.Zero,
                (IntPtr)ColorTranslator.ToWin32(BackColor));
        }
    }

    protected override void WndProc(ref Message m)
    {
        if (m.Msg == WM_ERASEBKGND)
        {
            using (var graphics = Graphics.FromHdc(m.WParam))
            using (var brush = new SolidBrush(ColorScheme.TableBackColor))
                graphics.FillRectangle(brush, ClientRectangle);

            m.Result = (IntPtr)1;
            return;
        }

        base.WndProc(ref m);
    }
}
