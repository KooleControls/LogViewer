using FormsLib.Maths;
using FormsLib.Scope;
using LogViewer.Devices.Gateway;
using LogViewer.Logging;
using LogViewer.Mapping.Interfaces;
using LogViewer.Mapping.Models;
using Microsoft.CodeAnalysis.CSharp;
using static FormsLib.Scope.Trace;

namespace LogViewer.Mapping.Mappers
{
    public class ThermostatMapper : ITraceMapper
    {
        public bool Map(LogEntry entry, ITraceBuilder builder)
        {
            if (entry.Device.Type != DeviceType.Thermostat)
                return false;

            var id = entry.Device.Id;
            var code = entry.AsGatewayLogCode();

            switch (code)
            {
                case GatewayLogCodes.Therm_TempActualChanged:
                    MapActualTemperature(builder, entry, id);
                    return true;
                case GatewayLogCodes.Therm_TempSetpointChanged:
                    MapSetpointTemperature(builder, entry, id, code);
                    return true;
                case GatewayLogCodes.TempSetpointOverride:
                    MapTempSetpointOverride(builder, entry, id, code);
                    return true;
                case GatewayLogCodes.Therm_HeatingRequestChanged:
                    MapHeatingRequest(builder, entry, id);
                    return true;
                case GatewayLogCodes.Therm_CoolingRequestChanged:
                    MapCoolingRequest(builder, entry, id);
                    return true;
                case GatewayLogCodes.ThermostatLinkChanged:
                    MapBleLink(builder, entry);
                    return true;
                case GatewayLogCodes.Therm_CommunicationTimeout:
                    MapOthComm(builder, entry);
                    return true;
            }
            return false;
        }

        private void MapActualTemperature(ITraceBuilder builder, LogEntry entry, int id)
        {
            var descriptor = new TraceDescriptor
            {
                TraceId = $"THR{id}_TempActual",
                Category = "Temperature",
                EntityId = $"THR:{id}",
                DrawStyle = DrawStyles.Lines,
                DrawOption = DrawOptions.None,
                BaseColor = Color.FromArgb(unchecked((int)0xFFFFFF00)),
                Source = nameof(ThermostatMapper)
            };

            var trace = builder.GetOrCreate(descriptor);
            trace.Trace.Points.Add(new PointD(
                entry.TimeStamp.Ticks,
                entry.Measurement ?? 0));
        }



        private void MapTempSetpointOverride(ITraceBuilder builder, LogEntry entry, int id, GatewayLogCodes? code)
        {
            var descriptor = new TraceDescriptor
            {
                TraceId = $"THR{id}_TempSetpoint",
                Category = "Setpoint",
                EntityId = $"THR:{id}",
                DrawStyle = DrawStyles.NonInterpolatedLine,
                DrawOption = DrawOptions.None | DrawOptions.ExtendEnd,
                BaseColor = Color.FromArgb(unchecked((int)0xFFFF0000)),
                Source = nameof(ThermostatMapper)
            };

            var trace = builder.GetOrCreate(descriptor);
            var label = new LinkedLabel(trace.Trace, entry.TimeStamp.Ticks, entry.Measurement ?? 0);
            label.Text = "OVE";
            trace.ScopeController.Labels.Add(label);
        }

        private void MapSetpointTemperature(ITraceBuilder builder, LogEntry entry, int id, GatewayLogCodes? code)
        {
            var descriptor = new TraceDescriptor
            {
                TraceId = $"THR{id}_TempSetpoint",
                Category = "Setpoint",
                EntityId = $"THR:{id}",
                DrawStyle = DrawStyles.NonInterpolatedLine,
                DrawOption = DrawOptions.None | DrawOptions.ExtendEnd,
                BaseColor = Color.FromArgb(unchecked((int)0xFFFF0000)),
                Source = nameof(ThermostatMapper)
            };

            var trace = builder.GetOrCreate(descriptor);
            trace.Trace.Points.Add(new PointD(
                entry.TimeStamp.Ticks,
                entry.Measurement ?? 0));
        }

        private void MapHeatingRequest(ITraceBuilder builder, LogEntry entry, int id)
        {
            var descriptor = new TraceDescriptor
            {
                TraceId = $"THR{id}_HeatReq",
                Category = "State",
                EntityId = $"THR:{id}",
                DrawStyle = DrawStyles.State,
                DrawOption = DrawOptions.DrawNames | DrawOptions.ExtendEnd,
                BaseColor = Color.FromArgb(unchecked((int)0xFFCC5A00)),
                ToHumanReadable = d => d == 0.0 ? "Off" : "On",
                Source = nameof(ThermostatMapper)
            };

            var trace = builder.GetOrCreate(descriptor);
            trace.Trace.Points.Add(new PointD(
                entry.TimeStamp.Ticks,
                entry.Measurement ?? 0));
        }

        private void MapCoolingRequest(ITraceBuilder builder, LogEntry entry, int id)
        {
            var descriptor = new TraceDescriptor
            {
                TraceId = $"THR{id}_CoolReq",
                Category = "State",
                EntityId = $"THR:{id}",
                DrawStyle = DrawStyles.State,
                DrawOption = DrawOptions.DrawNames | DrawOptions.ExtendEnd,
                BaseColor = Color.FromArgb(unchecked((int)0xFF008FFF)),
                ToHumanReadable = d => d == 0.0 ? "Off" : "On",
                Source = nameof(ThermostatMapper)
            };

            var trace = builder.GetOrCreate(descriptor);
            trace.Trace.Points.Add(new PointD(
                entry.TimeStamp.Ticks,
                entry.Measurement ?? 0));
        }

        /// <summary>
        /// The gateway's own BLE link to the thermostat. Spelled out as "BLE" because there are two
        /// other ways to lose the same thermostat - see <see cref="MapOthComm"/> and the PCB1246
        /// mark in GatewayMapper - and the label is the only thing telling them apart.
        /// </summary>
        private void MapBleLink(ITraceBuilder builder, LogEntry entry)
        {
            var text = (entry.Measurement ?? 0) != 0
                ? "Thermostat BLE connected"
                : "Thermostat BLE lost";

            EventRow.Add(builder, entry, text, nameof(ThermostatMapper));
        }

        /// <summary>
        /// The OpenTherm room unit's CTO flag, reported up by the PCB1246: the thermostat stopped
        /// answering on the OpenTherm bus. Nothing to do with BLE.
        /// </summary>
        private void MapOthComm(ITraceBuilder builder, LogEntry entry)
        {
            var text = (entry.Measurement ?? 0) != 0
                ? "Thermostat OpenTherm timeout"
                : "Thermostat OpenTherm restored";

            EventRow.Add(builder, entry, text, nameof(ThermostatMapper));
        }
    }
}