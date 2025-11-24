using LogViewer.Devices.Gateway;
using LogViewer.Logging;
using LogViewer.Mapping.Models;
using static FormsLib.Scope.Trace;

namespace LogViewer.Mapping.Mappers
{
    public class GatewayGpioMapper : ITraceMapper
    {
        public IEnumerable<TraceDescriptor> Map(IEnumerable<LogEntry> entries)
        {
            var groups = entries
                .Where(e => e.DeviceType == DeviceType.Gateway)
                .GroupBy(e => e.DeviceId);

            foreach (var g in groups)
            {
                int id = g.Key;

                yield return MapRelay_1(g, id);
                yield return MapRelay_2(g, id);
                yield return MapInput_1(g, id);
                yield return MapInput_2(g, id);
            }
        }

        private TraceDescriptor MapRelay_1(IEnumerable<LogEntry> group, int id)
        {
            return new TraceDescriptor
            {
                TraceId = $"GIO{id}_Relay_1",
                Category = "State",
                EntityId = $"GIO:{id}",
                DrawStyle = DrawStyles.State,
                DrawOption = DrawOptions.DrawNames,
                BaseColor = Color.FromArgb(unchecked((int)0xFF008FFF)),
                ToHumanReadable = d => d == 0.0 ? "Off" : "On",
                Generator = _ => group
                    .Where(e => e.AsGatewayLogCode() == GatewayLogCodes.Relais1Changed)
                    .Select(e => new TracePoint
                    {
                        X = e.TimeStamp,
                        Y = e.Measurement ?? 0,
                    })
            };
        }

        private TraceDescriptor MapRelay_2(IEnumerable<LogEntry> group, int id)
        {
            return new TraceDescriptor
            {
                TraceId = $"GIO{id}_Relay_2",
                Category = "State",
                EntityId = $"GIO:{id}",
                DrawStyle = DrawStyles.State,
                DrawOption = DrawOptions.DrawNames,
                BaseColor = Color.FromArgb(unchecked((int)0xFF008FFF)),
                ToHumanReadable = d => d == 0.0 ? "Off" : "On",
                Generator = _ => group
                    .Where(e => e.AsGatewayLogCode() ==GatewayLogCodes.Relais2Changed)
                    .Select(e => new TracePoint
                    {
                        X = e.TimeStamp,
                        Y = e.Measurement ?? 0,
                    })
            };
        }

        private TraceDescriptor MapInput_1(IEnumerable<LogEntry> group, int id)
        {
            return new TraceDescriptor
            {
                TraceId = $"GIO{id}_Input_1",
                Category = "State",
                EntityId = $"GIO:{id}",
                DrawStyle = DrawStyles.State,
                DrawOption = DrawOptions.DrawNames,
                BaseColor = Color.FromArgb(unchecked((int)0xFF008FFF)),
                ToHumanReadable = d => d == 0.0 ? "Off" : "On",
                Generator = _ => group
                    .Where(e => e.AsGatewayLogCode() ==GatewayLogCodes.Input1Changed)
                    .Select(e => new TracePoint
                    {
                        X = e.TimeStamp,
                        Y = e.Measurement ?? 0,
                    })
            };
        }

        private TraceDescriptor MapInput_2(IEnumerable<LogEntry> group, int id)
        {
            return new TraceDescriptor
            {
                TraceId = $"GIO{id}_Input_2",
                Category = "State",
                EntityId = $"GIO:{id}",
                DrawStyle = DrawStyles.State,
                DrawOption = DrawOptions.DrawNames,
                BaseColor = Color.FromArgb(unchecked((int)0xFF008FFF)),
                ToHumanReadable = d => d == 0.0 ? "Off" : "On",
                Generator = _ => group
                    .Where(e => e.AsGatewayLogCode() ==GatewayLogCodes.Input2Changed)
                    .Select(e => new TracePoint
                    {
                        X = e.TimeStamp,
                        Y = e.Measurement ?? 0,
                    })
            };
        }

    }
}

