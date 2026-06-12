using CoreLib.Ethernet;
using LogViewer.Devices.Gateway.Kc2;
using LogViewer.Logging;
using LogViewer.Providers.Tcp;

namespace LogViewer.Controls
{
    /// <summary>
    /// Connects to a KC gateway over TCP (Ethernet/Wi-Fi) and streams its log live.
    /// Reads a configurable number of recent lines, then keeps polling for new entries.
    /// </summary>
    public partial class TcpSourceControl : UserControl
    {
        private CancellationTokenSource? cancellationTokenSource;
        private Kc2Client? client;
        private State state = State.Disconnected;

        public event EventHandler<LogEntry>? OnLogReceived;

        public TcpSourceControl()
        {
            InitializeComponent();
            btnConnect.Click += BtnConnect_Click;
            UpdateState(State.Disconnected);
        }

        private async void BtnConnect_Click(object? sender, EventArgs e)
        {
            switch (state)
            {
                case State.Disconnected:
                    await ConnectAsync();
                    break;

                case State.Connected:
                    Cleanup();
                    UpdateState(State.Disconnected);
                    break;

                case State.Connecting:
                    cancellationTokenSource?.Cancel();
                    break;
            }
        }

        private async Task ConnectAsync()
        {
            string host = txtHost.Text.Trim();
            if (string.IsNullOrWhiteSpace(host))
            {
                MessageBox.Show("Host is required.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (!int.TryParse(txtPort.Text.Trim(), out int port))
            {
                MessageBox.Show("Port must be a number.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            bool encrypted = chkEncryption.Checked;
            int lookBack = (int)numLookBack.Value;

            UpdateState(State.Connecting);

            cancellationTokenSource = new CancellationTokenSource();
            var token = cancellationTokenSource.Token;

            client = new Kc2Client(encrypted);

            bool connected;
            try
            {
                connected = await client.ConnectAsync(host, port, token);
            }
            catch (Exception ex)
            {
                ShowError(ex.Message);
                Cleanup();
                UpdateState(State.Disconnected);
                return;
            }

            if (!connected)
            {
                ShowError("Could not connect to the device.");
                Cleanup();
                UpdateState(State.Disconnected);
                return;
            }

            UpdateState(State.Connected);

            var streamer = new DeviceLogStreamer(client, lookBack);
            streamer.OnLogReceived += (s, entry) => RaiseLog(entry);

            // Stream in the background until cancelled or the connection drops.
            _ = Task.Run(async () =>
            {
                try
                {
                    await streamer.RunAsync(token);
                }
                catch (OperationCanceledException)
                {
                }
                catch (Exception ex)
                {
                    RunOnUi(() => ShowError(ex.Message));
                }
                finally
                {
                    RunOnUi(() =>
                    {
                        if (state != State.Disconnected)
                        {
                            Cleanup();
                            UpdateState(State.Disconnected);
                        }
                    });
                }
            });
        }

        private void RaiseLog(LogEntry entry)
        {
            RunOnUi(() => OnLogReceived?.Invoke(this, entry));
        }

        private void RunOnUi(Action action)
        {
            if (IsDisposed)
                return;
            if (InvokeRequired)
                BeginInvoke(action);
            else
                action();
        }

        private void Cleanup()
        {
            cancellationTokenSource?.Cancel();
            client?.Dispose();
            client = null;
            cancellationTokenSource?.Dispose();
            cancellationTokenSource = null;
        }

        private void ShowError(string message)
        {
            MessageBox.Show($"An error occurred:\r\n{message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        private void UpdateState(State newState)
        {
            state = newState;
            switch (state)
            {
                case State.Disconnected:
                    btnConnect.Text = "Connect";
                    SetInputsEnabled(true);
                    break;

                case State.Connecting:
                    btnConnect.Text = "Cancel";
                    SetInputsEnabled(false);
                    break;

                case State.Connected:
                    btnConnect.Text = "Disconnect";
                    SetInputsEnabled(false);
                    break;
            }
        }

        private void SetInputsEnabled(bool enabled)
        {
            txtHost.Enabled = enabled;
            txtPort.Enabled = enabled;
            chkEncryption.Enabled = enabled;
            numLookBack.Enabled = enabled;
        }

        private enum State
        {
            Disconnected,
            Connecting,
            Connected
        }
    }
}
