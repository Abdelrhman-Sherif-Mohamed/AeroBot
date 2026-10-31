using System;
using System.IO;

namespace RSBot.Core.Components;

/// <summary>
///     Writes the important session events (client startup, proxy connection, disconnects, ...)
///     into a plain text file, so that problems can still be analyzed after the bot was closed.
/// </summary>
public static class SessionLog
{
    /// <summary>
    ///     The lock object
    /// </summary>
    private static readonly object _lock = new();

    /// <summary>
    ///     The path of the current log file
    /// </summary>
    private static string _filePath;

    /// <summary>
    ///     Gets the path of the log file
    /// </summary>
    private static string FilePath
    {
        get
        {
            if (_filePath != null)
                return _filePath;

            _filePath = Path.Combine(Kernel.BasePath, "Data", "Logs", "Session", $"{DateTime.Now:dd-MM-yyyy}.log");

            return _filePath;
        }
    }

    /// <summary>
    ///     Appends the given message to the session log.
    /// </summary>
    /// <param name="message">The message</param>
    public static void Write(string message)
    {
        try
        {
            if (!GlobalConfig.Get("RSBot.Log.logEnabled", true))
                return;

            lock (_lock)
            {
                var directory = Path.GetDirectoryName(FilePath);
                if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                    Directory.CreateDirectory(directory);

                File.AppendAllText(FilePath, $"[{DateTime.Now:HH:mm:ss}] {message}{Environment.NewLine}");
            }
        }
        catch
        {
            //A failing log must never break the bot
        }
    }

    /// <summary>
    ///     Appends the given exception to the session log.
    /// </summary>
    /// <param name="context">The context in which the exception occurred</param>
    /// <param name="exception">The exception</param>
    public static void Write(string context, Exception exception)
    {
        Write($"{context}: {exception}");
    }
}
