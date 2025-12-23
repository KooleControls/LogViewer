

using KCObjectsStandard.Device.KC.Gateway.SmartHome;
using LogViewer.Devices.Gateway;
using System.Reflection.Metadata;

namespace LogViewer.Logging
{
    public class LogEntry
    {
        public DateTime TimeStamp { get; set; }
        public SoftwareId SourceSoftwareId { get; set; }
        public DeviceType DeviceType { get; set; } = DeviceType.Unknown;
        public int DeviceId { get; set; }
        public UInt32 LogCode { get; set; }
        public byte[] RawData { get; set; } = Array.Empty<byte>();
        public string? Metadata { get; set; }
        public double? Measurement { get; set; }

        public GatewayLogCodes? AsGatewayLogCode()
        {
            return SourceSoftwareId == SoftwareId.Gateway_1245
                ? (GatewayLogCodes?)LogCode
                : null;
        }

        public override string ToString()
        {
            return AsGatewayLogCode()?.ToString() ?? $"{LogCode}";
        }

    }
}


