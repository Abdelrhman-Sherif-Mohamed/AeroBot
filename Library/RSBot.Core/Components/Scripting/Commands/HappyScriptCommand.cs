using System.Collections.Generic;

namespace RSBot.Core.Components.Scripting.Commands;

internal class HappyScriptCommand : IScriptCommand
{
    #region Properties

    /// <summary>
    /// Gets the name of the script command.
    /// </summary>
    public string Name => "happy";

    /// <summary>
    /// Gets a value indicating whether this command is currently running.
    /// </summary>
    public bool IsBusy { get; private set; }

    /// <summary>
    /// Gets the arguments dictionary.
    /// </summary>
    public Dictionary<string, string> Arguments => new();

    #endregion Properties

    #region Methods

    /// <summary>
    /// Executes the happy script command.
    /// </summary>
    public bool Execute(string[] arguments = null)
    {
        if (!ScriptManager.Running)
        {
            IsBusy = false;
            return false;
        }

        try
        {
            IsBusy = true;
            Log.Notify("[Script] Executing happy event command...");
            return HappyManager.Execute();
        }
        finally
        {
            IsBusy = false;
        }
    }

    /// <summary>
    /// Stops execution.
    /// </summary>
    public void Stop()
    {
        IsBusy = false;
    }

    #endregion Methods
}
