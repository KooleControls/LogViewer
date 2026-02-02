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
            }
            return false;
        }

        public void MapMainStarted(ITraceBuilder builder, LogEntry entry)
        {
            var descriptor = new TraceDescriptor
            {
                TraceId = "Events",
                Category = "Events",
                EntityId = "Events",
                DrawStyle = DrawStyles.Cross,
                DrawOption = DrawOptions.DrawNames,
                BaseColor = Color.LightBlue,
                Source = "GatewayMapper"
            };

            string text = Encoding.ASCII.GetString(entry.RawData);

            var trace = builder.GetOrCreate(descriptor);
            var label = new LinkedLabel(trace.Trace, entry.TimeStamp.Ticks, 1);
            label.Text = $"Boot {text}";
            trace.ScopeController.Labels.Add(label);
        }
    }
}

