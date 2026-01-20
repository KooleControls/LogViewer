using KC.InternalApiClient;
using Octokit;

namespace LogViewer.Providers.API
{
    public class ApiObjectItemProviderBuilder
    {
        private readonly InternalApiClient client;
        private readonly ApiObjectItemProvider provider;

        public ApiObjectItemProviderBuilder(InternalApiClient client)
        {
            this.client = client;
            provider = new ApiObjectItemProvider(client);
        }

        public ApiObjectItemProviderBuilder ForResort(int resortId)
        {
            provider.Filters.Add($"Resort.Id::{resortId}");
            return this;
        }

        public ApiObjectItemProviderBuilder WithRequireGateway()
        {
            provider.Filters.Add("Gateways.Id>:0");
            return this;
        }

        public ApiObjectItemProviderBuilder WhereName(string nameFilter)
        {
            provider.Filters.Add($"Name:%{nameFilter}%");
            return this;
        }

        public ApiObjectItemProviderBuilder WithSortByName()
        {
            provider.Sort.Clear();
            provider.Sort.Add("Name");
            return this;
        }

        public ApiObjectItemProvider Build()
        {
            return provider;
        }
    }
}
