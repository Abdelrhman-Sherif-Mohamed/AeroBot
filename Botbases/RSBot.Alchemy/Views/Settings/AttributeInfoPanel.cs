using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using RSBot.Core.Extensions;
using RSBot.Core.Objects;
using SDUI;
using SDUI.Controls;

namespace RSBot.Alchemy.Views.Settings;

[ToolboxItem(false)]
public partial class AttributeInfoPanel : UserControl
{
    public delegate void OnChangedEventHandler(bool @checked, int maxValue);

    private readonly bool _hasStones;

    /// <summary>
    ///     Gets the current attribute.
    /// </summary>
    /// <value>
    ///     The current attribute.
    /// </value>
    private ItemAttributesInfo _currentAttribute;

    /// <summary>
    ///     The current attribute slot
    /// </summary>
    private readonly byte _currentAttributeSlot;

    /// Initializes a new instance of the
    /// <see cref="AttributeInfoPanel" />
    /// class.
    /// </summary>
    /// <param name="group">The group.</param>
    /// <param name="stones">The stones.</param>
    /// <param name="item">The item.</param>
    public AttributeInfoPanel(ItemAttributeGroup group, IEnumerable<InventoryItem>? stones, InventoryItem item,
        int maxValue = 22)
    {
        CheckForIllegalCrossThreadCalls = false;

        InitializeComponent();
        DoubleBuffered = true;

        AttributeGroup = group;
        Stones = stones;
        _currentAttribute = item.Attributes;
        _currentAttributeSlot = ItemAttributesInfo.GetAttributeSlotForItem(group, item.Record);

        SetMaxValue(maxValue);

        var totalAmount = stones == null || !stones.Any() ? 0 : stones.Sum(i => i.Amount);
        _hasStones = totalAmount > 0;

        if (totalAmount == 0)
        {
            lblItemAmount.Text = "x0";

            comboMaxValue.Enabled = false;
            lblItemAmount.Enabled = false;
            checkSelected.Enabled = false;
        }
        else
        {
            lblItemAmount.Text = $"x{totalAmount}";
        }

        var attributeSlot = ItemAttributesInfo.GetAttributeSlotForItem(group, item.Record);
        checkSelected.Text = $"{group.GetTranslation()} +{item.Attributes.GetPercentage(attributeSlot)}%";
        checkSelected.CheckedChanged += CheckSelected_CheckedChanged;
        comboMaxValue.SelectedIndexChanged += ComboMaxValue_SelectedIndexChanged;

        if (Stones.Any())
            tipStone.SetToolTip(checkSelected, $"{Stones.First().Record.GetRealName()}x{totalAmount}");

        if (GetMaxValue() <= _currentAttribute.GetPercentage(_currentAttributeSlot))
            lblFinished.Show();
        else
            lblFinished.Hide();

        ApplyPanelLayout();
    }

    public event OnChangedEventHandler OnChange;

    private void ComboMaxValue_SelectedIndexChanged(object? sender, EventArgs e)
    {
        if (comboMaxValue.SelectedItem == null)
            return;

        if (GetMaxValue() <= _currentAttribute.GetPercentage(_currentAttributeSlot))
        {
            checkSelected.Checked = false;
            lblFinished.Show();
        }
        else
        {
            checkSelected.Checked = true;
            lblFinished.Hide();
        }

        OnChange?.Invoke(checkSelected.Checked, GetMaxValue());
    }

    #region Events

    private void CheckSelected_CheckedChanged(object? sender, EventArgs e)
    {
        OnChange?.Invoke(checkSelected.Checked, GetMaxValue());
    }

    #endregion Events

    #region Properties

    /// <summary>
    ///     Gets or sets a value indicating whether this <see cref="AttributeInfoPanel" /> is checked.
    /// </summary>
    /// <value>
    ///     <c>true</c> if checked; otherwise, <c>false</c>.
    /// </value>
    public bool Checked
    {
        get => checkSelected.Checked;

        set => checkSelected.Checked = GetMaxValue() > _currentAttribute.GetPercentage(_currentAttributeSlot) && value;
    }

    /// <summary>
    ///     Gets the maximum value this attribute should have.
    /// </summary>
    /// <value>
    ///     The maximum value.
    /// </value>
    public int MaxValue => GetMaxValue();

    /// <summary>
    ///     Gets a value indicating whether this row still has stones and can be
    ///     selected by the bulk actions.
    /// </summary>
    public bool CanBeSelected =>
        _hasStones && checkSelected.Enabled &&
        GetMaxValue() > _currentAttribute.GetPercentage(_currentAttributeSlot);

    /// <summary>
    ///     Gets or sets the attribute group.
    /// </summary>
    /// <value>
    ///     The attribute group.
    /// </value>
    public ItemAttributeGroup AttributeGroup { get; set; }

    /// <summary>
    ///     Gets the stones assigned to this attribute.
    /// </summary>
    /// <value>
    ///     The stones.
    /// </value>
    public IEnumerable<InventoryItem>? Stones { get; init; }

    #endregion Properties

    #region Methods

    /// <summary>
    ///     Gets the maximum value selected by the user.
    /// </summary>
    /// <returns></returns>
    private int GetMaxValue()
    {
        return comboMaxValue.SelectedIndex switch
        {
            0 => 22,
            1 => 41,
            2 => 61,
            3 => 80,
            4 => 100,
            _ => 22
        };
    }

    public void SetMaxValue(int maxValue)
    {
        switch (maxValue)
        {
            case <= 22:
                comboMaxValue.SelectedIndex = 0;

                return;

            case <= 41:
                comboMaxValue.SelectedIndex = 1;

                return;

            case <= 61:
                comboMaxValue.SelectedIndex = 2;

                return;

            case <= 80:
                comboMaxValue.SelectedIndex = 3;

                return;

            default:
                comboMaxValue.SelectedIndex = 0;
                break;
        }

        if (GetMaxValue() <= _currentAttribute.GetPercentage(_currentAttributeSlot))
            checkSelected.Checked = false;
    }

    public void ApplyLocalization()
    {
        // The row caption is the game's own attribute translation. The combo
        // values stay numeric because they represent percentages.
    }

    public void ApplyTheme()
    {
        var foreColor = ColorScheme.ForeColor;

        checkSelected.ForeColor = foreColor;
        comboMaxValue.BackColor = ColorScheme.InputBackColor;
        comboMaxValue.ForeColor = foreColor;
        lblItemAmount.ForeColor = ColorScheme.BackColor.IsDark()
            ? Color.FromArgb(168, 184, 204)
            : Color.FromArgb(90, 104, 124);
        lblFinished.ForeColor = ColorScheme.BackColor.IsDark()
            ? Color.FromArgb(52, 211, 153)
            : Color.FromArgb(5, 150, 105);
    }

    protected override void OnResize(EventArgs e)
    {
        base.OnResize(e);
        ApplyPanelLayout();
    }

    private void ApplyPanelLayout()
    {
        if (IsDisposed || checkSelected == null || Width < 120)
            return;

        const int pad = 12;
        var right = Width - pad;

        checkSelected.SetBounds(pad, 9, Math.Max(140, Width - 310), 24);
        comboMaxValue.SetBounds(Math.Max(pad + 150, right - 246), 6, 110, 27);
        lblItemAmount.SetBounds(Math.Max(pad + 270, right - 126), 8, 50, 22);
        lblFinished.SetBounds(Math.Max(pad + 320, right - 66), 3, 34, 28);
    }

    #endregion Methods
}