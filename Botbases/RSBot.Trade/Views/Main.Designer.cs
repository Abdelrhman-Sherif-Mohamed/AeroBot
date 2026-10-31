namespace RSBot.Trade.Views
{
    partial class Main
    {
        /// <summary>
        ///     Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///     Clean up any resources being used.
        /// </summary>
        protected override void Dispose(bool disposing)
        {
            if (disposing && components != null)
                components.Dispose();

            base.Dispose(disposing);
        }

        /// <summary>
        ///     Required method for designer support.
        ///     The layout is built in <see cref="BuildUi" /> to keep this file
        ///     readable while still allowing the whole page to be themed by SDUI.
        /// </summary>
        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();

            SuspendLayout();
            BuildUi();
            ResumeLayout(false);
        }

        private SDUI.Controls.TabControl tabControlMain;
        private System.Windows.Forms.TabPage tabPageSettings;
        private System.Windows.Forms.TabPage tabPageGoods;
        private System.Windows.Forms.TabPage tabPageRoute;

        private SDUI.Controls.Label lblStartCity;
        private SDUI.Controls.ComboBox comboStartCity;
        private SDUI.Controls.Label lblDestCity;
        private SDUI.Controls.ComboBox comboDestCity;
        private SDUI.Controls.Label lblTransport;
        private SDUI.Controls.ComboBox comboTransport;
        private SDUI.Controls.Button btnAddRoute;
        private SDUI.Controls.Button btnStartTrade;
        private SDUI.Controls.Button btnStopTrade;
        private SDUI.Controls.Label lblRouteList;
        private SDUI.Controls.ComboBox comboRouteList;
        private SDUI.Controls.Button btnNewRouteList;
        private SDUI.Controls.Button btnDeleteRouteList;
        private SDUI.Controls.ListView lvRoutes;
        private System.Windows.Forms.ColumnHeader colIndex;
        private System.Windows.Forms.ColumnHeader colStart;
        private System.Windows.Forms.ColumnHeader colEnd;
        private System.Windows.Forms.ColumnHeader colScript;
        private System.Windows.Forms.ColumnHeader colActive;
        private System.Windows.Forms.ColumnHeader colLoop;
        private SDUI.Controls.Button btnMoveUp;
        private SDUI.Controls.Button btnMoveDown;
        private SDUI.Controls.CheckBox checkDisableAutoRun;
        private System.Windows.Forms.LinkLabel linkGuide;
        private SDUI.Controls.Label lblRouteStatus;
        private SDUI.Controls.GroupBox groupBuySell;
        private SDUI.Controls.CheckBox checkSellGoods;
        private SDUI.Controls.CheckBox checkBuyGoods;
        private SDUI.Controls.NumUpDown numAmountGoods;
        private SDUI.Controls.Label lblGoods;
        private SDUI.Controls.Label lblNumGoodsDesc;
        private SDUI.Controls.ContextMenuStrip contextMenuRoutes;
        private System.Windows.Forms.ToolStripMenuItem menuToggleRoute;
        private System.Windows.Forms.ToolStripMenuItem menuEditLoops;
        private System.Windows.Forms.ToolStripMenuItem menuRemoveRoute;

        private SDUI.Controls.Label lblGoodsStatus;
        private SDUI.Controls.Button btnRefreshGoods;
        private SDUI.Controls.ListView lvGoods;
        private System.Windows.Forms.ColumnHeader colGoodsName;
        private System.Windows.Forms.ColumnHeader colGoodsAmount;
        private System.Windows.Forms.ColumnHeader colGoodsMax;
        private SDUI.Controls.Label lblGoodsHint;

        private SDUI.Controls.GroupBox groupSettings;
        private SDUI.Controls.CheckBox checkAttackThiefNpc;
        private SDUI.Controls.CheckBox checkAttackThiefPlayers;
        private SDUI.Controls.CheckBox checkCounterAttack;
        private SDUI.Controls.CheckBox checkProtectTransport;
        private SDUI.Controls.CheckBox checkMountTransport;
        private SDUI.Controls.CheckBox checkCastBuffs;
        private SDUI.Controls.CheckBox checkWaitForHunter;
        private SDUI.Controls.CheckBox checkAutoStartAfterLogin;
        private SDUI.Controls.Label lblMaxAttackDistance;
        private SDUI.Controls.NumUpDown numMaxAttackDistance;
        private SDUI.Controls.Label lblMaxAttackUnit;
        private SDUI.Controls.Label lblMaxTransportDistance;
        private SDUI.Controls.NumUpDown numMaxTransportDistance;
        private SDUI.Controls.Label lblMaxTransportUnit;

        private SDUI.Controls.GroupBox groupLoopOptions;
        private SDUI.Controls.Label lblTradeLoops;
        private SDUI.Controls.NumUpDown numTradeLoops;
        private SDUI.Controls.Label lblInfiniteLoops;
        private SDUI.Controls.CheckBox checkUseReturnScroll;
        private SDUI.Controls.CheckBox checkSkipTownScript;
        private SDUI.Controls.CheckBox checkSmartThiefCombat;
        private SDUI.Controls.CheckBox checkSkipNpcIfHasGoods;
        private SDUI.Controls.CheckBox checkSmartObstacleEvasion;
        private SDUI.Controls.Label lblRunMode;
        private SDUI.Controls.Radio radioCityToCity;
        private SDUI.Controls.Radio radioRouteList;
        private SDUI.Controls.Radio radioTracePlayer;
        private SDUI.Controls.Label lblTracePlayerName;
        private SDUI.Controls.TextBox txtTracePlayerName;
        private SDUI.Controls.Label lblSettingsHint;

        private System.Windows.Forms.Panel panelLog;
        private System.Windows.Forms.Panel panelLogHeader;
        private System.Windows.Forms.Label lblJobInfo;
        private System.Windows.Forms.Label lblTradeStatus;
        private System.Windows.Forms.Panel panelLogButtons;
        private SDUI.Controls.Button btnStartBot;
        private SDUI.Controls.Button btnStopBot;
        private SDUI.Controls.Button btnShowClient;
        private SDUI.Controls.Button btnClearLogs;
        private System.Windows.Forms.Panel panelLogBody;
        private System.Windows.Forms.RichTextBox rtbLog;

        private System.Windows.Forms.Timer uiTimer;
    }
}
