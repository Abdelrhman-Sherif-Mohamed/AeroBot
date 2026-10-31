using System.Collections.Generic;
using System.Linq;
using System.Threading;
using RSBot.Core;
using RSBot.Core.Components.Scripting;

namespace RSBot.Trade.Components.Scripting;

internal class SummonTransportScriptCommand : IScriptCommand
{
    public string Name => "summon-transport";

    public bool IsBusy { get; private set; }

    public Dictionary<string, string> Arguments => null;

    public bool Execute(string[] arguments = null)
    {
        if (Game.Player?.JobTransport != null)
            return true;

        if (IsBusy)
            return false;

        try
        {
            IsBusy = true;
            var jobTransportItem = Game.Player?.Inventory?
                .GetNormalPartItems(i => i?.Record != null && (i.Record.CodeName.Contains("COS_T_") || (i.Record.TypeID3 == 2 && i.Record.TypeID4 == 1)))
                .FirstOrDefault();

            if (jobTransportItem != null)
            {
                Log.Notify($"[Script] استدعاء حيوان النقل [{jobTransportItem.Record.GetRealName()}]...");
                jobTransportItem.Use();
            }
            else
            {
                Log.Warn("[Script] تعذر استدعاء حيوان النقل: لا يوجد سكرول في الحقيبة.");
                return false;
            }

            // Wait up to 3 seconds for transport to spawn
            for (var i = 0; i < 30; i++)
            {
                if (Game.Player?.JobTransport != null)
                    break;

                Thread.Sleep(100);
            }

            return true;
        }
        finally
        {
            IsBusy = false;
        }
    }

    public void Stop()
    {
        IsBusy = false;
    }
}
