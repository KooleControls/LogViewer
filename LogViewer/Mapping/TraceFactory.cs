using FormsLib.Design;
using FormsLib.Maths;
using FormsLib.Scope;
using LogViewer.Logging;
using LogViewer.Mapping.Models;

namespace LogViewer.Mapping
{

    public class TraceFactory
    {
        public BuiltTrace CreateTrace(AssignedTrace assigned, IEnumerable<LogEntry> entries)
        {
            var built = new BuiltTrace();

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

            built.Trace = trace;

            foreach (var pt in assigned.Descriptor.Generator(entries))
            {
                if (string.IsNullOrEmpty(pt.Label))
                {
                    trace.Points.Add(new PointD(pt.X.Ticks, pt.Y));
                }
                else
                {
                    var label = new LinkedLabel(trace)
                    {
                        Point = new PointD(pt.X.Ticks, pt.Y),
                        Text = pt.Label,
                    };

                    built.Labels.Add(label);
                }
            }

            return built;
        }

    }
}
