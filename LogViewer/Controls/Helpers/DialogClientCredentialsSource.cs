using LogViewer.Config.Models;

namespace LogViewer.Controls.Helpers
{
    public sealed class DialogClientCredentialsSource : IClientCredentialsSource
    {
        public ClientCredentials? GetClientCredentials(OrganisationConfig organisationConfig)
        {
            using var form = new Form
            {
                Width = 320,
                Height = 200,
                FormBorderStyle = FormBorderStyle.FixedDialog,
                Text = $"Enter the apiclient password for {organisationConfig.Name}",
                StartPosition = FormStartPosition.CenterScreen,
                MinimizeBox = false,
                MaximizeBox = false
            };

            var clientIdLabel = new Label
            {
                Left = 10,
                Top = 10,
                Text = "Client ID",
                AutoSize = true
            };

            var clientIdBox = new TextBox
            {
                Left = 10,
                Top = 30,
                Width = 280,
                Text = organisationConfig.ClientId ?? ""
            };

            var secretLabel = new Label
            {
                Left = 10,
                Top = 65,
                Text = "Client Secret",
                AutoSize = true
            };

            var secretBox = new TextBox
            {
                Left = 10,
                Top = 85,
                Width = 280,
                UseSystemPasswordChar = true
            };

            var ok = new Button
            {
                Text = "OK",
                Left = 220,
                Width = 70,
                Top = 125,
                DialogResult = DialogResult.OK
            };

            ok.Click += (_, _) => form.Close();

            form.Controls.Add(clientIdLabel);
            form.Controls.Add(clientIdBox);
            form.Controls.Add(secretLabel);
            form.Controls.Add(secretBox);
            form.Controls.Add(ok);

            form.AcceptButton = ok;

            return form.ShowDialog() == DialogResult.OK
                ? new ClientCredentials(clientIdBox.Text, secretBox.Text)
                : null;
        }
    }
}
