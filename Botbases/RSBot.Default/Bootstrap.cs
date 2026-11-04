using System;
using System.Windows.Forms;
using RSBot.Core;
using RSBot.Core.Components;
using RSBot.Core.Objects;
using RSBot.Core.Plugins;
using RSBot.Default.Bot;
using RSBot.Default.Bundle;
using RSBot.Default.Components;
using RSBot.Default.Subscriber;

namespace RSBot.Default;

public class Bootstrap : IBotbase
{
    public string Name => "RSBot.Default";

    public string DisplayName => "Training";

    public string TabText => DisplayName;

    public Area Area => Container.Bot.Area;

    /// <summary>
    ///     Ticks this instance. It's the botbase main-loop
    /// </summary>
    public void Tick()
    {
        try
        {
            if (!Kernel.Bot.Running)
                return;

            if (Game.Player == null)
                return;

            if (Game.Player.Exchanging)
                return;

            if (Game.Player.Untouchable)
                return;

            if (Game.Player.State == null || Game.Player.State.LifeState == LifeState.Dead)
                return;

            //Begin the loopback if needed
            var loopDist = Math.Max(80, (Container.Bot?.Area.Radius ?? 50) + 30);
            if (Container.Bot?.Area != null && Container.Bot.Area.Position.X != 0 && Container.Bot.Area.Position.DistanceToPlayer() > loopDist)
                Bundles.Loop.Start();

            if (Bundles.Loop.Running)
                return;

            //Nothing if in scroll state!
            if (Game.Player.State.ScrollState == ScrollState.NormalScroll ||
                Game.Player.State.ScrollState == ScrollState.ThiefScroll)
                return;

            Container.Bot.Tick();
        }
        catch (Exception ex)
        {
            Log.Fatal(ex);
        }
    }

    /// <summary>
    ///     Gets the view.
    /// </summary>
    /// <returns></returns>
    public Control View => Container.View;

    /// <summary>
    ///     Starts this instance.
    /// </summary>
    public void Start()
    {
        Container.Bot?.Reload();
        Bundles.Reload();

        // If no training area is configured yet, auto-set it to the player's current position!
        if ((Container.Bot?.Area.Position.X == 0 || Container.Bot?.Area.Position.Region.Id == 0) && Game.Player != null)
        {
            var pos = Game.Player.Position;
            PlayerConfig.Set("RSBot.Area.Region", pos.Region.Id);
            PlayerConfig.Set("RSBot.Area.X", pos.XOffset);
            PlayerConfig.Set("RSBot.Area.Y", pos.YOffset);
            PlayerConfig.Set("RSBot.Area.Z", pos.ZOffset);
            PlayerConfig.Set("RSBot.Area.Radius", PlayerConfig.Get("RSBot.Area.Radius", 50));
            PlayerConfig.Save();

            Container.Bot?.Reload();
            Bundles.Reload();
            Log.Notify($"[AeroBot] تم تعيين مركز منطقة التدريب تلقائياً على موقع الشخصية الحالي (X: {pos.X:0.0}, Y: {pos.Y:0.0})");
        }

        if (Kernel.Bot.Botbase.Area.Position.X == 0)
        {
            Log.WarnLang("ConfigureTrainingAreaBeforeStartBot");
            Kernel.Bot.Stop();
            return;
        }
    }

    /// <summary>
    ///     Stops this instance.
    /// </summary>
    public void Stop()
    {
        lock (Container.Lock)
        {
            if (Game.Player.InAction)
                SkillManager.CancelAction();

            Bundles.Stop();
        }
    }

    /// <summary>
    ///     Always initialize the botbase so other botbases can make use of its otherwise internal features.
    /// </summary>
    public void Register()
    {
        Container.Lock = new object();
        Container.Bot = new Botbase();

        //Bundles.Reload();

        BundleSubscriber.SubscribeEvents();
        ConfigSubscriber.SubscribeEvents();
        TeleportSubscriber.SubscribeEvents();

        ScriptManager.CommandHandlers.Add(new TrainingAreaScriptCommand());
        Log.Debug("[Training] Botbase registered to the kernel!");
    }

    /// <summary>
    ///     Translate the botbase plugin
    /// </summary>
    /// <param name="language">The language</param>
    public void Translate()
    {
        LanguageManager.Translate(View, Kernel.Language);
    }
}