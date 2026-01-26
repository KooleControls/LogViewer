using FormsLib.Maths;
using LogViewer.Devices.Gateway;
using LogViewer.Logging;
using LogViewer.Mapping.Interfaces;
using LogViewer.Mapping.Models;
using static FormsLib.Scope.Trace;

namespace LogViewer.Mapping.Mappers
{
    public class LegacyIndoorUnitMapper : ITraceMapper
    {
        public bool Map(LogEntry entry, ITraceBuilder builder)
        {
            if (entry.DeviceType != DeviceType.LegacyIndoorUnit)
                return false;

            var id = entry.DeviceId;
            var code = entry.AsGatewayLogCode();

            switch (code)
            {
                case GatewayLogCodes.CMN_IndoorUnit_ActualTempChanged:
                    MapActualTemp(builder, entry, id);
                    return true;
                case GatewayLogCodes.CMN_IndoorUnit_SetpointChanged:
                    MapSetpoint(builder, entry, id);
                    return true;
                case GatewayLogCodes.CMN_IndoorUnit_ModeChanged:
                    MapMode(builder, entry, id);
                    return true;
                case GatewayLogCodes.CMN_IndoorUnit_OnOffChanged:
                    MapOnOff(builder, entry, id);
                    return true;
                case GatewayLogCodes.CMN_IndoorUnit_FaultCodeChanged:
                    MapFaultCode(builder, entry, id);
                    return true;
            }
            return false;
        }

        private void MapActualTemp(ITraceBuilder builder, LogEntry entry, int id)
        {
            var descriptor = new TraceDescriptor
            {
                TraceId = $"IU{id}_TempActual",
                Category = "Temperature",
                EntityId = $"IU:{id}",
                DrawStyle = DrawStyles.Lines,
                DrawOption = DrawOptions.None,
                BaseColor = Color.FromArgb(unchecked((int)0xFFFFFF4D)),
                Source = nameof(LegacyIndoorUnitMapper)
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
                TraceId = $"IU{id}_Setpoint",
                Category = "Setpoint",
                EntityId = $"IU:{id}",
                DrawStyle = DrawStyles.NonInterpolatedLine,
                DrawOption = DrawOptions.None | DrawOptions.ExtendEnd,
                BaseColor = Color.FromArgb(unchecked((int)0xFFFF4D4D)),
                Source = nameof(LegacyIndoorUnitMapper)
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
                0 => "Cool",
                1 => "Heat",
                2 => "Auto",
                3 => "Dry",
                4 => "HAUX",
                5 => "Fan",
                6 => "HH",
                8 => "VAM_Auto",
                9 => "VAM_Bypass",
                10 => "VAM_Heat_Exch",
                11 => "VAM_Normal",
                12 => "Unknown",
                99 => "Off",

                _ => "Unknown",
            };




            var descriptor = new TraceDescriptor
            {
                TraceId = $"IU{id}_Mode",
                Category = "State",
                EntityId = $"IU:{id}",
                DrawStyle = DrawStyles.State,
                DrawOption = DrawOptions.DrawNames | DrawOptions.ExtendEnd,
                BaseColor = Color.FromArgb(unchecked((int)0xFFFF3D3D)),
                ToHumanReadable = modeLookup,
                Source = nameof(LegacyIndoorUnitMapper)
            };

            var trace = builder.GetOrCreate(descriptor);
            trace.Trace.Points.Add(new PointD(
                entry.TimeStamp.Ticks,
                entry.Measurement ?? 0));
        }

        private void MapOnOff(ITraceBuilder builder, LogEntry entry, int id)
        {
            var descriptor = new TraceDescriptor
            {
                TraceId = $"IU{id}_Power",
                Category = "State",
                EntityId = $"IU:{id}",
                DrawStyle = DrawStyles.State,
                DrawOption = DrawOptions.DrawNames | DrawOptions.ExtendEnd,
                BaseColor = Color.FromArgb(unchecked((int)0xFFFF6F00)),
                ToHumanReadable = d => d == 0.0 ? "Off" : "On",
                Source = nameof(LegacyIndoorUnitMapper)
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
                TraceId = $"IU{id}_FaultCode",
                Category = "Debug",
                EntityId = $"IU:{id}",
                DrawStyle = DrawStyles.State,
                DrawOption = DrawOptions.DrawNames | DrawOptions.ExtendEnd,
                BaseColor = Color.FromArgb(unchecked((int)0xFFFF3D3D)),
                ToHumanReadable = d => d.ToString(),
                Source = nameof(LegacyIndoorUnitMapper)
            };

            var trace = builder.GetOrCreate(descriptor);
            trace.Trace.Points.Add(new PointD(
                entry.TimeStamp.Ticks,
                entry.Measurement ?? 0));
        }
    }
}
