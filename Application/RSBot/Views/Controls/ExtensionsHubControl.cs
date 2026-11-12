using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using SDUI;
using Button = SDUI.Controls.Button;
using Panel = SDUI.Controls.Panel;

namespace RSBot.Views.Controls;

/// <summary>
///     Container page ("Extensions" / "الإضافات") that hosts secondary plugins and extra botbases
///     under a single sidebar entry while keeping each tool's UI and controls intact.
/// </summary>
[ToolboxItem(false)]
public class ExtensionsHubControl : UserControl
{
    private sealed class ToolEntry
    {
        public ToolEntry(string key, Func<string> titleProvider, Control view, Button tabButton, int order)
        {
            Key = key;
            TitleProvider = titleProvider;
            View = view;
            TabButton = tabButton;
            Order = order;
        }

        public string Key { get; }
        public Func<string> TitleProvider { get; }
        public Control View { get; }
        public Button TabButton { get; }
        public int Order { get; set; }
    }

    private readonly Panel _headerPanel;
    private readonly FlowLayoutPanel _tabStrip;
    private readonly System.Windows.Forms.Panel _divider;
    private readonly System.Windows.Forms.Panel _hostPanel;
    private readonly List<ToolEntry> _entries = new();
    private readonly Font _regularTabFont;
    private readonly Font _boldTabFont;
    private string _activeKey;

    public ExtensionsHubControl()
    {
        Font = new Font("Segoe UI", 9.25f, FontStyle.Regular);
        _regularTabFont = new Font(Font, FontStyle.Regular);
        _boldTabFont = new Font(Font, FontStyle.Bold);

        _headerPanel = new Panel
        {
            Dock = DockStyle.Top,
            Height = 46,
            Padding = new Padding(10, 8, 10, 6),
            Radius = 0,
            ShadowDepth = 0
        };

        _tabStrip = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.LeftToRight,
            WrapContents = true,
            AutoSize = false,
            BackColor = Color.Transparent,
            Padding = new Padding(0),
            Margin = new Padding(0)
        };
        _tabStrip.SizeChanged += (_, _) => UpdateHeaderHeight();
        _headerPanel.Controls.Add(_tabStrip);

        _divider = new System.Windows.Forms.Panel
        {
            Dock = DockStyle.Top,
            Height = 1
        };

        _hostPanel = new System.Windows.Forms.Panel
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(0)
        };

        Controls.Add(_hostPanel);
        Controls.Add(_divider);
        Controls.Add(_headerPanel);

        ApplyTheme();
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _regularTabFont.Dispose();
            _boldTabFont.Dispose();
        }
        base.Dispose(disposing);
    }

    public void AddTool(string key, Func<string> titleProvider, Control view, int order = int.MaxValue)
    {
        if (string.IsNullOrWhiteSpace(key) || view == null)
            return;

        if (_entries.Any(e => string.Equals(e.Key, key, StringComparison.OrdinalIgnoreCase)))
            return;

        var button = new Button
        {
            AutoSize = false,
            MinimumSize = new Size(96, 30),
            Size = new Size(120, 30),
            Margin = new Padding(0, 0, 8, 4),
            Padding = new Padding(12, 0, 12, 0),
            Radius = 6,
            ShadowDepth = 2f,
            Font = _regularTabFont,
            Text = titleProvider?.Invoke() ?? key,
            Tag = key
        };
        button.Click += (_, _) => SelectTool(key);

        view.Dock = DockStyle.Fill;
        view.Visible = false;

        var entry = new ToolEntry(key, titleProvider, view, button, order);
        _entries.Add(entry);
        _entries.Sort((a, b) => a.Order.CompareTo(b.Order));

        RebuildStrip();
        _hostPanel.Controls.Add(view);

        if (_activeKey == null)
            SelectTool(key);
        else
            UpdateTabVisuals();
    }

    public bool ContainsTool(string key)
    {
        return !string.IsNullOrWhiteSpace(key) &&
               _entries.Any(e => string.Equals(e.Key, key, StringComparison.OrdinalIgnoreCase));
    }

    public void SelectTool(string key)
    {
        if (string.IsNullOrWhiteSpace(key))
            return;

        var target = _entries.FirstOrDefault(e => string.Equals(e.Key, key, StringComparison.OrdinalIgnoreCase));
        if (target == null)
            return;

        _activeKey = target.Key;

        foreach (var entry in _entries)
        {
            var isTarget = ReferenceEquals(entry, target);
            entry.View.Visible = isTarget;
            if (isTarget)
                entry.View.BringToFront();
        }

        UpdateTabVisuals();
    }

    public void RefreshLocalization()
    {
        foreach (var entry in _entries)
        {
            var title = entry.TitleProvider?.Invoke();
            if (!string.IsNullOrWhiteSpace(title))
                entry.TabButton.Text = title;
        }

        UpdateTabButtonBounds();
    }

    public void ApplyTheme()
    {
        var isDark = ColorScheme.BackColor.IsDark();
        BackColor = ColorScheme.BackColor;
        _headerPanel.BackColor = ColorScheme.ChromeColor;
        _hostPanel.BackColor = ColorScheme.BackColor;
        _divider.BackColor = ColorScheme.BorderColor;

        UpdateTabVisuals(isDark);
    }

    public void SetIngameState(bool ingame)
    {
        foreach (var entry in _entries)
        {
            if (ingame)
            {
                entry.View.Enabled = true;
                entry.View.Controls["overlay"]?.Hide();
            }
            else if (entry.View.Controls.ContainsKey("overlay"))
            {
                entry.View.Enabled = false;
                entry.View.Controls["overlay"].Show();
            }
        }
    }

    private void RebuildStrip()
    {
        _tabStrip.SuspendLayout();
        try
        {
            _tabStrip.Controls.Clear();
            foreach (var entry in _entries)
                _tabStrip.Controls.Add(entry.TabButton);
        }
        finally
        {
            _tabStrip.ResumeLayout(true);
        }

        UpdateTabButtonBounds();
    }

    private void UpdateTabButtonBounds()
    {
        _tabStrip.SuspendLayout();
        try
        {
            foreach (var entry in _entries)
            {
                var btn = entry.TabButton;
                var width = MeasureTabButtonWidth(btn.Text, btn.Padding);
                btn.MinimumSize = new Size(width, 30);
                btn.Size = new Size(width, 30);
            }
        }
        finally
        {
            _tabStrip.ResumeLayout(true);
        }

        UpdateHeaderHeight();
    }

    private int MeasureTabButtonWidth(string text, Padding padding)
    {
        var label = text ?? string.Empty;
        var gdiWidth = TextRenderer.MeasureText(label, _boldTabFont, new Size(int.MaxValue, 30), TextFormatFlags.NoPadding).Width;

        float gdipWidth = gdiWidth;
        try
        {
            using var g = CreateGraphics();
            using var sf = new StringFormat(StringFormat.GenericTypographic)
            {
                FormatFlags = StringFormatFlags.NoWrap
            };
            gdipWidth = g.MeasureString(label, _boldTabFont, int.MaxValue, sf).Width;
        }
        catch { }

        return Math.Max(96, (int)Math.Ceiling(Math.Max(gdiWidth, gdipWidth)) + padding.Horizontal + 12);
    }

    private void UpdateHeaderHeight()
    {
        if (_entries.Count == 0 || _headerPanel == null || _tabStrip == null)
            return;

        _tabStrip.PerformLayout();

        var maxBottom = 34;
        foreach (var entry in _entries)
        {
            var bottom = entry.TabButton.Bottom + entry.TabButton.Margin.Bottom;
            if (bottom > maxBottom)
                maxBottom = bottom;
        }

        var targetHeight = Math.Max(46, maxBottom + _headerPanel.Padding.Vertical);
        if (_headerPanel.Height != targetHeight)
            _headerPanel.Height = targetHeight;
    }

    private void UpdateTabVisuals(bool? isDarkOverride = null)
    {
        var isDark = isDarkOverride ?? ColorScheme.BackColor.IsDark();
        var activeColor = ColorScheme.AccentColor;
        var inactiveBg = Color.Transparent;
        var inactiveFg = isDark ? ColorScheme.ForeColor : Color.FromArgb(40, 44, 52);

        foreach (var entry in _entries)
        {
            var isSelected = string.Equals(entry.Key, _activeKey, StringComparison.OrdinalIgnoreCase);
            entry.TabButton.Color = isSelected ? activeColor : inactiveBg;
            entry.TabButton.ForeColor = isSelected ? Color.White : inactiveFg;
            entry.TabButton.Font = isSelected ? _boldTabFont : _regularTabFont;
            entry.TabButton.Invalidate();
        }

        UpdateTabButtonBounds();
    }
}
