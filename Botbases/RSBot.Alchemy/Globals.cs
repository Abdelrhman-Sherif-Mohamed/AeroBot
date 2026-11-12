using RSBot.Alchemy.Bot;
using RSBot.Alchemy.Views;

namespace RSBot.Alchemy;

internal class Globals
{
    private static readonly object _initLock = new();

    static Globals()
    {
        lock (_initLock)
        {
            Botbase = new Botbase();
            View = new Main();
        }
    }

    /// <summary>
    ///     Gets or sets the bot base.
    /// </summary>
    public static Botbase Botbase { get; set; }

    /// <summary>
    ///     Gets the root alchemy view.
    /// </summary>
    public static Main View { get; }
}
