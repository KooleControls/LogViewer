using KC.InternalApi.Model;
using KCObjectsStandard.Data.CodeGeneration;
using LogViewer.Devices.Gateway;
using LogViewer.Logging;
using MQTTnet;
using MQTTnet.Protocol;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace LogViewer.Controls
{
    public partial class MqttSourceControl : UserControl
    {
        private CancellationTokenSource? cancellationTokenSource;
        private State state;
        private IMqttClient? client;
        public event EventHandler<LogEntry> OnLogReceived;

        public MqttSourceControl()
        {
            InitializeComponent();
            state = State.Disconnected;

            btnConnect.Click += BtnConnect_Click;

            // Create the MQTT client
            var factory = new MqttClientFactory();
            client = factory.CreateMqttClient();

            // Subscribe to receive messages
            client.ApplicationMessageReceivedAsync += Client_ApplicationMessageReceivedAsync;
        }

        private async Task Client_ApplicationMessageReceivedAsync(MqttApplicationMessageReceivedEventArgs arg)
        {
            var payload = arg.ApplicationMessage.ConvertPayloadToString();

            // Convert hex string -> byte[]
            byte[] buf = Enumerable.Range(0, payload.Length)
                                    .Where(i => i % 2 == 0)
                                    .Select(i => Convert.ToByte(payload.Substring(i, 2), 16))
                                    .ToArray();

            if (buf.Length != 64)
                return; // malformed or wrong log type

            // ---- Parse log_data_t ----
            int offset = 0;

            byte code = buf[offset++];

            byte[] owner = buf.Skip(offset).Take(8).ToArray();
            offset += 8;

            byte[] timestampBytes = buf.Skip(offset).Take(5).ToArray();
            offset += 5;

            byte actionCode = buf[offset++];

            byte[] data = buf.Skip(offset).Take(46).ToArray();
            offset += 46;

            byte[] versionArr = buf.Skip(offset).Take(3).ToArray();
            string version = $"{versionArr[0]}.{versionArr[1]}.{versionArr[2]}";

            // ---- Decode 5-byte timestamp ----
            DateTime timeStamp = ExtractDateTimeFromKcPacking(timestampBytes);

            // ---- Create GatewayLog ----
            var log = new GatewayLog(
                id: null,
                code: code,
                timeStamp: timeStamp,
                data: data,
                actionCode: actionCode,
                varVersion: version,
                gateway: null
            );

            LogEntry? entry = GatewayLogConverter.ConvertItem(log);
            if (entry == null)
                return;

            OnLogReceived?.Invoke(this, entry);
        }


        private async void BtnConnect_Click(object? sender, EventArgs e)
        {
            switch (state)
            {
                case State.Disconnected:
                    UpdateState(State.Connecting);
                    await RunWithDisabledControlsAsync(async ct =>
                    {
                        await ConnectAsync(ct);
                    });

                    UpdateState(client?.IsConnected == true ? State.Connected : State.Disconnected);
                    break;

                case State.Connected:
                    await RunWithDisabledControlsAsync(async ct =>
                    {
                        await DisconnectAsync(ct);
                    });
                    UpdateState(State.Disconnected);
                    break;

                case State.Connecting:
                    cancellationTokenSource?.Cancel();
                    break;
            }
        }

        private void UpdateState(State newState)
        {
            state = newState;

            switch (state)
            {
                case State.Disconnected:
                    btnConnect.Text = "Connect";
                    break;

                case State.Connecting:
                    btnConnect.Text = "Connecting...";
                    break;

                case State.Connected:
                    btnConnect.Text = "Disconnect";
                    break;
            }
        }

        private async Task ConnectAsync(CancellationToken token)
        {
            if (client == null)
                throw new InvalidOperationException("MQTT client not initialized.");

            var broker = textBox_BrokerUrl.Text.Trim();
            var listenToClient = textBox_topic.Text.Trim();

            if (string.IsNullOrWhiteSpace(broker))
                throw new Exception("Broker URL is required.");

            if (string.IsNullOrWhiteSpace(listenToClient))
                throw new Exception("Topic is required.");

            var options = new MqttClientOptionsBuilder()
                .WithTcpServer(broker)
                .WithClientId("LogViewerClient_" + Guid.NewGuid())
                .Build();

            await client.ConnectAsync(options, token);

            var filter = new MqttTopicFilterBuilder()
                .WithTopic($"{listenToClient}/log/new")
                .WithQualityOfServiceLevel(MqttQualityOfServiceLevel.AtLeastOnce)
                .Build();

            await client.SubscribeAsync(filter, token);
        }

        private async Task DisconnectAsync(CancellationToken token)
        {
            if (client != null && client.IsConnected)
            {
                await client.DisconnectAsync();
            }
        }

        private async Task RunWithDisabledControlsAsync(Func<CancellationToken, Task> task)
        {
            cancellationTokenSource?.Dispose();
            cancellationTokenSource = new CancellationTokenSource();

            try
            {
                SetControlsEnabledExceptButton(false);
                await task(cancellationTokenSource.Token);
            }
            catch (OperationCanceledException)
            {
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred:\r\n{ex.Message}",
                                "Error",
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Error);
            }
            finally
            {
                SetControlsEnabledExceptButton(true);
                cancellationTokenSource?.Dispose();
                cancellationTokenSource = null;
            }
        }

        private void SetControlsEnabledExceptButton(bool enabled)
        {
            foreach (Control c in this.Controls)
            {
                if (c != btnConnect)
                    c.Enabled = enabled;
            }
        }

        public static DateTime ExtractDateTimeFromKcPacking(byte[] ts)
        {
            if (ts == null || ts.Length < 5)
                throw new ArgumentException("Timestamp array must contain at least 5 bytes.", nameof(ts));

            // Byte layout based on the KC firmware packing:
            // ts[0] = year >> 6
            // ts[1] = (year & 0x3F) << 2 | (month >> 2)
            // ts[2] = (month & 0x03) << 6 | (day << 1) | (hour >> 4)
            // ts[3] = (hour & 0x0F) << 4 | (min >> 2)
            // ts[4] = (min & 0x03) << 6 | sec

            // ---- Decode YEAR (14 bits) ----
            int year = (ts[0] << 6) | (ts[1] >> 2);

            // ---- Decode MONTH (4 bits) ----
            int monthHigh2 = ts[1] & 0x03;            // bits 1..0
            int monthLow2 = (ts[2] >> 6) & 0x03;      // bits 7..6
            int month = (monthHigh2 << 2) | monthLow2;

            // ---- Decode DAY OF MONTH (5 bits) ----
            int day = (ts[2] >> 1) & 0x1F;

            // ---- Decode HOUR (5 bits) ----
            int hourMsb = ts[2] & 0x01;               // bit 0
            int hourLow4 = (ts[3] >> 4) & 0x0F;        // bits 7..4
            int hour = (hourMsb << 4) | hourLow4;

            // ---- Decode MINUTE (6 bits) ----
            int minuteHigh4 = ts[3] & 0x0F;            // bits 3..0
            int minuteLow2 = (ts[4] >> 6) & 0x03;     // bits 7..6
            int minute = (minuteHigh4 << 2) | minuteLow2;

            // ---- Decode SECOND (6 bits) ----
            int second = ts[4] & 0x3F;

            // ---- Build DateTime safely ----
            try
            {
                return new DateTime(year, month, day, hour, minute, second, DateTimeKind.Local);
            }
            catch
            {
                // In case the embedded device writes corrupt data
                return DateTime.MinValue;
            }
        }


        enum State
        {
            Disconnected,
            Connecting,
            Connected
        }
    }
}
