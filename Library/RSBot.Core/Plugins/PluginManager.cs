using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using RSBot.Core.Event;
using RSBot.Core.Network;

namespace RSBot.Core.Plugins;

public class PluginManager
{
    /// <summary>
    ///     Gets the extension directory.
    /// </summary>
    /// <value>
    ///     The extension directory.
    /// </value>
    public string InitialDirectory => Path.Combine(Kernel.BasePath, "Data", "Plugins");

    /// <summary>
    ///     Gets the extensions.
    /// </summary>
    /// <value>
    ///     The extensions.
    /// </value>
    public Dictionary<string, IPlugin> Extensions { get; private set; }

    /// <summary>
    ///     Loads the assemblies.
    /// </summary>
    public bool LoadAssemblies()
    {
        if (Extensions != null) return false;

        Extensions = new Dictionary<string, IPlugin>();

        try
        {
            if (!Directory.Exists(InitialDirectory))
                Directory.CreateDirectory(InitialDirectory);

            foreach (var file in Directory.GetFiles(InitialDirectory, "*.dll"))
            {
                try
                {
                    var loaded = GetExtensionsFromAssembly(file);
                    foreach (var ext in loaded)
                    {
                        if (ext.Key != null && ext.Value != null)
                        {
                            Extensions[ext.Key] = ext.Value;
                            Log.Debug($"Loaded plugin [{ext.Value.InternalName}]");
                        }
                    }
                }
                catch (Exception fileEx)
                {
                    Log.Error($"Failed loading plugin from '{Path.GetFileName(file)}': {fileEx.Message}");
                }
            }

            // Order by index
            Extensions = Extensions.Values
                .OrderBy(entry => entry.Index)
                .ToDictionary(x => x.InternalName, x => x);

            try
            {
                EventManager.FireEvent("OnLoadPlugins");
            }
            catch (Exception evEx)
            {
                Log.Error($"OnLoadPlugins event handler error: {evEx.Message}");
            }

            return true;
        }
        catch (Exception ex)
        {
            try
            {
                File.WriteAllText(Path.Combine(Kernel.BasePath, "boot-error.log"),
                    $"The plugin manager encountered a problem: \n{ex.Message} at {ex.StackTrace}");
            }
            catch { }
            return false;
        }
    }

    /// <summary>
    ///     Known non-plugin dependencies to skip during plugin discovery.
    /// </summary>
    private static readonly HashSet<string> IgnoredAssemblyNames = new(StringComparer.OrdinalIgnoreCase)
    {
        "WebView2Loader.dll",
        "Microsoft.Windows.SDK.NET.dll",
        "WinRT.Runtime.dll",
        "Microsoft.Web.WebView2.Core.dll",
        "Microsoft.Web.WebView2.WinForms.dll",
        "Microsoft.Web.WebView2.Wpf.dll",
        "SDUI.dll",
        "ExCSS.dll",
        "Svg.dll",
        "AeroBot.Core.dll",
        "AeroBot.FileSystem.dll",
        "AeroBot.NavMeshApi.dll",
        "AeroBot.Updater.dll",
        "Client.Library.dll",
        "Client.Library.Startup.dll"
    };

    /// <summary>
    ///     Gets the extensions from assembly.
    /// </summary>
    /// <param name="file">The file.</param>
    /// <returns></returns>
    private static Dictionary<string, IPlugin> GetExtensionsFromAssembly(string file)
    {
        var result = new Dictionary<string, IPlugin>();

        try
        {
            var fileName = Path.GetFileName(file);
            if (IgnoredAssemblyNames.Contains(fileName))
                return result;

            var assembly = Assembly.LoadFrom(file);
            Type[] assemblyTypes;
            try
            {
                assemblyTypes = assembly.GetTypes();
            }
            catch (ReflectionTypeLoadException rtle)
            {
                assemblyTypes = rtle.Types.Where(t => t != null).ToArray();
            }

            foreach (var type in assemblyTypes)
            {
                if (type == null || !type.IsPublic || type.IsAbstract) continue;
                if (type.GetInterface("IPlugin") == null) continue;

                try
                {
                    if (Activator.CreateInstance(type) is IPlugin extension && !string.IsNullOrWhiteSpace(extension.InternalName))
                    {
                        result[extension.InternalName] = extension;
                    }
                }
                catch (Exception instEx)
                {
                    Log.Error($"Failed instantiating plugin '{type.FullName}' from '{fileName}': {instEx.Message}");
                }
            }

            if (result.Count == 0)
                return result;

            var handlerType = typeof(IPacketHandler);
            var hookType = typeof(IPacketHook);

            var handlerTypes = assemblyTypes
                .Where(p => p != null && handlerType.IsAssignableFrom(p) && !p.IsInterface && !p.IsAbstract).ToArray();

            foreach (var handler in handlerTypes)
            {
                try
                {
                    PacketManager.RegisterHandler((IPacketHandler)Activator.CreateInstance(handler));
                }
                catch (Exception hEx)
                {
                    Log.Error($"Failed registering packet handler '{handler.FullName}': {hEx.Message}");
                }
            }

            var hookTypes = assemblyTypes
                .Where(p => p != null && hookType.IsAssignableFrom(p) && !p.IsInterface && !p.IsAbstract).ToArray();

            foreach (var hook in hookTypes)
            {
                try
                {
                    PacketManager.RegisterHook((IPacketHook)Activator.CreateInstance(hook));
                }
                catch (Exception hookEx)
                {
                    Log.Error($"Failed registering packet hook '{hook.FullName}': {hookEx.Message}");
                }
            }
        }
        catch
        {
            /* ignore, it's a native dll or an invalid extension */
        }

        return result;
    }
}