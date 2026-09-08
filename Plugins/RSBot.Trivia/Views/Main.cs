using System;
using System.ComponentModel;
using System.Windows.Forms;
using RSBot.Core;
using SDUI.Controls;
using SDUI.Helpers;

namespace RSBot.Trivia.Views;

[ToolboxItem(false)]
public partial class Main : DoubleBufferedControl
{
    private static Main _instance;
    public static Main Instance => _instance ??= new Main();

    public Main()
    {
        InitializeComponent();
        ControlSpacer.NormalizePage(this);

        TriviaManager.Instance.OnStateChanged += () => SafeInvoke(UpdateStateUI);
        TriviaManager.Instance.OnDatabaseChanged += () => SafeInvoke(UpdateDatabaseUI);
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
        chkEnabled.Checked = TriviaManager.Instance.Enabled;
        txtQuestionPattern.Text = TriviaManager.Instance.QuestionPattern;
        txtAnswerPattern.Text = TriviaManager.Instance.AnswerPattern;
        txtReplyTo.Text = TriviaManager.Instance.ReplyTo;

        UpdateStateUI();
        UpdateDatabaseUI();
    }

    private void UpdateStateUI()
    {
        lblStatus.Text = $"Status: {TriviaManager.Instance.StatusText}";
    }

    private void UpdateDatabaseUI()
    {
        lstQA.BeginUpdate();
        lstQA.Items.Clear();

        foreach (var entry in TriviaManager.Instance.Entries)
        {
            var lvi = new ListViewItem(entry.Question);
            lvi.SubItems.Add(entry.Answer);
            lvi.Tag = entry;
            lstQA.Items.Add(lvi);
        }

        lstQA.EndUpdate();
        lblCount.Text = $"Total stored Q&A: {TriviaManager.Instance.Entries.Count}";
    }

    private void chkEnabled_CheckedChanged(object sender, EventArgs e)
    {
        TriviaManager.Instance.Enabled = chkEnabled.Checked;
        TriviaManager.Instance.SaveConfig();
    }

    private void btnSaveConfig_Click(object sender, EventArgs e)
    {
        TriviaManager.Instance.QuestionPattern = txtQuestionPattern.Text.Trim();
        TriviaManager.Instance.AnswerPattern = txtAnswerPattern.Text.Trim();
        TriviaManager.Instance.ReplyTo = txtReplyTo.Text.Trim();
        TriviaManager.Instance.SaveConfig();
        lblStatus.Text = "Status: Settings saved";
    }

    private void btnAddQA_Click(object sender, EventArgs e)
    {
        var q = txtQuestion.Text.Trim();
        var a = txtAnswer.Text.Trim();

        if (!string.IsNullOrEmpty(q) && !string.IsNullOrEmpty(a))
        {
            TriviaManager.Instance.AddOrUpdate(q, a);
            txtQuestion.Text = string.Empty;
            txtAnswer.Text = string.Empty;
        }
    }

    private void btnRemoveQA_Click(object sender, EventArgs e)
    {
        if (lstQA.SelectedItems.Count > 0)
        {
            var q = lstQA.SelectedItems[0].Text;
            TriviaManager.Instance.Remove(q);
        }
    }
}
