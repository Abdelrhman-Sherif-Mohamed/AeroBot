using System.Threading;
using System.Threading.Tasks;
using RSBot.Core.Components;
using RSBot.Core.Event;
using RSBot.Core.Plugins;

namespace RSBot.Core;

public class Bot
{
    /// <summary>
    ///     Gets or sets a value indicating whether this <see cref="Bot" /> is running.
    /// </summary>
    /// <value>
    ///     <c>true</c> if running; otherwise, <c>false</c>.
    /// </value>
    public volatile bool Running;

    /// <summary>
    ///     Gets or sets to the <see cref="CancellationToken" />
    /// </summary>
    public CancellationTokenSource TokenSource;

    /// <summary>
    ///     Gets the base.
    /// </summary>
    /// <value>
    ///     The base.
    /// </value>
    public IBotbase Botbase { get; private set; }

    /// <summary>
    ///     Sets the botbase.
    /// </summary>
    /// <param name="botBase">The bot base.</param>
    public void SetBotbase(IBotbase botBase)
    {
        Botbase = botBase;
        //Botbase.Initialize();

        EventManager.FireEvent("OnSetBotbase", botBase);
    }

    /// <summary>
    ///     Starts this instance.
    /// </summary>
    public void Start()
    {
        if (Running || Botbase == null)
            return;

        if (!RSBot.Core.Security.PermissionManager.CanUseBotbase(Botbase.Name, out var reason))
        {
            Log.Notify($"[الصلاحيات] {reason}");
            EventManager.FireEvent("OnStopBot");
            return;
        }

        if (!RSBot.Core.Security.PermissionManager.CheckClientInstanceLimit(out int count, out int limit))
        {
            Log.Notify($"[الصلاحيات] تم تجاوز الحد الأقصى للحسابات المسموح بها في النسخة المجانية ({count}/{limit})!");
            EventManager.FireEvent("OnStopBot");
            return;
        }

        TokenSource = new CancellationTokenSource();

        Task.Factory.StartNew(async e =>
            {
                Running = true;

                EventManager.FireEvent("OnStartBot");
                Botbase.Start();

                while (!TokenSource.IsCancellationRequested)
                {
                    if (!Game.Ready)
                    {
                        await Task.Delay(100);
                        continue;
                    }

                    if (RSBot.Core.Security.PermissionManager.IsSessionExpired(out _, out var sessionLimit))
                    {
                        Log.Notify($"[الصلاحيات] انتهت مدة الجلسة المسموح بها في النسخة المجانية ({sessionLimit.TotalMinutes} دقيقة).");
                        Stop();
                        break;
                    }

                    Botbase.Tick();
                    await Task.Delay(100);
                }
            },
            TokenSource.Token, TaskCreationOptions.LongRunning);
    }

    /// <summary>
    ///     Stops this instance.
    /// </summary>
    public void Stop()
    {
        Running = false;

        try { ScriptManager.Stop(); } catch { }
        try { ShoppingManager.Stop(); } catch { }
        try { PickupManager.Stop(); } catch { }

        if (TokenSource != null && !TokenSource.IsCancellationRequested)
        {
            try { TokenSource.Cancel(); } catch { }
        }

        try { EventManager.FireEvent("OnStopBot"); } catch { }

        if (Botbase != null)
        {
            Log.Notify($"Stopping bot {Botbase.Name}");
            Game.SelectedEntity = null;
            try
            {
                Botbase.Stop();
            }
            catch { }

            Log.Notify($"Stoped bot {Botbase.Name}");
        }

        Log.Status("Bot stopped");
    }
}