using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace SDUI.Controls;

public class ComboBox : System.Windows.Forms.ComboBox
{
    private const int WM_CTLCOLOREDIT = 0x0133;
    private const int WM_CTLCOLORLISTBOX = 0x0134;

    [DllImport("gdi32.dll")]
    private static extern int SetTextColor(IntPtr hdc, int color);

    [DllImport("gdi32.dll")]
    private static extern int SetBkColor(IntPtr hdc, int color);

    [DllImport("gdi32.dll")]
    private static extern IntPtr CreateSolidBrush(int color);

    [DllImport("gdi32.dll")]
    private static extern bool DeleteObject(IntPtr hObject);

    private IntPtr _editBackgroundBrush = IntPtr.Zero;
    private Color _editBackgroundColor = Color.Empty;

    private int _radius = 5;
    public int Radius
    {
        get => _radius;
        set
        {
            _radius = value;

            Invalidate();
        }
    }

    private float _shadowDepth = 4f;
    public float ShadowDepth
    {
        get => _shadowDepth;
        set
        {
            if (_shadowDepth == value)
                return;

            _shadowDepth = value;
            Invalidate();
        }
    }

    public ComboBox()
    {
        SetStyle(
            ControlStyles.UserPaint |
            ControlStyles.ResizeRedraw |
            ControlStyles.OptimizedDoubleBuffer |
            ControlStyles.AllPaintingInWmPaint |
            ControlStyles.Selectable |
            ControlStyles.SupportsTransparentBackColor, true
        );

        DrawMode = DrawMode.OwnerDrawVariable;
        DropDownStyle = ComboBoxStyle.DropDownList;
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing && _editBackgroundBrush != IntPtr.Zero)
        {
            DeleteObject(_editBackgroundBrush);
            _editBackgroundBrush = IntPtr.Zero;
        }

        base.Dispose(disposing);
    }

    protected override void WndProc(ref Message m)
    {
        // Editable combo boxes host a native EDIT child. Owner drawing only
        // covers the drop-down list, so the EDIT area would otherwise stay
        // system-white. Answer the native colour messages with theme colours.
        if ((m.Msg == WM_CTLCOLOREDIT || m.Msg == WM_CTLCOLORLISTBOX) && m.WParam != IntPtr.Zero)
        {
            var background = ColorScheme.InputBackColor;
            if (_editBackgroundBrush == IntPtr.Zero || _editBackgroundColor != background)
            {
                if (_editBackgroundBrush != IntPtr.Zero)
                    DeleteObject(_editBackgroundBrush);

                _editBackgroundBrush = CreateSolidBrush(ColorTranslator.ToWin32(background));
                _editBackgroundColor = background;
            }

            SetBkColor(m.WParam, ColorTranslator.ToWin32(background));
            SetTextColor(m.WParam, ColorTranslator.ToWin32(ColorScheme.ForeColor));
            m.Result = _editBackgroundBrush;
            return;
        }

        base.WndProc(ref m);
    }

    protected override void OnDrawItem(DrawItemEventArgs e)
    {
        var index = e.Index;
        if (index < 0 || index >= Items.Count)
            return;

        var foreColor = ColorScheme.ForeColor;

        if ((e.State & DrawItemState.Selected) == DrawItemState.Selected ||
               (e.State & DrawItemState.Focus) == DrawItemState.Focus ||
               (e.State & DrawItemState.NoFocusRect) != DrawItemState.NoFocusRect)
        {
            foreColor = Color.White;
            using var brush = new SolidBrush(ColorScheme.SelectionColor);
            e.Graphics.FillRectangle(brush, e.Bounds);
        }
        else
        {
            using var brush = new SolidBrush(ColorScheme.InputBackColor);
            e.Graphics.FillRectangle(brush, e.Bounds);
        }

        var stringFormat = new StringFormat
        {
            LineAlignment = StringAlignment.Center,
            Alignment = StringAlignment.Near,
            FormatFlags = StringFormatFlags.NoWrap,
            Trimming = StringTrimming.EllipsisCharacter
        };

        if (RightToLeft == RightToLeft.Yes)
            stringFormat.FormatFlags |= StringFormatFlags.DirectionRightToLeft;

        using var textBrush = new SolidBrush(foreColor);
        e.Graphics.DrawString(Items[index].ToString(), e.Font, textBrush, e.Bounds, stringFormat);
    }

    protected override void OnParentBackColorChanged(EventArgs e)
    {
        base.OnParentBackColorChanged(e);
        Invalidate();
    }

    protected override void OnLostFocus(EventArgs e)
    {
        base.OnLostFocus(e);
        SuspendLayout();
        Update();
        ResumeLayout();
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        var graphics = e.Graphics;
        graphics.SmoothingMode = SmoothingMode.AntiAlias;

        ButtonRenderer.DrawParentBackground(graphics, ClientRectangle, this);

        var rectf = ClientRectangle.ToRectangleF();

        if (ColorScheme.DrawDebugBorders)
        {
            using var redPen = new Pen(Color.Red, 1);
            redPen.Alignment = PenAlignment.Outset;
            graphics.DrawRectangle(redPen, 0, 0, rectf.Width - 1, rectf.Height - 1);
        }

        var inflate = _shadowDepth / 4f;
        //rectf.Inflate(-inflate, -inflate);

        var textRectangle = new Rectangle(6 * (DeviceDpi / 96), 0, Width - (28 * (DeviceDpi / 96)), Height);

        var backColor = ColorScheme.InputBackColor;
        var borderColor = DroppedDown || Focused ? ColorScheme.AccentColor : ColorScheme.BorderColor;

        using var path = rectf.Radius(_radius);

        using var backBrush = new SolidBrush(backColor);
        e.Graphics.FillPath(backBrush, path);

        
        var _extendBoxRect = new RectangleF(rectf.Width - (24f * (DeviceDpi / 96)), 0, (16 * (DeviceDpi / 96)), rectf.Height - (4 * (DeviceDpi / 96)) + _shadowDepth);

        using var symbolPen = new Pen(ColorScheme.ForeColor, 1.5f);
        graphics.DrawLine(symbolPen,
                _extendBoxRect.Left + _extendBoxRect.Width / 2 - (5 * (DeviceDpi / 96)) - 1,
                _extendBoxRect.Top + _extendBoxRect.Height / 2 - (2 * (DeviceDpi / 96)),
                _extendBoxRect.Left + _extendBoxRect.Width / 2 - (1 * (DeviceDpi / 96)),
                _extendBoxRect.Top + _extendBoxRect.Height / 2 + (3 * (DeviceDpi / 96)));

        graphics.DrawLine(symbolPen,
            _extendBoxRect.Left + _extendBoxRect.Width / 2 + (5 * (DeviceDpi / 96)) - 1,
            _extendBoxRect.Top + _extendBoxRect.Height / 2 - (2 * (DeviceDpi / 96)),
            _extendBoxRect.Left + _extendBoxRect.Width / 2 - (1 * (DeviceDpi / 96)),
            _extendBoxRect.Top + _extendBoxRect.Height / 2 + (3 * (DeviceDpi / 96)));

        using var borderPen = new Pen(borderColor, 1f);
        e.Graphics.DrawPath(borderPen, path);

        if (DropDownStyle == ComboBoxStyle.DropDownList)
        {
            var flags = TextFormatFlags.EndEllipsis | TextFormatFlags.VerticalCenter | TextFormatFlags.TextBoxControl;
            if (RightToLeft == RightToLeft.Yes)
                flags |= TextFormatFlags.RightToLeft;
            TextRenderer.DrawText(graphics, Text, Font, textRectangle, ColorScheme.ForeColor, flags);
        }
    }

    public override Size GetPreferredSize(Size proposedSize)
    {
        var preferredSize = base.GetPreferredSize(proposedSize);
        preferredSize.Width += (int)_shadowDepth;
        preferredSize.Height += (int)_shadowDepth;

        return preferredSize;
    }
}
