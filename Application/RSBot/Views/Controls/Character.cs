using System;
using System.Windows.Forms;
using RSBot.Core;
using RSBot.Core.Components;
using RSBot.Core.Event;
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

        SubscribeEvents();
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
        if (InvokeRequired)
        {
            BeginInvoke(new Action<byte>(OnLevelUp), oldLevel);
            return;
        }

        try
        {
            lblLevel.Text = $"lv.{Game.Player?.Level ?? 0}";
        }
        catch (Exception ex)
        {
            Log.Debug($"[Character] Error on level up: {ex.Message}");
        }
    }

    private void OnInitialized()
    {
        if (InvokeRequired)
        {
            BeginInvoke(new Action(OnInitialized));
            return;
        }

        lblPlayerName.Text = LanguageManager.GetLang("LabelPlayerName");
    }

    private void OnUpdateSP()
    {
        if (InvokeRequired)
        {
            BeginInvoke(new Action(OnUpdateSP));
            return;
        }

        try
        {
            lblSP.Text = Game.Player?.SkillPoints.ToString("#,#0") ?? "0";
        }
        catch (Exception ex)
        {
            Log.Debug($"[Character] Error on update SP: {ex.Message}");
        }
    }

    private void OnUpdateGold()
    {
        if (InvokeRequired)
        {
            BeginInvoke(new Action(OnUpdateGold));
            return;
        }

        try
        {
            lblGold.Text = Game.Player?.Gold.ToString("#,#0") ?? "0";
        }
        catch (Exception ex)
        {
            Log.Debug($"[Character] Error on update Gold: {ex.Message}");
        }
    }

    /// <summary>
    ///     On Hp/MP update
    /// </summary>
    private void OnLoadCharacterStats()
    {
        if (InvokeRequired)
        {
            BeginInvoke(new Action(OnLoadCharacterStats));
            return;
        }

        try
        {
            if (Game.Player == null)
                return;

            lblInt.Text = Game.Player.Intelligence.ToString();
            lblStr.Text = Game.Player.Strength.ToString();

            if (Game.Player.MaximumHealth > 0)
            {
                progressHP.Maximum = Game.Player.MaximumHealth;
                progressHP.Value = Math.Clamp(Game.Player.Health, 0, progressHP.Maximum);
            }

            if (Game.Player.MaximumMana > 0)
            {
                progressMP.Maximum = Game.Player.MaximumMana;
                progressMP.Value = Math.Clamp(Game.Player.Mana, 0, progressMP.Maximum);
            }
        }
        catch (Exception ex)
        {
            Log.Debug($"[Character] Error on load stats: {ex.Message}");
        }
    }

    /// <summary>
    ///     On Exp update
    /// </summary>
    private void OnExpUpdate()
    {
        if (InvokeRequired)
        {
            BeginInvoke(new Action(OnExpUpdate));
            return;
        }

        try
        {
            if (Game.Player == null)
                return;

            var refLevel = Game.ReferenceManager?.GetRefLevel(Game.Player.Level);
            if (refLevel != null && refLevel.Exp_C > 0)
            {
                progressEXP.Maximum = refLevel.Exp_C;
                progressEXP.Value = Math.Clamp(Game.Player.Experience, 0, progressEXP.Maximum);
            }
            else
            {
                progressEXP.Maximum = 100;
                progressEXP.Value = 0;
            }
        }
        catch (Exception ex)
        {
            Log.Debug($"[Character] Error on exp update: {ex.Message}");
        }
    }

    /// <summary>
    ///     s the on load character.
    /// </summary>
    private void OnLoadCharacter()
    {
        if (InvokeRequired)
        {
            BeginInvoke(new Action(OnLoadCharacter));
            return;
        }

        try
        {
            if (Game.Player == null)
                return;

            lblPlayerName.Text = Game.Player.Name;

            OnLevelUp(Game.Player.Level);
            OnLoadCharacterStats();
            OnExpUpdate();
            OnUpdateSP();
            OnUpdateGold();
        }
        catch (Exception ex)
        {
            Log.Debug($"[Character] Error on load character: {ex.Message}");
        }
    }

    /// <summary>
    ///     Reset UI after character disconnect
    /// </summary>
    private void OnAgentServerDisconnected()
    {
        if (InvokeRequired)
        {
            BeginInvoke(new Action(OnAgentServerDisconnected));
            return;
        }

        lblPlayerName.Text = LanguageManager.GetLang("LabelPlayerName");
        lblLevel.Text = "0";
        lblStr.Text = "0";
        lblInt.Text = "0";
        lblGold.Text = "0";
        lblSP.Text = "0";
        progressHP.Maximum = 100;
        progressMP.Maximum = 100;
        progressEXP.Maximum = 100;
        progressHP.Value = 0;
        progressMP.Value = 0;
        progressEXP.Value = 0;
    }
}