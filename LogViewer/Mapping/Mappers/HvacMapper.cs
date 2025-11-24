using FormsLib.Design;
using LogViewer.Devices.Gateway;
using LogViewer.Logging;
using LogViewer.Mapping.Models;
using static FormsLib.Scope.Trace;

namespace LogViewer.Mapping.Mappers
{

    public class HvacMapper : ITraceMapper
    {
        public IEnumerable<TraceDescriptor> Map(IEnumerable<LogEntry> entries)
        {
            var groups = entries
                .Where(e => e.DeviceType == DeviceType.HvacUnit)
                .GroupBy(e => e.DeviceId);

            foreach (var g in groups)
            {
                int id = g.Key;

                yield return MapActualTemp(g, id);
                yield return MapSetpoint(g, id);
                yield return MapMode(g, id);
                yield return MapHeatingActive(g, id);
                yield return MapCoolingActive(g, id);
                yield return MapFaultCode(g, id);
            }
        }

        private TraceDescriptor MapActualTemp(IEnumerable<LogEntry> group, int id)
        {
            return new TraceDescriptor
            {
                TraceId = $"HVAC{id}_TempActual",
                Category = "Temperature",
                EntityId = $"HVAC:{id}",
                DrawStyle = DrawStyles.Lines,
                DrawOption = DrawOptions.None,
                BaseColor = Color.FromArgb(unchecked((int)0xFFFFFF4D)),
                Generator = _ => group
                    .Where(e => e.LogCode is GatewayLogCodes code &&
                                code == GatewayLogCodes.Hvac_ActualTempChanged)
                    .Select(e => new TracePoint
                    {
                        X = e.TimeStamp,
                        Y = Convert.ToDouble(e.Measurement)
                    })
            };
        }

        private TraceDescriptor MapSetpoint(IEnumerable<LogEntry> group, int id)
        {
            return new TraceDescriptor
            {
                TraceId = $"HVAC{id}_Setpoint",
                Category = "Setpoint",
                EntityId = $"HVAC:{id}",
                DrawStyle = DrawStyles.Lines,
                DrawOption = DrawOptions.None,
                BaseColor = Color.FromArgb(unchecked((int)0xFFFF4D4D)),
                Generator = _ => group
                    .Where(e => e.LogCode is GatewayLogCodes code &&
                                code == GatewayLogCodes.Hvac_SetpointChanged)
                    .Select(e => new TracePoint
                    {
                        X = e.TimeStamp,
                        Y = Convert.ToDouble(e.Measurement)
                    })
            };
        }

        private TraceDescriptor MapMode(IEnumerable<LogEntry> group, int id)
        {
            var modeLookup = (double mode) => mode switch
            {
                0.0 => "Off",
                1.0 => "Heating",
                2.0 => "Cooling",
                3.0 => "Auto",
                _ => "Unknown"
            };


            return new TraceDescriptor
            {
                TraceId = $"HVAC{id}_Mode",
                Category = "State",
                EntityId = $"HVAC:{id}",
                DrawStyle = DrawStyles.State,
                DrawOption = DrawOptions.DrawNames,
                BaseColor = Color.FromArgb(unchecked((int)0xFFFF3D3D)),
                ToHumanReadable = modeLookup,
                Generator = _ => group
                    .Where(e => e.LogCode is GatewayLogCodes code &&
                                code == GatewayLogCodes.Hvac_ModeChanged)
                    .Select(e => new TracePoint
                    {
                        X = e.TimeStamp,
                        Y = Convert.ToDouble(e.Measurement)
                    })
            };
        }

        private TraceDescriptor MapHeatingActive(IEnumerable<LogEntry> group, int id)
        {
            return new TraceDescriptor
            {
                TraceId = $"HVAC{id}_HeatingActive",
                Category = "State",
                EntityId = $"HVAC:{id}",
                DrawStyle = DrawStyles.State,
                DrawOption = DrawOptions.DrawNames,
                BaseColor = Color.FromArgb(unchecked((int)0xFFFF6F00)),
                ToHumanReadable = d => d == 0.0 ? "Off" : "On",
                Generator = _ => group
                    .Where(e => e.LogCode is GatewayLogCodes code &&
                                code == GatewayLogCodes.Hvac_HeatingActiveChanged)
                    .Select(e => new TracePoint
                    {
                        X = e.TimeStamp,
                        Y = Convert.ToBoolean(e.Measurement) ? 1 : 0
                    })
            };
        }

        private TraceDescriptor MapCoolingActive(IEnumerable<LogEntry> group, int id)
        {
            return new TraceDescriptor
            {
                TraceId = $"HVAC{id}_CoolingActive",
                Category = "State",
                EntityId = $"HVAC:{id}",
                DrawStyle = DrawStyles.State,
                DrawOption = DrawOptions.DrawNames,
                BaseColor = Color.FromArgb(unchecked((int)0xFF0074CC)),
                ToHumanReadable = d => d == 0.0 ? "Off" : "On",
                Generator = _ => group
                    .Where(e => e.LogCode is GatewayLogCodes code &&
                                code == GatewayLogCodes.Hvac_CoolingActiveChanged)
                    .Select(e => new TracePoint
                    {
                        X = e.TimeStamp,
                        Y = Convert.ToBoolean(e.Measurement) ? -1 : 0
                    })
            };
        }

        private TraceDescriptor MapFaultCode(IEnumerable<LogEntry> group, int id)
        {
            return new TraceDescriptor
            {
                TraceId = $"HVAC{id}_FaultCode",
                Category = "Debug",
                EntityId = $"HVAC:{id}",
                DrawStyle = DrawStyles.State,
                DrawOption = DrawOptions.DrawNames,
                BaseColor = Color.FromArgb(unchecked((int)0xFFFF3D3D)),
                ToHumanReadable = d => d.ToString(),
                Generator = _ => group
                    .Where(e => e.LogCode is GatewayLogCodes code &&
                                code == GatewayLogCodes.Hvac_FaultCodeChanged)
                    .Select(e => new TracePoint
                    {
                        X = e.TimeStamp,
                        Y = Convert.ToDouble(e.Measurement) // direct numeric code
                    })
            };
        }
    }
}
