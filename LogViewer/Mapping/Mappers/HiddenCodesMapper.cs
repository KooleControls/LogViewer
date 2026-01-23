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
    public class HiddenCodesMapper : ITraceMapper
    {
        public bool Map(LogEntry entry, ITraceBuilder builder)
        {
            var code = entry.AsGatewayLogCode();

            switch (code)
            {
                case GatewayLogCodes.TempMon_StateChanged: return true;
                case GatewayLogCodes.TempMon_OverTemp: return true;
                case GatewayLogCodes.TempMon_UnderTemp: return true;
                case GatewayLogCodes.TempMon_UnderTempHalf: return true;

            }
            return false;
        }
    }
}