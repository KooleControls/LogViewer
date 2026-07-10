using System.Text;

namespace LogViewer.Providers.Tcp
{
    /// <summary>
    /// A KC gateway that announced itself with a UDP "hello" broadcast on link-up.
    /// The broadcast (sent to 255.255.255.255:31500 by the firmware's
    /// <c>NetworkManager::SendHello</c>) carries only the sender IP, the 6-char
    /// installation code, and the KC1/KC2 TCP server ports. The gateway name,
    /// firmware version and software id (SID) are NOT in the broadcast - they are
    /// filled in afterwards by briefly connecting over KC2 (see <see cref="DeviceEnricher"/>).
    /// </summary>
    public class DiscoveredDevice
    {
        public string Ip { get; }
        public string InstallCode { get; }
        public int Kc1Port { get; }
        public int Kc2Port { get; }

        /// <summary>Gateway name (DGWN), filled in by enrichment.</summary>
        public string? Name { get; set; }

        /// <summary>Firmware version (DVER), filled in by enrichment.</summary>
        public string? Version { get; set; }

        /// <summary>Software id / SID (DVERSID, 8-hex), filled in by enrichment.</summary>
        public string? Sid { get; set; }

        public DateTime LastSeen { get; set; }

        private DiscoveredDevice(string ip, string installCode, int kc1Port, int kc2Port)
        {
            Ip = ip;
            InstallCode = installCode;
            Kc1Port = kc1Port;
            Kc2Port = kc2Port;
        }

        /// <summary>
        /// Parse a discovery datagram. Layout (see firmware <c>NetworkManager::SendHello</c>):
        /// <c>"&lt;text&gt; started! IIIIIIPPPPPPQQQQQQ"</c> where bytes 15..20 are the
        /// installation code, 21..26 the KC1 server port and 27..32 the KC2 server port,
        /// all ASCII. Returns null if the packet is too short or isn't a KC hello.
        /// </summary>
        public static DiscoveredDevice? Parse(byte[] data, string senderIp)
        {
            if (data.Length < 21)
                return null;

            // Guard against unrelated traffic on the discovery port.
            string prefix = Encoding.ASCII.GetString(data, 0, 15);
            if (!prefix.Contains("started", StringComparison.OrdinalIgnoreCase))
                return null;

            string installCode = Encoding.ASCII.GetString(data, 15, 6).Trim();
            int kc1Port = ParsePort(data, 21) ?? 31600;
            int kc2Port = ParsePort(data, 27) ?? 31700;

            return new DiscoveredDevice(senderIp, installCode, kc1Port, kc2Port);
        }

        private static int? ParsePort(byte[] data, int offset)
        {
            if (data.Length < offset + 6)
                return null;
            if (int.TryParse(Encoding.ASCII.GetString(data, offset, 6), out int port) && port > 0 && port <= 65535)
                return port;
            return null;
        }
    }
}
