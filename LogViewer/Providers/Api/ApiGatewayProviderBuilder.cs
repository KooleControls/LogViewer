using KC.InternalApiClient;

namespace LogViewer.Providers.API
{
    public class ApiGatewayProviderBuilder
    {
        private readonly InternalApiClient client;
        private readonly ApiGatewayProvider provider;

        public ApiGatewayProviderBuilder(InternalApiClient client)
        {
            this.client = client;
            provider = new ApiGatewayProvider(client);
        }

        public ApiGatewayProviderBuilder ForObjectItem(int objectItemId)
        {
            provider.Filters.Add($"ObjectItem.Id::{objectItemId}");
            return this;
        }

        public ApiGatewayProviderBuilder WithSortByName()
        {
            provider.Sort.Clear();
            provider.Sort.Add("Name");
            return this;
        }

        public ApiGatewayProvider Build()
        {
            return provider;
        }
    }

}
