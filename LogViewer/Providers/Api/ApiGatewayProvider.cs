using KC.InternalApi.Model;
using KC.InternalApiClient;
using System.Runtime.CompilerServices;

namespace LogViewer.Providers.API
{
    public class ApiGatewayProvider : IApiDataProvider<Gateway>
    {
        private readonly InternalApiClient client;
        private readonly int objectItemId;
        public ApiGatewayProvider(InternalApiClient client, int objectItemId)
        {
            this.client = client;
            this.objectItemId = objectItemId;
        }

        public async IAsyncEnumerable<Gateway> GetData([EnumeratorCancellation] CancellationToken token, IProgress<double>? progress)
        {
            List<string> filters = new List<string> { $"ObjectItem.Id::{objectItemId}" };
            List<string> sort = new List<string>() { "Name" };

            var items = client.GatewayApi.GetAllItemsAsync(filters, sort, token, progress);
            await foreach (var item in items)
                yield return item;
        }
    }
}
