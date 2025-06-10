using KC.InternalApi.Model;
using KC.InternalApiClient;
using System.Runtime.CompilerServices;

namespace LogViewer.Providers.API
{
    public class ApiObjectItemProvider : IApiDataProvider<ObjectItem>
    {
        private readonly InternalApiClient client;
        private readonly int resortId;
        public ApiObjectItemProvider(InternalApiClient client, int resortId)
        {
            this.client = client;
            this.resortId = resortId;
        }

        public async IAsyncEnumerable<ObjectItem> GetData([EnumeratorCancellation] CancellationToken token, IProgress<double>? progress)
        {
            List<string> filters = new List<string> { $"Resort.Id::{resortId}" };
            List<string> sort = new List<string>() { "Name" };

            var items = client.ObjectApi.GetAllItemsAsync(filters, sort, token, progress);
            await foreach (var item in items)
                yield return item;
        }
    }
}
