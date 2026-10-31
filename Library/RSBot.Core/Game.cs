using RSBot.Core.Client;
using RSBot.Core.Components;
using RSBot.Core.Extensions;
using RSBot.Core.Network;
using RSBot.Core.Objects;
using RSBot.Core.Objects.Party;
using RSBot.Core.Objects.Spawn;
using RSBot.FileSystem;
using RSBot.NavMeshApi;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace RSBot.Core;

public class Game
{
    /// <summary>
    ///     The signature that every Joymax pack file (pk2/ne/...) starts with.
    /// </summary>
    private const string PackFileSignature = "Joymax File Manager!";

    /// <summary>
    ///     Known pk2 keys that are used by common Silkroad clients.
    ///     "169841" is the default Joymax key, "Gde8bT" is used by NewEra Online.
    /// </summary>
    private static readonly string[] KnownPk2Keys = { "169841", "Gde8bT" };

    /// <summary>
    ///     Archive file extensions that may contain the game media/data.
    /// </summary>
    private static readonly string[] KnownArchiveExtensions = { ".pk2", ".ne" };

    /// <summary>
    ///     The acceptance request
    /// </summary>
    public static AcceptanceRequest AcceptanceRequest;

    /// <summary>
    ///     Gets or sets the port.
    /// </summary>
    /// <value>
    ///     The port.
    /// </value>
    public static ushort Port { get; private set; }

    /// <summary>
    ///     Gets or sets the Media.pk2 reader.
    /// </summary>
    /// <value>
    ///     The PK2 reader.
    /// </value>
    public static IFileSystem MediaPk2 { get; set; }

    /// <summary>
    ///     Gets or sets the Data.pk2 reader.
    /// </summary>
    /// <value>
    ///     The PK2 reader.
    /// </value>
    public static IFileSystem DataPk2 { get; set; }

    /// <summary>
    ///     Gets or sets the reference manager
    /// </summary>
    /// <value>
    ///     The reference manager
    /// </value>
    public static ReferenceManager ReferenceManager { get; set; }

    /// <summary>
    ///     Gets the character.
    /// </summary>
    /// <value>
    ///     The character.
    /// </value>
    public static Player Player { get; internal set; }

    /// <summary>
    ///     Gets or sets the selected entity.
    /// </summary>
    /// <value>
    ///     The selected entity.
    /// </value>
    public static SpawnedBionic? SelectedEntity { get; set; }

    /// <summary>
    ///     Gets or sets the spawn information.
    /// </summary>
    /// <value>
    ///     The spawn information.
    /// </value>
    internal static SpawnPacketInfo SpawnInfo { get; set; }

    /// <summary>
    ///     Gets or sets the chunked packet.
    /// </summary>
    /// <value>
    ///     The chunked packet.
    /// </value>
    internal static Packet ChunkedPacket { get; set; }

    /// <summary>
    ///     Gets or sets the party.
    /// </summary>
    /// <value>
    ///     The party.
    /// </value>
    public static Party Party { get; internal set; }

    /// <summary>
    ///     Gets a value indicating whether this <see cref="Game" /> is clientless.
    /// </summary>
    /// <value>
    ///     <c>true</c> if clientless; otherwise, <c>false</c>.
    /// </value>
    public static bool Clientless { get; set; }

    /// <summary>
    ///     Gets or sets the id of the game client process.
    /// </summary>
    public static int ClientProcessId { get; internal set; }

    /// <summary>
    ///     Gets or sets a value indicating whether this <see cref="Game" /> is started.
    /// </summary>
    /// <value><c>true</c> if started; otherwise, <c>false</c>.</value>
    public static bool Started { get; set; }

    /// <summary>
    ///     Gets a value indicating whether this <see cref="Game" /> is ready.
    /// </summary>
    /// <value>
    ///     <c>true</c> if ready; otherwise, <c>false</c>.
    /// </value>
    public static bool Ready { get; internal set; }

    /// <summary>
    ///     The game client type
    /// </summary>
    public static GameClientType ClientType { get; set; }

    /// <summary>
    ///     Starts the game.
    /// </summary>
    public static void Start()
    {
        Started = false;

        if (Kernel.Bot.Running)
            Kernel.Bot.Stop();

        Kernel.Proxy?.Shutdown();

        var divisionIndex = GlobalConfig.Get<int>("RSBot.DivisionIndex");
        var severIndex = GlobalConfig.Get<int>("RSBot.GatewayIndex");

        Port = NetworkUtilities.GetFreePort(1500, 2000, 1);

        Kernel.Proxy = new Proxy();
        Kernel.Proxy.Start(Port, ReferenceManager.DivisionInfo.Divisions[divisionIndex].GatewayServers[severIndex],
            ReferenceManager.GatewayInfo.Port);

        SessionLog.Write($"Game started: division {divisionIndex}, gateway {severIndex}, proxy port {Port}");

        Started = true;
    }

    /// <summary>
    ///     Initialize game archive files
    /// </summary>
    /// <returns></returns>
    public static bool InitializeArchiveFiles()
    {
        var directory = GlobalConfig.Get<string>("RSBot.SilkroadDirectory");
        if (string.IsNullOrWhiteSpace(directory) || !Directory.Exists(directory))
        {
            Log.Error($"The Silkroad directory '{directory}' does not exist.");

            return false;
        }

        try
        {
            MediaPk2 = OpenArchive(directory, "media");
            DataPk2 = OpenArchive(directory, "data");

            SessionLog.Write($"Client directory: {directory} | media: {FindArchiveFile(directory, "media") ?? "Media folder"} | data: {FindArchiveFile(directory, "data") ?? "Data folder"}");

            if (MediaPk2 == null)
                Log.Error("Could not find or open a supported media archive (Media.pk2, Media.ne, ...) or the Media folder.");
            if (DataPk2 == null)
                Log.Error("Could not find or open a supported data archive (Data.pk2, Data.ne, ...) or the Data folder.");

            return MediaPk2 != null && DataPk2 != null;
        }
        catch (Exception ex)
        {
            Log.Error(ex);

            return false;
        }
    }

    /// <summary>
    ///     Checks if the given directory belongs to a supported game client.
    /// </summary>
    /// <param name="directory">The client directory.</param>
    public static bool IsValidClientDirectory(string directory)
    {
        if (string.IsNullOrWhiteSpace(directory) || !Directory.Exists(directory))
            return false;

        try
        {
            return FindArchiveFile(directory, "media") != null ||
                   IsUnpackedArchiveFolder(directory, "media");
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    ///     Finds a game archive without relying on a fixed file name or extension.
    ///     This allows clients like NewEra Online that ship Media.ne/Data.ne instead of Media.pk2/Data.pk2.
    /// </summary>
    /// <param name="directory">The client directory.</param>
    /// <param name="archiveName">The base name of the archive (media, data, map, ...).</param>
    /// <returns>The archive path or <c>null</c> if no matching archive was found.</returns>
    public static string FindArchiveFile(string directory, string archiveName)
    {
        if (string.IsNullOrWhiteSpace(directory) || !Directory.Exists(directory) || string.IsNullOrWhiteSpace(archiveName))
            return null;

        //Prefer the well known extensions (fast path)
        foreach (var extension in KnownArchiveExtensions)
        {
            var path = Path.Combine(directory, archiveName + extension);
            if (File.Exists(path) && LooksLikePackFile(path))
                return path;
        }

        //Then accept any extension as long as the base name matches (Media.ne, Media.PK2, Media.pk2.bak, ...)
        var matchingFiles = Directory.GetFiles(directory)
            .Where(file => Path.GetFileNameWithoutExtension(file).Equals(archiveName, StringComparison.OrdinalIgnoreCase))
            .OrderBy(file => file, StringComparer.OrdinalIgnoreCase);

        foreach (var file in matchingFiles)
            if (LooksLikePackFile(file))
                return file;

        //Last resort: a pack file whose name contains the archive name (NewEra_Media.bin, ...)
        var candidateFiles = Directory.GetFiles(directory)
            .Where(file => Path.GetFileName(file).IndexOf(archiveName, StringComparison.OrdinalIgnoreCase) >= 0)
            .OrderBy(file => file, StringComparer.OrdinalIgnoreCase);

        foreach (var file in candidateFiles)
            if (LooksLikePackFile(file))
                return file;

        return null;
    }

    /// <summary>
    ///     Opens an archive file. Tries the configured key first and then the well known keys, so that
    ///     clients with a renamed or unencrypted archive keep working.
    /// </summary>
    private static IFileSystem OpenArchive(string directory, string archiveName)
    {
        var archivePath = FindArchiveFile(directory, archiveName);
        if (archivePath != null)
        {
            var keys = new List<string>();
            var configuredKey = GlobalConfig.Get<string>("RSBot.Pk2Key", "169841");

            if (!string.IsNullOrEmpty(configuredKey))
                keys.Add(configuredKey);

            foreach (var knownKey in KnownPk2Keys)
                if (!keys.Contains(knownKey))
                    keys.Add(knownKey);

            Exception lastError = null;

            foreach (var key in keys)
            {
                try
                {
                    return new PackFileSystem(archivePath, key);
                }
                catch (Exception ex)
                {
                    lastError = ex;
                }
            }

            //The archive might not be encrypted at all
            try
            {
                return new PackFileSystem(archivePath);
            }
            catch (Exception ex)
            {
                lastError = ex;
            }

            Log.Warn($"Failed to open archive '{Path.GetFileName(archivePath)}': {lastError?.Message}");

            return null;
        }

        //Some clients ship the archives fully extracted instead of packed
        if (IsUnpackedArchiveFolder(directory, archiveName))
            return new LocalFileSystem(Path.Combine(directory, archiveName), true);

        return null;
    }

    /// <summary>
    ///     Checks whether a file is a Joymax pack archive by its signature.
    /// </summary>
    private static bool LooksLikePackFile(string path)
    {
        try
        {
            using var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            var signatureLength = Math.Min(PackFileSignature.Length, (int)Math.Min(stream.Length, int.MaxValue));
            if (signatureLength <= 0)
                return false;

            var buffer = new byte[signatureLength];
            var read = stream.Read(buffer, 0, signatureLength);
            if (read != signatureLength)
                return false;

            return string.Equals(System.Text.Encoding.ASCII.GetString(buffer), PackFileSignature, StringComparison.OrdinalIgnoreCase);
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    ///     Checks if an extracted archive folder contains real game data.
    /// </summary>
    private static bool IsUnpackedArchiveFolder(string directory, string archiveName)
    {
        var path = Path.Combine(directory, archiveName);
        if (!Directory.Exists(path))
            return false;

        //A random folder named "media" (like the launcher videos in some clients) is not the game media.
        return Directory.Exists(Path.Combine(path, "server_dep")) ||
               Directory.Exists(Path.Combine(path, "icon")) ||
               Directory.Exists(Path.Combine(path, "interface")) ||
               Directory.Exists(Path.Combine(path, "textdata")) ||
               Directory.Exists(Path.Combine(path, "textures"));
    }

    /// <summary>
    ///     Initializes this instance.
    /// </summary>
    public static void Initialize()
    {
        ClientType = GlobalConfig.GetEnum("RSBot.Game.ClientType", GameClientType.Vietnam);
        ReferenceManager = new ReferenceManager();
        Party = new Party();

        SkillManager.Initialize();
        ShoppingManager.Initialize();
        ClientlessManager.Initialize();
        ScriptManager.Initialize();
    }

    /// <summary>
    ///     Shows a notification in the game client using the notice chat type.
    /// </summary>
    /// <param name="message"></param>
    public static void ShowNotification(string message)
    {
        if (!Ready)
            return;

        var chatPacket = new Packet(0x3026);
        chatPacket.WriteByte(ChatType.Notice);
        chatPacket.WriteConditonalString(message);

        PacketManager.SendPacket(chatPacket, PacketDestination.Client);
    }
}