using LogViewer.Config.Models;

namespace LogViewer.Controls.Helpers
{

    public interface IClientCredentialsSource
    {
        ClientCredentials? GetClientCredentials(OrganisationConfig organisationConfig);
    }
}
