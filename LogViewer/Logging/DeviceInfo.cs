namespace LogViewer.Logging
{
    /// <summary>
    /// Identifies the device a log entry is about.
    ///
    /// An id is only unique within its parent: HVAC unit 2 behind gateway 4711 and
    /// HVAC unit 2 behind gateway 4712 are different devices. The chain of parents
    /// is what makes the identity whole.
    ///
    /// Devices that are the only one of their kind on their parent (the built-in
    /// thermostat, the smarthome module, the gateway itself) keep <see cref="Id"/> 0.
    /// </summary>
    public class DeviceInfo
    {
        public DeviceType Type { get; set; } = DeviceType.Unknown;

        /// <summary>Id within <see cref="Parent"/>. For a root device this is its own id (a gateway's DGID).</summary>
        public int Id { get; set; }

        /// <summary>The device this one hangs off, or null when it is the root.</summary>
        public DeviceInfo? Parent { get; set; }

        public override string ToString()
            => Parent is null ? $"{Type}{Id}" : $"{Parent}/{Type}{Id}";
    }
}
