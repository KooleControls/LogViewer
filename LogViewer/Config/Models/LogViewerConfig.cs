namespace LogViewer.Config.Models
{
    public class LogViewerConfig
    {
        public Dictionary<string, OrganisationConfig> Organisations { get; set; } = new();
    }
}

