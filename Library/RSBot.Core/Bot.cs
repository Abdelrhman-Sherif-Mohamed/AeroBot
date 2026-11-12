using System;
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
        TryStart(out _);
    }

    /// <summary>
    ///     Attempts to start the Training bot, guarding against conflicting movement runners.
    /// </summary>
    public bool TryStart(out string errorMessage)
    {
        errorMessage = null;

        if (Botbase == null)
        {
            errorMessage = Kernel.Language == "ar_AR"
                ? "لم يتم تحديد بوت التدريب."
                : "Training botbase is not selected.";
            return false;
        }

        if (Running)
            return true;

        var conflict = Kernel.GetConflictingMovementBotDisplayName(Botbase.Name);
        if (!string.IsNullOrEmpty(conflict))
        {
            var selfName = Kernel.GetLocalizedBotDisplayName(Botbase.Name, Botbase.DisplayName);
            errorMessage = Kernel.Language == "ar_AR"
                ? $"لا يمكن تشغيل [{selfName}] أثناء عمل [{conflict}]. يرجى إيقاف [{conflict}] أولاً."
                : $"Cannot start {selfName} while {conflict} is running. Please stop {conflict} first.";
            Log.Warn(errorMessage);
            return false;
        }

        TokenSource?.Dispose();
        TokenSource = new CancellationTokenSource();
        var token = TokenSource.Token;

        Task.Factory.StartNew(async e =>
            {
                try
                {
                    Running = true;

                    EventManager.FireEvent("OnStartBot");
                    Botbase.Start();

                    while (!token.IsCancellationRequested)
                    {
                        if (!Game.Ready)
                        {
                            await Task.Delay(100);
                            continue;
                        }

                        try
                        {
                            Botbase.Tick();
                        }
                        catch (Exception ex)
                        {
                            Log.Error($"[Botbase] Error during tick: {ex.Message}");
                        }

                        await Task.Delay(100);
                    }
                }
                catch (Exception ex)
                {
                    Log.Fatal(ex);
                    Stop();
                }
            },
            token, TaskCreationOptions.LongRunning);

        return true;
    }

    /// <summary>
    ///     Stops this instance.
    /// </summary>
    public void Stop()
    {
        if (TokenSource != null && !TokenSource.IsCancellationRequested)
            TokenSource.Cancel();

        var wasRunning = Running;
        Running = false;

        if (Botbase == null || !wasRunning)
            return;

        ScriptManager.Stop(error: true);
        ShoppingManager.Stop();
        PickupManager.Stop();

        EventManager.FireEvent("OnStopBot");
        Log.Notify($"Stopping bot {Botbase.Name}");

        Game.SelectedEntity = null;
        Botbase.Stop();

        Log.Notify($"Stoped bot {Botbase.Name}");
        Log.Status("Bot stopped");
    }
}
