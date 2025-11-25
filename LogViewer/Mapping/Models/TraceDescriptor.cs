using FormsLib.Scope;
using static FormsLib.Scope.Trace;

namespace LogViewer.Mapping.Models
{
    public class TraceDescriptor
    {
        public string TraceId { get; set; } = "";
        public string Category { get; set; } = "General";
        public string? EntityId { get; set; }

        public DrawStyles DrawStyle { get; set; }
        public DrawOptions DrawOption { get; set; }
        public Color BaseColor { get; set; } = Color.Wheat;

        public Func<double, string> ToHumanReadable { get; set; }
            = d => d.ToHumanReadable(3);

        public string Source { get; set; } = "";

        public override string ToString() => TraceId;
    }
}