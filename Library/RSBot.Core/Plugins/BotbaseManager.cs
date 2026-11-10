using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using RSBot.Core.Event;

namespace RSBot.Core.Plugins;

public class BotbaseManager
{
    /// <summary>
    ///     Gets the extension directory.
    /// </summary>
    /// <value>
    ///     The extension directory.
    /// </value>
    public string DirectoryPath => Path.Combine(Kernel.BasePath, "Data", "Bots");

    /// <summary>
    ///     Gets the extensions.
    /// </summary>
    /// <value>
    ///     The extensions.
    /// </value>
    public Dictionary<string, IBotbase> Bots { get; private set; }

    /// <summary>
    ///     Loads the assemblies.
    /// </summary>
    public bool LoadAssemblies()
    {
        if (Bots != null)
            return true;

        try
        {
            Bots = new Dictionary<string, IBotbase>();

            if (!Directory.Exists(DirectoryPath))
                Directory.CreateDirectory(DirectoryPath);

            foreach (var file in Directory.GetFiles(DirectoryPath, "*.dll"))
            {
                try
                {
                    var loaded = GetExtensionsFromAssembly(file);
                    foreach (var extension in loaded)
                    {
                        if (extension.Key != null && extension.Value != null)
                        {
                            Bots[extension.Key] = extension.Value;
                            try
                            {
                                extension.Value.Register();
                            }
                            catch (Exception regEx)
                            {
                                Log.Error($"Error registering botbase [{extension.Value.Name}]: {regEx.Message}");
                            }

                            Log.Debug($"Loaded botbase [{extension.Value.Name}]");
                        }
                    }
                }
                catch (Exception fileEx)
                {
                    Log.Error($"Failed loading botbase from '{Path.GetFileName(file)}': {fileEx.Message}");
                }
            }

            try
            {
                EventManager.FireEvent("OnLoadBotbases");
            }
            catch (Exception evEx)
            {
                Log.Error($"OnLoadBotbases event handler error: {evEx.Message}");
            }

            return true;
        }
        catch (Exception ex)
        {
            try
            {
                File.WriteAllText(Path.Combine(Kernel.BasePath, "boot-error.log"),
                    $"The botbase manager encountered a problem: \n{ex.Message} at {ex.StackTrace}");
            }
            catch { }
            return false;
        }
    }

    /// <summary>
    ///     Gets the extensions from assembly.
    /// </summary>
    /// <param name="file">The file.</param>
    /// <returns></returns>
    private static Dictionary<string, IBotbase> GetExtensionsFromAssembly(string file)
    {
        var result = new Dictionary<string, IBotbase>();

        try
        {
            var fileName = Path.GetFileName(file);
            if (fileName.Equals("WebView2Loader.dll", StringComparison.OrdinalIgnoreCase))
                return result;

            var assembly = Assembly.LoadFrom(file);
            var types = assembly.GetTypes();

            foreach (var extension in (from type in types
                         where type.IsPublic && !type.IsAbstract && type.GetInterface("IBotbase") != null
                         select Activator.CreateInstance(type))
                     .OfType<IBotbase>()) result.Add(extension.Name, extension);
        }
        catch
        {
            /* ignore, it's a native dll or an invalid botbase */
        }

        return result;
    }
}