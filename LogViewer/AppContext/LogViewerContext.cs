using LogViewer.Config.Models;
using LogViewer.Logging;


namespace LogViewer.AppContext
{
    public class LogViewerContext
    {
        public LogCollection LogCollection { get; set; } = new LogCollection();
        public ScopeViewContext ScopeViewContext { get; set; } = new ScopeViewContext();
    }
}




