using KC.InternalApiClient;
using System.Runtime.CompilerServices;

namespace LogViewer.Providers.API
{
    public class ApiDataProvider<T> : IApiDataProvider<T>
    {
        private readonly InternalApiClient client;
        private readonly List<string> filters;
        private readonly List<string> sort;
        private readonly Func<InternalApiClient, List<string>, List<string>, CancellationToken, IProgress<double>?, IAsyncEnumerable<T>> getItemsFunc;

        public ApiDataProvider(
            InternalApiClient client,
            List<string> filters,
            List<string> sort,
            Func<InternalApiClient, List<string>, List<string>, CancellationToken, IProgress<double>?, IAsyncEnumerable<T>> getItemsFunc)
        {
            this.client = client;
            this.filters = filters;
            this.sort = sort;
            this.getItemsFunc = getItemsFunc;
        }

        public async IAsyncEnumerable<T> GetData([EnumeratorCancellation] CancellationToken token, IProgress<double>? progress)
        {
            var items = getItemsFunc(client, filters, sort, token, progress);
            await foreach (var item in items)
            {
                yield return item;
            }
        }
    }









}
