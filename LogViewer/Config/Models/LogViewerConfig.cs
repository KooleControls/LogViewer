namespace LogViewer.Config.Models
{
    public class LogViewerConfig
    {
        public List<string> Sources { get; set; } = new();
        public Dictionary<string, OrganisationConfig> Organisations { get; set; } = new();
    }
}

