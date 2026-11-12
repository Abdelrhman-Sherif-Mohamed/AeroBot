using System;
using System.Collections.Concurrent;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using RSBot.Core.Components;
using RSBot.Core.Event;
using RSBot.Core.Network;
using RSBot.Core.Plugins;

namespace RSBot.Core;

public static class Kernel
{
    /// <summary>
    ///     The updater token source
    /// </summary>
    private static CancellationTokenSource _updaterTokenSource;

    /// <summary>
    ///     Gets or sets the botbase manager.
    /// </summary>
    /// <value>
    ///     The botbase manager.
    /// </value>
    public static BotbaseManager BotbaseManager { get; private set; }

    /// <summary>
    ///     Gets the plugin manager.
    /// </summary>
    /// <value>
    ///     The plugin manager.
    /// </value>
    public static PluginManager PluginManager { get; private set; }

    /// <summary>
    ///     Gets the proxy.
    /// </summary>
    /// <value>
    ///     The proxy.
    /// </value>
    public static Proxy Proxy { get; set; }

    /// <summary>
    ///     Gets or sets the bot.
    /// </summary>
    /// <value>
    ///     The bot.
    /// </value>
    public static Bot Bot { get; set; }

    private static readonly ConcurrentDictionary<string, BotRunner> _independentRunners =
        new(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    ///     Gets or creates an independent runner for a non-training botbase (Trade, Lure, Alchemy)
    ///     so its start/stop lifecycle runs outside <see cref="Bot" />.
    /// </summary>
    public static BotRunner GetOrCreateRunner(
        IBotbase botbase,
        bool usesMovement,
        bool stopsPickupOnStop = false,
        bool stopsShoppingOnStop = false)
    {
        if (botbase == null || string.IsNullOrWhiteSpace(botbase.Name))
            return null;

        return _independentRunners.GetOrAdd(botbase.Name,
            _ => new BotRunner(botbase, usesMovement, stopsPickupOnStop, stopsShoppingOnStop));
    }

    public static BotRunner GetRunner(string botbaseName)
    {
        if (string.IsNullOrWhiteSpace(botbaseName))
            return null;

        return _independentRunners.TryGetValue(botbaseName, out var runner) ? runner : null;
    }

    public static bool IsRunnerActive(string botbaseName)
    {
        if (string.IsNullOrWhiteSpace(botbaseName))
            return false;

        if (Bot?.Running == true && string.Equals(Bot.Botbase?.Name, botbaseName, StringComparison.OrdinalIgnoreCase))
            return true;

        return _independentRunners.TryGetValue(botbaseName, out var runner) && runner.Running;
    }

    public static bool IsAnyMovementBotRunning =>
        (Bot?.Running == true) || _independentRunners.Values.Any(r => r.Running && r.UsesMovement);

    public static bool IsAnyBotRunning =>
        (Bot?.Running == true) || _independentRunners.Values.Any(r => r.Running);

    internal static string GetLocalizedBotDisplayName(string botbaseName, string fallbackDisplayName)
    {
        var isAr = Language == "ar_AR";
        return botbaseName switch
        {
            "RSBot.Default" => isAr ? "التدريب" : "Training",
            "RSBot.Trade" => isAr ? "التجارة" : "Trade",
            "RSBot.Lure" => isAr ? "سحب الوحوش" : "Lure",
            "RSBot.Alchemy" => isAr ? "الكمي" : "Alchemy",
            _ => string.IsNullOrWhiteSpace(fallbackDisplayName) ? botbaseName : fallbackDisplayName
        };
    }

    internal static string GetConflictingMovementBotDisplayName(string excludeBotName = null)
    {
        if (Bot?.Running == true &&
            !string.Equals(Bot.Botbase?.Name, excludeBotName, StringComparison.OrdinalIgnoreCase))
        {
            return GetLocalizedBotDisplayName(Bot.Botbase?.Name ?? "RSBot.Default",
                Bot.Botbase?.DisplayName ?? "Training");
        }

        var activeOther = _independentRunners.Values.FirstOrDefault(r =>
            r.Running &&
            r.UsesMovement &&
            !string.Equals(r.Botbase?.Name, excludeBotName, StringComparison.OrdinalIgnoreCase));

        return activeOther == null
            ? null
            : GetLocalizedBotDisplayName(activeOther.Botbase?.Name, activeOther.Botbase?.DisplayName);
    }

    public static void StopAllIndependentRunners(bool onlyMovement = false)
    {
        foreach (var runner in _independentRunners.Values)
        {
            if (onlyMovement && !runner.UsesMovement)
                continue;

            try
            {
                if (runner.Running)
                    runner.Stop();
            }
            catch (Exception ex)
            {
                Log.Debug($"[Kernel] Error stopping runner [{runner.Botbase?.Name}]: {ex.Message}");
            }
        }
    }

    /// <summary>
    ///     The application language
    /// </summary>
    public static string Language { get; set; }

    /// <summary>
    ///     Get environment fixed tick count
    /// </summary>
    public static int TickCount => Environment.TickCount & int.MaxValue;

    /// <summary>
    ///     Get environment base directory
    /// </summary>
    public static string BasePath => AppDomain.CurrentDomain.BaseDirectory;

    /// <summary>
    /// Returns a value indicating if the NavMeshApi should be used or not.
    /// </summary>
    public static bool EnableCollisionDetection
    {
        get => GlobalConfig.Get("RSBot.EnableCollisionDetection", true);
        set => GlobalConfig.Set("RSBot.EnableCollisionDetection", value);
    }

    /// <summary>
    /// Returns a value indicating if this is a debug environment.
    /// </summary>
    public static bool Debug
    {
        get {
            #if DEBUG
            return true;
            #endif
            return GlobalConfig.Get("RSBot.DebugEnvironment", false);
        }
        set => GlobalConfig.Set("RSBot.DebugEnvironments", value);
    }

    private static bool _assemblyResolverRegistered;

    /// <summary>
    ///     Initializes this instance.
    /// </summary>
    public static void Initialize()
    {
        if (!_assemblyResolverRegistered)
        {
            _assemblyResolverRegistered = true;
            AppDomain.CurrentDomain.AssemblyResolve += (sender, args) =>
            {
                try
                {
                    var assemblyName = new AssemblyName(args.Name).Name;
                    if (string.IsNullOrWhiteSpace(assemblyName)) return null;

                    var candidateDirs = new[]
                    {
                        BasePath,
                        Path.Combine(BasePath, "Data", "Plugins"),
                        Path.Combine(BasePath, "Data", "Bots"),
                        Path.Combine(BasePath, "runtimes", "win-x86", "native"),
                        Path.Combine(BasePath, "Data", "Plugins", "runtimes", "win-x86", "native")
                    };

                    foreach (var dir in candidateDirs)
                    {
                        if (!Directory.Exists(dir)) continue;
                        var path = Path.Combine(dir, assemblyName + ".dll");
                        if (File.Exists(path))
                        {
                            return Assembly.LoadFrom(path);
                        }
                    }
                }
                catch { }

                return null;
            };
        }

        Bot = new Bot();
        BotbaseManager = new BotbaseManager();
        PluginManager = new PluginManager();

        //Network handlers/hooks
        RegisterNetworkHandlers();
        RegisterNetworkHooks();

        EventManager.SubscribeEvent("OnAgentServerDisconnected", () => StopAllIndependentRunners());

        _updaterTokenSource = new CancellationTokenSource();

        Task.Factory.StartNew(ComponentUpdaterAsync, _updaterTokenSource.Token, TaskCreationOptions.LongRunning,
            TaskScheduler.Current);
    }

    private static async Task ComponentUpdaterAsync()
    {
        var lastTick = TickCount;
        var lastClockTick = TickCount;

        while (!_updaterTokenSource.IsCancellationRequested)
        {
            await Task.Delay(10);

            if (TickCount - lastClockTick >= 1000)
            {
                lastClockTick = TickCount;
                EventManager.FireEvent("OnClock");
            }

            var player = Game.Player;
            if (!Game.Ready || player == null)
            {
                lastTick = TickCount;
                continue;
            }

            try
            {
                var elapsed = TickCount - lastTick;

                player.Update(elapsed);
                player.Transport?.Update(elapsed);
                player.JobTransport?.Update(elapsed);
                player.AbilityPet?.Update(elapsed);
                player.Growth?.Update(elapsed);
                player.Fellow?.Update(elapsed);

                SpawnManager.Update(elapsed);

                EventManager.FireEvent("OnTick");

                lastTick = TickCount;
            }
            catch (Exception e)
            {
                Log.Fatal(e);
            }
        }
    }

    /// <summary>
    ///     Registers the network handler.
    /// </summary>
    private static void RegisterNetworkHandlers()
    {
        var type = typeof(IPacketHandler);
        var types = AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(s => s.GetTypes())
            .Where(p => type.IsAssignableFrom(p) && !p.IsInterface).ToArray();

        foreach (var handler in types)
        {
            var instance = (IPacketHandler)Activator.CreateInstance(handler);

            PacketManager.RegisterHandler(instance);
        }
    }

    /// <summary>
    ///     Registers the network hooks.
    /// </summary>
    private static void RegisterNetworkHooks()
    {
        var type = typeof(IPacketHook);
        var types = AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(s => s.GetTypes())
            .Where(p => type.IsAssignableFrom(p) && !p.IsInterface).ToArray();

        foreach (var hook in types)
        {
            var instance = (IPacketHook)Activator.CreateInstance(hook);

            PacketManager.RegisterHook(instance);
        }
    }
}