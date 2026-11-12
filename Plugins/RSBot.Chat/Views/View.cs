using System;
using System.Windows.Forms;

namespace RSBot.Chat.Views;

internal class View
{
    private static Main _instance;
    private static readonly object _sync = new();

    /// <summary>
    ///     Gets the singleton chat view instance, ensuring WinForms creation on the UI thread when available.
    /// </summary>
    public static Main Instance
    {
        get
        {
            if (_instance != null && !_instance.IsDisposed)
                return _instance;

            lock (_sync)
            {
                if (_instance == null || _instance.IsDisposed)
                {
                    if (Application.OpenForms.Count > 0 && Application.OpenForms[0] is Form mainForm &&
                        !mainForm.IsDisposed && mainForm.IsHandleCreated && mainForm.InvokeRequired)
                    {
                        _instance = (Main)mainForm.Invoke(new Func<Main>(() => new Main()));
                    }
                    else
                    {
                        _instance = new Main();
                    }
                }
            }

            return _instance;
        }
    }
}
