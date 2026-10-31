using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using RSBot.Core.Event;
using RSBot.Core.Extensions;
using static RSBot.Core.Extensions.NativeExtensions;

namespace RSBot.Core.Components;

public class ClientManager
{
    /// <summary>
    ///     The client process
    /// </summary>
    private static Process _process;

    /// <summary>
    ///     Get, has client exited <c>true</c> otherwise; <c>false</c>
    /// </summary>
    public static bool IsRunning => _process != null && !_process.HasExited;

    /// <summary>
    ///     Start the game client
    /// </summary>
    /// <returns>Has successfully started <c>true</c>; otherwise <c>false</c></returns>
    public static async Task<bool> Start()
    {
        var silkroadDirectory = GlobalConfig.Get<string>("RSBot.SilkroadDirectory");
        var path = Path.Combine(
            silkroadDirectory,
            GlobalConfig.Get<string>("RSBot.SilkroadExecutable")
        );

        var dllPath = Path.Combine(Kernel.BasePath, "Client.Library.dll");

        if (!File.Exists(path))
        {
            Log.Error($"The game client '{path}' does not exist.");
            SessionLog.Write($"Client start aborted: '{path}' does not exist");

            return false;
        }

        if (!File.Exists(dllPath))
        {
            Log.Error($"The loader library '{dllPath}' does not exist, the client can not be started.");
            SessionLog.Write($"Client start aborted: '{dllPath}' does not exist");

            return false;
        }

        var gatewayIndex = GlobalConfig.Get<byte>("RSBot.GatewayIndex");
        var divisionIndex = GlobalConfig.Get<byte>("RSBot.DivisionIndex");
        var contentId = Game.ReferenceManager.DivisionInfo.Locale;

        var args = $"/{contentId} {divisionIndex} {gatewayIndex} 0";

        var si = new STARTUPINFO();

        var full = $"\"{path}\" {args}";
        SessionLog.Write($"Starting client '{path}' with arguments '{args}' (work dir: {silkroadDirectory})");

        var result = CreateProcess(null, full, IntPtr.Zero, IntPtr.Zero, false, CREATE_SUSPENDED, IntPtr.Zero,
            silkroadDirectory, ref si, out var pi);
        if (!result)
        {
            SessionLog.Write($"CreateProcess failed (error {Marshal.GetLastWin32Error()})");
            return false;
        }

        PrepareTempConfigFile(pi.dwProcessId, divisionIndex);

        var semaphore = new Semaphore(0, 1, pi.dwProcessId.ToString());

        Process process;
        try
        {
            process = Process.GetProcessById((int)pi.dwProcessId);
        }
        catch (Exception ex)
        {
            SessionLog.Write("Could not access the game client process", ex);

            return false;
        }

        SessionLog.Write($"Client process created (pid {pi.dwProcessId}), proxy port {Kernel.Proxy?.Port}");

        Game.ClientProcessId = (int)pi.dwProcessId;

        if (Game.ClientType == GameClientType.VTC_Game ||
            Game.ClientType == GameClientType.Turkey)
        {
            try
            {
                var moduleMemory = new byte[process.MainModule.ModuleMemorySize];
                ReadProcessMemory(process.Handle, process.MainModule.BaseAddress, moduleMemory,
                    process.MainModule.ModuleMemorySize, out _);

                var patchNop = new byte[] { 0x90, 0x90 };
                var patchNop2 = new byte[] { 0x90, 0x90, 0x90, 0x90, 0x90 };
                var patchJmp = new byte[] { 0xEB };

                var address = FindPattern(
                    Game.ClientType == GameClientType.Turkey ?
                    "6A 00 68 B0 28 3C 01 68 C4 28 3C 01" :
                    "6A 00 68 48 A2 38 01 68 5C A2 38 01",
                    moduleMemory);
                if (address == IntPtr.Zero)
                {
                    Log.Error("XIGNCODE patching error! Maybe signatures are wrong?");
                    SessionLog.Write("XIGNCODE patch failed: signature not found");

                    return false;
                }

                WriteProcessMemory(pi.hProcess, address - 0x6F, patchJmp, 1, out _);
                WriteProcessMemory(pi.hProcess, address + 0x13, patchJmp, 1, out _);
                WriteProcessMemory(pi.hProcess, address + 0xC, patchNop2, 5, out _);
                WriteProcessMemory(pi.hProcess, address + 0x95, patchJmp, 1, out _);

                moduleMemory = null;
                GC.Collect();
            }
            catch (Exception ex)
            {
                Log.Error("XIGNCODE patching error: " + ex.Message);
                SessionLog.Write("XIGNCODE patching error", ex);

                return false;
            }
        }

        var exitCode = InjectClientLibrary(pi.dwProcessId, dllPath);

        if (exitCode == 0)
        {
            Log.Error($"Could not inject '{Path.GetFileName(dllPath)}' into the game client process! " +
                      "Check whether an antivirus program blocks the file or whether the bot has enough permissions.");
            SessionLog.Write("Injection failed for all attempts, closing the client");

            try
            {
                process.Kill();
            }
            catch
            {
            }

            return await Task.FromResult(false);
        }

        SessionLog.Write($"Loader injected successfully (module base 0x{exitCode:X8})");

        ResumeThread(pi.hThread);
        CloseHandle(pi.hThread);

        if (process.HasExited)
        {
            SessionLog.Write("The client exited directly after it was resumed");
            return await Task.FromResult(false);
        }

        process.EnableRaisingEvents = true;
        process.Exited += ClientProcess_Exited;
        _process = process;

        EventManager.FireEvent("OnStartClient");

        return await Task.FromResult(true);
    }

    /// <summary>
    ///     Injects the loader library into the given process.
    ///     The injection is repeated a few times, because the loader (and therefore the redirect)
    ///     silently fails when the game client is still initializing.
    /// </summary>
    /// <param name="processId">The id of the game client process</param>
    /// <param name="dllPath">The path of the loader library</param>
    /// <returns>The module base address, 0 if the injection failed</returns>
    private static uint InjectClientLibrary(uint processId, string dllPath)
    {
        const int attempts = 5;
        const uint maxWait = 15000;

        var buffer = Encoding.Unicode.GetBytes(dllPath + "\0");
        var pathLen = (uint)buffer.Length;

        //Right after CreateProcess the client is still initializing its loader,
        //injecting too early can make LoadLibrary return 0 without any error.
        Thread.Sleep(200);

        var handle = OpenProcess(PROCESS_ALL_ACCESS, false, processId);
        if (handle == IntPtr.Zero)
        {
            SessionLog.Write($"Could not open the client process (error {Marshal.GetLastWin32Error()})");

            return 0;
        }

        try
        {
            var kernelHandle = GetModuleHandle("kernel32.dll");
            var loadLibAddr = GetProcAddress(kernelHandle, "LoadLibraryW");
            if (loadLibAddr == IntPtr.Zero)
            {
                SessionLog.Write("Could not resolve LoadLibraryW");

                return 0;
            }

            for (var attempt = 1; attempt <= attempts; attempt++)
            {
                var remoteMemory = VirtualAllocEx(handle, IntPtr.Zero, pathLen, MEM_COMMIT | MEM_RESERVE,
                    PAGE_READWRITE);
                if (remoteMemory == IntPtr.Zero)
                {
                    SessionLog.Write("Could not allocate memory inside the client process");

                    return 0;
                }

                if (!WriteProcessMemory(handle, remoteMemory, buffer, pathLen, out _))
                {
                    SessionLog.Write("Could not write the library path into the client process");
                    VirtualFreeEx(handle, remoteMemory, pathLen, MEM_RELEASE);

                    return 0;
                }

                var remoteThread = CreateRemoteThread(handle, IntPtr.Zero, 0, loadLibAddr, remoteMemory, 0,
                    IntPtr.Zero);
                if (remoteThread == IntPtr.Zero)
                {
                    SessionLog.Write("Could not create the remote thread");
                    VirtualFreeEx(handle, remoteMemory, pathLen, MEM_RELEASE);

                    return 0;
                }

                WaitForSingleObject(remoteThread, maxWait);
                GetExitCodeThread(remoteThread, out var exitCode);

                CloseHandle(remoteThread);
                VirtualFreeEx(handle, remoteMemory, pathLen, MEM_RELEASE);

                SessionLog.Write($"Injection attempt {attempt}/{attempts} returned 0x{exitCode:X8}");

                if (exitCode != 0)
                {
                    //LoadLibrary succeeded, but the loader itself may have failed to install
                    //its hooks (for example because the client protects itself).
                    if (IsLoaderReady(processId))
                    {
                        DeleteLoaderLog(processId);

                        return exitCode;
                    }

                    var loaderError = ReadLoaderDiagnostics(processId);
                    SessionLog.Write($"The loader was loaded but did not start correctly: {loaderError}");

                    DeleteLoaderLog(processId);

                    //retrying would not help, the library is already loaded inside the client
                    return 0;
                }

                //the client was not ready yet, give it a moment and try again
                Thread.Sleep(400);
            }

            var diagnostics = ReadLoaderDiagnostics(processId);
            if (!string.IsNullOrEmpty(diagnostics))
                SessionLog.Write($"Loader diagnostics: {diagnostics}");

            return 0;
        }
        finally
        {
            CloseHandle(handle);
        }
    }

    /// <summary>
    ///     Kill the game client process
    /// </summary>
    public static void Kill()
    {
        if (!IsRunning)
            return;

        SessionLog.Write($"Killing the game client (pid {_process.Id})");

        try
        {
            //Process.Kill() terminates the client with exit code -1 (0xFFFFFFFF).
            //Some game launchers (like the NewEra launcher) interpret that as a crash
            //and show an error dialog, so the client is terminated with exit code 0 instead.
            var handle = OpenProcess(PROCESS_TERMINATE, false, (uint)_process.Id);
            if (handle != IntPtr.Zero)
            {
                try
                {
                    if (TerminateProcess(handle, 0))
                        return;
                }
                finally
                {
                    CloseHandle(handle);
                }
            }
        }
        catch
        {
        }

        try
        {
            _process.Kill();
        }
        catch
        {
        }
    }

    /// <summary>
    ///     Change client process title
    /// </summary>
    /// <param name="title">The new title</param>
    public static void SetTitle(string title)
    {
        if (_process == null)
            return;

        SetWindowText(_process.MainWindowHandle, title);
    }

    /// <summary>
    ///     Change client visible
    /// </summary>
    /// <param name="visible">The visible</param>
    public static void SetVisible(bool visible)
    {
        if (_process == null || _process.HasExited)
            return;

        if (visible)
            ShowWindow(_process.MainWindowHandle, SW_SHOW);
        else
            ShowWindow(_process.MainWindowHandle, SW_HIDE);
    }

    /// <summary>
    ///     Restores and focuses the game client window.
    /// </summary>
    public static void Show()
    {
        if (_process == null || _process.HasExited)
            return;

        var handle = _process.MainWindowHandle;
        if (handle == IntPtr.Zero)
            return;

        ShowWindow(handle, SW_SHOW);
        ShowWindow(handle, SW_RESTORE);
        SetForegroundWindow(handle);
    }

    /// <summary>
    ///     Observes the process.
    /// </summary>
    private static void ClientProcess_Exited(object sender, EventArgs e)
    {
        var exitCode = "unknown";
        var processId = "?";

        try
        {
            if (sender is Process process)
            {
                processId = process.Id.ToString();
                exitCode = $"0x{process.ExitCode:X8}";
            }
        }
        catch
        {
        }

        SessionLog.Write($"Client process {processId} exited (exit code {exitCode})");

        CleanupClientFiles(processId);

        Log.Warn("Client process exited!");
        EventManager.FireEvent("OnExitClient");
    }

    /// <summary>
    ///     Removes the temporary files the loader may have left behind.
    /// </summary>
    /// <param name="processId">The id of the client process</param>
    private static void CleanupClientFiles(string processId)
    {
        if (processId == "?")
            return;

        foreach (var extension in new[] { "tmp", "port", "loader.log" })
        {
            try
            {
                var path = Path.Combine(Path.GetTempPath(), $"RSBot_{processId}.{extension}");
                if (File.Exists(path))
                    File.Delete(path);
            }
            catch
            {
            }
        }
    }

    /// <summary>
    ///     Returns a value indicating whether the loader finished its installation.
    ///     The loader writes its progress into a small log file, so a loader that was loaded
    ///     but failed to install its hooks can be detected (and the injection retried).
    /// </summary>
    /// <param name="processId">The id of the client process</param>
    private static bool IsLoaderReady(uint processId)
    {
        try
        {
            var path = Path.Combine(Path.GetTempPath(), $"RSBot_{processId}.loader.log");
            if (!File.Exists(path))
                return true;

            return File.ReadAllText(path).Contains("DllMain: ready");
        }
        catch
        {
            return true;
        }
    }

    /// <summary>
    ///     Reads the diagnostic messages the loader wrote before the injection failed.
    /// </summary>
    /// <param name="processId">The id of the client process</param>
    /// <returns>The last steps the loader reached, empty if there is no log</returns>
    private static string ReadLoaderDiagnostics(uint processId)
    {
        try
        {
            var path = Path.Combine(Path.GetTempPath(), $"RSBot_{processId}.loader.log");
            if (!File.Exists(path))
                return string.Empty;

            return string.Join(" | ", File.ReadAllLines(path).TakeLast(4));
        }
        catch
        {
            return string.Empty;
        }
    }

    /// <summary>
    ///     Removes the diagnostic log of the loader of the given process.
    /// </summary>
    /// <param name="processId">The id of the client process</param>
    private static void DeleteLoaderLog(uint processId)
    {
        try
        {
            var path = Path.Combine(Path.GetTempPath(), $"RSBot_{processId}.loader.log");
            if (File.Exists(path))
                File.Delete(path);
        }
        catch
        {
        }
    }

    /// <summary>
    ///     Prepare the config file for loader
    /// </summary>
    /// <param name="processId"></param>
    /// <param name="divisionIndex"></param>
    private static void PrepareTempConfigFile(uint processId, int divisionIndex)
    {
        var tmpConfigFile = $"RSBot_{processId}.tmp";

        var division = Game.ReferenceManager.DivisionInfo.Divisions[divisionIndex];
        var gatewayPort = Game.ReferenceManager.GatewayInfo.Port;

        var redirectIp = "127.0.0.1";
        using var writer =
            new BinaryWriter(new FileStream(Path.Combine(Path.GetTempPath(), tmpConfigFile), FileMode.Create, FileAccess.Write));

        writer.Write(GlobalConfig.Get<bool>("RSBot.Loader.DebugMode"));
        writer.WriteAscii(redirectIp);
        writer.Write(Kernel.Proxy.Port);
        writer.Write(division.GatewayServers.Count);
        foreach (var gatewayServer in division.GatewayServers)
            writer.WriteAscii(gatewayServer);

        writer.Write(gatewayPort);
    }

    private static IntPtr FindPattern(string stringPattern, byte[] buffer)
    {
        var pattern = stringPattern.Split(' ')
            .Select(p => byte.Parse(p, NumberStyles.AllowHexSpecifier))
            .ToArray();

        for (uint i = 0; i < buffer.Length - pattern.Length; i++)
        {
            var found = true;
            for (uint j = 0; j < pattern.Length; j++)
                if (buffer[i + j] != pattern[j])
                {
                    found = false;
                    break;
                }

            if (found)
                return (IntPtr)(0x400000 + i);
        }

        return IntPtr.Zero;
    }
}
