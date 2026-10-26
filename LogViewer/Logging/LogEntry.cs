namespace LogViewer.Logging
{
    /// <summary>
    /// One fact, at one moment, about one device. Says nothing about where it came from:
    /// the same entry can be fetched from the API, streamed over TCP or read from a file.
    /// Decoding <see cref="LogCode"/> is the job of whoever owns the codebook named by
    /// <see cref="SourceSoftwareId"/> (see GatewayLogEntryExtensions for the gateway one).
    /// </summary>
    public class LogEntry
    {
        public DateTime TimeStamp { get; set; }

        /// <summary>Which software wrote this entry, and therefore which codebook decodes <see cref="LogCode"/>.</summary>
        public SoftwareId SourceSoftwareId { get; set; }

        /// <summary>The device this entry is about, parents included.</summary>
        public DeviceInfo Device { get; set; } = new();

        public UInt32 LogCode { get; set; }
        public byte[] RawData { get; set; } = Array.Empty<byte>();
        public string? Metadata { get; set; }
        public double? Measurement { get; set; }

        public override string ToString()
            => $"{TimeStamp:HH:mm:ss} {Device} {LogCode:X8}";

        // ---- Migration shims -------------------------------------------------
        // Keep the old flat names alive so the converter, the mappers and every
        // existing .yaml/.cyaml file keep working while call sites move over to
        // Device. Delete both once nothing reads them.

        [Obsolete("Use Device.Type")]
        public DeviceType DeviceType
        {
            get => Device.Type;
            set => Device.Type = value;
        }

        [Obsolete("Use Device.Id")]
        public int DeviceId
        {
            get => Device.Id;
            set => Device.Id = value;
        }
    }
}
