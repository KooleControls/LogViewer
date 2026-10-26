using KC.InternalApiClient;
using KC.InternalApiClient.Model.Exceptions;
using LogViewer.Devices.Gateway;
using LogViewer.Logging;
using System.Diagnostics;

namespace LogViewer.Providers.API
{
    public class ApiGatewayLogProvider
    {
        private readonly InternalApiClient client;
        private readonly int gatewayOid;
        private readonly int? gatewayDeviceId;
        private readonly DateTime from;
        private readonly DateTime until;

        public event EventHandler<TimeSpan>? OnResponseTimeReported;

        /// <param name="gatewayOid">Database key of the gateway record, used to scope the query.</param>
        /// <param name="gatewayDeviceId">The gateway's own device id (DGID), stamped onto the entries.</param>
        public ApiGatewayLogProvider(InternalApiClient client, int gatewayOid, int? gatewayDeviceId, DateTime from, DateTime until)
        {
            this.client = client;
            this.gatewayOid = gatewayOid;
            this.gatewayDeviceId = gatewayDeviceId;
            this.from = from;
            this.until = until;
        }

        public async Task<LogCollection> GetData(CancellationToken token = default, IProgress<double>? progress = default)
        {
            DateTime latestTimestamp = from;
            DateTime fromPercent = from;
            if (fromPercent > DateTime.Now)
                fromPercent = DateTime.Now;

            double totalTimeSpan = (until - fromPercent).TotalSeconds;

            Progress<DateTime> progressConverter = new Progress<DateTime>(timeStamp =>
            {
                if (timeStamp > latestTimestamp)
                    latestTimestamp = timeStamp;

                if (totalTimeSpan > 0)
                {
                    double progressValue = (latestTimestamp - fromPercent).TotalSeconds / totalTimeSpan;
                    progress?.Report(progressValue);
                }
            });

            LogCollection log = new();
            log.Entries.AddRange(await GetAllAsync(GatewayLogApi_GetPageAsync, token, progressConverter));
            log.Entries.AddRange(await GetAllAsync(DeviceApiData_GetPageAsync, token, progressConverter));

            StampGatewayAsParent(log);

            return log;
        }

        /// <summary>
        /// A log row only identifies the component inside the gateway (the thermostat, HVAC unit 2,
        /// the gateway itself) - never which gateway that is. This fetch was scoped to one gateway,
        /// so this is where that knowledge lives, and it gets stamped on as the parent.
        /// One shared instance: every entry here came from the same gateway.
        /// </summary>
        private void StampGatewayAsParent(LogCollection log)
        {
            if (gatewayDeviceId is not int deviceId)
                return;

            var gateway = new DeviceInfo
            {
                Type = LogViewer.Logging.DeviceType.Gateway,
                Id = deviceId,
            };

            foreach (var entry in log.Entries)
                entry.Device.Parent = gateway;
        }



        delegate Task<List<LogEntry>> GetPageDelegate(int page, CancellationToken token);

        private async Task<List<LogEntry>> GetAllAsync(GetPageDelegate getPage, CancellationToken token, IProgress<DateTime>? progress)
        {
            List<LogEntry> logEntries = new List<LogEntry>();
            try
            {
                for (int page = 0; !token.IsCancellationRequested; page++)
                {
                    var items = await getPage(page, token);

                    // Exit the loop when no more data is returned
                    if (items.Count == 0)
                        break;

                    logEntries.AddRange(items);
                    progress?.Report(items.Last().TimeStamp);
                }
            }
            catch (KCApiException ex)
            {
                if (ex.InnerException is OperationCanceledException operationCanceledException)
                {
                    return logEntries;
                }
                else if (ex.InnerException is KC.InternalApi.Client.ApiException argExApi)
                {
                    switch (argExApi.ErrorCode)
                    {
                        case 403:
                            return logEntries; // Suppress error on not updated api's
                    }
                }


                Debug.WriteLine($"Error occurred during API call: {ex.Message}");
                throw;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error occurred during API call: {ex.Message}");
                throw;
            }
            return logEntries;
        }

        private async Task<List<LogEntry>> GatewayLogApi_GetPageAsync(int page, CancellationToken token)
        {
            List<string> filters =
            [
                $"Gateway.Id::{gatewayOid}",
                $"TimeStamp>:{from:s}",  // ISO 8601 format
                $"TimeStamp<:{until:s}"  // ISO 8601 format
            ];

            var stopwatch = Stopwatch.StartNew();
            var apiResult = await client.GatewayLogApi.GetAllAsync(page, 25, filters, null, token);
            stopwatch.Stop();

            OnResponseTimeReported?.Invoke(this, stopwatch.Elapsed);
            token.ThrowIfCancellationRequested();
            return GatewayLogConverter.ConvertItems(apiResult.Data);
        }

        private async Task<List<LogEntry>> DeviceApiData_GetPageAsync(int page, CancellationToken token)
        {
            List<string> filters =
            [
                $"DeviceOid::{gatewayOid}",
                "Type::1",               // Gateway device
                $"TimeStamp>:{from:s}",  // ISO 8601 format
                $"TimeStamp<:{until:s}"  // ISO 8601 format
            ];

            List<string> sort = [
                "Id"
            ];

            var stopwatch = Stopwatch.StartNew();
            var apiResult = await client.DeviceApi.Data.GetAllAsync(page, 25, filters, sort, token);
            stopwatch.Stop();
            OnResponseTimeReported?.Invoke(this, stopwatch.Elapsed);
            token.ThrowIfCancellationRequested();
            return GatewayLogConverter.ConvertItems(apiResult.Data);
        }
    }


}


