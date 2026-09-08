using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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

    public static bool Paused { get; private set; }

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

        if (Commands == null || Commands.Length == 0 || Commands.Length < CurrentLineIndex)
            return;

        var error = false;
        foreach (var scriptLine in Commands?.Skip(CurrentLineIndex))
        {
            if (!Running || Paused || (!Kernel.Bot.Running && !ignoreBotRunning))
            {
                error = true;

                break;
            }

            Log.Debug($"[Script] Executing line #{CurrentLineIndex}");
            Log.Status("Running walk script");

            if (string.IsNullOrWhiteSpace(scriptLine))
                continue;

            var trimmedLine = scriptLine.Trim();
            if (trimmedLine.StartsWith("//") || trimmedLine.StartsWith("#"))
                continue;

            Log.Debug($"[Script] Executing line #{CurrentLineIndex}");
            Log.Status("Running walk script");

            var tokens = trimmedLine.Split(new[] { ' ', '\t', ',' }, StringSplitOptions.RemoveEmptyEntries);
            if (tokens.Length == 0)
                continue;

            var commandName = tokens[0].ToLowerInvariant();
            var arguments = tokens.Skip(1).ToArray();

            var handler = CommandHandlers.FirstOrDefault(h => h.Name.Equals(commandName, StringComparison.OrdinalIgnoreCase));
            if (handler == null)
            {
                LogScriptMessage("No script command handler found.", CurrentLineIndex, LogLevel.Warning, commandName);

                continue; //No matching handler found for this command
            }

            if (handler.IsBusy && Running && !Paused)
            {
                error = true;
                LogScriptMessage("The script command is still busy, stopping script execution.", CurrentLineIndex,
                    LogLevel.Debug, commandName);

                break;
            }

            EventManager.FireEvent("OnScriptStartExecuteCommand", handler, CurrentLineIndex);
            var executionResult = handler.Execute(arguments);
            EventManager.FireEvent("OnScriptFinishExecuteCommand", handler, executionResult, CurrentLineIndex);

            if (executionResult == false)
            {
                LogScriptMessage("The execution of the script command failed.", CurrentLineIndex, LogLevel.Warning,
                    commandName);

                continue;
            }

            CurrentLineIndex++;

            if (CurrentLineIndex > Commands?.Length - 1)
                break;
        }

        if (!Paused)
            Stop(error);
    }

    /// <summary>
    ///     Stops this instance.
    /// </summary>
    public static void Stop(bool error = false)
    {
        Running = false;
        Commands = null;
        File = null;

        foreach (var handler in CommandHandlers)
            handler.Stop();

        EventManager.FireEvent("OnFinishScript", error);
    }

    /// <summary>
    ///     A convenience function that returns all positions in the walk script.
    /// </summary>
    /// <returns></returns>
    public static List<Position> GetWalkScript()
    {
        if (Commands == null || Commands.Length == 0)
            return new List<Position>();

        var walkCommands = Commands.Where(c => !string.IsNullOrWhiteSpace(c) && c.Trim().StartsWith("move", StringComparison.OrdinalIgnoreCase));

        return walkCommands
            .Select(command => command.Split(new[] { ' ', '\t', ',' }, StringSplitOptions.RemoveEmptyEntries).Skip(1).ToArray())
            .Select(ParsePosition)
            .Where(p => p.Region.Id != 0 || p.XOffset != 0 || p.YOffset != 0)
            .ToList();
    }

    /// <summary>
    ///     Parses the position from the given arguments.
    ///     Supports both 5-argument native RSBot format and 3-argument Silkroad world coordinates format.
    /// </summary>
    /// <param name="args">The arguments.</param>
    /// <returns></returns>
    public static Position ParsePosition(string[] args)
    {
        if (args == null || args.Length < 3)
            return default;

        var cleaned = args.Select(a => a.Trim().TrimEnd(',')).ToArray();

        // 5-argument native format (xOffset, yOffset, zOffset, xSector, ySector)
        if (cleaned.Length >= 5 &&
            float.TryParse(cleaned[0], System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out var xOffset) &&
            float.TryParse(cleaned[1], System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out var yOffset) &&
            float.TryParse(cleaned[2], System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out var zOffset) &&
            byte.TryParse(cleaned[3], out var xSector) &&
            byte.TryParse(cleaned[4], out var ySector))
        {
            return new Position(xSector, ySector, xOffset, yOffset, zOffset);
        }

        // 3-argument World Coordinates format (x, y, z) from .vb scripts
        if (float.TryParse(cleaned[0], System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out var worldX) &&
            float.TryParse(cleaned[1], System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out var worldY))
        {
            float.TryParse(cleaned[2], System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out var worldZ);
            var pos = new Position(worldX, worldY)
            {
                ZOffset = worldZ
            };
            return pos;
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
    ///     Finds the nearest walk command line.
    /// </summary>
    /// <returns></returns>
    private static int FindNearestMoveCommandLine()
    {
        var playerPos = Game.Player.Movement.Source;

        var line = -1;
        var moveCommands = new Dictionary<int, Position>();

        foreach (var command in Commands)
        {
            line++;

            if (string.IsNullOrWhiteSpace(command))
                continue;

            var trimmed = command.Trim();
            if (trimmed.StartsWith("//") ||
                trimmed.StartsWith("#") ||
                !trimmed.StartsWith("move", StringComparison.OrdinalIgnoreCase))
                continue;

            var tokens = trimmed.Split(new[] { ' ', '\t', ',' }, StringSplitOptions.RemoveEmptyEntries);
            if (tokens.Length < 4) continue;
            var args = tokens.Skip(1).ToArray();
            var curPos = ParsePosition(args);
            var distance = curPos.DistanceToPlayer();

            if (distance < 150)
                moveCommands.Add(line, curPos);
        }

        return moveCommands.Count == 0 ? 0 : moveCommands.MinBy(c => c.Value.DistanceToPlayer()).Key;
    }
}