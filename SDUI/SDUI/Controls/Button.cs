using SDUI.Animation;
using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace SDUI.Controls;

public class Button : System.Windows.Forms.Button
{
    /// <summary>
    /// Button raised color
    /// </summary>
    public Color Color { get; set; } = Color.Transparent;

    /// <summary>
    /// Mouse state
    /// </summary>
    private int _mouseState = 0;

    private SizeF textSize;
    public override string Text
    {
        get { return base.Text; }
        set
        {
            base.Text = value;
            textSize = TextRenderer.MeasureText(value, Font);
            if (AutoSize)
                Size = GetPreferredSize();
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

    private int _radius = 6;
    public int Radius
    {
        get => _radius;
        set
        {
            if (_radius == value)
                return;

            _radius = value;
            Invalidate();
        }
    }

    private readonly Animation.AnimationEngine animationManager;
    private readonly Animation.AnimationEngine hoverAnimationManager;

    public Button()
    {
        SetStyle(ControlStyles.UserPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.AllPaintingInWmPaint | ControlStyles.SupportsTransparentBackColor, true);

        animationManager = new Animation.AnimationEngine(false)
        {
            Increment = 0.03,
            AnimationType = AnimationType.EaseOut
        };

        hoverAnimationManager = new Animation.AnimationEngine
        {
            Increment = 0.07,
            AnimationType = AnimationType.Linear,
        };

        hoverAnimationManager.OnAnimationFinished += (sender) =>
        {
        };
        hoverAnimationManager.OnAnimationProgress += sender => Invalidate();

        animationManager.OnAnimationProgress += sender => Invalidate();
        UpdateStyles();
    }

    protected override void OnMouseDown(MouseEventArgs e)
    {
        base.OnMouseDown(e);
        _mouseState = 2;
        animationManager.StartNewAnimation(AnimationDirection.In, e.Location);
        Invalidate();
    }

    protected override void OnMouseUp(MouseEventArgs e)
    {
        base.OnMouseUp(e);
        _mouseState = 1;
        Invalidate();
    }

    protected override void OnMouseEnter(EventArgs e)
    {
        base.OnMouseEnter(e);
        _mouseState = 1;
        hoverAnimationManager.StartNewAnimation(AnimationDirection.In);
        Invalidate();
    }

    protected override void OnMouseLeave(EventArgs e)
    {
        base.OnMouseLeave(e);

        _mouseState = 0;
        hoverAnimationManager.StartNewAnimation(AnimationDirection.Out);
        Invalidate();
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        var graphics = e.Graphics;
        graphics.SmoothingMode = SmoothingMode.AntiAlias;

        var isTransparentOrEmpty = Color.IsEmpty || Color == Color.Transparent;

        var isLightMode = !ColorScheme.BackColor.IsDark();
        var isStaleDarkBg = isLightMode && (BackColor == Color.Black || BackColor == Color.FromArgb(0, 0, 0) || BackColor == Color.FromArgb(16, 16, 16));

        if (BackColor != Color.Transparent && !BackColor.IsEmpty && BackColor != SystemColors.Control && BackColor != Control.DefaultBackColor && BackColor.A > 0 && !isStaleDarkBg)
        {
            using var bgBrush = new SolidBrush(BackColor);
            graphics.FillRectangle(bgBrush, ClientRectangle);
        }

        var rectf = ClientRectangle.ToRectangleF();

        if (ColorScheme.DrawDebugBorders)
        {
            var redPen = new Pen(Color.Red, 1);
            redPen.Alignment = PenAlignment.Outset;
            graphics.DrawRectangle(redPen, 0, 0, rectf.Width - 1, rectf.Height - 1);
        }

        var inflate = _shadowDepth / 4f;
        rectf.Inflate(-inflate, -inflate);

        var color = Color.Empty;
        if (!isTransparentOrEmpty && !(isLightMode && (Color == Color.Black || Color == Color.FromArgb(0, 0, 0))))
        {
            color = Enabled ? Color : Color.Alpha(200);
        }
        else if (BackColor != Color.Transparent && BackColor != Control.DefaultBackColor && BackColor != SystemColors.Control && !BackColor.IsEmpty && !isStaleDarkBg)
        {
            color = Enabled ? BackColor : BackColor.Alpha(200);
            isTransparentOrEmpty = false;
        }
        else if (ColorScheme.CustomButtonColor.HasValue)
        {
            color = Enabled ? ColorScheme.ButtonColor : ColorScheme.ButtonColor.Alpha(200);
            isTransparentOrEmpty = false;
        }
        else
        {
            // Crisp, adaptive neutral button background:
            // Dark Mode: deep charcoal surface #1E2636
            // Light Mode: clean light gray #F3F4F6 (never solid black!)
            color = ColorScheme.BackColor.IsDark()
                ? Color.FromArgb(30, 38, 54)
                : Color.FromArgb(243, 244, 246);
        }

        using var brush = new SolidBrush(color);
        using var outerPen = new Pen(ColorScheme.BorderColor);

        using (var path = rectf.Radius(_radius))
        {
            graphics.FillPath(brush, path);
            graphics.DrawPath(outerPen, path);

            var animationColor = Color.Transparent;
            if (!isTransparentOrEmpty)
            {
                var hColor = ColorScheme.CustomButtonHoverColor ?? color.Brightness(color.IsDark() ? 0.25f : -0.2f);
                animationColor = Color.FromArgb((int)(hoverAnimationManager.GetProgress() * 80), hColor.Determine());
            }
            else
            {
                var hColor = ColorScheme.BackColor.IsDark()
                    ? Color.FromArgb(45, 56, 78)
                    : Color.FromArgb(229, 231, 235);
                animationColor = Color.FromArgb((int)(hoverAnimationManager.GetProgress() * 120), hColor);
            }

            using var b = new SolidBrush(animationColor);
            graphics.FillPath(b, path);

            graphics.DrawShadow(rectf, _shadowDepth, _radius);

            //Ripple
            if (animationManager.IsAnimating())
            {
                var mode = graphics.SmoothingMode;
                graphics.SmoothingMode = SmoothingMode.AntiAlias;
                for (int i = 0; i < animationManager.GetAnimationCount(); i++)
                {
                    var animationValue = animationManager.GetProgress(i);
                    var animationSource = animationManager.GetSource(i);

                    using var rippleBrush = new SolidBrush(ColorScheme.BackColor.Alpha((int)(101 - (animationValue * 100))));
                    var rippleSize = (float)(animationValue * Width * 2.0);

                    var rippleRect = new RectangleF(animationSource.X - rippleSize / 2, animationSource.Y - rippleSize / 2, rippleSize, rippleSize);
                    path.AddEllipse(rippleRect);
                    graphics.FillPath(rippleBrush, path);
                }
                graphics.SmoothingMode = mode;
            }
        }

        var foreColor = isTransparentOrEmpty ? ColorScheme.ForeColor : ForeColor;
        if (!isTransparentOrEmpty && ColorScheme.CustomButtonForeColor.HasValue)
        {
            foreColor = ColorScheme.ButtonForeColor;
        }
        else if (isTransparentOrEmpty)
        {
            foreColor = ColorScheme.ForeColor;
        }
        else if (foreColor == SystemColors.ControlText || foreColor == Color.Empty)
        {
            foreColor = color.IsDark() ? Color.White : Color.FromArgb(17, 24, 39);
        }
        else
        {
            foreColor = ColorScheme.ResolveTextColor(foreColor);
        }

        if (!Enabled)
            foreColor = Color.Gray;

        var textRect = rectf.ToRectangle();
        if (Image != null)
        {
            //Image
            Rectangle imageRect = new Rectangle(8, 6, 24, 24);

            if (string.IsNullOrEmpty(Text))
                // Center Image
                imageRect.X += 2;

            if (Image != null)
                graphics.DrawImage(Image, imageRect);

            // First 8: left padding
            // 24: Image width
            // Second 4: space between Image and Text
            // Third 8: right padding
            textRect.Width -= 8 + 24 + 4 + 8;

            // First 8: left padding
            // 24: Image width
            // Second 4: space between Image and Text
            textRect.X += 8 + 24 + 4;
        }

        this.DrawString(graphics, TextAlign, foreColor, textRect, AutoEllipsis, UseMnemonic, noWrap: true);
    }

    private Size GetPreferredSize()
    {
        return GetPreferredSize(Size.Empty);
    }

    public override Size GetPreferredSize(Size proposedSize)
    {
        // Provides extra space for proper padding for content
        int extra = 16;

        if (Image != null)
            // 24 is for icon size
            // 4 is for the space between icon & text
            extra += 24 + 4;

        return new Size((int)Math.Ceiling(textSize.Width) + extra, 23);
    }
}