using LogViewer.Logging;
using LogViewer.Mapping.Models;

namespace LogViewer.Mapping
{
    public class TracePipeline
    {
        private readonly List<ITraceMapper> _mappers;

        public TracePipeline(IEnumerable<ITraceMapper> mappers)
        {
            _mappers = mappers.ToList();
        }

        public IEnumerable<AssignedTrace> Run(IEnumerable<LogEntry> entries)
        {
            var descriptors = _mappers
                .SelectMany(m => m.Map(entries))
                .ToList();

            return new TraceLayoutEngine().Layout(descriptors);
        }
    }
}
