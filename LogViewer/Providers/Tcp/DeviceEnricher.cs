using LogViewer.Devices.Gateway.Kc2;

namespace LogViewer.Providers.Tcp
{
    /// <summary>
    /// Fills in the fields a discovery broadcast doesn't carry - gateway name,
    /// firmware version and software id (SID) - by briefly opening a KC2 connection
    /// to the device and querying DGWN / DVER / DVERSID, then disconnecting.
    /// Best-effort: any query that fails simply leaves that field null.
    /// </summary>
    public static class DeviceEnricher
    {
        public static async Task EnrichAsync(DiscoveredDevice device, bool encrypted, CancellationToken token)
        {
            using var client = new Kc2Client(encrypted);

            if (!await client.ConnectAsync(device.Ip, device.Kc2Port, token))
                return;

            try
            {
                if (!await client.WaitReadyAsync(token))
                    return;

                device.Name = await QueryAsync(client, "DGWN", token);      // gateway name
                device.Version = await QueryAsync(client, "DVER", token);   // firmware version, e.g. 01.00.00
                device.Sid = await QueryAsync(client, "DVERSID", token);    // software id (8-hex)
            }
            finally
            {
                client.Disconnect();
            }
        }

        private static async Task<string?> QueryAsync(Kc2Client client, string command, CancellationToken token)
        {
            var reply = await client.SendCommandAsync(command, token);
            if (reply != null && reply.IsAck)
            {
                string value = reply.ArgsAsString.Trim();
                return value.Length > 0 ? value : null;
            }
            return null;
        }
    }
}
