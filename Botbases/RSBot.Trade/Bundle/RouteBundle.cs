using System;

using System.IO;

using System.Linq;

using System.Threading;

using System.Threading.Tasks;

using System.Windows.Forms;

using RSBot.Core;

using RSBot.Core.Components;

using RSBot.Core.Event;

using RSBot.Core.Objects;

using RSBot.Core.Objects.Spawn;

using RSBot.Trade.Components;

using SDUI.Controls;



namespace RSBot.Trade.Bundle;



/// <summary>

///     This bundle is used to keep control over the script manager before continuing to the next position.

/// </summary>

internal class RouteBundle

{

    private bool _blockedByRouteDialog;

    private bool _checkForTownScript;

    private bool _lastScriptIsTownScript;

    private bool _waitForReturnScroll;

    private DateTime _returnScrollStarted = DateTime.MinValue;



    public static string FirstCity { get; set; }

    public static string SecondCity { get; set; }



    /// <summary>

    ///     The current route file. This could ether be a trade route or a town script.

    /// </summary>

    public string CurrentRouteFile { get; set; }



    /// <summary>

    ///     A value indicating if the town script is running.

    /// </summary>

    public bool TownscriptRunning { get; private set; }



    /// <summary>

    ///     A value indicating if the bot is waiting for a hunter.

    /// </summary>

    public bool WaitingForHunter { get; private set; }



    /// <summary>

    ///     A value indicating if the bot is waiting for a player to trace nearby.

    /// </summary>

    public bool WaitingForTracePlayer { get; private set; }



    /// <summary>

    ///     Initializes the bundle.

    /// </summary>

    public void Initialize()

    {

        SubscribeEvents();

    }



    /// <summary>

    ///     Subscribes the events.

    /// </summary>

    private void SubscribeEvents()

    {

        EventManager.SubscribeEvent("OnFinishScript", new Action<bool>(OnFinishScript));

    }



    /// <summary>

    ///     Checks if the player's trade transport already has goods loaded.

    /// </summary>

    public static bool TransportHasGoods()

    {

        if (Game.Player?.JobTransport?.Inventory == null)

            return false;



        return Game.Player.JobTransport.Inventory.Any(i => i != null && i.Record != null && i.Amount > 0);

    }



    /// <summary>

    ///     Initializes the city pair for City-to-City Trade Loops.

    /// </summary>

    public static void InitializeCityPair()

    {

        var fromCity = TradeConfig.StartCity;

        if (string.IsNullOrEmpty(fromCity) || string.Equals(fromCity, "Auto-Detect", StringComparison.OrdinalIgnoreCase))

            fromCity = DetectCurrentCity();



        FirstCity = fromCity;

        SecondCity = TradeConfig.DestinationCity;

    }



    /// <summary>

    ///     Triggered when a script is finished.

    /// </summary>

    private void OnFinishScript(bool error)

    {

        // Error during script execution or bot stopped -> Do not advance trade loops!

        if (error || !TradeBotbase.IsActive || Kernel.Bot == null || !Kernel.Bot.Running || !Game.Ready)

            return;



        TownscriptRunning = false;



        // ONLY advance loop and clear CurrentRouteFile if the script ACTUALLY reached the destination end!

        // Never advance mid-route when paused, cancelled, or interrupted!

        var reachedDestination = !ScriptManager.Paused && 

                                  ScriptManager.Commands != null && 

                                  ScriptManager.Commands.Length > 0 && 

                                  ScriptManager.CurrentLineIndex >= ScriptManager.Commands.Length - 3;



        if (!reachedDestination)

        {

            Log.Debug($"[CityTrade] OnFinishScript fired at line #{ScriptManager.CurrentLineIndex} without reaching destination. Loop will not advance.");

            return;

        }



        CurrentRouteFile = null;



        //Townscript finished?

        if (!_lastScriptIsTownScript)

        {

            if (TradeConfig.UseCityToCity)

            {

                TradeConfig.CurrentTradeLoop++;



                var infiniteLoops = TradeConfig.RepeatTradeLoop && TradeConfig.TradeLoops <= 0;

                Log.Notify(infiniteLoops

                    ? $"[CityTrade] اكتملت الجولة #{TradeConfig.CurrentTradeLoop} بنجاح! (تكرار لانهائي)"

                    : $"[CityTrade] اكتملت الجولة #{TradeConfig.CurrentTradeLoop} بنجاح!");



                if ((!TradeConfig.RepeatTradeLoop && TradeConfig.CurrentTradeLoop >= 1) ||

                    (!infiniteLoops && TradeConfig.CurrentTradeLoop >= TradeConfig.TradeLoops))

                {

                    Log.Notify($"[CityTrade] تم إكمال جميع جولات التجارة المطلوبة بنجاح!");



                    if (TradeConfig.UseReturnScrollAfterLoop && !AttackBundle.IsPlayerInTown())

                    {

                        Log.Notify("[CityTrade] استخدام Return Scroll للعودة إلى المدينة بعد اكتمال آخر جولة...");

                        Game.Player?.UseReturnScroll();

                    }



                    if (TradeConfig.DismountTakeOffJobAndStartDefault)

                    {

                        Task.Run(async () =>

                        {

                            await Task.Delay(2500);

                            ExecuteDismountTakeOffJobAndStartDefault();

                        });

                        return;

                    }



                    TradeConfig.CurrentTradeLoop = 0;

                    FirstCity = null;

                    SecondCity = null;

                    CurrentRouteFile = null;

                    Kernel.Bot.Stop();

                    return;

                }



                CurrentRouteFile = null;

                _checkForTownScript = false;

                _lastScriptIsTownScript = false;



                // Returning to town between loops makes the next one-way route

                // start from the destination city instead of the middle of the

                // map. The tick loop waits for the teleport to finish.

                if (TradeConfig.UseReturnScrollAfterLoop && !AttackBundle.IsPlayerInTown())

                {

                    Log.Notify("[CityTrade] استخدام Return Scroll للعودة إلى المدينة قبل الجولة التالية...");

                    Game.Player.UseReturnScroll();

                    _waitForReturnScroll = true;

                    _returnScrollStarted = DateTime.UtcNow;

                }



                return;

            }



            _checkForTownScript = !TransportHasGoods();



            return;

        }



        _checkForTownScript = false;

        _lastScriptIsTownScript = false;

    }



    /// <summary>

    ///     Checks and runs the townscript.

    /// </summary>

    private string CheckForTownScript()

    {

        if (TransportHasGoods())

        {

            Log.Notify("[Trade] الجمل يحتوي بالفعل على بضاعة. تم تخطي اسكربت التاون لمنع اللف في المدينة دون داعٍ.");

            return null;

        }



        if (!TradeConfig.UseRouteScripts && !TradeConfig.UseCityToCity)

            return null;



        if (TradeConfig.SkipTownScript)

            return null;



        var filename = Path.Combine(ScriptManager.InitialDirectory, "Towns",

            Game.Player.Movement.Source.Region + ".rbs");



        if (!File.Exists(filename))

            return null;



        Log.NotifyLang("LoadingTownScript", filename);



        return filename;

    }



    /// <summary>

    ///     Starts the bundle.

    /// </summary>

    public void Start()

    {

        TownscriptRunning = false;

        WaitingForHunter = false;

        WaitingForTracePlayer = false;

        _waitForReturnScroll = false;

        _returnScrollStarted = DateTime.MinValue;



        _lastScriptIsTownScript = false;

        _checkForTownScript = !TradeConfig.TracePlayer && !TransportHasGoods();



        if (TradeConfig.UseCityToCity)

        {

            InitializeCityPair();

        }



        if (string.IsNullOrEmpty(CurrentRouteFile) || !File.Exists(CurrentRouteFile))

        {

            CurrentRouteFile = null;

        }

    }



    /// <summary>

    ///     Ticks the route bundle

    /// </summary>

    public void Tick()

    {

        if (Kernel.Bot == null || !Kernel.Bot.Running || !TradeBotbase.IsActive)

            return;



        //Wait for player to revive

        if (Game.Player.State.LifeState == LifeState.Dead)

        {

            Log.Status("Waiting for resurrection");



            if (ScriptManager.Running && !ScriptManager.Paused)

                ScriptManager.Pause();



            return;

        }



        if (ShoppingManager.Running)

            return;



        //Interrupt in case of attack or waiting for the transport

        if ((ScriptManager.Running &&

             Bundles.TransportBundle.Busy) ||

            Bundles.AttackBundle.Busy)

        {

            if (!ScriptManager.Paused && ScriptManager.Running)

                ScriptManager.Pause();



            return;

        }



        if (_blockedByRouteDialog)

            return;



        //Wait for the return scroll to finish before picking the next route.

        if (_waitForReturnScroll)

        {

            var elapsed = DateTime.UtcNow - _returnScrollStarted;



            if ((AttackBundle.IsPlayerInTown() && elapsed.TotalSeconds >= 2) || elapsed.TotalSeconds >= 12)

            {

                _waitForReturnScroll = false;

            }

            else

            {

                Log.Status("Waiting for return scroll...");

                return;

            }

        }



        //Can continue?

        if (!CheckHunterNearby() || !CheckTracePlayer())

            return;



        if (!TradeConfig.UseRouteScripts && !TradeConfig.UseCityToCity)

            return;



        if (Game.Player.Movement.HasDestination)

            return;



        //Run town script

        if (_checkForTownScript && !_lastScriptIsTownScript)

        {

            CurrentRouteFile = CheckForTownScript();

            _checkForTownScript = false;



            if (CurrentRouteFile != null)

            {

                _lastScriptIsTownScript = true;

                TownscriptRunning = true;



                ScriptManager.Load(CurrentRouteFile);

                Task.Run(() => ScriptManager.RunScript(false));



                Thread.Sleep(100);



                return;

            }



            TownscriptRunning = false;

        }



        //Nothing more to do, skip tick

        if (ScriptManager.Running && !ScriptManager.Paused)

            return;



        //Pick next route

        if (CurrentRouteFile == null && !ScriptManager.Running && TradeBotbase.IsActive && !_blockedByRouteDialog)

        {

            CurrentRouteFile = GetNextRouteFile();

            if (CurrentRouteFile == null)

            {

                CurrentRouteFile = ShowRouteDialog();

            }



            if (CurrentRouteFile == null)

            {

                Log.Warn("[Trade] Could not find the next route!");

                Kernel.Bot.Stop();



                return;

            }



            _blockedByRouteDialog = false;



            var routeName = Path.GetFileNameWithoutExtension(CurrentRouteFile);

            Game.ShowNotification($"[AeroBot] Picked trade route {routeName}");

            Log.Notify($"[AeroBot Trade] تم تفعيل مسار التجارة: {routeName}");



            ScriptManager.Load(CurrentRouteFile);



            if (TransportHasGoods())

            {

                var buyGoodsLine = ScriptManager.FindBuyGoodsCommandLine();

                var startLine = buyGoodsLine >= 0

                    ? ScriptManager.FindNearestMoveCommandLineAfter(buyGoodsLine + 1)

                    : ScriptManager.FindNearestMoveCommandLine();



                Log.Notify($"[Trade] الجمل محمّل بالبضاعة! التحرك فوراً من الخطوة #{startLine} نحو بوابة الخروج والوجهة دون الرجوع للخلف.");

                Task.Run(() => ScriptManager.RunScript(startLine));

            }

            else

            {

                var buyGoodsLine = ScriptManager.FindBuyGoodsCommandLine();

                var startLine = buyGoodsLine >= 0 ? 0 : ScriptManager.FindNearestMoveCommandLine();

                Log.Notify($"[Trade] بدء السير لشراء البضاعة والتحرك: السطر #{startLine}");

                Task.Run(() => ScriptManager.RunScript(startLine));

            }



            Thread.Sleep(100);



            return;

        }



        if (!ScriptManager.Running && CurrentRouteFile != null && (ScriptManager.Commands == null || ScriptManager.Commands.Length == 0))

            ScriptManager.Load(CurrentRouteFile);



        //Continue previous script (only when not in active combat and transport not lost)

        if (!Bundles.AttackBundle.IsAttacking && !Bundles.TransportBundle.Busy)

        {

            if (ScriptManager.Paused || (!ScriptManager.Running && CurrentRouteFile != null))

            {

                ScriptManager.ResumeScript();



                //Make sure that the next tick the script manager is ready

                Thread.Sleep(100);

            }

        }

    }



    /// <summary>

    ///     Returns a value indicating if a hunter is nearby.

    /// </summary>

    private bool CheckHunterNearby()

    {

        var hunterNearby = !TradeConfig.WaitForHunter || SpawnManager.TryGetEntity<SpawnedPlayer>(

            p => p.WearsJobSuite && p.Job == JobType.Hunter,

            out _);



        WaitingForHunter = !hunterNearby;



        if (WaitingForHunter)

            Log.Status("Waiting for a hunter nearby...");



        return hunterNearby;

    }



    /// <summary>

    ///     Checks for the player to trace nearby (if configured)

    /// </summary>

    private bool CheckTracePlayer()

    {

        if (TradeConfig.UseRouteScripts || TradeConfig.UseCityToCity)

        {

            WaitingForTracePlayer = false;



            return true;

        }



        if (string.IsNullOrEmpty(TradeConfig.TracePlayerName))

        {

            WaitingForTracePlayer = false;



            Log.Error("[Trade] Enter a name for a player to trace and try again.");



            Kernel.Bot.Stop();



            return false;

        }



        if (!SpawnManager.TryGetEntity<SpawnedPlayer>(p => p.Name == TradeConfig.TracePlayerName, out var player))

        {

            Log.Status($"Waiting for {TradeConfig.TracePlayerName} to trace");

            WaitingForTracePlayer = true;



            return false;

        }



        WaitingForTracePlayer = false;



        Game.Player.MoveTo(player.Position);



        return true;

    }



    /// <summary>

    ///     Shows the route picker dialog and waits for user input.

    /// </summary>

    private string ShowRouteDialog()

    {

        if (TradeConfig.UseCityToCity)

            return null; // Never show modal dialog in automated City-to-City trade!



        _blockedByRouteDialog = true;



        var inputDialog = new InputDialog("Select route", "Select route",

            "Select the route to start from at the current location.", InputDialog.InputType.Combobox)

        {

            TopLevel = true,

            StartPosition = FormStartPosition.CenterScreen,

            ShowInTaskbar = true

        };



        if (TradeConfig.RouteScriptList.Count < TradeConfig.SelectedRouteListIndex)

            return null;



        var selectedRouteList = TradeConfig.RouteScriptList[TradeConfig.SelectedRouteListIndex];

        if (selectedRouteList == null || !TradeConfig.RouteScripts.ContainsKey(selectedRouteList))

            return null;



        foreach (var fileName in TradeConfig.RouteScripts[selectedRouteList]

                     .Select(Path.GetFileNameWithoutExtension))

            inputDialog.Selector.Items.Add(fileName);



        if (inputDialog.ShowDialog(Application.OpenForms[0]) != DialogResult.OK)

        {

            Log.Error("[Trade] No route found!");



            Kernel.Bot.Stop();



            _blockedByRouteDialog = false;



            return null;

        }



        _blockedByRouteDialog = false;



        return TradeConfig.RouteScripts[selectedRouteList].FirstOrDefault(s =>

            Path.GetFileNameWithoutExtension(s) == inputDialog.Selector.SelectedItem.ToString());

    }



    /// <summary>

    ///     Returns the route file name based on configuration.

    /// </summary>

    public string GetNextRouteFile()

    {

        // Priority 1: Smart City-to-City Trade Route

        if (TradeConfig.UseCityToCity)

        {

            var cityRoute = GetCityToCityRouteFile(log: true);

            if (cityRoute != null)

                return cityRoute;

        }



        // Priority 2: Custom Route List

        var selectedRouteList = TradeConfig.RouteScriptList[TradeConfig.SelectedRouteListIndex];



        if (!TradeConfig.RouteScripts.ContainsKey(selectedRouteList))

        {

            Log.Warn("[Trade] Next route not found!");



            return null;

        }



        var random = new Random();



        //Randomize next route, but only consider routes enabled in the route table.

        foreach (var file in TradeConfig.RouteScripts[selectedRouteList]

                     .Where(TradeConfig.IsRouteActive)

                     .OrderBy(_ => random.Next(0, 100)))

        {

            ScriptManager.Load(file);



            var walkScript = ScriptManager.GetWalkScript();

            if (walkScript == null || walkScript.Count == 0)

                continue;



            var startPosition = walkScript.FirstOrDefault();

            if (startPosition.Region.Id != Game.Player.Position.Region.Id)

                continue;



            return file;

        }



        return null;

    }



    /// <summary>

    ///     Gets the route file for the selected City-to-City pair, alternating per loop.

    /// </summary>

    public static string GetCityToCityRouteFile(bool log = false)
    {
        if (string.IsNullOrEmpty(FirstCity) || string.IsNullOrEmpty(SecondCity))
            InitializeCityPair();

        var runIndex = TradeConfig.CurrentTradeLoop;
        string fromCity;
        string toCity;

        if (runIndex % 2 == 0)
        {
            fromCity = FirstCity;
            toCity = SecondCity;
        }
        else
        {
            fromCity = SecondCity;
            toCity = FirstCity;
        }

        if (log)
        {
            var totalLoops = TradeConfig.TradeLoops <= 0 ? "∞" : TradeConfig.TradeLoops.ToString();
            Log.Notify($"[CityTrade] مسار الجولة (#{runIndex + 1} من {totalLoops}): من {fromCity} إلى {toCity}");
        }

        return FindCityRouteFile(fromCity, toCity);

    }



    /// <summary>

    ///     Automatically detects the city where the player is currently located.

    /// </summary>

    public static string DetectCurrentCity()

    {

        if (Game.Player == null) return "jangan";



        var regionId = Game.Player.Position.Region.Id;

        var xSec = Game.Player.Position.Region.X;

        var ySec = Game.Player.Position.Region.Y;



        // Check exact region IDs

        if (regionId == 25000) return "jangan";

        if (regionId == 26265) return "donwhang";

        if (regionId == 23088) return "hotan";

        if (regionId == 23687) return "samarkand";

        if (regionId == 26959 || regionId == 22618 || regionId == 22106) return "constantinople";



        // Check sectors

        if (xSec == 168 && ySec == 97) return "jangan";

        if (xSec == 167 && ySec == 96) return "donwhang";

        if (xSec == 163 && ySec == 93) return "hotan";

        if (xSec == 165 && ySec == 99) return "samarkand";

        if ((xSec <= 162 && ySec >= 100) || (xSec == 79 && ySec == 105)) return "constantinople";



        // Fallback: distance to city centers

        var distJangan = Game.Player.Position.DistanceTo(new Position(168, 97, 1000, 1200, 0));

        var distDonwhang = Game.Player.Position.DistanceTo(new Position(167, 96, 3500, 2000, 0));

        var distHotan = Game.Player.Position.DistanceTo(new Position(163, 93, 100, 100, 0));

        var distSamarkand = Game.Player.Position.DistanceTo(new Position(165, 99, 2000, 2000, 0));

        var distConstantinople = Game.Player.Position.DistanceTo(new Position(79, 105, 500, 500, 0));



        var min = Math.Min(distJangan, Math.Min(distDonwhang, Math.Min(distHotan, Math.Min(distSamarkand, distConstantinople))));

        if (min == distJangan) return "jangan";

        if (min == distDonwhang) return "donwhang";

        if (min == distHotan) return "hotan";

        if (min == distSamarkand) return "samarkand";

        return "constantinople";

    }



    /// <summary>

    ///     Finds the matching script file between fromCity and toCity in Build\Data\Scripts\Trade.

    /// </summary>

    public static string FindCityRouteFile(string fromCity, string toCity)

    {

        var tradeDir = Path.Combine(ScriptManager.InitialDirectory, "Trade");

        if (!Directory.Exists(tradeDir)) return null;



        var from = (fromCity ?? "").Trim().ToLowerInvariant();

        var to = (toCity ?? "").Trim().ToLowerInvariant();



        var files = Directory.GetFiles(tradeDir, "*.rbs");

        foreach (var file in files)

        {

            var fileName = Path.GetFileNameWithoutExtension(file).ToLowerInvariant();

            var parts = fileName.Split(new[] { " to " }, StringSplitOptions.RemoveEmptyEntries);

            if (parts.Length != 2) continue;



            var f = parts[0].Trim();

            var t = parts[1].Trim();



            var fromMatch = (from.Contains("jangan") && f.Contains("jangan")) ||

                            (from.Contains("donwhang") && (f.Contains("donwhang") || f.Contains("downhang"))) ||

                            (from.Contains("hotan") && f.Contains("hotan")) ||

                            (from.Contains("samarkand") && f.Contains("samarkand")) ||

                            (from.Contains("constantinople") && (f.Contains("constantinople") || f.Contains("costantinople")));



            var toMatch = (to.Contains("jangan") && t.Contains("jangan")) ||

                          (to.Contains("donwhang") && (t.Contains("donwhang") || t.Contains("downhang"))) ||

                          (to.Contains("hotan") && t.Contains("hotan")) ||

                          (to.Contains("samarkand") && t.Contains("samarkand")) ||

                          (to.Contains("constantinople") && (t.Contains("constantinople") || t.Contains("costantinople")));



            if (fromMatch && toMatch && TradeConfig.IsRouteActive(file))

                return file;

        }



        return null;

    }



    /// <summary>

    ///     Stops the bundle

    /// </summary>

    public void Stop()

    {

        CurrentRouteFile = null;

        TownscriptRunning = false;

        WaitingForHunter = false;

        WaitingForTracePlayer = false;

        _waitForReturnScroll = false;

        _returnScrollStarted = DateTime.MinValue;



        _blockedByRouteDialog = false;

        _lastScriptIsTownScript = false;



        ScriptManager.Stop(error: true);

    }



    private static void ExecuteDismountTakeOffJobAndStartDefault()

    {

        try

        {

            Log.Notify("[CityTrade] جاري الترجل من وسيلة النقل...");

            if (Game.Player?.HasActiveVehicle == true)

                Game.Player.JobTransport?.Dismount();



            Thread.Sleep(1500);



            Log.Notify("[CityTrade] جاري خلع بدلة الـ Job...");

            var jobItem = Game.Player?.Inventory?.GetEquippedPartItems()?.FirstOrDefault(i => i?.Record != null && i.Record.IsJobOutfit);

            if (jobItem != null && Game.Player?.Inventory != null)

            {

                var freeSlot = Game.Player.Inventory.GetFreeSlot();

                if (freeSlot != 0)

                {

                    Game.Player.Inventory.MoveItem(jobItem.Slot, freeSlot);

                    Log.Notify($"[CityTrade] تم خلع بدلة الـ Job بنجاح ووضعها في الخانة {freeSlot}.");

                }

                else

                {

                    Log.Warn("[CityTrade] تعذر خلع بدلة الـ Job: الحقيبة ممتلئة!");

                }

            }



            Thread.Sleep(1500);



            Log.Notify("[CityTrade] جاري تفعيل البوت الافتراضي (التدريب)...");

            var defaultBot = Kernel.BotbaseManager.Bots.FirstOrDefault(b => b.Value.Name == "RSBot.Default").Value;

            if (defaultBot != null)

            {

                Kernel.Bot?.SetBotbase(defaultBot);

                Kernel.Bot?.Start();

                Log.Notify("[CityTrade] تم بدء تشغيل بوت التدريب الافتراضي بنجاح!");

            }

            else

            {

                Log.Warn("[CityTrade] لم يتم العثور على بوت التدريب الافتراضي (RSBot.Default)!");

                Kernel.Bot?.Stop();

            }

        }

        catch (Exception ex)

        {

            Log.Error($"[CityTrade] خطأ أثناء تنفيذ الترجل وخلع بدلة الـ Job: {ex.Message}");

        }

    }



}

