using FormsLib.Scope;
using LogViewer.Logging;
using LogViewer.Mapping.Models;

namespace LogViewer.Mapping
{
    public class ScopeControllerAdapter
    {
        private readonly ScopeController _scope;

        public ScopeControllerAdapter(ScopeController scope)
        {
            _scope = scope;
        }

        public void Load(IEnumerable<AssignedTrace> assignedTraces, IEnumerable<LogEntry> entries)
        {
            _scope.Traces.Clear();
            _scope.Labels.Clear();

            var factory = new TraceFactory();

            foreach (var at in assignedTraces)
            {
                var trace = factory.CreateTrace(at, entries);
                _scope.Traces.Add(trace);
            }
        }
    }
}
