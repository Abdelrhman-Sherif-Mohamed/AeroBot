using System;
using System.Drawing;
using System.Windows.Forms;
using RSBot.Alchemy.Stats;
using SDUI;

using Button = SDUI.Controls.Button;
using Label = SDUI.Controls.Label;

namespace RSBot.Alchemy.Views;

internal sealed class StatisticsView : UserControl
{
    private readonly AlchemySessionStats _stats;
    private readonly Timer _liveTimer;

    private Label _lblTimeCaption;
    private Label _lblTimeValue;
    private Label _lblHighPlusCaption;
    private Label _lblHighPlusValue;
    private Label _lblFailsCaption;
    private Label _lblFailsValue;
    private Label _lblElixirCaption;
    private Label _lblElixirValue;
    private Label _lblLuckyCaption;
    private Label _lblLuckyValue;
    private Label _lblSteadyCaption;
    private Label _lblSteadyValue;
    private Label _lblImmortalCaption;
    private Label _lblImmortalValue;
    private Label _lblAstralCaption;
    private Label _lblAstralValue;
    private Button _btnReset;

    public StatisticsView(AlchemySessionStats stats)
    {
        _stats = stats;
        DoubleBuffered = true;
        BackColor = Color.Transparent;

        BuildUi();
        _stats.StatsUpdated += () => SafeInvoke(RefreshValues);

        _liveTimer = new Timer { Interval = 1000 };
        _liveTimer.Tick += (_, _) =>
        {
            if (Visible && Bootstrap.IsActive)
                RefreshValues();
        };
        _liveTimer.Start();

        RefreshValues();
    }

    private void BuildUi()
    {
        var table = new TableLayoutPanel
        {
            Dock = DockStyle.Top,
            Height = 320,
            ColumnCount = 2,
            RowCount = 8,
            Padding = new Padding(44, 24, 44, 8),
            BackColor = Color.Transparent
        };
        table.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 210));
        table.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

        (_lblTimeCaption, _lblTimeValue) = AddRow(table, 0);
        (_lblHighPlusCaption, _lblHighPlusValue) = AddRow(table, 1);
        (_lblFailsCaption, _lblFailsValue) = AddRow(table, 2);
        (_lblElixirCaption, _lblElixirValue) = AddRow(table, 3);
        (_lblLuckyCaption, _lblLuckyValue) = AddRow(table, 4);
        (_lblSteadyCaption, _lblSteadyValue) = AddRow(table, 5);
        (_lblImmortalCaption, _lblImmortalValue) = AddRow(table, 6);
        (_lblAstralCaption, _lblAstralValue) = AddRow(table, 7);

        _btnReset = AlchemyUi.CreateButton(string.Empty, ButtonTone.Neutral, 30);
        _btnReset.Width = 130;
        _btnReset.Click += (_, _) => _stats.Reset();

        var footer = new FlowLayoutPanel
        {
            Dock = DockStyle.Top,
            Height = 44,
            Padding = new Padding(44, 6, 44, 4),
            BackColor = Color.Transparent
        };
        footer.Controls.Add(_btnReset);

        Controls.Add(footer);
        Controls.Add(table);

        ApplyLocalization();
    }

    private static (Label caption, Label value) AddRow(TableLayoutPanel table, int row)
    {
        table.RowStyles.Add(new RowStyle(SizeType.Absolute, 34));

        var caption = AlchemyUi.CreateLabel(string.Empty, ContentAlignment.MiddleLeft, bold: false);
        caption.Dock = DockStyle.Fill;

        var value = AlchemyUi.CreateLabel("--", ContentAlignment.MiddleLeft, bold: true);
        value.Dock = DockStyle.Fill;

        table.Controls.Add(caption, 0, row);
        table.Controls.Add(value, 1, row);

        return (caption, value);
    }

    public void RefreshValues()
    {
        if (IsDisposed || Disposing)
            return;

        var s = _stats.GetSnapshot();
        if (!s.HasData)
        {
            _lblTimeValue.Text = "--";
            _lblHighPlusValue.Text = "--";
            _lblFailsValue.Text = "--";
            _lblElixirValue.Text = "--";
            _lblLuckyValue.Text = "--";
            _lblSteadyValue.Text = "--";
            _lblImmortalValue.Text = "--";
            _lblAstralValue.Text = "--";
            return;
        }

        _lblTimeValue.Text = s.Elapsed.ToString(@"hh\:mm\:ss");
        _lblHighPlusValue.Text = s.HighestPlus.HasValue ? $"+{s.HighestPlus.Value}" : "--";
        _lblFailsValue.Text = s.FailCount.ToString();
        _lblElixirValue.Text = s.ElixirCount.ToString();
        _lblLuckyValue.Text = s.LuckyStoneCount.ToString();
        _lblSteadyValue.Text = s.SteadyStoneCount.ToString();
        _lblImmortalValue.Text = s.ImmortalStoneCount.ToString();
        _lblAstralValue.Text = s.AstralStoneCount.ToString();
    }

    public void ApplyLocalization()
    {
        _lblTimeCaption.Text = AlchemyUi.T("Alchemy.Stats.Time", "وقت الكمي", "Alchemy Time");
        _lblHighPlusCaption.Text = AlchemyUi.T("Alchemy.Stats.HighestPlus", "أعلى Plus", "Highest Plus");
        _lblFailsCaption.Text = AlchemyUi.T("Alchemy.Stats.Fails", "مرات الفشل", "Failures");
        _lblElixirCaption.Text = AlchemyUi.T("Alchemy.Stats.Elixirs", "الـ Elixir", "Elixirs");
        _lblLuckyCaption.Text = "Lucky Stone";
        _lblSteadyCaption.Text = "Steady Stone";
        _lblImmortalCaption.Text = "Immortal Stone";
        _lblAstralCaption.Text = "Astral Stone";
        _btnReset.Text = AlchemyUi.T("Alchemy.Stats.Reset", "تصفير الإحصائيات", "Reset Stats");
    }

    public void ApplyTheme()
    {
        AlchemyUi.ApplyThemeRecursive(this);
    }

    private void SafeInvoke(Action action)
    {
        if (IsDisposed || Disposing || !IsHandleCreated)
            return;

        try
        {
            if (InvokeRequired)
                BeginInvoke(action);
            else
                action();
        }
        catch (InvalidOperationException)
        {
        }
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
            _liveTimer?.Dispose();

        base.Dispose(disposing);
    }
}
