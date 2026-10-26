using LogViewer.Logging;

namespace LogViewer.Devices.Gateway
{
    /// <summary>
    /// Reads a <see cref="LogEntry"/> as gateway firmware output. Lives here, not on the
    /// model: which codebook applies to a log code is gateway knowledge, and a log entry
    /// is meant to be readable without it.
    /// </summary>
    public static class GatewayLogEntryExtensions
    {
        public static GatewayLogCodes? AsGatewayLogCode(this LogEntry entry)
        {
            return entry.SourceSoftwareId == SoftwareId.Gateway_1245
                ? (GatewayLogCodes?)entry.LogCode
                : null;
        }

        /// <summary>Code name when the codebook is known, hex otherwise.</summary>
        public static string DescribeLogCode(this LogEntry entry)
            => entry.AsGatewayLogCode()?.ToString() ?? $"0x{entry.LogCode:X8}";
    }
}
