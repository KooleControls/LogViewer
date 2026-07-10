using LogViewer.Devices.Gateway;
using LogViewer.Devices.Gateway.Kc2;
using LogViewer.Logging;
using System.Diagnostics;

namespace LogViewer.Providers.Tcp
{
    /// <summary>
    /// Drives a <see cref="Kc2Client"/> to stream device logs live:
    ///   1. rewinds the device read pointer <c>lookBackLines</c> entries before the write pointer,
    ///   2. drains everything from there to the present,
    ///   3. then polls once per second for newly written entries.
    ///
    /// Each parsed entry is raised via <see cref="OnLogReceived"/>.
    /// </summary>
    public class DeviceLogStreamer
    {
        private readonly Kc2Client client;
        private readonly int lookBackLines;
        private readonly int pollIntervalMs;

        public event EventHandler<LogEntry>? OnLogReceived;

        public DeviceLogStreamer(Kc2Client client, int lookBackLines, int pollIntervalMs = 1000)
        {
            this.client = client;
            this.lookBackLines = lookBackLines;
            this.pollIntervalMs = pollIntervalMs;
        }

        public async Task RunAsync(CancellationToken token)
        {
            if (!await client.WaitReadyAsync(token))
                throw new InvalidOperationException("Device did not complete the connection handshake.");

            await SetupLookBackAsync(token);

            // Drain the look-back window, then keep polling for new entries.
            while (!token.IsCancellationRequested)
            {
                var reply = await client.SendCommandAsync("DLGN01", token);

                if (reply != null && reply.IsAck && reply.Args.Length == GatewayLogFrameParser.FrameLength)
                {
                    var entry = GatewayLogFrameParser.Parse(reply.Args);
                    if (entry != null)
                        OnLogReceived?.Invoke(this, entry);
                    // Got one - immediately try the next so the back-log drains fast.
                }
                else
                {
                    // NAK (no more logs) or a timeout: wait before polling again.
                    await Task.Delay(pollIntervalMs, token);
                }
            }
        }

        private async Task SetupLookBackAsync(CancellationToken token)
        {
            int writePos = await ReadIntAsync("DLGCPOS", token);
            int max = await ReadIntAsync("DLGCMAX", token);

            if (writePos < 0 || max <= 0)
            {
                // Couldn't determine the buffer geometry; fall back to streaming new entries only.
                Debug.WriteLine($"DeviceLogStreamer: could not read log pointers (pos={writePos}, max={max}).");
                return;
            }

            int n = Math.Min(lookBackLines, max - 1);

            // Rewind n entries before the write pointer (wraps around the circular buffer).
            int start = ((writePos - n) % max + max) % max;

            await client.SendCommandAsync($"CLGP{start}", token);
        }

        private async Task<int> ReadIntAsync(string command, CancellationToken token)
        {
            var reply = await client.SendCommandAsync(command, token);
            if (reply != null && reply.IsAck && int.TryParse(reply.ArgsAsString.Trim(), out int value))
                return value;
            return -1;
        }
    }
}
