namespace LogViewer.Controls.Helpers
{
    public static class DialogHelper
    {
        public static string? ShowPasswordPrompt(string prompt, string title)
        {
            var form = new Form
            {
                Width = 300,
                Height = 150,
                FormBorderStyle = FormBorderStyle.FixedDialog,
                Text = title,
                StartPosition = FormStartPosition.CenterScreen,
                MinimizeBox = false,
                MaximizeBox = false
            };

            var label = new Label { Left = 10, Top = 10, Text = prompt, AutoSize = true };
            var inputBox = new TextBox { Left = 10, Top = 35, Width = 260, UseSystemPasswordChar = true };
            var ok = new Button { Text = "OK", Left = 200, Width = 70, Top = 70, DialogResult = DialogResult.OK };

            ok.Click += (sender, e) => form.Close();

            form.Controls.Add(label);
            form.Controls.Add(inputBox);
            form.Controls.Add(ok);
            form.AcceptButton = ok;

            return form.ShowDialog() == DialogResult.OK ? inputBox.Text : null;
        }
    }

}
