using KC.InternalApiClient;
using LogViewer.Config.Models;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.DependencyInjection;

namespace LogViewer.Controls.Helpers
{
    public class ApiClientProvider
    {
        private readonly HybridCache hybridCache;
        private readonly IClientCredentialsSource clientCredentialsSource;

        public ApiClientProvider(IClientCredentialsSource clientCredentialsSource)
        {
            hybridCache = Program.ServiceProvider.GetRequiredService<HybridCache>();
            this.clientCredentialsSource = clientCredentialsSource;
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
                AuthenticationMethods.GetOAuth2_OpenIdConnectClient => CreateClient_OpenIdConnectClient(org),
                AuthenticationMethods.GetOAuth2_ApplicationFlowClient => CreateClient_ApplicationFlowClient(org),
            };
        }


        private InternalApiClient? CreateClient_OpenIdConnectClient(OrganisationConfig org)
        {
            return InternalApiClient.GetOAuth2OpenIdConnectClient(org.BasePath, org.AuthPath, org.ClientId, hybridCache);
        }

        private InternalApiClient? CreateClient_ApplicationFlowClient(OrganisationConfig org)
        {
            var credentials = clientCredentialsSource.GetClientCredentials(org);
            if (credentials == null)
                return null;

            return InternalApiClient.GetOAuth2ApplicationFlowClient(org.BasePath, credentials.ClientId, credentials.ClientSecret, hybridCache);
        }
    }
}



