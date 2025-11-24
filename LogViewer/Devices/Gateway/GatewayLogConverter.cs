using KC.InternalApi.Model;
using LogViewer.Logging;
using System.Text;
using DeviceType = LogViewer.Logging.DeviceType;

namespace LogViewer.Devices.Gateway
{
    public class GatewayLogConverter
    {

        public static LogEntry? ConvertItem(GatewayLog logItem)
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
                LogCode = logCode,
                RawData = logItem.Data,
            };

            ExpandLogEntry(logEntry, logCode, logItem.Data);
            return logEntry;
        }

        public static void ExpandLogEntry(LogEntry logEntry, GatewayLogCodes logCode, byte[] data)
        {
            switch (logCode)
            {
                case GatewayLogCodes.x02_MAIN_STARTED:
                    logEntry.DeviceType = DeviceType.Gateway_1245;
                    logEntry.DeviceId = 0;
                    break;

                case GatewayLogCodes.xB1_EXTPCB_REP_VERS:
                    logEntry.DeviceType = DeviceType.ExtentionModule_1246;
                    logEntry.DeviceId = 0;
                    logEntry.Measurement = Encoding.ASCII.GetString(data);
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
                    logEntry.Measurement = BitConverter.ToBoolean(data, 3);
                    break;

                case GatewayLogCodes.SmartHomeStateChanged:
                case GatewayLogCodes.HeatmanagerStateChanged:
                    logEntry.DeviceType = DeviceType.Smarthome;
                    logEntry.DeviceId = 0;
                    logEntry.Measurement = data[4];
                    break;

                case GatewayLogCodes.Input1Changed:
                case GatewayLogCodes.Input2Changed:
                case GatewayLogCodes.Relais1Changed:
                case GatewayLogCodes.Relais2Changed:
                    logEntry.DeviceType = DeviceType.Gateway_1245;
                    logEntry.DeviceId = 0;
                    logEntry.Measurement = BitConverter.ToBoolean(data, 3);
                    break;

                case GatewayLogCodes.CMN_ModbusError:
                    logEntry.DeviceType = DeviceType.Gateway_1245;
                    logEntry.DeviceId = 0;
                    logEntry.Measurement = data[3];
                    break;

                case GatewayLogCodes.Hvac_IsFaultChanged:
                    logEntry.DeviceType = DeviceType.HvacUnit;
                    logEntry.DeviceId = data[3];
                    logEntry.Measurement = BitConverter.ToBoolean(data, 4);
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
