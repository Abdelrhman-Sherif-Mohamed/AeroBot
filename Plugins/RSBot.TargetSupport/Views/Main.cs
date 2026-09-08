using System;
using System.ComponentModel;
using System.Linq;
using System.Windows.Forms;
using RSBot.Core;
using SDUI.Controls;
using SDUI.Helpers;

namespace RSBot.TargetSupport.Views;

[ToolboxItem(false)]
public partial class Main : DoubleBufferedControl
{
    private static Main _instance;
    public static Main Instance => _instance ??= new Main();

    public Main()
    {
        InitializeComponent();
        ControlSpacer.NormalizePage(this);

        TargetSupportManager.Instance.OnStateChanged += () => SafeInvoke(UpdateUI);
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
        RefreshPartyMembers();
    }

    private void UpdateUI()
    {
        chkEnabled.Checked = TargetSupportManager.Instance.Enabled;
        chkDefensive.Checked = TargetSupportManager.Instance.DefensiveMode;
        chkAutoAttack.Checked = TargetSupportManager.Instance.AutoAttack;

        lstLeaders.BeginUpdate();
        lstLeaders.Items.Clear();
        foreach (var leader in TargetSupportManager.Instance.Leaders)
        {
            lstLeaders.Items.Add(new ListViewItem(leader));
        }
        lstLeaders.EndUpdate();

        lblStatus.Text = $"Status: {TargetSupportManager.Instance.StatusText}";
        lblTarget.Text = string.IsNullOrEmpty(TargetSupportManager.Instance.LastTargetName)
            ? "Current Target: None"
            : $"Current Target: {TargetSupportManager.Instance.LastTargetName} (UID: {TargetSupportManager.Instance.LastTargetUID})";
    }

    private void chkEnabled_CheckedChanged(object sender, EventArgs e)
    {
        TargetSupportManager.Instance.Enabled = chkEnabled.Checked;
        TargetSupportManager.Instance.SaveConfig();
    }

    private void chkDefensive_CheckedChanged(object sender, EventArgs e)
    {
        TargetSupportManager.Instance.DefensiveMode = chkDefensive.Checked;
        TargetSupportManager.Instance.SaveConfig();
    }

    private void chkAutoAttack_CheckedChanged(object sender, EventArgs e)
    {
        TargetSupportManager.Instance.AutoAttack = chkAutoAttack.Checked;
        TargetSupportManager.Instance.SaveConfig();
    }

    private void btnAddLeader_Click(object sender, EventArgs e)
    {
        var name = cboLeaderName.Text.Trim();
        if (!string.IsNullOrEmpty(name))
        {
            TargetSupportManager.Instance.AddLeader(name);
            cboLeaderName.Text = string.Empty;
            UpdateUI();
        }
    }

    private void btnRemoveLeader_Click(object sender, EventArgs e)
    {
        if (lstLeaders.SelectedItems.Count > 0)
        {
            var name = lstLeaders.SelectedItems[0].Text;
            TargetSupportManager.Instance.RemoveLeader(name);
            UpdateUI();
        }
    }

    private void btnGetPartyLeader_Click(object sender, EventArgs e)
    {
        if (Game.Party?.Members != null && Game.Party.Members.Count > 0)
        {
            // In Silkroad, party leader is the first member in party list or where IsLeader is true
            var leader = Game.Party.Leader;
            if (leader != null && !string.IsNullOrEmpty(leader.Name))
            {
                TargetSupportManager.Instance.AddLeader(leader.Name);
                UpdateUI();
                return;
            }

            var first = Game.Party.Members.FirstOrDefault();
            if (first != null && !string.IsNullOrEmpty(first.Name))
            {
                TargetSupportManager.Instance.AddLeader(first.Name);
                UpdateUI();
            }
        }
    }
}
