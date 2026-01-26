using FormsLib.Maths;
using LogViewer.Devices.Gateway;
using LogViewer.Logging;
using LogViewer.Mapping.Interfaces;
using LogViewer.Mapping.Models;
using static FormsLib.Scope.Trace;

namespace LogViewer.Mapping.Mappers
{
    public class HvacMapper : ITraceMapper
    {
        public bool Map(LogEntry entry, ITraceBuilder builder)
        {
            if (entry.DeviceType != DeviceType.HvacUnit)
                return false;

            var id = entry.DeviceId;
            var code = entry.AsGatewayLogCode();

            switch (code)
            {
                case GatewayLogCodes.Hvac_ActualTempChanged:
                    MapActualTemp(builder, entry, id);
                    return true;
                case GatewayLogCodes.Hvac_SetpointChanged:
                    MapSetpoint(builder, entry, id);
                    return true;
                case GatewayLogCodes.Hvac_ModeChanged:
                    MapMode(builder, entry, id);
                    return true;
                case GatewayLogCodes.Hvac_HeatingActiveChanged:
                    MapHeatingActive(builder, entry, id);
                    return true;
                case GatewayLogCodes.Hvac_CoolingActiveChanged:
                    MapCoolingActive(builder, entry, id);
                    return true;
                case GatewayLogCodes.Hvac_FaultCodeChanged:
                    MapFaultCode(builder, entry, id);
                    return true;
            }
            return false;
        }

        private void MapActualTemp(ITraceBuilder builder, LogEntry entry, int id)
        {
            var descriptor = new TraceDescriptor
            {
                TraceId = $"HVAC{id}_TempActual",
                Category = "Temperature",
                EntityId = $"HVAC:{id}",
                DrawStyle = DrawStyles.Lines,
                DrawOption = DrawOptions.None,
                BaseColor = Color.FromArgb(unchecked((int)0xFFFFFF4D)),
                Source = nameof(HvacMapper)
            };

            var trace = builder.GetOrCreate(descriptor);
            trace.Trace.Points.Add(new PointD(
                entry.TimeStamp.Ticks,
                entry.Measurement ?? 0));
        }

        private void MapSetpoint(ITraceBuilder builder, LogEntry entry, int id)
        {
            var descriptor = new TraceDescriptor
            {
                TraceId = $"HVAC{id}_Setpoint",
                Category = "Setpoint",
                EntityId = $"HVAC:{id}",
                DrawStyle = DrawStyles.NonInterpolatedLine,
                DrawOption = DrawOptions.None | DrawOptions.ExtendEnd,
                BaseColor = Color.FromArgb(unchecked((int)0xFFFF4D4D)),
                Source = nameof(HvacMapper)
            };

            var trace = builder.GetOrCreate(descriptor);
            trace.Trace.Points.Add(new PointD(
                entry.TimeStamp.Ticks,
                entry.Measurement ?? 0));
        }

        private void MapMode(ITraceBuilder builder, LogEntry entry, int id)
        {
            var modeLookup = (double mode) => mode switch
            {
                0.0 => "Off",
                1.0 => "Heating",
                2.0 => "Cooling",
                3.0 => "Auto",
                _ => $"Unknown ({mode})" 
            };

            var descriptor = new TraceDescriptor
            {
                TraceId = $"HVAC{id}_Mode",
                Category = "State",
                EntityId = $"HVAC:{id}",
                DrawStyle = DrawStyles.State,
                DrawOption = DrawOptions.DrawNames | DrawOptions.ExtendEnd,
                BaseColor = Color.FromArgb(unchecked((int)0xFFFF3D3D)),
                ToHumanReadable = modeLookup,
                Source = nameof(HvacMapper)
            };

            var trace = builder.GetOrCreate(descriptor);
            trace.Trace.Points.Add(new PointD(
                entry.TimeStamp.Ticks,
                entry.Measurement ?? 0));
        }

        private void MapHeatingActive(ITraceBuilder builder, LogEntry entry, int id)
        {
            var descriptor = new TraceDescriptor
            {
                TraceId = $"HVAC{id}_HeatingActive",
                Category = "State",
                EntityId = $"HVAC:{id}",
                DrawStyle = DrawStyles.State,
                DrawOption = DrawOptions.DrawNames | DrawOptions.ExtendEnd,
                BaseColor = Color.FromArgb(unchecked((int)0xFFFF6F00)),
                ToHumanReadable = d => d == 0.0 ? "Off" : "On",
                Source = nameof(HvacMapper)
            };

            var trace = builder.GetOrCreate(descriptor);
            trace.Trace.Points.Add(new PointD(
                entry.TimeStamp.Ticks,
                entry.Measurement ?? 0));
        }

        private void MapCoolingActive(ITraceBuilder builder, LogEntry entry, int id)
        {
            var descriptor = new TraceDescriptor
            {
                TraceId = $"HVAC{id}_CoolingActive",
                Category = "State",
                EntityId = $"HVAC:{id}",
                DrawStyle = DrawStyles.State,
                DrawOption = DrawOptions.DrawNames | DrawOptions.ExtendEnd,
                BaseColor = Color.FromArgb(unchecked((int)0xFF0074CC)),
                ToHumanReadable = d => d == 0.0 ? "Off" : "On",
                Source = nameof(HvacMapper)
            };

            var trace = builder.GetOrCreate(descriptor);
            trace.Trace.Points.Add(new PointD(
                entry.TimeStamp.Ticks,
                entry.Measurement ?? 0));
        }

        private void MapFaultCode(ITraceBuilder builder, LogEntry entry, int id)
        {
            var descriptor = new TraceDescriptor
            {
                TraceId = $"HVAC{id}_FaultCode",
                Category = "Debug",
                EntityId = $"HVAC:{id}",
                DrawStyle = DrawStyles.State,
                DrawOption = DrawOptions.DrawNames | DrawOptions.ExtendEnd,
                BaseColor = Color.FromArgb(unchecked((int)0xFFFF3D3D)),
                ToHumanReadable = d => d.ToString(),
                Source = nameof(HvacMapper)
            };

            var trace = builder.GetOrCreate(descriptor);
            trace.Trace.Points.Add(new PointD(
                entry.TimeStamp.Ticks,
                entry.Measurement ?? 0));
        }
    }
}