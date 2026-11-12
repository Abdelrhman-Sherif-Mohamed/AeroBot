using System;
using System.Drawing;
using System.Windows.Forms;
using RSBot.Core;
using RSBot.Core.Client.ReferenceObjects;
using RSBot.Core.Event;
using RSBot.Core.Objects;
using SDUI;
using SDUI.Controls;

using TabControl = SDUI.Controls.TabControl;

namespace RSBot.Alchemy.Views;

public class Main : DoubleBufferedControl
{
    private TabControl _tabMain;
    private TabPage _tabMultiItem;
    private TabPage _tabStats;
    private TabPage _tabManufacture;
    private TabPage _tabDismantle;

    private MultiItemAlchemyView _multiItemView;
    private StatisticsView _statisticsView;
    private ManufactureView _manufactureView;
    private DismantleView _dismantleView;

    public bool IsRefreshing => _multiItemView?.IsRefreshing ?? false;

    public InventoryItem SelectedItem
    {
        get => _multiItemView?.SelectedItem;
        set
        {
            if (_multiItemView != null)
                _multiItemView.SelectedItem = value;
        }
    }

    public Main()
    {
        CheckForIllegalCrossThreadCalls = false;
        DoubleBuffered = true;

        BuildUi();
        SubscribeEvents();
        ApplyLocalization();
        ApplyTheme();
        Disposed += (_, _) =>
        {
            if (_runnerSubscribed && Bootstrap.Runner != null)
                Bootstrap.Runner.StateChanged -= OnBotRunningStateChanged;
        };
    }

    private bool _runnerSubscribed;

    protected override void OnHandleCreated(EventArgs e)
    {
        base.OnHandleCreated(e);
        EnsureRunnerSubscription();
        ApplyTheme();
        OnBotRunningStateChanged();
    }

    private void BuildUi()
    {
        _tabMain = AlchemyUi.CreateTabControl(128, 30);

        _tabMultiItem = new TabPage("كمي كذا إيتم") { Name = "tabMultiItem", Padding = new Padding(0) };
        _tabStats = new TabPage("الإحصائيات") { Name = "tabStats", Padding = new Padding(0) };
        _tabManufacture = new TabPage("تصنيع") { Name = "tabManufacture", Padding = new Padding(0) };
        _tabDismantle = new TabPage("تكسير") { Name = "tabDismantle", Padding = new Padding(0) };

        _multiItemView = new MultiItemAlchemyView { Dock = DockStyle.Fill };
        _statisticsView = new StatisticsView(Globals.Botbase.SessionStats) { Dock = DockStyle.Fill };
        _manufactureView = new ManufactureView { Dock = DockStyle.Fill };
        _dismantleView = new DismantleView { Dock = DockStyle.Fill };

        _tabMultiItem.Controls.Add(_multiItemView);
        _tabStats.Controls.Add(_statisticsView);
        _tabManufacture.Controls.Add(_manufactureView);
        _tabDismantle.Controls.Add(_dismantleView);

        _tabMain.TabPages.AddRange(new[]
        {
            _tabMultiItem,
            _tabStats,
            _tabManufacture,
            _tabDismantle
        });

        _tabMain.SelectedIndexChanged += (_, _) =>
        {
            if (_tabMain.SelectedTab == _tabMultiItem)
                _multiItemView.ReloadItemList();
            else if (_tabMain.SelectedTab == _tabStats)
                _statisticsView.RefreshValues();
            else if (_tabMain.SelectedTab == _tabManufacture)
                _manufactureView.RefreshAll();
            else if (_tabMain.SelectedTab == _tabDismantle)
                _dismantleView.RefreshAll();
        };

        Controls.Add(_tabMain);
    }

    private void SubscribeEvents()
    {
        EventManager.SubscribeEvent("OnEnterGame", new System.Action(OnInventoryRefreshNeeded));
        EventManager.SubscribeEvent("OnLoadCharacter", new System.Action(OnInventoryRefreshNeeded));
        EventManager.SubscribeEvent("OnInventoryUpdate", new System.Action(OnInventoryRefreshNeeded));
        EventManager.SubscribeEvent("OnAlchemy", new System.Action<AlchemyType>(_ => OnInventoryRefreshNeeded()));
        EventManager.SubscribeEvent("OnChangeTheme", new System.Action<bool>(_ => RunOnUi(ApplyTheme)));
        EnsureRunnerSubscription();
    }

    private void EnsureRunnerSubscription()
    {
        if (_runnerSubscribed)
            return;

        var runner = Bootstrap.EnsureRunner();
        if (runner == null)
            return;

        runner.StateChanged += OnBotRunningStateChanged;
        _runnerSubscribed = true;
    }

    private void OnInventoryRefreshNeeded()
    {
        RunOnUi(() =>
        {
            _multiItemView.ReloadItemList();
            _manufactureView.RefreshAll();
            _dismantleView.RefreshAll();
        });
    }

    private void OnBotRunningStateChanged()
    {
        RunOnUi(() =>
        {
            _multiItemView.RefreshQueueGrid();
            _statisticsView.RefreshValues();
            _manufactureView.RefreshStatus();
            _dismantleView.RefreshStatus();
        });
    }

    public void AddLog(string itemName, string message)
    {
        if (string.IsNullOrWhiteSpace(message))
            return;

        if (string.IsNullOrWhiteSpace(itemName))
            Log.Notify($"[Alchemy] {message}");
        else
            Log.Notify($"[Alchemy] [{itemName}] {message}");
    }

    public void ApplyLocalization()
    {
        var ar = AlchemyUi.IsArabic;

        _tabMultiItem.Text = AlchemyUi.T("tabMultiItem", "كمي كذا إيتم", "Multi Alchemy");
        _tabStats.Text = AlchemyUi.T("tabStats", "الإحصائيات", "Statistics");
        _tabManufacture.Text = AlchemyUi.T("tabManufacture", "تصنيع", "Manufacture");
        _tabDismantle.Text = AlchemyUi.T("tabDismantle", "تكسير", "Dismantle");

        _multiItemView?.ApplyLocalization();
        _statisticsView?.ApplyLocalization();
        _manufactureView?.ApplyLocalization();
        _dismantleView?.ApplyLocalization();

        if (!ar)
        {
            _tabMain.ItemSize = new Size(124, 30);
        }
        else
        {
            _tabMain.ItemSize = new Size(128, 30);
        }
    }

    public void ApplyTheme()
    {
        BackColor = ColorScheme.BackColor;
        ForeColor = ColorScheme.ForeColor;
        _tabMain.BackColor = ColorScheme.BackColor;

        _multiItemView?.ApplyTheme();
        _statisticsView?.ApplyTheme();
        _manufactureView?.ApplyTheme();
        _dismantleView?.ApplyTheme();

        Invalidate(true);
    }

    private void RunOnUi(System.Action action)
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
}
