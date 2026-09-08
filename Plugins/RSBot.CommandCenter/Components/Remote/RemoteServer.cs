using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RSBot.CommandCenter.Components.Command;
using RSBot.Core;
using RSBot.Core.Components;
using RSBot.Core.Objects.Skill;

namespace RSBot.CommandCenter.Components.Remote;

/// <summary>
///     Named-pipe control server for the standalone SonicMini app.
///     Protocol: one UTF-8 text command per line, one or more response lines.
///     Runs on its own background thread and never throws into the bot.
///     Pipe name: "SonicBotRemote".
/// </summary>
internal static class RemoteServer
{
    public const string PipeName = "SonicBotRemote";

    private static bool _started;
    private static string _lastError = "none";
    private static readonly object _errorLock = new();

    private static void SetLastError(string message)
    {
        lock (_errorLock)
        {
            _lastError = string.IsNullOrWhiteSpace(message) ? "none" : message;
        }
    }

    public static void Start()
    {
        if (_started)
            return;
        _started = true;

        Task.Run(ServerLoop);
    }

    private static void ServerLoop()
    {
        while (true)
        {
            NamedPipeServerStream server = null;
            try
            {
                server = new NamedPipeServerStream(
                    PipeName, PipeDirection.InOut, 10,
                    PipeTransmissionMode.Byte, PipeOptions.Asynchronous);

                server.WaitForConnection();

                // Hand each client to its own task so slow requests
                // (STARTCLIENT, ICONS) never block STATUS polls.
                var accepted = server;
                server = null;
                Task.Run(() => ServeAccepted(accepted));
            }
            catch
            {
                try
                {
                    server?.Dispose();
                }
                catch
                {
                }

                Task.Delay(1000).Wait();
            }
        }
    }

    private static void ServeAccepted(NamedPipeServerStream server)
    {
        try
        {
            ServeClient(server);
        }
        catch
        {
        }
        finally
        {
            try
            {
                if (server.IsConnected)
                    server.Disconnect();
            }
            catch
            {
            }

            try
            {
                server.Dispose();
            }
            catch
            {
            }
        }
    }

    private static void ServeClient(NamedPipeServerStream server)
    {
        try
        {
            using var reader = new StreamReader(server, Encoding.UTF8, false, 1024, true);
            using var writer = new StreamWriter(server, Encoding.UTF8, 1024, true) { AutoFlush = true };

            string line;
            while (server.IsConnected && (line = reader.ReadLine()) != null)
            {
                string response = null;
                List<string> extraLines = null;
                try
                {
                    response = Handle(line, out extraLines);
                }
                catch (Exception ex)
                {
                    response = "ERR " + ex.Message;
                }

                try
                {
                    writer.WriteLine(response ?? "ERR empty");
                    if (extraLines != null)
                        foreach (var extra in extraLines)
                            writer.WriteLine(extra);
                }
                catch
                {
                    break;
                }
            }
        }
        catch
        {
        }
    }

    private static string Handle(string line, out List<string> extraLines)
    {
        extraLines = null;

        if (string.IsNullOrWhiteSpace(line))
            return "ERR empty";

        var parts = line.Trim().Split(new[] { ' ' }, 2, StringSplitOptions.RemoveEmptyEntries);
        var cmd = parts[0].ToUpperInvariant();
        var arg = parts.Length > 1 ? parts[1] : string.Empty;

        switch (cmd)
        {
            case "PING":
                return "PONG SonicBot 2";

            case "STATUS":
                return StatusLine();

            case "START":
                return CommandManager.Execute("start", true) ? "OK started" : "ERR start failed";

            case "STOP":
                return CommandManager.Execute("stop", true) ? "OK stopped" : "ERR stop failed";

            case "TOWN":
                return CommandManager.Execute("town", true) ? "OK return scroll used" : "ERR no return scroll";

            case "STARTCLIENT":
                return StartClient();

            case "KILLCLIENT":
                ClientManager.Kill();
                return "OK client killed";

            case "GETDIR":
                return GetDir();

            case "SETDIR":
                return SetDir(arg);

            case "DIVISIONS":
                return Divisions(out extraLines);

            case "SERVERS":
                return Servers(arg, out extraLines);

            case "GETSEL":
                return GetSel();

            case "SETSEL":
                return SetSel(arg);

            case "SETTYPE":
                return SetType(arg);

            case "LASTERR":
                lock (_errorLock)
                {
                    return "LASTERR " + _lastError;
                }

            case "SKILLS":
                return SkillsList(arg, out extraLines);

            case "ADD":
                return AddSkill(arg);

            case "DEL":
                return DelSkill(arg);

            case "LEARN":
                return Learn(arg);

            case "ICON":
                return IconBase64(arg, out extraLines);

            case "ICONS":
                return IconsBatch(arg, out extraLines);

            default:
                return "ERR unknown command";
        }
    }

    private static string Fail(string message)
    {
        SetLastError(message);
        return message;
    }

    private static string StartClient()
    {
        try
        {
            if (ClientManager.IsRunning)
                return "ERR client already running";

            var dir = GlobalConfig.Get<string>("RSBot.SilkroadDirectory");
            if (string.IsNullOrWhiteSpace(dir) || !System.IO.Directory.Exists(dir))
                return Fail("ERR no game folder - pick sro_client.exe first");

            var exe = GlobalConfig.Get<string>("RSBot.SilkroadExecutable");
            if (string.IsNullOrWhiteSpace(exe))
            {
                exe = "sro_client.exe";
                GlobalConfig.Set("RSBot.SilkroadExecutable", exe);
                GlobalConfig.Save();
            }

            var fullPath = System.IO.Path.Combine(dir, exe);
            if (!System.IO.File.Exists(fullPath))
                return Fail($"ERR not found: {exe} in folder");

            // Like the big program: media.pk2 must sit next to the client
            if (!System.IO.File.Exists(System.IO.Path.Combine(dir, "media.pk2")))
                return Fail("ERR media.pk2 not found next to sro_client.exe - pick the real game folder");

            if (!IsAdministrator())
                return Fail("ERR cannot inject - close SonicBot and run it as Administrator");

            if (Game.ReferenceManager?.DivisionInfo == null)
                return Fail("ERR game data not loaded - restart SonicBot");

            // Same safety as the big window: clamp saved division/gateway
            // indices to the loaded lists (stale profile values crash Game.Start).
            try
            {
                var divisions = Game.ReferenceManager.DivisionInfo.Divisions;
                if (divisions == null || divisions.Count == 0)
                    return Fail("ERR no divisions loaded");

                var divIdx = GlobalConfig.Get<int>("RSBot.DivisionIndex");
                if (divIdx < 0 || divIdx >= divisions.Count)
                {
                    divIdx = divisions.Count - 1;
                    GlobalConfig.Set("RSBot.DivisionIndex", divIdx);
                }

                var gates = divisions[divIdx].GatewayServers;
                if (gates == null || gates.Count == 0)
                    return Fail("ERR no gateway servers for division");

                var gateIdx = GlobalConfig.Get<int>("RSBot.GatewayIndex");
                if (gateIdx < 0 || gateIdx >= gates.Count)
                {
                    gateIdx = 0;
                    GlobalConfig.Set("RSBot.GatewayIndex", gateIdx);
                }

                GlobalConfig.Save();
            }
            catch (Exception ex)
            {
                return Fail("ERR bad division/gateway: " + ex.Message);
            }

            Game.Clientless = false;

            // Launch in the background: the pipe stays responsive and the
            // mini watches progress via STATUS (client=1 when running).
            Task.Run(() =>
            {
                try
                {
                    Game.Start();
                    var started = ClientManager.Start().GetAwaiter().GetResult();
                    if (!started)
                    {
                        SetLastError("ERR client failed - run SonicBot as Administrator, check Log tab");
                        Log.Warn("Remote STARTCLIENT failed - run SonicBot as Administrator");
                    }
                }
                catch (Exception ex)
                {
                    SetLastError("ERR launch crashed: " + ex.Message);
                    Log.Error("Remote STARTCLIENT error: " + ex.Message);
                }
            });

            return "OK launching client...";
        }
        catch (Exception ex)
        {
            return Fail("ERR " + ex.Message);
        }
    }

    private static bool IsAdministrator()
    {
        try
        {
            var identity = System.Security.Principal.WindowsIdentity.GetCurrent();
            var principal = new System.Security.Principal.WindowsPrincipal(identity);
            return principal.IsInRole(System.Security.Principal.WindowsBuiltInRole.Administrator);
        }
        catch
        {
            return false;
        }
    }

    private static string GetDir()
    {
        try
        {
            var dir = GlobalConfig.Get<string>("RSBot.SilkroadDirectory") ?? string.Empty;
            var exe = GlobalConfig.Get<string>("RSBot.SilkroadExecutable") ?? "sro_client.exe";
            return $"DIR path={Encode(dir)} exe={Encode(exe)}";
        }
        catch (Exception ex)
        {
            return "ERR " + ex.Message;
        }
    }

    /// <summary>All divisions: "BEGIN n" + "idx|name" lines + END.</summary>
    private static string Divisions(out List<string> extraLines)
    {
        extraLines = new List<string>();

        try
        {
            var divisions = Game.ReferenceManager?.DivisionInfo?.Divisions;
            if (divisions == null || divisions.Count == 0)
                return "ERR no divisions loaded";

            for (var i = 0; i < divisions.Count; i++)
                extraLines.Add($"{i}|{Encode(divisions[i].Name ?? ("Division " + i))}");

            extraLines.Add("END");
            return $"BEGIN {divisions.Count}";
        }
        catch (Exception ex)
        {
            extraLines = null;
            return "ERR " + ex.Message;
        }
    }

    /// <summary>Gateway servers of a division: "SERVERS divIdx".</summary>
    private static string Servers(string arg, out List<string> extraLines)
    {
        extraLines = new List<string>();

        try
        {
            var divisions = Game.ReferenceManager?.DivisionInfo?.Divisions;
            if (divisions == null || divisions.Count == 0)
                return "ERR no divisions loaded";

            if (!int.TryParse(arg.Trim(), out var divIdx) || divIdx < 0 || divIdx >= divisions.Count)
                return "ERR usage: SERVERS <divisionIndex>";

            var gates = divisions[divIdx].GatewayServers;
            if (gates == null || gates.Count == 0)
                return "ERR no gateway servers";

            for (var i = 0; i < gates.Count; i++)
                extraLines.Add($"{i}|{Encode(gates[i] ?? string.Empty)}");

            extraLines.Add("END");
            return $"BEGIN {gates.Count}";
        }
        catch (Exception ex)
        {
            extraLines = null;
            return "ERR " + ex.Message;
        }
    }

    /// <summary>Current selection: "SEL div=X gate=Y type=Name".</summary>
    private static string GetSel()
    {
        try
        {
            var div = GlobalConfig.Get<int>("RSBot.DivisionIndex");
            var gate = GlobalConfig.Get<int>("RSBot.GatewayIndex");
            var type = GlobalConfig.Get<string>("RSBot.Game.ClientType") ?? "Vietnam";
            return $"SEL div={div} gate={gate} type={Encode(type)}";
        }
        catch (Exception ex)
        {
            return "ERR " + ex.Message;
        }
    }

    /// <summary>Save selection: "SETSEL div gate".</summary>
    private static string SetSel(string arg)
    {
        try
        {
            var tokens = arg.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            if (tokens.Length < 2 ||
                !int.TryParse(tokens[0], out var div) ||
                !int.TryParse(tokens[1], out var gate))
                return "ERR usage: SETSEL <divisionIndex> <gatewayIndex>";

            var divisions = Game.ReferenceManager?.DivisionInfo?.Divisions;
            if (divisions == null || div < 0 || div >= divisions.Count)
                return "ERR bad division index";

            var gates = divisions[div].GatewayServers;
            if (gates == null || gate < 0 || gate >= gates.Count)
                return "ERR bad gateway index";

            GlobalConfig.Set("RSBot.DivisionIndex", div);
            GlobalConfig.Set("RSBot.GatewayIndex", gate);
            GlobalConfig.Save();
            return "OK selection saved";
        }
        catch (Exception ex)
        {
            return "ERR " + ex.Message;
        }
    }

    /// <summary>Save client type: "SETTYPE Vietnam" (takes effect after bot restart).</summary>
    private static string SetType(string arg)
    {
        try
        {
            var name = arg.Trim();
            if (string.IsNullOrWhiteSpace(name))
                return "ERR usage: SETTYPE <ClientTypeName>";

            var names = Enum.GetNames(typeof(GameClientType));
            var match = names.FirstOrDefault(n => n.Equals(name, StringComparison.OrdinalIgnoreCase));
            if (match == null)
                return "ERR unknown type - use: " + string.Join(",", names);

            GlobalConfig.Set("RSBot.Game.ClientType", match);
            GlobalConfig.Save();
            return "OK type saved (restart bot to apply)";
        }
        catch (Exception ex)
        {
            return "ERR " + ex.Message;
        }
    }

    private static string SetDir(string arg)
    {
        try
        {
            // Format: "dir" or "dir|exe"
            var sep = arg.IndexOf('|');
            var dir = (sep >= 0 ? arg.Substring(0, sep) : arg).Trim();
            var exe = sep >= 0 ? arg.Substring(sep + 1).Trim() : string.Empty;

            if (string.IsNullOrWhiteSpace(dir) || !System.IO.Directory.Exists(dir))
                return "ERR folder does not exist";

            GlobalConfig.Set("RSBot.SilkroadDirectory", dir);

            if (string.IsNullOrWhiteSpace(exe))
            {
                exe = GlobalConfig.Get<string>("RSBot.SilkroadExecutable");
                if (string.IsNullOrWhiteSpace(exe))
                    exe = "sro_client.exe";
            }

            GlobalConfig.Set("RSBot.SilkroadExecutable", exe);
            GlobalConfig.Save();
            return "OK saved: " + exe;
        }
        catch (Exception ex)
        {
            return "ERR " + ex.Message;
        }
    }

    private static string StatusLine()
    {
        try
        {
            var running = Kernel.Bot?.Running == true ? 1 : 0;
            var ready = Game.Ready ? 1 : 0;
            var learn = LearnMode.IsArmed ? 1 : 0;
            var client = ClientManager.IsRunning ? 1 : 0;

            var level = 0;
            var name = "-";
            var hp = 0;
            var maxHp = 0;
            var mp = 0;
            var maxMp = 0;
            var x = 0;
            var y = 0;

            if (Game.Player != null)
            {
                level = Game.Player.Level;
                name = (Game.Player.Name ?? "-").Replace(' ', '_').Replace('|', '_');
                hp = Game.Player.Health;
                maxHp = Game.Player.MaximumHealth;
                mp = Game.Player.Mana;
                maxMp = Game.Player.MaximumMana;
                x = (int)Game.Player.Position.X;
                y = (int)Game.Player.Position.Y;
            }

            var attacks = 0;
            var buffs = 0;
            try
            {
                attacks = SkillChatHelper.CurrentAttacks().Count;
                buffs = SkillChatHelper.CurrentBuffs().Count;
            }
            catch
            {
            }

            return $"STATUS running={running} ready={ready} learn={learn} client={client} admin={(IsAdministrator() ? 1 : 0)} level={level} name={name} " +
                   $"hp={hp} maxhp={maxHp} mp={mp} maxmp={maxMp} x={x} y={y} attacks={attacks} buffs={buffs}";
        }
        catch (Exception ex)
        {
            return "ERR " + ex.Message;
        }
    }

    private static string SkillsList(string arg, out List<string> extraLines)
    {
        extraLines = new List<string>();

        try
        {
            // Format: "attack=1 buff=1 filter=text"
            var wantAttack = true;
            var wantBuff = true;
            var filter = string.Empty;

            foreach (var token in arg.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries))
            {
                var kv = token.Split(new[] { '=' }, 2);
                if (kv.Length == 2)
                {
                    if (kv[0].Equals("attack", StringComparison.OrdinalIgnoreCase))
                        wantAttack = kv[1] != "0";
                    else if (kv[0].Equals("buff", StringComparison.OrdinalIgnoreCase))
                        wantBuff = kv[1] != "0";
                    else if (kv[0].Equals("filter", StringComparison.OrdinalIgnoreCase))
                        filter = Decode(kv[1]);
                }
                else
                {
                    filter = (filter + " " + token).Trim();
                }
            }

            var count = 0;
            foreach (var (header, skills) in SkillChatHelper.MySkillsGrouped(wantAttack, wantBuff))
            {
                foreach (var s in skills)
                {
                    string name;
                    try
                    {
                        name = s.Record.GetRealName();
                    }
                    catch
                    {
                        continue;
                    }

                    if (filter.Length > 0)
                    {
                        var needle = SkillChatHelper.Normalize(filter);
                        if (!SkillChatHelper.Normalize(name).Contains(needle, StringComparison.Ordinal) &&
                            !SkillChatHelper.Initials(name).StartsWith(needle, StringComparison.Ordinal))
                            continue;
                    }

                    var isAttack = s.IsAttack || s.Record.TargetGroup_Enemy_M;
                    extraLines.Add($"{s.Id}|{Encode(name)}|{s.Record.Basic_Level}|{Encode(header)}|{(isAttack ? 1 : 0)}");
                    count++;

                    if (count >= 400)
                        break;
                }

                if (count >= 400)
                    break;
            }

            extraLines.Add("END");
            return $"BEGIN {count}";
        }
        catch (Exception ex)
        {
            extraLines = null;
            return "ERR " + ex.Message;
        }
    }

    private static string AddSkill(string arg)
    {
        try
        {
            if (!uint.TryParse(arg.Trim(), out var id))
                return "ERR usage: ADD <skillId>";

            if (Game.Player == null)
                return "ERR not in game";

            Game.Player.TryGetAbilitySkills(out var abilitySkills);
            SkillInfo info = Game.Player.Skills.GetSkillInfoById(id) ??
                             abilitySkills?.FirstOrDefault(p => p.Id == id);

            if (info == null)
                return "ERR skill not learned in game";

            var name = SkillChatHelper.Describe(info);
            bool added;
            if (info.IsAttack || info.Record.TargetGroup_Enemy_M)
                added = SkillChatHelper.AddAttack(info);
            else
                added = SkillChatHelper.AddBuff(info);

            return added ? "OK Added: " + name : "ERR Already have: " + name;
        }
        catch (Exception ex)
        {
            return "ERR " + ex.Message;
        }
    }

    private static string DelSkill(string arg)
    {
        try
        {
            if (!uint.TryParse(arg.Trim(), out var id))
                return "ERR usage: DEL <skillId>";

            SkillInfo target = SkillChatHelper.CurrentAttacks().FirstOrDefault(s => s.Id == id);
            if (target != null)
                return SkillChatHelper.RemoveAttack(target)
                    ? "OK Removed: " + SkillChatHelper.Describe(target)
                    : "ERR remove failed";

            target = SkillChatHelper.CurrentBuffs().FirstOrDefault(s => s.Id == id);
            if (target != null)
                return SkillChatHelper.RemoveBuff(target)
                    ? "OK Removed: " + SkillChatHelper.Describe(target)
                    : "ERR remove failed";

            return "ERR not in bot lists";
        }
        catch (Exception ex)
        {
            return "ERR " + ex.Message;
        }
    }

    private static string Learn(string arg)
    {
        try
        {
            var mode = arg.Trim().ToLowerInvariant();
            if (mode == "cancel")
            {
                LearnMode.Cancel();
                return "OK learn cancelled";
            }

            if (Game.Player == null || !Game.Ready)
                return "ERR not in game";

            if (mode == "buff")
                LearnMode.ArmBuff();
            else
                LearnMode.ArmAttack();

            return "OK cast the skill once in game";
        }
        catch (Exception ex)
        {
            return "ERR " + ex.Message;
        }
    }

    private static string IconBase64(string arg, out List<string> extraLines)
    {
        extraLines = null;

        try
        {
            if (!uint.TryParse(arg.Trim(), out var id))
                return "ERR usage: ICON <skillId>";

            var b64 = LoadIconBase64(id);
            if (b64 == null)
                return "ERR no icon";

            extraLines = new List<string> { b64 };
            return $"ICONBASE64 {b64.Length}";
        }
        catch (Exception ex)
        {
            return "ERR " + ex.Message;
        }
    }

    /// <summary>Batch icons: "ICONS id1,id2" -> "ICONBATCH n" + n lines "id|base64-or-empty".</summary>
    private static string IconsBatch(string arg, out List<string> extraLines)
    {
        extraLines = new List<string>();

        try
        {
            var count = 0;
            foreach (var token in arg.Split(new[] { ',', ' ', ';' }, StringSplitOptions.RemoveEmptyEntries))
            {
                if (count >= 200)
                    break;

                if (!uint.TryParse(token.Trim(), out var id))
                    continue;

                extraLines.Add($"{id}|{LoadIconBase64(id) ?? string.Empty}");
                count++;
            }

            return $"ICONBATCH {count}";
        }
        catch (Exception ex)
        {
            extraLines = null;
            return "ERR " + ex.Message;
        }
    }

    private static string LoadIconBase64(uint id)
    {
        try
        {
            var reference = Game.ReferenceManager.GetRefSkill(id);
            var icon = reference?.GetIcon();
            if (icon == null)
                return null;

            using var ms = new MemoryStream();
            icon.Save(ms, ImageFormat.Png);
            icon.Dispose();

            return Convert.ToBase64String(ms.ToArray());
        }
        catch
        {
            return null;
        }
    }

    private static string Encode(string value)
    {
        return Convert.ToBase64String(Encoding.UTF8.GetBytes(value ?? string.Empty));
    }

    private static string Decode(string value)
    {
        try
        {
            return Encoding.UTF8.GetString(Convert.FromBase64String(value));
        }
        catch
        {
            return value;
        }
    }
}
