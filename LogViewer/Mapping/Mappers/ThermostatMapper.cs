using FormsLib.Scope;
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
                Generator = entries =>
                {
                    var actual = group
                        .Where(e => e.AsGatewayLogCode() ==GatewayLogCodes.Therm_TempActualChanged)
                        .Select(e => new TracePoint
                        {
                            X = e.TimeStamp,
                            Y = e.Measurement ?? 0,
                        });
                    return actual;
                }
            };
        }

        private TraceDescriptor MapSetpointTemperature(IEnumerable<LogEntry> group, int id)
        {
            return new TraceDescriptor
            {
                TraceId = $"THR{id}_TempSetpoint",
                Category = "Setpoint",
                EntityId = $"THR:{id}",
                DrawStyle = DrawStyles.NonInterpolatedLine,
                DrawOption = DrawOptions.None,
                BaseColor = Color.FromArgb(unchecked((int)0xFFFF0000)),
                Generator = entries =>
                {
                    var actual = group
                        .Where(e => e.AsGatewayLogCode() ==GatewayLogCodes.Therm_TempSetpointChanged)
                        .Select(e => new TracePoint
                        {
                            X = e.TimeStamp,
                            Y = e.Measurement ?? 0,
                        });
                    
                    var overwrite = group
                        .Where(e => e.AsGatewayLogCode() ==GatewayLogCodes.TempSetpointOverride)
                        .Select(e => new TracePoint
                        {
                            X = e.TimeStamp,
                            Y = e.Measurement ?? 0,
                            Label = "OVE"
                        });

                    return actual.Concat(overwrite).OrderBy(p => p.X);
                }

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
                    .Where(e => e.AsGatewayLogCode() ==GatewayLogCodes.Therm_HeatingRequestChanged)
                    .Select(e => new TracePoint
                    {
                        X = e.TimeStamp,
                        Y = e.Measurement ?? 0,
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
                    .Where(e => e.AsGatewayLogCode() ==GatewayLogCodes.Therm_CoolingRequestChanged)
                    .Select(e => new TracePoint
                    {
                        X = e.TimeStamp,
                        Y = e.Measurement ?? 0,
                    })
            };
        }
    }
}

