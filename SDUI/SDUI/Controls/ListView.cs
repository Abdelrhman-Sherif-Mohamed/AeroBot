using SDUI.Controls.Subclasses;
using SDUI.Helpers;
using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using static SDUI.NativeMethods;

namespace SDUI.Controls;

public class ListView : System.Windows.Forms.ListView
{
    /// <summary>
    /// The column sorter
    /// </summary>
    private ListViewColumnSorter LvwColumnSorter { get; set; }

    /// <summary>
    /// The header sub class
    /// </summary>
    private ListViewHeaderSubclassedWindow _headerSubClass;

    /// <summary>
    /// Initializes a new instance of the <see cref="AeroListView"/> class.
    /// </summary>
    public ListView()
        : base()
    {
        SetStyle(
            ControlStyles.AllPaintingInWmPaint |
            ControlStyles.OptimizedDoubleBuffer |
            ControlStyles.EnableNotifyMessage, true);

        DoubleBuffered = true;
        BackColor = ColorScheme.TableBackColor;
        ForeColor = ColorScheme.ForeColor;

        LvwColumnSorter = new ListViewColumnSorter();
        ListViewItemSorter = LvwColumnSorter;
        View = View.Details;
        FullRowSelect = true;
        UpdateStyles();

        _headerSubClass = new();

    }

    protected override void OnSelectedIndexChanged(EventArgs e)
    {
        base.OnSelectedIndexChanged(e);
        Invalidate();
    }

    protected override void OnMouseDown(MouseEventArgs e)
    {
        base.OnMouseDown(e);
        Invalidate();
    }

    protected override void OnMouseWheel(MouseEventArgs e)
    {
        base.OnMouseWheel(e);
    }

    /// <summary>
    /// Raises the <see cref="E:HandleCreated" /> event.
    /// </summary>
    /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
    protected override void OnHandleCreated(EventArgs e)
    {
        base.OnHandleCreated(e);

        BackColor = ColorScheme.TableBackColor;
        ForeColor = ColorScheme.ForeColor;

        WindowsHelper.UseImmersiveDarkMode(Handle, ColorScheme.BackColor.IsDark());
        _headerSubClass.AssignHandle(this);
        SendMessage(Handle, LVM_SETEXTENDEDLISTVIEWSTYLE, LVS_EX_DOUBLEBUFFER, LVS_EX_DOUBLEBUFFER);

        ApplyNativeTheme();
    }

    protected override void OnBackColorChanged(EventArgs e)
    {
        base.OnBackColorChanged(e);
        ApplyNativeTheme();
    }

    /// <summary>
    ///     Applies the active theme directly to the native list view. The
    ///     standard BackColor/ForeColor properties alone are not enough for an
    ///     empty (or disabled) list body.
    /// </summary>
    internal void ApplyNativeTheme()
    {
        if (!IsHandleCreated)
            return;

        try
        {
            // An unthemed list view ignores LVM_SETBKCOLOR for empty bodies
            // and falls back to the system button face. Activating the native
            // Explorer theme makes the list honor the colors below.
            SetWindowTheme(Handle, "Explorer", null);

            SendMessage(Handle, 0x1001, IntPtr.Zero, (IntPtr)ColorTranslator.ToWin32(ColorScheme.TableBackColor));
            SendMessage(Handle, 0x1024, IntPtr.Zero, (IntPtr)ColorTranslator.ToWin32(ColorScheme.ForeColor));
            SendMessage(Handle, 0x1026, IntPtr.Zero, (IntPtr)ColorTranslator.ToWin32(ColorScheme.TableBackColor));

            var header = SendMessage(Handle, LVM_GETHEADER, IntPtr.Zero, IntPtr.Zero);
            if (header != IntPtr.Zero && WindowsHelper.IsModern)
            {
                AllowDarkModeForWindow(header, ColorScheme.BackColor.IsDark());
                SetWindowTheme(header, ColorScheme.BackColor.IsDark() ? "DarkMode_ItemsView" : "ItemsView", null);
            }
        }
        catch
        {
            // Native theming must never break the page.
        }
    }

    protected override void OnNotifyMessage(Message m)
    {
        if (m.Msg != 0x14)
        {
            base.OnNotifyMessage(m);
        }
    }

    /// <summary>
    /// Raises the <see cref="E:ColumnClick" /> event.
    /// </summary>
    /// <param name="e">The <see cref="ColumnClickEventArgs"/> instance containing the event data.</param>
    protected override void OnColumnClick(ColumnClickEventArgs e)
    {
        base.OnColumnClick(e);

        for (int i = 0; i < Columns.Count; i++)
            SetSortArrow(i, SortOrder.None);

        // Determine if clicked column is already the column that is being sorted.
        if (e.Column == LvwColumnSorter.SortColumn)
        {
            // Reverse the current sort direction for this column.
            LvwColumnSorter.Order = (LvwColumnSorter.Order == SortOrder.Ascending)
                ? SortOrder.Descending
                : SortOrder.Ascending;
        }
        else
        {
            // Set the column number that is to be sorted; default to ascending.
            LvwColumnSorter.SortColumn = e.Column;
            LvwColumnSorter.Order = SortOrder.Ascending;
        }

        SetSortArrow(e.Column, LvwColumnSorter.Order);

        // Perform the sort with these new sort options.
        if (!VirtualMode)
            Sort();
    }

    /// <summary>
    /// Select all rows on the given listview
    /// </summary>
    /// <param name="list">The listview whose items are to be selected</param>
    public void SelectAllItems()
    {
        var s = System.Diagnostics.Stopwatch.StartNew();
        Focus();
        SetItemState(-1, 2, 2);
        //MessageBox.Show($"Selected in: {s.ElapsedMilliseconds} ms");
    }

    /// <summary>
    /// Deselect all rows on the given listview
    /// </summary>
    /// <param name="list">The listview whose items are to be deselected</param>
    public void DeselectAllItems()
    {
        SetItemState(-1, 2, 0);
    }

    /// <summary>
    /// Set the item state on the given item
    /// </summary>
    /// <param name="list">The listview whose item's state is to be changed</param>
    /// <param name="itemIndex">The index of the item to be changed</param>
    /// <param name="mask">Which bits of the value are to be set?</param>
    /// <param name="value">The value to be set</param>
    public void SetItemState(int itemIndex, int mask, int value)
    {
        LVITEM lvItem = new LVITEM();
        lvItem.stateMask = mask;
        lvItem.state = value;
        SendMessageLVItem(Handle, LVM_SETITEMSTATE, itemIndex, ref lvItem);

        EnsureVisible(itemIndex);
    }

    public int SetGroupInfo(IntPtr hWnd, int nGroupID, uint nSate)
    {
        var lvg = new LVGROUP();
        lvg.cbSize = (uint)Marshal.SizeOf(lvg);
        lvg.mask = LVGF_STATE | LVGF_GROUPID | LVGF_HEADER;
        // for test
        SendMessage(hWnd, LVM_GETGROUPINFO, nGroupID, ref lvg);
        lvg.state = nSate;
        lvg.mask = LVGF_STATE;
        SendMessage(hWnd, LVM_SETGROUPINFO, nGroupID, ref lvg);
        return -1;
    }

    private void PaintClientBackground()
    {
        if (!IsHandleCreated || IsDisposed)
            return;

        try
        {
            var fill = ClientRectangle;
            if (fill.Width <= 0 || fill.Height <= 0)
                return;

            if (Items.Count > 0)
            {
                Rectangle lastItem;
                try
                {
                    lastItem = GetItemRect(Items.Count - 1, ItemBoundsPortion.Entire);
                }
                catch
                {
                    // The last item is not realized; do not risk painting over
                    // visible rows.
                    return;
                }

                if (lastItem.Bottom > fill.Top)
                {
                    if (lastItem.Bottom >= fill.Bottom)
                        return;

                    fill.Y = lastItem.Bottom;
                    fill.Height = ClientRectangle.Bottom - lastItem.Bottom;
                }
            }

            if (fill.Height <= 0)
                return;

            using (var graphics = Graphics.FromHwnd(Handle))
            using (var brush = new SolidBrush(ColorScheme.TableBackColor))
            {
                graphics.FillRectangle(brush, fill);
            }
        }
        catch
        {
            // Painting must never break the list.
        }
    }

    protected override void WndProc(ref Message m)
    {
        if (m.Msg == WM_PAINT)
        {
            base.WndProc(ref m);
            PaintClientBackground();
            return;
        }

        // The native list view paints the area below its last item with the
        // system window color, even when LVM_SETBKCOLOR is set. Fill the client
        // background ourselves so the whole list body keeps the dark theme.
        if (m.Msg == WM_ERASEBKGND)
        {
            try
            {
                using (var graphics = Graphics.FromHdc(m.WParam))
                using (var brush = new SolidBrush(ColorScheme.TableBackColor))
                {
                    graphics.FillRectangle(brush, ClientRectangle);
                }
            }
            catch
            {
                // Painting must never break the list.
            }

            m.Result = (IntPtr)1;
            return;
        }

        if (m.Msg == WM_REFLECT + WM_NOFITY)
        {
            var pnmhdr = (NMHDR)m.GetLParam(typeof(NMHDR));
            if (pnmhdr.code == NM_CUSTOMDRAW)
            {
                var pnmlv = (NMLVCUSTOMDRAW)m.GetLParam(typeof(NMLVCUSTOMDRAW));
                switch ((CDDS)pnmlv.nmcd.dwDrawStage)
                {
                    case CDDS.CDDS_PREPAINT:
                        if (pnmlv.dwItemType == LVCDI_GROUP)
                        {
                            var rectHeader = new Rect
                            {
                                Top = LVGGR_HEADER
                            };
                            var nItem = (int)pnmlv.nmcd.dwItemSpec;

                            SendMessage(m.HWnd, LVM_GETGROUPRECT, nItem, ref rectHeader);

                            using (var graphics = Graphics.FromHdc(pnmlv.nmcd.hdc))
                            {
                                var rect = new Rectangle(rectHeader.Left, rectHeader.Top, rectHeader.Right - rectHeader.Left, rectHeader.Bottom - rectHeader.Top);

                                //var backgroundBrush = new SolidBrush(_groupHeadingBackColor);
                                //graphics.FillRectangle(backgroundBrush, rect);

                                var lvg = new LVGROUP();
                                lvg.cbSize = (uint)Marshal.SizeOf(lvg);
                                lvg.mask = LVGF_STATE | LVGF_GROUPID | LVGF_HEADER;

                                SendMessage(m.HWnd, LVM_GETGROUPINFO, nItem, ref lvg);
                                var sText = Marshal.PtrToStringUni(lvg.pszHeader);
                                var textSize = graphics.MeasureString(sText, Font);

                                var rectHeightMiddle = (int)Math.Round((rect.Height - textSize.Height) / 2f);

                                rect.Offset(10, rectHeightMiddle);

                                var color = Color.FromArgb(80, 1, 52, 153).Brightness(ColorScheme.BackColor.Determine().GetBrightness());
                                using (var drawBrush = new SolidBrush(color))
                                {
                                    TextRenderer.DrawText(graphics, sText, Font, rect, color, TextFormatFlags.Left);

                                    rect.Offset(0, -rectHeightMiddle);

                                    using (var lineBrush = new SolidBrush(color))
                                    {
                                        graphics.DrawLine(new Pen(lineBrush), rect.X + graphics.MeasureString(sText, Font).Width + 10, rect.Y + (int)Math.Round(rect.Height / 2d), rect.X + (int)Math.Round(rect.Width * 95 / 100d), rect.Y + (int)Math.Round(rect.Height / 2d));
                                    }
                                }
                            }

                            m.Result = new IntPtr((int)CDRF.CDRF_SKIPDEFAULT); return;
                        }
                        else
                        {
                            // Fill the empty body (and the area below the last
                            // item) with the table background. Without this,
                            // comctl32 uses the theme's default light brush.
                            pnmlv.clrTextBk = ColorTranslator.ToWin32(ColorScheme.TableBackColor);
                            pnmlv.clrText = ColorTranslator.ToWin32(ColorScheme.ForeColor);
                            Marshal.StructureToPtr(pnmlv, m.LParam, false);

                            m.Result = new IntPtr((int)CDRF.CDRF_NOTIFYITEMDRAW);
                        }

                        break;

                    case CDDS.CDDS_ITEMPREPAINT:
                        if (ColorScheme.BackColor.IsDark())
                        {
                            var itemIndex = (int)pnmlv.nmcd.dwItemSpec;
                            bool isAlt = (itemIndex % 2) == 1;
                            pnmlv.clrTextBk = ColorTranslator.ToWin32(isAlt ? ColorScheme.TableAlternateRowColor : ColorScheme.TableBackColor);
                            pnmlv.clrText = ColorTranslator.ToWin32(ColorScheme.ForeColor);
                            Marshal.StructureToPtr(pnmlv, m.LParam, false);
                            m.Result = new IntPtr((int)CDRF.CDRF_NEWFONT);
                            return;
                        }

                        break;
                }
            }
        }
        else if (m.Msg == WM_THEMECHANGED)
        {
            //AllowDarkModeForWindow(m.HWnd, ColorScheme.BackColor.IsDark());
            try
            {

                BackColor = ColorScheme.TableBackColor;
                ForeColor = ColorScheme.ForeColor;
            }
            catch (Exception)
            {
            }
        }

        base.WndProc(ref m);
    }

    public void SetSortArrow(int column, SortOrder sortOrder)
    {
        var pHeader = SendMessage(this.Handle, LVM_GETHEADER, 0, 0);

        var pColumn = new IntPtr(column);
        var headerItem = new HDITEM { mask = HDITEM.Mask.Format };

        SendMessage(pHeader, HDM_GETITEM, pColumn, ref headerItem);

        switch (sortOrder)
        {
            case SortOrder.Ascending:
                headerItem.fmt &= ~HDITEM.Format.SortDown;
                headerItem.fmt |= HDITEM.Format.SortUp;
                break;
            case SortOrder.Descending:
                headerItem.fmt &= ~HDITEM.Format.SortUp;
                headerItem.fmt |= HDITEM.Format.SortDown;
                break;
            case SortOrder.None:
                headerItem.fmt &= ~(HDITEM.Format.SortDown | HDITEM.Format.SortUp);
                break;
        }

        SendMessage(pHeader, HDM_SETITEM, pColumn, ref headerItem);
    }
}