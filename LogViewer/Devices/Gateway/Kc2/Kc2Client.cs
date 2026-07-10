using CoreLib.Ethernet;
using System.Text;

namespace LogViewer.Devices.Gateway.Kc2
{
    /// <summary>
    /// A KC2 protocol client. Connects to a KC gateway's TCP server (default port 31700),
    /// performs the optional RC4 key exchange, and exposes a request/reply command interface.
    ///
    /// Wire format: each frame is '&lt;' + byte-stuffed payload + '&gt;'. When encryption is enabled
    /// the device sends an 8-byte public key on connect, after which every frame is RC4-processed
    /// (encrypt on send / decrypt on receive) over the whole framed buffer.
    /// </summary>
    public class Kc2Client : IDisposable
    {
        private readonly TcpSocketClient socket = new();
        private readonly Arc4Cipher cipher;
        private readonly bool encrypted;

        private readonly SemaphoreSlim commandLock = new(1, 1);
        private readonly object pendingLock = new();
        private byte[]? pendingCommand;
        private TaskCompletionSource<Kc2Reply>? pendingTcs;

        private readonly List<byte> frameBuffer = new();
        private readonly object frameLock = new();

        private volatile bool publicKeyReceived;
        private TaskCompletionSource<bool> readyTcs = new(TaskCreationOptions.RunContinuationsAsynchronously);

        public event EventHandler<ConnectionStates>? ConnectionStateChanged;

        public Kc2Client(bool encrypted, byte[]? privateKey = null)
        {
            this.encrypted = encrypted;
            cipher = privateKey != null ? new Arc4Cipher(privateKey) : new Arc4Cipher();
            socket.OnDataRecieved += OnDataReceived;
            socket.OnConnectionStateChanged += OnConnectionStateChanged;
        }

        public ConnectionStates ConnectionState => socket.ConnectionState;

        /// <summary>Connect to the device. On success, await <see cref="WaitReadyAsync"/> before sending.</summary>
        public async Task<bool> ConnectAsync(string host, int port, CancellationToken token = default)
        {
            publicKeyReceived = !encrypted;
            readyTcs = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);

            lock (frameLock)
                frameBuffer.Clear();

            bool connected = await socket.ConnectAsync(host, port, token);
            if (connected && !encrypted)
                readyTcs.TrySetResult(true); // no key exchange needed
            return connected;
        }

        /// <summary>
        /// Wait until the client is ready to send commands (key exchange complete for encrypted links).
        /// </summary>
        public async Task<bool> WaitReadyAsync(CancellationToken token = default, int timeoutMs = 3000)
        {
            using var cts = CancellationTokenSource.CreateLinkedTokenSource(token);
            cts.CancelAfter(timeoutMs);
            using (cts.Token.Register(() => readyTcs.TrySetResult(false)))
            {
                return await readyTcs.Task;
            }
        }

        public void Disconnect() => socket.Disconnect();

        #region Sending commands

        public Task<Kc2Reply?> SendCommandAsync(string command, CancellationToken token = default, int timeoutMs = 2000)
            => SendCommandAsync(Encoding.ASCII.GetBytes(command), token, timeoutMs);

        public async Task<Kc2Reply?> SendCommandAsync(byte[] command, CancellationToken token = default, int timeoutMs = 2000)
        {
            await commandLock.WaitAsync(token);
            try
            {
                var tcs = new TaskCompletionSource<Kc2Reply>(TaskCreationOptions.RunContinuationsAsynchronously);
                lock (pendingLock)
                {
                    pendingCommand = command;
                    pendingTcs = tcs;
                }

                if (!Send(command))
                    return null;

                using var cts = CancellationTokenSource.CreateLinkedTokenSource(token);
                cts.CancelAfter(timeoutMs);
                using (cts.Token.Register(() => tcs.TrySetCanceled()))
                {
                    try
                    {
                        return await tcs.Task;
                    }
                    catch (OperationCanceledException)
                    {
                        return null; // timeout or cancellation
                    }
                }
            }
            finally
            {
                lock (pendingLock)
                {
                    pendingCommand = null;
                    pendingTcs = null;
                }
                commandLock.Release();
            }
        }

        private bool Send(byte[] payload)
        {
            byte[] stuffed = KcByteStuffing.Stuff(payload);
            byte[] framed = new byte[stuffed.Length + 2];
            framed[0] = (byte)'<';
            Array.Copy(stuffed, 0, framed, 1, stuffed.Length);
            framed[^1] = (byte)'>';

            if (encrypted)
                framed = cipher.Process(framed);

            return socket.SendData(framed);
        }

        #endregion

        #region Receiving data

        private void OnDataReceived(object? sender, byte[] data)
        {
            byte[] chunk = data;

            if (encrypted && !publicKeyReceived)
            {
                if (chunk.Length < 8)
                {
                    socket.Disconnect();
                    return;
                }

                cipher.SetPublicKey(chunk[..8]);
                publicKeyReceived = true;
                readyTcs.TrySetResult(true);

                chunk = chunk[8..];
                if (chunk.Length == 0)
                    return;
            }

            if (encrypted)
                chunk = cipher.Process(chunk);

            ProcessChunk(chunk);
        }

        private void ProcessChunk(byte[] chunk)
        {
            lock (frameLock)
            {
                frameBuffer.AddRange(chunk);

                while (true)
                {
                    int start = frameBuffer.IndexOf((byte)'<');
                    if (start < 0)
                    {
                        // No frame start yet; drop accumulated noise.
                        frameBuffer.Clear();
                        break;
                    }

                    int end = frameBuffer.IndexOf((byte)'>', start + 1);
                    if (end < 0)
                    {
                        // Incomplete frame; keep from the start marker onward.
                        if (start > 0)
                            frameBuffer.RemoveRange(0, start);
                        break;
                    }

                    int len = end - start - 1;
                    byte[] stuffed = new byte[len];
                    frameBuffer.CopyTo(start + 1, stuffed, 0, len);
                    frameBuffer.RemoveRange(0, end + 1);

                    byte[] content = KcByteStuffing.UnStuff(stuffed);
                    HandleFrame(content);
                }
            }
        }

        private void HandleFrame(byte[] content)
        {
            var reply = new Kc2Reply(content);

            lock (pendingLock)
            {
                if (pendingTcs != null && pendingCommand != null && reply.MatchesCommand(pendingCommand))
                {
                    pendingTcs.TrySetResult(reply);
                }
            }
        }

        private void OnConnectionStateChanged(object? sender, ConnectionStates state)
        {
            if (state is ConnectionStates.Disconnected or ConnectionStates.Error or ConnectionStates.Canceled)
            {
                readyTcs.TrySetResult(false);
                lock (pendingLock)
                    pendingTcs?.TrySetCanceled();
            }

            ConnectionStateChanged?.Invoke(this, state);
        }

        #endregion

        public void Dispose()
        {
            socket.OnDataRecieved -= OnDataReceived;
            socket.OnConnectionStateChanged -= OnConnectionStateChanged;
            socket.Disconnect();
            commandLock.Dispose();
        }
    }
}
