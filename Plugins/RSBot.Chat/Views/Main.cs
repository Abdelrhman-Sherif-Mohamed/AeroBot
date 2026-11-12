using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using RSBot.Core;
using RSBot.Core.Event;
using RSBot.Core.Objects;
using SDUI;
using SDUI.Controls;
using Action = System.Action;

namespace RSBot.Chat.Views;

[ToolboxItem(false)]
public partial class Main : DoubleBufferedControl
{
    private const int MaxPendingLinesPerChannel = 500;

    private readonly object _pendingLock = new();
    private readonly Dictionary<System.Windows.Forms.RichTextBox, Queue<string>> _pendingMessages = new();
    private readonly Dictionary<System.Windows.Forms.Control, SDUI.Controls.TextBox> _buttonTargets;
    private readonly Dictionary<ChatType, System.Windows.Forms.RichTextBox> _outputByType;

    private readonly Action _enterGameHandler;
    private readonly Action _disconnectedHandler;
    private readonly Action _initializedHandler;
    private readonly Action<bool> _themeChangedHandler;
    private bool _eventsSubscribed;

    public Main()
    {
        InitializeComponent();

        _buttonTargets = new Dictionary<System.Windows.Forms.Control, SDUI.Controls.TextBox>
        {
            { btnSendAll, txtSendAll },
            { btnSendPrivate, textBoxPrivateMsg },
            { btnSendParty, txtSendParty },
            { btnSendGuild, txtSendGuild },
            { btnSendUnion, txtSendUnion },
            { btnSendAcademy, txtSendAcademy }
        };

        _outputByType = new Dictionary<ChatType, System.Windows.Forms.RichTextBox>
        {
            { ChatType.All, txtAll },
            { ChatType.AllGM, txtAll },
            { ChatType.Npc, txtAll },
            { ChatType.Private, txtPrivate },
            { ChatType.Party, txtParty },
            { ChatType.Guild, txtGuild },
            { ChatType.Union, txtUnion },
            { ChatType.Academy, txtAcademy },
            { ChatType.Global, txtGlobal },
            { ChatType.Notice, txtGlobal }
        };

        foreach (var rtb in GetOutputBoxes())
            rtb.HandleCreated += OnOutputBoxHandleCreated;

        _enterGameHandler = OnEnterGame;
        _disconnectedHandler = OnDisconnected;
        _initializedHandler = () => RunOnUi(ApplyLocalization);
        _themeChangedHandler = _ => RunOnUi(ApplyTheme);

        SubscribeEvents();
        ApplyTheme();
        ApplyLocalization();
    }

    private System.Windows.Forms.RichTextBox[] GetOutputBoxes()
    {
        return new[]
        {
            txtAll, txtPrivate, txtParty, txtGuild, txtUnion, txtAcademy, txtGlobal, txtStall, UniqueText
        };
    }

    private SDUI.Controls.Button[] GetSendButtons()
    {
        return new[]
        {
            btnSendAll, btnSendPrivate, btnSendParty, btnSendGuild, btnSendUnion, btnSendAcademy
        };
    }

    private System.Windows.Forms.Panel[] GetInputPanels()
    {
        return new[]
        {
            panelAllInput, panel1, panelPartyInput, panelGuildInput, panelUnionInput, panelAcademyInput,
            spacerAll, spacerPrivateLeft, spacerPrivateRight, spacerParty, spacerGuild, spacerUnion, spacerAcademy
        };
    }

    private TabPage[] GetTabPages()
    {
        return new[]
        {
            tabAll, tabPrivate, tabParty, tabGuild, tabUnion, tabAcademy, tabGlobal, tabStall, tabUnique
        };
    }

    public void ApplyTheme()
    {
        var backColor = ColorScheme.BackColor;
        var cardColor = ColorScheme.CardColor;
        var textColor = ColorScheme.ForeColor;
        var accentColor = ColorScheme.AccentColor;

        BackColor = backColor;
        ForeColor = textColor;
        tabMain.BackColor = backColor;

        foreach (var page in GetTabPages())
        {
            if (page == null) continue;
            page.BackColor = cardColor;
            page.ForeColor = textColor;
        }

        foreach (var panel in GetInputPanels())
        {
            if (panel == null) continue;
            panel.BackColor = cardColor;
        }

        foreach (var rtb in GetOutputBoxes())
        {
            if (rtb == null) continue;
            rtb.BackColor = cardColor;
            rtb.ForeColor = textColor;
        }

        foreach (var btn in GetSendButtons())
        {
            if (btn == null) continue;
            btn.BackColor = accentColor;
            btn.ForeColor = Color.White;
            btn.Invalidate();
        }
    }

    public void ApplyLocalization()
    {
        var isArabic = IsArabicLanguage();

        tabAll.Text = isArabic ? "الكل" : "All";
        tabPrivate.Text = isArabic ? "خاص" : "Private";
        tabParty.Text = isArabic ? "فريق" : "Party";
        tabGuild.Text = isArabic ? "عشيرة" : "Guild";
        tabUnion.Text = isArabic ? "اتحاد" : "Union";
        tabAcademy.Text = isArabic ? "أكاديمية" : "Academy";
        tabGlobal.Text = isArabic ? "عالمي / ملاحظة" : "Global / Notice";
        tabStall.Text = isArabic ? "دكان" : "Stall";
        tabUnique.Text = isArabic ? "اليونيك" : "Unique";

        var sendText = isArabic ? "إرسال" : "Send";
        foreach (var btn in GetSendButtons())
        {
            if (btn != null)
                btn.Text = sendText;
        }

        txtRecievePrivate.PlaceholderText = isArabic ? "اسم اللاعب..." : "Player name...";
        textBoxPrivateMsg.PlaceholderText = isArabic ? "اكتب رسالة خاصة..." : "Type private message...";

        var msgPlaceholder = isArabic ? "اكتب رسالتك واضغط Enter..." : "Type message and press Enter...";
        txtSendAll.PlaceholderText = msgPlaceholder;
        txtSendParty.PlaceholderText = msgPlaceholder;
        txtSendGuild.PlaceholderText = msgPlaceholder;
        txtSendUnion.PlaceholderText = msgPlaceholder;
        txtSendAcademy.PlaceholderText = msgPlaceholder;
    }

    protected override void OnHandleCreated(EventArgs e)
    {
        base.OnHandleCreated(e);
        ApplyTheme();
        ApplyLocalization();
        FlushAllPendingMessages();
    }

    protected override void OnVisibleChanged(EventArgs e)
    {
        base.OnVisibleChanged(e);
        if (!Visible || IsDisposed)
            return;

        ApplyTheme();
        ApplyLocalization();
        FlushAllPendingMessages();
    }

    private void OnSelectedTabChanged(object sender, EventArgs e)
    {
        var selectedPage = tabMain.SelectedTab;
        if (selectedPage == null)
            return;

        foreach (Control child in selectedPage.Controls)
        {
            if (child is System.Windows.Forms.RichTextBox rtb && rtb.IsHandleCreated)
                FlushPendingMessages(rtb);
        }
    }

    private void OnOutputBoxHandleCreated(object sender, EventArgs e)
    {
        if (sender is System.Windows.Forms.RichTextBox rtb)
            FlushPendingMessages(rtb);
    }

    private void SubscribeEvents()
    {
        if (_eventsSubscribed)
            return;

        _eventsSubscribed = true;
        EventManager.SubscribeEvent("OnEnterGame", _enterGameHandler);
        EventManager.SubscribeEvent("OnAgentServerDisconnected", _disconnectedHandler);
        EventManager.SubscribeEvent("OnInitialized", _initializedHandler);
        EventManager.SubscribeEvent("OnChangeTheme", _themeChangedHandler);
    }

    private void UnsubscribeEvents()
    {
        if (!_eventsSubscribed)
            return;

        _eventsSubscribed = false;
        EventManager.UnsubscribeEvent("OnEnterGame", _enterGameHandler);
        EventManager.UnsubscribeEvent("OnAgentServerDisconnected", _disconnectedHandler);
        EventManager.UnsubscribeEvent("OnInitialized", _initializedHandler);
        EventManager.UnsubscribeEvent("OnChangeTheme", _themeChangedHandler);
    }

    private void RunOnUi(Action action)
    {
        if (IsDisposed || Disposing || !IsHandleCreated)
            return;

        if (InvokeRequired)
        {
            try
            {
                BeginInvoke(action);
            }
            catch (ObjectDisposedException)
            {
            }
            catch (InvalidOperationException)
            {
            }
        }
        else
        {
            action();
        }
    }

    /// <summary>
    ///     Sends the chat message from the associated input box.
    /// </summary>
    private void SendChatMessage(Control sender)
    {
        if (sender?.Tag == null || !Enum.TryParse<ChatType>(sender.Tag.ToString(), out var chatType))
            return;

        var message = sender.Text?.Trim();
        if (string.IsNullOrEmpty(message))
            return;

        var receiver = txtRecievePrivate.Text?.Trim();
        var isPrivate = chatType == ChatType.Private;
        if (!Bundle.Chat.SendChatPacket(chatType, message, receiver, out var errorReason))
        {
            var isArabic = IsArabicLanguage();
            var errorText = errorReason switch
            {
                "NotInGame" => isArabic
                    ? "لا يمكن الإرسال قبل دخول الشخصية للعبة."
                    : "Cannot send chat until character enters the game.",
                "MissingReceiver" => isArabic
                    ? "اكتب اسم اللاعب المستلم أولاً في الخانة الجانبية."
                    : "Enter receiver character name first.",
                _ => null
            };

            if (!string.IsNullOrEmpty(errorText) && _outputByType.TryGetValue(chatType, out var targetBox))
                AppendFormattedLine(targetBox, $"[!] {errorText}");

            if (errorReason == "MissingReceiver")
                txtRecievePrivate.Focus();

            return;
        }

        if (isPrivate)
        {
            PlayerConfig.Set("RSBot.Chat.LastWhisper", receiver);
            var toPrefix = IsArabicLanguage() ? "إلى" : "To";
            AppendFormattedLine(txtPrivate, $"([{toPrefix} {receiver}]): {message}");
        }

        sender.Text = string.Empty;
    }

    /// <summary>
    ///     Appends an incoming chat message to the matching channel tab (and Stall tab for ChatType.Stall).
    /// </summary>
    public void AppendMessage(string message, string sender, ChatType type)
    {
        var formatted = $"({sender}): {message}";

        if (type == ChatType.Stall)
        {
            AppendFormattedLine(txtStall, formatted);
            return;
        }

        if (_outputByType.TryGetValue(type, out var targetBox))
            AppendFormattedLine(targetBox, formatted);
    }

    /// <summary>
    ///     Appends a unique spawn/kill notification safely to the Unique tab.
    /// </summary>
    public void AppendUniqueNotice(string notice)
    {
        if (string.IsNullOrWhiteSpace(notice))
            return;

        AppendFormattedLine(UniqueText, notice);
    }

    private void AppendFormattedLine(System.Windows.Forms.RichTextBox box, string line)
    {
        var timestamped = $"[{DateTime.Now:HH:mm:ss}]\t{line}{Environment.NewLine}";
        if (box == null || IsDisposed || Disposing)
            return;

        if (!IsHandleCreated || !box.IsHandleCreated)
        {
            EnqueuePendingLine(box, timestamped);
            return;
        }

        if (box.InvokeRequired)
        {
            try
            {
                box.BeginInvoke(new Action(() => WriteToOutputBox(box, timestamped)));
            }
            catch (ObjectDisposedException)
            {
            }
            catch (InvalidOperationException)
            {
                EnqueuePendingLine(box, timestamped);
            }

            return;
        }

        WriteToOutputBox(box, timestamped);
    }

    private void EnqueuePendingLine(System.Windows.Forms.RichTextBox box, string timestampedLine)
    {
        lock (_pendingLock)
        {
            if (!_pendingMessages.TryGetValue(box, out var queue))
            {
                queue = new Queue<string>();
                _pendingMessages[box] = queue;
            }

            while (queue.Count >= MaxPendingLinesPerChannel)
                queue.Dequeue();

            queue.Enqueue(timestampedLine);
        }
    }

    private void FlushAllPendingMessages()
    {
        foreach (var box in GetOutputBoxes())
        {
            if (box != null && box.IsHandleCreated)
                FlushPendingMessages(box);
        }
    }

    private void FlushPendingMessages(System.Windows.Forms.RichTextBox box)
    {
        if (box == null || box.IsDisposed || box.Disposing || !box.IsHandleCreated)
            return;

        List<string> drained = null;
        lock (_pendingLock)
        {
            if (_pendingMessages.TryGetValue(box, out var queue) && queue.Count > 0)
            {
                drained = new List<string>(queue);
                queue.Clear();
            }
        }

        if (drained == null || drained.Count == 0)
            return;

        if (box.InvokeRequired)
        {
            try
            {
                box.BeginInvoke(new Action(() =>
                {
                    if (box.IsDisposed || !box.IsHandleCreated)
                        return;

                    box.AppendText(string.Concat(drained));
                    box.ScrollToCaret();
                }));
            }
            catch
            {
            }

            return;
        }

        box.AppendText(string.Concat(drained));
        box.ScrollToCaret();
    }

    private static void WriteToOutputBox(System.Windows.Forms.RichTextBox box, string timestampedLine)
    {
        if (box.IsDisposed || box.Disposing || !box.IsHandleCreated)
            return;

        box.AppendText(timestampedLine);
        box.ScrollToCaret();
    }

    private void OnEnterGame()
    {
        Bundle.Chat.ClearPendingAcks();
        var lastWhisper = PlayerConfig.Get<string>("RSBot.Chat.LastWhisper");
        RunOnUi(() =>
        {
            if (!string.IsNullOrEmpty(lastWhisper))
                txtRecievePrivate.Text = lastWhisper;
        });
    }

    private void OnDisconnected()
    {
        Bundle.Chat.ClearPendingAcks();
    }

    private void PrivateTargetKeyDown(object sender, KeyEventArgs e)
    {
        if (e.KeyCode != Keys.Enter)
            return;

        e.Handled = true;
        e.SuppressKeyPress = true;
        textBoxPrivateMsg.Focus();
    }

    private void MessageInputKeyDown(object sender, KeyEventArgs e)
    {
        if (e.KeyCode != Keys.Enter)
            return;

        e.Handled = true;
        e.SuppressKeyPress = true;
        SendChatMessage((Control)sender);
    }

    private void SendButtonClick(object sender, EventArgs e)
    {
        if (sender is Control btn && _buttonTargets.TryGetValue(btn, out var textBox))
            SendChatMessage(textBox);
    }

    private static bool IsArabicLanguage()
    {
        return Kernel.Language != null && Kernel.Language.StartsWith("ar", StringComparison.OrdinalIgnoreCase);
    }
}
