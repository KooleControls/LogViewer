using KC.InternalApi.Model;
using LogViewer.Logging;

namespace LogViewer.Devices.Gateway
{
    /// <summary>
    /// Parses a raw 64-byte gateway <c>log_data_t</c> frame (as delivered over MQTT or the KC2 TCP
    /// protocol) into a <see cref="LogEntry"/>.
    ///
    /// Layout (64 bytes): code(1) owner(8) timestamp(5) actionCode(1) data(46) version(3).
    /// </summary>
    public static class GatewayLogFrameParser
    {
        public const int FrameLength = 64;

        public static LogEntry? Parse(byte[] buf)
        {
            if (buf == null || buf.Length != FrameLength)
                return null;

            int offset = 0;

            byte code = buf[offset++];

            offset += 8; // owner (unused)

            byte[] timestampBytes = buf.Skip(offset).Take(5).ToArray();
            offset += 5;

            byte actionCode = buf[offset++];

            byte[] data = buf.Skip(offset).Take(46).ToArray();
            offset += 46;

            byte[] versionArr = buf.Skip(offset).Take(3).ToArray();
            string version = $"{versionArr[0]}.{versionArr[1]}.{versionArr[2]}";

            DateTime timeStamp = ExtractDateTimeFromKcPacking(timestampBytes);

            var log = new GatewayLog(
                id: null,
                code: code,
                timeStamp: timeStamp,
                data: data,
                actionCode: actionCode,
                varVersion: version,
                gateway: null
            );

            return GatewayLogConverter.FromGatewayLog(log);
        }

        /// <summary>
        /// Decodes the firmware's 5-byte packed timestamp.
        /// Bit layout: year(14) month(4) day(5) hour(5) minute(6) second(6).
        /// </summary>
        public static DateTime ExtractDateTimeFromKcPacking(byte[] ts)
        {
            if (ts == null || ts.Length < 5)
                throw new ArgumentException("Timestamp array must contain at least 5 bytes.", nameof(ts));

            int year = (ts[0] << 6) | (ts[1] >> 2);

            int monthHigh2 = ts[1] & 0x03;
            int monthLow2 = (ts[2] >> 6) & 0x03;
            int month = (monthHigh2 << 2) | monthLow2;

            int day = (ts[2] >> 1) & 0x1F;

            int hourMsb = ts[2] & 0x01;
            int hourLow4 = (ts[3] >> 4) & 0x0F;
            int hour = (hourMsb << 4) | hourLow4;

            int minuteHigh4 = ts[3] & 0x0F;
            int minuteLow2 = (ts[4] >> 6) & 0x03;
            int minute = (minuteHigh4 << 2) | minuteLow2;

            int second = ts[4] & 0x3F;

            try
            {
                return new DateTime(year, month, day, hour, minute, second, DateTimeKind.Local);
            }
            catch
            {
                return DateTime.MinValue;
            }
        }
    }
}
