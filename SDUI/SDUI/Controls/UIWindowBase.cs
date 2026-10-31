using SDUI.Helpers;
using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using static SDUI.NativeMethods;

namespace SDUI.Controls;

public class UIWindowBase : Form
{
    protected bool enableFullDraggable;
    private int dwmMargin = 1;
    private bool right = false;
    private Point location;

    /// <summary>
    /// Gets or sets whether the form borders can be resized by the user.
    /// </summary>
    public bool Resizable { get; set; } = true;

    public int DwmMargin
    {
        get => dwmMargin;
        set => dwmMargin = value;
    }

    /// <summary>
    /// Get DPI
    /// </summary>
    public float DPI => DeviceDpi / 96.0f;

    /// <summary>
    /// Has aero enabled by windows <c>true</c>; otherwise <c>false</c>
    /// </summary>
    private bool _aeroEnabled
    {
        get
        {
            if (Environment.OSVersion.Version.Major >= 6)
            {
                int enabled = 0;
                DwmIsCompositionEnabled(ref enabled);
                return enabled == 1;
            }

            return false;
        }
    }

    public UIWindowBase()
    {
        //BackColor = Color.FromArgb(0, 0, 0, 0);
        ResizeRedraw = true;
    }

    protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
    {
        if (msg.Msg == 256 | msg.Msg == 260)
            if (keyData == Keys.Escape)
                Close();

        return base.ProcessCmdKey(ref msg, keyData);
    }

    protected override void OnMouseDown(MouseEventArgs e)
    {
        base.OnMouseDown(e);
        right = e.Button == MouseButtons.Right;
        if (right)
        {
            location = e.Location;
            Invalidate();
        }

        if (!enableFullDraggable)
            return;

        if (e.Button == MouseButtons.Left)
            DragForm(Handle);
    }

    protected void DragForm(IntPtr handle)
    {
        ReleaseCapture();
        SendMessage(handle, WM_SYSCOMMAND, SC_MOVE + HTCAPTION, 0);
    }

    protected override CreateParams CreateParams
    {
        get
        {
            var cp = base.CreateParams;
            if (!_aeroEnabled)
                cp.ClassStyle |= CS_DROPSHADOW;

            cp.ClassStyle |= CS_DBLCLKS;

            var style = (uint)cp.Style;
            
            if(DesignMode)
                style &= ~(uint)SetWindowLongFlags.WS_CAPTION;

            style &= ~(uint)SetWindowLongFlags.WS_SYSMENU;
            style &= ~(uint)SetWindowLongFlags.WS_THICKFRAME;
            style &= ~(uint)SetWindowLongFlags.WS_MINIMIZE;
            style &= ~(uint)SetWindowLongFlags.WS_MINIMIZEBOX;
            style &= ~(uint)SetWindowLongFlags.WS_MAXIMIZE;
            style &= ~(uint)SetWindowLongFlags.WS_MAXIMIZEBOX;
            //style &= ~(uint)SetWindowLongFlags.WS_BORDER;
            style |= (uint)SetWindowLongFlags.WS_TILED;
            cp.Style = (int)style;

            return cp;
        }
    }

    protected override void OnLoad(EventArgs e)
    {
        base.OnLoad(e);

        if (DesignMode)
            return;

        // Otherwise, it will not be applied.
        if (StartPosition == FormStartPosition.CenterScreen)
            CenterToScreen();

        if (BackColor != ColorScheme.BackColor)
            BackColor = ColorScheme.BackColor;
    }

    private const int htLeft = 10;
    private const int htRight = 11;
    private const int htTop = 12;
    private const int htTopLeft = 13;
    private const int htTopRight = 14;
    private const int htBottom = 15;
    private const int htBottomLeft = 16;
    private const int htBottomRight = 17;
    protected override void WndProc(ref Message m)
    {
        if (DesignMode)
        {
            base.WndProc(ref m);
            return;
        }

        switch (m.Msg)
        {
            case WM_NCHITTEST:
                {
                    if (WindowState != FormWindowState.Maximized && Resizable)
                    {
                        int gripDist = 10;
                        
                        var pt = PointToClient(Cursor.Position);

                        Size clientSize = ClientSize;
                        ///allow resize on the lower right corner
                        if (pt.X >= clientSize.Width - gripDist && pt.Y >= clientSize.Height - gripDist && clientSize.Height >= gripDist)
                        {
                            m.Result = (IntPtr)(IsMirrored ? htBottomLeft : htBottomRight);
                            return;
                        }
                        ///allow resize on the lower left corner
                        if (pt.X <= gripDist && pt.Y >= clientSize.Height - gripDist && clientSize.Height >= gripDist)
                        {
                            m.Result = (IntPtr)(IsMirrored ? htBottomRight : htBottomLeft);
                            return;
                        }
                        ///allow resize on the upper right corner
                        if (pt.X <= gripDist && pt.Y <= gripDist && clientSize.Height >= gripDist)
                        {
                            m.Result = (IntPtr)(IsMirrored ? htTopRight : htTopLeft);
                            return;
                        }
                        ///allow resize on the upper left corner
                        if (pt.X >= clientSize.Width - gripDist && pt.Y <= gripDist && clientSize.Height >= gripDist)
                        {
                            m.Result = (IntPtr)(IsMirrored ? htTopLeft : htTopRight);
                            return;
                        }
                        ///allow resize on the top border
                        if (pt.Y <= 2 && clientSize.Height >= 2)
                        {
                            m.Result = (IntPtr)htTop;
                            return;
                        }
                        ///allow resize on the bottom border
                        if (pt.Y >= clientSize.Height - gripDist && clientSize.Height >= gripDist)
                        {
                            m.Result = (IntPtr)htBottom;
                            return;
                        }
                        ///allow resize on the left border
                        if (pt.X <= gripDist && clientSize.Height >= gripDist)
                        {
                            m.Result = (IntPtr)htLeft;
                            return;
                        }
                        ///allow resize on the right border
                        if (pt.X >= clientSize.Width - gripDist && clientSize.Height >= gripDist)
                        {
                            m.Result = (IntPtr)htRight;
                            return;
                        }
                    }

                    if ((int)m.Result == HTCLIENT)     // drag the form
                        m.Result = (IntPtr)HTCAPTION;

                    break;
                }
            case WM_NCCALCSIZE:

                var handle = Handle;

                var lpwp = (WINDOWPOS)Marshal.PtrToStructure(m.LParam, typeof(WINDOWPOS));
                if (lpwp.HWND == IntPtr.Zero)
                    return;

                if ((lpwp.flags & (SetWindowPosFlags.SWP_NOSIZE | SetWindowPosFlags.SWP_NOREDRAW)) != 0)
                {
                    return;
                }
                // TEMPORARY CODE
                // if (OS.IsAppThemed ()) {
                // OS.InvalidateRect (handle, null, true);
                // return result;
                // }
                int bits = GetWindowLong(handle, WindowLongIndexFlags.GWL_STYLE).ToInt32();
                if ((bits & TCS_MULTILINE) != 0)
                {
                    InvalidateRect(handle, new Rect(), true);
                    return;
                }

                var rect = new Rect();
                SetRect(rect, 0, 0, lpwp.cx, lpwp.cy);

                SendMessage(handle, WM_NCCALCSIZE, 0, ref rect);
                int newWidth = rect.Right - rect.Left;
                int newHeight = rect.Bottom - rect.Top;
                GetClientRect(handle, ref rect);
                int oldWidth = rect.Right - rect.Left;
                int oldHeight = rect.Bottom - rect.Top;
                if (newWidth == oldWidth && newHeight == oldHeight)
                {
                    return;
                }
                var inset = new Rect();
                SendMessage(handle, TCM_ADJUSTRECT, 0, ref inset);
                int marginX = -inset.Right, marginY = -inset.Bottom;
                if (newWidth != oldWidth)
                {
                    int left = oldWidth;
                    if (newWidth < oldWidth)
                        left = newWidth;
                    SetRect(rect, left - marginX, 0, newWidth, newHeight);
                    InvalidateRect(handle, rect, true);
                }
                if (newHeight != oldHeight)
                {
                    int bottom = oldHeight;
                    if (newHeight < oldHeight)
                        bottom = newHeight;
                    if (newWidth < oldWidth)
                        oldWidth -= marginX;
                    SetRect(rect, 0, bottom - marginY, oldWidth, newHeight);
                    InvalidateRect(handle, rect, true);

                }
                return;
        }

        base.WndProc(ref m);
    }

    public void ChangeControlsTheme(Control control)
    {
        if (control == null)
            return;

        var isDark = ColorScheme.BackColor.IsDark();
        var fore = ColorScheme.ForeColor;
        var back = ColorScheme.BackColor;
        var card = ColorScheme.CardColor;
        var card2 = ColorScheme.CardColor2;
        var border = ColorScheme.BorderColor;
        var inputBg = ColorScheme.InputBackColor;

        if (control is ListView lv)
        {
            try
            {
                lv.BackColor = ColorScheme.TableBackColor;
                lv.ForeColor = fore;
                if (WindowsHelper.IsModern)
                {
                    SendMessage(control.Handle, 0x0127, (1 << 16) | (1 & 0xffff), 0);
                    AllowDarkModeForWindow(control.Handle, isDark);
                    SetWindowTheme(control.Handle, isDark ? "DarkMode_Explorer" : "Explorer", null);
                }

                lv.ApplyNativeTheme();
            }
            catch { }
        }
        else if (control is RichTextBox || control is ListBox || control is TreeView)
        {
            try
            {
                control.BackColor = ColorScheme.TableBackColor;
                control.ForeColor = fore;
                if (control is ListBox lb)
                    lb.BorderStyle = BorderStyle.FixedSingle;

            }
            catch { }
        }
        else if (control is System.Windows.Forms.ListView sysLv)
        {
            try
            {
                sysLv.BackColor = ColorScheme.TableBackColor;
                sysLv.ForeColor = fore;

            }
            catch { }
        }
        else if (control is SDUI.Controls.Button sduiBtn)
        {
            sduiBtn.ForeColor = fore;
            sduiBtn.BackColor = Color.Transparent;
            sduiBtn.Color = Color.Transparent;
            sduiBtn.Invalidate();
        }
        else if (control is System.Windows.Forms.Button sysBtn)
        {
            try
            {
                sysBtn.UseVisualStyleBackColor = false;
                sysBtn.FlatStyle = FlatStyle.Flat;
                sysBtn.BackColor = isDark ? card2 : Color.FromArgb(243, 244, 246);
                sysBtn.ForeColor = fore;
                sysBtn.FlatAppearance.BorderColor = border;
                sysBtn.FlatAppearance.BorderSize = 1;
                sysBtn.FlatAppearance.MouseOverBackColor = isDark ? card : Color.FromArgb(229, 231, 235);
                sysBtn.FlatAppearance.MouseDownBackColor = isDark ? border : Color.FromArgb(209, 213, 219);
                sysBtn.Invalidate();
            }
            catch { }
        }
        else if (control is SDUI.Controls.CheckBox sduiChk)
        {
            sduiChk.ForeColor = fore;
            sduiChk.BackColor = Color.Transparent;
            sduiChk.UseVisualStyleBackColor = false;
            sduiChk.Invalidate();
        }
        else if (control is System.Windows.Forms.CheckBox sysChk)
        {
            try
            {
                sysChk.ForeColor = fore;
                sysChk.BackColor = Color.Transparent;
                sysChk.FlatStyle = FlatStyle.Flat;
                sysChk.FlatAppearance.BorderColor = border;
                sysChk.UseVisualStyleBackColor = false;
                sysChk.Invalidate();
            }
            catch { }
        }
        else if (control is SDUI.Controls.Radio sduiRadio)
        {
            sduiRadio.ForeColor = fore;
            sduiRadio.BackColor = Color.Transparent;
            sduiRadio.UseVisualStyleBackColor = false;
            sduiRadio.Invalidate();
        }
        else if (control is System.Windows.Forms.RadioButton sysRb)
        {
            try
            {
                sysRb.ForeColor = fore;
                sysRb.BackColor = Color.Transparent;
                sysRb.FlatStyle = FlatStyle.Flat;
                sysRb.FlatAppearance.BorderColor = border;
                sysRb.Invalidate();
            }
            catch { }
        }
        else if (control is SDUI.Controls.GroupBox sduiGb)
        {
            sduiGb.ForeColor = fore;
            sduiGb.BackColor = Color.Transparent;
            sduiGb.Invalidate();
        }
        else if (control is System.Windows.Forms.GroupBox sysGb)
        {
            sysGb.ForeColor = fore;
            sysGb.BackColor = Color.Transparent;
            sysGb.Invalidate();
        }
        else if (control is SDUI.Controls.Panel sduiPnl)
        {
            sduiPnl.BackColor = Color.Transparent;
            sduiPnl.ForeColor = fore;
            sduiPnl.Invalidate();
        }
        else if (control is System.Windows.Forms.Panel sysPnl)
        {
            sysPnl.BackColor = Color.Transparent;
            sysPnl.ForeColor = fore;
            sysPnl.Invalidate();
        }
        else if (control is SDUI.Controls.Label sduiLbl)
        {
            sduiLbl.ForeColor = fore;
            sduiLbl.BackColor = Color.Transparent;
            sduiLbl.Invalidate();
        }
        else if (control is System.Windows.Forms.Label sysLbl)
        {
            sysLbl.ForeColor = fore;
            sysLbl.BackColor = Color.Transparent;
            sysLbl.Invalidate();
        }
        else if (control is SDUI.Controls.TextBox sduiTxt)
        {
            sduiTxt.BackColor = inputBg;
            sduiTxt.ForeColor = fore;
            sduiTxt.Invalidate();
        }
        else if (control is System.Windows.Forms.TextBox sysTxt)
        {
            sysTxt.BackColor = inputBg;
            sysTxt.ForeColor = fore;
            sysTxt.BorderStyle = BorderStyle.FixedSingle;
            sysTxt.Invalidate();
        }
        else if (control is SDUI.Controls.ComboBox sduiCmb)
        {
            sduiCmb.BackColor = inputBg;
            sduiCmb.ForeColor = fore;
            sduiCmb.Invalidate();
        }
        else if (control is System.Windows.Forms.ComboBox sysCmb)
        {
            sysCmb.BackColor = inputBg;
            sysCmb.ForeColor = fore;
            sysCmb.Invalidate();
        }

        // Only apply immersive dark mode to top-level Form window handle, NOT child controls
        if (control is Form form)
        {
            try
            {
                WindowsHelper.UseImmersiveDarkMode(form.Handle, isDark);
            }
            catch { }
        }

        control.Invalidate(true);

        foreach (Control subControl in control.Controls)
        {
            ChangeControlsTheme(subControl);
        }
    }

    protected override void OnHandleCreated(EventArgs e)
    {
        base.OnHandleCreated(e);

        if (DesignMode)
            return;

        WindowsHelper.ApplyRoundCorner(this.Handle);

        if (_aeroEnabled)
        {
            var v = 2;

            DwmSetWindowAttribute(Handle, DWMWINDOWATTRIBUTE.DWMWA_NCRENDERING_POLICY, ref v, 4);
            var margins = new MARGINS()
            {
                Bottom = 1,
                Left = 1,
                Right = 1,
                Top = 1
            };

            DwmExtendFrameIntoClientArea(this.Handle, ref margins);
        }

        SetWindowPos(Handle, IntPtr.Zero, 0, 0, 0, 0, SetWindowPosFlags.SWP_FRAMECHANGED | SetWindowPosFlags.SWP_NOSIZE | SetWindowPosFlags.SWP_NOMOVE | SetWindowPosFlags.SWP_NOZORDER | SetWindowPosFlags.SWP_NOOWNERZORDER | SetWindowPosFlags.SWP_NOACTIVATE);
    }

    protected override void OnBackColorChanged(EventArgs e)
    {
        base.OnBackColorChanged(e);
        if (DesignMode)
            return;

        ChangeControlsTheme(this);

        if (!WindowsHelper.IsModern)
            return;
        WindowsHelper.UseImmersiveDarkMode(Handle, ColorScheme.BackColor.IsDark());
        /*
        
                var flag = DWMSBT_TRANSIENTWINDOW;
                DwmSetWindowAttribute(
                    Handle,
                    DWMWINDOWATTRIBUTE.DWMWA_SYSTEMBACKDROP_TYPE,
                    ref flag,
                     Marshal.SizeOf<int>());
        */
    }
}