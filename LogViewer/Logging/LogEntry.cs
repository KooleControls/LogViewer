

using LogViewer.Devices.Gateway;

namespace LogViewer.Logging
{
    public class LogEntry
    {
        public DateTime TimeStamp { get; set; }
        public DeviceType DeviceType { get; set; } = DeviceType.Unknown;
        public int DeviceId { get; set; }
        public UInt32 LogCode { get; set; }
        public byte[] RawData { get; set; } = Array.Empty<byte>();
        public string? Metadata { get; set; }
        public double? Measurement { get; set; }

        public override string ToString()
        {
            return $"{DeviceType}:{LogCode.ToString()}";
        }

        public bool Is<TEnum>(TEnum code) where TEnum : Enum
        {
            return LogCode == Convert.ToUInt32(code);
        }

        public bool IsGateway(GatewayLogCodes code)
        {
            if (!GatewayRoutedDevices.Contains(DeviceType))
                return false;

            return LogCode == (uint)code;
        }


        private static readonly DeviceType[] GatewayRoutedDevices =
        {
            DeviceType.Gateway_1245,
            DeviceType.ExtentionModule_1246,
            DeviceType.Thermostat,
            DeviceType.HvacUnit,
            DeviceType.Smarthome
        };
    }
}


