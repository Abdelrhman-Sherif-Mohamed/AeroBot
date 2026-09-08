using System;
using System.Threading;
using System.Windows.Forms;
using RSBot.Core;

namespace RSBot.CommandCenter.Components.InGamePanel;

/// <summary>
///     Owns the single on-demand in-game panel (opened by the !bot command).
///     Never auto-shows; all access is marshalled to the UI thread.
/// </summary>
internal static class GamePanelManager
{
    private static GamePanelForm _form;
    private static SynchronizationContext _uiContext;

    public static bool IsVisible => _form != null && !_form.IsDisposed && _form.Visible;

    public static void Initialize()
    {
        _uiContext ??= SynchronizationContext.Current;
    }

    public static void Toggle()
    {
        if (_uiContext != null && SynchronizationContext.Current != _uiContext)
        {
            _uiContext.Post(_ => Toggle(), null);
            return;
        }

        try
        {
            if (IsVisible)
            {
                _form.Hide();
                return;
            }

            if (_form == null || _form.IsDisposed)
                _form = new GamePanelForm();

            if (!_form.Visible)
                _form.Show();
            _form.BringToFront();
        }
        catch (Exception ex)
        {
            Log.Error(ex);
        }
    }

    public static void Hide()
    {
        if (_uiContext != null && SynchronizationContext.Current != _uiContext)
        {
            _uiContext.Post(_ => Hide(), null);
            return;
        }

        try
        {
            if (_form == null || _form.IsDisposed)
                return;
            _form.Hide();
        }
        catch (Exception ex)
        {
            Log.Error(ex);
        }
    }
}
