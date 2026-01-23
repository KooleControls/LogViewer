using LogViewer.Logging;

namespace LogViewer.Mapping.Interfaces
{
    public interface ITraceMapper
    {
        bool Map(LogEntry entry, ITraceBuilder builder);
    }
}