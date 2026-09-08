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
                    var loadedExtensions = GetExtensionsFromAssembly(file);
                    foreach (var extension in loadedExtensions)
                    {
                        if (Extensions.ContainsKey(extension.Key))
                        {
                            Log.Warn($"Plugin [{extension.Key}] already loaded, skipping duplicate from {Path.GetFileName(file)}.");
                            continue;
                        }

                        Extensions.Add(extension.Key, extension.Value);
                        Log.Debug($"Loaded plugin [{extension.Value.InternalName}]");
                    }
                }
                catch (Exception ex)
                {
                    Log.Warn($"Could not load plugins from [{Path.GetFileName(file)}]: {ex.Message}");
                }
            }

            // Order by index, not alphabetically
            Extensions = Extensions.OrderBy(entry => entry.Value.Index)
                .ToDictionary(x => x.Key, x => x.Value);

            EventManager.FireEvent("OnLoadPlugins");

            return true;
        }
        catch (Exception ex)
        {
            File.WriteAllText(Path.Combine(Kernel.BasePath, "boot-error.log"),
                $"The plugin manager encountered a problem: \n{ex.Message} at {ex.StackTrace}");
            return false;
        }
    }

    /// <summary>
    ///     Dynamically loads plugins from a single assembly file at runtime.
    /// </summary>
    /// <param name="file">The DLL path.</param>
    /// <returns>List of newly loaded plugins.</returns>
    public List<IPlugin> LoadSingleAssembly(string file)
    {
        var loaded = new List<IPlugin>();
        if (Extensions == null)
            Extensions = new Dictionary<string, IPlugin>();

        try
        {
            var newExtensions = GetExtensionsFromAssembly(file);
            foreach (var extension in newExtensions)
            {
                if (Extensions.ContainsKey(extension.Key))
                {
                    Log.Warn($"Plugin [{extension.Key}] is already loaded.");
                    continue;
                }

                try
                {
                    extension.Value.Initialize();
                    Extensions.Add(extension.Key, extension.Value);
                    loaded.Add(extension.Value);
                    Log.Notify($"Dynamically loaded plugin [{extension.Value.DisplayName}]!");
                }
                catch (Exception ex)
                {
                    Log.Error($"Failed to initialize dynamic plugin [{extension.Value.DisplayName}]: {ex.Message}");
                }
            }
        }
        catch (Exception ex)
        {
            Log.Error($"Failed to load assembly [{file}]: {ex.Message}");
        }

        return loaded;
    }

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
            var assembly = Assembly.LoadFrom(file);
            Type[] assemblyTypes;
            try
            {
                assemblyTypes = assembly.GetTypes();
            }
            catch (ReflectionTypeLoadException ex)
            {
                assemblyTypes = ex.Types.Where(t => t != null).ToArray();
            }

            foreach (var type in assemblyTypes.Where(type => type != null && type.IsPublic && !type.IsAbstract && typeof(IPlugin).IsAssignableFrom(type)))
            {
                try
                {
                    var extension = (IPlugin)Activator.CreateInstance(type);
                    if (extension != null && !result.ContainsKey(extension.InternalName))
                        result.Add(extension.InternalName, extension);
                }
                catch (Exception ex)
                {
                    Log.Warn($"Failed to create plugin instance [{type.FullName}]: {ex.Message}");
                }
            }

            if (result.Count == 0)
                return result;

            var handlerType = typeof(IPacketHandler);
            var hookType = typeof(IPacketHook);

            var types = assemblyTypes
                .Where(p => p != null && handlerType.IsAssignableFrom(p) && !p.IsInterface && !p.IsAbstract).ToArray();

            foreach (var handler in types)
            {
                try
                {
                    PacketManager.RegisterHandler((IPacketHandler)Activator.CreateInstance(handler));
                }
                catch (Exception ex)
                {
                    Log.Warn($"Failed to register packet handler [{handler.Name}]: {ex.Message}");
                }
            }

            types = assemblyTypes
                .Where(p => p != null && hookType.IsAssignableFrom(p) && !p.IsInterface && !p.IsAbstract).ToArray();

            foreach (var hook in types)
            {
                try
                {
                    PacketManager.RegisterHook((IPacketHook)Activator.CreateInstance(hook));
                }
                catch (Exception ex)
                {
                    Log.Warn($"Failed to register packet hook [{hook.Name}]: {ex.Message}");
                }
            }
        }
        catch
        {
            /* ignore, it's an invalid or non-managed extension DLL */
        }

        return result;
    }
}