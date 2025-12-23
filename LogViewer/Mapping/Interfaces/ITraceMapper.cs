using LogViewer.Logging;

namespace LogViewer.Mapping.Interfaces
{
    public interface ITraceMapper
    {
        void Map(LogEntry entry, ITraceBuilder builder);
    }
}