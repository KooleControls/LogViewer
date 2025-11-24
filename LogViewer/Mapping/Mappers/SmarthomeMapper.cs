using LogViewer.Devices.Gateway;
using LogViewer.Logging;
using LogViewer.Mapping.Models;
using static FormsLib.Scope.Trace;

namespace LogViewer.Mapping.Mappers
{
    public class SmarthomeMapper : ITraceMapper
    {
        public IEnumerable<TraceDescriptor> Map(IEnumerable<LogEntry> entries)
        {
            var groups = entries
                .Where(e => e.DeviceType == DeviceType.Smarthome)
                .GroupBy(e => e.DeviceId);

            foreach (var g in groups)
            {
                int id = g.Key;

                yield return MapSmarthomeManager(g, id);
                yield return MapHeatManager(g, id);

            }
        }

        private TraceDescriptor MapSmarthomeManager(IEnumerable<LogEntry> group, int id)
        {
            var stateLookup = (double mode) => mode switch
            {
                0.0 => "Vacant",
                1.0 => "Preheating",
                2.0 => "Occupied",
                3.0 => "VacantWarm",
                4.0 => "TempOverride",
                5.0 => "Initializing",
                6.0 => "NightTime",
                _ => "Unknown"
            };


            return new TraceDescriptor
            {
                TraceId = $"SHM{id}_Mode",
                Category = "State",
                EntityId = $"SHM:{id}",
                DrawStyle = DrawStyles.State,
                DrawOption = DrawOptions.DrawNames,
                BaseColor = Color.FromArgb(unchecked((int)0xFF1FFF53)),
                ToHumanReadable = stateLookup,
                Generator = _ => group
                    .Where(e => e.LogCode is GatewayLogCodes code &&
                                code == GatewayLogCodes.SmartHomeStateChanged)
                    .Select(e => new TracePoint
                    {
                        X = e.TimeStamp,
                        Y = Convert.ToDouble(e.Measurement)
                    })
            };
        }



        private TraceDescriptor MapHeatManager(IEnumerable<LogEntry> group, int id)
        {
            var stateLookup = (double mode) => mode switch
            {
                0.0 => "Initializing",
                1.0 => "Wait",
                2.0 => "Cooling",
                3.0 => "Heating",
                4.0 => "Off",
                5.0 => "FrostProtection",
                _ => "Unknown"
            };

            return new TraceDescriptor
            {
                TraceId = $"HEATMAN{id}_Mode",
                Category = "State",
                EntityId = $"HEATMAN:{id}",
                DrawStyle = DrawStyles.State,
                DrawOption = DrawOptions.DrawNames,
                BaseColor = Color.FromArgb(unchecked((int)0xFF18CC43)),
                ToHumanReadable = stateLookup,
                Generator = _ => group
                    .Where(e => e.LogCode is GatewayLogCodes code &&
                                code == GatewayLogCodes.SmartHomeStateChanged)
                    .Select(e => new TracePoint
                    {
                        X = e.TimeStamp,
                        Y = Convert.ToDouble(e.Measurement)
                    })
            };
        }
    }
}
