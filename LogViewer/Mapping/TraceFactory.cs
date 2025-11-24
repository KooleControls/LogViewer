using FormsLib.Design;
using FormsLib.Maths;
using FormsLib.Scope;
using LogViewer.Logging;
using LogViewer.Mapping.Models;

namespace LogViewer.Mapping
{
    public class TraceFactory
    {
        public Trace CreateTrace(AssignedTrace assigned, IEnumerable<LogEntry> entries)
        {
            var trace = new Trace
            {
                Name = assigned.Descriptor.TraceId,
                Offset = assigned.VerticalOffset,
                Scale = assigned.Scale,
                Unit = assigned.Unit ?? "",
                Color = assigned.Descriptor.BaseColor,
                DrawStyle = assigned.Descriptor.DrawStyle,
                DrawOption = assigned.Descriptor.DrawOption,
                ToHumanReadable = assigned.Descriptor.ToHumanReadable,
            };

            foreach (var pt in assigned.Descriptor.Generator(entries))
            {
                trace.Points.Add(new PointD(pt.X.Ticks, pt.Y));
            }

            return trace;
        }
    }
}
