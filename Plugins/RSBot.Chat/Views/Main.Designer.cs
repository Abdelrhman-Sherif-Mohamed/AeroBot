using System.Drawing;
using System.Windows.Forms;

namespace RSBot.Chat.Views
{
    partial class Main
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                UnsubscribeEvents();
                components?.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        private void InitializeComponent()
        {
            tabMain = new SDUI.Controls.TabControl();
            tabAll = new System.Windows.Forms.TabPage();
            txtAll = new System.Windows.Forms.RichTextBox();
            separator6 = new SDUI.Controls.Separator();
            panelAllInput = new System.Windows.Forms.Panel();
            txtSendAll = new SDUI.Controls.TextBox();
            spacerAll = new System.Windows.Forms.Panel();
            btnSendAll = new SDUI.Controls.Button();

            tabPrivate = new System.Windows.Forms.TabPage();
            txtPrivate = new System.Windows.Forms.RichTextBox();
            separator5 = new SDUI.Controls.Separator();
            panel1 = new System.Windows.Forms.Panel();
            textBoxPrivateMsg = new SDUI.Controls.TextBox();
            txtRecievePrivate = new SDUI.Controls.TextBox();
            spacerPrivateLeft = new System.Windows.Forms.Panel();
            spacerPrivateRight = new System.Windows.Forms.Panel();
            btnSendPrivate = new SDUI.Controls.Button();

            tabParty = new System.Windows.Forms.TabPage();
            txtParty = new System.Windows.Forms.RichTextBox();
            separator4 = new SDUI.Controls.Separator();
            panelPartyInput = new System.Windows.Forms.Panel();
            txtSendParty = new SDUI.Controls.TextBox();
            spacerParty = new System.Windows.Forms.Panel();
            btnSendParty = new SDUI.Controls.Button();

            tabGuild = new System.Windows.Forms.TabPage();
            txtGuild = new System.Windows.Forms.RichTextBox();
            separator3 = new SDUI.Controls.Separator();
            panelGuildInput = new System.Windows.Forms.Panel();
            txtSendGuild = new SDUI.Controls.TextBox();
            spacerGuild = new System.Windows.Forms.Panel();
            btnSendGuild = new SDUI.Controls.Button();

            tabUnion = new System.Windows.Forms.TabPage();
            txtUnion = new System.Windows.Forms.RichTextBox();
            separator2 = new SDUI.Controls.Separator();
            panelUnionInput = new System.Windows.Forms.Panel();
            txtSendUnion = new SDUI.Controls.TextBox();
            spacerUnion = new System.Windows.Forms.Panel();
            btnSendUnion = new SDUI.Controls.Button();

            tabAcademy = new System.Windows.Forms.TabPage();
            txtAcademy = new System.Windows.Forms.RichTextBox();
            separator1 = new SDUI.Controls.Separator();
            panelAcademyInput = new System.Windows.Forms.Panel();
            txtSendAcademy = new SDUI.Controls.TextBox();
            spacerAcademy = new System.Windows.Forms.Panel();
            btnSendAcademy = new SDUI.Controls.Button();

            tabGlobal = new System.Windows.Forms.TabPage();
            txtGlobal = new System.Windows.Forms.RichTextBox();
            tabStall = new System.Windows.Forms.TabPage();
            txtStall = new System.Windows.Forms.RichTextBox();
            tabUnique = new System.Windows.Forms.TabPage();
            UniqueText = new System.Windows.Forms.RichTextBox();

            tabMain.SuspendLayout();
            tabAll.SuspendLayout();
            panelAllInput.SuspendLayout();
            tabPrivate.SuspendLayout();
            panel1.SuspendLayout();
            tabParty.SuspendLayout();
            panelPartyInput.SuspendLayout();
            tabGuild.SuspendLayout();
            panelGuildInput.SuspendLayout();
            tabUnion.SuspendLayout();
            panelUnionInput.SuspendLayout();
            tabAcademy.SuspendLayout();
            panelAcademyInput.SuspendLayout();
            tabGlobal.SuspendLayout();
            tabStall.SuspendLayout();
            tabUnique.SuspendLayout();
            SuspendLayout();

            // tabMain
            tabMain.Controls.Add(tabAll);
            tabMain.Controls.Add(tabPrivate);
            tabMain.Controls.Add(tabParty);
            tabMain.Controls.Add(tabGuild);
            tabMain.Controls.Add(tabUnion);
            tabMain.Controls.Add(tabAcademy);
            tabMain.Controls.Add(tabGlobal);
            tabMain.Controls.Add(tabStall);
            tabMain.Controls.Add(tabUnique);
            tabMain.Dock = System.Windows.Forms.DockStyle.Fill;
            tabMain.ItemSize = new System.Drawing.Size(96, 30);
            tabMain.Location = new System.Drawing.Point(0, 0);
            tabMain.Name = "tabMain";
            tabMain.SelectedIndex = 0;
            tabMain.Size = new System.Drawing.Size(754, 477);
            tabMain.TabIndex = 0;
            tabMain.SelectedIndexChanged += OnSelectedTabChanged;

            // tabAll
            ConfigureOutputTextBox(txtAll, "txtAll");
            ConfigureSeparator(separator6, "separator6");
            ConfigureSendBar(panelAllInput, "panelAllInput", txtSendAll, "txtSendAll", "1", spacerAll, btnSendAll, "btnSendAll");
            ConfigureSendTabPage(tabAll, "tabAll", "All", 0, txtAll, separator6, panelAllInput);

            // tabPrivate
            ConfigureOutputTextBox(txtPrivate, "txtPrivate");
            ConfigureSeparator(separator5, "separator5");
            panel1.Dock = System.Windows.Forms.DockStyle.Bottom;
            panel1.Height = 40;
            panel1.Name = "panel1";
            panel1.Padding = new System.Windows.Forms.Padding(4, 4, 4, 4);

            textBoxPrivateMsg.Dock = System.Windows.Forms.DockStyle.Fill;
            textBoxPrivateMsg.MaxLength = 32767;
            textBoxPrivateMsg.MultiLine = false;
            textBoxPrivateMsg.Name = "textBoxPrivateMsg";
            textBoxPrivateMsg.PassFocusShow = false;
            textBoxPrivateMsg.Radius = 4;
            textBoxPrivateMsg.TabIndex = 4;
            textBoxPrivateMsg.Tag = "2";
            textBoxPrivateMsg.TextAlignment = System.Windows.Forms.HorizontalAlignment.Left;
            textBoxPrivateMsg.UseSystemPasswordChar = false;
            textBoxPrivateMsg.KeyDown += MessageInputKeyDown;

            spacerPrivateLeft.Dock = System.Windows.Forms.DockStyle.Left;
            spacerPrivateLeft.Width = 6;
            spacerPrivateLeft.Name = "spacerPrivateLeft";

            txtRecievePrivate.Dock = System.Windows.Forms.DockStyle.Left;
            txtRecievePrivate.MaxLength = 64;
            txtRecievePrivate.MultiLine = false;
            txtRecievePrivate.Name = "txtRecievePrivate";
            txtRecievePrivate.PassFocusShow = false;
            txtRecievePrivate.Radius = 4;
            txtRecievePrivate.Width = 175;
            txtRecievePrivate.TabIndex = 3;
            txtRecievePrivate.TextAlignment = System.Windows.Forms.HorizontalAlignment.Left;
            txtRecievePrivate.UseSystemPasswordChar = false;
            txtRecievePrivate.KeyDown += PrivateTargetKeyDown;

            spacerPrivateRight.Dock = System.Windows.Forms.DockStyle.Right;
            spacerPrivateRight.Width = 6;
            spacerPrivateRight.Name = "spacerPrivateRight";

            ConfigureSendButton(btnSendPrivate, "btnSendPrivate");

            panel1.Controls.Add(textBoxPrivateMsg);
            panel1.Controls.Add(spacerPrivateLeft);
            panel1.Controls.Add(txtRecievePrivate);
            panel1.Controls.Add(spacerPrivateRight);
            panel1.Controls.Add(btnSendPrivate);
            textBoxPrivateMsg.BringToFront();

            ConfigureSendTabPage(tabPrivate, "tabPrivate", "Private", 1, txtPrivate, separator5, panel1);

            // tabParty
            ConfigureOutputTextBox(txtParty, "txtParty");
            ConfigureSeparator(separator4, "separator4");
            ConfigureSendBar(panelPartyInput, "panelPartyInput", txtSendParty, "txtSendParty", "4", spacerParty, btnSendParty, "btnSendParty");
            ConfigureSendTabPage(tabParty, "tabParty", "Party", 2, txtParty, separator4, panelPartyInput);

            // tabGuild
            ConfigureOutputTextBox(txtGuild, "txtGuild");
            ConfigureSeparator(separator3, "separator3");
            ConfigureSendBar(panelGuildInput, "panelGuildInput", txtSendGuild, "txtSendGuild", "5", spacerGuild, btnSendGuild, "btnSendGuild");
            ConfigureSendTabPage(tabGuild, "tabGuild", "Guild", 3, txtGuild, separator3, panelGuildInput);

            // tabUnion
            ConfigureOutputTextBox(txtUnion, "txtUnion");
            ConfigureSeparator(separator2, "separator2");
            ConfigureSendBar(panelUnionInput, "panelUnionInput", txtSendUnion, "txtSendUnion", "11", spacerUnion, btnSendUnion, "btnSendUnion");
            ConfigureSendTabPage(tabUnion, "tabUnion", "Union", 4, txtUnion, separator2, panelUnionInput);

            // tabAcademy
            ConfigureOutputTextBox(txtAcademy, "txtAcademy");
            ConfigureSeparator(separator1, "separator1");
            ConfigureSendBar(panelAcademyInput, "panelAcademyInput", txtSendAcademy, "txtSendAcademy", "16", spacerAcademy, btnSendAcademy, "btnSendAcademy");
            ConfigureSendTabPage(tabAcademy, "tabAcademy", "Academy", 5, txtAcademy, separator1, panelAcademyInput);

            // tabGlobal
            ConfigureOutputTextBox(txtGlobal, "txtGlobal");
            ConfigureReadOnlyTabPage(tabGlobal, "tabGlobal", "Global / Notice", 6, txtGlobal);

            // tabStall
            ConfigureOutputTextBox(txtStall, "txtStall");
            ConfigureReadOnlyTabPage(tabStall, "tabStall", "Stall", 7, txtStall);

            // tabUnique
            ConfigureOutputTextBox(UniqueText, "UniqueText");
            ConfigureReadOnlyTabPage(tabUnique, "tabUnique", "Unique", 8, UniqueText);

            // Main
            AutoScaleDimensions = new SizeF(96F, 96F);
            AutoScaleMode = AutoScaleMode.Dpi;
            Controls.Add(tabMain);
            Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point);
            Name = "Main";
            Size = new Size(754, 477);

            tabMain.ResumeLayout(false);
            tabAll.ResumeLayout(false);
            panelAllInput.ResumeLayout(false);
            tabPrivate.ResumeLayout(false);
            panel1.ResumeLayout(false);
            tabParty.ResumeLayout(false);
            panelPartyInput.ResumeLayout(false);
            tabGuild.ResumeLayout(false);
            panelGuildInput.ResumeLayout(false);
            tabUnion.ResumeLayout(false);
            panelUnionInput.ResumeLayout(false);
            tabAcademy.ResumeLayout(false);
            panelAcademyInput.ResumeLayout(false);
            tabGlobal.ResumeLayout(false);
            tabStall.ResumeLayout(false);
            tabUnique.ResumeLayout(false);
            ResumeLayout(false);
        }

        private static void ConfigureOutputTextBox(System.Windows.Forms.RichTextBox box, string name)
        {
            box.BorderStyle = BorderStyle.None;
            box.DetectUrls = true;
            box.Dock = DockStyle.Fill;
            box.Name = name;
            box.ReadOnly = true;
            box.ScrollBars = RichTextBoxScrollBars.Vertical;
            box.WordWrap = true;
            box.Text = string.Empty;
        }

        private static void ConfigureSeparator(SDUI.Controls.Separator sep, string name)
        {
            sep.Dock = DockStyle.Bottom;
            sep.IsVertical = false;
            sep.Name = name;
            sep.Size = new Size(740, 8);
        }

        private void ConfigureSendButton(SDUI.Controls.Button btn, string name)
        {
            btn.Dock = DockStyle.Right;
            btn.Width = 86;
            btn.Name = name;
            btn.Text = "Send";
            btn.Tag = "PreserveColor";
            btn.Cursor = Cursors.Hand;
            btn.Click += SendButtonClick;
        }

        private void ConfigureSendBar(
            System.Windows.Forms.Panel panel,
            string panelName,
            SDUI.Controls.TextBox textBox,
            string textBoxName,
            string chatTag,
            System.Windows.Forms.Panel spacer,
            SDUI.Controls.Button sendButton,
            string buttonName)
        {
            panel.Dock = DockStyle.Bottom;
            panel.Height = 40;
            panel.Name = panelName;
            panel.Padding = new Padding(4, 4, 4, 4);

            textBox.Dock = DockStyle.Fill;
            textBox.MaxLength = 32767;
            textBox.MultiLine = false;
            textBox.Name = textBoxName;
            textBox.PassFocusShow = false;
            textBox.Radius = 4;
            textBox.Tag = chatTag;
            textBox.TextAlignment = HorizontalAlignment.Left;
            textBox.UseSystemPasswordChar = false;
            textBox.KeyDown += MessageInputKeyDown;

            spacer.Dock = DockStyle.Right;
            spacer.Width = 6;
            spacer.Name = panelName + "_spacer";

            ConfigureSendButton(sendButton, buttonName);

            panel.Controls.Add(textBox);
            panel.Controls.Add(spacer);
            panel.Controls.Add(sendButton);
            textBox.BringToFront();
        }

        private static void ConfigureSendTabPage(
            TabPage page,
            string name,
            string text,
            int tabIndex,
            System.Windows.Forms.RichTextBox output,
            SDUI.Controls.Separator sep,
            System.Windows.Forms.Panel inputPanel)
        {
            page.Name = name;
            page.Padding = new Padding(6);
            page.TabIndex = tabIndex;
            page.Text = text;
            page.Controls.Add(output);
            page.Controls.Add(sep);
            page.Controls.Add(inputPanel);
            output.BringToFront();
        }

        private static void ConfigureReadOnlyTabPage(
            TabPage page,
            string name,
            string text,
            int tabIndex,
            System.Windows.Forms.RichTextBox output)
        {
            page.Name = name;
            page.Padding = new Padding(6);
            page.TabIndex = tabIndex;
            page.Text = text;
            page.Controls.Add(output);
        }

        #endregion

        private SDUI.Controls.TabControl tabMain;
        private System.Windows.Forms.TabPage tabAll;
        private System.Windows.Forms.TabPage tabPrivate;
        private System.Windows.Forms.TabPage tabParty;
        private System.Windows.Forms.TabPage tabGuild;
        private System.Windows.Forms.TabPage tabAcademy;
        private System.Windows.Forms.TabPage tabGlobal;
        private System.Windows.Forms.TabPage tabStall;
        private SDUI.Controls.TextBox txtSendAll;
        private System.Windows.Forms.RichTextBox txtAll;
        private SDUI.Controls.TextBox txtSendParty;
        private System.Windows.Forms.RichTextBox txtParty;
        private SDUI.Controls.TextBox txtSendGuild;
        private System.Windows.Forms.RichTextBox txtGuild;
        private SDUI.Controls.TextBox txtSendAcademy;
        private System.Windows.Forms.RichTextBox txtAcademy;
        private System.Windows.Forms.RichTextBox txtGlobal;
        private System.Windows.Forms.RichTextBox txtStall;
        private SDUI.Controls.TextBox txtRecievePrivate;
        private System.Windows.Forms.TabPage tabUnion;
        private SDUI.Controls.TextBox txtSendUnion;
        private System.Windows.Forms.RichTextBox txtUnion;
        private System.Windows.Forms.TabPage tabUnique;
        internal System.Windows.Forms.RichTextBox UniqueText;
        private SDUI.Controls.Separator separator1;
        private SDUI.Controls.Separator separator2;
        private SDUI.Controls.Separator separator3;
        private SDUI.Controls.Separator separator4;
        private SDUI.Controls.Separator separator5;
        private SDUI.Controls.Separator separator6;
        private System.Windows.Forms.RichTextBox txtPrivate;
        private SDUI.Controls.TextBox textBoxPrivateMsg;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Panel panelAllInput;
        private System.Windows.Forms.Panel spacerAll;
        private SDUI.Controls.Button btnSendAll;
        private System.Windows.Forms.Panel spacerPrivateLeft;
        private System.Windows.Forms.Panel spacerPrivateRight;
        private SDUI.Controls.Button btnSendPrivate;
        private System.Windows.Forms.Panel panelPartyInput;
        private System.Windows.Forms.Panel spacerParty;
        private SDUI.Controls.Button btnSendParty;
        private System.Windows.Forms.Panel panelGuildInput;
        private System.Windows.Forms.Panel spacerGuild;
        private SDUI.Controls.Button btnSendGuild;
        private System.Windows.Forms.Panel panelUnionInput;
        private System.Windows.Forms.Panel spacerUnion;
        private SDUI.Controls.Button btnSendUnion;
        private System.Windows.Forms.Panel panelAcademyInput;
        private System.Windows.Forms.Panel spacerAcademy;
        private SDUI.Controls.Button btnSendAcademy;
    }
}
