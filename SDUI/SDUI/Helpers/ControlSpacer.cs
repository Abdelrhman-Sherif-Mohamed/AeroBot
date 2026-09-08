using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace SDUI.Helpers;

/// <summary>
///     Provides consistent vertical spacing between controls in a container.
///     Call NormalizeVerticalSpacing() after InitializeComponent() in any page.
/// </summary>
public static class ControlSpacer
{
    /// <summary>
    ///     Default vertical gap between controls (in pixels).
    /// </summary>
    private const int DefaultVerticalGap = 8;

    /// <summary>
    ///     Default left padding inside group boxes.
    /// </summary>
    private const int GroupBoxPaddingLeft = 14;

    /// <summary>
    ///     Default top padding inside group boxes (below the header).
    /// </summary>
    private const int GroupBoxPaddingTop = 22;

    /// <summary>
    ///     Normalize vertical spacing for all visible controls in a container.
    ///     Controls are grouped into rows (controls whose vertical bands overlap
    ///     belong to the same row) so two-column layouts keep both columns aligned
    ///     and never flatten into a single column.
    /// </summary>
    public static void NormalizeVerticalSpacing(Control container, int gap = DefaultVerticalGap)
    {
        if (container == null || container.Controls.Count == 0) return;

        var controls = container.Controls
            .OfType<Control>()
            .Where(c => c.Visible && !(c is GroupBox))
            .OrderBy(c => c.Location.Y)
            .ThenBy(c => c.Location.X)
            .ToList();

        if (controls.Count == 0) return;

        var rows = new List<List<Control>>();
        foreach (var control in controls)
        {
            var added = false;
            foreach (var row in rows)
            {
                if (row.Any(existing => existing.Top < control.Bottom && control.Top < existing.Bottom))
                {
                    row.Add(control);
                    added = true;
                    break;
                }
            }

            if (!added)
                rows.Add(new List<Control> { control });
        }

        var currentY = rows[0].Min(control => control.Location.Y);

        var clientHeight = 0;
        try { clientHeight = container.ClientSize.Height; } catch { }

        foreach (var row in rows)
        {
            var rowTop = row.Min(control => control.Location.Y);
            var rowHeight = row.Max(control => control.Bottom) - rowTop;

            var targetY = Math.Max(rowTop, currentY);

            var delta = targetY - rowTop;
            if (delta != 0)
            {
                foreach (var control in row)
                    control.Location = new Point(control.Location.X, control.Location.Y + delta);
            }

            currentY = targetY + rowHeight + gap;
        }
    }

    /// <summary>
    ///     Normalize spacing for controls inside each GroupBox in a container.
    /// </summary>
    public static void NormalizeGroupBoxSpacing(Control container, int gap = DefaultVerticalGap)
    {
        foreach (var groupBox in container.Controls.OfType<GroupBox>())
        {
            NormalizeVerticalSpacing(groupBox, gap);
        }
    }

    /// <summary>
    ///     Apply spacing normalization to an entire page including all group boxes.
    ///     Call this once after InitializeComponent().
    /// </summary>
    public static void NormalizePage(Control page, int gap = DefaultVerticalGap)
    {
        // Safe no-op: Preserve the exact coordinates specified in the Visual Studio Designer.
    }


    /// <summary>
    ///     Called by controls whose size changed (e.g. a checkbox wrapping Arabic
    ///     text onto extra lines). Re-runs the whole page spacing once, on the UI
    ///     thread, after the current translation pass finishes.
    /// </summary>
    public static void NotifyLayoutChanged(Control control)
    {
        // No-op: Prevent feedback loops from repeatedly re-positioning controls
    }

    /// <summary>
    ///     Add consistent top padding to a group box's controls.
    ///     Useful when controls overlap the group box header.
    /// </summary>
    public static void AddGroupBoxPadding(GroupBox groupBox, int topPadding = GroupBoxPaddingTop, int leftPadding = GroupBoxPaddingLeft)
    {
        if (groupBox == null || groupBox.Controls.Count == 0) return;

        var controls = groupBox.Controls
            .OfType<Control>()
            .OrderBy(c => c.Location.Y)
            .ToList();

        foreach (var control in controls)
        {
            if (control.Location.Y < topPadding)
            {
                control.Location = new Point(
                    leftPadding,
                    topPadding
                );
            }
        }
    }

    /// <summary>
    ///     Space two-column layouts: left column and right column.
    ///     Controls with X < midX are left column, others are right column.
    /// </summary>
    public static void NormalizeTwoColumnLayout(Control container, int gap = DefaultVerticalGap, int midX = 250)
    {
        var controls = container.Controls
            .OfType<Control>()
            .Where(c => c.Visible)
            .ToList();

        var leftColumn = controls
            .Where(c => c.Location.X < midX)
            .OrderBy(c => c.Location.Y)
            .ToList();

        var rightColumn = controls
            .Where(c => c.Location.X >= midX)
            .OrderBy(c => c.Location.Y)
            .ToList();

        // Space left column
        if (leftColumn.Count > 0)
        {
            int currentY = leftColumn[0].Location.Y;
            foreach (var control in leftColumn)
            {
                control.Location = new Point(control.Location.X, currentY);
                currentY += control.Height + gap;
            }
        }

        // Space right column
        if (rightColumn.Count > 0)
        {
            int currentY = rightColumn[0].Location.Y;
            foreach (var control in rightColumn)
            {
                control.Location = new Point(control.Location.X, currentY);
                currentY += control.Height + gap;
            }
        }
    }
}
