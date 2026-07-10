using System.Net;
using System.Net.Sockets;

namespace LogViewer.Providers.Tcp
{
    /// <summary>
    /// Listens for KC gateway "hello" broadcasts on UDP port 31500 and raises
    /// <see cref="OnDeviceDiscovered"/> for each valid packet. Devices broadcast
    /// only once - when they acquire an IP (Ethernet plugged in / reboot) - so this
    /// must already be running to catch a device as it comes online.
    /// </summary>
    public class DeviceDiscoveryListener : IDisposable
    {
        public const int DiscoveryPort = 31500;

        private UdpClient? udp;
        private CancellationTokenSource? cts;

        public event EventHandler<DiscoveredDevice>? OnDeviceDiscovered;

        /// <summary>Bind the socket and start the receive loop. Throws if the port can't be bound.</summary>
        public void Start()
        {
            if (udp != null)
                return;

            var client = new UdpClient();
            // ReuseAddress so we can share port 31500 with other listeners/tools on this machine.
            client.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
            client.Client.Bind(new IPEndPoint(IPAddress.Any, DiscoveryPort));
            udp = client;

            cts = new CancellationTokenSource();
            _ = ReceiveLoopAsync(cts.Token);
        }

        private async Task ReceiveLoopAsync(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                UdpReceiveResult result;
                try
                {
                    result = await udp!.ReceiveAsync(token);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (ObjectDisposedException)
                {
                    break;
                }
                catch
                {
                    // Transient socket error - keep listening.
                    continue;
                }

                var device = DiscoveredDevice.Parse(result.Buffer, result.RemoteEndPoint.Address.ToString());
                if (device != null)
                    OnDeviceDiscovered?.Invoke(this, device);
            }
        }

        public void Dispose()
        {
            cts?.Cancel();
            udp?.Dispose();
            udp = null;
            cts?.Dispose();
            cts = null;
        }
    }
}
