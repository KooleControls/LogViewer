namespace LogViewer.Config.Models
{
    public class ProfileConfig
    {
        public string? Name { get; set; }
        public Dictionary<string, TraceConfig> Traces { get; set; } = new();
        public UnhandeledConfig? Unhandeled { get; set; }

        public override string ToString() => Name ?? "";
    }
}

