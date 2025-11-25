using LogViewer.Devices.Gateway;
using LogViewer.Logging;
using LogViewer.Mapping.Models;
using static FormsLib.Scope.Trace;

namespace LogViewer.Mapping.Mappers
{
    public class UnknownMapper : ITraceMapper
    {
        public IEnumerable<TraceDescriptor> Map(IEnumerable<LogEntry> entries)
        {
            return new[]
            {
                new TraceDescriptor
                {
                    TraceId = "Unknown",
                    Category = "Unknown",
                    EntityId = "Unknown",
                    DrawStyle = DrawStyles.Cross,
                    DrawOption = DrawOptions.DrawNames,
                    Generator = UnknownGenerator(entries)
                }
            };
        }

        private Func<IEnumerable<LogEntry>, IEnumerable<TracePoint>> UnknownGenerator(IEnumerable<LogEntry> all)
        {
            return _ =>
            {
                return all
                    .Where(e => !IsKnown(e))
                    .Select(e => new TracePoint
                    {
                        X = e.TimeStamp,
                        Y = 1f,
                        Label = $"{e.AsGatewayLogCode()?.ToString() ?? e.LogCode.ToString()}",
                    });
            };
        }

        private bool IsKnown(LogEntry entry)
        {

            var code = entry.AsGatewayLogCode();

            return code switch
            {
                GatewayLogCodes.Therm_TempActualChanged => true,
                GatewayLogCodes.Therm_TempSetpointChanged => true,
                GatewayLogCodes.Therm_HeatingRequestChanged => true,
                GatewayLogCodes.Therm_CoolingRequestChanged => true,
                GatewayLogCodes.Hvac_ActualTempChanged => true,
                GatewayLogCodes.Hvac_SetpointChanged => true,
                GatewayLogCodes.Hvac_ModeChanged => true,
                GatewayLogCodes.Hvac_FaultCodeChanged => true,
                GatewayLogCodes.Hvac_HeatingActiveChanged => true,
                GatewayLogCodes.Hvac_CoolingActiveChanged => true,
                GatewayLogCodes.HeatmanagerStateChanged => true,
                GatewayLogCodes.SmartHomeStateChanged => true,
                GatewayLogCodes.CMN_ModbusError => true,
                GatewayLogCodes.Relais1Changed => true,
                GatewayLogCodes.Relais2Changed => true,
                GatewayLogCodes.Input1Changed => true,
                GatewayLogCodes.Input2Changed => true,

                _ => false,
            };
        }
    }

}
