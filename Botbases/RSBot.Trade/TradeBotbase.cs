using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using RSBot.Core;
using RSBot.Core.Components;
using RSBot.Core.Event;
using RSBot.Core.Objects;
using RSBot.Core.Plugins;
using RSBot.Trade.Bundle;
using RSBot.Trade.Components;
using RSBot.Trade.Components.Scripting;

namespace RSBot.Trade;

public class TradeBotbase : IBotbase
{
    public static bool IsActive => Kernel.Bot?.Botbase.Name == "RSBot.Trade" && Kernel.Bot.Running;
    public string Name => "RSBot.Trade";

    public string DisplayName => "Trade";

    public string TabText => DisplayName;

    public Area Area => new();

    /// <summary>
    ///     Ticks this instance. It's the botbase main-loop
    /// </summary>
    public void Tick()
    {
        if (!Game.Ready || Game.Player == null)
            return;

        Bundles.Tick();
    }

    /// <summary>
    ///     Gets the view.
    /// </summary>
    /// <returns></returns>
    public Control View => Views.View.Main;

    /// <summary>
    ///     Starts this instance.
    /// </summary>
    public void Start()
    {
        if (!AssertPlayerIsTrader())
        {
            MessageBox.Show("The active character is not a trader! Make sure that you have the trader job and suite.",
                "Not a trader", MessageBoxButtons.OK, MessageBoxIcon.Error);
            Kernel.Bot.Stop();

            return;
        }

        Bundles.Start();
    }

    /// <summary>
    ///     Stops this instance.
    /// </summary>
    public void Stop()
    {
        Bundles.Stop();
    }

    /// <summary>
    ///     Triggered after the botbase was registered to the kernel.
    /// </summary>
    public void Register()
    {
        Log.Debug("[Trade] Botbase registered to the kernel!");

        if (ScriptManager.CommandHandlers.All(h => h.Name != "buy-goods"))
            ScriptManager.CommandHandlers.Add(new BuyGoodsScriptCommand());

        if (ScriptManager.CommandHandlers.All(h => h.Name != "sell-goods"))
            ScriptManager.CommandHandlers.Add(new SellGoodsScriptCommand());

        if (ScriptManager.CommandHandlers.All(h => h.Name != "summon-transport"))
            ScriptManager.CommandHandlers.Add(new SummonTransportScriptCommand());

        EventManager.SubscribeEvent("OnLoadCharacter", OnLoadCharacter);

        Bundles.Initialize();
    }

    /// <summary>
    ///     Starts the trade bot automatically after the character finished
    ///     loading, if the user enabled the "start after login" option.
    /// </summary>
    private void OnLoadCharacter()
    {
        if (!TradeConfig.AutoStartAfterLogin)
            return;

        if (Kernel.Bot == null || Kernel.Bot.Running)
            return;

        if (!IsTrader())
            return;

        // Make sure the trade botbase is the active one, otherwise the auto
        // start would run whatever botbase the user had selected before.
        if (Kernel.Bot.Botbase != this)
        {
            var registered = Kernel.BotbaseManager?.Bots
                .FirstOrDefault(b => b.Value.Name == Name).Value;

            if (registered == null)
                return;

            Kernel.Bot.SetBotbase(registered);
        }

        Task.Run(async () =>
        {
            // Give the client enough time to finish loading the character,
            // inventory, job information and the surrounding entities.
            await Task.Delay(4000);

            if (!TradeConfig.AutoStartAfterLogin)
                return;

            if (Kernel.Bot == null || Kernel.Bot.Running)
                return;

            if (!Game.Ready || Game.Player == null || !IsTrader())
                return;

            Log.Notify("[Trade] بدء التجارة تلقائياً بعد تسجيل الدخول...");
            Kernel.Bot.Start();
        });
    }

    /// <summary>
    ///     Returns a value indicating whether the active character can trade.
    /// </summary>
    public static bool IsTrader()
    {
        if (Game.Player?.JobInformation is not { Type: JobType.Trade })
            return false;

        return Game.Player.Inventory?.GetEquippedPartItems()
            .FirstOrDefault(i => i.Record != null && i.Record.IsJobOutfit) != null;
    }

    /// <summary>
    ///     Translate the botbase plugin
    /// </summary>
    public void Translate()
    {
        LanguageManager.Translate(View, Kernel.Language);
        if (View is Views.Main tradeView)
            tradeView.ApplyLanguage();
    }

    /// <summary>
    ///     Returns a value indicating if the active character is a trader
    /// </summary>
    /// <returns></returns>
    private bool AssertPlayerIsTrader()
    {
        return IsTrader();
    }
}