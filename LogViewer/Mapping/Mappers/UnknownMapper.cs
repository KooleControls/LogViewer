using FormsLib.Scope;
using KCObjectsStandard.Data.CodeGeneration;
using LogViewer.Devices.Gateway;
using LogViewer.Logging;
using LogViewer.Mapping.Interfaces;
using LogViewer.Mapping.Models;
using static FormsLib.Scope.Trace;

namespace LogViewer.Mapping.Mappers
{
    public class UnknownMapper : ITraceMapper
    {
        public void Map(LogEntry entry, ITraceBuilder builder)
        {
            if (IsKnown(entry))
                return;

            var descriptor = new TraceDescriptor
            {
                TraceId = "Unknown",
                Category = "Unknown",
                EntityId = "Unknown",
                DrawStyle = DrawStyles.Cross,
                DrawOption = DrawOptions.DrawNames,
                BaseColor = Color.Wheat,
                Source = nameof(UnknownMapper)
            };

            var trace = builder.GetOrCreate(descriptor);
            var label = new LinkedLabel(trace.Trace, entry.TimeStamp.Ticks, 1);
            label.Text = $"{entry.AsGatewayLogCode()?.ToString() ?? entry.LogCode.ToString()}";
            trace.ScopeController.Labels.Add(label);
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
                GatewayLogCodes.TempSetpointOverride => true,
                GatewayLogCodes.HeaterActiveChanged => true,
                GatewayLogCodes.CoolerActiveChanged => true,
                GatewayLogCodes.TempMon_StateChanged => true,
                GatewayLogCodes.TempMon_OverTemp => true,
                GatewayLogCodes.TempMon_UnderTemp => true,
                GatewayLogCodes.TempMon_UnderTempHalf => true,
                _ => false,
            };
        }
    }
}

