using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using RSBot.Core;
using RSBot.Core.Client.ReferenceObjects;
using RSBot.Core.Components;
using RSBot.Core.Event;
using RSBot.Core.Extensions;
using RSBot.Core.Objects.Party;
using RSBot.Core.Objects.Skill;
using SDUI;
using SDUI.Controls;
using Button = SDUI.Controls.Button;
using ListViewExtensions = RSBot.Core.Extensions.ListViewExtensions;

namespace RSBot.Party.Views;

[ToolboxItem(false)]
public partial class Main : DoubleBufferedControl
{
    /// <summary>
    ///     <inheritdoc />
    /// </summary>
    private bool _applySettings = true;

    /// <summary>
    ///     The buffing party member list
    /// </summary>
    private List<BuffingPartyMember> _buffings;

    /// <summary>
    ///     The selected buffing group
    /// </summary>
    private ListViewItem _selectedBuffingGroup;

    /// <summary>
    ///     Initializes a new instance of the <see cref="Main" /> class.
    /// </summary>
    public Main()
    {
        InitializeComponent();

        selectedMemberBuffs.SmallImageList = ListViewExtensions.StaticImageList;
        listPartyBuffSkills.SmallImageList = ListViewExtensions.StaticImageList;

        _selectedBuffingGroup = new ListViewItem();
        _buffings = new List<BuffingPartyMember>();
        CheckForIllegalCrossThreadCalls = false;
        cbPartySearchPurpose.SelectedIndex = 0;

        SubscribeEvents();
    }

    /// <summary>
    ///     Translated No Guild Text
    /// </summary>
    private string _noGuildText => LanguageManager.GetLang("NoGuild");

    private string _none => LanguageManager.GetLang("None");

    private bool _applyingPartyLayout;

    protected override void OnResize(EventArgs e)
    {
        base.OnResize(e);
        ApplyPartyLayout();
    }

    protected override void OnVisibleChanged(EventArgs e)
    {
        base.OnVisibleChanged(e);

        if (!Visible)
            return;

        ApplyPartyLocalization();
        ApplyPartyLayout();
    }

    private int S(float value)
    {
        return (int)Math.Round(value * DeviceDpi / 96f);
    }

    private static string T(string key, string fallback)
    {
        return LanguageManager.GetLangBySpecificKey("RSBot.Party", key, fallback);
    }

    private void Place(Control control, int x, int y, int width, int height)
    {
        if (control == null)
            return;

        control.Anchor = AnchorStyles.None;
        control.Dock = DockStyle.None;
        control.AutoSize = false;
        control.SetBounds(S(x), S(y), Math.Max(1, S(width)), Math.Max(1, S(height)));
    }

    private void PlaceLabel(SDUI.Controls.Label label, int x, int y, int width, int height,
        ContentAlignment alignment)
    {
        if (label == null)
            return;

        Place(label, x, y, width, height);
        label.TextAlign = alignment;
    }

    /// <summary>
    ///     Lays out the party page for the actual page surface. The designer
    ///     coordinates were made for the original 746x435 tab page, while the
    ///     application shows the page at roughly 1116x577. Re-positioning the
    ///     controls keeps the Arabic captions readable and uses the available
    ///     width instead of leaving large empty areas.
    /// </summary>
    private void ApplyPartyLayout()
    {
        if (_applyingPartyLayout || IsDisposed || tabMain == null || tabMain.TabPages.Count < 4)
            return;

        var area = tabMain.DisplayRectangle;
        if (area.Width < 200 || area.Height < 200)
            return;

        _applyingPartyLayout = true;
        try
        {
            // The old fixed layout used docked separator lines between the
            // panels. The responsive cards already provide enough spacing, so
            // these stray lines are hidden.
            separator3.Visible = false;
            separator4.Visible = false;
            separator5.Visible = false;
            separator6.Visible = false;

            var scale = DeviceDpi / 96f;
            var pageW = (int)(area.Width / scale);
            var pageH = (int)(area.Height / scale);
            var layoutW = Math.Max(pageW, 980);
            var layoutH = Math.Max(pageH, 560);

            LayoutCurrentParty(layoutW, layoutH);
            LayoutAutoParty(layoutW, layoutH);
            LayoutPartyMatching(layoutW, layoutH);
            LayoutBuffing(layoutW, layoutH);

            // Only reserve virtual space when the page is actually smaller
            // than the design minimum. This avoids needless scrollbars at the
            // default 1300x820 window size.
            var needsScroll = layoutW > pageW || layoutH > pageH;
            foreach (TabPage page in tabMain.TabPages)
            {
                page.AutoScroll = true;
                page.AutoScrollMinSize = needsScroll
                    ? new Size(S(layoutW), S(layoutH))
                    : Size.Empty;
            }
        }
        finally
        {
            _applyingPartyLayout = false;
        }
    }

    private void LayoutCurrentParty(int width, int height)
    {
        const int margin = 14;
        const int gap = 12;
        const int topH = 76;

        var contentW = width - margin * 2;
        var topY = margin;

        Place(grpPartySettings, margin, topY, contentW, topH);

        var cbGap = 16;
        var cbW = (contentW - 32 - cbGap * 2) / 3;
        Place(checkCurrentAutoShareEXP, 16, 28, cbW, 30);
        Place(checkCurrentAllowInvitations, 16 + cbW + cbGap, 28, cbW, 30);
        Place(checkCurrentAutoShareItems, 16 + (cbW + cbGap) * 2, 28, cbW, 30);

        var bodyY = topY + topH + gap;
        var footerH = 36;
        var bodyH = height - bodyY - margin - footerH - gap;

        Place(listParty, margin, bodyY, contentW, Math.Max(80, bodyH));
        Place(panel1, margin, height - margin - footerH, contentW, footerH);

        Place(label1, 16, 6, 60, 24);
        Place(lblLeader, 78, 6, Math.Max(180, contentW - 78 - 166), 24);
        Place(btnLeaveParty, contentW - 16 - 150, 4, 150, 26);

        colMemberName.Width = 220;
        colLevel.Width = 70;
        colGuild.Width = 180;
        colMasteries.Width = 300;
        colLocation.Width = Math.Max(150, contentW - 2 - 220 - 70 - 180 - 300);
    }

    private void LayoutAutoParty(int width, int height)
    {
        const int margin = 14;
        const int gap = 12;
        const int topH = 76;

        var contentW = width - margin * 2;
        var topY = margin;

        Place(grbAutoPartySettings, margin, topY, contentW, topH);

        var cbGap = 16;
        var cbW = (contentW - 32 - cbGap * 2) / 3;
        Place(checkAutoExpAutoShare, 16, 28, cbW, 30);
        Place(checkAutoAllowInvitations, 16 + cbW + cbGap, 28, cbW, 30);
        Place(checkAutoItemAutoShare, 16 + (cbW + cbGap) * 2, 28, cbW, 30);

        var bodyY = topY + topH + gap;
        var bodyH = height - bodyY - margin;
        var leftW = Math.Min(340, Math.Max(280, (int)(contentW * 0.30f)));
        var rightX = margin + leftW + gap;
        var rightW = contentW - leftW - gap;

        Place(panel2, margin, bodyY, leftW, bodyH);

        var upperH = Math.Max(180, (int)(bodyH * 0.48f));
        Place(groupBox3, 0, 0, leftW, upperH);
        Place(groupBox7, 0, upperH + gap, leftW, bodyH - upperH - gap);

        var leftInner = leftW - 4;
        var leftBtnW = Math.Max(60, (leftInner - 24) / 2);
        Place(btnAddToAutoParty, 8, 5, leftBtnW, 24);
        Place(btnRemoveFromAutoParty, leftInner - 8 - leftBtnW, 5, leftBtnW, 24);
        Place(buttonCommandPlayerAdd, 8, 5, leftBtnW, 24);
        Place(buttonCommandPlayerRemove, leftInner - 8 - leftBtnW, 5, leftBtnW, 24);

        if (listAutoParty.Columns.Count > 0)
            listAutoParty.Columns[0].Width = Math.Max(100, leftInner - 10);
        if (listCommandPlayers.Columns.Count > 0)
            listCommandPlayers.Columns[0].Width = Math.Max(100, leftInner - 10);

        // ---- Auto party settings card --------------------------------------
        Place(groupBox2, rightX, bodyY, rightW, bodyH);

        var innerW = rightW - 32;
        var colW = (innerW - 24) / 2;
        var rightColX = 16 + colW + 24;

        Place(checkAcceptAll, 16, 34, colW, 28);
        Place(checkAcceptFromList, 16, 68, colW, 28);
        Place(checkAcceptIfBotStopped, 16, 102, colW, 28);
        Place(checkAcceptAtTrainingPlace, 16, 136, colW, 28);

        Place(checkInviteAll, rightColX, 34, colW, 28);
        Place(checkInviteFromList, rightColX, 68, colW, 28);
        Place(checkBoxFollowMaster, rightColX, 102, colW, 28);

        Place(separator10, 16 + colW + 11, 30, 2, 140);
        Place(separator1, 16, 174, innerW, 10);

        Place(checkBoxLeaveIfMasterNot, 16, 186, 300, 28);
        Place(textBoxLeaveIfMasterNotName, 328, 189, 220, 25);

        Place(checkBoxListenMasterCommands, 16, 224, colW, 28);
        Place(checkBoxListenCommandsOnlyList, 16, 256, colW, 28);

        Place(separator2, 16, 292, innerW, 10);
        PlaceLabel(labelCommandsInfo, 16, 305, innerW, 88, ContentAlignment.MiddleLeft);
        PlaceLabel(label2, 16, bodyH - 34, innerW, 22, ContentAlignment.MiddleLeft);
    }

    private void LayoutPartyMatching(int width, int height)
    {
        const int margin = 14;
        const int gap = 12;
        const int footerH = 46;

        var contentW = width - margin * 2;
        var topH = topPartyPanel.Height > 60 ? 170 : 48;

        Place(topPartyPanel, margin, margin, contentW, topH);

        Place(label3, 16, 15, 45, 20);
        Place(tbPartySearchName, 63, 13, 150, 23);
        Place(label4, 224, 15, 50, 20);
        Place(cbPartySearchPurpose, 276, 13, 140, 23);
        Place(label5, 428, 15, 55, 20);
        Place(nudPartySearchMin, 485, 13, 85, 25);
        PlaceLabel(label6, 574, 15, 18, 20, ContentAlignment.MiddleCenter);
        Place(nudPartySearchMax, 596, 13, 85, 25);
        Place(btnPartySearch, 694, 14, 90, 23);
        Place(btnPartyRefresh, 792, 14, 90, 23);
        Place(buttonAutoJoinConfig, contentW - 16 - 28, 12, 28, 25);

        if (topH > 60)
        {
            Place(separator11, 16, 48, contentW - 32, 10);
            Place(checkBoxJoinByName, 16, 60, 220, 28);
            Place(textBoxJoinByName, 250, 62, 260, 23);
            Place(checkBoxJoinByTitle, 16, 94, 220, 28);
            Place(textBoxJoinByTitle, 250, 96, 260, 23);
            Place(buttonConfirmJoinConfig, contentW - 16 - 110, 128, 110, 26);
        }

        var listY = margin + topH + gap;
        var listH = height - listY - margin - footerH - gap;
        Place(lvPartyMatching, margin, listY, contentW, Math.Max(100, listH));

        Place(bottomPartyPanel, margin, height - margin - footerH, contentW, footerH);

        Place(btnJoinFormedParty, 12, 10, 110, 26);
        Place(btnWhisperPartyMaster, 128, 10, 80, 26);
        Place(btnAutoMatchParty, 214, 10, 130, 26);

        var navCenter = contentW / 2;
        Place(btnPrev, navCenter - 80, 11, 24, 24);
        PlaceLabel(lbl_partyPageRange, navCenter - 48, 10, 70, 24, ContentAlignment.MiddleCenter);
        Place(btnNext, navCenter + 56, 11, 24, 24);

        Place(btnPartyMatchDeleteEntry, contentW - 12 - 90, 10, 90, 26);
        Place(btnPartyMatchChangeEntry, contentW - 12 - 90 - 8 - 90, 10, 90, 26);
        Place(btnPartyMatchForm, contentW - 12 - 90 - 8 - 90 - 8 - 110, 10, 110, 26);

        if (bottomPartyPanel.Height > 0)
        {
            btnJoinFormedParty.Height = 26;
            btnWhisperPartyMaster.Height = 26;
            btnAutoMatchParty.Height = 26;
        }

        chPartyMatchNo.Width = 60;
        chPartyMatchRace.Width = 80;
        chPartyMatchName.Width = 170;
        chPartyMatchPurporse.Width = 130;
        chPartyMatchMember.Width = 100;
        chPartyMatchRange.Width = 80;
        chPartyMatchTitle.Width =
            Math.Max(220, contentW - 2 - 60 - 80 - 170 - 130 - 100 - 80);
    }

    private void LayoutBuffing(int width, int height)
    {
        const int margin = 14;
        const int gap = 12;

        var contentW = width - margin * 2;
        var totalH = height - margin * 2;
        var leftW = 330;
        var middleW = 310;
        var rightW = contentW - leftW - middleW - gap * 2;
        var middleX = margin + leftW + gap;

        Place(groupBox1, margin, margin, leftW, totalH);
        Place(groupBox5, middleX + middleW + gap, margin, rightW, totalH);

        var upperH = Math.Max(180, (int)(totalH * 0.42f));
        Place(groupBox6, middleX, margin, middleW, upperH);
        Place(groupBox4, middleX, margin + upperH + gap, middleW, totalH - upperH - gap);

        separator7.Visible = false;
        separator8.Visible = false;
        separator9.Visible = false;

        var middleInner = middleW - 4;
        var halfButtonW = Math.Max(60, (middleInner - 24) / 2);

        listViewGroups.Height = Math.Max(60, upperH - 24 - 38);
        Place(buttonAddGroup, 8, upperH - 32, halfButtonW, 24);
        Place(buttonRemoveGroup, middleInner - 8 - halfButtonW, upperH - 32, halfButtonW, 24);

        var memberH = totalH - upperH - gap;
        listViewPartyMembers.Height = Math.Max(60, memberH - 24 - 76);
        Place(buttonRemoveCharFromBuffing, middleInner - 34, memberH - 72, 26, 26);
        Place(btnAddBuffToMember, 8, memberH - 38, halfButtonW, 24);
        Place(btnRemoveBuffFromMember, middleInner - 8 - halfButtonW, memberH - 38, halfButtonW, 24);

        columnName.Width = Math.Max(120, leftW - 100);
        columnLimit.Width = 80;
        columnHeaderGroupName.Width = Math.Max(100, middleInner - 70);
        columnHeaderMembersCount.Width = 60;
        chPlayerName.Width = Math.Max(120, middleInner - 70);
        chPlayerLevel.Width = 60;
        columnHeader1.Width = Math.Max(120, rightW - 12);
    }

    /// <summary>
    ///     Localizes the non-control texts (list column headers and context
    ///     menus) and rebuilds the purpose filter with translated captions.
    /// </summary>
    private void ApplyPartyLocalization()
    {
        if (IsDisposed)
            return;

        colMemberName.Text = T("RSBot.Party.Columns.MemberName", "Name");
        colLevel.Text = T("RSBot.Party.Columns.Level", "Level");
        colGuild.Text = T("RSBot.Party.Columns.Guild", "Guild");
        colMasteries.Text = T("RSBot.Party.Columns.Masteries", "Masteries");
        colLocation.Text = T("RSBot.Party.Columns.Location", "Location");

        chPartyMatchNo.Text = T("RSBot.Party.Columns.No", "No");
        chPartyMatchRace.Text = T("RSBot.Party.Columns.Race", "Race");
        chPartyMatchName.Text = T("RSBot.Party.Columns.Name", "Name");
        chPartyMatchTitle.Text = T("RSBot.Party.Columns.Title", "Title");
        chPartyMatchPurporse.Text = T("RSBot.Party.Columns.Purpose", "Purpose");
        chPartyMatchMember.Text = T("RSBot.Party.Columns.Member", "Members");
        chPartyMatchRange.Text = T("RSBot.Party.Columns.Range", "Level");

        chPlayerName.Text = T("RSBot.Party.Columns.Name", "Name");
        chPlayerLevel.Text = T("RSBot.Party.Columns.Level", "Level");
        columnHeaderGroupName.Text = T("RSBot.Party.Columns.GroupName", "Group Name");
        columnHeaderMembersCount.Text = T("RSBot.Party.Columns.Members", "Members");
        columnName.Text = T("RSBot.Party.Columns.Name", "Name");
        columnLimit.Text = T("RSBot.Party.Columns.Limit", "Limit");
        columnHeader1.Text = T("RSBot.Party.Columns.Name", "Name");

        menuBanish.Text = T("RSBot.Party.ContextMenu.Banish", "Banish");
        menuLeave.Text = T("RSBot.Party.ContextMenu.Leave", "Leave");
        menuItemAddToBuffing.Text = T("RSBot.Party.ContextMenu.AddToBuffing", "Add to buffing");
        menuItemRefreshBuffs.Text = T("RSBot.Party.ContextMenu.RefreshBuffs", "Refresh buffs");

        var selectedPurpose = cbPartySearchPurpose.SelectedIndex;
        cbPartySearchPurpose.BeginUpdate();
        cbPartySearchPurpose.Items.Clear();
        cbPartySearchPurpose.Items.AddRange(new object[]
        {
            T("RSBot.Party.Purpose.All", "All"),
            T("RSBot.Party.Purpose.Hunting", "Hunting"),
            T("RSBot.Party.Purpose.Quest", "Quest"),
            T("RSBot.Party.Purpose.Trade", "Trade Union"),
            T("RSBot.Party.Purpose.Thief", "Thief Union")
        });
        cbPartySearchPurpose.EndUpdate();
        cbPartySearchPurpose.SelectedIndex = selectedPurpose >= 0 && selectedPurpose < 5 ? selectedPurpose : 0;

        if (labelCommandsInfo.Font.Name != "Segoe UI")
            labelCommandsInfo.Font = new Font("Segoe UI", 9F);

        if (label2.Font.Name != "Segoe UI")
            label2.Font = new Font("Segoe UI", 9F);
    }

    private static string GetPurposeText(PartyPurpose purpose)
    {
        switch (purpose)
        {
            case PartyPurpose.Hunting:
                return T("RSBot.Party.Purpose.Hunting", "Hunting");
            case PartyPurpose.Quest:
                return T("RSBot.Party.Purpose.Quest", "Quest");
            case PartyPurpose.Trade:
                return T("RSBot.Party.Purpose.Trade", "Trade");
            case PartyPurpose.Thief:
                return T("RSBot.Party.Purpose.Thief", "Thief");
            default:
                return purpose.ToString();
        }
    }

    private static string GetRaceText(ObjectCountry country)
    {
        switch (country)
        {
            case ObjectCountry.Chinese:
                return T("RSBot.Party.Race.Chinese", "Chinese");
            case ObjectCountry.Europe:
                return T("RSBot.Party.Race.Europe", "Europe");
            case ObjectCountry.Islam:
                return T("RSBot.Party.Race.Islam", "Islam");
            default:
                return T("RSBot.Party.Race.Unassigned", "Unassigned");
        }
    }

    private static PartyPurpose? GetPurposeFromFilterIndex(int index)
    {
        switch (index)
        {
            case 1:
                return PartyPurpose.Hunting;
            case 2:
                return PartyPurpose.Quest;
            case 3:
                return PartyPurpose.Trade;
            case 4:
                return PartyPurpose.Thief;
            default:
                return null;
        }
    }

    /// <summary>
    ///     Subscribes the events.
    /// </summary>
    private void SubscribeEvents()
    {
        EventManager.SubscribeEvent("OnLoadCharacter", OnLoadCharacter);
        EventManager.SubscribeEvent("OnCreatePartyEntry", OnCreatePartyEntry);
        EventManager.SubscribeEvent("OnChangePartyEntry", OnChangePartyEntry);
        EventManager.SubscribeEvent("OnDeletePartyEntry", OnDeletePartyEntry);
        EventManager.SubscribeEvent("OnPartyData", OnPartyData);
        EventManager.SubscribeEvent("OnPartyMemberJoin", new Action<PartyMember>(OnPartyMemberJoin));
        EventManager.SubscribeEvent("OnPartyMemberLeave", new Action<PartyMember>(OnPartyMemberLeave));
        EventManager.SubscribeEvent("OnPartyMemberBanned", new Action<PartyMember>(OnPartyMemberBanned));
        EventManager.SubscribeEvent("OnPartyMemberUpdate", new Action<PartyMember>(OnPartyMemberUpdate));
        EventManager.SubscribeEvent("OnPartyDismiss", OnPartyDismiss);
    }

    /// <summary>
    ///     Add new party member
    /// </summary>
    /// <param name="member"></param>
    public void AddNewPartyMember(PartyMember member)
    {
        var viewItem = listParty.Items.Add(member.Name, member.Name, 0);
        viewItem.UseItemStyleForSubItems = false;
        viewItem.Tag = member;

        viewItem.SubItems.Add(member.Level.ToString());
        if (string.IsNullOrWhiteSpace(member.Guild))
            viewItem.SubItems.Add(_noGuildText).ForeColor = Color.DarkGray;
        else
            viewItem.SubItems.Add(member.Guild);

        var mastery1 = Game.ReferenceManager.GetRefSkillMastery(member.MasteryId1);
        var mastery2 = Game.ReferenceManager.GetRefSkillMastery(member.MasteryId2);
        var location = Game.ReferenceManager.GetTranslation(member.Position.Region.ToString());

        var masteryInfo = _none;
        if (mastery1 != null)
            masteryInfo = mastery1.Name;
        if (mastery2 != null)
            masteryInfo += $", {mastery2.Name}";

        viewItem.SubItems.Add(masteryInfo);
        viewItem.SubItems.Add(location);
    }

    /// <summary>
    ///     Load the buffing groups
    /// </summary>
    /// <returns>The groups</returns>
    private string[] LoadBuffingGroups()
    {
        return PlayerConfig.GetArray<string>("RSBot.Party.Buffing.Groups");
    }

    /// <summary>
    ///     Save the buffing groups
    /// </summary>
    private void SaveBuffingGroups()
    {
        PlayerConfig.SetArray("RSBot.Party.Buffing.Groups",
            listViewGroups.Items.Cast<ListViewItem>().Select(p => p.Text));
    }

    /// <summary>
    ///     Get party buffing members from the config
    /// </summary>
    /// <returns>Configured buffed party members</returns>
    private void LoadBuffingMembers()
    {
        var buffingMembers = new List<BuffingPartyMember>();

        var settings = PlayerConfig.Get("RSBot.Party.Buffing", string.Empty);
        var collection = settings.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);

        foreach (var item in collection)
            buffingMembers.Add(new BuffingPartyMember(item));

        _buffings = buffingMembers;
    }

    /// <summary>
    ///     Saves the buffing party members
    /// </summary>
    /// <param name="members"></param>
    private void SaveBuffingPartyMembers()
    {
        var stringBuilder = new StringBuilder();
        foreach (var item in _buffings)
            stringBuilder.Append(item.Serialize());

        PlayerConfig.Set("RSBot.Party.Buffing", stringBuilder.ToString());
        PlayerConfig.Save();

        EventManager.FireEvent("OnPartyBuffSettingsChanged");
    }

    /// <summary>
    ///     Saves the automatic party player list.
    /// </summary>
    private void SaveAutoPartyPlayerList()
    {
        PlayerConfig.SetArray("RSBot.Party.AutoPartyList",
            listAutoParty.Items.OfType<ListViewItem>().Select(p => p.Text).ToArray());

        Bundle.Container.Refresh();
    }

    /// <summary>
    ///     Saves the automatic party player list.
    /// </summary>
    private void SaveCommandPlayersList()
    {
        PlayerConfig.SetArray("RSBot.Party.Commands.PlayersList",
            listCommandPlayers.Items.OfType<ListViewItem>().Select(p => p.Text).ToArray());

        Bundle.Container.Refresh();
    }

    /// <summary>
    ///     Refresh the buffing group members
    /// </summary>
    private void RefreshGroupMembers()
    {
        listViewPartyMembers.Items.Clear();

        foreach (ListViewItem itemGroup in listViewGroups.Items)
        {
            var members = _buffings.FindAll(p => p.Group == itemGroup.Text);

            itemGroup.SubItems[1].Text = members.Count.ToString();

            if (itemGroup.Text == _selectedBuffingGroup.Text)
                foreach (var member in members)
                {
                    var item = listViewPartyMembers.Items.Add(member.Name, member.Name, 0);
                    if (item.Index == 0)
                        item.Selected = true;
                }
        }

        LoadPartyBuffSkills();
    }

    /// <summary>
    ///     Requests the party list.
    /// </summary>
    private void RequestPartyList(byte page = 0)
    {
        Task.Run(() =>
        {
            lvPartyMatching.BeginUpdate();
            lvPartyMatching.Items.Clear();

            var listViewItems = new List<ListViewItem>();
            var currentPage = Bundle.Container.PartyMatching.RequestPartyList(page);

            var pageCount = Math.Max(0, (int)currentPage.PageCount);
            var safePage = pageCount > 0
                ? Math.Min(Math.Max(0, (int)currentPage.Page), pageCount - 1)
                : 0;

            btnPrev.Enabled = safePage > 0;
            btnNext.Enabled = pageCount > 0 && safePage < pageCount - 1;
            btnPrev.Tag = safePage - 1;
            btnNext.Tag = safePage + 1;

            lbl_partyPageRange.Tag = safePage;
            lbl_partyPageRange.Text = pageCount > 0
                ? $"{safePage + 1} / {pageCount}"
                : "0 / 0";

            foreach (var party in currentPage.Parties)
            {
                var existingEntry = listViewItems.Find(p => p.Name == party.Id.ToString());
                //For a self created party!
                if (existingEntry != null)
                    continue;

                var listItem = new ListViewItem { Text = party.Id.ToString(), Name = party.Id.ToString() };
                listItem.SubItems.Add(GetRaceText(party.Race));
                listItem.SubItems.Add(party.Leader);
                listItem.SubItems.Add(party.Title);
                listItem.SubItems.Add(GetPurposeText(party.Purpose));
                listItem.SubItems.Add(party.MemberCount.ToString("#/" + party.Settings.MaxMember));
                listItem.SubItems.Add(party.MinLevel + "~" + party.MaxLevel);
                listItem.Tag = party.Purpose;

                listItem.ToolTipText = party.Settings.ToString();
                if (party.Leader == Game.Player.Name ||
                    party.Leader == Game.Player.JobInformation.Name ||
                    Game.Party?.Leader?.Name == party.Leader)
                {
                    listItem.Font = new Font(Font, FontStyle.Bold);

                    listItem.BackColor = ControlPaint.Light(ColorScheme.BackColor, .15f);
                    listItem.Font = new Font(Font, FontStyle.Bold);

                    listViewItems.Insert(0, listItem);

                    continue;
                }

                listViewItems.Add(listItem);
            }

            foreach (var item in listViewItems)
                lvPartyMatching.Items.Add(item);

            lvPartyMatching.EndUpdate();
        });
    }

    /// <summary>
    ///     Loads the settings.
    /// </summary>
    private void LoadSettings()
    {
        _applySettings = false;

        Bundle.Container.Refresh();

        checkAutoExpAutoShare.Checked = PlayerConfig.Get<bool>("RSBot.Party.EXPAutoShare");
        checkAutoItemAutoShare.Checked = PlayerConfig.Get<bool>("RSBot.Party.ItemAutoShare");
        checkAutoAllowInvitations.Checked = PlayerConfig.Get<bool>("RSBot.Party.AllowInvitations");
        checkAcceptAtTrainingPlace.Checked = Bundle.Container.AutoParty.Config.OnlyAtTrainingPlace;
        checkInviteAll.Checked = Bundle.Container.AutoParty.Config.InviteAll;
        checkInviteFromList.Checked = Bundle.Container.AutoParty.Config.InviteFromList;
        checkAcceptAll.Checked = Bundle.Container.AutoParty.Config.AcceptAll;
        checkAcceptFromList.Checked = Bundle.Container.AutoParty.Config.AcceptFromList;

        checkBoxLeaveIfMasterNot.Checked = Bundle.Container.AutoParty.Config.LeaveIfMasterNot;
        textBoxLeaveIfMasterNotName.Text = Bundle.Container.AutoParty.Config.LeaveIfMasterNotName;
        textBoxLeaveIfMasterNotName.Enabled = !checkBoxLeaveIfMasterNot.Checked;
        checkBoxFollowMaster.Checked = PlayerConfig.Get("RSBot.Party.AlwaysFollowPartyMaster", false);

        checkAcceptIfBotStopped.Checked = Bundle.Container.AutoParty.Config.AcceptIfBotIsStopped;
        checkBoxListenMasterCommands.Checked = Bundle.Container.Commands.Config.ListenOnlyMaster;
        checkBoxListenCommandsOnlyList.Checked = Bundle.Container.Commands.Config.ListenFromList;

        var autoPartyList = PlayerConfig.GetArray<string>("RSBot.Party.AutoPartyList");
        foreach (var item in autoPartyList)
            listAutoParty.Items.Add(item);

        var playerList = PlayerConfig.GetArray<string>("RSBot.Party.Commands.PlayersList");
        foreach (var item in playerList)
            listCommandPlayers.Items.Add(item);

        _applySettings = true;
    }

    /// <summary>
    ///     This event will fire as soon as character loaded
    /// </summary>
    private void OnLoadCharacter()
    {
        listViewGroups.Items.Clear();

        var selectedGroup = PlayerConfig.Get("RSBot.Party.Buffing.SelectedGroup", "Default");

        LoadBuffingMembers();
        var groups = LoadBuffingGroups();
        if (groups.Length == 0)
        {
            var item = listViewGroups.Items.Add("Default", "Default", 0);
            item.SubItems.Add("0");
            item.Selected = true;
            _selectedBuffingGroup = item;

            SaveBuffingGroups();
        }
        else
        {
            foreach (var group in groups)
            {
                var item = listViewGroups.Items.Add(group, group, 0);
                item.SubItems.Add("0");

                if (group == selectedGroup)
                {
                    item.Selected = true;
                    _selectedBuffingGroup = item;
                }
            }
        }

        RefreshGroupMembers();
    }

    /// <summary>
    ///     Load party related buffs
    /// </summary>
    /// <summary>
    ///     Loads the skills.
    /// </summary>
    private void LoadPartyBuffSkills()
    {
        Log.Notify("Refreshing party buff skills");

        if (Game.Player == null)
            return;

        listPartyBuffSkills.BeginUpdate();
        listPartyBuffSkills.Items.Clear();
        listPartyBuffSkills.Groups.Clear();

        foreach (var mastery in Game.Player.Skills.Masteries)
        {
            var group = new ListViewGroup(Game.ReferenceManager.GetTranslation(mastery.Record.NameCode) + " (lv. " +
                                          mastery.Level + ")");
            group.Tag = mastery.Id;
            listPartyBuffSkills.Groups.Add(group);
        }

        var skills = Game.Player.Skills.KnownSkills
            .Where(s => s.Enabled && s.Record.TargetGroup_Party && !s.Record.TargetEtc_SelectDeadBody);

        foreach (var skill in skills)
        {
            if (skill.IsPassive)
                continue;

            if (checkHideLowerLevelSkills.Checked && skill.IsLowLevel())
                continue;

            var limit = 0;
            var paramIndex = skill.Record.Params.FindIndex(p => p == 1819175795);

            if (paramIndex != -1)
                limit = skill.Record.Params[paramIndex + 3];

            var count = _buffings.Count(p => p.Group == _selectedBuffingGroup.Text && p.Buffs.Any(v => v == skill.Id));

            var item = new ListViewItem($"{skill.Record.GetRealName()}");
            item.Tag = skill;

            var subItem = item.SubItems.Add(limit != 0 ? $"{limit - count}" : "No limit");
            item.UseItemStyleForSubItems = false;

            if (limit == 0)
                subItem.ForeColor = Color.DarkGreen;
            else
                subItem.ForeColor = Color.DarkRed;

            subItem.Font = new Font("Segoe UI", 9f, FontStyle.Bold);

            foreach (var group in listPartyBuffSkills.Groups.Cast<ListViewGroup>()
                         .Where(group => Convert.ToInt32(group.Tag) == skill.Record.ReqCommon_Mastery1))
                item.Group = group;

            listPartyBuffSkills.Items.Add(item);

            item.LoadSkillImageAsync();
        }

        listPartyBuffSkills.EndUpdate();
    }

    /// <summary>
    ///     Displays the party members.
    /// </summary>
    public void OnPartyData()
    {
        listParty.BeginUpdate();
        try
        {
            listParty.Items.Clear();

            if (Game.Party.Members == null)
            {
                listParty.EndUpdate();
                OnPartyDismiss();
                return;
            }

            foreach (var member in Game.Party.Members.FindAll(p =>
                         p.Name != Game.Player.Name || p.Name != Game.Player.JobInformation.Name))
                AddNewPartyMember(member);

            menuBanish.Enabled = Game.Party.IsLeader;

            //Update other UI components
            lblLeader.Text = Game.Party.Leader.Name;
            btnJoinFormedParty.Enabled = false;
            btnPartyMatchForm.Enabled = false;

            _applySettings = false;
            checkCurrentAllowInvitations.Checked = Game.Party.Settings.AllowInvitation;
            checkCurrentAutoShareEXP.Checked = Game.Party.Settings.ExperienceAutoShare;
            checkCurrentAutoShareItems.Checked = Game.Party.Settings.ItemAutoShare;
            _applySettings = true;

            btnLeaveParty.Enabled = true;
            menuLeave.Enabled = true;
        }
        catch
        {
        }

        listParty.EndUpdate();
    }

    private void OnChangePartyEntry()
    {
        Log.NotifyLang("FormedPartyChanged", Bundle.Container.PartyMatching.Id);

        if (tabMain.SelectedTab == tpPartyMatching)
            RequestPartyList();
    }

    private void OnDeletePartyEntry()
    {
        if (tabMain.SelectedTab == tpPartyMatching &&
            lvPartyMatching.Items.Count > 0 &&
            lvPartyMatching.Items[0].Name == Bundle.Container.PartyMatching.Id.ToString())
            lvPartyMatching.Items.Remove(lvPartyMatching.Items[0]);

        Bundle.Container.PartyMatching.Id = 0;

        btnPartyMatchForm.Enabled = true;
        btnPartyMatchChangeEntry.Enabled = false;
        btnPartyMatchDeleteEntry.Enabled = false;
        btnJoinFormedParty.Enabled = true;
        grbAutoPartySettings.Enabled = true;

        if (Bundle.Container.PartyMatching.Config.AutoReform)
            Bundle.Container.PartyMatching.Create();
    }

    private void OnCreatePartyEntry()
    {
        Log.NotifyLang("FormedPartyEntry", Bundle.Container.PartyMatching.Id);

        btnPartyMatchChangeEntry.Enabled = true;
        btnPartyMatchDeleteEntry.Enabled = true;
        btnPartyMatchForm.Enabled = false;
        btnJoinFormedParty.Enabled = false;
        grbAutoPartySettings.Enabled = false;

        _applySettings = false;
        checkCurrentAllowInvitations.Checked = Game.Party.Settings.AllowInvitation;
        checkCurrentAutoShareEXP.Checked = Game.Party.Settings.ExperienceAutoShare;
        checkCurrentAutoShareItems.Checked = Game.Party.Settings.ItemAutoShare;
        _applySettings = true;

        if (tabMain.SelectedTab == tpPartyMatching)
            RequestPartyList();
    }

    private void OnPartyMemberJoin(PartyMember member)
    {
        Log.NotifyLang("UserJoinedParty", member.Name);
        AddNewPartyMember(member);
    }

    private void OnPartyMemberLeave(PartyMember member)
    {
        Log.NotifyLang("UserLeftParty", member.Name);

        listParty.Items.RemoveByKey(member.Name);
    }

    private void OnPartyMemberBanned(PartyMember member)
    {
        Log.NotifyLang("UserBannedParty", member.Name);

        if (member.Name != Game.Player.Name)
            listParty.Items.RemoveByKey(member.Name);
        else
            OnPartyDismiss();
    }

    /// <summary>
    ///     Called when [party member update].
    /// </summary>
    /// <param name="member">The member.</param>
    private void OnPartyMemberUpdate(PartyMember member)
    {
        if (!listParty.Items.ContainsKey(member.Name))
            return;

        var lvItem = listParty.Items[member.Name];

        lvItem.Text = member.Name;
        lvItem.SubItems[1].Text = member.Level.ToString();
        if (string.IsNullOrWhiteSpace(member.Guild))
        {
            lvItem.SubItems[2].Text = _noGuildText;
            lvItem.SubItems[2].ForeColor = Color.DarkGray;
        }
        else
        {
            lvItem.SubItems[2].Text = member.Guild;
            lvItem.SubItems[2].ResetStyle();
        }

        var mastery1 = Game.ReferenceManager.GetRefSkillMastery(member.MasteryId1);
        var mastery2 = Game.ReferenceManager.GetRefSkillMastery(member.MasteryId2);
        var location = Game.ReferenceManager.GetTranslation(member.Position.Region.ToString());

        var masteryInfo = _none;
        if (mastery1 != null)
            masteryInfo = mastery1.Name;
        if (mastery2 != null)
            masteryInfo += $", {mastery2.Name}";

        lvItem.SubItems[3].Text = masteryInfo;
        lvItem.SubItems[4].Text = location;
    }

    /// <summary>
    ///     Clear party
    /// </summary>
    public void OnPartyDismiss()
    {
        Bundle.Container.PartyMatching.HasMatchingEntry = false;
        btnLeaveParty.Enabled = false;
        menuLeave.Enabled = false;
        lblLeader.Text = LanguageManager.GetLang("NotInParty");
        listParty.Items.Clear();
        Log.NotifyLang("PartyDismissed");
        OnDeletePartyEntry();
    }

    /// <summary>
    ///     Handles the Click event of the btnLeaveParty control.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="System.EventArgs" /> instance containing the event data.</param>
    private void btnLeaveParty_Click(object sender, EventArgs e)
    {
        Game.Party.Leave();
    }

    /// <summary>
    ///     Handles the CheckedChanged event of the checkSettings control.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="System.EventArgs" /> instance containing the event data.</param>
    private void checkSettings_CheckedChanged(object sender, EventArgs e)
    {
        if (!_applySettings)
            return;

        PlayerConfig.Set("RSBot.Party.EXPAutoShare", checkAutoExpAutoShare.Checked);
        PlayerConfig.Set("RSBot.Party.ItemAutoShare", checkAutoItemAutoShare.Checked);
        PlayerConfig.Set("RSBot.Party.AllowInvitations", checkAutoAllowInvitations.Checked);

        Bundle.Container.AutoParty.Refresh();
    }

    /// <summary>
    ///     Handles the Click event of the menuBanish control.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="System.EventArgs" /> instance containing the event data.</param>
    private void menuBanish_Click(object sender, EventArgs e)
    {
        if (listParty.SelectedItems.Count == 1 && Game.Party.IsLeader)
            Game.Party.GetMemberByName(listParty.SelectedItems[0].Text)?.Banish();
    }

    /// <summary>
    ///     Handles the Click event of the btnAddToAutoParty control.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="System.EventArgs" /> instance containing the event data.</param>
    private void btnAddToAutoParty_Click(object sender, EventArgs e)
    {
        var dialog = new InputDialog(
            "Input",
            LanguageManager.GetLang("CharName"),
            LanguageManager.GetLang("EnterCharNameForPartyList"));

        if (dialog.ShowDialog(this) != DialogResult.OK)
            return;

        listAutoParty.Items.Add(dialog.Value.ToString());
        SaveAutoPartyPlayerList();
    }

    /// <summary>
    ///     Handles the Click event of the btnRemoveFromAutoParty control.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="System.EventArgs" /> instance containing the event data.</param>
    private void btnRemoveFromAutoParty_Click(object sender, EventArgs e)
    {
        if (listAutoParty.SelectedIndices.Count == 0)
            return;

        listAutoParty.Items.RemoveAt(listAutoParty.SelectedIndices[0]);

        SaveAutoPartyPlayerList();
    }

    /// <summary>
    ///     Handles the CheckedChanged event of the checkAutoPartySetting control.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="System.EventArgs" /> instance containing the event data.</param>
    private void checkAutoPartySetting_CheckedChanged(object sender, EventArgs e)
    {
        if (!_applySettings)
            return;

        PlayerConfig.Set("RSBot.Party.AcceptAll", checkAcceptAll.Checked);
        PlayerConfig.Set("RSBot.Party.AcceptList", checkAcceptFromList.Checked);
        PlayerConfig.Set("RSBot.Party.InviteAll", checkInviteAll.Checked);
        PlayerConfig.Set("RSBot.Party.InviteList", checkInviteFromList.Checked);
        PlayerConfig.Set("RSBot.Party.AtTrainingPlace", checkAcceptAtTrainingPlace.Checked);
        PlayerConfig.Set("RSBot.Party.AcceptIfBotStopped", checkAcceptIfBotStopped.Checked);
        PlayerConfig.Set("RSBot.Party.LeaveIfMasterNot", checkBoxLeaveIfMasterNot.Checked);
        PlayerConfig.Set("RSBot.Party.LeaveIfMasterNotName", textBoxLeaveIfMasterNotName.Text);

        textBoxLeaveIfMasterNotName.Enabled = !checkBoxLeaveIfMasterNot.Checked;

        PlayerConfig.Set("RSBot.Party.Commands.ListenFromMaster", checkBoxListenMasterCommands.Checked);
        PlayerConfig.Set("RSBot.Party.Commands.ListenOnlyList", checkBoxListenCommandsOnlyList.Checked);

        Bundle.Container.Refresh();
    }

    /// <summary>
    ///     Handles the Click event of the ctxJoinParty control.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="System.EventArgs" /> instance containing the event data.</param>
    private void btnJoinFormedParty_Click(object sender, EventArgs e)
    {
        if (lvPartyMatching.SelectedItems.Count != 1)
            return;

        var partyNumber = Convert.ToUInt32(lvPartyMatching.SelectedItems[0].Text);

        Log.NotifyLang("JoinFormedParty", partyNumber);

        Task.Run(() =>
        {
            btnJoinFormedParty.Enabled = false;
            btnJoinFormedParty.Text = LanguageManager.GetLang("Joining");

            var joiningResult = Bundle.Container.PartyMatching.Join(partyNumber);
            if (joiningResult)
                RequestPartyList();

            btnJoinFormedParty.Text = LanguageManager.GetLang("JoinParty");
            btnJoinFormedParty.Enabled = !joiningResult;
        });
    }

    /// <summary>
    ///     Handles the SelectedIndexChanged event of the tpPartyMatching page.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="System.EventArgs" /> instance containing the event data.</param>
    private void tabMain_SelectedIndexChanged(object sender, EventArgs e)
    {
        ApplyPartyLayout();

        if (tabMain.SelectedTab == tpPartyMatching)
            RequestPartyList();
    }

    /// <summary>
    ///     Handles the Click event of the btnPartyMatchForm control.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
    private void btnPartyMatchForm_Click(object sender, EventArgs e)
    {
        var senderName = (sender as Button).Name;

        View.PartyWindow.Text =
            senderName == nameof(btnPartyMatchForm)
                ? LanguageManager.GetLang("FormingParty")
                : LanguageManager.GetLang("ChangeEntry");

        if (View.PartyWindow.ShowDialog(FindForm()) == DialogResult.OK)
        {
            if (senderName == nameof(btnPartyMatchForm))
                Bundle.Container.PartyMatching.Create();
            else
                Bundle.Container.PartyMatching.Change();
        }
    }

    private void PageNatigateBtn_Click(object sender, EventArgs e)
    {
        if (sender is not Button btn || btn.Tag == null)
            return;

        var page = Convert.ToInt32(btn.Tag);
        if (page < 0)
            return;

        btn.Enabled = false;

        RequestPartyList(Convert.ToByte(page));
    }

    private void btnPartyMatchDeleteEntry_Click(object sender, EventArgs e)
    {
        Bundle.Container.PartyMatching.Config.AutoReform = false;
        Bundle.Container.PartyMatching.Delete();
    }

    private void btnPartyRefresh_Click(object sender, EventArgs e)
    {
        var page = lbl_partyPageRange.Tag == null
            ? 0
            : Math.Max(0, Convert.ToInt32(lbl_partyPageRange.Tag));

        RequestPartyList(Convert.ToByte(page));
    }

    private void btnPartySearch_Click(object sender, EventArgs e)
    {
        var lvItems = lvPartyMatching.Items.Cast<ListViewItem>().ToList();

        var selectedPurpose = GetPurposeFromFilterIndex(cbPartySearchPurpose.SelectedIndex);
        if (selectedPurpose.HasValue)
            lvItems.RemoveAll(p => !(p.Tag is PartyPurpose purpose) || purpose != selectedPurpose.Value);

        if (!string.IsNullOrWhiteSpace(tbPartySearchName.Text))
            //No case sensitivity
            lvItems.RemoveAll(p =>
                !p.SubItems[2].Text.ToLowerInvariant().Contains(tbPartySearchName.Text.ToLowerInvariant()));

        if (nudPartySearchMin.Value > 1 || nudPartySearchMax.Value < 140)
            lvItems.RemoveAll(p => p.SubItems[6].Text != nudPartySearchMin.Value + "~" + nudPartySearchMax.Value);

        if (lvItems?.Count() > 0 && lvItems?.Count() != lvPartyMatching.Items.Count)
        {
            lvPartyMatching.Items.Clear();

            lvPartyMatching.Items.AddRange(lvItems.ToArray());
        }
    }

    private void checkHideLowerLevelSkills_CheckedChanged(object sender, EventArgs e)
    {
        LoadPartyBuffSkills();
        PlayerConfig.Set("RSBot.Party.Buffing.HideLowerLevelSkills", checkHideLowerLevelSkills.Checked);
    }

    private void listViewGroups_SelectedIndexChanged(object sender, EventArgs e)
    {
        if (listViewGroups.SelectedIndices.Count == 0 ||
            listViewGroups.SelectedIndices[0] == _selectedBuffingGroup.Index)
            return;

        _selectedBuffingGroup = listViewGroups.SelectedItems[0];

        PlayerConfig.Set("RSBot.Party.Buffing.SelectedGroup", _selectedBuffingGroup.Text);
        RefreshGroupMembers();
    }

    private void listViewPartyMembers_SelectedIndexChanged(object sender, EventArgs e)
    {
        if (listViewPartyMembers.SelectedItems.Count == 0)
            return;

        selectedMemberBuffs.Items.Clear();

        var name = listViewPartyMembers.SelectedItems[0].Text;
        var member = _buffings.Find(p => p.Name == name);
        if (member == null)
            return;

        foreach (var skillId in member.Buffs)
        {
            var listViewItemOfMainList = listPartyBuffSkills.Items.Cast<ListViewItem>()
                .FirstOrDefault(p => ((SkillInfo)p.Tag).Id == skillId);
            if (listViewItemOfMainList == null)
                continue;

            var clone = (ListViewItem)listViewItemOfMainList.Clone();
            clone.SubItems.RemoveAt(1);
            selectedMemberBuffs.Items.Add(clone);
        }
    }

    private void btnAddBuffToMember_Click(object sender, EventArgs e)
    {
        if (listViewPartyMembers.SelectedItems.Count == 0)
            return;

        if (listPartyBuffSkills.SelectedItems.Count == 0)
            return;

        var memberName = listViewPartyMembers.SelectedItems[0].Text;

        var buffingMember = _buffings.Find(p => p.Name == memberName);
        if (buffingMember == null)
            return;

        foreach (ListViewItem viewItem in listPartyBuffSkills.SelectedItems)
        {
            var skill = viewItem.Tag as SkillInfo;
            if (skill == null)
                continue;

            if (buffingMember.Buffs.Any(id => id == skill.Id ||
                                              (Game.ReferenceManager.SkillData.TryGetValue(id, out var refSkill) &&
                                               refSkill.Action_Overlap == skill.Record.Action_Overlap)))
                continue;

            buffingMember.Buffs.Add(skill.Id);

            var buffItem = (ListViewItem)viewItem.Clone();
            buffItem.SubItems.RemoveAt(1);
            selectedMemberBuffs.Items.Add(buffItem);

            var subItem = viewItem.SubItems[1];
            if (int.TryParse(subItem.Text, out var limit))
                subItem.Text = (limit - 1).ToString();
        }

        SaveBuffingPartyMembers();
    }

    private void btnRemoveBuffFromMember_Click(object sender, EventArgs e)
    {
        if (listViewPartyMembers.SelectedItems.Count == 0)
            return;

        if (selectedMemberBuffs.SelectedItems.Count == 0)
            return;

        var memberName = listViewPartyMembers.SelectedItems[0].Text;

        var buffingMember = _buffings.FirstOrDefault(p => p.Name == memberName);
        if (buffingMember == null)
            return;

        foreach (ListViewItem viewItem in selectedMemberBuffs.SelectedItems)
        {
            var skill = viewItem.Tag as SkillInfo;
            if (skill == null)
                continue;

            buffingMember.Buffs.Remove(skill.Id);
            viewItem.Remove();

            var mainItem = listPartyBuffSkills.Items.Cast<ListViewItem>().FirstOrDefault(p => p.Tag == skill);
            if (mainItem == null)
                continue;

            var subItem = mainItem.SubItems[1];
            if (int.TryParse(subItem.Text, out var limit))
                subItem.Text = (limit + 1).ToString();
        }

        SaveBuffingPartyMembers();
    }

    private void menuItemAddToBuffing_Click(object sender, EventArgs e)
    {
        if (listParty.SelectedItems.Count == 0)
            return;

        var partyMember = listParty.SelectedItems[0].Tag as PartyMember;
        if (partyMember == null)
            return;

        if (partyMember.Name == Game.Player.Name)
            return;

        var dialogTitle = LanguageManager.GetLang("SelectGroup", partyMember.Name);
        var dialogDesc = LanguageManager.GetLang("SelectGroupDesc");
        var dialog = new InputDialog(dialogTitle, dialogTitle, dialogDesc, InputDialog.InputType.Combobox);

        var groups = listViewGroups.Items.Cast<ListViewItem>()
            .Select(p => p.Text)
            .ToArray();

        dialog.Selector.Items.AddRange(groups);

        dialog.Selector.SelectedIndex = 0;

        if (dialog.ShowDialog(this) == DialogResult.OK)
        {
            var dialogValue = dialog.Value.ToString();

            if (_buffings.Any(p => p.Group == dialogValue && p.Name == partyMember.Name))
            {
                MessageBox.Show(LanguageManager.GetLang("MsgBoxGroupAlreadyIn", partyMember.Name));
                return;
            }

            _buffings.Add(new BuffingPartyMember
            {
                Name = partyMember.Name,
                Group = dialogValue
            });

            if (dialogValue == _selectedBuffingGroup.Text)
                RefreshGroupMembers();

            SaveBuffingPartyMembers();

            MessageBox.Show(LanguageManager.GetLang("SuccessAddedBuffing"));
        }
    }

    private void menuItemRefreshBuffs_Click(object sender, EventArgs e)
    {
        LoadPartyBuffSkills();
    }

    private void buttonAddGroup_Click(object sender, EventArgs e)
    {
        var title = LanguageManager.GetLang("CreateNewGroup");
        var desc = LanguageManager.GetLang("CreateNewGroupDesc");

        var dialog = new InputDialog(title, title, desc);
        if (dialog.ShowDialog(this) == DialogResult.OK)
        {
            var value = dialog.Value.ToString();
            var item = listViewGroups.Items.Add(value, value, 0);
            item.SubItems.Add("0");
            item.Selected = true;

            SaveBuffingGroups();
        }
    }

    private void buttonRemoveGroup_Click(object sender, EventArgs e)
    {
        if (listViewGroups.SelectedItems.Count == 0)
            return;

        var selectedItem = listViewGroups.SelectedItems[0];
        var group = selectedItem.Text;

        var count = _buffings.Count(p => p.Group == group);
        if (count == 0)
        {
            selectedItem.Remove();

            SaveBuffingGroups();

            return;
        }

        if (MessageBox.Show(this, LanguageManager.GetLang("GroupDeleteWarn", count),
                "Warning", MessageBoxButtons.YesNo) == DialogResult.Yes)
        {
            if (group != "Default")
                selectedItem.Remove();

            var affected = _buffings.RemoveAll(p => p.Group == group) > 0;

            SaveBuffingGroups();
            SaveBuffingPartyMembers();

            if (affected)
                LoadPartyBuffSkills();
        }
    }

    private void buttonRemoveCharFromBuffing_Click(object sender, EventArgs e)
    {
        if (listViewPartyMembers.SelectedItems.Count == 0)
            return;

        if (MessageBox.Show(this, LanguageManager.GetLang("GroupCharDeleteWarn"),
                "Warning", MessageBoxButtons.YesNo) == DialogResult.Yes)
        {
            var selectedItem = listViewPartyMembers.SelectedItems[0];
            var name = selectedItem.Text;

            var affected = _buffings.RemoveAll(p => p.Name == name);
            if (affected > 0)
            {
                selectedItem.Remove();
                RefreshGroupMembers();
                SaveBuffingPartyMembers();
                LoadPartyBuffSkills();
            }
        }
    }

    private void buttonCommandPlayerAdd_Click(object sender, EventArgs e)
    {
        var diag = new InputDialog(
            "Input",
            LanguageManager.GetLang("CharName"),
            LanguageManager.GetLang("EnterCharNameForCommandList"));

        if (diag.ShowDialog(this) != DialogResult.OK)
            return;

        listCommandPlayers.Items.Add(diag.Value.ToString());
        SaveCommandPlayersList();
    }

    private void buttonCommandPlayerRemove_Click(object sender, EventArgs e)
    {
        if (listCommandPlayers.SelectedIndices.Count == 0)
            return;

        listCommandPlayers.Items.RemoveAt(listCommandPlayers.SelectedIndices[0]);

        SaveCommandPlayersList();
    }

    private void buttonAutoJoinConfig_Click(object sender, EventArgs e)
    {
        checkBoxJoinByName.Checked = PlayerConfig.Get("RSBot.Party.AutoJoin.ByName", false);
        checkBoxJoinByTitle.Checked = PlayerConfig.Get("RSBot.Party.AutoJoin.ByTitle", false);
        textBoxJoinByName.Text = PlayerConfig.Get("RSBot.Party.AutoJoin.Name", string.Empty);
        textBoxJoinByTitle.Text = PlayerConfig.Get("RSBot.Party.AutoJoin.Title", string.Empty);
        topPartyPanel.Height = 162;

        buttonAutoJoinConfig.Color = ColorScheme.BackColor;
        ApplyPartyLayout();
    }

    private void buttonConfirmJoinConfig_Click(object sender, EventArgs e)
    {
        PlayerConfig.Set("RSBot.Party.AutoJoin.ByName", checkBoxJoinByName.Checked);
        PlayerConfig.Set("RSBot.Party.AutoJoin.ByTitle", checkBoxJoinByTitle.Checked);

        if (checkBoxJoinByName.Checked)
            PlayerConfig.Set("RSBot.Party.AutoJoin.Name", textBoxJoinByName.Text);

        if (checkBoxJoinByTitle.Checked)
            PlayerConfig.Set("RSBot.Party.AutoJoin.Title", textBoxJoinByTitle.Text);

        Bundle.Container.Refresh();

        buttonAutoJoinConfig.Color = Color.Transparent;
        topPartyPanel.Height = 47;
        ApplyPartyLayout();
    }

    private void checkBoxFollowMaster_CheckedChanged(object sender, EventArgs e)
    {
        PlayerConfig.Set("RSBot.Party.AlwaysFollowPartyMaster", checkBoxFollowMaster.Checked);

        Bundle.Container.Refresh();
    }

    /// <summary>
    ///     Occurs before Main form is displayed.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void Main_Load(object sender, EventArgs e)
    {
        LoadSettings();
        ApplyPartyLocalization();
        ApplyPartyLayout();
    }
}