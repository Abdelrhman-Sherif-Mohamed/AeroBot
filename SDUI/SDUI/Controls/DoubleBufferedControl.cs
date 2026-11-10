using System.Windows.Forms;

namespace SDUI.Controls
{
    public class DoubleBufferedControl : UserControl
    {
        public DoubleBufferedControl() { 
            SetStyle(ControlStyles.OptimizedDoubleBuffer, true);
            SetStyle(ControlStyles.AllPaintingInWmPaint, true);
            SetStyle(ControlStyles.DoubleBuffer, true);
        }

        protected override CreateParams CreateParams
        {
            get
            {
                CreateParams cp = base.CreateParams;
                // Note: WS_EX_COMPOSITED (0x02000000) intentionally omitted here because it breaks
                // hardware accelerated / DirectComposition child HWND controls such as WebView2 (rendering black screen).
                return cp;
            }
        }
    }
}
