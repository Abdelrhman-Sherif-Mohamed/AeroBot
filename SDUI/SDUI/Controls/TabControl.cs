using System;
using System.Drawing;
using System.Windows.Forms;

namespace SDUI.Controls;

public class TabControl : System.Windows.Forms.TabControl
{
    private Padding _borderRadius = new System.Windows.Forms.Padding(4);
    private int _hoveredTabIndex = -1;

    public Padding Radius
    {
        get => _borderRadius;
        set
        {
            _borderRadius = value;
            Invalidate();
        }
    }

    public TabControl()
    {
        SetStyle(ControlStyles.SupportsTransparentBackColor |
                 ControlStyles.OptimizedDoubleBuffer |
                 ControlStyles.ResizeRedraw |
                 ControlStyles.AllPaintingInWmPaint |
                 ControlStyles.UserPaint, true);

        UpdateStyles();
    }

    protected override void OnParentBackColorChanged(EventArgs e)
    {
        base.OnParentBackColorChanged(e);
        Invalidate();
    }

    protected override void CreateHandle()
    {
        base.CreateHandle();

        if (SizeMode != TabSizeMode.Fixed)
            ItemSize = new Size(Math.Max(ItemSize.Width, 90), Math.Max(ItemSize.Height, 26));

        Padding = new Point(14, 5);
        Alignment = TabAlignment.Top;
    }

    protected override void OnMouseDown(MouseEventArgs e)
    {
        base.OnMouseDown(e);

        if (e.Button == MouseButtons.Left)
        {
            for (int i = 0; i < TabCount; i++)
            {
                if (GetTabRect(i).Contains(e.Location))
                {
                    if (SelectedIndex != i)
                    {
                        var cancelArgs = new TabControlCancelEventArgs(TabPages[i], i, false, TabControlAction.Selecting);
                        OnSelecting(cancelArgs);
                        if (!cancelArgs.Cancel)
                        {
                            SelectedIndex = i;
                            var actionArgs = new TabControlEventArgs(TabPages[i], i, TabControlAction.Selected);
                            OnSelected(actionArgs);
                            Invalidate();
                        }
                    }
                    break;
                }
            }
        }
    }

    protected override void OnMouseMove(MouseEventArgs e)
    {
        base.OnMouseMove(e);

        int newHover = -1;
        for (int i = 0; i < TabCount; i++)
        {
            if (GetTabRect(i).Contains(e.Location))
            {
                newHover = i;
                break;
            }
        }

        if (_hoveredTabIndex != newHover)
        {
            _hoveredTabIndex = newHover;
            Cursor = newHover >= 0 ? Cursors.Hand : Cursors.Default;
            Invalidate();
        }
    }

    protected override void OnMouseLeave(EventArgs e)
    {
        base.OnMouseLeave(e);

        if (_hoveredTabIndex != -1)
        {
            _hoveredTabIndex = -1;
            Cursor = Cursors.Default;
            Invalidate();
        }
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        if (TabCount <= 0 || SelectedTab == null)
            return;

        var graphics = e.Graphics;

        GroupBoxRenderer.DrawParentBackground(graphics, this.ClientRectangle, this);

        graphics.SetHighQuality();
        using var borderBrush = new Pen(Color.FromArgb(70, 0, 0, 0));
        using var backBrush = new SolidBrush(ColorScheme.BackColor.IsDark() ? Color.FromArgb(10, 255, 255, 255) : Color.FromArgb(50, 0, 0, 0));

        // Draw container rectangle
        var r = new RectangleF(0, ItemSize.Height, Width - 1, Height - ItemSize.Height - 1);
        using (var path = r.Radius(SelectedIndex == 0 ? 2 : _borderRadius.Left, _borderRadius.Top, _borderRadius.Right, _borderRadius.Bottom))
        {
            graphics.DrawPath(borderBrush, path);
        }

        for (int i = 0; i <= TabCount - 1; i++)
        {
            var tabBounds = GetTabRect(i);
            var textRect = tabBounds;
            textRect.Offset(0, -2);

            if (i == SelectedIndex)
            {
                var rect = new RectangleF(tabBounds.X - 1, tabBounds.Y - 1, tabBounds.Width + 2, tabBounds.Height - 2);
                using var path = rect.Radius(4, 4, 0, 0);
                graphics.FillPath(backBrush, path);

                // Gold accent underline under the selected tab
                using var accentBrush = new SolidBrush(ColorScheme.AccentColor);
                graphics.FillRectangle(accentBrush, rect.X + 4, rect.Bottom - 3, rect.Width - 8, 3);
            }
            else if (i == _hoveredTabIndex)
            {
                var rect = new RectangleF(tabBounds.X - 1, tabBounds.Y - 1, tabBounds.Width + 2, tabBounds.Height - 2);
                using var path = rect.Radius(4, 4, 0, 0);
                using var hoverBrush = new SolidBrush(Color.FromArgb(ColorScheme.BackColor.IsDark() ? 25 : 15, ColorScheme.AccentColor));
                graphics.FillPath(hoverBrush, path);
            }

            var textColor = (i == SelectedIndex || i == _hoveredTabIndex) ? ColorScheme.ForeColor : Color.Gray;
            TabPages[i].DrawString(graphics, textColor, textRect);

            if (TabPages[i].BackColor != ColorScheme.BackColor)
                TabPages[i].BackColor = ColorScheme.BackColor;
        }

        graphics.SetDefaultQuality();
    }

    protected override CreateParams CreateParams
    {
        get
        {
            var parms = base.CreateParams;
            parms.Style &= ~0x02000000;  // Turn off WS_CLIPCHILDREN
            return parms;
        }
    }
}
