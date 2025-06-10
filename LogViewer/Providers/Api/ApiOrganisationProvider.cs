using LogViewer.Config;
using LogViewer.Config.Models;
using System.Runtime.CompilerServices;

namespace LogViewer.Providers.API
{
    public class ApiOrganisationProvider : IApiDataProvider<OrganisationConfig>
    {
        private readonly List<OrganisationConfig> _organisationConfigs;
        public ApiOrganisationProvider(List<OrganisationConfig> organisationConfigs)
        {
            _organisationConfigs = organisationConfigs;
        }

        public async IAsyncEnumerable<OrganisationConfig> GetData([EnumeratorCancellation] CancellationToken token, IProgress<double>? progress)
        {
            foreach (var item in _organisationConfigs.OrderBy(a => a.Name))
                yield return item;
        }
    }
}
