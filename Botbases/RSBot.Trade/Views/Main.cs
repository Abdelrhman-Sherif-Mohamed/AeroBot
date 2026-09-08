using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using RSBot.Core;
using RSBot.Core.Components;
using RSBot.Core.Event;
using RSBot.Core.Objects;
using RSBot.Trade.Components;
using SDUI.Controls;
using SDUI.Helpers;

namespace RSBot.Trade.Views;

public partial class Main : DoubleBufferedControl
{
    private bool _loadingConfig;

    public Main()
    {
        InitializeComponent();
        ControlSpacer.NormalizePage(this);
        SubscribeEvents();
    }

    private void SubscribeEvents()
    {
        EventManager.SubscribeEvent("OnTalkToNpc", OnTalkToNpc);
        EventManager.SubscribeEvent("OnJobScaleUpdate", OnUpdateJobInfo);
        EventManager.SubscribeEvent("OnJobExperienceUpdate", OnUpdateJobInfo);
        EventManager.SubscribeEvent("OnJobJoin", OnUpdateJobInfo);
        EventManager.SubscribeEvent("OnJobLeave", OnUpdateJobInfo);
        EventManager.SubscribeEvent("OnJobAliasUpdate", OnUpdateJobInfo);
        EventManager.SubscribeEvent("OnTradeRoutesUpdated", OnTradeRoutesUpdated);
        EventManager.SubscribeEvent("OnAddLog", new Action<string, LogLevel>(OnAddLog));
    }

    private void OnUpdateJobInfo()
    {
        if (InvokeRequired)
        {
            BeginInvoke(new System.Action(OnUpdateJobInfo));
            return;
        }

        if (Game.Player?.TradeInfo == null)
            return;

        if (Game.Player.JobInformation.Type == JobType.None)
        {
            lblJobExp.Text = "0";
            lblJobLevel.Text = "0";
            lblJobAlias.Text = "<none>";
            lblTradeScale.Text = "■";
            return;
        }

        lblTradeScale.Text = new string('■', Math.Max(1, (int)Game.Player.TradeInfo.Scale));
        lblJobExp.Text = Game.Player.JobInformation.Experience.ToString();
        lblJobAlias.Text = Game.Player.JobInformation.Name ?? "<none>";
        lblJobLevel.Text = Game.Player.JobInformation.Level.ToString();
    }

    private void OnTalkToNpc(uint uniqueId)
    {
        if (Game.Player.State.DialogState is not { IsInDialog: true, TalkOption: TalkOption.Trade })
            return;

        EventManager.FireEvent("AppendScriptCommand", $"buy-goods {Game.Player.State.DialogState.Npc.Record.CodeName} Full");
    }

    private void OnTradeRoutesUpdated()
    {
        RefreshRouteListView();
    }

    private void OnAddLog(string message, LogLevel level)
    {
        if (string.IsNullOrEmpty(message)) return;
        if (message.Contains("[Trade]") || TradeBotbase.IsActive)
        {
            AppendTradeLog(message, level);
        }
    }

    public void AppendTradeLog(string message, LogLevel level = LogLevel.Notify)
    {
        if (txtTradeLog == null || txtTradeLog.IsDisposed) return;

        if (InvokeRequired)
        {
            BeginInvoke(new System.Action<string, LogLevel>(AppendTradeLog), message, level);
            return;
        }

        var timestamp = DateTime.Now.ToString("HH:mm:ss");
        var prefix = $"[{timestamp}] ";
        var color = level switch
        {
            LogLevel.Error => Color.FromArgb(248, 113, 113),
            LogLevel.Warning => Color.FromArgb(251, 191, 36),
            LogLevel.Debug => Color.FromArgb(148, 163, 184),
            _ => Color.FromArgb(226, 232, 240)
        };

        txtTradeLog.SelectionStart = txtTradeLog.TextLength;
        txtTradeLog.SelectionLength = 0;
        txtTradeLog.SelectionColor = Color.FromArgb(100, 116, 139);
        txtTradeLog.AppendText(prefix);

        txtTradeLog.SelectionColor = color;
        txtTradeLog.AppendText(message + Environment.NewLine);
        txtTradeLog.ScrollToCaret();
    }

    private void ReloadView()
    {
        _loadingConfig = true;

        // Populate Start and End cities
        if (comboStartCity.Items.Count == 0)
        {
            var cities = new[] { "Jangan", "Donwhang", "Hotan", "Samarkand", "Constantinople" };
            comboStartCity.Items.AddRange(cities);
            comboEndCity.Items.AddRange(cities);
            comboStartCity.SelectedIndex = 0; // Jangan
            comboEndCity.SelectedIndex = 2; // Hotan
        }

        // Populate Transport
        if (comboTransport.Items.Count == 0)
        {
            comboTransport.Items.AddRange(new object[] { "Auto / Any", "White Elephant", "Camel", "Horse" });
            var savedTransport = TradeConfig.SelectedTransport;
            var foundIdx = comboTransport.FindStringExact(savedTransport);
            comboTransport.SelectedIndex = foundIdx >= 0 ? foundIdx : 0;
        }

        // Route subtab controls
        checkSellGoods.Checked = TradeConfig.SellGoods;
        checkBuyGoods.Checked = TradeConfig.BuyGoods;
        numAmountGoods.Value = TradeConfig.BuyGoodsQuantity;
        numAmountGoods.Enabled = TradeConfig.BuyGoods;
        checkDisableAutoActivation.Checked = TradeConfig.DisableAutoActivation;

        // Settings tab controls
        checkRepeatLoop.Checked = TradeConfig.RepeatLoop;
        numRepeatTimes.Value = TradeConfig.RepeatTimes;
        numRepeatTimes.Enabled = TradeConfig.RepeatLoop;
        checkReturnScroll.Checked = TradeConfig.ReturnScrollAfterLoop;
        checkUnequipJobSuit.Checked = TradeConfig.UnequipJobSuitAfterLoop;
        checkSkipTownScripts.Checked = TradeConfig.SkipTownScripts;

        checkMountTransport.Checked = TradeConfig.MountTransport;
        checkProtectTransport.Checked = TradeConfig.ProtectTransport;
        numMaxDistance.Value = TradeConfig.MaxTransportDistance;
        numMaxDistance.Enabled = TradeConfig.ProtectTransport;

        checkAttackThiefNpc.Checked = TradeConfig.AttackThiefNpcs;
        checkAttackThiefPlayers.Checked = TradeConfig.AttackThiefPlayers;
        checkCounterAttack.Checked = TradeConfig.CounterAttack;
        checkCastBuffs.Checked = TradeConfig.CastBuffs;
        checkWaitForHunter.Checked = TradeConfig.WaitForHunter;

        RefreshRouteListView();

        _loadingConfig = false;
    }

    private void RefreshRouteListView()
    {
        if (InvokeRequired)
        {
            BeginInvoke(new System.Action(RefreshRouteListView));
            return;
        }

        lvRouteList.BeginUpdate();
        lvRouteList.Items.Clear();

        var routes = TradeConfig.Routes;
        if (routes != null)
        {
            for (int i = 0; i < routes.Count; i++)
            {
                var r = routes[i];
                r.Index = i + 1;
                var scriptDisplay = !string.IsNullOrEmpty(r.ScriptFile) ? Path.GetFileName(r.ScriptFile) : "<None>";
                var item = new ListViewItem(r.Index.ToString());
                item.SubItems.Add(r.StartCity ?? "");
                item.SubItems.Add(r.EndCity ?? "");
                item.SubItems.Add(scriptDisplay);
                item.SubItems.Add(r.Active ? "Yes" : "No");
                item.SubItems.Add(r.LoopCount.ToString());

                if (r.Active)
                {
                    item.Font = new Font(lvRouteList.Font, FontStyle.Bold);
                    item.ForeColor = Color.FromArgb(16, 185, 129);
                }

                lvRouteList.Items.Add(item);
            }
        }

        lvRouteList.EndUpdate();
    }

    private void ReindexRoutes(List<TradeRouteItem> routes)
    {
        for (int i = 0; i < routes.Count; i++)
            routes[i].Index = i + 1;
    }

    private void btnAddRoute_Click(object sender, EventArgs e)
    {
        var startCity = comboStartCity.Text?.Trim();
        var endCity = comboEndCity.Text?.Trim();

        if (string.IsNullOrWhiteSpace(startCity) || string.IsNullOrWhiteSpace(endCity))
        {
            AppendTradeLog("Please select valid Start and End cities.", LogLevel.Warning);
            return;
        }

        if (startCity.Equals(endCity, StringComparison.OrdinalIgnoreCase))
        {
            AppendTradeLog("Start and End cities cannot be the same.", LogLevel.Warning);
            return;
        }

        var scriptFile = TradeConfig.ResolveTradeScript(startCity, endCity);
        if (string.IsNullOrEmpty(scriptFile) || !File.Exists(scriptFile))
        {
            var ofd = new OpenFileDialog
            {
                Title = $"Select trade script for {startCity} to {endCity}",
                Filter = "Silkroad Script (*.vb;*.txt;*.rbs)|*.vb;*.txt;*.rbs|All files (*.*)|*.*",
                InitialDirectory = Path.Combine(Kernel.BasePath, "Data", "Scripts", "Trade")
            };

            if (ofd.ShowDialog() == DialogResult.OK)
            {
                scriptFile = ofd.FileName;
            }
            else
            {
                AppendTradeLog($"No script found for {startCity} to {endCity}. Route not added.", LogLevel.Warning);
                return;
            }
        }

        var routes = TradeConfig.Routes ?? new List<TradeRouteItem>();
        var isFirst = routes.Count == 0;
        var newRoute = new TradeRouteItem
        {
            Index = routes.Count + 1,
            StartCity = startCity,
            EndCity = endCity,
            ScriptFile = scriptFile,
            Active = isFirst,
            LoopCount = 1
        };

        routes.Add(newRoute);
        TradeConfig.Routes = routes;
        RefreshRouteListView();
        AppendTradeLog($"Added Route #{newRoute.Index}: {startCity} -> {endCity} ({Path.GetFileName(scriptFile)})", LogLevel.Notify);
    }

    private void btnStartTrade_Click(object sender, EventArgs e)
    {
        if (!Kernel.Bot.Running)
        {
            Kernel.Bot.Start();
            AppendTradeLog("Trade bot started.", LogLevel.Notify);
        }
    }

    private void btnStopTrade_Click(object sender, EventArgs e)
    {
        if (Kernel.Bot.Running)
        {
            Kernel.Bot.Stop();
            AppendTradeLog("Trade bot stopped.", LogLevel.Notify);
        }
    }

    private void btnMoveUp_Click(object sender, EventArgs e)
    {
        if (lvRouteList.SelectedIndices.Count == 0) return;
        var idx = lvRouteList.SelectedIndices[0];
        var routes = TradeConfig.Routes;
        if (idx > 0 && idx < routes.Count)
        {
            var item = routes[idx];
            routes.RemoveAt(idx);
            routes.Insert(idx - 1, item);
            ReindexRoutes(routes);
            TradeConfig.Routes = routes;
            RefreshRouteListView();
            if (idx - 1 < lvRouteList.Items.Count)
                lvRouteList.Items[idx - 1].Selected = true;
        }
    }

    private void btnMoveDown_Click(object sender, EventArgs e)
    {
        if (lvRouteList.SelectedIndices.Count == 0) return;
        var idx = lvRouteList.SelectedIndices[0];
        var routes = TradeConfig.Routes;
        if (idx >= 0 && idx < routes.Count - 1)
        {
            var item = routes[idx];
            routes.RemoveAt(idx);
            routes.Insert(idx + 1, item);
            ReindexRoutes(routes);
            TradeConfig.Routes = routes;
            RefreshRouteListView();
            if (idx + 1 < lvRouteList.Items.Count)
                lvRouteList.Items[idx + 1].Selected = true;
        }
    }

    private void menuActivateRoute_Click(object sender, EventArgs e)
    {
        if (lvRouteList.SelectedIndices.Count == 0) return;
        var idx = lvRouteList.SelectedIndices[0];
        var routes = TradeConfig.Routes;
        if (idx >= 0 && idx < routes.Count)
        {
            for (int i = 0; i < routes.Count; i++)
                routes[i].Active = (i == idx);
            TradeConfig.Routes = routes;
            RefreshRouteListView();
            AppendTradeLog($"Activated Route #{routes[idx].Index}: {routes[idx].StartCity} -> {routes[idx].EndCity}", LogLevel.Notify);
        }
    }

    private void menuSetScript_Click(object sender, EventArgs e)
    {
        if (lvRouteList.SelectedIndices.Count == 0) return;
        var idx = lvRouteList.SelectedIndices[0];
        var routes = TradeConfig.Routes;
        if (idx >= 0 && idx < routes.Count)
        {
            var ofd = new OpenFileDialog
            {
                Title = "Select script file",
                Filter = "Silkroad Script (*.vb;*.txt;*.rbs)|*.vb;*.txt;*.rbs|All files (*.*)|*.*",
                InitialDirectory = Path.Combine(Kernel.BasePath, "Data", "Scripts", "Trade")
            };
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                routes[idx].ScriptFile = ofd.FileName;
                TradeConfig.Routes = routes;
                RefreshRouteListView();
                AppendTradeLog($"Updated script for Route #{routes[idx].Index} to: {Path.GetFileName(ofd.FileName)}", LogLevel.Notify);
            }
        }
    }

    private void menuRemoveRoute_Click(object sender, EventArgs e)
    {
        if (lvRouteList.SelectedIndices.Count == 0) return;
        var idx = lvRouteList.SelectedIndices[0];
        var routes = TradeConfig.Routes;
        if (idx >= 0 && idx < routes.Count)
        {
            var removed = routes[idx];
            routes.RemoveAt(idx);
            if (removed.Active && routes.Count > 0)
                routes[0].Active = true;
            ReindexRoutes(routes);
            TradeConfig.Routes = routes;
            RefreshRouteListView();
            AppendTradeLog($"Removed Route: {removed.StartCity} -> {removed.EndCity}", LogLevel.Notify);
        }
    }

    private void menuClearRoutes_Click(object sender, EventArgs e)
    {
        var routes = TradeConfig.Routes;
        routes.Clear();
        TradeConfig.Routes = routes;
        RefreshRouteListView();
        AppendTradeLog("All trade routes cleared.", LogLevel.Notify);
    }

    private void checkSellGoods_CheckedChanged(object sender, EventArgs e)
    {
        if (_loadingConfig) return;
        TradeConfig.SellGoods = checkSellGoods.Checked;
    }

    private void checkBuyGoods_CheckedChanged(object sender, EventArgs e)
    {
        if (_loadingConfig) return;
        TradeConfig.BuyGoods = checkBuyGoods.Checked;
        numAmountGoods.Enabled = checkBuyGoods.Checked;
    }

    private void numAmountGoods_ValueChanged(object sender, EventArgs e)
    {
        if (_loadingConfig) return;
        TradeConfig.BuyGoodsQuantity = Convert.ToInt32(numAmountGoods.Value);
    }

    private void checkDisableAutoActivation_CheckedChanged(object sender, EventArgs e)
    {
        if (_loadingConfig) return;
        TradeConfig.DisableAutoActivation = checkDisableAutoActivation.Checked;
    }

    private void linkTradeGuide_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
    {
        var guideText = "AeroBot Trade System Guide:\n\n" +
                        "1. Equip your Trader / Hunter / Thief Job Suit.\n" +
                        "2. Have transport summon scrolls (Elephant/Camel/Horse) in inventory.\n" +
                        "3. Choose Start and End cities, then click 'Add Route'.\n" +
                        "4. Add multiple routes for a multi-city trade run (e.g. Jangan -> Hotan, then Hotan -> Jangan).\n" +
                        "5. Check 'Repeat trade loop' in Settings to repeat back and forth automatically.\n" +
                        "6. Check 'Buy Special Goods' and 'Sell goods' as desired.\n" +
                        "7. Click '▶ Start' to launch the automated trade expedition!";

        MessageBox.Show(guideText, "AeroBot Trade Guide", MessageBoxButtons.OK, MessageBoxIcon.Information);
    }

    private void checkRepeatLoop_CheckedChanged(object sender, EventArgs e)
    {
        if (_loadingConfig) return;
        TradeConfig.RepeatLoop = checkRepeatLoop.Checked;
        numRepeatTimes.Enabled = checkRepeatLoop.Checked;
    }

    private void numRepeatTimes_ValueChanged(object sender, EventArgs e)
    {
        if (_loadingConfig) return;
        TradeConfig.RepeatTimes = Convert.ToInt32(numRepeatTimes.Value);
    }

    private void checkReturnScroll_CheckedChanged(object sender, EventArgs e)
    {
        if (_loadingConfig) return;
        TradeConfig.ReturnScrollAfterLoop = checkReturnScroll.Checked;
    }

    private void checkUnequipJobSuit_CheckedChanged(object sender, EventArgs e)
    {
        if (_loadingConfig) return;
        TradeConfig.UnequipJobSuitAfterLoop = checkUnequipJobSuit.Checked;
    }

    private void checkSkipTownScripts_CheckedChanged(object sender, EventArgs e)
    {
        if (_loadingConfig) return;
        TradeConfig.SkipTownScripts = checkSkipTownScripts.Checked;
    }

    private void checkMountTransport_CheckedChanged(object sender, EventArgs e)
    {
        if (_loadingConfig) return;
        TradeConfig.MountTransport = checkMountTransport.Checked;
    }

    private void checkProtectTransport_CheckedChanged(object sender, EventArgs e)
    {
        if (_loadingConfig) return;
        TradeConfig.ProtectTransport = checkProtectTransport.Checked;
        numMaxDistance.Enabled = checkProtectTransport.Checked;
    }

    private void numMaxDistance_ValueChanged(object sender, EventArgs e)
    {
        if (_loadingConfig) return;
        TradeConfig.MaxTransportDistance = Convert.ToInt32(numMaxDistance.Value);
    }

    private void checkAttackThiefNpc_CheckedChanged(object sender, EventArgs e)
    {
        if (_loadingConfig) return;
        TradeConfig.AttackThiefNpcs = checkAttackThiefNpc.Checked;
    }

    private void checkAttackThiefPlayers_CheckedChanged(object sender, EventArgs e)
    {
        if (_loadingConfig) return;
        TradeConfig.AttackThiefPlayers = checkAttackThiefPlayers.Checked;
    }

    private void checkCounterAttack_CheckedChanged(object sender, EventArgs e)
    {
        if (_loadingConfig) return;
        TradeConfig.CounterAttack = checkCounterAttack.Checked;
    }

    private void checkCastBuffs_CheckedChanged(object sender, EventArgs e)
    {
        if (_loadingConfig) return;
        TradeConfig.CastBuffs = checkCastBuffs.Checked;
    }

    private void checkWaitForHunter_CheckedChanged(object sender, EventArgs e)
    {
        if (_loadingConfig) return;
        TradeConfig.WaitForHunter = checkWaitForHunter.Checked;
    }

    private void comboTransport_SelectedIndexChanged(object sender, EventArgs e)
    {
        if (_loadingConfig) return;
        TradeConfig.SelectedTransport = comboTransport.SelectedItem?.ToString() ?? "Auto";
    }

    private void Main_Load(object sender, EventArgs e)
    {
        ReloadView();
    }
}