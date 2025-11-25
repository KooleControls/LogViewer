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
        public void Map(LogEntry entry, ITraceBuilder builder)
        {
            if (entry.DeviceType != DeviceType.Thermostat)
                return;

            var id = entry.DeviceId;
            var code = entry.AsGatewayLogCode();

            switch (code)
            {
                case GatewayLogCodes.Therm_TempActualChanged:
                    MapActualTemperature(builder, entry, id);
                    break;
                case GatewayLogCodes.Therm_TempSetpointChanged:
                case GatewayLogCodes.TempSetpointOverride:
                    MapSetpointTemperature(builder, entry, id, code);
                    break;
                case GatewayLogCodes.Therm_HeatingRequestChanged:
                    MapHeatingRequest(builder, entry, id);
                    break;
                case GatewayLogCodes.Therm_CoolingRequestChanged:
                    MapCoolingRequest(builder, entry, id);
                    break;
            }
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
            if (code == GatewayLogCodes.TempSetpointOverride)
            {
                var label = new LinkedLabel(trace.Trace, entry.TimeStamp.Ticks, entry.Measurement ?? 0);
                label.Text = "OVE";
                trace.ScopeController.Labels.Add(label);
            }

            trace.Trace.Points.Add(new PointD(
                entry.TimeStamp.Ticks,
                entry.Measurement ?? 0));

            // Note: ordering by X is handled later (TraceFactory uses OrderBy on Points)
        }

        private void MapHeatingRequest(ITraceBuilder builder, LogEntry entry, int id)
        {
            var descriptor = new TraceDescriptor
            {
                TraceId = $"THR{id}_HeatingReq",
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
                TraceId = $"THR{id}_CoolingReq",
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
    }
}