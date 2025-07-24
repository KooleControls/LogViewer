using DevExpress.Data.Svg;
using KC.InternalApi.Model;
using KC.InternalApiClient;
using LogViewer.Logging;
using System.ComponentModel.Design;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace LogViewer.Providers.API
{
    public class ApiGatewayLogProvider
    {
        private readonly InternalApiClient client;
        private readonly int gatewayId;
        private readonly DateTime from;
        private readonly DateTime until;

        public event EventHandler<TimeSpan>? OnResponseTimeReported;

        public ApiGatewayLogProvider(InternalApiClient client, int gatewayId, DateTime from, DateTime until)
        {
            this.client = client;
            this.gatewayId = gatewayId;
            this.from = from;
            this.until = until;
        }

        public async Task<LogCollection> GetData(CancellationToken token = default, IProgress<double>? progress = default)
        {

            LogCollection log = new ();
            int page = 0;            
            DateTime latestTimestamp = from;
            DateTime fromPercent = from;
            if (fromPercent > DateTime.Now)
                fromPercent = DateTime.Now;

            double totalTimeSpan = (until - fromPercent).TotalSeconds;

            List<string> filters =
            [
                $"Gateway.Id::{gatewayId}",
                $"TimeStamp >:{from:o}",  // ISO 8601 format
                $"TimeStamp <:{until:o}"  // ISO 8601 format
            ];


            while (true)
            {
                var stopwatch = Stopwatch.StartNew();
                try
                {
                    var apiResult = await client.GatewayLogApi.GetAllAsync(page, 25, filters, null, token);
                    stopwatch.Stop();
                    OnResponseTimeReported?.Invoke(this, stopwatch.Elapsed);
                    token.ThrowIfCancellationRequested();

                    // Exit the loop when no more data is returned
                    if (apiResult.Data.Count == 0)
                        break;

                    foreach (var item in apiResult.Data)
                    {
                        var convertedItem = WebApiLogItemConverter.ConvertItem(item);
                        if (convertedItem != null)
                        {
                            log.Entries.Add(convertedItem);

                            // Track the latest timestamp
                            if (latestTimestamp < item.TimeStamp)
                                latestTimestamp = item.TimeStamp.Value;
                        }
                    }

                    // Report progress
                    if (totalTimeSpan > 0)
                    {
                        double progressValue = (latestTimestamp - fromPercent).TotalSeconds / totalTimeSpan;
                        progress?.Report(progressValue);
                    }

                    page++; // Move to the next page
                }
                catch (OperationCanceledException)
                {
                    Debug.WriteLine("Operation was canceled.");
                    return log; // Return the log collected so far if the operation is canceled
                }
                catch (Exception ex) when (!(ex is OperationCanceledException))
                {
                    Debug.WriteLine($"Error occurred during API call: {ex.Message}");
                    throw; // Re-throw the exception to be handled by the caller
                }
            }

            return log;
        }
    }
}
