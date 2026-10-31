using SDUI.Helpers;
using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace SDUI.Controls;

public class Label : System.Windows.Forms.Label
{
    public float Angle = 45;
    public bool ApplyGradient { get; set; }

    private bool _gradientAnimation;
    public bool GradientAnimation
    {
        get => _gradientAnimation;
        set
        {
            _gradientAnimation = value;
            Invalidate();
        }
    }

    /// <summary>
    /// Gradient text colors
    /// </summary>
    private Color[] _gradient = new[] { Color.Gray, Color.Black };
    public Color[] Gradient
    {
        get => _gradient;
        set
        {
            _gradient = value; Invalidate();
        }
    }

    public Label()
    {
        SetStyle(ControlStyles.UserPaint | ControlStyles.SupportsTransparentBackColor | ControlStyles.OptimizedDoubleBuffer, true);

        // Labels are drawn directly on whatever surface their parent paints
        // (cards, panels, gradients). An opaque inherited BackColor would be
        // painted as a dark rectangle behind the text.
        BackColor = Color.Transparent;
    }

    protected override void OnParentBackColorChanged(EventArgs e)
    {
        base.OnParentBackColorChanged(e);
        Invalidate();
    }

    protected override void OnSizeChanged(EventArgs e)
    {
        base.OnSizeChanged(e);

        Invalidate();
    }

    protected override void OnTextChanged(EventArgs e)
    {
        base.OnTextChanged(e);

        FitTextToContent();
        Invalidate();
    }

    protected override void OnFontChanged(EventArgs e)
    {
        base.OnFontChanged(e);

        FitTextToContent();
        Invalidate();
    }

    protected override void OnHandleCreated(EventArgs e)
    {
        base.OnHandleCreated(e);
        FitTextToContent();
    }

    /// <summary>
    ///     Auto-fit: big words always get the room they need (Arabic-safe) and are
    ///     never ellipsized. The width stops at the parent edge or at the next
    ///     control to the right; longer text wraps onto extra lines and the height
    ///     grows so descriptive text flows instead of truncating with "...".
    /// </summary>
    private bool _isFittingText;

    private void FitTextToContent()
    {
        // Only auto-fit controls that actually opted into automatic sizing.
        // Fixed-layout pages rely on their explicit bounds and must never be
        // resized or pushed around by text measurement at runtime.
        if (DesignMode || _isFittingText || !IsHandleCreated || !AutoSize || Dock != DockStyle.None)
            return;

        try
        {
            _isFittingText = true;
            if (!string.IsNullOrEmpty(Text))
            {
                var textWidth = TextRenderer.MeasureText(Text, Font).Width;
                int extraPadding = DrawingExtensions.HasArabic(Text) ? (int)(textWidth * 0.25f) + 16 : 14;
                var need = textWidth + extraPadding;

                if (Parent != null)
                {
                    var maxLimit = Math.Max(Width, Parent.ClientSize.Width - Left - 4);
                    var targetWidth = Math.Min(need, maxLimit);
                    if (targetWidth > Width)
                    {
                        TextLayout.ShiftRightSiblingsIfNeeded(this, targetWidth, 8);
                        Width = targetWidth;
                    }
                }
                else if (need > Width)
                {
                    Width = need;
                }

                using var g = CreateGraphics();
                var usableWidth = Math.Max(1, Width - 4);
                var measured = g.MeasureString(Text, Font, usableWidth);
                var neededHeight = (int)Math.Ceiling(measured.Height) + 4;

                if (neededHeight > Height && Parent != null && Height < Parent.ClientSize.Height - Top)
                {
                    Height = Math.Min(neededHeight, Parent.ClientSize.Height - Top - 4);
                }
            }
        }
        catch
        {
        }
        finally
        {
            _isFittingText = false;
        }
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        if (BackColor != Color.Transparent && !BackColor.IsEmpty && BackColor.A > 0)
        {
            using var bgBrush = new SolidBrush(BackColor);
            e.Graphics.FillRectangle(bgBrush, ClientRectangle);
        }

        if (GradientAnimation)
            Angle = Angle % 360 + 1;

        e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
        e.Graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;

        if (ApplyGradient)
        {
            var c1 = ColorScheme.ResolveTextColor(_gradient[0]);
            var c2 = ColorScheme.ResolveTextColor(_gradient[1]);
            using var brush = new LinearGradientBrush(ClientRectangle, c1, c2, Angle/*LinearGradientMode.Horizontal */);

            using var format = this.CreateStringFormat(TextAlign, AutoEllipsis, UseMnemonic);
            e.Graphics.DrawString(Text, Font, brush, ClientRectangle, format);
        }
        else
        {
            var foreColor = ColorScheme.ResolveTextColor(ForeColor);
            bool singleLine = Height <= Font.Height * 1.6f;
            this.DrawString(e.Graphics, TextAlign, foreColor, AutoEllipsis, UseMnemonic, noWrap: singleLine);
        }
    }
}