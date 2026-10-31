using SDUI.Helpers;
using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace SDUI.Controls;

public class GroupBox : System.Windows.Forms.GroupBox
{
    private int _shadowDepth = 4;
    public int ShadowDepth
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

    private int _radius = 10;
    public int Radius
    {
        get => _radius;
        set
        {
            _radius = value;

            Invalidate();
        }
    }

    private bool _borderless = false;
    public bool Borderless
    {
        get => _borderless;
        set
        {
            if (_borderless == value)
                return;

            _borderless = value;
            Invalidate();
        }
    }

    public GroupBox()
    {
        SetStyle(ControlStyles.SupportsTransparentBackColor |
            ControlStyles.AllPaintingInWmPaint |
                  ControlStyles.OptimizedDoubleBuffer |
                  ControlStyles.DoubleBuffer |
                  ControlStyles.ResizeRedraw |
                  ControlStyles.UserPaint, true);

        UpdateStyles();
        this.DoubleBuffered = true;
        this.BackColor = Color.Transparent;
        this.Padding = new Padding(8, 12, 8, 8);
    }

    protected override void OnParentBackColorChanged(EventArgs e)
    {
        base.OnParentBackColorChanged(e);
        Invalidate();
    }

    protected override void OnPaintBackground(PaintEventArgs e)
    {
        // Transparent child controls ask their parent to paint the surface behind
        // them. The card itself is painted in OnPaint, so mirror the card fill in
        // OnPaintBackground as well; otherwise labels and checkboxes render on the
        // page background instead of on the card (visible as dark rectangles).
        var graphics = e.Graphics;
        graphics.SmoothingMode = SmoothingMode.AntiAlias;

        GroupBoxRenderer.DrawParentBackground(graphics, ClientRectangle, this);

        if (_borderless)
            return;

        var rect = ClientRectangle.ToRectangleF();
        rect.Inflate(-1f, -1f);

        using var path = rect.Radius(_radius);
        using var brush = new SolidBrush(ColorScheme.CardColor);
        graphics.FillPath(brush, path);
    }

    protected override void OnControlAdded(ControlEventArgs e)
    {
        base.OnControlAdded(e);
        Invalidate(true);
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        var graphics = e.Graphics;
        graphics.SmoothingMode = SmoothingMode.AntiAlias;
        graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

        GroupBoxRenderer.DrawParentBackground(graphics, ClientRectangle, this);
        if (ColorScheme.DrawDebugBorders)
        {
            using var redPen = new Pen(Color.Red, 1);
            redPen.Alignment = PenAlignment.Inset;
            e.Graphics.DrawRectangle(redPen, new Rectangle(0, 0, Width - 1, Height - 1));
        }

        var rect = ClientRectangle.ToRectangleF();
        rect.Inflate(-1f, -1f);

        if (!_borderless)
        {
            using var path = rect.Radius(_radius);

            using (var brush = new SolidBrush(ColorScheme.CardColor))
                graphics.FillPath(brush, path);

            using (var borderPen = new Pen(ColorScheme.BorderColor, 1f))
                graphics.DrawPath(borderPen, path);
        }

        if (!string.IsNullOrWhiteSpace(Text))
        {
            var headerHeight = Font.Height + 6;
            var headerRect = new RectangleF(rect.Left + (_borderless ? 2 : 12), rect.Top + 4, Math.Max(0, rect.Width - (_borderless ? 4 : 24)), headerHeight);

            using var titleFont = new Font(Font.FontFamily, Font.Size, FontStyle.Bold);
            using var textFormat = new StringFormat
            {
                Alignment = RightToLeft == RightToLeft.Yes ? StringAlignment.Far : StringAlignment.Near,
                LineAlignment = StringAlignment.Center,
                Trimming = StringTrimming.EllipsisCharacter,
                FormatFlags = StringFormatFlags.NoWrap
            };
            if (RightToLeft == RightToLeft.Yes)
                textFormat.FormatFlags |= StringFormatFlags.DirectionRightToLeft;

            var titleColor = ColorScheme.ResolveTextColor(ForeColor);
            using var textBrush = new SolidBrush(titleColor);
            graphics.DrawString(DrawingExtensions.FormatBidiText(DrawingExtensions.StripEmojis(Text)), titleFont, textBrush, headerRect, textFormat);

            if (!_borderless)
            {
                var lineY = rect.Top + headerHeight + 2;
                using var linePen = new Pen(ColorScheme.BorderColor.Alpha(100), 1f);
                graphics.DrawLine(linePen, rect.Left + 8, lineY, rect.Right - 8, lineY);
            }
        }
    }

    public override Size GetPreferredSize(Size proposedSize)
    {
        var preferredSize = base.GetPreferredSize(proposedSize);
        preferredSize.Width += _shadowDepth;
        preferredSize.Height += _shadowDepth;

        return preferredSize;
    }
}