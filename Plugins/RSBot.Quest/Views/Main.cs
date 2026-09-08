using System;
using System.ComponentModel;
using System.Linq;
using System.Windows.Forms;
using RSBot.Core;
using RSBot.Core.Event;
using RSBot.Core.Objects.Quests;
using SDUI;
using SDUI.Controls;
using SDUI.Helpers;

namespace RSBot.Quest.Views;

[ToolboxItem(false)]
public partial class Main : DoubleBufferedControl
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="Main" /> class.
    /// </summary>
    public Main()
    {
        CheckForIllegalCrossThreadCalls = false;
        InitializeComponent();
        ControlSpacer.NormalizePage(this);

        SubscribeEvents();
        ApplyTheme();
    }

    private void ApplyTheme()
    {
        treeQuests.BackColor = ColorScheme.BackColor;
        treeQuests.ForeColor = ColorScheme.ForeColor;
        treeQuests.LineColor = ColorScheme.BorderColor;
    }

    private void SubscribeEvents()
    {
        EventManager.SubscribeEvent("OnLoadCharacter", RefreshQuestList);
        EventManager.SubscribeEvent("OnUpdateQuests", RefreshQuestList);
    }

    private void RefreshQuestList()
    {
        try
        {
            if (!treeQuests.Created)
                return;

            treeQuests.Invoke(() =>
            {
                treeQuests.Nodes.Clear();

                if (Game.Player == null)
                {
                    treeQuests.Nodes.Add(new TreeNode("من فضلك ادخل إلى اللعبة لعرض المهام (Please enter game)"));
                    return;
                }

                if (Game.Player.QuestLog?.ActiveQuests == null || Game.Player.QuestLog.ActiveQuests.Count == 0)
                {
                    treeQuests.Nodes.Add(new TreeNode("لا توجد مهام نشطة حالياً (No active quests)"));
                }
                else
                {
                    foreach (var activeQuest in Game.Player.QuestLog.ActiveQuests)
                    {
                        var node = CreateNode(activeQuest.Value);
                        node.Tag = activeQuest.Key;
                        node.ContextMenuStrip = contextQuest;
                        treeQuests.Nodes.Add(node);
                    }
                }

                if (!checkShowCompleted.Checked) return;

                if (Game.Player.QuestLog?.CompletedQuests != null && Game.Player.QuestLog.CompletedQuests.Length > 0)
                {
                    var completedNode = new TreeNode("المهام المكتملة (Completed Quests)");
                    foreach (var questId in Game.Player.QuestLog.CompletedQuests)
                    {
                        var quest = Game.ReferenceManager.GetRefQuest(questId);
                        if (quest != null)
                        {
                            var node = new TreeNode($"{quest.GetTranslatedName()} (lv. {quest.Level})");
                            completedNode.Nodes.Add(node);
                        }
                    }

                    treeQuests.Nodes.Add(completedNode);
                }
            });
        }
        catch
        {
            // ignored
        }
    }

    private TreeNode CreateNode(ActiveQuest quest)
    {
        if (quest.Quest == null)
            return new TreeNode($"Unknown quest [{quest.Id}]");

        var name = Game.ReferenceManager.GetTranslation(quest.Quest.NameString);

        var node = new TreeNode(name);
        node.Nodes.Add($"Id: {quest.Id}");
        node.Nodes.Add($"Level: {quest.Quest.Level}");
        node.Nodes.Add($"Status: {GetStatusText(quest.Status)}");

        node.Tag = quest.Id;

        if (quest.Npcs?.Length > 0)
        {
            var npcNode = new TreeNode("NPCs");
            foreach (var npcId in quest.Npcs)
            {
                var npc = Game.ReferenceManager.GetRefObjChar(npcId);
                if (npc != null)
                {
                    var npcName = Game.ReferenceManager.GetTranslation(npc.NameStrID);
                    npcNode.Nodes.Add(npcName);
                }
            }

            node.Nodes.Add(npcNode);
        }

        var rewardNode = new TreeNode("Rewards");

        if (quest.Quest.Reward != null)
        {
            if (quest.Quest.Reward.Exp > 0)
                rewardNode.Nodes.Add($"Exp: {quest.Quest.Reward.Exp:N0}");

            if (quest.Quest.Reward.Gold > 0)
                rewardNode.Nodes.Add($"Gold: {quest.Quest.Reward.Gold:N0}");

            if (quest.Quest.Reward.SP > 0)
                rewardNode.Nodes.Add($"Skill Points (SP): {quest.Quest.Reward.SP:N0}");

            if (quest.Quest.Reward.SPExp > 0)
                rewardNode.Nodes.Add($"Skill Exp: {quest.Quest.Reward.SPExp:N0}");

            if (quest.Quest.Reward.InventorySlots > 0)
                rewardNode.Nodes.Add($"Inv. Slots: {quest.Quest.Reward.InventorySlots}");

            if (quest.Quest.Reward.Hwan > 0)
                rewardNode.Nodes.Add($"Hwan: {quest.Quest.Reward.Hwan}");
        }

        if (quest.Quest.RewardItems != null && quest.Quest.RewardItems.Any())
        {
            var itemsNode = new TreeNode("Items");

            foreach (var rewardItem in quest.Quest.RewardItems)
            {
                if (rewardItem.Item != null)
                    itemsNode.Nodes.Add($"{rewardItem.Item.GetRealName()}");

                if (rewardItem.OptionalItem != null)
                    itemsNode.Nodes.Add($"{rewardItem.OptionalItem.GetRealName()}");
            }

            rewardNode.Nodes.Add(itemsNode);
        }

        node.Nodes.Add(rewardNode);

        if (quest.Objectives?.Length > 0)
            foreach (var objective in quest.Objectives)
            {
                var objectiveName = Game.ReferenceManager.GetTranslation(objective.NameStrId);
                var objectiveSubNode = new TreeNode(objectiveName);
                if (objective.InProgress)
                    objectiveSubNode.Nodes.Add("Status: In progress (قيد التنفيذ)");
                else
                    objectiveSubNode.Nodes.Add("Status: Complete (مكتمل)");

                foreach (var task in objective.Tasks)
                {
                    var actualTitle = objectiveName.Replace("%d", task.ToString());

                    objectiveSubNode.Nodes.Add($"Progress: {task}");
                    objectiveSubNode.Text = actualTitle;
                }

                node.Nodes.Add(objectiveSubNode);
            }

        return node;
    }

    private string GetStatusText(QuestStatus status)
    {
        switch (status)
        {
            case QuestStatus.Cancelled:
                return "Cancelled (ملغاة)";
            case QuestStatus.Completed:
            case QuestStatus.CompletedXTimes:
                return "Completed (مكتملة)";
            case QuestStatus.CompletedButNotSupplied:
            case QuestStatus.CompletedByUserButNotSupplied:
                return "Ready for delivery (جاهزة للتسليم)";
            case QuestStatus.Initialized:
            case QuestStatus.StartedByUser:
                return "In progress (قيد التنفيذ)";
            case QuestStatus.Unavailable:
                return "Unavailable (غير متاحة)";
            default:
                return status.ToString();
        }
    }

    private void checkShowCompleted_CheckedChanged(object sender, EventArgs e)
    {
        RefreshQuestList();
    }

    private void btnRefresh_Click(object sender, EventArgs e)
    {
        RefreshQuestList();
    }

    private void Main_Load(object sender, EventArgs e)
    {
        RefreshQuestList();
    }

    private void watchQuestToolStripMenuItem_Click(object sender, EventArgs e)
    {
        if (treeQuests.SelectedNode?.Tag == null || !uint.TryParse(treeQuests.SelectedNode.Tag.ToString(), out var questId))
            return;

        if (View.SidebarElement != null)
        {
            if (View.SidebarElement.HasQuest(questId))
                View.SidebarElement.RemoveQuest(questId);
            else
                View.SidebarElement.AddQuest(questId);
        }
    }

    private void treeQuests_NodeMouseClick(object sender, TreeNodeMouseClickEventArgs e)
    {
        treeQuests.SelectedNode = e.Node;
    }

    private void abandonToolStripMenuItem_Click(object sender, EventArgs e)
    {
        if (treeQuests.SelectedNode?.Tag == null || !uint.TryParse(treeQuests.SelectedNode.Tag.ToString(), out var questId))
            return;

        if (Game.Player?.QuestLog?.ActiveQuests == null || !Game.Player.QuestLog.ActiveQuests.TryGetValue(questId, out var activeQuest))
            return;

        var questTitle = activeQuest.Quest?.GetTranslatedName() ?? $"Quest {questId}";
        if (MessageBox.Show($"هل أنت متأكد من رغبتك في إلغاء المهمة [{questTitle}]؟\n(Do you really want to abandon this quest?)",
                "إلغاء المهمة - Abandon Quest", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            Game.Player.QuestLog.AbandonQuest(questId);
    }
}