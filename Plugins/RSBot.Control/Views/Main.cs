using System;
using System.ComponentModel;
using System.Linq;
using System.Windows.Forms;
using RSBot.Core;
using SDUI.Controls;
using SDUI.Helpers;

namespace RSBot.Control.Views;

[ToolboxItem(false)]
public partial class Main : DoubleBufferedControl
{
    private static Main _instance;
    public static Main Instance => _instance ??= new Main();

    public Main()
    {
        InitializeComponent();
        ControlSpacer.NormalizePage(this);

        ControlManager.Instance.OnStateChanged += () => SafeInvoke(UpdateUI);
        ControlManager.Instance.OnLogAdded += log => SafeInvoke(() => AppendLog(log));

        cboLeaderName.DropDown += (s, e) => RefreshPartyMembers();
    }

    private void RefreshPartyMembers()
    {
        var currentText = cboLeaderName.Text;
        cboLeaderName.Items.Clear();

        if (Game.Party?.Members != null)
        {
            foreach (var member in Game.Party.Members)
            {
                if (!string.IsNullOrEmpty(member.Name) && member.Name != Game.Player?.Name)
                {
                    if (!cboLeaderName.Items.Contains(member.Name))
                        cboLeaderName.Items.Add(member.Name);
                }
            }
        }

        cboLeaderName.Text = currentText;
    }

    private void SafeInvoke(System.Action action)
    {
        if (IsHandleCreated && InvokeRequired)
            BeginInvoke(action);
        else
            action();
    }

    private void Main_Load(object sender, EventArgs e)
    {
        UpdateUI();
        LoadLogs();
        RefreshPartyMembers();
    }

    private void UpdateUI()
    {
        chkEnabled.Checked = ControlManager.Instance.Enabled;

        lstLeaders.BeginUpdate();
        lstLeaders.Items.Clear();
        foreach (var leader in ControlManager.Instance.Leaders)
        {
            lstLeaders.Items.Add(new ListViewItem(leader));
        }
        lstLeaders.EndUpdate();
    }

    private void LoadLogs()
    {
        txtLogs.Clear();
        foreach (var l in ControlManager.Instance.Logs)
        {
            txtLogs.AppendText(l + "\n");
        }
        txtLogs.ScrollToCaret();
    }

    private void AppendLog(string log)
    {
        txtLogs.AppendText(log + "\n");
        txtLogs.ScrollToCaret();
    }

    private void chkEnabled_CheckedChanged(object sender, EventArgs e)
    {
        ControlManager.Instance.Enabled = chkEnabled.Checked;
        ControlManager.Instance.SaveConfig();
    }

    private void btnAddLeader_Click(object sender, EventArgs e)
    {
        var name = cboLeaderName.Text.Trim();
        if (!string.IsNullOrEmpty(name))
        {
            ControlManager.Instance.AddLeader(name);
            cboLeaderName.Text = string.Empty;
            UpdateUI();
        }
    }

    private void btnRemoveLeader_Click(object sender, EventArgs e)
    {
        if (lstLeaders.SelectedItems.Count > 0)
        {
            var name = lstLeaders.SelectedItems[0].Text;
            ControlManager.Instance.RemoveLeader(name);
            UpdateUI();
        }
    }

    private void btnGetPartyLeader_Click(object sender, EventArgs e)
    {
        if (Game.Party?.Members != null && Game.Party.Members.Count > 0)
        {
            var leader = Game.Party.Leader;
            if (leader != null && !string.IsNullOrEmpty(leader.Name))
            {
                ControlManager.Instance.AddLeader(leader.Name);
                UpdateUI();
                return;
            }

            var first = Game.Party.Members.FirstOrDefault();
            if (first != null && !string.IsNullOrEmpty(first.Name))
            {
                ControlManager.Instance.AddLeader(first.Name);
                UpdateUI();
            }
        }
    }

    private void btnClearLogs_Click(object sender, EventArgs e)
    {
        ControlManager.Instance.Logs.Clear();
        txtLogs.Clear();
    }
}
