namespace RSBot.Stall.Views
{
    partial class Main
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Component Designer generated code

        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();
            groupStallInfo = new SDUI.Controls.GroupBox();
            lblTitle = new SDUI.Controls.Label();
            txtTitle = new SDUI.Controls.TextBox();
            lblNote = new SDUI.Controls.Label();
            txtNote = new SDUI.Controls.TextBox();
            lblStatus = new SDUI.Controls.Label();
            btnCreateStall = new SDUI.Controls.Button();
            btnToggleState = new SDUI.Controls.Button();
            btnCloseStall = new SDUI.Controls.Button();
            btnUpdateTitleNote = new SDUI.Controls.Button();

            groupInventory = new SDUI.Controls.GroupBox();
            listViewInventory = new SDUI.Controls.ListView();
            colInvName = new System.Windows.Forms.ColumnHeader();
            colInvAmount = new System.Windows.Forms.ColumnHeader();
            colInvSlot = new System.Windows.Forms.ColumnHeader();
            lblPrice = new SDUI.Controls.Label();
            txtPrice = new SDUI.Controls.TextBox();
            lblQuantity = new SDUI.Controls.Label();
            numQuantity = new SDUI.Controls.NumUpDown();
            btnAddToStall = new SDUI.Controls.Button();
            btnRefreshInventory = new SDUI.Controls.Button();

            groupStall = new SDUI.Controls.GroupBox();
            listViewStall = new SDUI.Controls.ListView();
            colSlot = new System.Windows.Forms.ColumnHeader();
            colStallItem = new System.Windows.Forms.ColumnHeader();
            colStallQty = new System.Windows.Forms.ColumnHeader();
            colStallPrice = new System.Windows.Forms.ColumnHeader();
            contextMenuStall = new SDUI.Controls.ContextMenuStrip();
            menuEditPrice = new System.Windows.Forms.ToolStripMenuItem();
            menuRemoveItem = new System.Windows.Forms.ToolStripMenuItem();
            lblLog = new SDUI.Controls.Label();
            btnClearLog = new SDUI.Controls.Button();
            txtLog = new System.Windows.Forms.RichTextBox();

            groupStallInfo.SuspendLayout();
            groupInventory.SuspendLayout();
            groupStall.SuspendLayout();
            contextMenuStall.SuspendLayout();
            SuspendLayout();

            // 
            // groupStallInfo
            // 
            groupStallInfo.Controls.Add(lblTitle);
            groupStallInfo.Controls.Add(txtTitle);
            groupStallInfo.Controls.Add(lblNote);
            groupStallInfo.Controls.Add(txtNote);
            groupStallInfo.Controls.Add(lblStatus);
            groupStallInfo.Controls.Add(btnCreateStall);
            groupStallInfo.Controls.Add(btnToggleState);
            groupStallInfo.Controls.Add(btnCloseStall);
            groupStallInfo.Controls.Add(btnUpdateTitleNote);
            groupStallInfo.Font = new System.Drawing.Font("Segoe UI Semibold", 9.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            groupStallInfo.Location = new System.Drawing.Point(8, 6);
            groupStallInfo.Name = "groupStallInfo";
            groupStallInfo.Size = new System.Drawing.Size(952, 88);
            groupStallInfo.TabIndex = 0;
            groupStallInfo.TabStop = false;
            groupStallInfo.Text = "Stall Settings";
            // 
            // lblTitle
            // 
            lblTitle.AutoSize = true;
            lblTitle.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            lblTitle.Location = new System.Drawing.Point(10, 23);
            lblTitle.Name = "lblTitle";
            lblTitle.Size = new System.Drawing.Size(50, 20);
            lblTitle.TabIndex = 0;
            lblTitle.Text = "Title:";
            // 
            // txtTitle
            // 
            txtTitle.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            txtTitle.Location = new System.Drawing.Point(62, 20);
            txtTitle.Name = "txtTitle";
            txtTitle.Size = new System.Drawing.Size(240, 24);
            txtTitle.TabIndex = 1;
            txtTitle.Text = "SonicBot Shop";
            // 
            // lblNote
            // 
            lblNote.AutoSize = true;
            lblNote.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            lblNote.Location = new System.Drawing.Point(315, 23);
            lblNote.Name = "lblNote";
            lblNote.Size = new System.Drawing.Size(65, 20);
            lblNote.TabIndex = 2;
            lblNote.Text = "Note:";
            // 
            // txtNote
            // 
            txtNote.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            txtNote.Location = new System.Drawing.Point(382, 20);
            txtNote.Name = "txtNote";
            txtNote.Size = new System.Drawing.Size(240, 24);
            txtNote.TabIndex = 3;
            txtNote.Text = "Welcome!";
            // 
            // lblStatus
            // 
            lblStatus.AutoSize = false;
            lblStatus.Font = new System.Drawing.Font("Segoe UI Semibold", 9.5F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            lblStatus.ForeColor = System.Drawing.Color.Gray;
            lblStatus.Location = new System.Drawing.Point(635, 21);
            lblStatus.Name = "lblStatus";
            lblStatus.Size = new System.Drawing.Size(305, 22);
            lblStatus.TabIndex = 4;
            lblStatus.Text = "Status: Not Created";
            // 
            // btnCreateStall
            // 
            btnCreateStall.Color = System.Drawing.Color.Empty;
            btnCreateStall.Font = new System.Drawing.Font("Segoe UI Semibold", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            btnCreateStall.Location = new System.Drawing.Point(10, 52);
            btnCreateStall.Name = "btnCreateStall";
            btnCreateStall.Size = new System.Drawing.Size(140, 28);
            btnCreateStall.TabIndex = 5;
            btnCreateStall.Text = "Create Stall";
            btnCreateStall.Click += btnCreateStall_Click;
            // 
            // btnToggleState
            // 
            btnToggleState.Color = System.Drawing.Color.Empty;
            btnToggleState.Enabled = false;
            btnToggleState.Font = new System.Drawing.Font("Segoe UI Semibold", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            btnToggleState.Location = new System.Drawing.Point(160, 52);
            btnToggleState.Name = "btnToggleState";
            btnToggleState.Size = new System.Drawing.Size(140, 28);
            btnToggleState.TabIndex = 6;
            btnToggleState.Text = "Open Stall";
            btnToggleState.Click += btnToggleState_Click;
            // 
            // btnCloseStall
            // 
            btnCloseStall.Color = System.Drawing.Color.Empty;
            btnCloseStall.Enabled = false;
            btnCloseStall.Font = new System.Drawing.Font("Segoe UI Semibold", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            btnCloseStall.Location = new System.Drawing.Point(310, 52);
            btnCloseStall.Name = "btnCloseStall";
            btnCloseStall.Size = new System.Drawing.Size(140, 28);
            btnCloseStall.TabIndex = 7;
            btnCloseStall.Text = "Close Stall";
            btnCloseStall.Click += btnCloseStall_Click;
            // 
            // btnUpdateTitleNote
            // 
            btnUpdateTitleNote.Color = System.Drawing.Color.Empty;
            btnUpdateTitleNote.Enabled = false;
            btnUpdateTitleNote.Font = new System.Drawing.Font("Segoe UI Semibold", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            btnUpdateTitleNote.Location = new System.Drawing.Point(460, 52);
            btnUpdateTitleNote.Name = "btnUpdateTitleNote";
            btnUpdateTitleNote.Size = new System.Drawing.Size(150, 28);
            btnUpdateTitleNote.TabIndex = 8;
            btnUpdateTitleNote.Text = "Update Info";
            btnUpdateTitleNote.Click += btnUpdateTitleNote_Click;
            // 
            // groupInventory
            // 
            groupInventory.Controls.Add(listViewInventory);
            groupInventory.Controls.Add(lblPrice);
            groupInventory.Controls.Add(txtPrice);
            groupInventory.Controls.Add(lblQuantity);
            groupInventory.Controls.Add(numQuantity);
            groupInventory.Controls.Add(btnAddToStall);
            groupInventory.Controls.Add(btnRefreshInventory);
            groupInventory.Font = new System.Drawing.Font("Segoe UI Semibold", 9.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            groupInventory.Location = new System.Drawing.Point(8, 100);
            groupInventory.Name = "groupInventory";
            groupInventory.Size = new System.Drawing.Size(466, 400);
            groupInventory.TabIndex = 1;
            groupInventory.TabStop = false;
            groupInventory.Text = "Your Inventory";
            // 
            // listViewInventory
            // 
            listViewInventory.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] { colInvName, colInvAmount, colInvSlot });
            listViewInventory.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            listViewInventory.FullRowSelect = true;
            listViewInventory.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
            listViewInventory.Location = new System.Drawing.Point(10, 24);
            listViewInventory.MultiSelect = false;
            listViewInventory.Name = "listViewInventory";
            listViewInventory.Size = new System.Drawing.Size(446, 275);
            listViewInventory.TabIndex = 0;
            listViewInventory.UseCompatibleStateImageBehavior = false;
            listViewInventory.View = System.Windows.Forms.View.Details;
            listViewInventory.SelectedIndexChanged += listViewInventory_SelectedIndexChanged;
            // 
            // colInvName
            // 
            colInvName.Text = "Item";
            colInvName.Width = 266;
            // 
            // colInvAmount
            // 
            colInvAmount.Text = "Qty";
            colInvAmount.Width = 95;
            // 
            // colInvSlot
            // 
            colInvSlot.Text = "Slot";
            colInvSlot.Width = 80;
            // 
            // lblPrice
            // 
            lblPrice.AutoSize = false;
            lblPrice.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            lblPrice.Location = new System.Drawing.Point(10, 310);
            lblPrice.Name = "lblPrice";
            lblPrice.Size = new System.Drawing.Size(85, 22);
            lblPrice.TabIndex = 1;
            lblPrice.Text = "Price (Gold):";
            // 
            // txtPrice
            // 
            txtPrice.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            txtPrice.Location = new System.Drawing.Point(98, 307);
            txtPrice.Name = "txtPrice";
            txtPrice.Size = new System.Drawing.Size(155, 24);
            txtPrice.TabIndex = 2;
            txtPrice.Text = "1000";
            // 
            // lblQuantity
            // 
            lblQuantity.AutoSize = false;
            lblQuantity.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            lblQuantity.Location = new System.Drawing.Point(265, 310);
            lblQuantity.Name = "lblQuantity";
            lblQuantity.Size = new System.Drawing.Size(55, 22);
            lblQuantity.TabIndex = 3;
            lblQuantity.Text = "Qty:";
            // 
            // numQuantity
            // 
            numQuantity.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            numQuantity.Location = new System.Drawing.Point(325, 307);
            numQuantity.Maximum = new decimal(new int[] { 9999, 0, 0, 0 });
            numQuantity.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
            numQuantity.Name = "numQuantity";
            numQuantity.Size = new System.Drawing.Size(130, 24);
            numQuantity.TabIndex = 4;
            numQuantity.Value = new decimal(new int[] { 1, 0, 0, 0 });
            // 
            // btnAddToStall
            // 
            btnAddToStall.Color = System.Drawing.Color.Empty;
            btnAddToStall.Font = new System.Drawing.Font("Segoe UI Semibold", 9.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            btnAddToStall.Location = new System.Drawing.Point(10, 348);
            btnAddToStall.Name = "btnAddToStall";
            btnAddToStall.Size = new System.Drawing.Size(218, 38);
            btnAddToStall.TabIndex = 5;
            btnAddToStall.Text = "Add to Stall";
            btnAddToStall.Click += btnAddToStall_Click;
            // 
            // btnRefreshInventory
            // 
            btnRefreshInventory.Color = System.Drawing.Color.Empty;
            btnRefreshInventory.Font = new System.Drawing.Font("Segoe UI Semibold", 9.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            btnRefreshInventory.Location = new System.Drawing.Point(238, 348);
            btnRefreshInventory.Name = "btnRefreshInventory";
            btnRefreshInventory.Size = new System.Drawing.Size(218, 38);
            btnRefreshInventory.TabIndex = 6;
            btnRefreshInventory.Text = "Refresh";
            btnRefreshInventory.Click += btnRefreshInventory_Click;
            // 
            // groupStall
            // 
            groupStall.Controls.Add(listViewStall);
            groupStall.Controls.Add(lblLog);
            groupStall.Controls.Add(btnClearLog);
            groupStall.Controls.Add(txtLog);
            groupStall.Font = new System.Drawing.Font("Segoe UI Semibold", 9.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            groupStall.Location = new System.Drawing.Point(484, 100);
            groupStall.Name = "groupStall";
            groupStall.Size = new System.Drawing.Size(476, 400);
            groupStall.TabIndex = 2;
            groupStall.TabStop = false;
            groupStall.Text = "Stall Items (Slots 1-10)";
            // 
            // listViewStall
            // 
            listViewStall.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] { colSlot, colStallItem, colStallQty, colStallPrice });
            listViewStall.ContextMenuStrip = contextMenuStall;
            listViewStall.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            listViewStall.FullRowSelect = true;
            listViewStall.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
            listViewStall.Location = new System.Drawing.Point(10, 24);
            listViewStall.MultiSelect = false;
            listViewStall.Name = "listViewStall";
            listViewStall.Size = new System.Drawing.Size(456, 205);
            listViewStall.TabIndex = 0;
            listViewStall.UseCompatibleStateImageBehavior = false;
            listViewStall.View = System.Windows.Forms.View.Details;
            // 
            // colSlot
            // 
            colSlot.Text = "#";
            colSlot.Width = 40;
            // 
            // colStallItem
            // 
            colStallItem.Text = "Item Name";
            colStallItem.Width = 215;
            // 
            // colStallQty
            // 
            colStallQty.Text = "Qty";
            colStallQty.Width = 75;
            // 
            // colStallPrice
            // 
            colStallPrice.Text = "Price";
            colStallPrice.Width = 120;
            // 
            // contextMenuStall
            // 
            contextMenuStall.Items.AddRange(new System.Windows.Forms.ToolStripItem[] { menuEditPrice, menuRemoveItem });
            contextMenuStall.Name = "contextMenuStall";
            contextMenuStall.Size = new System.Drawing.Size(180, 52);
            // 
            // menuEditPrice
            // 
            menuEditPrice.Name = "menuEditPrice";
            menuEditPrice.Size = new System.Drawing.Size(179, 22);
            menuEditPrice.Text = "Edit Price";
            menuEditPrice.Click += menuEditPrice_Click;
            // 
            // menuRemoveItem
            // 
            menuRemoveItem.Name = "menuRemoveItem";
            menuRemoveItem.Size = new System.Drawing.Size(179, 22);
            menuRemoveItem.Text = "Remove from Stall";
            menuRemoveItem.Click += menuRemoveItem_Click;
            // 
            // lblLog
            // 
            lblLog.AutoSize = false;
            lblLog.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            lblLog.Location = new System.Drawing.Point(10, 240);
            lblLog.Name = "lblLog";
            lblLog.Size = new System.Drawing.Size(340, 22);
            lblLog.TabIndex = 1;
            lblLog.Text = "Stall Activity Log:";
            // 
            // btnClearLog
            // 
            btnClearLog.Color = System.Drawing.Color.Empty;
            btnClearLog.Font = new System.Drawing.Font("Segoe UI", 8.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            btnClearLog.Location = new System.Drawing.Point(366, 237);
            btnClearLog.Name = "btnClearLog";
            btnClearLog.Size = new System.Drawing.Size(100, 26);
            btnClearLog.TabIndex = 2;
            btnClearLog.Text = "Clear";
            btnClearLog.Click += btnClearLog_Click;
            // 
            // txtLog
            // 
            txtLog.BackColor = System.Drawing.Color.FromArgb(28, 28, 30);
            txtLog.BorderStyle = System.Windows.Forms.BorderStyle.None;
            txtLog.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            txtLog.ForeColor = System.Drawing.Color.Gainsboro;
            txtLog.Location = new System.Drawing.Point(10, 268);
            txtLog.Name = "txtLog";
            txtLog.ReadOnly = true;
            txtLog.Size = new System.Drawing.Size(456, 120);
            txtLog.TabIndex = 3;
            txtLog.Text = "";
            // 
            // Main
            // 
            Controls.Add(groupStallInfo);
            Controls.Add(groupInventory);
            Controls.Add(groupStall);
            Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            Name = "Main";
            Size = new System.Drawing.Size(970, 510);

            groupStallInfo.ResumeLayout(false);
            groupStallInfo.PerformLayout();
            groupInventory.ResumeLayout(false);
            groupInventory.PerformLayout();
            groupStall.ResumeLayout(false);
            groupStall.PerformLayout();
            contextMenuStall.ResumeLayout(false);
            ResumeLayout(false);
        }

        #endregion

        private SDUI.Controls.GroupBox groupStallInfo;
        private SDUI.Controls.Label lblTitle;
        private SDUI.Controls.TextBox txtTitle;
        private SDUI.Controls.Label lblNote;
        private SDUI.Controls.TextBox txtNote;
        private SDUI.Controls.Label lblStatus;
        private SDUI.Controls.Button btnCreateStall;
        private SDUI.Controls.Button btnToggleState;
        private SDUI.Controls.Button btnCloseStall;
        private SDUI.Controls.Button btnUpdateTitleNote;

        private SDUI.Controls.GroupBox groupInventory;
        private SDUI.Controls.ListView listViewInventory;
        private System.Windows.Forms.ColumnHeader colInvName;
        private System.Windows.Forms.ColumnHeader colInvAmount;
        private System.Windows.Forms.ColumnHeader colInvSlot;
        private SDUI.Controls.Label lblPrice;
        private SDUI.Controls.TextBox txtPrice;
        private SDUI.Controls.Label lblQuantity;
        private SDUI.Controls.NumUpDown numQuantity;
        private SDUI.Controls.Button btnAddToStall;
        private SDUI.Controls.Button btnRefreshInventory;

        private SDUI.Controls.GroupBox groupStall;
        private SDUI.Controls.ListView listViewStall;
        private System.Windows.Forms.ColumnHeader colSlot;
        private System.Windows.Forms.ColumnHeader colStallItem;
        private System.Windows.Forms.ColumnHeader colStallQty;
        private System.Windows.Forms.ColumnHeader colStallPrice;
        private SDUI.Controls.ContextMenuStrip contextMenuStall;
        private System.Windows.Forms.ToolStripMenuItem menuEditPrice;
        private System.Windows.Forms.ToolStripMenuItem menuRemoveItem;
        private SDUI.Controls.Label lblLog;
        private SDUI.Controls.Button btnClearLog;
        private System.Windows.Forms.RichTextBox txtLog;
    }
}
