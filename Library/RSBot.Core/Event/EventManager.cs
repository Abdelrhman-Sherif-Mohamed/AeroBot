using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace RSBot.Core.Event;

public class EventManager
{
    private static readonly List<(string name, Delegate handler)> _listeners = new();

    /// <summary>
    ///     Registers the event.
    /// </summary>
    /// <param name="name">The name.</param>
    /// <param name="handler">The handler.</param>
    public static void SubscribeEvent(string name, Delegate handler)
    {
        if (handler == null) return;

        lock (_listeners)
        {
            _listeners.Add((name, handler));
        }
    }

    /// <summary>
    ///     Registers the event.
    /// </summary>
    /// <param name="name">The name.</param>
    /// <param name="handler">The handler.</param>
    public static void SubscribeEvent(string name, Action handler)
    {
        if (handler == null) return;

        lock (_listeners)
        {
            _listeners.Add((name, handler));
        }
    }

    /// <summary>
    ///     Unregisters the event.
    /// </summary>
    /// <param name="name">The name.</param>
    /// <param name="handler">The handler.</param>
    public static void UnsubscribeEvent(string name, Delegate handler)
    {
        if (handler == null) return;

        lock (_listeners)
        {
            _listeners.RemoveAll(l => l.name == name && Equals(l.handler, handler));
        }
    }

    /// <summary>
    ///     Unregisters the event.
    /// </summary>
    /// <param name="name">The name.</param>
    /// <param name="handler">The handler.</param>
    public static void UnsubscribeEvent(string name, Action handler)
    {
        if (handler == null) return;

        lock (_listeners)
        {
            _listeners.RemoveAll(l => l.name == name && Equals(l.handler, handler));
        }
    }

    /// <summary>
    ///     Fires the event.
    /// </summary>
    /// <param name="name">The name.</param>
    /// <param name="parameters">The parameters.</param>
    public static void FireEvent(string name, params object[] parameters)
    {
        try
        {
            Delegate[] targets;
            lock (_listeners)
            {
                targets = (from o in _listeners
                    where o.name == name && o.handler.Method.GetParameters().Length == parameters.Length
                    select o.handler).ToArray();
            }

            foreach (var target in targets)
                if (Thread.CurrentThread.Name == "Network.PacketProcessor")
                    Task.Run(() =>
                    {
                        try
                        {
                            target.DynamicInvoke(parameters);
                        }
                        catch (Exception ex)
                        {
                            var realEx = (ex is System.Reflection.TargetInvocationException tie && tie.InnerException != null) ? tie.InnerException : ex;
                            Log.Debug($"[EventManager] Error executing event '{name}': {realEx.Message}");
                        }
                    });
                else
                    target.DynamicInvoke(parameters);
        }
        catch (Exception e)
        {
            Log.Fatal(e);
        }
    }
}