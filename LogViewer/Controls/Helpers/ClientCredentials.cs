namespace LogViewer.Controls.Helpers
{
    public sealed class ClientCredentials
    {
        public string ClientId { get; }
        public string ClientSecret { get; }

        public ClientCredentials(string clientId, string clientSecret)
        {
            ClientId = clientId;
            ClientSecret = clientSecret;
        }
    }
}
