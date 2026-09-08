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

    public Dictionary<string, string> Arguments => new();

    public bool Execute(string[] arguments = null)
    {
        if (Game.Player == null)
            return false;

        if (Game.Player.JobTransport != null)
        {
            Log.Notify("[Trade] Job transport is already summoned.");
            return true;
        }

        try
        {
            IsBusy = true;

            while (Game.Player.InAction)
                Thread.Sleep(100);

            var jobTransportItem = Game.Player.Inventory
                .GetNormalPartItems(i => (i.Record.CodeName.Contains("COS_T_") || i.Record.CodeName.Contains("ITEM_ETC_TRANS_")) && i.Record.Tid == 4588)
                .FirstOrDefault();

            if (jobTransportItem == null)
            {
                jobTransportItem = Game.Player.Inventory
                    .GetNormalPartItems(i => i.Record.CodeName.Contains("COS_T_"))
                    .FirstOrDefault();
            }

            if (jobTransportItem == null)
            {
                Log.Warn("[Trade] Can not summon transport: No transport scroll found in inventory!");
                return true;
            }

            Log.Notify($"[Trade] Summoning transport scroll [{jobTransportItem.Record.GetRealName()}]...");
            jobTransportItem.Use();

            var timeout = 50;
            while (Game.Player.JobTransport == null && timeout-- > 0)
            {
                Thread.Sleep(100);
            }

            if (Game.Player.JobTransport != null)
            {
                Log.Notify("[Trade] Job transport spawned successfully.");
                return true;
            }

            Log.Warn("[Trade] Timed out waiting for job transport to spawn.");
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
