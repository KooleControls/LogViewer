using CoreLib.Ethernet;
using LogViewer.Devices.Gateway.Kc2;
using LogViewer.Logging;
using LogViewer.Providers.Tcp;
using System.ComponentModel;
using System.Diagnostics;

namespace LogViewer.Controls
{
    /// <summary>
    /// Connects to a KC gateway over TCP (Ethernet/Wi-Fi) and streams its log live.
    /// Reads a configurable number of recent lines, then keeps polling for new entries.
    /// </summary>
    public partial class TcpSourceControl : UserControl
    {
        private const int PollIntervalMs = 1000;

        private CancellationTokenSource? cancellationTokenSource;
        private Kc2Client? client;
        private State state = State.Disconnected;
        private int fetchedCount;

        private DeviceDiscoveryListener? discovery;
        private CancellationTokenSource? discoveryCts;
        private readonly Dictionary<string, DiscoveredDevice> devicesByIp = new();

        public event EventHandler<LogEntry>? OnLogReceived;

        /// <summary>Raised when a fresh connection starts, so the view can be cleared before streaming.</summary>
        public event EventHandler? OnClearRequested;

        public TcpSourceControl()
        {
            InitializeComponent();
            btnConnect.Click += BtnConnect_Click;
            lstDevices.DrawItem += LstDevices_DrawItem;
            lstDevices.DoubleClick += LstDevices_DoubleClick;
            UpdateState(State.Disconnected);
            StartDiscovery();
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

            // Fresh look-back: clear the view so existing entries don't duplicate.
            fetchedCount = 0;
            UpdateInfoLabels();
            OnClearRequested?.Invoke(this, EventArgs.Empty);

            var streamer = new DeviceLogStreamer(client, lookBack, PollIntervalMs);
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

        #region Device discovery

        private void StartDiscovery()
        {
            // Don't open sockets while the WinForms designer is hosting this control.
            if (LicenseManager.UsageMode != LicenseUsageMode.Runtime)
                return;

            try
            {
                discoveryCts = new CancellationTokenSource();
                discovery = new DeviceDiscoveryListener();
                discovery.OnDeviceDiscovered += Discovery_OnDeviceDiscovered;
                discovery.Start();
            }
            catch (Exception ex)
            {
                lblDevices.Text = "Device discovery unavailable (UDP 31500 in use)";
                Debug.WriteLine($"Discovery start failed: {ex.Message}");
            }
        }

        private void StopDiscovery()
        {
            discoveryCts?.Cancel();
            discovery?.Dispose();
            discovery = null;
            discoveryCts?.Dispose();
            discoveryCts = null;
        }

        private void Discovery_OnDeviceDiscovered(object? sender, DiscoveredDevice device)
        {
            RunOnUi(() => AddOrUpdateDevice(device));
        }

        private void AddOrUpdateDevice(DiscoveredDevice device)
        {
            device.LastSeen = DateTime.Now;

            if (devicesByIp.TryGetValue(device.Ip, out var existing))
            {
                // Replace the card for this IP with the fresh broadcast (may be a reboot).
                int idx = lstDevices.Items.IndexOf(existing);
                if (idx >= 0)
                    lstDevices.Items[idx] = device;
                else
                    lstDevices.Items.Add(device);
            }
            else
            {
                lstDevices.Items.Add(device);
            }
            devicesByIp[device.Ip] = device;

            // Fill in name/version/SID (re-query on every broadcast in case the device rebooted).
            _ = EnrichDeviceAsync(device);
        }

        private async Task EnrichDeviceAsync(DiscoveredDevice device)
        {
            bool encrypted = chkEncryption.Checked;
            var parentToken = discoveryCts?.Token ?? CancellationToken.None;

            try
            {
                using var cts = CancellationTokenSource.CreateLinkedTokenSource(parentToken);
                cts.CancelAfter(6000);
                await DeviceEnricher.EnrichAsync(device, encrypted, cts.Token);
            }
            catch
            {
                // Best-effort: leave whatever we managed to read.
            }

            RunOnUi(() => RefreshCard(device));
        }

        /// <summary>Repaint just the card for <paramref name="device"/>, if it's still listed.</summary>
        private void RefreshCard(DiscoveredDevice device)
        {
            int idx = lstDevices.Items.IndexOf(device);
            if (idx >= 0)
                lstDevices.Invalidate(lstDevices.GetItemRectangle(idx));
        }

        /// <summary>Draws each device as a two-line "card": name + version on top, IP · install · SID below.</summary>
        private void LstDevices_DrawItem(object? sender, DrawItemEventArgs e)
        {
            e.DrawBackground();

            if (e.Index < 0 || e.Index >= lstDevices.Items.Count || lstDevices.Items[e.Index] is not DiscoveredDevice d)
                return;

            bool selected = (e.State & DrawItemState.Selected) != 0;
            Color fg = selected ? SystemColors.HighlightText : SystemColors.ControlText;
            Color dim = selected ? SystemColors.HighlightText : SystemColors.GrayText;

            using var titleFont = new Font(lstDevices.Font, FontStyle.Bold);
            using var subFont = new Font(lstDevices.Font.FontFamily, lstDevices.Font.Size - 0.5f);

            const int pad = 7;
            int left = e.Bounds.Left + pad;
            int line1Y = e.Bounds.Top + 5;
            int line2Y = line1Y + titleFont.Height + 1;
            var flags = TextFormatFlags.NoPrefix | TextFormatFlags.EndEllipsis;

            // Line 1: gateway name (left) + version (right).
            string name = string.IsNullOrEmpty(d.Name) ? "KC gateway" : d.Name!;
            string version = d.Version != null ? "v" + d.Version : "…";
            Size vSize = TextRenderer.MeasureText(e.Graphics, version, subFont);
            int nameWidth = e.Bounds.Right - pad - vSize.Width - left - 6;

            TextRenderer.DrawText(e.Graphics, name, titleFont,
                new Rectangle(left, line1Y, Math.Max(nameWidth, 20), titleFont.Height), fg, flags);
            TextRenderer.DrawText(e.Graphics, version, subFont,
                new Point(e.Bounds.Right - pad - vSize.Width, line1Y + 2), dim, TextFormatFlags.NoPrefix);

            // Line 2: IP · install · SID.
            string line2 = $"{d.Ip}   ·   Inst {d.InstallCode}";
            if (!string.IsNullOrEmpty(d.Sid))
                line2 += $"   ·   SID {d.Sid}";
            TextRenderer.DrawText(e.Graphics, line2, subFont,
                new Rectangle(left, line2Y, e.Bounds.Right - pad - left, subFont.Height), dim, flags);

            // Subtle separator between cards.
            if (!selected)
                using (var pen = new Pen(SystemColors.ControlLight))
                    e.Graphics.DrawLine(pen, e.Bounds.Left, e.Bounds.Bottom - 1, e.Bounds.Right, e.Bounds.Bottom - 1);
        }

        private async void LstDevices_DoubleClick(object? sender, EventArgs e)
        {
            if (lstDevices.SelectedItem is not DiscoveredDevice device)
                return;

            txtHost.Text = device.Ip;
            txtPort.Text = device.Kc2Port.ToString();

            // Drop any current connection, then connect to the picked device.
            if (state == State.Connected)
            {
                Cleanup();
                UpdateState(State.Disconnected);
            }

            if (state == State.Disconnected)
                await ConnectAsync();
        }

        #endregion

        private void RaiseLog(LogEntry entry)
        {
            RunOnUi(() =>
            {
                fetchedCount++;
                UpdateInfoLabels();
                OnLogReceived?.Invoke(this, entry);
            });
        }

        private void UpdateInfoLabels()
        {
            lblPollInterval.Text = $"Poll interval: {PollIntervalMs} ms";
            lblFetched.Text = $"Lines fetched: {fetchedCount}";
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
