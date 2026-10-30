using FormsLib.Maths;
using LogViewer.Devices.Gateway;
using LogViewer.Logging;
using LogViewer.Mapping.Interfaces;
using LogViewer.Mapping.Models;
using System.Text.RegularExpressions;
using static FormsLib.Scope.Trace;

namespace LogViewer.Mapping.Mappers
{
    public class HvacMapper : ITraceMapper
    {
        public bool Map(LogEntry entry, ITraceBuilder builder)
        {
            // The boot itself is the GatewayMapper's; this only resets what a boot resets.
            if (entry.AsGatewayLogCode() == GatewayLogCodes.x02_MAIN_STARTED)
            {
                ResetCommunication(builder, entry);
                return false;
            }

            if (entry.Device.Type != DeviceType.HvacUnit)
                return false;

            var id = entry.Device.Id;
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
                case GatewayLogCodes.Hvac_CommunicationLostChanged:
                    MapCommunicationLost(builder, entry, id);
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
                TraceId = $"HVAC{id}_Heat",
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
                TraceId = $"HVAC{id}_Cool",
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

        private void MapCommunicationLost(ITraceBuilder builder, LogEntry entry, int id)
        {
            var descriptor = new TraceDescriptor
            {
                TraceId = $"HVAC{id}_Communication",
                Category = "State",
                EntityId = $"HVAC:{id}",
                DrawStyle = DrawStyles.State,
                DrawOption = DrawOptions.DrawNames | DrawOptions.ExtendEnd,
                BaseColor = Color.FromArgb(unchecked((int)0xFFD50000)),
                ToHumanReadable = d => d == 0.0 ? "Responding" : "Not responding",
                Source = nameof(HvacMapper)
            };

            var trace = builder.GetOrCreate(descriptor);
            trace.Trace.Points.Add(new PointD(
                entry.TimeStamp.Ticks,
                entry.Measurement ?? 0));
        }

        /// <summary>
        /// After a boot the gateway counts every unit as not responding until it answers, and logs
        /// each unit's state once it has polled it. So a boot puts every unit back at "Not
        /// responding", and a "Responding" from before the boot does not carry over. Only this
        /// gateway's traces: the builder qualifies them with the parent.
        /// </summary>
        private static void ResetCommunication(ITraceBuilder builder, LogEntry entry)
        {
            string suffix = entry.Device.Parent is null ? "" : $"@{entry.Device.Parent}";
            var traces = builder.Traces.Where(t =>
                t.Descriptor.Source == nameof(HvacMapper)
                && Regex.IsMatch(t.Descriptor.TraceId, $@"^HVAC\d+_Communication{Regex.Escape(suffix)}$")
                && t.Trace.Points.Count > 0
                && t.Trace.Points[^1].Y == 0);

            foreach (var trace in traces)
                trace.Trace.Points.Add(new PointD(entry.TimeStamp.Ticks, 1));
        }
    }
}