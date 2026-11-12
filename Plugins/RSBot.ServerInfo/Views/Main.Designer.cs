using System.Drawing;
using System.Windows.Forms;
using SDUI.Controls;

namespace RSBot.ServerInfo.Views;

partial class Main
{
    private System.ComponentModel.IContainer components = null;
    private System.Windows.Forms.Panel _headerPanel;
    private SDUI.Controls.Label _lblStatus;
    private SDUI.Controls.Button _btnRefresh;
    private SDUI.Controls.ListView lvServerInfo;
    private ColumnHeader colServer;
    private ColumnHeader colCapacity;
    private ColumnHeader colStatus;

    /// <summary>
    ///     Clean up any resources being used.
    /// </summary>
    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            UnsubscribeEvents();
            components?.Dispose();
        }
        base.Dispose(disposing);
    }

    private void InitializeComponent()
    {
        _headerPanel = new System.Windows.Forms.Panel();
        _lblStatus = new SDUI.Controls.Label();
        _btnRefresh = new SDUI.Controls.Button();
        lvServerInfo = new SDUI.Controls.ListView();
        colServer = new ColumnHeader();
        colCapacity = new ColumnHeader();
        colStatus = new ColumnHeader();
        _headerPanel.SuspendLayout();
        SuspendLayout();

        // _headerPanel
        _headerPanel.Dock = DockStyle.Top;
        _headerPanel.Height = 46;
        _headerPanel.Padding = new Padding(12, 8, 12, 8);
        _headerPanel.Controls.Add(_lblStatus);
        _headerPanel.Controls.Add(_btnRefresh);

        // _lblStatus
        _lblStatus.Dock = DockStyle.Fill;
        _lblStatus.TextAlign = ContentAlignment.MiddleLeft;
        _lblStatus.AutoEllipsis = true;
        _lblStatus.Text = "Server Status";

        // _btnRefresh
        _btnRefresh.Dock = DockStyle.Right;
        _btnRefresh.Width = 110;
        _btnRefresh.Text = "Refresh";
        _btnRefresh.Tag = "PreserveColor";
        _btnRefresh.Cursor = Cursors.Hand;
        _btnRefresh.Click += OnRefreshClicked;

        // lvServerInfo
        lvServerInfo.BorderStyle = BorderStyle.None;
        lvServerInfo.Dock = DockStyle.Fill;
        lvServerInfo.FullRowSelect = true;
        lvServerInfo.GridLines = false;
        lvServerInfo.HeaderStyle = ColumnHeaderStyle.Nonclickable;
        lvServerInfo.HideSelection = false;
        lvServerInfo.Location = new Point(0, 46);
        lvServerInfo.Name = "lvServerInfo";
        lvServerInfo.Size = new Size(520, 320);
        lvServerInfo.TabIndex = 0;
        lvServerInfo.UseCompatibleStateImageBehavior = false;
        lvServerInfo.View = System.Windows.Forms.View.Details;
        lvServerInfo.Columns.AddRange(new[]
        {
            colServer,
            colCapacity,
            colStatus
        });

        colServer.Text = "Server";
        colServer.Width = 220;
        colCapacity.Text = "Capacity";
        colCapacity.Width = 150;
        colStatus.Text = "Status";
        colStatus.Width = 140;

        // Main
        AutoScaleDimensions = new SizeF(7F, 15F);
        AutoScaleMode = AutoScaleMode.Font;
        Controls.Add(lvServerInfo);
        Controls.Add(_headerPanel);
        Name = "Main";
        Size = new Size(520, 366);

        _headerPanel.ResumeLayout(false);
        ResumeLayout(false);
    }
}
