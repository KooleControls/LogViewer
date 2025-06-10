using LogViewer.Logging;
using static FormsLib.Scope.Trace;

namespace LogViewer.Config.Models
{
    public class TraceConfig
    {
        public string? Name { get; set; }
        public string? Unit { get; set; }
        public Color? Color { get; set; }
        public VisibleOptions? Visible { get; set; }
        public float? Offset { get; set; }
        public float? Scale { get; set; } 
        public DrawStyles? DrawStyle { get; set; }
        public DrawOptions? DrawOption { get; set; }
        public Dictionary<int, string>? StateNames { get; set; }
        public Dictionary<LogKeys, string> IncludeLogs { get; set; } = new();

        public override string ToString() => Name ?? "";
    }

    public enum VisibleOptions
    {
        Always,
        WhenData,
        Never
    }

}
