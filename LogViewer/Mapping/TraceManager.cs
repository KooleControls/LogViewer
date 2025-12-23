using FormsLib.Scope;
using LogViewer.Logging;
using LogViewer.Mapping.Interfaces;

namespace LogViewer.Mapping
{
    public class TraceManager
    {
        private readonly ScopeController _scope;
        private readonly IReadOnlyList<ITraceMapper> _mappers;
        private readonly TraceRegistry _registry;

        public TraceManager(
            ScopeController scope,
            IEnumerable<ITraceMapper> mappers)
        {
            _scope = scope;
            _mappers = mappers.ToList();
            _registry = new TraceRegistry(_scope);
        }

        public void Clear()
        {
            _registry.Clear();
            _scope.Traces.Clear();
            _scope.Labels.Clear();
            _scope.RedrawAll();
        }

        public void Append(LogEntry entry)
        {
            foreach (var mapper in _mappers)
            {
                mapper.Map(entry, _registry);
            }

            _scope.RedrawAll();
        }

        public void LoadAll(IEnumerable<LogEntry> entries)
        {
            Clear();

            foreach (var entry in entries)
            {
                foreach (var mapper in _mappers)
                {
                    mapper.Map(entry, _registry);
                }
            }
            _scope.RedrawAll();
        }
    }
}