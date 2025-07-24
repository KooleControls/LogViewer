using KC.InternalApi.Model;
using LogViewer.Devices.Gateway;
using LogViewer.Logging;
using System.Text;

namespace LogViewer.Providers.API
{
    public class WebApiLogItemConverter
    {

        public static LogEntry? ConvertItem(GatewayLog logItem)
        {
            if (logItem.TimeStamp == null)
                return null;

            if (logItem.Code == null)
                return null;

            var logCode = (GatewayLogCodes)logItem.Code;
            if (logCode == GatewayLogCodes.xA0_SMARTHOME_EVENT)

                logCode = (GatewayLogCodes)(0xA0000000 | (logItem.Data[0] << 8) | logItem.Data[1]);

            var builder = LogEntryBuilder.CreateBuilder(logItem.TimeStamp.Value)
                .WithSegment(LogKeys.SoftwareId, Devices.DeviceId.SmarthomeGateway)
                .WithSegment(LogKeys.LogCode, logCode);


            switch (logCode)
            {
                case GatewayLogCodes.x02_MAIN_STARTED:
                    builder.WithSegment(LogKeys.Version, logItem.VarVersion);
                    break;

                case GatewayLogCodes.xB1_EXTPCB_REP_VERS:
                    builder.WithSegment(LogKeys.Version, Encoding.ASCII.GetString(logItem.Data));
                    break;

                case GatewayLogCodes.Therm_TempActualChanged:
                case GatewayLogCodes.Therm_TempSetpointChanged:
                    builder.WithSegment(LogKeys.Measurement, BitConverter.ToSingle(logItem.Data, 3));
                    break;

                case GatewayLogCodes.Therm_CoolingRequestChanged:
                case GatewayLogCodes.Therm_HeatingRequestChanged:
                    builder.WithSegment(LogKeys.Measurement, logItem.Data[3]);
                    break;

                case GatewayLogCodes.SmartHomeStateChanged:
                case GatewayLogCodes.HeatmanagerStateChanged:
                    builder.WithSegment(LogKeys.Measurement, logItem.Data[4]);
                    break;

                case GatewayLogCodes.HeaterActiveChanged:
                case GatewayLogCodes.CoolerActiveChanged:
                case GatewayLogCodes.Input1Changed:
                case GatewayLogCodes.Input2Changed:
                case GatewayLogCodes.Relais1Changed:
                case GatewayLogCodes.Relais2Changed:
                    builder.WithSegment(LogKeys.Measurement, logItem.Data[3]);
                    break;


                case GatewayLogCodes.CMN_ModbusError:
                    builder.WithSegment(LogKeys.Measurement, logItem.Data[3]);
                    break;

                case GatewayLogCodes.CMN_IndoorUnit_ActualTempChanged:
                case GatewayLogCodes.CMN_IndoorUnit_SetpointChanged:
                    builder.WithSegment(LogKeys.DeviceId, logItem.Data[3]);
                    builder.WithSegment(LogKeys.Measurement, (float)BitConverter.ToUInt16(logItem.Data, 4) / 10f);
                    // builder.WithSegment(LogKeys.IU_Written, logItem.Data[6] > 0);
                    // builder.WithSegment(LogKeys.IU_IsMaster, logItem.Data[7] > 0);
                    break;

                case GatewayLogCodes.CMN_IndoorUnit_ModeChanged:
                case GatewayLogCodes.CMN_IndoorUnit_FaultCodeChanged:
                case GatewayLogCodes.CMN_IndoorUnit_OnOffChanged:
                    builder.WithSegment(LogKeys.DeviceId, logItem.Data[3]);
                    builder.WithSegment(LogKeys.Measurement, BitConverter.ToUInt16(logItem.Data, 4));
                    // builder.WithSegment(LogKeys.IU_Written, logItem.Data[6] > 0);
                    // builder.WithSegment(LogKeys.IU_IsMaster, logItem.Data[7] > 0);
                    break;


                default:
                    break;
            }


            builder.WithSegment(LogKeys.RawData, logItem.Data);

            return builder.Build();
        }
    }
}
