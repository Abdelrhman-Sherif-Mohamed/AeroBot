using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows.Forms;

namespace RSBot.Core.Components;

using LangDict = Dictionary<string, string>;

public class LanguageManager
{
    private static readonly string _path = Path.Combine(Kernel.BasePath, "Data", "Languages");

    /// <summary>
    ///     Parsed language values
    /// </summary>
    private static readonly Dictionary<string, LangDict> _values = new();

    /// <summary>
    ///     Get all menu items
    /// </summary>
    /// <param name="menuItem">The toolstrip menu item</param>
    /// <returns></returns>
    private static List<ToolStripMenuItem> GetAllMenuItems(ToolStripMenuItem menuItem)
    {
        var collection = new List<ToolStripMenuItem> { menuItem };
        foreach (ToolStripMenuItem item in menuItem.DropDownItems)
            collection.AddRange(GetAllMenuItems(item));

        return collection;
    }

    /// <summary>
    ///     Parse the language file
    /// </summary>
    /// <param name="file">The language file</param>
    /// <returns>Parsed language strings</returns>
    public static LangDict ParseLanguageFile(string file)
    {
        var languages = new LangDict();

        var text = File.ReadAllText(file);

        var builder = new StringBuilder();

        var key = string.Empty;
        var value = string.Empty;

        var doubleQuotes = false;
        var skipDoubleQuotes = false;

        foreach (var c in text)
        {
            if (c == '=')
            {
                key = builder.ToString().Trim();
                builder = new StringBuilder();
                value = string.Empty;

                if (languages.ContainsKey(key))
                {
                    key = string.Empty;
                    skipDoubleQuotes = true;
                }

                continue;
            }

            if (c == '"')
            {
                doubleQuotes = !doubleQuotes;
                if (skipDoubleQuotes)
                    continue;

                value = builder.ToString(); /*.Trim()*/
                builder.Clear();

                languages[key] = value;
                value = string.Empty;

                continue;
            }

            if (doubleQuotes && skipDoubleQuotes)
                continue;

            if ((c == '\r' || c == '\n') && !doubleQuotes)
                continue;

            builder.Append(c);
        }

        return languages;
    }

    /// <summary>
    ///     Compare between controls ang languages strings if there have any missing complete and write to language file
    /// </summary>
    /// <param name="file">The language file path</param>
    /// <param name="controls">The controls</param>
    /// <param name="languages">the parsed languages list</param>
    private static void CheckMissings(string file, string header, Control main, LangDict languages)
    {
        var contents = new List<string>();

        foreach (Control control in main.Controls)
        {
            var headerEx = $"{header}.{control.Parent.GetType().Name}";
            if (control is ToolStrip toolStrip)
            {
                var menuItems = new List<ToolStripItem>();
                foreach (var menuItem in toolStrip.Items.OfType<ToolStripMenuItem>())
                    menuItems.AddRange(GetAllMenuItems(menuItem));

                foreach (var item in menuItems)
                {
                    if (string.IsNullOrEmpty(item.Name) ||
                        string.IsNullOrEmpty(item.Text))
                        continue;

                    var menuItemCheckName = $"{headerEx}.{item.Name}";
                    if (!languages.ContainsKey(menuItemCheckName))
                    {
                        contents.Add($"{menuItemCheckName}=\"{item.Text}\"");

                        languages[item.Name] = item.Text;
                    }
                }
            }

            if (control is ToolStrip)
                continue;

            CheckMissings(file, headerEx, control, languages);

            if (!(control is Label) &&
                !(control is GroupBox) &&
                !(control is ButtonBase) &&
                !(control is TabControl) &&
                !(control is TabPage) &&
                !(control is ToolStrip))
                continue;

            if (string.IsNullOrEmpty(control.Name) ||
                string.IsNullOrEmpty(control.Text))
                continue;

            var checkName = $"{headerEx}.{control.Name}";
            if (!languages.ContainsKey(checkName))
            {
                contents.Add($"{checkName}=\"{control.Text}\"");

                languages[control.Name] = control.Text;
            }
        }

        if (contents.Count > 0)
            File.AppendAllLines(file, contents);
    }

    /// <summary>
    ///     Get language value
    /// </summary>
    /// <param name="key">The key</param>
    [MethodImpl(MethodImplOptions.NoInlining)]
    public static string GetLang(string key)
    {
        // Fast path: direct callers resolve without building a stack trace.
        // NoInlining guarantees GetCallingAssembly() sees the real caller.
        string parent = null;
        try
        {
            parent = Assembly.GetCallingAssembly()?.GetName().Name;
        }
        catch
        {
        }

        if (string.IsNullOrEmpty(parent) || parent == "AeroBot.Core" || parent == "SonicBot.Core" || parent == "RSBot.Core")
            parent = FindNonCoreCaller();

        if (!string.IsNullOrEmpty(parent))
        {
            foreach (var candidate in GetParentCandidates(parent))
            {
                if (_values.TryGetValue(candidate, out var dict) && dict.TryGetValue(key, out var val))
                    return val;
            }
        }

        return string.Empty;
    }

    private static IEnumerable<string> GetParentCandidates(string parent)
    {
        if (string.IsNullOrEmpty(parent)) yield break;
        yield return parent;

        yield return parent.Replace("AeroBot", "SonicBot").Replace("RSBot", "SonicBot");
        yield return parent.Replace("AeroBot", "RSBot").Replace("SonicBot", "RSBot");
        yield return parent.Replace("SonicBot", "AeroBot").Replace("RSBot", "AeroBot");
    }

    /// <summary>
    ///     Walks the stack for the first frame outside of the core assembly (slow path,
    ///     only used when GetLang is called indirectly through core code).
    /// </summary>
    private static string FindNonCoreCaller()
    {
        var trace = new StackTrace();

        var parent = string.Empty;
        for (var i = 0; i < trace.FrameCount; i++)
        {
            parent = Path.GetFileNameWithoutExtension(trace.GetFrame(i).GetMethod().Module.Name);
            if (parent != "AeroBot.Core" && parent != "SonicBot.Core" && parent != "RSBot.Core")
                break;
        }

        return parent;
    }

    /// <summary>
    ///     Get language value
    /// </summary>
    /// <param name="key">The key</param>
    public static string GetLang(string key, params object[] args)
    {
        return string.Format(GetLang(key), args);
    }

    /// <summary>
    ///     Get language value
    /// </summary>
    /// <param name="key">The key</param>
    /// <param name="default">The default value that will be returned if the translation could not be found</param>
    public static string GetLangBySpecificKey(string parent, string key, string @default = "")
    {
        foreach (var candidate in GetParentCandidates(parent))
        {
            if (_values.TryGetValue(candidate, out var dict) && dict.TryGetValue(key, out var val))
                return val;
        }

        return @default;
    }

    /// <summary>
    ///     Translate the control
    /// </summary>
    /// <param name="view">The control view</param>
    /// <param name="file">The language file path</param>
    public static void Translate(Control view, string language = "en_US")
    {
        var type = view.GetType();

        var controlName = type.FullName;
        var assembly = type.Assembly.GetName().Name;

        var path = Path.Combine(_path, assembly, language + ".rsl");
        if (!File.Exists(path))
        {
            foreach (var candidate in GetParentCandidates(assembly))
            {
                var candPath = Path.Combine(_path, candidate, language + ".rsl");
                if (File.Exists(candPath))
                {
                    path = candPath;
                    break;
                }
            }
        }

        var dir = Path.GetDirectoryName(path);

        if (!Directory.Exists(dir))
            Directory.CreateDirectory(dir);

        var stopwatch = Stopwatch.StartNew();

        if (!File.Exists(path))
            return;
        //File.CreateText(path).Dispose();

        //var instance = (Control)Activator.CreateInstance(type);

        var values = ParseLanguageFile(path);
        //CheckMissings(path, assembly, instance, values);


        _values[assembly] = values;

        TranslateControls(values, view, assembly);
    }

    private static void TranslateControls(LangDict values, Control view, string header)
    {
        foreach (Control control in view.Controls)
        {
            var headerEx = $"{header}.{control.Parent.GetType().Name}";

            string translatedText;

            if (control is ToolStrip strip)
            {
                foreach (var toolStripItem in strip.Items.OfType<ToolStripMenuItem>())
                {
                    var subItems = GetAllMenuItems(toolStripItem);
                    foreach (var subMenuItem in subItems)
                        if (values.TryGetValue($"{headerEx}.{subMenuItem.Name}", out translatedText))
                            if (!string.IsNullOrWhiteSpace(translatedText))
                                subMenuItem.Text = translatedText;
                }

                continue;
            }

            if (control is System.Windows.Forms.ListView lv)
            {
                foreach (ColumnHeader col in lv.Columns)
                {
                    if (values.TryGetValue($"{headerEx}.{col.Name}", out translatedText) ||
                        values.TryGetValue($"{header}.{col.Name}", out translatedText) ||
                        values.TryGetValue($"{col.Name}", out translatedText))
                    {
                        if (!string.IsNullOrWhiteSpace(translatedText))
                            col.Text = translatedText;
                    }
                }
            }

            if (control.ContextMenuStrip != null)
            {
                foreach (var item in control.ContextMenuStrip.Items.OfType<ToolStripItem>())
                {
                    if (values.TryGetValue($"{headerEx}.{item.Name}", out translatedText) ||
                        values.TryGetValue($"{header}.{item.Name}", out translatedText) ||
                        values.TryGetValue($"{item.Name}", out translatedText))
                    {
                        if (!string.IsNullOrWhiteSpace(translatedText))
                            item.Text = translatedText;
                    }
                }
            }

            if (values.TryGetValue($"{headerEx}.{control.Name}", out translatedText) ||
                values.TryGetValue($"{header}.{control.Name}", out translatedText) ||
                values.TryGetValue($"{control.Name}", out translatedText))
            {
                if (!string.IsNullOrWhiteSpace(translatedText))
                    control.Text = translatedText;
            }

            TranslateControls(values, control, headerEx);
        }
    }

    public static Dictionary<string, string> GetLanguages()
    {
        var filePath = Path.Combine(_path, "langs.rsl");
        if (!File.Exists(filePath))
        {
            MessageBox.Show($"Language list file is missing! \n {filePath}");
            Environment.Exit(0);
        }

        return File.ReadAllLines(filePath)
            .ToDictionary(p => p.Split(':')[0], p => p.Split(':')[1]);
    }
}