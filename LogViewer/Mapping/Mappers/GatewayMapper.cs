using FormsLib.Maths;
using FormsLib.Scope;
using LogViewer.Devices.Gateway;
using LogViewer.Logging;
using LogViewer.Mapping.Interfaces;
using LogViewer.Mapping.Models;
using System.Text;
using static FormsLib.Scope.Trace;

namespace LogViewer.Mapping.Mappers
{
    public class GatewayMapper : ITraceMapper
    {
        public bool Map(LogEntry entry, ITraceBuilder builder)
        {
            var code = entry.AsGatewayLogCode();

            switch (code)
            {
                case GatewayLogCodes.x02_MAIN_STARTED:
                    MapMainStarted(builder, entry);
                    return true;
                case GatewayLogCodes.PCB1246CommunicationTimeout:
                    MapOpenThermTimeout(builder, entry);
                    return true;
            }
            return false;
        }

        public void MapMainStarted(ITraceBuilder builder, LogEntry entry)
        {
            string text = Encoding.ASCII.GetString(entry.RawData);
            EventRow.Add(builder, entry, $"Gateway booted {text}", nameof(GatewayMapper));
        }

        /// <summary>
        /// The OpenTherm board stopped answering the minute-ly CTES ping on the internal serial
        /// link and the gateway restarted itself, so this mark always sits just before a boot: it
        /// says why that reboot happened. Not a thermostat event - the board going quiet takes all
        /// of OpenTherm with it, but says nothing about the BLE thermostat.
        /// </summary>
        public void MapOpenThermTimeout(ITraceBuilder builder, LogEntry entry)
        {
            EventRow.Add(builder, entry, "OpenTherm board (PCB1246) silent - gateway rebooted", nameof(GatewayMapper));
        }
    }
}

