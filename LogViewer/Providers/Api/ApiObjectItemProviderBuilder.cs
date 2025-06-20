using KC.InternalApi.Model;
using KC.InternalApiClient;
using Octokit;

namespace LogViewer.Providers.API
{
    public class ApiObjectItemProviderBuilder
    {
        private readonly InternalApiClient client;
        private readonly List<string> filters = new();
        private readonly List<string> sort = new();

        public ApiObjectItemProviderBuilder(InternalApiClient client)
        {
            this.client = client;
        }

        public ApiObjectItemProviderBuilder ForResort(int resortId)
        {
            filters.Add($"Resort.Id::{resortId}");
            return this;
        }

        public ApiObjectItemProviderBuilder WithRequireGateway()
        {
            filters.Add("Gateways.Id>:0");
            return this;
        }

        public ApiObjectItemProviderBuilder WhereName(string nameFilter)
        {
            filters.Add($"Name:%{nameFilter}%");
            return this;
        }

        public ApiObjectItemProviderBuilder WithSortByName()
        {
            sort.Clear();
            sort.Add("Name");
            return this;
        }

        public ApiDataProvider<ObjectItem> Build()
        {
            Func<InternalApiClient, List<string>, List<string>, CancellationToken, IProgress<double>?, IAsyncEnumerable<ObjectItem>> getItemsFunc =
                (c, f, s, t, p) =>
                {
                    return c.ObjectApi.GetAllItemsAsync(f, s, t, p);
                };
            
            return new ApiDataProvider<ObjectItem>(
                client,
                filters,
                sort,
                getItemsFunc
            );
        }
    }









}
