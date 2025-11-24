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

            foreach (var assigned in assignedTraces)
            {
                var built = factory.CreateTrace(assigned, entries);
                _scope.Traces.Add(built.Trace);

                foreach (var label in built.Labels)
                    _scope.Labels.Add(label);
            }
        }
    }
}
