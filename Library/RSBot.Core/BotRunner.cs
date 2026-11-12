using System;
using System.Threading;
using System.Threading.Tasks;
using RSBot.Core.Components;
using RSBot.Core.Plugins;
using RSBot.Core.Security;

namespace RSBot.Core;

/// <summary>
///     Dedicated execution runner for an independent <see cref="IBotbase" /> instance (e.g. Trade, Lure, Alchemy)
///     so it runs and stops completely outside the main Training <see cref="Bot" /> (<see cref="Kernel.Bot" />).
/// </summary>
public sealed class BotRunner
{
    private readonly object _stateLock = new();
    private CancellationTokenSource _tokenSource;
    private volatile bool _running;

    public BotRunner(
        IBotbase botbase,
        bool usesMovement,
        bool stopsPickupOnStop = false,
        bool stopsShoppingOnStop = false)
    {
        Botbase = botbase ?? throw new ArgumentNullException(nameof(botbase));
        UsesMovement = usesMovement;
        StopsPickupOnStop = stopsPickupOnStop;
        StopsShoppingOnStop = stopsShoppingOnStop;
    }

    public IBotbase Botbase { get; }
    public bool UsesMovement { get; }
    public bool StopsPickupOnStop { get; }
    public bool StopsShoppingOnStop { get; }
    public bool Running => _running;

    public event Action StateChanged;

    public bool Start()
    {
        return TryStart(out _);
    }

    public bool TryStart(out string errorMessage)
    {
        errorMessage = null;

        if (Botbase == null)
        {
            errorMessage = Kernel.Language == "ar_AR"
                ? "لم يتم تهيئة هذا النمط."
                : "Botbase is not initialized.";
            return false;
        }

        if (!PermissionManager.CanUseBotbase(Botbase.Name, out var permReason))
        {
            errorMessage = permReason;
            Log.Warn(permReason);
            return false;
        }

        CancellationToken token;

        lock (_stateLock)
        {
            if (_running)
                return true;

            if (UsesMovement)
            {
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
            }

            _tokenSource?.Dispose();
            _tokenSource = new CancellationTokenSource();
            token = _tokenSource.Token;
            _running = true;
        }

        RaiseStateChanged();

        Task.Factory.StartNew(async () =>
        {
            try
            {
                Botbase.Start();

                while (!token.IsCancellationRequested && _running)
                {
                    if (!Game.Ready)
                    {
                        await Task.Delay(100, token);
                        continue;
                    }

                    try
                    {
                        Botbase.Tick();
                    }
                    catch (Exception ex)
                    {
                        Log.Error($"[{Botbase.Name}] Error during tick: {ex.Message}");
                    }

                    await Task.Delay(100, token);
                }
            }
            catch (OperationCanceledException)
            {
                // Expected on normal cancellation.
            }
            catch (Exception ex)
            {
                Log.Fatal(ex);
            }
            finally
            {
                // If the loop exited due to an unexpected error while still marked running, reset cleanly.
                if (_running && !token.IsCancellationRequested)
                    Stop();
            }
        }, token, TaskCreationOptions.LongRunning, TaskScheduler.Default);

        return true;
    }

    public void Stop()
    {
        bool wasRunning;

        lock (_stateLock)
        {
            wasRunning = _running;
            _running = false;

            if (_tokenSource != null && !_tokenSource.IsCancellationRequested)
                _tokenSource.Cancel();
        }

        if (!wasRunning)
            return;

        if (UsesMovement)
        {
            try { ScriptManager.Stop(error: true); } catch { }
            Game.SelectedEntity = null;
        }

        if (StopsShoppingOnStop)
        {
            try { ShoppingManager.Stop(); } catch { }
        }

        if (StopsPickupOnStop)
        {
            try { PickupManager.Stop(); } catch { }
        }

        try
        {
            Botbase?.Stop();
        }
        catch (Exception ex)
        {
            Log.Error($"[{Botbase?.Name}] Error during stop: {ex.Message}");
        }

        RaiseStateChanged();
    }

    private void RaiseStateChanged()
    {
        try
        {
            StateChanged?.Invoke();
        }
        catch (Exception ex)
        {
            Log.Debug($"[{Botbase?.Name}] StateChanged listener error: {ex.Message}");
        }
    }
}
