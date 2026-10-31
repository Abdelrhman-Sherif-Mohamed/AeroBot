using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static SDUI.NativeMethods;

namespace SDUI.Controls.Subclasses
{
    class ListViewHeaderSubclassedWindow : MarshalByRefObject, System.Windows.Forms.IWin32Window
    {
        // prevents collection of ListViewHeaderSubclassedWindow that is still in use
        static private HashSet<ListViewHeaderSubclassedWindow> _instancesInUse = new();

        // The number of uses we still have for this instances:
        // - some window attached, or
        // - inside a window procedure
        private int _uses = 0;

        // Our window procedure delegate
        private SUBCLASSPROC _windowProc;

        // The native handle for our delegate
        private IntPtr _windowProcHandle;

        static ListViewHeaderSubclassedWindow()
        {
            AppDomain.CurrentDomain.ProcessExit += OnShutdown;
        }

        public ListViewHeaderSubclassedWindow()
        {
            _windowProc = new SUBCLASSPROC(Callback);
            _windowProcHandle = System.Runtime.InteropServices.Marshal.GetFunctionPointerForDelegate(_windowProc);
        }

        private ListView _listView;

        /// <summary>
        ///  Gets the handle for this window.
        /// </summary>
        public IntPtr Handle { get; private set; }

        /// <summary>
        ///  Assigns a handle to this <see cref="NativeWindow"/> instance with associated ListView.
        /// </summary>
        public void AssignHandle(ListView listView)
        {
            _listView = listView;
            AssignHandle(listView.Handle);
        }

        /// <summary>
        ///  Assigns a handle to this <see cref="NativeWindow"/> instance.
        /// </summary>
        public void AssignHandle(IntPtr handle)
        {
            CheckReleased();
            Debug.Assert(handle != IntPtr.Zero, "handle is 0");

            if (0 == _uses)
            {
                lock (_instancesInUse)
                {
                    _instancesInUse.Add(this);
                }
            } // else may happen if handle gets reassigned inside WndProc.
            // This is legal after any call to DefWndProc.

            ++_uses;
            Handle = handle;

            SetWindowSubclass(handle, _windowProcHandle, UIntPtr.Zero, UIntPtr.Zero);

            IntPtr hHeader = SendMessage(handle, LVM_GETHEADER, 0, 0);
            bool isDark = ColorScheme.BackColor.IsDark();
            AllowDarkModeForWindow(handle, isDark);
            AllowDarkModeForWindow(hHeader, isDark);
            if (isDark)
            {
                SetWindowTheme(handle, "DarkMode_Explorer", null);
                SetWindowTheme(hHeader, "DarkMode_Explorer", null);
            }
            else
            {
                SetWindowTheme(handle, "Explorer", null);
                SetWindowTheme(hHeader, "Explorer", null);
            }

            OnHandleChange();
        }

        /// <summary>
        ///  Window message callback method. Control arrives here when a window
        ///  message is sent to this Window. This method packages the window message
        ///  in a Message object and invokes the WndProc() method. A WM_NCDESTROY
        ///  message automatically causes the ReleaseHandle() method to be called.
        /// </summary>
        private IntPtr Callback(
            IntPtr hWnd,
            int msg,
            IntPtr wParam,
            IntPtr lParam,
            UIntPtr uIdSubclass,
            UIntPtr dwRefData
        )
        {
            Debug.Assert(0 < _uses);
            ++_uses;

            try
            {
                var m = System.Windows.Forms.Message.Create(hWnd, msg, wParam, lParam);
                switch (m.Msg)
                {
                    case WM_NOTIFY:
                        var pnmhdr = (NMHDR)m.GetLParam(typeof(NMHDR));
                        IntPtr hHeader = SendMessage(Handle, LVM_GETHEADER, 0, 0);

                        if (pnmhdr.hwndFrom == hHeader && pnmhdr.code == NM_CUSTOMDRAW)
                        {
                            var nmcd = (NMCUSTOMDRAW)m.GetLParam(typeof(NMCUSTOMDRAW));

                            switch (nmcd.dwDrawStage)
                            {
                                case (int)CDDS.CDDS_PREPAINT:
                                    Rect hRect = new Rect();
                                    if (GetClientRect(hHeader, ref hRect))
                                    {
                                        using (var g = Graphics.FromHdc(nmcd.hdc))
                                        {
                                            var headerBg = ColorScheme.TableHeaderBackColor;
                                            using var brush = new SolidBrush(headerBg);
                                            g.FillRectangle(brush, 0, 0, hRect.Right - hRect.Left, hRect.Bottom - hRect.Top);

                                            using var bottomPen = new Pen(ColorScheme.BorderColor);
                                            g.DrawLine(bottomPen, 0, (hRect.Bottom - hRect.Top) - 1, hRect.Right - hRect.Left, (hRect.Bottom - hRect.Top) - 1);
                                        }
                                    }

                                    m.Result = new IntPtr((int)(CDRF.CDRF_NOTIFYITEMDRAW | CDRF.CDRF_NOTIFYPOSTPAINT));
                                    return m.Result;

                                case (int)CDDS.CDDS_POSTPAINT:
                                    Rect pRect = new Rect();
                                    if (GetClientRect(hHeader, ref pRect))
                                    {
                                        using (var g = Graphics.FromHdc(nmcd.hdc))
                                        {
                                            var headerBg = ColorScheme.TableHeaderBackColor;
                                            using var brush = new SolidBrush(headerBg);

                                            int totalColWidth = 0;
                                            if (_listView != null)
                                            {
                                                foreach (ColumnHeader col in _listView.Columns)
                                                    totalColWidth += col.Width;
                                            }

                                            int headerWidth = pRect.Right - pRect.Left;
                                            if (totalColWidth < headerWidth)
                                            {
                                                g.FillRectangle(brush, totalColWidth, 0, headerWidth - totalColWidth, pRect.Bottom - pRect.Top);
                                                using var bottomPen = new Pen(ColorScheme.BorderColor);
                                                g.DrawLine(bottomPen, totalColWidth, (pRect.Bottom - pRect.Top) - 1, headerWidth, (pRect.Bottom - pRect.Top) - 1);
                                            }
                                        }
                                    }

                                    m.Result = new IntPtr((int)CDRF.CDRF_SKIPDEFAULT);
                                    return m.Result;

                                case (int)CDDS.CDDS_ITEMPREPAINT:
                                    var colIndex = (int)nmcd.dwItemSpec;
                                    var rect = new Rectangle(nmcd.rc.Left, nmcd.rc.Top, nmcd.rc.Right - nmcd.rc.Left, nmcd.rc.Bottom - nmcd.rc.Top);

                                    using (var g = Graphics.FromHdc(nmcd.hdc))
                                    {
                                        bool isSelected = (nmcd.uItemState & 0x0001) != 0;
                                        var itemBg = isSelected
                                            ? (ColorScheme.BackColor.IsDark() ? Color.FromArgb(45, 58, 79) : Color.FromArgb(215, 222, 235))
                                            : ColorScheme.TableHeaderBackColor;

                                        using (var bgBrush = new SolidBrush(itemBg))
                                            g.FillRectangle(bgBrush, rect);

                                        using (var pen = new Pen(ColorScheme.BorderColor))
                                        {
                                            g.DrawLine(pen, rect.Right - 1, rect.Top + 3, rect.Right - 1, rect.Bottom - 4);
                                            g.DrawLine(pen, rect.Left, rect.Bottom - 1, rect.Right, rect.Bottom - 1);
                                        }

                                        if (_listView != null && colIndex >= 0 && colIndex < _listView.Columns.Count)
                                        {
                                            var col = _listView.Columns[colIndex];
                                            var textFlags = TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis | TextFormatFlags.SingleLine;
                                            if (col.TextAlign == HorizontalAlignment.Right)
                                                textFlags |= TextFormatFlags.Right;
                                            else if (col.TextAlign == HorizontalAlignment.Center)
                                                textFlags |= TextFormatFlags.HorizontalCenter;
                                            else
                                                textFlags |= TextFormatFlags.Left;

                                            var textRect = new Rectangle(rect.Left + 8, rect.Top, Math.Max(0, rect.Width - 16), rect.Height);
                                            var font = _listView.Font ?? SystemFonts.DefaultFont;
                                            TextRenderer.DrawText(g, col.Text, font, textRect, ColorScheme.TableHeaderForeColor, textFlags);
                                        }
                                    }

                                    m.Result = new IntPtr((int)CDRF.CDRF_SKIPDEFAULT);
                                    return m.Result;

                                default:
                                    m.Result = new IntPtr((int)CDRF.CDRF_DODEFAULT);
                                    return m.Result;
                            }
                        }
                        break;

                    case WM_THEMECHANGED:
                        IntPtr hHdr = SendMessage(Handle, LVM_GETHEADER, 0, 0);
                        RedrawWindow(Handle, IntPtr.Zero, IntPtr.Zero, 0x0400 | 0x0001);
                        if (hHdr != IntPtr.Zero)
                            RedrawWindow(hHdr, IntPtr.Zero, IntPtr.Zero, 0x0400 | 0x0001);
                        break;
                    default:
                        break;
                }

                m.Result = DefSubclassProc(Handle, m.Msg, m.WParam, m.LParam);
                return m.Result;
            }
            catch (Exception e)
            {
                OnThreadException(e);
                return IntPtr.Zero;
            }
            finally
            {
                if (msg == 0x82/*WM_NCDESTROY*/ && Handle != IntPtr.Zero)
                {
                    InternalReleaseHandle();
                }
                if (0 == --_uses)
                {
                    lock (_instancesInUse)
                    {
                        _instancesInUse.Remove(this);
                    }
                }
            }
        }

        /// <summary>
        ///  Raises an exception if the window handle is not zero.
        /// </summary>
        private void CheckReleased()
        {
            if (Handle != IntPtr.Zero)
            {
                throw new InvalidOperationException("Window handle already exists.");
            }
        }

        /// <summary>
        ///  Invokes the default window procedure associated with this Window. It is
        ///  an error to call this method when the Handle property is zero.
        /// </summary>
        public void DefWndProc(ref System.Windows.Forms.Message m)
        {
            Debug.Assert(m.HWnd == Handle, "ListViewHeaderSubclassedWindow is not attached to the window m is addressed to.");


        }

        /// <summary>
        ///  Specifies a notification method that is called when the handle for a
        ///  window is changed.
        /// </summary>
        protected virtual void OnHandleChange()
        {
        }

        /// <summary>
        ///  On class load, we connect an event to Application to let us know when
        ///  the process or domain terminates.  When this happens, we attempt to
        ///  clear our window class cache.  We cannot destroy windows (because we don't
        ///  have access to their thread), and we cannot unregister window classes
        ///  (because the classes are in use by the windows we can't destroy).  Instead,
        ///  we move the class and window procs to DefWndProc
        /// </summary>
        [System.Runtime.ConstrainedExecution.PrePrepareMethod]
        private static void OnShutdown(object sender, EventArgs e)
        {
            // No lock because access here should be race-free, no concurrent ListViewHeaderSubclassedWindow.AttachHandle/ReleaseHandle
            // should happen while shutting down.
            //Debug.Assert(0 == _instancesInUse.Count);
        }

        /// <summary>
        ///  When overridden in a derived class, manages an unhandled thread exception.
        /// </summary>
        protected virtual void OnThreadException(Exception e)
        {
        }

        private void InternalReleaseHandle()
        {
            Debug.Assert(Handle != IntPtr.Zero);
            RemoveWindowSubclass(Handle, _windowProcHandle, UIntPtr.Zero);
            Handle = IntPtr.Zero;
            OnHandleChange();
            --_uses;
        }

        /// <summary>
        ///  Releases the handle associated with this window.
        /// </summary>
        public void ReleaseHandle()
        {
            if (Handle != IntPtr.Zero)
            {
                InternalReleaseHandle();
                if (0 == _uses)
                {
                    lock (_instancesInUse)
                    {
                        _instancesInUse.Remove(this);
                    }
                }
            }
        }

        /// <summary>
        ///  Invokes the default window procedure associated with this window.
        /// </summary>
        protected virtual void WndProc(ref System.Windows.Forms.Message m)
        {
            DefWndProc(ref m);
        }
    }
}
