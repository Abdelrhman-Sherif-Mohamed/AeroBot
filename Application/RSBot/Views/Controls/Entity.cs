using System;
using System.Windows.Forms;
using RSBot.Core;
using RSBot.Core.Components;
using RSBot.Core.Event;
using RSBot.Core.Extensions;
using RSBot.Core.Objects.Spawn;
using SDUI.Controls;

namespace RSBot.Views.Controls;

public partial class Entity : DoubleBufferedControl
{
    public Entity()
    {
        CheckForIllegalCrossThreadCalls = false;
        InitializeComponent();
        SubscribeEvents();
    }

    /// <summary>
    ///     Subscribes the events.
    /// </summary>
    private void SubscribeEvents()
    {
        EventManager.SubscribeEvent("OnSelectEntity", new Action<SpawnedBionic>(OnSelectEntity));
        EventManager.SubscribeEvent("OnDeselectEntity", OnDeselectEntity);
        EventManager.SubscribeEvent("OnUpdateEntityHp", new Action<SpawnedBionic>(OnUpdateEntityHp));
        EventManager.SubscribeEvent("OnKillSelectedEnemy", OnKillSelectedEnemy);
        EventManager.SubscribeEvent("OnAgentServerDisconnected", OnAgentServerDisconnected);
        EventManager.SubscribeEvent("OnInitialized", OnInitialized);
    }

    private void OnInitialized()
    {
        if (InvokeRequired)
        {
            BeginInvoke(new Action(OnInitialized));
            return;
        }

        lblEntityName.Text = LanguageManager.GetLang("LabelEntityName");
    }

    /// <summary>
    ///     Fired when an entity was selected
    /// </summary>
    private void OnSelectEntity(SpawnedBionic entity)
    {
        if (InvokeRequired)
        {
            BeginInvoke(new Action<SpawnedBionic>(OnSelectEntity), entity);
            return;
        }

        try
        {
            if (entity == null)
            {
                Clear();
                return;
            }

            lblType.Text = string.Empty;
            if (entity is SpawnedPlayer player)
                lblEntityName.Text = player.Name;
            else
                lblEntityName.Text = entity.Record?.GetRealName() ?? "Unknown";

            if (entity is SpawnedMonster monster)
            {
                progressHP.Maximum = Math.Max(1, monster.MaxHealth);
                progressHP.Value = Math.Clamp(monster.Health, 0, progressHP.Maximum);

                lblType.Text = monster.Rarity.GetName();
            }
            else
            {
                progressHP.Maximum = 100;
                progressHP.Value = 100;

                if (Game.Player?.State?.DialogState is { IsInDialog: true })
                    lblType.Text = "<in conversation>";
            }
        }
        catch (Exception ex)
        {
            Log.Debug($"[Entity] Error on select entity: {ex.Message}");
        }
    }

    /// <summary>
    ///     Core_s the on update selected entity hp.
    /// </summary>
    private void OnUpdateEntityHp(SpawnedBionic entity)
    {
        if (InvokeRequired)
        {
            BeginInvoke(new Action<SpawnedBionic>(OnUpdateEntityHp), entity);
            return;
        }

        try
        {
            if (entity == null || Game.SelectedEntity?.UniqueId != entity.UniqueId)
                return;

            if (!entity.HasHealth)
            {
                progressHP.Maximum = 100;
                progressHP.Value = 100;
                return;
            }

            if (entity is SpawnedMonster monster)
            {
                progressHP.Maximum = Math.Max(1, monster.MaxHealth);
                progressHP.Value = Math.Clamp(monster.Health, 0, progressHP.Maximum);
            }
        }
        catch (Exception ex)
        {
            Log.Debug($"[Entity] Error on update HP: {ex.Message}");
        }
    }

    /// <summary>
    ///     Fired when an entity was deselected
    /// </summary>
    private void OnDeselectEntity()
    {
        if (InvokeRequired)
        {
            BeginInvoke(new Action(OnDeselectEntity));
            return;
        }

        Clear();
    }

    /// <summary>
    ///     Fired when the selected entity was killed
    /// </summary>
    private void OnKillSelectedEnemy()
    {
        if (InvokeRequired)
        {
            BeginInvoke(new Action(OnKillSelectedEnemy));
            return;
        }

        Clear();
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

        Clear();
    }

    /// <summary>
    ///     Clear the control
    /// </summary>
    private void Clear()
    {
        if (InvokeRequired)
        {
            BeginInvoke(new Action(Clear));
            return;
        }

        lblEntityName.Text = LanguageManager.GetLang("LabelEntityName");
        progressHP.Maximum = 100;
        progressHP.Value = 0;
        lblType.Text = "";
    }
}