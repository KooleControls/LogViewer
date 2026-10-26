using FormsLib.Scope;
using LogViewer.Logging;
using LogViewer.Mapping.Interfaces;
using LogViewer.Mapping.Models;
using static FormsLib.Scope.Trace;

namespace LogViewer.Mapping.Mappers
{
    /// <summary>
    /// The shared "Events" row: one labelled mark per notable moment - a boot, a link coming or
    /// going. It lives apart from the mappers because several of them write to it and the registry
    /// keys traces by id, so whichever mapper got there first would otherwise silently decide the
    /// row's colour and style for all of them.
    ///
    /// The label is everything the reader gets, so it names the subsystem and the wire, not just
    /// the state: the gateway can lose the thermostat over BLE, over OpenTherm, or by losing the
    /// OpenTherm board itself, and "link lost" would not say which.
    /// </summary>
    internal static class EventRow
    {
        public static void Add(ITraceBuilder builder, LogEntry entry, string text, string source)
        {
            var descriptor = new TraceDescriptor
            {
                TraceId = "Events",
                Category = "Events",
                EntityId = "Events",
                DrawStyle = DrawStyles.Cross,
                DrawOption = DrawOptions.DrawNames,
                BaseColor = Color.LightBlue,
                Source = source
            };

            var trace = builder.GetOrCreate(descriptor);
            var label = new LinkedLabel(trace.Trace, entry.TimeStamp.Ticks, 1);
            label.Text = text;
            trace.ScopeController.Labels.Add(label);
        }
    }
}
