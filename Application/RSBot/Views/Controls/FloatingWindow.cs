using System.Windows.Forms;
using SDUI.Controls;

namespace RSBot.Views.Controls;

/// <summary>
///     A borderless, non-resizable window that hosts one of the detached
///     side panels (navigation rail or character sidebar).
/// </summary>
public class FloatingWindow : UIWindow
{
    public FloatingWindow()
    {
        ShowInTaskbar = false;
        StartPosition = FormStartPosition.Manual;
        Resizable = false;
        MaximizeBox = false;
        MinimizeBox = false;
        ShowControlBox = false;
        Movable = true;
    }

    /// <summary>
    ///     The detached panels must not steal focus from the main window when
    ///     they are displayed together.
    /// </summary>
    protected override bool ShowWithoutActivation => true;

    /// <summary>
    ///     Escape must never close an attached side panel.
    /// </summary>
    protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
    {
        if (keyData == Keys.Escape)
            return true;

        return base.ProcessCmdKey(ref msg, keyData);
    }
}
