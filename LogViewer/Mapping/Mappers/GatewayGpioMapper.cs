using FormsLib.Maths;
using LogViewer.Devices.Gateway;
using LogViewer.Logging;
using LogViewer.Mapping.Interfaces;
using LogViewer.Mapping.Models;
using static FormsLib.Scope.Trace;

namespace LogViewer.Mapping.Mappers
{
    public class GatewayGpioMapper : ITraceMapper
    {
        public bool Map(LogEntry entry, ITraceBuilder builder)
        {
            if (entry.DeviceType != DeviceType.Gateway)
                return false;

            var code = entry.AsGatewayLogCode();
            var id = entry.DeviceId;

            switch (code)
            {
                case GatewayLogCodes.Relais1Changed:
                    MapRelay(builder, entry, id, 1, GatewayLogCodes.Relais1Changed);
                    return true;
                case GatewayLogCodes.Relais2Changed:
                    MapRelay(builder, entry, id, 2, GatewayLogCodes.Relais2Changed);
                    return true;
                case GatewayLogCodes.Input1Changed:
                    MapInput(builder, entry, id, 1, GatewayLogCodes.Input1Changed);
                    return true;
                case GatewayLogCodes.Input2Changed:
                    MapInput(builder, entry, id, 2, GatewayLogCodes.Input2Changed);
                    return true;
            }
            return false;
        }

        private void MapRelay(ITraceBuilder builder, LogEntry entry, int id, int relayIndex, GatewayLogCodes expectedCode)
        {
            var descriptor = new TraceDescriptor
            {
                TraceId = $"GIO{id}_Relay_{relayIndex}",
                Category = "State",
                EntityId = $"GIO:{id}",
                DrawStyle = DrawStyles.State,
                DrawOption = DrawOptions.DrawNames | DrawOptions.ExtendEnd,
                BaseColor = Color.FromArgb(unchecked((int)0xFF008FFF)),
                ToHumanReadable = d => d == 0.0 ? "Off" : "On",
                Source = nameof(GatewayGpioMapper)
            };

            var trace = builder.GetOrCreate(descriptor);

            trace.Trace.Points.Add(new PointD(
                entry.TimeStamp.Ticks,
                entry.Measurement ?? 0));
        }

        private void MapInput(ITraceBuilder builder, LogEntry entry, int id, int inputIndex, GatewayLogCodes expectedCode)
        {
            var descriptor = new TraceDescriptor
            {
                TraceId = $"GIO{id}_Input_{inputIndex}",
                Category = "State",
                EntityId = $"GIO:{id}",
                DrawStyle = DrawStyles.State,
                DrawOption = DrawOptions.DrawNames | DrawOptions.ExtendEnd,
                BaseColor = Color.FromArgb(unchecked((int)0xFF008FFF)),
                ToHumanReadable = d => d == 0.0 ? "Off" : "On",
                Source = nameof(GatewayGpioMapper)
            };

            var trace = builder.GetOrCreate(descriptor);

            trace.Trace.Points.Add(new PointD(
                entry.TimeStamp.Ticks, 
                entry.Measurement ?? 0));
        }
    }
}