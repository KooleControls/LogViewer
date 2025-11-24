using System.Reflection;
using System.Runtime.CompilerServices;
using YamlDotNet.Core;
using YamlDotNet.Core.Events;
using YamlDotNet.Serialization;

namespace LogViewer.Logging
{
    public class LogEntry
    {
        public DateTime TimeStamp { get; set; }
        public DeviceType DeviceType { get; set; } = DeviceType.Unknown;
        public int DeviceId { get; set; }
        public object? LogCode { get; set; }
        public byte[] RawData { get; set; } = Array.Empty<byte>();
        public object? Measurement { get; set; }

        public override string ToString()
        {
            return $"{DeviceType}:{LogCode?.ToString()}";
        }
    }
}


