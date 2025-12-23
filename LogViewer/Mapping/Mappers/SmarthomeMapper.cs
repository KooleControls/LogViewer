using FormsLib.Maths;
using LogViewer.Devices.Gateway;
using LogViewer.Logging;
using LogViewer.Mapping.Interfaces;
using LogViewer.Mapping.Models;
using static FormsLib.Scope.Trace;

namespace LogViewer.Mapping.Mappers
{
    public class SmarthomeMapper : ITraceMapper
    {
        public void Map(LogEntry entry, ITraceBuilder builder)
        {
            if (entry.DeviceType != DeviceType.SmartHome)
                return;

            var id = entry.DeviceId;
            var code = entry.AsGatewayLogCode();

            switch (code)
            {
                case GatewayLogCodes.SmartHomeStateChanged:
                    MapSmarthomeManager(builder, entry, id);
                    break;

                case GatewayLogCodes.HeatmanagerStateChanged:
                    MapHeatManager(builder, entry, id);
                    break;

                case GatewayLogCodes.CMN_ModbusError:
                    MapModbusError(builder, entry, id);
                    break;
            }
        }

        private void MapSmarthomeManager(ITraceBuilder builder, LogEntry entry, int id)
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

            var descriptor = new TraceDescriptor
            {
                TraceId = $"SHM{id}_Smarthome",
                Category = "State",
                EntityId = $"SHM:{id}",
                DrawStyle = DrawStyles.State,
                DrawOption = DrawOptions.DrawNames | DrawOptions.ExtendEnd,
                BaseColor = Color.FromArgb(unchecked((int)0xFF1FFF53)),
                ToHumanReadable = stateLookup,
                Source = nameof(SmarthomeMapper)
            };

            var trace = builder.GetOrCreate(descriptor);
            trace.Trace.Points.Add(new PointD(
                entry.TimeStamp.Ticks,
                entry.Measurement ?? 0));
        }

        private void MapHeatManager(ITraceBuilder builder, LogEntry entry, int id)
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

            var descriptor = new TraceDescriptor
            {
                TraceId = $"SHM{id}_HeatMan",
                Category = "State",
                EntityId = $"SHM:{id}",
                DrawStyle = DrawStyles.State,
                DrawOption = DrawOptions.DrawNames | DrawOptions.ExtendEnd,
                BaseColor = Color.FromArgb(unchecked((int)0xFF18CC43)),
                ToHumanReadable = stateLookup,
                Source = nameof(SmarthomeMapper)
            };

            var trace = builder.GetOrCreate(descriptor);
            trace.Trace.Points.Add(new PointD(
                entry.TimeStamp.Ticks,
                entry.Measurement ?? 0));
        }

        private void MapModbusError(ITraceBuilder builder, LogEntry entry, int id)
        {
            var errorLookup = (double code) => code switch
            {
                0.0 => "No Error",
                1.0 => "IllegalFunction",
                2.0 => "IllegalDataAddress",
                3.0 => "IllegalDataValue",
                4.0 => "SlaveDeviceFailure",
                5.0 => "InvalidArguments",
                6.0 => "InvalidReplyLength",
                7.0 => "UnknownException",
                8.0 => "InvalidReplyFunctionCode",
                9.0 => "Timeout",
                10.0 => "MutexFailed",
                11.0 => "Disconnected",
                12.0 => "OutOfMemory",
                13.0 => "InsufficientRxBufferSize",
                14.0 => "NotImplemented",
                15.0 => "InvalidCRC",
                16.0 => "InvalidDeviceReply",
                17.0 => "ReadingNotAllowed",
                18.0 => "WritingNotAllowed",
                19.0 => "NotInitialized",
                20.0 => "Acknowledge",
                21.0 => "SlaveDeviceBusy",
                22.0 => "MemoryParityError",
                23.0 => "GatewayPathUnavailable",
                24.0 => "GatewayTargetFailed",
                25.0 => "InvalidDeviceReply_EchoMismatch",
                26.0 => "InvalidDeviceReply_ProtocolId",
                27.0 => "InvalidDeviceReply_UnitId",
                28.0 => "InvalidDeviceReply_ReplyLength",
                _ => "Unknown"
            };

            var descriptor = new TraceDescriptor
            {
                TraceId = $"SHM{id}_ModbusError",
                Category = "Modbus Errors",
                EntityId = $"SHM:{id}",
                DrawStyle = DrawStyles.State,
                DrawOption = DrawOptions.DrawNames | DrawOptions.ExtendEnd,
                BaseColor = Color.FromArgb(unchecked((int)0xFFFF0000)),
                ToHumanReadable = errorLookup,
                Source = nameof(SmarthomeMapper)
            };

            var trace = builder.GetOrCreate(descriptor);
            trace.Trace.Points.Add(new PointD(
                entry.TimeStamp.Ticks,
                entry.Measurement ?? 0));
        }
    }
}