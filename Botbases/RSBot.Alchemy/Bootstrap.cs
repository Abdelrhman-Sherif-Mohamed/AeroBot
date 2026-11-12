using System.Windows.Forms;
using RSBot.Alchemy.Bot;
using RSBot.Alchemy.Subscriber;
using RSBot.Core;
using RSBot.Core.Components;
using RSBot.Core.Objects;
using RSBot.Core.Plugins;

namespace RSBot.Alchemy;

public class Bootstrap : IBotbase
{
    private static readonly string _name = "RSBot.Alchemy";

    internal static Bootstrap Instance { get; private set; }
    public static BotRunner Runner { get; private set; }

    public static bool IsActive => Runner?.Running == true;

    public static BotRunner EnsureRunner()
    {
        if (Runner == null && Instance != null)
            Runner = Kernel.GetOrCreateRunner(Instance, usesMovement: false, stopsPickupOnStop: false, stopsShoppingOnStop: false);

        return Runner;
    }

    public static void StopRunner()
    {
        Runner?.Stop();
    }

    public string Name => _name;

    public string DisplayName => "Alchemy";

    public string TabText => DisplayName;

    public Area Area => new();

    public Control View => Globals.View;

    public void Start()
    {
        Globals.Botbase?.Start();
        Log.AppendFormat(LogLevel.Debug, "[Alchemy] Starting automated alchemy...");
    }

    public void Stop()
    {
        Globals.Botbase?.Stop();
        Log.AppendFormat(LogLevel.Debug, "[Alchemy] Stopped automated alchemy");
    }

    public void Register()
    {
        Instance = this;
        EnsureRunner();
        Initialize();
        Log.Debug("[Alchemy] Botbase registered to the kernel!");
    }

    public void Tick()
    {
        if (!Globals.View.IsRefreshing && !AlchemyManager.IsFusing)
            Globals.Botbase?.Tick();
    }

    public void Translate()
    {
        LanguageManager.Translate(View, Kernel.Language);
        Globals.View?.ApplyLocalization();
    }

    public void Initialize()
    {
        Instance = this;
        EnsureRunner();
        AlchemyEventsSubscriber.Subscribe();
        Globals.Botbase ??= new Botbase();

        Log.AppendFormat(LogLevel.Notify, "[Alchemy] Initialized botbase");
    }
}
