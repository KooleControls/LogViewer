using KC.InternalApiClient;
using LogViewer.Config.Models;

namespace LogViewer.Controls.Helpers
{
    public class ApiClientProvider
    {
        private Func<string, string, string?> passwordProvider;

        public ApiClientProvider()
        {
            passwordProvider = (prompt, title) => "";
        }

        public void SetPasswordProvider(Func<string, string, string?> provider)
        {
            passwordProvider = provider ?? throw new ArgumentNullException(nameof(provider));
        }


        public async Task<InternalApiClient?> CreateAuthenticatedClientAsync(OrganisationConfig organisation, CancellationToken token)
        {
            var client = CreateClientInternal(organisation);
            if (client == null)
            {
                MessageBox.Show("Unsupported authentication method", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return null;
            }

            try
            {
                bool isValid = await client.AuthApi.CheckSession(token);
                if (!isValid)
                {
                    MessageBox.Show("Authentication failed", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return null;
                }

                return client;
            }
            catch (OperationCanceledException)
            {
                // Optional: handle cancellation separately if you want
                return null;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Authentication error: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return null;
            }
        }

        private InternalApiClient? CreateClientInternal(OrganisationConfig org)
        {
            return org.AuthenticationMethod switch
            {
                AuthenticationMethods.GetOAuth2_OpenIdConnectClient =>
                    InternalApiClient.GetOAuth2OpenIdConnectClient(org.BasePath, org.AuthPath, org.ClientId),

                AuthenticationMethods.GetOAuth2_ApplicationFlowClient =>
                    InternalApiClient.GetOAuth2ApplicationFlowClient(
                        org.BasePath,
                        org.ClientId,
                        GetClientSecret(org)),

                _ => null
            };
        }

        private string? GetClientSecret(OrganisationConfig organisation)
        {
            return passwordProvider($"Enter the apiclient password for {organisation.Name}:", "Login");
        }
    }
}
