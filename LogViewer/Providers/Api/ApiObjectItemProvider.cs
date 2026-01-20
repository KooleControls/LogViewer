using KC.InternalApi.Model;
using KC.InternalApiClient;
using System.Runtime.CompilerServices;

namespace LogViewer.Providers.API
{
    public class ApiObjectItemProvider : IApiDataProvider<ObjectItem>
    {
        private readonly InternalApiClient client;
        public List<string> Filters { get; } = new List<string>();
        public List<string> Sort { get; } = new List<string>();

        public ApiObjectItemProvider(InternalApiClient client)
        {
            this.client = client;
        }

        public async IAsyncEnumerable<ObjectItem> GetData([EnumeratorCancellation] CancellationToken token, IProgress<double>? progress)
        {
            var items = client.ObjectApi.GetAllItemsAsync(Filters, Sort, token, progress);
            await foreach (var item in items)
            {
                yield return item;
            }
        }
    }







}
