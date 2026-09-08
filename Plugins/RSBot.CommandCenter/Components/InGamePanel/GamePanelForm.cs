using System;
using System.Drawing;
using System.Windows.Forms;
using RSBot.Core;
using RSBot.Core.Components;

namespace RSBot.CommandCenter.Components.InGamePanel;

/// <summary>
///     Slim on-demand control panel inside the game (opened by !bot).
///     Never steals focus from the game client, never follows it around:
///     it opens centered once and stays where the user drags it.
/// </summary>
internal sealed class GamePanelForm : Form
{
    private readonly Panel _titleBar;
    private readonly Label _lblTitle;
    private readonly Button _btnClose;
    private readonly Button _btnStartStop;
    private readonly Label _lblStatus;
    private readonly Button _btnHere;
    private readonly Button _btnTown;
    private readonly Timer _refreshTimer;

    private bool _dragging;
    private Point _dragOffset;

    public GamePanelForm()
    {
        TopMost = true;
        ShowInTaskbar = false;
        FormBorderStyle = FormBorderStyle.None;
        StartPosition = FormStartPosition.Manual;
        Size = new Size(300, 218);
        BackColor = Color.FromArgb(22, 22, 28);
        ForeColor = Color.White;
        Text = "Sonic Panel";

        // Center over the game window once (no follow timer = zero interference)
        try
        {
            var handle = ClientManager.ClientWindowHandle;
            Rectangle area;
            if (handle != IntPtr.Zero && NativeGetWindowRect(handle, out var r) && r.Width > 200)
                area = r;
            else
                area = Screen.PrimaryScreen?.WorkingArea ?? new Rectangle(0, 0, 1024, 768);
            Location = new Point(
                area.X + Math.Max(0, (area.Width - Width) / 2),
                area.Y + Math.Max(0, (area.Height - Height) / 2));
        }
        catch
        {
            StartPosition = FormStartPosition.CenterScreen;
        }

        _titleBar = new Panel
        {
            Dock = DockStyle.Top,
            Height = 30,
            BackColor = Color.FromArgb(38, 38, 48),
            Cursor = Cursors.SizeAll
        };
        _lblTitle = new Label
        {
            Text = "* Sonic",
            ForeColor = Color.Gold,
            Font = new Font("Segoe UI", 11f, FontStyle.Bold),
            Location = new Point(8, 5),
            AutoSize = true
        };
        _btnClose = FlatButton("X", new Point(266, 3), new Size(30, 24));
        _titleBar.Controls.Add(_lblTitle);
        _titleBar.Controls.Add(_btnClose);

        var body = new Panel { Dock = DockStyle.Fill, BackColor = Color.FromArgb(22, 22, 28) };

        _btnStartStop = new Button
        {
            Text = "> Start",
            Location = new Point(10, 8),
            Size = new Size(280, 36),
            FlatStyle = FlatStyle.Flat,
            BackColor = Color.FromArgb(56, 155, 90),
            ForeColor = Color.White,
            Font = new Font("Segoe UI", 11f, FontStyle.Bold),
            TabStop = false
        };
        _btnStartStop.FlatAppearance.BorderSize = 0;

        _lblStatus = new Label
        {
            Location = new Point(10, 48),
            Size = new Size(280, 54),
            ForeColor = Color.Gainsboro,
            Font = new Font("Segoe UI", 9f),
            Text = "Waiting for game..."
        };

        _btnHere = FlatButton("Here", new Point(10, 106), new Size(136, 30));
        _btnTown = FlatButton("Town", new Point(154, 106), new Size(136, 30));

        var lblHint = new Label
        {
            Text = "Chat: !here !stop !area !town   |   !bot closes",
            Location = new Point(10, 140),
            Size = new Size(280, 16),
            ForeColor = Color.Gray,
            Font = new Font("Segoe UI", 8f)
        };
        var lblCopy = new Label
        {
            Text = "(c) abdelrhman sherif | WhatsApp: +201150238481",
            Location = new Point(10, 158),
            Size = new Size(280, 16),
            ForeColor = Color.Gray,
            Font = new Font("Segoe UI", 7.5f)
        };

        body.Controls.Add(_btnStartStop);
        body.Controls.Add(_lblStatus);
        body.Controls.Add(_btnHere);
        body.Controls.Add(_btnTown);
        body.Controls.Add(lblHint);
        body.Controls.Add(lblCopy);

        Controls.Add(body);
        Controls.Add(_titleBar);

        _btnStartStop.Click += (_, _) => Safe(ToggleBot);
        _btnHere.Click += (_, _) => Safe(() => CommandManager.Execute("here"));
        _btnTown.Click += (_, _) => Safe(() => CommandManager.Execute("town"));
        _btnClose.Click += (_, _) => Hide();

        _titleBar.MouseDown += (_, e) =>
        {
            if (e.Button != MouseButtons.Left) return;
            _dragging = true;
            _dragOffset = new Point(e.X, e.Y);
        };
        _titleBar.MouseMove += (_, e) =>
        {
            if (!_dragging) return;
            var screen = PointToScreen(e.Location);
            Location = new Point(screen.X - _dragOffset.X, screen.Y - _dragOffset.Y);
        };
        _titleBar.MouseUp += (_, _) => _dragging = false;

        _refreshTimer = new Timer { Interval = 1000 };
        _refreshTimer.Tick += (_, _) => RefreshStatus();
        Load += (_, _) => { _refreshTimer.Start(); RefreshStatus(); };
        FormClosed += (_, _) => _refreshTimer.Stop();
    }

    private static Button FlatButton(string text, Point location, Size size)
    {
        var btn = new Button
        {
            Text = text,
            Location = location,
            Size = size,
            FlatStyle = FlatStyle.Flat,
            BackColor = Color.FromArgb(55, 55, 68),
            ForeColor = Color.White,
            Font = new Font("Segoe UI", 9f),
            TabStop = false
        };
        btn.FlatAppearance.BorderSize = 0;
        return btn;
    }

    private static void ToggleBot()
    {
        try
        {
            if (Kernel.Bot?.Running == true)
                CommandManager.Execute("stop");
            else
                CommandManager.Execute("start");
        }
        catch (Exception ex)
        {
            Log.Error(ex);
        }
    }

    private static void Safe(Action action)
    {
        try
        {
            action();
        }
        catch (Exception ex)
        {
            Log.Error(ex);
        }
    }

    private void RefreshStatus()
    {
        try
        {
            var running = Kernel.Bot?.Running == true;
            _btnStartStop.Text = running ? "Stop" : "> Start";
            _btnStartStop.BackColor = running
                ? Color.FromArgb(190, 70, 70)
                : Color.FromArgb(56, 155, 90);
            _lblTitle.ForeColor = running ? Color.FromArgb(0, 230, 140) : Color.Gold;

            if (Game.Player != null && Game.Ready)
            {
                var hpPct = Game.Player.MaximumHealth > 0
                    ? Game.Player.Health * 100 / Game.Player.MaximumHealth : 0;
                var mpPct = Game.Player.MaximumMana > 0
                    ? Game.Player.Mana * 100 / Game.Player.MaximumMana : 0;
                _lblStatus.Text =
                    $"{(running ? "RUNNING" : "STOPPED")} Lv.{Game.Player.Level} {Game.Player.Name}\nHP {hpPct}%  MP {mpPct}%\nX:{Game.Player.Position.X:0} Y:{Game.Player.Position.Y:0}";
            }
            else
            {
                _lblStatus.Text = "Waiting for game...";
            }
        }
        catch
        {
        }
    }

    // ---------- No-focus behavior: clicks never steal focus from the game ----------

    private const int WS_EX_NOACTIVATE = 0x08000000;
    private const int WS_EX_TOOLWINDOW = 0x00000080;
    private const int WM_MOUSEACTIVATE = 0x21;
    private const int MA_NOACTIVATE = 3;

    protected override bool ShowWithoutActivation => true;

    protected override CreateParams CreateParams
    {
        get
        {
            var cp = base.CreateParams;
            cp.ExStyle |= WS_EX_NOACTIVATE | WS_EX_TOOLWINDOW;
            return cp;
        }
    }

    protected override void WndProc(ref Message m)
    {
        if (m.Msg == WM_MOUSEACTIVATE)
        {
            m.Result = (IntPtr)MA_NOACTIVATE;
            return;
        }

        base.WndProc(ref m);
    }

    [System.Runtime.InteropServices.DllImport("user32.dll")]
    private static extern bool GetWindowRect(IntPtr hWnd, out NativeRect lpRect);

    private static bool NativeGetWindowRect(IntPtr handle, out Rectangle rect)
    {
        rect = Rectangle.Empty;
        try
        {
            if (!GetWindowRect(handle, out var r))
                return false;
            rect = Rectangle.FromLTRB(r.Left, r.Top, r.Right, r.Bottom);
            return true;
        }
        catch
        {
            return false;
        }
    }

    [System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential)]
    private struct NativeRect
    {
        public int Left;
        public int Top;
        public int Right;
        public int Bottom;
    }
}
