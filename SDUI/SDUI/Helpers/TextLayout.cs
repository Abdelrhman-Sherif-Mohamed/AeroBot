using System;
using System.Drawing;
using System.Windows.Forms;

namespace SDUI.Helpers
{
    /// <summary>
    ///     Central text-layout rules: comfortable Arabic line-height, safe gaps
    ///     and growth caps so labels/checkboxes never collide with inputs.
    /// </summary>
    public static class TextLayout
    {
        /// <summary>
        ///     Gap kept between a text control and the next control to its right (px).
        /// </summary>
        public const int Gap = 8;

        /// <summary>
        ///     Comfortable Arabic line-height (1.5x the natural font line height).
        /// </summary>
        public const float LineHeightRatio = 1.5f;

        /// <summary>
        ///     Line height (px) for a control's font at LineHeightRatio.
        /// </summary>
        public static float LineHeight(Control control)
        {
            try
            {
                return control.Font.GetHeight() * LineHeightRatio;
            }
            catch
            {
                return control.Font.Height * LineHeightRatio;
            }
        }

        /// <summary>
        ///     Width limit imposed by the control's parent (right margin).
        /// </summary>
        public static int ParentLimit(Control ctl, int rightMargin = 4)
        {
            if (ctl.Parent == null)
                return int.MaxValue;

            return Math.Max(10, ctl.Parent.ClientSize.Width - ctl.Left - rightMargin);
        }

        /// <summary>
        ///     Width limit imposed by the nearest control to the right on the same row,
        ///     minus the gap. Returns int.MaxValue when nothing sits on the right.
        /// </summary>
        public static int NextSiblingLimit(Control ctl, int gap = Gap)
        {
            var parent = ctl.Parent;
            if (parent == null)
                return int.MaxValue;

            var top = ctl.Top;
            var bottom = ctl.Bottom;
            var right = ctl.Right;

            var edge = int.MaxValue;
            foreach (Control other in parent.Controls)
            {
                if (other == ctl || !other.Visible || !other.Enabled)
                    continue;

                if (other.Top < bottom && other.Bottom > top && other.Left >= right - 1 && other.Left < edge)
                    edge = other.Left;
            }

            if (edge == int.MaxValue)
                return int.MaxValue;

            return Math.Max(10, edge - ctl.Left - gap);
        }

        /// <summary>
        ///     Width of the given text measured with GenericTypographic (Arabic-safe),
        ///     plus the extra padding.
        /// </summary>
        public static int AutoWidth(Control ctl, string text, int extra = 12)
        {
            if (string.IsNullOrEmpty(text))
                return extra;

            try
            {
                using var g = ctl.CreateGraphics();
                return (int)Math.Ceiling(g.MeasureString(text, ctl.Font).Width) + extra;
            }
            catch
            {
                return ctl.Width;
            }
        }
    }
}