using KC.InternalApi.Model;
using KC.InternalApiClient;
using LogViewer.Devices.Gateway;
using LogViewer.Logging;

namespace LogViewer.Providers.API
{
    /// <summary>
    /// When each part of a house last communicated, found in the logs of its gateway:
    /// SNTP comes from the gateway itself, the actual temperature from the OpenTherm thermostat
    /// and from the Modbus indoor unit. Null means nothing was found within the searched period.
    /// </summary>
    public class LastCommunication
    {
        public DateTime? Sntp { get; private set; }
        public DateTime? Thermostat { get; private set; }
        public DateTime? IndoorUnit { get; private set; }
        public DateTime? AnyLog { get; private set; }

        /// <summary>All logs fetched while searching, covering <see cref="From"/> .. <see cref="Until"/>.</summary>
        public LogCollection Logs { get; } = new();
        public DateTime From { get; set; }
        public DateTime Until { get; set; }

        public bool AllFound => Sntp != null && Thermostat != null && IndoorUnit != null;

        public void Add(IEnumerable<LogEntry> entries)
        {
            foreach (var entry in entries)
            {
                Logs.Entries.Add(entry);
                AnyLog = Latest(AnyLog, entry.TimeStamp);

                switch ((GatewayLogCodes)entry.LogCode)
                {
                    case GatewayLogCodes.x3E_TCPIP_SNTP_INFO:
                        Sntp = Latest(Sntp, entry.TimeStamp);
                        break;
                    case GatewayLogCodes.Therm_TempActualChanged:
                        Thermostat = Latest(Thermostat, entry.TimeStamp);
                        break;
                    case GatewayLogCodes.CMN_IndoorUnit_ActualTempChanged:
                    case GatewayLogCodes.Hvac_ActualTempChanged:
                        IndoorUnit = Latest(IndoorUnit, entry.TimeStamp);
                        break;
                }
            }
        }

        private static DateTime Latest(DateTime? current, DateTime candidate)
            => current == null || candidate > current ? candidate : current.Value;
    }

    public static class LastCommunicationScanner
    {
        private static readonly TimeSpan FirstChunk = TimeSpan.FromHours(6);

        /// <summary>
        /// The API returns logs oldest first, so finding the latest entry means fetching a whole period.
        /// Searching backwards in growing chunks keeps that cheap: a healthy house is done after the
        /// first chunk, and a silent house has few logs, so its large chunks cost few pages.
        /// </summary>
        public static async Task<LastCommunication> ScanAsync(
            InternalApiClient client, Gateway gateway, TimeSpan maxLookBack, CancellationToken token)
        {
            if (gateway.Id == null)
                throw new ArgumentException("Gateway has no id.", nameof(gateway));

            DateTime until = DateTime.Now;
            DateTime oldest = until - maxLookBack;
            var result = new LastCommunication { From = until, Until = until };

            DateTime chunkEnd = until;
            TimeSpan chunk = FirstChunk;
            while (chunkEnd > oldest && !result.AllFound)
            {
                DateTime chunkStart = chunkEnd - chunk < oldest ? oldest : chunkEnd - chunk;

                var provider = new ApiGatewayLogProvider(client, gateway.Id.Value, (int?)gateway.GatewayId, chunkStart, chunkEnd);
                var logs = await provider.GetData(token);
                // The provider returns what it has on cancel; a partial chunk must not look like "not found".
                token.ThrowIfCancellationRequested();

                result.Add(logs.Entries);
                result.From = chunkStart;

                chunkEnd = chunkStart;
                chunk *= 2;
            }
            return result;
        }
    }
}
