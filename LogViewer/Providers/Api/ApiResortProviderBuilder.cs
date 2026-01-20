using KC.InternalApiClient;
using System.Threading;

namespace LogViewer.Providers.API
{
    public class ApiResortProviderBuilder
    {
        private readonly InternalApiClient client;
        private readonly ApiResortProvider apiResortProvider;

        public ApiResortProviderBuilder(InternalApiClient client)
        {
            this.client = client;
            apiResortProvider = new ApiResortProvider(client);
        }

        public ApiResortProviderBuilder ForOrganization(int organizationId)
        {
            apiResortProvider.Filters.Add($"Organization.Id::{organizationId}");
            return this;
        }

        public ApiResortProviderBuilder WithSortByName()
        {
            apiResortProvider.Sort.Clear();
            apiResortProvider.Sort.Add("Name");
            return this;
        }

        public ApiResortProvider Build()
        {
            return apiResortProvider;
        }
    }
}
