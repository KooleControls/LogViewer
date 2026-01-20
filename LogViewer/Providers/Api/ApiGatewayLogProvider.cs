using DevExpress.Data.Svg;
using KC.InternalApi.Model;
using KC.InternalApiClient;
using KCObjectsStandard.Data.Api.KC;
using KCObjectsStandard.Data.KC.Api;
using LogViewer.Devices.Gateway;
using LogViewer.Logging;
using Microsoft.VisualBasic.Logging;
using Octokit;
using System;
using System.Collections.Generic;
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
            return log;
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
            catch (OperationCanceledException)
            {
                Debug.WriteLine("Operation was canceled.");
                return logEntries; // Return the log collected so far if the operation is canceled
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error occurred during API call: {ex.Message}");
                if (!ex.Message.Contains("canceled"))
                    throw;
            }
            return logEntries;
        }

        private async Task<List<LogEntry>> GatewayLogApi_GetPageAsync(int page, CancellationToken token)
        {
            List<string> filters =
            [
                $"Gateway.Id::{gatewayId}",
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
                $"DeviceOid::{gatewayId}",
                "Type::1",               // Gateway device
                $"TimeStamp>:{from:s}",  // ISO 8601 format
                $"TimeStamp<:{until:s}"  // ISO 8601 format
            ];

            var stopwatch = Stopwatch.StartNew();
            var apiResult = await client.DeviceApi.Data.GetAllAsync(page, 25, filters, null, token);
            stopwatch.Stop();
            OnResponseTimeReported?.Invoke(this, stopwatch.Elapsed);
            token.ThrowIfCancellationRequested();
            return GatewayLogConverter.ConvertItems(apiResult.Data);
        }
    }


}


