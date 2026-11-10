using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using RSBot.Core.Components.Scripting;
using RSBot.Core.Event;
using RSBot.Core.Objects;

namespace RSBot.Core.Components;

public class ScriptManager
{
    /// <summary>
    ///     Gets the initial directory
    /// </summary>
    public static string InitialDirectory => Path.Combine(Kernel.BasePath, "Data", "Scripts");

    /// <summary>
    ///     Gets or sets the file.
    /// </summary>
    /// <value>
    ///     The file.
    /// </value>
    public static string File { get; set; }

    /// <summary>
    ///     Gets or sets the commands.
    /// </summary>
    /// <value>
    ///     The commands.
    /// </value>
    public static string[] Commands { get; set; }

    /// <summary>
    ///     Gets or sets a value indicating whether this <see cref="ScriptManager" /> is running.
    /// </summary>
    /// <value>
    ///     <c>true</c> if running; otherwise, <c>false</c>.
    /// </value>
    public static bool Running { get; set; }

    /// <summary>
    ///     Gets the command handlers.
    /// </summary>
    /// <value>The command handlers.</value>
    public static List<IScriptCommand> CommandHandlers { get; private set; }

    /// <summary>
    ///     Gets the index of the current line.
    /// </summary>
    /// <value>
    ///     The index of the current line.
    /// </value>
    public static int CurrentLineIndex { get; private set; }

    /// <summary>
    ///     Gets or sets the argument separator.
    ///     Modify this value in case of custom script syntax support
    /// </summary>
    /// <value>
    ///     The argument separator.
    /// </value>
    public static char ArgumentSeparator { get; set; } = ' ';

    /// <summary>
    ///     Separators supported across scripts (supports space, comma, and tab for .rbs and .vb).
    /// </summary>
    public static readonly char[] CommandSeparators = { ' ', ',', '\t' };

    public static bool Paused { get; private set; }

    private static readonly object _loopLock = new();
    private static volatile bool _isExecutingLoop;

    public static void Initialize()
    {
        CommandHandlers = new List<IScriptCommand>(10);

        var type = typeof(IScriptCommand);
        var types = AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(s => s.GetTypes())
            .Where(p => type.IsAssignableFrom(p) && !p.IsInterface).ToArray();

        foreach (var handler in types)
        {
            var instance = (IScriptCommand)Activator.CreateInstance(handler);

            CommandHandlers.Add(instance);
        }
    }

    /// <summary>
    ///     Loads the specified file.
    /// </summary>
    /// <param name="file">The file.</param>
    public static void Load(string file)
    {
        if (!System.IO.File.Exists(file))
        {
            Log.Notify($"Cannot load file [{file}] (File does not exist)!");
            return;
        }

        Running = false;
        File = file;
        Commands = System.IO.File.ReadAllLines(file);

        EventManager.FireEvent("OnLoadScript");
    }

    /// <summary>
    ///     Loads the specified commands.
    /// </summary>
    /// <param name="commands">The commands.</param>
    public static void Load(string[] commands)
    {
        Commands = commands;
        EventManager.FireEvent("OnLoadScript");
    }

    /// <summary>
    ///     Pauses the command execution.
    /// </summary>
    public static void Pause()
    {
        Paused = true;

        EventManager.FireEvent("OnPauseScript");
    }

    /// <summary>
    ///     Resumes the currently paused or running script from its current line index.
    /// </summary>
    public static void ResumeScript(bool ignoreBotRunning = false)
    {
        if (Commands == null || Commands.Length == 0)
        {
            if (!string.IsNullOrEmpty(File) && System.IO.File.Exists(File))
            {
                Load(File);
            }
            else
            {
                LogScriptMessage("No script loaded to resume.", CurrentLineIndex, LogLevel.Warning);
                return;
            }
        }

        Running = true;
        Paused = false;

        Log.Notify($"[Script] Resuming script execution from line #{CurrentLineIndex}");
        Task.Run(() => ExecuteScriptLoop(ignoreBotRunning));
    }

    /// <summary>
    ///     Runs this instance starting from a specific line index.
    /// </summary>
    public static void RunScript(int startLineIndex, bool ignoreBotRunning = false)
    {
        if (Commands == null || Commands.Length == 0)
        {
            LogScriptMessage("No script loaded.", 0, LogLevel.Warning);
            return;
        }

        if (Running && !Paused)
            return;

        Running = true;
        Paused = false;
        CurrentLineIndex = Math.Max(0, Math.Min(startLineIndex, Commands.Length - 1));

        Log.Debug($"[Script] Running script starting from line #{CurrentLineIndex}");

        ExecuteScriptLoop(ignoreBotRunning);
    }

    /// <summary>
    ///     Runs this instance.
    /// </summary>
    public static void RunScript(bool useNearbyWaypoint = true, bool ignoreBotRunning = false)
    {
        if (Commands == null || Commands.Length == 0)
        {
            LogScriptMessage("No script loaded.", 0, LogLevel.Warning);

            return;
        }

        if (Running && !Paused)
            return;

        Running = true;
        Paused = false;
        CurrentLineIndex = useNearbyWaypoint ? FindNearestMoveCommandLine() : 0;

        if (CurrentLineIndex != 0)
            Log.Debug($"[Script] Found nearby walk position at line #{CurrentLineIndex}");

        ExecuteScriptLoop(ignoreBotRunning);
    }

    private static void ExecuteScriptLoop(bool ignoreBotRunning)
    {
        lock (_loopLock)
        {
            if (_isExecutingLoop)
            {
                Log.Debug("[Script] An execution loop is already active. Ignoring duplicate call.");
                return;
            }
            _isExecutingLoop = true;
        }

        try
        {
            if (Commands == null || Commands.Length == 0 || Commands.Length <= CurrentLineIndex)
                return;

            var error = false;
            var consecutiveFailures = 0;
            var commandList = Commands;
            if (commandList == null)
                return;

            for (var i = CurrentLineIndex; i < commandList.Length; i++)
            {
                CurrentLineIndex = i;

                if (!Running || Paused || (!Kernel.Bot.Running && !ignoreBotRunning))
                {
                    error = true;
                    break;
                }

                var scriptLine = commandList[i];
                if (string.IsNullOrEmpty(scriptLine))
                    continue;

                Log.Debug($"[Script] Executing line #{CurrentLineIndex}");
                Log.Status("Running walk script");

                var arguments = scriptLine.Split(CommandSeparators, StringSplitOptions.RemoveEmptyEntries);
                if (arguments.Length == 0)
                    continue;

                var commandName = arguments[0].Trim();

                if (string.IsNullOrEmpty(commandName)
                    || commandName.StartsWith("//")
                    || commandName.StartsWith("#"))
                    continue;

                var handler = CommandHandlers.FirstOrDefault(h => h.Name == commandName);
                if (handler == null)
                {
                    LogScriptMessage("No script command handler found.", CurrentLineIndex, LogLevel.Warning);
                    continue;
                }

                if (handler.IsBusy && Running && !Paused)
                {
                    error = true;
                    LogScriptMessage("The script command is still busy, stopping script execution.", CurrentLineIndex,
                        LogLevel.Debug, commandName);
                    break;
                }

                EventManager.FireEvent("OnScriptStartExecuteCommand", handler, CurrentLineIndex);
                var executionResult = handler.Execute(arguments.Skip(1).ToArray());
                EventManager.FireEvent("OnScriptFinishExecuteCommand", handler, executionResult, CurrentLineIndex);

                if (executionResult == false)
                {
                    LogScriptMessage("The execution of the script command failed.", CurrentLineIndex, LogLevel.Warning,
                        commandName);

                    if (!Running || Paused || (!Kernel.Bot.Running && !ignoreBotRunning))
                    {
                        error = true;
                        break;
                    }

                    // Critical commands can never be skipped (teleporting, buying, selling, mounting, etc.)
                    var isCritical = commandName is "teleport" or "buy" or "sell" or "dismount" or "mount" or "repair";
                    if (isCritical)
                    {
                        Log.Warn($"[Script] Critical command [{commandName}] failed at line #{CurrentLineIndex}. Aborting script execution!");
                        error = true;
                        break;
                    }

                    consecutiveFailures++;
                    if (consecutiveFailures >= 3)
                    {
                        Log.Warn($"[Script] Multiple consecutive commands failed ({consecutiveFailures}) at line #{CurrentLineIndex}. Aborting script execution!");
                        error = true;
                        break;
                    }

                    continue;
                }

                consecutiveFailures = 0;
            }

            if (!Paused)
                Stop(error);
        }
        finally
        {
            _isExecutingLoop = false;
        }
    }

    /// <summary>
    ///     Stops this instance.
    /// </summary>
    public static void Stop(bool error = false)
    {
        Running = false;
        Paused = false;

        foreach (var handler in CommandHandlers)
        {
            try
            {
                handler.Stop();
            }
            catch { }
        }

        EventManager.FireEvent("OnFinishScript", error);
    }

    /// <summary>
    ///     A convenience function that returns all positions in the walk script.
    ///     Supports both .rbs (5 args) and .vb (3 args: worldX, worldY, worldZ) formats.
    /// </summary>
    /// <returns></returns>
    public static List<Position> GetWalkScript()
    {
        var walkCommands = Commands.Where(c => c.Trim().StartsWith("move", StringComparison.OrdinalIgnoreCase));

        return walkCommands.Select(command => command.Split(CommandSeparators, StringSplitOptions.RemoveEmptyEntries).Skip(1).ToArray()).Select(ParsePosition)
            .ToList();
    }

    /// <summary>
    ///     Parses the position from the given arguments.
    ///     Supports both 3-arg vBot syntax (WorldX, WorldY, WorldZ) and 5-arg RSBot syntax.
    /// </summary>
    /// <param name="args">The arguments.</param>
    /// <returns></returns>
    private static Position ParsePosition(string[] args)
    {
        if (args == null) return default;

        var tokens = args
            .Select(a => a?.Trim().TrimEnd(','))
            .Where(a => !string.IsNullOrEmpty(a))
            .ToArray();

        // Format 1: 3 arguments -> vBot format (WorldX, WorldY, WorldZ)
        if (tokens.Length == 3)
        {
            if (float.TryParse(tokens[0], NumberStyles.Float, CultureInfo.InvariantCulture, out var wx) &&
                float.TryParse(tokens[1], NumberStyles.Float, CultureInfo.InvariantCulture, out var wy) &&
                float.TryParse(tokens[2], NumberStyles.Float, CultureInfo.InvariantCulture, out var wz))
            {
                return Position.FromWorld(wx, wy, wz);
            }

            return default;
        }

        // Format 2: 5 arguments -> RSBot native format (XOffset, YOffset, ZOffset, XSector, YSector)
        if (tokens.Length >= 5)
        {
            if (!float.TryParse(tokens[0], NumberStyles.Float, CultureInfo.InvariantCulture, out var xOffset)
                || !float.TryParse(tokens[1], NumberStyles.Float, CultureInfo.InvariantCulture, out var yOffset)
                || !float.TryParse(tokens[2], NumberStyles.Float, CultureInfo.InvariantCulture, out var zOffset)
                || !byte.TryParse(tokens[3], NumberStyles.Integer, CultureInfo.InvariantCulture, out var xSector)
                || !byte.TryParse(tokens[4], NumberStyles.Integer, CultureInfo.InvariantCulture, out var ySector))
                return default;

            return new Position(xSector, ySector, xOffset, yOffset, zOffset);
        }

        return default;
    }

    /// <summary>
    ///     Logs the script message.
    /// </summary>
    /// <param name="message">The message.</param>
    /// <param name="line">The line.</param>
    /// <param name="level">The level.</param>
    /// <param name="command">The command.</param>
    private static void LogScriptMessage(string message, int line, LogLevel level = LogLevel.Notify,
        string command = null)
    {
        if (command == null)
            command = "<none>";

        Log.Append(level, $"[Script] {message} (command={command}; line={line})");
    }

    /// <summary>
    ///     Finds the line index of the 'buy-goods' command if present in the script.
    /// </summary>
    public static int FindBuyGoodsCommandLine()
    {
        if (Commands == null) return -1;
        for (var i = 0; i < Commands.Length; i++)
        {
            var cmd = Commands[i].Trim();
            if (cmd.StartsWith("buy-goods", StringComparison.OrdinalIgnoreCase))
                return i;
        }
        return -1;
    }

    /// <summary>
    ///     Finds the nearest walk command line at or after a minimum line index.
    /// </summary>
    public static int FindNearestMoveCommandLineAfter(int afterLine = 0)
    {
        if (Commands == null || Commands.Length == 0) return 0;

        var line = -1;
        var allMoves = new Dictionary<int, Position>();

        foreach (var command in Commands)
        {
            line++;
            if (line < afterLine)
                continue;

            if (command.Trim().StartsWith("//") ||
                command.Trim().StartsWith("#") ||
                !command.Trim().StartsWith("move", StringComparison.OrdinalIgnoreCase) ||
                string.IsNullOrWhiteSpace(command))
                continue;

            var args = command.Split(CommandSeparators, StringSplitOptions.RemoveEmptyEntries).Skip(1).ToArray();
            var curPos = ParsePosition(args);

            allMoves[line] = curPos;
        }

        if (allMoves.Count == 0)
            return Math.Max(0, afterLine);

        // Find the absolute closest forward waypoint in the script
        var closest = allMoves.MinBy(c => c.Value.DistanceToPlayer());

        Log.Notify($"[Script] تم تحديد أقرب نقطة أمامية على المسار: السطر #{closest.Key} على مسافة {Math.Round(closest.Value.DistanceToPlayer(), 1)} متر.");

        return closest.Key;
    }

    /// <summary>
    ///     Finds the nearest walk command line before or at a maximum line index.
    /// </summary>
    public static int FindNearestMoveCommandLineBefore(int beforeLine)
    {
        if (Commands == null || Commands.Length == 0) return 0;

        var line = -1;
        var allMoves = new Dictionary<int, Position>();

        foreach (var command in Commands)
        {
            line++;
            if (line > beforeLine)
                break;

            if (command.Trim().StartsWith("//") ||
                command.Trim().StartsWith("#") ||
                !command.Trim().StartsWith("move", StringComparison.OrdinalIgnoreCase) ||
                string.IsNullOrWhiteSpace(command))
                continue;

            var args = command.Split(CommandSeparators, StringSplitOptions.RemoveEmptyEntries).Skip(1).ToArray();
            var curPos = ParsePosition(args);

            allMoves[line] = curPos;
        }

        if (allMoves.Count == 0)
            return 0;

        var closest = allMoves.MinBy(c => c.Value.DistanceToPlayer());
        Log.Notify($"[Script] تم تحديد أقرب نقطة داخل المدينة: السطر #{closest.Key} على مسافة {Math.Round(closest.Value.DistanceToPlayer(), 1)} متر.");
        return closest.Key;
    }

    /// <summary>
    ///     Finds the nearest walk command line.
    /// </summary>
    /// <returns></returns>
    public static int FindNearestMoveCommandLine()
    {
        return FindNearestMoveCommandLineAfter(0);
    }
}