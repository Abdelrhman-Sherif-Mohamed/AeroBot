using SDUI.Animation;
using SDUI.Helpers;
using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.Windows.Forms;

namespace SDUI.Controls;

public class Radio : RadioButton
{

    private const int RADIOBUTTON_INNER_CIRCLE_SIZE = RADIOBUTTON_SIZE - (2 * RADIOBUTTON_OUTER_CIRCLE_WIDTH);

    private const int RADIOBUTTON_OUTER_CIRCLE_WIDTH = 1;

    // size constants
    private const int RADIOBUTTON_SIZE = 15;

    private const int RADIOBUTTON_SIZE_HALF = RADIOBUTTON_SIZE / 2;

    // animation managers
    private readonly Animation.AnimationEngine animationManager;

    private readonly Animation.AnimationEngine rippleAnimationManager;

    private int boxOffset;
    private int _mouseState;

    // size related variables which should be recalculated onsizechanged
    private Rectangle radioButtonBounds;

    private bool ripple;
    private bool _isFittingText;

    [Browsable(false)]
    private Point _mouseLocation { get; set; }

    [Category("Behavior")]
    public bool Ripple
    {
        get { return ripple; }
        set
        {
            ripple = value;
            AutoSize = AutoSize; //Make AutoSize directly set the bounds.

            if (value)
            {
                Margin = new Padding(0);
            }

            Invalidate();
        }
    }

    public Radio()
    {
        SetStyle(ControlStyles.UserPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.AllPaintingInWmPaint | ControlStyles.SupportsTransparentBackColor, true);

        BackColor = Color.Transparent;

        animationManager = new Animation.AnimationEngine
        {
            AnimationType = AnimationType.EaseInOut,
            Increment = 0.06
        };
        rippleAnimationManager = new Animation.AnimationEngine(false)
        {
            AnimationType = AnimationType.Linear,
            Increment = 0.10,
            SecondaryIncrement = 0.08
        };
        animationManager.OnAnimationProgress += sender => Invalidate();
        rippleAnimationManager.OnAnimationProgress += sender => Invalidate();

        CheckedChanged += (sender, args) => animationManager.StartNewAnimation(Checked ? AnimationDirection.In : AnimationDirection.Out);

        SizeChanged += OnSizeChanged;

        Ripple = true;
        _mouseLocation = new Point(-1, -1);
    }

    public override Size GetPreferredSize(Size proposedSize)
    {
        var width = boxOffset + RADIOBUTTON_SIZE + TextLayout.Gap + TextRenderer.MeasureText(Text, Font).Width + 8;
        return new Size(width, 24);
    }

    protected override void OnTextChanged(EventArgs e)
    {
        base.OnTextChanged(e);

        FitTextToContent();
    }

    protected override void OnFontChanged(EventArgs e)
    {
        base.OnFontChanged(e);

        FitTextToContent();
    }

    protected override void OnHandleCreated(EventArgs e)
    {
        base.OnHandleCreated(e);

        FitTextToContent();
    }

    /// <summary>
    ///     Auto-fit: the radio width grows so words are never clipped.
    /// </summary>
    private void FitTextToContent()
    {
        if (DesignMode || !IsHandleCreated || _isFittingText || !AutoSize || Dock != DockStyle.None)
            return;

        try
        {
            _isFittingText = true;
            if (!string.IsNullOrEmpty(Text))
            {
                int gap = Math.Max(10, TextLayout.Gap);
                var textWidth = TextRenderer.MeasureText(Text, Font).Width;
                int extraPadding = DrawingExtensions.HasArabic(Text) ? (int)(textWidth * 0.25f) + 16 : 14;
                var need = boxOffset + RADIOBUTTON_SIZE + gap + textWidth + extraPadding;

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

    protected override void OnCreateControl()
    {
        base.OnCreateControl();
        if (DesignMode) return;

        FitTextToContent();
        _mouseState = 0;
        MouseEnter += (sender, args) =>
        {
            _mouseState = 1;
        };
        MouseLeave += (sender, args) =>
        {
            _mouseLocation = new Point(-1, -1);
            _mouseState = 0;
        };
        MouseDown += (sender, args) =>
        {
            _mouseState = 2;

            if (Ripple && args.Button == MouseButtons.Left && IsMouseInCheckArea())
            {
                rippleAnimationManager.SecondaryIncrement = 0;
                rippleAnimationManager.StartNewAnimation(AnimationDirection.InOutIn, new object[] { Checked });
            }
        };
        MouseUp += (sender, args) =>
        {
            _mouseState = 1;
            rippleAnimationManager.SecondaryIncrement = 0.08;
        };
        MouseMove += (sender, args) =>
        {
            _mouseLocation = args.Location;
            Cursor = IsMouseInCheckArea() ? Cursors.Hand : Cursors.Default;
        };
    }

    protected override void OnPaint(PaintEventArgs pevent)
    {
        var graphics = pevent.Graphics;
        graphics.SmoothingMode = SmoothingMode.HighQuality;
        graphics.TextRenderingHint = TextRenderingHint.AntiAliasGridFit;

        // Native radio-button erasing can leave a solid black rectangle behind
        // the text. Repaint the parent surface first so transparent radios blend
        // into cards and panels.
        if (BackColor == Color.Transparent || BackColor.A == 0)
            GroupBoxRenderer.DrawParentBackground(graphics, ClientRectangle, this);

        var isLightMode = !ColorScheme.BackColor.IsDark();
        var isStaleDarkBg = isLightMode && (BackColor == Color.Black || BackColor == Color.FromArgb(0, 0, 0) || BackColor == Color.FromArgb(16, 16, 16));

        if (BackColor != Color.Transparent && !BackColor.IsEmpty && BackColor.A > 0 && !isStaleDarkBg)
        {
            using var bgBrush = new SolidBrush(BackColor);
            graphics.FillRectangle(bgBrush, ClientRectangle);
        }

        var RADIOBUTTON_CENTER = boxOffset + RADIOBUTTON_SIZE_HALF;

        var animationProgress = animationManager.GetProgress();

        var disabledOffColor = Color.LightGray;

        var colorAlpha = Enabled ? (int)(animationProgress * 255.0) : disabledOffColor.A;
        var animationSize = (float)(animationProgress * 8f);
        var animationSizeHalf = animationSize / 2f;

        using var brush = new SolidBrush(Color.FromArgb(colorAlpha, Enabled ? ColorScheme.AccentColor : disabledOffColor));
        using var pen = new Pen(Enabled ? (animationProgress > 0.5 ? ColorScheme.AccentColor : ColorScheme.BorderColor) : disabledOffColor, 1.2f);

        // draw ripple animation
        if (Ripple && rippleAnimationManager.IsAnimating())
        {
            for (int i = 0; i < rippleAnimationManager.GetAnimationCount(); i++)
            {
                var animationValue = rippleAnimationManager.GetProgress(i);
                var animationSource = new Point(RADIOBUTTON_CENTER, RADIOBUTTON_CENTER);

                using var rippleBrush = new SolidBrush(Color.FromArgb((int)((animationValue * 40)), ((bool)rippleAnimationManager.GetData(i)[0]) ? Color.Black : brush.Color));
                var rippleHeight = (Height % 2 == 0) ? Height - 3 : Height - 2;
                var rippleSize = (rippleAnimationManager.GetDirection(i) == AnimationDirection.InOutIn) ? (int)(rippleHeight * (0.8d + (0.2d * animationValue))) : rippleHeight;

                using var path = DrawingExtensions.CreateRoundPath(animationSource.X - rippleSize / 2, animationSource.Y - rippleSize / 2, rippleSize, rippleSize, rippleSize / 2);
                graphics.FillPath(rippleBrush, path);
            }
        }

        // Circle background: Pure white in Light mode, dark input surface in Dark mode
        var circleBg = ColorScheme.BackColor.IsDark() ? ColorScheme.InputBackColor : Color.White;
        using (var bgBrush = new SolidBrush(circleBg))
        {
            graphics.FillEllipse(bgBrush, boxOffset, boxOffset, RADIOBUTTON_SIZE, RADIOBUTTON_SIZE);
            graphics.DrawEllipse(pen, boxOffset, boxOffset, RADIOBUTTON_SIZE, RADIOBUTTON_SIZE);
        }

        // Inner circle dot when checked
        if (animationProgress > 0)
        {
            graphics.FillEllipse(brush, RADIOBUTTON_CENTER - animationSizeHalf, RADIOBUTTON_CENTER - animationSizeHalf, animationSize, animationSize);
        }

        var textColor = Enabled ? ColorScheme.ResolveTextColor(ForeColor) : Color.Gray;

        // The text wraps naturally onto further lines (never ellipsized); the
        // control grows its height in FitTextToContent so it never clips.
        var textX = boxOffset + RADIOBUTTON_SIZE + Math.Max(10, TextLayout.Gap);
        this.DrawString(graphics, TextAlign, textColor,
            new RectangleF(textX, 0, Math.Max(0, ClientRectangle.Width - textX), ClientRectangle.Height),
            showEllipsis: false, useMnemonic: false, noWrap: true);
    }

    private bool IsMouseInCheckArea()
    {
        return radioButtonBounds.Contains(_mouseLocation);
    }

    private void OnSizeChanged(object sender, EventArgs eventArgs)
    {
        boxOffset = Height / 2 - (int)Math.Ceiling(RADIOBUTTON_SIZE / 2d);
        radioButtonBounds = new Rectangle(boxOffset, boxOffset, RADIOBUTTON_SIZE, RADIOBUTTON_SIZE);
    }
}