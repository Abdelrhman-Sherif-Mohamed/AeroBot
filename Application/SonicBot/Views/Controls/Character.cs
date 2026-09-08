using System;
using System.Drawing;
using System.Windows.Forms;
using RSBot.Core;
using RSBot.Core.Components;
using RSBot.Core.Event;
using SDUI;
using SDUI.Controls;

namespace RSBot.Views.Controls;

public partial class Character : DoubleBufferedControl
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="Character" /> class.
    /// </summary>
    public Character()
    {
        InitializeComponent();
        RefreshTheme();
        SubscribeEvents();
    }

    public void RefreshTheme()
    {
        var isDark = ColorScheme.BackColor.IsDark();
        var subTextColor = isDark ? Color.FromArgb(148, 163, 184) : Color.FromArgb(100, 116, 139);

        lblPlayerName.ForeColor = isDark ? Color.FromArgb(56, 189, 248) : Color.FromArgb(2, 132, 199);
        lblLevel.ForeColor = isDark ? Color.FromArgb(250, 204, 21) : Color.FromArgb(217, 119, 6);

        label3.ForeColor = subTextColor; // Gold:
        label4.ForeColor = subTextColor; // SP:
        label9.ForeColor = subTextColor; // STR:
        label11.ForeColor = subTextColor; // INT:

        lblGold.ForeColor = isDark ? Color.FromArgb(251, 191, 36) : Color.FromArgb(180, 83, 9);
        lblSP.ForeColor = isDark ? Color.FromArgb(192, 132, 252) : Color.FromArgb(147, 51, 234);
        lblStr.ForeColor = isDark ? Color.FromArgb(248, 113, 113) : Color.FromArgb(220, 38, 38);
        lblInt.ForeColor = isDark ? Color.FromArgb(96, 165, 250) : Color.FromArgb(37, 99, 235);

        progressHP.Gradient = new Color[] { Color.FromArgb(185, 28, 28), Color.FromArgb(239, 68, 68) };
        progressMP.Gradient = new Color[] { Color.FromArgb(29, 78, 216), Color.FromArgb(59, 130, 246) };
        progressEXP.Gradient = new Color[] { Color.FromArgb(4, 120, 87), Color.FromArgb(16, 185, 129) };
    }

    /// <summary>
    ///     Subscribes the events.
    /// </summary>
    private void SubscribeEvents()
    {
        EventManager.SubscribeEvent("OnLoadCharacter", OnLoadCharacter);
        EventManager.SubscribeEvent("OnLoadCharacterStats", OnLoadCharacterStats);
        EventManager.SubscribeEvent("OnLevelUp", new Action<byte>(OnLevelUp));
        EventManager.SubscribeEvent("OnExpSpUpdate", OnExpUpdate);
        EventManager.SubscribeEvent("OnUpdateHPMP", OnLoadCharacterStats);
        EventManager.SubscribeEvent("OnUpdateGold", OnUpdateGold);
        EventManager.SubscribeEvent("OnUpdateSP", OnUpdateSP);
        EventManager.SubscribeEvent("OnAgentServerDisconnected", OnAgentServerDisconnected);
        EventManager.SubscribeEvent("OnInitialized", OnInitialized);
    }

    private void OnLevelUp(byte oldLevel)
    {
        lblLevel.Text = $"lv.{Game.Player.Level}";
    }

    public void Translate()
    {
        var gold = LanguageManager.GetLang("Gold");
        if (!string.IsNullOrEmpty(gold)) label3.Text = gold;

        var sp = LanguageManager.GetLang("SP");
        if (!string.IsNullOrEmpty(sp)) label4.Text = sp;

        var str = LanguageManager.GetLang("STR");
        if (!string.IsNullOrEmpty(str)) label9.Text = str;

        var intText = LanguageManager.GetLang("INT");
        if (!string.IsNullOrEmpty(intText)) label11.Text = intText;

        if (Game.Player == null)
        {
            var notInGame = LanguageManager.GetLang("LabelPlayerName");
            if (!string.IsNullOrEmpty(notInGame)) lblPlayerName.Text = notInGame;
        }
    }

    private void OnInitialized()
    {
        Translate();
    }

    private void OnUpdateSP()
    {
        lblSP.Text = Game.Player.SkillPoints.ToString("#,#0");
    }

    private void OnUpdateGold()
    {
        lblGold.Text = Game.Player.Gold.ToString("#,#0");
    }

    /// <summary>
    ///     On Hp/MP update
    /// </summary>
    private void OnLoadCharacterStats()
    {
        lblInt.Text = Game.Player.Intelligence.ToString();
        lblStr.Text = Game.Player.Strength.ToString();

        if (Game.Player.MaximumHealth == 0)
            return;

        if (Game.Player.MaximumMana == 0)
            return;

        progressHP.Maximum = Game.Player.MaximumHealth;
        progressMP.Maximum = Game.Player.MaximumMana;
        progressHP.Value = Game.Player.Health;
        progressMP.Value = Game.Player.Mana;
    }

    /// <summary>
    ///     On Exp update
    /// </summary>
    /// <exception cref="System.NotImplementedException"></exception>
    private void OnExpUpdate()
    {
        progressEXP.Value = Game.Player.Experience;
        progressEXP.Maximum = Game.ReferenceManager.GetRefLevel(Game.Player.Level).Exp_C;
    }

    /// <summary>
    ///     s the on load character.
    /// </summary>
    private void OnLoadCharacter()
    {
        lblPlayerName.Text = Game.Player.Name;

        OnLevelUp(Game.Player.Level);
        OnLoadCharacterStats();
        OnExpUpdate();
        OnUpdateSP();
        OnUpdateGold();
    }

    /// <summary>
    ///     Reset UI after character disconnect
    /// </summary>
    private void OnAgentServerDisconnected()
    {
        lblPlayerName.Text = LanguageManager.GetLang("LabelPlayerName");
        lblLevel.Text = "0";
        lblStr.Text = "0";
        lblInt.Text = "0";
        lblGold.Text = "0";
        lblSP.Text = "0";
        progressHP.Value = 0;
        progressMP.Value = 0;
        progressEXP.Value = 0;
        progressHP.Maximum = 0;
        progressMP.Maximum = 0;
        progressEXP.Maximum = 0;
    }
}
