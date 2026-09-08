namespace RSBot.CommandCenter.Components.Command;

/// <summary>
///     Holds the arguments of the currently executing in-game chat command (e.g. "!trace PlayerName").
///     Set by <see cref="Network.Hook.ChatRequestHook"/> before calling CommandManager.Execute.
/// </summary>
internal static class InGameCommandContext
{
    public static string[] Args { get; set; } = System.Array.Empty<string>();

    public static string Arg0 => Args.Length > 0 ? Args[0] : string.Empty;

    public static string FullArgs => string.Join(" ", Args);
}
