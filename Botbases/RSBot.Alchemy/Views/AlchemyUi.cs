using System;
using System.Drawing;
using System.Reflection;
using System.Windows.Forms;
using Panel = System.Windows.Forms.Panel;
using RSBot.Alchemy.Bot;
using RSBot.Core;
using RSBot.Core.Components;
using SDUI;
using SDUI.Controls;

using Button = SDUI.Controls.Button;
using CheckBox = SDUI.Controls.CheckBox;
using ComboBox = SDUI.Controls.ComboBox;
using GroupBox = SDUI.Controls.GroupBox;
using Label = SDUI.Controls.Label;
using ProgressBar = SDUI.Controls.ProgressBar;
using Radio = SDUI.Controls.Radio;
using TabControl = SDUI.Controls.TabControl;

namespace RSBot.Alchemy.Views;

internal enum ButtonTone
{
    Neutral,
    Primary,
    Success,
    Danger
}

internal static class AlchemyUi
{
    public static bool IsArabic => string.Equals(Kernel.Language, "ar_AR", StringComparison.OrdinalIgnoreCase);

    public static string T(string key, string arabic, string english)
    {
        var localized = LanguageManager.GetLangBySpecificKey("RSBot.Alchemy", key, string.Empty);
        if (!string.IsNullOrWhiteSpace(localized))
            return localized;

        return IsArabic ? arabic : english;
    }

    public static Font CreateFont(float size = 9f, FontStyle style = FontStyle.Regular)
    {
        var family = string.IsNullOrWhiteSpace(ColorScheme.GlobalFontFamily)
            ? "Segoe UI"
            : ColorScheme.GlobalFontFamily;

        return new Font(family, Math.Max(8f, size + ColorScheme.GlobalFontSizeOffset), style, GraphicsUnit.Point);
    }

    public static GroupBox CreateGroupBox(string text = "")
    {
        return new GroupBox
        {
            Text = text,
            BackColor = Color.Transparent,
            ForeColor = ColorScheme.ForeColor,
            Font = CreateFont(9f, FontStyle.Bold),
            Radius = 8,
            ShadowDepth = 2,
            Padding = new Padding(8, 12, 8, 8),
            Margin = new Padding(4),
            TabStop = false
        };
    }

    public static Label CreateLabel(string text = "", ContentAlignment align = ContentAlignment.MiddleLeft,
        bool bold = false, bool secondary = false)
    {
        return new Label
        {
            Text = text,
            AutoSize = false,
            BackColor = Color.Transparent,
            ForeColor = secondary ? ColorScheme.SecondaryForeColor : ColorScheme.ForeColor,
            Font = CreateFont(9f, bold ? FontStyle.Bold : FontStyle.Regular),
            TextAlign = align,
            Margin = new Padding(2)
        };
    }

    public static CheckBox CreateCheckBox(string text, bool isChecked = false)
    {
        return new CheckBox
        {
            Text = text,
            Checked = isChecked,
            AutoSize = false,
            BackColor = Color.Transparent,
            ForeColor = ColorScheme.ForeColor,
            Font = CreateFont(9f),
            Height = 24,
            Margin = new Padding(2),
            UseVisualStyleBackColor = false
        };
    }

    public static Radio CreateRadio(string text, bool isChecked = false)
    {
        return new Radio
        {
            Text = text,
            Checked = isChecked,
            AutoSize = false,
            BackColor = Color.Transparent,
            ForeColor = ColorScheme.ForeColor,
            Font = CreateFont(9f, FontStyle.Bold),
            Height = 26,
            Margin = new Padding(4, 2, 4, 2)
        };
    }

    public static Button CreateButton(string text, ButtonTone tone = ButtonTone.Neutral, int height = 30)
    {
        var btn = new Button
        {
            Text = text,
            Tag = tone,
            BackColor = Color.Transparent,
            Font = CreateFont(9f),
            Radius = 5,
            ShadowDepth = 1,
            Height = height,
            Margin = new Padding(3),
            Cursor = Cursors.Hand
        };

        ApplyButtonStyle(btn, tone);
        return btn;
    }

    public static void ApplyButtonStyle(Button btn, ButtonTone tone)
    {
        if (btn == null)
            return;

        btn.Tag = tone;
        var isDark = ColorScheme.BackColor.IsDark();

        switch (tone)
        {
            case ButtonTone.Success:
                btn.Color = Color.FromArgb(34, 160, 107);
                btn.ForeColor = Color.White;
                break;

            case ButtonTone.Danger:
                btn.Color = Color.FromArgb(208, 56, 56);
                btn.ForeColor = Color.White;
                break;

            case ButtonTone.Primary:
                btn.Color = ColorScheme.ButtonColor;
                btn.ForeColor = ColorScheme.ButtonForeColor;
                break;

            default:
                btn.Color = isDark ? Color.FromArgb(40, 50, 66) : Color.FromArgb(241, 245, 249);
                btn.ForeColor = ColorScheme.ForeColor;
                break;
        }

        btn.Invalidate();
    }

    public static ComboBox CreateComboBox(DropDownStyleMode style = DropDownStyleMode.DropDownList)
    {
        var combo = new ComboBox
        {
            DropDownStyle = style == DropDownStyleMode.DropDownList
                ? ComboBoxStyle.DropDownList
                : ComboBoxStyle.DropDown,
            BackColor = ColorScheme.InputBackColor,
            ForeColor = ColorScheme.ForeColor,
            Font = CreateFont(9f),
            Radius = 5,
            ShadowDepth = 0,
            Height = 28,
            Margin = new Padding(2)
        };

        return combo;
    }

    public enum DropDownStyleMode
    {
        DropDownList,
        DropDown
    }

    public static NumUpDown CreateNumUpDown(decimal min, decimal max, decimal value, int width = 76)
    {
        // Note: Set Maximum before Minimum, then Value last, because SDUI.NumUpDown clamps against current bounds.
        var num = new NumUpDown
        {
            Maximum = max,
            Minimum = min,
            Value = Math.Clamp(value, min, max),
            Width = width,
            Height = 26,
            Font = CreateFont(9f),
            BoxColor = ColorScheme.InputBackColor,
            ForeColor = ColorScheme.ForeColor,
            Margin = new Padding(2)
        };

        return num;
    }

    public static ProgressBar CreateProgressBar()
    {
        return new ProgressBar
        {
            MinimumSize = new Size(60, 18),
            Size = new Size(160, 20),
            Height = 20,
            Maximum = 100,
            Value = 0,
            Radius = 4,
            ShowAsPercent = true,
            ForeColor = ColorScheme.ForeColor,
            BackColor = ColorScheme.InputBackColor
        };
    }

    public static TabControl CreateTabControl(int tabWidth = 115, int tabHeight = 28)
    {
        return new TabControl
        {
            Dock = DockStyle.Fill,
            Font = CreateFont(9f),
            SizeMode = TabSizeMode.Fixed,
            ItemSize = new Size(tabWidth, tabHeight),
            Margin = new Padding(0)
        };
    }

    public static DataGridView CreateGrid(bool allowEdit = false)
    {
        var grid = new DataGridView
        {
            Dock = DockStyle.Fill,
            AllowUserToAddRows = false,
            AllowUserToDeleteRows = false,
            AllowUserToResizeRows = false,
            MultiSelect = false,
            ReadOnly = !allowEdit,
            EditMode = allowEdit ? DataGridViewEditMode.EditOnEnter : DataGridViewEditMode.EditProgrammatically,
            RowHeadersVisible = false,
            SelectionMode = DataGridViewSelectionMode.FullRowSelect,
            AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
            BorderStyle = BorderStyle.FixedSingle,
            CellBorderStyle = DataGridViewCellBorderStyle.Single,
            ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.Single,
            ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing,
            ColumnHeadersHeight = 28,
            EnableHeadersVisualStyles = false,
            Font = CreateFont(9f)
        };

        if (allowEdit)
        {
            grid.CurrentCellDirtyStateChanged += (_, _) =>
            {
                if (grid.IsCurrentCellDirty)
                    grid.CommitEdit(DataGridViewDataErrorContexts.Commit);
            };
        }

        grid.DataError += (_, e) => { e.ThrowException = false; };
        grid.RowTemplate.Height = 25;
        EnableDoubleBuffering(grid);
        ApplyGridTheme(grid);
        return grid;
    }

    public static void ApplyGridTheme(DataGridView grid)
    {
        if (grid == null || grid.IsDisposed)
            return;

        grid.BackgroundColor = ColorScheme.TableBackColor;
        grid.GridColor = ColorScheme.BorderColor;
        grid.ForeColor = ColorScheme.ForeColor;

        grid.ColumnHeadersDefaultCellStyle = new DataGridViewCellStyle
        {
            BackColor = ColorScheme.TableHeaderBackColor,
            ForeColor = ColorScheme.TableHeaderForeColor,
            SelectionBackColor = ColorScheme.TableHeaderBackColor,
            SelectionForeColor = ColorScheme.TableHeaderForeColor,
            Font = CreateFont(8.5f, FontStyle.Bold),
            Alignment = DataGridViewContentAlignment.MiddleCenter,
            Padding = new Padding(2, 2, 2, 2)
        };

        grid.DefaultCellStyle = new DataGridViewCellStyle
        {
            BackColor = ColorScheme.TableBackColor,
            ForeColor = ColorScheme.ForeColor,
            SelectionBackColor = ColorScheme.SelectionColor,
            SelectionForeColor = Color.White,
            Font = CreateFont(9f),
            Padding = new Padding(4, 1, 4, 1)
        };

        grid.AlternatingRowsDefaultCellStyle = new DataGridViewCellStyle
        {
            BackColor = ColorScheme.TableAlternateRowColor,
            ForeColor = ColorScheme.ForeColor,
            SelectionBackColor = ColorScheme.SelectionColor,
            SelectionForeColor = Color.White,
            Font = CreateFont(9f),
            Padding = new Padding(4, 1, 4, 1)
        };
    }

    public static void ApplyThemeRecursive(Control root)
    {
        if (root == null || root.IsDisposed)
            return;

        foreach (Control control in root.Controls)
        {
            switch (control)
            {
                case GroupBox gb:
                    gb.BackColor = Color.Transparent;
                    gb.ForeColor = ColorScheme.ForeColor;
                    break;

                case Label lbl:
                    lbl.BackColor = Color.Transparent;
                    if (lbl.Tag as string != "CustomFore")
                        lbl.ForeColor = ColorScheme.ForeColor;
                    break;

                case CheckBox cb:
                    cb.BackColor = Color.Transparent;
                    cb.ForeColor = ColorScheme.ForeColor;
                    break;

                case Radio rb:
                    rb.BackColor = Color.Transparent;
                    rb.ForeColor = ColorScheme.ForeColor;
                    break;

                case Button btn:
                    ApplyButtonStyle(btn, btn.Tag is ButtonTone tone ? tone : ButtonTone.Neutral);
                    break;

                case ComboBox combo:
                    combo.BackColor = ColorScheme.InputBackColor;
                    combo.ForeColor = ColorScheme.ForeColor;
                    break;

                case NumUpDown num:
                    num.BoxColor = ColorScheme.InputBackColor;
                    num.ForeColor = ColorScheme.ForeColor;
                    break;

                case ProgressBar pb:
                    pb.BackColor = ColorScheme.InputBackColor;
                    pb.ForeColor = ColorScheme.ForeColor;
                    break;

                case DataGridView grid:
                    ApplyGridTheme(grid);
                    break;

                case TabPage page:
                    page.BackColor = ColorScheme.BackColor;
                    page.ForeColor = ColorScheme.ForeColor;
                    break;

                case Panel panel when panel.Tag as string == "CardSurface":
                    panel.BackColor = ColorScheme.CardColor;
                    panel.ForeColor = ColorScheme.ForeColor;
                    break;

                case TableLayoutPanel or FlowLayoutPanel or Panel:
                    if (control.Tag as string != "KeepBack")
                        control.BackColor = Color.Transparent;
                    control.ForeColor = ColorScheme.ForeColor;
                    break;
            }

            if (control.HasChildren)
                ApplyThemeRecursive(control);
        }

        root.Invalidate(true);
    }

    public static void EnsureAlchemyBotStarted(AlchemyEngine engine)
    {
        if (Globals.Botbase == null)
            return;

        var runner = Bootstrap.EnsureRunner();
        if (runner == null)
            return;

        if (runner.Running)
            runner.Stop();

        Globals.Botbase.AlchemyEngine = engine;
        if (!runner.TryStart(out var errorMessage) && !string.IsNullOrWhiteSpace(errorMessage))
            MessageBox.Show(errorMessage, "AeroBot", MessageBoxButtons.OK, MessageBoxIcon.Warning);
    }

    public static void StopAlchemyBot()
    {
        if (Bootstrap.Runner?.Running == true)
            Bootstrap.StopRunner();
        else
            Globals.Botbase?.Stop();
    }

    public static void EnableDoubleBuffering(Control control)
    {
        try
        {
            typeof(Control).InvokeMember("DoubleBuffered",
                BindingFlags.SetProperty | BindingFlags.Instance | BindingFlags.NonPublic,
                null, control, new object[] { true });
        }
        catch
        {
            // Ignore reflection failure.
        }
    }
}
