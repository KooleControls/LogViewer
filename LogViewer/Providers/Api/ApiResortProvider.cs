using KC.InternalApi.Model;
using KC.InternalApiClient;
using System.Runtime.CompilerServices;
using System.Threading;

namespace LogViewer.Providers.API
{
    public class ApiResortProvider : IApiDataProvider<Resort>
    {
        private readonly InternalApiClient client;
        private readonly int organisationId;
        public ApiResortProvider(InternalApiClient client, int organisationId)
        {
            this.client = client;
            this.organisationId = organisationId;
        }

        public async IAsyncEnumerable<Resort> GetData([EnumeratorCancellation] CancellationToken token, IProgress<double>? progress)
        {
            List<string> filters = new List<string> { $"Organization.Id::{organisationId}" };
            List<string> sort = new List<string>(){ "Name" };

            var items = client.ResortApi.GetAllItemsAsync(filters, sort, token, progress);
            await foreach (var item in items)
                yield return item;

        }
    }
}
