using FormsLib.Maths;
using LogViewer.Devices.Gateway;
using LogViewer.Logging;
using LogViewer.Mapping.Interfaces;
using LogViewer.Mapping.Models;
using static FormsLib.Scope.Trace;

namespace LogViewer.Mapping.Mappers
{
    public class ConnectionMapper : ITraceMapper
    {
        public bool Map(LogEntry entry, ITraceBuilder builder)
        {
            var code = entry.AsGatewayLogCode();
            var id = entry.DeviceId;

            switch (code)
            {
                case GatewayLogCodes.x53_KC2_CLIENT_CONNECTIONESTABLISHED:
                    MapConnectionState(builder, entry, true);
                    return true;
                case GatewayLogCodes.x52_KC2_CLIENT_CONNECTIONLOST:
                    MapConnectionState(builder, entry, false);
                    return true;
            }
            return false;
        }

        private void MapConnectionState(ITraceBuilder builder, LogEntry entry, bool connected)
        {
            var descriptor = new TraceDescriptor
            {
                TraceId = $"ConSrv",
                Category = "State",
                EntityId = $"ConSrv",
                DrawStyle = DrawStyles.State,
                DrawOption = DrawOptions.DrawNames | DrawOptions.ExtendEnd,
                BaseColor = Color.FromArgb(unchecked((int)0xFF008FFF)),
                ToHumanReadable = d => d == 0.0 ? "Disconnected" : "Connected",
                Source = nameof(ConnectionMapper)
            };

            var trace = builder.GetOrCreate(descriptor);

            trace.Trace.Points.Add(new PointD(
                entry.TimeStamp.Ticks,
                connected ? 1 : 0));
        }


    }
}