using System.IO;
using System.Text;
using System.Threading;

namespace System.Windows.Forms;

public static class TextBoxBaseExtensions
{
    /// <summary>
    /// Sync lock object
    /// </summary>
    private static readonly object _lock = new object();

    /// <summary>
    /// Append string to type in the text controls
    /// </summary>
    /// <param name="value">The TextBoxBase</param>
    /// <param name="str">The string to type in the <seealso cref="TextBoxBase"/></param>
    /// <param name="time">The time</param>
    public static void Write(this TextBoxBase value, string str, bool time = true, bool writeToFile = false, string filePath = "")
    {
        if (value == null || value.IsDisposed || value.Disposing)
            return;

        var stringBuilder = new StringBuilder();
        if (time)
            stringBuilder.Append(DateTime.Now.ToString("[hh:mm:ss]\t"));

        stringBuilder.Append(str);
        stringBuilder.Append(Environment.NewLine);

        value.RunInUIThread(() =>
        {
            if (value.IsDisposed || value.Disposing)
                return;

            value.AppendText(stringBuilder.ToString());
            value.ScrollToCaret();
        });

        if (writeToFile && !string.IsNullOrWhiteSpace(filePath))
        {
            lock (_lock)
            {
                var dir = Path.GetDirectoryName(filePath);
                if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
                    Directory.CreateDirectory(dir);

                using (var stream = File.AppendText(filePath))
                    stream.Write(stringBuilder.ToString());
            }
        }
    }

    /// <summary>
    /// Run action a required thread on controls
    /// </summary>
    /// <param name="target">The target</param>
    /// <param name="action">The action</param>
    public static void RunInUIThread(this Control target, Action action)
    {
        if (target == null || action == null || target.IsDisposed || target.Disposing)
            return;

        Control invoker = target.IsHandleCreated ? target : target.FindForm();
        if ((invoker == null || !invoker.IsHandleCreated) && Application.OpenForms.Count > 0)
            invoker = Application.OpenForms[0];

        if (invoker != null && !invoker.IsDisposed && !invoker.Disposing && invoker.IsHandleCreated)
        {
            if (invoker.InvokeRequired)
            {
                try
                {
                    invoker.Invoke(action);
                }
                catch (ObjectDisposedException)
                {
                }
                catch (InvalidOperationException)
                {
                }
            }
            else
            {
                action();
            }

            return;
        }

        // Never create a Win32 control handle on an MTA worker thread when no UI handle exists yet.
        if (Thread.CurrentThread.GetApartmentState() == ApartmentState.STA)
            action();
    }
}