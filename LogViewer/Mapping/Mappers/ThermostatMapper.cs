using LogViewer.Devices.Gateway;
using LogViewer.Logging;
using LogViewer.Mapping.Models;
using static FormsLib.Scope.Trace;

namespace LogViewer.Mapping.Mappers
{
    public class ThermostatMapper : ITraceMapper
    {
        public IEnumerable<TraceDescriptor> Map(IEnumerable<LogEntry> entries)
        {
            var groups = entries
                .Where(e => e.DeviceType == DeviceType.Thermostat)
                .GroupBy(e => e.DeviceId);

            foreach (var g in groups)
            {
                int id = g.Key;

                yield return MapActualTemperature(g, id);
                yield return MapSetpointTemperature(g, id);
                yield return MapHeatingRequest(g, id);
                yield return MapCoolingRequest(g, id);
            }
        }

        private TraceDescriptor MapActualTemperature(IEnumerable<LogEntry> group, int id)
        {
            return new TraceDescriptor
            {
                TraceId = $"THR{id}_TempActual",
                Category = "Temperature",
                EntityId = $"THR:{id}",
                DrawStyle = DrawStyles.Lines,
                DrawOption = DrawOptions.None,
                BaseColor = Color.FromArgb(unchecked((int)0xFFFFFF00)),
                Generator = _ => group
                    .Where(e => e.LogCode is GatewayLogCodes code &&
                                code == GatewayLogCodes.Therm_TempActualChanged)
                    .Select(e => new TracePoint
                    {
                        X = e.TimeStamp,
                        Y = Convert.ToDouble(e.Measurement ?? 0),
                    })
            };
        }

        private TraceDescriptor MapSetpointTemperature(IEnumerable<LogEntry> group, int id)
        {
            return new TraceDescriptor
            {
                TraceId = $"THR{id}_TempSetpoint",
                Category = "Setpoint",
                EntityId = $"THR:{id}",
                DrawStyle = DrawStyles.Lines,
                DrawOption = DrawOptions.None,
                BaseColor = Color.FromArgb(unchecked((int)0xFFFF0000)),
                Generator = _ => group
                    .Where(e => e.LogCode is GatewayLogCodes code &&
                                code == GatewayLogCodes.Therm_TempSetpointChanged)
                    .Select(e => new TracePoint
                    {
                        X = e.TimeStamp,
                        Y = Convert.ToDouble(e.Measurement ?? 0),
                    })
            };
        }

        private TraceDescriptor MapHeatingRequest(IEnumerable<LogEntry> group, int id)
        {
            return new TraceDescriptor
            {
                TraceId = $"THR{id}_HeatingReq",
                Category = "State",
                EntityId = $"THR:{id}",
                DrawStyle = DrawStyles.State,
                DrawOption = DrawOptions.DrawNames,
                BaseColor = Color.FromArgb(unchecked((int)0xFFCC5A00)),
                ToHumanReadable = d => d == 0.0 ? "Off" : "On",
                Generator = _ => group
                    .Where(e => e.LogCode is GatewayLogCodes code &&
                                code == GatewayLogCodes.Therm_HeatingRequestChanged)
                    .Select(e => new TracePoint
                    {
                        X = e.TimeStamp,
                        Y = Convert.ToBoolean(e.Measurement) ? 1.0 : 0.0,
                    })
            };
        }

        private TraceDescriptor MapCoolingRequest(IEnumerable<LogEntry> group, int id)
        {
            return new TraceDescriptor
            {
                TraceId = $"THR{id}_CoolingReq",
                Category = "State",
                EntityId = $"THR:{id}",
                DrawStyle = DrawStyles.State,
                DrawOption = DrawOptions.DrawNames,
                BaseColor = Color.FromArgb(unchecked((int)0xFF008FFF)),
                ToHumanReadable = d => d == 0.0 ? "Off" : "On",
                Generator = _ => group
                    .Where(e => e.LogCode is GatewayLogCodes code &&
                                code == GatewayLogCodes.Therm_CoolingRequestChanged)
                    .Select(e => new TracePoint
                    {
                        X = e.TimeStamp,
                        Y = Convert.ToBoolean(e.Measurement) ? -1.0 : 0.0,
                    })
            };
        }
    }
}
