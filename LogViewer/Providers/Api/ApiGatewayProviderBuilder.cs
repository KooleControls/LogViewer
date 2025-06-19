using KC.InternalApi.Model;
using KC.InternalApiClient;
using System.Runtime.CompilerServices;

namespace LogViewer.Providers.API
{
    public class ApiGatewayProviderBuilder
    {
        private readonly InternalApiClient client;
        private readonly List<string> filters = new();
        private readonly List<string> sort = new();

        public ApiGatewayProviderBuilder(InternalApiClient client)
        {
            this.client = client;
        }

        public ApiGatewayProviderBuilder ForObjectItem(int objectItemId)
        {
            filters.Add($"ObjectItem.Id::{objectItemId}");
            return this;
        }

        public ApiGatewayProviderBuilder WithSortByName()
        {
            sort.Clear();
            sort.Add("Name");
            return this;
        }

        public ApiDataProvider<Gateway> Build()
        {
            Func<InternalApiClient, List<string>, List<string>, CancellationToken, IProgress<double>?, IAsyncEnumerable<Gateway>> getItemsFunc =
                (c, f, s, t, p) => c.GatewayApi.GetAllItemsAsync(f, s, t, p);

            return new ApiDataProvider<Gateway>(
                client,
                filters,
                sort,
                getItemsFunc
            );
        }
    }

}
