using KC.InternalApi.Model;
using KC.InternalApiClient;
using System.Runtime.CompilerServices;
using System.Threading;

namespace LogViewer.Providers.API
{
    public class ApiResortProviderBuilder
    {
        private readonly InternalApiClient client;
        private readonly List<string> filters = new();
        private readonly List<string> sort = new();

        public ApiResortProviderBuilder(InternalApiClient client)
        {
            this.client = client;
        }

        public ApiResortProviderBuilder ForOrganisation(int organisationId)
        {
            filters.Add($"Organization.Id::{organisationId}");
            return this;
        }

        public ApiResortProviderBuilder WithSortByName()
        {
            sort.Clear();
            sort.Add("Name");
            return this;
        }

        public ApiDataProvider<Resort> Build()
        {
            Func<InternalApiClient, List<string>, List<string>, CancellationToken, IProgress<double>?, IAsyncEnumerable<Resort>> getItemsFunc =
                (c, f, s, t, p) => c.ResortApi.GetAllItemsAsync(f, s, t, p);

            return new ApiDataProvider<Resort>(
                client,
                filters,
                sort,
                getItemsFunc
            );
        }
    }


}
