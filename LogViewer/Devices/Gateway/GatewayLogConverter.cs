using DevExpress.Data.Svg;
using KC.InternalApi.Model;
using LogViewer.Logging;
using System.Text;
using DeviceType = LogViewer.Logging.DeviceType;

namespace LogViewer.Devices.Gateway
{

    public class GatewayLogConverter
    {

        public static List<LogEntry> ConvertItems(IEnumerable<GatewayLog> items)
        {
            List<LogEntry> logEntries = new List<LogEntry>();
            foreach (var item in items)
            {
                var logEntry = FromGatewayLog(item);
                if (logEntry != null)
                {
                    logEntries.Add(logEntry);
                }
            }
            return logEntries;

        }
        public static List<LogEntry> ConvertItems(IEnumerable<DeviceData> items)
        {
            List<LogEntry> logEntries = new List<LogEntry>();
            foreach (var item in items)
            {
                var logEntry = FromDeviceData(item);
                if (logEntry != null)
                {
                    logEntries.Add(logEntry);
                }
            }
            return logEntries;

        }

        public static LogEntry? FromGatewayLog(GatewayLog logItem)
        {
            if (logItem.TimeStamp == null)
                return null;

            if (logItem.Code == null)
                return null;

            GatewayLogCodes logCode = (GatewayLogCodes)logItem.Code;
            if (logCode == GatewayLogCodes.xA0_SMARTHOME_EVENT)
                logCode = (GatewayLogCodes)(0xA0000000 | (logItem.Data[0] << 8) | logItem.Data[1]);

            LogEntry logEntry = new LogEntry()
            {
                TimeStamp = logItem.TimeStamp.Value,
                LogCode = (UInt32)logCode,
                RawData = logItem.Data,
                SourceSoftwareId = SoftwareId.Gateway_1245,
                DeviceId = 0,
            };

            ExpandLogEntry(logEntry, logCode, logItem.Data);
            return logEntry;
        }

        public static LogEntry? FromDeviceData(DeviceData data)
        {
            data.SubDeviceType ??= 1;    // When we dont have rights, assume it's a gateway log
            if (data.SubDeviceType != 1)
                return null;
            if (data.TimeStamp == null)
                return null;
            if (data.Code == null)
                return null;

            GatewayLogCodes logCode = (GatewayLogCodes)data.Code;
            if (logCode == GatewayLogCodes.xA0_SMARTHOME_EVENT)
                logCode = (GatewayLogCodes)(0xA0000000 | (data.Data[0] << 8) | data.Data[1]);

            LogEntry logEntry = new LogEntry()
            {
                TimeStamp = data.TimeStamp.Value,
                LogCode = (UInt32)logCode,
                RawData = data.Data,
                SourceSoftwareId = SoftwareId.Gateway_1245,
                DeviceId = 0,
            };
            ExpandLogEntry(logEntry, logCode, data.Data);
            return logEntry;
        }

        public static void ExpandLogEntry(LogEntry logEntry, GatewayLogCodes logCode, byte[] data)
        {
            switch (logCode)
            {
                case GatewayLogCodes.xB1_EXTPCB_REP_VERS:
                    logEntry.Metadata = Encoding.ASCII.GetString(data);
                    break;
                case GatewayLogCodes.TempSetpointOverride:
                case GatewayLogCodes.Therm_TempActualChanged:
                case GatewayLogCodes.Therm_TempSetpointChanged:
                    logEntry.DeviceType = DeviceType.Thermostat;
                    logEntry.DeviceId = 0;
                    logEntry.Measurement = BitConverter.ToSingle(data, 3);
                    break;

                case GatewayLogCodes.Therm_CoolingRequestChanged:
                case GatewayLogCodes.Therm_HeatingRequestChanged:
                    logEntry.DeviceType = DeviceType.Thermostat;
                    logEntry.DeviceId = 0;
                    logEntry.Measurement = BitConverter.ToBoolean(data, 3) ? 1 : 0;
                    break;

                case GatewayLogCodes.SmartHomeStateChanged:
                case GatewayLogCodes.HeatmanagerStateChanged:
                    logEntry.DeviceType = DeviceType.SmartHome;
                    logEntry.DeviceId = 0;
                    logEntry.Measurement = data[4];
                    break;

                case GatewayLogCodes.Input1Changed:
                case GatewayLogCodes.Input2Changed:
                case GatewayLogCodes.Relais1Changed:
                case GatewayLogCodes.Relais2Changed:
                    logEntry.DeviceType = DeviceType.Gateway;
                    logEntry.DeviceId = 0;
                    logEntry.Measurement = BitConverter.ToBoolean(data, 3) ? 1 : 0;
                    break;

                case GatewayLogCodes.CMN_ModbusError:
                    logEntry.DeviceType = DeviceType.SmartHome;
                    logEntry.DeviceId = 0;
                    logEntry.Measurement = data[3];
                    break;

                case GatewayLogCodes.CMN_IndoorUnit_ActualTempChanged:
                case GatewayLogCodes.CMN_IndoorUnit_SetpointChanged:
                    logEntry.DeviceType = DeviceType.LegacyIndoorUnit;
                    logEntry.DeviceId = data[3];
                    logEntry.Measurement = (float)BitConverter.ToUInt16(data, 4) / 10f;
                    break;

                case GatewayLogCodes.CMN_IndoorUnit_ModeChanged:
                case GatewayLogCodes.CMN_IndoorUnit_FaultCodeChanged:
                case GatewayLogCodes.CMN_IndoorUnit_OnOffChanged:
                    logEntry.DeviceType = DeviceType.LegacyIndoorUnit;
                    logEntry.DeviceId = data[3];
                    logEntry.Measurement = BitConverter.ToUInt16(data, 4);
                    break;

                case GatewayLogCodes.Hvac_IsFaultChanged:
                    logEntry.DeviceType = DeviceType.HvacUnit;
                    logEntry.DeviceId = data[3];
                    logEntry.Measurement = BitConverter.ToBoolean(data, 4) ? 1 : 0;
                    break;

                case GatewayLogCodes.Hvac_FaultCodeChanged:
                case GatewayLogCodes.Hvac_ModeChanged:
                case GatewayLogCodes.Hvac_RemoteCTRLChanged:
                case GatewayLogCodes.Hvac_HeatingActiveChanged:
                case GatewayLogCodes.Hvac_CoolingActiveChanged:
                    logEntry.DeviceType = DeviceType.HvacUnit;
                    logEntry.DeviceId = data[3];
                    logEntry.Measurement = BitConverter.ToUInt16(data, 4);
                    break;

                case GatewayLogCodes.Hvac_SetpointChanged:
                case GatewayLogCodes.Hvac_ActualTempChanged:
                    logEntry.DeviceType = DeviceType.HvacUnit;
                    logEntry.DeviceId = data[3];
                    logEntry.Measurement = ((float)BitConverter.ToUInt16(data, 4) / 10f);
                    break;

                default:
                    break;
            }
        }
    }
}
