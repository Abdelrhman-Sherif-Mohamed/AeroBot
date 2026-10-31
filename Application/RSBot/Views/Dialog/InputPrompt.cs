using System;
using System.Drawing;
using System.Windows.Forms;

namespace RSBot.Views;

public static class InputPrompt
{
    public static string Show(string prompt, string title, string defaultValue = "")
    {
        var form = new Form
        {
            Width = 420,
            Height = 180,
            FormBorderStyle = FormBorderStyle.FixedDialog,
            Text = title,
            StartPosition = FormStartPosition.CenterParent,
            MaximizeBox = false,
            MinimizeBox = false,
            BackColor = Color.FromArgb(32, 33, 36),
            ForeColor = Color.White
        };

        var lblPrompt = new Label
        {
            Left = 20,
            Top = 15,
            Width = 360,
            Height = 35,
            Text = prompt,
            Font = new Font("Segoe UI", 9.5F, FontStyle.Regular),
            ForeColor = Color.Gainsboro
        };

        var txtInput = new TextBox
        {
            Left = 20,
            Top = 55,
            Width = 360,
            Text = defaultValue,
            Font = new Font("Segoe UI", 10F, FontStyle.Regular),
            BackColor = Color.FromArgb(48, 50, 54),
            ForeColor = Color.White,
            BorderStyle = BorderStyle.FixedSingle
        };

        var btnOk = new Button
        {
            Text = "موافق (OK)",
            Left = 200,
            Width = 85,
            Top = 95,
            Height = 30,
            DialogResult = DialogResult.OK,
            FlatStyle = FlatStyle.Flat,
            BackColor = Color.FromArgb(0, 122, 204),
            ForeColor = Color.White
        };

        var btnCancel = new Button
        {
            Text = "إلغاء",
            Left = 295,
            Width = 85,
            Top = 95,
            Height = 30,
            DialogResult = DialogResult.Cancel,
            FlatStyle = FlatStyle.Flat,
            BackColor = Color.FromArgb(60, 64, 67),
            ForeColor = Color.Gainsboro
        };

        form.Controls.Add(lblPrompt);
        form.Controls.Add(txtInput);
        form.Controls.Add(btnOk);
        form.Controls.Add(btnCancel);
        form.AcceptButton = btnOk;
        form.CancelButton = btnCancel;

        return form.ShowDialog() == DialogResult.OK ? txtInput.Text.Trim() : null;
    }

        public static (string Username, string Password, string Notes, bool IsAdmin)? ShowNewAccountDialog()
    {
        var form = new Form
        {
            Width = 430,
            Height = 310,
            FormBorderStyle = FormBorderStyle.FixedDialog,
            Text = "إضافة حساب جديد وتحديد الصلاحيات",
            StartPosition = FormStartPosition.CenterParent,
            MaximizeBox = false,
            MinimizeBox = false,
            BackColor = Color.FromArgb(32, 33, 36),
            ForeColor = Color.White
        };

        var lblUser = new Label { Left = 20, Top = 15, Width = 150, Text = "اسم المستخدم:", ForeColor = Color.Gainsboro };
        var txtUser = new TextBox { Left = 20, Top = 35, Width = 370, Font = new Font("Segoe UI", 9.5F), BackColor = Color.FromArgb(48, 50, 54), ForeColor = Color.White, BorderStyle = BorderStyle.FixedSingle };

        var lblPass = new Label { Left = 20, Top = 70, Width = 150, Text = "كلمة المرور:", ForeColor = Color.Gainsboro };
        var txtPass = new TextBox { Left = 20, Top = 90, Width = 370, Font = new Font("Segoe UI", 9.5F), BackColor = Color.FromArgb(48, 50, 54), ForeColor = Color.White, BorderStyle = BorderStyle.FixedSingle };

        var lblNotes = new Label { Left = 20, Top = 125, Width = 250, Text = "ملاحظات (اختياري - اسم العميل):", ForeColor = Color.Gainsboro };
        var txtNotes = new TextBox { Left = 20, Top = 145, Width = 370, Font = new Font("Segoe UI", 9.5F), BackColor = Color.FromArgb(48, 50, 54), ForeColor = Color.White, BorderStyle = BorderStyle.FixedSingle };

        var chkIsAdmin = new CheckBox
        {
            Left = 20,
            Top = 185,
            Width = 370,
            Height = 25,
            Text = "⭐ منح صلاحيات مسؤول كاملة (Admin)",
            ForeColor = Color.Gold,
            Checked = false,
            Font = new Font("Segoe UI", 9.5F, FontStyle.Bold)
        };

        var btnOk = new Button { Text = "حفظ وإضافة", Left = 195, Width = 100, Top = 225, Height = 32, DialogResult = DialogResult.OK, FlatStyle = FlatStyle.Flat, BackColor = Color.FromArgb(46, 125, 50), ForeColor = Color.White };
        var btnCancel = new Button { Text = "إلغاء", Left = 305, Width = 85, Top = 225, Height = 32, DialogResult = DialogResult.Cancel, FlatStyle = FlatStyle.Flat, BackColor = Color.FromArgb(60, 64, 67), ForeColor = Color.Gainsboro };

        form.Controls.AddRange(new Control[] { lblUser, txtUser, lblPass, txtPass, lblNotes, txtNotes, chkIsAdmin, btnOk, btnCancel });
        form.AcceptButton = btnOk;
        form.CancelButton = btnCancel;

        if (form.ShowDialog() == DialogResult.OK)
        {
            if (string.IsNullOrWhiteSpace(txtUser.Text) || string.IsNullOrWhiteSpace(txtPass.Text))
                return null;
            return (txtUser.Text.Trim(), txtPass.Text.Trim(), txtNotes.Text.Trim(), chkIsAdmin.Checked);
        }

        return null;
    }

    /// <summary>
    /// Prompts the user to specify a duration in Days or Months.
    /// </summary>
    public static (bool isMonths, int count)? ShowCustomDurationDialog(string username)
    {
        var form = new Form
        {
            Width = 440,
            Height = 230,
            FormBorderStyle = FormBorderStyle.FixedDialog,
            Text = $"إضافة مدة اشتراك للحساب [{username}]",
            StartPosition = FormStartPosition.CenterParent,
            MaximizeBox = false,
            MinimizeBox = false,
            BackColor = Color.FromArgb(32, 33, 36),
            ForeColor = Color.White
        };

        var lblPrompt = new Label
        {
            Left = 20,
            Top = 15,
            Width = 380,
            Height = 25,
            Text = "اختر وحدة الوقت والمدة المراد إضافتها للاشتراك:",
            Font = new Font("Segoe UI", 9.5F, FontStyle.Bold),
            ForeColor = Color.Gold
        };

        var rbDays = new RadioButton
        {
            Left = 40,
            Top = 45,
            Width = 130,
            Text = "بالأيام (Days)",
            Checked = true,
            Font = new Font("Segoe UI", 9.5F, FontStyle.Regular),
            ForeColor = Color.White
        };

        var rbMonths = new RadioButton
        {
            Left = 210,
            Top = 45,
            Width = 140,
            Text = "بالشهور (Months)",
            Font = new Font("Segoe UI", 9.5F, FontStyle.Regular),
            ForeColor = Color.White
        };

        var lblCount = new Label
        {
            Left = 20,
            Top = 85,
            Width = 110,
            Text = "العدد / المدة:",
            Font = new Font("Segoe UI", 9.5F),
            ForeColor = Color.Gainsboro
        };

        var numValue = new NumericUpDown
        {
            Left = 140,
            Top = 82,
            Width = 120,
            Minimum = 1,
            Maximum = 3650,
            Value = 30,
            Font = new Font("Segoe UI", 10F, FontStyle.Bold),
            BackColor = Color.FromArgb(48, 50, 54),
            ForeColor = Color.White
        };

        rbDays.CheckedChanged += (s, e) =>
        {
            if (rbDays.Checked)
            {
                numValue.Maximum = 3650;
                if (numValue.Value > 365) numValue.Value = 30;
            }
        };

        rbMonths.CheckedChanged += (s, e) =>
        {
            if (rbMonths.Checked)
            {
                numValue.Maximum = 120;
                if (numValue.Value > 120) numValue.Value = 1;
            }
        };

        var btnOk = new Button
        {
            Text = "تمديد الاشتراك ➕",
            Left = 180,
            Width = 135,
            Top = 135,
            Height = 32,
            DialogResult = DialogResult.OK,
            FlatStyle = FlatStyle.Flat,
            BackColor = Color.FromArgb(0, 150, 136),
            ForeColor = Color.White,
            Font = new Font("Segoe UI", 9F, FontStyle.Bold)
        };

        var btnCancel = new Button
        {
            Text = "إلغاء",
            Left = 325,
            Width = 80,
            Top = 135,
            Height = 32,
            DialogResult = DialogResult.Cancel,
            FlatStyle = FlatStyle.Flat,
            BackColor = Color.FromArgb(60, 64, 67),
            ForeColor = Color.Gainsboro
        };

        form.Controls.AddRange(new Control[] { lblPrompt, rbDays, rbMonths, lblCount, numValue, btnOk, btnCancel });
        form.AcceptButton = btnOk;
        form.CancelButton = btnCancel;

        if (form.ShowDialog() == DialogResult.OK)
        {
            return (rbMonths.Checked, (int)numValue.Value);
        }

        return null;
    }

    /// <summary>
    /// Prompts the user to pick an exact expiration date & time.
    /// </summary>
    public static DateTime? ShowDatePickerPrompt(string prompt, string title, DateTime defaultDate)
    {
        var form = new Form
        {
            Width = 420,
            Height = 200,
            FormBorderStyle = FormBorderStyle.FixedDialog,
            Text = title,
            StartPosition = FormStartPosition.CenterParent,
            MaximizeBox = false,
            MinimizeBox = false,
            BackColor = Color.FromArgb(32, 33, 36),
            ForeColor = Color.White
        };

        var lblPrompt = new Label
        {
            Left = 20,
            Top = 15,
            Width = 360,
            Height = 30,
            Text = prompt,
            Font = new Font("Segoe UI", 9.5F, FontStyle.Regular),
            ForeColor = Color.Gainsboro
        };

        var dtp = new DateTimePicker
        {
            Left = 20,
            Top = 55,
            Width = 360,
            Format = DateTimePickerFormat.Custom,
            CustomFormat = "yyyy-MM-dd  HH:mm",
            Value = defaultDate > DateTime.Now ? defaultDate : DateTime.Now.AddDays(30),
            Font = new Font("Segoe UI", 10F, FontStyle.Bold),
            BackColor = Color.FromArgb(48, 50, 54),
            ForeColor = Color.White
        };

        var btnOk = new Button
        {
            Text = "تطبيق التاريخ 💾",
            Left = 190,
            Width = 110,
            Top = 110,
            Height = 32,
            DialogResult = DialogResult.OK,
            FlatStyle = FlatStyle.Flat,
            BackColor = Color.FromArgb(0, 122, 204),
            ForeColor = Color.White,
            Font = new Font("Segoe UI", 9F, FontStyle.Bold)
        };

        var btnCancel = new Button
        {
            Text = "إلغاء",
            Left = 310,
            Width = 70,
            Top = 110,
            Height = 32,
            DialogResult = DialogResult.Cancel,
            FlatStyle = FlatStyle.Flat,
            BackColor = Color.FromArgb(60, 64, 67),
            ForeColor = Color.Gainsboro
        };

        form.Controls.AddRange(new Control[] { lblPrompt, dtp, btnOk, btnCancel });
        form.AcceptButton = btnOk;
        form.CancelButton = btnCancel;

        if (form.ShowDialog() == DialogResult.OK)
        {
            return dtp.Value;
        }

        return null;
    }
}
