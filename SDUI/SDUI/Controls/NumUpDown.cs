using SDUI;
using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace SDUI.Controls
{
    public class NumUpDown : Control
    {
        private const int LONG_PRESS_TIMER_INTERVAL = 250;

        public event EventHandler ValueChanged;

        private decimal _value;
        private decimal _min;
        private decimal _max;
        private Point _mouseLocation;
        private bool _isUsingKeyboard;
        private Timer _longPressTimer = new();

        private RectangleF _upButtonRect;
        private RectangleF _downButtonRect;

        private const int SIZE = 20;
        private float _dpi => DeviceDpi / 96f;

        private Color _boxColor = Color.Empty;

        public decimal Value
        {
            get
            {
                return _value;
            }
            set
            {
                if (value <= _max && value >= _min)
                {
                    _value = value;
                    ValueChanged?.Invoke(this, EventArgs.Empty);
                }
                Invalidate();
            }
        }

        public decimal Minimum
        {
            get
            {
                return _min;
            }
            set
            {
                if (value < _max)
                {
                    _min = value;
                }

                if (_value < _min)
                    Value = _min;

                Invalidate();
            }
        }

        public decimal Maximum
        {
            get
            {
                return _max;
            }
            set
            {
                if (value > _min)
                    _max = value;

                if (_value > _max)
                    Value = _max;

                Invalidate();
            }
        }

        public override string Text
        {
            get => Value.ToString();
            set
            {
                if (decimal.TryParse(value, out var d))
                    Value = d;
                base.Text = value;
                Invalidate();
            }
        }

        public Color BoxColor
        {
            get => _boxColor;
            set
            {
                _boxColor = value;
                Invalidate();
            }
        }

        public override Color BackColor
        {
            get => _boxColor.IsEmpty ? base.BackColor : _boxColor;
            set
            {
                _boxColor = value;
                base.BackColor = value;
                Invalidate();
            }
        }

        public NumUpDown()
        {
            SetStyle(ControlStyles.SupportsTransparentBackColor |
                ControlStyles.AllPaintingInWmPaint |
                      ControlStyles.OptimizedDoubleBuffer |
                      ControlStyles.DoubleBuffer |
                      ControlStyles.ResizeRedraw |
                      ControlStyles.UserPaint, true);

            UpdateStyles();
            this.DoubleBuffered = true;

            _min = 0;
            _max = 100;
            _value = 0;
            RightToLeft = RightToLeft.No;

            _longPressTimer.Tick += LongPressTimer_Tick;
            _longPressTimer.Interval = LONG_PRESS_TIMER_INTERVAL;

            _upButtonRect = new(Width - SIZE * _dpi, 0, SIZE * _dpi, Height);
            _downButtonRect = new(Width - SIZE * _dpi * 2, 0, SIZE * _dpi, Height);
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            _mouseLocation = e.Location;
        }

        private void ClickButton()
        {
            if (_mouseLocation.InRect(_upButtonRect))
            {
                if (_value + 1 <= _max)
                    Value++;
            }
            else
            {
                if (_mouseLocation.InRect(_downButtonRect))
                {
                    if (_value - 1 >= _min)
                        Value--;
                }
                _isUsingKeyboard = !_isUsingKeyboard;
            }
            Focus();
            Invalidate();
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseClick(e);
            ClickButton();
            _longPressTimer.Start();
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);
            _longPressTimer.Stop();
            _longPressTimer.Interval = LONG_PRESS_TIMER_INTERVAL;
        }

        private void LongPressTimer_Tick(object sender, EventArgs e)
        {
            ClickButton();

            if (_longPressTimer.Interval == LONG_PRESS_TIMER_INTERVAL)
                _longPressTimer.Interval = 50;
        }

        protected override void OnKeyPress(KeyPressEventArgs e)
        {
            base.OnKeyPress(e);
            try
            {
                if (_isUsingKeyboard == true && _value < _max)
                    Value = long.Parse(_value.ToString() + e.KeyChar.ToString());
            }
            catch (Exception)
            {
            }
        }

        protected override void OnKeyUp(KeyEventArgs e)
        {
            base.OnKeyUp(e);
            if (e.KeyCode == Keys.Back)
            {
                var tempVal = _value.ToString();
                tempVal = tempVal.Remove(Convert.ToInt32(tempVal.Length - 1));
                if (tempVal.Length == 0)
                    tempVal = "0";

                Value = Convert.ToInt32(tempVal);
            }
            Invalidate();
        }

        protected override void OnMouseWheel(MouseEventArgs e)
        {
            base.OnMouseWheel(e);
            if (e.Delta > 0)
            {
                if (_value + 1 <= _max)
                {
                    Value++;
                }
                Invalidate();
            }
            else
            {
                if (_value - 1 >= _min)
                {
                    Value--;
                }
                Invalidate();
            }
        }

        protected override void OnSizeChanged(EventArgs e)
        {
            base.OnSizeChanged(e);

            _upButtonRect = new(Width - SIZE * _dpi, 0, SIZE * _dpi, Height);
            _downButtonRect = new(Width - SIZE * _dpi * 2, 0, SIZE * _dpi, Height);
        }

        protected override void OnDpiChangedAfterParent(EventArgs e)
        {
            base.OnDpiChangedAfterParent(e);

            _upButtonRect = new(Width - SIZE * _dpi, 0, SIZE * _dpi, Height);
            _downButtonRect = new(Width - SIZE * _dpi * 2, 0, SIZE * _dpi, Height);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            var graphics = e.Graphics;
            ButtonRenderer.DrawParentBackground(e.Graphics, Bounds, this);

            using var borderPen = new Pen(ColorScheme.BorderColor);
            
            var boxBrush = (_boxColor.A != 0 && _boxColor != Color.Empty && _boxColor != Color.Transparent)
                ? new SolidBrush(_boxColor)
                : ColorScheme.BackColor.Alpha(90).Brush();

            graphics.SmoothingMode = SmoothingMode.AntiAlias;
            using var round = ClientRectangle.Radius(8);

            graphics.FillPath(boxBrush, round);
            graphics.DrawPath(borderPen, round);

            if (_boxColor.A != 0 && _boxColor != Color.Empty && _boxColor != Color.Transparent)
                boxBrush.Dispose();

            var textColor = ColorScheme.ResolveTextColor(ForeColor);

            // Draw crisp geometric arrows
            DrawArrow(graphics, _upButtonRect, true, textColor);
            DrawArrow(graphics, _downButtonRect, false, textColor);

            graphics.DrawLine(borderPen, _upButtonRect.X, 0, _upButtonRect.X, _upButtonRect.Height);
            graphics.DrawLine(borderPen, _downButtonRect.X, 0, _downButtonRect.X, _downButtonRect.Height);

            // Numeric value
            var buttonsWidth = (int)(SIZE * _dpi * 2);
            var valueRect = new Rectangle(Padding.Left + 6, 0,
                Math.Max(1, Width - buttonsWidth - Padding.Left - Padding.Right - 10),
                Height - 1);

            TextRenderer.DrawText(graphics, Value.ToString(), Font, valueRect, textColor,
                TextFormatFlags.NoPrefix | TextFormatFlags.VerticalCenter | TextFormatFlags.Left | TextFormatFlags.EndEllipsis);
        }

        private static void DrawArrow(Graphics g, RectangleF rect, bool up, Color color)
        {
            using var brush = new SolidBrush(color);
            float cx = rect.X + rect.Width / 2f;
            float cy = rect.Y + rect.Height / 2f;
            float size = 4f;

            PointF[] points;
            if (up)
            {
                points = new PointF[]
                {
                    new PointF(cx, cy - size),
                    new PointF(cx - size, cy + size),
                    new PointF(cx + size, cy + size)
                };
            }
            else
            {
                points = new PointF[]
                {
                    new PointF(cx - size, cy - size),
                    new PointF(cx + size, cy - size),
                    new PointF(cx, cy + size)
                };
            }

            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.FillPolygon(brush, points);
        }
    }
}
